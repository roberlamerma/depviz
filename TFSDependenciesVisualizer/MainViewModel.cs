using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Shields.GraphViz.Components;
using Shields.GraphViz.Models;
using Shields.GraphViz.Services;
using Technewlogic.WpfDialogManagement;
using Technewlogic.WpfDialogManagement.Contracts;
using TFSDependenciesVisualizer.Helpers;
using TFSDependencyVisualizer;
using TFSDependencyVisualizer.Helpers;

namespace TFSDependenciesVisualizer
{
    class MainViewModel
    {
        public WorkItemStore WorkItemStore { get; private set; }

        public bool IsAppValidAndReady { get; private set; }

        public Dictionary<int, DependencyListItem> Model { get; private set; }

        private readonly Action<object, MouseButtonEventArgs> doubleClickDelegate;

        private readonly IDialogManager dialogManager;

        public MainViewModel(IDialogManager dialogManager)
        {
            this.IsAppValidAndReady = false;
            this.dialogManager = dialogManager;

            this.Initialize();

            this.doubleClickDelegate = this.BuildModelFromTfsQuery;
            this.Model = new Dictionary<int, DependencyListItem>();
        }

        public void BuildTreeView(ref TreeView queryTreeView)
        {
            // This should be done with a Model with Bindings, not
            // modifying the TreeView directly...
            // ToDo: change this to a Model bound to the Main view

            try
            {
                TreeViewHelper.BuildTreeViewFromTfs(
                                                    ref queryTreeView,
                                                    this.WorkItemStore.Projects[ConfigurationManager.AppSettings["projectName"]].QueryHierarchy,
                                                    ConfigurationManager.AppSettings["projectName"],
                                                    this.doubleClickDelegate);
            }
            catch (Exception ex)
            {
                this.dialogManager
                    .CreateMessageDialog(ex.Message, "ERROR", DialogMode.Ok)
                    .Show();
            }
        }

        private void Initialize()
        {
            try
            {
                var graphvizPath = ConfigurationManager.AppSettings["graphvizPath"];
                if (Directory.Exists(graphvizPath))
                {
                    var urlString = ConfigurationManager.AppSettings["tfsUrl"];
                    var projectString = ConfigurationManager.AppSettings["projectName"];

                    if (TfsHelper.IsTfsUrlValid(urlString, projectString, out Uri uri))
                    {
                        try
                        {
                            this.WorkItemStore = TfsHelper.GetWorkItemStore(uri);
                        }
                        catch (Exception)
                        {
                            this.dialogManager
                                .CreateMessageDialog(string.Format(@"Make sure you are connected to the corporate network, and that the 'tfsUrl' and 'projectName' settings are correct. Current values: {0}tfsUrl = '{1}'{2}projectName = '{3}'", Environment.NewLine, urlString, Environment.NewLine, projectString), "ERROR", DialogMode.Ok)
                                .Show();
                        }

                        this.IsAppValidAndReady = true;
                    }
                    else
                    {
                        this.dialogManager
                            .CreateMessageDialog(string.Format(@"Please check on the application config file if the 'tfsUrl' and 'projectName' settings are correct. Current values: {0}tfsUrl = '{1}'{2}projectName = '{3}'", Environment.NewLine, urlString, Environment.NewLine, projectString), "ERROR", DialogMode.Ok)
                            .Show();
                    }
                }
                else
                {
                    this.dialogManager
                        .CreateMessageDialog(string.Format(@"Have you installed Graphviz? If so, please add the correct path on the application config file. Current value: '{0}", graphvizPath), "ERROR", DialogMode.Ok)
                        .Show();
                }
            }
            catch (Exception ex)
            {
                this.dialogManager
                    .CreateMessageDialog(ex.Message, "ERROR", DialogMode.Ok)
                    .Show();
            }
        }

        private void BuildModelFromTfsQuery(object sender, MouseButtonEventArgs mouseEvtArgs)
        {
            TreeViewItem item = (TreeViewItem)sender;

            var queryDef = this.WorkItemStore.GetQueryDefinition((Guid)item.Tag);

            var queryTextWithProject = queryDef.QueryText.Replace("@project", string.Format("'{0}'", ConfigurationManager.AppSettings["projectName"]));

            var query = new Query(this.WorkItemStore, queryTextWithProject);

            if (!query.IsLinkQuery || query.IsTreeQuery)
            {
                this.dialogManager
                    .CreateMessageDialog(string.Format(@"The query '{0}' is not a 'Direct Link' query.", queryDef.Name), "ERROR", DialogMode.Ok)
                    .Show();
            }
            else
            {
                var dialog = this.dialogManager.
                    CreateWaitDialog(string.Format(@"1.- Retreiving items from the '{0}' query. This operation might take a while{1}2.- After this message dissapears, search for your image '{2}.png' on this path: '{3}'", queryDef.Name, Environment.NewLine, queryDef.Name, Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)), DialogMode.None);

                dialog.Show(async () => 
                {
                    var queryResults = query.RunLinkQuery();

                    foreach (WorkItemLinkInfo workItemInfo in queryResults)
                    {
                        if (!this.Model.ContainsKey(workItemInfo.TargetId))
                        {
                            var dependencyListItem = this.GetDependencyListItemFromWorkItem(workItemInfo.TargetId);

                            this.Model.Add(workItemInfo.TargetId, dependencyListItem);
                        }
                    }

                    var graph = this.CreateDependencyGraph(queryDef.Name);

                    await this.Render(graph, queryDef.Name);
                });
            }
        }

        private DependencyListItem GetDependencyListItemFromWorkItem(int targetId)
        {
            var workItem = this.WorkItemStore.GetWorkItem(targetId);

            var dependencyListItem = new DependencyListItem() { Id = targetId, Title = workItem.Title };

            // here we retrieve the succesors
            dependencyListItem.Successors.AddRange(TfsHelper.GetLinksOfType(workItem, "Successor"));

            // Here we retrieve the tags
            dependencyListItem.Tags.AddRange(TfsHelper.GetTags(workItem));

            return dependencyListItem;
        }

        private Graph CreateDependencyGraph(string queryName)
        {
            try
            {
                var statements = new List<Statement>();

                GraphVizHelper.AddGeneralStatements(ref statements);

                foreach (KeyValuePair<int, DependencyListItem> entry in this.Model)
                {
                    if (entry.Value.Successors.Any())
                    {
                        foreach (var succesor in entry.Value.Successors)
                        {
                            GraphVizHelper.AddEdgeStatement(ref statements, entry.Value.ToString(), this.Model[succesor].ToString());
                        }
                    }

                    if (entry.Value.Tags.Any())
                    {
                        if (entry.Value.Tags.Any(str => str.Contains("External")))
                        {
                            GraphVizHelper.ColorizeNode(ref statements, entry.Value.ToString(), Colors.Green);
                        }
                    }
                }

                return new Graph(GraphKinds.Directed, queryName, statements.ToImmutableList());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task Render(Graph graph, string queryName)
        {
            IRenderer renderer = new Renderer(ConfigurationManager.AppSettings["graphvizPath"]);
            using (Stream file = File.Create(string.Format(@"{0}.png", queryName)))
            {
                await renderer.RunAsync(graph,
                    file,
                    RendererLayouts.Dot,
                    RendererFormats.Png,
                    CancellationToken.None);
            }
        }
    }
}
