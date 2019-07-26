using log4net;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using WpfPageTransitions;
using FoTos.Services.SlideShowProvider;
using System.Windows.Media;
using FoTos.camera;

namespace FoTos.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        MainWindow MainWindow { get { return Dispatcher.Invoke(() => Window.GetWindow(this) as MainWindow); } }

        private ISlideShowService _slideShowService;

        public HomeView()
        {
            InitializeComponent();
        }

        public HomeView(ISlideShowService slideShowService) : this()
        {

            // register slide show
            _slideShowService = slideShowService;


            if (DesignerProperties.GetIsInDesignMode(this))
                return;


            // transition
            this.transitionBox.TransitionType = PageTransitionType.Fade;
            
        }


        private void HomeView_Unloaded(object sender, RoutedEventArgs e)
        {
            log.Debug("HomeView::Unloaded");

            // unregister slideshow
            if (_slideShowService != null)
            {
                _slideShowService.Stop();
                _slideShowService.NewPhoto -= NextPhoto;
            }
        }

        private void HomeView_Loaded(object sender, RoutedEventArgs e)
        {
            log.Debug("HomeView::Loaded");

            // register slideshow
            if (_slideShowService != null)
            {
                _slideShowService.NewPhoto += NextPhoto;
                _slideShowService.Start(((App)Application.Current).Settings.SlideShowFolder); // TODO
            }

            // QR code
            if (MainWindow?.QRCode != null)
            {
                this.imageLeftQR.Source = BitmapUtils.BitmapToImageSource(MainWindow.QRCode);
                this.imageRightQR.Source = BitmapUtils.BitmapToImageSource(MainWindow.QRCode);
            }
        }

        // prevent doucle-click => start/stop/start liveview bug
        // cracra, à revoir
        private bool exiting = false;

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (exiting) return;

            exiting = true;
            MainWindow.GotoShootingPage();
        }


        private void NextPhoto(ImageSource imgSource)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                var img = new Image();
                img.Source = imgSource;
                //this.transitionBox.Content = img;
                var uc = new UserControl();
                uc.Content = img;
                this.transitionBox.ShowPage(uc);
               });
        }

       
    }
}
