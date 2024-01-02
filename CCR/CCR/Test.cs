using System.Drawing;

namespace CCR;

public class Test
{
    static void Main()
    {
        var redColor = new Color(
            new[]
            {
                new Range(0, 15), new Range(340, 360)
            },
            new[]
            {
                new Range(90, 100)
            },
            new[]
            {
                new Range(80, 100)
            }
        );
        var colors = new Dictionary<string, Color>();
        colors["red"] = redColor;
        
        var r = new Recognizer(colors);

        var path = "C:\\Users\\KirSu\\Desktop\\screenshotik.jpg";
        var img = new Bitmap(Image.FromFile(path));

        var recognizedColors = new Dictionary<string, bool>();
        var resultImg = r.SuperimposeRecognizedPoints(img, out recognizedColors);
        // Console.WriteLine(recognizedColors.ToString());
        resultImg.Save("C:\\Users\\KirSu\\Desktop\\recognized_points.jpg");
    }
}