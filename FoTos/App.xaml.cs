using log4net;
using System;
using System.Configuration;
using System.Reflection;
using System.Windows;
using FoTos.Views;
using FoTos.Services.Camera;
using FoTos.Services.Camera.mock;
using FoTos.Services.SlideShowProvider;

namespace FoTos
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
            log.Info("Load application services");
            LoadServices();

            // Create main application window
            log.Info("Start MainWindow");
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }


        #region Settings
        
        public AppSettings Settings { get; private set; }


        private void LoadSettings()
        {
            Settings = new AppSettings();
           
        }


        public class AppSettings
        {
            public String SlideShowFolder   { get; private set; }
            public String CameraRollFolder  { get; private set; }
            public String FinalPhotosFolder { get; private set; }
            public Boolean UseCameraMock    { get; private set; }

            public AppSettings()
            {
                SlideShowFolder     = ConfigurationManager.AppSettings["SlideShowFolder"];
                CameraRollFolder    = ConfigurationManager.AppSettings["CameraRollFolder"];
                FinalPhotosFolder   = ConfigurationManager.AppSettings["FinalPhotosFolder"];
                UseCameraMock       = Boolean.Parse(ConfigurationManager.AppSettings["UseCameraMock"] ?? "false");
            }
        }


        #endregion

        #region Services

        public AppServices Services { get; private set; }

        private void LoadServices()
        {
            ICameraService camera;
            if (Settings.UseCameraMock)
            {
                log.Warn("using cameraman mock");
                camera = new CameraServiceMock();
            }
            else
            {
                camera = new CameraService(Settings.CameraRollFolder);

            }

            var slideShow = new SlideShowService();

            Services = new AppServices(camera, slideShow);
        }

        public class AppServices
        {
            public ICameraService CameraService { get; private set; }
            public ISlideShowService SlideshowService { get; private set; }

            public AppServices(ICameraService cameraService, ISlideShowService slideshowService)
            {
                this.CameraService      = cameraService;
                this.SlideshowService   = slideshowService;
            }

        }

        #endregion

    }


}
