using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using MyEntity = LinkChecker2.App_Code.BusObj.ResourceLink;
using Known404Page = LinkChecker2.App_Code.BusObj.Known404Page;
using KnownBadTitle = LinkChecker2.App_Code.BusObj.KnownBadTitle;
using KnownBadContent = LinkChecker2.App_Code.BusObj.KnownBadContent;
using ResourceRedirect = LinkChecker2.App_Code.BusObj.ResourceRedirect;
using MyManager = LinkChecker2.App_Code.DAL.ResourceLinkManager;
using RedirectManager = LinkChecker2.App_Code.DAL.ResourceRedirectManager;
using SearchManager = Isle.BizServices.ResourceV2Services;
using ESManager = LRWarehouse.DAL.ElasticSearchManager;

//using SearchManager = LinkChecker2.App_Code.DAL.ElasticSearchManager;
using LinkChecker2.App_Code.Utilities;

namespace LinkChecker2
{
    class Program
    {
        static MyManager myManager = new MyManager();
        static SearchManager searchManager = new SearchManager();
        static RedirectManager redirectManager = new RedirectManager();
        static ESManager esManager = new ESManager();
        static List<MyEntity> links;

        static int i = 0;
        static int checkedRecords = 0;
        static int nbrRedirects = 0;
        static bool firstWrite = true;
        static string resourceList = "";
        static string redirectUrl = "";
        static string originalUrl = "";

        static StreamWriter logFile;
        public static List<Known404Page> known404Pages;
        public static List<KnownBadTitle> knownBadTitles;
        public static List<KnownBadContent> knownBadContent;
        public static int linksCount = 0;
        
        static void Main(string[] args)
        {
            known404Pages = myManager.GetKnown404Pages();
            knownBadTitles = myManager.GetKnownBadTitles();
            knownBadContent = myManager.GetKnownBadContentRules();
            string phase = "";
            if (args != null && args.Length > 0)
            {
                phase = args[0];
            }
            switch (phase)
            {
                case "1":
                    Phase1Processing();
                    break;
                case "2":
                    Phase2Processing();
                    break;
                case "4":
                    SyncESWithDatabase();
                    break;
                default:
                    int choice = 0;
                    string[] choices = { "1", "2", "3", "4", "5", "6", "999" };
                    while (choice != 999)
                    {
                        Console.Write("1) Do Phase 1 Link Checking\n2) Do Phase 2 Link Checking\n3) Phase 1 Link Checking for Id range\n" + 
                            "4) Delete resources from ES that are deleted in database\n5) Delete resourceIntId from ES that is deleted in database\n" +
                            "6) Do Phase 1 Link Checking for a specific host name\n999) Quit\nSelect One ===> ");
                        string strChoice = Console.ReadLine();
                        if (choices.Contains(strChoice))
                        {
                            choice = int.Parse(strChoice);
                            switch (choice)
                            {
                                case 1:
                                    Phase1Processing();
                                    choice = 0;
                                    break;
                                case 2:
                                    Phase2Processing();
                                    choice = 0;
                                    break;
                                case 3:
                                    Phase1ProcessingForRange();
                                    break;
                                case 4:
                                    SyncESWithDatabase();
                                    break;
                                case 5:
                                    DeleteFromES();
                                    break;
                                case 6:
                                    Phase1ProcessingForHost();
                                    choice = 0;
                                    break;
                                case 999:
                                    break;
                                default:
                                    Console.WriteLine("Option not implemented.");
                                    choice = 0;
                                    break;
                            }
                        }
                    }
                    break;
            }
        }// Main

