// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InlineComparer{T}.cs" company="">
//   
// </copyright>
// <summary>
//   The inline comparer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SubSearch.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>The inline comparer.</summary>
    /// <typeparam name="T"></typeparam>
    public class InlineComparer<T> : IEqualityComparer<T>
    {
        /// <summary>The get equals.</summary>
        private readonly Func<T, T, bool> getEquals;

        /// <summary>The get hash code.</summary>
        private readonly Func<T, int> getHashCode;

        /// <summary>Initializes a new instance of the <see cref="InlineComparer{T}"/> class.</summary>
        /// <param name="equals">The equals.</param>
        /// <param name="hashCode">The hash code.</param>
        public InlineComparer(Func<T, T, bool> equals, Func<T, int> hashCode)
        {
            this.getEquals = equals;
            this.getHashCode = hashCode;
        }

        /// <summary>The equals.</summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Equals(T x, T y)
        {
            return this.getEquals(x, y);
        }

        /// <summary>The get hash code.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public int GetHashCode(T obj)
        {
            return this.getHashCode(obj);
        }
    }
}