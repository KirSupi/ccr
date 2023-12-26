namespace CCR;

public class Color
{
    public Range[] HueRanges;
    public Range[] SaturationRanges;
    public Range[] BrightnessRanges;

    public Color(Range[] hue, Range[] saturation, Range[] brightness)
    {
        // validation
        foreach (var range in hue)
            if (range.Min < 0 || range.Max > 360)
                throw new ArgumentException("hue cant be < 0 or > 360");
        foreach (var range in saturation)
            if (range.Min < 0 || range.Max > 100)
                throw new ArgumentException("saturation cant be < 0 or > 100");
        foreach (var range in brightness)
            if (range.Min < 0 || range.Max > 100)
                throw new ArgumentException("hue cant be < 0 or > 100");

        HueRanges = hue;
        SaturationRanges = saturation;
        BrightnessRanges = brightness;
    }

    public bool Recognize(int hue, int saturation, int brightness)
    {
        var hueCollides = false;

        foreach (var hueRange in HueRanges)
        {
            if (!hueRange.Includes(hue)) continue;

            hueCollides = true;
            break;
        }

        if (!hueCollides) return false;


        var saturationCollides = false;

        foreach (var saturationRange in SaturationRanges)
        {
            if (!saturationRange.Includes(saturation)) continue;

            saturationCollides = true;
            break;
        }

        if (!saturationCollides) return false;


        var brightnessCollides = false;

        foreach (var brightnessRange in BrightnessRanges)
        {
            if (brightnessRange.Includes(brightness)) continue;

            brightnessCollides = true;
            break;
        }

        if (!brightnessCollides) return false;


        return true;
    }
}