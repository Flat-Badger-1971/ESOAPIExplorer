using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ESOAPIExplorer.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private ObservableCollection<string> _SearchAlgorithmItemSource;
        public ObservableCollection<string> SearchAlgorithmItemSource
        {
            get => _SearchAlgorithmItemSource;
            set
            {
                SetProperty(ref _SearchAlgorithmItemSource, value);
            }
        }

        public override async Task InitializeAsync(object data)
        {
            await base.InitializeAsync(data);

            List<Type> searchAlgorithms = Utility.ListSearchAlgorithms();

            SearchAlgorithmItemSource = new ObservableCollection<string>(
                searchAlgorithms
                    .Select(a => a.Name)
                    .OrderBy(a => a)
                );
        }
    }
}
