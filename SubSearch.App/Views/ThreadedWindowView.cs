namespace SubSearch.WPF.Views
{
    using System;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    /// The <see cref="ThreadedWindowView{TWindow}" /> represents a Window that is running on its own thread.
    /// </summary>
    /// <typeparam name="TWindow">The type of the window.</typeparam>
    internal abstract class ThreadedWindowView<TWindow> : IDisposable where TWindow : Window
    {
        /// <summary>The disposing.</summary>
        private bool disposing;

        /// <summary>The UI thread.</summary>
        private Thread uiThread;

        /// <summary>The Window.</summary>
        private TWindow window;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadedWindowView{TWindow}"/> class.
        /// </summary>
        protected ThreadedWindowView()
        {
            this.CreateThreadedWindowView();
        }

        /// <summary>
        /// Gets the Window.
        /// </summary>
        protected TWindow Window
        {
            get { return this.window; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.window != null && this.window.Dispatcher != null && !this.window.Dispatcher.HasShutdownStarted)
            {
                this.window.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            }

            this.disposing = true;
        }

        /// <summary>
        /// Creates the Window Window.
        /// </summary>
        /// <returns>The Window Window.</returns>
        protected abstract TWindow CreateWindowView();

        /// <summary>
        /// Creates the Window on its own UI thread.
        /// </summary>
        private void CreateThreadedWindowView()
        {
            var token = new CancellationTokenSource();
            this.uiThread = new Thread(
                () =>
                {
                    Thread.CurrentThread.Name = "WpfView." + DateTime.Now.ToString("HH.mm.ss");
                    while (!this.disposing)
                    {
                        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
                        this.window = this.CreateWindowView();
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