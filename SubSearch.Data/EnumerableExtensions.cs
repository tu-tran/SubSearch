namespace SubSearch.Data
{
    using System.Collections;

    /// <summary>
    /// The <see cref="EnumerableExtensions"/> class.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Determines whether the <paramref name="target"/> has any element.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns>A value indicating whether the <paramref name="target"/> has any element.</returns>
        public static bool HasAny(this IEnumerable target)
        {
            if (target != null)
            {
                foreach (var item in target)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
