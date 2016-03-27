// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubSceneDb.cs" company="">
//   
// </copyright>
// <summary>
//   The sub scene db.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SubSearch.Data
{
    using HtmlAgilityPack;
    using SharpCompress.Archive;
    using SubSearch.Resources;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;

    /// <summary>The sub scene db.</summary>
    public sealed class SubSceneDb : ISubtitleDb
    {
        /// <summary>The language codes.</summary>
        private static readonly Dictionary<Language, string> LanguageCodes = new Dictionary<Language, string>
                                                                                 {
                                                                                     { Language.English, "13" },
                                                                                     { Language.Vietnamese, "45" }
                                                                                 };

        /// <summary>The view.</summary>
        private readonly IViewHandler view;

        /// <summary>Initializes a new instance of the <see cref="SubSceneDb"/> class.</summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="viewHandler">The view handler.</param>
        /// <param name="language">The language.</param>
        public SubSceneDb(string filePath, IViewHandler viewHandler, Language language)
            : this(filePath, viewHandler)
        {
            this.Language = language;
        }

        /// <summary>Initializes a new instance of the <see cref="SubSceneDb"/> class.</summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="viewHandler">The view handler.</param>
        public SubSceneDb(string filePath, IViewHandler viewHandler)
        {
            this.FilePath = filePath;
            this.Title = Path.GetFileNameWithoutExtension(filePath);
            this.view = viewHandler;
        }

        /// <summary>The file path.</summary>
        public string FilePath { get; private set; }

        /// <summary>The language.</summary>
        public Language Language { get; private set; }

        /// <summary>Gets or sets the title.</summary>
        public string Title { get; set; }

        /// <summary>The download subtitle.</summary>
        /// <param name="subtitleDownloadUrl">The subtitle download url.</param>
        /// <param name="cookies">The cookies.</param>
        /// <returns>The query result.</returns>
        public QueryResult DownloadSubtitle(string subtitleDownloadUrl, CookieContainer cookies = null)
        {
            if (subtitleDownloadUrl == null)
            {
                return QueryResult.Failure;
            }

            if (subtitleDownloadUrl == string.Empty)
            {
                return QueryResult.Skipped;
            }

            var title = Path.GetFileNameWithoutExtension(this.FilePath);
            var path = Path.GetDirectoryName(this.FilePath);
            var targetFile = Path.Combine(path, title);
            this.view.ShowProgress(this.FilePath, string.Format(Literals.Data_Downloading_video_subtitle, this.Language.Localize()));
            var result = this.DoDownloadSubtitle(subtitleDownloadUrl, subtitleDownloadUrl, cookies, targetFile);
            this.view.ShowProgress(this.FilePath, Literals.Data_Idle);
            return result ? QueryResult.Success : QueryResult.Failure;
        }

        /// <summary>Queries Subscene database.</summary>
        /// <returns>The query result.</returns>
        public QueryResult Query()
        {
            this.view.ShowProgress(this.FilePath, Literals.Data_Searching_video_title);
            var encodedTitle = HttpUtility.UrlEncode(this.Title);
            var queryUrl = string.Format("http://subscene.com/subtitles/release?q={0}&l=", encodedTitle);

            string languageCode;
            if (!LanguageCodes.TryGetValue(this.Language, out languageCode))
            {
                languageCode = LanguageCodes.Values.FirstOrDefault();
            }

            var cookies = new CookieContainer();
            cookies.Add(new Cookie("LanguageFilter", languageCode, "/", ".subscene.com"));

            var queryResultDoc = this.GetDocument(queryUrl, "http://subscene.com", cookies, false);
            var searchResultUrl = this.ParseQueryDoc(queryResultDoc.Item1);
            if (searchResultUrl.Item1 == QueryResult.Cancelled)
            {
                return QueryResult.Cancelled;
            }

            Tuple<HtmlDocument, CookieContainer> subtitleDownloadDoc;
            if (string.IsNullOrEmpty(searchResultUrl.Item2))
            {
                subtitleDownloadDoc = queryResultDoc;
            }
            else
            {
                subtitleDownloadDoc = this.GetDocument(searchResultUrl.Item2, queryUrl, queryResultDoc.Item2, false);
            }

            this.view.ShowProgress(this.FilePath, Literals.Data_Searching_video_subtitle);
            var subtitleDownloadUrl = this.ParseSubDownloadDoc(subtitleDownloadDoc.Item1);
            if (subtitleDownloadUrl.Item1 == QueryResult.Cancelled)
            {
                return QueryResult.Cancelled;
            }

            return this.DownloadSubtitle(subtitleDownloadUrl.Item2, subtitleDownloadDoc.Item2);
        }

        /// <summary>The get request.</summary>
        /// <param name="url">The url.</param>
        /// <param name="referrer">The referrer.</param>
        /// <param name="cookies">The cookies.</param>
        /// <param name="isMobile">The is Mobile.</param>
        /// <returns>The <see cref="HttpWebRequest"/>.</returns>
        private static HttpWebRequest GetRequest(string url, string referrer = "", CookieContainer cookies = null, bool isMobile = true)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = isMobile
                                    ? "Mozilla/5.0 (Linux; U; Android 4.2; en-us; SonyC6903 Build/14.1.G.1.518) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30"
                                    : "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; AS; rv:11.0) like Gecko";
            request.Referer = referrer;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";

            if (cookies == null)
            {
                request.CookieContainer = new CookieContainer();
            }
            else
            {
                request.CookieContainer = cookies;
            }

            return request;
        }

        /// <summary>The download subtitle.</summary>
        /// <param name="subtitleDownloadUrl">The subtitle download url.</param>
        /// <param name="referrer">The referrer.</param>
        /// <param name="cookies">The cookies.</param>
        /// <param name="targetFileWithoutExtension">The target file without extension.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool DoDownloadSubtitle(string subtitleDownloadUrl, string referrer, CookieContainer cookies, string targetFileWithoutExtension)
        {
            var htmlDoc = this.GetDocument("http://subscene.com" + subtitleDownloadUrl, referrer, cookies);
            var downloadNodes = htmlDoc.Item1.DocumentNode.SelectNodes("//a[@id='downloadButton']");
            if (downloadNodes == null)
            {
                return false;
            }

            try
            {
                foreach (var downloadNode in downloadNodes)
                {
                    var link = downloadNode.GetAttributeValue("href", string.Empty);
                    var response = GetRequest("http://subscene.com" + link).GetResponse();

                    using (var respStream = response.GetResponseStream())
                    {
                        if (respStream == null)
                        {
                            continue;
                        }

                        using (var ms = new MemoryStream())
                        {
                            respStream.CopyTo(ms);
                            ms.Seek(0, SeekOrigin.Begin);

                            var reader = ArchiveFactory.Open(ms);
                            if (reader != null)
                            {
                                foreach (var entry in reader.Entries)
                                {
                                    if (entry.IsDirectory)
                                    {
                                        continue;
                                    }

                                    var entryPath = targetFileWithoutExtension + Path.GetExtension(entry.Key);
                                    entry.WriteToFile(entryPath);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.view.Notify(Literals.Data_Failed_download_subtitles + ex.Message);
                return false;
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
            var request = GetRequest(url, referrer, cookies, isMobile);
            using (var respStream = request.GetResponse().GetResponseStream())
            {
                if (respStream != null)
                {
                    htmlDoc.Load(respStream, Encoding.UTF8);
                }
            }

            return Tuple.Create(htmlDoc, request.CookieContainer);
        }

        /// <summary>The get matching url.</summary>
        /// <param name="exactList">The exact list.</param>
        /// <param name="popularList">The popular list.</param>
        /// <param name="closeList">The close list.</param>
        /// <returns>The <see cref="ItemData"/>.</returns>
        private Tuple<QueryResult, ItemData> GetMatchingUrl(List<ItemData> exactList, List<ItemData> popularList, List<ItemData> closeList)
        {
            // Process the actual subtitle link
            foreach (var matchingTitle in exactList)
            {
                return Tuple.Create(QueryResult.Success, matchingTitle);
            }

            var comparer = new InlineComparer<ItemData>((a, b) => string.Equals(a.Text, b.Text), i => i == null ? 0 : i.GetHashCode());
            var selections = popularList.Union(closeList, comparer).ToList();
            if (!selections.Any())
            {
                this.view.Notify(Literals.Data_No_matching_title_for + this.FilePath);
                return Tuple.Create<QueryResult, ItemData>(QueryResult.Failure, null);
            }

            var matchingUrl = this.view.GetSelection(selections, this.FilePath, Literals.Data_Select_matching_video_title);
            return matchingUrl;
        }

        /// <summary>The parse query doc.</summary>
        /// <param name="htmlDoc">The html doc.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private Tuple<QueryResult, string> ParseQueryDoc(HtmlDocument htmlDoc)
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

            var matchingUrl = this.GetMatchingUrl(exactList, popularList, closeList);
            var itemData = matchingUrl.Item2;
            var url = itemData == null || string.IsNullOrEmpty(itemData.Tag as string) ? string.Empty : "http://subscene.com" + itemData.Tag;
            return Tuple.Create(matchingUrl.Item1, url);
        }

        /// <summary>The parse sub download doc.</summary>
        /// <param name="htmlDoc">The html doc.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private Tuple<QueryResult, string> ParseSubDownloadDoc(HtmlDocument htmlDoc)
        {
            var subtitleNodes = htmlDoc.DocumentNode.SelectNodes("//td[@class='a1']");

            ItemData selectedItem = null;
            var result = QueryResult.Failure;

            if (subtitleNodes != null)
            {
                var selections = new List<ItemData>();
                foreach (var subtitleNode in subtitleNodes)
                {
                    var linkNode = subtitleNode.SelectSingleNode(".//a");
                    if (linkNode == null)
                    {
                        continue;
                    }

                    var link = linkNode.GetAttributeValue("href", string.Empty);
                    var icon = Icon.Dot;
                    var linkSpanNodes = linkNode.SelectNodes(".//span");
                    var rateNode = linkSpanNodes.FirstOrDefault();
                    if (rateNode != null)
                    {
                        var rateClass = rateNode.GetAttributeValue("class", string.Empty).Split(' ').LastOrDefault();
                        if (!string.IsNullOrEmpty(rateClass) && rateClass.Contains("-icon"))
                        {
                            var rate = rateClass.Substring(0, rateClass.Length - 5);
                            Enum.TryParse(rate, true, out icon);
                        }
                    }

                    var titleNode = linkSpanNodes.LastOrDefault();
                    if (titleNode == null)
                    {
                        continue;
                    }

                    var title = titleNode.InnerText.Trim();
                    var commentNode = subtitleNode.ParentNode.SelectSingleNode(".//td[@class='a6']/div");
                    string description = null;
                    if (commentNode != null)
                    {
                        description = commentNode.InnerText.Trim();
                    }

                    selections.Add(
                        new ItemData(HttpUtility.HtmlDecode(title), link)
                        {
                            Icon = icon,
                            Description =
                                HttpUtility.HtmlDecode(description.Replace(Environment.NewLine, " "))
                        });
                }

                if (selections.Count == 1)
                {
                    selectedItem = selections.First();
                    result = QueryResult.Success;
                }
                else if (selections.Count > 1)
                {
                    var selection = this.view.GetSelection(
                        selections,
                        this.FilePath,
                        string.Format(Literals.Data_Select_subtitle, this.Language.Localize()));
                    result = selection.Item1;
                    selectedItem = selection.Item2;
                }
                else
                {
                    this.view.Notify(Literals.Data_No_subtitle_for + this.FilePath);
                }
            }

            var downloadUrl = selectedItem == null ? string.Empty : selectedItem.Tag as string;
            return Tuple.Create(result, downloadUrl);
        }
    }
}