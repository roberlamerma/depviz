﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using DependenciesVisualizer.Connectors.Services;
using DependenciesVisualizer.Contracts;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Shields.GraphViz.Components;
using Shields.GraphViz.Models;
using Shields.GraphViz.Services;
using Technewlogic.WpfDialogManagement;
using Technewlogic.WpfDialogManagement.Contracts;

namespace DependenciesVisualizer.Model
{
    /*
    class MainViewModel
    {
        //public WorkItemStore WorkItemStore { get; private set; }

        public bool IsAppValidAndReady { get; private set; }

        public Dictionary<int, DependencyItem> DependenciesModel { get; private set; }

        private readonly Action<object, MouseButtonEventArgs> doubleClickDelegate;

        private readonly IDialogManager dialogManager;

        private IDependenciesService dependencyItemImporter;

        public MainViewModel(IDialogManager dialogManager)
        {
            this.IsAppValidAndReady = false;
            this.dialogManager = dialogManager;
            this.dependencyItemImporter = new TfsService();

            this.Initialize();

            this.doubleClickDelegate = this.BuildModelFromTfsQuery;
            this.DependenciesModel = new Dictionary<int, DependencyItem>();
        }

        public void BuildTreeView(ref TreeView queryTreeView)
        {
            // This should be done with a DependenciesModel with Bindings, not
            // modifying the TreeView directly...
            // ToDo: change this to a DependenciesModel bound to the Main view

            try
            {
                TreeViewHelper.BuildTreeViewFromTfs(
                                                    ref queryTreeView,
                                                    ((TfsService)this.dependencyItemImporter).WorkItemStore.Projects[ConfigurationManager.AppSettings["tfsprojectName"]].QueryHierarchy,
                                                    ConfigurationManager.AppSettings["tfsprojectName"],
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
                    var projectString = ConfigurationManager.AppSettings["tfsprojectName"];

                    if (TfsHelper.IsTfsUrlValid(urlString, projectString, out Uri uri))
                    {
                        //try
                        //{
                        //    ((TfsService)this.dependencyItemImporter).WorkItemStore = TfsHelper.GetWorkItemStore(uri);
                        //}
                        //catch (Exception)
                        //{
                        //    this.dialogManager
                        //        .CreateMessageDialog(string.Format(@"Make sure you are connected to the corporate network, and that the 'tfsUrl' and 'tfsprojectName' settings are correct. Current values: {0}tfsUrl = '{1}'{2}projectName = '{3}'", Environment.NewLine, urlString, Environment.NewLine, projectString), "ERROR", DialogMode.Ok)
                        //        .Show();
                        //}

                        //this.IsAppValidAndReady = true;
                    }
                    else
                    {
                        this.dialogManager
                            .CreateMessageDialog(string.Format(@"Please check on the application config file if the 'tfsUrl' and 'tfsprojectName' settings are correct. Current values: {0}tfsUrl = '{1}'{2}projectName = '{3}'", Environment.NewLine, urlString, Environment.NewLine, projectString), "ERROR", DialogMode.Ok)
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

            // ToDo: Cannot have on this ViewModel references to the TFSDI
            var queryDef = ((TfsService)this.dependencyItemImporter).WorkItemStore.GetQueryDefinition((Guid)item.Tag);

            var queryTextWithProject = queryDef.QueryText.Replace("@project", string.Format("'{0}'", ConfigurationManager.AppSettings["tfsprojectName"]));

            var query = new Query(((TfsService)this.dependencyItemImporter).WorkItemStore, queryTextWithProject);

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
                    // ToDo: Force that on each query run no cache is used
                    var queryResults = query.RunLinkQuery();

                    // ToDo: Make this more elegantly, using the helper or some DependenciesService
                    // Populate the model (parent id's and successors) from the query
                    foreach (WorkItemLinkInfo workItemInfo in queryResults)
                    {
                        if (workItemInfo.SourceId == 0) // parent
                        {
                            if (!this.DependenciesModel.ContainsKey(workItemInfo.TargetId))
                            {
                                this.DependenciesModel.Add(workItemInfo.TargetId, new DependencyItem(workItemInfo.TargetId));
                            }
                        } else // child
                        {
                            // ToDo: Make this also work with Predecessors
                            if (workItemInfo.LinkTypeId == 3) // successor
                            {
                                this.DependenciesModel.TryGetValue(workItemInfo.SourceId, out var dependencyItem);
                                if (dependencyItem != null) dependencyItem.Successors.Add(workItemInfo.TargetId);
                            }
                        }
                    }

                    var successorsThatAreNotParents = new List<DependencyItem>();

                    // Add Title and Tags to items in the DependenciesModel
                    foreach (KeyValuePair<int, DependencyItem> entry in this.DependenciesModel)
                    {
                        if (entry.Value.Title == null)
                        {
                            var workItem = ((TfsService)this.dependencyItemImporter).WorkItemStore.GetWorkItem(entry.Key);
                            entry.Value.Title = workItem.Title;

                            entry.Value.Tags.AddRange(TfsHelper.GetTags(workItem));
                        }

                        foreach (var successor in entry.Value.Successors)
                        {
                            // If successors are not parents, retrieve them from TFS and add them
                            if (!this.DependenciesModel.ContainsKey(successor))
                            {
                                var workItem = ((TfsService)this.dependencyItemImporter).WorkItemStore.GetWorkItem(successor);
                                var dependencyItem = new DependencyItem(successor) { Title = workItem.Title };
                                dependencyItem.Tags.AddRange(TfsHelper.GetTags(workItem));

                                successorsThatAreNotParents.Add(dependencyItem);
                            }
                        }
                    }

                    // Adde successors That Are Not Parents to the DependenciesModel
                    foreach (var successor in successorsThatAreNotParents)
                    {
                        if (!this.DependenciesModel.ContainsKey(successor.Id))
                        {
                            this.DependenciesModel.Add(successor.Id, successor);
                        }
                    }

                    //foreach (WorkItemLinkInfo workItemInfo in queryResults)
                    //{
                    //    if (!this.DependenciesModel.ContainsKey(workItemInfo.TargetId))
                    //    {
                    //        var dependencyListItem = this.GetDependencyListItemFromWorkItem(workItemInfo.TargetId, true);

                    //        this.DependenciesModel.Add(workItemInfo.TargetId, dependencyListItem);
                    //    }
                    //}

                    //// As a PBI might just appears on the query as a child, we make sure it is also added to the model
                    //foreach (KeyValuePair<int, DependencyItem> entry in this.DependenciesModel)
                    //{
                    //    if (entry.Value.Successors.Any())
                    //    {
                    //        foreach (var succesor in entry.Value.Successors)
                    //        {
                    //            if (!this.DependenciesModel.ContainsKey(succesor))
                    //            {
                    //                var dependencyListItem = this.GetDependencyListItemFromWorkItem(succesor, false);

                    //                this.DependenciesModel.Add(succesor, dependencyListItem);
                    //            }
                    //        }
                    //    }
                    //}

                    var graph = this.CreateDependencyGraph(queryDef.Name);

                    await this.Render(graph, queryDef.Name);
                });
            }
        }

        //private DependencyItem GetDependencyListItemFromWorkItem(int targetId, bool addSuccesors)
        //{
        //    var workItem = this.WorkItemStore.GetWorkItem(targetId);

        //    var dependencyListItem = new DependencyItem() { Id = targetId, Title = workItem.Title };

        //    // here we retrieve the succesors
        //    dependencyListItem.Successors.AddRange(TfsHelper.GetLinksOfType(workItem, "Successor"));

        //    // Here we retrieve the tags
        //    dependencyListItem.Tags.AddRange(TfsHelper.GetTags(workItem));

        //    return dependencyListItem;
        //}

        private Graph CreateDependencyGraph(string queryName)
        {
            try
            {
                var statements = new List<Statement>();

                GraphVizHelper.AddGeneralStatements(ref statements);

                foreach (KeyValuePair<int, DependencyItem> entry in this.DependenciesModel)
                {
                    if (entry.Value.Successors.Any())
                    {
                        foreach (var succesor in entry.Value.Successors)
                        {
                            GraphVizHelper.AddEdgeStatement(ref statements, entry.Value.ToString(), this.DependenciesModel[succesor].ToString());
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
                // ToDo: Add message with error
                // ToDo: Add Logger!
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
    }*/
}