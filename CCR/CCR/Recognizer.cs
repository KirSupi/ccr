namespace CCR;

using System.Drawing;

public class Recognizer
{
    private Dictionary<string, Color> _colors;

    public Recognizer(Dictionary<string, Color> colors)
    {
        _colors = colors;
    }

    public string[][] RecognizeColorCollisions(Bitmap img)
    {
        var masks = new Dictionary<string, Bitmap>();

        foreach (var (name, color) in _colors)
        {
            masks[name] = new Bitmap(img);
        }

        return Array.Empty<string[]>();
    }

    public Bitmap SuperimposeRecognizedPoints(Bitmap source, out Dictionary<string, bool> recognizedColors)
    {
        var threads = new List<Thread>();
        var outputLock = new object();
        var resultColors = new Dictionary<string, bool>();
        var colorsPoints = new Dictionary<string, (int, int)>();
        var result = (Bitmap)source.Clone();

        foreach (var c in _colors)
        {
            var thread = new Thread(val =>
            {
                if (val == null) return;

                var threadParams = (ThreadParams)val;

                // var mask = new bool[source.Width][];
                var widthSummaries = Enumerable.Repeat(0, threadParams.Source.Width).ToArray();
                var heightSummaries = Enumerable.Repeat(0, threadParams.Source.Height).ToArray();

                for (var x = 0; x < threadParams.Source.Width; x++)
                {
                    // mask[x] = new bool[source.Height];

                    for (var y = 0; y < threadParams.Source.Height; y++)
                    {
                        var pixelColor = threadParams.Source.GetPixel(x, y);
                        var recognized = threadParams.Color.Recognize(
                            (int)pixelColor.GetHue(),
                            (int)(pixelColor.GetSaturation() * 100),
                            (int)(pixelColor.GetBrightness() * 100)
                        );

                        if (!recognized) continue;

                        widthSummaries[x] += 1;
                        heightSummaries[y] += 1;

                        // mask[x][y] = recognized;
                    }
                }
                // Console.WriteLine("widthSummaries");
                // Console.WriteLine(string.Join(", ", widthSummaries.Select(v=>v.ToString())));
                // Console.WriteLine("heightSummaries");
                // Console.WriteLine(string.Join(", ", heightSummaries.Select(v=>v.ToString())));

                var (maxRangeStart, maxRangeLength) = _getMaxRange(widthSummaries);
                // если не распознали цвет
                if (maxRangeStart == null || maxRangeLength == null)
                {
                    lock (outputLock)
                    {
                        resultColors[threadParams.ColorName] = false;
                    }

                    return;
                }
                
                var resultPointX = (maxRangeStart + maxRangeStart + maxRangeLength - 1) / 2;

                // если "пятно" нужного цвета по ширине меньше 1% от ширины картинки, то цвет считается нераспознанным
                if (maxRangeLength < threadParams.Source.Width / 100)
                {
                    lock (outputLock)
                    {
                        resultColors[threadParams.ColorName] = false;
                    }

                    return;
                }

                (maxRangeStart, maxRangeLength) = _getMaxRange(heightSummaries);
                // если не распознали цвет
                if (maxRangeStart == null || maxRangeLength == null)
                {
                    lock (outputLock)
                    {
                        resultColors[threadParams.ColorName] = false;
                    }

                    return;
                }
                
                var resultPointY = (maxRangeStart + maxRangeStart + maxRangeLength - 1) / 2;

                // если "пятно" нужного цвета по высоте меньше 1% от высоты картинки, то цвет считается нераспознанным
                if (maxRangeLength < threadParams.Source.Height / 100)
                {
                    lock (outputLock)
                    {
                        resultColors[threadParams.ColorName] = false;
                    }

                    return;
                }


                lock (outputLock)
                {
                    colorsPoints[threadParams.ColorName] = (resultPointX ?? 0, resultPointY ?? 0);
                    resultColors[threadParams.ColorName] = true;
                }
            });

            threads.Add(thread);
            thread.Start(new ThreadParams{
                Source = (Bitmap)source.Clone(),
                Color = (Color)c.Value.Clone(),
                ColorName = c.Key
            });
        }

        // ждём завершения всех потоков
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // рисуем "пятна" с распознанными точками
        using var grf = Graphics.FromImage(result);
        using Brush brsh = new SolidBrush(ColorTranslator.FromHtml("#ff00ffff"));
        foreach (var (colorName, pointCoords) in colorsPoints)
        {
            var (x, y) = pointCoords;
            // Console.WriteLine($"point x: {x}, y: {y}");
            grf.FillEllipse(brsh, x-25, y-25, 50, 50);
        }

        recognizedColors = resultColors;

        return result;
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
}