        static void Phase1Processing()
        {
            Stopwatch swatch = new Stopwatch();
            swatch.Start();
            i = checkedRecords = 0;

            string fileName = string.Format(MyManager.GetAppKeyValue("logFileTemplate", @"C:\IOER_Tools\LinkChecker\Logs\LinkCheckerLog_{0}.csv"), DateTime.Now.ToString("s").Replace(":",""));
            logFile = new StreamWriter(fileName);
            firstWrite = true;
            
            //myManager.LinkAddUpdate();
            links = myManager.GetLeastRecentlyChecked();
            if (links == null)
            {
                Console.WriteLine("Error retrieving links to check.  No processing done.");
                MyManager.LogError("Error retrieving links to check in Phase1Processing().  No processing done.");
                return;
            }
            List<string> badHostList = new List<string>(links.Count);

            // Start multithreaded process
            int nbrThreads = MyManager.GetAppKeyValue("nbrThreads", 4);
            ThreadedLinkCheck[] linkCheckers = new ThreadedLinkCheck[nbrThreads];
            for (int j = 0; j < nbrThreads; j++)
            {
                linkCheckers[j] = new ThreadedLinkCheck();
            }
            int nbrRecsSplit = 0;
            foreach (MyEntity link in links)
            {
                linkCheckers[nbrRecsSplit % nbrThreads].Links.Add(link);
                nbrRecsSplit++;
            }
            for (int threadNbr = 0; threadNbr < nbrThreads; threadNbr++)
            {
                Thread thread = new Thread(new ThreadStart(linkCheckers[threadNbr].CheckLinks));
                thread.Start();
            }

            // Check to see if threads are complete
            bool allDone = false;
            while (!allDone)
            {
                allDone = true;
                foreach (ThreadedLinkCheck linkChecker in linkCheckers)
                {
                    if (!linkChecker.ThreadComplete)
                    {
                        allDone = false;
                        break;
                    }
                }
                if (!allDone)
                {
                    Thread.Sleep(60000); // Sleep 1 minute then check again
                }
            }


            // Check for redirects
            CheckRedirects();
            swatch.Stop();
            Console.WriteLine("Elapsed time: " + swatch.Elapsed.ToString("c"));
            logFile.Close();

            // Send email
            string fromEmail = MyManager.GetAppKeyValue("fromEmail", @"LR_LinkChecker2@ilsharedlearning.org");
            string[] toEmail = MyManager.GetAppKeyValue("toEmail", @"jgrimmer@siuccwd.com, mparsons@siuccwd.com, hwilliams@siuccwd.com, nathan.argo@siuccwd.com").Split(',');
            string subject = "IOER Link Checker Status, Phase 1";
            string emailBody = "Here is the status from Phase 1 of the IOER Link Checker";
            string[] attachments = new string[1];
            attachments[0] = fileName;
            EmailHelper.SendEmail(fromEmail, toEmail, null, null, subject, emailBody, attachments);
        }// Phase1Processing


        static void Phase1ProcessingForRange()
        {
            Console.Write("Start of ID range: ");
            string strStart = Console.ReadLine();
            Console.Write("End of ID range: ");
            string strEnd = Console.ReadLine();
            int iStartLine = 0;
            int iEndLine = 0;
            int.TryParse(strStart, out iStartLine);
            int.TryParse(strEnd, out iEndLine);
            Stopwatch swatch = new Stopwatch();
            swatch.Start();
            i = checkedRecords = 0;

            string fileName = string.Format(MyManager.GetAppKeyValue("logFileTemplate", @"C:\IOER_Tools\LinkChecker\Logs\LinkCheckerLog_{0}.csv"), DateTime.Now.ToString("s").Replace(":", ""));
            logFile = new StreamWriter(fileName);
            firstWrite = true;

            List<MyEntity> dbLinks = myManager.GetLeastRecentlyChecked(10000000);

            if (dbLinks == null)
            {
                Console.WriteLine("Error retrieving links to check.  No processing done.");
                MyManager.LogError("Error retrieving links to check in Phase1Processing().  No processing done.");
                return;
            }
            links = dbLinks.Where(l => l.ResourceIntId >= iStartLine && l.ResourceIntId <= iEndLine).ToList();
            linksCount = links.Count;

            ThreadedLinkCheck linkChecker = new ThreadedLinkCheck();
            List<string> badHostList = new List<string>(links.Count);

            foreach (MyEntity link in links)
            {
                i++;
                string status = "successful";
                if (badHostList.Contains(link.HostName))
                {
                    link.IsDeleted = true;
                    link.LastCheckDate = DateTime.Now;
                    myManager.Update(link);
                    continue;
                }

                nbrRedirects = 0;
                originalUrl = link.ResourceUrl;
                linkChecker.CheckLink(link);
            }//foreach

            // Check for redirected URL duplicates
            CheckRedirects();

            swatch.Stop();
            Console.WriteLine("Elapsed time: " + swatch.Elapsed.ToString("c"));
            logFile.Close();
            // Send email
            string fromEmail = MyManager.GetAppKeyValue("fromEmail", @"LR_LinkChecker2@ilsharedlearning.org");
            string[] toEmail = MyManager.GetAppKeyValue("toEmail", @"jgrimmer@siuccwd.com, mparsons@siuccwd.com, hwilliams@siuccwd.com, nathan.argo@siuccwd.com").Split(',');
            string subject = "IOER Link Checker Status, Phase 1 (Range)";
            string emailBody = string.Format("Here is the status from Phase 1 of the IOER Link Checker for ResourceIntId range of {0}-{1}",iStartLine,iEndLine);
            string[] attachments = new string[1];
            attachments[0] = fileName;
            EmailHelper.SendEmail(fromEmail, toEmail, null, null, subject, emailBody, attachments);
        }// Phase1Processing

