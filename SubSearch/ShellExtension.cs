namespace SubSearch
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Forms;

    using SharpShell.Attributes;
    using SharpShell.SharpContextMenu;

    using SubSearch.Properties;
    using SubSearch.Resources;

    using MessageBox = System.Windows.MessageBox;

    /// <summary>The shell extension.</summary>
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.FileExtension, ".avi", ".mkv", ".wmv", ".mp4", ".m4p", ".m4v", ".mpg", ".3gp")]
    [COMServerAssociation(AssociationType.Directory)]
    public class ShellExtension : SharpContextMenu
    {
        /// <summary>The file associations.</summary>
        public static readonly IEnumerable<string> FileAssociations;

        /// <summary>The supported languages.</summary>
        private static readonly List<Language> SupportedLanguage = new List<Language> { Language.English, Language.Vietnamese };

        /// <summary>Initializes static members of the <see cref="ShellExtension" /> class.</summary>
        static ShellExtension()
        {
            var attribute =
                typeof(ShellExtension).GetCustomAttributes<COMServerAssociationAttribute>()
                    .FirstOrDefault(attrib => attrib.AssociationType == AssociationType.FileExtension);
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
            menu.Items.Add(this.GetLanguageItems(false));
            menu.Items.Add(this.GetLanguageItems(true));
            return menu;
        }

        /// <summary>Gets the language icon.</summary>
        /// <param name="name">The language.</param>
        /// <returns>The language icon.</returns>
        private static Bitmap GetLanguageIcon(string name)
        {
            var resource = Icons.ResourceManager.GetObject(name.ToLower()) as Bitmap;
            return resource;
        }

        /// <summary>Download subtitle for the selected file.</summary>
        /// <param name="option">The download option.</param>
        private void DownloadSubtitle(DownloadOption option)
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
                queueFile.WriteLine(option.Language);
                queueFile.WriteLine(option.IsLuckyMode ? Constants.SilentModeIdentifier : Constants.NormalModeIdentifier);
                foreach (var selectedFile in this.SelectedItemPaths)
                {
                    queueFile.WriteLine(selectedFile);
                }
            }

            Process.Start(executable, newQueue);
        }

        /// <summary>Get menu item.</summary>
        /// <param name="isLuckyMode">Is lucky mode.</param>
        /// <returns>Menu item.</returns>
        private ToolStripMenuItem GetLanguageItems(bool isLuckyMode)
        {
            var option = new DownloadOption { Language = SupportedLanguage.FirstOrDefault(), IsLuckyMode = isLuckyMode };
            var topMenu = new ToolStripMenuItem
                              {
                                  Text =
                                      isLuckyMode
                                          ? Literals.ShellExtension_CreateMenu_Download_subtitle_lucky
                                          : Literals.ShellExtension_CreateMenu_Download_subtitle, 
                                  Image = isLuckyMode ? Icons.SubSearchLucky : Icons.SubSearch, 
                                  Tag = option
                              };
            topMenu.Click += (sender, args) => this.DownloadSubtitle(option);

            foreach (var language in SupportedLanguage)
            {
                var subOption = new DownloadOption { Language = language, IsLuckyMode = isLuckyMode };
                var newLanguageItem = new ToolStripMenuItem
                                          {
                                              Text = language.ToString(), 
                                              Image = GetLanguageIcon(language.ToString()), 
                                              Tag = subOption
                                          };

                newLanguageItem.Click += (sender, args) => this.DownloadSubtitle(subOption);
                topMenu.DropDownItems.Add(newLanguageItem);
            }

            return topMenu;
        }

        /// <summary>The download option.</summary>
        private struct DownloadOption
        {
            /// <summary>The language.</summary>
            public Language Language;

            /// <summary>Is lucky mode.</summary>
            public bool IsLuckyMode;
        }
    }
}