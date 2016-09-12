namespace SubSearch.Tests
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using SubSearch.Data;

    /// <summary>
    /// The <see cref="DummyView"/> class represents a dummy view handler.
    /// </summary>
    /// <seealso cref="SubSearch.Data.IView" />
    public sealed class DummyView : IView
    {
        /// <summary>
        /// Occurs when custom action requested.
        /// </summary>
        public event CustomActionDelegate CustomActionRequested;

        /// <summary>
        /// Continues this instance.
        /// </summary>
        public void Continue()
        {
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        /// <returns>The query result.</returns>
        public QueryResult<ItemData> GetSelection(ICollection<ItemData> data, string title, string status)
        {
            return new QueryResult<ItemData>(Status.Success, data.FirstOrDefault());
        }

        /// <summary>
        /// Notifies a message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Notify(string message)
        {
            Trace.WriteLine(message);
        }

        /// <summary>
        /// Shows progress.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        public void ShowProgress(string title, string status)
        {
            Trace.WriteLine(string.Format("[{0}] {1}", title, status));
        }

        /// <summary>
        /// Shows progress.
        /// </summary>
        /// <param name="done">The done.</param>
        /// <param name="total">The total.</param>
        public void ShowProgress(int done, int total)
        {
            Trace.WriteLine(string.Format("Progress: {0}/{1}", done, total));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
