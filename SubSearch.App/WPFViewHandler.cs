// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WPFViewHandler.cs" company="">
//   
// </copyright>
// <summary>
//   The wpf view handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SubSearch.WPF
{
    using System;
    using System.Collections.Generic;

    using SubSearch.Data;
    using SubSearch.WPF.View;

    /// <summary>The wpf view handler.</summary>
    internal class WpfViewHandler : IViewHandler
    {
        /// <summary>Initializes a new instance of the <see cref="WpfViewHandler" /> class.</summary>
        public WpfViewHandler()
        {
            MainWindow.Attach(this);
        }

        /// <summary>Occurs when the view handler custom action is requested.</summary>
        public event CustomActionDelegate CustomActionRequested;

        /// <summary>Occurs when the view handler is disposed.</summary>
        public event Action Disposed;

        /// <summary>Gets or sets the target file.</summary>
        public string TargetFile
        {
            get
            {
                return MainWindow.TargetFile;
            }

            set
            {
                MainWindow.TargetFile = value;
            }
        }

        /// <summary>Continues the pending operation and cancel any selection.</summary>
        public void Continue()
        {
            MainWindow.Continue();
        }

        /// <summary>The dispose.</summary>
        public void Dispose()
        {
            MainWindow.CloseAll();
            MainWindow.Detach(this);
            if (this.Disposed != null)
            {
                this.Disposed();
            }
        }

        /// <summary>The get selection.</summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        /// <returns>The <see cref="ItemData"/>.</returns>
        public virtual ItemData GetSelection(ICollection<ItemData> data, string title, string status)
        {
            return MainWindow.GetSelection(data, title, status);
        }

        /// <summary>Notifies a message.</summary>
        /// <param name="message">The message.</param>
        public void Notify(string message)
        {
            NotificationWindow.Show(message);
        }

        /// <summary>The on custom action.</summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="actionNames">The action names.</param>
        public void OnCustomAction(object parameter, params string[] actionNames)
        {
            if (this.CustomActionRequested != null)
            {
                this.CustomActionRequested(this, parameter, actionNames);
            }
        }

        /// <summary>The show progress.</summary>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        public void ShowProgress(string title, string status)
        {
            MainWindow.ShowProgress(title, status);
        }

        /// <summary>Sets the progress.</summary>
        /// <param name="done">Done.</param>
        /// <param name="total">Total</param>
        public void ShowProgress(int done, int total)
        {
            MainWindow.ShowProgress(done, total);
        }

        /// <summary>Starts the view and wait for interaction.</summary>
        public void Start()
        {
            MainWindow.Start();
        }
    }
}