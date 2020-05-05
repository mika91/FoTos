using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoTos.Services.Printer
{
    // TODO: ensure async
    public interface IPrinterService
    {
        void Print(String path);
        List<PrinterDefinition> ListPrinters();

        void SelectPrinter(PrinterDefinition printer);
    }

    public class PrinterDefinition
    {
        public String Name { get; set; }
        public String Status { get; set; }
        public Boolean IsDefault { get; set; }
        public Boolean IsNetworkPrinter { get; set; }
    }
}
