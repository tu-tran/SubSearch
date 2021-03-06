﻿namespace SubSearch.Data
{
    using System.Collections.Generic;
    using System.Diagnostics;

    using SubSearch.Data.Handlers;

    /// <summary>
    /// The <see cref="Subtitle"/> class.
    /// </summary>
    [DebuggerDisplay("[{DataSource}] {Name}")]
    public class Subtitle : ItemData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Subtitle"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="downloadUrl">The download URL.</param>
        /// <param name="rating">The rating.</param>
        public Subtitle(string name, string description, string downloadUrl, Rating rating, ISubtitleDb dataSource)
            : base(name, description, rating)
        {
            this.DownloadUrl = downloadUrl;
            this.DataSource = dataSource;
        }

        /// <summary>
        /// Gets the download URL.
        /// </summary>
        public string DownloadUrl { get; private set; }

        /// <summary>
        /// Gets the data source.
        /// </summary>
        public ISubtitleDb DataSource { get; private set; }
    }

    /// <summary>
    /// The <see cref="Subtitles"/> class.
    /// </summary>
    public class Subtitles : List<Subtitle> { }
}