        static void Phase1ProcessingForHost()
        {
            Console.Write("Host name to check (FQDN): ");
            string hostName = Console.ReadLine().ToLower();
            Stopwatch swatch = new Stopwatch();
            swatch.Start();
            i = checkedRecords = 0;

            string fileName = string.Format(MyManager.GetAppKeyValue("logFileTemplate", @"C:\IOER_Tools\LinkChecker\Logs\LinkCheckerLog_{0}.csv"), DateTime.Now.ToString("s").Replace(":", ""));
            logFile = new StreamWriter(fileName);
            firstWrite = true;

            List<MyEntity> dbLinks = myManager.GetLeastRecentlyChecked(10000000);

            if (dbLinks == null)
            {
                Console.WriteLine("Error retrieving links to check.  No processing done.");
                MyManager.LogError("Error retrieving links to check in Phase1Processing().  No processing done.");
                return;
            }
            links = dbLinks.Where(l => l.HostName.ToLower() == hostName).ToList();
            linksCount = links.Count;

            List<string> badHostList = new List<string>(links.Count);
            ThreadedLinkCheck linkChecker = new ThreadedLinkCheck();

            foreach (MyEntity link in links)
            {
                i++;
                string status = "successful";
                if (badHostList.Contains(link.HostName))
                {
                    link.IsDeleted = true;
                    link.LastCheckDate = DateTime.Now;
                    myManager.Update(link);
                    continue;
                }

                nbrRedirects = 0;
                originalUrl = link.ResourceUrl;
                linkChecker.CheckLink(link);
            }//foreach

            // Check for redirected URL duplicates
            CheckRedirects();

            swatch.Stop();
            Console.WriteLine("Elapsed time: " + swatch.Elapsed.ToString("c"));
            logFile.Close();
            // Send email
            string fromEmail = MyManager.GetAppKeyValue("fromEmail", @"LR_LinkChecker2@ilsharedlearning.org");
            string[] toEmail = MyManager.GetAppKeyValue("toEmail", @"jgrimmer@siuccwd.com, mparsons@siuccwd.com, hwilliams@siuccwd.com, nathan.argo@siuccwd.com").Split(',');
            string subject = "IOER Link Checker Status, Phase 1 (Host)";
            string emailBody = string.Format("Here is the status from Phase 1 of the IOER Link Checker for host name {0}", hostName);
            string[] attachments = new string[1];
            attachments[0] = fileName;
            EmailHelper.SendEmail(fromEmail, toEmail, null, null, subject, emailBody, attachments);

        }
        
