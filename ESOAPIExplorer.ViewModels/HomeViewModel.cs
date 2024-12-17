using ESOAPIExplorer.DisplayModels;
using ESOAPIExplorer.Models;
using ESOAPIExplorer.Models.Search;
using ESOAPIExplorer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ESOAPIExplorer.ViewModels;

public partial class HomeViewModel(IDialogService dialogService, IESODocumentationService eSODocumentationService) : ViewModelBase
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
        set
        {
            SetProperty(ref _SelectedGlobalEnum, value);
        }
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
        }
    }

    private string _FilterText;
    public string FilterText
    {
        get => _FilterText;
        set
        {
            SetProperty(ref _FilterText, value);
            FilterItems().Wait();
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
        await eSODocumentationService.InitialiseAsync();

        ObservableCollection<APIElement> events = new ObservableCollection<APIElement>(eSODocumentationService.Documentation.Events
            .Select(item => new APIElement { Id = item.Key, Name = item.Value.Name, ElementType = APIElementType.Event }));

        ObservableCollection<APIElement> functions = new ObservableCollection<APIElement>(eSODocumentationService.Documentation.Functions
            .Select(item => new APIElement { Id = item.Key, Name = item.Value.Name, ElementType = APIElementType.Function })
            );

        ObservableCollection<APIElement> globals = new ObservableCollection<APIElement>(eSODocumentationService.Documentation.Globals
            .SelectMany(item => item.Value.Select(detail => new APIElement { Id = detail, Name = detail, ElementType = APIElementType.Global, Parent = item.Key }))
            .Concat(eSODocumentationService.Documentation.Globals
            .SelectMany(item => item.Value.Select(detail => new APIElement { Id = item.Key, Name = item.Key, ElementType = APIElementType.Enum }))
            .GroupBy(e => e.Id)
            .Select(e => e.First())));

        AllItems = new ObservableCollection<DisplayModelBase<APIElement>>(
            events.Select(e => new DisplayModelBase<APIElement> { Value = e })
            .Concat(functions.Select(f => new DisplayModelBase<APIElement> { Value = f }))
            .Concat(globals.Select(c => new DisplayModelBase<APIElement> { Value = c })));

        await FilterItems();
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
                    case APIElementType.Event:
                        dialogService.RunOnMainThread(() =>
                        {
                            SelectedEventDetails = eSODocumentationService.Data.Events[element.Id];
                            UpdateObjects(SelectedEventDetails.Args);
                        });
                        break;
                    case APIElementType.Function:
                        dialogService.RunOnMainThread(() =>
                        {
                            SelectedFunctionDetails = eSODocumentationService.Data.Functions[element.Id];
                            UpdateObjects(SelectedFunctionDetails.Args);
                            UpdateObjects(SelectedFunctionDetails.Returns);
                        });
                        break;
                    case APIElementType.Global:
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
                                ValueNames = eSODocumentationService.Data.Globals[element.Parent],
                                Name = element.Name,
                                UsedBy = usedBy
                            };
                        });
                        break;
                    case APIElementType.Enum:
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
                                ValueNames = eSODocumentationService.Data.Globals[element.Id],
                                Name = element.Name,
                                UsedBy = eusedBy
                            };
                        });
                        break;
                }
            }
        }, _SelectedElementTokenSource.Token);
    }

    private static IEnumerable<APIElement> FilterKeywords(IEnumerable<APIElement> keywordList, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return keywordList;
        }

        return FastFuzzy.Search(filter, keywordList);
    }

    CancellationTokenSource token;

    private async Task FilterItems(Action callback = null)
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
                IEnumerable<APIElement> filtered = FilterKeywords(AllItems.Select(i => i.Value), FilterText);

                dialogService.RunOnMainThread(() =>
                {
                    FilteredItems = new ObservableCollection<APIElement>(filtered.Order());
                    callback?.Invoke();
                });
            }
        }, token.Token);
    }


    public ICommand ClearCommand => new RelayCommand<string>((selectItem) => 
    {
        //if (parameter is IList<object> selected)
        //{
        //    Thread.Sleep(300);
        //    dialogService.RunOnMainThread(() =>
        //    {
        //        if (selected.Count > 0)
        //        {
        //            selected.Clear();
        //        }
        //    });
        //}
    });

    public ICommand SearchGithubCommand => new RelayCommand(() =>
    {
        _ = Windows.System.Launcher.LaunchUriAsync(new Uri($"https://github.com/esoui/esoui/search?q={_SelectedElement.Name}&type=code"));
    });

    private IEnumerable<string> GetUsedByParallel(string enumName)
    {
        List<string> usedBy = [];

        Parallel.ForEach(eSODocumentationService.Data.Events, item =>
        {
            Parallel.ForEach(item.Value.Args, arg =>
            {
                if (arg.Type.Name == enumName)
                {
                    usedBy.Add(item.Key);
                }
            });
        });

        Parallel.ForEach(eSODocumentationService.Data.Functions, item =>
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

    private void SelectElement(string elementName)
    {
        SetProperty(ref _SelectedElement, FilteredItems.First(i => i.Name == elementName), nameof(SelectedElement));
        UpdateSelectedElementDetails();
    }

    public ICommand HandleSelectedItemElement => new RelayCommand<string>((elementName) =>
    {
        SelectElement(elementName);
    });
}
