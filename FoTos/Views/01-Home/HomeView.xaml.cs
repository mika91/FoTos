using log4net;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace FoTos.Views
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
        }


        #region Dependency Injection

        // dependency injection
        public void Init(String slideShowFolder)
        {
            // run slideshow
            //var folder = ((App)System.Windows.Application.Current).SlideShowFolder;
            slideShowControl.Start(slideShowFolder);
        }

        // clean dependencies (should be called form Unloaded event)
        public void Release()
        {
            slideShowControl.Stop();
        }

        #endregion


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.GotoShootingPage();
        }
    }
}
