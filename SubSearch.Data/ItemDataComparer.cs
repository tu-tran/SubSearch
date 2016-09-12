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
        /// The filter regex.
        /// </summary>
        private static readonly Regex FilterRegex = new Regex(@"(.+?)(\d+p)", RegexOptions.Compiled);

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
        /// Normalizes the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        private static string Normalize(string x)
        {
            var keys = new[] { '.', ' ', '-' };
            var arr = x.ToCharArray();
            for (var i = 0; i < arr.Length; i++)
            {
                if (keys.Contains(arr[i]))
                {
                    arr[i] = ZeroWidthChar;
                }
            }

            return new string(arr);
        }

        /// <summary>
        /// Gets the groups.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns>The string groups.</returns>
        private static string[] GetGroups(string x)
        {
            return Normalize(x).Split(ZeroWidthChar);
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

            var result = 0;
            foreach (var c in keyword)
            {
                var point = char.IsLetter(c) ? 2 : 1;
                result += point;
            }

            return result * keyword.Length;
        }

        /// <summary>
        /// Gets matches count.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns>The number of matches count.</returns>
        private int GetMatchesPoints(string x)
        {
            var filteredMatch = FilterRegex.Match(x);
            var input = filteredMatch.Success ? filteredMatch.Groups[1].Value : x;
            var groups = GetGroups(input);
            return this.targetGroups.Sum(
                t =>
                {
                    var match = groups.FirstOrDefault(g => string.Equals(g, t, StringComparison.InvariantCultureIgnoreCase)) ?? string.Empty;
                    return GetPoints(match);
                });
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

            var strCompare = this.CompareString(x.Name, y.Name);
            return strCompare == 0 ? x.Rating.CompareTo(y.Rating) : strCompare;
        }
    }
}
