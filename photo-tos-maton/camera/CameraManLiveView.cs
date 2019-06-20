using CameraControl.Devices;
using CameraControl.Devices.Classes;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace photo_tos_maton.camera
{
    public partial class CameraMan
    {

        // TODO: watchdog pour éteindre le timer du liveview si jamais on a perdu la caméra ?
        // d'un autre côté, 15 appels par seconde, ça vaut peut-etre pas les coup de se faire chier et crée d'éventuels effets de bord...

        public event Action<Bitmap> NewLiveViewImage;

        private System.Timers.Timer _timerLiveView;
 

        public void StartLiveView()
        {
            log.Info("CameraMan::StartLiveView");

            try
            {
                // get and check camera device
                var device = CheckDevice();
                if (device == null)
                    return;


                // start live views timer (get liveview image every XX seconds)

                _timerLiveView = new System.Timers.Timer(1000 / 25); // Display 15 images per seconds

                string resp = device?.GetProhibitionCondition(OperationEnum.LiveView);
                if (string.IsNullOrEmpty(resp))
                {
                    Thread thread = new Thread(StartLiveViewThread);
                    thread.Start();
                    thread.Join();
                }
                else
                {
                    Log.Error("Error starting live view " + resp);
                    _timerLiveView.Stop();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error starting live view " + ex.ToString());
                _timerLiveView?.Stop();
            }
        }

        public void StopLiveView()
        {
            log.Info("CameraMan::StopLiveView");

            try
            {
                // stop liveview timer
                if (_timerLiveView != null)
                {
                    _timerLiveView.Dispose();
                    _timerLiveView = null;
                }

                // stop camera device liveview mode
                var device = _mng.SelectedCameraDevice;
                if (device?.GetLiveViewImage()?.IsLiveViewRunning ?? false)
                {
                    log.Info("call device liveview stop");
                    device?.StopLiveView();
                }
               
            }
            catch
            {
                // Do nothing
            }
        }




        private void StartLiveViewThread()
        {
            try
            {
                bool retry = false;
                int retryNum = 0;
                do
                {
                    try
                    {
                        log.Info("call device liveview start");
                        _mng.SelectedCameraDevice?.StartLiveView();
                    }
                    catch (DeviceException deviceException)
                    {
                        if (deviceException.ErrorCode == ErrorCodes.ERROR_BUSY ||
                            deviceException.ErrorCode == ErrorCodes.MTP_Device_Busy)
                        {
                            Thread.Sleep(100);
                            Log.Debug("Retry live view :" + deviceException.ErrorCode.ToString("X"));
                            retry = true;
                            retryNum++;
                        }
                    }
                } while (retry && retryNum < 35);


                // notify new liveview image available on timer tick
                _timerLiveView.Elapsed += (sender, eventArgs) => Task.Factory.StartNew(NotifyNewLiveViewImage);
                _timerLiveView.Start();

            }
            catch (Exception exception)
            {
                Log.Debug("Unable to start liveview ! " + exception.ToString());
            }
        }


        private void NotifyNewLiveViewImage()
        {
            // nobody to notify
            if (NewLiveViewImage == null)
                return;

            var liveViewData = _mng.SelectedCameraDevice?.GetLiveViewImage();
            if (liveViewData != null && liveViewData.ImageData != null)
            {
                var bitmap = new Bitmap(new MemoryStream(liveViewData.ImageData,
                     liveViewData.ImageDataPosition,
                     liveViewData.ImageData.Length - liveViewData.ImageDataPosition));

                NewLiveViewImage?.Invoke(bitmap);
            }
        }

  
    }
}
