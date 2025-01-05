namespace ESOAPIExplorer.Models;

public class EsoUIInstance(string name, string InstanceOf, string code, string file)
{
    public string InstanceOf { get; set; } = InstanceOf;
    public string Name { get; set; } = name;
    public string Code { get; set; } = code;
    public string File { get; set; } = file;
}
