// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WPFViewHandler.cs" company="">
//   
// </copyright>
// <summary>
//   The wpf view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SubSearch.WPF
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows.Threading;

    using SubSearch.Data;
    using SubSearch.WPF.View;

    /// <summary>
    /// The <see cref="WpfView"/> class.
    /// </summary>
    internal class WpfView : IView
    {
        /// <summary>The disposing.</summary>
        private bool disposing;

        /// <summary>The UI thread.</summary>
        private Thread uiThread;

        /// <summary>The window.</summary>
        private MainWindow window;

        /// <summary>Initializes a new instance of the <see cref="WpfView" /> class.</summary>
        public WpfView()
        {
            this.CreateWindow();
        }

        /// <summary>Occurs when the view custom action is requested.</summary>
        public event CustomActionDelegate CustomActionRequested;

        /// <summary>Continues the pending operation and cancel any selection.</summary>
        public void Continue()
        {
            this.window.Accept(QueryResult.Skipped);
        }

        /// <summary>The dispose.</summary>
        public void Dispose()
        {
            if (this.window != null && this.window.Dispatcher != null && !this.window.Dispatcher.HasShutdownStarted)
            {
                this.window.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            }

            this.disposing = true;
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
            this.window.SetSelections(data, title, status, token);
            token.Token.WaitHandle.WaitOne();
            return new QueryResult<ItemData>(this.window.SelectionState, this.window.SelectedItem);
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
            this.window.SetProgress(title, status);
        }

        /// <summary>Sets the progress.</summary>
        /// <param name="done">Done.</param>
        /// <param name="total">Total</param>
        public void ShowProgress(int done, int total)
        {
            this.window.SetProgress(done, total);
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
        /// Creates the window on its own UI thread.
        /// </summary>
        private void CreateWindow()
        {
            var token = new CancellationTokenSource();
            this.uiThread = new Thread(
                () =>
                    {
                        Thread.CurrentThread.Name = "WpfView." + DateTime.Now.ToString("HH.mm.ss");
                        while (!this.disposing)
                        {
                            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
                            this.window = new MainWindow(this);
                            this.window.Closed += (sender, args) => Dispatcher.ExitAllFrames();
                            token.Cancel();
                            Dispatcher.Run();
                        }
                    });

            this.uiThread.SetApartmentState(ApartmentState.STA);
            this.uiThread.Start();
            token.Token.WaitHandle.WaitOne();
        }
    }
}