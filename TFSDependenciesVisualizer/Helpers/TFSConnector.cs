using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSDependencyVisualizer.Helpers
{
    static class TFSConnector
    {
        public static WorkItemStore GetWorkItemStore(Uri tfsUri)
        {
            var tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(tfsUri);
            return tfs.GetService<WorkItemStore>();
        }

        public static bool IsTFSUrlValid(string tfsUrl, string project, out Uri uriResult)
        {
            bool result = Uri.TryCreate(tfsUrl, UriKind.Absolute, out uriResult)
                && uriResult.Scheme == Uri.UriSchemeHttp;

            return result;
        }
    }
}
