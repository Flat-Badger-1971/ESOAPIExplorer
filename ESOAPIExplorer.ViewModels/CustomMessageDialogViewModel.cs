using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;

namespace ESOAPIExplorer.ViewModels;

public partial class CustomMessageDialogViewModel:ViewModelBase
{
    public CustomMessageDialogViewModel()
    {
        ActionMessage = "";
    }

    private string _Title;
    public string Title { get => _Title; set => SetProperty(ref _Title, value); }

    private string _Message;
    public string Message
    {
        get => _Message;
        set => SetProperty(ref _Message, value);
    }

    private string _ActionMessage;
    public string ActionMessage
    {
        get => _ActionMessage;
        set => SetProperty(ref _ActionMessage, value);
    }

    private bool _IsSelectable = false;
    public bool IsSelectable { get => _IsSelectable; set => SetProperty(ref _IsSelectable, value); }

    private string _PosativeText;
    public string PosativeText { get => _PosativeText; set => SetProperty(ref _PosativeText, value); }

    private string _NegativeText;
    public string NegativeText 
    { 
        get => _NegativeText;
        set
        {
            IsNegativeButtonVisible = !string.IsNullOrEmpty(value);
            SetProperty(ref _NegativeText, value);
        }
    }

    private bool _IsNegativeButtonVisible;
    public bool IsNegativeButtonVisible { get => _IsNegativeButtonVisible; set => SetProperty(ref _IsNegativeButtonVisible, value); }

    public ICommand ResponseEnteredCommand => new RelayCommand<bool>((response) => 
    {
        OnResponseEntered(response);
    });

    protected virtual void OnResponseEntered(bool response)
    {
        bool args = false;
        ResponseEntered?.Invoke(this, args);
    }

    DateTime _LastCopied = DateTime.Now;
    public ICommand CopyToClipboardCommand => new RelayCommand(async () =>
    {
        DataPackage dataPackage = new()
        {
            RequestedOperation = DataPackageOperation.Copy
        };
        dataPackage.SetText(_Message);
        Clipboard.SetContent(dataPackage);
        ActionMessage = "Copied to clipboard";

        DateTime now = DateTime.Now;
        _LastCopied = now;

        await Task.Delay(4000);
        if (_LastCopied == now)
        {
            ActionMessage = "";
        }
        
    });

    public event EventHandler<bool>ResponseEntered;
}
