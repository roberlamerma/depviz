using DependenciesVisualizer.Contracts;
using DependenciesVisualizer.ViewModels;

namespace DependenciesVisualizer.Connectors.ViewModels
{
    public class CsvConnectorViewModel : ViewModelBase, IConnectorViewModel
    {
        public string Name => "CSV";

        public void Initialize()
        {
        }

        public IDependenciesService DependenciesService { get; set; }
    }
}
