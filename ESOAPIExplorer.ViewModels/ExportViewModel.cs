using ESOAPIExplorer.DisplayModels;
using ESOAPIExplorer.Models;
using ESOAPIExplorer.Services;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ESOAPIExplorer.ViewModels;

#pragma warning disable CA1416, CsWinRT1028
public class ExportViewModel(ILuaCheckRcGeneratorService _luaCheckRcGeneratorService, ILuaLanguageServerDefinitionsGeneratorService _luaLanguageServerDefinitionsGeneratorService) : ViewModelBase
{
    //private DisplayModelBase<ExportOption> _selectedExportOption;

    //public DisplayModelBase<ExportOption> SelectedExportOption
    //{
    //    get => _selectedExportOption;
    //    set
    //    {
    //        SetProperty(ref _selectedExportOption, value);
    //    }
    //}

    //private readonly ObservableCollection<DisplayModelBase<ExportOption>> _ExportOptions =
    //[
    //    new DisplayModelBase<ExportOption> { Value = new ExportOption {Id = 1, DisplayName = "Luacheck configuration file '.luacheckrc'" } },
    //    new DisplayModelBase<ExportOption> { Value = new ExportOption {Id = 2, DisplayName = "lua-language-server globals '.luarc.json'" } }
    //];

    //public ObservableCollection<DisplayModelBase<ExportOption>> ExportOptions
    //{
    //    get => _ExportOptions;
    //}

    //private readonly Window _MainWindow = (Window)Application.Current.GetType().GetProperty("MainWindow").GetValue(Application.Current);
    //private FolderPicker _FolderPicker;

    //public override async Task InitializeAsync(object data)
    //{
    //    await base.InitializeAsync(data);

    //    // Get the current window's HWND by passing in the Window object        
    //    nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_MainWindow);

    //    _FolderPicker = new FolderPicker
    //    {
    //        SuggestedStartLocation = PickerLocationId.ComputerFolder,
    //        CommitButtonText = "Select Folder"
    //    };

    //    _FolderPicker.FileTypeFilter.Add("*");

    //    // Associate the HWND with the folder picker
    //    WinRT.Interop.InitializeWithWindow.Initialize(_FolderPicker, hwnd);
    //}

    public ICommand ExportCommand => new RelayCommand(async () =>
    {
        //switch (SelectedExportOption.Value.Id)
        //{
        //    case 1:
        //        await ExportLuaCheck();
        //        break;
        //    case 2:
                await ExportLLSDefinition();
        //        break;
        //    default:
        //        break;
        //}
    });

    public async Task ExportLuaCheck()
    {
        //StorageFolder folder = await _FolderPicker.PickSingleFolderAsync();

        //if (folder != null)
        //{
        //    StringBuilder luacheckrc = _luaCheckRcGeneratorService.Generate();
        //    StorageFile file = await folder.CreateFileAsync(".luacheckrc", CreationCollisionOption.ReplaceExisting);
        //    await FileIO.WriteTextAsync(file, luacheckrc.ToString());
        //}
    }

    public async Task ExportLLSDefinition()
    {
        // TODO: change this so all entries are added to the .luarc.json file in the "diagnostics.globals" section
        //StorageFolder folder = await _FolderPicker.PickSingleFolderAsync();

        //if (folder != null)
        //{
        //    await _luaLanguageServerDefinitionsGeneratorService.Generate(folder);            
        //}
    }
}
