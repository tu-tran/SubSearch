// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandBase.cs" company="">
//   
// </copyright>
// <summary>
//   The  class represents an input command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SubSearch.WPF
{
    using System;
    using System.Windows.Input;

    /// <summary>The <see cref="CommandBase" /> class represents an input command.</summary>
    public abstract class CommandBase : ICommand
    {
        /// <summary>Raises when the command executable state has been changed.</summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>Determines whether the command can be executed.</summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        public abstract bool CanExecute(object parameter);

        /// <summary>Executes the command.</summary>
        /// <param name="parameter">The command parameter.</param>
        public abstract void Execute(object parameter);

        /// <summary>Raises the <see cref="CanExecuteChanged" /> event.</summary>
        public void RaiseCanExecuteChanged()
        {
            if (this.CanExecuteChanged != null)
            {
                this.CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}