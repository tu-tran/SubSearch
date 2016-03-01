// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISubtitleDb.cs" company="">
//   
// </copyright>
// <summary>
//   The SubtitleDb interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SubSearch.Data
{
    using System.Net;

    using SubSearch.Resources;

    /// <summary>The SubtitleDb interface.</summary>
    public interface ISubtitleDb
    {
        /// <summary>The file path.</summary>
        string FilePath { get; }

        /// <summary>The language.</summary>
        Language Language { get; }

        string Title { get; set; }

        /// <summary>The download subtitle.</summary>
        /// <param name="subtitleDownloadUrl">The subtitle download url.</param>
        /// <param name="cookies">The cookies.</param>
        /// <returns>-1 on failure, 0 on skipping, 1 on success.</returns>
        int DownloadSubtitle(string subtitleDownloadUrl, CookieContainer cookies = null);

        /// <summary>Queries the subtitle database.</summary>
        /// <returns>-1 on failure, 0 on skipping, 1 on success.</returns>
        int Query();
    }
}