// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SilentView.cs" company="">
//   
// </copyright>
// <summary>
//   The silent view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SubSearch.WPF.Views
{
    using System.Collections.Generic;
    using System.Linq;

    using SubSearch.Data;

    /// <summary>The silent view.</summary>
    internal sealed class SilentView : WpfView
    {
        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        /// <returns>The query result.</returns>
        public override QueryResult<ItemData> GetSelection(ICollection<ItemData> data, string title, string status)
        {
            if (data == null || !data.Any())
            {
                return new QueryResult<ItemData>(QueryResult.Failure, null);
            }

            return new QueryResult<ItemData>(QueryResult.Success, data.FirstOrDefault());
        }
    }
}