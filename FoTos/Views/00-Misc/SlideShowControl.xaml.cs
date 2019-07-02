using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FoTos.Views
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class SlideShowControl : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        // TODO: release time ???
        private Timer _timer = new Timer();
        private string[] _files;

        public SlideShowControl()
        {
            InitializeComponent();

         
            // init timer
            _timer.Interval = 4000; // TODO
            _timer.Enabled = false;
            _timer.Elapsed += (s, e) => NextPhoto();

            // start


        }

        public static ILog Log => log;

        public void Start(String dirPath)
        {
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

        public void Stop()
        {
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
                img.Source = new BitmapImage(new Uri(file, UriKind.Absolute));
                this.transitionBox.Content = img;
            });
        }
    }
}
