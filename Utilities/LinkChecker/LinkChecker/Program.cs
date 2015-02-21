using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Collections;

using LinkChecker.library;

using ILPathways.Utilities;
using MyManager = LinkChecker.library.ResourceLinkManager;

namespace LinkChecker
{
    class Program
    {


        static void Main( string[] args )
        {
            Stopwatch sWatch = new Stopwatch();
            sWatch.Start();

            //Timer timer = new Timer();
            //timer.Start();
            HandleLinkChecking.CheckLinks();

            sWatch.Stop();
            string msg1 = "Finished in " + sWatch.Elapsed.Hours + "h:" + sWatch.Elapsed.Minutes + "m:" + sWatch.Elapsed.Seconds + "s";
            string msg2 = "Checked " + HandleLinkChecking.CheckedRecords + " records";
            string RunType = MyManager.GetAppKeyValue( "runType", "console" );

            LoggingHelper.DoTrace( 1, msg1 + "\r\n" + msg2 );
            LoggingHelper.DoTrace( 1, "Last url: " + HandleLinkChecking.LastUrl );
            LoggingHelper.DoTrace( 1, "Bad sites: " + HandleLinkChecking.badSites);
            LoggingHelper.DoTrace( 1, "Bad Urls: " + HandleLinkChecking.badURLs);

            Console.WriteLine( msg1 );
            Console.WriteLine( msg2 );
            Console.WriteLine( "Last url: " + HandleLinkChecking.LastUrl );

            if ( RunType == "debug" )
            {

                Console.ReadLine();
            }
            //timer.Finish( HandleLinkChecking);
        }

    }

    class Timer
    {
        Stopwatch sWatch = new Stopwatch();
        public void Start()
        {
            sWatch.Start();
        }
        public void Finish( HandleLinkChecking service )
        {
            sWatch.Stop();
            string msg1 = "Finished in " + sWatch.Elapsed.Hours + "h:" + sWatch.Elapsed.Minutes + "m:" + sWatch.Elapsed.Seconds + "s";
            //string msg2 = "Checked " + service.CheckedRecords + " records";

            //LoggingHelper.DoTrace( 1, msg1 + "\r\n" + msg2);
            
            Console.WriteLine( msg1 );
           // Console.WriteLine( msg2 );
            Console.ReadLine();
        }
    }

    
}
