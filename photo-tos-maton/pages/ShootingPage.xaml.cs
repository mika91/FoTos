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
    /// Interaction logic for ShootingPage.xaml
    /// </summary>
    public partial class ShootingPage : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        MainWindow MainWindow { get { return Dispatcher.Invoke(()=> Window.GetWindow(this) as MainWindow); } }


        private ICameraMan _cameraMan;
        public ICameraMan CameraMan {
            get { return _cameraMan; }
            set {
                _cameraMan = value;
                _cameraMan.NewLiveViewImage += _cameraMan_NewLiveViewImage;
                _cameraMan.NewPhoto += cameraMan_NewPhoto;
            } }

 
        private void _cameraMan_NewLiveViewImage(Bitmap bitmap)
        {
            //log.Trace("cameraMan_NewLiveViewImage");
            this.LiveViewImage.Dispatcher.Invoke(() => this.LiveViewImage.Source = BitmapUtils.BitmapToImageSource(bitmap));
        }


        public ShootingPage()
        {
            log.Debug("ShootingPage::Contructor");
            InitializeComponent();
        }

 

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("ShootingPage::ButtonBack_Click");
            MainWindow.GotoHomePage();
        }

        private void ButtonTakePicture_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("ShootingPage::ButtonTakePicture_Click");
            if (_inProgressPhotoShoot)
            {
                log.Warn("photo shoot is already in progess");
                return;
            }

            // start new photo shoot process
            log.Info("start photo shoot countdown");
            Task.Factory.StartNew(PhotoShootTask);
        }

        private void PhotoShootTask()
        {
            _inProgressPhotoShoot = true;
            _photoFilename = null;

            try
            {
                // display countdown
                SetVisibility(EVisibilityMode.Countdown);

                // countdown
                int countdown = 3;
                for (int i = countdown; i > 0; i--)
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


                Thread.Sleep(1000);

                // take picture
                _photoFilename = null;
                _cameraMan?.TakePicture(); // TODO: appel blocant ?

          


                // wait from the photo to be available
                // if timeout is reached, return to liveview
                // timeout task:
                // https://stackoverflow.com/questions/20717414/creating-tasks-with-timeouts
                // https://devblogs.microsoft.com/pfxteam/crafting-a-task-timeoutafter-method/
                var photoWasCaptured = SpinWait.SpinUntil(() => !String.IsNullOrEmpty(_photoFilename), TimeSpan.FromSeconds(5)); // TODO: make configurable
                if (photoWasCaptured)
                {
                    log.Info(string.Format("new photo captured = '{0}'", _photoFilename));
                    MainWindow.GotoPhotoPage(_photoFilename);
                    //this.Dispatcher.Invoke(() =>
                    //{
                    //    //// set photo source
                    //    //this.PhotoResultImage.Source = new BitmapImage(new Uri(_photoFilename, UriKind.Absolute));

                    //    //// set visibility
                    //    //SetVisibility(EVisibilityMode.Photo);

                    //    // TODO: comment on revient à l'écran liveview ?
                    //});
                }
                else
                {
                    log.Warn("photo capture timeout");
                    SetVisibility(EVisibilityMode.LiveView);
                }
            } catch (Exception ex)
            {
                log.Error("error furing photo shoot session", ex);
                SetVisibility(EVisibilityMode.LiveView);
            }
            finally
            {
                // reset var
                _inProgressPhotoShoot = false;
                _photoFilename = null;
            }
        }

        private bool _inProgressPhotoShoot     = false; // TODO: mutex
        private String _photoFilename = null;


        private void cameraMan_NewPhoto(string filename)
        {
            log.Info("cameraMan_NewPhoto");
            if (_inProgressPhotoShoot /* && _photoFilename == null*/)
            {
                _photoFilename = filename;
            }
        }


        private void ShootingPage_Loaded(object sender, RoutedEventArgs e)
        {
            log.Debug("ShootingPage::Loaded");
            StartLiveView();
        }

        private void ShootingPage_Unloaded(object sender, RoutedEventArgs e)
        {
            log.Debug("ShootingPage::Unloaded");
            StopLiveView();
        }

        // TODO: should be done on OnLoad() ???
        private void StartLiveView()
        {
            log.Info("Start Camera LiveView");
            _inProgressPhotoShoot = false;
            _photoFilename = null;
            CameraMan?.StartLiveView();
            SetVisibility(EVisibilityMode.LiveView);
        }

        // TODO: should be done on OnUnloadLoad() ???

        private void StopLiveView()
        {
            log.Info("Stop Camera LiveView");
            CameraMan?.StopLiveView();
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
                        this.PhotoResultGrid.Visibility = Visibility.Collapsed;
                        break;

                    case EVisibilityMode.Countdown:
                        this.LiveViewGrid.Visibility = Visibility.Visible;
                        this.CountdownGrid.Visibility = Visibility.Visible;
                        this.SmileGrid.Visibility = Visibility.Collapsed;
                        this.PhotoResultGrid.Visibility = Visibility.Collapsed;
                        break;

                    case EVisibilityMode.Smile:
                        this.LiveViewGrid.Visibility = Visibility.Collapsed;
                        this.CountdownGrid.Visibility = Visibility.Collapsed;
                        this.SmileGrid.Visibility = Visibility.Visible;
                        this.PhotoResultGrid.Visibility = Visibility.Collapsed;
                        break;

                    case EVisibilityMode.Photo:
                        this.LiveViewGrid.Visibility = Visibility.Collapsed;
                        this.CountdownGrid.Visibility = Visibility.Collapsed;
                        this.SmileGrid.Visibility = Visibility.Collapsed;
                        this.PhotoResultGrid.Visibility = Visibility.Visible;
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
