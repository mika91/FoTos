using System;
using System.Drawing;

namespace FoTos.Services.Camera
{
    public interface ICameraService
    {

        void StartLiveView();
        void StopLiveView();

        event Action<Bitmap> NewLiveViewImage;

        void TakePicture();

        event Action<String> NewPhoto;
    }
}
