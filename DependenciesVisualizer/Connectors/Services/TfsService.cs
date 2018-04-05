using System;
using System.Collections.Generic;
using DependenciesVisualizer.Contracts;
using DependenciesVisualizer.Helpers;
using DependenciesVisualizer.Model;
using log4net;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Ninject;

namespace DependenciesVisualizer.Connectors.Services
{
    public class TfsService : IDependenciesService, ITfsService
    {
        /// <summary>
        /// Event arised when the DependenciesModel changed
        /// </summary>
        public event EventHandler<EventArgs> DependenciesModelChanged = delegate { };

        /// <summary>
        /// Event arised when the DependenciesModel is about to change
        /// </summary>
        public event EventHandler<EventArgs> DependenciesModelAboutToChange = delegate { };

        /// <summary>
        /// Event arised when the DependenciesModel could not be changed (error, exception...)
        /// </summary>
        public event EventHandler<EventArgs> DependenciesModelCouldNotBeChanged = delegate { };

        public ILog Logger { get; private set; }

        private Dictionary<int, DependencyItem> dependenciesModel;

        [Inject]
        public TfsService(ILog logger)
        {
            this.Logger = logger;
        }

        //public string ProjectName { get; private set; }

        //public Guid QueryId { get; set; }

        //public Uri Uri { get; set; }

        public void ImportDependenciesFromTfs(string projectName, Guid queryGuid)
        {
            var queryDef = this.WorkItemStore.GetQueryDefinition(queryGuid);

            var queryTextWithProject = queryDef.QueryText.Replace("@project", string.Format("'{0}'", projectName));

            var query = new Query(this.WorkItemStore, queryTextWithProject);

            if (!query.IsLinkQuery || query.IsTreeQuery)
            {
                //this.dialogManager
                //    .CreateMessageDialog(string.Format(@"The query '{0}' is not a 'Direct Link' query.", queryDef.Name), "ERROR", DialogMode.Ok)
                //    .Show();
                string error = string.Format(@"[TFS] The query '{0}' is not a 'Direct Link' query.", queryDef.Name);
                this.Logger.Error(error);
                throw new Exception(error);
            }
            else
            {
                //var dialog = this.dialogManager.
                //    CreateWaitDialog(string.Format(@"1.- Retreiving items from the '{0}' query. This operation might take a while{1}2.- After this message dissapears, search for your image '{2}.png' on this path: '{3}'", queryDef.Name, Environment.NewLine, queryDef.Name, Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)), DialogMode.None);


                // ToDo: Force that on each query run no cache is used
                var queryResults = query.RunLinkQuery();

                var theModel = new Dictionary<int, DependencyItem>();

                int successorsCount = 0;

                try
                {
                    this.RaiseDependenciesModelAboutToChange();

                    DependencyItem tempItem;

                    // Populate the model (parent id's and successors) from the query
                    foreach (WorkItemLinkInfo workItemInfo in queryResults)
                    {
                        if (workItemInfo.SourceId == 0) // parent
                        {
                            if (!theModel.ContainsKey(workItemInfo.TargetId))
                            {
                                tempItem = new DependencyItem(workItemInfo.TargetId);
                                theModel.Add(workItemInfo.TargetId, tempItem);
                                this.Logger.Debug(string.Format(@"[TFS] Got PARENT: {0}", tempItem.ToString()));
                            }
                        }
                        else // child
                        {
                            // ToDo: Make this also work with Predecessors queries
                            if (workItemInfo.LinkTypeId == 3) // successor
                            {
                                // Get the parent
                                theModel.TryGetValue(workItemInfo.SourceId, out var dependencyItem);
                                if (dependencyItem != null)
                                {
                                    dependencyItem.Successors.Add(workItemInfo.TargetId);
                                    successorsCount++;
                                }
                                else
                                {
                                    this.Logger.Debug(string.Format(@"[TFS] Could not get PARENT: {0} for SUCCESSOR: {1}", workItemInfo.SourceId, workItemInfo.TargetId));
                                }
                            }
                        }
                    }

                    if (successorsCount == 0)
                    {
                        this.RaiseDependenciesModelCouldNotBeChanged();
                        string error = string.Format("[TFS] The query '{0}' does not return any successors", queryDef.Name);
                        this.Logger.Error(error);
                        throw new Exception(error);
                    }

                    var successorsThatAreNotParents = new List<DependencyItem>();

                    // Add Title and Tags to items in the DependenciesModel
                    foreach (KeyValuePair<int, DependencyItem> entry in theModel)
                    {
                        if (entry.Value.Title == null)
                        {
                            var workItem = this.WorkItemStore.GetWorkItem(entry.Key);
                            entry.Value.Title = workItem.Title;
                            entry.Value.State = workItem.State;

                            entry.Value.Tags.AddRange(TfsHelper.GetTags(workItem));
                        }

                        foreach (var successor in entry.Value.Successors)
                        {
                            // If successors are not parents, retrieve them from TFS and add them
                            if (!theModel.ContainsKey(successor))
                            {
                                var workItem = this.WorkItemStore.GetWorkItem(successor);
                                var dependencyItem = new DependencyItem(successor) { Title = workItem.Title, State = workItem.State };
                                dependencyItem.Tags.AddRange(TfsHelper.GetTags(workItem));

                                successorsThatAreNotParents.Add(dependencyItem);
                            }
                        }
                    }

                    // Adde successors That Are Not Parents to the DependenciesModel
                    foreach (var successor in successorsThatAreNotParents)
                    {
                        if (!theModel.ContainsKey(successor.Id))
                        {
                            theModel.Add(successor.Id, successor);
                            this.Logger.Debug(string.Format(@"[TFS] Got SUCCESSOR: {0}", successor.ToString()));
                        }
                    }

                    //foreach (WorkItemLinkInfo workItemInfo in queryResults)
                    //{
                    //    if (!this.DependenciesModel.ContainsKey(workItemInfo.TargetId))
                    //    {
                    //        var dependencyListItem = this.GetDependencyListItemFromWorkItem(workItemInfo.TargetId, true);

                    //        this.DependenciesModel.Add(workItemInfo.TargetId, dependencyListItem);
                    //    }
                    //}

                    //// As a PBI might just appears on the query as a child, we make sure it is also added to the model
                    //foreach (KeyValuePair<int, DependencyItem> entry in this.DependenciesModel)
                    //{
                    //    if (entry.Value.Successors.Any())
                    //    {
                    //        foreach (var succesor in entry.Value.Successors)
                    //        {
                    //            if (!this.DependenciesModel.ContainsKey(succesor))
                    //            {
                    //                var dependencyListItem = this.GetDependencyListItemFromWorkItem(succesor, false);

                    //                this.DependenciesModel.Add(succesor, dependencyListItem);
                    //            }
                    //        }
                    //    }
                    //}



                    this.DependenciesModel = theModel;
                }
                catch (Exception ex)
                {
                    this.Logger.Error(string.Format(@"{0}{1}{2}",ex.Message, Environment.NewLine, ex.StackTrace));
                    this.RaiseDependenciesModelCouldNotBeChanged();
                    throw;
                }

            }
        }

