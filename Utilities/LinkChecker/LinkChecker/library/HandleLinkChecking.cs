using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using ILPathways.Utilities;
using LRWarehouse.Business;
using LRWarehouse.DAL;

using MyEntity = LinkChecker.library.ResourceLink;
using MyManager = LinkChecker.library.ResourceLinkManager;
using SearchManager = LinkChecker.library.ElasticSearchManager;
using LinkChecker.library;

namespace LinkChecker.library
{
    public class HandleLinkChecking
    {
        static MyManager myManager = new MyManager();
        static SearchManager searchManager = new SearchManager();
        static List<MyEntity> links;
        
        public static int CheckedRecords = 0;
        static int nbrRedirects = 0;
        static bool firstWrite = true;
        static StreamWriter logFile;

        static int someIdx = 0;
        public static int MaxRecords = 10000;
        public static int MaxRunSeconds= 3600;
        public static string LastUrl = "";
        public static string ProcessStatus = "";
        public static string RunType = "";

        static string LRLogErrors = "";
        static string LRNoHost = "";
        static string HTMLTraceFile = "";
        public static int badURLs = 0;
        public static int badSites = 0;

        static ArrayList badHosts = new ArrayList();
        static ArrayList goodHosts = new ArrayList();
        static string currentHost = "";
        static string detailPageUrl = "";
        static string title = "";
        static string sortTitle = "";
        static int resourceVersionIntId = 0;


        public static void CheckLinks()
        {
            string linkSource = MyManager.GetAppKeyValue( "linkSource", "oer" );
        }

