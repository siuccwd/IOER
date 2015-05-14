using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

using ILPathways.Utilities;
using Isle.BizServices;

namespace IllinoisPathways
{
    public class Global : System.Web.HttpApplication
    {

        void Application_Start( object sender, EventArgs e )
        {
            // Code that runs on application startup

            RegisterRoutes( RouteTable.Routes );

        }

        void RegisterRoutes( RouteCollection routes )
        {
            //resources =======================================
            // Register a route for detail page
            routes.MapPageRoute(
               "RV Record",      // Route name
               "IOER/{RouteVID}/{*ItemTitle}",      // Route URL
               "~/ResourceDetail.aspx" // Web page to handle route
            );
            //using resourceId
            routes.MapPageRoute(
               "Resource Record",      // Route name
               "Resource/{RouteRID}/{*ItemTitle}",      // Route URL
               "~/ResourceDetail.aspx" // Web page to handle route
            );
            routes.MapPageRoute(
              "Detail Record2",      // Route name
              "IOER2/{RouteVID}/{*ItemTitle}",      // Route URL
              "~/ResourceDetail2.aspx" // Web page to handle route
           );

            //communites =======================================
            routes.MapPageRoute(
                 "Community Home",      // Route name
                 "Community/{RouteID}/{*ItemTitle}",      // Route URL
                 "~/Communities/Community.aspx" // Web page to handle route
              );

            routes.MapPageRoute(
                "Communities Home",
                "Communities",     
                "~/Communities/Default.aspx" 
             );
 
            //search K12 content =========================================
            routes.MapPageRoute(
               "IOER Custom content",
               "Repository/All",
               "~/Repository/Search.aspx"
            );
            //search K12 content
            routes.MapPageRoute(
               "K12 Districts",
               "K12/All",
               "~/Repository/Search.aspx"
            );
            //search curriculum
            routes.MapPageRoute(
               "curriculum search",
               "Curriculum",
               "~/Repository/Search.aspx?t=curriculum"
            );
            //K12 author's pages
            routes.MapPageRoute(
             "K12 Author Search",      // Route name
             "K12/Search/{OrgName}/{Author}",      // Route URL
             "~/Repository/Search.aspx" // Web page to handle route
            );
            // The {*ItemTitle} instructs the route to match all content after the first slash, which is needed b/c some titles names contain a slash or other special characters
            // See http://forums.asp.net/p/1417546/3131024.aspx for more information

            routes.MapPageRoute(
                "Content Record",      // Route name
                "CONTENT/{RouteID}/{*ItemTitle}",      // Route URL
                "~/Repository/ResourcePage.aspx" // Web page to handle route
             );
            routes.MapPageRoute(
                "Curriculum Node",         //Route name
                "Curriculum/{node}/{*title}",          //Route URL
                "~/Controls/Curriculum/Default.aspx"         //Web page to handle route
            );
            //routes.MapPageRoute(
            //    "Curriculum Node2",         //Route name
            //    "Curriculum/{node}/{node2}/{*title}",          //Route URL
            //    "~/Controls/Curriculum/Default.aspx"         //Web page to handle route
            //);
            routes.MapPageRoute( 
                "Curriculum Editor",
                "My/Curriculum/{node}/{*title}",
                "~/My/Curriculum.aspx"
            );
            routes.MapPageRoute(
                "Learning List Node",         //Route name
                "LearningList/{node}/{*title}",          //Route URL
                "~/Controls/Curriculum/Default.aspx"         //Web page to handle route
            );
            routes.MapPageRoute(
                "Learning List Editor",
                "My/LearningList/{node}/{*title}",
                "~/My/Curriculum.aspx"
            );


            //K12 author's home page
            routes.MapPageRoute(
             "K12 Author Home",      // Route name
             "K12/Home/{OrgName}/{Author}",      // Route URL
             "~/Repository/ResourcePage.aspx" // Web page to handle route
            );
            //search K12 district content
            routes.MapPageRoute(
             "K12 District87",      // Route name
             "K12/{*DistrictName}",      // Route URL
             "~/Repository/Search.aspx" // Web page to handle route
            );

            //library ===============================================
            routes.MapPageRoute(
             "My Library",      // Route name
             "My/Library",      // Route URL
             "~/Libraries/Library.aspx?id=mine"             // Web page to handle route
            );
            //library by name. Check org then personal
            routes.MapPageRoute(
             "Named Library",      // Route name
             "Library_/{*LbraryTitle}",      // Route URL
             "~/Libraries/Library.aspx"             // Web page to handle route
            );
            routes.MapPageRoute(
                "Collection Home",
                "Library/Collection/{libID}/{colID}/{*ItemTitle}",
                "~/Libraries/Library.aspx"
             );

            routes.MapPageRoute(
                "Library Home",      // Route name
                "Library/{libID}/{*ItemTitle}",      // Route URL
                "~/Libraries/Library.aspx" // Web page to handle route
             );
            routes.MapPageRoute(
                 "LibrariesSearch",      // Route name
                 "Libraries/Search",      // Route URL
                 "~/Libraries/Default.aspx"
            );
            //Timeline =======================================
            routes.MapPageRoute(
             "My Timeline",      // Route name
             "My/Timeline",      // Route URL
             "~/Activity/Default.aspx?id=mine"             // Web page to handle route
            );
            //IOER Timeline
            routes.MapPageRoute(
             "IOER Timeline",      // Route name
             "IOER_Timeline",      // Route URL
             "~/Activity/Default.aspx"             // Web page to handle route
            );

            //people =======================================
            routes.MapPageRoute(
            "My Dashboard",      // Route name
            "My/Dashboard",      // Route URL
            "~/Pages/Dashboard.aspx?id=mine"    
           );
            routes.MapPageRoute(
            "Profiles",      // Route name
            "Profile/{userId}/{*ItemTitle}",      // Route URL
            "~/Pages/Dashboard.aspx"    
            );

            //partners =======================================
            routes.MapPageRoute(
            "Organizations",      // Route name
            "Organizations",      // Route URL
            "~/Organizations/Default.aspx"
            );

            routes.MapPageRoute(
             "Organization",      // Route name
             "Organizations/{orgId}/{*ItemTitle}",      // Route URL
             "~/Organizations/Organization.aspx"
             );

            routes.MapPageRoute(
             "OrganizationTimeline",      // Route name
             "Org/{orgId}/{*ItemTitle}",      // Route URL
             "~/Activity/Default.aspx"            
             );
            routes.MapPageRoute(
            "UnityPoint",      // Route name
            "UnityPoint",      // Route URL
            "~/Organizations/UnityPoint.aspx"
           );
            routes.MapPageRoute(
             "Search",      // Route name
             "Search",      // Route URL
             "~/Search.aspx"
            );
            routes.MapPageRoute(
             "Gooru",      // Route name
             "gooruSearch",      // Route URL
             "~/Pages/GooruSearch.aspx"
            );
            routes.MapPageRoute(
             "GooruPlayer",      // Route name
             "gooruResource",      // Route URL
             "~/Pages/GooruPlayer.aspx"
            );
            //other =======================================
            routes.MapPageRoute(
            "ContactUs",
            "ContactUs",
            "~/Pages/ContactUs.aspx"
            );

          //Ubertagger
            routes.MapPageRoute(
              "UberTagger",
              "ubertagger",
              "~/ubertagger.aspx"
            );
        }

