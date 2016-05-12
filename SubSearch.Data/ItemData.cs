// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ItemData.cs" company="">
//   
// </copyright>
// <summary>
//   The icon.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SubSearch.Data
{
    /// <summary>The icon.</summary>
    public enum Rating
    {
        /// <summary>The dot.</summary>
        Dot,

        /// <summary>The neutral.</summary>
        Neutral,

        /// <summary>The negative.</summary>
        Negative,

        /// <summary>The positive.</summary>
        Positive
    }

    /// <summary>The item data.</summary>
    public class ItemData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemData"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="rating">The rating.</param>
        /// <param name="tag">The tag.</param>
        public ItemData(string name, string description, Rating rating = Rating.Neutral, object tag = null)
        {
            this.Name = name;
            this.Description = description;
            this.Rating = rating;
            this.Tag = tag;
        }
        
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the rating.
        /// </summary>
        public Rating Rating { get; private set; }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        public object Tag { get; private set; }
    }
}