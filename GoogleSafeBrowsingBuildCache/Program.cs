using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SafebrowseV2;

namespace ReputationBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            string apiKey = ConfigurationManager.AppSettings["googleSafeBrowsingApiKey"];
            string cacheDir = ConfigurationManager.AppSettings["googleSafeBrowsingCache"];

            ReputationEngine rep = new ReputationEngine();
            rep.Initialize(apiKey, cacheDir);
            ReputationEngine.SafeBrowsing.UpdateList(rep.SafeBrowser.MasterLists);
        }
    }
}
