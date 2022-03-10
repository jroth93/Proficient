using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Windows.Interop;

namespace Proficient.Utilities
{
    internal class Icons
    {
        /// <summary>
        /// Convert a BitmapImage to Bitmap
        /// </summary>
        static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            Bitmap bmp = new Bitmap(bitmapImage.PixelWidth,bitmapImage.PixelHeight,PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(new Rectangle(System.Drawing.Point.Empty, bmp.Size),ImageLockMode.WriteOnly,PixelFormat.Format32bppPArgb);
            bitmapImage.CopyPixels(Int32Rect.Empty,data.Scan0,data.Height * data.Stride,data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// Convert a Bitmap to a BitmapSource
        /// </summary>
        static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();

            BitmapSource retval;

            try
            {
                retval = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }
            return retval;
        }

        static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);

            var destImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            
            destImage.SetResolution(96,96);

            using (var g = Graphics.FromImage(destImage))
            {
                g.Clear(Color.Green);
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;


                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        /// <summary>
        /// Scale down large icon to desired size for Revit 
        /// ribbon button, e.g., 32 x 32 or 16 x 16
        /// </summary>
        public static BitmapSource ScaledIcon(string imgName, int size)
        {

            Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Proficient.Icons.{imgName}.png");
            BitmapImage icon = new BitmapImage();

            icon.BeginInit();
            icon.StreamSource = s;
            icon.EndInit();

            return BitmapToBitmapSource(ResizeImage(BitmapImageToBitmap(icon), size, size));
        }
    }
}
