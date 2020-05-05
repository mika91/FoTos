using log4net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using FoTos.Services.GoogleUploader;
using FoTos.Services.PhotoProcessing;
using Image = System.Windows.Controls.Image;
using System.IO;
using System.Threading.Tasks;
using FoTos.Services.Printer;

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

 
        private IGPhotosUploader _uploader;
        private PhotoProcessing _processor;
        private IPrinterService _printerService;


        public DevelopingView()
        {
            InitializeComponent();
        }

        public DevelopingView(PhotoProcessing processor, IGPhotosUploader uploader, IPrinterService printerService) : this()
        {
            _processor = processor;
            _uploader = uploader;
            _printerService = printerService;
        }

        private void PhotoPage_Loaded(object sender, RoutedEventArgs e)
        {
            log.Debug("DevelopingView::Loaded");

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

          

            // set image and refresh thumbnails
            this.Dispatcher.Invoke(() =>
            {
                // preview
                PhotoImage.Source = _processor.ThumbnailColor;
                // thumbnails
                this.ThumbnailColor.Source = _processor.ThumbnailColor;
                this.ThumbnailSepia.Source = _processor.ThumbnailSepia;
                this.ThumbnailGrayscale.Source = _processor.ThumbnailGray;
            });
        }

        private void PhotoPage_Unloaded(object sender, RoutedEventArgs e)
        {
            log.Debug("DevelopingView::Unloaded");
        }


        private PhotoProcessing.Filter _currentFilter = PhotoProcessing.Filter.None;

        private void ThumbOrig_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //log.Debug("no filter");
            _currentFilter = PhotoProcessing.Filter.None;
            this.PhotoImage.Source = (sender as Image)?.Source;
        }

        private void ThumbGray_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //log.Debug("grayscale filter");
            _currentFilter = PhotoProcessing.Filter.Grayscale;
            this.PhotoImage.Source = (sender as Image)?.Source;
        }

        private void ThumbSepia_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //log.Debug("sepia filter");
            _currentFilter = PhotoProcessing.Filter.Sepia;
            this.PhotoImage.Source = (sender as Image)?.Source;
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GotoShootingPage(WpfPageTransitions.PageTransitionType.SlideAndFadeLeftRight);
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() => ExportAndUpload());
            MainWindow.GotoThanksPage();
        }

        private async void ExportAndUpload()
        {

            // TODO: rework !!!


            // upload to Gphotos
            // TODO: why filter is not applied ???
            _uploader?.Upload(Path.GetFileName(_processor.OriginalFilename));

            // export to disk
            var exportedFileFullName = await _processor.Export(_currentFilter);
            // print
            _printerService.Print(exportedFileFullName);
        }
      

    }
}
