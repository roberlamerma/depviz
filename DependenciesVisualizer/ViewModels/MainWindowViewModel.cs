using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Windows;
using DependenciesVisualizer.Contracts;
using DependenciesVisualizer.Model;
using Ninject;

namespace DependenciesVisualizer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<string> ConnectorNames { get; private set; }

        public IKernel Ioc { get; private set; }

        private Dictionary<int, DependencyItem> Model { get; set; }

        private int selectedVMIndex;

        private IConnectorViewModel currentConnectorViewModel;

        private readonly List<IConnectorViewModel> connectorViewModels;

        public MainWindowViewModel(IKernel ioc)
        {
            this.Ioc = ioc;

            this.connectorViewModels = new List<IConnectorViewModel>();
            this.ConnectorNames = new ObservableCollection<string>();
            foreach (var vm in this.Ioc.GetAll<IConnectorViewModel>())
            {
                this.connectorViewModels.Add(vm);
                this.ConnectorNames.Add(vm.Name);
            }

            this.CurrentConnectorViewModel = this.connectorViewModels[0];
        }

        public int SelectedVMIndex
        {
            get => this.selectedVMIndex;

            set
            {
                if (this.selectedVMIndex == value)
                {
                    return;
                }

                this.selectedVMIndex = value;
                this.OnPropertyChanged("SelectedVMIndex");

                this.CurrentConnectorViewModel = this.connectorViewModels[this.selectedVMIndex];
            }
        }

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

        private void DependenciesModelChangedHandler(object sender, EventArgs e)
        {
            this.OnPropertyChanged("DependenciesModel");
            this.OnPropertyChanged("DependenciesModelCount");
        }

    }
}