        static void Phase2Processing()
        {
            Stopwatch swatch = new Stopwatch();
            swatch.Start();
            i = checkedRecords = 0;

            string fileName = string.Format(MyManager.GetAppKeyValue("logFileTemplate", @"C:\IOER_Tools\LinkChecker\Logs\LinkCheckerLog_{0}.csv"), DateTime.Now.ToString("s").Replace(":",""));
            logFile = new StreamWriter(fileName);
            firstWrite = true;

            links = myManager.GetItemsForPhase2();
            linksCount = links.Count;
            ThreadedLinkCheck linkChecker = new ThreadedLinkCheck();
            if (links == null)
            {
                Console.WriteLine("Error retrieving links to check.  No processing done.");
                MyManager.LogError("Error retrieving links to check in Phase1Processing().  No processing done."); 
                return;
            }

            foreach (MyEntity link in links)
            {
                i++;
                nbrRedirects = 0;
                linkChecker.CheckLink(link);
            }
            swatch.Stop();
            Console.WriteLine("Elapsed time: " + swatch.Elapsed.ToString("c"));
            logFile.Close();

            // Send email
            string fromEmail = MyManager.GetAppKeyValue("fromEmail", @"LR_LinkChecker2@ilsharedlearning.org");
            string[] toEmail = MyManager.GetAppKeyValue("toEmail", @"jgrimmer@siuccwd.com, mparsons@siuccwd.com, hwilliams@siuccwd.com, nathan.argo@siuccwd.com").Split(',');
            string subject = "IOER Link Checker Status, Phase 2";
            string emailBody = "Here is the status from Phase 2 of the IOER Link Checker";
            string[] attachments = new string[1];
            attachments[0] = fileName;
            EmailHelper.SendEmail(fromEmail, toEmail, null, null, subject, emailBody, attachments);
        }// Phase2Processing

        static string ExtractTitleTag(string pageContent)
        {
            // Avoid malformed XHTML by extracting only the title tag
            Regex titleEx = new Regex(@"<title([\s\S]*?)>([\s\S])*?</title>");
            Match titleMatch = titleEx.Match(pageContent);
            if (titleMatch != null)
            {
                string title = titleEx.Match(pageContent).Value;
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml("<root>\n" + title + "\n</root>");
                XmlNodeList nodes = xdoc.GetElementsByTagName("title");
                if (nodes != null && nodes.Count > 0)
                {
                    title = TrimWhitespace(nodes[0].InnerXml);
                    return title;
                }
            }
            return null;
        }

        /// <summary>
        /// Regular "Trim()" does not handle \r and \n.  This handles it.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static string TrimWhitespace(string str)
        {
            Regex leadingWhitespace = new Regex(@"^\s+");
            Regex trailingWhitespace = new Regex(@"\s+$");

            str = leadingWhitespace.Replace(str, "");
            str = trailingWhitespace.Replace(str, "");

            return str;
        }


        static void CheckRedirects()
        {
            string status = "successful";
            List<ResourceRedirect> redirects = redirectManager.Select("", ref status);
            foreach (ResourceRedirect redirect in redirects)
            {
                MyEntity newLink = myManager.GetByUrl(redirect.NewUrl);
                MyEntity oldLink = myManager.Get(redirect.ResourceIntId);
                if (newLink != null)
                {
                    DataSet oldLibraries = redirectManager.GetResourceCollections(oldLink.ResourceIntId, ref status);
                    DataSet newLibraries = redirectManager.GetResourceCollections(newLink.ResourceIntId, ref status);

                    if (MyManager.DoesDataSetHaveRows(oldLibraries))
                    {
                        foreach (DataRow oldRow in oldLibraries.Tables[0].Rows)
                        {
                            if (MyManager.DoesDataSetHaveRows(newLibraries))
                            {
                                DataView dv = newLibraries.Tables[0].DefaultView;
                                int collectionId = MyManager.GetRowColumn(oldRow, "LibrarySectionId", 0);
                                dv.RowFilter = string.Format("LibrarySectionId = {0}", collectionId);
                                if (dv.ToTable().Rows.Count == 0)
                                {
                                    //New resource is not in collection, add it
                                    DateTime created = MyManager.GetRowColumn(oldRow, "Created", DateTime.Now);
                                    int createdById = MyManager.GetRowColumn(oldRow, "CreatedById", 0);
                                    redirectManager.AddResourceCollection(newLink.ResourceIntId, collectionId, created, createdById);
                                }
                            }
                            else
                            {
                                // Handle instance where new resource is not in any libraries but old one is
                                int collectionId = MyManager.GetRowColumn(oldRow, "LibrarySectionId", 0);
                                DateTime created = MyManager.GetRowColumn(oldRow, "Created", DateTime.Now);
                                int createdById = MyManager.GetRowColumn(oldRow, "CreatedById", 0);
                                redirectManager.AddResourceCollection(newLink.ResourceIntId, collectionId, created, createdById);
                            }
                        }
                    }

                    if (resourceList.IndexOf(newLink.ResourceIntId.ToString() + ",") == -1)
                    {
                        resourceList += string.Format("{0},", newLink.ResourceIntId);
                    }
                    //searchManager.DeleteByIntID(redirect.ResourceIntId, ref status);
                    searchManager.DeleteResource(redirect.ResourceIntId);
                    oldLink.IsDeleted = true;
                    myManager.Update(oldLink);
                    WriteLogFile(oldLink, "Redirects to new version of resource already in our database", "Resource deleted from search.");
                }
                redirectManager.Delete(redirect);
            }

            // Add to index
            //DataSet ds = searchManager.GetSqlDataForElasticSearch(resourceList, ref status);
            //searchManager.BulkUpload(ds);
            if (resourceList != null && resourceList != string.Empty)
            {
                List<string> strResources = resourceList.Split(',').ToList();
                List<int> resources = new List<int>();
                foreach (string rid in strResources)
                {
                    if (rid != "")
                    {
                        resources.Add(int.Parse(rid));
                    }
                }
                searchManager.ImportRefreshResources(resources);
            }
        }

