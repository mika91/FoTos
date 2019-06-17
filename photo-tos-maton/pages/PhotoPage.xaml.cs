using CameraControl.Devices.Classes;
using log4net;
using photo_tos_maton.camera;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace photo_tos_maton.pages
{
    /// <summary>
    /// Interaction logic for PhotoPage.xaml
    /// </summary>
    public partial class PhotoPage : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ICameraMan CameraMan { get; set }

        public Action GotoBackPageHandler { get; set; }

        public PhotoPage()
        {
            log.Debug("PhotoPage::Contructor");
            // TODO: logger
            InitializeComponent();
        }

        // TODO: should be done on OnLoad() ???
        public void StartLiveView()
        {
            log.Debug("PhotoPage::StartLiveView");
            this.liveViewControl.StartLiveView(Camera.Instance.Device);
        }

        public void StopLiveView()
        {
            log.Debug("PhotoPage::StopLiveView");
            this.liveViewControl.StopLiveView();
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("PhotoPage::ButtonBack_Click");

            GotoBackPageHandler?.Invoke();
        }

        private void ButtonTakePicture_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("PhotoPage::ButtonTakePicture_Click");
            
    
            if (Camera.Instance.Device == null)
            {
                log.Warn("impossible to take picture: no camera device");
                return;
            }

            

        }

    }
}
