using System;
using System.Windows.Media.Imaging;

namespace photo_tos_maton.camera
{
    public interface ICameraMan
    {

        void StartLiveView();
        void StopLiveView();

        event Action<BitmapSource> NewLiveViewImage;

        void TakePicture();

        event Action<String> NewPhoto;

    }
}
