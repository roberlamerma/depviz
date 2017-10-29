using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependenciesVisualizer.Connectors.Services
{
    public interface ICsvService
    {
        void ImportDependenciesFromCsvFile(string csvFile);
    }
}
