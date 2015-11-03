namespace SubSearch.WPF
{
    using System;
    using System.Text;
    using System.Windows;
    using System.Windows.Threading;

    using SubSearch.Resources;
    using SubSearch.WPF.View;

    /// <summary>Interaction logic for App.xaml</summary>
    public partial class App
    {
        /// <summary>
        /// The on startup.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InitializeErrorHandler();
            var result = new QueueHandler(e.Args).Process();
            if (result == 0)
            {
                this.Dispatcher.InvokeShutdown();
                return;
            }

            var message = result == 1 ? Literals.ShellExtension_Subtitles_downloaded_successfully : Literals.ShellExtension_Failed_process_request;

            NotificationWindow.Show(
                message,
                (sender, args) =>
                {
                    if (args.NewValue.Equals(false))
                    {
                        this.Dispatcher.InvokeShutdown();
                    }
                });
        }

        /// <summary>
        /// Initializes the error handler.
        /// </summary>
        private static void InitializeErrorHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
        }

        /// <summary>
        /// Raises when there is unhandled exception.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private static void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs eventArgs)
        {
            CurrentDomainOnUnhandledException(sender, new UnhandledExceptionEventArgs(eventArgs.Exception, false));
            eventArgs.Handled = true;
        }

        /// <summary>
        /// Raises when there is unhandled exception.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs eventArgs)
        {
            var exception = eventArgs.ExceptionObject as Exception;
            var sb = new StringBuilder();

            while (exception != null)
            {
                sb.AppendLine(exception.ToString());
                sb.AppendLine("----------------------");
                exception = exception.InnerException;
            }

            var dialog = new MessageDialog { Title = "Unexpected errors", Message = sb.ToString() };
            dialog.Closed += (o, args) => Current.Dispatcher.InvokeShutdown();
            dialog.Show();
        }
    }
}