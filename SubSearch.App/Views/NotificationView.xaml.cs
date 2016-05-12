// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationWindow.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for NotificationWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SubSearch.WPF.View
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;

    /// <summary>Interaction logic for NotificationWindow.xaml</summary>
    public partial class NotificationView : INotifyPropertyChanged
    {
        /// <summary>The end event handler.</summary>
        internal static DependencyPropertyChangedEventHandler endEventHandler;

        /// <summary>The window.</summary>
        private static NotificationView view;

        /// <summary>The message.</summary>
        private string message;

        /// <summary>Initializes a new instance of the <see cref="NotificationView" /> class.</summary>
        public NotificationView()
        {
            this.InitializeComponent();
        }

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

        /// <summary>The property changed.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>The initialize.</summary>
        /// <exception cref="NotImplementedException"></exception>
        public static void Initialize()
        {
            view = new NotificationView();
        }

        /// <summary>The show.</summary>
        /// <param name="message">The message.</param>
        /// <param name="endHandler">The end handler.</param>
        public static void Show(string message, DependencyPropertyChangedEventHandler endHandler = null)
        {
            view.Dispatcher.Invoke(
                () =>
                    {
                        view.Hide();
                        view.Message = message;
                        endEventHandler = endHandler;
                        view.Show();
                    });
        }

        /// <summary>Attaches the end handler.</summary>
        /// <param name="endHandler">The end handler.</param>
        public static void AttachEndHandler(DependencyPropertyChangedEventHandler endHandler)
        {
            endEventHandler += endHandler;
            if (view != null && !view.IsVisible)
            {
                endEventHandler(null, new DependencyPropertyChangedEventArgs());
                endEventHandler = null;
            }
        }

        /// <summary>The grid_ on is visible changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The eventArgs.</param>
        private void Grid_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Equals(e.NewValue, false))
            {
                if (endEventHandler != null)
                {
                    endEventHandler(sender, e);
                }
            }
        }

        /// <summary>Occurs when the window is loaded.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void NotificationWindow_OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            this.Dispatcher.Invoke(
                DispatcherPriority.ApplicationIdle,
                new Action(
                    () =>
                        {
                            var screen = Utils.GetActiveScreen();
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

        /// <summary>The notification window_ on preview mouse up.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The eventArgs.</param>
        private void NotificationWindow_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Hide();
        }
    }
}