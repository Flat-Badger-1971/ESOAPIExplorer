using ESOAPIExplorer.DisplayModels;
using ESOAPIExplorer.Models;
using ESOAPIExplorer.Models.Search;
using ESOAPIExplorer.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;

namespace ESOAPIExplorer.ViewModels;
public partial class HomeViewModel(IDialogService _dialogService, IESODocumentationService _esoDocumentationService, IRegexService _regexService, IEventService eventService) : ViewModelBase
{
    private static readonly TimeSpan FilterDebounceDelay = TimeSpan.FromMilliseconds(300);

    #region Properties
    private APIElement _selectedElement;
    public APIElement SelectedElement
    {
        get => _selectedElement;
        set
        {
            SetProperty(ref _selectedElement, value);
            UpdateSelectedElementDetails();

            if (value != null)
            {
                AddToHistory(_selectedElement.Name);
            }
        }
    }

    private string _loadingMessage;
    public string LoadingMessage
    {
        get => _loadingMessage;
        set => SetProperty(ref _loadingMessage, value);
    }

    private Visibility _loadingVisibility = Visibility.Collapsed;
    public Visibility LoadingVisibility
    {
        get => _loadingVisibility;
        set => SetProperty(ref _loadingVisibility, value);
    }

    private CancellationTokenSource _selectedElementTokenSource;

    private StatusInformation _status;
    public StatusInformation Status
    {
        get => _status;
        set
        {
            SetProperty(ref _status, value);
        }
    }

    private bool _canGoBack;
    public bool CanGoBack
    {
        get => _canGoBack;
        set => SetProperty(ref _canGoBack, value);
    }

    private EsoUIEvent _selectedEventDetails;
    public EsoUIEvent SelectedEventDetails
    {
        get => _selectedEventDetails;
        set
        {
            UpdateProperties(nameof(SelectedEventDetails), value);
        }
    }

    private EsoUIInstance _selectedInstanceDetails;
    public EsoUIInstance SelectedInstanceDetails
    {
        get => _selectedInstanceDetails;
        set
        {
            UpdateProperties(nameof(SelectedInstanceDetails), value);
        }
    }

    private EsoUIObject _selectedObjectDetails;
    public EsoUIObject SelectedObjectDetails
    {
        get => _selectedObjectDetails;
        set
        {
            UpdateProperties(nameof(SelectedObjectDetails), value);
        }
    }

    private EsoUIFunction _selectedMethodDetails;
    public EsoUIFunction SelectedMethodDetails
    {
        get => _selectedMethodDetails;
        set
        {
            UpdateProperties(nameof(SelectedMethodDetails), value);
        }
    }

    private EsoUIFunction _selectedFunctionDetails;
    public EsoUIFunction SelectedFunctionDetails
    {
        get => _selectedFunctionDetails;
        set
        {
            UpdateProperties(nameof(SelectedFunctionDetails), value);
        }
    }

    private EsoUIGlobal _selectedEnumName;
    public EsoUIGlobal SelectedEnumName
    {
        get => _selectedEnumName;
        set
        {
            UpdateProperties(nameof(SelectedEnumName), value);
        }
    }

    private EsoUIGlobal _selectedGlobalDetails;
    public EsoUIGlobal SelectedGlobalDetails
    {
        get => _selectedGlobalDetails;
        set
        {
            UpdateProperties(nameof(SelectedGlobalDetails), value);
        }
    }

    private EsoUIConstantValue _selectedConstantDetails;
    public EsoUIConstantValue SelectedConstantDetails
    {
        get => _selectedConstantDetails;
        set
        {
            UpdateProperties(nameof(SelectedConstantDetails), value);
        }
    }

    private EsoUIEnumValue _selectedEnumValue;
    public EsoUIEnumValue SelectedEnumValue
    {
        get => _selectedEnumValue;
        set
        {
            if (!SetProperty(ref _selectedEnumValue, value))
            {
                return;
            }

            if (value != null)
            {
                SelectElement(value.Name);
                SetProperty(ref _selectedEnumValue, null, nameof(SelectedEnumValue));
            }
        }
    }

    public EsoUIEnum SelectedGlobalEnum
    {
        get => _selectedGlobalEnum;
        set => SetProperty(ref _selectedGlobalEnum, value);
    }

