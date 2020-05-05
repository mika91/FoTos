using log4net;
using System;
using System.Configuration;
using System.Reflection;
using System.Windows;
using FoTos.Views;
using FoTos.Services.Camera;
using FoTos.Services.Camera.mock;
using FoTos.Services.SlideShowProvider;
using FoTos.Services.GoogleUploader;
using FoTos.Services.Printer;

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
            public String SlideShowFolder           { get; private set; }
            public String CameraRollFolder          { get; private set; }
            public int CameraCropFactor             { get; private set; }
            public int ExportJpegQualityLevel       { get; private set; }
            public String PreferedCamera            { get; private set; }
            public String PrinterName               { get; private set; }
            public String GoogleUploadFolder        { get; private set; }
            public String GoogleTokenStoreFolder    { get; private set; }
            public String GoogleCredentialsFile     { get; private set; }
            public String GoogleUserName            { get; private set; }
            public String GoogleAlbumName           { get; private set; }
            public String GoogleAlbumShareUrl       { get; private set; }
            public int GoogleSyncSeconds { get; private set; }
            public Boolean UseCameraMock            { get; private set; }
            public int ShootingViewIdleTimeSeconds { get; private set; }
            public int ThanksViewIdleTimeSeconds { get; private set; }


            public AppSettings()
            {
                // photomaton settings
                SlideShowFolder         = ConfigurationManager.AppSettings["SlideShowFolder"];
                CameraRollFolder        = ConfigurationManager.AppSettings["CameraRollFolder"];
                PreferedCamera          = ConfigurationManager.AppSettings["PreferedCamera"];
                PrinterName             = ConfigurationManager.AppSettings["PrinterName"];
                CameraCropFactor        = int.Parse(ConfigurationManager.AppSettings["CameraCropFactor"] ?? "100");
                ExportJpegQualityLevel  = int.Parse(ConfigurationManager.AppSettings["ExportJpegQualityLevel"] ?? "90");

                // google services settings
                GoogleUploadFolder      = ConfigurationManager.AppSettings["GoogleUploadFolder"];
                GoogleTokenStoreFolder  = ConfigurationManager.AppSettings["GoogleTokenStoreFolder"];
                GoogleCredentialsFile   = ConfigurationManager.AppSettings["GoogleCredentialsFile"];
                GoogleUserName          = ConfigurationManager.AppSettings["GoogleUserName"];
                GoogleAlbumName         = ConfigurationManager.AppSettings["GoogleAlbumName"];
                GoogleAlbumShareUrl     = ConfigurationManager.AppSettings["GoogleAlbumShareUrl"];
                GoogleSyncSeconds       = int.Parse(ConfigurationManager.AppSettings["GoogleSyncSeconds"] ?? "900");

                // UI related stuffs
                ShootingViewIdleTimeSeconds = int.Parse(ConfigurationManager.AppSettings["ShootingViewIdleTimeSeconds"] ?? "20");
                ThanksViewIdleTimeSeconds   = int.Parse(ConfigurationManager.AppSettings["ThanksViewIdleTimeSeconds"] ?? "3");

                // camera mock
                UseCameraMock           = Boolean.Parse(ConfigurationManager.AppSettings["UseCameraMock"] ?? "false");
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
                camera = new CameraServiceMock(Settings.CameraRollFolder);
            }
            else
            {
                //camera = new CameraService(Settings.CameraRollFolder, Settings.PreferedCamera, Settings.CameraCropFactor);
                camera = new CanonCameraService(Settings.CameraRollFolder, Settings.PreferedCamera, Settings.CameraCropFactor);
            }

            var printerService = new PrinterService(Settings.PrinterName);

            var slideShow = new SlideShowService();
            var uploader = new GPhotosUploader(Settings.GoogleCredentialsFile, Settings.GoogleTokenStoreFolder,
                 Settings.GoogleAlbumName, Settings.GoogleUserName,Settings.GoogleUploadFolder, Settings.GoogleSyncSeconds);
            Services = new AppServices(camera, printerService, slideShow, uploader);
        }

        public class AppServices
        {
            public ICameraService CameraService { get; private set; }
            public IPrinterService PrinterService { get; private set; }
            public ISlideShowService SlideshowService { get; private set; }
            public IGPhotosUploader GPhotosUploader { get; private set; }

            public AppServices(ICameraService cameraService, IPrinterService printerService, ISlideShowService slideshowService, IGPhotosUploader uploaderService)
            {
                this.CameraService      = cameraService;
                this.SlideshowService   = slideshowService;
                this.GPhotosUploader    = uploaderService;
                this.PrinterService     = printerService;
            }

        }

        #endregion

    }


}
