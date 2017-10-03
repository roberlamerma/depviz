using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TFSDependenciesVisualizer.Properties;

namespace TFSDependencyVisualizer.Helpers
{
    static class TreeViewHelper
    {
        private static Action<object, MouseButtonEventArgs> doubleClickDelegate;

        public static void BuildTreeViewFromTfs(ref TreeView queryTreeView, QueryHierarchy queryHierarchy, string header, Action<object, MouseButtonEventArgs> _doubleClickDelegate)
        {
            doubleClickDelegate = _doubleClickDelegate;

            TreeViewItem root = new TreeViewItem();
            root.Header = header;

            foreach (var queryItem in queryHierarchy)
            {
                if (queryItem is QueryFolder qf)
                {
                    DefineFolder(qf, root);
                }
            }

            queryTreeView.Items.Add(root);
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
