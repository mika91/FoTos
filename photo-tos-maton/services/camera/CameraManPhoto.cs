using CameraControl.Devices.Classes;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;

namespace photo_tos_maton.camera
{
    public partial class CameraMan
    {
        public event Action<Bitmap> NewPhoto;

        public void TakePicture()
        {
            log.Info("CameraMan::TakePicture");
            
            try
            {
                log.Info("call device photo capture");
                Camera?.CapturePhotoNoAf();
            } catch (Exception ex)
            {
                log.Error("An error occured whil taking picture", ex);
            }
            
        }

        private void Manager_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
        {
            log.Info(string.Format("new photo captured: filename = {0}", eventArgs.FileName));

            // to prevent UI freeze start the transfer process in a new thread
            Thread thread = new Thread(OnPhotoCapturedThread);
            thread.Start(eventArgs);

        }
        private void OnPhotoCapturedThread(Object o)
        {
            var eventArgs = o as PhotoCapturedEventArgs;
            if (eventArgs == null)
                return;

            try
            {
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

                // TODO: add option to enable/disable feature
                // TODO: controller avant si la photo a bien été copiée
                if (!eventArgs.CameraDevice.CaptureInSdRam)
                {
                    log.Debug("remove picture from SD card");
                    eventArgs.CameraDevice.DeleteObject(new DeviceObject() { Handle = eventArgs.Handle });
                }

                // move photo to destination folder
                log.Info(string.Format("move photo to '{0}", fileName));
                File.Copy(tempFile, fileName);
                WaitForFile(fileName);

                try
                {
                    File.Delete(tempFile);
                } catch (Exception ex)
                {
                    log.Warn(string.Format("failed to remove tmeporary photo file '{0}'", tempFile), ex);
                }
               

                // notify new photo
                if (NewPhoto != null)
                {
                    try{
                        var img = new Bitmap(fileName);
                        NewPhoto.Invoke(img);
                    } catch(Exception ex)
                    {
                        log.Error(string.Format("failed to load camera roll photo: filename = '{0}'", fileName));
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
                eventArgs.CameraDevice.IsBusy = false;
                log.Debug("Error download photo from camera :\n" + ex.Message);
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
