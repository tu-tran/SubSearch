// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for SelectionWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SubSearch.WPF.Views
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Input;

    using SubSearch.Data;

    using Clipboard = System.Windows.Clipboard;
    using KeyEventArgs = System.Windows.Input.KeyEventArgs;
    using TextBox = System.Windows.Controls.TextBox;

    /// <summary>Interaction logic for SelectionWindow.xaml</summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        /// <summary>The selections.</summary>
        private readonly ObservableCollection<ItemData> selections = new ObservableCollection<ItemData>();

        /// <summary>Is disposing.</summary>
        private bool disposing;

        /// <summary>The cancellation token source for windows hidden event.</summary>
        private CancellationTokenSource hideCancellationToken;

        /// <summary>The last position.</summary>
        private Tuple<double, double> lastPosition;

        /// <summary>The max comment width.</summary>
        private int maxCommentWidth = Screen.PrimaryScreen.WorkingArea.Width / 5;

        /// <summary>The selected item.</summary>
        private ItemData selectedItem;

        /// <summary>The status.</summary>
        private string status;

        /// <summary>The title text.</summary>
        private string titleText;

        /// <summary>Initializes a new instance of the <see cref="MainWindow" /> class.</summary>
        /// <param name="view">The view.</param>
        internal MainWindow(WpfView view = null)
        {
            this.View = view;
            this.InitializeComponent();
        }

        /// <summary>Gets the command for CopyItem.</summary>
        public ICommand CopyItem
        {
            get
            {
                return
                    new ActionCommand<object>(
                        o =>
                        Clipboard.SetText(
                            this.SelectedItem == null || string.IsNullOrEmpty(this.SelectedItem.Name) ? this.Title : this.SelectedItem.Name));
            }
        }

        /// <summary>Gets the command for AcceptItem.</summary>
        public ICommand AcceptItem
        {
            get
            {
                return new ActionCommand<object>(o => this.Accept());
            }
        }

        /// <summary>Gets the command for Download.</summary>
        public ICommand DownloadCommand
        {
            get
            {
                return new ActionCommand<object>(this.Download);
            }
        }

        /// <summary>Gets the command for Download and Exit.</summary>
        public ICommand DownloadExitCommand
        {
            get
            {
                return new ActionCommand<object>(this.DownloadExit);
            }
        }

        /// <summary>Gets the command for Download and Play.</summary>
        public ICommand DownloadPlayCommand
        {
            get
            {
                return new ActionCommand<object>(this.DownloadPlay);
            }
        }

        /// <summary>Gets the command for Skip.</summary>
        public ICommand SkipCommand
        {
            get
            {
                return new ActionCommand<object>(this.Skip);
            }
        }

        /// <summary>Gets the command for Cancel.</summary>
        public ICommand CancelCommand
        {
            get
            {
                return new ActionCommand<object>(this.Cancel);
            }
        }

        /// <summary>Gets or sets the max comment width.</summary>
        public int MaxCommentWidth
        {
            get
            {
                return this.maxCommentWidth;
            }

            set
            {
                this.maxCommentWidth = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>Gets the query box enter key command.</summary>
        public ICommand QueryBoxEnterKeyCommand
        {
            get
            {
                return new ActionCommand<object>(this.QueryBoxEnterKey);
            }
        }

        /// <summary>Gets the selected item.</summary>
        public ItemData SelectedItem
        {
            get
            {
                return this.selectedItem;
            }

            set
            {
                this.selectedItem = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>Gets the selections.</summary>
        public ObservableCollection<ItemData> Selections
        {
            get
            {
                return this.selections;
            }
        }

        /// <summary>Gets a value indicating whether the selection has been made.</summary>
        public Status SelectionState { get; private set; }

        /// <summary>Gets or sets the status.</summary>
        public string Status
        {
            get
            {
                return this.status;
            }

            set
            {
                this.status = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>Gets or sets the title text.</summary>
        public string TitleText
        {
            get
            {
                return this.titleText;
            }

            set
            {
                this.titleText = value;
                this.RaisePropertyChanged();
                this.QueryBox.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        /// <summary>Gets or sets the view.</summary>
        internal WpfView View { get; set; }

        /// <summary>The property changed.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Disposes the Window.</summary>
        public void Dispose()
        {
            this.disposing = true;
            this.Close();
        }

        /// <summary>The show.</summary>
        public new void Show()
        {
            if (!this.IsVisible)
            {
                base.Show();
            }
        }

        /// <summary>The set progress.</summary>
        /// <param name="title">The title.</param>
        /// <param name="newStatus">The status.</param>
        internal void SetProgress(string title, string newStatus)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.SetProgress(title, newStatus));
                return;
            }

            this.ProgressBar.Visibility = Visibility.Visible;
            this.TitleText = title;
            this.Status = newStatus;
            this.Show();
        }

        /// <summary>Sets the progress.</summary>
        /// <param name="done">Done.</param>
        /// <param name="total">Total</param>
        internal void SetProgress(int done, int total)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.SetProgress(done, total));
                return;
            }

            this.ProgressBar.Visibility = Visibility.Visible;
            this.SelectionBox.Visibility = Visibility.Collapsed;
            this.AutoSize();
            this.ProgressBar.Value = done;
            this.ProgressBar.Maximum = total;
            this.ProgressBar.IsIndeterminate = done < 1;
            this.Show();
        }

        /// <summary>The set selections.</summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        internal void SetSelections(ICollection<ItemData> data, string title, string status, CancellationTokenSource token)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.SetSelections(data, title, status, token));
                return;
            }

            this.hideCancellationToken = token;
            this.selections.Clear();
            foreach (var itemData in data)
            {
                this.selections.Add(itemData);
            }

            this.TitleText = title;
            this.Status = status;
            this.SelectionBox.Visibility = Visibility.Visible;
            this.ProgressBar.Visibility = Visibility.Collapsed;
            this.ProgressBar.IsIndeterminate = false;
            this.lastPosition = null;
            this.AutoSize();
            if (!this.IsVisible)
            {
                this.AutoPosition();
                this.Show();
            }
        }

        /// <summary>The list box item mouse double click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The eventArgs.</param>
        protected void ListBoxItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Accept(Data.Status.Success, false);
        }

        /// <summary>The raise property changed.</summary>
        /// <param name="propertyName">The property name.</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>The accept.</summary>
        internal void Accept(Status result = Data.Status.Success, bool hide = true)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.Accept(result, hide));
                return;
            }

            this.SelectionState = result;
            if (hide)
            {
                this.Hide();
            }
            else
            {
                this.OnNotifyToken();
            }
        }

        /// <summary>Auto position.</summary>
        private void AutoPosition()
        {
            if (this.WindowState != WindowState.Normal)
            {
                return;
            }

            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.SizeToContent = SizeToContent.Manual;
            if (this.lastPosition == null)
            {
                var mousePosition = Control.MousePosition;
                var activeScreenArea = Screen.FromPoint(mousePosition).WorkingArea;
                if (this.ActualWidth > activeScreenArea.Width)
                {
                    this.Width = activeScreenArea.Width;
                }

                if (this.ActualHeight > activeScreenArea.Height)
                {
                    this.Height = activeScreenArea.Height;
                }

                var left = mousePosition.X - this.ActualWidth / 2;
                var top = mousePosition.Y - this.ActualHeight / 2;

                if (left < SystemParameters.VirtualScreenLeft)
                {
                    left = SystemParameters.VirtualScreenLeft;
                }
                else if (left + this.ActualWidth > SystemParameters.VirtualScreenWidth)
                {
                    left = SystemParameters.VirtualScreenWidth - this.ActualWidth;
                }

                if (top < SystemParameters.VirtualScreenTop)
                {
                    top = SystemParameters.VirtualScreenTop;
                }
                else if (top + this.ActualHeight > SystemParameters.VirtualScreenHeight)
                {
                    top = SystemParameters.VirtualScreenHeight - this.ActualHeight;
                }

                this.Left = left;
                this.Top = top;
            }
            else
            {
                this.Left = this.lastPosition.Item1;
                this.Top = this.lastPosition.Item2;
            }
        }

        /// <summary>Auto size.</summary>
        private void AutoSize()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                return;
            }

            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.SizeToContent = SizeToContent.Manual;
            var mousePosition = Control.MousePosition;
            var activeScreenArea = Screen.FromPoint(mousePosition).WorkingArea;
            if (this.Left + this.ActualWidth > activeScreenArea.Right)
            {
                var newWidth = activeScreenArea.Right - this.Left;
                if (newWidth > 0)
                {
                    this.Width = newWidth;
                }
            }

            if (this.Top + this.ActualHeight > activeScreenArea.Bottom)
            {
                var newHeight = activeScreenArea.Bottom - this.Top;
                if (newHeight > 0)
                {
                    this.Height = newHeight;
                }
            }
        }

        /// <summary>The download.</summary>
        /// <param name="parameter">The parameter.</param>
        private void Download(object parameter)
        {
            this.RaiseCustomAction(parameter, CustomActions.DownloadSubtitle);
        }

        /// <summary>The download exit.</summary>
        /// <param name="parameter">The parameter.</param>
        private void DownloadExit(object parameter)
        {
            this.RaiseCustomAction(parameter, CustomActions.DownloadSubtitle);
            this.OnNotifyToken();
        }

        /// <summary>The download play.</summary>
        /// <param name="parameter">The parameter.</param>
        private void DownloadPlay(object parameter)
        {
            this.RaiseCustomAction(parameter, CustomActions.DownloadSubtitle, CustomActions.Play);
        }

        /// <summary>The skip.</summary>
        /// <param name="parameter">The parameter.</param>
        private void Skip(object parameter)
        {
            this.Accept(Data.Status.Skipped, false);
        }

        /// <summary>The cancel.</summary>
        /// <param name="parameter">The parameter.</param>
        private void Cancel(object parameter)
        {
            this.Accept(Data.Status.Cancelled);
        }

        /// <summary>When the Window is closing.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!this.disposing)
            {
                this.SelectionState = Data.Status.Cancelled;
            }
        }

        /// <summary>The main window_ on content rendered.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The eventArgs.</param>
        private void MainWindow_OnContentRendered(object sender, EventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
        }

        /// <summary>When the visibility is changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void MainWindow_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.Equals(false))
            {
                this.lastPosition = new Tuple<double, double>(this.Left, this.Top);
                this.OnNotifyToken();
            }
        }

        private void OnNotifyToken()
        {
            if (this.hideCancellationToken != null)
            {
                this.hideCancellationToken.Cancel();
                this.hideCancellationToken = null;
            }
        }

        /// <summary>The main window_ on loaded.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The eventArgs.</param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.AutoPosition();
        }

        /// <summary>The main window_ on preview key up.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The eventArgs.</param>
        private void MainWindow_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        /// <summary>The query box enter key.</summary>
        /// <param name="parameter">The parameter.</param>
        private void QueryBoxEnterKey(object parameter)
        {
            this.RaiseCustomAction(parameter, CustomActions.CustomQuery);
        }

        /// <summary>Queries the box got keyboard focus.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyboardFocusChangedEventArgs" /> instance containing the event data.</param>
        private void QueryBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null && tb.IsKeyboardFocusWithin && e.OriginalSource == sender)
            {
                ((TextBox)sender).SelectAll();
            }
        }

        /// <summary>Selectivelies the ignore mouse button.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null && !tb.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                tb.Focus();
            }
        }

        /// <summary>The raise custom action.</summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="actionNames">The action names.</param>
        private void RaiseCustomAction(object parameter, params string[] actionNames)
        {
            if (this.View != null)
            {
                this.View.OnCustomAction(parameter, actionNames);
            }
        }
    }
}