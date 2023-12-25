namespace CCR;

public class Range
{
    public int Min { get; set; } = 0;
    public int Max { get; set; } = 0;

    public Range(int min, int max)
    {
        Min = min;
        Max = max;
    }
}