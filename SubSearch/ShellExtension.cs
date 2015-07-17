namespace SubSearch
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Forms;

    using SharpShell.Attributes;
    using SharpShell.SharpContextMenu;

    using SubSearch.Properties;

    using MessageBox = System.Windows.MessageBox;

    /// <summary>The shell extension.</summary>
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".avi", ".mkv", ".wmv", ".mp4", ".m4p", ".m4v", ".mpg", ".3gp")]
    [COMServerAssociation(AssociationType.Directory)]
    public class ShellExtension : SharpContextMenu
    {
        /// <summary>The file associations.</summary>
        public static readonly IEnumerable<string> FileAssociations;

        /// <summary>Initializes static members of the <see cref="ShellExtension" /> class.</summary>
        static ShellExtension()
        {
            var attribute =
                typeof(ShellExtension).GetCustomAttributes<COMServerAssociationAttribute>()
                    .FirstOrDefault(attrib => attrib.AssociationType == AssociationType.ClassOfExtension);
            if (attribute != null)
            {
                FileAssociations = attribute.Associations;
            }
        }

        /// <summary>Determines whether this instance can a shell context show menu, given the specified selected file list.</summary>
        /// <returns>
        /// <c>true</c> if this instance should show a shell context menu for the specified file list; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanShowMenu()
        {
            return true;
        }

        /// <summary>Creates the context menu. This can be a single menu item or a tree of them.</summary>
        /// <returns>The context menu for the shell context menu.</returns>
        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();
            var downloadSubtitleItem = new ToolStripMenuItem
                                           {
                                               Text = Resources.ShellExtension_CreateMenu_Download_subtitle, 
                                               Image = Resources.SubSearch
                                           };

            downloadSubtitleItem.Click += (sender, args) => this.DownloadSubtitle();
            menu.Items.Add(downloadSubtitleItem);
            return menu;
        }

        /// <summary>Download subtitle for the selected file.</summary>
        private void DownloadSubtitle()
        {
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            currentPath = Path.GetFullPath(currentPath);
            var executable = Path.GetFullPath(Path.Combine(currentPath, "SubSearch.App.exe"));
            if (!File.Exists(executable))
            {
                MessageBox.Show(
                    "SubSearch application was not properly installed. Please try restarting the computer and reinstall SubSearch!", 
                    "Corrupted Installation", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return;
            }

            var queuePath = Path.Combine(currentPath, "Queue");
            Directory.CreateDirectory(queuePath);
            var newQueue = Path.Combine(queuePath, Guid.NewGuid().ToString());
            using (var queueFile = File.CreateText(newQueue))
            {
                foreach (var selectedFile in this.SelectedItemPaths)
                {
                    queueFile.WriteLine(selectedFile);
                }
            }

            Process.Start(executable, newQueue);
        }
    }
}