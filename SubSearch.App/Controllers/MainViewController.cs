namespace SubSearch.WPF.Controllers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using SubSearch.Data;
    using SubSearch.Resources;

    /// <summary>
    /// The <see cref="MainViewController"/> class.
    /// </summary>
    internal class MainViewController
    {
        /// <summary>
        /// The view
        /// </summary>
        private readonly IView view;

        /// <summary>
        /// </summary>
        private readonly ISubtitleDb db;

        /// <summary>
        /// The file path.
        /// </summary>
        private string filePath;

        /// <summary>Initializes a new instance of the <see cref="MainViewController" /> class.</summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="view">The view.</param>
        /// <param name="db">The database.</param>
        public MainViewController(string filePath, IView view, ISubtitleDb db)
        {
            this.FilePath = filePath;
            this.view = view;
            this.db = db;
        }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        public string FilePath
        {
            get { return this.filePath; }
            private set
            {
                this.filePath = value;
                this.Title = Path.GetFileNameWithoutExtension(value);
            }
        }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Downloads subtitle.
        /// </summary>
        /// <param name="subtitle">The subtitle.</param>
        /// <returns>The query result.</returns>
        public QueryResult Download(Subtitle subtitle)
        {
            this.view.ShowProgress(this.FilePath, string.Format(Literals.Data_Downloading_video_subtitle, AppContext.Global.LocalizedLanguage));
            QueryResult result;
            try
            {
                result = this.db.Download(this.FilePath, subtitle);
            }
            catch (Exception ex)
            {
                this.view.Notify(Literals.Data_Failed_download_subtitles + ex.Message);
                return QueryResult.Fatal;
            }

            this.view.ShowProgress(this.FilePath, Literals.Data_Idle);
            return result;
        }

        /// <summary>
        /// Queries this instance.
        /// </summary>
        /// <returns>The query result.</returns>
        public QueryResult Query()
        {
            this.view.ShowProgress(this.FilePath, Literals.Data_Searching_video_title);
            this.view.ShowProgress(this.FilePath, Literals.Data_Searching_video_subtitle);
            var subtitlesMetaResult = this.db.GetSubtitlesMeta(this.Title, AppContext.Global.Language);
            if (subtitlesMetaResult.Status != QueryResult.Success)
            {
                return subtitlesMetaResult.Status;
            }

            if (!subtitlesMetaResult.Data.HasAny())
            {
                this.view.Notify(Literals.Data_No_subtitle_for + this.FilePath);
                return QueryResult.Failure;
            }

            if (subtitlesMetaResult.Data.Count == 1)
            {
                return this.Download(subtitlesMetaResult.Data[0]);
            }

            var selections = subtitlesMetaResult.Data.OrderByDescending(i => i, new ItemDataComparer(this.Title)).Cast<ItemData>().ToList();
            var selectionResult = this.view.GetSelection(
                        selections,
                        this.FilePath,
                        string.Format(Literals.Data_Select_subtitle, AppContext.Global.LocalizedLanguage));

            if (selectionResult.Status != QueryResult.Success || selectionResult.Data == null)
            {
                return selectionResult.Status;
            }

            return this.Download((Subtitle)selectionResult.Data);
        }
    }
}