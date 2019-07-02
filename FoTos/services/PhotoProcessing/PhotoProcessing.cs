using photo_tos_maton.camera;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using photo_tos_maton.utils;
using System.IO;
using log4net;
using System.Reflection;

namespace photo_tos_maton.services.PhotoProcessing
{
    public class PhotoProcessing
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly int ThumnailsSize = 1000;   // TODO
        private static readonly int MaxExportSize = 4000;   // TODO -> google as a limit to 16Mpixels for free storage

        public Bitmap OriginalBitmap   { get; private set; }
        public String OriginalFilename { get; private set; }

        private Bitmap _thumbnail;
        public Bitmap Thumbnail
        {
            get
            {
                // create thumbnail on demand
                if (_thumbnail == null)
                    _thumbnail = BitmapUtils.Scale(OriginalBitmap, ThumnailsSize);
                  
                return _thumbnail;
            }
        }

        public PhotoProcessing(String filename, Bitmap bitmap)
        {
            OriginalFilename = filename;
            OriginalBitmap = bitmap;
        }

        public enum Filter { None, Sepia, Greyscale }

        public async Task<Bitmap> GetThumbnail(Filter filter)
        {
            return ApplyFilter(Thumbnail, filter);
        }

        public async Task Export(Filter filter, String outpuDir)
        {
            // resize image to output resolution
            var output = (OriginalBitmap.Width > MaxExportSize || OriginalBitmap.Height > MaxExportSize)
                ? BitmapUtils.Scale(OriginalBitmap, MaxExportSize)
                : OriginalBitmap;

            // apply filter
            var outputProcessed = ApplyFilter(output, filter);

            outputProcessed.Save(Path.Combine(outpuDir, OriginalFilename));
        }

        private Bitmap ApplyFilter(Bitmap bitmap, Filter filter)
        {
            switch (filter)
            {
                case Filter.Sepia: return bitmap.Sepia();
                case Filter.Greyscale: return bitmap.Grayscale();
                default: return bitmap;
            }
        }
    }
}