        public static void CheckLinks( string linkSource )
        {
            Stopwatch sWatch = new Stopwatch();
            sWatch.Start();

            RunType = MyManager.GetAppKeyValue( "runType", "console" );

            MaxRecords = MyManager.GetAppKeyValue( "maxRowsToCheck", 1000 );
            MaxRunSeconds = MyManager.GetAppKeyValue( "maxRunSeconds", 120 );
            string sqlByUrl = "SELECT ResourceIntId As [Id], [ResourceUrl], [ResourceVersionIntId],[Title], SortTitle FROM [Resource.Version_Summary] WHERE  [ResourceUrl] >= '{0}' ORDER BY [ResourceUrl] ";

            string sql = "";
            string startingUrl = MyManager.GetAppKeyValue( "startingUrl", "" );
            
          
            string datePrefix = System.DateTime.Today.ToString( "u" ).Substring( 0, 10 );
            LRLogErrors = ConfigurationManager.AppSettings[ "path.LRLogErrors" ];
            LRLogErrors = LRLogErrors.Replace( "[date]", datePrefix );

            LRNoHost = ConfigurationManager.AppSettings[ "path.LRLogHosts" ];
            LRNoHost = LRNoHost.Replace( "[date]", datePrefix );

            HTMLTraceFile = UtilityManager.GetAppKeyValue( "path.HTMLTrace", "C:\\VOS_LOGS.txt" );
            HTMLTraceFile = HTMLTraceFile.Replace( "[date]", datePrefix );

            ResourceManager resourceManager = new ResourceManager();
            ElasticSearchManager searchManager = new ElasticSearchManager();
            string status = "successful";
            DataSet ds = new DataSet();

            if ( linkSource == "disabiltyWorks" )
            {
                LRLogErrors = LRLogErrors.Replace( "_LinkCheck_", "_DWLinkCheck_" );
                LRNoHost = LRNoHost.Replace( "_LinkCheck_", "_DWLinkCheck_" );
                HTMLTraceFile = HTMLTraceFile.Replace( "_LinkCheck_", "_DWLinkCheck_" );
            }

            if ( linkSource == "disabiltyWorks" )
            {
                sql = MyManager.GetAppKeyValue( "disabilityResourcesSql", "" );
                startingUrl = MyManager.GetAppKeyValue( "startingDisabilityUrl", "" );
                detailPageUrl = ""; //no detail
                ds = LinkChecker.library.BaseDataManager.DoDWQuery( sql );
            }
            else
            {
                sql = MyManager.GetAppKeyValue( "oerResourcesSql", sqlByUrl );
                detailPageUrl = MyManager.GetAppKeyValue( "detailPageUrl", "http://ioer.ilsharedlearning.org/IOER/{0}/{1}" );
                ds = DatabaseManager.DoQuery( sql );
            }

            Console.WriteLine( "Data Pulled: " + ds.Tables.Count + " Table with " + ds.Tables[ 0 ].Rows.Count + " Rows. Continuing..." );
            if ( MyManager.DoesDataSetHaveRows( ds ) == false )
            {
                NoData();
                return;
            }

            DataRowCollection rows = ds.Tables[ 0 ].Rows;

            int skippedRecords = 0;
            int intervalIdx = 0;

            int totalRows = ds.Tables[ 0 ].Rows.Count;


            Console.WriteLine( "Starting at " + DateTime.Now );
            WriteTrace( "Starting at " + DateTime.Now );
            WriteHTMLTrace( "<html>" + Environment.NewLine +
                "<body style=\"font-family: Calibri, sans-serif\">" + Environment.NewLine +
                "<head>" + Environment.NewLine +
                "<style type=\"text/css\">" + Environment.NewLine +
                "table tr:nth-child(odd) { background-color: #F5F5F5; }" + Environment.NewLine +
                "table tr td:nth-child(1) { width: 300px; }" + Environment.NewLine +
                "table tr.badHost { background-color: #FDD; font-weight: bold; }" + Environment.NewLine +
                "</style>" + Environment.NewLine +
                "<table style=\"width:100%\">" + Environment.NewLine +
                "<p>Starting at " + DateTime.Now + "</p>"
                , true );

            if ( MaxRecords < totalRows )
                totalRows = MaxRecords;

            for ( int idx = 0 ; idx < totalRows ; idx++ )
            {
                intervalIdx++;
                if ( intervalIdx > 1000 )
                {
                    double totalMinutes = sWatch.Elapsed.TotalMinutes;
                    double perMinute = ( idx + 1 ) / totalMinutes;
                    WriteTrace( string.Format("====== Processed {0} records in {1} minutes for rate of {2} per minute ======", idx+ 1, totalMinutes, perMinute ));
                    intervalIdx = 1;
                }
                currentHost = ParseCurrentHost( GetField( rows[ idx ], "ResourceUrl" ) );
                LastUrl = MyManager.GetRowColumn(rows[ idx ], "ResourceUrl","" );
                int currentID = MyManager.GetRowColumn( rows[ idx ], "Id", 0 );
                string title = MyManager.GetRowColumn( rows[ idx ], "title", "missing" );
                string sortTitle = MyManager.GetRowColumn( rows[ idx ], "sortTitle", "missing" );
                int resourceVersionIntId = MyManager.GetRowColumn( rows[ idx ], "resourceVersionIntId", 0 );

                if ( badHosts.Contains( currentHost ) )
                {
                    skippedRecords++;
                    DisplayTrace( idx + ", " + currentID + " skipped. (" + skippedRecords + ")");
                    badURLs++;

                    string errorString = currentID + "," + LastUrl;
                    string htmlErrorString = "<tr>"
                        + "<td>" + idx + " - Skipped</td>"
                        + "<td>" + currentID + " " + "</td>" 
                        + "<td><a target=\"_blank\" href=\"" + LastUrl + "\">" + LastUrl + "</a></td>"
                        + "<td><a target=\"_blank\" href=\"" + string.Format( detailPageUrl, resourceVersionIntId, sortTitle ) + "\">" + title + "</a></td></tr>";
                    WriteTrace( errorString );
                    WriteHTMLTrace( htmlErrorString, false );
                    if ( linkSource == "oer" )
                    {
                        Resource resource = resourceManager.Get( currentID );
                        resource.IsActive = false;
                        resourceManager.SetResourceActiveState( resource.Id, false );
                        searchManager.DeleteByIntID( resource.Id.ToString(), ref status );
                    }
                    else if ( linkSource == "disabiltyWorks" )
                    {
                        ResourceLinkManager.SetResourceActiveState( currentID, false );
                        //searchManager.DeleteByIntID( resource.Id.ToString(), ref status );
                    }
                    continue;
                }

                if ( LastUrl.ToLower().IndexOf( "http:" ) > -1 || LastUrl.ToLower().IndexOf( "https:" ) > -1 )
                {
                    DoHttpCheck( LastUrl, idx, totalRows, currentID, resourceManager );
                
                }
                else if ( LastUrl.ToLower().IndexOf( "ftp:" ) > -1 )
                {
                    DoFtpCheck( LastUrl, idx, totalRows, currentID, resourceManager );
                }
                else
                {
                    MyManager.LogError( "Unknown Protocol for URL " + LastUrl, false );
                }
            } // loop ==============================================================

            DisplayTrace( "Finished at " + DateTime.Now );
            DisplayTrace( rows.Count + " records target:" );
            DisplayTrace( CheckedRecords + " records checked. " + skippedRecords + " records skipped. " + badURLs + " faulty URLs. " + badSites + " faulty hosts." );

            WriteHTMLTrace( "</table>" + Environment.NewLine + "<p>Finished at " + DateTime.Now + "</p><p>" + totalRows + " records processed:</p><p>" + CheckedRecords + " records checked. " + skippedRecords + " records skipped. " + badURLs + " faulty URLs. " + badSites + " faulty hosts." + "</p></body>" + Environment.NewLine + "</html>", false );
 }

