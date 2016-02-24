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
    public enum Icon
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
        /// <summary>Initializes a new instance of the <see cref="ItemData"/> class.</summary>
        /// <param name="text">The text.</param>
        /// <param name="tag">The tag.</param>
        public ItemData(string text, object tag)
        {
            this.Text = text;
            this.Tag = tag;
        }

        /// <summary>Gets or sets the description.</summary>
        public string Description { get; set; }

        /// <summary>Gets or sets the icon.</summary>
        public Icon Icon { get; set; }

        /// <summary>Gets or sets the tag.</summary>
        public object Tag { get; set; }

        /// <summary>Gets or sets the name.</summary>
        public string Text { get; set; }
    }
}