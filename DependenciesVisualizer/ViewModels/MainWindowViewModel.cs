using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ninject;

namespace DependenciesVisualizer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<string> ConnectorNames { get; private set; }

        public IKernel Ioc { get; private set; }

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

                this.currentConnectorViewModel = value;
                this.currentConnectorViewModel.Initialize();
                this.OnPropertyChanged("CurrentConnectorViewModel");
            }
        }

    }
}
