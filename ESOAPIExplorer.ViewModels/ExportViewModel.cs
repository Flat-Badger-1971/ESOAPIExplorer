using ESOAPIExplorer.Services;
using Microsoft.UI.Xaml;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ESOAPIExplorer.ViewModels;

#pragma warning disable CA1416, CsWinRT1028
public class ExportViewModel(ILuaCheckRcGeneratorService luaCheckRcGeneratorService) : ViewModelBase
{
    private readonly Window _MainWindow = (Window)Application.Current.GetType().GetProperty("MainWindow").GetValue(Application.Current);
    private FolderPicker _FolderPicker;

    public override async Task InitializeAsync(object data)
    {
        await base.InitializeAsync(data);

        // Get the current window's HWND by passing in the Window object        
        nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_MainWindow);

        _FolderPicker = new FolderPicker
        {
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            CommitButtonText = "Select Folder"
        };

        _FolderPicker.FileTypeFilter.Add("*");

        // Associate the HWND with the folder picker
        WinRT.Interop.InitializeWithWindow.Initialize(_FolderPicker, hwnd);
    }

    // public ICommand exportLuchCheck = new RelayCommand(async () => await ExportLuaCheckFile());

    private async Task ExportLuaCheckFile()
    {
        StorageFolder folder = await _FolderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            StringBuilder luacheckrc = luaCheckRcGeneratorService.Generate();
            StorageFile file = await folder.CreateFileAsync(".luacheckrc", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, luacheckrc.ToString());
        }
    }
}
