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
            IViewHandler viewHandler = null;

            var workerThread = new Thread(() => this.ExecutionEngineException(ref viewHandler, ref success, ref fail));
            workerThread.SetApartmentState(ApartmentState.STA);
            workerThread.Start();

            Thread.Sleep(1000);
            while (viewHandler != null)
            {
                Thread.Sleep(1000);
            }

            return success > 0 ? 1 : (fail > 0 ? -1 : 0);
        }

        /// <summary>
        /// Executes the queue.
        /// </summary>
        /// <param name="viewHandler">The view handler.</param>
        /// <param name="success">The success count.</param>
        /// <param name="fail">The fail count.</param>
        private void ExecutionEngineException(ref IViewHandler viewHandler, ref int success, ref int fail)
        {
            using (var fileReader = new StreamReader(this.ID))
            {
                var languageStr = fileReader.ReadLine();
                Language language;
                Enum.TryParse(languageStr, out language);
                viewHandler = fileReader.ReadLine() == "__SILENT__" ? new SilentViewHandler() : new WpfViewHandler();

                using (viewHandler)
                {
                    string line;
                    while ((line = fileReader.ReadLine()) != null)
                    {
                        if (File.Exists(line))
                        {
                            new SubSceneDb(line, viewHandler, language).Query();
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
                    }
                }
            }

            if (!this.keepQueueFile)
            {
                File.Delete(this.ID);
            }

            viewHandler = null;
        }
    }
}