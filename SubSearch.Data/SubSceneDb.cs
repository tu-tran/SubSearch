namespace SubSearch
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Web;

    using HtmlAgilityPack;

    using Ionic.Zip;

    using SubSearch.Data;

    /// <summary>The sub scene db.</summary>
    public sealed class SubSceneDb
    {
        /// <summary>The file path.</summary>
        private readonly string filePath;

        /// <summary>The view.</summary>
        private readonly IViewHandler view;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubSceneDb"/> class.
        /// </summary>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <param name="viewHandler">
        /// The view handler.
        /// </param>
        public SubSceneDb(string filePath, IViewHandler viewHandler)
        {
            this.filePath = filePath;
            this.view = viewHandler;
        }

        /// <summary>The query.</summary>
        public void Query()
        {
            this.view.ShowProgress(this.filePath, "Searching for video title...");
            var title = Path.GetFileNameWithoutExtension(this.filePath);
            var encodedTitle = HttpUtility.UrlEncode(title);
            var queryUrl = string.Format("http://subscene.com/subtitles/title?q={0}&l=", encodedTitle);
            var cookies = new CookieContainer();
            cookies.Add(new Cookie("LanguageFilter", "13", "/", ".subscene.com"));

            var queryResultDoc = this.GetDocument(queryUrl, "http://subscene.com", cookies);

            var searchResultUrl = this.ParseQueryDoc(queryResultDoc.Item1);

            Tuple<HtmlDocument, CookieContainer> subtitleDownloadDoc;
            if (string.IsNullOrEmpty(searchResultUrl))
            {
                subtitleDownloadDoc = queryResultDoc;
            }
            else
            {
                subtitleDownloadDoc = this.GetDocument(searchResultUrl, queryUrl, queryResultDoc.Item2);
            }

            this.view.ShowProgress(this.filePath, "Searching for movie subtitle...");
            var subtitleDownloadUrl = this.ParseSubDownloadDoc(subtitleDownloadDoc.Item1);
            if (string.IsNullOrEmpty(subtitleDownloadUrl))
            {
                return;
            }

            var path = Path.GetDirectoryName(this.filePath);
            var targetFile = Path.Combine(path, title);
            this.view.ShowProgress(this.filePath, "Downloading the movie subtitle...");
            this.DownloadSubtitle(subtitleDownloadUrl, subtitleDownloadUrl, subtitleDownloadDoc.Item2, targetFile);
        }

        /// <summary>
        /// The get request.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="referrer">
        /// The referrer.
        /// </param>
        /// <param name="cookies">
        /// The cookies.
        /// </param>
        /// <returns>
        /// The <see cref="HttpWebRequest"/>.
        /// </returns>
        private static HttpWebRequest GetRequest(string url, string referrer = "", CookieContainer cookies = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent =
                "Mozilla/5.0 (Linux; U; Android 4.2; en-us; SonyC6903 Build/14.1.G.1.518) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30";
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

        /// <summary>
        /// The download subtitle.
        /// </summary>
        /// <param name="subtitleDownloadUrl">
        /// The subtitle download url.
        /// </param>
        /// <param name="referrer">
        /// The referrer.
        /// </param>
        /// <param name="cookies">
        /// The cookies.
        /// </param>
        /// <param name="targetFileWithoutExtension">
        /// The target file without extension.
        /// </param>
        private void DownloadSubtitle(string subtitleDownloadUrl, string referrer, CookieContainer cookies, string targetFileWithoutExtension)
        {
            var htmlDoc = this.GetDocument("http://subscene.com" + subtitleDownloadUrl, referrer, cookies);
            var downloadNodes = htmlDoc.Item1.DocumentNode.SelectNodes("//a[@id='downloadButton']");
            if (downloadNodes == null)
            {
                return;
            }

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
                        using (var zip = new ZipInputStream(ms))
                        {
                            ZipEntry entry;
                            while ((entry = zip.GetNextEntry()) != null)
                            {
                                var filePath = targetFileWithoutExtension + Path.GetExtension(entry.FileName);
                                using (var fileStream = File.OpenWrite(filePath))
                                {
                                    int n;
                                    var buffer = new byte[2048];
                                    while ((n = zip.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        fileStream.Write(buffer, 0, n);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The parse sub download doc.
        /// </summary>
        /// <param name="htmlDoc">
        /// The html doc.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string ParseSubDownloadDoc(HtmlDocument htmlDoc)
        {
            var subtitleNodes = htmlDoc.DocumentNode.SelectNodes("//a[@class='list-group-item']");
            if (subtitleNodes == null)
            {
                return string.Empty;
            }

            var selections = new List<ItemData>();
            foreach (var subtitleNode in subtitleNodes)
            {
                var link = subtitleNode.GetAttributeValue("href", string.Empty);
                var titleNode = subtitleNode.SelectSingleNode(".//h4");
                var title = titleNode.InnerText.Trim(' ', '\r', '\n');
                selections.Add(new ItemData(title, link));
            }

            ItemData selectedItem = null;
            if (selections.Count == 1)
            {
                selectedItem = selections.First();
            }
            else if (selections.Count > 1)
            {
                selectedItem = this.view.GetSelection(selections, this.filePath, "Select the subtitle to download");
            }

            return selectedItem == null ? string.Empty : selectedItem.Tag as string;
        }

        /// <summary>
        /// The get document.
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <param name="referrer">
        /// The referrer.
        /// </param>
        /// <param name="cookies">
        /// The cookies.
        /// </param>
        /// <returns>
        /// The <see cref="Tuple"/>.
        /// </returns>
        private Tuple<HtmlDocument, CookieContainer> GetDocument(string url, string referrer = "", CookieContainer cookies = null)
        {
            var htmlDoc = new HtmlDocument();
            var request = GetRequest(url, referrer, cookies);
            using (var respStream = request.GetResponse().GetResponseStream())
            {
                if (respStream != null)
                {
                    htmlDoc.Load(respStream);
                }
            }

            return Tuple.Create(htmlDoc, request.CookieContainer);
        }

        /// <summary>
        /// The parse query doc.
        /// </summary>
        /// <param name="htmlDoc">
        /// The html doc.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string ParseQueryDoc(HtmlDocument htmlDoc)
        {
            var resultNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='search-result']");
            if (resultNodes == null)
            {
                return string.Empty;
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
                        var titleNodes = childNode.SelectNodes(".//div[@class='title']/a");
                        if (titleNodes == null)
                        {
                            continue;
                        }

                        foreach (var titleNode in titleNodes)
                        {
                            var link = titleNode.GetAttributeValue("href", string.Empty);
                            if (!string.IsNullOrEmpty(link))
                            {
                                activeList.Add(new ItemData(titleNode.InnerText, link));
                            }
                        }

                        activeList = null;
                    }
                }
            }

            var matchingUrl = this.GetMatchingUrl(exactList, popularList, closeList);
            return matchingUrl == null || string.IsNullOrEmpty(matchingUrl.Tag as string) ? string.Empty : "http://subscene.com" + matchingUrl.Tag;
        }

        /// <summary>
        /// The get matching url.
        /// </summary>
        /// <param name="exactList">
        /// The exact list.
        /// </param>
        /// <param name="popularList">
        /// The popular list.
        /// </param>
        /// <param name="closeList">
        /// The close list.
        /// </param>
        /// <returns>
        /// The <see cref="ItemData"/>.
        /// </returns>
        private ItemData GetMatchingUrl(List<ItemData> exactList, List<ItemData> popularList, List<ItemData> closeList)
        {
            // Process the actual subtitle link
            foreach (var matchingTitle in exactList)
            {
                return matchingTitle;
            }

            var selections = popularList.Join(closeList, s => s.Name, s => s.Name, (s, s1) => s);
            var matchingUrl = this.view.GetSelection(selections, this.filePath, "Select the matching movie title");
            return matchingUrl;
        }
    }
}