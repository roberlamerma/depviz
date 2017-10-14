using System.Collections;
using System.Collections.Generic;

namespace DependenciesVisualizer.Services
{
    public interface IDependencyItemImporter
    {
        Dictionary<int, DependencyItem> Import();
    }
}