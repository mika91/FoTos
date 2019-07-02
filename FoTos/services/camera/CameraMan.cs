using CameraControl.Devices;
using CameraControl.Devices.Classes;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace photo_tos_maton.camera
{
    public partial class CameraMan : ICameraMan
    {
        // TODO: avant chaque appel, si pas de camera détecté, tenter de faire un TryConnect ???

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool HasCamera { get { return _mng.SelectedCameraDevice?.IsConnected == true; } }
        public event Action CameraChanged;

        public CameraMan()
        {
            InitManager();
        }

        #region camera device manager

        private CameraDeviceManager _mng;

        private void InitManager()
        {
            log.Info("init camera manager");
            _mng = new CameraDeviceManager();
            _mng.CameraConnected += Manager_CameraConnected;
            _mng.CameraDisconnected += Manager_CameraDisconnected;
            _mng.CameraSelected += Manager_CameraSelected;
            _mng.PhotoCaptured += Manager_PhotoCaptured;
            _mng.PropertyChanged += Manager_PropertyChanged;

            // For experimental Canon driver support- to use canon driver the canon sdk files should be copied in application folder
            _mng.UseExperimentalDrivers = true;
            _mng.DisableNativeDrivers = false;

            //CloseAllAndConnect();
            // see https://github.com/dukus/digiCamControl/issues/305
            _mng.ConnectToCamera();
            Thread.Sleep(500);
            // TODO: timeout
            while (_mng.SelectedCameraDevice?.IsBusy ?? true)
            {
                log.Warn("camera is busy, please wait...");
                Thread.Sleep(100);
            }

            InitLiveView();
        }

        private ICameraDevice Camera { get { return (_mng.SelectedCameraDevice?.IsConnected == true) ? _mng.SelectedCameraDevice : null; } }

        //private void CloseAllAndConnect()
        //{
        //    log.Info("attempt to connect a camera...");
        //    _mng.CloseAll();
        //    //_mng.DetectWebcams = true;
        //    _mng.ConnectToCamera();

        //    _mng.SelectedCameraDevice = _mng.ConnectedDevices.FirstOrDefault();
        //    if (_mng.SelectedCameraDevice == null)
        //        log.Error("failed to find any camera");
            
        //}

        private void Manager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            log.Info(string.Format("property '{0}'changed", e.PropertyName));
        }

        private void Manager_CameraConnected(ICameraDevice cameraDevice)
        {
            log.Info("camera connected = " + cameraDevice?.DisplayName);

            //// in case no camera selected, try to connect to the new connected one
            //if (_mng.SelectedCameraDevice == null)
            //    CloseAllAndConnect();
            
        }

        private void Manager_CameraDisconnected(ICameraDevice cameraDevice)
        {
            log.Info("camera disonnected = " + cameraDevice?.DisplayName);

            StopLiveView();

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

        public void Save(Bitmap picture)
        {
            var dir = ((App)System.Windows.Application.Current).FinalPhotosFolder;

            if (!Directory.Exists(dir))
            {
                log.Info("create final photos folder = " + dir);
                Directory.CreateDirectory(dir);
            }


        }


        #endregion




        public event Action CameraOn;
        public event Action CameraOff;



   


    }
}
