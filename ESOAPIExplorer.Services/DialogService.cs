using ESOAPIExplorer.ViewModels;
using ESOAPIExplorer.Views;
using ESOAPIExplorer.Views.Dialogs;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services;

public class DialogService(DispatcherQueue mainDispatcherQueue, MainWindow mainWindow, CustomMessageDialogViewModel viewModel) : IDialogService
{
    private readonly MainWindow _MainWindow = mainWindow;
    private readonly DispatcherQueue _MainDispatcherQueue = mainDispatcherQueue;
    private readonly CustomMessageDialogViewModel _ViewModel = viewModel;

    public Task ShowAsync(string message, string title = "Message") => ShowAsync(message, title, "Ok", null);

    public async Task ShowAsync(string message, string title = "Message", string positiveText = "Ok", string negativeText = "Cancel", Action positiveCallback = null, Action negativeCallback = null, bool isSelectable = false)
    {
        CustomMessageDialog messageDialog = new()
        {
            DataContext = _ViewModel,
            XamlRoot = _MainWindow.Content.XamlRoot
        };

        // Create the message dialog and set its content
        _ViewModel.Title = title;
        _ViewModel.Message = message;
        _ViewModel.PositiveText = positiveText;
        _ViewModel.NegativeText = negativeText;
        _ViewModel.IsSelectable = isSelectable;

        positiveCallback ??= () => messageDialog.Hide();
        negativeCallback ??= () => messageDialog.Hide();

        EventHandler<bool> responseEnteredHandler = (source, args) =>
        {
            if (args)
            {
                positiveCallback?.Invoke();
            }
            else
            {
                negativeCallback?.Invoke();
            }
        };

        _ViewModel.ResponseEntered += responseEnteredHandler;

        try
        {
            await messageDialog.ShowAsync();
        }
        finally
        {
            _ViewModel.ResponseEntered -= responseEnteredHandler;
        }
    }

    public bool RunOnMainThread(Action action)
    {
        bool success = _MainDispatcherQueue.TryEnqueue(new DispatcherQueueHandler(action));

        return success;
    }
}
