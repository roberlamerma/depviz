using System;
using System.Collections;
using System.Collections.Generic;
using DependenciesVisualizer.Model;

namespace DependenciesVisualizer.Contracts
{
    public interface IDependenciesService
    {
        Dictionary<int, DependencyItem> DependenciesModel { get; }

        event EventHandler<EventArgs> DependenciesModelChanged;

        event EventHandler<EventArgs> DependenciesModelAboutToChange;

        void RaiseDependenciesModelChanged();
    }
}