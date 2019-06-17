using CameraControl.Devices;
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

namespace photo_tos_maton.camera
{
    public class CameraMan : ICameraMan
    {
        // TODO: avant chaque appel, si pas de camera détecté, tenter de faire un TryConnect ???

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool HasCamera { get { return _mng.SelectedCameraDevice != null; } }
        public event Action CameraChanged;

        public CameraMan()
        {
            log.Info("CameraMan::Constructor");
        }

        #region camera device manager

        private CameraDeviceManager _mng;

        private void InitManager()
        {
            log.Info("init camera manager");
            _mng = new CameraDeviceManager();
            _mng.CameraConnected    += Manager_CameraConnected;
            _mng.CameraDisconnected += Manager_CameraDisconnected;
            _mng.CameraSelected     += Manager_CameraSelected;
        }

        private void CloseAllAndConnect()
        {
            log.Info("attempt to connect a camera...");
            _mng.CloseAll();
            //_mng.DetectWebcams = true;
            _mng.ConnectToCamera();

            _mng.SelectedCameraDevice = _mng.ConnectedDevices.FirstOrDefault();
            if (_mng.SelectedCameraDevice == null)
                log.Error("failed to find any camera");

            _mng.SelectedCameraDevice.PhotoCaptured += SelectedCameraDevice_PhotoCaptured;
        }

        private void Manager_CameraConnected(ICameraDevice cameraDevice)
        {
            log.Info("camera connected = " + cameraDevice?.DisplayName);

            // in case no camera selected, try to connect to the new connected one
            if (_mng.SelectedCameraDevice == null)
                CloseAllAndConnect();
            
        }

        private void Manager_CameraDisconnected(ICameraDevice cameraDevice)
        {
            log.Info("camera disonnected = " + cameraDevice?.DisplayName);

            // TODO: vérifier si le fait de déconnecter une caméra met SelectedCameraDevice à null
            // fire camera change event
            if (CameraChanged != null && _mng.SelectedCameraDevice != null)
                CameraChanged();

        }

        private void Manager_CameraSelected(ICameraDevice oldcameraDevice, ICameraDevice newcameraDevice)
        {
            log.Info("camera selected = " + newcameraDevice?.DisplayName);

            // fire camera change event
            if (CameraChanged != null && newcameraDevice != null)
                CameraChanged();
        }

        #endregion

        #region LiveView

        public event Action NewLiveViewImage;
        public event Action CameraOn;
        public event Action CameraOff;
        public event Action NewPhoto;

        public void StartLiveView()
        {

        }

        public void StopLiveView()
        {

        }

        #endregion

        #region Take Picture

        public void TakePicture()
        {
            if (_mng.SelectedCameraDevice == null)
                log.Error("no camera selected");

            _mng.SelectedCameraDevice?.CapturePhotoNoAf();
        }
       
        private void SelectedCameraDevice_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
        {
            log.Info(string.Format("PhotoPage::Device_PhotoCaptured: filename = {0}", eventArgs.FileName));

            // to prevent UI freeze start the transfer process in a new thread
            Thread thread = new Thread(OnPhotoCapturedThread);
            thread.Start(eventArgs);
           
        }
        private void OnPhotoCapturedThread(object o)
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

        #endregion



    }
}
