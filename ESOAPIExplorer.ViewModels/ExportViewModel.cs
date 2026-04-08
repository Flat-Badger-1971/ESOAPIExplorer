using ESOAPIExplorer.Models;
using ESOAPIExplorer.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;

namespace ESOAPIExplorer.ViewModels;

#pragma warning disable CA1416, CsWinRT1028
public class ExportViewModel(ILuaCheckRcGeneratorService _luaCheckRcGeneratorService, ILuaLanguageServerDefinitionsGeneratorService _luaLanguageServerDefinitionsGeneratorService, IFolderPickerService folderPickerService) : ViewModelBase
{
    private string _statusMessage;

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private readonly ObservableCollection<ExportOption> _ExportOptions =
    [
        new ExportOption { Id = 1, DisplayName = "Luacheck configuration", Description = "Writes a project `.luacheckrc` with ESO globals.", OutputTarget = ".luacheckrc", IsSelected = true },
        new ExportOption { Id = 2, DisplayName = "Lua language server definitions", Description = "Generates the Lua definition files used for completions and type information.", OutputTarget = "aliases.lua, api.lua, classes.lua, events.lua, functions.lua, globals.lua, sounds.lua", IsSelected = true },
        new ExportOption { Id = 3, DisplayName = "Lua language server diagnostics globals", Description = "Merges ESO globals into `.luarc.json` under `diagnostics.globals`.", OutputTarget = ".luarc.json", IsSelected = true }
    ];

    public ObservableCollection<ExportOption> ExportOptions
    {
        get => _ExportOptions;
    }

    public override async Task InitializeAsync(object data)
    {
        await base.InitializeAsync(data);
        StatusMessage = "Choose one or more export targets, then pick an output folder.";
    }

    public ICommand ExportCommand => new RelayCommand(() => _ = ExportAsync());

    private async Task ExportAsync()
    {
        bool exportLuaCheck = ExportOptions.Any(option => option.Id == 1 && option.IsSelected);
        bool exportDefinitions = ExportOptions.Any(option => option.Id == 2 && option.IsSelected);
        bool exportDiagnosticsGlobals = ExportOptions.Any(option => option.Id == 3 && option.IsSelected);

        if (!exportLuaCheck && !exportDefinitions && !exportDiagnosticsGlobals)
        {
            StatusMessage = "Select at least one export option.";
            return;
        }

        SetBusyState(true, "Exporting files...");

        StorageFolder folder = await folderPickerService.PickSingleFolderAsync(new FolderPickerOptions
        {
            SuggestedStartLocation = PickerStartLocation.ComputerFolder,
            CommitButtonText = "Select Folder",
            FileTypeFilter = ["*"]
        });

        if (folder == null)
        {
            StatusMessage = "Export cancelled.";
            SetBusyState(false);
            return;
        }

        try
        {
            if (exportLuaCheck)
            {
                await ExportLuaCheckAsync(folder);
            }

            if (exportDefinitions || exportDiagnosticsGlobals)
            {
                await _luaLanguageServerDefinitionsGeneratorService.Generate(folder, exportDefinitions, exportDiagnosticsGlobals);
            }

            StatusMessage = $"Export completed to {folder.Path}.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            SetBusyState(false);
        }
    }

    private async Task ExportLuaCheckAsync(StorageFolder folder)
    {
        StringBuilder luacheckrc = _luaCheckRcGeneratorService.Generate();
        StorageFile file = await folder.CreateFileAsync(".luacheckrc", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(file, luacheckrc.ToString());
    }
}
