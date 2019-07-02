using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace FoTos.camera
{
    class BitmapUtils
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        internal static BitmapSource BitmapToImageSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapSource retval;
            try
            {
                retval = Imaging.CreateBitmapSourceFromHBitmap(
                             hBitmap,
                             IntPtr.Zero,
                             Int32Rect.Empty,
                             BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return retval;
        }

        internal static Bitmap Scale(Bitmap img, float scale)
        {
            return new Bitmap(img, (int)(img.Width * scale), (int)(img.Height * scale));
        }

        internal static Bitmap Scale (Bitmap img, int max)
        {
            return Scale(img, max, max, true);
        }

        // TODO: use better and faster algorithm
        internal static Bitmap Scale(Bitmap img, int width, int height, bool keepAspectRatio = true)
        {
            int rWidth  = width;
            int rHeight = height;

            if (keepAspectRatio)
            {
                float ratio = img.Width / (float)img.Height;
                rWidth = (int)(height * (float)ratio);
                if (rWidth > width)
                {
                    rWidth = width;
                    rHeight = (int)(width / (float)ratio);
                }
                
            }

            log.Debug(string.Format("resized bitmap: width = {0}px, height = {1}px", rWidth, rHeight));
            return new Bitmap(img, rWidth, rHeight);
        }


    }

}
