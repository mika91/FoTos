using CameraControl.Devices.Classes;
using log4net;
using photo_tos_maton.camera;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace photo_tos_maton.pages
{
    /// <summary>
    /// Interaction logic for PhotoPage.xaml
    /// </summary>
    public partial class PhotoPage : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ICameraMan _cameraMan;
        public ICameraMan CameraMan {
            get { return _cameraMan; }
            set {
                _cameraMan = value;
                _cameraMan.NewLiveViewImage += _cameraMan_NewLiveViewImage;
                _cameraMan.NewPhoto += cameraMan_NewPhoto;
            } }

        private void cameraMan_NewPhoto(string filename)
        {
            log.Info("cameraMan_NewPhoto");

        }


        private void _cameraMan_NewLiveViewImage(Bitmap bitmap)
        {
            //log.Trace("cameraMan_NewLiveViewImage");
            this.LiveViewImage.Dispatcher.Invoke(() => this.LiveViewImage.Source = BitmapUtils.BitmapToImageSource(bitmap));
        }

        public Action GotoBackPageHandler { get; set; }

        public PhotoPage()
        {
            log.Debug("PhotoPage::Contructor");
            InitializeComponent();
        }

        // TODO: should be done on OnLoad() ???
        private void StartLiveView()
        {
            log.Debug("PhotoPage::StartLiveView");
            CameraMan?.StartLiveView();
        }

        private void StopLiveView()
        {
            log.Debug("PhotoPage::StopLiveView");
            CameraMan?.StopLiveView();
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("PhotoPage::ButtonBack_Click");
            GotoBackPageHandler?.Invoke();
        }

        private void ButtonTakePicture_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("PhotoPage::ButtonTakePicture_Click");
            CameraMan?.TakePicture();
        }


        private void PhotoPage_Loaded(object sender, RoutedEventArgs e)
        {
            log.Info("PhotoPage::Loaded");
            _cameraMan?.StartLiveView();
        }

        private void PhotoPage_Unloaded(object sender, RoutedEventArgs e)
        {
            log.Info("PhotoPage::Unloaded");
            _cameraMan?.StopLiveView();
        }
    }
}
