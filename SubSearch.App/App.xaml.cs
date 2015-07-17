namespace SubSearch.WPF
{
    using System.Linq;
    using System.Windows;

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
            string message = null;
            if (new QueueHandler(e.Args.FirstOrDefault()).Process())
            {
                message = "Subtitles were downloaded successfully!";
            }
            else
            {
                message = "Could not process request. Please reinstall the application or try again from the Windows Shell's context menu!";
            }

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
    }
}