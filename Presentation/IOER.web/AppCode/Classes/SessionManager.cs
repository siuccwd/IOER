using System;
using System.Resources;
using System.Text.RegularExpressions;
using System.Web.SessionState;
using System.Globalization;
using System.Threading;
using System.Web;

using WU = ILPathways.Utilities;
using ILPathways.Utilities;
using WBE = ILPathways.Business;
//using wnDAL=workNet.DAL;

namespace ILPathways.classes
{
    /// <summary>
    /// Summary description for SessionManager.
    /// </summary>
    public class SessionManager
    {
        public SessionManager()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #region Constants
        /// <summary>
        /// Session variable for message to display in the appMessage control
        /// </summary>
        public const string APPLICATION_MESSAGE = "appMessage";

        /// <summary>
        /// Session variable for message to display on a form
        /// </summary>
        public const string FORM_MESSAGE = "FormMessage";

       

        /// <summary>
        /// Session variable for url to follow after a successful registration
        /// </summary>
        public const string POST_REGISTRATION_URL = "PostRegistrationUrl";

        /// <summary>
        /// Session variable for message to display in the system console
        /// </summary>
        public const string SYSTEM_CONSOLE_MESSAGE = "SystemConsoleMessage";

        /// <summary>
        /// Session variable for message to display in the system console
        /// </summary>
        public const string SYSTEM_GRID_PAGESIZE = "SystemGridPagesize";

        /// <summary>
        /// Session variable for SQL being passed to workNet query object
        /// </summary>
        public const string SESSION_WORKNETQUERY = "WORKNETQUERY_OBJECT";
        public const string SESSION_WORKNETQUERY_SQL = "WORKNETQUERY_SQL";

        /// <summary>
        /// Session variable for real user logged in as another user
        /// </summary>
        public const string SESSION_INCOGNITO_USER = "IncognitoUser";
        #endregion

        #region User I/O Methods
        /// <summary>
        /// Get an WBE.AppUser object from the Session. First try a registered (logged in) user 
        /// and if not found check for an MCMS user
        /// </summary>
        /// <returns></returns>
        public static WBE.IWebUser GetUserFromSession( HttpSessionState session )
        {
            return GetUserFromSession( session, WU.Constants.USER_REGISTER  );
        } //
        public static WBE.IWebUser GetUserFromSession( HttpSessionState session, string userType )
        {
            WBE.IWebUser appUser = null;
            if ( session == null )
                return appUser;

            if ( userType.Equals( WU.Constants.USER_REGISTER ) )
            {
                appUser = ( WBE.IWebUser ) session[ WU.Constants.USER_REGISTER ];
            }

            if ( appUser == null )
            {
                return null;
            }

            return appUser;
        } //


        /// <summary>
        /// Method to detemine if the current user is a guest or registered
        /// </summary>
        /// <returns></returns>
        public static bool IsGuestUser( HttpSessionState session )
        {

            if ( GetCurrentUserName( session ) == "guest" )
                return true;
            else
                return false;

        } //

        /// <summary>
        /// Method to get the userName of the current user; returns guest if an anonomous user
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentUserName( HttpSessionState session )
        {
            string userid = "";

            try
            {
                WBE.IWebUser appUser = GetUserFromSession( session, WU.Constants.USER_REGISTER );
                if ( appUser == null )
                {

                }
                else
                {
                    userid = appUser.UserName;
                }
            }
            catch
            {
                userid = "guest";
            }
            return userid;
        } //

        /// <summary>
        /// Sets the current user to the session.
        /// </summary>
        /// <param name="session">HTTP Session</param>
        /// <param name="appUser">application User</param>
        /// <param name="userType">User Type</param>
        public static void SetUserToSession( HttpSessionState session, WBE.IWebUser appUser )
        {
            session[ WU.Constants.USER_REGISTER ] = appUser;

        } //			
        #endregion
        #region Custom Menu Methods
        /// <summary>
        /// Retrieve a custom menu using the current top channel name
        /// </summary>
        /// <param name="session">HttpSessionState</param>
        /// <returns>CustomMenu</returns>
        public static CustomMenu GetCustomMenu( HttpSessionState session )
        {
            string menuName = GetDefaultCustomMenuName();

            return GetCustomMenu( session, menuName );
        } //

        /// <summary>
        /// Get a CustomMenu menu from the session
        /// </summary>
        /// <param name="session">HttpSessionState</param>
        /// <returns>CustomMenu</returns>
        public static WU.CustomMenu GetCustomMenu( HttpSessionState session, string menuName )
        {

            WU.CustomMenu menu = new WU.CustomMenu();
            try
            {
                menu = ( WU.CustomMenu ) session[ menuName ];

            }
            catch ( Exception ex )
            {
                menu = new WU.CustomMenu();
                menu.ParentMenu = "";
                LoggingHelper.LogError( ex, "SessionManager.GetCustomMenu - exception encounted" );
            }

            if ( menu == null )
            {
                return new WU.CustomMenu();
            }
            return menu;
        } //

        /// <summary>
        ///  Set a custom menu to the session using the current top channel name
        /// </summary>
        /// <param name="session">HttpSessionState</param>
        /// <param name="menu">CustomMenu</param>
        public static void SetCustomMenu( HttpSessionState session, WU.CustomMenu menu )
        {
            string menuName = GetDefaultCustomMenuName();

            SetCustomMenu( session, menu, menuName );
        } //

