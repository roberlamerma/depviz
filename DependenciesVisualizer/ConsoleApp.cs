using CommandLine;
using DependenciesVisualizer.Connectors.Services;
using DependenciesVisualizer.Contracts;
using DependenciesVisualizer.Helpers;
using DependenciesVisualizer.Model;
using log4net;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Shields.GraphViz.Components;
using Shields.GraphViz.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DependenciesVisualizer
{
    class ConsoleApp
    {
        //Dictionary<int, DependencyItem> DependenciesModel { get; set; }

        IDependenciesService DependenciesService { get; set; }

        string ImagePath { get; set; }

        public ConsoleApp(string[] args)
        {
            Parser.Default.ParseArguments<ConsoleOptions>(args)
                .WithParsed<ConsoleOptions>(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed<ConsoleOptions>((errs) => HandleParseError(errs));
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            throw new NotImplementedException();
        }

        private void RunOptionsAndReturnExitCode(ConsoleOptions opts)
        {
            if (Path.IsPathRooted(opts.Output)) {
                this.ImagePath = opts.Output;
            } else
            {
                this.ImagePath = Path.Combine(Directory.GetCurrentDirectory(), opts.Output);
            }

            var logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            this.DependenciesService = new TfsService(logger);
            WeakEventManager<IDependenciesService, EventArgs>.AddHandler(this.DependenciesService, "DependenciesModelChanged", this.DependenciesModelChangedHandler);

            ((TfsService)this.DependenciesService).SetWorkItemStore(new Uri(opts.TfsUrl), opts.Project);
            //var queryHierarchy = tfsService.WorkItemStore.Projects[opts.Project].QueryHierarchy;

            var queries = new List<QueryDefinition>();
            foreach (QueryItem queryItem in ((TfsService)this.DependenciesService).WorkItemStore.Projects[opts.Project].QueryHierarchy)
            {
                var folder = (QueryFolder)queryItem;
                if (folder != null)
                {
                    GetQueryDefinitions(folder, queries);
                }
            }

            Guid queryGuid = Guid.Empty;
            // ToDo: there might be much better ways to search for the Query Guid
            foreach (var query in queries)
            {
                if (query.Path.Contains(opts.Query))
                {
                    queryGuid = query.Id;
                    break;
                }
            }

            ((TfsService)this.DependenciesService).ImportDependenciesFromTfs(opts.Project, queryGuid);
        }

        private void GetQueryDefinitions(IEnumerable<QueryItem> queryFolder, ICollection<QueryDefinition> list)
        {
            foreach (QueryItem i in queryFolder)
            {
                if (i.GetType() == typeof(QueryFolder))
                {
                    GetQueryDefinitions((QueryFolder)i, list);
                }
                else
                {
                    var item = i as QueryDefinition;
                    if (item != null)
                    {
                        list.Add(item);
                    }
                }
            }
        }

        private void DependenciesModelChangedHandler(object sender, EventArgs e)
        {
            var graph = GraphVizHelper.CreateDependencyGraph(this.DependenciesService.DependenciesModel, Properties.Settings.Default.maxLineLength);

            IRenderer renderer = new Renderer(Properties.Settings.Default.graphvizPath);

            using (Stream fileStream = File.Create(this.ImagePath))
            {
                Task.Run(async () =>
                {
                    await renderer.RunAsync(graph,
                        fileStream,
                        RendererLayouts.Dot,
                        RendererFormats.Png,
                        CancellationToken.None);
                }).Wait();
            }
        }
    }
}
