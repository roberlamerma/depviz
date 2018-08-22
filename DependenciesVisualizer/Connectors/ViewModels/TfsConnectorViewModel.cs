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
using System.Threading;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using DependenciesVisualizer.Connectors.UserControls;

namespace DependenciesVisualizer.Connectors.ViewModels
{
    public class TfsConnectorViewModel : ViewModelBase, IConnectorViewModel
    {
        public string Name => "TFS";

        private readonly ITfsService tfsService;
        private Visibility isLoading;
        private bool queriesAlreadyLoaded;

        [Inject]
        public TfsConnectorViewModel(ITfsService tfsService)
        {
            this.tfsService = tfsService;
            this.ProjectName = Properties.Settings.Default.tfsprojectName;
            this.queriesAlreadyLoaded = false;
            this.IsLoading = Visibility.Hidden;
            this.ReloadTFSQueries = new RelayCommand<object>(this.ExecuteReloadTFSQueries, o => true);

            this.ConfigureTfsUriAndProject = new RelayCommand<object>(this.ExecuteConfigureTfsUriAndProject, o => true);

            this.RenderDependenciesImageFromQuery = new RelayCommand<object>(this.ExecuteRenderDependenciesImageFromQuery, o => true);

            this.SearchPbiById = new RelayCommand<object>(this.ExecuteSearchPbiById, this.CanExecuteSearchPbiById);
        }

        public void Initialize()
        {
            //this.IsLoading = Visibility.Visible;
            try
            {
                this.tfsService.SetWorkItemStore(new Uri(Properties.Settings.Default.tfsUrl), Properties.Settings.Default.tfsprojectName);
                this.ExecuteReloadTFSQueries(null);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
            //this.IsLoading = Visibility.Hidden;
        }

        public IDependenciesService DependenciesService => (IDependenciesService)this.tfsService;

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

        private void ExecuteConfigureTfsUriAndProject(object obj)
        {
            var tfsUriAndProjectSelectorUserControl = new TfsUriAndProjectSelector();
            Window window = new Window
            {
                Title = "Set TFS server and Project name",
                Content = tfsUriAndProjectSelectorUserControl,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            };

            window.ShowDialog();

            // Todo: Evaluate raising an event "TFS and Project changed" from tfsUriAndProjectSelectorViewModel. Listen to it here, and reload the queries. 
            // Maybe tfsUriAndProjectSelectorViewModel.HaveSettingsChanged won't be needed anymore. All the following code will be gone or heavily modified
            var tfsUriAndProjectSelectorViewModel = (TfsUriAndProjectSelectorViewModel)tfsUriAndProjectSelectorUserControl.DataContext;

            // If settings have changed, then reload queries
            if (tfsUriAndProjectSelectorViewModel.HaveSettingsChanged)
            {
                this.tfsService.SetWorkItemStore(tfsUriAndProjectSelectorViewModel.Store);
                this.ProjectName = Properties.Settings.Default.tfsprojectName;

                this.ExecuteReloadTFSQueries(null);
            }
        }

        private async void ExecuteReloadTFSQueries(object obj)
        {
            this.IsLoading = Visibility.Visible;
            await Task.Run(
                           () =>
                           {
                               try
                               {
                                   if (this.tfsService.WorkItemStore == null)
                                   {
                                       throw new CannotConnectException(string.Format(@"Cannot connect to TFS uri: {0}", Properties.Settings.Default.tfsUrl));
                                   }

                                   // In order to show potentially newly created queries, we reload them (just when they were already loaded before)
                                   if (this.queriesAlreadyLoaded)
                                   {
                                       this.tfsService.WorkItemStore.Projects[Properties.Settings.Default.tfsprojectName].QueryHierarchy.Refresh();
                                   }

                                   var root = TreeViewHelper.BuildTreeViewFromTfs(this.tfsService.WorkItemStore.Projects[Properties.Settings.Default.tfsprojectName].QueryHierarchy,
                                                                                                         Properties.Settings.Default.tfsprojectName,
                                                                                                         this.RenderDependenciesImageFromQuery);

                                   queries = new ObservableCollection<TfsQueryTreeItemViewModel>() { root };

                                   if (root.Children.Count() > 0)
                                   {
                                       this.queriesAlreadyLoaded = true;
                                   }

                                   this.OnPropertyChanged("Queries");
                               }
                               catch (Exception ex)
                               {
                                   this.ErrorMessage = ex.Message;
                               }
                           });
            this.IsLoading = Visibility.Hidden;
        }

        public ICommand RenderDependenciesImageFromQuery { get; private set; }

        private void ExecuteRenderDependenciesImageFromQuery(object obj)
        {
            Task.Run(() =>
            {
                try
                {
                    this.tfsService.ImportDependenciesFromTfs(this.ProjectName, (Guid)obj);
                }
                catch (Exception ex)
                {
                    this.ErrorMessage = ex.Message + Environment.NewLine;
                }
            });
                //this.tfsService.ImportDependenciesFromTfs(this.ProjectName, (Guid)obj);
            //try
            //{
            //    task1.Wait();
            //}
            ////catch (Exception ex)
            //catch (AggregateException ae)
            //{
            //    foreach (var ex in ae.InnerExceptions)
            //    {
            //        this.ErrorMessage = ex.Message + Environment.NewLine;
            //    }
            //}
        }

        private void ExecuteSearchPbiById(object obj)
        {
            Task.Run(() =>
            {
                try
                {
                    this.tfsService.ImportDependenciesFromTfs(Convert.ToInt32(obj));
                }
                catch (Exception ex)
                {
                    this.ErrorMessage = ex.Message + Environment.NewLine;
                }
            });
        }

        private bool CanExecuteSearchPbiById(object obj)
        {
            return uint.TryParse(obj.ToString(), out uint n);
        }

        public string ProjectName { get; set; }

        public ICommand ReloadTFSQueries { get; private set; }

        public ICommand ConfigureTfsUriAndProject { get; private set; }

        public ICommand SearchPbiById { get; private set; }

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
