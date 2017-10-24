using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependenciesVisualizer.Model;
using DependenciesVisualizer.Contracts;
using Ninject;

namespace DependenciesVisualizer.ViewModels
{
    public class DependenciesImageViewModel : ViewModelBase
    {
        //public Dictionary<int, DependencyItem> Model { get; set; }

        //[Inject]
        //public IDependenciesService Importer { private get; set; }

        //public byte[] MyImage
        //{
        //    get
        //    {
        //        Image img = Image.FromFile(@"C:\Work\TFSDependenciesVisualizer\DependenciesVisualizer\Resources\green-plug.png");
        //        return (byte[])(new ImageConverter()).ConvertTo(img, typeof(byte[]));
        //    }
        //}

        private double zoomFactor = 1;
        public double ZoomFactor
        {
            get => this.zoomFactor;
            set
            {
                this.zoomFactor = value;
                this.OnPropertyChanged("ZoomFactor");
            }
        }


    }
}
