using System;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services;

public interface IDialogService
{
    Task ShowAsync(string message, string title = "Message");
    Task ShowAsync(string message, string title = "Message", string positiveText = "Ok", string negativeText = "Cancel", Action posativeback = null, Action negativeback = null, bool isSelectable = false);
    void RunOnMainThread(Action action);
}
