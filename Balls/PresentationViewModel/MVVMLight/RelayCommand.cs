
using System;
using System.Windows.Input;

namespace ConcurrentProgramming.Presentation.ViewModel.MVVMLight
{
  public class RelayCommand : ICommand
  {
    #region constructors

    public RelayCommand(Action execute) : this(execute, null) { }

    public RelayCommand(Action execute, Func<bool> canExecute)
    {
      this.m_Execute = execute ?? throw new ArgumentNullException(nameof(execute));
      this.m_CanExecute = canExecute;
    }

    #endregion constructors

    #region ICommand

    public bool CanExecute(object parameter)
    {
      if (this.m_CanExecute == null)
        return true;
      if (parameter == null)
        return this.m_CanExecute();
      return this.m_CanExecute();
    }

    public virtual void Execute(object parameter)
    {
      this.m_Execute();
    }

    public event EventHandler CanExecuteChanged;

    #endregion ICommand

    #region API

    internal void RaiseCanExecuteChanged()
    {
      this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion API

    #region private

    private readonly Action m_Execute;
    private readonly Func<bool> m_CanExecute;

    #endregion private
  }
}