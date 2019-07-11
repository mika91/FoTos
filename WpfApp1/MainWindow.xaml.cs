using GPhotosClientApi;
using System;
using System.Collections.Generic;
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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

           

            this.Loaded += MainWindow_Loaded;



        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var client = new GPhotosClient(
               @"C:\tmp\credentials.json",
               @"C:\tmp\tokenStore",
               "photomaton");


            var albums = await client.GetAlbumsPage();
            albums.albums.ForEach(a => Console.WriteLine(a.title));

        
        }
    }
}
