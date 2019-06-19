using log4net;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Drawing;
using photo_tos_maton.utils;
using photo_tos_maton.camera;

namespace photo_tos_maton.pages
{
    /// <summary>
    /// Interaction logic for PhotoPage.xaml
    /// </summary>
    public partial class PhotoPage : UserControl
    {
        // TODO: time out on inactivity

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        MainWindow MainWindow { get { return Dispatcher.Invoke(() => Window.GetWindow(this) as MainWindow); } }

        public PhotoPage()
        {
            InitializeComponent();
        }


        public void SetImage(String filename)
        {
            log.Info(string.Format("set photo image = '{0}'", filename));



            this.Dispatcher.Invoke(() =>
            {
                // TODO: isn't possible to get the bitmap instead of loading it from filesystem ?
                //var originalBitmap = new BitmapImage(new Uri(filename, UriKind.Absolute));
                var originalBitmap = new Bitmap(filename);

                PhotoImage.Source = BitmapUtils.BitmapToImageSource(originalBitmap);
                Task.Factory.StartNew(() => RefreshThumbnails(originalBitmap));
            });

          
        }


     
        private void RefreshThumbnails(Bitmap original)
        {

            // TODO: resize before ?

         

            this.Dispatcher.Invoke(() =>
            {
                var grayscale = original.Grayscale();
                var sepia = original.Sepia();

                this.ThumbnailColor.Source      = BitmapUtils.BitmapToImageSource(original);
                this.ThumbnailSepia.Source      = BitmapUtils.BitmapToImageSource(sepia);
                this.ThumbnailGrayscale.Source  = BitmapUtils.BitmapToImageSource(grayscale);
            });
        }

        private void PhotoPage_Loaded(object sender, RoutedEventArgs e)
        {
            log.Debug("PhotoPage::Loaded");

        }

        private void PhotoPage_Unloaded(object sender, RoutedEventArgs e)
        {
            log.Debug("PhotoPage::Unloaded");
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
    }
}
