namespace SubSearch.WPF.View
{
    using System.ComponentModel;

    /// <summary>Interaction logic for MessageDialog.xaml</summary>
    public partial class MessageDialog : INotifyPropertyChanged
    {
        /// <summary>The message.</summary>
        private string message;

        /// <summary>Initializes a new instance of the <see cref="MessageDialog" /> class.</summary>
        public MessageDialog()
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
    }
}