// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for App.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SubSearch.WPF
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;

    using SubSearch.Data;
    using SubSearch.Resources;
    using SubSearch.WPF.Views;

    /// <summary>Interaction logic for App.xaml</summary>
    public partial class App
    {
        /// <summary>The on startup.</summary>
        /// <param name="e">The e.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.InitializeErrorHandler();
            NotificationView.Initialize();
            var result = new QueueHandler(e.Args).Process();

            string message = null;
            if (result == QueryResult.Success)
            {
                message = Literals.ShellExtension_Subtitles_downloaded_successfully;
            }
            else if (result == QueryResult.Fatal)
            {
                message = Literals.ShellExtension_Failed_process_request;
            }

            if (string.IsNullOrEmpty(message))
            {
                NotificationView.AttachEndHandler((sender, args) => this.Dispatcher.InvokeShutdown());
            }
            else
            {
                NotificationView.Show(message, (sender, args) => this.Dispatcher.InvokeShutdown());
            }
        }

        /// <summary>Raises when there is unhandled exception.</summary>
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

        /// <summary>Raises when there is unhandled exception.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private static void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs eventArgs)
        {
            CurrentDomainOnUnhandledException(sender, new UnhandledExceptionEventArgs(eventArgs.Exception, false));
            eventArgs.Handled = true;
        }

        /// <summary>Raises when there is unhandled exception.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs eventArgs)
        {
            CurrentDomainOnUnhandledException(sender, new UnhandledExceptionEventArgs(eventArgs.Exception, false));
        }

        /// <summary>Initializes the error handler.</summary>
        private void InitializeErrorHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            this.Dispatcher.UnhandledException += CurrentOnDispatcherUnhandledException;
            this.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }
    }
}