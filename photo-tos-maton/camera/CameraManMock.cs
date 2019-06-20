using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace photo_tos_maton.camera
{
    class CameraManMock : ICameraMan
    {
        public event Action<Bitmap> NewLiveViewImage;
        public event Action<Bitmap> NewPhoto;

        private CancellationTokenSource _liveViewToken;

        public void StartLiveView()
        {
            _liveViewToken = new CancellationTokenSource();
            CancellationToken ct = _liveViewToken.Token;
            Task.Factory.StartNew(()=>
            {
                while (true)
                {
                    Thread.Sleep(66); // 15fps

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
                NewPhoto?.Invoke(img);

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
    }
}
