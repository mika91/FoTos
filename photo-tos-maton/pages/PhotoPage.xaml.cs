using System;
using System.Collections.Generic;
using System.Linq;
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

namespace photo_tos_maton.pages
{
    /// <summary>
    /// Interaction logic for PhotoPage.xaml
    /// </summary>
    public partial class PhotoPage : UserControl
    {
        public Action GotoBackPageHandler { get; set; }

        public PhotoPage()
        {
            // TODO: logger
            InitializeComponent();
        }

        // TODO: should be done on OnLoad() ???
        public void StartLiveView()
        {
            this.liveViewControl.StartLiveView(Camera.Device);
        }

        public void StopLiveView()
        {
            this.liveViewControl.StopLiveView();
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            GotoBackPageHandler?.Invoke();
        }
    }
}
