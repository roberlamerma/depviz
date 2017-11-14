using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.Linq;
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

namespace DependenciesVisualizer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        //public ObservableCollection<string> ConnectorNames { get; private set; }

        public ObservableCollection<IConnectorViewModel> Connectors { get; private set; }

        public IKernel Ioc { get; private set; }

        private Dictionary<int, DependencyItem> Model { get; set; }

        private int selectedVMIndex;

        private IConnectorViewModel currentConnectorViewModel;

        //private readonly List<IConnectorViewModel> connectorViewModels;

        public MainWindowViewModel(IKernel ioc)
        {
            this.Ioc = ioc;

            //this.connectorViewModels = new List<IConnectorViewModel>();
            //this.ConnectorNames = new ObservableCollection<string>();

            this.Connectors = new ObservableCollection<IConnectorViewModel>(this.Ioc.GetAll<IConnectorViewModel>());

            foreach (var vm in this.Connectors)
            {
                if (vm.Name.ToLower().Equals(ConfigurationManager.AppSettings["selectedConnector"].ToLower()))
                {
                    this.CurrentConnectorViewModel = vm;
                }
            }

            /*
            int selectedViewModelIndex = -1;
            byte i = 0;

            foreach (var vm in this.Ioc.GetAll<IConnectorViewModel>())
            {
                this.connectorViewModels.Add(vm);
                this.ConnectorNames.Add(vm.Name);

                if (selectedViewModelIndex == -1 && vm.Name.ToLower().Equals(ConfigurationManager.AppSettings["selectedConnector"].ToLower()))
                {
                    selectedViewModelIndex = i;
                }

                i++;
            }

            this.SelectedVMIndex = selectedViewModelIndex;
            */

            this.SelectConnector = new RelayCommand<IConnectorViewModel>(ExecuteSelectConnector, o => true );

            this.RenderAndDownloadDependenciesAsImage = new RelayCommand<string>(this.ExecuteRenderAndDownloadDependenciesAsImage, o => this.IsRenderable);

            //var configuredViewModel = this.connectorViewModels.SingleOrDefault(vm => vm.Name.ToLower().Equals(ConfigurationManager.AppSettings["selectedConnector"].ToLower()));

            //this.CurrentConnectorViewModel = configuredViewModel ?? this.connectorViewModels[0];
        }

        private void ExecuteRenderAndDownloadDependenciesAsImage(string fileType)
        {
            var graph = GraphVizHelper.CreateDependencyGraph(this.currentConnectorViewModel.DependenciesService.DependenciesModel);

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Png Image|*.png|Svg Image|*.svg";
            saveFileDialog1.Title = "Save dependencies as... (image)";
            bool? result = saveFileDialog1.ShowDialog();

            if (result == true && !string.IsNullOrWhiteSpace(saveFileDialog1.FileName))
            {
                IRenderer renderer = new Renderer(ConfigurationManager.AppSettings["graphvizPath"]);

                using (Stream fileStream = File.Create(saveFileDialog1.FileName))
                {
                    Task.Run(async () => { await renderer.RunAsync(graph, fileStream, RendererLayouts.Dot, (fileType == "png"?RendererFormats.Png: RendererFormats.Svg), CancellationToken.None); }).Wait();
                }
            }
        }

        private void ExecuteSelectConnector(IConnectorViewModel connectorViewModel)
        {
            this.CurrentConnectorViewModel = connectorViewModel;
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
                }

                this.currentConnectorViewModel = value;
                this.currentConnectorViewModel.Initialize();

                WeakEventManager<IDependenciesService, EventArgs>.AddHandler(this.currentConnectorViewModel.DependenciesService, "DependenciesModelChanged", this.DependenciesModelChangedHandler);

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

        private void DependenciesModelChangedHandler(object sender, EventArgs e)
        {
            this.OnPropertyChanged("DependenciesModel");
            this.OnPropertyChanged("DependenciesModelCount");
            this.OnPropertyChanged("IsRenderable");
        }

        public ICommand RenderAndDownloadDependenciesAsImage { get; private set; }

    }
}
