// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActionCommand{T}.cs" company="">
//   
// </copyright>
// <summary>
//   The <see cref="ActionCommand{T}" /> class represents an input command which invokes a delegate.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SubSearch.WPF
{
    using System;

    /// <summary>The <see cref="ActionCommand{T}"/> class represents an input command which invokes a delegate.</summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    public class ActionCommand<T> : CommandBase
    {
        /// <summary>The command action.</summary>
        private readonly Action<T> action;

        /// <summary>The predicate determining whether the command can be executed.</summary>
        private readonly Predicate<T> canExecute;

        /// <summary>Initializes a new instance of the <see cref="ActionCommand{T}"/> class.</summary>
        /// <param name="action">The action.</param>
        public ActionCommand(Action<T> action)
            : this(action, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ActionCommand{T}"/> class.</summary>
        /// <param name="action">The action.</param>
        /// <param name="canExecute">The predicate determining whether the command can be executed.</param>
        public ActionCommand(Action<T> action, Predicate<T> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        /// <summary>Determines whether the command can be executed.</summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        public override bool CanExecute(object parameter)
        {
            return this.action != null && (this.canExecute == null || this.canExecute(GetParameter(parameter)));
        }

        /// <summary>Executes the command.</summary>
        /// <param name="parameter">The command parameter.</param>
        public override void Execute(object parameter)
        {
            this.action(GetParameter(parameter));
        }

        /// <summary>Get the parameter object based on the <paramref name="data"/>.</summary>
        /// <param name="data">The data.</param>
        /// <returns>The parameter object based on the <paramref name="data"/>.</returns>
        private static T GetParameter(object data)
        {
            if (data != null)
            {
                if (data is T)
                {
                    return (T)data;
                }

                if (data is IConvertible)
                {
                    return (T)Convert.ChangeType(data, typeof(T));
                }
            }

            return default(T);
        }
    }
}