using log4net;
using photo_tos_maton.camera;
using photo_tos_maton.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

namespace photo_tos_maton.Views._03_DevelopingView
{
    /// <summary>
    /// Interaction logic for DevelopingView.xaml
    /// </summary>
    public partial class DevelopingView : UserControl
    {
        // TODO: time out on inactivity

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        MainWindow MainWindow { get { return Dispatcher.Invoke(() => Window.GetWindow(this) as MainWindow); } }

        public DevelopingView()
        {
            InitializeComponent();
        }


        private Bitmap _img;
        public Bitmap Image
        {
            get { return _img; }
            set
            {
                _img = value;
                this.Dispatcher.Invoke(() =>
                {
                    var originalBitmap = _img;

                    PhotoImage.Source = BitmapUtils.BitmapToImageSource(originalBitmap);
                    RefreshThumbnails(originalBitmap);
                });
            }


        }



        private async Task RefreshThumbnails(Bitmap img)
        {
            log.Debug("Refreshing filter thumbnails...");
            //await Task.Delay(5000);
            var sw = new Stopwatch();
            sw.Start();
            // TODO: resize before ?

            this.Dispatcher.Invoke(() =>
            {
                var original = BitmapUtils.Scale(img, 1000, 1000, true);

                var grayscale = original.Grayscale();
                var sepia = original.Sepia();

                this.ThumbnailColor.Source = BitmapUtils.BitmapToImageSource(original);
                this.ThumbnailSepia.Source = BitmapUtils.BitmapToImageSource(sepia);
                this.ThumbnailGrayscale.Source = BitmapUtils.BitmapToImageSource(grayscale);
            });

            sw.Stop();
            log.Debug(string.Format("thumbnails refreshed in {0}ms", sw.ElapsedMilliseconds));
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
