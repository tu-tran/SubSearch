namespace SubSearch.WPF
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using SubSearch.Data;

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
        /// <returns>The <see cref="int" />.</returns>
        internal int Process()
        {
            if (string.IsNullOrEmpty(this.ID) || !File.Exists(this.ID))
            {
                return -1;
            }

            int success = 0, fail = 0;
            using (var fileReader = new StreamReader(this.ID))
            {
                IViewHandler viewHandler;
                if (fileReader.ReadLine() == "__SILENT__")
                {
                    viewHandler = new SilentViewHandler();
                }
                else
                {
                    viewHandler = new WpfViewHandler();
                }

                using (viewHandler)
                {
                    ThreadPool.QueueUserWorkItem(
                        o =>
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
                                                .Where(
                                                    f =>
                                                    ShellExtension.FileAssociations.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));
                                        foreach (var file in files)
                                        {
                                            var entryResult = new SubSceneDb(file, viewHandler).Query();
                                            if (entryResult > 0)
                                            {
                                                success += 1;
                                            }
                                            else if (entryResult < 0)
                                            {
                                                fail += 1;
                                            }
                                        }
                                    }
                                }

                                viewHandler.Dispose();
                                viewHandler = null;
                            });

                    viewHandler.Start();
                }
            }

            if (!this.keepQueueFile)
            {
                File.Delete(this.ID);
            }

            return success > 0 ? 1 : (fail > 0 ? -1 : 0);
        }
    }
}