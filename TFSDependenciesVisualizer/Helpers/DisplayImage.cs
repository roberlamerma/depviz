using System;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Drawing;
using System.Windows;

namespace DependenciesVisualizer.Helpers
{
    public static class DisplayImage
    {
        public static BitmapSource GetImageSource(Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