        /// <summary>
        /// Set a custom menu to the session
        /// </summary>
        /// <param name="session">HttpSessionState</param>
        /// <param name="menu">CustomMenu</param>
        /// <param name="menuName">Key Name for the CustomMenu</param>
        public static void SetCustomMenu( HttpSessionState session, WU.CustomMenu menu, string menuName )
        {

            try
            {
                session.Add( menuName, ( object ) menu );

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "SessionManager.SetCustomMenu - exception encounted" );
            }

        } //

        /// <summary>
        /// Retrieve default name for a custom menu 
        /// </summary>
        /// <param name="session">HttpSessionState</param>
        /// <returns>CustomMenu</returns>
        public static string GetDefaultCustomMenuName()
        {
            string mSiteSectionName = UtilityManager.getPathType().ToLower();
            string topChannel = UtilityManager.getPathChannel();
            string actualUrl = HttpContext.Current.Request.Url.AbsolutePath;
            //string actualChannel = CmsHttpContext.Current.Channel.Path;

            string menuName = "";

            if ( mSiteSectionName == "industry" )
                menuName = actualUrl;
            else
                menuName = topChannel + "Menu";

            return menuName;
        } //


        #endregion

        #region HttpSessionState Methods
        /// <summary>
        /// Helper Session method - future use if required to chg to another session provider such as SQL Server 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <param name="sysObject"></param>
        public static void Set( HttpSessionState session, string key, System.Object sysObject )
        {

            session[ key ] = sysObject;

        } //			Application.Add("IDESJobFamily", (System.Object) ds);

        /// <summary>
        /// Get a key from a session, default to blank if not found
        /// </summary>
        /// <param name="session">HttpSessionState</param>
        /// <param name="key">Key for session</param>
        /// <returns>string</returns>
        public static string Get( HttpSessionState session, string key )
        {

            string value = "";
            try
            {
                if ( session[ key ] != null )
                    value = session[ key ].ToString();
                else
                    value = "";

            }
            catch ( Exception ex )
            {
                value = "";
            }


            return value;
        } //

        /// <summary>
        /// Get a key from a session, return default value if not found
        /// </summary>
        /// <param name="session">HttpSessionState</param>
        /// <param name="key">Key for session</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string Get( HttpSessionState session, string key, string defaultValue )
        {

            string value = "";
            try
            {
                if ( session[ key ] != null )
                    value = session[ key ].ToString();
                else
                    value = defaultValue;

            }
            catch ( Exception ex )
            {
                value = defaultValue;
            }


            return value;
        } //

        /// <summary>
        /// Get a key from a session, return default value if not found
        /// </summary>
        /// <param name="session">HttpSessionState</param>
        /// <param name="key">Key for session</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int Get( HttpSessionState session, string key, int defaultValue )
        {

            int value;
            try
            {
                if ( session[ key ] != null )
                    value = Int32.Parse( session[ key ].ToString() );
                else
                    value = defaultValue;

            }
            catch ( Exception ex )
            {
                value = defaultValue;
            }


            return value;
        } //
        #endregion
        #region Resource Methods
        /// <summary>
        /// getResourceValueByKey - look up a resource string by key
        /// </summary>
        /// <param name="lKeyString"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static string getResourceValueByKey( string resKey, HttpSessionState session )
        {

            string lang = WU.UtilityManager.getLanguage();
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture( lang );
            Thread.CurrentThread.CurrentUICulture = new CultureInfo( lang );

            //ResourceManager rm = Resources.resources.content.ResourceManager;
            ResourceManager rm = getResourceManager( session );

            return GetResourceByKey( rm, resKey );

        }

        /// <summary>
        /// Get a resource value using a passed resource manager and key name
        /// </summary>
        /// <param name="rm">Resource Manager</param>
        /// <param name="keyName">Key name to the applcable resource file</param>
        /// <returns>Applicable resource string</returns>
        public static string GetResourceByKey( ResourceManager rm, string resKey )
        {
            string keyName = "";
            //do any necessary translation
            try
            {
                keyName = Regex.Replace( resKey, "\\.", "_" );
                keyName = Regex.Replace( keyName, "\\*", "_" );
            }
            catch ( ArgumentException ex )
            {
                // Syntax error in the regular expression
                keyName = resKey;
            }

            return rm.GetString( keyName );

        } //

        /// <summary>
        /// Get appropriate resource manager
        /// </summary>
        /// <param name="session">HttpSessionState</param>
        /// <returns>ResourceManager</returns>
        public static ResourceManager getResourceManager( HttpSessionState session )
        {
            ResourceManager rm = new ResourceManager( "Resources", typeof( SessionManager ).Assembly );
            //10-01-25 twright - the ResourceManager is not serializable. We had to stop saving it to a session variable 
            //									 in order to implement SQL Server out or process session management
            //if ( session[ "resourceManager" ] != null )
            //{
            //  rm = ( ResourceManager ) session[ "resourceManager" ];
            //} else
            //{
            //rm = System.Resources.ResourceManager.CreateFileBasedResourceManager;
            //session[ "resourceManager" ] = rm;
            //}

            return rm;
        }
        #endregion
    }
}
