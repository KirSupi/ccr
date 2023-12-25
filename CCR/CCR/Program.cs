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
         Dictionary<string, Bitmap> masks = new Dictionary<string, Bitmap>();
         
         foreach ((string name, Color color) in _colors)
         {
             masks[name] = new Bitmap(img);
         }
        return Array.Empty<string[]>();
    }

    static void Main()
    {
    }
}