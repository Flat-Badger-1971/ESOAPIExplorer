using ESOAPIExplorer.ViewModels;
using ESOAPIExplorer.Views.Dialogs;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Window = Microsoft.UI.Xaml.Window;

namespace ESOAPIExplorer.Services;

public class DialogService(DispatcherQueue mainDispatcherQueue, CustomMessageDialogViewModel viewModel) : IDialogService
{
    readonly Window _MainWindow = (Window)Application.Current.GetType().GetProperty("MainWindow").GetValue(Application.Current);
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

        _ViewModel.ResponseEntered += (source, args) =>
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

        await messageDialog.ShowAsync();
    }

    public bool RunOnMainThread(Action action)
    {
        bool success = _MainDispatcherQueue.TryEnqueue(new DispatcherQueueHandler(action));

        return success;
    }
}
