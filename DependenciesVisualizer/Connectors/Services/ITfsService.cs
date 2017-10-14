#region Copyright
// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="ITfsService.cs" company="Roche Diagnostics International Ltd">
// //   Copyright (c) Roche Diagnostics International Ltd. All rights reserved.
// // </copyright>
// // <summary>
// //   
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------
#endregion

using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace DependenciesVisualizer.Connectors.Services
{
    public interface ITfsService
    {
        WorkItemStore WorkItemStore { get; }

        void SetWorkItemStore(Uri tfsUri, string project);
    }
}