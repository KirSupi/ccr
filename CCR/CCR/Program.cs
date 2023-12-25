namespace CCR;
using System.Drawing;
class Recognizer
{
    private Dictionary<string, Color> _colors;

    public Recognizer(Dictionary<string, Color> colors)
    {
        _colors = colors;
    }

    public string[][] RecognizeColorCollisions(Bitmap img)
    {
         Dictionary<string, Bitmap> masks;
         foreach ((string name, Color color) in _colors)
         {
             if
         }
        return Array.Empty<string[]>();
    }

    static void Main()
    {
    }
}