using ESOAPIExplorer.DisplayModels;
using ESOAPIExplorer.Models;
using ESOAPIExplorer.Models.Search;
using ESOAPIExplorer.Services;
using Microsoft.UI.Xaml.Controls;
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

    #region Properties
    private APIElement _SelectedElement;
    public APIElement SelectedElement
    {
        get => _SelectedElement;
        set
        {
            SetProperty(ref _SelectedElement, value);
            UpdateSelectedElementDetails();
        }
    }

    CancellationTokenSource _SelectedElementTokenSource;

    private EsoUIEvent _SelectedEventDetails;
    public EsoUIEvent SelectedEventDetails
    {
        get => _SelectedEventDetails;
        set
        {
            SetProperty(ref _SelectedEventDetails, value);
            SetProperty(ref _SelectedFunctionDetails, null, nameof(SelectedFunctionDetails));
            SetProperty(ref _SelectedGlobalDetails, null, nameof(SelectedGlobalDetails));
            SetProperty(ref _SelectedGlobalEnum, null, nameof(SelectedGlobalEnum));
            SetProperty(ref _SelectedEnumName, null, nameof(SelectedEnumName));
        }
    }

    private EsoUIFunction _SelectedFunctionDetails;
    public EsoUIFunction SelectedFunctionDetails
    {
        get => _SelectedFunctionDetails;
        set
        {
            SetProperty(ref _SelectedFunctionDetails, value);
            SetProperty(ref _SelectedEventDetails, null, nameof(SelectedEventDetails));
            SetProperty(ref _SelectedGlobalDetails, null, nameof(SelectedGlobalDetails));
            SetProperty(ref _SelectedGlobalEnum, null, nameof(SelectedGlobalEnum));
            SetProperty(ref _SelectedEnumName, null, nameof(SelectedEnumName));
        }
    }

    private EsoUIGlobal _SelectedEnumName;
    public EsoUIGlobal SelectedEnumName
    {
        get => _SelectedEnumName;
        set
        {
            SetProperty(ref _SelectedFunctionDetails, null, nameof(SelectedFunctionDetails));
            SetProperty(ref _SelectedEventDetails, null, nameof(SelectedEventDetails));
            SetProperty(ref _SelectedGlobalDetails, null, nameof(SelectedGlobalDetails));
            SetProperty(ref _SelectedGlobalEnum, null, nameof(SelectedGlobalEnum));
            SetProperty(ref _SelectedEnumName, value);
        }
    }

    public EsoUIEnum SelectedGlobalEnum
    {
        get => _SelectedGlobalEnum;
        set
        {
            SetProperty(ref _SelectedGlobalEnum, value);
        }
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
                SetProperty(ref _ShowEvents, true, nameof(ShowEvents));
                SetProperty(ref _ShowFunctions, true, nameof(ShowFunctions));
                SetProperty(ref _ShowGlobals, true, nameof(ShowGlobals));

                FilterItems(() =>
                {
                    _DialogService.RunOnMainThread(() =>
                    {
                        SelectedElement = _AllItems.FirstOrDefault(i => i.Value.Name == value)?.Value;
                    });
                });
            }
        }
    }

    private EsoUIGlobal _SelectedGlobalDetails;
    public EsoUIGlobal SelectedGlobalDetails
    {
        get => _SelectedGlobalDetails;
        set
        {
            SetProperty(ref _SelectedEventDetails, null, nameof(SelectedEventDetails));
            SetProperty(ref _SelectedFunctionDetails, null, nameof(SelectedFunctionDetails));
            SetProperty(ref _SelectedGlobalDetails, value);
            SetProperty(ref _SelectedEnumName, null, nameof(SelectedEnumName));
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
    public ObservableCollection<APIElement> Globals
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
    private EsoUIEnum _SelectedGlobalEnum;

    public ObservableCollection<APIElement> FilteredItems
    {
        get => _FilteredItems;
        set
        {
            SetProperty(ref _FilteredItems, value);
        }
    }

    #endregion Properties
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

        Globals = new ObservableCollection<APIElement>(_ESODocumentationService.Documentation.Globals
            .SelectMany(item => item.Value.Select(detail => new APIElement { Id = detail, Name = detail, ElementType = APIElementType.Global, Parent = item.Key }))
            .Concat(_ESODocumentationService.Documentation.Globals
            .SelectMany(item => item.Value.Select(detail => new APIElement { Id = item.Key, Name = item.Key, ElementType = APIElementType.Enum }))
            .GroupBy(e => e.Id)
            .Select(e => e.First()))
            .OrderBy(c => c.Name));

        AllItems = new ObservableCollection<DisplayModelBase<APIElement>>(
            Events.Select(e => new DisplayModelBase<APIElement> { Value = e })
            .Concat(Functions.Select(f => new DisplayModelBase<APIElement> { Value = f }))
            .Concat(Globals.Select(c => new DisplayModelBase<APIElement> { Value = c }))
            .OrderBy(item => item.Value.Name));

        FilterItems();
        //_ = _DialogService.ShowAsync("I fixed the dialog service, press 'Ok' if you acknowledge that I'm the best plzkthx.", "Yey");
    }

    private void UpdateSelectedElementDetails()
    {
        if (_SelectedElementTokenSource != null && !_SelectedElementTokenSource.IsCancellationRequested)
        {
            _SelectedElementTokenSource.Cancel();
        }

        _SelectedElementTokenSource = new CancellationTokenSource();

        Task.Run(() =>
        {
            APIElement element = _SelectedElement;

            if (element != null)
            {
                switch (element.ElementType)
                {
                    case APIElementType.Event:
                        _DialogService.RunOnMainThread(() =>
                        {
                            SelectedEventDetails = _ESODocumentationService.Data.Events[element.Id];
                        });
                        break;
                    case APIElementType.Function:
                        _DialogService.RunOnMainThread(() =>
                        {
                            SelectedFunctionDetails = _ESODocumentationService.Data.Functions[element.Id];
                        });
                        break;
                    case APIElementType.Global:
                        _DialogService.RunOnMainThread(() =>
                        {
                            SelectedGlobalDetails = new EsoUIGlobal
                            {
                                Name = element.Name,
                                ParentName = element.Parent
                            };
                        });

                        IEnumerable<string> usedBy = GetUsedByParallel(element.Parent);

                        _DialogService.RunOnMainThread(() =>
                        {
                            SelectedGlobalEnum = new EsoUIEnum
                            {
                                ValueNames = _ESODocumentationService.Data.Globals[element.Parent],
                                Name = element.Name,
                                UsedBy = usedBy
                            };
                        });
                        break;
                    case APIElementType.Enum:
                        _DialogService.RunOnMainThread(() =>
                        {
                            SelectedEnumName = new EsoUIGlobal
                            {
                                Name = element.Name                            
                            };
                        });

                        IEnumerable<string> eusedBy = GetUsedByParallel(element.Id);

                        _DialogService.RunOnMainThread(() =>
                        {
                            SelectedGlobalEnum = new EsoUIEnum
                            {
                                ValueNames = _ESODocumentationService.Data.Globals[element.Id],
                                Name = element.Name,
                                UsedBy = eusedBy
                            };
                        });
                        break;
                }
            }
        }, _SelectedElementTokenSource.Token);
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

    private async void FilterItems(Action callback = null)
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
                            || (ShowGlobals && i.Value.ElementType == APIElementType.Global)
                            || (ShowGlobals && i.Value.ElementType == APIElementType.Enum))
                        )
                        .Select(d => d.Value);

                IEnumerable<APIElement> filtered = FilterKeywords(items, FilterText);

                _DialogService.RunOnMainThread(() => { 
                    FilteredItems = new ObservableCollection<APIElement>(filtered);

                    callback?.Invoke();
                });
            }
        }, token.Token);
    }

    public ICommand SearchGithubCommand => new RelayCommand(() =>
    {
        _ = Windows.System.Launcher.LaunchUriAsync(new Uri($"https://github.com/esoui/esoui/search?q={_SelectedElement.Name}&type="));
    });

    private IEnumerable<string> GetUsedByParallel(string enumName)
    {
        List<string> usedBy = [];

        Parallel.ForEach(_ESODocumentationService.Data.Events, item =>
        {
            Parallel.ForEach(item.Value.Args, arg =>
            {
                if (arg.Type.Name == enumName)
                {
                    usedBy.Add(item.Key);
                }
            });
        });

        Parallel.ForEach(_ESODocumentationService.Data.Functions, item =>
        {
            Parallel.ForEach(item.Value.Args, arg =>
            {
                if (arg.Type.Name == enumName)
                {
                    usedBy.Add(item.Key);
                }
            });

            Parallel.ForEach(item.Value.Returns, retval =>
            {
                if (retval.Type.Name == enumName)
                {
                    usedBy.Add(item.Key);
                }
            });
        });

        return usedBy.Order();
    }
}
