using EOSDigital.API;
using EOSDigital.SDK;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.IO;
using CanonCamera = EOSDigital.API.Camera;
using System.Threading.Tasks;
using FoTos.utils;

namespace FoTos.Services.Camera
{
    public class CanonCameraService : ICameraService
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Properties

        public String CameraRollFolder { get; private set; }

        public String PreferedCamera { get; private set; }

        public int CameraCropFactor { get; private set; }

        #endregion


        #region Variables

        CanonAPI APIHandler;
        CanonCamera MainCamera;
        List<CanonCamera> CamList;
        bool IsInit = false;

        int ErrCount;
        object ErrLock = new object();

        #endregion


        public CanonCameraService(String cameraRollFolder, String preferedCamera = null, int cameraCropFactor = 100)
        {
            // DI
            CameraRollFolder = cameraRollFolder;
            PreferedCamera = preferedCamera;
            CameraCropFactor = cameraCropFactor;


            try
            {
                log.Info("init canon camera service");
                var APIHandler = new CanonAPI();
                APIHandler.CameraAdded += APIHandler_CameraAdded;
                ErrorHandler.SevereErrorHappened += ErrorHandler_SevereErrorHappened;
                ErrorHandler.NonSevereErrorHappened += ErrorHandler_NonSevereErrorHappened;
                RefreshCamera();
                IsInit = true;
            }
                catch (DllNotFoundException) { ReportError("Canon DLLs not found!", true);
            }       
                catch (Exception ex) { ReportError(ex.Message, true);
            }
        }