        public static void DoHttpCheck( string url, int idx, int totalRows, int currentID, ResourceManager resourceManager )
        {
            string status = "";
            try
            {
                var request = ( HttpWebRequest ) WebRequest.Create( LastUrl );
                request.Timeout = 15000; //15 seconds
                request.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1309.0 Safari/537.17";
                HttpWebResponse response;
                response = ( HttpWebResponse ) request.GetResponse();
                var urlStatusCode = ( int ) response.StatusCode;
                if ( urlStatusCode == 200 | response.StatusCode.ToString() == "OK" )
                {
                    string percent = ( Math.Round( decimal.Divide( idx, totalRows ), 5 ) * 100 ).ToString( "0.0000000" );
                    DisplayTrace( idx + ". " + currentID + " is Okay " + "( " + percent.Substring( 0, percent.Length - 2 ) + "% )", true, false );
                    CheckedRecords++;
                    response.Close();
                }
                else
                {
                    //CheckedRecords++;
                    response.Close();

                    throw new Exception( "#" + idx + " failed with: " + response.StatusCode );
                }
            }
            catch ( Exception ex )
            {
                string errorString = currentID + "," + LastUrl;
                string htmlErrorString = "<tr>"
                    + "<td>" + ex.Message + "</td>"
                    + "<td>" + currentID + "</td>"
                        + "<td><a target=\"_blank\" href=\"" + LastUrl + "\">" + LastUrl + "</a></td>"
                        + "<td><a target=\"_blank\" href=\"" + string.Format( detailPageUrl, resourceVersionIntId, sortTitle ) + "\">" + title + "</a></td></tr>";
                if ( ex.ToString().Contains( "The operation has timed out" ) )
                {
                    DisplayTrace( idx + ". " + LastUrl + " timed out." );
                }
                else if ( ex.ToString().Contains( "(404) Not Found" ) )
                {
                    DisplayTrace( idx + ". " + LastUrl + " was not found. (404)" );

                    Resource resource = resourceManager.Get( currentID );
                    resource.IsActive = false;
                    resourceManager.SetResourceActiveState( resource.Id, false );
                    searchManager.DeleteByIntID( resource.Id.ToString(), ref status );
                }
                else if ( ex.ToString().Contains( "(403) Forbidden" ) )
                {
                    DisplayTrace( idx + ". " + LastUrl + " is Forbidden. (403)" );

                    Resource resource = resourceManager.Get( currentID );
                    resource.IsActive = false;
                    resourceManager.SetResourceActiveState( resource.Id, false );
                    searchManager.DeleteByIntID( resource.Id.ToString(), ref status );
                }
                else
                {
                    DisplayTrace( idx + ". " + LastUrl + " has an Error: " + ex.ToString() );
                }

                WriteTrace( errorString );
                WriteHTMLTrace( htmlErrorString, false );

                checkHost( currentHost, ref badHosts, ref goodHosts, ref badSites );

                badURLs++;
                CheckedRecords++;
            }
        }

