// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LocalizationManager.cs" company="">
//   
// </copyright>
// <summary>
//   The localization manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SubSearch.Resources
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// The <see cref="Localizer"/> class.
    /// </summary>
    public static class Localizer
    {
        /// <summary>
        /// Initializes the specified language.
        /// </summary>
        /// <param name="language">The language.</param>
        public static void Initialize(Language language)
        {
            var matchCulture = language.GetCultureInfo();
            if (matchCulture != null)
            {
                Thread.CurrentThread.CurrentUICulture = matchCulture;
                CultureInfo.DefaultThreadCurrentUICulture = matchCulture;
            }
        }

        /// <summary>
        /// Gets culture information.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns>The culture information.</returns>
        public static CultureInfo GetCultureInfo(this Language language)
        {
            var allCultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
            var matchCulture = allCultures.FirstOrDefault(c => c.EnglishName.Equals(language.ToString(), StringComparison.InvariantCultureIgnoreCase));
            return matchCulture;
        }

        /// <summary>Localizes the object value.</summary>
        /// <param name="target">The target object.</param>
        /// <returns>The localized object value.</returns>
        public static string Localize(this object target)
        {
            if (target == null)
            {
                return string.Empty;
            }

            var objectValue = target.ToString();
            try
            {
                return Literals.ResourceManager.GetString(objectValue);
            }
            catch (Exception)
            {
                return objectValue;
            }
        }
    }
}