    private int _selectedFilterIndex;
    public int SelectedFilterIndex
    {
        get => _selectedFilterIndex;
        set => SetProperty(ref _selectedFilterIndex, value);
    }

    private string _selectedUsedByItem;
    public string SelectedUsedByItem
    {
        get => _selectedUsedByItem;
        set
        {
            SetProperty(ref _selectedUsedByItem, value);

            if (value != null)
            {
                string selected = value;

                switch (_selectedElement.ElementType)
                {
                    case APIElementType.OBJECT_TYPE:
                    case APIElementType.C_OBJECT_TYPE:
                        if (!_regexService.UppercaseMatcher().IsMatch(value))
                        {
                            // object instance
                            selected = $"{SelectedObjectDetails.Name}:{value}";
                        }

                        break;
                    case APIElementType.INSTANCE_NAME:
                        selected = SelectedInstanceDetails.InstanceOf;
                        break;
                }

                SelectElement(selected);
            }
        }
    }

    private string _filterText;
    public string FilterText
    {
        get => _filterText;
        set
        {
            SetProperty(ref _filterText, value);
            _ = FilterItemsAsync();
        }
    }

    private ObservableCollection<DisplayModelBase<APIElement>> _allItems;
    public ObservableCollection<DisplayModelBase<APIElement>> AllItems
    {
        get => _allItems;
        set => SetProperty(ref _allItems, value);
    }

    private ObservableCollection<APIElement> _filteredItems;
    private EsoUIEnum _selectedGlobalEnum;

    public ObservableCollection<APIElement> FilteredItems
    {
        get => _filteredItems;
        set => SetProperty(ref _filteredItems, value);
    }

    #endregion Properties

    private readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;
    private readonly Stack<string> _historyStack = new Stack<string>();
    private readonly IEventService _eventService = eventService;
    private CancellationTokenSource _filterItemsTokenSource;
    private string _currentAlgorithmName;
    private bool _isSubscribedToDocumentationChanges;
    private IEnumerable<Type> _searchAlgorithms;
    private ISearchAlgorithm _searchAlgorithm;

    public override async Task InitializeAsync(object data)
    {
        await base.InitializeAsync(data);

        SetTaskbarColour();

        // intialise the constants dictionary
        ConstantValues.InitialiseConstants();

        if (!_isSubscribedToDocumentationChanges)
        {
            _eventService.DocumentationChanged += HandleDocumentationChanged;
            _isSubscribedToDocumentationChanges = true;
        }

        if (_allItems == null || _allItems?.Count == 0)
        {
            await LoadItemsAsync();
        }
    }

