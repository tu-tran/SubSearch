namespace SubSearch.WPF
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using SubSearch.Resources;

    /// <summary>The queue handler.</summary>
    internal sealed class QueueHandler
    {
        /// <summary>The id.</summary>
        private readonly string id;

        /// <summary>Keeps the queue file after processing.</summary>
        private readonly bool keepQueueFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueHandler"/> class.
        /// </summary>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        internal QueueHandler(params string[] arguments)
        {
            if (arguments == null || arguments.Length < 1)
            {
                throw new ArgumentException("argument");
            }

            this.id = arguments[0];
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
            if (string.IsNullOrEmpty(this.id) || !File.Exists(this.id))
            {
                return -1;
            }

            int success = 0, fail = 0;
            var fileReader = new StreamReader(this.id);
            var languageStr = fileReader.ReadLine();
            Language language;
            Enum.TryParse(languageStr, out language);
            var viewHandler = fileReader.ReadLine() == "__SILENT__" ? new SilentViewHandler() : new WpfViewHandler();
            ThreadPool.QueueUserWorkItem(
                o =>
                {
                    LocalizationManager.Initialize(language);
                    string line;
                    while ((line = fileReader.ReadLine()) != null)
                    {
                        IEnumerable<string> targets = null;
                        if (File.Exists(line))
                        {
                            targets = new[] { line };
                        }
                        else if (Directory.Exists(line))
                        {
                            targets =
                                Directory.EnumerateFiles(line, "*.*", SearchOption.AllDirectories)
                                    .Where(f => ShellExtension.FileAssociations.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));
                        }

                        if (targets == null)
                        {
                            continue;
                        }

                        foreach (var file in targets)
                        {
                            var entryResult = new SubSceneDb(file, viewHandler, language).Query();
                            if (entryResult > 0)
                            {
                                success += 1;
                            }
                            else if (entryResult < 0)
                            {
                                fail += 1;
                            }
                            else if (entryResult == 0)
                            {
                                break; // Users cancel
                            }
                        }
                    }

                    fileReader.Dispose();
                    viewHandler.Dispose();
                    viewHandler = null;
                });

            viewHandler.Start();
            if (!this.keepQueueFile)
            {
                File.Delete(this.id);
            }

            return success > 0 ? 1 : (fail > 0 ? -1 : 0);
        }
    }
}