        public WorkItemStore WorkItemStore { get; private set; }

        public void SetWorkItemStore(Uri tfsUri, string project)
        {
            var tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsUri);
            this.WorkItemStore = tfs.GetService<WorkItemStore>();
        }

        //private bool IsTfsUrlValid(string tfsUrl, string project, out Uri uriResult)
        //{
        //    bool result = Uri.TryCreate(tfsUrl, UriKind.Absolute, out uriResult)
        //                  && uriResult.Scheme == Uri.UriSchemeHttp;

        //    return result;
        //}
        public Dictionary<int, DependencyItem> DependenciesModel
        {
            get => this.dependenciesModel;
            private set
            {
                if (this.dependenciesModel == value)
                {
                    return;
                }

                this.dependenciesModel = value;
                this.RaiseDependenciesModelChanged();
            }
        }

        public void RaiseDependenciesModelChanged()
        {
            this.DependenciesModelChanged(this, EventArgs.Empty);
        }

        public void RaiseDependenciesModelAboutToChange()
        {
            this.DependenciesModelAboutToChange(this, EventArgs.Empty);
        }

        public void RaiseDependenciesModelCouldNotBeChanged()
        {
            this.DependenciesModelCouldNotBeChanged(this, EventArgs.Empty);
        }

        public void SetWorkItemStore(WorkItemStore store)
        {
            this.WorkItemStore = store;
        }
    }
}
