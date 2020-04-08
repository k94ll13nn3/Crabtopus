using System;
using System.Windows.Input;

namespace Crabtopus.ViewModels
{
    internal class DelegateCommand : ICommand
    {
        private readonly Func<bool> _canExecute;

        private readonly Action _execute;

        public DelegateCommand(Action execute)
            : this(execute, () => true)
        {
        }

        public DelegateCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged;

        /// <inheritdoc/>
        public bool CanExecute(object parameter)
        {
            return _canExecute();
        }

        /// <inheritdoc/>
        public void Execute(object parameter)
        {
            _execute();
        }

        public void UpdateCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
