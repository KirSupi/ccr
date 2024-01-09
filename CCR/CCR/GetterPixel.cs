using System.Drawing;

namespace CCR;

public interface IRecognizingSource
{
    System.Drawing.Color GetPixel(int x, int y);
    int GetWidth();
    int GetHeight();
    object Clone();
}

public class BitmapWrapper : IRecognizingSource
{
    private readonly Bitmap _source;

    public BitmapWrapper(Bitmap source)
    {
        _source = source;
    }

    public System.Drawing.Color GetPixel(int x, int y) => _source.GetPixel(x, y);
    public int GetWidth() => _source.Width;
    public int GetHeight() => _source.Height;
    public object Clone() => _source.Clone();
}