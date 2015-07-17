namespace SubSearch.Data
{
    /// <summary>The item data.</summary>
    public class ItemData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemData"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="tag">
        /// The tag.
        /// </param>
        public ItemData(string name, object tag)
        {
            this.Name = name;
            this.Tag = tag;
        }

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the tag.</summary>
        public object Tag { get; set; }
    }
}