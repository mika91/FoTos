using CameraControl.Devices;
using photo_tos_maton.camera;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace photo_tos_maton.user_controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class LiveViewControl : UserControl 
    {
        private ICameraDevice _cameraDevice;
        private LiveView _liveView;

        public LiveViewControl()
        {
            InitializeComponent();
        }


        public void StartLiveView(ICameraDevice cameraDevice)
        {
            _cameraDevice = cameraDevice;
            // todo: à revoir
            this.cameraOffIcon.Visibility = _cameraDevice != null ? Visibility.Visible : Visibility.Hidden;
            this.LiveViewImage.Visibility = _cameraDevice != null ? Visibility.Hidden : Visibility.Visible;

            // start liveview
            _liveView = new LiveView(_cameraDevice, () => StopLiveView());
            _liveView.Start(LiveViewImage, true);

        }

        public void StopLiveView()
        {
            // todo: à revoir: pas faire ici + pause
            this.LiveViewImage.Visibility = Visibility.Hidden;
            this.cameraOffIcon.Visibility =  Visibility.Visible;


            if (_liveView != null)
            {
                _liveView.Stop();
                _liveView = null;
            }

            //_cameraDevice.PhotoCaptured -= DeviceManager_PhotoCaptured;

            //if (_timerBeforeStoppingPhoto != null)
            //{
            //    _timerBeforeStoppingPhoto.Dispose();
            //    _timerBeforeStoppingPhoto = null;
            //}
            //if (_timerWatchDog != null)
            //{
            //    _timerWatchDog.Dispose();
            //    _timerWatchDog = null;
            //}

            //GridPhoto.Dispatcher.Invoke(() => VisibilityManagement(0));

            //_playMain(_error);
            //Hide();
        }

    }
}
