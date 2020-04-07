using System;
using System.Windows.Input;

namespace Crabtopus.ViewModels
{
    internal class DelegateCommand<T> : ICommand
    {
        private readonly Predicate<object> _canExecute;

        private readonly Action<T> _execute;

        public DelegateCommand(Action<T> execute)
            : this(execute, _ => true)
        {
        }

        public DelegateCommand(Action<T> execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged;

        /// <inheritdoc/>
        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        /// <inheritdoc/>
        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public void UpdateCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
