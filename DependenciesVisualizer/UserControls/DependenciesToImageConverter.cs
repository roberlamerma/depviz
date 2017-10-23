using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using DependenciesVisualizer.Helpers;
using DependenciesVisualizer.Model;
using Shields.GraphViz.Components;
using Shields.GraphViz.Models;
using Shields.GraphViz.Services;

namespace DependenciesVisualizer.UserControls
{
    public class DependenciesToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is Dictionary<int, DependencyItem>)
            {
                var graph = this.CreateDependencyGraph((Dictionary<int, DependencyItem>)value);

                using (MemoryStream memStream = new MemoryStream())
                {
                    IRenderer renderer = new Renderer(ConfigurationManager.AppSettings["graphvizPath"]);

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

        private Graph CreateDependencyGraph(Dictionary<int, DependencyItem> model)
        {
            try
            {
                var statements = new List<Statement>();

                GraphVizHelper.AddGeneralStatements(ref statements);

                foreach (KeyValuePair<int, DependencyItem> entry in model)
                {
                    if (entry.Value.Successors.Any())
                    {
                        foreach (var succesor in entry.Value.Successors)
                        {
                            GraphVizHelper.AddEdgeStatement(ref statements, entry.Value.ToString(), model[succesor].ToString());
                        }
                    }

                    if (entry.Value.Tags.Any())
                    {
                        if (entry.Value.Tags.Any(str => str.Contains("External")))
                        {
                            GraphVizHelper.ColorizeNode(ref statements, entry.Value.ToString(), Colors.Green);
                        }
                    }
                }

                return new Graph(GraphKinds.Directed, "Name", statements.ToImmutableList());
            }
            catch (Exception ex)
            {
                // ToDo: Add message with error
                // ToDo: Add Logger!
                throw;
            }
        }

        //private async Task Render(Graph graph, string queryName)
        //{
        //    IRenderer renderer = new Renderer(ConfigurationManager.AppSettings["graphvizPath"]);
        //    using (Stream file = File.Create(string.Format(@"{0}.png", queryName)))
        //    {
        //        await renderer.RunAsync(graph,
        //                                file,
        //                                RendererLayouts.Dot,
        //                                RendererFormats.Png,
        //                                CancellationToken.None);
        //    }
        //}
    }
}