        static void SyncESWithDatabase()
        {
            string status = "successful";
            List<int> resourcesToDelete = myManager.GetDeletedResourceIntIds(ref status);
            int nbrDeletes = 0;
            int totalDeletes = resourcesToDelete.Count();
            if (status == "successful")
            {
                DoDeletes(resourcesToDelete);
            }
        }

        static void DeleteFromES()
        {
            Console.Write("ID to delete: ");
            string strId = Console.ReadLine();
            string status = "";
            int idToDelete = 0;
            bool isInt = int.TryParse(strId, out idToDelete);
            List<int> resourcesToDelete = myManager.GetDeletedResourceIntIds(ref status);
            resourcesToDelete = resourcesToDelete.Where(x => x == idToDelete).ToList();
            DoDeletes(resourcesToDelete);
        }

        static void DoDeletes(List<int> resourcesToDelete)
        {
            int nbrDeletes = 0;
            int totalDeletes = resourcesToDelete.Count();
            int nbrDeletionsThisRequest = 0;
            int maxDeletesPerRequest = 100;
            List<int> idsToDelete = new List<int>();
            foreach (int resourceToDelete in resourcesToDelete)
            {
                nbrDeletes++;
                Console.WriteLine("ResourceIntId " + nbrDeletes.ToString() + "/" + totalDeletes.ToString() + ": " + resourceToDelete.ToString());
                idsToDelete.Add(resourceToDelete);
                nbrDeletionsThisRequest++;
                if (nbrDeletionsThisRequest >= maxDeletesPerRequest)
                {
                    esManager.DeleteResources(idsToDelete);
                    System.Threading.Thread.Sleep(20000);
                    nbrDeletionsThisRequest = 0;
                    idsToDelete = new List<int>();
                }
            }
            if (idsToDelete.Count() > 0)
            {
                esManager.DeleteResources(idsToDelete);
                nbrDeletionsThisRequest = 0;
            }
        }


        public static void WriteLogFile(MyEntity link, string message, string action)
        {
            string quote = "\"";
            string delim = "\",\"";
            if (firstWrite)
            {
                logFile.WriteLine(quote + "ResourceIntId" + delim + "URL" + delim + "Message" + quote);
                firstWrite = false;
            }
            logFile.WriteLine(quote + link.ResourceIntId + delim + link.ResourceUrl + delim + message + delim + action + quote);
        }

        public static bool ThresholdReached(int count, string type)
        {
            int max = 0;
            switch (type.ToLower())
            {
                case "timeout":
                    max = MyManager.GetAppKeyValue("TimeoutThreshold", 10);
                    break;
                case "dns":
                    max = MyManager.GetAppKeyValue("DnsThreshold", 10);
                    break;
                case "internal":
                    max = MyManager.GetAppKeyValue("InternalThreshold", 10);
                    break;
                case "can't connect":
                    max = MyManager.GetAppKeyValue("CantConnectThreshold", 10);
                    break;
                default:
                    max = 100;
                    break;
            }

            return (count > max);
        }
    }
}
