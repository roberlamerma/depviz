using System.Windows.Controls;
using DependenciesVisualizer.Connectors.ViewModels;
using DependenciesVisualizer.ViewModels;

namespace DependenciesVisualizer.Connectors.UserControls
{
    /// <summary>
    /// Interaction logic for TfsUriAndProjectSelector.xaml
    /// </summary>
    public partial class GraphVizPathSelector : UserControl
    {
        public GraphVizPathSelector()
        {
            InitializeComponent();
            this.DataContext = new GraphVizPathSelectorViewModel();
        }
    }
}