        public static void DoFtpCheck( string url, int idx, int totalRows, int currentID, ResourceManager resourceManager )
        {
            string status = "";
            try
            {
                //link.LastCheckDate = DateTime.Now;
                FtpWebRequest request = ( FtpWebRequest ) WebRequest.Create( url );
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                FtpWebResponse response = ( FtpWebResponse ) request.GetResponse();
                if ( ( int ) response.StatusCode == 150 || ( int ) response.StatusCode == 125 )
                {
                    // OK, file found
                    string percent = ( Math.Round( decimal.Divide( idx, totalRows ), 5 ) * 100 ).ToString( "0.0000000" );
                    DisplayTrace( idx + url + " is Okay " + "( " + percent.Substring( 0, percent.Length - 2 ) + "% )", true, false );
                    //link.NbrDnsErrors = 0;
                    //link.NbrInternalServerErrors = 0;
                    //link.NbrTimeouts = 0;
                    //myManager.Update( link );
                    CheckedRecords++;
                    response.Close();
                }
            }
            catch ( Exception ex )
            {
                CheckedRecords++;
                string errorString = currentID + "," + LastUrl;
                string htmlErrorString = "<tr>"
                    + "<td>" + ex.Message + "</td>"
                    + "<td>" + currentID + "</td>"
                        + "<td><a target=\"_blank\" href=\"" + LastUrl + "\">" + LastUrl + "</a></td>"
                        + "<td><a target=\"_blank\" href=\"" + string.Format( detailPageUrl, resourceVersionIntId, sortTitle ) + "\">" + title + "</a></td></tr>";
                if ( ex is WebException )
                {
                    WebException wex = ( WebException ) ex;
                    if ( ( int ) ( ( FtpWebResponse ) ( wex.Response ) ).StatusCode == 550 )
                    {
                        // Not found
                        DisplayTrace( idx + ". " + url + " was not found. (550)" );
                        //link.IsDeleted = true;
                        searchManager.DeleteByIntID( currentID, ref status );
                        resourceManager.SetResourceActiveState( currentID, false );
                        //myManager.Update( link );
                    }
                    else if ( ( int ) ( ( FtpWebResponse ) ( wex.Response ) ).StatusCode == 530 )
                    {
                        // Not logged in
                        DisplayTrace( idx + ". " + url + " not logged in. (530)" );
                        //link.IsDeleted = true;
                        searchManager.DeleteByIntID( currentID, ref status );
                        resourceManager.SetResourceActiveState( currentID, false );
                        //myManager.Update( link );
                    }
                    else if ( ( int ) ( ( FtpWebResponse ) ( wex.Response ) ).StatusCode == 450 )
                    {
                        // Internal error (temporary)
                        DisplayTrace( idx + ". " + url + " Temporary internal error. (450).  Try again later." );
                        //link.NbrInternalServerErrors++;
                        //if ( ThresholdReached( link.NbrInternalServerErrors, "InternalThreshold" ) )
                        //{
                        //    link.IsDeleted = true;
                        //    searchManager.DeleteByIntID( link.ResourceIntId, ref status );
                        //}
                        //myManager.Update( link );
                    }
                    else
                    {
                        MyManager.LogError( idx + ". " + url + " " + ex.ToString() );
                    }
                }
                else
                {
                    MyManager.LogError( idx + ". " + url + " " + ex.ToString() );
                }
            }
        }

        public static void NoData()
        {
            ProcessStatus = "Error: no records were found, ending process";

            DisplayTrace( ProcessStatus );

        }