        void Application_End( object sender, EventArgs e )
        {
            //  Code that runs on application shutdown

        }

        void Application_Error( object sender, EventArgs e )
        {
            // Code that runs when an unhandled error occurs

        }

        void Session_Start( object sender, EventArgs e )
        {
            try
            {
                //Do we want to track the referer somehow??
                string lRefererPage = GetUserReferrer();
               
                string ipAddress = this.GetUserIPAddress();
                ///check for bots
                ///bot, crawler, spider, slurp, crawling
                string agent = Request.UserAgent != null ? Request.UserAgent : "none";
                bool isBot = false;
                if ( agent.ToLower().IndexOf( "bot" ) > -1
                    || agent.ToLower().IndexOf( "spider" ) > -1
                    || agent.ToLower().IndexOf( "slurp" ) > -1
                    || agent.ToLower().IndexOf( "crawl" ) > -1
                    )
                    isBot = true;

                if ( isBot == false )
                {

                    LoggingHelper.DoTrace( 2, string.Format( "Session_Start. referrer: {0}, agent: {1}, IP Address: ", lRefererPage, agent, ipAddress ) );

                    string startMsg = "Session Started. SessionID: " + Session.SessionID;
                    //2015-04 mparsons - referrer is now stored separately, and often very large, so skip
                    //if ( lRefererPage.Length > 0 )
                    //    startMsg += ", Referrer: " + lRefererPage;
                    //string ipAddress = Request.ServerVariables[ "REMOTE_HOST" ] == null ? "unknown" : Request.ServerVariables[ "REMOTE_HOST" ];
                    startMsg += ", IP Address: " + ipAddress;
                    startMsg += ", Agent: " + agent;

                    ActivityBizServices.SessionStartActivity( startMsg, Session.SessionID.ToString(), ipAddress, lRefererPage );

                    //Log page visit
                    if ( UtilityManager.GetAppKeyValue( "loggingPageVisits", "no" ) == "yes" )
                    {
                        //string path = Request.ServerVariables["SERVER_NAME"];

                        string serverName = UtilityManager.GetAppKeyValue( "serverName" );

                        GatewayServices.LogSessionStart( Session.SessionID.ToString(), serverName, startMsg, ipAddress, lRefererPage );
                    }
                }
                else
                {
                    LoggingHelper.DoTrace(2, string.Format( "Session_Start. Skipping bot: referrer: {0}, agent: {1}", lRefererPage, agent ) );
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "Session_Start. =================" );
            }

        } //
        private string GetUserReferrer()
        {
            string lRefererPage = "unknown";
            try
            {
                if ( Request.UrlReferrer != null )
                {
                    lRefererPage = Request.UrlReferrer.ToString();
                    //check for link to us parm
                    //??

                    //handle refers from illinoisworknet.com 
                    if ( lRefererPage.ToLower().IndexOf( ".illinoisworknet.com" ) > -1 )
                    {
                        //may want to keep reference to determine source of this condition. 
                        //For ex. user may have let referring page get stale and so a new session was started when user returned! 

                    }
                }
            }
            catch ( Exception ex )
            {
                lRefererPage = ex.Message;
            }

            return lRefererPage;
        } //
        private string GetUserIPAddress()
        {
            string ip = "unknown";
            try
            {
                ip = Request.ServerVariables[ "HTTP_X_FORWARDED_FOR" ];
                if ( ip == null || ip == "" || ip.ToLower() == "unknown" )
                {
                    ip = Request.ServerVariables[ "REMOTE_ADDR" ];
                }
            }
            catch ( Exception ex )
            {

            }

            return ip;
        } //
        void Session_End( object sender, EventArgs e )
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }

    }
}
