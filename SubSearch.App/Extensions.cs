// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="">
//   
// </copyright>
// <summary>
//   The utilities.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SubSearch.WPF
{
    using System.Windows.Forms;

    /// <summary>The utilities.</summary>
    internal static class Utils
    {
        /// <summary>The get active screen.</summary>
        /// <returns>The <see cref="Screen" />.</returns>
        public static Screen GetActiveScreen()
        {
            var mousePosition = Control.MousePosition;
            return Screen.FromPoint(mousePosition);
        }
    }
}