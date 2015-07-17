namespace SubSearch.WPF
{
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>The queue handler.</summary>
    internal sealed class QueueHandler
    {
        /// <summary>The id.</summary>
        private readonly string ID;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueHandler"/> class.
        /// </summary>
        /// <param name="queueId">
        /// The queue id.
        /// </param>
        internal QueueHandler(string queueId)
        {
            this.ID = queueId;
        }

        /// <summary>The process.</summary>
        /// <returns>The <see cref="bool" />.</returns>
        internal bool Process()
        {
            if (string.IsNullOrEmpty(this.ID) || !File.Exists(this.ID))
            {
                return false;
            }

            using (var fileReader = new StreamReader(this.ID))
            {
                string line;
                using (var viewHandler = new WPFViewHandler())
                {
                    while ((line = fileReader.ReadLine()) != null)
                    {
                        if (File.Exists(line))
                        {
                            new SubSceneDb(line, viewHandler).Query();
                        }
                        else if (Directory.Exists(line))
                        {
                            var files =
                                Directory.EnumerateFiles(line, "*.*", SearchOption.AllDirectories)
                                    .Where(f => ShellExtension.FileAssociations.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));
                            foreach (var file in files)
                            {
                                new SubSceneDb(file, viewHandler).Query();
                            }
                        }
                    }
                }
            }

            File.Delete(this.ID);
            return true;
        }
    }
}