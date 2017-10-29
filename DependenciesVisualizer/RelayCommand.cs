using System;
using System.Windows.Input;

namespace DependenciesVisualizer
{
    public class RelayCommand<T> : ICommand
    {
        #region Fields

        readonly Action<T> execute = null;
        readonly Predicate<T> canExecute = null;

        #endregion

        #region Constructors

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null ? true : this.canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void Execute(object parameter)
        {
            this.execute((T)parameter);
        }

        #endregion
    }

}
