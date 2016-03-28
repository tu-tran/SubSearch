// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WPFViewHandler.cs" company="">
//   
// </copyright>
// <summary>
//   The wpf view handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SubSearch.WPF
{
    using SubSearch.Data;
    using SubSearch.WPF.View;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows.Threading;

    /// <summary>The wpf view handler.</summary>
    internal class WpfViewHandler : IViewHandler
    {
        /// <summary>
        /// The UI thread.
        /// </summary>
        private Thread uiThread;

        /// <summary>
        /// The window.
        /// </summary>
        private MainWindow window;

        /// <summary>
        /// The disposing.
        /// </summary>
        private bool disposing;

        /// <summary>Initializes a new instance of the <see cref="WpfViewHandler" /> class.</summary>
        public WpfViewHandler()
        {
            this.CreateWindow();
        }

        /// <summary>Occurs when the view handler custom action is requested.</summary>
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

        /// <summary>The get selection.</summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        /// <returns>The <see cref="ItemData"/>.</returns>
        public virtual Tuple<QueryResult, ItemData> GetSelection(ICollection<ItemData> data, string title, string status)
        {
            var sortData = data.OrderByDescending(i => i.Icon).ThenBy(i => i.Text).ToList();
            var token = new CancellationTokenSource();
            this.window.SetSelections(sortData, title, status, token);
            token.Token.WaitHandle.WaitOne();
            return Tuple.Create(this.window.SelectionState, this.window.SelectedItem);
        }

        /// <summary>Notifies a message.</summary>
        /// <param name="message">The message.</param>
        public void Notify(string message)
        {
            NotificationWindow.Show(message);
        }

        /// <summary>The on custom action.</summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="actionNames">The action names.</param>
        public void OnCustomAction(object parameter, params string[] actionNames)
        {
            if (this.CustomActionRequested != null)
            {
                this.CustomActionRequested(this, parameter, actionNames);
            }
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