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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using SubSearch.Data;
    using SubSearch.Data.Handlers;
    using SubSearch.Resources;
    using SubSearch.WPF.Controllers;
    using SubSearch.WPF.Views;

    /// <summary>
    /// The <see cref="QueueHandler"/> class.
    /// </summary>
    internal sealed class QueueHandler
    {
        /// <summary>The id.</summary>
        private readonly string id;

        /// <summary>Keeps the queue file after processing.</summary>
        private readonly bool keepQueueFile;

        /// <summary>The active index.</summary>
        private int activeIndex;

        /// <summary>
        /// The active controller.
        /// </summary>
        private MainViewController activeController;

        /// <summary>Initializes a new instance of the <see cref="QueueHandler" /> class.</summary>
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

        /// <summary>Processes this instance.</summary>
        /// <returns>The result.</returns>
        internal QueryResult Process()
        {
            if (string.IsNullOrEmpty(this.id) || !File.Exists(this.id))
            {
                return QueryResult.Fatal;
            }

            int success = 0, fail = 0;
            using (var fileReader = new StreamReader(this.id))
            {
                var languageStr = fileReader.ReadLine();
                Language language;
                Enum.TryParse(languageStr, out language);
                AppContext.Global.Language = language;

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

                        this.ProcessRequest(targets, viewHandler, ref success, ref fail);
                    }
                }
            }

            if (!this.keepQueueFile)
            {
                File.Delete(this.id);
            }

            var result = QueryResult.Cancelled;
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

        /// <summary>Processes the request.</summary>
        /// <param name="targets">The targets.</param>
        /// <param name="view">The view.</param>
        /// <param name="success">The success.</param>
        /// <param name="fail">The fail.</param>
        private void ProcessRequest(IReadOnlyList<string> targets, IView view, ref int success, ref int fail)
        {
            for (this.activeIndex = 0; this.activeIndex < targets.Count; this.activeIndex++)
            {
                try
                {
                    var currentFile = targets[this.activeIndex];
                    var fileSizeLimit = 10 * 1024 * 1024;
                    var fileInfo = new FileInfo(currentFile);
                    if (!fileInfo.Exists || fileInfo.Length < fileSizeLimit)
                    {
                        continue;
                    }

                    this.activeController = new MainViewController(currentFile, view, new AggregateDb());
                    view.ShowProgress(this.activeIndex, targets.Count);
                    var entryResult = this.activeController.Query();
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
            if (this.activeController != null)
            {
                if (actionName == CustomActions.CustomQuery && parameter != null)
                {
                    this.activeController.Title = Path.GetFileNameWithoutExtension(parameter.ToString());
                    this.activeIndex--;
                    sender.Continue();
                    return;
                }

                var itemData = parameter as Subtitle;
                if (itemData == null)
                {
                    return;
                }

                if (actionName == CustomActions.DownloadSubtitle)
                {
                    this.activeController.Download(itemData);
                }
                else if (actionName == CustomActions.Play)
                {
                    try
                    {
                        if (File.Exists(this.activeController.FilePath))
                        {
                            System.Diagnostics.Process.Start(this.activeController.FilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        sender.Notify(string.Format("Failed to play {0}: {1}", this.activeController.FilePath, ex.Message));
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
        private void OnViewHandlerCustomActionRequested(IView sender, object parameter, params string[] actionNames)
        {
            foreach (var actionName in actionNames)
            {
                this.HandleAction(sender, parameter, actionName);
            }
        }
    }
}