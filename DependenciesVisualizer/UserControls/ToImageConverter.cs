using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DependenciesVisualizer.UserControls
{
    public class ToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage img = new BitmapImage();
            if (value is byte[])
            {
                img = this.ConvertByteArrayToBitMapImage(value as byte[]);
            }
            return img;

        }

        public BitmapImage ConvertByteArrayToBitMapImage(byte[] imageByteArray)
        {
            
            using (MemoryStream memStream = new MemoryStream(imageByteArray))
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.StreamSource = memStream;
                img.EndInit();
                return img;
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
