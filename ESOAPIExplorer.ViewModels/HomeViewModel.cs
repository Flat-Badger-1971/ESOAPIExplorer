using ESOAPIExplorer.DisplayModels;
using ESOAPIExplorer.Models;
using ESOAPIExplorer.Models.Search;
using ESOAPIExplorer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ESOAPIExplorer.ViewModels;

public partial class HomeViewModel(IDialogService dialogService, IESODocumentationService eSODocumentationService) : ViewModelBase
{
    private readonly IDialogService _DialogService = dialogService;
    private readonly IESODocumentationService _ESODocumentationService = eSODocumentationService;

    private APIElement _SelectedElement;
    public APIElement SelectedElement
    {
        get => _SelectedElement;
        set
        {
            SetProperty(ref _SelectedElement, value);

            SelectedFunctionDetails = null;
            SelectedEventDetails = null;
            SelectedGlobalDetails = null;

            if (value != null)
            {
                SelectedUsedByItem = null;
                // try
                // {
                switch (value.ElementType)
                {
                    case APIElementType.Event:
                        SelectedEventDetails = _ESODocumentationService.Data.Events[value.Id];
                        break;
                    case APIElementType.Function:
                        SelectedFunctionDetails = _ESODocumentationService.Data.Functions[value.Id];
                        break;
                    case APIElementType.Global:
                        SelectedGlobalDetails = new EsoUIGlobal
                        {
                            Name = value.Name,
                            ParentName = value.Parent
                        };

                        SelectedGlobalEnum = new EsoUIEnum
                        {
                            ValueNames = _ESODocumentationService.Data.Globals[value.Parent],
                            Name = value.Name,
                            UsedBy = GetUsedBy(value.Parent)
                        };

                        break;
                }
                // }
                // catch
                // {
                // _DialogService.ShowAsync("Error", $"Failed to load details for {value.Name}");
                // }
            }
        }
    }

    private EsoUIEvent _SelectedEventDetails;
    public EsoUIEvent SelectedEventDetails
    {
        get => _SelectedEventDetails;
        set
        {
            SetProperty(ref _SelectedEventDetails, value);
            SetProperty(ref _SelectedFunctionDetails, null);
            SetProperty(ref _SelectedGlobalDetails, null);
        }
    }

    private EsoUIFunction _SelectedFunctionDetails;
    public EsoUIFunction SelectedFunctionDetails
    {
        get => _SelectedFunctionDetails;
        set
        {
            SetProperty(ref _SelectedEventDetails, null);
            SetProperty(ref _SelectedFunctionDetails, value);
            SetProperty(ref _SelectedGlobalDetails, null);
        }
    }

    public EsoUIEnum SelectedGlobalEnum
    {
        get => selectedGlobalEnum;
        set => SetProperty(ref selectedGlobalEnum, value);
    }

    private string _SelectedUsedByItem;
    public string SelectedUsedByItem
    {
        get => _SelectedUsedByItem;
        set
        {
            SetProperty(ref _SelectedUsedByItem, value);
            if (value != null)
            {
                SelectedElement = _AllItems.FirstOrDefault(i => i.Value.Name == value)?.Value;
            }
        }
    }

    private EsoUIGlobal _SelectedGlobalDetails;
    public EsoUIGlobal SelectedGlobalDetails
    {
        get => _SelectedGlobalDetails;
        set
        {
            SetProperty(ref _SelectedEventDetails, null);
            SetProperty(ref _SelectedFunctionDetails, null);
            SetProperty(ref _SelectedGlobalDetails, value);
        }
    }

    private string _FilterText;
    public string FilterText
    {
        get => _FilterText;
        set
        {
            SetProperty(ref _FilterText, value);
            FilterItems();
        }
    }

    private bool _ShowEvents = true;
    public bool ShowEvents
    {
        get => _ShowEvents;
        set
        {
            SetProperty(ref _ShowEvents, value);
            FilterItems();
        }
    }
    private bool _ShowFunctions = true;
    public bool ShowFunctions
    {
        get => _ShowFunctions;
        set
        {
            SetProperty(ref _ShowFunctions, value);
            FilterItems();
        }
    }
    private bool _ShowGlobals = true;
    public bool ShowGlobals
    {
        get => _ShowGlobals;
        set
        {
            SetProperty(ref _ShowGlobals, value);
            FilterItems();
        }
    }

    private ObservableCollection<APIElement> _events;
    public ObservableCollection<APIElement> Events
    {
        get => _events;
        set
        {
            SetProperty(ref _events, value);
        }
    }

    private ObservableCollection<APIElement> _functions;
    public ObservableCollection<APIElement> Functions
    {
        get => _functions;
        set
        {
            SetProperty(ref _functions, value);
        }
    }

