using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DependenciesVisualizer.Connectors.Services;
using DependenciesVisualizer.Contracts;
using DependenciesVisualizer.Helpers;
using DependenciesVisualizer.ViewModels;
using Ninject;
using Technewlogic.WpfDialogManagement;
using Technewlogic.WpfDialogManagement.Contracts;
using System.Threading;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace DependenciesVisualizer.Connectors.ViewModels
{
    public class TfsConnectorViewModel : ViewModelBase, IConnectorViewModel
    {
        public string Name => "TFS";

        private readonly ITfsService tfsService;
        private Visibility isLoading;

        private TreeView treeViewQueryRef;
        private Dispatcher uiThreadRef;

        [Inject]
        public TfsConnectorViewModel(ITfsService tfsService)
        {
            this.tfsService = tfsService;
            this.ProjectName = ConfigurationManager.AppSettings["tfsprojectName"];
            this.IsLoading = Visibility.Hidden;
            this.ReloadTFSQueries = new RelayCommand<object>(this.ExecuteReloadTFSQueries, o => true);

            this.RenderDependenciesImageFromQuery = new RelayCommand<object>(this.ExecuteRenderDependenciesImageFromQuery, o => true);
        }

        public void Initialize()
        {
            //this.IsLoading = Visibility.Visible;
            try
            {
                this.tfsService.SetWorkItemStore(new Uri(ConfigurationManager.AppSettings["tfsUrl"]), ConfigurationManager.AppSettings["tfsprojectName"]);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
            //this.IsLoading = Visibility.Hidden;
        }

        public IDependenciesService DependenciesService => (IDependenciesService)this.tfsService;

        public void BuildTreeView(TreeView treeViewQuery, Dispatcher uiThread)
        {
            this.treeViewQueryRef = treeViewQuery;
            this.uiThreadRef = uiThread;
            this.IsLoading = Visibility.Visible;
            TreeViewHelper.BuildTreeViewFromTfs(
                                                treeViewQuery,
                                                uiThread,
                                                this.tfsService.WorkItemStore.Projects[ConfigurationManager.AppSettings["tfsprojectName"]].QueryHierarchy,
                                                ConfigurationManager.AppSettings["tfsprojectName"],
                                                this.QuerySelected);
            this.IsLoading = Visibility.Hidden;
            
        }

        private void QuerySelected(object sender, MouseButtonEventArgs mouseEvtArgs)
        {
            this.tfsService.ImportDependenciesFromTfs((Guid)((TreeViewItem)sender).Tag);
        }

        public Visibility IsLoading
        {
            get => this.isLoading;
            set
            {
                if (this.isLoading == value)
                {
                    return;
                }

                this.isLoading = value;
                this.OnPropertyChanged("IsLoading");
            }
        }

        

        private async void ExecuteReloadTFSQueries(object obj)
        {
            this.IsLoading = Visibility.Visible;
            await Task.Run(
                           () =>
                           {
                               var root = TreeViewHelper.BuildTreeViewFromTfs2(this.tfsService.WorkItemStore.Projects[ConfigurationManager.AppSettings["tfsprojectName"]].QueryHierarchy,
                                                                               ConfigurationManager.AppSettings["tfsprojectName"],
                                                                               this.RenderDependenciesImageFromQuery);
                               queries = new ObservableCollection<TfsQueryTreeItemViewModel>() { root };

                               this.OnPropertyChanged("Queries");
                           });
            this.IsLoading = Visibility.Hidden;
        }

        public ICommand RenderDependenciesImageFromQuery { get; private set; }

        private void ExecuteRenderDependenciesImageFromQuery(object obj)
        {
            this.tfsService.ImportDependenciesFromTfs((Guid) obj);
        }

        public string ProjectName { get; private set; }

        public ICommand ReloadTFSQueries { get; private set; }

        ObservableCollection<TfsQueryTreeItemViewModel> queries = new ObservableCollection<TfsQueryTreeItemViewModel>();
        

        public ObservableCollection<TfsQueryTreeItemViewModel> Queries
        {
            get
            {
                return queries;
            }
        }

        public string ErrorMessage
        {
            get => this.errorMessage;
            set
            {
                if (this.errorMessage == value)
                {
                    return;
                }

                this.errorMessage = value;
                this.OnPropertyChanged("ErrorMessage");
            }
        }
        private string errorMessage;

        public bool IsConfigurable
        {
            get { return true; }
        }
    }
}
