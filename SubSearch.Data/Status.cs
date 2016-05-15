namespace SubSearch.Data
{
    using System;

    /// <summary>
    /// The <see cref="Status"/> enum.
    /// </summary>
    [Flags]
    public enum Status
    {
        /// <summary>
        /// The success.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The cancelled.
        /// </summary>
        Cancelled = 1 << 1,

        /// <summary>
        /// The skipped.
        /// </summary>
        Skipped = 1 << 2,

        /// <summary>
        /// The failure.
        /// </summary>
        Failure = 1 << 3,

        /// <summary>
        /// The fatal.
        /// </summary>
        Fatal = 1 << 4
    }

    /// <summary>
    /// The result.
    /// </summary>
    public struct Result
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> struct.
        /// </summary>
        /// <param name="status">The result.</param>
        /// <param name="message">The message.</param>
        public Result(Status status, string message)
            : this()
        {
            this.Status = status;
            this.Message = message;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the result.
        /// </summary>
        public Status Status { get; private set; }
    }

    /// <summary>
    /// The <see cref="QueryResult{TData}" /> class.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    public struct QueryResult<TData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResult{TData}" /> class.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="data">The data.</param>
        public QueryResult(Result result, TData data)
            : this()
        {
            this.Result = result;
            this.Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResult{TData}" /> class.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="data">The data.</param>
        /// <param name="message">The message.</param>
        public QueryResult(Status status, TData data, string message = null)
            : this(new Result(status, message), data)
        {
        }

        /// <summary>
        /// Gets the result.
        /// </summary>
        public Result Result { get; private set; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        public TData Data { get; private set; }

        /// <summary>
        /// Gets the status.
        /// </summary>
        public Status Status
        {
            get { return this.Result.Status; }
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message
        {
            get { return this.Result.Message; }
        }
    }
}
