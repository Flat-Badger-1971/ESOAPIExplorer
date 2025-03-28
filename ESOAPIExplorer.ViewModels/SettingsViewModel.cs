using ESOAPIExplorer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESOAPIExplorer.Models;
namespace ESOAPIExplorer.ViewModels;

#pragma warning disable CA1416, CsWinRT1028
public class SettingsViewModel(ILuaParserService luaParserService, IThemeService themeService) : ViewModelBase
{
    private readonly Window _MainWindow = (Window)Application.Current.GetType().GetProperty("MainWindow").GetValue(Application.Current);
    //private readonly ApplicationDataContainer _Settings = ApplicationData.Current.LocalSettings;
    //private FileOpenPicker _FileOpenPicker;
    //private FileSavePicker _FileSavePicker;
    private ComboBoxItem _SelectedThemeName;
    private int _SelectedAlgorithmIndex;
    private DateTime _LastScanDateTime;
    private ObservableCollection<string> _Themes;

    public int SelectedAlgorithmIndex
    {
        get => _SelectedAlgorithmIndex;
        set
        {
            if (value > -1)
            {
                // _Settings.Values["SearchAlgorithm"] = SearchAlgorithmItemSource[value];
                SetProperty(ref _SelectedAlgorithmIndex, value);
            }
        }
    }

    public string SelectedThemeName
    {
        get => _SelectedThemeName?.Content.ToString();
        set
        {
            if (value != null)
            {
                // _Settings.Values["ThemeName"] = value;
                SetProperty(ref _SelectedThemeName, new ComboBoxItem { Content = value });

                // change theme
                if (Enum.TryParse(value, out ElementTheme theme))
                {
                    themeService.SetTheme(theme);
                }
            }
        }
    }

    public DateTime LastScanDateTime
    {
        get => _LastScanDateTime;
        set
        {
            // _Settings.Values["LastScanDateTime"] = value;
            SetProperty(ref _LastScanDateTime, value);
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
        get => _Themes;
        set {
            SetProperty(ref _Themes, value);
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

        //await base.InitializeAsync(data);

        //if (_Settings.Values["SearchAlgorithm"] == null)
        //{
        //    _Settings.Values["SearchAlgorithm"] = "Fast Fuzzy";
        //}

        //List<Type> searchAlgorithms = Utility.ListSearchAlgorithms();

        //SearchAlgorithmItemSource = new ObservableCollection<string>(
        //    searchAlgorithms
        //        .Select(a => a.GetPropertyValue("Name"))
        //        .OrderBy(a => a)
        //    );

        // SelectedAlgorithmIndex = SearchAlgorithmItemSource.IndexOf(_Settings.Values["SearchAlgorithm"].ToString());

        //Themes =
        //[
        //    "Light",
        //    "Dark",
        //    "SystemDefault"
        //];

        // string defaultTheme = _Settings.Values["ThemeName"] as string ?? "Dark";

        //SelectedThemeName = Themes.FirstOrDefault(t => t == defaultTheme) ?? Themes.First();

        //if (Enum.TryParse(defaultTheme, out ElementTheme theme))
        //{
        //    themeService.SetTheme(theme);
        //}
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
