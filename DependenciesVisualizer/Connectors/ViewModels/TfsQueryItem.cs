using System;
using System.Windows;
using System.Windows.Input;

namespace DependenciesVisualizer.Connectors.ViewModels
{
    public class TfsTreeQueryItem : TfsQueryTreeItemViewModel
    {
        public TfsTreeQueryItem(TfsQueryTreeItemViewModel parent, string name) : base(parent, name)
        {
        }
    }

    public class TfsFlatQueryItem : TfsQueryTreeItemViewModel
    {
        public TfsFlatQueryItem(TfsQueryTreeItemViewModel parent, string name, ICommand command, Guid guid) : base(parent, name)
        {
            this.RenderDependenciesImageFromQuery = command;
            this.QueryId = guid;
        }

        public ICommand RenderDependenciesImageFromQuery { get; private set; }

        public Guid QueryId { get; private set; }
    }

    public class TfsLinkedListQueryItem : TfsQueryTreeItemViewModel
    {
        public TfsLinkedListQueryItem(TfsQueryTreeItemViewModel parent, string name, ICommand command, Guid guid) : base(parent, name)
        {
            this.RenderDependenciesImageFromQuery = command;
            this.QueryId = guid;
        }

        public ICommand RenderDependenciesImageFromQuery { get; private set; }

        public Guid QueryId { get; private set; }

    }

    public class TfsFolderQueryItem : TfsQueryTreeItemViewModel
    {
        public TfsFolderQueryItem(TfsQueryTreeItemViewModel parent, string name) : base(parent, name)
        {
        }
    }

    public class TfsSharedFolderQueryItem : TfsQueryTreeItemViewModel
    {
        public TfsSharedFolderQueryItem(TfsQueryTreeItemViewModel parent, string name) : base(parent, name)
        {
        }
    }

    public class TfsPersonalFolderQueryItem : TfsQueryTreeItemViewModel
    {
        public TfsPersonalFolderQueryItem(TfsQueryTreeItemViewModel parent, string name) : base(parent, name)
        {
        }
    }

    public class TfsRootFolderQueryItem : TfsQueryTreeItemViewModel
    {
        public TfsRootFolderQueryItem(TfsQueryTreeItemViewModel parent, string name) : base(parent, name)
        {
        }
    }

    //public abstract class TfsQueryItem : TfsQueryTreeItemViewModel
    //{
    //    protected TfsQueryItem(TfsQueryTreeItemViewModel parent, string name) : base(parent, name)
    //    {
    //    }
    //}
}
