using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using System.Windows.Input;
using DependenciesVisualizer.Contracts;
using DependenciesVisualizer.Helpers;
using DependenciesVisualizer.Model;
using Ninject;
using Microsoft.Win32;
using Shields.GraphViz.Services;
using Shields.GraphViz.Components;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DependenciesVisualizer.Connectors.UserControls;
using System.Windows.Media.Imaging;
using log4net;
using System.Windows.Media;

namespace DependenciesVisualizer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        //public ObservableCollection<string> ConnectorNames { get; private set; }

        public ObservableCollection<IConnectorViewModel> Connectors { get; private set; }

        public IKernel Ioc { get; private set; }

        public ICommand ZoomInCommand { get; private set; }
        public ICommand ZoomOutCommand { get; private set; }
        public ICommand GoToDepvizWikiHowto { get; private set; }
        public ICommand GoToGraphvizHome { get; private set; }
        public ICommand GoToDepvizHome { get; private set; }

        private IConnectorViewModel currentConnectorViewModel;

        private ILog logger;

        public MainWindowViewModel(IKernel ioc)
        {
            this.IsLoading = false;

            this.Ioc = ioc;

            this.logger = this.Ioc.TryGet<ILog>();

            this.Connectors = new ObservableCollection<IConnectorViewModel>(this.Ioc.GetAll<IConnectorViewModel>());

            foreach (var vm in this.Connectors)
            {
                if (vm.Name.ToLower().Equals(Properties.Settings.Default.selectedConnector.ToLower()))
                {
                    this.CurrentConnectorViewModel = vm;
                }
            }

            this.SelectConnector = new RelayCommand<IConnectorViewModel>(ExecuteSelectConnector, o => true );

            this.RenderAndDownloadDependenciesAsImage = new RelayCommand<string>(this.ExecuteRenderAndDownloadDependenciesAsImage, o => this.IsRenderable);

            this.ConfigureConnector = new RelayCommand<string>(this.ExecuteConfigureConnector, o => true);

            this.ZoomInCommand = new RelayCommand<object>(this.ExecuteZoomInCommand, o => true);
            this.ZoomOutCommand = new RelayCommand<object>(this.ExecuteZoomOutCommand, o => true);

            this.GoToDepvizWikiHowto = new RelayCommand<IConnectorViewModel>(ExecuteGoToDepvizWikiHowto, o => true);
            this.GoToGraphvizHome = new RelayCommand<IConnectorViewModel>(ExecuteGoToGraphvizHome, o => true);
            this.GoToDepvizHome = new RelayCommand<IConnectorViewModel>(ExecuteGoToDepvizHome, o => true);
        }

        private void ExecuteGoToDepvizHome(object obj)
        {
            System.Diagnostics.Process.Start("https://github.com/roberlamerma/depviz");
        }

        private void ExecuteGoToGraphvizHome(object obj)
        {
            System.Diagnostics.Process.Start("https://www.graphviz.org/");
        }

        private void ExecuteGoToDepvizWikiHowto(object obj)
        {
            System.Diagnostics.Process.Start("https://github.com/roberlamerma/depviz/wiki/How-to-use-depviz");
        }

        private void ExecuteZoomOutCommand(object obj)
        {
            ((System.Windows.Controls.Primitives.RangeBase)obj).Value -= 0.05;
        }

        private void ExecuteZoomInCommand(object obj)
        {
            ((System.Windows.Controls.Primitives.RangeBase)obj).Value += 0.05;
        }

        private void ExecuteRenderAndDownloadDependenciesAsImage(string fileType)
        {
            var graph = GraphVizHelper.CreateDependencyGraph(this.currentConnectorViewModel.DependenciesService.DependenciesModel, Properties.Settings.Default.maxLineLength);

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            //saveFileDialog1.Filter = "Png Image|*.png|Svg Image|*.svg";
            saveFileDialog1.Filter = (fileType == "png" ? "Png Image|*.png" : "Svg Image|*.svg");
            saveFileDialog1.Title = "Save dependencies as... (image)";
            bool? result = saveFileDialog1.ShowDialog();

            if (result == true && !string.IsNullOrWhiteSpace(saveFileDialog1.FileName))
            {
                IRenderer renderer = new Renderer(Properties.Settings.Default.graphvizPath);

                using (Stream fileStream = File.Create(saveFileDialog1.FileName))
                {
                    Task.Run(async () =>
                    {
                        await renderer.RunAsync(graph, 
                            fileStream, 
                            RendererLayouts.Dot, 
                            (fileType == "png" ? RendererFormats.Png : RendererFormats.Svg),
                            CancellationToken.None);
                    }).Wait();
                }
            }
        }

        private void ExecuteSelectConnector(IConnectorViewModel connectorViewModel)
        {
            this.CurrentConnectorViewModel = connectorViewModel;
            Properties.Settings.Default.selectedConnector = connectorViewModel.Name.ToLower();
            Properties.Settings.Default.Save();
        }

        public ICommand SelectConnector { get; private set; }

        private void ExecuteConfigureConnector(string connectorName)
        {
            connectorName = connectorName.ToLower();
            switch (connectorName)
            {
                case "tfs":
                    var tfsUriAndProjectSelectorUserControl = new TfsUriAndProjectSelector();

                    Window window = new Window
                    {
                        Title = "Set TFS server and Project name",
                        Content = tfsUriAndProjectSelectorUserControl,
                        SizeToContent = SizeToContent.WidthAndHeight,
                        ResizeMode = ResizeMode.NoResize
                    };

                    window.ShowDialog();


                    // Todo: Raise an event!!!

                    //var tfsUriAndProjectSelectorViewModel = (TfsUriAndProjectSelectorViewModel)tfsUriAndProjectSelectorUserControl.DataContext;

                    //// If settings have changed, then reload queries
                    //if (tfsUriAndProjectSelectorViewModel.HaveSettingsChanged)
                    //{
                    //    var tfsService = this.Ioc.Get<ITfsService>();
                    //    tfsService.SetWorkItemStore(tfsUriAndProjectSelectorViewModel.Store);

                    //    var connectorViewModel = this.Ioc.Get<IConnectorViewModel>();
                    //    if (connectorViewModel is TfsConnectorViewModel)
                    //    {
                    //        ((TfsConnectorViewModel)connectorViewModel).ProjectName = Properties.Settings.Default.tfsprojectName;
                    //        ((TfsConnectorViewModel)connectorViewModel).ReloadTFSQueries(null);
                    //    }
                    //}
                    break;
                default:
                    break;
            }
        }

        public ICommand ConfigureConnector { get; private set; }

        public IConnectorViewModel CurrentConnectorViewModel
        {
            get => this.currentConnectorViewModel;
            private set
            {
                if (this.currentConnectorViewModel == value)
                {
                    return;
                }

                if (this.currentConnectorViewModel != null)
                {
                    WeakEventManager<IDependenciesService, EventArgs>.RemoveHandler(this.currentConnectorViewModel.DependenciesService, "DependenciesModelChanged", this.DependenciesModelChangedHandler);
                    WeakEventManager<IDependenciesService, EventArgs>.RemoveHandler(this.currentConnectorViewModel.DependenciesService, "DependenciesModelAboutToChange", this.DependenciesModelAboutToChangeHandler);
                    WeakEventManager<IDependenciesService, EventArgs>.RemoveHandler(this.currentConnectorViewModel.DependenciesService, "DependenciesModelCouldNotBeChanged", this.DependenciesModelCouldNotBeChanged);
                }

                this.currentConnectorViewModel = value;
                this.currentConnectorViewModel.Initialize();

                WeakEventManager<IDependenciesService, EventArgs>.AddHandler(this.currentConnectorViewModel.DependenciesService, "DependenciesModelChanged", this.DependenciesModelChangedHandler);
                WeakEventManager<IDependenciesService, EventArgs>.AddHandler(this.currentConnectorViewModel.DependenciesService, "DependenciesModelAboutToChange", this.DependenciesModelAboutToChangeHandler);
                WeakEventManager<IDependenciesService, EventArgs>.AddHandler(this.currentConnectorViewModel.DependenciesService, "DependenciesModelCouldNotBeChanged", this.DependenciesModelCouldNotBeChanged);

                this.OnPropertyChanged("CurrentConnectorViewModel");
            }
        }

        public Dictionary<int, DependencyItem> DependenciesModel => this.currentConnectorViewModel.DependenciesService.DependenciesModel;

        public BitmapImage DependenciesImage
        {
            get
            {
                if (this.currentConnectorViewModel.DependenciesService != null && this.currentConnectorViewModel.DependenciesService.DependenciesModel != null && this.currentConnectorViewModel.DependenciesService.DependenciesModel.Count > 0)
                {
                    return this.dependenciesImage;
                }

                return null;
            }
        }
        private BitmapImage dependenciesImage;

        private int DependenciesModelCount
        {
            get
            {
                if (this.currentConnectorViewModel.DependenciesService.DependenciesModel != null)
                {
                    return this.currentConnectorViewModel.DependenciesService.DependenciesModel.Count;
                }
                return 0;
            }
        }

        public bool IsRenderable
        {
            get => (this.DependenciesModelCount > 0);
        }

        public bool IsLoading
        {
            get { return this.isLoading; }
            set
            {
                if (value != this.isLoading)
                {
                    this.isLoading = value;
                    this.OnPropertyChanged("IsLoading");
                }
            }
        }
        private bool isLoading;

        private void DependenciesModelChangedHandler(object sender, EventArgs e)
        {
            try
            {
                var graph = GraphVizHelper.CreateDependencyGraph(
                    this.currentConnectorViewModel.DependenciesService.DependenciesModel,
                    Properties.Settings.Default.maxLineLength);

                using (MemoryStream memStream = new MemoryStream())
                {
                    IRenderer renderer = new Renderer(Properties.Settings.Default.graphvizPath);

                    // We wait synchronously for the memStream to be filled up
                    Task.Run(async () => { await renderer.RunAsync(graph, memStream, RendererLayouts.Dot, RendererFormats.Png, CancellationToken.None); }).Wait();

                    this.dependenciesImage = new BitmapImage();
                    this.dependenciesImage.BeginInit();
                    this.dependenciesImage.CacheOption = BitmapCacheOption.OnLoad;
                    this.dependenciesImage.StreamSource = memStream;
                    this.dependenciesImage.EndInit();
                    this.dependenciesImage.Freeze();
                }

                //var encoder = new PngBitmapEncoder();
                //var colorImageWritableBitmap = new WriteableBitmap(10, 10, 75, 75, PixelFormats.Bgr32, null);
                //encoder.Frames.Add(BitmapFrame.Create(colorImageWritableBitmap));
                //using (var stream = new FileStream("MyImage.png", FileMode.Create, FileAccess.Write))
                //{
                //    IRenderer renderer = new Renderer(Properties.Settings.Default.graphvizPath);
                //    Task.Run(async () => { await renderer.RunAsync(graph, stream, RendererLayouts.Dot, RendererFormats.Png, CancellationToken.None); }).Wait();
                //    encoder.Save(stream);
                //}
                //this.dependenciesImage = new BitmapImage(new Uri("MyImage.png", UriKind.Relative));
                //this.dependenciesImage.Freeze();
            }
            catch (Exception ex)
            {
                // this is a bad idea, as we loose the exception (it is not logged)
                this.logger.Error(string.Format(@"Could not render dependencies image: {0}{1}{2}", ex.Message, Environment.NewLine, ex.StackTrace));
                this.dependenciesImage = new BitmapImage(new Uri(@"pack://application:,,,/Resources/RenderError.png", UriKind.Absolute));
                this.dependenciesImage.Freeze();
            }

            this.OnPropertyChanged("DependenciesImage");
            //this.OnPropertyChanged("DependenciesModelCount");
            this.OnPropertyChanged("IsRenderable");
            this.IsLoading = false;
        }

        private void DependenciesModelAboutToChangeHandler(object sender, EventArgs e)
        {
            this.IsLoading = true;
        }

        private void DependenciesModelCouldNotBeChanged(object sender, EventArgs e)
        {
            this.OnPropertyChanged("IsRenderable");
            this.IsLoading = false;
        }

        public ICommand RenderAndDownloadDependenciesAsImage { get; private set; }

    }
}
