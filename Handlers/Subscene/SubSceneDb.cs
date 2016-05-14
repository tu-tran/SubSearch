// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubSceneDb.cs" company="">
//   
// </copyright>
// <summary>
//   The sub scene db.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SubSearch.Data.Handlers.Subscene
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;

    using HtmlAgilityPack;

    using SubSearch.Data.Handlers;
    using SubSearch.Resources;

    public sealed class SubSceneDb : ISubtitleDb
    {
        /// <summary>
        /// The language codes.
        /// </summary>
        private static readonly Dictionary<Language, string> LanguageCodes = new Dictionary<Language, string>
                                                                                 {
                                                                                     { Language.English, "13" },
                                                                                     { Language.Vietnamese, "45" }
                                                                                 };

        private static readonly string BaseUrl = "http://subscene.com";

        /// <summary>
        /// Downloads the specified movie file.
        /// </summary>
        /// <param name="releaseFile">The movie file.</param>
        /// <param name="subtitle">The subtitle.</param>
        /// <returns>The query result.</returns>
        public QueryResult Download(string releaseFile, Subtitle subtitle)
        {
            return this.HandleDownloadRequest(releaseFile, subtitle);
        }

        /// <summary>
        /// Gets subtitles meta.
        /// </summary>
        /// <param name="releaseName">Name of the release.</param>
        /// <param name="language"></param>
        /// <returns>The query result.</returns>
        public QueryResult<Subtitles> GetSubtitlesMeta(string releaseName, Language language)
        {
            var encodedTitle = HttpUtility.UrlEncode(releaseName);
            var queryUrl = string.Format("http://subscene.com/subtitles/release?q={0}&r=true", encodedTitle);
            var referrer = string.Format("http://subscene.com/subtitles/title?q={0}&l=", encodedTitle);

            string languageCode;
            if (!LanguageCodes.TryGetValue(language, out languageCode))
            {
                languageCode = LanguageCodes.Values.FirstOrDefault();
            }

            var cookies = new CookieContainer();
            cookies.Add(new Cookie("LanguageFilter", languageCode, "/", ".subscene.com"));
            cookies.Add(new Cookie("HearingImpaired", "", "/", ".subscene.com"));

            var searchByReleaseDoc = this.GetDocument(queryUrl, referrer, cookies, false);
            return this.ParseSubtitleReleaseDoc(searchByReleaseDoc.Item1);
        }

        /// <summary>
        /// Handles download request.
        /// </summary>
        /// <param name="releaseFile">The movie file.</param>
        /// <param name="subtitle">The subtitle.</param>
        /// <param name="cookies">The cookies.</param>
        /// <returns>The query result.</returns>
        private QueryResult HandleDownloadRequest(string releaseFile, Subtitle subtitle, CookieContainer cookies = null)
        {
            if (string.IsNullOrEmpty(subtitle.DownloadUrl))
            {
                return QueryResult.Failure;
            }

            var result = this.DoDownloadSubtitle(subtitle.DownloadUrl, subtitle.DownloadUrl, cookies, releaseFile);
            return result ? QueryResult.Success : QueryResult.Failure;
        }

        /// <summary>
        /// Does download subtitle.
        /// </summary>
        /// <param name="subtitleDownloadUrl">The subtitle download URL.</param>
        /// <param name="referrer">The referrer.</param>
        /// <param name="cookies">The cookies.</param>
        /// <param name="targetFile">The target release file.</param>
        /// <returns>True on success; otherwise, false.</returns>
        private bool DoDownloadSubtitle(string subtitleDownloadUrl, string referrer, CookieContainer cookies, string targetFile)
        {
            var htmlDoc = this.GetDocument(subtitleDownloadUrl, referrer, cookies);
            var downloadNodes = htmlDoc.Item1.DocumentNode.SelectNodes("//a[@id='downloadButton']");
            if (downloadNodes == null)
            {
                return false;
            }

            foreach (var downloadNode in downloadNodes)
            {
                var link = downloadNode.GetAttributeValue("href", string.Empty);
                var url = "http://subscene.com" + link;
                targetFile.DownloadSubtitle(url);
            }

            return true;
        }

        /// <summary>The get document.</summary>
        /// <param name="url">The url.</param>
        /// <param name="referrer">The referrer.</param>
        /// <param name="cookies">The cookies.</param>
        /// <param name="isMobile">The is mobile.</param>
        /// <returns>The <see cref="Tuple"/>.</returns>
        private Tuple<HtmlDocument, CookieContainer> GetDocument(
            string url,
            string referrer = "",
            CookieContainer cookies = null,
            bool isMobile = true)
        {
            var htmlDoc = new HtmlDocument();
            var request = url.GetRequest(referrer, cookies, isMobile);
            using (var respStream = request.GetResponse().GetResponseStream())
            {
                if (respStream != null)
                {
                    htmlDoc.Load(respStream, Encoding.UTF8);
                }
            }

            return Tuple.Create(htmlDoc, request.CookieContainer);
        }

        /* TODO: Not in use as we are not searching for movie title
        /// <summary>The parse query doc.</summary>
        /// <param name="htmlDoc">The html doc.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private Tuple<QueryResult, string> ParseTitleDoc(HtmlDocument htmlDoc)
        {
            var resultNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='search-result']");
            if (resultNodes == null)
            {
                return Tuple.Create(QueryResult.Failure, string.Empty);
            }

            var exactList = new List<ItemData>();
            var popularList = new List<ItemData>();
            var closeList = new List<ItemData>();
            List<ItemData> activeList = null;

            foreach (var resultNode in resultNodes)
            {
                foreach (var childNode in resultNode.ChildNodes)
                {
                    if (childNode.Name == "h2")
                    {
                        var nodeClass = childNode.GetAttributeValue("class", string.Empty);
                        activeList = nodeClass == "exact"
                                         ? exactList
                                         : (nodeClass == "popular" ? popularList : (nodeClass == "close" ? closeList : null));
                        continue;
                    }

                    if (activeList != null && childNode.Name == "ul")
                    {
                        var titleNodes = childNode.SelectNodes(".//div[@class='title']");
                        if (titleNodes == null)
                        {
                            continue;
                        }

                        foreach (var titleNode in titleNodes)
                        {
                            var linkNode = titleNode.SelectNodes(".//a").FirstOrDefault();
                            if (linkNode == null)
                            {
                                continue;
                            }

                            var link = linkNode.GetAttributeValue("href", string.Empty).Trim();
                            if (!string.IsNullOrEmpty(link))
                            {
                                string count = null;
                                var countNode = titleNode.NextSibling;
                                while (countNode != null)
                                {
                                    if (countNode.GetAttributeValue("class", string.Empty).Contains("count"))
                                    {
                                        count = string.Format("({0})", countNode.InnerText.Trim());
                                        break;
                                    }

                                    countNode = countNode.NextSibling;
                                }

                                var title = HttpUtility.HtmlDecode(titleNode.InnerText).Trim();
                                activeList.Add(new ItemData(title, link) { Description = HttpUtility.HtmlDecode(count) });
                            }
                        }

                        activeList = null;
                    }
                }
            }

            var matchingUrl = this.GetSubtitlePageUrl(exactList, popularList, closeList);
            var itemData = matchingUrl.Item2;
            var url = itemData == null || string.IsNullOrEmpty(itemData.Tag as string) ? string.Empty : "http://subscene.com" + itemData.Tag;
            return Tuple.Create(matchingUrl.Item1, url);
        }

        /// <summary>The get matching url.</summary>
        /// <param name="exactList">The exact list.</param>
        /// <param name="popularList">The popular list.</param>
        /// <param name="closeList">The close list.</param>
        /// <returns>The <see cref="ItemData"/>.</returns>
        private Tuple<QueryResult, ItemData> GetSubtitlePageUrl(List<ItemData> exactList, List<ItemData> popularList, List<ItemData> closeList)
        {
            // Process the actual subtitle link
            foreach (var matchingTitle in exactList)
            {
                return Tuple.Create(QueryResult.Success, matchingTitle);
            }

            var comparer = new InlineComparer<ItemData>((a, b) => string.Equals(a.Name, b.Name), i => i == null ? 0 : i.GetHashCode());
            var selections = popularList.Union(closeList, comparer).ToList();
            if (!selections.Any())
            {
                this.view.Notify(Literals.Data_No_matching_title_for + this.FilePath);
                return Tuple.Create<QueryResult, ItemData>(QueryResult.Failure, null);
            }

            var matchingUrl = this.view.GetSelection(selections, this.FilePath, Literals.Data_Select_matching_video_title);
            return matchingUrl;
        }
         */

        /// <summary>
        /// Parses subtitle release document.
        /// </summary>
        /// <param name="htmlDoc">The HTML document.</param>
        /// <returns>The query result.</returns>
        private QueryResult<Subtitles> ParseSubtitleReleaseDoc(HtmlDocument htmlDoc)
        {
            var subtitleNodes = htmlDoc.DocumentNode.SelectNodes("//td[@class='a1']");

            Subtitles subtitles = new Subtitles();
            var result = QueryResult.Failure;

            if (subtitleNodes != null)
            {
                foreach (var subtitleNode in subtitleNodes)
                {
                    var linkNode = subtitleNode.SelectSingleNode(".//a");
                    if (linkNode == null)
                    {
                        continue;
                    }

                    var link = linkNode.GetAttributeValue("href", string.Empty);
                    var rating = Rating.Dot;
                    var linkSpanNodes = linkNode.SelectNodes(".//span");
                    var rateNode = linkSpanNodes.FirstOrDefault();
                    if (rateNode != null)
                    {
                        var rateClass = rateNode.GetAttributeValue("class", string.Empty).Split(' ').LastOrDefault();
                        if (!string.IsNullOrEmpty(rateClass) && rateClass.Contains("-icon"))
                        {
                            var rate = rateClass.Substring(0, rateClass.Length - 5);
                            Enum.TryParse(rate, true, out rating);
                        }
                    }

                    var titleNode = linkSpanNodes.LastOrDefault();
                    if (titleNode == null)
                    {
                        continue;
                    }

                    var title = titleNode.InnerText.Trim();
                    var commentNode = subtitleNode.ParentNode.SelectSingleNode(".//td[@class='a6']/div");
                    string description = string.Empty;
                    if (commentNode != null)
                    {
                        description = commentNode.InnerText.Trim();
                    }

                    description = HttpUtility.HtmlDecode(description.Replace(Environment.NewLine, " "));
                    title = HttpUtility.HtmlDecode(title);
                    var url = BaseUrl + "/" + link;
                    subtitles.Add(new Subtitle(title, description, url, rating, this));
                }

                result = QueryResult.Success;
            }

            return new QueryResult<Subtitles>(result, subtitles);
        }
    }
}