using System.Collections.Concurrent;

namespace SubSearch.Data.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    using SubSearch.Resources;

    /// <summary>
    /// The <see cref="AggregateDb"/> class.
    /// </summary>
    public class AggregateDb : SubtitleDbBase
    {
        /// <summary>
        /// The handlers.
        /// </summary>
        private static readonly List<ISubtitleDb> Handlers;

        /// <summary>
        /// Initializes the <see cref="AggregateDb"/> class.
        /// </summary>
        static AggregateDb()
        {
            Handlers = new List<ISubtitleDb>();
            var handlersPath = Path.Combine(Path.GetDirectoryName((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location) ?? string.Empty, "Handlers");
            AppDomain.CurrentDomain.AppendPrivatePath(handlersPath);
            if (!Directory.Exists(handlersPath))
            {
                Trace.TraceError("Invalid handlers path: " + handlersPath);
                return;
            }

            string[] pluginFiles = Directory.GetFiles(handlersPath, "Handlers.*.dll");
            try
            {
                foreach (var file in pluginFiles)
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(file);
                        var types = assembly.GetTypes().Where(t => typeof(ISubtitleDb).IsAssignableFrom(t));
                        foreach (var type in types)
                        {
                            try
                            {
                                var db = (ISubtitleDb)Activator.CreateInstance(type);
                                Handlers.Add(db);
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError("Failed to create db {0}: {1}", type, ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Failed to load {0}: {1}", file, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to load DB: {0}", ex);
            }
        }

        /// <summary>
        /// Downloads the specified movie file.
        /// </summary>
        /// <param name="releaseFile">The movie file.</param>
        /// <param name="subtitle">The subtitle.</param>
        public override void Download(string releaseFile, Subtitle subtitle)
        {
            if (subtitle.DataSource != null && subtitle.DataSource != this)
            {
                subtitle.DataSource.Download(releaseFile, subtitle);
            }
            else
            {
                base.Download(releaseFile, subtitle);
            }
        }

        /// <summary>
        /// Gets subtitles meta.
        /// </summary>
        /// <param name="releaseName">Name of the release.</param>
        /// <param name="language"></param>
        /// <returns>The query result.</returns>
        public override QueryResult<Subtitles> GetSubtitlesMeta(string releaseName, Language language)
        {
            var subtitles = new Subtitles();
            var statuses = new ConcurrentBag<Status>();
            Status status = Status.Fatal;
            var tasks = new List<Task>(Handlers.Count);
            var sb = new StringBuilder();

            if (Handlers.Count > 0)
            {
                foreach (var subtitleDb in Handlers)
                {
                    var db = subtitleDb;
                    var dbTask = Task.Run(
                        () =>
                        {
                            Status dbStatus;
                            try
                            {
                                var meta = db.GetSubtitlesMeta(releaseName, language);
                                dbStatus = meta.Status;
                                if (dbStatus == Status.Success && meta.Data != null && meta.Data.Count > 0)
                                {
                                    subtitles.AddRange(meta.Data);
                                }
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError(Literals.Data_Failed_to_get_subtitles_meta, releaseName, db, ex);
                                dbStatus = Status.Fatal;
                                sb.AppendLine(string.Format(Literals.Data_Failed_to_get_subtitles_meta, releaseName, db, ex.Message));
                            }

                            statuses.Add(dbStatus);
                        });

                    tasks.Add(dbTask);
                }

                Task.WaitAll(tasks.ToArray());
                status = statuses.Any(s => s == Status.Success) ? Status.Success : Status.Failure;
            }

            return new QueryResult<Subtitles>(status, subtitles, sb.ToString());
        }
    }
}
