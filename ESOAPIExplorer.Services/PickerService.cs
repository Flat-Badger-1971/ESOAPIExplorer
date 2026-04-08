using ESOAPIExplorer.Views;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ESOAPIExplorer.Services;

#pragma warning disable CA1416
public class PickerService(MainWindow mainWindow) : IFilePickerService, IFolderPickerService
{
    private readonly MainWindow _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

    public async Task<StorageFile> PickSingleFileAsync(FileOpenPickerOptions options)
    {
        FileOpenPicker picker = new()
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = MapStartLocation(options?.SuggestedStartLocation ?? PickerStartLocation.DocumentsLibrary),
            CommitButtonText = options?.CommitButtonText,
        };

        foreach (string fileType in options?.FileTypeFilter ?? [])
        {
            picker.FileTypeFilter.Add(fileType);
        }

        EnsureDefaultFileTypeFilter(picker.FileTypeFilter);
        InitializeWithWindow(picker);

        return await picker.PickSingleFileAsync();
    }

    public async Task<StorageFile> PickSaveFileAsync(FileSavePickerOptions options)
    {
        FileSavePicker picker = new()
        {
            SuggestedStartLocation = MapStartLocation(options?.SuggestedStartLocation ?? PickerStartLocation.DocumentsLibrary),
            SuggestedFileName = options?.SuggestedFileName,
            CommitButtonText = options?.CommitButtonText,
        };

        foreach (KeyValuePair<string, IReadOnlyList<string>> fileTypeChoice in options?.FileTypeChoices ?? new Dictionary<string, IReadOnlyList<string>>())
        {
            picker.FileTypeChoices.Add(fileTypeChoice.Key, [.. fileTypeChoice.Value]);
        }

        InitializeWithWindow(picker);

        return await picker.PickSaveFileAsync();
    }

    public async Task<StorageFolder> PickSingleFolderAsync(FolderPickerOptions options)
    {
        FolderPicker picker = new()
        {
            SuggestedStartLocation = MapStartLocation(options?.SuggestedStartLocation ?? PickerStartLocation.DocumentsLibrary),
            CommitButtonText = options?.CommitButtonText,
        };

        foreach (string fileType in options?.FileTypeFilter ?? [])
        {
            picker.FileTypeFilter.Add(fileType);
        }

        EnsureDefaultFileTypeFilter(picker.FileTypeFilter);
        InitializeWithWindow(picker);

        return await picker.PickSingleFolderAsync();
    }

    private void InitializeWithWindow(object picker)
    {
        nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_mainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
    }

    private static PickerLocationId MapStartLocation(PickerStartLocation startLocation)
    {
        return startLocation switch
        {
            PickerStartLocation.ComputerFolder => PickerLocationId.ComputerFolder,
            _ => PickerLocationId.DocumentsLibrary,
        };
    }

    private static void EnsureDefaultFileTypeFilter(IList<string> fileTypeFilter)
    {
        if (!fileTypeFilter.Any())
        {
            fileTypeFilter.Add("*");
        }
    }
}
