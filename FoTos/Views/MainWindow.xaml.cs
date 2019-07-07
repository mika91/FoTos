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

            GotoHomePage();
        }


        #region pages init


        public void GotoHomePage()
        {
            this.Dispatcher.Invoke(() =>
        {
            log.Debug("Goto Home Page");


            var homeView = new HomeView(App.Services.SlideshowService);
            this.TransitionControl.ShowPage(homeView);
            
        });
        }

        public void GotoShootingPage()
        {
            this.Dispatcher.Invoke(() =>
            {
                log.Debug("Goto Shooting Page");

                var shootingView = new ShootingView(App.Services.CameraService);
                this.TransitionControl.ShowPage(shootingView);
                
            });
        }



        public void GotoPhotoPage(Bitmap img)
        {
            this.Dispatcher.Invoke(() =>
            {
                log.Debug("Goto Photo Page");

                var photoView = new DevelopingView(img);
                this.TransitionControl.ShowPage(photoView);
                
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
                this.Topmost = true;
                this.Visibility = Visibility.Visible;
            }
        }

        #endregion
    }
}
