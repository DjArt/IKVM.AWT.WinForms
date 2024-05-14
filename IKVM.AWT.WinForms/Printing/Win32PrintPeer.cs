/*
  Copyright (C) 2009 Volker Berlin (i-net software)
  Copyright (C) 2010 Karsten Heinrich (i-net software)
  Copyright (C) 2023-2024 Dj Art.

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net 

*/

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IKVM.AWT.WinForms.Printing
{
    /// <summary>
    /// Implementation of the PrintPeer for Windows
    /// </summary>
    class Win32PrintPeer : BasePrintPeer
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct PRINTER_INFO_2
        {
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pServerName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pPrinterName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pShareName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pPortName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDriverName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pComment;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pLocation;
            public nint pDevMode;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pSepFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pPrintProcessor;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pDatatype;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pParameters;
            public nint pSecurityDescriptor;
            public uint Attributes;
            public uint Priority;
            public uint DefaultPriority;
            public uint StartTime;
            public uint UntilTime;
            public int Status;
            public int cJobs;
            public uint AveragePPM;
        }

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetPrinter(SafePrinterHandle hPrinter, int dwLevel, ref PRINTER_INFO_2 info, int cbBuf, out int pcbNeeded);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool OpenPrinter(string pPrinterName, out SafePrinterHandle hPrinter, nint pDefault);

        private const int ERROR_INSUFFICIENT_BUFFER = 122;

        private const int PRINTER_STATUS_PAUSED = 0x1;
        private const int PRINTER_STATUS_ERROR = 0x2; // MSDN says that this value is not used
        private const int PRINTER_STATUS_PENDING_DELETION = 0x4;
        private const int PRINTER_STATUS_PAPER_JAM = 0x8;
        private const int PRINTER_STATUS_PAPER_OUT = 0x10;
        private const int PRINTER_STATUS_MANUAL_FEED = 0x20;
        private const int PRINTER_STATUS_PAPER_PROBLEM = 0x40;
        private const int PRINTER_STATUS_OFFLINE = 0x80;
        private const int PRINTER_STATUS_IO_ACTIVE = 0x100;
        private const int PRINTER_STATUS_BUSY = 0x200;
        private const int PRINTER_STATUS_PRINTING = 0x400;
        private const int PRINTER_STATUS_OUTPUT_BIN_FULL = 0x800;
        private const int PRINTER_STATUS_NOT_AVAILABLE = 0x1000;
        private const int PRINTER_STATUS_WAITING = 0x2000;
        private const int PRINTER_STATUS_PROCESSING = 0x4000;
        private const int PRINTER_STATUS_INITIALIZING = 0x8000;
        private const int PRINTER_STATUS_WARMING_UP = 0x10000;
        private const int PRINTER_STATUS_TONER_LOW = 0x20000;
        private const int PRINTER_STATUS_NO_TONER = 0x40000;
        private const int PRINTER_STATUS_PAGE_PUNT = 0x80000;
        private const int PRINTER_STATUS_USER_INTERVENTION = 0x100000;
        private const int PRINTER_STATUS_OUT_OF_MEMORY = 0x200000;
        private const int PRINTER_STATUS_DOOR_OPEN = 0x400000;
        private const int PRINTER_STATUS_SERVER_UNKNOWN = 0x800000;
        private const int PRINTER_STATUS_POWER_SAVE = 0x1000000;

        /// <summary>
        /// Get a printer status
        /// </summary>
        /// <param name="printerName">a valid printer name</param>
        /// <param name="category">a category that should request</param>
        /// <returns>a printer attribute or null</returns>
        public override object getPrinterStatus(string printerName, java.lang.Class category)
        {
            if (GetPrinterInfo2(printerName, out int cJobs, out int status))
            {
                if (category == (java.lang.Class)typeof(javax.print.attribute.standard.PrinterState))
                {
                    if ((status &
                            (PRINTER_STATUS_ERROR | PRINTER_STATUS_NO_TONER | PRINTER_STATUS_OUT_OF_MEMORY |
                             PRINTER_STATUS_OFFLINE | PRINTER_STATUS_USER_INTERVENTION |
                             PRINTER_STATUS_DOOR_OPEN | PRINTER_STATUS_NOT_AVAILABLE)) > 0)
                    {
                        return javax.print.attribute.standard.PrinterState.STOPPED;
                    }
                    return (status & (PRINTER_STATUS_BUSY | PRINTER_STATUS_PRINTING)) > 0 ? javax.print.attribute.standard.PrinterState.PROCESSING : (object)null;
                    // null seems to be the default instead of unknown - UNKOWN ist not used in the reference RT
                    // javax.print.attribute.standard.PrinterState.UNKNOWN;
                }
                if (category == (java.lang.Class)typeof(javax.print.attribute.standard.PrinterStateReasons))
                {
                    return extractResions(status);
                }
                if (category == (java.lang.Class)typeof(javax.print.attribute.standard.QueuedJobCount))
                {
                    return new javax.print.attribute.standard.QueuedJobCount(cJobs);
                }
                if (category == (java.lang.Class)typeof(javax.print.attribute.standard.PrinterIsAcceptingJobs))
                {
                    return (status &
                            (PRINTER_STATUS_ERROR |
                             PRINTER_STATUS_NO_TONER |
                             PRINTER_STATUS_OUT_OF_MEMORY |
                             PRINTER_STATUS_OFFLINE |
                             PRINTER_STATUS_USER_INTERVENTION |
                             PRINTER_STATUS_DOOR_OPEN)) > 0
                        ? javax.print.attribute.standard.PrinterIsAcceptingJobs.NOT_ACCEPTING_JOBS
                        : (object)javax.print.attribute.standard.PrinterIsAcceptingJobs.ACCEPTING_JOBS;
                }
            }
            return null;
        }

        private javax.print.attribute.standard.PrinterStateReasons extractResions(int status)
        {
            javax.print.attribute.standard.PrinterStateReasons reasons = new();
            if ((status & PRINTER_STATUS_PAUSED) > 0)
            {
                reasons.put(javax.print.attribute.standard.PrinterStateReason.PAUSED, javax.print.attribute.standard.Severity.REPORT);
            }
            if ((status & PRINTER_STATUS_ERROR) > 0)
            {
                reasons.put(javax.print.attribute.standard.PrinterStateReason.OTHER, javax.print.attribute.standard.Severity.ERROR);
            }
            if ((status & PRINTER_STATUS_PENDING_DELETION) > 0) { }
            if ((status & PRINTER_STATUS_PAPER_JAM) > 0)
            {
                reasons.put(javax.print.attribute.standard.PrinterStateReason.MEDIA_JAM, javax.print.attribute.standard.Severity.WARNING);
            }
            if ((status & PRINTER_STATUS_PAPER_OUT) > 0)
            {
                reasons.put(javax.print.attribute.standard.PrinterStateReason.MEDIA_EMPTY, javax.print.attribute.standard.Severity.WARNING);
            }
            if ((status & PRINTER_STATUS_MANUAL_FEED) > 0) { }
            if ((status & PRINTER_STATUS_PAPER_PROBLEM) > 0) { }
            if ((status & PRINTER_STATUS_OFFLINE) > 0)
            {
                reasons.put(javax.print.attribute.standard.PrinterStateReason.TIMED_OUT, javax.print.attribute.standard.Severity.ERROR);
            }
            if ((status & PRINTER_STATUS_IO_ACTIVE) > 0) { }
            if ((status & PRINTER_STATUS_BUSY) > 0) { }
            if ((status & PRINTER_STATUS_PRINTING) > 0) { }
            if ((status & PRINTER_STATUS_OUTPUT_BIN_FULL) > 0)
            {
                reasons.put(javax.print.attribute.standard.PrinterStateReason.OUTPUT_AREA_FULL, javax.print.attribute.standard.Severity.WARNING);
            }
            if ((status & PRINTER_STATUS_NOT_AVAILABLE) > 0) { }
            if ((status & PRINTER_STATUS_WAITING) > 0) { }
            if ((status & PRINTER_STATUS_PROCESSING) > 0) { }
            if ((status & PRINTER_STATUS_INITIALIZING) > 0) { }
            if ((status & PRINTER_STATUS_WARMING_UP) > 0) { }
            if ((status & PRINTER_STATUS_TONER_LOW) > 0)
            {
                reasons.put(javax.print.attribute.standard.PrinterStateReason.TONER_LOW, javax.print.attribute.standard.Severity.WARNING);
            }
            if ((status & PRINTER_STATUS_NO_TONER) > 0)
            {
                reasons.put(javax.print.attribute.standard.PrinterStateReason.TONER_EMPTY, javax.print.attribute.standard.Severity.ERROR);
            }
            if ((status & PRINTER_STATUS_PAGE_PUNT) > 0) { }
            if ((status & PRINTER_STATUS_USER_INTERVENTION) > 0)
            {
                reasons.put(javax.print.attribute.standard.PrinterStateReason.OTHER, javax.print.attribute.standard.Severity.ERROR);
            }
            if ((status & PRINTER_STATUS_OUT_OF_MEMORY) > 0)
            {
                reasons.put(javax.print.attribute.standard.PrinterStateReason.OTHER, javax.print.attribute.standard.Severity.ERROR);
            }
            if ((status & PRINTER_STATUS_DOOR_OPEN) > 0)
            {
                reasons.put(javax.print.attribute.standard.PrinterStateReason.DOOR_OPEN, javax.print.attribute.standard.Severity.ERROR);
            }
            if ((status & PRINTER_STATUS_SERVER_UNKNOWN) > 0) { }
            if ((status & PRINTER_STATUS_POWER_SAVE) > 0)
            {
                reasons.put(javax.print.attribute.standard.PrinterStateReason.PAUSED, javax.print.attribute.standard.Severity.REPORT);
            }
            return reasons.isEmpty() ? null : reasons;
        }

        /// <summary>
        /// Get infos from the PRINTER_INFO_2 struc
        /// </summary>
        /// <param name="printerName">The name of a valid printer</param>
        /// <param name="cJobs">returns the current count of print jobs</param>
        /// <param name="status">returns the current status of the printer</param>
        /// <returns>true if the return is valid</returns>
        [System.Security.SecuritySafeCritical]
        private static bool GetPrinterInfo2(string printerName, out int cJobs, out int status)
        {
            SafePrinterHandle printer = null;
            try
            {
                if (OpenPrinter(printerName, out printer, nint.Zero)
                    && !GetPrinter(printer, 2, ref Unsafe.NullRef<PRINTER_INFO_2>(), 0, out int needed))
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    if (lastWin32Error == ERROR_INSUFFICIENT_BUFFER)
                    {
                        PRINTER_INFO_2 info = new();
                        if (GetPrinter(printer, 2, ref info, needed, out needed))
                        {
                            cJobs = info.cJobs;
                            status = info.Status;
                            return true;
                        }
                    }
                }
            }
            finally
            {
                printer?.Close();
            }
            cJobs = 0;
            status = 0;
            return false;
        }

    }
}
