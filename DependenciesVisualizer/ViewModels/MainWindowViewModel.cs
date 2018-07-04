using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using System.Windows.Input;
using DependenciesVisualizer.Contracts;
using DependenciesVisualizer.Helpers;
using DependenciesVisualizer.Model;
using Ninject;
using Microsoft.Win32;
using Shields.GraphViz.Services;
using Shields.GraphViz.Components;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DependenciesVisualizer.Connectors.UserControls;

namespace DependenciesVisualizer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        //public ObservableCollection<string> ConnectorNames { get; private set; }

        public ObservableCollection<IConnectorViewModel> Connectors { get; private set; }

        public IKernel Ioc { get; private set; }

        public ICommand ZoomInCommand { get; private set; }
        public ICommand ZoomOutCommand { get; private set; }
        public ICommand GoToDepvizWikiHowto { get; private set; }
        public ICommand GoToGraphvizHome { get; private set; }
        public ICommand GoToDepvizHome { get; private set; }

        private Dictionary<int, DependencyItem> Model { get; set; }
        private IConnectorViewModel currentConnectorViewModel;

        public MainWindowViewModel(IKernel ioc)
        {
            this.IsLoading = false;

            this.Ioc = ioc;

            this.Connectors = new ObservableCollection<IConnectorViewModel>(this.Ioc.GetAll<IConnectorViewModel>());

            foreach (var vm in this.Connectors)
            {
                if (vm.Name.ToLower().Equals(Properties.Settings.Default.selectedConnector.ToLower()))
                {
                    this.CurrentConnectorViewModel = vm;
                }
            }

            this.SelectConnector = new RelayCommand<IConnectorViewModel>(ExecuteSelectConnector, o => true );

            this.RenderAndDownloadDependenciesAsImage = new RelayCommand<string>(this.ExecuteRenderAndDownloadDependenciesAsImage, o => this.IsRenderable);

            this.ConfigureConnector = new RelayCommand<string>(this.ExecuteConfigureConnector, o => true);

            this.ZoomInCommand = new RelayCommand<object>(this.ExecuteZoomInCommand, o => true);
            this.ZoomOutCommand = new RelayCommand<object>(this.ExecuteZoomOutCommand, o => true);

            this.GoToDepvizWikiHowto = new RelayCommand<IConnectorViewModel>(ExecuteGoToDepvizWikiHowto, o => true);
            this.GoToGraphvizHome = new RelayCommand<IConnectorViewModel>(ExecuteGoToGraphvizHome, o => true);
            this.GoToDepvizHome = new RelayCommand<IConnectorViewModel>(ExecuteGoToDepvizHome, o => true);
        }

        private void ExecuteGoToDepvizHome(object obj)
        {
            System.Diagnostics.Process.Start("https://github.com/roberlamerma/depviz");
        }

        private void ExecuteGoToGraphvizHome(object obj)
        {
            System.Diagnostics.Process.Start("https://www.graphviz.org/");
        }

        private void ExecuteGoToDepvizWikiHowto(object obj)
        {
            System.Diagnostics.Process.Start("https://github.com/roberlamerma/depviz/wiki/How-to-use-depviz");
        }

        private void ExecuteZoomOutCommand(object obj)
        {
            ((System.Windows.Controls.Primitives.RangeBase)obj).Value -= 0.05;
        }

        private void ExecuteZoomInCommand(object obj)
        {
            ((System.Windows.Controls.Primitives.RangeBase)obj).Value += 0.05;
        }

        private void ExecuteRenderAndDownloadDependenciesAsImage(string fileType)
        {
            var graph = GraphVizHelper.CreateDependencyGraph(this.currentConnectorViewModel.DependenciesService.DependenciesModel, Properties.Settings.Default.maxLineLength);

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            //saveFileDialog1.Filter = "Png Image|*.png|Svg Image|*.svg";
            saveFileDialog1.Filter = (fileType == "png" ? "Png Image|*.png" : "Svg Image|*.svg");
            saveFileDialog1.Title = "Save dependencies as... (image)";
            bool? result = saveFileDialog1.ShowDialog();

            if (result == true && !string.IsNullOrWhiteSpace(saveFileDialog1.FileName))
            {
                IRenderer renderer = new Renderer(Properties.Settings.Default.graphvizPath);

                using (Stream fileStream = File.Create(saveFileDialog1.FileName))
                {
                    Task.Run(async () =>
                    {
                        await renderer.RunAsync(graph, 
                            fileStream, 
                            RendererLayouts.Dot, 
                            (fileType == "png" ? RendererFormats.Png : RendererFormats.Svg),
                            CancellationToken.None);
                    }).Wait();
                }
            }
        }

        private void ExecuteSelectConnector(IConnectorViewModel connectorViewModel)
        {
            this.CurrentConnectorViewModel = connectorViewModel;
            Properties.Settings.Default.selectedConnector = connectorViewModel.Name.ToLower();
            Properties.Settings.Default.Save();
        }


        //public int SelectedVMIndex
        //{
        //    get => this.selectedVMIndex;

        //    set
        //    {
        //        if (this.selectedVMIndex == value && this.CurrentConnectorViewModel != null)
        //        {
        //            return;
        //        }

        //        this.selectedVMIndex = value;
        //        this.OnPropertyChanged("SelectedVMIndex");

        //        this.CurrentConnectorViewModel = this.connectorViewModels[this.selectedVMIndex];
        //    }
        //}

        public ICommand SelectConnector { get; private set; }

        private void ExecuteConfigureConnector(string connectorName)
        {
            connectorName = connectorName.ToLower();
            switch (connectorName)
            {
                case "tfs":
                    var tfsUriAndProjectSelectorUserControl = new TfsUriAndProjectSelector();

                    Window window = new Window
                    {
                        Title = "Set TFS server and Project name",
                        Content = tfsUriAndProjectSelectorUserControl,
                        SizeToContent = SizeToContent.WidthAndHeight,
                        ResizeMode = ResizeMode.NoResize
                    };

                    window.ShowDialog();


                    // Todo: Raise an event!!!

                    //var tfsUriAndProjectSelectorViewModel = (TfsUriAndProjectSelectorViewModel)tfsUriAndProjectSelectorUserControl.DataContext;

                    //// If settings have changed, then reload queries
                    //if (tfsUriAndProjectSelectorViewModel.HaveSettingsChanged)
                    //{
                    //    var tfsService = this.Ioc.Get<ITfsService>();
                    //    tfsService.SetWorkItemStore(tfsUriAndProjectSelectorViewModel.Store);

                    //    var connectorViewModel = this.Ioc.Get<IConnectorViewModel>();
                    //    if (connectorViewModel is TfsConnectorViewModel)
                    //    {
                    //        ((TfsConnectorViewModel)connectorViewModel).ProjectName = Properties.Settings.Default.tfsprojectName;
                    //        ((TfsConnectorViewModel)connectorViewModel).ReloadTFSQueries(null);
                    //    }
                    //}
                    break;
                default:
                    break;
            }
        }

        public ICommand ConfigureConnector { get; private set; }

        public IConnectorViewModel CurrentConnectorViewModel
        {
            get => this.currentConnectorViewModel;
            private set
            {
                if (this.currentConnectorViewModel == value)
                {
                    return;
                }

                if (this.currentConnectorViewModel != null)
                {
                    WeakEventManager<IDependenciesService, EventArgs>.RemoveHandler(this.currentConnectorViewModel.DependenciesService, "DependenciesModelChanged", this.DependenciesModelChangedHandler);
                    WeakEventManager<IDependenciesService, EventArgs>.RemoveHandler(this.currentConnectorViewModel.DependenciesService, "DependenciesModelAboutToChange", this.DependenciesModelAboutToChangeHandler);
                    WeakEventManager<IDependenciesService, EventArgs>.RemoveHandler(this.currentConnectorViewModel.DependenciesService, "DependenciesModelCouldNotBeChanged", this.DependenciesModelCouldNotBeChanged);
                }

                this.currentConnectorViewModel = value;
                this.currentConnectorViewModel.Initialize();

                WeakEventManager<IDependenciesService, EventArgs>.AddHandler(this.currentConnectorViewModel.DependenciesService, "DependenciesModelChanged", this.DependenciesModelChangedHandler);
                WeakEventManager<IDependenciesService, EventArgs>.AddHandler(this.currentConnectorViewModel.DependenciesService, "DependenciesModelAboutToChange", this.DependenciesModelAboutToChangeHandler);
                WeakEventManager<IDependenciesService, EventArgs>.AddHandler(this.currentConnectorViewModel.DependenciesService, "DependenciesModelCouldNotBeChanged", this.DependenciesModelCouldNotBeChanged);

                this.OnPropertyChanged("CurrentConnectorViewModel");
            }
        }

        public Dictionary<int, DependencyItem> DependenciesModel => this.currentConnectorViewModel.DependenciesService.DependenciesModel;
        public int DependenciesModelCount
        {
            get
            {
                if (this.currentConnectorViewModel.DependenciesService.DependenciesModel != null)
                {
                    return this.currentConnectorViewModel.DependenciesService.DependenciesModel.Count;
                }
                return 0;
            }
        }

        public bool IsRenderable
        {
            get => (this.DependenciesModelCount > 0);
        }

        public bool IsLoading
        {
            get { return this.isLoading; }
            set
            {
                if (value != this.isLoading)
                {
                    this.isLoading = value;
                    this.OnPropertyChanged("IsLoading");
                }
            }
        }
        private bool isLoading;

        private void DependenciesModelChangedHandler(object sender, EventArgs e)
        {
            this.OnPropertyChanged("DependenciesModel");
            this.OnPropertyChanged("DependenciesModelCount");
            this.OnPropertyChanged("IsRenderable");
            this.IsLoading = false;
        }

        private void DependenciesModelAboutToChangeHandler(object sender, EventArgs e)
        {
            this.IsLoading = true;
        }

        private void DependenciesModelCouldNotBeChanged(object sender, EventArgs e)
        {
            this.OnPropertyChanged("IsRenderable");
            this.IsLoading = false;
        }

        public ICommand RenderAndDownloadDependenciesAsImage { get; private set; }

    }
}
