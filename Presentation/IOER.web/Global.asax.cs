using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            // Register a route for detail page
            routes.MapPageRoute(
               "Detail Record",      // Route name
               "IOER/{RouteVID}/{*ItemTitle}",      // Route URL
               "~/ResourceDetail.aspx" // Web page to handle route
            );
            routes.MapPageRoute(
              "Detail Record2",      // Route name
              "IOER2/{RouteVID}/{*ItemTitle}",      // Route URL
              "~/ResourceDetail2.aspx" // Web page to handle route
           );
            // The {*ItemTitle} instructs the route to match all content after the first slash, which is needed b/c some titles names contain a slash or other special characters
            // See http://forums.asp.net/p/1417546/3131024.aspx for more information

            routes.MapPageRoute(
                "Content Record",      // Route name
                "CONTENT/{RouteID}/{*ItemTitle}",      // Route URL
                "~/Repository/ResourcePage.aspx" // Web page to handle route
             );

            //communites =======================================
            routes.MapPageRoute(
                "Communities Home",
                "Communities",     
                "~/Communities/Default.aspx" 
             );
            routes.MapPageRoute(
                "Community Home",      // Route name
                "Community/{RouteID}/{*ItemTitle}",      // Route URL
                "~/Communities/Community.aspx" // Web page to handle route
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
            //K12 author's pages
            routes.MapPageRoute(
             "K12 Author Search",      // Route name
             "K12/Search/{OrgName}/{Author}",      // Route URL
             "~/Repository/Search.aspx" // Web page to handle route
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
             "LibraryOld/{*LbraryTitle}",      // Route URL
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
            //UtilityManager.DoTrace( mAppTraceLevel, "___ Session_Start Entry___" );	

            //increment user count
            //Application.Lock();
            //Application["GuestUsers"] = Convert.ToInt32(Application["GuestUsers"]) + 1;
            //Application.UnLock();
            //UtilityManager.DoTrace(8,"User count update: Guests= " + Application["GuestUsers"].ToString() + " Registered= " + Application["RegisteredUsers"].ToString());

            //Do we want to track the referer somehow??
            string lRefererPage = "";
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
            string startMsg = "Session Started.";
            if ( lRefererPage.Length > 0 )
                startMsg += " Referrer: " + lRefererPage;

            //Log page visit
            if ( UtilityManager.GetAppKeyValue( "loggingPageVisits", "no" ) == "yes" )
            {
                //string path = Request.ServerVariables["SERVER_NAME"];
    
                string serverName = UtilityManager.GetAppKeyValue( "serverName" );

                GatewayServices.LogSessionStart( Session.SessionID.ToString(), serverName, startMsg, Request.ServerVariables[ "REMOTE_HOST" ] );
            }
    
            //optionally dump the request variables
            if ( UtilityManager.GetAppKeyValue( "SessionShowRequestKeys", "no" ) == "yes" )
            {
               // string reqParms = GetRequestVariables();
                //
                //				string reqParms = "####### Request variables for Session = " +  Session.SessionID.ToString() + "\r\n";
                //
                //				foreach (string key in Request.ServerVariables.Keys) {
                //					//Response.Write(String.Format("<b>{0}</b> = {1}<br>",  key, Request.ServerVariables[key]));
                //
                //					reqParms +=  "\r\n" + String.Format("<b>{0}</b> = {1}<br>",  key, Request.ServerVariables[key]);
                //				}
                //UtilityManager.DoTrace(12,reqParms);
            }

            /* Code ensures that users come in through default page only	
              string startPage = Request.Url.ToString().ToLower();
                if (!startPage.EndsWith("default.aspx") || !startPage.EndsWith("debugstartpage.htm"))
                {
                    Response.Redirect("~/default.aspx");   // prevent the user from bypassing the javascript check
                }*/


        }

        void Session_End( object sender, EventArgs e )
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }

    }
}
