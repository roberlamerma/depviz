using System;
using System.Collections.Generic;
using System.Configuration;
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
using DependenciesVisualizer.Helpers;
using DependenciesVisualizer.Services;
using DependenciesVisualizer.ViewModels;

namespace DependenciesVisualizer.UserControls
{
    /// <summary>
    /// Interaction logic for TfsConnector.xaml
    /// </summary>
    public partial class TfsConnector : UserControl
    {
        public TfsConnector()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ((TfsConnectorViewModel)this.DataContext).BuildTreeView(ref this.Queries);
        }

    }
}
