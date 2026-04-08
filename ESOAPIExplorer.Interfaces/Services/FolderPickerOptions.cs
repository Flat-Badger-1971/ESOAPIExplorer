using System.Collections.Generic;

namespace ESOAPIExplorer.Services;

public class FolderPickerOptions
{
    public PickerStartLocation SuggestedStartLocation { get; set; } = PickerStartLocation.DocumentsLibrary;
    public string CommitButtonText { get; set; }
    public IReadOnlyList<string> FileTypeFilter { get; set; } = [];
}
