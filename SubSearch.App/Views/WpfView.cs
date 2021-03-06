// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WPFViewHandler.cs" company="">
//   
// </copyright>
// <summary>
//   The wpf view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SubSearch.WPF.Views
{
    using SubSearch.Data;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// The <see cref="WpfView"/> class.
    /// </summary>
    internal class WpfView : ThreadedWindowView<MainWindow>, IView
    {
        /// <summary>Occurs when the view custom action is requested.</summary>
        public event CustomActionDelegate CustomActionRequested;

        /// <summary>Continues the pending operation and cancel any selection.</summary>
        public void Continue()
        {
            this.Window.Accept(Status.Skipped);
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        /// <returns>The query result.</returns>
        public virtual QueryResult<ItemData> GetSelection(ICollection<ItemData> data, string title, string status)
        {
            var token = new CancellationTokenSource();
            this.Window.SetSelections(data, title, status, token);
            token.Token.WaitHandle.WaitOne();
            return new QueryResult<ItemData>(this.Window.SelectionState, this.Window.SelectedItem);
        }

        /// <summary>Notifies a message.</summary>
        /// <param name="message">The message.</param>
        public void Notify(string message)
        {
            NotificationView.Show(message);
        }

        /// <summary>The show progress.</summary>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        public void ShowProgress(string title, string status)
        {
            this.Window.SetProgress(title, status);
        }

        /// <summary>Sets the progress.</summary>
        /// <param name="done">Done.</param>
        /// <param name="total">Total</param>
        public void ShowProgress(int done, int total)
        {
            this.Window.SetProgress(done, total);
        }

        public void SetActiveFile(string filePath)
        {
            this.Window.SetActiveFile(filePath);
        }

        public void ResetSelections()
        {
            this.Window.ResetSelections();
        }

        /// <summary>The on custom action.</summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="actionNames">The action names.</param>
        public void OnCustomAction(object parameter, params string[] actionNames)
        {
            if (this.CustomActionRequested != null)
            {
                ThreadPool.QueueUserWorkItem(o => this.CustomActionRequested(this, parameter, actionNames));
            }
        }

        /// <summary>
        /// Creates the Window view.
        /// </summary>
        /// <returns>The Window view.</returns>
        protected override MainWindow CreateWindowView()
        {
            return new MainWindow(this);
        }
    }
}