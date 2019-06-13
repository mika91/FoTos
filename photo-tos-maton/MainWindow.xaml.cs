using CameraControl.Devices;
using log4net;
using photo_tos_maton.pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace photo_tos_maton
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private PhotoPage _photoPage;
        private HomePage _homePage;

        public MainWindow()
        {
            log.Info("MainWindow InitializeComponent");
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            initPages();
            LoadHomePage();
        }

        private void initPages()
        {
            // TODO: ajouter un 'OnUnload' sur toutes les pages pour centraliser ici toute la gestion des etas IHM / Camera

            // home page
            _homePage = new HomePage();
            _homePage.GotoPhotoPageHandler = LoadPhotoPage;

            // photo page
            _photoPage = new PhotoPage();
            _photoPage.GotoBackPageHandler = LoadHomePage;

            LoadHomePage();
        }

        private void LoadPhotoPage()
        {
            transitionBox.Content = _photoPage;
            _photoPage.StartLiveView();
        }

        private void LoadHomePage()
        {
            // TODO: should be done in photopage unload 
            _photoPage.StopLiveView();

            transitionBox.Content = _homePage;
        }
    }
}
