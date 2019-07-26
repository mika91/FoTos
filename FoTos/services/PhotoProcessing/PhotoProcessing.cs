using FoTos.camera;
using System;
using System.Drawing;
using System.Threading.Tasks;
using FoTos.utils;
using System.IO;
using log4net;
using System.Reflection;
using FoTos.Utils;
using System.Windows.Media.Imaging;

namespace FoTos.Services.PhotoProcessing
{
    // TODO: find fast algorithms to BItmapSource (not bitmap)

    // WriteableBitmapEx is not quite fast on 'ForEach' method    https://archive.codeplex.com/?p=writeablebitmapex
    //
    public class PhotoProcessing
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly int ThumnailsSize = 1000;   // TODO
        private static readonly int MaxExportSize = 4000;   // TODO -> google as a limit to 16Mpixels for free storage

        public BitmapSource OriginalBitmap { get; private set; }
        public String OriginalFilename { get; private set; }

        public String OutputDir { get; set; } 
        public int CropFactor { get; set; }

        public BitmapSource ThumbnailColor      { get; private set; }
        public BitmapSource ThumbnailSepia { get; private set; }
        public BitmapSource ThumbnailGray  { get; private set; }

        public PhotoProcessing(String filename, BitmapSource bitmap, String outputDir)
        {
            OriginalFilename = filename;
            OutputDir = outputDir;

            OriginalBitmap = bitmap;

            // create thumbs
            ThumbnailColor = bitmap.Scale(ThumnailsSize).AndFreeze();
            var thumb = ThumbnailColor.ToBitmap();
            ThumbnailGray = ApplyFilter(thumb, Filter.Grayscale).AndFreeze();
            ThumbnailSepia = ApplyFilter(thumb, Filter.Sepia).AndFreeze();


            // check output dir exists, if not found create it
            if (!Directory.Exists(outputDir))
            {
                log.Info("create output dir = " + outputDir);
                Directory.CreateDirectory(outputDir);
            }
        }

        public enum Filter { None, Sepia, Grayscale }

       
        public async Task<String> Export(Filter filter)
        {
            // resize image to output resolution
            var output = ScaleToExportSizeAsync(OriginalBitmap); 

            // apply filter
            log.Info("apply filter = " + filter);
            var outputProcessed = ApplyFilter(output, filter);

            // save image
            var outputFile = Path.Combine(OutputDir, Path.GetFileNameWithoutExtension(OriginalFilename) + ".jpg");
            log.Info(String.Format("save jpeg img = '{0}'", outputFile));
            await outputProcessed.SaveAsJpeg(outputFile);

            return outputFile;
        }

        private Bitmap ScaleToExportSizeAsync(BitmapSource img)
        {
            if (OriginalBitmap.Width > MaxExportSize || OriginalBitmap.Height > MaxExportSize)
            {
                log.Info("resize image");
                return OriginalBitmap.Scale(MaxExportSize).ToBitmap();
            };

            return img.ToBitmap();
        }

        private BitmapSource ApplyFilter(Bitmap bitmap, Filter filter)
        {
            switch (filter)
            {
                case Filter.Sepia: return BitmapUtils.BitmapToImageSource(bitmap.Sepia());
                case Filter.Grayscale: return BitmapUtils.BitmapToImageSource(bitmap.Grayscale());
                default: return BitmapUtils.BitmapToImageSource(bitmap);
            }
        }
    }
}
