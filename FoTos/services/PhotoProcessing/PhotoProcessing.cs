using FoTos.camera;
using System;
using System.Drawing;
using System.Threading.Tasks;
using FoTos.utils;
using System.IO;
using log4net;
using System.Reflection;

namespace FoTos.Services.PhotoProcessing
{
    public class PhotoProcessing
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly int ThumnailsSize = 1000;   // TODO
        private static readonly int MaxExportSize = 4000;   // TODO -> google as a limit to 16Mpixels for free storage

        public Bitmap OriginalBitmap   { get; private set; }
        public String OriginalFilename { get; private set; }

        public String OutputDir { get; set; } 

        private Bitmap _thumbnail;
        public Bitmap Thumbnail
        {
            get
            {
                // create thumbnail on demand
                if (_thumbnail == null)
                    _thumbnail = BitmapUtils.Scale(OriginalBitmap, ThumnailsSize).Result;
                  
                return _thumbnail;
            }
        }

        public PhotoProcessing(String filename, Bitmap bitmap, String outputDir)
        {
            OriginalFilename = filename;
            OriginalBitmap = bitmap;
            OutputDir = outputDir;

            // check output dir exists, if not found create it
            if (!Directory.Exists(outputDir))
            {
                log.Info("create output dir = " + outputDir);
                Directory.CreateDirectory(outputDir);
            }
        }

        public enum Filter { None, Sepia, Grayscale }

        public async Task<Bitmap> GetThumbnail(Filter filter)
        {
            return await ApplyFilter(Thumbnail, filter);
        }


        //private async Task SavePhoto(BitmapImage image, String filename)
        //{
        //    // check output dir exists, if not found create it
        //    var dir = Path.GetDirectoryName(UploadDir);
        //    if (!System.IO.Directory.Exists(dir))
        //    {
        //        log.Info("create GPhotos upload dir = " + dir);
        //        System.IO.Directory.CreateDirectory(dir);
        //    }

        //    // save image
        //    var outputFile = Path.Combine(dir, filename);
        //    log.Info(String.Format("save jpeg img = '{0}'", outputFile));
        //    await image.SaveAsJpeg(outputFile);

        //}


        public async Task<String> Export(Filter filter)
        {
            // resize image to output resolution
            var output = await Scale(OriginalBitmap); 

            // apply filter
            log.Info("apply filter = " + filter);
            var outputProcessed = await ApplyFilter(output, filter);

            // save image
            var outputFile = Path.Combine(OutputDir, Path.GetFileNameWithoutExtension(OriginalFilename) + ".jpg");
            log.Info(String.Format("save jpeg img = '{0}'", outputFile));
            var bitmapSource = BitmapUtils.BitmapToImageSource(outputProcessed);
            await bitmapSource.SaveAsJpeg(outputFile);

            return outputFile;
        }

        private async Task<Bitmap> Scale(Bitmap img)
        {
            if (OriginalBitmap.Width > MaxExportSize || OriginalBitmap.Height > MaxExportSize)
            {
                log.Info("resize image");
                return await BitmapUtils.Scale(OriginalBitmap, MaxExportSize);
            };

            return (Bitmap) img.Clone();
        }

        private async Task<Bitmap> ApplyFilter(Bitmap bitmap, Filter filter)
        {
            switch (filter)
            {
                case Filter.Sepia: return bitmap.Sepia();
                case Filter.Grayscale: return bitmap.Grayscale();
                default: return bitmap;
            }
        }
    }
}
