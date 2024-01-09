namespace CCR;

public class Color
{
    public Range[] HueRanges;
    public Range[] SaturationRanges;
    public Range[] BrightnessRanges;

    public Color(Range[] hue, Range[] saturation, Range[] brightness)
    {
        // validation
        if (hue.Any(range => range.Min < 0 || range.Max > 360))
            throw new ArgumentException("hue cant be < 0 or > 360");
        
        if (saturation.Any(range => range.Min < 0 || range.Max > 100))
            throw new ArgumentException("saturation cant be < 0 or > 100");
        
        if (brightness.Any(range => range.Min < 0 || range.Max > 100))
            throw new ArgumentException("hue cant be < 0 or > 100");

        HueRanges = hue;
        SaturationRanges = saturation;
        BrightnessRanges = brightness;
    }
    
    public object Clone()
    {
        return this.MemberwiseClone();
    }

    public bool Recognize(int hue, int saturation, int brightness)
    {
        var hueCollides = HueRanges.Any(hueRange => hueRange.Includes(hue));
        if (!hueCollides) return false;


        var saturationCollides = SaturationRanges.Any(saturationRange => saturationRange.Includes(saturation));
        if (!saturationCollides) return false;


        var brightnessCollides = BrightnessRanges.Any(brightnessRange => !brightnessRange.Includes(brightness));

        return brightnessCollides;
    }
}