        public static void checkHost( string targetHost, ref ArrayList badHosts, ref ArrayList goodHosts, ref int badSites )
        {
            if ( goodHosts.Contains( targetHost ) | badHosts.Contains( targetHost ) )
            {
                return;
            }
            else
            {

                try
                {
                    var request = ( HttpWebRequest ) WebRequest.Create( targetHost );
                    request.Timeout = 15000; //15 seconds
                    request.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1309.0 Safari/537.17";
                    DisplayTrace( targetHost + " is being checked...", true, false );
                    var response = ( HttpWebResponse ) request.GetResponse();
                    if ( ( int ) response.StatusCode == 200 | response.StatusCode.ToString() == "OK" )
                    {
                        DisplayTrace( targetHost + " is a valid host.", true, false );
                        goodHosts.Add( targetHost );
                        response.Close();
                        return;
                    }
                    else
                    {
                        //badSites++;
                        //badHosts.Add( targetHost );
                        response.Close();
                        throw new Exception( "" + response.StatusCode );
                    }

                }
                catch ( Exception ex )
                {
                    if ( ex.ToString().Contains( "(404) Not Found" ) )
                    {
                        DisplayTrace( "Bad host: " + targetHost + " was not found (404)." );
                        
                    }
                    else if ( ex.ToString().Contains( "The operation has timed out" ) )
                    {
                        DisplayTrace( "Bad host: " + targetHost + " timed out." );
                    }
                    else
                    {
                        DisplayTrace( targetHost + " is a bad host. Error: " + ex.ToString() );
                        
                    }

                    badSites++;
                    badHosts.Add( targetHost );
                    WriteTrace( "Bad host," + targetHost );
                    WriteHTMLTrace( "<tr class=\"badHost\">" 
                        + "<td>Bad Host: " + "</td>"
                        + "<td> " + "</td>"
                        + "<td><a target=\"_blank\" href=\"" + targetHost + "\">" + targetHost + "</a></td>"
                        + "<td> " + "</td>"
                        + "</tr>", false );
                    return;
                }
            }
        }

        public static string GetField( DataRow dr, string columnName )
        {
            return dr[ columnName ].ToString();
        }

        public static void DisplayTrace( string line )
        {
            DisplayTrace( line, true, true );
        }
        public static void DisplayTrace( string line, bool doConsole, bool doLog )
        {
            if ( doConsole && RunType == "debug")
                Console.WriteLine( line );
            if ( doLog )
                LoggingHelper.DoTrace( 1, line );
        }

        public static void WriteTrace( string line )
        {
            LoggingHelper.DoTrace( 8, line, false );

            try
            {
                File.AppendAllText( LRLogErrors, line + Environment.NewLine );
                if ( !line.ToLower().Contains( "bad host" ) ) //a log of JUST the resource URLs/GUIDs
                {
                    File.AppendAllText( LRNoHost, line + Environment.NewLine );
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "Error Writing to file: " + ex.ToString() );
            }
        }

        public static void WriteHTMLTrace( string line, bool overwrite )
        {

            try
            {
                if ( overwrite )
                {
                    File.WriteAllText( HTMLTraceFile, line );
                }
                else
                {
                    File.AppendAllText( HTMLTraceFile, line + Environment.NewLine );
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "Error Writing to file: " + ex.ToString() );
            }
        }

        public static string ParseCurrentHost( string rawURL )
        {
            try
            {
                string[] urlParts = rawURL.Split( '/' );
                return ( urlParts[ 0 ] + "//" + urlParts[ 2 ] );
            }
            catch ( Exception ex )
            {
                return ( "http://null.null/null" );
            }
        }
        public static string SessionSummary()
        {
            string summary = "";

            return summary;

        }

        //====================================================================
        #region Jerome's updates

