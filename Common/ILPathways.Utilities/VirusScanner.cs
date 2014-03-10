using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using nClam;

namespace ILPathways.Utilities
{
    public class VirusScanner
    {
        ClamClient clam;
        bool shouldScan;
        /// <summary>
        /// Construct an instance of VirusScanner with default maximum stream size in nClam.  Currently default nClam maximum stream size is 25MB.
        /// </summary>
        public VirusScanner()
        {
            shouldScan = bool.Parse(UtilityManager.GetAppKeyValue("ScanForViruses", "false"));
            string hostName = UtilityManager.GetAppKeyValue("scannerHost", "localhost");
            int hostPort = UtilityManager.GetAppKeyValue("scannerPort", 3310);

            clam = new ClamClient(hostName, hostPort);
        }

        /// <summary>
        /// Construct an instance of VirusScanner with a specified maximum stream size (in bytes) in nClam.
        /// </summary>
        /// <param name="maxStreamSize"></param>
        public VirusScanner(int maxStreamSize)
        {
            shouldScan = bool.Parse(UtilityManager.GetAppKeyValue("ScanForViruses", "false"));
            string hostName = UtilityManager.GetAppKeyValue("scannerHost", "localhost");
            int hostPort = UtilityManager.GetAppKeyValue("scannerPort", 3310);

            clam = new ClamClient(hostName, hostPort);
            clam.MaxStreamSize = maxStreamSize;
        }

        public string ScanFileOnServer(string filePath)
        {
            if (shouldScan)
            {
                var scanResult = clam.ScanFileOnServer(filePath);
                return ConvertResultToString(scanResult);
            }
            else
            {
                return "no scan done";
            }
        }

        public string Scan(Stream stream)
        {
            if (shouldScan)
            {
                var scanResult = clam.SendAndScanFile(stream);
                return ConvertResultToString(scanResult);
            }
            else
            {
                return "no scan done";
            }
        }

        public string Scan(byte[] bytes)
        {
            if (shouldScan)
            {
                var scanResult = clam.SendAndScanFile(bytes);
                return ConvertResultToString(scanResult);
            }
            else
            {
                return "no scan done";
            }
        }

        public string Scan(string filePath)
        {
            if (shouldScan)
            {
                var scanResult = clam.SendAndScanFile(filePath);
                return ConvertResultToString(scanResult);
            }
            else
            {
                return "no scan done";
            }
        }

        private string ConvertResultToString(ClamScanResult result)
        {
            string status = "ERROR: Unknown status returned.";
            switch (result.Result)
            {
                case ClamScanResults.Clean:
                    status = "OK";
                    break;
                case ClamScanResults.VirusDetected:
                    status = "Infected";
                    break;
                case ClamScanResults.Error:
                    status = "ERROR: " + result.RawResult;
                    break;
            }
            return status;
        }
    }
}
