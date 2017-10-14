using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DependenciesVisualizer.Connectors.Services;
using DependenciesVisualizer.Connectors.ViewModels;
using DependenciesVisualizer.Contracts;
using DependenciesVisualizer.ViewModels;
using Ninject;
using Ninject.Activation;

namespace DependenciesVisualizer.Model
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
            kernel.Bind(typeof(ITfsService)).To(typeof(TfsService)).InSingletonScope();

            kernel.Bind<IDependencyItemImporter>().To<TfsService>().When(this.ChooseTfsImporter);
            kernel.Bind<IDependencyItemImporter>().To<TfsService>().When(this.ChooseCsvImporter);

            kernel.Bind(typeof(IConnectorViewModel)).To(typeof(TfsConnectorViewModel)).Named("TfsConnectorViewModel");
            kernel.Bind(typeof(IConnectorViewModel)).To(typeof(CsvConnectorViewModel)).Named("CsvConnectorViewModel");

            this.MainWindow = new MainWindow(kernel);
            this.MainWindow.Show();

            base.OnStartup(e);
        }

        private bool ChooseTfsImporter(IRequest request)
        {
            return this.ChooseImporter("tfs");
        }

        private bool ChooseCsvImporter(IRequest request)
        {
            return this.ChooseImporter("csv");
        }

        private bool ChooseImporter(string target)
        {
            return ConfigurationManager.AppSettings["tfsprojectName"].ToLower().Equals(target);
        }
    }
}
