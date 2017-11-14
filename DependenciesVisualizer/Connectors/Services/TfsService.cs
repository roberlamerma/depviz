using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependenciesVisualizer.Connectors.Services;
using DependenciesVisualizer.Contracts;
using DependenciesVisualizer.Helpers;
using DependenciesVisualizer.Model;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Ninject;

namespace DependenciesVisualizer.Connectors.Services
{
    public class TfsService : IDependenciesService, ITfsService
    {
        private Dictionary<int, DependencyItem> dependenciesModel;

        [Inject]
        public TfsService()
        {
            this.dependenciesModel = new Dictionary<int, DependencyItem>();
        }

        public string ProjectName { get; private set; }

        //public Guid QueryId { get; set; }

        //public Uri Uri { get; set; }

        public void ImportDependenciesFromTfs(Guid queryGuid)
        {
            var queryDef = this.WorkItemStore.GetQueryDefinition(queryGuid);

            var queryTextWithProject = queryDef.QueryText.Replace("@project", string.Format("'{0}'", this.ProjectName));

            var query = new Query(this.WorkItemStore, queryTextWithProject);

            if (!query.IsLinkQuery || query.IsTreeQuery)
            {
                //this.dialogManager
                //    .CreateMessageDialog(string.Format(@"The query '{0}' is not a 'Direct Link' query.", queryDef.Name), "ERROR", DialogMode.Ok)
                //    .Show();
                throw new Exception(string.Format(@"The query '{0}' is not a 'Direct Link' query.", queryDef.Name));
            }
            else
            {
                //var dialog = this.dialogManager.
                //    CreateWaitDialog(string.Format(@"1.- Retreiving items from the '{0}' query. This operation might take a while{1}2.- After this message dissapears, search for your image '{2}.png' on this path: '{3}'", queryDef.Name, Environment.NewLine, queryDef.Name, Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)), DialogMode.None);


                // ToDo: Force that on each query run no cache is used
                var queryResults = query.RunLinkQuery();

                var theModel = new Dictionary<int, DependencyItem>();

                int successorsCount = 0;

                // Populate the model (parent id's and successors) from the query
                foreach (WorkItemLinkInfo workItemInfo in queryResults)
                {
                    if (workItemInfo.SourceId == 0) // parent
                    {
                        if (!theModel.ContainsKey(workItemInfo.TargetId))
                        {
                            theModel.Add(workItemInfo.TargetId, new DependencyItem(workItemInfo.TargetId));
                        }
                    }
                    else // child
                    {
                        // ToDo: Make this also work with Predecessors queries
                        if (workItemInfo.LinkTypeId == 3) // successor
                        {
                            theModel.TryGetValue(workItemInfo.SourceId, out var dependencyItem);
                            if (dependencyItem != null)
                            {
                                dependencyItem.Successors.Add(workItemInfo.TargetId);
                                successorsCount++;
                            }
                        }
                    }
                }

                if (successorsCount == 0)
                {
                    throw new Exception(string.Format("The query '{0}' does not return any successors", queryDef.Name));
                }

                var successorsThatAreNotParents = new List<DependencyItem>();

                // Add Title and Tags to items in the DependenciesModel
                foreach (KeyValuePair<int, DependencyItem> entry in theModel)
                {
                    if (entry.Value.Title == null)
                    {
                        var workItem = this.WorkItemStore.GetWorkItem(entry.Key);
                        entry.Value.Title = workItem.Title;

                        entry.Value.Tags.AddRange(TfsHelper.GetTags(workItem));
                    }

                    foreach (var successor in entry.Value.Successors)
                    {
                        // If successors are not parents, retrieve them from TFS and add them
                        if (!theModel.ContainsKey(successor))
                        {
                            var workItem = this.WorkItemStore.GetWorkItem(successor);
                            var dependencyItem = new DependencyItem(successor) { Title = workItem.Title };
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
        }

        public WorkItemStore WorkItemStore { get; private set; }

        public void SetWorkItemStore(Uri tfsUri, string project)
        {
            var tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsUri);
            this.WorkItemStore = tfs.GetService<WorkItemStore>();

            this.ProjectName = project;
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

        public event EventHandler<EventArgs> DependenciesModelChanged = delegate { };

        public void RaiseDependenciesModelChanged()
        {
            this.DependenciesModelChanged(this, EventArgs.Empty);
        }
    }
}
