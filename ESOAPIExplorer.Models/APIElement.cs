namespace ESOAPIExplorer.Models;

public class APIElement
{
    public string Id { get; set; }
    public string Name { get; set; }
    public APIElementType ElementType { get; set; }
    public string Parent { get; set; }
}
