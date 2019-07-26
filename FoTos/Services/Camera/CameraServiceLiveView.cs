
//using CameraControl.Devices.Classes;
//using FoTos.utils;
//using System;
//using System.Drawing;
//using System.IO;
//using System.Threading;
//using System.Timers;
//using System.Windows;
//using System.Windows.Media.Imaging;
//using Timer = System.Timers.Timer;

//namespace FoTos.Services.Camera
//{
//    /// <summary>
//    /// code based on DigiCamContol samples
//    /// </summary>
//    public partial class CameraService
//    {
//        public event Action<BitmapSource> NewLiveViewImage;


//        private System.Timers.Timer _liveViewTimer = new Timer();

//        private void InitLiveView()
//        {
//            log.Info("init liveview");

//            //set live view default frame rate to 15
//            _liveViewTimer.Interval = 1000 / 15;
//            _liveViewTimer.Elapsed += _liveViewTimer_Tick;
//        }

//        private bool _lockLiveview = false;

//        void _liveViewTimer_Tick(object sender, ElapsedEventArgs e)
//        {
//            if (_lockLiveview)
//            {
//                log.Warn("liveview locked: skip image acquisition");
//                return;
//            }

//            _lockLiveview = true;
//            LiveViewData liveViewData = null;
//            try
//            {
//                liveViewData = Camera?.GetLiveViewImage();
//            }
//            catch (Exception ex)
//            {
//                log.Error("an exception occured while getting liveview image", ex);
//                return;
//            }
//            finally
//            {
//                _lockLiveview = false;
//            }

//            if (liveViewData == null || liveViewData.ImageData == null)
//            {
//                log.Error("no live view image");
//                _lockLiveview = false;
//                return;
//            }
//            try
//            {
//                NotifyNewLiveViewImage(liveViewData);
//            }
//            catch (Exception ex)
//            {
//                log.Error("an exception occured while notifying liveview image", ex);
//            }
//            finally
//            {
//                _lockLiveview = false;
//            }
//        }

//        private void StartLiveViewThread()
//        {
//            bool retry;
//            int retryNum = 0;
//            int retryMax = 10;
//            do
//            {
//                retry = false;
//                try
//                {
//                    log.Info("call device liveview start");
//                    Camera?.StartLiveView();
//                }
//                catch (DeviceException exception)
//                {
//                    if (exception.ErrorCode == ErrorCodes.MTP_Device_Busy || exception.ErrorCode == ErrorCodes.ERROR_BUSY)
//                    {
//                        Thread.Sleep(100);
//                        retryNum++;
//                        retry = true;
//                    }
//                    else
//                    {
//                        log.Error("unexpected error while starting liveview", exception);
//                    }
//                }
//                catch (Exception exception)
//                {
//                    log.Error("unexpected error while starting liveview", exception);
//                }

//            } while (retry && retryNum < retryMax) ;

//            if (retryNum >= retryMax)
//            {
//                log.Warn("failed to stop liveview before max retry count");
//            }

//            // start liveview timer
//            log.Debug("start live view timer");
//            _liveViewTimer.Start();
//        }



//        public void StartLiveView()
//        {
//            log.Info("CameraMan::StartLiveView");
//            new Thread(StartLiveViewThread).Start();

//            //try
//            //{
//            //    // get and check camera device
//            //    var device = CheckDevice();
//            //    if (device == null)
//            //        return;


//            //    // start live views timer (get liveview image every XX seconds)

//            //    _timerLiveView = new System.Timers.Timer(1000 / 25); // Display 15 images per seconds

//            //    string resp = device?.GetProhibitionCondition(OperationEnum.LiveView);
//            //    if (string.IsNullOrEmpty(resp))
//            //    {
//            //        Thread thread = new Thread(StartLiveViewThread);
//            //        thread.Start();
//            //        thread.Join();
//            //    }
//            //    else
//            //    {
//            //        Log.Error("Error starting live view " + resp);
//            //        _timerLiveView.Stop();
//            //    }
//            //}
//            //catch (Exception ex)
//            //{
//            //    Log.Error("Error starting live view " + ex.ToString());
//            //    _timerLiveView?.Stop();
//            //}
//        }

