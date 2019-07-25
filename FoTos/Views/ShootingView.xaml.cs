using log4net;
using FoTos.camera;
using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using FoTos.Services.Camera;
using System.ComponentModel;
using FoTos.utils;

namespace FoTos.Views
{
    /// <summary>
    /// Interaction logic for ShootingView.xaml
    /// </summary>
    public partial class ShootingView : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        MainWindow MainWindow { get { return Dispatcher.Invoke(() => Window.GetWindow(this) as MainWindow); } }

        public ShootingView()
        {
            InitializeComponent();
        }

        public ShootingView(ICameraService cameraService) : this()
        {
            _cameraService = cameraService;
        }

        private ICameraService _cameraService;
        private System.Timers.Timer _idleTimer;

        private void ShootingPage_Loaded(object sender, RoutedEventArgs e)
        {
            log.Debug("ShootingPage:Loaded");

            _inProgressPhotoShoot = false;
            _lastPhoto = null;

            if (_cameraService != null)
            {
                // register camera events camera
                _cameraService.NewLiveViewImage += cameraService_NewLiveViewImage;
                _cameraService.NewPhoto         += cameraService_NewPhoto;

                // start live view
                _cameraService.StartLiveView();
            }


            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            // idle time
            _idleTimer = new System.Timers.Timer();
            _idleTimer.Interval = 1000;
            _idleTimer.Elapsed += idleTimer_Elapsed;
            _idleTimer.Enabled = true;

            // default view
            SetVisibility(EVisibilityMode.LiveView);
        }

        private void ShootingPage_Unloaded(object sender, RoutedEventArgs e)
        {
            log.Debug("ShootingPage::Unloaded");

            // camera
            if (_cameraService != null)
            {
                // stop live view
                _cameraService?.StopLiveView();

                // unregister camera events
                _cameraService.NewLiveViewImage += cameraService_NewLiveViewImage;
                _cameraService.NewPhoto += cameraService_NewPhoto;
            }
            _cameraService = null;

            // idle timer
            if (_idleTimer != null)
            {
                _idleTimer.Stop();
                _idleTimer.Elapsed -= idleTimer_Elapsed;
                _idleTimer = null;
            }
        }


        private void cameraService_NewLiveViewImage(Bitmap bitmap)
        {
            //log.Trace("cameraService_NewLiveViewImage");
            this.LiveViewImage.Dispatcher.Invoke(() => this.LiveViewImage.Source = BitmapUtils.BitmapToImageSource(bitmap));
        }

