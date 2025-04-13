using System;
using System.Windows.Input;

namespace ESOAPIExplorer.ViewModels;

public partial class CustomMessageDialogViewModel : ViewModelBase
{
    public CustomMessageDialogViewModel() => ActionMessage = "";

    private string _title;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _message;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private string _actionMessage;
    public string ActionMessage
    {
        get => _actionMessage;
        set => SetProperty(ref _actionMessage, value);
    }

    private bool _isSelectable = false;
    public bool IsSelectable
    {
        get => _isSelectable;
        set => SetProperty(ref _isSelectable, value);
    }

    private string _positiveText;
    public string PositiveText
    {
        get => _positiveText;
        set => SetProperty(ref _positiveText, value);
    }

    private string _negativeText;
    public string NegativeText
    {
        get => _negativeText;
        set
        {
            IsNegativeButtonVisible = !string.IsNullOrEmpty(value);
            SetProperty(ref _negativeText, value);
        }
    }

    private bool _isNegativeButtonVisible;
    public bool IsNegativeButtonVisible
    {
        get => _isNegativeButtonVisible;
        set => SetProperty(ref _isNegativeButtonVisible, value);
    }

    protected virtual void OnResponseEntered(bool response)
    {
        bool args = false;
        ResponseEntered?.Invoke(this, args);
    }

    private DateTime _lastCopied = DateTime.Now;

    public ICommand CopyToClipboardCommand => new RelayCommand(async () =>
    {
        //DataPackage dataPackage = new()
        //{
        //    RequestedOperation = DataPackageOperation.Copy
        //};

        //dataPackage.SetText(_Message);
        //Clipboard.SetContent(dataPackage);
        //ActionMessage = "Copied to clipboard";

        //DateTime now = DateTime.Now;
        //_LastCopied = now;

        //await Task.Delay(4000);

        //if (_LastCopied == now)
        //{
        //    ActionMessage = "";
        //}

    });

    public event EventHandler<bool> ResponseEntered;
}