        static void Phase1Processing()
        {
            Stopwatch swatch = new Stopwatch();
            swatch.Start();
            someIdx = CheckedRecords = 0;

            string fileName = string.Format(MyManager.GetAppKeyValue("logFileTemplate", @"C:\IOER_Tools\LinkChecker\Logs\LinkCheckerLog_{0}.csv"), DateTime.Now.ToString("s").Replace(":",""));
            logFile = new StreamWriter(fileName);
            firstWrite = true;

            //myManager.LinkAddUpdate();
            links = myManager.GetLeastRecentlyChecked();
            if ( links == null )
            {
                Console.WriteLine("Error retrieving links to check.  No processing done.");
                MyManager.LogError("Error retrieving links to check in Phase1Processing().  No processing done.");
                return;
            }
            List<string> badHostList = new List<string>( links.Count );

            foreach ( MyEntity link in links )
            {
                someIdx++;
                //string status = "successful";
                if (badHostList.Contains(link.HostName))
                {
                    link.IsDeleted = true;
                    link.LastCheckDate = DateTime.Now;
                    myManager.Update(link);
                    continue;
                }

                nbrRedirects = 0;
                CheckLink(link);
            }//foreach
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
            someIdx = CheckedRecords = 0;

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

            List<string> badHostList = new List<string>(links.Count);

            foreach (MyEntity link in links)
            {
                someIdx++;
                //string status = "successful";
                if (badHostList.Contains(link.HostName))
                {
                    link.IsDeleted = true;
                    link.LastCheckDate = DateTime.Now;
                    myManager.Update(link);
                    continue;
                }

                nbrRedirects = 0;
                CheckLink(link);
            }//foreach
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
        }// Phase1ProcessingForRange

