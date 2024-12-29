using ESOAPIExplorer.DisplayModels;
using ESOAPIExplorer.Models;
using ESOAPIExplorer.Models.Search;
using ESOAPIExplorer.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;

namespace ESOAPIExplorer.ViewModels;

public partial class HomeViewModel(IDialogService dialogService, IESODocumentationService esoDocumentationService) : ViewModelBase
{
    #region Properties
    private APIElement _SelectedElement;
    public APIElement SelectedElement
    {
        get => _SelectedElement;
        set
        {
            SetProperty(ref _SelectedElement, value);
            UpdateSelectedElementDetails();

            if (value != null)
            {
                AddToHistory(_SelectedElement.Name);
            }
        }
    }

    CancellationTokenSource _SelectedElementTokenSource;

    private StatusInformation _Status;
    public StatusInformation Status
    {
        get => _Status;
        set
        {
            SetProperty(ref _Status, value);
        }
    }

    private bool _CanGoBack;
    public bool CanGoBack
    {
        get => _CanGoBack;
        set => SetProperty(ref _CanGoBack, value);
    }

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
            SetProperty(ref _SelectedConstantValue, null, nameof(SelectedConstantValue));
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
            SetProperty(ref _SelectedConstantValue, null, nameof(SelectedConstantValue));
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
            SetProperty(ref _SelectedConstantValue, null, nameof(SelectedConstantValue));
        }
    }

    private int _SelectedEnum;
    public int SelectedEnum
    {
        get => _SelectedEnum;
        set
        {
            SetProperty(ref _SelectedEnum, value);

            Task.Run(async () =>
            {
                await Task.Delay(10);
                dialogService.RunOnMainThread(() => SelectedEnum = -1);
            });
        }
    }

    public EsoUIEnum SelectedGlobalEnum
    {
        get => _SelectedGlobalEnum;
        set => SetProperty(ref _SelectedGlobalEnum, value);
    }

    private int _SelectedFilterIndex;
    public int SelectedFilterIndex
    {
        get => _SelectedFilterIndex;
        set => SetProperty(ref _SelectedFilterIndex, value);
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
                SelectElement(value);
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
            SetProperty(ref _SelectedGlobalEnum, null, nameof(SelectedGlobalEnum));
            SetProperty(ref _SelectedConstantValue, null, nameof(SelectedConstantValue));
        }
    }

    private EsoUIGlobalConstantValue _SelectedConstantValue;
    public EsoUIGlobalConstantValue SelectedConstantValue
    {
        get => _SelectedConstantValue;
        set
        {
            SetProperty(ref _SelectedEventDetails, null, nameof(SelectedEventDetails));
            SetProperty(ref _SelectedFunctionDetails, null, nameof(SelectedFunctionDetails));
            SetProperty(ref _SelectedGlobalDetails, null, nameof(SelectedGlobalDetails));
            SetProperty(ref _SelectedEnumName, null, nameof(SelectedEnumName));
            SetProperty(ref _SelectedGlobalEnum, null, nameof(SelectedGlobalEnum));
            SetProperty(ref _SelectedConstantValue, value);
        }
    }

    private string _FilterText;
    public string FilterText
    {
        get => _FilterText;
        set
        {
            SetProperty(ref _FilterText, value);
            FilterItemsAsync().Wait();
        }
    }

    private ObservableCollection<DisplayModelBase<APIElement>> _AllItems;
    public ObservableCollection<DisplayModelBase<APIElement>> AllItems
    {
        get => _AllItems;
        set => SetProperty(ref _AllItems, value);
    }

    private ObservableCollection<APIElement> _FilteredItems;
    private EsoUIEnum _SelectedGlobalEnum;

    public ObservableCollection<APIElement> FilteredItems
    {
        get => _FilteredItems;
        set => SetProperty(ref _FilteredItems, value);
    }

    #endregion Properties

    private readonly ApplicationDataContainer _Settings = ApplicationData.Current.LocalSettings;
    private readonly Stack<string> _HistoryStack = new Stack<string>();
    private string _CurrentAlgorithmName;
    private IEnumerable<Type> _SearchAlgorithms;
    private ISearchAlgorithm _SearchAlgorithm;

    public override async Task InitializeAsync(object data)
    {
        await base.InitializeAsync(data);

        // intialise the constants dictionary
        ConstantValues.InitialiseConstants();

        if (_AllItems == null || _AllItems?.Count == 0)
        {
            _SearchAlgorithms = Utility.ListSearchAlgorithms();
            await esoDocumentationService.InitialiseAsync();

            ObservableCollection<APIElement> events = new ObservableCollection<APIElement>(esoDocumentationService.Documentation.Events
                .Select(item =>
                    new APIElement
                    {
                        Id = item.Key,
                        Name = item.Value.Name,
                        ElementType = APIElementType.EVENT,
                        Code = item.Value.Code
                    }));

            ObservableCollection<APIElement> functions = new ObservableCollection<APIElement>(esoDocumentationService.Documentation.Functions
                .Select(item =>
                    new APIElement
                    {
                        Id = item.Key,
                        Name = item.Value.Name,
                        ElementType = item.Value.Name.StartsWith("zo_", StringComparison.OrdinalIgnoreCase) ? APIElementType.FUNCTION : APIElementType.C_FUNCTION,
                        Code = item.Value.Code
                    }));

            ObservableCollection<APIElement> globals = new ObservableCollection<APIElement>(esoDocumentationService.Documentation.Globals
                .SelectMany(item =>
                    item.Value
                    .Select(detail =>
                        new APIElement
                        {
                            Id = detail.Name,
                            Name = detail.Name,
                            ElementType = APIElementType.ENUM_CONSTANT,
                            Parent = item.Key,
                            Code = [$"* {detail.Name}"]
                        }))
                .Concat(esoDocumentationService.Documentation.Globals
                .SelectMany(item =>
                    item.Value
                    .Select(detail =>
                        new APIElement
                        {
                            Id = item.Key,
                            Name = item.Key,
                            ElementType = APIElementType.ENUM_TYPE,
                            Code = [$"* {item.Key}"]
                        }))
                .GroupBy(e => e.Id)
                .Select(e => e.First())));

            ObservableCollection<APIElement> constants = new ObservableCollection<APIElement>(esoDocumentationService.Documentation.Constants
                .Select(item =>
                    new APIElement
                    {
                        Id = item.Key,
                        Name = item.Value.Name,
                        ElementType = APIElementType.CONSTANT,
                        Code = [$"{item.Value.Name} = {item.Value.Value}"]
                    }));

            AllItems = new ObservableCollection<DisplayModelBase<APIElement>>(
                events.Select(e => new DisplayModelBase<APIElement> { Value = e })
                .Concat(functions.Select(f => new DisplayModelBase<APIElement> { Value = f }))
                .Concat(globals.Select(c => new DisplayModelBase<APIElement> { Value = c }))
                .Concat(constants.Select(c => new DisplayModelBase<APIElement> { Value = c })));

            await FilterItemsAsync();
        }
    }

    private void UpdateObjects(List<EsoUIArgument> arguments)
    {
        foreach (EsoUIArgument e in arguments)
        {
            if (!string.IsNullOrWhiteSpace(e.Type.Type))
            {
                DisplayModelBase<APIElement> matchingElement = AllItems.FirstOrDefault(i => i.Value.Name == e.Type.Type);

                if (matchingElement?.Value != null)
                {
                    e.Type.IsObject = true;
                }
            }
        }
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
                    case APIElementType.EVENT:
                        dialogService.RunOnMainThread(() =>
                        {
                            SelectedEventDetails = esoDocumentationService.Data.Events[element.Id];
                            UpdateObjects(SelectedEventDetails.Args);
                        });
                        break;
                    case APIElementType.FUNCTION:
                    case APIElementType.C_FUNCTION:
                        dialogService.RunOnMainThread(() =>
                        {
                            SelectedFunctionDetails = esoDocumentationService.Data.Functions[element.Id];
                            UpdateObjects(SelectedFunctionDetails.Args);
                            UpdateObjects(SelectedFunctionDetails.Returns);
                        });
                        break;
                    case APIElementType.ENUM_CONSTANT:
                        dialogService.RunOnMainThread(() =>
                        {
                            SelectedGlobalDetails = new EsoUIGlobal
                            {
                                Name = element.Name,
                                ParentName = element.Parent
                            };
                        });

                        IEnumerable<string> usedBy = GetUsedByParallel(element.Parent);

                        dialogService.RunOnMainThread(() =>
                        {
                            SelectedGlobalEnum = new EsoUIEnum
                            {
                                Values = esoDocumentationService.Data.Globals[element.Parent],
                                Name = element.Name,
                                UsedBy = usedBy
                            };
                        });
                        break;
                    case APIElementType.ENUM_TYPE:
                        dialogService.RunOnMainThread(() =>
                        {
                            SelectedEnumName = new EsoUIGlobal
                            {
                                Name = element.Name
                            };
                        });

                        IEnumerable<string> eusedBy = GetUsedByParallel(element.Id);

                        dialogService.RunOnMainThread(() =>
                        {
                            SelectedGlobalEnum = new EsoUIEnum
                            {
                                Values = esoDocumentationService.Data.Globals[element.Id],
                                Name = element.Name,
                                UsedBy = eusedBy
                            };
                        });
                        break;
                    case APIElementType.CONSTANT:
                        dialogService.RunOnMainThread(() =>
                        {
                            SelectedConstantValue = esoDocumentationService.Data.Constants[element.Id];
                        });
                        break;
                }
            }
        }, _SelectedElementTokenSource.Token);
    }

    private void SetSearchAlgorithm()
    {
        string selectedAlgorithmName = _Settings.Values["SearchAlgorithm"].ToString();

        if (selectedAlgorithmName != _CurrentAlgorithmName)
        {
            Type algorithm = _SearchAlgorithms.FirstOrDefault(a => a.GetPropertyValue("Name") == selectedAlgorithmName);
            _SearchAlgorithm = Activator.CreateInstance(algorithm) as ISearchAlgorithm;
            _CurrentAlgorithmName = selectedAlgorithmName;
        }
    }

    private IOrderedEnumerable<APIElement> FilterKeywords(IEnumerable<APIElement> keywordList, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return keywordList.Order();
        }

        SetSearchAlgorithm();

        return _SearchAlgorithm.Search(filter, keywordList);
    }

    CancellationTokenSource token;

    private Task FilterItemsAsync()
    {
        if (token != null && !token.IsCancellationRequested)
        {
            token.Cancel();
        }

        token = new CancellationTokenSource();

        _ = Task.Run(() =>
        {
            string searchQuery = _FilterText;
            Thread.Sleep(300);

            if (!token.IsCancellationRequested)
            {
                if (searchQuery == _FilterText)
                {
                    IOrderedEnumerable<APIElement> filtered = FilterKeywords(AllItems.Select(i => i.Value), FilterText);
                    dialogService.RunOnMainThread(() =>
                    {
                        FilteredItems = new ObservableCollection<APIElement>(filtered);
                        Status = new StatusInformation
                        {
                            APIItems = _FilteredItems.Count,
                            APIVersion = esoDocumentationService.Documentation.ApiVersion,
                            CFunctionItems = _FilteredItems.Where(f => f.ElementType == APIElementType.C_FUNCTION).Count(),
                            ConstantItems = esoDocumentationService.Documentation.Constants.Count,
                            EnumConstants = _FilteredItems.Where(g => g.ElementType == APIElementType.ENUM_CONSTANT).Count(),
                            EnumTypes = _FilteredItems.Where(g => g.ElementType == APIElementType.ENUM_TYPE).Count(),
                            EventItems = _FilteredItems.Where(e => e.ElementType == APIElementType.EVENT).Count(),
                            FunctionItems = _FilteredItems.Where(f => f.ElementType == APIElementType.FUNCTION).Count(),
                            GlobalItems = 0
                        };
                    });
                }
            }
        }
        , token.Token);

        return Task.CompletedTask;
    }

    public ICommand SearchGithubCommand => new RelayCommand(() => _ = Windows.System.Launcher.LaunchUriAsync(new Uri($"https://github.com/esoui/esoui/search?q={_SelectedElement.Name}&type=code")));

    private IEnumerable<string> GetUsedByParallel(string enumName)
    {
        ConcurrentBag<string> usedBy = [];

        Parallel.ForEach(esoDocumentationService.Data.Events, item =>
        {
            Parallel.ForEach(item.Value.Args, arg =>
            {
                if (arg.Type.Name == enumName)
                {
                    usedBy.Add(item.Key);
                }
            });
        });

        Parallel.ForEach(esoDocumentationService.Data.Functions, item =>
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

    private void AddToHistory(string value)
    {
        _HistoryStack.Push(value);

        if (_HistoryStack.Count > 1 && !CanGoBack)
        {
            SetProperty(ref _CanGoBack, true, nameof(CanGoBack));
        }
    }

    private void SelectElement(string elementName, bool doNotAddToHistory = false)
    {
        if (!doNotAddToHistory)
        {
            AddToHistory(elementName);
        }

        SetProperty(ref _SelectedElement, AllItems.First(i => i.Value.Name == elementName).Value, nameof(SelectedElement));
        UpdateSelectedElementDetails();
    }

    public ICommand HandleSelectedItemElement => new RelayCommand<string>((elementName) =>
    {
        SelectElement(elementName);
    });

    public ICommand GoBack => new RelayCommand(() =>
    {
        string previous = _HistoryStack.Pop();

        if (previous == SelectedElement?.Name)
        {
            // pop again to remove the current selection from the stack
            previous = _HistoryStack.Pop();
        }

        if (_HistoryStack.Count == 0)
        {
            SetProperty(ref _CanGoBack, false, nameof(CanGoBack));
        }

        if (!string.IsNullOrEmpty(previous))
        {
            SelectElement(previous, true);
        }
    });
}
