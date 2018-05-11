using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DependenciesVisualizer.Helpers;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Windows.Threading;
using DependenciesVisualizer.Connectors.ViewModels;

namespace DependenciesVisualizer.Helpers
{
    static class TreeViewHelper
    {
        private static Action<object, MouseButtonEventArgs> doubleClickDelegate;

        public static TfsRootFolderQueryItem BuildTreeViewFromTfs(QueryHierarchy queryHierarchy, string header, ICommand command)
        {
            TfsRootFolderQueryItem root = new TfsRootFolderQueryItem(null, header);

            foreach (var queryItem in queryHierarchy)
            {
                if (queryItem is QueryFolder qf)
                {
                    DefineRootFolder(qf, root, command);
                }
            }

            return root;
        }

        private static void DefineRootFolder(QueryFolder query, TfsQueryTreeItemViewModel parent, ICommand command)
        {
            TfsQueryTreeItemViewModel firstLevelFolder = null;

            if (query.IsPersonal)
            {
                firstLevelFolder = new TfsPersonalFolderQueryItem(parent, query.Name);
            }
            else if (query.Name == "Shared Queries")
            {
                firstLevelFolder = new TfsSharedFolderQueryItem(parent, query.Name);
            }
            else
            {
                return;
            }

            parent.Children.Add(firstLevelFolder);

            foreach (QueryItem subQuery in query)
            {
                if (subQuery.GetType() == typeof(QueryFolder))
                    DefineFolder((QueryFolder)subQuery, firstLevelFolder, command);
                else
                    DefineQuery((QueryDefinition)subQuery, firstLevelFolder, command);
            }
        }

        private static void DefineFolder(QueryFolder query, TfsQueryTreeItemViewModel parent, ICommand command)
        {
            TfsQueryTreeItemViewModel firstLevelFolder = new TfsFolderQueryItem(parent, query.Name); ;

            parent.Children.Add(firstLevelFolder);

            foreach (QueryItem subQuery in query)
            {
                if (subQuery.GetType() == typeof(QueryFolder))
                    DefineFolder((QueryFolder)subQuery, firstLevelFolder, command);
                else
                    DefineQuery((QueryDefinition)subQuery, firstLevelFolder, command);
            }
        }

        private static void DefineQuery(QueryDefinition query, TfsQueryTreeItemViewModel parent, ICommand command)
        {
            TfsQueryTreeItemViewModel queryTreeItem = null;

            switch (query.QueryType)
            {
                case QueryType.List:
                    queryTreeItem = new TfsFlatQueryItem(parent, query.Name, command, query.Id);
                    break;
                case QueryType.OneHop:
                    queryTreeItem = new TfsLinkedListQueryItem(parent, query.Name, command, query.Id);
                    break;
                case QueryType.Tree:
                    queryTreeItem = new TfsTreeQueryItem(parent, query.Name);
                    break;
                default:
                    return;
            }

            parent.Children.Add(queryTreeItem);
        }
    }
}
