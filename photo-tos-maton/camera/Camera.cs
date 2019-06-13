using CameraControl.Devices;
using log4net;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace photo_tos_maton
{
    // TODO: static vs singleton + gérer les events + gérer connexion/reconnexion...

        // TODO: maybe Acquire(UserControl) pour bien gérer les changements d'etat de IHM ?
    class Camera
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static CameraDeviceManager _cameraDeviceManager;

        private static CameraDeviceManager Manager
        {
            get
            {
                if (_cameraDeviceManager == null || _cameraDeviceManager.SelectedCameraDevice == null)
                {
                    _cameraDeviceManager = GetCameraManager();
                }
                return _cameraDeviceManager;
            }
        }

        public static ICameraDevice Device
        {
            get
            {
                return Manager?.SelectedCameraDevice;
            }
        }

        private static CameraDeviceManager GetCameraManager()
        {
            try
            {
                log.Info("attempt to get new camera...");
                var cameraDeviceManager = new CameraDeviceManager();
                cameraDeviceManager.CloseAll();
                cameraDeviceManager.DetectWebcams = true;
                cameraDeviceManager.ConnectToCamera();

                // TODO: choose camera, if not in properties
                cameraDeviceManager.SelectedCameraDevice = cameraDeviceManager.ConnectedDevices.FirstOrDefault();
                Log.Info(string.Format("selected camera = {0}", cameraDeviceManager?.SelectedCameraDevice?.DisplayName));
                return cameraDeviceManager;

            }
            catch (Exception ex)
            {
                // TODO
                MessageBox.Show(ex.Message, "Vérifier l'appareil photo");
            }
            return null;
        }

    }
}
