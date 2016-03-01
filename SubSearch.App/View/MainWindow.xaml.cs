// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for SelectionWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SubSearch.WPF.View
{
    using SubSearch.Data;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Input;
    using KeyEventArgs = System.Windows.Input.KeyEventArgs;

    /// <summary>Interaction logic for SelectionWindow.xaml</summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        /// <summary>The active window.</summary>
        private static readonly MainWindow activeWindow;

        /// <summary>The last position.</summary>
        private static Tuple<double, double> lastPosition;

        /// <summary>The selections.</summary>
        private readonly ObservableCollection<ItemData> selections = new ObservableCollection<ItemData>();

        /// <summary>Is disposing.</summary>
        private bool disposing;

        /// <summary>The max comment width.</summary>
        private int maxCommentWidth = Screen.PrimaryScreen.WorkingArea.Width / 5;

        /// <summary>The status.</summary>
        private string status;

        /// <summary>The title text.</summary>
        private string titleText;

        /// <summary>Initializes static members of the <see cref="MainWindow" /> class. Static initialization.</summary>
        static MainWindow()
        {
            activeWindow = new MainWindow();
        }

        /// <summary>Prevents a default instance of the <see cref="MainWindow" /> class from being created. Initializes a new instance of the
        /// <see cref="MainWindow" /> class.</summary>
        private MainWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>The property changed.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Gets or sets the target file.</summary>
        public static string TargetFile { get; set; }

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
                return this.SelectionBox.SelectedItem as ItemData;
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
        public QueryResult SelectionState { get; private set; }

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

        /// <summary>Gets or sets the view handler.</summary>
        internal WpfViewHandler ViewHandler { get; set; }

        /// <summary>The close all.</summary>
        public static void CloseAll()
        {
            if (activeWindow != null)
            {
                activeWindow.Dispatcher.Invoke(() => activeWindow.Dispose());
            }
        }

        /// <summary>Continues the pending operation and cancel any selection.</summary>
        public static void Continue()
        {
            activeWindow.Accept(QueryResult.Skipped);
        }

        /// <summary>The get selection.</summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        /// <returns>The <see cref="ItemData"/>.</returns>
        public static Tuple<QueryResult, ItemData> GetSelection(ICollection<ItemData> data, string title, string status)
        {
            activeWindow.Dispatcher.Invoke(
                () =>
                {
                    activeWindow.SetSelections(data, title, status);
                    if (!activeWindow.IsVisible)
                    {
                        activeWindow.Show();
                    }
                });

            while (activeWindow.Dispatcher.Invoke(() => activeWindow.ShowInTaskbar))
            {
                Thread.Sleep(500);
            }

            return activeWindow.Dispatcher.Invoke(() => Tuple.Create(activeWindow.SelectionState, activeWindow.SelectedItem));
        }

        /// <summary>The show progress.</summary>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        public static void ShowProgress(string title, string status)
        {
            activeWindow.SetProgress(title, status);
        }

        /// <summary>Sets the progress.</summary>
        /// <param name="done">Done.</param>
        /// <param name="total">Total</param>
        public static void ShowProgress(int done, int total)
        {
            activeWindow.SetProgress(done, total);
        }

        /// <summary>Starts the view and wait for interaction.</summary>
        public static void Start()
        {
            activeWindow.Dispatcher.Invoke(() => { activeWindow.ShowDialog(); });
        }

        /// <summary>Disposes the window.</summary>
        public void Dispose()
        {
            this.disposing = true;
            this.Close();
        }

        /// <summary>Hides the window.</summary>
        public new void Hide()
        {
            if (this.IsVisible)
            {
                this.WindowState = WindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }

        /// <summary>The show.</summary>
        public new void Show()
        {
            if (!this.IsVisible)
            {
                base.Show();
            }

            this.WindowState = WindowState.Normal;
            this.ShowInTaskbar = true;
            this.SelectionState = QueryResult.Skipped;
        }

        /// <summary>The attach.</summary>
        /// <param name="viewHandler">The view handler.</param>
        internal static void Attach(WpfViewHandler viewHandler)
        {
            activeWindow.ViewHandler = viewHandler;
        }

        /// <summary>The detach.</summary>
        /// <param name="viewHandler">The view handler.</param>
        internal static void Detach(WpfViewHandler viewHandler)
        {
            if (activeWindow.ViewHandler == viewHandler)
            {
                activeWindow.ViewHandler = null;
            }
        }

        /// <summary>The list box item mouse double click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void ListBoxItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Accept();
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
        private void Accept(QueryResult result = QueryResult.Success)
        {
            this.SelectionState = result;
            this.Hide();
        }

        /// <summary>Auto position.</summary>
        private void AutoPosition()
        {
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.SizeToContent = SizeToContent.Manual;
            if (lastPosition == null)
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

                if (left < 0)
                {
                    left = 0;
                }
                else if (left + this.ActualWidth > activeScreenArea.Right)
                {
                    left = activeScreenArea.Right - this.ActualWidth;
                }

                if (top < 0)
                {
                    top = 0;
                }
                else if (top + this.ActualHeight > activeScreenArea.Bottom)
                {
                    top = activeScreenArea.Bottom - this.ActualHeight;
                }

                this.Left = left;
                this.Top = top;
            }
            else
            {
                this.Left = lastPosition.Item1;
                this.Top = lastPosition.Item2;
            }
        }

        /// <summary>Auto size.</summary>
        private void AutoSize()
        {
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
            this.RaiseCustomAction(parameter, CustomActions.DownloadSubtitle, CustomActions.Close);
        }

        /// <summary>The download play.</summary>
        /// <param name="parameter">The parameter.</param>
        private void DownloadPlay(object parameter)
        {
            this.RaiseCustomAction(parameter, CustomActions.DownloadSubtitle, CustomActions.Play);
        }

        /// <summary>The list box item preview key up.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ListBoxItemPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Accept();
            }
        }

        /// <summary>When the window is closing.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!this.disposing)
            {
                this.SelectionState = QueryResult.Cancelled;
                e.Cancel = true;
                this.Hide();
            }
        }

        /// <summary>The main window_ on content rendered.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
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
                lastPosition = new Tuple<double, double>(this.Left, this.Top);
            }
        }

        /// <summary>The main window_ on loaded.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.AutoPosition();
        }

        /// <summary>The main window_ on preview key up.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
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

        /// <summary>The raise custom action.</summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="actionNames">The action names.</param>
        private void RaiseCustomAction(object parameter, params string[] actionNames)
        {
            if (this.ViewHandler != null)
            {
                this.ViewHandler.OnCustomAction(parameter, actionNames);
            }
        }

        /// <summary>The set progress.</summary>
        /// <param name="title">The title.</param>
        /// <param name="newStatus">The status.</param>
        private void SetProgress(string title, string newStatus)
        {
            this.Dispatcher.Invoke(
                () =>
                {
                    this.ProgressBar.Visibility = Visibility.Visible;
                    this.TitleText = title;
                    this.Status = newStatus;
                    this.Show();
                });
        }

        /// <summary>Sets the progress.</summary>
        /// <param name="done">Done.</param>
        /// <param name="total">Total</param>
        private void SetProgress(int done, int total)
        {
            this.Dispatcher.Invoke(
                () =>
                {
                    this.ProgressBar.Visibility = Visibility.Visible;
                    this.ProgressBar.Value = done;
                    this.ProgressBar.Maximum = total;
                    this.ProgressBar.IsIndeterminate = done < 1;
                    this.Show();
                });
        }

        /// <summary>The set selections.</summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        private void SetSelections(ICollection<ItemData> data, string title, string status)
        {
            this.Dispatcher.Invoke(
                () =>
                {
                    this.selections.Clear();
                    foreach (var itemData in data)
                    {
                        this.selections.Add(itemData);
                    }

                    var remainderHeight = this.ActualHeight - this.SelectionBox.ActualHeight;
                    var maxHeight = Utils.GetActiveScreen().WorkingArea.Height - remainderHeight;
                    var newHeight = (this.SelectionBox.FontSize + 5) * data.Count;

                    if (newHeight > maxHeight)
                    {
                        newHeight = maxHeight;
                    }

                    if (newHeight > 0)
                    {
                        this.Height = newHeight + remainderHeight;
                    }

                    this.TitleText = title;
                    this.Status = status;
                    this.SelectionBox.Visibility = Visibility.Visible;
                    this.ProgressBar.Visibility = Visibility.Collapsed;
                    lastPosition = null;
                    this.AutoSize();
                });
        }
    }
}