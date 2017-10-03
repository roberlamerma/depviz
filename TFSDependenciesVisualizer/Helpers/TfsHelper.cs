using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSDependencyVisualizer.Helpers
{
    static class TfsHelper
    {
        public static WorkItemStore GetWorkItemStore(Uri tfsUri)
        {
            var tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsUri);
            return tfs.GetService<WorkItemStore>();
        }

        public static bool IsTfsUrlValid(string tfsUrl, string project, out Uri uriResult)
        {
            bool result = Uri.TryCreate(tfsUrl, UriKind.Absolute, out uriResult)
                && uriResult.Scheme == Uri.UriSchemeHttp;

            return result;
        }

        public static DependencyListItem GetDependencyListItemFromWorkItem(WorkItemStore store, int targetId)
        {
            var workItem = store.GetWorkItem(targetId);

            var dependencyListItem = new DependencyListItem() { Id = targetId, Title = workItem.Title };

            // here we retrieve the succesors
            dependencyListItem.Successors.AddRange(GetLinksOfType(workItem, "Successor"));

            // Here we retrieve the tags
            dependencyListItem.Tags.AddRange(GetTags(workItem));

            return dependencyListItem;
        }

        private static IEnumerable<int> GetLinksOfType(WorkItem workItem, string type)
        {
            foreach (var link in workItem.Links)
            {
                if (link is RelatedLink rl && rl.LinkTypeEnd.Name.Equals(type))
                {
                    yield return rl.RelatedWorkItemId;
                }
            }
        }

        private static IEnumerable<string> GetTags(WorkItem workItem)
        {
            foreach (Field field in workItem.Fields)
            {
                if (field.Name.Equals("Tags") && field.Value is string s && !string.IsNullOrEmpty(s))
                {
                    var nonSpacesString = s.Replace(" ", string.Empty);

                    if (nonSpacesString.Contains(";"))
                    {
                        foreach (var tag in (IEnumerable<string>)nonSpacesString.Split(';').GetEnumerator()) yield return tag;
                    }
                    else
                    {
                        yield return nonSpacesString;
                    }

                    break;
                }
            }
        }
    }
}
