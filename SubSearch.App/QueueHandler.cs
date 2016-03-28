// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueHandler.cs" company="">
//   
// </copyright>
// <summary>
//   The queue handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using SubSearch.Data;
using SubSearch.Resources;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace SubSearch.WPF
{
    using System.Diagnostics;

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

        /// <summary>Initializes a new instance of the <see cref="QueueHandler" /> class.</summary>
        /// <param name="arguments">The arguments.</param>
        internal QueueHandler(params string[] arguments)
        {
            if (arguments == null || arguments.Length < 1)
            {
                throw new ArgumentException("argument");
            }

            id = arguments[0];
            for (var i = 1; i < arguments.Length; i++)
            {
                var arg = arguments[i];
                if (arg == "/K")
                {
                    this.keepQueueFile = true;
                }
            }
        }

        /// <summary>
        /// Processes this instance.
        /// </summary>
        /// <returns>The result.</returns>
        internal QueryResult Process()
        {
            if (string.IsNullOrEmpty(this.id) || !File.Exists(this.id))
            {
                return QueryResult.Fatal;
            }

            int success = 0, fail = 0;
            using (var fileReader = new StreamReader(id))
            {
                var languageStr = fileReader.ReadLine();
                Language language;
                Enum.TryParse(languageStr, out language);
                LocalizationManager.Initialize(language);

                using (var viewHandler = fileReader.ReadLine() == Constants.SilentModeIdentifier ? new SilentView() : new WpfView())
                {
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

                        this.ProcessRequest(targets, viewHandler, language, ref success, ref fail);
                    }
                }
            }

            if (!this.keepQueueFile)
            {
                File.Delete(id);
            }

            QueryResult result = QueryResult.Cancelled;
            if (success > 0)
            {
                result = QueryResult.Success;
            }

            if (fail > 0)
            {
                if (result == QueryResult.Success)
                {
                    result |= QueryResult.Failure;
                }
                else
                {
                    result = QueryResult.Failure;
                }
            }
            return result;
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="targets">The targets.</param>
        /// <param name="view">The view.</param>
        /// <param name="language">The language.</param>
        /// <param name="success">The success.</param>
        /// <param name="fail">The fail.</param>
        private void ProcessRequest(string[] targets, IView view, Language language, ref int success, ref int fail)
        {
            for (this.activeIndex = 0; this.activeIndex < targets.Length; this.activeIndex++)
            {
                try
                {
                    this.activeQuery = new SubSceneDb(targets[this.activeIndex], view, language);
                    view.ShowProgress(this.activeIndex, targets.Length);
                    var entryResult = this.activeQuery.Query();
                    if (entryResult == QueryResult.Success || entryResult == QueryResult.Skipped)
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
                    Trace.TraceError(ex.ToString());
                }
            }
        }

        /// <summary>The handle action.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="actionName">The action name.</param>
        private void HandleAction(IView sender, object parameter, string actionName)
        {
            if (this.activeQuery != null)
            {
                if (actionName == CustomActions.CustomQuery && parameter != null)
                {
                    this.activeQuery.Title = parameter.ToString();
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
                    this.activeQuery.DownloadSubtitle(itemData.Tag as string);
                }
                else if (actionName == CustomActions.Play)
                {
                    try
                    {
                        if (File.Exists(this.activeQuery.FilePath))
                        {
                            System.Diagnostics.Process.Start(this.activeQuery.FilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        sender.Notify(string.Format("Failed to play {0}: {1}", this.activeQuery.FilePath, ex.Message));
                    }
                }
            }

            if (actionName == CustomActions.Close)
            {
                sender.Continue();
            }
        }

        /// <summary>The on view custom action requested.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="actionNames">The action names.</param>
        private void OnViewHandlerCustomActionRequested(IView sender, object parameter,
            params string[] actionNames)
        {
            foreach (var actionName in actionNames)
            {
                this.HandleAction(sender, parameter, actionName);
            }
        }
    }
}