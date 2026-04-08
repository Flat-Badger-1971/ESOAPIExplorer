using ESOAPIExplorer.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;

namespace ESOAPIExplorer.ViewModels;

#pragma warning disable CA1416, CsWinRT1028
public class SettingsViewModel(ILuaParserService luaParserService, IThemeService themeService, IFilePickerService filePickerService, IESODocumentationService esoDocumentationService, IEventService eventService) : ViewModelBase
{
    private const string SearchAlgorithmSettingKey = "SearchAlgorithm";
    private const string ThemeNameSettingKey = "ThemeName";
    private const string LastScanDateTimeSettingKey = "LastScanDateTime";

    private readonly ApplicationDataContainer _Settings = ApplicationData.Current.LocalSettings;
    private string _SelectedThemeName;
    private int _SelectedAlgorithmIndex;
    private DateTime _LastScanDateTime;
    private string _StatusMessage;
    private ObservableCollection<string> _Themes;

    public int SelectedAlgorithmIndex
    {
        get => _SelectedAlgorithmIndex;
        set
        {
            if (value > -1)
            {
                _Settings.Values[SearchAlgorithmSettingKey] = SearchAlgorithmItemSource[value];
                SetProperty(ref _SelectedAlgorithmIndex, value);
            }
        }
    }

    public string SelectedThemeName
    {
        get => _SelectedThemeName;
        set
        {
            if (value != null)
            {
                _Settings.Values[ThemeNameSettingKey] = value;
                SetProperty(ref _SelectedThemeName, value);

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
            _Settings.Values[LastScanDateTimeSettingKey] = value.ToString("O");
            SetProperty(ref _LastScanDateTime, value);
        }
    }

    public string StatusMessage
    {
        get => _StatusMessage;
        set => SetProperty(ref _StatusMessage, value);
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
        await base.InitializeAsync(data);

        if (_Settings.Values[SearchAlgorithmSettingKey] == null)
        {
            _Settings.Values[SearchAlgorithmSettingKey] = "Fast Fuzzy";
        }

        List<Type> searchAlgorithms = SearchAlgorithmDiscovery.ListSearchAlgorithms();

        SearchAlgorithmItemSource = new ObservableCollection<string>(
            searchAlgorithms
                .Select(a => a.GetStaticPropertyValue("Name"))
                .OrderBy(a => a)
            );

        SelectedAlgorithmIndex = SearchAlgorithmItemSource.IndexOf(_Settings.Values[SearchAlgorithmSettingKey].ToString());

        Themes =
        [
            "Light",
            "Dark",
            "SystemDefault"
        ];

        string defaultTheme = _Settings.Values[ThemeNameSettingKey] as string ?? "Dark";

        SelectedThemeName = Themes.FirstOrDefault(t => t == defaultTheme) ?? Themes.First();

        if (Enum.TryParse(defaultTheme, out ElementTheme theme))
        {
            themeService.SetTheme(theme);
        }

        if (_Settings.Values[LastScanDateTimeSettingKey] is string lastScanValue
            && DateTime.TryParse(lastScanValue, out DateTime lastScanDateTime))
        {
            SetProperty(ref _LastScanDateTime, lastScanDateTime, nameof(LastScanDateTime));
        }

        StatusMessage = LastScanDateTime == default ? "No API rescan has been run yet." : $"Last API rescan completed on {LastScanDateTime:G}.";
    }

    public ICommand Generate => new RelayCommand(() => _ = GenerateAsync());

    private async Task GenerateAsync()
    {
        SetBusyState(true, "Generating ConstantValues class...");

        try
        {
            StorageFile merFilePath = await filePickerService.PickSingleFileAsync(new FileOpenPickerOptions
            {
                SuggestedStartLocation = PickerStartLocation.DocumentsLibrary,
                CommitButtonText = "Open merTorchbug.lua",
                FileTypeFilter = [".lua"]
            });

            if (merFilePath == null)
            {
                StatusMessage = "Constant generation cancelled.";
                return;
            }

            string content = File.ReadAllText(merFilePath.Path).Replace("\r", "");
            Dictionary<string, Models.EsoUIGlobalValue> parsedData = luaParserService.ParseLuaContent(content);

            StorageFile saveLocation = await filePickerService.PickSaveFileAsync(new FileSavePickerOptions
            {
                SuggestedStartLocation = PickerStartLocation.DocumentsLibrary,
                SuggestedFileName = "ConstantValues",
                CommitButtonText = "Save ConstantValues.cs",
                FileTypeChoices = new Dictionary<string, IReadOnlyList<string>>
                {
                    ["C Sharp Class"] = [".cs"]
                }
            });

            if (saveLocation == null)
            {
                StatusMessage = "Constant generation cancelled.";
                return;
            }

            CodeGenerator.GenerateClassFile(parsedData, saveLocation.Path);
            StatusMessage = $"Generated ConstantValues class at {saveLocation.Path}.";
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

    public ICommand Rescan => new RelayCommand(() => _ = RescanAsync());

    private async Task RescanAsync()
    {
        SetBusyState(true, "Rescanning ESO API documentation...");

        try
        {
            await esoDocumentationService.ReloadAsync(clearCache: true);
            LastScanDateTime = DateTime.Now;
            StatusMessage = $"API rescan completed on {LastScanDateTime:G}.";
            await eventService.RaiseDocumentationChanged();
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
}
