using FoTos.camera;
using FoTos.utils;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FoTos.Services.Camera.mock
{
    class CameraServiceMock : ICameraService
    {
        public event Action<Bitmap> NewLiveViewImage;
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
                        var img =GenerateRandomBitmap();
                        NewLiveViewImage.Invoke(img);
                    }
                }
            }, ct);
        }

        public void StopLiveView()
        {
            _liveViewToken?.Cancel();
        }

        public void TakePicture()
        {
            if (NewPhoto != null)
            {
                var img = GenerateRandomBitmap();

                var imgSource = BitmapUtils.BitmapToImageSource(img);
                var filename = Path.Combine(_folder, DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".jpg");

                var dir = Path.GetDirectoryName(filename);
                if (!Directory.Exists(dir))
                {
                    //log.Info("create camera roll folder = " + dir);
                    Directory.CreateDirectory(dir);
                }



                imgSource.SaveAsJpeg(filename).Wait();

                NewPhoto?.Invoke(filename);

            }
           
        }


        private Random rnd = new Random();


        private Bitmap GenerateRandomBitmap()
        {
            return GenerateBitmap(3000, 2000, Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256)));
        }

        private Bitmap GenerateBitmap(int width, int height, Color color)
        {
            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(color);
            return bmp;
        }

        public void Save(Bitmap picture)
        {
            throw new NotImplementedException();
        }
    }
}
