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
using Technewlogic.WpfDialogManagement;
using TFSDependencyVisualizer.Helpers;

namespace TFSDependenciesVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            var theViewModel = new MainViewModel(new DialogManager(this, this.Dispatcher));

            if (theViewModel.IsAppValidAndReady)
            {
                this.DataContext = theViewModel;
                //TreeViewHelper.BuildTreeViewFromTFS(
                //                                ref this.Queries,
                //                                theViewModel.WorkItemStore.Projects[ConfigurationManager.AppSettings["projectName"]].QueryHierarchy,
                //                                ConfigurationManager.AppSettings["projectName"],
                //                                theViewModel.DoubleClickDelegate);
                theViewModel.BuildTreeView(ref this.Queries);

            }
        }
    }
}
