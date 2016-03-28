// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IView.cs" company="">
//   
// </copyright>
// <summary>
//   The delegate for view custom action.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SubSearch.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>The delegate for view custom action.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="parameter">The action parameter.</param>
    /// <param name="actionNames">The action names.</param>
    public delegate void CustomActionDelegate(IView sender, object parameter, params string[] actionNames);

    /// <summary>The ViewHandler interface.</summary>
    public interface IView : IDisposable
    {
        /// <summary>Occurs when the view custom action is requested.</summary>
        event CustomActionDelegate CustomActionRequested;

        /// <summary>Continues the pending operation and cancel any selection.</summary>
        void Continue();

        /// <summary>The get selection.</summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        /// <returns>The <see cref="ItemData"/>.</returns>
        Tuple<QueryResult, ItemData> GetSelection(ICollection<ItemData> data, string title, string status);

        /// <summary>Notifies a message.</summary>
        /// <param name="message">The message.</param>
        void Notify(string message);

        /// <summary>The show progress.</summary>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        void ShowProgress(string title, string status);

        /// <summary>Sets the progress.</summary>
        /// <param name="done">Done.</param>
        /// <param name="total">Total</param>
        void ShowProgress(int done, int total);
    }
}