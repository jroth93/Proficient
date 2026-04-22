using System.Windows.Input;

namespace Proficient.Forms;

public class RelayCommand : ICommand
{
    readonly Action _targetExecuteMethod;
    private readonly Func<bool>? _targetCanExecuteMethod;

    public RelayCommand(Action executeMethod)
    {
        _targetExecuteMethod = executeMethod;
    }

    public RelayCommand(Action executeMethod, Func<bool> canExecuteMethod)
    {
        _targetExecuteMethod = executeMethod;
        _targetCanExecuteMethod = canExecuteMethod;
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
    #region ICommand Members

    bool ICommand.CanExecute(object? parameter)
    {
        if (_targetCanExecuteMethod != null)
        {
            return _targetCanExecuteMethod();
        }
        return _targetExecuteMethod != null;
    }

    // Beware - should use weak references if command instance lifetime is longer than lifetime of UI objects that get hooked up to command
    // Prism commands solve this in their implementation
    public event EventHandler? CanExecuteChanged = delegate { };

    void ICommand.Execute(object? parameter)
    {
        if (_targetExecuteMethod != null)
        {
            _targetExecuteMethod();
        }
    }
    #endregion
}

public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _targetExecuteMethod;
    private readonly Func<T, bool>? _targetCanExecuteMethod;

    public RelayCommand(Action<T> executeMethod)
    {
        _targetExecuteMethod = executeMethod;
    }

    public RelayCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
    {
        _targetExecuteMethod = executeMethod;
        _targetCanExecuteMethod = canExecuteMethod;
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    bool ICommand.CanExecute(object? parameter)
    {
        if (_targetCanExecuteMethod != null)
        {
            return parameter is T tPar && _targetCanExecuteMethod(tPar);
        }
        return _targetExecuteMethod != null;
    }

    // Beware - should use weak references if command instance lifetime is longer than lifetime of UI objects that get hooked up to command
    // Prism commands solve this in their implementation
    public event EventHandler? CanExecuteChanged = delegate { };

    void ICommand.Execute(object? parameter)
    {
        if (_targetExecuteMethod != null && parameter is T tPar)
        {
            _targetExecuteMethod(tPar);
        }
    }
}