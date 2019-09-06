using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GPhotosClientApi.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


         


        }

        private GPhotosClient _client = null;


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // variables
            var credentialsFilePath = @"c:\tmp\credentials.json";
            var tokenStoreDirPath   = @"c:\tmp\tokenStore";
            var userName            = "test";
            var scopes = new String[]{
                "https://www.googleapis.com/auth/photoslibrary",
                "https://www.googleapis.com/auth/photoslibrary.sharing" };



            // need to clear tokenStore when credentials/scope has changed
            //if (Directory.Exists(tokenStoreDirPath))
            //     Directory.Delete(tokenStoreDirPath, true);


            // instantiate Google photos client 
            _client = new GPhotosClient(credentialsFilePath, tokenStoreDirPath, userName, scopes);

  

            var toto = 0;
        }


        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {         
            
            // list all albums
            var albums = _client.GetMediaItems(null).Result;

            int toto = 0;

        }
    }
}
