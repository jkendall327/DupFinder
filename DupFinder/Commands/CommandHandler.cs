using System;
using System.Windows.Input;

namespace DupFinderApp.Commands
{
    // boilerplate
    // https://stackoverflow.com/questions/12422945/how-to-bind-wpf-button-to-a-command-in-viewmodelbase
    public class CommandHandler : ICommand
    {
        private readonly Action _action;
        private readonly Func<bool> _canExecute;

        public CommandHandler(Action action, Func<bool> canExecutePredicate)
        {
            _action = action;
            _canExecute = canExecutePredicate;
        }

        public CommandHandler(Action action)
        {
            _action = action;
            _canExecute = new Func<bool>(() => true);
        }

        /// <summary>
        /// Wires CanExecuteChanged event 
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute.Invoke();
        public void Execute(object? parameter) => _action();
    }
}
