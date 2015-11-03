﻿namespace SubSearch.WPF.View
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Input;

    using SubSearch.Data;

    using Control = System.Windows.Forms.Control;
    using KeyEventArgs = System.Windows.Input.KeyEventArgs;

    /// <summary>Interaction logic for SelectionWindow.xaml</summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        /// <summary>The active window.</summary>
        private static MainWindow activeWindow;

        private static Tuple<double, double> lastPosition;

        /// <summary>The selections.</summary>
        private readonly ObservableCollection<ItemData> selections = new ObservableCollection<ItemData>();

        /// <summary>The status.</summary>
        private string status;

        /// <summary>The title text.</summary>
        private string titleText;

        /// <summary>
        /// Is disposing.
        /// </summary>
        private bool disposing;

        /// <summary>
        /// Static initialization.
        /// </summary>
        static MainWindow()
        {
            activeWindow = new MainWindow();
        }

        /// <summary>Initializes a new instance of the <see cref="MainWindow" /> class.</summary>
        private MainWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>The property changed.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Gets the selected item.</summary>
        public ItemData SelectedItem
        {
            get
            {
                return this.SelectionBox.SelectedItem as ItemData;
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
                this.RaisePropertyChanged("TitleText");
                this.TitleBlock.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

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
                this.RaisePropertyChanged("Status");
            }
        }

        /// <summary>
        /// Gets the selection state.
        /// </summary>
        public bool SelectionState { get; private set; }

        /// <summary>Gets the selections.</summary>
        public ObservableCollection<ItemData> Selections
        {
            get
            {
                return this.selections;
            }
        }

        /// <summary>
        /// The show progress.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        public static void ShowProgress(string title, string status)
        {
            activeWindow.SetProgress(title, status);
        }

        /// <summary>
        /// The get selection.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <returns>
        /// The <see cref="ItemData"/>.
        /// </returns>
        public static ItemData GetSelection(ICollection<ItemData> data, string title, string status)
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

            return activeWindow.Dispatcher.Invoke(() => activeWindow.SelectionState ? activeWindow.SelectedItem : null);
        }

        /// <summary>
        /// Starts the view and wait for interaction.
        /// </summary>
        public static void Start()
        {
            activeWindow.Dispatcher.Invoke(
                () =>
                { activeWindow.ShowDialog(); });
        }

        /// <summary>The close all.</summary>
        public static void CloseAll()
        {
            if (activeWindow != null)
            {
                activeWindow.Dispatcher.Invoke(() => activeWindow.Dispose());
            }
        }

        /// <summary>
        /// Hides the window.
        /// </summary>
        public new void Hide()
        {
            if (this.IsVisible)
            {
                this.WindowState = WindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }

        public new void Show()
        {
            if (!this.IsVisible)
            {
                base.Show();
            }

            this.WindowState = WindowState.Normal;
            this.ShowInTaskbar = true;
            this.SelectionState = false;
        }

        /// <summary>
        /// Disposes the window.
        /// </summary>
        public void Dispose()
        {
            this.disposing = true;
            this.Close();
        }

        /// <summary>
        /// The list box item mouse double click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void ListBoxItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Accept();
        }

        /// <summary>
        /// The raise property changed.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// The set progress.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="newStatus">
        /// The status.
        /// </param>
        private void SetProgress(string title, string newStatus)
        {
            this.Dispatcher.Invoke(
                () =>
                {
                    this.SelectionBox.Visibility = Visibility.Collapsed;
                    this.ProgressBar.Visibility = Visibility.Visible;
                    this.TitleText = title;
                    this.Status = newStatus;
                    this.SizeToContent = SizeToContent.WidthAndHeight;
                    this.Show();
                    this.SizeToContent = SizeToContent.Manual;
                });
        }

        /// <summary>
        /// The set selections.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
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
                });
        }

        /// <summary>The accept.</summary>
        private void Accept()
        {
            this.SelectionState = true;
            this.Hide();
        }

        /// <summary>
        /// The list box item preview key up.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ListBoxItemPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Accept();
            }
        }

        /// <summary>
        /// The main window_ on preview key up.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void MainWindow_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        /// <summary>
        /// The main window_ on content rendered.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void MainWindow_OnContentRendered(object sender, EventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
        }

        /// <summary>
        /// The main window_ on loaded.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (lastPosition == null)
            {
                var mousePosition = Control.MousePosition;
                var activeScreenArea = Screen.FromPoint(mousePosition).WorkingArea;
                this.MaxHeight = activeScreenArea.Height;
                this.MaxWidth = activeScreenArea.Width;
                var left = mousePosition.X - (this.ActualWidth / 2);
                var top = mousePosition.Y - (this.ActualHeight / 2);
                this.Left = left > 0 ? left : 0;
                this.Top = top > 0 ? top : 0;
            }
            else
            {
                this.Left = lastPosition.Item1;
                this.Top = lastPosition.Item2;
            }
        }

        /// <summary>
        /// When the visibility is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void MainWindow_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.Equals(false))
            {
                lastPosition = new Tuple<double, double>(this.Left, this.Top);
            }
        }

        /// <summary>
        /// When the window is closing.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!this.disposing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
    }
}