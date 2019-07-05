using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Timers;
using log4net;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace FoTos.Services.SlideShowProvider
{
    public  class SlideShowService : ISlideShowService
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public SlideShowService()
        {
            // init timer
            _timer.Interval = 5000;
            _timer.Enabled = false;
            _timer.Elapsed += (s, e) => EmitNewPhotoEvent();

        }


        bool IsRunning { get { return _timer?.Enabled ?? false;  } }

        public void Start(String folder, int interval = 5000)
        {
            if (IsRunning) Stop();

            // parse folder
            if (!Directory.Exists(folder))
            {
                log.Error(String.Format("Abort start: slideshow directory '{0}' doesn't exit: ", folder));
                return;
            }

            log.Info(String.Format("Slideshow directory = '{0}'", folder));
            _files = Directory.GetFiles(folder).ToList();


            // emit first photo
            EmitNewPhotoEvent();

            // then enable refresh timer       
            _timer.Interval = interval;
            _timer.Enabled = true;

        }

        public void Stop()
        {
            _timer.Enabled = false;
            _files = null;
        }


        Timer _timer = new Timer();
        private List<String> _files = null;

      

        public event Action<ImageSource> NewPhoto;

        public void EmitNewPhotoEvent()
        {
            if (NewPhoto != null && NewPhoto.GetInvocationList().Any() && _files != null && _files.Count > 0)
            {
                // load image
                var ind = new Random().Next() % _files.Count;
                var file = _files[ind];
                var img = new BitmapImage(new Uri(file, UriKind.Absolute));

                // fire event
                NewPhoto(img);
            }
        }


    }
}
