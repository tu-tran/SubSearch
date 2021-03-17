// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for SelectionWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SubSearch.Data;

using WinForms = System.Windows.Forms;

namespace SubSearch.WPF.Views
{
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
        private int maxCommentWidth = WinForms.Screen.PrimaryScreen.WorkingArea.Width / 5;

        /// <summary>The selected item.</summary>
        private ItemData selectedItem;

        /// <summary>The status.</summary>
        private string status;

        /// <summary>The title text.</summary>
        private string titleText;

        private string activeFile;
        private object dummyNode = null;

        /// <summary>Initializes a new instance of the <see cref="MainWindow" /> class.</summary>
        /// <param name="view">The view.</param>
        internal MainWindow(WpfView view = null)
        {
            View = view;
            InitializeComponent();
        }

        /// <summary>Gets the command for CopyItem.</summary>
        public ICommand CopyItem
        {
            get
            {
                return
                    new ActionCommand<object>(
                        o =>
                        {
                            var sel = SelectionBox.SelectedItems.OfType<ItemData>().ToList();
                            var content = string.Join(Environment.NewLine, sel.Count > 0 ? sel.Select(s => s.Name) : new[] { Title });
                            Clipboard.SetDataObject(content);
                        });
            }
        }

        /// <summary>Gets the command for AcceptItem.</summary>
        public ICommand AcceptItem
        {
            get { return new ActionCommand<object>(o => Accept()); }
        }

        /// <summary>Gets the command for Download.</summary>
        public ICommand DownloadCommand => new ActionCommand<object>(Download);

        /// <summary>Gets the command for Download and Exit.</summary>
        public ICommand DownloadExitCommand => new ActionCommand<object>(DownloadExit);

        /// <summary>Gets the command for Download and Play.</summary>
        public ICommand DownloadPlayCommand => new ActionCommand<object>(DownloadPlay);

        /// <summary>Gets the command for Skip.</summary>
        public ICommand SkipCommand => new ActionCommand<object>(Skip);

        /// <summary>Gets the command for Cancel.</summary>
        public ICommand CancelCommand => new ActionCommand<object>(Cancel);

        /// <summary>Gets or sets the max comment width.</summary>
        public int MaxCommentWidth
        {
            get => maxCommentWidth;

            set
            {
                maxCommentWidth = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>Gets the query box enter key command.</summary>
        public ICommand QueryBoxEnterKeyCommand => new ActionCommand<object>(QueryBoxEnterKey);

        /// <summary>Gets the selected item.</summary>
        public ItemData SelectedItem
        {
            get => selectedItem;

            set
            {
                selectedItem = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>Gets the selections.</summary>
        public ObservableCollection<ItemData> Selections => selections;

        /// <summary>Gets a value indicating whether the selection has been made.</summary>
        public Status SelectionState { get; private set; }

        /// <summary>Gets or sets the status.</summary>
        public string Status
        {
            get => status;

            set
            {
                status = value;
                RaisePropertyChanged();
            }
        }

        public string ActiveFile
        {
            get => activeFile;

            set
            {
                if (activeFile != value)
                {
                    activeFile = value;
                    RaisePropertyChanged();
                    this.OnActiveFileChanged();
                }
            }
        }

        public string SelectedParentPath { get; set; }

        /// <summary>Gets or sets the title text.</summary>
        public string TitleText
        {
            get => titleText;

            set
            {
                titleText = value;
                RaisePropertyChanged();
                QueryBox.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        /// <summary>Gets or sets the view.</summary>
        internal WpfView View { get; set; }

        /// <summary>The property changed.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Disposes the Window.</summary>
        public void Dispose()
        {
            disposing = true;
            Close();
        }

        /// <summary>The show.</summary>
        public new void Show()
        {
            if (!IsVisible) base.Show();
        }

        public void SetActiveFile(string filePath)
        {
            this.ActiveFile = filePath;
        }

        public void ResetSelections()
        {
            Dispatcher.Invoke(() =>
            {
                this.selections.Clear();
                this.selectedItem = null;
            });
        }

        /// <summary>The set progress.</summary>
        /// <param name="title">The title.</param>
        /// <param name="newStatus">The status.</param>
        internal void SetProgress(string title, string newStatus)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SetProgress(title, newStatus));
                return;
            }

            ProgressBar.Visibility = Visibility.Visible;
            TitleText = title;
            Status = newStatus;
            Show();
        }

        /// <summary>Sets the progress.</summary>
        /// <param name="done">Done.</param>
        /// <param name="total">Total</param>
        internal void SetProgress(int done, int total)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SetProgress(done, total));
                return;
            }

            ProgressBar.Visibility = Visibility.Visible;
            SelectionBox.Visibility = Visibility.Collapsed;
            AutoSize();
            ProgressBar.Value = done;
            ProgressBar.Maximum = total;
            ProgressBar.IsIndeterminate = done < 1;
            Show();
        }

