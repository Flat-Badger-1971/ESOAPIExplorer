namespace ESOAPIExplorer.Models.Search;

internal record FzyGapScoring
{
    public double Leading { get; init; }
    public double Trailing { get; init; }
    public double Inner { get; init; }
}
