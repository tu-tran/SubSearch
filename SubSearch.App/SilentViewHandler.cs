// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SilentViewHandler.cs" company="">
//   
// </copyright>
// <summary>
//   The silent view handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SubSearch.WPF
{
    using System.Collections.Generic;
    using System.Linq;

    using SubSearch.Data;

    /// <summary>The silent view handler.</summary>
    internal sealed class SilentViewHandler : WpfViewHandler
    {
        /// <summary>The get selection.</summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        /// <returns>The <see cref="ItemData"/>.</returns>
        public override ItemData GetSelection(ICollection<ItemData> data, string title, string status)
        {
            return data.FirstOrDefault();
        }
    }
}