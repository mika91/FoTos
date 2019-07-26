
using FoTos.utils;
using log4net;
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

namespace FoTos.Views
{
    /// <summary>
    /// Interaction logic for ThanksView.xaml
    /// </summary>
    public partial class ThanksView : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        MainWindow MainWindow { get { return Dispatcher.Invoke(() => Window.GetWindow(this) as MainWindow); } }

        private System.Timers.Timer _idleTimer;
        private DateTime _loadedDate;


        public ThanksView()
        {
            InitializeComponent();
        }

        private void ThanksView_Unloaded(object sender, RoutedEventArgs e)
        {
            log.Debug("ThanksView::Unloaded");

            // idle timer
            if (_idleTimer != null)
            {
                _idleTimer.Stop();
                _idleTimer.Elapsed -= idleTimer_Elapsed;
                _idleTimer = null;
            }
        }

        private void ThanksView_Loaded(object sender, RoutedEventArgs e)
        {
            log.Debug("ThanksView::Unloaded");

            if (DesignerProperties.GetIsInDesignMode(this))
                return;


            // idle time
            _loadedDate = DateTime.Now;
            _idleTimer = new System.Timers.Timer();
            _idleTimer.Interval = 1000 * MainWindow.App.Settings.ThanksViewIdleTimeSeconds;
            _idleTimer.Elapsed += idleTimer_Elapsed;
            _idleTimer.Enabled = true;
        }

        private void idleTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MainWindow.GotoHomePage();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.GotoHomePage();
        }
    }
}
