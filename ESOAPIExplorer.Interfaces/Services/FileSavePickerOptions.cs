using System.Collections.Generic;

namespace ESOAPIExplorer.Services;

public class FileSavePickerOptions
{
    public PickerStartLocation SuggestedStartLocation { get; set; } = PickerStartLocation.DocumentsLibrary;
    public string SuggestedFileName { get; set; }
    public string CommitButtonText { get; set; }
    public IReadOnlyDictionary<string, IReadOnlyList<string>> FileTypeChoices { get; set; } = new Dictionary<string, IReadOnlyList<string>>();
}
