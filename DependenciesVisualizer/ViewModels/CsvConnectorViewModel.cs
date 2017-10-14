using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependenciesVisualizer.ViewModels
{
    public class CsvConnectorViewModel : ViewModelBase, IConnectorViewModel
    {
        public string Name => "CSV";

        public void Initialize()
        {
        }
    }
}
