using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace photo_tos_maton.pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : UserControl
    {
        public Action GotoPhotoPageHandler { get; set; }

        public HomePage()
        {
            InitializeComponent();

            // run slideshow
            slideShowControl.Start(ConfigurationManager.AppSettings["SlideShowDirPath"].ToString());
        }

        



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GotoPhotoPageHandler?.Invoke();
        }
    }
}
