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
using Technewlogic.WpfDialogManagement;
using Technewlogic.WpfDialogManagement.Contracts;

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

            //kernel.Bind<IDialogManager>().To<DialogManager>().InSingletonScope();

            //kernel.Bind<ITfsService, TfsService>();
            //kernel.Bind<ICsvService, CsvService>();

            //kernel.Bind(typeof(ITfsService)).To(typeof(TfsService)).InSingletonScope();
            //kernel.Bind(typeof(ICsvService)).To(typeof(CsvService)).InSingletonScope();

            kernel.Bind<ITfsService>().To<TfsService>().InSingletonScope();
            kernel.Bind<ICsvService>().To<CsvService>().InSingletonScope();

            //kernel.Bind<IDependenciesService>().To<TfsService>().When(this.ChooseTfsImporter);
            //kernel.Bind<IDependenciesService>().To<CsvService>().When(this.ChooseCsvImporter);

            //kernel.Bind(typeof(IConnectorViewModel)).To(typeof(TfsConnectorViewModel)).Named("TfsConnectorViewModel");
            //kernel.Bind(typeof(IConnectorViewModel)).To(typeof(CsvConnectorViewModel)).Named("CsvConnectorViewModel");
            kernel.Bind<IConnectorViewModel>().To<TfsConnectorViewModel>().InSingletonScope().Named("TfsConnectorViewModel");
            kernel.Bind<IConnectorViewModel>().To<CsvConnectorViewModel>().InSingletonScope().Named("CsvConnectorViewModel");

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
            return ConfigurationManager.AppSettings["selectedConnector"].ToLower().Equals(target);
        }
    }
}
