using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;

namespace FoTos.Services.Printer
{

    // TODO: move to WPF code
    // TODO: dialog to choose printer


    /// <summary>
    /// https://docs.microsoft.com/fr-fr/dotnet/api/system.drawing.printing.printdocument.print?view=dotnet-plat-ext-3.1
    /// </summary>
    public class PrinterService : IPrinterService
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private String _imgToPrint;
        private PrinterDefinition _printer;

        public PrinterService(String printerName)
        {

            var printers = ListPrinters();

            // TODO: popup to choose printer ???
            // if (printerName == null)
            // {
            //    //
            //} 

            var selectedPrinter = printers.FirstOrDefault(p => p.Name == printerName) ?? printers.FirstOrDefault(p => p.IsDefault) ?? printers.First();
            SelectPrinter(selectedPrinter);
        }


        public void Print(string filePath)
        {
            log.InfoFormat("Print {0}", filePath);


            if (_printer == null)
            {
                log.Error("no printer selected");
            }

            _imgToPrint = filePath;

            PrintDocument pd = new PrintDocument();

            pd.PrinterSettings.PrinterName = _printer?.Name;



            pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            pd.OriginAtMargins = false;
            pd.DefaultPageSettings.Landscape = true;

            pd.BeginPrint += BeginPrint;
            pd.EndPrint += EndPrint;
            pd.PrintPage += PrintPage;

            //printPreviewDialog1.Document = pd;
            //printPreviewDialog1.ShowDialog();

            try
            {
                pd.Print();
            } catch (InvalidPrinterException ex)
            {
                // TODO log
            }
        }
  

        // The PrintPage event is raised for each page to be printed.
        private void PrintPage(object sender, PrintPageEventArgs ev)
        {
            log.Debug("PrintPage");
            using (var img = System.Drawing.Image.FromFile(_imgToPrint))
            {
                var loc = new System.Drawing.Point(0, 0);
                ev.Graphics.DrawImage(img, loc);
            }       
        }

        private void BeginPrint(object sender, PrintEventArgs e)
        {
            log.Debug("BeginPrint");
        }


        private void EndPrint(object sender, PrintEventArgs e)
        {
            log.Debug("EndPrint");
        }


        // https://stackoverflow.com/questions/2354435/how-to-get-the-list-of-all-printers-in-computer
        public List<PrinterDefinition> ListPrinters()
        {
            List<PrinterDefinition> printers = new List<PrinterDefinition>();

            // get list of printers
            var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");
            foreach (var printer in printerQuery.Get())
            {
                printers.Add(new PrinterDefinition() {
                    Name = (string) printer.GetPropertyValue("Name"),
                    Status = (string) printer.GetPropertyValue("Status"),
                    IsDefault = (Boolean) printer.GetPropertyValue("Default"),
                    IsNetworkPrinter = (Boolean) printer.GetPropertyValue("Network")
                }); 
            }

            // log
            if (log.IsInfoEnabled)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("List of printers:");
                printers.ForEach(p => sb.AppendFormat("\n\t- {0}: Status: {1}, Default: {2}, Network: {3}",
                    p.Name, p.Status, p.IsDefault, p.IsNetworkPrinter));

                log.Info(sb.ToString());
            }


            return printers;
        }

        public void SelectPrinter(PrinterDefinition printer)
        {
            log.InfoFormat("use printer: {0}", printer.Name);
            _printer = printer;

            if (printer == null)
            {
                log.Error("no printer selected");
            }

            // list capabilities
            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = printer.Name;

            // paper sizes
            StringBuilder sb = new StringBuilder();
            sb.Append("list of paper sizes:");
            foreach(var ps in pd.PrinterSettings.PaperSizes.Cast<PaperSize>())
            {
                sb.AppendFormat("\n\t- paper: name:{1}, kind:{0}, width:{2}, height{3}", ps.Kind, ps.PaperName, ps.Width, ps.Height);
            }
            log.Info(sb.ToString());

            // printer resolution
            sb = new StringBuilder();
            sb.Append("list of resolutions:");
            foreach (var ps in pd.PrinterSettings.PrinterResolutions.Cast<PrinterResolution>())
            {
                sb.AppendFormat("\n\t- resolution: kind:{0}, X:{1}, Y:{2}", ps.Kind, ps.X, ps.Y);
            }
            log.Info(sb.ToString());

            // paper sources
            sb = new StringBuilder();
            sb.Append("list of paper sources:");
            foreach (var ps in pd.PrinterSettings.PaperSources.Cast<PaperSource>())
            {
                sb.AppendFormat("\n\t- resolution: name:{1}, kind:{0}", ps.Kind, ps.SourceName);
            }
            log.Info(sb.ToString());

        }

    }
}
