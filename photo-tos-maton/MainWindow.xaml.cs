using CameraControl.Devices;
using log4net;
using photo_tos_maton.pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
using NHotkey.Wpf;
using NHotkey;
using photo_tos_maton.camera;
using System.Configuration;
using System.Drawing;

namespace photo_tos_maton
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ICameraMan _cameraMan;
        private ShootingPage _shootingPage;
        private HomePage _homePage;
        private PhotoPage _photoPage;

        public MainWindow()
        {
            log.Info("MainWindow InitializeComponent");
            InitializeComponent();

            // register global hotkeys
            HotkeyManager.Current.AddOrReplace("Increment", Key.Enter, ModifierKeys.Alt, OnToggleFullScreen);


            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            // cameraman
            var useCameraMock = Boolean.Parse(ConfigurationManager.AppSettings["UseCameraMock"] ?? "false");
            if (useCameraMock)
            {
                log.Warn("using cameraman mock");
                _cameraMan = new CameraManMock();
            } else
            {
                _cameraMan = new CameraMan();
            }
        

            // init pages
            initPages();
            GotoHomePage();
        }


        #region pages init

        private void initPages()
        {
            // TODO: ajouter un 'OnUnload' sur toutes les pages pour centraliser ici toute la gestion des etas IHM / Camera

            // home page
            _homePage = new HomePage();
           
            // shooting page
            _shootingPage = new ShootingPage();
            _shootingPage.CameraMan = _cameraMan;

            // photo page
            _photoPage = new PhotoPage();


            GotoHomePage();
        }



        public void GotoHomePage()
        {
            this.Dispatcher.Invoke(() =>
        {
            log.Debug("Goto Home Page");
            transitionBox.Content = _homePage;
        });
         }

    public void GotoShootingPage()
        {
        this.Dispatcher.Invoke(() =>
        {
            log.Debug("Goto Shooting Page");
            transitionBox.Content = _shootingPage;
        });
             }



    public void GotoPhotoPage(Bitmap img)
        {
        this.Dispatcher.Invoke(() =>
        {
            log.Debug("Goto Photo Page");
            _photoPage.SetImage(img);
            transitionBox.Content = _photoPage;
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
                this.Left = Math.Max(_winLeft,100);
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
