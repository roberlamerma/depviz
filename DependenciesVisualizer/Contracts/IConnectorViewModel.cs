#region Copyright
// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="IConnectorViewModel.cs" company="Roche Diagnostics International Ltd">
// //   Copyright (c) Roche Diagnostics International Ltd. All rights reserved.
// // </copyright>
// // <summary>
// //   
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------
#endregion
namespace DependenciesVisualizer.Contracts
{
    public interface IConnectorViewModel
    {
        string Name { get; }

        void Initialize();

        IDependencyItemImporter Service { get; set; }
    }
}