// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SilentView.cs" company="">
//   
// </copyright>
// <summary>
//   The silent view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SubSearch.WPF
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SubSearch.Data;

    /// <summary>The silent view.</summary>
    internal sealed class SilentView : WpfView
    {
        /// <summary>The get selection.</summary>
        /// <param name="data">The data.</param>
        /// <param name="title">The title.</param>
        /// <param name="status">The status.</param>
        /// <returns>The <see cref="ItemData"/>.</returns>
        public override Tuple<QueryResult, ItemData> GetSelection(ICollection<ItemData> data, string title, string status)
        {
            if (data == null || !data.Any())
            {
                return Tuple.Create<QueryResult, ItemData>(QueryResult.Failure, null);
            }

            return Tuple.Create<QueryResult, ItemData>(QueryResult.Success, data.FirstOrDefault());
        }
    }
}