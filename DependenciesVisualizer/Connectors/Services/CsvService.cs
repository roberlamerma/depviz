using System;
using System.Collections.Generic;
using DependenciesVisualizer.Model;
using DependenciesVisualizer.Contracts;

namespace DependenciesVisualizer.Connectors.Services
{
    public class CsvService : IDependenciesService, ICsvService
    {
        public Dictionary<int, DependencyItem> DependenciesModel { get; }
        public event EventHandler<EventArgs> DependenciesModelChanged;

        public void RaiseDependenciesModelChanged()
        {
            throw new NotImplementedException();
        }
    }
}
