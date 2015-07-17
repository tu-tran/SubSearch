namespace SubSearch.WPF
{
    using System.Collections.Generic;

    using SubSearch.Data;
    using SubSearch.WPF.View;

    /// <summary>The wpf view handler.</summary>
    internal sealed class WPFViewHandler : IViewHandler
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
        public ItemData GetSelection(IEnumerable<ItemData> data, string title, string status)
        {
            return MainWindow.GetSelection(data, title, status);
        }

        /// <summary>The dispose.</summary>
        public void Dispose()
        {
            MainWindow.CloseAll();
        }
    }
}