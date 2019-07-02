using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace photo_tos_maton.Views._01_HomeView
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        MainWindow MainWindow { get { return Dispatcher.Invoke(() => Window.GetWindow(this) as MainWindow); } }

        public HomeView()
        {
            InitializeComponent(); 
            
            // run slideshow
            var folder = ((App)System.Windows.Application.Current).SlideShowFolder;
            //slideShowControl.Start(folder);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GotoShootingPage();
        }
    }
}
