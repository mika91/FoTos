using log4net;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Timers;
using WpfPageTransitions;

namespace FoTos.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        MainWindow MainWindow { get { return Dispatcher.Invoke(() => Window.GetWindow(this) as MainWindow); } }

        // TODO: release time ???
        private Timer _timer = new Timer();
        private string[] _files;

        public HomeView()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            // init timer
            _timer.Interval = 4000; // TODO
            _timer.Enabled = false;
            _timer.Elapsed += (s, e) => NextPhoto();

            // transition
            this.transitionBox.TransitionType = PageTransitionType.Fade;
        }


        private void HomeView_Unloaded(object sender, RoutedEventArgs e)
        {
            log.Debug("HomeView::Unloaded");
            // ensure slideshow is disable
            if (_timer?.Enabled == true)
                Stop();
        }

        private void HomeView_Loaded(object sender, RoutedEventArgs e)
        {
            log.Debug("HomeView::Loaded");
        }

        #region Dependency Injection

        // dependency injection
        public void Init(String slideShowFolder)
        {
            // run slideshow
            Start(slideShowFolder);
        }

        // clean dependencies (should be called form Unloaded event)
        public void Release()
        {
            Stop();
        }

        #endregion

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow.GotoShootingPage();
        }

        // TODO: move in service
        #region Slideshow logic

        private void Start(String dirPath)
        {
            log.Info("starting slideshow");

            if (!Directory.Exists(dirPath))
            {
                log.Error(String.Format("Slideshow directory '{0}' doesn't exit: ", dirPath));
                return;
            }

            log.Info(String.Format("Slideshow directory = '{0}'", dirPath));
            _files = Directory.GetFiles(dirPath);


            NextPhoto();
            _timer.Enabled = true;
        }

        private void Stop()
        {
            log.Info("stopping slideshow");

            if (_timer != null)
            {
                _timer.Enabled = false;
                // TODO: clear slideshow image ?
                // TODO: dispose += events
            }

            _files = null;
        }

        private void NextPhoto()
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                var ind = new Random().Next() % _files.Length;
                var file = _files[ind];

                if (!File.Exists(file))
                {
                    // TODO: reload dir ???
                    log.Error(string.Format("file '{0}' doesn't exist anymore", file));
                    return;
                }

                // TODO: gérer erreurs
                var img = new Image();
                log.Info(String.Format("display new slideshow picture='{0}'", file));
                img.Source = new BitmapImage(new Uri(file, UriKind.Absolute));
                //this.transitionBox.Content = img;
                var uc = new UserControl();
                uc.Content = img;
                this.transitionBox.ShowPage(uc);
                //this.transitionBox.Source = new BitmapImage(new Uri(file, UriKind.Absolute));
            });
        }

        #endregion

       
    }
}
