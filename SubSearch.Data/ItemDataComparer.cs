namespace SubSearch.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The item data comparer.
    /// </summary>
    public class ItemDataComparer : IComparer<ItemData>, IComparer<Subtitle>
    {
        /// <summary>
        /// The zero space char.
        /// </summary>
        private const char ZeroWidthChar = '\u200B';

        /// <summary>
        /// The priority keywords.
        /// </summary>
        private static readonly string[] PriorityKeywords =
        {
            "1080",
            "1080p",
            "10bit",
            "5¶1",
            "720",
            "720p",
            "8bit",
            "AAC",
            "AMZN",
            "BDRip",
            "BluRay",
            "BRRip",
            "DL",
            "DTS",
            "h264",
            "HDRip",
            "HDTV",
            "HEVC",
            "WEB",
            "WEBDL",
            "x264",
            "x265",
            "Xvid",
            "DivX"

        };

        /// <summary>
        /// The regex for season episode
        /// </summary>
        private static readonly Regex SeasonRegex = new Regex(@"S\d+?E\d+?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        /// <summary>
        /// The target groups.
        /// </summary>
        private string[] targetGroups;

        /// <summary>
        /// The target.
        /// </summary>
        private string target;

        public ItemDataComparer(string target)
        {
            this.Target = target;
        }

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        public string Target
        {
            get
            {
                return this.target;
            }

            set
            {
                this.target = value;
                this.targetGroups = GetGroups(value);
            }
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>.
        /// Less than zero: <paramref name="x"/> is less than <paramref name="y"/>.
        /// Zero: <paramref name="x"/> equals <paramref name="y"/>.
        /// Greater than zero: <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        public int Compare(ItemData x, ItemData y)
        {
            return this.CompareItem(x, y);
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        public int Compare(Subtitle x, Subtitle y)
        {
            return this.CompareItem(x, y);
        }

        /// <summary>
        /// Gets matches count.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The number of matches count.</returns>
        public int GetMatchesPoints(string input)
        {
            var groups = GetGroups(input);
            return this.targetGroups.Sum(
                t =>
                {
                    var match = groups.FirstOrDefault(g => string.Equals(g, t, StringComparison.InvariantCultureIgnoreCase)) ?? string.Empty;
                    return GetPoints(match);
                });
        }

        /// <summary>
        /// Normalizes the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        private static string Normalize(string x)
        {
            var keys = new[] { '.', ' ', '-', '_', '(', ')' };
            var arr = x.Replace("5.1", "5¶1").ToCharArray();
            for (var i = 0; i < arr.Length; i++)
            {
                if (keys.Contains(arr[i]))
                {
                    arr[i] = ZeroWidthChar;
                }
            }

            return new string(arr).Replace("5¶1", "5.1");
        }

        /// <summary>
        /// Gets the groups.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns>The string groups.</returns>
        private static string[] GetGroups(string x)
        {
            return Normalize(x).Split(new[] { ZeroWidthChar }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Gets points.
        /// </summary>
        /// <param name="keyword">The keyword.</param>
        /// <returns>The matching points.</returns>
        private static int GetPoints(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return 0;
            }

            if (SeasonRegex.IsMatch(keyword))
            {
                return 60;
            }

            return PriorityKeywords.Contains(keyword, StringComparer.InvariantCultureIgnoreCase) ? 12 : 10;
        }

        /// <summary>
        /// Compares the string.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>The relative comparison between 2 strings.</returns>
        private int CompareString(string x, string y)
        {
            var result = this.GetMatchesPoints(x).CompareTo(this.GetMatchesPoints(y));
            return result == 0 ? string.CompareOrdinal(x, y) : result;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>.
        /// Less than zero: <paramref name="x"/> is less than <paramref name="y"/>.
        /// Zero: <paramref name="x"/> equals <paramref name="y"/>.
        /// Greater than zero: <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        private int CompareItem(ItemData x, ItemData y)
        {
            if (x.Name == y.Name)
            {
                return x.Rating.CompareTo(y.Rating);
            }

            if (x.Name == this.Target)
            {
                return 1;
            }

            if (y.Name == this.Target)
            {
                return -1;
            }

            var nameCompare = this.CompareString(x.Name, y.Name);
            var ratingCompare = x.Rating.CompareTo(y.Rating) * 10;
            return nameCompare + ratingCompare;
        }
    }
}
