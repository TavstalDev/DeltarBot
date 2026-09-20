namespace Tavstal.DeltarBot.Extensions;

public static class NumberExtensions
{
    private static readonly string[] suffixes = ["", "K", "M", "B", "T", "Q"];
    
    public static string ToShortString(this int value)
    {
        int magnitude = 0;
        while (Math.Abs(value) >= 1000 && magnitude < suffixes.Length - 1)
        {
            magnitude++;
            value /= 1000;
        }
        return $"{value:0.##} {suffixes[magnitude]}".Trim();
    }
    
    public static string ToShortString(this uint value)
    {
        int magnitude = 0;
        while (value >= 1000 && magnitude < suffixes.Length - 1)
        {
            magnitude++;
            value /= 1000;
        }
        return $"{value:0.##} {suffixes[magnitude]}".Trim();
    }
    
    public static string ToShortString(this long value)
    {
        int magnitude = 0;
        while (Math.Abs(value) >= 1000 && magnitude < suffixes.Length - 1)
        {
            magnitude++;
            value /= 1000;
        }
        return $"{value:0.##} {suffixes[magnitude]}".Trim();
    }
    
    public static string ToShortString(this ulong value)
    {
        int magnitude = 0;
        while (value >= 1000 && magnitude < suffixes.Length - 1)
        {
            magnitude++;
            value /= 1000;
        }
        return $"{value:0.##} {suffixes[magnitude]}".Trim();
    }
    
    public static string ToShortString(this double value)
    {
        int magnitude = 0;
        while (Math.Abs(value) >= 1000 && magnitude < suffixes.Length - 1)
        {
            magnitude++;
            value /= 1000;
        }
        return $"{value:0.##} {suffixes[magnitude]}".Trim();
    }
}