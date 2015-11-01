using System;
using System.Text;

namespace SubSearch.WPF
{
    using SubSearch.WPF.View;
    using System.Windows;

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
            this.InitializeErrorHandler();
            var result = new QueueHandler(e.Args).Process();
            if (result == 0)
            {
                return;
            }

            string message = result == 1
                ? "Subtitles downloads were completed successfully!"
                : "Could not process request. Please reinstall the application or try again from the Windows Shell's context menu!";

            NotificationWindow.Show(
                message,
                (sender, args) =>
                {
                    if (args.NewValue.Equals(false))
                    {
                        this.Shutdown();
                    }
                });
        }

        /// <summary>
        /// Initializes the error handler.
        /// </summary>
        private void InitializeErrorHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
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

            new MessageDialog { Title = "Unexpected errors", Message = sb.ToString() }.Show();
        }
    }
}