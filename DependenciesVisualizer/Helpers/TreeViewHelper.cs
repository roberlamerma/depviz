using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DependenciesVisualizer.Helpers;
using DependenciesVisualizer.Model.Model.Properties;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Windows.Threading;
using DependenciesVisualizer.Connectors.ViewModels;

namespace DependenciesVisualizer.Helpers
{
    static class TreeViewHelper
    {
        private static Action<object, MouseButtonEventArgs> doubleClickDelegate;

        public static TfsRootFolderQueryItem BuildTreeViewFromTfs2(QueryHierarchy queryHierarchy, string header, ICommand command)
        {
            TfsRootFolderQueryItem root = new TfsRootFolderQueryItem(null, header);

            foreach (var queryItem in queryHierarchy)
            {
                if (queryItem is QueryFolder qf)
                {
                    DefineFolder2(qf, root, command);
                }
            }

            return root;
        }

        private static void DefineFolder2(QueryFolder query, TfsQueryTreeItemViewModel parent, ICommand command)
        {
            //TreeViewItem item = new TreeViewItem();
            //QueryTypes type = QueryTypes.Folder;

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
                return;//firstLevelFolder = new TfsFolderQueryItem(parent, query.Name);
            }

            parent.Children.Add(firstLevelFolder);

            foreach (QueryItem subQuery in query)
            {
                if (subQuery.GetType() == typeof(QueryFolder))
                    DefineFolder3((QueryFolder)subQuery, firstLevelFolder, command);
                else
                    DefineQuery2((QueryDefinition)subQuery, firstLevelFolder, command);
            }
        }

        private static void DefineFolder3(QueryFolder query, TfsQueryTreeItemViewModel parent, ICommand command)
        {
            //TreeViewItem item = new TreeViewItem();
            //QueryTypes type = QueryTypes.Folder;

            TfsQueryTreeItemViewModel firstLevelFolder = new TfsFolderQueryItem(parent, query.Name); ;

            parent.Children.Add(firstLevelFolder);

            foreach (QueryItem subQuery in query)
            {
                if (subQuery.GetType() == typeof(QueryFolder))
                    DefineFolder3((QueryFolder)subQuery, firstLevelFolder, command);
                else
                    DefineQuery2((QueryDefinition)subQuery, firstLevelFolder, command);
            }
        }

        private static void DefineQuery2(QueryDefinition query, TfsQueryTreeItemViewModel parent, ICommand command)
        {
            //TreeViewItem item = new TreeViewItem();
            //QueryTypes type;

            TfsQueryTreeItemViewModel queryTreeItem = null;

            switch (query.QueryType)
            {
                case QueryType.List:
                    queryTreeItem = new TfsFlatQueryItem(parent, query.Name);
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

        public static void BuildTreeViewFromTfs(TreeView queryTreeView, Dispatcher uiThread, QueryHierarchy queryHierarchy, string header, Action<object, MouseButtonEventArgs> _doubleClickDelegate)
        {
            uiThread.Invoke(() =>
            {
                doubleClickDelegate = _doubleClickDelegate;

                TreeViewItem root = new TreeViewItem();
                queryTreeView.Items.Clear();
                root.Header = header;

                foreach (var queryItem in queryHierarchy)
                {
                    if (queryItem is QueryFolder qf)
                    {
                        DefineFolder(qf, root);
                    }
                }

                queryTreeView.Items.Add(root);
            });
        }

        private static void DefineFolder(QueryFolder query, TreeViewItem father)
        {
            TreeViewItem item = new TreeViewItem();
            QueryTypes type = QueryTypes.Folder;

            if (query.IsPersonal) type = QueryTypes.MyQ;
            else if (query.Name == "Team Queries") type = QueryTypes.TeamQ;

            item.Header = CreateTreeItem(query.Name, type);

            father.Items.Add(item);

            foreach (QueryItem subQuery in query)
            {
                if (subQuery.GetType() == typeof(QueryFolder))
                    DefineFolder((QueryFolder)subQuery, item);
                else
                    DefineQuery((QueryDefinition)subQuery, item);
            }
        }

        private static void DefineQuery(QueryDefinition query, TreeViewItem queryFolder)
        {
            TreeViewItem item = new TreeViewItem();
            QueryTypes type;

            switch (query.QueryType)
            {
                case QueryType.List: type = QueryTypes.FView; break;
                case QueryType.OneHop: type = QueryTypes.DView; break;
                case QueryType.Tree: type = QueryTypes.HView; break;
                default: type = QueryTypes.None; break;
            }

            item.Header = CreateTreeItem(query.Name, type);
            item.Tag = query.Id;
            item.MouseDoubleClick += new MouseButtonEventHandler(doubleClickDelegate);
            queryFolder.Items.Add(item);
        }

        private static StackPanel CreateTreeItem(string value, QueryTypes type)
        {
            StackPanel stake = new StackPanel();
            stake.Orientation = Orientation.Horizontal;

            Image img = new Image();
            img.Stretch = Stretch.Uniform;
            img.Source = GetImage(type);
            Label lbl = new Label();
            lbl.Content = value;

            stake.Children.Add(img);
            stake.Children.Add(lbl);

            return stake;
        }

        private static BitmapSource GetImage(QueryTypes type)
        {
            switch (type)
            {
                case QueryTypes.MyQ:
                    return DisplayImage.GetImageSource(Resources.MyQ);
                case QueryTypes.TeamQ:
                    return DisplayImage.GetImageSource(Resources.TeamQ);
                case QueryTypes.Folder:
                    return DisplayImage.GetImageSource(Resources.Folder);
                case QueryTypes.FView:
                    return DisplayImage.GetImageSource(Resources.FView);
                case QueryTypes.DView:
                    return DisplayImage.GetImageSource(Resources.DView);
                case QueryTypes.HView:
                    return DisplayImage.GetImageSource(Resources.HView);
                default:
                    return null;
            }
        }
    }
}
