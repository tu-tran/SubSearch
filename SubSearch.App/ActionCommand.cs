// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActionCommand.cs" company="">
//   
// </copyright>
// <summary>
//   The  class represents an input command which invokes a delegate.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SubSearch.WPF
{
    using System;

    /// <summary>The <see cref="ActionCommand" /> class represents an input command which invokes a delegate.</summary>
    public class ActionCommand : CommandBase
    {
        /// <summary>The command action.</summary>
        private readonly Action action;

        /// <summary>The predicate determining whether the command can be executed.</summary>
        private readonly Func<bool> canExecute;

        /// <summary>Initializes a new instance of the <see cref="ActionCommand" /> class.</summary>
        /// <param name="action">The action.</param>
        public ActionCommand(Action action)
            : this(action, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ActionCommand" /> class.</summary>
        /// <param name="action">The action.</param>
        /// <param name="canExecute">The predicate determining whether the command can be executed.</param>
        public ActionCommand(Action action, Func<bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        /// <summary>Determines whether the command can be executed.</summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        public override bool CanExecute(object parameter)
        {
            return this.action != null && (this.canExecute == null || this.canExecute());
        }

        /// <summary>Executes the command.</summary>
        /// <param name="parameter">The command parameter.</param>
        public override void Execute(object parameter)
        {
            this.action();
        }
    }
}