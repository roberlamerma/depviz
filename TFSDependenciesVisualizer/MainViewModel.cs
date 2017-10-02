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
using TFSDependencyVisualizer.Helpers;

namespace TFSDependenciesVisualizer
{
    class MainViewModel
    {
        public WorkItemStore WorkItemStore { get; private set; }

        public Action<object, MouseButtonEventArgs> DoubleClickDelegate { get; private set; }

        public bool IsAppValidAndReady { get; private set; }

        private readonly IDialogManager dialogManager;

        public MainViewModel(IDialogManager dialogManager)
        {
            this.IsAppValidAndReady = false;
            this.dialogManager = dialogManager;

            this.Initialize();

            this.DoubleClickDelegate = (x, y) => { };
        }

        private void Initialize()
        {
            var graphvizPath = ConfigurationManager.AppSettings["graphvizPath"];
            if (Directory.Exists(graphvizPath))
            {
                var urlString = ConfigurationManager.AppSettings["tfsUrl"];
                var projectString = ConfigurationManager.AppSettings["projectName"];

                if (TFSConnector.IsTFSUrlValid(urlString, projectString, out Uri uri))
                {
                    try
                    {
                        this.WorkItemStore = TFSConnector.GetWorkItemStore(uri);
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

        public void BuildTreeView(ref TreeView queryTreeView)
        {
            // This should be done with a Model with Bindings, not
            // modifying the TreeView directly...
            // ToDo: change this to a Model bound to the Main view

            try
            {
                TreeViewHelper.BuildTreeViewFromTFS(
                                                    ref queryTreeView,
                                                    this.WorkItemStore.Projects[ConfigurationManager.AppSettings["projectName"]].QueryHierarchy,
                                                    ConfigurationManager.AppSettings["projectName"],
                                                    this.DoubleClickDelegate);
            }
            catch (Exception ex)
            {
                this.dialogManager
                    .CreateMessageDialog(ex.Message, "ERROR", DialogMode.Ok)
                    .Show();
            }
        }
    }
}
