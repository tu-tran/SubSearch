namespace SubSearch.Data
{
    using System;

    /// <summary>
    /// The <see cref="QueryResult"/> enum.
    /// </summary>
    [Flags]
    public enum QueryResult
    {
        /// <summary>The fatal.</summary>
        Fatal = 0,

        /// <summary>The cancelled.</summary>
        Cancelled = 1 << 1,

        /// <summary>The skipped.</summary>
        Skipped = 1 << 2,

        /// <summary>The success.</summary>
        Success = 1 << 3,

        /// <summary>The failure.</summary>
        Failure = 1 << 4
    }

    /// <summary>The <see cref="QueryResult{TData}" /> class.</summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    public struct QueryResult<TData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResult"/> class.
        /// </summary>
        public QueryResult(QueryResult status, TData data)
            : this()
        {
            this.Status = status;
            this.Data = data;
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        public QueryResult Status { get; private set; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        public TData Data { get; private set; }
    }
}
