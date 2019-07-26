using System;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace FoTos.Services.Camera
{
    public interface ICameraService
    {

        void StartLiveView();
        void StopLiveView();

        event Action<BitmapSource> NewLiveViewImage;

        void TakePictureAsync();

        event Action<String> NewPhoto;
    }
}
