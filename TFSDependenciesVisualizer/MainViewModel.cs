using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
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
                    CreateWaitDialog(string.Format(@"1.- Retreiving items from the '{0}' query. This operation might take a while{1}2.- After this message dissapears, search for your image '{2}.png' on this path:{3}", queryDef.Name, Environment.NewLine, queryDef.Name, Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)), DialogMode.None);

                dialog.Show(() => {

                    var queryResults = query.RunLinkQuery();

                    foreach (WorkItemLinkInfo workItemInfo in queryResults)
                    {
                        if (!this.Model.ContainsKey(workItemInfo.TargetId))
                        {
                            var dependencyListItem = TfsHelper.GetDependencyListItemFromWorkItem(this.WorkItemStore, workItemInfo.TargetId);

                            this.Model.Add(workItemInfo.TargetId, dependencyListItem);
                        }
                    }

                    //this.CreateDependencyGraph(queryDef.Name);
                });
            }
        }

        private async Task CreateDependencyGraph(string queryName)
        {
            try
            {
                List<Statement> statements = new List<Statement>();

                var generalStyleSettings = new Dictionary<Id, Id>();
                generalStyleSettings.Add(new Id("rankdir"), new Id("LR"));
                var generalStyleAttributes = new AttributeStatement(AttributeKinds.Graph, generalStyleSettings.ToImmutableDictionary());

                statements.Add(generalStyleAttributes);

                var generalNodeStyleSettings = new Dictionary<Id, Id>();
                generalNodeStyleSettings.Add(new Id("shape"), new Id("rectangle"));
                generalNodeStyleSettings.Add(new Id("style"), new Id("filled"));
                //generalNodeStyleSettings.Add(new Id("fontname"), new Id("Monospace"));
                var generalNodeStyleAttributes = new AttributeStatement(AttributeKinds.Node, generalNodeStyleSettings.ToImmutableDictionary());

                statements.Add(generalNodeStyleAttributes);

                var emptyDict = new Dictionary<Id, Id>();

                foreach (KeyValuePair<int, DependencyListItem> entry in this.Model)
                {
                    if (entry.Value.Successors.Any())
                    {
                        foreach (var succesor in entry.Value.Successors)
                        {
                            var edge = new EdgeStatement(
                                new NodeId(entry.Value.ToString()),
                                new NodeId(this.Model[succesor].ToString()),
                                emptyDict.ToImmutableDictionary());

                            statements.Add(edge);
                        }
                    }

                    if (entry.Value.Tags.Any())
                    {
                        Dictionary<Id, Id> nodeStyleSettings = null;
                        //if (entry.Value.Tags.Any(str => str.Contains("Internal")))
                        //{
                        //    nodeStyleSettings = new Dictionary<Id, Id>();
                        //    nodeStyleSettings.Add(new Id("color"), new Id("0.647 0.204 1.000"));
                        //}
                        //else 
                        if (entry.Value.Tags.Any(str => str.Contains("External")))
                        {
                            nodeStyleSettings = new Dictionary<Id, Id>();
                            nodeStyleSettings.Add(new Id("color"), new Id("0.408 0.498 1.000"));
                        }

                        if (nodeStyleSettings != null)
                        {
                            var node = new NodeStatement(new Id(entry.Value.ToString()), nodeStyleSettings.ToImmutableDictionary());
                            statements.Add(node);
                        }
                    }
                }

                Graph graph = new Graph(GraphKinds.Directed, queryName, statements.ToImmutableList());

                IRenderer renderer = new Renderer(ConfigurationManager.AppSettings["graphvizPath"]);
                using (Stream file = File.Create(string.Format(@"{0}.png", queryName)))
                {
                    await renderer.RunAsync(
                        graph, file,
                        RendererLayouts.Dot,
                        RendererFormats.Png,
                        CancellationToken.None);
                }
            }
            catch (Exception ex)
            {

            }

        }
    }
}
