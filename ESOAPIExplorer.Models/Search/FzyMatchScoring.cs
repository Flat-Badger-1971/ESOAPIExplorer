namespace ESOAPIExplorer.Models.Search;

internal record FzyMatchScoring
{
    public double Consecutive { get; init; }
    public double Slash { get; init; }
    public double Word { get; init; }
    public double Capital { get; init; }
    public double Dot { get; init; }
}