    private ObservableCollection<APIElement> _Constants;
    public ObservableCollection<APIElement> Constants
    {
        get => _Constants;
        set
        {
            SetProperty(ref _Constants, value);
        }
    }

    private ObservableCollection<DisplayModelBase<APIElement>> _AllItems;
    public ObservableCollection<DisplayModelBase<APIElement>> AllItems
    {
        get => _AllItems;
        set
        {
            SetProperty(ref _AllItems, value);
        }
    }

    private ObservableCollection<APIElement> _FilteredItems;
    private EsoUIEnum selectedGlobalEnum;

    public ObservableCollection<APIElement> FilteredItems
    {
        get => _FilteredItems;
        set
        {
            SetProperty(ref _FilteredItems, value);
        }
    }

    public override async Task InitializeAsync(object data)
    {
        await base.InitializeAsync(data);

        await _ESODocumentationService.InitialiseAsync();

        Events = new ObservableCollection<APIElement>(_ESODocumentationService.Documentation.Events
            .Select(item => new APIElement { Id = item.Key, Name = item.Value.Name, ElementType = APIElementType.Event })
            .OrderBy(e => e.Name));


        Functions = new ObservableCollection<APIElement>(_ESODocumentationService.Documentation.Functions
            .Select(item => new APIElement { Id = item.Key, Name = item.Value.Name, ElementType = APIElementType.Function })
            .OrderBy(f => f.Name)
            );

        Constants = new ObservableCollection<APIElement>(_ESODocumentationService.Documentation.Globals
            .SelectMany(item => item.Value.Select(detail => new APIElement { Id = detail, Name = detail, ElementType = APIElementType.Global, Parent = item.Key }))
            .OrderBy(c => c.Name)
            );

        AllItems = new ObservableCollection<DisplayModelBase<APIElement>>(
            Events.Select(e =>
            {
                return new DisplayModelBase<APIElement> { Value = e };
            })
            .Concat(Functions.Select(f =>
            {
                return new DisplayModelBase<APIElement> { Value = f };
            })
            .Concat(Constants.Select(c =>
            {
                return new DisplayModelBase<APIElement> { Value = c };
            })
                ))
            .OrderBy(item => item.Value.Name));

        FilterItems();
    }

    private static bool ContainsInOrder(string source, string filter)
    {
        if (string.IsNullOrEmpty(filter)) return true;
        int index = 0;
        foreach (char c in source)
        {
            if (char.ToLowerInvariant(c) == char.ToLowerInvariant(filter[index]))
            {
                index++;
                if (index == filter.Length) return true;
            }
        }
        return false;
    }

    private IEnumerable<APIElement> FilterKeywords(IEnumerable<APIElement> keywordList, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return keywordList;
        }

        return FastFuzzy.Search(filter, keywordList);
    }

    CancellationTokenSource token;
    private async void FilterItems()
    {
        if (token != null && !token.IsCancellationRequested)
        {
            token.Cancel();
        }
        token = new CancellationTokenSource();
        await Task.Run(async () =>
        {
            string searchQuery = _FilterText;
            await Task.Delay(300);
            if (searchQuery == _FilterText)
            {
                IEnumerable<APIElement> items = AllItems
                        .Where(i =>
                            ((ShowEvents && i.Value.ElementType == APIElementType.Event)
                            || (ShowFunctions && i.Value.ElementType == APIElementType.Function)
                            || (ShowGlobals && i.Value.ElementType == APIElementType.Global))
                        )
                        .Select(d => d.Value);

                IEnumerable<APIElement> filtered = FilterKeywords(items, FilterText);

                _DialogService.RunOnMainThread(() => { FilteredItems = new ObservableCollection<APIElement>(filtered); });
            }
        }, token.Token);
    }

    public ICommand SearchGithubCommand => new RelayCommand(() =>
    {
        _ = Windows.System.Launcher.LaunchUriAsync(new Uri($"https://github.com/esoui/esoui/search?q={_SelectedElement.Name}&type="));
    });

    private IEnumerable<string> GetUsedBy(string enumName)
    {
        List<string> usedBy = [];

        foreach (KeyValuePair<string, EsoUIEvent> item in _ESODocumentationService.Data.Events)
        {
            foreach (EsoUIArgument arg in item.Value.Args)
            {
                if (arg.Type.Name == enumName)
                {
                    usedBy.Add(item.Key);
                }
            }
        }

        foreach (KeyValuePair<string, EsoUIFunction> item in _ESODocumentationService.Data.Functions)
        {
            foreach (EsoUIArgument arg in item.Value.Args)
            {
                if (arg.Type.Name == enumName)
                {
                    usedBy.Add(item.Key);
                }
            }

            foreach (EsoUIArgument retval in item.Value.Returns)
            {
                if (retval.Type.Name == enumName)
                {
                    usedBy.Add(item.Key);
                }
            }
        }

        return usedBy.Order();
    }
}
