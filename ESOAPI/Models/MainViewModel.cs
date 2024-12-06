using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using ESOAPI.Parser;
using System;
using ESOAPI.Helpers;

namespace ESOAPI.Models;

public class MainViewModel : BindableBase
{
    private readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;
    private readonly FileOpenPicker _filePicker;
    private EsoUIDocumentation _documentation;

    private List<APIElement> _events;
    public List<APIElement> Events
    {
        get => _events;
        set => SetProperty(ref _events, value);
    }

    private List<APIElement> _functions;
    public List<APIElement> Functions
    {
        get => _functions;
        set => SetProperty(ref _functions, value);
    }

    private List<APIElement> _constants;
    public List<APIElement> Constants
    {
        get => _constants;
        set => SetProperty(ref _constants, value);
    }

    public MainViewModel()
    {
        _filePicker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        _filePicker.FileTypeFilter.Add(".txt");

        Events = [];
        Functions = [];
        Constants = [];
    }

    public async Task InitialiseAsync()
    {
        _documentation = await GetDocumentationAsync();
        Events = _documentation.Events
            .Select(item => new APIElement { Id = item.Key, Name = item.Value.Name })
            .OrderBy(e => e.Name)
            .ToList();

        Functions = _documentation.Functions
            .Select(item => new APIElement { Id = item.Key, Name = item.Value.Name })
            .OrderBy(f => f.Name)
            .ToList();

        Constants = _documentation.Globals
            .SelectMany(item => item.Value.Select(detail => new APIElement { Id = detail, Name = detail }))
            .OrderBy(c => c.Name)
            .ToList();
    }

    private async Task<EsoUIDocumentation> GetDocumentationAsync()
    {
        string path = await GetPathAsync();
        EsoUIDocumentParser parser = new EsoUIDocumentParser();
        return await parser.ParseAsync(path);
    }

    private async Task<string> GetPathAsync()
    {
        string path = _settings.Values["last path"] as string;

        if (string.IsNullOrEmpty(path))
        {
            StorageFile file = await _filePicker.PickSingleFileAsync();
            if (file != null)
            {
                _settings.Values["last path"] = file.Path;
                path = file.Path;
            }
        }
        else if (!System.IO.File.Exists(path))
        {
            _settings.Values["last path"] = null;
            path = await GetPathAsync();
        }

        return path;
    }
}
