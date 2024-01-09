// ReSharper disable MemberCanBePrivate.Global

namespace CCR;

using System.Drawing;

public class Recognizer
{
    private Dictionary<string, Color> _colors;
    private int _pointHitBoxRadius = 50;

    public Recognizer(Dictionary<string, Color> colors)
    {
        _colors = colors;
        _stopped = true;
        Configure();
    }

    public void SetPointHitBoxRadius(int radius)
    {
        if (radius <= 0)
            throw new ArgumentException("radius cant be less than 1");

        _pointHitBoxRadius = radius;
    }

    public int GetPointHitBoxRadius() => _pointHitBoxRadius;

    public string[][] RecognizeColorCollisions(ref IRecognizingSource source)
    {
        var colorsPoints = RecognizeInThreads(ref source);
        return RecognizeColorCollisionsByPoints(colorsPoints);
    }

    private string[][] RecognizeColorCollisionsByPoints(Dictionary<string, (int, int)> colorsPoints)
    {
        var collisions = new List<List<string>>();

        foreach (var (colorName1, (x1, y1)) in colorsPoints)
        {
            var pointCollisions = new List<string>();

            foreach (var (colorName2, (x2, y2)) in colorsPoints)
            {
                if (colorName1 == colorName2) continue;

                var distanceBetweenPoints = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
                if (distanceBetweenPoints <= _pointHitBoxRadius * 2)
                    pointCollisions.Add(colorName2);
            }

            var alreadyParticipates = false;
            for (var i = 0; i < collisions.Count; i++)
            {
                if (!collisions[i].Contains(colorName1))
                    continue;

                alreadyParticipates = true;
                collisions[i] = collisions[i].Union(pointCollisions).ToList();
            }

            if (!alreadyParticipates && pointCollisions.Count > 0)
                collisions.Add(pointCollisions);
        }

        return collisions.Select(c => c.ToArray()).ToArray();
    }

    private Dictionary<string, (int, int)> Recognize(ref IRecognizingSource source)
    {
        var threads = new List<Thread>();
        var outputLock = new object();
        var colorsPoints = new Dictionary<string, (int, int)>();

        foreach (var c in _colors)
        {
            var thread = new Thread(val =>
            {
                if (val == null) return;
                var threadParams = (ThreadParams)val;

                // todo
                // var point = RecognizeColor(threadParams.Color, ref source);

                // if (point == null) return;

                lock (outputLock)
                {
                    // colorsPoints[threadParams.ColorName] = point ?? (0, 0);
                }
            });

            threads.Add(thread);
            thread.Start(new ThreadParams
            {
                Color = c.Value,
                ColorName = c.Key
            });
        }

        // ждём завершения всех потоков
        foreach (var thread in threads)
            thread.Join();

        return colorsPoints;
    }

    private (int, int)? RecognizeColor(Color color, ref IRecognizingSource source)
    {
        // заполняем количества распознанных пикселей по рядам и столбцам
        var widthSummaries = Enumerable.Repeat(0, source.GetWidth()).ToArray();
        var heightSummaries = Enumerable.Repeat(0, source.GetHeight()).ToArray();

        for (var x = 0; x < source.GetWidth(); x++)
        {
            for (var y = 0; y < source.GetHeight(); y++)
            {
                var pixelColor = source.GetPixel(x, y);

                var recognized = color.Recognize(
                    (int)pixelColor.GetHue(),
                    (int)(pixelColor.GetSaturation() * 100),
                    (int)(pixelColor.GetBrightness() * 100)
                );

                if (!recognized) continue;

                widthSummaries[x] += 1;
                heightSummaries[y] += 1;
            }
        }

        var (maxRangeStart, maxRangeLength) = _getMaxRange(widthSummaries);
        // если не распознали цвет
        if (maxRangeStart == null || maxRangeLength == null) return null;

        var resultPointX = (maxRangeStart + maxRangeStart + maxRangeLength - 1) / 2;

        // если "пятно" нужного цвета по ширине меньше 1% от ширины картинки, то цвет считается нераспознанным
        if (maxRangeLength < source.GetWidth() / 100) return null;


        (maxRangeStart, maxRangeLength) = _getMaxRange(heightSummaries);
        // если не распознали цвет
        if (maxRangeStart == null || maxRangeLength == null) return null;

        var resultPointY = (maxRangeStart + maxRangeStart + maxRangeLength - 1) / 2;

        // если "пятно" нужного цвета по высоте меньше 1% от высоты картинки, то цвет считается нераспознанным
        if (maxRangeLength < source.GetHeight() / 100) return null;

        return (resultPointX ?? 0, resultPointY ?? 0);
    }

    public Bitmap SuperimposeRecognizedPoints(ref IRecognizingSource source,
        out Dictionary<string, bool> recognizedColors)
    {
        var colorsPoints = RecognizeInThreads(ref source);
        return DrawPoints(source, out recognizedColors, ref colorsPoints);
    }

    private Bitmap DrawPoints(
        IRecognizingSource source,
        out Dictionary<string, bool> recognizedColors,
        ref Dictionary<string, (int, int)> colorsPoints)
    {
        var recognized = new Dictionary<string, bool>();
        var result = (Bitmap)source.Clone();

        // рисуем "пятна" с распознанными точками
        using var grf = Graphics.FromImage(result);
        using Brush brsh = new SolidBrush(ColorTranslator.FromHtml("#ff00ffff"));
        foreach (var (colorName, pointCoords) in colorsPoints)
        {
            recognized[colorName] = true;

            // рисуем кружок
            var (x, y) = pointCoords;
            grf.FillEllipse(
                brsh,
                x - _pointHitBoxRadius / 2,
                y - _pointHitBoxRadius / 2,
                _pointHitBoxRadius,
                _pointHitBoxRadius
            );
        }

        recognizedColors = recognized;

        return result;
    }

