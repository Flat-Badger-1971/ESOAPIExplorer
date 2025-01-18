using ESOAPIExplorer.DisplayModels;
using ESOAPIExplorer.Models;
using ESOAPIExplorer.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ESOAPIExplorer.ViewModels;

#pragma warning disable CA1416, CsWinRT1028
public class ExportViewModel(ILuaCheckRcGeneratorService _luaCheckRcGeneratorService, ILuaLanguageServerDefinitionsGeneratorService _luaLanguageServerDefinitionsGeneratorService) : ViewModelBase
{
    private DisplayModelBase<ExportOption> _selectedExportOption;

    public DisplayModelBase<ExportOption> SelectedExportOption
    {
        get => _selectedExportOption;
        set
        {
            SetProperty(ref _selectedExportOption, value);
        }
    }

    private readonly ObservableCollection<DisplayModelBase<ExportOption>> _ExportOptions =
    [
        new DisplayModelBase<ExportOption> { Value = new ExportOption {Id = 1, DisplayName = "Luacheck configuration file '.luacheckrc'" } },
        new DisplayModelBase<ExportOption> { Value = new ExportOption {Id = 2, DisplayName = "lua-language-server definitions file 'esoapi.lua'" } }
    ];

    public ObservableCollection<DisplayModelBase<ExportOption>> ExportOptions
    {
        get => _ExportOptions;
    }

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

    public ICommand ExportCommand => new RelayCommand(async () =>
    {
        switch (SelectedExportOption.Value.Id)
        {
            case 1:
                await ExportLuaCheck();
                break;
            case 2:
                await ExportLLSDefinition();
                break;
            default:
                break;
        }
    });

    public async Task ExportLuaCheck()
    {
        StorageFolder folder = await _FolderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            StringBuilder luacheckrc = _luaCheckRcGeneratorService.Generate();
            StorageFile file = await folder.CreateFileAsync(".luacheckrc", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, luacheckrc.ToString());
        }
    }

    // TODO: add not to user to add the path to workspace.library (string array) in .luarc.json
    public async Task ExportLLSDefinition()
    {
        StorageFolder folder = await _FolderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            StringBuilder llsdefinition = _luaLanguageServerDefinitionsGeneratorService.Generate();
            StorageFile file = await folder.CreateFileAsync("eso_lls.def", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, llsdefinition.ToString());
        }
    }
}
