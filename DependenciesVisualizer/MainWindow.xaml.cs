using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using DependenciesVisualizer.Connectors.UserControls;
using DependenciesVisualizer.ViewModels;
using log4net;
using Ninject;

namespace DependenciesVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ILog Logger { get; private set; }

        public MainWindow(IKernel ioc)
        {
            this.Logger = ioc.Get<ILog>();
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            //if (!Directory.Exists(ConfigurationManager.AppSettings["graphvizPath"]))
            //{
            //    var error = string.Format("Could not find the given GraphViz directory: '{0}'", ConfigurationManager.AppSettings["graphvizPath"]);
            //    this.Logger.Fatal(error);

            //    var messageError = string.Format("{0}.{1}{2} Check the path on this config file: '{3}.conf'", error, Environment.NewLine, Environment.NewLine, Assembly.GetEntryAssembly().Location);
            //    MessageBox.Show(messageError, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    Application.Current.Shutdown();
            //}

            GraphVizPathSelector graphVizPathSelector = null;

            if (!Directory.Exists(Properties.Settings.Default.graphvizPath))
            {
                graphVizPathSelector = new GraphVizPathSelector();
                Window window = new Window
                {
                    Title = "Select the Graphviz path",
                    Content = graphVizPathSelector,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    ResizeMode =  ResizeMode.NoResize
                };

                window.ShowDialog();
            }

            if (graphVizPathSelector != null && ((GraphVizPathSelectorViewModel)graphVizPathSelector.DataContext).CloseApplication)
            {
                Application.Current.Shutdown();
            }

            this.DataContext = new MainWindowViewModel(ioc);
            this.InitializeComponent();

            //var theViewModel = new MainViewModel(new DialogManager(this, this.Dispatcher));

            //if (theViewModel.IsAppValidAndReady)
            //{
            //    this.DataContext = theViewModel;

            //    theViewModel.BuildTreeView(ref this.Queries);
            //}
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            this.Logger.Fatal(string.Format("Object {0} produced a fatal unhanded exception: {1}{2}{3}", sender, e.Exception.Message, Environment.NewLine, e.Exception.StackTrace));
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


    }
}
