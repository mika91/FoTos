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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

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



         
            Task.Factory.StartNew(TakePictureTask);
        }

        private void TakePictureTask()
        {
            // display countdown
            SetVisibility(EVisibilityMode.Countdown);

            // countdown
            int countdown = 3;
            for (int i=countdown; i>0; i--)
            {
                Dispatcher.Invoke(() =>
                {
                    // decrement countdown
                    this.TextCountdown.Text = i.ToString();
                    // animation
                    var sb = this.FindResource("StorayBoardCountdown") as Storyboard;
                    BeginStoryboard(sb);
                });
                Thread.Sleep(1000);
            }
        
            // stop live view
            _cameraMan?.StopLiveView();

            // display smile icon
            SetVisibility(EVisibilityMode.Smile);

            // take picture
            _cameraMan?.TakePicture();

            // TODO: when phot captured (or timeout), return to LiveView 
        }

        private void StartCountdownAnimation()
        {
          
        }


        private void PhotoPage_Loaded(object sender, RoutedEventArgs e)
        {
            log.Info("PhotoPage::Loaded");
            StartLiveView();
            SetVisibility(EVisibilityMode.LiveView);
        }

        private void PhotoPage_Unloaded(object sender, RoutedEventArgs e)
        {
            log.Info("PhotoPage::Unloaded");
            StopLiveView();
        }

        #region Visibility Management

        enum EVisibilityMode { LiveView, Countdown, Smile, Photo }

        private void SetVisibility(EVisibilityMode mode)
        {
            Dispatcher.Invoke(() =>
            {
                switch (mode)
                {
                    case EVisibilityMode.LiveView:
                        this.LiveViewGrid.Visibility = Visibility.Visible;
                        this.CountdownGrid.Visibility = Visibility.Collapsed;
                        this.SmileGrid.Visibility = Visibility.Collapsed;
                        break;

                    case EVisibilityMode.Countdown:
                        this.LiveViewGrid.Visibility = Visibility.Visible;
                        this.CountdownGrid.Visibility = Visibility.Visible;
                        this.SmileGrid.Visibility = Visibility.Collapsed;
                        break;

                    case EVisibilityMode.Smile:
                        this.LiveViewGrid.Visibility = Visibility.Collapsed;
                        this.CountdownGrid.Visibility = Visibility.Collapsed;
                        this.SmileGrid.Visibility = Visibility.Visible;
                        break;

                    case EVisibilityMode.Photo:
                        this.LiveViewGrid.Visibility = Visibility.Collapsed;
                        this.CountdownGrid.Visibility = Visibility.Collapsed;
                        this.SmileGrid.Visibility = Visibility.Collapsed;
                        break;
                }
            }, DispatcherPriority.Background);
        }
  
        public void WpfInvoke(Action handler)
        {
            Dispatcher.Invoke(handler, DispatcherPriority.Background);
        }

        #endregion
    }
}
