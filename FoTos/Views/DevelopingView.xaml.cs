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
        private string _filename;

        private PhotoProcessing _processor;


        public DevelopingView()
        {
            InitializeComponent();
        }

        public DevelopingView(String filename, IGPhotosUploader uploader) : this()
        {

           
            _uploader = uploader;
            _filename = filename;

            _originalImage = new Bitmap(filename);

            _processor = new PhotoProcessing(filename, _originalImage, uploader.UploadDirectory);
        }

        private void PhotoPage_Loaded(object sender, RoutedEventArgs e)
        {
            log.Debug("DevelopingView::Loaded");

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            // set image and refresh thumbnails
            this.Dispatcher.Invoke(() =>
            {
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
                var thumbGrayscale = _processor.GetThumbnail(PhotoProcessing.Filter.Greyscale).Result;

                this.ThumbnailColor.Source = BitmapUtils.BitmapToImageSource(thumb);
                this.ThumbnailSepia.Source = BitmapUtils.BitmapToImageSource(thumbSepia);
                this.ThumbnailGrayscale.Source = BitmapUtils.BitmapToImageSource(thumbGrayscale);
            });

            sw.Stop();
            log.Debug(string.Format("thumbnails refreshed in {0}ms", sw.ElapsedMilliseconds));
        }

      

        private void Thumbnail_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.Image img)
            {
                log.Info(string.Format("{0}: apply photo filter", img.Name));
                this.PhotoImage.Source = img.Source;
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GotoShootingPage();
        }

        private async void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            ExportAndUpload();

            MainWindow.GotoThanksPage();
        }

        async Task ExportAndUpload()
        {
            await _processor.Export(PhotoProcessing.Filter.None); // TODO
            await _uploader?.Upload(_filename);
        }
      

    }
}