        private void idleTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var idle = IdleTimeDetector.GetIdleTimeInfo();
            if (idle.IdleTime.Seconds > MainWindow.App.Settings.ShootingViewIdleTimeSeconds)
                MainWindow.GotoHomePage(WpfPageTransitions.PageTransitionType.SlideAndFadeLeftRight);
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GotoHomePage(WpfPageTransitions.PageTransitionType.SlideAndFadeLeftRight);
        }
     
        private void ButtonTakePicture_Click(object sender, RoutedEventArgs e)
        {
            TakePictureAsync();
        }

        private void TakePictureAsync()
        {
            if (_inProgressPhotoShoot)
            {
                log.Warn("photo shooting is already in progess");
                return;
            }

            // start new photo shoot process
            log.Info("start photo shooting countdown");
            Task.Factory.StartNew(PhotoShootTask);
        }
        private void PhotoShootTask()
        {
            _inProgressPhotoShoot = true;
            _lastPhoto = null;

            try
            {
                // display countdown
                SetVisibility(EVisibilityMode.CountdownPre);

                // scale liveview
                Dispatcher.Invoke(() =>
                {
                    var sb1 = this.FindResource("StoryBoardScaleLiveView") as Storyboard;
                    BeginStoryboard(sb1);
                });
                // wait liveview to be scaled
                Thread.Sleep(1000);


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
                        // animation coutndown
                        var sb2 = this.FindResource("StorayBoardCountdown") as Storyboard;
                        BeginStoryboard(sb2);
                    });
                    Thread.Sleep(1000);
                }

                // stop live view
                _cameraService?.StopLiveView();

                // display smile icon
                SetVisibility(EVisibilityMode.Smile);


                Thread.Sleep(2000);

                // take picture
                _lastPhoto = null;
                _cameraService?.TakePicture(); // TODO: appel blocant ?




                // wait from the photo to be available
                // if timeout is reached, return to liveview
                // timeout task:
                // https://stackoverflow.com/questions/20717414/creating-tasks-with-timeouts
                // https://devblogs.microsoft.com/pfxteam/crafting-a-task-timeoutafter-method/
                var photoWasCaptured = SpinWait.SpinUntil(() => _lastPhoto != null, TimeSpan.FromSeconds(5)); // TODO: make configurable
                if (photoWasCaptured)
                {
                    MainWindow.GotoDeveloppingPage(_lastPhoto);
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
                    log.Warn("photo shooting timeout");
                    SetVisibility(EVisibilityMode.LiveView);
                }
            }
            catch (Exception ex)
            {
                log.Error("error during photo shooting session", ex);
                SetVisibility(EVisibilityMode.LiveView);
            }
            finally
            {
                // reset var
                _inProgressPhotoShoot = false;
                _lastPhoto = null;
            }
        }

        private bool _inProgressPhotoShoot = false; // TODO: mutex
        private String _lastPhoto = null;
        private void cameraService_NewPhoto(String fileFullName)
        {
            if (_inProgressPhotoShoot /* && _photoFilename == null*/)
            {
                _lastPhoto = fileFullName;
            }
        }


        #region Visibility Management

        enum EVisibilityMode { LiveView, CountdownPre, Countdown, Smile, Photo }

        private void SetVisibility(EVisibilityMode mode)
        {
            Dispatcher.Invoke(() =>
            {
                switch (mode)
                {
                    case EVisibilityMode.LiveView:
                        this.LiveViewImage.Visibility = Visibility.Visible;
                        this.LiveViewImage.BitmapEffect = null;
                        this.LiveViewImage.Opacity = 1.0;
                        this.CountdownGrid.Visibility = Visibility.Collapsed;
                        this.SmileGrid.Visibility = Visibility.Collapsed;
                        this.panelTakePicture.Visibility = Visibility.Visible;
                        this.ArrowsUpGrid.Visibility = Visibility.Collapsed;
                        this.Title.Visibility = Visibility.Collapsed;
                        break;

                    case EVisibilityMode.CountdownPre:
                        this.LiveViewImage.Visibility = Visibility.Visible;
                        this.LiveViewImage.BitmapEffect = null;
                        this.LiveViewImage.Opacity = 1.0;
                        this.CountdownGrid.Visibility = Visibility.Collapsed;
                        this.SmileGrid.Visibility = Visibility.Collapsed;
                        this.panelTakePicture.Visibility = Visibility.Collapsed;
                        this.ArrowsUpGrid.Visibility = Visibility.Collapsed;
                        this.Title.Visibility = Visibility.Collapsed;
                        break;

                    case EVisibilityMode.Countdown:
                        this.LiveViewImage.Visibility = Visibility.Visible;
                        this.LiveViewImage.BitmapEffect = null;
                        this.LiveViewImage.Opacity = 1.0;

                       
                        this.CountdownGrid.Visibility = Visibility.Visible;
                        this.SmileGrid.Visibility = Visibility.Collapsed;
                        this.panelTakePicture.Visibility = Visibility.Collapsed;
                        this.ArrowsUpGrid.Visibility = Visibility.Collapsed;
                        this.Title.Text = "Prenez la pause...";
                        this.Title.Visibility = Visibility.Visible;
                        break;

                    case EVisibilityMode.Smile:
                        //this.LiveViewImage.BitmapEffect = new BlurBitmapEffect() { Radius = 20, KernelType = KernelType.Gaussian };
                        this.LiveViewImage.Visibility = Visibility.Collapsed;
                        //this.LiveViewImage.Opacity = 0.7;
                        this.CountdownGrid.Visibility = Visibility.Collapsed;
                        this.SmileGrid.Visibility = Visibility.Visible;
                        this.panelTakePicture.Visibility = Visibility.Collapsed;
                        this.ArrowsUpGrid.Visibility = Visibility.Visible;
                        this.Title.Text = "Regardez là-haut!";
                        this.Title.Visibility = Visibility.Visible;
                        break;

                    // TODO: not used for now
                    case EVisibilityMode.Photo:
                        this.LiveViewImage.Visibility = Visibility.Collapsed;
                        this.CountdownGrid.Visibility = Visibility.Collapsed;
                        this.SmileGrid.Visibility = Visibility.Collapsed;

                        this.ArrowsUpGrid.Visibility = Visibility.Collapsed;
                        this.panelTakePicture.Visibility = Visibility.Collapsed;
                        this.Title.Visibility = Visibility.Collapsed;
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
