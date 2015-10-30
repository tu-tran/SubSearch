namespace SubSearch.WPF
{
    using System;
    using System.Collections.Generic;

    using SubSearch.Data;
    using SubSearch.WPF.View;

    /// <summary>The wpf view handler.</summary>
    internal class WpfViewHandler : IViewHandler
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
        public void ShowProgress(string title, string status)
        {
            MainWindow.ShowProgress(title, status);
        }

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
        public virtual ItemData GetSelection(IEnumerable<ItemData> data, string title, string status)
        {
            return MainWindow.GetSelection(data, title, status);
        }

        /// <summary>
        /// Notifies a message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Notify(string message)
        {
            NotificationWindow.Show(message);
        }

        /// <summary>
        /// Starts the view and wait for interaction.
        /// </summary>
        public void Start()
        {
            MainWindow.Start();
        }

        /// <summary>The dispose.</summary>
        void IDisposable.Dispose()
        {
            MainWindow.CloseAll();
        }
    }
}