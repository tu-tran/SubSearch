namespace SubSearch.Data.Handlers.OpenSubtitles
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using OSDBnet;

    using SubSearch.Data.Handlers;
    using SubSearch.Resources;

    using Language = SubSearch.Resources.Language;
    using Subtitle = SubSearch.Data.Subtitle;

    public class OpenSubtitlesDb : SubtitleDbBase
    {
        /// <summary>
        /// The agent.
        /// </summary>
        private static readonly string agent = "SubSearchTu";

        /// <summary>
        /// The client.
        /// </summary>
        private readonly IAnonymousClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSubtitlesDb"/> class.
        /// </summary>
        public OpenSubtitlesDb()
        {
            this.client = Osdb.Login("en", agent);
        }

        /// <summary>
        /// Gets subtitles meta.
        /// </summary>
        /// <param name="releaseName">Name of the release.</param>
        /// <param name="language"></param>
        /// <returns>The query result.</returns>
        public override QueryResult<Subtitles> GetSubtitlesMeta(string releaseName, Language language)
        {
            var cultureInfo = language.GetCultureInfo();
            var langString = cultureInfo == null ? "eng" : cultureInfo.ThreeLetterISOLanguageName;
            var subtitles = new Subtitles();
            subtitles.AddRange(this.client.SearchSubtitlesFromQuery(langString, releaseName).Select(this.Convert));
            return new QueryResult<Subtitles>(QueryResult.Success, subtitles);
        }

        /// <summary>
        /// Converts the specified subtitle.
        /// </summary>
        /// <param name="subtitle">The subtitle.</param>
        /// <returns>The subtitle.</returns>
        private Subtitle Convert(OSDBnet.Subtitle subtitle)
        {
            Rating rating = Rating.Neutral;
            double ratingFigure;
            if (double.TryParse(subtitle.Rating, NumberStyles.Any, CultureInfo.InvariantCulture, out ratingFigure))
            {
                if (ratingFigure < 0.1)
                {
                    rating = Rating.Neutral;
                }
                else if (ratingFigure < 5.0)
                {
                    rating = Rating.Negative;
                }
                else if (ratingFigure > 5.0)
                {
                    rating = Rating.Positive;
                }
            }

            return new Subtitle(subtitle.MovieName, subtitle.SubtitleFileName, subtitle.SubTitleDownloadLink.AbsoluteUri, rating, this);
        }
    }
}
