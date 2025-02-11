﻿using System;
using System.Windows.Input;

namespace ESOAPIExplorer.ViewModels;

public partial class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    /// <summary>
    /// Raised when RaiseCanExecuteChanged is called.
    /// </summary>
    public event EventHandler CanExecuteChanged;

    /// <summary>
    /// Creates a new command that can always execute.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    public RelayCommand(Action execute) : this(execute, null) { }

    /// <summary>
    /// Creates a new command.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    public RelayCommand(Action execute, Func<bool> canExecute)
    {
        ArgumentNullException.ThrowIfNull(execute);
        _execute = execute;
        _canExecute = canExecute;
    }

    /// <summary>
    /// Determines whether this <see cref="RelayCommand"/> can execute in its current state.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
    /// </param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

    /// <summary>
    /// Executes the <see cref="RelayCommand"/> on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
    /// </param>
    public void Execute(object parameter) => _execute();

    /// <summary>
    /// Method used to raise the <see cref="CanExecuteChanged"/> event
    /// to indicate that the return value of the <see cref="CanExecute"/>
    /// method has changed.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        EventHandler handler = CanExecuteChanged;

        if (handler != null)
        {
            try
            {
                handler(this, EventArgs.Empty);
            }
#pragma warning disable JS0025
            catch { }
        }
    }
}
