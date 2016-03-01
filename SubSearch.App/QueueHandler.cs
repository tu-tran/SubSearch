// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueHandler.cs" company="">
//   
// </copyright>
// <summary>
//   The queue handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace SubSearch.WPF
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using SubSearch.Data;
    using SubSearch.Resources;

    /// <summary>The queue handler.</summary>
    internal sealed class QueueHandler
    {
        /// <summary>The id.</summary>
        private readonly string id;

        /// <summary>Keeps the queue file after processing.</summary>
        private readonly bool keepQueueFile;

        /// <summary>The active index.</summary>
        private int activeIndex;

        /// <summary>The active query.</summary>
        private ISubtitleDb activeQuery;

        /// <summary>The active queue.</summary>
        private List<ISubtitleDb> activeQueue;

        /// <summary>Initializes a new instance of the <see cref="QueueHandler"/> class.</summary>
        /// <param name="arguments">The arguments.</param>
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
            var viewHandler = fileReader.ReadLine() == Constants.SilentModeIdentifier ? new SilentViewHandler() : new WpfViewHandler();
            ThreadPool.QueueUserWorkItem(
                o =>
                    {
                        LocalizationManager.Initialize(language);
                        viewHandler.CustomActionRequested += this.OnViewHandlerCustomActionRequested;
                        string line;
                        while ((line = fileReader.ReadLine()) != null)
                        {
                            string[] targets = null;
                            if (File.Exists(line))
                            {
                                targets = new[] { line };
                            }
                            else if (Directory.Exists(line))
                            {
                                targets =
                                    Directory.EnumerateFiles(line, "*.*", SearchOption.AllDirectories)
                                        .Where(f => ShellExtension.FileAssociations.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                                        .ToArray();
                            }

                            if (targets == null)
                            {
                                continue;
                            }

                            var handler = viewHandler;
                            this.activeQueue = targets.Select<string, ISubtitleDb>(f => new SubSceneDb(f, handler, language)).ToList();

                            for (this.activeIndex = 0; this.activeIndex < this.activeQueue.Count; this.activeIndex++)
                            {
                                try
                                {
                                    this.activeQuery = this.activeQueue[this.activeIndex];
                                    viewHandler.TargetFile = this.activeQuery.FilePath;
                                    viewHandler.ShowProgress(this.activeIndex, this.activeQueue.Count);
                                    var entryResult = this.activeQuery.Query();
                                    if (entryResult == QueryResult.Success)
                                    {
                                        success++;
                                    }
                                    else if (entryResult == QueryResult.Failure)
                                    {
                                        fail++;
                                    }
                                    else if (entryResult == QueryResult.Cancelled)
                                    {
                                        break; // Users cancel
                                    }
                                }
                                catch (Exception ex)
                                {
                                    fail++;
                                    Console.Error.WriteLine(ex);
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

        /// <summary>The handle action.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="actionName">The action name.</param>
        private void HandleAction(IViewHandler sender, object parameter, string actionName)
        {
            var file = sender.TargetFile;
            var language = this.activeQuery != null ? this.activeQuery.Language : Language.English;
            if (actionName == CustomActions.CustomQuery && parameter != null)
            {
                var query = this.activeQuery ?? new SubSceneDb(file, sender, language);
                query.Title = parameter.ToString();
                this.activeIndex--;
                sender.Continue();
                return;
            }

            var itemData = parameter as ItemData;
            if (itemData == null)
            {
                return;
            }

            if (actionName == CustomActions.DownloadSubtitle)
            {
                new SubSceneDb(file, sender, language).DownloadSubtitle(itemData.Tag as string);
            }
            else if (actionName == CustomActions.Play)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        System.Diagnostics.Process.Start(file);
                    }
                }
                catch (Exception ex)
                {
                    sender.Notify(string.Format("Failed to play {0}: {1}", file, ex.Message));
                }
            }
            else if (actionName == CustomActions.Close)
            {
                sender.Continue();
            }
        }

        /// <summary>The on view handler custom action requested.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="actionNames">The action names.</param>
        private void OnViewHandlerCustomActionRequested(IViewHandler sender, object parameter, params string[] actionNames)
        {
            foreach (var actionName in actionNames)
            {
                this.HandleAction(sender, parameter, actionName);
            }
        }
    }
}