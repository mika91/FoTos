using CameraControl.Devices;
using log4net;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FoTos.Services.Camera
{
    public partial class CameraService : ICameraService
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool HasCamera { get { return _mng.SelectedCameraDevice?.IsConnected == true; } }
        public event Action CameraChanged;

        public String CameraRollFolder { get; private set; }
    
        public String PreferedCamera { get; private set; }

        public CameraService(String cameraRollFolder, String preferedCamera = null)
        {
            CameraRollFolder = cameraRollFolder;
            PreferedCamera = preferedCamera;

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

            // list camera
            var devices = _mng.ConnectedDevices;
            log.Info(String.Format("connected cameras: {0}", String.Join(", ", devices.Select(d => d.DisplayName))));

            // prefered camera
            if (PreferedCamera != null) {
                var prefered = devices.FirstOrDefault(camera => camera.DisplayName.ToUpper().Contains(PreferedCamera.ToUpper()));
                if (prefered != null)
                {
                    log.Info(String.Format("prefered camera '{0}' availabale: use it", prefered.DisplayName));
                    _mng.SelectedCameraDevice = prefered;
                }
            }
            else
            {
                log.Info("search fo a reflex camera");
                var reflexCamera = devices.FirstOrDefault(camera => camera.DisplayName.ToUpper().Contains("CANON") || camera.DisplayName.Contains("NIKON"));
                if (reflexCamera != null)
                {
                    log.Info("user reflex camera = " + reflexCamera.DisplayName);
                    _mng.SelectedCameraDevice = reflexCamera;
                }
                else
                {
                    log.Info("no reflex camera found");
                }
            }

            log.Debug("camera selected = " + _mng?.SelectedCameraDevice?.DisplayName);
            
                
            // apply configuration
            log.Info("apply camera settings");
            _mng.SelectedCameraDevice.CaptureInSdRam = false;
            //_mng.SelectedCameraDevice.FNumber.SetValue(8);
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


        #endregion

    }
}
