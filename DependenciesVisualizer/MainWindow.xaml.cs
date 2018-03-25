using System;
using System.Windows;
using System.Windows.Threading;
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
