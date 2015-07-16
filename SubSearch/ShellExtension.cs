namespace SubSearch
{
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    using SharpShell.Attributes;
    using SharpShell.SharpContextMenu;

    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".avi", ".mkv", ".mp4", ".wmv")]
    public class ShellExtension : SharpContextMenu
    {
        /// <summary>
        /// Determines whether this instance can a shell
        /// context show menu, given the specified selected file list.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance should show a shell context
        /// menu for the specified file list; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanShowMenu()
        {
            return true;
        }

        /// <summary>
        /// Creates the context menu. This can be a single menu item or a tree of them.
        /// </summary>
        /// <returns>
        /// The context menu for the shell context menu.
        /// </returns>
        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();
            var downloadSubtitleItem = new ToolStripMenuItem
            {
                Text = "Download subtitle",
                Image = Properties.Resources.SubSearch
            };

            downloadSubtitleItem.Click += (sender, args) => this.DownloadSubtitle();
            menu.Items.Add(downloadSubtitleItem);
            return menu;
        }

        /// <summary>
        /// Counts the lines in the selected files.
        /// </summary>
        private void DownloadSubtitle()
        {
            using (var db = new SubSceneDb())
            {
                foreach (var selectedFile in this.SelectedItemPaths)
                {
                    db.Query(selectedFile);
                }
            }
        }
    }
}
