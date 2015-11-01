namespace SubSearch.WPF.View
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Threading;

    /// <summary>Interaction logic for NotificationWindow.xaml</summary>
    public partial class NotificationWindow : INotifyPropertyChanged
    {
        /// <summary>The window.</summary>
        private static readonly NotificationWindow Window = new NotificationWindow();

        /// <summary>The end event handler.</summary>
        private static DependencyPropertyChangedEventHandler endEventHandler;

        /// <summary>The message.</summary>
        private string message;

        /// <summary>Initializes a new instance of the <see cref="NotificationWindow" /> class.</summary>
        public NotificationWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>The property changed.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Gets or sets the message.</summary>
        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                this.message = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("Message"));
                }
            }
        }

        /// <summary>
        /// The show.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="endHandler">
        /// The end handler.
        /// </param>
        public static void Show(string message, DependencyPropertyChangedEventHandler endHandler = null)
        {
            Window.Hide();
            Window.Message = message;
            endEventHandler = endHandler;
            Window.Show();
        }

        /// <summary>
        /// Occurs when the window is loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void NotificationWindow_OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            this.Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                new Action(
                    () =>
                    {
                        var mousePosition = Control.MousePosition;
                        var screen = Screen.FromPoint(mousePosition);
                        var workingArea = screen.WorkingArea;
                        var presentationSource = PresentationSource.FromVisual(this);
                        if (presentationSource != null && presentationSource.CompositionTarget != null)
                        {
                            var transform = presentationSource.CompositionTarget.TransformFromDevice;
                            var corner = transform.Transform(new Point(workingArea.Right, workingArea.Bottom));

                            this.Left = corner.X - this.ActualWidth;
                            this.Top = corner.Y - this.ActualHeight;
                        }
                    }));
        }

        /// <summary>
        /// The notification window_ on preview mouse up.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The eventArgs.
        /// </param>
        private void NotificationWindow_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
        }

        /// <summary>
        /// The grid_ on is visible changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The eventArgs.
        /// </param>
        private void Grid_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (endEventHandler != null)
            {
                endEventHandler(sender, e);
            }
        }
    }
}