namespace SubSearch.WPF.Styles.Default
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Interop;

    /// <summary>The local extensions.</summary>
    internal static class LocalExtensions
    {
        /// <summary>The for window from template.</summary>
        /// <param name="templateFrameworkElement">The template framework element.</param>
        /// <param name="action">The action.</param>
        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<Window> action)
        {
            var window = ((FrameworkElement)templateFrameworkElement).TemplatedParent as Window;
            if (window != null)
            {
                action(window);
            }
        }

        /// <summary>The get window handle.</summary>
        /// <param name="window">The window.</param>
        /// <returns>The <see cref="IntPtr"/>.</returns>
        public static IntPtr GetWindowHandle(this Window window)
        {
            var helper = new WindowInteropHelper(window);
            return helper.Handle;
        }
    }

    /// <summary>The default window style.</summary>
    public partial class DefaultWindowStyle
    {
        /// <summary>The close button click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => SystemCommands.CloseWindow(w));
        }

        /// <summary>The icon mouse left button down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void IconMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                sender.ForWindowFromTemplate(w => SystemCommands.CloseWindow(w));
            }
        }

        /// <summary>The icon mouse up.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void IconMouseUp(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            var point = element.PointToScreen(new Point(element.ActualWidth / 2, element.ActualHeight));
            sender.ForWindowFromTemplate(w => SystemCommands.ShowSystemMenu(w, point));
        }

        /// <summary>The max button click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MaxButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(
                w =>
                    {
                        if (w.WindowState == WindowState.Maximized)
                        {
                            SystemCommands.RestoreWindow(w);
                        }
                        else
                        {
                            SystemCommands.MaximizeWindow(w);
                        }
                    });
        }

        /// <summary>The min button click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MinButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => SystemCommands.MinimizeWindow(w));
        }

        /// <summary>The window loaded.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            ((Window)sender).StateChanged += this.WindowStateChanged;
        }

        /// <summary>The window state changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void WindowStateChanged(object sender, EventArgs e)
        {
            var w = (Window)sender;
            var handle = w.GetWindowHandle();
            var containerBorder = (Border)w.Template.FindName("PART_Container", w);

            if (w.WindowState == WindowState.Maximized)
            {
                // Make sure window doesn't overlap with the taskbar.
                var screen = Screen.FromHandle(handle);
                if (screen.Primary)
                {
                    containerBorder.Padding = new Thickness(
                        SystemParameters.WorkArea.Left + 7, 
                        SystemParameters.WorkArea.Top + 7, 
                        SystemParameters.PrimaryScreenWidth - SystemParameters.WorkArea.Right + 7, 
                        SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Bottom + 5);
                }
            }
            else
            {
                containerBorder.Padding = new Thickness(7, 7, 7, 5);
            }
        }
    }
}