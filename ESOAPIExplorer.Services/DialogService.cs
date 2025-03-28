using ESOAPIExplorer.ViewModels;
using ESOAPIExplorer.Views.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ESOAPIExplorer.Services;

public class DialogService(Dispatcher mainDispatcherQueue, CustomMessageDialogViewModel viewModel) : IDialogService
{
    private readonly Dispatcher _mainDispatcher = mainDispatcherQueue;
    private readonly CustomMessageDialogViewModel _viewModel = viewModel;

    public Task ShowAsync(string message, string title = "Message") => ShowAsync(message, title, "Ok", null);

    public Task ShowAsync(string message, string title = "Message", string positiveText = "Ok", string negativeText = "Cancel", Action positiveCallback = null, Action negativeCallback = null, bool isSelectable = false)
    {
        CustomMessageDialog messageDialog = new()
        {
            DataContext = _viewModel,
            // XamlRoot = _MainWindow.Content.XamlRoot
        };

        // Create the message dialog and set its content
        _viewModel.Title = title;
        _viewModel.Message = message;
        _viewModel.PositiveText = positiveText;
        _viewModel.NegativeText = negativeText;
        _viewModel.IsSelectable = isSelectable;

        positiveCallback ??= () => messageDialog.Hide();
        negativeCallback ??= () => messageDialog.Hide();

        _viewModel.ResponseEntered += (source, args) =>
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

        return Task.FromResult(messageDialog.ShowDialog());
    }

    public bool RunOnMainThread(Action action)
    {
        try
        {
            _mainDispatcher.Invoke(action);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
