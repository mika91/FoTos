using photo_tos_maton.pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace photo_tos_maton
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PhotoPage _photoPage;
        private HomePage _homePage;

        public MainWindow()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            initPages();
            LoadHomePage();
        }

        private void initPages()
        {
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
        }

        private void LoadHomePage()
        {
            transitionBox.Content = _homePage;
        }
    }
}
