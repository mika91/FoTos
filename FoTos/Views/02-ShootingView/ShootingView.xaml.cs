using log4net;
using photo_tos_maton.camera;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace photo_tos_maton.Views._02_ShootingView
{
    /// <summary>
    /// Interaction logic for ShootingView.xaml
    /// </summary>
    public partial class ShootingView : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        MainWindow MainWindow { get { return Dispatcher.Invoke(() => Window.GetWindow(this) as MainWindow); } }


        private ICameraMan _cameraMan;
        public ICameraMan CameraMan
        {
            get { return _cameraMan; }
            set
            {
                _cameraMan = value;
                _cameraMan.NewLiveViewImage += _cameraMan_NewLiveViewImage;
                _cameraMan.NewPhoto += cameraMan_NewPhoto;
            }
        }


        private void _cameraMan_NewLiveViewImage(Bitmap bitmap)
        {
            //log.Trace("cameraMan_NewLiveViewImage");
            this.LiveViewImage.Dispatcher.Invoke(() => this.LiveViewImage.Source = BitmapUtils.BitmapToImageSource(bitmap));
        }


        public ShootingView()
        {
            InitializeComponent();
        }



        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GotoHomePage();
        }

        private void ButtonTakePicture_Click(object sender, RoutedEventArgs e)
        {
            TakePicture();
        }

        private void TakePicture()
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
                _lastPhoto = null;
                _cameraMan?.TakePicture(); // TODO: appel blocant ?




                // wait from the photo to be available
                // if timeout is reached, return to liveview
                // timeout task:
                // https://stackoverflow.com/questions/20717414/creating-tasks-with-timeouts
                // https://devblogs.microsoft.com/pfxteam/crafting-a-task-timeoutafter-method/
                var photoWasCaptured = SpinWait.SpinUntil(() => _lastPhoto != null, TimeSpan.FromSeconds(5)); // TODO: make configurable
                if (photoWasCaptured)
                {
                    MainWindow.GotoPhotoPage(_lastPhoto);
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
        private Bitmap _lastPhoto = null;


        private void cameraMan_NewPhoto(Bitmap img)
        {
            if (_inProgressPhotoShoot /* && _photoFilename == null*/)
            {
                _lastPhoto = img;
            }
        }


        private void ShootingPage_Loaded(object sender, RoutedEventArgs e)
        {
            log.Debug("ShootingPage:Loaded");
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
            _inProgressPhotoShoot = false;
            _lastPhoto = null;
            CameraMan?.StartLiveView();
            SetVisibility(EVisibilityMode.LiveView);
        }

        // TODO: should be done on OnUnloadLoad() ???

        private void StopLiveView()
        {
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
                        this.LiveViewImage.BitmapEffect = null;
                        this.LiveViewGrid.Opacity = 1.0;
                        this.CountdownGrid.Visibility = Visibility.Collapsed;
                        this.SmileGrid.Visibility = Visibility.Collapsed;
                        this.panelTakePicture.Visibility = Visibility.Visible;
                        this.ArrowsUpGrid.Visibility = Visibility.Collapsed;
                        break;

                    case EVisibilityMode.Countdown:
                        this.LiveViewGrid.Visibility = Visibility.Visible;
                        this.LiveViewImage.BitmapEffect = null;
                        this.LiveViewGrid.Opacity = 1.0;
                        this.CountdownGrid.Visibility = Visibility.Visible;
                        this.SmileGrid.Visibility = Visibility.Collapsed;
                        this.panelTakePicture.Visibility = Visibility.Collapsed;
                        this.ArrowsUpGrid.Visibility = Visibility.Visible;
                        break;

                    case EVisibilityMode.Smile:
                        this.LiveViewImage.BitmapEffect = new BlurBitmapEffect() { Radius = 20, KernelType = KernelType.Gaussian };
                        this.LiveViewGrid.Visibility = Visibility.Visible;
                        this.LiveViewGrid.Opacity = 0.7;
                        this.CountdownGrid.Visibility = Visibility.Collapsed;
                        this.SmileGrid.Visibility = Visibility.Visible;
                        this.panelTakePicture.Visibility = Visibility.Collapsed;
                        this.ArrowsUpGrid.Visibility = Visibility.Visible;
                        break;

                    // TODO: not used for now
                    case EVisibilityMode.Photo:
                        this.LiveViewGrid.Visibility = Visibility.Collapsed;
                        this.CountdownGrid.Visibility = Visibility.Collapsed;
                        this.SmileGrid.Visibility = Visibility.Collapsed;

                        this.ArrowsUpGrid.Visibility = Visibility.Collapsed;
                        this.panelTakePicture.Visibility = Visibility.Collapsed;
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