    private async Task LoadItemsAsync(string loadingMessage = "Loading ESO API documentation...")
    {
        SetLoadingState(true, loadingMessage);

        try
        {
            await _esoDocumentationService.InitialiseAsync();
            EsoUIDocumentation documentation = _esoDocumentationService.Documentation ?? throw new InvalidOperationException("ESO API documentation could not be loaded.");

            SetLoadingState(true, "Building API index...");
            BuildItems(documentation);
            await FilterItemsAsync();
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void BuildItems(EsoUIDocumentation documentation)
    {
        _searchAlgorithms = SearchAlgorithmDiscovery.ListSearchAlgorithms();

        ObservableCollection<APIElement> events = new ObservableCollection<APIElement>(documentation.Events
            .Select(item =>
                new APIElement
                {
                    Id = item.Key,
                    Name = item.Value.Name,
                    ElementType = APIElementType.EVENT,
                    Code = item.Value.Code
                }));

        ObservableCollection<APIElement> functions = new ObservableCollection<APIElement>(documentation.Functions
            .Select(item =>
                new APIElement
                {
                    Id = item.Key,
                    Name = item.Value.Name,
                    ElementType = item.Value.ElementType,
                    Code = item.Value.Code
                }));

        ObservableCollection<APIElement> enums = new ObservableCollection<APIElement>(documentation.Globals
            .Where(g => g.Key != "Globals")
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
            .Concat(documentation.Globals
            .Where(g => g.Key != "Global")
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

        ObservableCollection<APIElement> constants = new ObservableCollection<APIElement>(documentation.Constants
            .Select(item =>
                new APIElement
                {
                    Id = item.Key,
                    Name = item.Value.Name,
                    ElementType = item.Value.Name.StartsWith("SI_") ? APIElementType.SI_GLOBAL : APIElementType.CONSTANT,
                    Code = [$"{item.Value.Name} = {item.Value.Value}"]
                })
            .Concat(documentation.Globals
                .Where(g => g.Key == "Globals")
                .SelectMany(item =>
                    item.Value
                    .Select(detail =>
                        new APIElement
                        {
                            Id = detail.Name,
                            Name = detail.Name,
                            ElementType = APIElementType.CONSTANT,
                            Code = [$"* {detail.Name}"]
                        }))));

        ObservableCollection<APIElement> objects = new ObservableCollection<APIElement>(documentation.Objects
            .Select(item =>
                new APIElement
                {
                    Id = item.Key,
                    Name = item.Value.Name,
                    ElementType = item.Value.ElementType,
                    Code = item.Value.Code
                }));

        ObservableCollection<APIElement> methods = new ObservableCollection<APIElement>(documentation.Objects
            .SelectMany(item =>
                item.Value.Functions
                .Select(func =>
                    new APIElement
                    {
                        Id = func.Key,
                        Name = $"{item.Key}:{func.Value.Name}",
                        ElementType = func.Value.ElementType,
                        Code = func.Value.Code,
                        Parent = item.Value.Name
                    })));

        ObservableCollection<APIElement> instanceNames = new ObservableCollection<APIElement>(documentation.InstanceNames
            .Select(n => new APIElement
            {
                Id = n.Key,
                Name = n.Value.Name,
                ElementType = APIElementType.INSTANCE_NAME,
                Code = [n.Value.Code]
            }));

        AllItems = new ObservableCollection<DisplayModelBase<APIElement>>(
            events.Select(e => new DisplayModelBase<APIElement> { Value = e })
            .Concat(functions.Select(f => new DisplayModelBase<APIElement> { Value = f }))
            .Concat(enums.Select(c => new DisplayModelBase<APIElement> { Value = c }))
            .Concat(constants.Select(c => new DisplayModelBase<APIElement> { Value = c }))
            .Concat(objects.Select(o => new DisplayModelBase<APIElement> { Value = o }))
            .Concat(methods.Select(m => new DisplayModelBase<APIElement> { Value = m }))
            .Concat(instanceNames.Select(n => new DisplayModelBase<APIElement> { Value = n })));
    }

    private async void HandleDocumentationChanged(object sender, EventArgs e)
    {
        try
        {
            await RefreshItemsAsync();
        }
        catch (Exception ex)
        {
            SetLoadingState(false);
            await _dialogService.ShowAsync(ex.Message, "Rescan failed");
        }
    }

    private async Task RefreshItemsAsync()
    {
        EsoUIDocumentation documentation = _esoDocumentationService.Documentation ?? throw new InvalidOperationException("ESO API documentation could not be refreshed.");

        SetLoadingState(true, "Refreshing API index...");

        try
        {
            _historyStack.Clear();
            CanGoBack = false;
            SelectedFilterIndex = -1;
            SelectedUsedByItem = null;
            SetProperty(ref _selectedElement, null, nameof(SelectedElement));
            SetProperty(ref _selectedEnumValue, null, nameof(SelectedEnumValue));
            UpdateProperties(string.Empty, null);

            BuildItems(documentation);
            await FilterItemsAsync();
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void SetLoadingState(bool isLoading, string message = null)
    {
        SetBusyState(isLoading, message);
        LoadingVisibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        LoadingMessage = BusyMessage;
    }

    private void UpdateProperties(string propertyName, object value)
    {
        SetProperty(ref _selectedConstantDetails, propertyName == nameof(SelectedConstantDetails) ? value as EsoUIConstantValue : default, nameof(SelectedConstantDetails));
        SetProperty(ref _selectedEnumName, propertyName == nameof(SelectedEnumName) ? value as EsoUIGlobal : default, nameof(SelectedEnumName));
        SetProperty(ref _selectedEventDetails, propertyName == nameof(SelectedEventDetails) ? value as EsoUIEvent : default, nameof(SelectedEventDetails));
        SetProperty(ref _selectedFunctionDetails, propertyName == nameof(SelectedFunctionDetails) ? value as EsoUIFunction : default, nameof(SelectedFunctionDetails));
        SetProperty(ref _selectedGlobalDetails, propertyName == nameof(SelectedGlobalDetails) ? value as EsoUIGlobal : default, nameof(SelectedGlobalDetails));
        SetProperty(ref _selectedGlobalEnum, propertyName == nameof(SelectedGlobalEnum) ? value as EsoUIEnum : default, nameof(SelectedGlobalEnum));
        SetProperty(ref _selectedInstanceDetails, propertyName == nameof(SelectedInstanceDetails) ? value as EsoUIInstance : default, nameof(SelectedInstanceDetails));
        SetProperty(ref _selectedObjectDetails, propertyName == nameof(SelectedObjectDetails) ? value as EsoUIObject : default, nameof(SelectedObjectDetails));
        SetProperty(ref _selectedMethodDetails, propertyName == nameof(SelectedMethodDetails) ? value as EsoUIFunction : default, nameof(SelectedMethodDetails));
    }

    private static void SetTaskbarColour()
    {
        Window MainWindow = (Window)Application.Current.GetType().GetProperty("MainWindow").GetValue(Application.Current);
        MainWindow.AppWindow.TitleBar.BackgroundColor = Colors.Gray;
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

    private void UpdateObjects(List<EsoUIReturn> returns)
    {
        foreach (EsoUIReturn r in returns)
        {
            UpdateObjects(r.Values);
        }
    }

    private void UpdateSelectedElementDetails()
    {
        CancellationToken token = ResetCancellationTokenSource(ref _selectedElementTokenSource);
        APIElement element = _selectedElement;
        List<Action> actions = [];
        EsoUIDocumentation doc = _esoDocumentationService.Documentation;

        if (element == null || doc == null || token.IsCancellationRequested)
        {
            return;
        }

        switch (element.ElementType)
        {
            case APIElementType.EVENT:
                if (doc.Events.TryGetValue(element.Id, out EsoUIEvent eventInfo))
                {
                    actions.Add(() =>
                    {
                        SelectedEventDetails = eventInfo;
                        UpdateObjects(eventInfo.Args);
                    });
                }
                break;
            case APIElementType.FUNCTION:
            case APIElementType.C_FUNCTION:
                if (doc.Functions.TryGetValue(element.Id, out EsoUIFunction func))
                {
                    actions.Add(() =>
                    {
                        SelectedFunctionDetails = func;
                        UpdateObjects(func.Args);
                        UpdateObjects(func.Returns);
                    });
                }
                break;
            case APIElementType.ENUM_CONSTANT:
                actions.Add(() => SelectedGlobalDetails = new EsoUIGlobal { Name = element.Name, ParentName = element.Parent });

                IEnumerable<string> usedBy = GetUsedBy(element.Parent);

                if (doc.Globals.TryGetValue(element.Parent, out ICollection<EsoUIEnumValue> enumValues))
                {
                    actions.Add(() => SelectedGlobalEnum = new EsoUIEnum { Values = enumValues, Name = element.Name, UsedBy = usedBy });
                }

                break;
            case APIElementType.ENUM_TYPE:
                actions.Add(() => SelectedEnumName = new EsoUIGlobal { Name = element.Name });

                IEnumerable<string> enumTypeUsedBy = GetUsedBy(element.Id);

                if (doc.Globals.TryGetValue(element.Id, out ICollection<EsoUIEnumValue> enumTypes))
                {
                    actions.Add(() => SelectedGlobalEnum = new EsoUIEnum { Values = enumTypes, Name = element.Name, UsedBy = enumTypeUsedBy });
                }

                break;
            case APIElementType.CONSTANT:
                if (doc.Constants.TryGetValue(element.Id, out EsoUIConstantValue constantValue))
                {
                    actions.Add(() => SelectedConstantDetails = constantValue);
                }
                else if (doc.Globals.TryGetValue("Globals", out ICollection<EsoUIEnumValue> globalList))
                {
                    EsoUIEnumValue constant = globalList.SingleOrDefault(g => g.Name == element.Id);

                    if (constant != null)
                    {
                        EsoUIConstantValue cvalue = new EsoUIConstantValue(constant.Name, new EsoUIGlobalValue { DisplayValue = constant.Value, Name = constant.Name });
                        actions.Add(() => SelectedConstantDetails = cvalue);
                    }
                }
                break;
            case APIElementType.SI_GLOBAL:
            case APIElementType.GLOBAL:
                if (doc.Constants.TryGetValue(element.Id, out EsoUIConstantValue globalValue))
                {
                    if (element.ElementType == APIElementType.SI_GLOBAL && doc.SI_Lookup.TryGetValue(element.Id, out string stringValue))
                    {
                        globalValue.StringValue = stringValue;
                    }

                    actions.Add(() => SelectedConstantDetails = globalValue);
                }
                break;
            case APIElementType.C_OBJECT_TYPE:
            case APIElementType.OBJECT_TYPE:
                if (doc.Objects.TryGetValue(element.Id, out EsoUIObject esoUIObject))
                {
                    actions.Add(() => SelectedObjectDetails = esoUIObject);
                }
                break;
            case APIElementType.C_OBJECT_METHOD:
            case APIElementType.OBJECT_METHOD:
                if (doc.Objects.TryGetValue(element.Parent, out EsoUIObject parent)
                    && parent.Functions.TryGetValue(element.Id, out EsoUIFunction function))
                {
                    actions.Add(() => SelectedMethodDetails = function);
                }
                break;
            case APIElementType.INSTANCE_NAME:
                if (doc.InstanceNames.TryGetValue(element.Id, out EsoUIInstance instanceName))
                {
                    actions.Add(() => SelectedInstanceDetails = instanceName);
                }
                break;
        }

        if (!token.IsCancellationRequested)
        {
            foreach (Action action in actions)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                _dialogService.RunOnMainThread(action);
            }
        }
    }

    private static CancellationToken ResetCancellationTokenSource(ref CancellationTokenSource cancellationTokenSource)
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = new CancellationTokenSource();
        return cancellationTokenSource.Token;
    }

    private void SetSearchAlgorithm()
    {
        string selectedAlgorithmName = _settings.Values["SearchAlgorithm"].ToString();

        if (selectedAlgorithmName != _currentAlgorithmName)
        {
            Type algorithm = _searchAlgorithms.FirstOrDefault(a => a.GetStaticPropertyValue("Name") == selectedAlgorithmName);
            _searchAlgorithm = Activator.CreateInstance(algorithm) as ISearchAlgorithm;
            _currentAlgorithmName = selectedAlgorithmName;
        }
    }

    private bool HasMatchingArgument(APIElement element, string value)
    {
        EsoUIDocumentation docs = _esoDocumentationService.Documentation;

        if (docs == null)
        {
            return false;
        }

        switch (element.ElementType)
        {
            case APIElementType.C_FUNCTION:
            case APIElementType.FUNCTION:
                if (docs.Functions.TryGetValue(element.Name, out EsoUIFunction esofunction))
                {
                    return (esofunction.Args != null && esofunction.Args.Any(a => a.Name.StartsWith(value, StringComparison.OrdinalIgnoreCase)));
                }
                break;
            case APIElementType.C_OBJECT_METHOD:
            case APIElementType.OBJECT_METHOD:
                if (docs.Objects.TryGetValue(element.Parent, out EsoUIObject esoobject))
                {
                    if (esoobject != null && !esoobject.Functions.IsEmpty)
                    {
                        return esoobject.Functions.Any(f => f.Value.Args.Any(a => a.Name.StartsWith(value, StringComparison.OrdinalIgnoreCase)));
                    }
                }
                break;
            case APIElementType.EVENT:
                if (docs.Events.TryGetValue(element.Name, out EsoUIEvent esoevent))
                {
                    return (esoevent.Args != null && esoevent.Args.Any(a => a.Name.StartsWith(value, StringComparison.OrdinalIgnoreCase)));
                }
                break;
        }

        return false;
    }

    private bool HasMatchingValue(APIElement element, string value)
    {
        EsoUIDocumentation docs = _esoDocumentationService.Documentation;

        if (docs == null || string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        switch (element.ElementType)
        {
            case APIElementType.CONSTANT:
            case APIElementType.GLOBAL:
            case APIElementType.SI_GLOBAL:
                if (docs.Constants.TryGetValue(element.Id, out EsoUIConstantValue constantValue))
                {
                    return (constantValue.Value?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false)
                        || (constantValue.StringValue?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false);
                }

                if (docs.Globals.TryGetValue("Globals", out ICollection<EsoUIEnumValue> globals))
                {
                    return globals.Any(global => global.Name == element.Id && (global.Value?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                break;
            case APIElementType.ENUM_CONSTANT:
                if (docs.Globals.TryGetValue(element.Parent, out ICollection<EsoUIEnumValue> enumValues))
                {
                    return enumValues.Any(global => global.Name == element.Id && (global.Value?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                break;
            case APIElementType.ENUM_TYPE:
                if (docs.Globals.TryGetValue(element.Id, out ICollection<EsoUIEnumValue> enumTypeValues))
                {
                    return enumTypeValues.Any(global => global.Value?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false);
                }

                break;
        }

        return false;
    }

    private void UpdateStatus(IReadOnlyCollection<APIElement> filteredItems)
    {
        Status = new StatusInformation
        {
            APIItems = filteredItems.Count,
            APIVersion = _esoDocumentationService.Documentation?.ApiVersion ?? 0,
            CFunctionItems = filteredItems.Count(f => f.ElementType == APIElementType.C_FUNCTION),
            CMethodItems = filteredItems.Count(f => f.ElementType == APIElementType.C_OBJECT_METHOD),
            CObjectItems = filteredItems.Count(f => f.ElementType == APIElementType.C_OBJECT_TYPE),
            ConstantItems = filteredItems.Count(f => f.ElementType == APIElementType.CONSTANT),
            EnumConstants = filteredItems.Count(g => g.ElementType == APIElementType.ENUM_CONSTANT),
            EnumTypes = filteredItems.Count(g => g.ElementType == APIElementType.ENUM_TYPE),
            EventItems = filteredItems.Count(e => e.ElementType == APIElementType.EVENT),
            FunctionItems = filteredItems.Count(f => f.ElementType == APIElementType.FUNCTION),
            GlobalInstanceItems = filteredItems.Count(f => f.ElementType == APIElementType.INSTANCE_NAME),
            MethodItems = filteredItems.Count(f => f.ElementType == APIElementType.OBJECT_METHOD),
            ObjectItems = filteredItems.Count(f => f.ElementType == APIElementType.OBJECT_TYPE),
            SIGlobalItems = filteredItems.Count(f => f.ElementType == APIElementType.SI_GLOBAL),
        };
    }

    private void ClearFilteredItems()
    {
        FilteredItems = [];
        UpdateStatus(FilteredItems);
    }

    private void ApplyFilteredItems(IReadOnlyList<APIElement> filteredItems)
    {
        FilteredItems = new ObservableCollection<APIElement>(filteredItems);
        UpdateStatus(FilteredItems);
    }

    private async Task<List<APIElement>> FilterKeywordsAsync(IEnumerable<APIElement> keywordList, string filter, CancellationToken cancellationToken)
    {
        return await Task.Run(() => FilterKeywords(keywordList, filter).ToList(), cancellationToken);
    }

    private IOrderedEnumerable<APIElement> FilterKeywords(IEnumerable<APIElement> keywordList, string filter)
    {
        IEnumerable<APIElement> keywords = keywordList ?? [];
        EsoUIDocumentation documentation = _esoDocumentationService.Documentation;

        if (string.IsNullOrWhiteSpace(filter))
        {
            return keywords.Order();
        }

        SetSearchAlgorithm();

        if (filter.StartsWith('@'))
        {
            return (AllItems ?? [])
                .Where(d =>
                    d.Value.ElementType == APIElementType.C_FUNCTION ||
                    d.Value.ElementType == APIElementType.C_OBJECT_METHOD ||
                    d.Value.ElementType == APIElementType.EVENT ||
                    d.Value.ElementType == APIElementType.FUNCTION ||
                    d.Value.ElementType == APIElementType.OBJECT_METHOD)
                .Select(d => d.Value)
                .Where(v => HasMatchingArgument(v, filter.Substring(1)))
                .OrderBy(v => v.Name);
        }

        if (filter.StartsWith('$'))
        {
            return (AllItems ?? [])
                .Where(d =>
                    d.Value.ElementType == APIElementType.SI_GLOBAL &&
                    documentation?.SI_Lookup != null &&
                    documentation.SI_Lookup.TryGetValue(d.Value.Name, out string value) &&
                    value.Contains(filter.Substring(1), StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Value)
                .OrderBy(v => v.Name);
        }

        if (filter.StartsWith('='))
        {
            return (AllItems ?? [])
                .Select(d => d.Value)
                .Where(v => HasMatchingValue(v, filter.Substring(1)))
                .OrderBy(v => v.Name);
        }

        return _searchAlgorithm.Search(filter, keywords);
    }

    private async Task FilterItemsAsync()
    {
        CancellationToken cancellationToken = ResetCancellationTokenSource(ref _filterItemsTokenSource);

        try
        {
            await Task.Delay(FilterDebounceDelay, cancellationToken);

            IReadOnlyList<APIElement> allItems = AllItems?.Select(i => i.Value).ToList();

            if (allItems == null || allItems.Count == 0)
            {
                _dialogService.RunOnMainThread(ClearFilteredItems);
                return;
            }

            string filterText = FilterText ?? string.Empty;
            List<APIElement> filtered = await FilterKeywordsAsync(allItems, filterText, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _dialogService.RunOnMainThread(() => ApplyFilteredItems(filtered));
        }
        catch (OperationCanceledException)
        {
        }
    }


    public ICommand SearchGithubCommand => new RelayCommand(() => _ = Windows.System.Launcher.LaunchUriAsync(new Uri($"https://github.com/esoui/esoui/search?q={_selectedElement.Name}&type=code")));
    public ICommand SearchWikiCommand => new RelayCommand(() =>
    {
        string subpath;

        switch (_selectedElement.ElementType)
        {
            case APIElementType.OBJECT_TYPE:
                subpath = "Controls#";
                break;
            case APIElementType.SI_GLOBAL:
            case APIElementType.GLOBAL:
                subpath = _selectedElement.ElementType == APIElementType.SI_GLOBAL ? "Constant_Values_SI*" : "Constant_Values#";
                break;
            case APIElementType.CONSTANT:
                subpath = "Constant_Values#";
                break;
            case APIElementType.ENUM_CONSTANT:
            case APIElementType.ENUM_TYPE:
                subpath = "Globals#";
                break;
            default:
                subpath = string.Empty;
                break;
        }

        _ = Windows.System.Launcher.LaunchUriAsync(new Uri($"https://wiki.esoui.com/{subpath}{_selectedElement.Name}"));
    });

    private IEnumerable<string> GetUsedBy(string enumName)
    {
        EsoUIDocumentation documentation = _esoDocumentationService.Documentation;

        if (documentation == null)
        {
            return [];
        }

        HashSet<string> usedBy = [];

        foreach (KeyValuePair<string, EsoUIEvent> item in documentation.Events)
        {
            if (item.Value.Args.Any(arg => arg.Type.Name == enumName))
            {
                usedBy.Add(item.Key);
            }
        }

        foreach (KeyValuePair<string, EsoUIFunction> item in documentation.Functions)
        {
            if (item.Value.Args.Any(arg => arg.Type.Name == enumName)
                || item.Value.Returns.Any(retval => retval.Values.Any(ret => ret.Type.Name == enumName)))
            {
                usedBy.Add(item.Key);
            }
        }

        return usedBy.Order();
    }

    private void AddToHistory(string value)
    {
        _historyStack.Push(value);

        if (_historyStack.Count > 1 && !CanGoBack)
        {
            SetProperty(ref _canGoBack, true, nameof(CanGoBack));
        }
    }

    private void SelectElement(string elementName, bool doNotAddToHistory = false)
    {
        if (!doNotAddToHistory)
        {
            AddToHistory(elementName);
        }

        SetProperty(ref _selectedElement, AllItems.First(i => i.Value.Name == elementName).Value, nameof(SelectedElement));
        UpdateSelectedElementDetails();
    }

    public ICommand HandleSelectedItemElement => new RelayCommand<string>((elementName) =>
    {
        SelectElement(elementName);
    });

    public ICommand GoBack => new RelayCommand(() =>
    {
        string previous = _historyStack.Pop();

        if (previous == SelectedElement?.Name)
        {
            // pop again to remove the current selection from the stack
            previous = _historyStack.Pop();
        }

        if (_historyStack.Count == 0)
        {
            SetProperty(ref _canGoBack, false, nameof(CanGoBack));
        }

        if (!string.IsNullOrEmpty(previous))
        {
            SelectElement(previous, true);
        }
    });
}
