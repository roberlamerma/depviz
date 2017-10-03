﻿using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;

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

        public static IEnumerable<int> GetLinksOfType(WorkItem workItem, string type)
        {
            foreach (var link in workItem.Links)
            {
                if (link is RelatedLink rl && rl.LinkTypeEnd.Name.Equals(type))
                {
                    yield return rl.RelatedWorkItemId;
                }
            }
        }

        public static IEnumerable<string> GetTags(WorkItem workItem)
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
