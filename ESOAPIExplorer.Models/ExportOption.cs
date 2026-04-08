namespace ESOAPIExplorer.Models
{
    public class ExportOption
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string OutputTarget { get; set; }
        public bool IsSelected { get; set; }
    }
}
