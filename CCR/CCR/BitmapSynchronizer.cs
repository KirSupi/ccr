using System.Drawing;

namespace CCR;

public class BitmapSynchronizer : IRecognizingSource
{
    private readonly Bitmap _source;

    // private readonly Mutex _sourceMutex = new ();
    private readonly object _sourceLock = new();

    public BitmapSynchronizer(Bitmap source)
    {
        _source = source;
    }

    public System.Drawing.Color GetPixel(int x, int y)
    {
        lock (_sourceLock)
        {
            // _sourceMutex.WaitOne();
            // var data = _source.LockBits(
            //     new Rectangle(
            //         new Point(x, y),
            //         new Size(1, 1)
            //     ),
            //     System.Drawing.Imaging.ImageLockMode.ReadOnly,
            //     _source.PixelFormat
            // );
            var result = _source.GetPixel(x, y);
            // _source.UnlockBits(data);
            // _sourceMutex.ReleaseMutex();
            return result;
        }
    }

    public int GetWidth()
    {
        lock (_sourceLock)
        {
            // _sourceMutex.WaitOne();
            var result = _source.Width;
            // _sourceMutex.ReleaseMutex();

            return result;
        }
    }

    public int GetHeight()
    {
        lock (_sourceLock)
        {
            // _sourceMutex.WaitOne();
            var result = _source.Height;
            // _sourceMutex.ReleaseMutex();

            return result;
        }
    }

    public object Clone()
    {
        lock (_sourceLock)
        {
            // _sourceMutex.WaitOne();
            var result = _source.Clone();
            // _sourceMutex.ReleaseMutex();

            return result;
        }
    }
}