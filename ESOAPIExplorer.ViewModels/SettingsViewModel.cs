using ESOAPIExplorer.Services;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ESOAPIExplorer.ViewModels;

#pragma warning disable CA1416, CsWinRT1028
public class SettingsViewModel(ILuaParserService luaParserService, ISettingsService settingsService) : ViewModelBase
{
    private readonly Window _mainWindow = (Window)Application.Current.GetType().GetProperty("MainWindow").GetValue(Application.Current);
    private readonly ISettingsService _settingsService = settingsService;
    //private FileOpenPicker _FileOpenPicker;
    //private FileSavePicker _FileSavePicker;
    private ComboBoxItem _selectedThemeName;
    private int _selectedAlgorithmIndex;
    private DateTime _lastScanDateTime;
    private ObservableCollection<string> _themes;

    public int SelectedAlgorithmIndex
    {
        get => _selectedAlgorithmIndex;
        set
        {
            if (value > -1)
            {
                _settingsService.SaveSetting("SearchAlgorithm", SearchAlgorithmItemSource[value]);
                SetProperty(ref _selectedAlgorithmIndex, value);
            }
        }
    }

    public string SelectedThemeName
    {
        get => _selectedThemeName?.Content.ToString();
        set
        {
            if (value != null)
            {
                _settingsService.SaveSetting("ThemeName", value);
                SetProperty(ref _selectedThemeName, new ComboBoxItem { Content = value });

                // change theme
                if (Enum.TryParse(value, out ApplicationTheme theme))
                {
                    // themeService.SetTheme(theme);
                    ThemeManager.Current.ApplicationTheme = theme;
                }
            }
        }
    }

    public DateTime LastScanDateTime
    {
        get => _lastScanDateTime;
        set
        {
            _settingsService.SaveSetting("LastScanDateTime", value);
            SetProperty(ref _lastScanDateTime, value);
        }
    }

    private ObservableCollection<string> _SearchAlgorithmItemSource;
    public ObservableCollection<string> SearchAlgorithmItemSource
    {
        get => _SearchAlgorithmItemSource;
        set => SetProperty(ref _SearchAlgorithmItemSource, value);
    }

    public ObservableCollection<string> Themes
    {
        get => _themes;
        set {
            SetProperty(ref _themes, value);
        }
    }

    public override async Task InitializeAsync(object data)
    {
        // Get the current window's HWND by passing in the Window object        
        //nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_MainWindow);

        //_FileOpenPicker = new FileOpenPicker
        //{
        //    ViewMode = PickerViewMode.List,
        //    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        //    CommitButtonText = "Open merTorchbug.lua"
        //};

        //_FileOpenPicker.FileTypeFilter.Add(".lua");

        //_FileSavePicker = new FileSavePicker
        //{
        //    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        //    SuggestedFileName = "ConstantValues",
        //    CommitButtonText = "Save ConstantValues.cs"
        //};

        //_FileSavePicker.FileTypeChoices.Add("C Sharp Class", [".cs"]);

        //// Associate the HWND with the file pickers
        //WinRT.Interop.InitializeWithWindow.Initialize(_FileOpenPicker, hwnd);
        //WinRT.Interop.InitializeWithWindow.Initialize(_FileSavePicker, hwnd);

        await base.InitializeAsync(data);

        if (_settingsService.GetSetting<string>("SearchAlgorithm") == null)
        {
            _settingsService.SaveSetting("SearchAlgorithm", "Fast Fuzzy");
        }

        List<Type> searchAlgorithms = Utility.ListSearchAlgorithms();

        SearchAlgorithmItemSource = new ObservableCollection<string>(
            searchAlgorithms
                .Select(a => a.GetPropertyValue("Name"))
                .OrderBy(a => a)
            );

        SelectedAlgorithmIndex = SearchAlgorithmItemSource.IndexOf(_settingsService.GetSetting<string>("SearchAlgorithm"));

        Themes =
        [
            "Light",
            "Dark"
            // "SystemDefault"
        ];

        string defaultTheme = _settingsService.GetSetting("ThemeName", "Dark");
        SelectedThemeName = Themes.FirstOrDefault(t => t == defaultTheme) ?? Themes.First();

        if (Enum.TryParse(defaultTheme, out ApplicationTheme theme))
        {
            ThemeManager.Current.ApplicationTheme = theme;
        }
    }

    public ICommand Generate => new RelayCommand(async () =>
    {
        // Select merTorchbug saved vars file
       //  StorageFile merFilePath = await _FileOpenPicker.PickSingleFileAsync();

        // Parse the file
        //string content = File.ReadAllText(merFilePath.Path).Replace("\r", "");
        //Dictionary<string, Models.EsoUIGlobalValue> parsedData = luaParserService.ParseLuaContent(content);

        // Generate and save the file
        //StorageFile saveLocation = await _FileSavePicker.PickSaveFileAsync();
        //CodeGenerator.GenerateClassFile(parsedData, saveLocation.Path);
    });

    public ICommand Rescan => new RelayCommand(() =>
    {
        // TODO: initiate rescan
        LastScanDateTime = DateTime.Now;
    });
}
