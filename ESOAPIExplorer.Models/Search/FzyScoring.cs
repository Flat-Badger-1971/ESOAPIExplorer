namespace ESOAPIExplorer.Models.Search;

internal record FzyScoring
{
    public FzyGapScoring Gap { get; init; }
    public FzyMatchScoring Match { get; init; }
    public double Max { get; init; }
    public double Min { get; init; }
}
