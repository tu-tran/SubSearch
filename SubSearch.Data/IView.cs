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

    /// <summary>
    /// The <see cref="CustomActionDelegate"/> delegate provides the signature for.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="parameter">The parameter.</param>
    /// <param name="actionNames">The action names.</param>
    public delegate void CustomActionDelegate(IView sender, object parameter, params string[] actionNames);

    /// <summary>
    /// The <see cref="IView"/> interfaces.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IView : IDisposable
    {
        /// <summary>
        /// Occurs when custom action requested.
        /// </summary>
        event CustomActionDelegate CustomActionRequested;

        /// <summary>
        /// Continues this instance.
        /// </summary>
        void Continue();

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        /// <returns>The query result.</returns>
        QueryResult<ItemData> GetSelection(ICollection<ItemData> data, string title, string status);

        /// <summary>Notifies a message.</summary>
        /// <param name="message">The message.</param>
        void Notify(string message);

        /// <summary>
        /// Shows progress.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        void ShowProgress(string title, string status);

        /// <summary>
        /// Shows progress.
        /// </summary>
        /// <param name="done">The done.</param>
        /// <param name="total">The total.</param>
        void ShowProgress(int done, int total);
    }
}