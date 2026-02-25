using System.Drawing.Printing;
using System.Collections.Generic;
using System.Linq;

namespace EposBridge.Services;

public class PrinterService
{
    public List<string> GetInstalledPrinters()
    {
        var list = new List<string>();
        foreach (string printer in PrinterSettings.InstalledPrinters)
        {
            list.Add(printer);
        }
        return list;
    }

    public bool PrintRaw(string printerName, byte[] data)
    {
        return RawPrinterHelper.SendBytesToPrinter(printerName, data);
    }
}