        /// <summary>The set selections.</summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        internal void SetSelections(ICollection<ItemData> data, string title, string status,
            CancellationTokenSource token)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SetSelections(data, title, status, token));
                return;
            }

            hideCancellationToken = token;
            selections.Clear();
            foreach (var itemData in data) selections.Add(itemData);

            TitleText = title;
            Status = status;
            SelectionBox.Visibility = Visibility.Visible;
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
            lastPosition = null;
            AutoSize();
            if (!IsVisible)
            {
                AutoPosition();
                Show();
            }
        }

        /// <summary>The list box item mouse double click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The eventArgs.</param>
        protected void ListBoxItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Accept(Data.Status.Success, false);
        }

        /// <summary>The raise property changed.</summary>
        /// <param name="propertyName">The property name.</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>The accept.</summary>
        internal void Accept(Status result = Data.Status.Success, bool hide = true)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => Accept(result, hide));
                return;
            }

            SelectionState = result;
            if (hide)
                Hide();
            else
                OnNotifyToken();
        }

        /// <summary>Auto position.</summary>
        private void AutoPosition()
        {
            if (WindowState != WindowState.Normal) return;

            SizeToContent = SizeToContent.WidthAndHeight;
            SizeToContent = SizeToContent.Manual;
            if (lastPosition == null)
            {
                var mousePosition = WinForms.Control.MousePosition;
                var activeScreenArea = WinForms.Screen.FromPoint(mousePosition).WorkingArea;
                if (ActualWidth > activeScreenArea.Width) Width = activeScreenArea.Width;

                if (ActualHeight > activeScreenArea.Height) Height = activeScreenArea.Height;

                var left = mousePosition.X - ActualWidth / 2;
                var top = mousePosition.Y - ActualHeight / 2;

                if (left < SystemParameters.VirtualScreenLeft)
                    left = SystemParameters.VirtualScreenLeft;
                else if (left + ActualWidth > SystemParameters.VirtualScreenWidth)
                    left = SystemParameters.VirtualScreenWidth - ActualWidth;

                if (top < SystemParameters.VirtualScreenTop)
                    top = SystemParameters.VirtualScreenTop;
                else if (top + ActualHeight > SystemParameters.VirtualScreenHeight)
                    top = SystemParameters.VirtualScreenHeight - ActualHeight;

                Left = left;
                Top = top;
            }
            else
            {
                Left = lastPosition.Item1;
                Top = lastPosition.Item2;
            }
        }

        /// <summary>Auto size.</summary>
        private void AutoSize()
        {
            if (WindowState == WindowState.Maximized) return;

            SizeToContent = SizeToContent.WidthAndHeight;
            SizeToContent = SizeToContent.Manual;
            var activeScreenArea = WinForms.Screen.FromPoint(new System.Drawing.Point((int)(Left + Width / 2), (int)(Top + Height / 2))).WorkingArea;
            if (Left + ActualWidth > activeScreenArea.Right - 100)
            {
                var newWidth = activeScreenArea.Right - Left - 100;
                if (newWidth > 0) Width = newWidth;
            }

            if (Top + ActualHeight > activeScreenArea.Bottom - 100)
            {
                var newHeight = activeScreenArea.Bottom - Top - 100;
                if (newHeight > 0) Height = newHeight;
            }
        }

        /// <summary>The download.</summary>
        /// <param name="parameter">The parameter.</param>
        private void Download(object parameter)
        {
            RaiseCustomAction(parameter, CustomActions.DownloadSubtitle);
        }

        /// <summary>The download exit.</summary>
        /// <param name="parameter">The parameter.</param>
        private void DownloadExit(object parameter)
        {
            RaiseCustomAction(parameter, CustomActions.DownloadSubtitle);
            OnNotifyToken();
        }

        /// <summary>The download play.</summary>
        /// <param name="parameter">The parameter.</param>
        private void DownloadPlay(object parameter)
        {
            RaiseCustomAction(parameter, CustomActions.DownloadSubtitle, CustomActions.Play);
        }

        /// <summary>The skip.</summary>
        /// <param name="parameter">The parameter.</param>
        private void Skip(object parameter)
        {
            Accept(Data.Status.Skipped, false);
        }

        /// <summary>The cancel.</summary>
        /// <param name="parameter">The parameter.</param>
        private void Cancel(object parameter)
        {
            Accept(Data.Status.Cancelled);
        }

        /// <summary>When the Window is closing.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!disposing) SelectionState = Data.Status.Cancelled;
        }

        /// <summary>The main window_ on content rendered.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The eventArgs.</param>
        private void MainWindow_OnContentRendered(object sender, EventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
        }

        /// <summary>When the visibility is changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void MainWindow_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.Equals(false))
            {
                lastPosition = new Tuple<double, double>(Left, Top);
                OnNotifyToken();
            }
        }

        private void OnNotifyToken()
        {
            if (hideCancellationToken != null)
            {
                hideCancellationToken.Cancel();
                hideCancellationToken = null;
            }
        }

        /// <summary>The main window_ on loaded.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The eventArgs.</param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            AutoPosition();
        }

        private void InitDirectoryTree()
        {
            ParentPathTreeView.Items.Clear();
            var root = Directory.GetLogicalDrives().ToList();
            root.Add(Directory.GetParent(this.activeFile).FullName);

            foreach (string s in root)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = s;
                item.Tag = s;
                item.FontWeight = FontWeights.Normal;
                item.Items.Add(dummyNode);
                item.Expanded += new RoutedEventHandler(folder_Expanded);
                ParentPathTreeView.Items.Add(item);
            }

            var tvi = ParentPathTreeView.ItemContainerGenerator.ContainerFromItem(ParentPathTreeView.Items[ParentPathTreeView.Items.Count - 1]) as TreeViewItem;
            if (tvi != null)
            {
                tvi.IsSelected = true;
            }
        }

        /// <summary>The main window_ on preview key up.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The eventArgs.</param>
        private void MainWindow_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }

        /// <summary>The query box enter key.</summary>
        /// <param name="parameter">The parameter.</param>
        private void QueryBoxEnterKey(object parameter)
        {
            RaiseCustomAction(parameter, CustomActions.CustomQuery);
        }

        /// <summary>Queries the box got keyboard focus.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyboardFocusChangedEventArgs" /> instance containing the event data.</param>
        private void QueryBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null && tb.IsKeyboardFocusWithin && e.OriginalSource == sender) ((TextBox)sender).SelectAll();
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
            if (View != null) View.OnCustomAction(parameter, actionNames);
        }

        private void OnActiveFileChanged()
        {
            this.Dispatcher.Invoke(this.InitDirectoryTree);
        }

        private void ParentPathTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView tree = (TreeView)sender;
            TreeViewItem temp = ((TreeViewItem)tree.SelectedItem);

            if (temp == null)
                return;
            SelectedParentPath = "";
            string temp1 = "";
            string temp2 = "";
            while (true)
            {
                temp1 = temp.Header.ToString();
                if (temp1.Contains(@"\"))
                {
                    temp2 = "";
                }
                SelectedParentPath = temp1 + temp2 + SelectedParentPath;
                if (temp.Parent.GetType().Equals(typeof(TreeView)))
                {
                    break;
                }
                temp = ((TreeViewItem)temp.Parent);
                temp2 = @"\";
            }

            var files = ShellExtension.GetSupportedFiles(this.SelectedParentPath);
            this.FileListView.Items.Clear();
            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                var item = new ListViewItem { Content = name, Tag = file };
                item.MouseDoubleClick += FileListView_ItemOnMouseDoubleClick;
                if (file.Equals(this.activeFile, StringComparison.OrdinalIgnoreCase))
                {
                    item.IsSelected = true;
                }

                this.FileListView.Items.Add(item);
            }
        }

        private void FileListView_ItemOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.activeFile = ((ListViewItem)sender).Tag?.ToString();
            this.RaiseCustomAction(this.activeFile, CustomActions.ChangeActiveFile);
        }

        void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    foreach (string s in Directory.GetDirectories(item.Tag.ToString()))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                        subitem.Tag = s;
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception) { }
            }
        }
    }
}