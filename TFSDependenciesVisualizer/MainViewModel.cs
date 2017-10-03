using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
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

                    if (TfsConnector.IsTfsUrlValid(urlString, projectString, out Uri uri))
                    {
                        try
                        {
                            this.WorkItemStore = TfsConnector.GetWorkItemStore(uri);
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
                            var dependencyListItem = TfsConnector.GetDependencyListItemFromWorkItem(this.WorkItemStore, workItemInfo.TargetId);

                            this.Model.Add(workItemInfo.TargetId, dependencyListItem);
                        }
                    }

                    //this.CreateDependencyGraph(queryDef.Name);
                });
            }
        }
    }
}
