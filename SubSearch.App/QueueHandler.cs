// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueHandler.cs" company="">
//   
// </copyright>
// <summary>
//   The queue handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CommandLine;

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
        /// <summary>The jobPath.</summary>
        private string jobPath;

        /// <summary>Keeps the queue file after processing.</summary>
        private bool keepQueueFile;

        /// <summary>The active index.</summary>
        private int activeIndex;

        private bool quiet;

        private string[] initTargets;

        /// <summary>
        /// The active controller.
        /// </summary>
        private MainViewController activeController;

        /// <summary>Initializes a new instance of the <see cref="QueueHandler" /> class.</summary>
        /// <param name="arguments">The arguments.</param>
        internal QueueHandler(params string[] arguments)
        {
            Parser.Default.ParseArguments<CommandLineArgs>(arguments).WithParsed(a =>
            {
                this.jobPath = a.JobPath;
                this.initTargets = a.Targets?.ToArray();
                this.keepQueueFile = a.KeepJobFile;
                this.quiet = a.Quiet;
            });
        }

        /// <summary>Processes this instance.</summary>
        /// <returns>The result.</returns>
        internal Status Process()
        {
            int success = 0, fail = 0;
            string viewMode = null;
            var targets = new List<string>();
            if (this.initTargets != null && initTargets.Length > 0)
            {
                targets.AddRange(this.GetTarget(this.initTargets));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(this.jobPath) || !File.Exists(this.jobPath))
                {
                    return Status.Fatal;
                }

                using (var fileReader = new StreamReader(this.jobPath))
                {
                    var languageStr = fileReader.ReadLine();
                    if (Enum.TryParse(languageStr, out Language language))
                    {
                        AppContext.Global.Language = language;
                    }

                    viewMode = fileReader.ReadLine();
                    string line;
                    while ((line = fileReader.ReadLine()) != null)
                    {
                        targets.AddRange(this.GetTarget(new[] { line }));
                    }
                }

                if (!this.keepQueueFile && File.Exists(this.jobPath))
                {
                    File.Delete(this.jobPath);
                }
            }

            var isQuiet = string.IsNullOrWhiteSpace(viewMode) ? this.quiet : viewMode == Constants.SilentModeIdentifier;
            using (var viewHandler = isQuiet
                ? new SilentView()
                : new WpfView())
            {
                viewHandler.CustomActionRequested += this.OnViewHandlerCustomActionRequested;
                this.ProcessRequest(targets.Distinct(StringComparer.OrdinalIgnoreCase).ToList(), viewHandler, ref success, ref fail);
            }

            var result = Status.Cancelled;
            if (success > 0)
            {
                result = Status.Success;
            }

            if (fail > 0)
            {
                if (result == Status.Success)
                {
                    result |= Status.Failure;
                }
                else
                {
                    result = Status.Failure;
                }
            }
            return result;
        }

        /// <summary>Processes the request.</summary>
        /// <param name="targets">The initTargets.</param>
        /// <param name="view">The view.</param>
        /// <param name="success">The success.</param>
        /// <param name="fail">The fail.</param>
        private void ProcessRequest(IReadOnlyList<string> targets, IView view, ref int success, ref int fail)
        {
            var retry = false;
            for (this.activeIndex = 0; this.activeIndex < targets.Count; this.activeIndex++)
            {
                var currentIndex = this.activeIndex;
                try
                {
                    var currentFile = targets[this.activeIndex];
                    if (retry)
                    {
                        retry = false;
                    }
                    else
                    {
                        this.activeController = new MainViewController(currentFile, view, new AggregateDb());
                    }

                    view.ShowProgress(this.activeIndex, targets.Count);
                    var entryResult = this.activeController.Query();

                    retry = this.activeIndex != currentIndex;
                    if (entryResult == Status.Success || entryResult == Status.Skipped)
                    {
                        success++;
                    }
                    else if (entryResult == Status.Failure)
                    {
                        fail++;
                    }
                    else if (entryResult == Status.Cancelled)
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
                else if (actionName == CustomActions.ChangeActiveFile)
                {
                    this.activeController.FilePath = parameter?.ToString();
                    this.activeController.Query();
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

        private IEnumerable<string> GetTarget(IEnumerable<string> paths)
        {
            foreach (var path in (paths ?? new string[0]))
            {
                if (Directory.Exists(path))
                {
                    var files = ShellExtension.GetSupportedFiles(path);
                    foreach (var f in files)
                    {
                        yield return f;

                    }
                }

                if (File.Exists(path))
                {
                    yield return path;
                }

            }
        }
    }
}