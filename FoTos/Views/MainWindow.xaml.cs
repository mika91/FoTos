using log4net;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using NHotkey.Wpf;
using NHotkey;
using System.Drawing;
using FoTos.Services.Camera.mock;
using FoTos.Services.Camera;
using FoTos.Services.PhotoProcessing;
using System.Windows.Media.Imaging;
using FoTos.Utils;
using System.IO;

namespace FoTos.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public App App { get { return ((App)System.Windows.Application.Current); } }

        public MainWindow()
        {
            log.Info("MainWindow InitializeComponent");
            InitializeComponent();

            // register global hotkeys
            HotkeyManager.Current.AddOrReplace("Increment", Key.Enter, ModifierKeys.Alt, OnToggleFullScreen);



            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            generateQRCode();

            GotoHomePage();
        }

        public Bitmap QRCode { get; private set; }

        private void generateQRCode()
        {
            // bar code
            log.Info("genrate album QR code");
            Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
            var img = qrcode.Draw(App.Settings.GoogleAlbumShareUrl, 500);
            QRCode = new Bitmap(img);
        }


        #region pages init


        public void GotoHomePage(WpfPageTransitions.PageTransitionType transitionType = WpfPageTransitions.PageTransitionType.SlideAndFade)
        {
            this.Dispatcher.Invoke(() =>
        {
            log.Debug("Goto Home Page");


            var homeView = new HomeView(App.Services.SlideshowService);
            this.TransitionControl.ShowPage(homeView, transitionType);
            
        });
        }

        public void GotoShootingPage(WpfPageTransitions.PageTransitionType transitionType = WpfPageTransitions.PageTransitionType.SlideAndFade)
        {
            this.Dispatcher.Invoke(() =>
            {
                log.Debug("Goto Shooting Page");

                var shootingView = new ShootingView(App.Services.CameraService);
                this.TransitionControl.ShowPage(shootingView, transitionType);
                
            });
        }



        public void GotoDeveloppingPage(String fileFullName, WpfPageTransitions.PageTransitionType transitionType = WpfPageTransitions.PageTransitionType.SlideAndFade)
        {
            if (!File.Exists(fileFullName))
            {
                log.WarnFormat("file not exists = {0}", fileFullName);
                GotoHomePage(WpfPageTransitions.PageTransitionType.SlideAndFadeLeftRight);
            }
            else
            {
                // instanciate image processor
                var img = new BitmapImage(new Uri(fileFullName, UriKind.Absolute));
                var cropped = img.Crop(App.Settings.CameraCropFactor);
                var imgProcessor = new PhotoProcessing(fileFullName, cropped, App.Services.GPhotosUploader.UploadDirectory);

                // goto developing page
                this.Dispatcher.Invoke(() =>
                {
                    log.Debug("Goto Developing Page");

                    var photoView = new DevelopingView(imgProcessor, App.Services.GPhotosUploader);
                    this.TransitionControl.ShowPage(photoView, transitionType);

                });
            }
        }

        public void GotoThanksPage(WpfPageTransitions.PageTransitionType transitionType = WpfPageTransitions.PageTransitionType.SlideAndFade)
        {
            this.Dispatcher.Invoke(() =>
            {
                log.Debug("Goto Thanks Page");

                var thanksView = new ThanksView();
                this.TransitionControl.ShowPage(thanksView, transitionType);
            });
        }



        #endregion


        #region toggle fullscreen

        private double _winLeft;
        private double _winTop;
        private double _winWidth;
        private double _winHeight;

        public void OnToggleFullScreen(object sender, HotkeyEventArgs e)
        {
            log.Info("toggle full screen");
            if (this.WindowState == WindowState.Maximized)
            {
                this.Visibility = Visibility.Collapsed;

                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.ResizeMode = ResizeMode.CanResize;
                this.WindowState = WindowState.Normal;

                // restore position
                this.Top = Math.Max(_winTop, 100);
                this.Left = Math.Max(_winLeft, 100);
                this.Width = Math.Max(_winWidth, 640);
                this.Height = Math.Max(_winHeight, 480);

                this.Topmost = false;
                this.Visibility = Visibility.Visible;
            }
            else
            {
                // save position
                _winTop = this.Top;
                _winLeft = this.Left;
                _winWidth = this.ActualWidth;
                _winHeight = this.ActualHeight;

                this.Visibility = Visibility.Collapsed;
                this.WindowStyle = WindowStyle.None;
                this.ResizeMode = ResizeMode.NoResize;
                this.WindowState = WindowState.Maximized;
                //this.Topmost = true;
                this.Visibility = Visibility.Visible;
            }
        }

        #endregion
    }
}
