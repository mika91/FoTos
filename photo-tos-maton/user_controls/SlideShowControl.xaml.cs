using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace photo_tos_maton.user_controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        // TODO
        private Timer _timer = new Timer();

        private string[] _files;
        private int pos = 0;

        public UserControl1()
        {
            InitializeComponent();



            // init timer
            _timer.Interval = 4000; // TODO
            _timer.Enabled = false;
            _timer.Elapsed += (s, e) => NextPhoto();

            // start

        }



        public void Start(String dirPath)
        {
            _files = Directory.GetFiles(dirPath);
            pos = 0;

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
            pos = 0;
        }

        private void NextPhoto()
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                // TODO: gérer erreurs
                var img = new Image();
                var file = _files[pos];
                if (!File.Exists(file))
                {
                    // TODO
                }
                img.Source = new BitmapImage(new Uri(file, UriKind.Relative));
                pos = (pos + 1) % _files.Length;

                this.transitionBox.Content = img;
            });
        }
    }
}
