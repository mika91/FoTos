using CameraControl.Devices;
using log4net;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace photo_tos_maton
{
    // TODO: gérer la connexion/deconexion d'une camera (doit etre transparent pour la UI)
    // TODO: gérer la copy des photos
    // TODO: gérer les réglages

    // TODO: static vs singleton + gérer les events + gérer connexion/reconnexion...

        // TODO: maybe Acquire(UserControl) pour bien gérer les changements d'etat de IHM ?
    class Camera
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // TODO: not singleton, use some sort DI or application context instead
        private static Lazy<Camera> _instance = new Lazy<Camera>(() => new Camera());
        public static Camera Instance
        {
            get { return _instance.Value; }
        }

        private CameraDeviceManager _mng;

        // TODO: maybe remove, all logic in this class ?
        public ICameraDevice Device { get
            {
                if (null == _mng.SelectedCameraDevice)
                    TryConnect();
                return _mng.SelectedCameraDevice;
            } }

        private Camera()
        {
            log.Info("instantiate new camera wrapper");

            //TODO: camera connect/disconnect seems buggy with Canon (see docs)
            // maybe recreate CameraDeviceManager ?

            // camera device manager
            _mng = new CameraDeviceManager();
            _mng.CameraConnected    += _mng_CameraConnected;
            _mng.CameraDisconnected += _mng_CameraDisconnected;
            _mng.CameraSelected += _mng_CameraSelected;

            // connect camera
            TryConnect();
        }

        private void TryConnect()
        {
            log.Info("attempt to connect a camera...");
            _mng.CloseAll();
            _mng.DetectWebcams = true;
            _mng.ConnectToCamera();

            _mng.SelectedCameraDevice = _mng.ConnectedDevices.FirstOrDefault();
            if (_mng.SelectedCameraDevice == null)
                log.Error("failed to find any camera");
        }

        private void _mng_CameraSelected(ICameraDevice oldcameraDevice, ICameraDevice newcameraDevice)
        {
            log.Info("camera selected = " + newcameraDevice?.DisplayName);
        }

        private void _mng_CameraDisconnected(ICameraDevice cameraDevice)
        {
            log.Info("camera disonnected = " + cameraDevice?.DisplayName);
        }

        private void _mng_CameraConnected(ICameraDevice cameraDevice)
        {
            log.Info("camera connected = " + cameraDevice?.DisplayName);
        }

        
       

    }
}
