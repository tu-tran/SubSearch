// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISubtitleDb.cs" company="">
//   
// </copyright>
// <summary>
//   The SubtitleDb interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SubSearch.Data.Handlers
{
    using SubSearch.Resources;

    /// <summary>
    /// The <see cref="ISubtitleDb"/> interfaces.
    /// </summary>
    public interface ISubtitleDb
    {
        /// <summary>
        /// Downloads the specified movie file.
        /// </summary>
        /// <param name="releaseFile">The movie file.</param>
        /// <param name="subtitle">The subtitle.</param>
        /// <returns>The query result.</returns>
        QueryResult Download(string releaseFile, Subtitle subtitle);

        /// <summary>
        /// Gets subtitles meta.
        /// </summary>
        /// <param name="releaseName">Name of the release.</param>
        /// <param name="language"></param>
        /// <returns>The query result.</returns>
        QueryResult<Subtitles> GetSubtitlesMeta(string releaseName, Language language);
    }
}