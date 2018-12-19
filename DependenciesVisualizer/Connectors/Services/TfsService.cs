using System;
using System.Collections.Generic;
using System.IO;
using DependenciesVisualizer.Contracts;
using DependenciesVisualizer.Helpers;
using DependenciesVisualizer.Model;
using log4net;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Ninject;

namespace DependenciesVisualizer.Connectors.Services
{
    public enum LinkType
    {
        Predecessors = -3,
        Successors = 3
    }

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

        /// <summary>
        /// Obtains dependencies (predecessors/succcesors) for the given PBI, and raises an event informing about this.
        /// </summary>
        /// <param name="pbiId">Product Backlog Item Id</param>
        public void ImportDependenciesFromTfs(int pbiId)
        {
            try
            {
                this.RaiseDependenciesModelAboutToChange();

                byte successorsDepth = 3;
                byte predecessorsDepth = 3;

                var workItem = this.WorkItemStore.GetWorkItem(pbiId);

                var theModel = new Dictionary<int, DependencyItem>();

                DependencyItem mainDependencyItem = new DependencyItem(pbiId) { Title = workItem.Title, State = workItem.State, Comment = Path.GetFileName(workItem.IterationPath) };
                mainDependencyItem.Tags.AddRange(TfsHelper.GetTags(workItem));
                theModel.Add(pbiId, mainDependencyItem);

                this.GetPredecessorsRec(theModel, workItem, predecessorsDepth);
                this.GetSuccessorsRec(theModel, workItem, successorsDepth);

                this.DependenciesModel = theModel;
                this.RaiseDependenciesModelChanged();

            }
            catch (Exception ex)
            {
                this.Logger.Error(string.Format(@"{0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace));
                this.RaiseDependenciesModelCouldNotBeChanged();
                throw;
            }
        }

        /// <summary>
        /// Obtains dependencies (predecessors/succcesors) from the given query, and raises an event informing about this. 
        /// 
        /// Flat Queries: gets dependencies (predecessors/succcesors) for all the PBI's retrieved by the query.
        /// Link Queries: linked items should be successors. The obtained dependencies match the items retrieved by the query.
        /// Tree Queries: not supported.
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="queryGuid"></param>
        public void ImportDependenciesFromTfs(string projectName, Guid queryGuid)
        {
            try
            {
                this.RaiseDependenciesModelAboutToChange();

                var queryDef = this.WorkItemStore.GetQueryDefinition(queryGuid);
                var queryTextWithProject = queryDef.QueryText.Replace("@project", string.Format("'{0}'", projectName));
                var query = new Query(this.WorkItemStore, queryTextWithProject);

                var theModel = new Dictionary<int, DependencyItem>();

                if (query.IsTreeQuery)
                {
                    throw new Exception(string.Format(@"[TFS] Tree queries like '{0}' are not supported.", queryDef.Name));
                }
                else if (query.IsLinkQuery)
                {
                    this.PopulateDependenciesWithLinkQuery(theModel, query, queryDef);
                }
                else // Flat list Query
                {
                    this.PopulateDependenciesWithFlatQuery(theModel, query, queryDef);
                }

                this.DependenciesModel = theModel;
                this.RaiseDependenciesModelChanged();

            } catch (Exception ex)
            {
                this.Logger.Error(string.Format(@"{0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace));
                this.RaiseDependenciesModelCouldNotBeChanged();
                throw;
            }
        }

        private void PopulateDependenciesWithLinkQuery(Dictionary<int, DependencyItem> dependenciesModel, Query query, QueryDefinition queryDef)
        {
            var queryResults = query.RunLinkQuery();

            int successorsCount = 0;

            DependencyItem tempItem;

            // Populate the model (parent id's and successors) from the query
            foreach (WorkItemLinkInfo workItemInfo in queryResults)
            {
                if (workItemInfo.SourceId == 0) // parent
                {
                    if (!dependenciesModel.ContainsKey(workItemInfo.TargetId))
                    {
                        tempItem = new DependencyItem(workItemInfo.TargetId);
                        dependenciesModel.Add(workItemInfo.TargetId, tempItem);
                        this.Logger.Debug(string.Format(@"[TFS] Got PARENT: {0}", tempItem.ToString()));
                    }
                }
                else // child
                {
                    // ToDo: Make this also work with Predecessors queries
                    if (workItemInfo.LinkTypeId == 3) // successor
                    {
                        // Get the parent
                        dependenciesModel.TryGetValue(workItemInfo.SourceId, out var dependencyItem);
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
                throw new Exception(string.Format("[TFS] The query '{0}' does not return any successors", queryDef.Name));
            }

            var successorsThatAreNotParents = new List<DependencyItem>();

            // Add Title and Tags to items in the DependenciesModel
            foreach (KeyValuePair<int, DependencyItem> entry in dependenciesModel)
            {
                if (entry.Value.Title == null)
                {
                    var workItem = this.WorkItemStore.GetWorkItem(entry.Key);
                    entry.Value.Title = workItem.Title;
                    entry.Value.State = workItem.State;
                    entry.Value.Comment = Path.GetFileName(workItem.IterationPath);

                    entry.Value.Tags.AddRange(TfsHelper.GetTags(workItem));
                }

                foreach (var successor in entry.Value.Successors)
                {
                    // If successors are not parents, retrieve them from TFS and add them
                    if (!dependenciesModel.ContainsKey(successor))
                    {
                        var workItem = this.WorkItemStore.GetWorkItem(successor);
                        var dependencyItem = new DependencyItem(successor) { Title = workItem.Title, State = workItem.State, Comment = Path.GetFileName(workItem.IterationPath) };
                        dependencyItem.Tags.AddRange(TfsHelper.GetTags(workItem));

                        successorsThatAreNotParents.Add(dependencyItem);
                    }
                }
            }

            // Add successors That Are Not Parents to the DependenciesModel
            foreach (var successor in successorsThatAreNotParents)
            {
                if (!dependenciesModel.ContainsKey(successor.Id))
                {
                    dependenciesModel.Add(successor.Id, successor);
                    this.Logger.Debug(string.Format(@"[TFS] Got SUCCESSOR: {0}", successor.ToString()));
                }
            }
        }

        private void PopulateDependenciesWithFlatQuery(Dictionary<int, DependencyItem> dependenciesModel, Query query, QueryDefinition queryDef)
        {
            byte successorsDepth = 3;
            byte predecessorsDepth = 3;
            int pbis = 0;

            var queryResults = query.RunQuery();

            foreach (WorkItem workItem in queryResults)
            {
                if (workItem.Type.Name != "Product Backlog Item")
                {
                    continue;
                }

                pbis++;

                if (!dependenciesModel.ContainsKey(workItem.Id))
                {
                    DependencyItem mainDependencyItem = new DependencyItem(workItem.Id) { Title = workItem.Title, State = workItem.State, Comment = Path.GetFileName(workItem.IterationPath) };
                    mainDependencyItem.Tags.AddRange(TfsHelper.GetTags(workItem));
                    dependenciesModel.Add(workItem.Id, mainDependencyItem);
                }

                // Todo: optimize this rec's, these used to be inside the if above
                this.GetPredecessorsRec(dependenciesModel, workItem, predecessorsDepth);
                this.GetSuccessorsRec(dependenciesModel, workItem, successorsDepth);
                
            }

            if (pbis == 0)
            {
                throw new Exception(string.Format("[TFS] The query '{0}' does not have any PBI's", queryDef.Name));
            }
        }

        private void GetPredecessorsRec(Dictionary<int, DependencyItem> model, WorkItem parent, byte level)
        {
            if (level == 0)
            {
                return;
            }
            level--;

            DependencyItem innerDependencyItem;

            var predecessors = this.GetPredecessorsFromTfs(parent);

            foreach (var predecessor in predecessors)
            {
                innerDependencyItem = new DependencyItem(predecessor.Id) { Title = predecessor.Title, State = predecessor.State, Comment = Path.GetFileName(predecessor.IterationPath) };
                innerDependencyItem.Tags.AddRange(TfsHelper.GetTags(predecessor));


                try
                {
                    if (!model.ContainsKey(predecessor.Id))
                    {
                        model.Add(predecessor.Id, innerDependencyItem);
                    }

                    if (!model[predecessor.Id].Successors.Contains(parent.Id))
                    {
                        model[predecessor.Id].Successors.Add(parent.Id);
                    }
                }
                catch (Exception)
                {

                    throw;
                }

                this.GetPredecessorsRec(model, predecessor, level);
            }
        }

        private void GetSuccessorsRec(Dictionary<int, DependencyItem> model, WorkItem parent, byte level)
        {
            if (level == 0)
            {
                return;
            }
            level--;

            DependencyItem innerDependencyItem;

            var successors = this.GetSuccessorsFromTfs(parent);
            foreach (var successor in successors)
            {
                innerDependencyItem = new DependencyItem(successor.Id) { Title = successor.Title, State = successor.State, Comment = Path.GetFileName(successor.IterationPath) };
                innerDependencyItem.Tags.AddRange(TfsHelper.GetTags(successor));

                try
                {
                    if (!model[parent.Id].Successors.Contains(successor.Id))
                    {
                        model[parent.Id].Successors.Add(successor.Id);
                    }

                    if (!model.ContainsKey(successor.Id))
                    {
                        model.Add(successor.Id, innerDependencyItem);
                    }
                }
                catch (Exception)
                {

                    throw;
                }
                
                this.GetSuccessorsRec(model, successor, level);
            }

        }

        private IEnumerable<WorkItem> GetSuccessorsFromTfs(WorkItem workItem)
        {
            return this.GetLinkTypeFromTfs(workItem, LinkType.Successors);
        }

        private IEnumerable<WorkItem> GetPredecessorsFromTfs(WorkItem workItem)
        {
            return this.GetLinkTypeFromTfs(workItem, LinkType.Predecessors);
        }

        private IEnumerable<WorkItem> GetLinkTypeFromTfs(WorkItem workItem, LinkType linkType)
        {
            foreach (var link in workItem.Links)
            {
                if (link is RelatedLink)
                {
                    var relatedLink = (RelatedLink)link;
                    if ((LinkType)relatedLink.LinkTypeEnd.Id == linkType)
                    {
                        workItem = this.WorkItemStore.GetWorkItem(relatedLink.RelatedWorkItemId);
                        yield return workItem;
                    }
                }
            }
        }

        public WorkItemStore WorkItemStore { get; private set; }

        public void SetWorkItemStore(Uri tfsUri, string project)
        {
            var tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsUri);
            this.WorkItemStore = tfs.GetService<WorkItemStore>();
        }

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
