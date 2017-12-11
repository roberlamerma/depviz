using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DependenciesVisualizer.Connectors.ViewModels;

namespace DependenciesVisualizer.Connectors.UserControls
{
    /// <summary>
    /// Interaction logic for TfsUriAndProjectSelector.xaml
    /// </summary>
    public partial class TfsUriAndProjectSelector : UserControl
    {
        public TfsUriAndProjectSelector()
        {
            InitializeComponent();
            this.DataContext = new TfsUriAndProjectSelectorViewModel();
        }
    }
}
