using System.Windows;
using DependenciesVisualizer.Connectors.Services;
using DependenciesVisualizer.Connectors.ViewModels;
using DependenciesVisualizer.Contracts;
using log4net;
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

            //kernel.Bind<IDialogManager>().To<DialogManager>().InSingletonScope();

            //kernel.Bind<ITfsService, TfsService>();
            //kernel.Bind<ICsvService, CsvService>();

            //kernel.Bind(typeof(ITfsService)).To(typeof(TfsService)).InSingletonScope();
            //kernel.Bind(typeof(ICsvService)).To(typeof(CsvService)).InSingletonScope();

            //kernel.Bind<ILog>().ToMethod(context => LogManager.GetLogger(context.Request.Target.Member.ReflectedType));
            kernel.Bind<ILog>().ToMethod(context => LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType));

            
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
    }
}