        static void Phase2Processing()
        {
            Stopwatch swatch = new Stopwatch();
            swatch.Start();

            someIdx = CheckedRecords = 0;

            string fileName = string.Format(MyManager.GetAppKeyValue("logFileTemplate", @"C:\IOER_Tools\LinkChecker\Logs\LinkCheckerLog_{0}.csv"), DateTime.Now.ToString("s").Replace(":",""));
            logFile = new StreamWriter(fileName);
            firstWrite = true;

            links = myManager.GetItemsForPhase2();
            if (links == null)
            {
                Console.WriteLine("Error retrieving links to check.  No processing done.");
                MyManager.LogError("Error retrieving links to check in Phase1Processing().  No processing done."); 
                return;
            }

            foreach (MyEntity link in links)
            {
                someIdx++;
                nbrRedirects = 0;
                CheckLink(link);
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

         static void CheckLink(MyEntity link)
        {
            string status = "successful";
            Regex knownBadProtocols = new Regex(@"^(title)|(docid)|(grade)", RegexOptions.IgnoreCase);
            Regex knownHttpProtocols = new Regex(@"^(http:)|(https:)|(www)", RegexOptions.IgnoreCase);
            if (knownBadProtocols.IsMatch(link.ResourceUrl))
            {
                Console.WriteLine(link.ResourceUrl + " known bad protocol.");
                WriteLogFile(link, "Known bad protocol", "Resource deleted from search.");
                link.LastCheckDate = DateTime.Now;
                link.IsDeleted = true;
                searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                myManager.Update(link);
            }
            else if (knownHttpProtocols.IsMatch(link.ResourceUrl))
            {
                HttpWebResponse response = null;
                HttpWebRequest request = null;
                try
                {
                    link.LastCheckDate = DateTime.Now;
                    if (link.ResourceUrl.IndexOf("http") != 0)
                    {
                        link.ResourceUrl = "http://" + link.ResourceUrl;
                    }
                    request = (HttpWebRequest)WebRequest.Create(link.ResourceUrl);
                    request.Timeout = 15000; //15 seconds
                    request.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1309.0 Safari/537.17";
                    response = (HttpWebResponse)request.GetResponse();
                    /*if (response.Headers["Location"] != null)
                    {
                        link.ResourceUrl = response.Headers["Location"].ToString();
                        CheckLink(link);
                        return;
                    }*/
                    var urlStatusCode = (int)response.StatusCode;
                    if (urlStatusCode == 200 | response.StatusCode.ToString() == "OK")
                    {
                        string percent = (Math.Round(decimal.Divide(someIdx, links.Count), 5) * 100).ToString("0.0000000");
                        Console.WriteLine(link.ResourceUrl + " is Okay " + "( " + percent.Substring(0, percent.Length - 2) + "% )");
                        link.NbrDnsErrors = 0;
                        link.NbrInternalServerErrors = 0;
                        link.NbrTimeouts = 0;
                        link.NbrUnableToConnect = 0;
                        myManager.Update(link);
                        CheckedRecords++;
                        response.Close();
                    }
                    else
                    {
                        //CheckedRecords++;
                        response.Close();

                        throw new Exception("#" + someIdx + " failed with: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.ToString().Contains("The operation has timed out"))
                    {
                        Console.WriteLine(link.ResourceUrl + " timed out.");

                        link.NbrTimeouts++;
                        if (ThresholdReached(link.NbrTimeouts, "timeout"))
                        {
                            link.IsDeleted = true;
                            searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                            WriteLogFile(link, "Timed out threshold exceeded.", "Resource deleted from search.");
                        }
                        else
                        {
                            WriteLogFile(link, "Timed out.", "No action taken.");
                        }
                        myManager.Update(link);
                    }
                    else if (ex.ToString().Contains("(404) Not Found"))
                    {
                        Console.WriteLine(link.ResourceUrl + " was not found. (404)");
                        WriteLogFile(link, "Not found (404).", "Resource deleted from search.");
                        link.IsDeleted = true;
                        searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                        myManager.Update(link);
                    }
                    else if (ex.ToString().Contains("(403) Forbidden"))
                    {
                        Console.WriteLine(link.ResourceUrl + " is Forbidden. (403)");
                        WriteLogFile(link, "Is forbidden (403).", "Resource deleted from search.");
                        link.IsDeleted = true;
                        searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                        myManager.Update(link);
                    }
                    else if (ex.ToString().Contains("Unable to connect"))
                    {
                        Console.WriteLine(link.ResourceUrl + " cannot connect to server.");

                        link.NbrUnableToConnect++;
                        if (ThresholdReached(link.NbrUnableToConnect, "can't connect"))
                        {
                            link.IsDeleted = true;
                            searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                            WriteLogFile(link, "Cannot connect threshold exceeded.", "Resource deleted from search.");
                        }
                        else
                        {
                            WriteLogFile(link, "Cannot connect to server.", "No action taken.");
                        }
                        myManager.Update(link);
                    }
                    else if (ex.ToString().Contains("Too many automatic redirections were attempted"))
                    {
                        // This is probably because you must login.  Ignore this error
                        myManager.Update(link);
                        WriteLogFile(link,"Too many automatic redirections were attempted.","No action taken.");
                    }
                    else if (ex.ToString().Contains("Invalid URI"))
                    {
                        Console.WriteLine(link.ResourceUrl + " Invalid URI");
                        WriteLogFile(link,"Invalid URI","Resource deleted from search.");
                        link.IsDeleted = true;
                        searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                        myManager.Update(link);
                    }
                    else if (ex is WebException)
                    {
                        WebException wex = (WebException)ex;
                        if (wex.Status == WebExceptionStatus.NameResolutionFailure)
                        {
                            link.NbrDnsErrors++;
                            if (ThresholdReached(link.NbrDnsErrors, "dns"))
                            {
                                link.IsDeleted = true;
                                searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                                WriteLogFile(link, "DNS error threshold exceeded.", "Resource deleted from search.");
                            }
                            else
                            {
                                WriteLogFile(link, "DNS error.", "No action taken.");
                            }
                            myManager.Update(link);
                        }
                        else if (wex.Response != null)
                        {
                            if (((HttpWebResponse)wex.Response).StatusCode == HttpStatusCode.InternalServerError)
                            {
                                link.NbrInternalServerErrors++;
                                if (ThresholdReached(link.NbrInternalServerErrors, "internal"))
                                {
                                    link.IsDeleted = true;
                                    searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                                    WriteLogFile(link, "Internal Server error (500) threshold exceeded.", "Resource deleted from search.");
                                }
                                else
                                {
                                    WriteLogFile(link, "Internal Server error (500).", "No action taken.");
                                }
                                myManager.Update(link);
                            }
                            else if (((HttpWebResponse)wex.Response).StatusCode == HttpStatusCode.RedirectMethod)
                            {
                                nbrRedirects++;
                                if (nbrRedirects <= 10)
                                {
                                    link.ResourceUrl = ((HttpWebResponse)wex.Response).Headers["Location"].ToString().Replace("////", "//");
                                    CheckLink(link);
                                }
                                else
                                {
                                    // Too many redirect attempts!
                                    WriteLogFile(link, "Too many redirects were attempted.", "No action taken.");
                                }
                                --nbrRedirects;
                            }
                        }
                        else
                        {
                            MyManager.LogError(link.ResourceUrl + " " + ex.ToString());
                            WriteLogFile(link, ex.Message + " I don't know what to do with this.", "No action taken.");
                        }
                    }
                    else
                    {
                        MyManager.LogError(link.ResourceUrl + " " + ex.ToString());
                        WriteLogFile(link, ex.Message + " I don't know what to do with this.", "No action taken.");
                    }
                }
            }
            else if (link.ResourceUrl.IndexOf("ftp:") > -1)
            {
                FtpWebResponse response = null;
                StreamReader reader = null;
                try
                {
                    link.LastCheckDate = DateTime.Now;
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(link.ResourceUrl);
                    request.Method = WebRequestMethods.Ftp.DownloadFile;
                    response = (FtpWebResponse)request.GetResponse();
                    if ((int)response.StatusCode == 150 || (int)response.StatusCode == 125)
                    {
                        // OK, file found
                        Stream responseStream = response.GetResponseStream();
                        reader = new StreamReader(responseStream);
                        reader.ReadToEnd();
                        string percent = (Math.Round(decimal.Divide(someIdx, links.Count), 5) * 100).ToString("0.0000000");
                        Console.WriteLine(link.ResourceUrl + " is Okay " + "( " + percent.Substring(0, percent.Length - 2) + "% )");
                        link.NbrDnsErrors = 0;
                        link.NbrInternalServerErrors = 0;
                        link.NbrTimeouts = 0;
                        myManager.Update(link);
                        CheckedRecords++;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is WebException)
                    {
                        WebException wex = (WebException)ex;
                        if ((int)((FtpWebResponse)(wex.Response)).StatusCode == 550)
                        {
                            // Not found
                            Console.WriteLine(link.ResourceUrl + " was not found. (550)");
                            WriteLogFile(link, "Not found (550).", "Resource deleted from search.");
                            link.IsDeleted = true;
                            searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                            myManager.Update(link);
                        }
                        else if ((int)((FtpWebResponse)(wex.Response)).StatusCode == 530)
                        {
                            // Not logged in
                            Console.WriteLine(link.ResourceUrl + " not logged in. (530)");
                            WriteLogFile(link, "Not logged in (530).", "Resource deleted from search.");
                            link.IsDeleted = true;
                            searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                            myManager.Update(link);
                        }
                        else if ((int)((FtpWebResponse)(wex.Response)).StatusCode == 450)
                        {
                            // Internal error (temporary)
                            Console.WriteLine(link.ResourceUrl + " Temporary internal error. (450).  Try again later.");
                            link.NbrInternalServerErrors++;
                            if (ThresholdReached(link.NbrInternalServerErrors, "InternalThreshold"))
                            {
                                WriteLogFile(link, "Temporary internal error (450) threshold exceeded.", "Resource deleted from search.");
                                link.IsDeleted = true;
                                searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                            }
                            else
                            {
                                WriteLogFile(link, "Temporary internal error (450).  Try again later.", "No action taken.");
                            }
                            myManager.Update(link);
                        }
                        else
                        {
                            MyManager.LogError(link.ResourceUrl + " " + ex.ToString());
                            WriteLogFile(link, ex.Message + " I don't know what to do with this.", "No action taken.");
                        }
                    }
                    else
                    {
                        MyManager.LogError(link.ResourceUrl + " " + ex.ToString());
                        WriteLogFile(link, ex.Message + " I don't know what to do with this.", "No action taken.");
                    }
                }
                finally
                {
                    if (response != null)
                    {
                        response.Close();
                    }
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }
            else
            {
                MyManager.LogError("Unknown Protocol for URL " + link.ResourceUrl, false);
                WriteLogFile(link, "Unknown protocol.", "No action taken.");
            }

        }

         static void WriteLogFile(MyEntity link, string message, string action)
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

        static bool ThresholdReached(int count, string type)
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
        #endregion
    }
}
