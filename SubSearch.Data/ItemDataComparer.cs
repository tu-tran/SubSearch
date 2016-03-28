namespace SubSearch.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The item data comparer.
    /// </summary>
    public class ItemDataComparer : IComparer<ItemData>
    {
        /// <summary>
        /// The zero space char.
        /// </summary>
        private const char ZeroWidthChar = '\u200B';

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
            if (x.Text == y.Text)
            {
                return x.Icon.CompareTo(y.Icon);
            }

            if (x.Text == this.Target)
            {
                return 1;
            }

            if (y.Text == this.Target)
            {
                return -1;
            }

            var strCompare = this.CompareString(x.Text, y.Text);
            return strCompare == 0 ? x.Icon.CompareTo(y.Icon) : strCompare;
        }

        private int GetMatchesCount(string x)
        {
            var groups = GetGroups(x);
            return this.targetGroups.Count(t => groups.Any(g => string.Equals(g, t, StringComparison.InvariantCultureIgnoreCase)));
        }

        /// <summary>
        /// Compares the string.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>The relative comparison between 2 strings.</returns>
        private int CompareString(string x, string y)
        {
            var result = this.GetMatchesCount(x).CompareTo(this.GetMatchesCount(y));
            return result == 0 ? string.CompareOrdinal(x, y) : result;
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
    }
}
