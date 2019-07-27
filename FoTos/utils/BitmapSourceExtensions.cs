using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoTos.Utils
{
    public static class BitmapSourceExtensions
    {

        //private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public static Bitmap ToBitmap(this BitmapSource bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        #region crop

        public static BitmapSource Crop(this BitmapSource b, int cropFactor)
        {
            if (cropFactor <= 0 || cropFactor >= 100)
                return b;

            var cropRect = new Int32Rect(
                (int)(b.PixelWidth * ((100 - cropFactor) / 200.0)), (int)(b.PixelHeight * ((100 - cropFactor) / 200.0)),
                (int)(b.PixelWidth * (cropFactor / 100.0)), (int)(b.PixelHeight * (cropFactor / 100.0)));

            return b.Crop(cropRect);
        }

        // https://docs.microsoft.com/fr-fr/dotnet/framework/wpf/controls/how-to-crop-an-image
        public static BitmapSource Crop(this BitmapSource bitmapSource, Int32Rect rec)
        {

            if (bitmapSource == null)
                return null;

            // Create a CroppedBitmap based off of a xaml defined resource.
            CroppedBitmap cb = new CroppedBitmap(
                bitmapSource,
                rec);       //select region rect


            return cb;
        }

        #endregion

        #region Transform

        public static BitmapSource Scale(this BitmapSource bitmapSource, int max, bool keepAspectRatio = true)
        {
            return bitmapSource.Scale(max, max, keepAspectRatio);
        }

        // https://docs.microsoft.com/fr-fr/dotnet/framework/wpf/graphics-multimedia/how-to-apply-a-transform-to-a-bitmapimage
        internal static BitmapSource Scale(this BitmapSource img, int width, int height, bool keepAspectRatio = true)
        {
            if (img == null)
                return null;

       
            double scaleX = (double) width / img.PixelWidth;
            double scaleY = (double) height / img.PixelHeight;

            if (keepAspectRatio)
            {
                scaleX = Math.Min(scaleX, scaleY);
                scaleY = Math.Min(scaleX, scaleY);
            }

            return img.Scale(scaleX, scaleY, keepAspectRatio);
        }

        // https://docs.microsoft.com/fr-fr/dotnet/framework/wpf/graphics-multimedia/how-to-apply-a-transform-to-a-bitmapimage
        internal static BitmapSource Scale(this BitmapSource img, double scaleX, double scaleY, bool keepAspectRatio = true)
        {
   
            // Create the new BitmapSource that will be used to scale the size of the source.
            TransformedBitmap scaleBitmap = new TransformedBitmap();
            scaleBitmap.BeginInit();
            scaleBitmap.Source = img;
            scaleBitmap.Transform = new ScaleTransform(scaleX, scaleY, 0.5, 0.5);
            scaleBitmap.EndInit();

            return scaleBitmap;
        }

            #endregion

            #region Save 

            public static async Task SaveAsJpeg(this BitmapSource img, String filePath, int qualityLevel = 90)
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


        public static BitmapSource AndFreeze(this BitmapSource bitmapSource)
        {
            if (bitmapSource?.IsFrozen != true)
                bitmapSource?.Freeze();

            return bitmapSource;
        }

        //// Grayscale ColorMatrix
        //private static readonly ColorMatrix GrayScaleColorMatrix = new ColorMatrix(new float[][]{
        //    new float[] {.3f, .3f, .3f, 0, 0},
        //    new float[] {.59f, .59f, .59f, 0, 0},
        //    new float[] {.11f, .11f, .11f, 0, 0},
        //    new float[] {0, 0, 0, 1, 0},
        //    new float[] {0, 0, 0, 0, 1}
        //});

        //// Sepia ColorMatrix
        //private static readonly ColorMatrix SepiaColorMatrix = new ColorMatrix(new float[][]{
        //    new float[]{.393f, .349f, .272f, 0, 0},
        //    new float[]{.769f, .686f, .534f, 0, 0},
        //    new float[]{.189f, .168f, .131f, 0, 0},
        //    new float[]{0, 0, 0, 1, 0},
        //    new float[]{0, 0, 0, 0, 1}
        //});


        //public static BitmapSource Sepia(this BitmapSource bitmapSource)
        //{
        //    //WriteableBitmap wb = new WriteableBitmap(bitmapSource);
        //    //uint[] PixelData = new uint[wb.PixelWidth * wb.PixelHeight];
        //    //wb.CopyPixels(PixelData, 4 * wb.PixelWidth, 0);

        //    //int brightness = 10;

        //    //for (uint y = 0; y < wb.PixelHeight; y++)
        //    //{
        //    //    for (uint x = 0; x < wb.PixelWidth; x++)
        //    //    {
        //    //        uint pixel = PixelData[y * wb.PixelWidth + x];
        //    //        byte[] dd = BitConverter.GetBytes(pixel);

        //    //        int Bi = (int)dd[0];
        //    //        int Gi = (int)dd[1];
        //    //        int Ri = (int)dd[2];

        //    //        int R = (int)(.393f * Ri + .769f * Gi + .189f * Bi);
        //    //        int G = (int)(.349f * Ri + .686f * Gi + .168f * Bi);
        //    //        int B = (int)(.272 * Ri + .534 * Gi + .131f * Bi);


        //    //        if (B < 0) B = 0;
        //    //        if (B > 255) B = 255;
        //    //        if (G < 0) G = 0;
        //    //        if (G > 255) G = 255;
        //    //        if (R < 0) R = 0;
        //    //        if (R > 255) R = 255;


        //    //        dd[0] = (byte)B;
        //    //        dd[1] = (byte)G;
        //    //        dd[2] = (byte)R;
        //    //        PixelData[y * wb.PixelWidth + x] = BitConverter.ToUInt32(dd, 0);
        //    //    }
        //    //}
        //    //wb.WritePixels(new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight), PixelData, 4 * wb.PixelWidth, 0);

        //    //return wb;

        //    var start = DateTime.Now;

        //    WriteableBitmap wb = new WriteableBitmap(bitmapSource);               // create the WritableBitmap using the source


        //    wb.ForEach((x, y, color) => System.Windows.Media.Color.FromRgb(
        //       (byte)(.393f * color.R + .769f * color.G + .189f * color.B),
        //       (byte)(.349f * color.R + .686f * color.G + .168f * color.B),
        //       (byte)(.272f * color.R + .534f * color.G + .131f * color.B)));

        //    var stop = DateTime.Now;

        //    log.Info("sepia time = " + (stop - start).Milliseconds);



        //    // TEST


        //    start = DateTime.Now;
        //    var b = GetBitmap(bitmapSource);
        //    var tmp = b.Sepia();


        //    var res =  BitmapUtils.BitmapToImageSource(b);

        //    stop = DateTime.Now;

        //    log.Info("sepia time bitmap = " + (stop - start).Milliseconds);
        //    return res;

        //    return wb;
        //}

        //public static Bitmap GetBitmap(BitmapSource source)
        //{
        //    Bitmap bmp = new Bitmap(
        //      source.PixelWidth,
        //      source.PixelHeight,
        //      System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        //    BitmapData data = bmp.LockBits(
        //      new Rectangle(System.Drawing.Point.Empty, bmp.Size),
        //      ImageLockMode.WriteOnly,
        //      System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        //    source.CopyPixels(
        //      Int32Rect.Empty,
        //      data.Scan0,
        //      data.Height * data.Stride,
        //      data.Stride);
        //    bmp.UnlockBits(data);
        //    return bmp;
        //}


    }
}
