using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photo_tos_maton.camera
{
    interface ICameraMan
    {
        bool HasCamera { get; }
        event Action CameraChanged;


        void StartLiveView();
        void StopLiveView();

        event Action NewLiveViewImage;

        void TakePicture();

        event Action NewPhoto;

    }
}
