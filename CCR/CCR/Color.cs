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
}