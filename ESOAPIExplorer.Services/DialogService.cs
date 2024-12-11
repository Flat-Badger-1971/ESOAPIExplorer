using ESOAPIExplorer.ViewModels;
using ESOAPIExplorer.Views.Dialogs;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Window = Microsoft.UI.Xaml.Window;

namespace ESOAPIExplorer.Services;

public class DialogService(DispatcherQueue mainDispatcherQueue, CustomMessageDialogViewModel viewModel) : IDialogService
{
    readonly Window _MainWindow = (Window)Application.Current.GetType().GetProperty("Window").GetValue(Application.Current);
    private readonly DispatcherQueue _MainDispatcherQueue = mainDispatcherQueue;
    private readonly CustomMessageDialogViewModel _ViewModel = viewModel;

    public Task ShowAsync(string message, string title = "Message")
    {
        return ShowAsync(message, title, "Ok", null);
    }

    public async Task ShowAsync(string message, string title = "Message", string positiveText = "Ok", string negativeText = "Cancel", Action posativeCallback = null, Action negativeCallback = null, bool isSelectable = false)
    {
        CustomMessageDialog messageDialog = new()
        {
            DataContext = _ViewModel,
            XamlRoot = _MainWindow.Content.XamlRoot
        };
        //IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_MainWindow);
        //WinRT.Interop.InitializeWithWindow.Initialize(messageDialog, hwnd);
        // Create the message dialog and set its content

        _ViewModel.Title = title;
        _ViewModel.Message = message;
        _ViewModel.PositiveText = positiveText;
        _ViewModel.NegativeText = negativeText;
        _ViewModel.IsSelectable = isSelectable;

        if (posativeCallback == null)
        {
            posativeCallback = () => messageDialog.Hide();
        }
        if (negativeCallback == null)
        {
            negativeCallback = () => messageDialog.Hide();
        }

        _ViewModel.ResponseEntered += (source, args) =>
        {
            //messageDialog.Hide();
            if (args)
            {
                posativeCallback?.Invoke();
            }
            else
            {
                negativeCallback?.Invoke();
            }
        };

        await messageDialog.ShowAsync();
    }

    public async Task ShowAsync_Legacy(string message, string title = "Message", string positiveText = "Ok", string negativeText = "Cancel", Action posativeCallback = null, Action negativeCallback = null)
    {
        var messageDialog = new MessageDialog(message, title);
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(messageDialog, hwnd);
        // Create the message dialog and set its content

        //Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
        messageDialog.Commands.Add(new UICommand(
            positiveText,
             new UICommandInvokedHandler((command) =>
             {
                 posativeCallback?.Invoke();
             })));

        if (!string.IsNullOrEmpty(negativeText))
        {
            messageDialog.Commands.Add(new UICommand(
                negativeText,
                new UICommandInvokedHandler((command) =>
                {
                    negativeCallback?.Invoke();
                })));

        }

        // Set the command that will be invoked by default
        messageDialog.DefaultCommandIndex = 0;

        // Set the command to be invoked when escape is pressed
        messageDialog.CancelCommandIndex = 1;

        await messageDialog.ShowAsync();

    }

    public void RunOnMainThread(Action action)
    {
        _MainDispatcherQueue.TryEnqueue(new DispatcherQueueHandler(action));
    }
}
