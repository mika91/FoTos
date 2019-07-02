using log4net;
using photo_tos_maton.camera;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace photo_tos_maton
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        void App_Startup(object sender, StartupEventArgs e)
        {
            // load settings from app.config
            log.Info("Load application settings");
            LoadSettings();

            // load services

            // Create main application window
            log.Info("Start MainWindow");
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }


        #region Settings

        public String SlideShowFolder { get; private set; }
        public String CameraRollFolder { get; private set; }
        public String FinalPhotosFolder { get; private set; }
        public Boolean UseCameraMock { get; private set; }


        private void LoadSettings()
        {
            SlideShowFolder = ConfigurationManager.AppSettings["SlideShowFolder"];
            CameraRollFolder = ConfigurationManager.AppSettings["CameraRollFolder"];
            FinalPhotosFolder = ConfigurationManager.AppSettings["FinalPhotosFolder"];
            UseCameraMock = Boolean.Parse(ConfigurationManager.AppSettings["UseCameraMock"] ?? "false");
        }

        #endregion

        #region Services

        public AppServices Services { get; private set; }

        private void LoadServices()
        {
            Services = new AppServices(
                new CameraMan()
            //TODO: uploader
            );

        }

        public class AppServices
        {
            public ICameraMan CameraMan { get; private set; }

            public AppServices(ICameraMan cameraMan)
            {
                this.CameraMan = cameraMan;
            }

        }

        #endregion

    }


}
