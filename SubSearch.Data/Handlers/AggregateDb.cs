namespace SubSearch.Data.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
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
            var handlersPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? string.Empty, "Handlers");
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
        /// <returns>The query result.</returns>
        public override QueryResult Download(string releaseFile, Subtitle subtitle)
        {
            if (subtitle.DataSource != null && subtitle.DataSource != this)
            {
                return subtitle.DataSource.Download(releaseFile, subtitle);
            }

            return base.Download(releaseFile, subtitle);
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
            var statuses = new List<QueryResult>(Handlers.Count);
            QueryResult status = QueryResult.Success;
            var tasks = new List<Task>(Handlers.Count);

            if (Handlers.Count > 0)
            {
                foreach (var subtitleDb in Handlers)
                {
                    var db = subtitleDb;
                    var dbTask = Task.Run(
                        () =>
                        {
                            QueryResult dbStatus;
                            try
                            {
                                var meta = db.GetSubtitlesMeta(releaseName, language);
                                dbStatus = meta.Status;
                                if (dbStatus == QueryResult.Success && meta.Data != null && meta.Data.Count > 0)
                                {
                                    subtitles.AddRange(meta.Data);
                                }
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError("Failed to search for [{0}] from {1}: {2}", releaseName, db, ex);
                                dbStatus = QueryResult.Fatal;
                            }

                            statuses.Add(dbStatus);
                        });

                    tasks.Add(dbTask);
                }

                Task.WaitAll(tasks.ToArray());
                if (statuses.Distinct().Count() == statuses.Count)
                {
                    status = statuses.First();
                }
                else if (statuses.Any(s => s == QueryResult.Success))
                {
                    status = QueryResult.Success;
                }
                else if (statuses.Any(s => s == QueryResult.Fatal))
                {
                    status = QueryResult.Fatal;
                }
                else if (statuses.Any(s => s == QueryResult.Failure))
                {
                    status = QueryResult.Failure;
                }
            }

            return new QueryResult<Subtitles>(status, subtitles);
        }
    }
}
