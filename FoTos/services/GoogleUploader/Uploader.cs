//using log4net;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace FoTos.uploader
//{
//    class Uploader
//    {
//        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


//        public String UploadDirectory { get; private set; }
//        private String CredentialsPath { get; set; }
//        private String CredentiralsStore { get; set; }

//        public String Album { get; private set; }

//        public Uploader(String dir, String credentialsPath, String credentiralsStore, String albumName)
//        {
//            UploadDirectory = dir;
//            CredentialsPath = credentialsPath;
//            CredentiralsStore = credentiralsStore;
//            Album = albumName;

//            // create dir if not exists
//            if (!Directory.Exists(dir))
//            {
//                log.Info(string.Format("create upload directory: '{0}'", dir));
//                Directory.CreateDirectory(dir);
//            }

//            // Create a new FileSystemWatcher and set its properties.
//            using (FileSystemWatcher watcher = new FileSystemWatcher())
//            {
//                watcher.Path = dir;

//                // Watch for changes in LastAccess and LastWrite times, and
//                // the renaming of files or directories.
//                watcher.NotifyFilter = NotifyFilters.LastAccess
//                                     | NotifyFilters.LastWrite
//                                     | NotifyFilters.FileName
//                                     | NotifyFilters.DirectoryName;

//                // Only watch text files.
//                watcher.Filter = "*.*";

//                // Add event handlers.
//                watcher.Created += OnFileCreated;
//                //watcher.Changed += OnChanged;
//                //watcher.Deleted += OnChanged;
//                //watcher.Renamed += OnRenamed;

//                // Begin watching.
//                watcher.EnableRaisingEvents = true;
//            }
//        }

//        // TODO: task ? thread ? bacground worker ?
//        public void Run()
//        {
                
//        }
//    }
//}
