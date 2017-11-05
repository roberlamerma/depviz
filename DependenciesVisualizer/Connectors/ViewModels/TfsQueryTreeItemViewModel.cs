using DependenciesVisualizer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependenciesVisualizer.Connectors.ViewModels
{
    public abstract class TfsQueryTreeItemViewModel : ViewModelBase
    {
        private readonly ObservableCollection<TfsQueryTreeItemViewModel> children;
        private readonly TfsQueryTreeItemViewModel parent;
        private readonly string name;

        private bool isExpanded;
        private bool isSelected;

        protected TfsQueryTreeItemViewModel(TfsQueryTreeItemViewModel parent, string name)
        {
            this.parent = parent;
            this.name = name;

            this.children = new ObservableCollection<TfsQueryTreeItemViewModel>();
        }

        public string Name { get => this.name; }

        public ObservableCollection<TfsQueryTreeItemViewModel> Children
        {
            get { return this.children; }
        }

        public bool IsExpanded
        {
            get { return this.isExpanded; }
            set
            {
                if (value != this.isExpanded)
                {
                    this.isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (this.isExpanded && this.parent != null)
                    this.parent.IsExpanded = true;
            }
        }

        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (value != this.isSelected)
                {
                    this.isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        public TfsQueryTreeItemViewModel Parent
        {
            get { return this.parent; }
        }
    }
}
