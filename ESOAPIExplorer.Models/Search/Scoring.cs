namespace ESOAPIExplorer.Models.Search;

internal static class Scoring
{
    public static readonly double Max = double.PositiveInfinity;
    public static readonly double Min = double.NegativeInfinity;
    public static readonly int MaxMatchLength = 1024;

    public static readonly FzyGapScoring Gap = new FzyGapScoring
    {
        Leading = -0.005,
        Trailing = -0.005,
        Inner = -0.01
    };

    public static readonly FzyMatchScoring Match = new FzyMatchScoring
    {
        Consecutive = 1.0,
        Slash = 0.9,
        Word = 0.8,
        Capital = 0.7,
        Dot = 0.6
    };
}
