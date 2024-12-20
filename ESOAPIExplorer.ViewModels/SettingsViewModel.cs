using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace ESOAPIExplorer.ViewModels;

#pragma warning disable CA1416, CsWinRT1028
public class SettingsViewModel : ViewModelBase
{
    private readonly ApplicationDataContainer _Settings = ApplicationData.Current.LocalSettings;

    private int _SelectedAlgorithmIndex;
    public int SelectedAlgorithmIndex
    {
        get => _SelectedAlgorithmIndex;
        set
        {
            if (value > -1)
            {
                _Settings.Values["SearchAlgorithm"] = SearchAlgorithmItemSource[value];
                SetProperty(ref _SelectedAlgorithmIndex, value);
            }
        }
    }

    private ObservableCollection<string> _SearchAlgorithmItemSource;
    public ObservableCollection<string> SearchAlgorithmItemSource
    {
        get => _SearchAlgorithmItemSource;
        set => SetProperty(ref _SearchAlgorithmItemSource, value);
    }

    public override async Task InitializeAsync(object data)
    {
        await base.InitializeAsync(data);

        if (_Settings.Values["SearchAlgorithm"] == null)
        {
            _Settings.Values["SearchAlgorithm"] = "Fast Fuzzy";
        }

        List<Type> searchAlgorithms = Utility.ListSearchAlgorithms();

        SearchAlgorithmItemSource = new ObservableCollection<string>(
            searchAlgorithms
                .Select(a => a.GetPropertyValue("Name"))
                .OrderBy(a => a)
            );

        SelectedAlgorithmIndex = SearchAlgorithmItemSource.IndexOf(_Settings.Values["SearchAlgorithm"].ToString());
    }
}
