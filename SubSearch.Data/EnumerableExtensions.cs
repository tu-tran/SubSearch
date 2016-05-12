namespace SubSearch.Data
{
    using System.Collections;

    /// <summary>
    /// The <see cref="EnumerableExtensions"/> class.
    /// </summary>
    public static class EnumerableExtensions
    {
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
