// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceExtensions.cs" company="">
//   
// </copyright>
// <summary>
//   The resource extensions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SubSearch.Resources
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Threading;
    using System.Windows.Media.Imaging;

    /// <summary>The resource extensions.</summary>
    public static class ResourceExtensions
    {
        /// <summary>Gets the icon URI.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The URI.</returns>
        public static Uri GetResourceUri(string name)
        {
            var assemblyName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().ManifestModule.Name);
            return new Uri("pack://application:,,,/" + assemblyName + ";component/resources/" + name);
        }

        /// <summary>Tries to get the image from the application resource with the specific <paramref name="resourceName"/>.</summary>
        /// <param name="resourceName">The resource name.</param>
        /// <returns>The image from the application resource with the specific <paramref name="resourceName"/>.</returns>
        public static BitmapImage TryGetImageResource(this string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                return null;
            }

            try
            {
                var resourceUri = GetResourceUri(resourceName);
                return new BitmapImage(resourceUri);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to get resource: " + ex);
            }

            return null;
        }

        /// <summary>Enumerates all resource keys within this assembly.</summary>
        /// <returns>Collection of keys.</returns>
        private static IEnumerable<object> GetResourcePaths()
        {
            var currentAssembly = Assembly.GetExecutingAssembly();

            var culture = Thread.CurrentThread.CurrentCulture;
            var resourceName = currentAssembly.GetName().Name + ".g";
            var resourceManager = new ResourceManager(resourceName, currentAssembly);

            try
            {
                var resourceSet = resourceManager.GetResourceSet(culture, true, true);

                foreach (DictionaryEntry resource in resourceSet)
                {
                    yield return resource.Key;
                }
            }
            finally
            {
                resourceManager.ReleaseAllResources();
            }
        }
    }
}