using System;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services;

public interface IDialogService
{
    public Task ShowAsync(string message, string title = "Message");
    public Task ShowAsync(string message, string title = "Message", string positiveText = "Ok", string negativeText = "Cancel", Action positiveback = null, Action negativeback = null, bool isSelectable = false);
    public bool RunOnMainThread(Action action);
}
