using log4net;
using FoTos.camera;
using FoTos.utils;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using FoTos.Services.GoogleUploader;
using System;
using FoTos.Services.PhotoProcessing;
using Image = System.Windows.Controls.Image;
using System.IO;

namespace FoTos.Views
{
    /// <summary>
    /// Interaction logic for DevelopingView.xaml
    /// </summary>
    public partial class DevelopingView : UserControl
    {
        // TODO: time out on inactivity

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        MainWindow MainWindow { get { return Dispatcher.Invoke(() => Window.GetWindow(this) as MainWindow); } }



        private Bitmap _originalImage;
        private IGPhotosUploader _uploader;
        private string _outputDir;
        private string _fileFullName;

        private PhotoProcessing _processor;


        public DevelopingView()
        {
            InitializeComponent();
        }

        public DevelopingView(String fileFullName, IGPhotosUploader uploader) : this()
        {

           
            _uploader = uploader;
            _fileFullName = fileFullName;

                  }

        private void PhotoPage_Loaded(object sender, RoutedEventArgs e)
        {
            log.Debug("DevelopingView::Loaded");

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            // set image and refresh thumbnails
            this.Dispatcher.Invoke(() =>
            {
                var cropFactor = MainWindow.App.Settings.CameraCropFactor;


                _originalImage = new Bitmap(_fileFullName);
                if (cropFactor > 0 && cropFactor < 100)
                    _originalImage = _originalImage.crop(cropFactor);

                _processor = new PhotoProcessing(_fileFullName, _originalImage, _uploader.UploadDirectory);

     
                PhotoImage.Source = BitmapUtils.BitmapToImageSource(_originalImage);
                RefreshThumbnails();
            });
        }

        private void PhotoPage_Unloaded(object sender, RoutedEventArgs e)
        {
            log.Debug("DevelopingView::Unloaded");
        }


        private async Task RefreshThumbnails()
        {
            log.Debug("Refreshing filter thumbnails...");
            //await Task.Delay(5000);
            var sw = new Stopwatch();
            sw.Start();
            // TODO: resize before ?

            this.Dispatcher.Invoke(() =>
            {

                var thumb          = _processor.GetThumbnail(PhotoProcessing.Filter.None).Result;
                var thumbSepia     = _processor.GetThumbnail(PhotoProcessing.Filter.Sepia).Result;
                var thumbGrayscale = _processor.GetThumbnail(PhotoProcessing.Filter.Grayscale).Result;

                this.ThumbnailColor.Source = BitmapUtils.BitmapToImageSource(thumb);
                this.ThumbnailSepia.Source = BitmapUtils.BitmapToImageSource(thumbSepia);
                this.ThumbnailGrayscale.Source = BitmapUtils.BitmapToImageSource(thumbGrayscale);
            });

            sw.Stop();
            log.Debug(string.Format("thumbnails refreshed in {0}ms", sw.ElapsedMilliseconds));
        }

        private PhotoProcessing.Filter _currentFilter = PhotoProcessing.Filter.None;

        private void ThumbOrig_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            log.Info("no filter");
            _currentFilter = PhotoProcessing.Filter.None;
            this.PhotoImage.Source = (sender as Image)?.Source;
        }

        private void ThumbGray_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            log.Info("grayscale filter");
            _currentFilter = PhotoProcessing.Filter.Grayscale;
            this.PhotoImage.Source = (sender as Image)?.Source;
        }

        private void ThumbSepia_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            log.Info("sepia filter");
            _currentFilter = PhotoProcessing.Filter.Sepia;
            this.PhotoImage.Source = (sender as Image)?.Source;
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GotoShootingPage(WpfPageTransitions.PageTransitionType.SlideAndFadeLeftRight);
        }

        private async void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            ExportAndUpload();

            MainWindow.GotoThanksPage();
        }

        async Task ExportAndUpload()
        {
            var exportedFileFullName = await _processor.Export(_currentFilter);
            await _uploader?.Upload(Path.GetFileName(_fileFullName));
        }
      

    }
}
