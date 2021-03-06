namespace SubSearch.WPF
{
    using SubSearch.Resources;

    /// <summary>
    /// The <see cref="AppContext"/> class.
    /// </summary>
    internal sealed class AppContext
    {
        /// <summary>
        /// The global app context.
        /// </summary>
        public static readonly AppContext Global = new AppContext();

        private Language language = Language.English;

        /// <summary>
        /// Prevents a default instance of the <see cref="AppContext"/> class from being created.
        /// </summary>
        private AppContext()
        {
        }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        public Language Language
        {
            get
            {
                return this.language;
            }

            set
            {
                this.language = value;
                Localizer.Initialize(value);
            }
        }

        /// <summary>
        /// Gets the localized language name.
        /// </summary>
        public string LocalizedLanguage
        {
            get
            {
                return this.Language.Localize();
            }
        }
    }
}
