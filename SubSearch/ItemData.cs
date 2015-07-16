namespace SubSearch
{
    public class ItemData
    {
        public string Name { get; set; }

        public object Tag { get; set; }

        public ItemData(string name, object tag)
        {
            this.Name = name;
            this.Tag = tag;
        }
    }
}
