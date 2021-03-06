﻿using System.Threading;

namespace SubSearch.WPF.Controllers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using SubSearch.Data;
    using SubSearch.Data.Handlers;
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
            get => this.filePath;
            set
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
        public Status Download(Subtitle subtitle)
        {
            this.view.ShowProgress(this.FilePath, string.Format(Literals.Data_Downloading_video_subtitle, AppContext.Global.LocalizedLanguage));
            var retries = 4;
            var tries = 0;
            var status = Status.Failure;

            while (tries++ < retries)
            {
                try
                {
                    this.db.Download(this.FilePath, subtitle);
                    status = Status.Success;
                    break;
                }
                catch (Exception ex)
                {
                    this.view.Notify(Literals.Data_Failed_download_subtitles + ex.Message);
                    Thread.Sleep(1000);
                }
            }

            this.view.ShowProgress(this.FilePath, Literals.Data_Idle);
            return status;
        }

        /// <summary>
        /// Gets the subtitles meta.
        /// </summary>
        /// <returns></returns>
        private QueryResult<Subtitles> GetSubtitlesMeta()
        {
            var retries = 4;
            var tries = 0;
            QueryResult<Subtitles> subtitlesMetaResult = new QueryResult<Subtitles>(Status.Fatal, new Subtitles(), string.Empty);

            while (tries++ < retries)
            {
                try
                {
                    subtitlesMetaResult = this.db.GetSubtitlesMeta(this.Title, AppContext.Global.Language);
                    break;
                }
                catch (Exception ex)
                {
                    Trace.TraceError(Literals.Data_Failed_to_get_subtitles_meta, this.Title, this.db.GetType().Name, ex);
                    subtitlesMetaResult = new QueryResult<Subtitles>(
                        Status.Fatal,
                        null,
                        string.Format(Literals.Data_Failed_to_get_subtitles_meta, this.Title, this.db.GetType().Name, ex.Message));
                }
            }

            return subtitlesMetaResult;
        }

        /// <summary>
        /// Queries this instance.
        /// </summary>
        /// <returns>The query result.</returns>
        public Status Query()
        {
            this.view.SetActiveFile(this.FilePath);
            this.view.ResetSelections();
            this.view.ShowProgress(this.FilePath, Literals.Data_Searching_video_subtitle);
            var subtitlesMetaResult = this.GetSubtitlesMeta();

            if (subtitlesMetaResult.Status != Status.Success)
            {
                if (!string.IsNullOrEmpty(subtitlesMetaResult.Message))
                {
                    this.view.Notify(subtitlesMetaResult.Message);
                    return subtitlesMetaResult.Status;
                }
            }

            if (!subtitlesMetaResult.Data.HasAny())
            {
                this.view.Notify(Literals.Data_No_subtitle_for + this.FilePath);
                return Status.Failure;
            }

            if (subtitlesMetaResult.Data.Count == 1)
            {
                return this.Download(subtitlesMetaResult.Data[0]);
            }

            var comparer = new ItemDataComparer(this.Title);
            var selections = subtitlesMetaResult.Data.OrderByDescending(i => i, comparer).Cast<ItemData>().ToList();
            var selectionResult = this.view.GetSelection(
                        selections,
                        this.FilePath,
                        string.Format(Literals.Data_Select_subtitle, AppContext.Global.LocalizedLanguage));

            if (selectionResult.Status != Status.Success || selectionResult.Data == null)
            {
                return selectionResult.Status;
            }

            return this.Download((Subtitle)selectionResult.Data);
        }
    }
}