        public void Release()
        {
            log.Info("release canon camera service");
            try
            {
                IsInit = false;
                MainCamera?.CloseSession();
                MainCamera?.Dispose();
                APIHandler?.Dispose();
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }



        #region API Events

        private void APIHandler_CameraAdded(CanonAPI sender)
        {
            //try { Dispatcher.Invoke((Action)delegate { RefreshCamera(); }); }
            //catch (Exception ex) { ReportError(ex.Message, false); }

            log.Info("camera added");
            try {  RefreshCamera(); }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void MainCamera_StateChanged(CanonCamera sender, StateEventID eventID, int parameter)
        {
            //try { if (eventID == StateEventID.Shutdown && IsInit) { Dispatcher.Invoke((Action)delegate { CloseSession(); }); } }
            //catch (Exception ex) { ReportError(ex.Message, false); }

            log.InfoFormat("camera state changed = {0}", eventID);
            try { if (eventID == StateEventID.Shutdown && IsInit) {  CloseSession(); } }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void MainCamera_ProgressChanged(object sender, int progress)
        {
            //try { MainProgressBar.Dispatcher.Invoke((Action)delegate { MainProgressBar.Value = progress; }); }
            //catch (Exception ex) { ReportError(ex.Message, false); }

            try { log.DebugFormat("progress = {0}", progress); }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private async void MainCamera_LiveViewUpdated(CanonCamera sender, Stream img)
        {
            log.Info("live view updated");
            await NotifyNewLiveViewImage(img);
        }

        private async void MainCamera_DownloadReady(CanonCamera sender, DownloadInfo Info)
        {
            log.Info("download ready");
            await SavePictureAsync(sender, Info);
        }

        private void ErrorHandler_NonSevereErrorHappened(object sender, ErrorCode ex)
        {
            ReportError($"SDK Error code: {ex} ({((int)ex).ToString("X")})", false);
        }

        private void ErrorHandler_SevereErrorHappened(object sender, Exception ex)
        {
            ReportError(ex.Message, true);
        }

        #endregion




        #region LiveView

        public event Action<Bitmap> NewLiveViewImage;

        public void StartLiveView()
        {
            try
            {
                log.Info("start liveview");
                MainCamera?.StartLiveView();
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        public void StopLiveView()
        {
            try
            {
                log.Info("stop liveview");
                MainCamera?.StopLiveView();
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private async Task NotifyNewLiveViewImage(Stream img)
        {
            try
            {
                // nobody to notify
                if (NewLiveViewImage == null || img == null )
                    return;

                var bitmap = new Bitmap(img);

                // crop
                if (CameraCropFactor > 0 && CameraCropFactor < 100)
                {
                    bitmap = bitmap.crop(CameraCropFactor);
                }

                NewLiveViewImage?.Invoke(bitmap);

            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }


        #endregion

        #region take picture

        public event Action<string> NewPhoto;

        public void TakePicture()
        {
            try
            {
               log.Info("take picture");
               MainCamera?.TakePhotoAsync();
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private async Task SavePictureAsync(CanonCamera sender, DownloadInfo Info)
        {
          
            try
            {
                //// check the folder of filename, if not found create it
                //var dir = Path.GetDirectoryName(fileFullName);
                //if (!Directory.Exists(dir))
                //{
                //    log.Info("create camera roll folder = " + dir);
                //    Directory.CreateDirectory(dir);
                //}


                // naming
                var date = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                Info.FileName = date + ".jpg";
                var fileFullName = Path.Combine(CameraRollFolder, Info.FileName);

                log.InfoFormat("saving picture = {0}", fileFullName);
                sender.DownloadFile(Info);


                //string tempFile = Path.GetTempFileName();

                //// todo
                //if (File.Exists(tempFile))
                //    File.Delete(tempFile);

                //Stopwatch stopWatch = new Stopwatch();

                //log.Debug("transfer photo to tempory folder");
                //// transfer file from camera  
                //// in this way if the session folder is used as hot folder will prevent write errors
                //stopWatch.Start();
                //eventArgs.CameraDevice.TransferFile(eventArgs.Handle, tempFile); // TODO: gérer erreur ??

                //// release resource
                //eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);

                //eventArgs.CameraDevice.IsBusy = false;
                //stopWatch.Stop();

                //// TODO: add option to enable/disable feature
                //// TODO: controller avant si la photo a bien été copiée
                //if (!eventArgs.CameraDevice.CaptureInSdRam)
                //{
                //    log.Debug("remove picture from SD card");
                //    eventArgs.CameraDevice.DeleteObject(new DeviceObject() { Handle = eventArgs.Handle });
                //}

                //// move photo to destination folder
                //log.Info(string.Format("move photo to '{0}", fileFullName));
                //File.Copy(tempFile, fileFullName);
                //WaitForFile(fileFullName);

                //try
                //{
                //    File.Delete(tempFile);
                //}
                //catch (Exception ex)
                //{
                //    log.Warn(string.Format("failed to remove tmeporary photo file '{0}'", tempFile), ex);
                //}


                // notify new photo
                if (NewPhoto != null)
                {
                    try
                    {
                        NewPhoto.Invoke(fileFullName);
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("failed to load camera roll photo: filename = '{0}'", fileFullName), ex);
                    }
                }




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
               // eventArgs.CameraDevice.IsBusy = false;
               ReportError("Error download photo from camera :\n" + ex.Message);
            }
        }

        #endregion

        #region subroutines

        // TODO
        private void SelectCamera()
        {
            log.Info("select camera");
            if (MainCamera?.SessionOpen == true) CloseSession();

            if (CamList.Count > 0)
            {
                OpenSession(CamList[0]);

                // save to host
                MainCamera.SetSetting(PropertyID.SaveTo, (int)SaveTo.Host);
                MainCamera.SetCapacity(4096, int.MaxValue);


            }
        }

        private void RefreshCamera()
        {
            //CameraListBox.Items.Clear();
            //CamList = APIHandler.GetCameraList();
            //foreach (Camera cam in CamList) CameraListBox.Items.Add(cam.DeviceName);
            //if (MainCamera?.SessionOpen == true) CameraListBox.SelectedIndex = CamList.FindIndex(t => t.ID == MainCamera.ID);
            //else if (CamList.Count > 0) CameraListBox.SelectedIndex = 0;

            try {
                CamList = APIHandler.GetCameraList();
                log.InfoFormat("available cameras: ", String.Join("\n\t- ", CamList.Select(c => c.DeviceName)));
            }
            catch (Exception ex) { ReportError(ex.Message, false); }

        }

        private void CloseSession()
        {
            if (MainCamera != null)
            {
                log.InfoFormat("close camera session = {0}", MainCamera?.DeviceName);
                MainCamera.LiveViewUpdated -= MainCamera_LiveViewUpdated;
                MainCamera.ProgressChanged -= MainCamera_ProgressChanged;
                MainCamera.StateChanged -= MainCamera_StateChanged;
                MainCamera.DownloadReady -= MainCamera_DownloadReady;
                MainCamera.CloseSession();
                MainCamera = null;
            }
        }

        private void OpenSession(CanonCamera camera)
        {
            if (camera != null)
            {
                log.InfoFormat("close camera session = {0}", camera?.DeviceName);
                MainCamera = camera;
                MainCamera.OpenSession();
                MainCamera.LiveViewUpdated += MainCamera_LiveViewUpdated;
                MainCamera.ProgressChanged += MainCamera_ProgressChanged;
                MainCamera.StateChanged += MainCamera_StateChanged;
                MainCamera.DownloadReady += MainCamera_DownloadReady;

            }
        }


        private void ReportError(string message, bool lockdown = false)
        {
            int errc;
            lock (ErrLock) { errc = ++ErrCount; }

            //if (lockdown) EnableUI(false);

            //if (errc < 4) MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //else if (errc == 4) MessageBox.Show("Many errors happened!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            log.Error(message);

            lock (ErrLock) { ErrCount--; }
        }

        #endregion

        #region wrapstream
        /// <summary>
        /// A stream that does nothing more but wrap another stream (needed for a WPF memory leak)
        /// </summary>
        public sealed class WrapStream : Stream
        {
            /// <summary>
            /// Gets a value indicating whether the current stream supports reading.
            /// </summary>
            public override bool CanRead
            {
                get { return Base.CanRead; }
            }
            /// <summary>
            /// Gets a value indicating whether the current stream supports seeking.
            /// </summary>
            public override bool CanSeek
            {
                get { return Base.CanSeek; }
            }
            /// <summary>
            /// Gets a value indicating whether the current stream supports writing.
            /// </summary>
            public override bool CanWrite
            {
                get { return Base.CanWrite; }
            }
            /// <summary>
            /// Gets the length in bytes of the stream.
            /// </summary>
            public override long Length
            {
                get { return Base.Length; }
            }
            /// <summary>
            /// Gets or sets the position within the current stream.
            /// </summary>
            public override long Position
            {
                get { return Base.Position; }
                set { Base.Position = value; }
            }

            private Stream Base;

            /// <summary>
            /// Creates a new instance of the <see cref="WrapStream"/> class.
            /// </summary>
            /// <param name="inStream">The stream that gets wrapped</param>
            public WrapStream(Stream inStream)
            {
                Base = inStream;
            }

            /// <summary>
            /// reads a sequence of bytes from the current stream and advances
            /// the position within the stream by the number of bytes read.
            /// </summary>
            /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified
            /// byte array with the values between offset and (offset + count - 1) replaced
            /// by the bytes read from the current source.</param>
            /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read
            /// from the current stream.</param>
            /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
            /// <returns>The total number of bytes read into the buffer. This can be less than the
            /// number of bytes requested if that many bytes are not currently available,
            /// or zero (0) if the end of the stream has been reached.</returns>
            public override int Read(byte[] buffer, int offset, int count)
            {
                return Base.Read(buffer, offset, count);
            }

            /// <summary>
            /// When overridden in a derived class, writes a sequence of bytes to the current
            /// stream and advances the current position within this stream by the number
            /// of bytes written.
            /// </summary>
            /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
            /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the
            /// current stream.</param>
            /// <param name="count">The number of bytes to be written to the current stream.</param>
            public override void Write(byte[] buffer, int offset, int count)
            {
                Base.Write(buffer, offset, count);
            }

            /// <summary>
            /// sets the position within the current stream.
            /// </summary>
            /// <param name="offset">A byte offset relative to the origin parameter.</param>
            /// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used
            /// to obtain the new position.</param>
            /// <returns>The new position within the current stream.</returns>
            public override long Seek(long offset, System.IO.SeekOrigin origin)
            {
                return Base.Seek(offset, origin);
            }

            /// <summary>
            /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
            /// </summary>
            public override void Flush()
            {
                Base.Flush();
            }

            /// <summary>
            /// Sets the length of the current stream.
            /// </summary>
            /// <param name="value">The desired length of the current stream in bytes.</param>
            public override void SetLength(long value)
            {
                Base.SetLength(value);
            }
        }

        #endregion
    }
}