    public void RecognizeCollisionsAndSuperimposePoints(
        IRecognizingSource source,
        out Dictionary<string, bool> recognizedColors,
        out Bitmap destination,
        out string[][] collisions
    )
    {
        var colorsPoints = RecognizeInThreads(ref source);
        collisions = RecognizeColorCollisionsByPoints(colorsPoints);
        destination = DrawPoints(source, out recognizedColors, ref colorsPoints);
    }

    // возвращает индекс начала и длину самого длинного диапазона из идущих подряд чисел, которые больше медианы массива
    private static (int?, int?) _getMaxRange(IReadOnlyList<int> summaries)
    {
        var filteredNumbers = summaries.Where(x => x > 0).ToArray();
        if (filteredNumbers.Length == 0)
        {
            return (null, null);
        }

        var median = filteredNumbers.OrderBy(x => x).ElementAt(filteredNumbers.Length / 2);

        var maxRangeStart = (int?)null;
        var maxRangeLength = (int?)null;
        var maxRangeSum = (int?)null;
        var currRangeStart = (int?)null;
        var currRangeLength = (int?)null;
        var currRangeSum = (int?)null;

        for (var i = 0; i < summaries.Count; i++)
        {
            if (summaries[i] > median)
            {
                currRangeStart ??= i; // это то же, что и if (currRangeStart == null) currRangeStart = i;
                currRangeLength = currRangeLength == null ? 1 : currRangeLength + 1;
                currRangeSum = currRangeSum == null ? summaries[i] : currRangeSum + summaries[i];
            }
            else if (currRangeStart != null)
            {
                if (maxRangeLength == null ||
                    currRangeLength > maxRangeLength ||
                    currRangeLength == maxRangeLength && currRangeSum > maxRangeSum)
                {
                    maxRangeStart = currRangeStart;
                    maxRangeLength = currRangeLength;
                    maxRangeSum = currRangeSum;
                }

                currRangeStart = null;
                currRangeLength = null;
            }
        }

        if (maxRangeLength == null || currRangeLength > maxRangeLength)
        {
            maxRangeStart = currRangeStart;
            maxRangeLength = currRangeLength;
        }

        // Console.WriteLine($"median: {median}, maxRangeStart: {maxRangeStart}, maxRangeLength: {maxRangeLength}");

        return (maxRangeStart ?? 0, maxRangeLength ?? 1);
    }


    private Thread[] _threads;
    private IRecognizingSource? _currentFrame;
    private Dictionary<string, (int, int)?> _currentResult;
    private bool _stopped;
    private readonly object _lockObject = new();
    private readonly object _lockForCopy = new();

    public void Configure()
    {
        if (!_stopped) Stop();

        _threads = new Thread[_colors.Count];
        var i = 0;
        foreach (var (key, value) in _colors)
        {
            var colorName = key;
            var colorValue = value;
            _threads[i] = new Thread(() => ProcessColor(colorName, colorValue));
            i++;
        }

        Start();
    }

    public void Start()
    {
        if (!_stopped) return;

        _stopped = false;
        foreach (var thread in _threads)
        {
            thread.Start();
        }
    }

    private void ProcessColor(string colorName, Color colorValue)
    {
        while (true)
        {
            IRecognizingSource frameToProcess;
            lock (_lockObject)
            {
                while (_currentFrame == null && !_stopped)
                {
                    Monitor.Wait(_lockObject);
                }

                if (_stopped)
                {
                    break;
                }

                lock (_lockForCopy)
                {
                    frameToProcess = new BitmapSynchronizer(
                        (Bitmap)(
                            _currentFrame ??
                            new BitmapSynchronizer(new Bitmap(0, 0))
                        ).Clone()
                    );
                }
            }

            // Здесь осуществляется обработка кадра для определенного цвета
            var result = RecognizeColor(colorValue, ref frameToProcess);

            // Отправка результата в основной поток
            lock (_lockObject)
            {
                _currentResult[colorName] = result;

                if (_currentResult.Count != _colors.Count) continue;

                _currentFrame = null; // Освобождаем кадр для следующей обработки
                Monitor.PulseAll(_lockObject); // Уведомляем основной поток
            }
        }
    }

    public Dictionary<string, (int, int)> RecognizeInThreads(ref IRecognizingSource source)
    {
        lock (_lockObject)
        {
            _currentResult = new Dictionary<string, (int, int)?>();

            while (_currentFrame != null)
                Monitor.Wait(_lockObject);

            _currentFrame = source;
            Monitor.PulseAll(_lockObject);

            while (_currentFrame != null)
                Monitor.Wait(_lockObject);
        }

        // Фильтруем и оставляем только распознанные точки
        var result = _currentResult
            .Where(kvp => kvp.Value.HasValue)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value ?? (0, 0));

        return result;
    }

    public void Stop()
    {
        lock (_lockObject)
        {
            _stopped = true;
            Monitor.PulseAll(_lockObject);
        }

        foreach (var thread in _threads)
        {
            thread.Join();
        }
    }
}