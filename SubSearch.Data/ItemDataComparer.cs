using System;
using System.Collections.Generic;
using System.Linq;

namespace SubSearch.Data
{
    /// <summary>
    ///     The item data comparer.
    /// </summary>
    public class ItemDataComparer : IComparer<ItemData>, IComparer<Subtitle>
    {
        private static readonly string[][] EquivalentFormats =
        {
            new[] {"x264", "OpenH264", "x265"},
            new[]
            {
                "BDRip", "BluRay", "Blu-Ray", "BRip", "BRRip",
                "WEBDL", "WEB-DL", "WEB-DLRip", "WEBRip", "WEB-Rip", "720p", "1080p", "2160p", "4K"
            }
        };

        public ItemDataComparer(string releaseName)
        {
            Release = new ReleaseInfo(releaseName);
        }

        public ReleaseInfo Release { get; }

        /// <summary>
        ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        ///     A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />.
        ///     Less than zero: <paramref name="x" /> is less than <paramref name="y" />.
        ///     Zero: <paramref name="x" /> equals <paramref name="y" />.
        ///     Greater than zero: <paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        public int Compare(ItemData x, ItemData y)
        {
            return CompareItem(x, y);
        }

        /// <summary>
        ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        ///     A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in
        ///     the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero
        ///     <paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than
        ///     <paramref name="y" />.
        /// </returns>
        public int Compare(Subtitle x, Subtitle y)
        {
            return CompareItem(x, y);
        }

        /// <summary>
        ///     Gets matches count.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The number of matches count.</returns>
        public int GetMatchesPoints(ReleaseInfo input)
        {
            var points = 0;
            var fullGroup = false;
            if (!string.IsNullOrWhiteSpace(Release.Title) && !string.IsNullOrWhiteSpace(input.Title))
            {
                if (Release.Title == input.Title) points += 20;
                if (!string.IsNullOrWhiteSpace(Release.Episode) && Release.Episode == input.Episode) points += 10;
            }
            else
            {
                fullGroup = true;
            }

            var curGroups = GetMatchGroups(Release, fullGroup);
            var targetGroups = GetMatchGroups(input, fullGroup);
            points += GetPoints(curGroups, targetGroups);

            foreach (var equivalentFormat in EquivalentFormats)
            {
                var match = targetGroups.FirstOrDefault(i => equivalentFormat.Contains(i, StringComparer.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(match) && !curGroups.Contains(match, StringComparer.OrdinalIgnoreCase))
                {
                    points += GetPoints(curGroups, equivalentFormat, 1);
                }
            }

            return points;
        }

        private static bool IsSame(string a, string b)
        {
            return string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
        }

        private static int GetPoints(ICollection<string> curGroups, ICollection<string> targetGroups, int matchPoints = 2)
        {
            return curGroups.Sum(
                t =>
                {
                    var match = targetGroups.FirstOrDefault(g =>
                                    IsSame(g,t)) ??
                                string.Empty;
                    return string.IsNullOrWhiteSpace(match) ? 0 : matchPoints;
                });
        }

        private static ISet<string> GetMatchGroups(ReleaseInfo info, bool full = false)
        {
            var result = new HashSet<string>();
            if (full)
            {
                AddGroups(info.NormalizedFullName, result);
            }
            else
            {
                AddGroups(info.Format, result);
                AddGroups(info.Extra, result);
                AddGroups(info.Year, result);
            }

            return result;
        }

        private static void AddGroups(string field, ICollection<string> target)
        {
            if (string.IsNullOrWhiteSpace(field)) return;
            foreach (var e in field.Split(new[] {ReleaseInfo.Separator}, StringSplitOptions.RemoveEmptyEntries))
            {
                target.Add(e);
            }
        }

        /// <summary>
        ///     Compares the string.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>The relative comparison between 2 strings.</returns>
        private int CompareRelease(ReleaseInfo x, ReleaseInfo y)
        {
            var result = GetMatchesPoints(x).CompareTo(GetMatchesPoints(y));
            return result == 0 ? string.CompareOrdinal(x.NormalizedFullName, y.NormalizedFullName) : result;
        }

        /// <summary>
        ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        ///     A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />.
        ///     Less than zero: <paramref name="x" /> is less than <paramref name="y" />.
        ///     Zero: <paramref name="x" /> equals <paramref name="y" />.
        ///     Greater than zero: <paramref name="x" /> is greater than <paramref name="y" />.
        /// </returns>
        private int CompareItem(ItemData x, ItemData y)
        {
            if (x.Name == y.Name) return x.Rating.CompareTo(y.Rating);

            var xInfo = new ReleaseInfo(x.Name);
            var yInfo = new ReleaseInfo(y.Name);
            if (xInfo.NormalizedFullName == Release.NormalizedFullName) return 1;
            if (yInfo.NormalizedFullName == Release.NormalizedFullName) return -1;
            var releaseCompare = CompareRelease(xInfo, yInfo);
            var ratingCompare = releaseCompare == 0 ? x.Rating.CompareTo(y.Rating) * 3 : 0;
            return releaseCompare + ratingCompare;
        }
    }
}