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

            var client = new GPhotosClient(
      @"C:\Users\mika\Documents\PhotoBox\FoTos\output\private\credentials.json",
      @"C:\Users\mika\Documents\PhotoBox\FoTos\output\private\tokenStore",
      "photomaton");

            var albums = client.GetAlbumsPage().Result;
            int toto = 0;

        }
    }
}
