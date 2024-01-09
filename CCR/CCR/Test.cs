using System.Drawing;
using System.Text.Json;

namespace CCR;

public class Test
{
    static void Main()
    {
        // Console.WriteLine(JsonSerializer.Serialize(res, new JsonSerializerOptions()));
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

        var path = "C:\\Users\\KirSu\\Desktop\\one_more_balloon.jpg";
        var img = new Bitmap(Image.FromFile(path));

        var recognizedColors = new Dictionary<string, bool>();
        var syncedImg = (IRecognizingSource)new BitmapSynchronizer(img);
        r.Start();

        var resultImg = r.SuperimposeRecognizedPoints(ref syncedImg, out recognizedColors);

        var threads = new Thread[20];
        for (var i = 0; i < 20; i++)
        {
            threads[i] = new Thread(() =>
            {
                r.SuperimposeRecognizedPoints(ref syncedImg, out recognizedColors);
                r.SuperimposeRecognizedPoints(ref syncedImg, out recognizedColors);
                r.SuperimposeRecognizedPoints(ref syncedImg, out recognizedColors);
                r.SuperimposeRecognizedPoints(ref syncedImg, out recognizedColors);
                r.SuperimposeRecognizedPoints(ref syncedImg, out recognizedColors);
                r.SuperimposeRecognizedPoints(ref syncedImg, out recognizedColors);
                r.SuperimposeRecognizedPoints(ref syncedImg, out recognizedColors);
                r.SuperimposeRecognizedPoints(ref syncedImg, out recognizedColors);
                r.SuperimposeRecognizedPoints(ref syncedImg, out recognizedColors);
            });
            threads[i].Start();
        }
        
        foreach (var thread in threads)
        {
            thread.Join();
        }
        
        r.Stop();
        // Console.WriteLine(recognizedColors.ToString());
        Console.WriteLine(JsonSerializer.Serialize(recognizedColors, new JsonSerializerOptions()));
        resultImg.Save("C:\\Users\\KirSu\\Desktop\\recognized_points.jpg");
    }
}