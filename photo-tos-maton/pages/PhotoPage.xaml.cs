using CameraControl.Devices.Classes;
using log4net;
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

            // TODO: la gestion des photos et des events doit etre dans la classe camere

            // Camera.Instance.Device.CaptureInSdRam = false; // TODO
            Camera.Instance.Device.PhotoCaptured += Device_PhotoCaptured;
            Camera.Instance.Device.CapturePhotoNoAf();

        }

        private void Device_PhotoCaptured(object sender, CameraControl.Devices.Classes.PhotoCapturedEventArgs eventArgs)
        {
            log.Info(string.Format("PhotoPage::Device_PhotoCaptured: filename = {0}", eventArgs.FileName));

            // to prevent UI freeze start the transfer process in a new thread
            Thread thread = new Thread(PhotoCaptured);
            thread.Start(eventArgs);

            // TODO: beurk
            Camera.Instance.Device.PhotoCaptured -= Device_PhotoCaptured;
        }

        // TODO: should be in camera class
        private void PhotoCaptured(object o)
        {
            PhotoCapturedEventArgs eventArgs = o as PhotoCapturedEventArgs;
            if (eventArgs == null)
                return;
            try
            {
                //GridPhoto.Dispatcher.Invoke(() =>
                //{
                //    VisibilityManagement(3);
                //});

                eventArgs.CameraDevice.IsBusy = true;
                var date = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                var fileName = Path.Combine(ConfigurationManager.AppSettings["CameraRollDirPath"].ToString(), date + ".jpg");

                // check the folder of filename, if not found create it
                var dir = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(dir))
                {
                    log.Info("create camera roll folder = " + dir);
                    Directory.CreateDirectory(dir);
                }

                string tempFile = Path.GetTempFileName();

                // todo
                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                Stopwatch stopWatch = new Stopwatch();

                log.Debug("transfer photo to tempory folder");
                // transfer file from camera  
                // in this way if the session folder is used as hot folder will prevent write errors
                stopWatch.Start();
                eventArgs.CameraDevice.TransferFile(eventArgs.Handle, tempFile); // TODO: gérer erreur ??
                eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);
                eventArgs.CameraDevice.IsBusy = false;
                stopWatch.Stop();

                // TODO: add option to enable/disable fiture
                // TODO: controller avant si la photo a bien été copiée
                if (!eventArgs.CameraDevice.CaptureInSdRam)
                {
                    log.Debug("remove picture from SD card");
                    eventArgs.CameraDevice.DeleteObject(new DeviceObject() { Handle = eventArgs.Handle });
                }

                log.Info(string.Format("move photo to '{0}", fileName));
                File.Copy(tempFile, fileName);

                // TODO: what for ???
                WaitForFile(fileName);

                //_photos.Add(fileName);

                //if (_timerWatchDog != null)
                //{
                //    _timerWatchDog.Dispose();
                //    _timerWatchDog = null;
                //}
                //if (_photos.Count >= _nbPhotos)
                //{
                //    // TODO: MGU: unloaded
                //    //_photos.ForEach(p => _facesCreation.EnqueueFileName(p));
                //}
                //GridPhoto.Dispatcher.Invoke(() => DisplayPrintPhotos());
            }
            catch (Exception ex)
            {
                eventArgs.CameraDevice.IsBusy = false;
                log.Debug("Error download photo from camera :\n" + ex.Message);
                //GridPhoto.Dispatcher.Invoke(() => StopWithErrorMessage());
            }
        }

        // TODO: cracra
        public static void WaitForFile(string file)
        {
            if (!File.Exists(file))
                return;
            int retry = 15;
            while (IsFileLocked(file) && retry > 0)
            {
                Thread.Sleep(100);
                retry--;
            }
        }

        public static bool IsFileLocked(string file)
        {
            FileStream stream = null;
            try
            {
                stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }


    }
}
