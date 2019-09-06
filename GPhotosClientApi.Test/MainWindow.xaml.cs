using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GPhotosClientApi.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string CREDENTIALS_FILE       = @"c:\tmp\credentials.json";
        const string TOKEN_STORE_DIR_PATH   = @"c:\tmp\tokenStore";
        const string USER_NAME              = "test";
        const string ALBUM_NAME             = "TEST_ALBUM_2";
        const string UPLOAD_DIR             = @"C:\tmp\pictures";

        public MainWindow()
        {
            InitializeComponent();
        }

        private GPhotosClient Client { get; set; }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // need to clear tokenStore when credentials/scope has changed
            
            //if (Directory.Exists(tokenStoreDirPath))
            //     Directory.Delete(tokenStoreDirPath, true);

            // instantiate Google photos client 
            Client = new GPhotosClient(CREDENTIALS_FILE, TOKEN_STORE_DIR_PATH, USER_NAME);
         }


        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private async void ButtonUpload_Click(object sender, RoutedEventArgs e)
        {
            // get album Id
            var albumId = await GetOrCreateAlbumdId();

            // list photos to upload
            var pictures = GetPicturesToUpload();

            // then upload each one
            foreach (var filename in pictures)
                await UploadFile(filename, albumId);
        }


        int nextIndex = 0;
        private async void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            // refresh media items 
            var mediaItems = await GetMedias();

            // set image in viewer
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(mediaItems[nextIndex].baseUrl);
            bitmap.EndInit();

            this.ImageViewer.Source = bitmap;

            // increment counter
            nextIndex = (nextIndex + 1) % mediaItems.Count;
        }

        private async Task<List<MediaItem>> GetMedias()
        {
            // get album Id
            var albumId = await GetOrCreateAlbumdId();

            // list all pictures in album (paginated 100 items)
            var albums = await Client.GetAlbumMediaItems(albumId);

            // store items (to be displayed)
            return albums.mediaItems;
        }


        private async Task<String> GetOrCreateAlbumdId()
        {
            // list all albums
            var albums = await Client.GetAllAlbums();
            
            // check album exists
            var album = albums.FirstOrDefault(a => a.title == ALBUM_NAME);
           
            // if not, create it
            if (album == null)
            {
                album = await Client.CreateAlbum(ALBUM_NAME);
            }

            return album.id;
        }


        private async Task UploadFile(String fileName, String albumId)
        {
            // upload and get upload token
            var uploadToken = await Client.UploadMedia(fileName);
            if (!String.IsNullOrEmpty(uploadToken))
            {
                var result = await Client.BatchCreate(uploadToken, albumId);
                result.newMediaItemResults.ForEach(r => Console.WriteLine(string.Format("mediaItem='{0}' added to album='{1}'", r.mediaItem.filename, albumId)));
            }
        }

        private List<String> GetPicturesToUpload()
        {
            // list pictures to upload
            var allowedExtensions = new[] { ".jpg", ".jpeg" };
            return Directory
                 .GetFiles(UPLOAD_DIR)
                 .Where(file => allowedExtensions.Any(System.IO.Path.GetFileName(file).ToLower().EndsWith))
                 .ToList();
        }

     
    }
}
