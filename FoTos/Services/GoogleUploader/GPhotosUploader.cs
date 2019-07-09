using GPhotosClientApi;
using log4net;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace FoTos.Services.GoogleUploader
{
    class GPhotosUploader : IGPhotosUploader
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public String UploadDirectory { get; private set; }
        public String CredentialsFile { get; private set; }
        public String TokenStoreDir { get; private set; }

        public String AlbumName { get; private set; }

        public GPhotosClient Client { get; private set; }

        public GPhotosUploader(String credentialsFile, String tokenStoreDir, String albumName)
        {
            CredentialsFile = credentialsFile;
            TokenStoreDir = tokenStoreDir;
            AlbumName = albumName;

            // init GPhotos client
            log.Info("Init GPhotos client");
            Client = new GPhotosClient(@"c:\tmp\credentials.json", @"c:\tmp\tokenStore", "photomaton");

            // check album exists
            log.Info(String.Format("Check album '{0}' exists",  albumName));
            // TODO

            // create dir if not exists
            if (!Directory.Exists(dir))
            {
                log.Info(string.Format("create upload directory: '{0}'", dir));
                Directory.CreateDirectory(dir);
            }

            //// Create a new FileSystemWatcher and set its properties.
            //using (FileSystemWatcher watcher = new FileSystemWatcher())
            //{
            //    watcher.Path = dir;

            //    // Watch for changes in LastAccess and LastWrite times, and
            //    // the renaming of files or directories.
            //    watcher.NotifyFilter = NotifyFilters.LastAccess
            //                         | NotifyFilters.LastWrite
            //                         | NotifyFilters.FileName
            //                         | NotifyFilters.DirectoryName;

            //    // Only watch text files.
            //    watcher.Filter = "*.*";

            //    // Add event handlers.
            //    watcher.Created += OnFileCreated;
            //    //watcher.Changed += OnChanged;
            //    //watcher.Deleted += OnChanged;
            //    //watcher.Renamed += OnRenamed;

            //    // Begin watching.
            //    watcher.EnableRaisingEvents = true;
            //}
        }


        public async Task Upload(String filename)
        {
            log.Info(String.Format("GPhotos upload: '{0}'", filename));
            try
            {
                Client.UploadMedia(filename);
            }
            catch(Exception ex)
            {
                log.Error(String.Format("failed to upload photo '{0}' to Google Photos", filename));
            }
            
        }
    }
}
