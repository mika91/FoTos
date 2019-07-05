using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;


namespace FoTos.Services.SlideShowProvider
{
    public interface  ISlideShowService
    {

        event Action<ImageSource> NewPhoto;

        void Start(String folder, int interval = 5000);

        void Stop();

    }
}