//        public void StopLiveView()
//        {
//            log.Info("CameraMan::StopLiveView");
//            new Thread(StopLiveViewThread).Start();

//            //try
//            //{
//            //    // stop liveview timer
//            //    if (_timerLiveView != null)
//            //    {
//            //        _timerLiveView.Dispose();
//            //        _timerLiveView = null;
//            //    }

//            //    // stop camera device liveview mode
//            //    if (Camera?.GetLiveViewImage()?.IsLiveViewRunning ?? false)
//            //    {
//            //        log.Info("call device liveview stop");
//            //        device?.StopLiveView();
//            //    }

//            //}
//            //catch
//            //{
//            //    // Do nothing
//            //}
//        }

//        private void StopLiveViewThread()
//        {
//            bool retry;
//            int retryNum = 0;
//            int retryMax = 10;
//            do
//            {
//                retry = false;
//                try
//                {
//                    log.Debug("stop live view timer");
//                    _liveViewTimer?.Stop();
//                    // wait for last get live view image
//                    Thread.Sleep(500);
//                    log.Info("call device liveview stop");
//                    Camera?.StopLiveView();
//                }
//                catch (DeviceException exception)
//                {
//                    if (exception.ErrorCode == ErrorCodes.MTP_Device_Busy || exception.ErrorCode == ErrorCodes.ERROR_BUSY)
//                    {
//                        Thread.Sleep(100);
//                        retryNum++;
//                        retry = true;
//                    }
//                    else
//                    {
//                        log.Error("unexpected error while stopping liveview", exception);
//                    }
//                }
//                catch (Exception exception)
//                {
//                    log.Error("unexpected error while stopping liveview", exception);
//                }

//            } while (retry && retryNum < retryMax);

//            if (retryNum >= retryMax)
//            {
//                log.Warn("failed to stop liveview before max retry count");
//            }
//        }



//        //private void StartLiveViewThread()
//        //{
//        //    try
//        //    {
//        //        bool retry = false;
//        //        int retryNum = 0;
//        //        do
//        //        {
//        //            try
//        //            {
//        //                log.Info("call device liveview start");
//        //                Camera?.StartLiveView();
//        //            }
//        //            catch (DeviceException deviceException)
//        //            {
//        //                if (deviceException.ErrorCode == ErrorCodes.ERROR_BUSY ||
//        //                    deviceException.ErrorCode == ErrorCodes.MTP_Device_Busy)
//        //                {
//        //                    Thread.Sleep(100);
//        //                    Log.Debug("Retry live view :" + deviceException.ErrorCode.ToString("X"));
//        //                    retry = true;
//        //                    retryNum++;
//        //                }
//        //            }
//        //        } while (retry && retryNum < 35);


//        //        // notify new liveview image available on timer tick
//        //        _timerLiveView.Elapsed += (sender, eventArgs) => Task.Factory.StartNew(NotifyNewLiveViewImage);
//        //        _timerLiveView.Start();

//        //    }
//        //    catch (Exception exception)
//        //    {
//        //        Log.Debug("Unable to start liveview ! " + exception.ToString());
//        //    }
//        //}


//        private void NotifyNewLiveViewImage(LiveViewData  liveViewData)
//        {
//            // nobody to notify
//            if (NewLiveViewImage == null || liveViewData == null || liveViewData.ImageData == null)
//                return;


//            //    var bitmap = new Bitmap(new MemoryStream(liveViewData.ImageData,
//            //         liveViewData.ImageDataPosition,
//            //         liveViewData.ImageData.Length - liveViewData.ImageDataPosition));


//            //    if ( CameraCropFactor > 0 && CameraCropFactor < 100)
//            //{
//            //    bitmap = bitmap.crop(CameraCropFactor);
//            //}

//            //    NewLiveViewImage?.Invoke(bitmap);
           
//        }

       


//    }
//}
