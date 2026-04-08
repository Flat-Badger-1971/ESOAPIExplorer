using System;
using System.Windows.Input;

namespace ESOAPIExplorer.ViewModels;

public partial class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<bool> _canExecute;
    private readonly Func<T, bool> _canExecuteWithParameter;

    /// <summary>
    /// Raised when RaiseCanExecuteChanged is called.
    /// </summary>
    public event EventHandler CanExecuteChanged;

    /// <summary>
    /// Creates a new command that can always execute.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    public RelayCommand(Action<T> execute)
        : this(execute, (Func<bool>)null)
    {
    }

    /// <summary>
    /// Creates a new command.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    public RelayCommand(Action<T> execute, Func<bool> canExecute)
    {
        ArgumentNullException.ThrowIfNull(execute);

        _execute = execute;
        _canExecute = canExecute;
    }

    public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
    {
        ArgumentNullException.ThrowIfNull(execute);

        _execute = execute;
        _canExecuteWithParameter = canExecute;
    }

    /// <summary>
    /// Determines whether this <see cref="RelayCommand"/> can execute in its current state.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
    /// </param>
    /// <returns>true if this command can be executed; otherwise, false.</returns>
    public bool CanExecute(object parameter)
    {
        if (_canExecute != null)
        {
            return _canExecute();
        }

        if (_canExecuteWithParameter != null)
        {
            return TryGetParameter(parameter, out T value) && _canExecuteWithParameter(value);
        }

        return parameter == null || TryGetParameter(parameter, out _);
    }

    /// <summary>
    /// Executes the <see cref="RelayCommand"/> on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
    /// </param>
    public void Execute(object parameter)
    {
        if (TryGetParameter(parameter, out T value))
        {
            _execute(value);
        }
    }

    private static bool TryGetParameter(object parameter, out T value)
    {
        if (parameter is T typedParameter)
        {
            value = typedParameter;
            return true;
        }

        if (parameter == null)
        {
            value = default;
            return default(T) == null;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Method used to raise the <see cref="CanExecuteChanged"/> event
    /// to indicate that the return value of the <see cref="CanExecute"/>
    /// method has changed.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        var handler = CanExecuteChanged;
        if (handler != null)
        {
            try
            {
                handler(this, EventArgs.Empty);
            }
            catch { }
        }
    }
}