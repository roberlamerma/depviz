using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DependenciesVisualizer.Services;
using DependenciesVisualizer.ViewModels;
using Ninject;

namespace DependenciesVisualizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            IKernel kernel = new StandardKernel();
            //kernel.Bind<ITfsService, TfsService>();
            kernel.Bind(typeof(ITfsService)).To(typeof(TfsService));
            kernel.Bind(typeof(IConnectorViewModel)).To(typeof(TfsConnectorViewModel)).Named("TfsConnectorViewModel");
            kernel.Bind(typeof(IConnectorViewModel)).To(typeof(CsvConnectorViewModel)).Named("CsvConnectorViewModel");

            this.MainWindow = new MainWindow(kernel);
            this.MainWindow.Show();

            base.OnStartup(e);
        }

    }
}
