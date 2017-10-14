using System.Collections;
using System.Collections.Generic;
using DependenciesVisualizer.Model;

namespace DependenciesVisualizer.Contracts
{
    public interface IDependencyItemImporter
    {
        Dictionary<int, DependencyItem> Import();
    }
}