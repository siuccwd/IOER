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
using LRWarehouse.Business;
namespace IOER
{
    public class Global : System.Web.HttpApplication
    {

        void Application_Start( object sender, EventArgs e )
        {
            // Code that runs on application startup

            RegisterRoutes( RouteTable.Routes );

        }

		void RegisterRoutes(RouteCollection routes)
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
			   "Content/Search",
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
			 "My Authored Search",
			 "My/Authored",
			 "~/My/Authored2.aspx"
			);
			//K12 author's pages
			//routes.MapPageRoute(
			// "My Authored SearchOLD",
			// "My/AuthoredOLD",
			// "~/My/Authored.aspx"
			//);
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
			routes.MapPageRoute(
				"Learning Set Display",         
				"LearningSet/{node}/{*title}",         
				"~/Controls/Curriculum/Default.aspx"
			);
			routes.MapPageRoute(
				"Learning Set Editor",
				"My/LearningSet/{node}/{*title}",
				"~/My/Curriculum.aspx",
				false,
				new RouteValueDictionary { { "mode", "learningset" } }
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
			 "Library_/{*LibraryTitle}",      // Route URL
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
			 "~/Activity/Default.aspx?id=mine"           
			);
			//IOER Timeline
			routes.MapPageRoute(
			 "IOER Timeline",      // Route name
			 "IOER_Timeline",      // Route URL
			 "~/Activity/Default.aspx"           
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

			//Organizations =======================================
			routes.MapPageRoute(
			"MyOrganizationsOld",      // Route name
			"MyOrganizationsOld",      // Route URL
			"~/Organizations/Organizations.aspx"
			);
			routes.MapPageRoute(
			"MyOrganizations",      // Route name
			"MyOrganizations",      // Route URL
			"~/Organizations/Default.aspx"
			);
			routes.MapPageRoute(
			"Organizations",      // Route name
			"Organizations",      // Route URL
			"~/Organizations/Default.aspx"
			);

			//routes.MapPageRoute(
			// "Organization",      // Route name
			// "Organizations/{orgId}/{*ItemTitle}",      // Route URL
			// "~/Organizations/Default.aspx"
			// );

			routes.MapPageRoute(
			 "OrganizationTimeline",      // Route name
			 "Org/{orgId}/{*ItemTitle}",      // Route URL
			 "~/Activity/Default.aspx"
			 );

			//search =======================================
			routes.MapPageRoute(
			 "Search",      // Route name
			 "Search",      // Route URL
			 "~/Search.aspx",
				false,
				new RouteValueDictionary { //Parameters and Values to pass to the page
					{ "theme", "ioer" } //Override default theme - ioer theme IS the default, but for consistency, I'm leaving this here
				}
			);
			routes.MapPageRoute(
			 "Gooru",      // Route name
			 "gooruSearch",      // Route URL
			 "~/Search.aspx",
				false,
				new RouteValueDictionary { //Parameters and Values to pass to the page
					{ "theme", "gooru" } //Override default theme to use the gooru theme instead
				}
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

			//Developer documentation
			routes.MapPageRoute(
				"DevelopersPages",
				"Developers/{page}",
				"~/Pages/Developers/Default.aspx"
			);
			routes.MapPageRoute(
				"DevelopersIndex",
				"Developers",
				"~/Pages/Developers/Default.aspx"
			);

			//User Guides
			routes.MapPageRoute(
				"UserGuide",
				"Help/Guide/{*tab}",
				"~/Help/Guide.aspx"
			);

			//Custom searches
			routes.MapPageRoute(
				"LearningListsSearch",
				"LearningLists",
				"~/Search.aspx",
				false,
				new RouteValueDictionary { //Values to pass to control
					{ "title", "IOER Learning List / Learning Set Search" }, //Override default title
					{ "collectionIDs", UtilityManager.GetAppKeyValue( "learningListCollectionId", "693" ) }, //Search within the special learning list collection
					{ "theme", "ioer_library" }, //override default theme to use the library theme, since it supports customizing the filters based on collection ID
					{ "doAutoNewestSearch", "false" } //Disable doing an auto search for newest
				}
			);

			//Rubrics
			routes.MapPageRoute(
				"Rubrics",
				"evaluate/{resourceID}/{*ItemTitle}",
				"~/controls/rubricsv2/default.aspx"
				);

			//Ubertagger
			routes.MapPageRoute(
				"newbertagger",
				"tagger",
				"~/controls/ubertaggerv4/default.aspx"
				);

			routes.MapPageRoute(
				"ErrorHandler",
				"Error/{error}",
				"~/ErrorPage.aspx"
				);


		} //

        void Application_End( object sender, EventArgs e )
        {
            //  Code that runs on application shutdown

        }
		void Application_EndRequest(object sender, System.EventArgs e)
		{
			if ( UtilityManager.GetAppKeyValue( "envType", "prod" ) == "prod" )
			{
				// If the user is not authorised to see this page or access this function, send them to the error page.
				if ( Response.StatusCode == 401 )
				{
					Response.ClearContent();
					ServiceHelper.SetConsoleErrorMessage( "Error: You must be authenticated and authorized to use that feature." );
					Response.Redirect( "/Error/401", true );
				}
				else if ( Response.StatusCode == 404 )
				{
					Response.ClearContent();
					Response.Redirect( "/Error/404", true );
					//Response.Redirect("/PageNotFound.aspx", true); 
				}
			}
			else
			{
				return;
			}
		}
        void Application_Error( object sender, EventArgs e )
        {
            // Code that runs when an unhandled error occurs
			Exception objErr = Server.GetLastError().GetBaseException();
			string url = "Startup";
			string reqUrl = "unknown";
			string remoteIP = "";
			string sessionId = "";
			bool loggingError = true;
			string userId = "";
			string userEmail = "";
			string message = "";
			try
			{
				url = Request.Url.ToString();
				message += "Url: " + url;
				reqUrl = Request.UserHostAddress;
				message += "\r\nreqUrl: " + reqUrl;
				remoteIP = Request.ServerVariables[ "REMOTE_HOST" ];
				if ( Session.SessionID != null || Session.SessionID != "" )
				{
					sessionId = Session.SessionID.ToString();
				}
				Patron user = AccountServices.GetUserFromSession( Session );
				if ( user != null && user.Id > 0)
				{
					userEmail = user.Email;
					message += "user: " + user.Email;
				}
				string err = "Application_Error: Error in: " + Request.Url.ToString() +
					". Error Message:" + objErr.Message.ToString();
				LoggingHelper.DoTrace( 2, err );
				LoggingHelper.LogError( err );

			}
			catch
			{
				//no request object - continue
			}
			string errType = objErr.GetType().ToString();
			string errMessage = objErr.Message.ToString();

			string errMsg = "Unhandled exception Caught in Application_Error event" +
				"\r\nError in: " + url +
				"\r\nType: " + errType +
				"\r\nUser: " + userEmail + "_______Session Id - " + sessionId +
				"\r\nfrom: " + reqUrl +
				"\r\nError Message:" + errMessage +
				"\r\nStack Trace:" + objErr.StackTrace.ToString();
			try
			{
				errMsg += "\n\rSource: " + objErr.Source.ToString();
				errMsg += "\n\rInnerException: " + objErr.InnerException.ToString();

			}
			catch
			{
				//ignore, possibly nulls
			}

			//check for messages to ignore
			if ( errType.Equals( "System.Web.HttpException" ) )
			{
				//check
				if ( errMessage.IndexOf( ".aspx' does not exist." ) > -1 )
				{
					//probably bot related error
					loggingError = false;
				}
			}

			bool doRedirect = false;
			//check if the "user" is a known robot
			//84.109.121.176 - ripe in amsterdam
			// actually 84.109.0.0 - 84.109.255.255 has been allocated to a site in Isreal
			string bots = "84.109.121.176 94.102.60.35 61.234.105.61";
			if ( bots.IndexOf( remoteIP ) > -1
				|| remoteIP.IndexOf( "66.249" ) > -1 )
			{
				loggingError = false;

			}
			//just redirect always:
			//doRedirect = true;

			try
			{
				Session[ "AppErrorUrl" ] = url;
				Session[ "AppError" ] = objErr;

				if ( doRedirect )
				{
					string redirectUrl = "http://ioer.ilsharedlearning.org";
					Response.Redirect( redirectUrl, true );
				}
			}
			catch ( System.Threading.ThreadAbortException taex )
			{
				//Ignore this exception, it's okay!
			}
			catch
			{
				//ignore
			}

			//If you do not call Server.ClearError or trap the error in the Page_Error or Application_Error event handler, 
			//the error is handled based on the settings in the <customErrors> section of the Web.config file
			//Server.ClearError();
        }

        void Session_Start( object sender, EventArgs e )
        {
            try
            {
                //Do we want to track the referer somehow??
                string lRefererPage = GetUserReferrer();
				bool isBot = false;
                string ipAddress = this.GetUserIPAddress();
                //check for bots
				//use common method
				string agent = ActivityBizServices.GetUserAgent( ref isBot );
               
                //string agent = Request.UserAgent != null ? Request.UserAgent : "none";
				//ISSUE - if we don't add the initial bot session to the activity log, we can't check for follow on activity related to the bot!
				//if ( agent.ToLower().IndexOf( "bot" ) > -1
				//	|| agent.ToLower().IndexOf( "spider" ) > -1
				//	|| agent.ToLower().IndexOf( "slurp" ) > -1
				//	|| agent.ToLower().IndexOf( "crawl" ) > -1
				//	|| agent.ToLower().IndexOf( "addthis.com" ) > -1
				//	)
				//	isBot = true;

                if ( isBot == false )
                {

                    LoggingHelper.DoTrace( 6, string.Format( "Session_Start. referrer: {0}, agent: {1}, IP Address: ", lRefererPage, agent, ipAddress ) );

                    string startMsg = "Session Started. SessionID: " + Session.SessionID;
                    //2015-04 mparsons - referrer is now stored separately, and often very large, so skip
                    //if ( lRefererPage.Length > 0 )
                    //    startMsg += ", Referrer: " + lRefererPage;
                    //string ipAddress = Request.ServerVariables[ "REMOTE_HOST" ] == null ? "unknown" : Request.ServerVariables[ "REMOTE_HOST" ];
                    startMsg += ", IP Address: " + ipAddress;
                    startMsg += ", Agent: " + agent;

					if ( User.Identity.IsAuthenticated )
					{
						Patron user = new AccountServices().GetByEmail( User.Identity.Name );
					}

					ActivityBizServices.SessionStartActivity( startMsg, Session.SessionID.ToString(), ipAddress, lRefererPage, isBot );

                    //Log page visit
                    if ( UtilityManager.GetAppKeyValue( "loggingPageVisits", "no" ) == "yes" )
                    {
                        //string path = Request.ServerVariables["SERVER_NAME"];

                        string serverName = UtilityManager.GetAppKeyValue( "serverName" );

                        //GatewayServices.LogSessionStart( Session.SessionID.ToString(), serverName, startMsg, ipAddress, lRefererPage );
                    }
                }
                else
                {
                    ServiceHelper.DoBotTrace(8, string.Format( "Session_Start. Skipping bot: referrer: {0}, agent: {1}", lRefererPage, agent ) );
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
