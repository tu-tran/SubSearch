namespace SubSearch.Data.Handlers
{
    using SubSearch.Resources;

    /// <summary>
    /// The <see cref="SubtitleDbBase"/> is the abstract class for all subtitle data handlers.
    /// </summary>
    public abstract class SubtitleDbBase : ISubtitleDb
    {
        /// <summary>
        /// Downloads the specified movie file.
        /// </summary>
        /// <param name="releaseFile">The movie file.</param>
        /// <param name="subtitle">The subtitle.</param>
        public virtual void Download(string releaseFile, Subtitle subtitle)
        {
            releaseFile.DownloadSubtitle(subtitle.DownloadUrl);
        }

        /// <summary>
        /// Gets subtitles meta.
        /// </summary>
        /// <param name="releaseName">Name of the release.</param>
        /// <param name="language"></param>
        /// <returns>The query result.</returns>
        public abstract QueryResult<Subtitles> GetSubtitlesMeta(string releaseName, Language language);
    }
}