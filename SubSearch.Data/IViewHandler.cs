namespace SubSearch.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>The ViewHandler interface.</summary>
    public interface IViewHandler : IDisposable
    {
        /// <summary>
        /// The show progress.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        void ShowProgress(string title, string status);

        /// <summary>
        /// The get selection.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <returns>
        /// The <see cref="ItemData"/>.
        /// </returns>
        ItemData GetSelection(IEnumerable<ItemData> data, string title, string status);

        /// <summary>
        /// Notifies a message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Notify(string message);

        /// <summary>
        /// Starts the view and wait for interaction.
        /// </summary>
        void Start();
    }
}