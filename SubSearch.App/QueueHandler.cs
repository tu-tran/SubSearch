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

        /// <summary>Keeps the queue file after processing.</summary>
        private readonly bool keepQueueFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueHandler"/> class.
        /// </summary>
        /// <param name="queueId">
        /// The queue id.
        /// </param>
        internal QueueHandler(params string[] arguments)
        {
            if (arguments == null || arguments.Length < 1)
            {
                throw new ArgumentException("argument");
            }

            this.ID = arguments[0];
            for (var i = 1; i < arguments.Length; i++)
            {
                var arg = arguments[i];
                if (arg == "/K")
                {
                    this.keepQueueFile = true;
                }
            }
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
                using (var viewHandler = new WPFViewHandler())
                {
                    string line;
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

            if (!this.keepQueueFile)
            {
                File.Delete(this.ID);
            }

            return true;
        }
    }
}