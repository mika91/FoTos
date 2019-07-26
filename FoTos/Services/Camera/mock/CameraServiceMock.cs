using FoTos.camera;
using FoTos.utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FoTos.Services.Camera.mock
{
    class CameraServiceMock : ICameraService
    {
        public event Action<BitmapSource> NewLiveViewImage;
        public event Action<String> NewPhoto;

        private CancellationTokenSource _liveViewToken;

        String _folder;
        public CameraServiceMock(String outputFolder)
        {
            _folder = outputFolder;
        }

        public void StartLiveView()
        {
            _liveViewToken = new CancellationTokenSource();
            CancellationToken ct = _liveViewToken.Token;
            Task.Factory.StartNew(()=>
            {
                while (true)
                {
                    Thread.Sleep(2000); // 15fps

                    if (ct.IsCancellationRequested)
                    {
                        // another thread decided to cancel
                        Console.WriteLine("mock liveview stopped");
                        break;
                    }

                    if (NewLiveViewImage != null)
                    {
                        var img = GenerateRandomBitmap();
                        NewLiveViewImage.Invoke(img);
                    }
                }
            }, ct);
        }

        public void StopLiveView()
        {
            _liveViewToken?.Cancel();
        }

        public void TakePictureAsync()
        {
            Task.Factory.StartNew(() =>
            {
                if (NewPhoto != null)
                {
                    //var img = GenerateRandomBitmap();

                    //var imgSource = BitmapUtils.BitmapToImageSource(img);
                    //var filename = Path.Combine(_folder, DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".jpg");

                    //var dir = Path.GetDirectoryName(filename);
                    //if (!Directory.Exists(dir))
                    //{
                    //    //log.Info("create camera roll folder = " + dir);
                    //    Directory.CreateDirectory(dir);
                    //}



                    //imgSource.SaveAsJpeg(filename).Wait();

                    //NewPhoto?.Invoke(filename);

                }
            });
           
        }


        private Random rnd = new Random();


        private BitmapSource GenerateRandomBitmap()
        {
            return GenerateBitmap(3000, 2000, new System.Windows.Media.Color() { R = (byte) rnd.Next(256), G = (byte) rnd.Next(256), B = (byte) rnd.Next(256) });
        }

        private BitmapSource GenerateBitmap(int width, int height, System.Windows.Media.Color color)
        {
            //Bitmap bmp = new Bitmap(width, height);
            //Graphics g = Graphics.FromImage(bmp);
            //g.Clear(color);
            //return bmp;

            // Define parameters used to create the BitmapSource.
            PixelFormat pf = PixelFormats.Bgr32;
            int rawStride = (width * pf.BitsPerPixel + 7) / 8;
            byte[] rawImage = new byte[rawStride * height];

            // Initialize the image with data.
            Random value = new Random();
            value.NextBytes(rawImage);

            // Create a BitmapSource.
            BitmapSource bitmap = BitmapSource.Create(width, height,
                96, 96, pf, null,
                rawImage, rawStride);

            // thread compatible
            bitmap.Freeze();
            return bitmap;
        }

        public void Save(Bitmap picture)
        {
            throw new NotImplementedException();
        }
    }
}
