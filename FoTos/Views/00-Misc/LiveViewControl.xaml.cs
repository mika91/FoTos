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
        private ICameraMan _cameraRam;
        private LiveView _liveView;

        public LiveViewControl()
        {
            InitializeComponent();
        }


        public void StartLiveView(ICameraMan cameraRam)
        {
            _cameraRam = cameraRam;

            // todo: utiliser un checl
            this.LiveViewImage.Visibility = _cameraRam != null ? Visibility.Visible : Visibility.Collapsed;
            this.cameraOffIcon.Visibility = _cameraRam != null ? Visibility.Collapsed : Visibility.Visible;

            // start liveview
            _liveView = new LiveView(_cameraRam, () => StopLiveView());
            _liveView.Start(LiveViewImage, true);

        }

        public void StopLiveView()
        {
            // todo: à revoir: pas faire ici + pause
            this.LiveViewImage.Visibility = Visibility.Collapsed;
            this.cameraOffIcon.Visibility =  Visibility.Visible;


            if (_liveView != null)
            {
                _liveView.Stop();
                _liveView = null;
            }

                 }

    }
}
