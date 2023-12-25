namespace CCR;

public class Color
{
    public Range[] HueRanges;
    public Range[] SaturationRanges;
    public Range[] BrightnessRanges;

    public Color(Range[] hue, Range[] saturation, Range[] brightness)
    {
        // todo add params validation
        HueRanges = hue;
        SaturationRanges = saturation;
        BrightnessRanges = brightness;
    }
    
    public bool Recognize(int hue, int saturation, int brightness)
    {
        var hueCollides = false;
        
        foreach (var hueRange in HueRanges)
        {
            if (hueRange.Includes(hue))
            {
                hueCollides = true;
                break;
            }
        }

        if (!hueCollides)
        {
            return false;
        }
        
        var saturationCollides = false;
        
        foreach (var saturationRange in SaturationRanges)
        {
            if (saturationRange.Includes(saturation))
            {
                saturationCollides = true;
                break;
            }
        }

        if (!saturationCollides)
        {
            return false;
        }
        
        var brightnessCollides = false;
        
        foreach (var brightnessRange in BrightnessRanges)
        {
            if (brightnessRange.Includes(brightness))
            {
                brightnessCollides = true;
                break;
            }
        }

        if (!brightnessCollides)
        {
            return false;
        }
        
        return true;
    }
}