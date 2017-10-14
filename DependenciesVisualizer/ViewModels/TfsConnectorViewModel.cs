using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using DependenciesVisualizer.Helpers;
using DependenciesVisualizer.Services;

namespace DependenciesVisualizer.ViewModels
{
    public class TfsConnectorViewModel : ViewModelBase, IConnectorViewModel
    {
        public string Name => "TFS";

        public Guid QueryId { get; set; }

        private readonly ITfsService tfsService;

        public void Initialize()
        {
            this.tfsService.SetWorkItemStore(new Uri(ConfigurationManager.AppSettings["tfsUrl"]), ConfigurationManager.AppSettings["tfsprojectName"]);
        }

        public void BuildTreeView(ref TreeView treeViewQuery)
        {
            TreeViewHelper.BuildTreeViewFromTfs(
                                                ref treeViewQuery,
                                                this.tfsService.WorkItemStore.Projects[ConfigurationManager.AppSettings["tfsprojectName"]].QueryHierarchy,
                                                ConfigurationManager.AppSettings["tfsprojectName"],
                                                this.QuerySelected);
        }

        private void QuerySelected(object sender, MouseButtonEventArgs mouseEvtArgs)
        {
        }

        public TfsConnectorViewModel(ITfsService tfsService)
        {
            this.tfsService = tfsService;
            this.ProjectName = ConfigurationManager.AppSettings["tfsprojectName"];
        }
        public string ProjectName { get; private set; }
    }
}
