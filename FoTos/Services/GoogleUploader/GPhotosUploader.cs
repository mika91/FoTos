using GPhotosClientApi;
using log4net;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using FoTos.utils;
using System.Linq;

namespace FoTos.Services.GoogleUploader
{
    class GPhotosUploader : IGPhotosUploader
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    
        public String CredentialsFile { get; private set; }
        public String TokenStoreFolder { get; private set; }

        public String UploadDirectory { get; private set; }

        public String AlbumName { get; private set; }

        public String AlbumId { get; private set; }

        public String UserName { get; private set; }

        public GPhotosClient Client { get; private set; }

        public GPhotosUploader(String credentialsFile, String tokenStoreFolder, String albumName, string userName, String uploadDir)
        {
            CredentialsFile     = credentialsFile;
            TokenStoreFolder    = tokenStoreFolder;
            UserName            = UserName;
            AlbumName           = albumName;
            UploadDirectory     = uploadDir;

            // init GPhotos client
            log.Info("Init GPhotos client");
            Client = new GPhotosClient(
               @credentialsFile,
               @tokenStoreFolder,
               userName);


           


            //// create dir if not exists
            //if (!Directory.Exists(dir))
            //{
            //    log.Info(string.Format("create upload directory: '{0}'", dir));
            //    Directory.CreateDirectory(dir);
            //}

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

        public async Task CheckAlbum()
        {
            // check album exists
            log.Info(String.Format("Check album '{0}' exists", AlbumName));

            var albums = await Client.GetAllAlbums();
            log.Info("albums = " + String.Join(", ", albums.Select(a => a.title)));

            AlbumId = albums.FirstOrDefault(a => a.title == AlbumName)?.id;
            if (AlbumId == null)
            {
                log.Info(String.Format("Create album '{0}'", AlbumName));
                var createdAlbum = await Client?.CreateAlbum(AlbumName);
                log.Info(String.Format("Album successfully created"));
                AlbumId = createdAlbum.id;
            }
        }


        public async Task Upload(String filename)
        {
            log.Info(String.Format("GPhotos upload: '{0}'", filename));
            try
            {
                // check album
                if (AlbumId == null)
                    await CheckAlbum();

                // upload photon then add to 
                var fileFullName = Path.Combine(UploadDirectory, filename);
                var uploadToken = await Client.UploadMedia(fileFullName);
                log.Info("uploadToken = " + uploadToken);
                if (!String.IsNullOrEmpty(uploadToken))
                {
                    var result = await Client.BatchCreate(uploadToken, AlbumId);
                    result.newMediaItemResults.ForEach(r => Console.WriteLine(string.Format("mediaItem='{0}' added to album='{1}'", r.mediaItem.filename, AlbumId)));
                }
            }
            catch(Exception ex)
            {
                log.Error(String.Format("failed to upload photo '{0}' to Google Photos", filename), ex);
            }
            
        }
        
    }
}
