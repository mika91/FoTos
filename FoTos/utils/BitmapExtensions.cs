using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace FoTos.utils
{
    public static class BitmapExtensions
    {

        // Grayscale ColorMatrix
        private static readonly ColorMatrix GrayScaleColorMatrix = new ColorMatrix( new float[][]{
            new float[] {.3f, .3f, .3f, 0, 0},
            new float[] {.59f, .59f, .59f, 0, 0},
            new float[] {.11f, .11f, .11f, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {0, 0, 0, 0, 1}
        });

        // Sepia ColorMatrix
        private static readonly ColorMatrix SepiaColorMatrix = new ColorMatrix( new float[][]{
            new float[]{.393f, .349f, .272f, 0, 0},
            new float[]{.769f, .686f, .534f, 0, 0},
            new float[]{.189f, .168f, .131f, 0, 0},
            new float[]{0, 0, 0, 1, 0},
            new float[]{0, 0, 0, 0, 1}
        });


        public static Bitmap Clone(this Bitmap original, ColorMatrix colorMatrix)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);


            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        public static Bitmap Grayscale(this Bitmap original)
        {
            return original?.Clone(GrayScaleColorMatrix);
        }

        public static Bitmap Sepia(this Bitmap original)
        {
            return original?.Clone(SepiaColorMatrix);
        }



        public static Bitmap crop(this Bitmap b, int factor)
        {
            var cropRect = new Rectangle(
                (int)(b.Width * ((100-factor) / 200.0)), (int)(b.Height * ((100-factor) / 200.0)),
                (int)(b.Width * (factor / 100.0)), (int) (b.Height * (factor / 100.0)));

            return crop(b, cropRect);
        }

        public static Bitmap crop(this Bitmap b, Rectangle r)
        {
            Bitmap nb = new Bitmap(r.Width, r.Height);
            Graphics g = Graphics.FromImage(nb);
            g.DrawImage(b, -r.X, -r.Y);
            return nb;
        }


        #region bitmap <-> bitmapsource

        // -------------------------------------------------------------------------------------------------------
        // https://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
        // -------------------------------------------------------------------------------------------------------

        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            BitmapSource i = Imaging.CreateBitmapSourceFromHBitmap(
                           bitmap.GetHbitmap(),
                           IntPtr.Zero,
                           Int32Rect.Empty,
                           BitmapSizeOptions.FromEmptyOptions());
            return (BitmapImage)i;
        }

        public static Bitmap ToBitmap(this BitmapImage bitmapImage)
        {
            // TODO: benchmark to find faster version
            //return new BitmapImage(bitmapImage.StreamSource));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        #endregion


        #region Save 

        public static async Task SaveAsJpeg(this BitmapSource img, String filePath, int qualityLevel = 80)
        {
            if (img == null)
                return;

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = qualityLevel;
            encoder.Frames.Add(BitmapFrame.Create(img));

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

       

        #endregion

    }

}
