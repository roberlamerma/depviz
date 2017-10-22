using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependenciesVisualizer.Model;
using DependenciesVisualizer.Contracts;
using Ninject;

namespace DependenciesVisualizer.ViewModels
{
    public class DependenciesImageViewModel : ViewModelBase
    {
        public Dictionary<int, DependencyItem> Model { get; set; }

        [Inject]
        public IDependenciesService Importer { private get; set; }
    }
}
