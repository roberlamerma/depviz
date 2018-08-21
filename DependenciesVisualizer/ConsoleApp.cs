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

        private ILog Logger { get; set; }

        private string[] Args { get; set; }

        private IDependenciesService DependenciesService { get; set; }

        private string ImagePath { get; set; }

        /// <summary>
        /// Example: --tfsUrl http://tfsprod:8080/tfs/defaultcollection --project Roche.DP.PocPCR --query "Software/cobas Liat 3.3/WBS/PO Cockpit/Linux - Liatmanet/Liatmanet Dependencies to other units (Epics)" --output MyDependencyImage.png
        /// </summary>
        /// <param name="args"></param>
        public ConsoleApp(string[] args)
        {
            this.Args = args;
            this.Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public void Run()
        {
            Parser.Default.ParseArguments<ConsoleOptions>(this.Args)
                .WithParsed<ConsoleOptions>(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed<ConsoleOptions>((errs) => HandleParseError(errs));
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            throw new NotImplementedException();
        }

        private void RunOptionsAndReturnExitCode(ConsoleOptions opts)
        {
            try
            {
                if (Path.IsPathRooted(opts.Output))
                {
                    this.ImagePath = opts.Output;
                }
                else
                {
                    this.ImagePath = Path.Combine(Directory.GetCurrentDirectory(), opts.Output);
                }


                this.DependenciesService = new TfsService(this.Logger);
                WeakEventManager<IDependenciesService, EventArgs>.AddHandler(this.DependenciesService, "DependenciesModelChanged", this.DependenciesModelChangedHandler);
                WeakEventManager<IDependenciesService, EventArgs>.AddHandler(this.DependenciesService, "DependenciesModelAboutToChange", this.DependenciesModelAboutToChangeHandler);
                WeakEventManager<IDependenciesService, EventArgs>.AddHandler(this.DependenciesService, "DependenciesModelCouldNotBeChanged", this.DependenciesModelCouldNotBeChanged);

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

                this.Logger.Debug(string.Format(@"Executing query '{0}'", opts.Query));
                ((TfsService)this.DependenciesService).ImportDependenciesFromTfs(opts.Project, queryGuid);
            }
            catch (Exception ex)
            {
                this.Logger.Error(string.Format(@"Got this error while running the query '{0}': {1}", opts.Query, ex.Message));
                this.Logger.Debug(ex.StackTrace);
            }
        }

        private void DependenciesModelCouldNotBeChanged(object sender, EventArgs e)
        {
            this.Logger.Error(@"Could not create dependency image");
        }

        private void DependenciesModelAboutToChangeHandler(object sender, EventArgs e)
        {
            this.Logger.Debug(@"Starting creation of dependency image");
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
            try
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
            catch (Exception ex)
            {
                this.Logger.Error(string.Format(@"Got an error while trying to generate the image '{0}'", this.ImagePath));
                this.Logger.Debug(ex.StackTrace);
            }
        }
    }
}
