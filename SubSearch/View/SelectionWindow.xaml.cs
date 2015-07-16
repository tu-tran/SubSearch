namespace SubSearch
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for SelectionWindow.xaml
    /// </summary>
    public partial class SelectionWindow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ItemData SelectedItem { get { return SelectionBox.SelectedItem as ItemData; } }

        private string status;

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

        private readonly ObservableCollection<ItemData> selections = new ObservableCollection<ItemData>();
        public ObservableCollection<ItemData> Selections { get { return this.selections; } }

        private static SelectionWindow activeWindow = null;

        public SelectionWindow()
        {
            this.InitializeComponent();
        }

        public static void ShowProgress(string status)
        {
            CloseAll();
            activeWindow.SetProgress(status);
        }

        public static ItemData GetSelection(IEnumerable<ItemData> data, string status)
        {
            CloseAll();
            activeWindow.SetSelections(data, status);
            var confirm = activeWindow.ShowDialog();
            if (confirm.HasValue && confirm.Value)
            {
                return activeWindow.SelectedItem;
            }

            return null;
        }

        public static void CloseAll()
        {
            if (activeWindow != null)
            {
                activeWindow.Close();
            }

            activeWindow = new SelectionWindow();
        }

        private void SetProgress(string status)
        {
            this.SelectionBox.Visibility = Visibility.Hidden;
            this.ProgressBar.Visibility = Visibility.Visible;
            this.Status = status;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.Show();
        }

        private void SetSelections(IEnumerable<ItemData> data, string status)
        {
            this.selections.Clear();
            foreach (var itemData in data)
            {
                this.selections.Add(itemData);
            }

            this.Status = status;
            this.SelectionBox.Visibility = Visibility.Visible;
            this.ProgressBar.Visibility = Visibility.Hidden;
            this.SizeToContent = SizeToContent.Width;
        }

        private void Accept()
        {
            this.DialogResult = true;
            this.Close();
        }

        private void ListBoxItemPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Accept();
            }
        }

        private void SelectionWindow_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        protected void ListBoxItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Accept();
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
