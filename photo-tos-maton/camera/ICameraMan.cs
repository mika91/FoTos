using System;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace photo_tos_maton.camera
{
    public interface ICameraMan
    {

        void StartLiveView();
        void StopLiveView();

        event Action<Bitmap> NewLiveViewImage;

        void TakePicture();

        event Action<Bitmap> NewPhoto;

    }
}
