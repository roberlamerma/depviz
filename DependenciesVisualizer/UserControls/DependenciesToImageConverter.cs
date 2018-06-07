using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using DependenciesVisualizer.Helpers;
using DependenciesVisualizer.Model;
using Shields.GraphViz.Components;
using Shields.GraphViz.Services;

namespace DependenciesVisualizer.UserControls
{
    public class DependenciesToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is Dictionary<int, DependencyItem>)
            {
                var graph = GraphVizHelper.CreateDependencyGraph((Dictionary<int, DependencyItem>)value);

                using (MemoryStream memStream = new MemoryStream())
                {
                    IRenderer renderer = new Renderer(Properties.Settings.Default.graphvizPath);

                    // We wait synchronously for the memStream to be filled up
                    Task.Run(async () => { await renderer.RunAsync(graph, memStream, RendererLayouts.Dot, RendererFormats.Png, CancellationToken.None); }).Wait();

                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.StreamSource = memStream;
                    img.EndInit();
                    return img;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
