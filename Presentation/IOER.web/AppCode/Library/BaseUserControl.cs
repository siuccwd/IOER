using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mail;
using RegEx = System.Text.RegularExpressions.Regex;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using ILPathways.Utilities;
using EmailHelper = ILPathways.Utilities.EmailManager;
using IOER.classes;

using IPB = ILPathways.Business;
using AppUser = LRWarehouse.Business.Patron; //ILPathways.Business.AppUser;
using IWebUser = ILPathways.Business.IWebUser;
using LRWarehouse.Business;

namespace IOER.Library
{
    /// <summary>
    /// Summary description for BaseUserControl
    /// </summary>
    public class BaseUserControl : System.Web.UI.UserControl
    {
        private const string thisBaseClassName = "BaseUserControl";

        protected AppUser _appUser = new AppUser();

        protected string gPageLang = "";
        protected string gSiteSectionName = "";
        protected string gResPrefix = "";

        protected string mCurrentTemplate = "";
        #region Contants, etc
        const string showHideTemplate = "<div style=\"float: left\"><a style=\"float: left\" onclick=\"ShowHideSection('{0}');\" "
    + "href=\"javascript:void(0);\"><img alt=\"Show/Hide\" src=\"/vos_portal/images/infoBubble.gif\" />{1}</a> <br /></div>	"
    + "<div class=\"clear\"></div> "
    + "<div id=\"{2}\" class=\"infoMessage\" style=\"display: none; width:90%;padding-left:5px;\">{3}</div>";

        const string showHideSplitTemplatePart1 = "<a href=\"javascript:void(0);\" onclick=\"ShowHideSection('{0}');\"> "
        + "<img alt=\"{1}\" src=\"/vos_portal/images/infoBubble.gif\" /></a>	";
        const string showHideSplitTemplatePart1a = "<a class='infoField' href=\"javascript:void(0);\" onclick=\"ShowHideSection('{0}');\"> "
        + "<img alt=\"{1}\" src=\"/vos_portal/images/infoBubble.gif\" />&nbsp;{2}</a>	";

        const string showHideSplitTemplatePart2 = "<div id=\"{0}\" class=\"infoMessage\" style=\"display: none; width:90%;padding-left:5px;\">{1}</div>";

        #endregion
        private enum SMTPResponse : int
        {
            CONNECT_SUCCESS = 220,
            GENERIC_SUCCESS = 250,
            DATA_SUCCESS = 354,
            QUIT_SUCCESS = 221
        }

        #region Properties
        /// <summary>
        /// INTERNAL PROPERTY: FormPrivileges
        /// Set initially and store in ViewState
        /// </summary>
        public IPB.ApplicationRolePrivilege FormPrivileges
        {
            get { return ViewState[ "formPrivileges" ] as IPB.ApplicationRolePrivilege; }
            set { ViewState[ "formPrivileges" ] = value; }
        }

        /// <summary>
        /// INTERNAL PROPERTY: RecordPrivileges
        /// Used where privileges are checked and assigned at a record level rather than only at the control level
        /// </summary>
        public IPB.ApplicationRolePrivilege RecordPrivileges
        {
            get { return ViewState[ "recordPrivileges" ] as IPB.ApplicationRolePrivilege; }
            set { ViewState[ "recordPrivileges" ] = value; }
        }

        /// <summary>
        ///Determines sort direction
        /// </summary>
        public System.Web.UI.WebControls.SortDirection GridViewSortDirection
        {
            get
            {
                if ( ViewState[ "GridSortDir" ] == null || ViewState[ "GridSortDir" ].ToString() == "" )
                    ViewState[ "GridSortDir" ] = System.Web.UI.WebControls.SortDirection.Ascending;

                return ( System.Web.UI.WebControls.SortDirection ) ViewState[ "GridSortDir" ];
            }
            set { ViewState[ "GridSortDir" ] = value; }
        }//

        /// <summary>
        ///Determines sort expression
        /// </summary>
        public string GridViewSortExpression
        {
            get
            {
                if ( ViewState[ "GridSortExp" ] == null || ViewState[ "GridSortExp" ].ToString() == "" )
                    ViewState[ "GridSortExp" ] = "";

                return ViewState[ "GridSortExp" ].ToString();
            }
            set { ViewState[ "GridSortExp" ] = value; }
        }//

        /// <summary>
        /// INTERNAL PROPERTY: CanCreate - is current user allowed to create a record
        /// </summary>
        public bool CanCreate
        {
            get { return Boolean.Parse( ViewState[ "CanCreate" ].ToString() ); }
            set { ViewState[ "CanCreate" ] = value; }
        }

        /// <summary>
        /// INTERNAL PROPERTY: CanUpdate - is current user allowed to create a record
        /// </summary>
        public bool CanUpdate
        {
            get { return Boolean.Parse( ViewState[ "CanUpdate" ].ToString() ); }
            set { ViewState[ "CanUpdate" ] = value; }
        }


        /// <summary>
        /// UsingAdvancedInterface - if false, default to most accessible interface
        /// --probably should be moved elsewhere - SessionManager?
        /// </summary>
        public bool UsingAdvancedInterface
        {
            get
            {
                if ( HttpContext.Current.Session[ "UsingAdvancedInterface" ] == null )
                    HttpContext.Current.Session[ "UsingAdvancedInterface" ] = false;
                return Boolean.Parse( HttpContext.Current.Session[ "UsingAdvancedInterface" ].ToString() );
            }
            set { HttpContext.Current.Session[ "UsingAdvancedInterface" ] = value; }
        }
        public void ToogleInterfaceElements( DropDownList list, Button btnAction )
        {

            if ( UsingAdvancedInterface == true )
            {
                //allow autopostback
                list.AutoPostBack = true;
                btnAction.Visible = false;
            }
            else
            {
                //don't allow autopostback
                list.AutoPostBack = false;
                btnAction.Visible = true;
            }

        } //
        #endregion

        public BaseUserControl()
        {
            
            FormPrivileges = new IPB.ApplicationRolePrivilege();
            RecordPrivileges = new IPB.ApplicationRolePrivilege();

        }

        #region Methods that are cloned in the BaseAppPage
        /// <summary>
        /// Check popups are allowed - for current user or site
        /// </summary>
        /// <returns>True if popups can be used</returns>
        protected bool AllowingPopups()
        {
            string usingPopups = UtilityManager.GetAppKeyValue( "usingPopups", "no" );

            return AllowingPopups( usingPopups );
        }
        /// <summary>
        /// Check popups are allowed - for current user or site
        /// </summary>
        /// <param name="usingPopups">Default value</param>
        /// <returns>True if popups can be used</returns>
        protected bool AllowingPopups( string usingPopups )
        {
            bool allowPopups = false;

            if ( usingPopups.Equals( "yes" ) )
                allowPopups = true;

            //future check if user has OK'd - allows for override

            return allowPopups;
        }

        #endregion

        #region Logging Methods - sync with BaseAppPage

        /// <summary>
        /// Format parameters regarding current page and log the visit with a blank comment
        /// </summary>
        protected void LogVisit()
        {
            LogVisit( "" );
        }

        /// <summary>
        /// Format parameters regarding current page and log the visit with the provided comment
        /// </summary>
        /// <param name="comment">Comment related to log entry</param>
        protected void LogVisit( string comment )
        {
            LogVisit( comment, "title" );
        }

        /// <summary>
        /// Format parameters regarding current page and log the visit with the provided comment
        /// </summary>
        /// <param name="comment">Comment related to log entry</param>
        /// <param name="titlePlaceholder">Name of the placeholder holding the page title for the current page</param>
        /// <remarks>Remember to keep in sync with the BaseAppPage.LogVisit method</remarks>
        protected void LogVisit( string comment, string titlePlaceholder )
        {
            string userid = "";
            string mcmsUrl = "";
            string parmsString = "";

            try
            {
                string sessionId = HttpContext.Current.Session.SessionID.ToString();
                mCurrentTemplate = HttpContext.Current.Request.Path;
                string remoteIP = HttpContext.Current.Request.ServerVariables[ "REMOTE_HOST" ];

                //08-03-21 mparsons - server name is now added separating in the LogManager method
                string serverName = UtilityManager.GetAppKeyValue( "serverName", HttpContext.Current.Request.ServerVariables[ "LOCAL_ADDR" ] );

                string pageTitle = GetPageTitle( titlePlaceholder );
                if ( pageTitle.ToLower() == "unknown" )
                {
                    if ( comment.Length > 0 )
                    {
                        pageTitle = comment;
                    }
                }

                string path = serverName + mCurrentTemplate;
                if ( FormHelper.IsValidRequestString() == false )
                {
                    this.LogError( thisBaseClassName + ".LogVisit: Potential invalid request string encountered!" );
                }
                if ( HttpContext.Current.Request.QueryString.Count > 0 )
                {
                    mcmsUrl = GetPublicUrl( HttpContext.Current.Request.QueryString.ToString() );

                    mcmsUrl = Server.UrlDecode( mcmsUrl );
                }

                if ( comment.ToLower().IndexOf( "externallink" ) > -1 || comment.ToLower().Equals( "landingpage" ) )
                {
                    parmsString = this.GetRequestKeyValue( "pred" );
                    parmsString = Server.HtmlDecode( parmsString );
                }
                else if ( mcmsUrl.IndexOf( "?" ) > -1 )
                {
                    parmsString = mcmsUrl.Substring( mcmsUrl.IndexOf( "?" ) + 1 );
                    mcmsUrl = mcmsUrl.Substring( 0, mcmsUrl.IndexOf( "?" ) );
                    //check for a second ?
                    if ( parmsString.IndexOf( "?" ) > -1 )
                    {
                        parmsString = parmsString.Substring( parmsString.IndexOf( "?" ) + 1 );
                    }
                }
                if ( parmsString.Length > 0 )
                {
                    parmsString = "parms:" + parmsString;
                }
                if ( pageTitle.Length > 0 && pageTitle != comment )
                {
                    comment = "pageTitle=" + pageTitle + ";" + comment;
                }

                userid = this.GetCurrentUserid();


                //skip pages that have auto refresh processing
                if ( Page.IsPostBack && parmsString.Equals( "pv=asmt" ) )
                {
                    //skip
                }
                else
                {
                    //do the log
                    //UtilityManager.LogPageVisit( sessionId, path, mcmsUrl, parmsString, Page.IsPostBack, userid, lwia, comment, remoteIP, officeId );
                }
            }
            catch ( System.Threading.ThreadAbortException abex )
            {
                //ignore this one, probably due to a redirect

            }
            catch ( Exception ex )
            {
                //ignore errors
                this.LogError( ex, thisBaseClassName + ".LogVisit: " + ex.ToString() );
            }
        } //
        /// <summary>
        /// Gets the title for the current page using the channel and a "title" placeholder. The default placeholder name is title
        /// </summary>
        /// <returns>Page Title</returns>
        protected string GetPageTitle()
        {
            return GetPageTitle( "title" );
        } //

        /// <summary>
        /// Gets the title for the current page using the channel and a "title" placeholder - based on passed parameter
        /// </summary>
        /// <param name="placeholderTitleName">Name of the text placeholder that holds the text to use in the title</param>
        /// <returns>Page Title</returns>
        protected string GetPageTitle( string placeholderTitleName )
        {

            string pageTitle;
            //string parentChannel = "";
           // string defaultTitle = "";
           // string separator = "";

            pageTitle = ""; //or default to ???

            try
            {

            //    string siteHomePath = UtilityManager.GetAppKeyValue( "mcmsRootChannel", "vos_portal" );
            //    string usingChannel = UtilityManager.GetAppKeyValue( "pageTitle.UseChannel", "no" );

            //    string cp = CmsHttpContext.Current.Channel.Path.ToString();

            //    string fullPath = cmsContext.Channel.Path;

            //    if ( usingChannel.ToLower() == "yes" )
            //    {

            //        parentChannel = UtilityManager.getPathChannel();

            //        pageTitle = defaultTitle + parentChannel;
            //        separator = " - ";
            //    }

            //    string title = McmsHelper.GetPlaceHolder( posting, placeholderTitleName );
            //    if ( title.Length > 0 )
            //    {
            //        pageTitle += separator + title;
            //    }

            }
            catch ( Exception ex )
            {
                //if there is no placeholder or any other error, leave with default title
                this.LogError( ex, thisBaseClassName + ".GetPageTitle exception" );
            }

            if ( pageTitle.Length == 0 )
                pageTitle = "Unknown";

            return pageTitle;


        } //

        #endregion


        #region Common control Methods - sync with BaseAppPage
        /// <summary>
        /// Common method to set the selected value in a list using a key name. The key name is used to first do a look up on the request query string, then the session.
        /// </summary>
        /// <param name="list">A DropDownList</param>
        /// <param name="keyName">Key name to use when inspecting the request and session objects, if not one of the latter then assume searching for its value</param>
        protected void SetListSelection( DropDownList list, string keyName )
        {

            string selection = GetRequestKeyValue( keyName );
            if ( selection.Length == 0 )
            {
                if ( HttpContext.Current.Session[ keyName ] != null )
                    selection = HttpContext.Current.Session[ keyName ].ToString();
                else
                    selection = keyName;
            }

            if ( selection.Length > 0 )
            {
                SetListSelection( list, keyName, selection );
                //if(list.Items.FindByValue(selection.ToString()) != null) {
                //  list.SelectedIndex = -1;
                //  list.Items.FindByValue(selection.ToString()).Selected = true;
                //}
            }

        } //

        /// <summary>
        /// Common method to set the selected value in a list using a key name - where related value must be an integer. 
        /// The key name is used to first do a look up on the request query string, then the session.
        /// </summary>
        /// <param name="list">A DropDownList</param>
        /// <param name="keyName">Key name to use when inspecting the request and session objects, if not one of the latter then assume searching for its value</param>
        protected void SetListSelectionInt( DropDownList list, string keyName )
        {

            string selection = "";
            int reqValue = GetRequestKeyValue( keyName, 0 );
            if ( reqValue > 0 )
            {
                selection = reqValue.ToString();
            }
            else if ( HttpContext.Current.Session[ keyName ] != null )
            {
                selection = HttpContext.Current.Session[ keyName ].ToString();
            }
            else
            {
                selection = keyName;
            }

            if ( selection.Length > 0 )
            {
                SetListSelection( list, keyName, selection );
                //if(list.Items.FindByValue(selection.ToString()) != null) {
                //  list.SelectedIndex = -1;
                //  list.Items.FindByValue(selection.ToString()).Selected = true;
                //}
            }

        } //

        /// <summary>
        /// Common method to set the selected value in a list using a key name. The key name is used to first do a look up on the request query string, then the session.
        /// </summary>
        /// <param name="list">A DropDownList</param>
        /// <param name="keyName">Key name </param>
        /// <param name="keyValue">Key value to find in the list </param>/// 
        protected void SetListSelection( DropDownList list, string keyName, string keyValue )
        {

            if ( keyValue.Length > 0 )
            {
                if ( list.Items.FindByValue( keyValue.ToString() ) != null )
                {
                    list.SelectedIndex = -1;
                    list.Items.FindByValue( keyValue.ToString() ).Selected = true;
                }
            }

        } //

        /// <summary>
        /// Common method to set the selected value in a list by text value.
        /// </summary>
        /// <param name="list">A DropDownList</param>
        /// <param name="textValue">Value to search for</param>
        protected void SetListSelectionByText( DropDownList list, string textValue )
        {
            list.SelectedIndex = -1;

            if ( textValue.Length > 0 )
            {
                if ( list.Items.FindByText( textValue.ToString() ) != null )
                {
                    list.SelectedIndex = -1;
                    list.Items.FindByText( textValue.ToString() ).Selected = true;
                }
            }

        } //
        /// <summary>
        /// Common method to set the selected value in a list using a key name. The key name is used to first do a look up on the request query string, then the session.
        /// </summary>
        /// <param name="list">A ListBox</param>
        /// <param name="keyName">Key name to use when inspecting the request and session objects, if not one of the latter then assume searching for its value</param>
        protected void SetListSelection( ListBox list, string keyName )
        {

            string selection = GetRequestKeyValue( keyName );
            if ( selection.Length == 0 )
            {
                if ( HttpContext.Current.Session[ keyName ] != null )
                    selection = HttpContext.Current.Session[ keyName ].ToString();
                else
                    selection = keyName;
            }

            if ( selection.Length > 0 )
            {
                if ( list.Items.FindByValue( selection.ToString() ) != null )
                {
                    list.SelectedIndex = -1;
                    list.Items.FindByValue( selection.ToString() ).Selected = true;
                }
            }

        } //

        /// <summary>
        /// Integer friendly common method to set the selected value in a list using a key name. 
        /// The key name is used to first do a look up on the request query string, then the session.
        /// </summary>
        /// <param name="list">A ListBox</param>
        /// <param name="keyName">Key name to use when inspecting the request and session objects, if not one of the latter then assume searching for its value</param>
        protected void SetListSelectionInt( ListBox list, string keyName )
        {

            string selection = "";
            int reqValue = GetRequestKeyValue( keyName, 0 );
            if ( reqValue > 0 )
            {
                selection = reqValue.ToString();

            }
            else if ( HttpContext.Current.Session[ keyName ] != null )
            {
                selection = HttpContext.Current.Session[ keyName ].ToString();
            }
            else
            {
                selection = keyName;
            }

            if ( selection.Length > 0 )
            {
                if ( list.Items.FindByValue( selection.ToString() ) != null )
                {
                    list.SelectedIndex = -1;
                    list.Items.FindByValue( selection.ToString() ).Selected = true;
                }
            }

        } //

        /// <summary>
        /// Common method to set the selected value in a list using a key name. The key name is used to first do a look up on the request query string, then the session.
        /// </summary>
        /// <param name="list">A RadioButtonList</param>
        /// <param name="keyName">Key name to use when inspecting the request and session objects, if not one of the latter then assume searching for its value</param>
        protected void SetListSelection( RadioButtonList list, string keyName )
        {

            string selection = GetRequestKeyValue( keyName );
            if ( selection.Length == 0 )
            {
                if ( HttpContext.Current.Session[ keyName ] != null )
                    selection = HttpContext.Current.Session[ keyName ].ToString();
                else
                    selection = keyName;
            }

            if ( selection.Length > 0 )
            {
                foreach ( ListItem item in list.Items )
                {
                    if ( item.Value.ToUpper() == selection.ToUpper() )
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }

        } //

        protected void SetListSelectionInt( RadioButtonList list, string keyName )
        {

            string selection = "";
            int reqValue = GetRequestKeyValue( keyName, 0 );
            if ( reqValue > 0 )
            {
                selection = reqValue.ToString();
            }
            else if ( HttpContext.Current.Session[ keyName ] != null )
            {
                selection = HttpContext.Current.Session[ keyName ].ToString();
            }
            else
            {
                selection = keyName;
            }

            if ( selection.Length > 0 )
            {
                foreach ( ListItem item in list.Items )
                {
                    if ( item.Value.ToUpper() == selection.ToUpper() )
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }

        } //


        #endregion



        #region Form Helper Methods - sync with BaseAppPage
        /// <summary>
        /// Add a validator message to the validation summary control. 
        /// Uses a default value of false for the setInvalid flag
        /// </summary>
        /// <param name="valSummary">Validation Summary control</param>
        /// <param name="message">Error message string</param>
        /// <param name="controlName">name of control related to the message</param>
        public void AddReqValidatorError( ValidationSummary valSummary, string message, string controlName )
        {
            AddReqValidatorError( valSummary, message, controlName, false );

        } //

        /// <summary>
        /// Add a validator message to the validation summary control
        /// </summary>
        /// <param name="valSummary">Validation Summary control</param>
        /// <param name="message">Error message string</param>
        /// <param name="controlName">name of control related to the message</param>
        /// <param name="setInvalid">Set to true if validator is to be set to invalid</param>
        /// <remarks>Note the setInvalid is typically false for a control that has specific validator assigned. It should be set to true when a new validator is created in code - often in a cross-edit condition </remarks>
        public void AddReqValidatorError( ValidationSummary valSummary, string message, string controlName, bool setInvalid )
        {
            RequiredFieldValidator rq = new RequiredFieldValidator();

            rq.ErrorMessage = message;
            rq.Display = ValidatorDisplay.None;
            //The ControlToValidate is required but doesn't really matter
            rq.ControlToValidate = controlName;

            //06-12-19 mparsons for some reason IsValid can't be set in this method or the message will be displayed twice
            if ( setInvalid )
                rq.IsValid = false;

            valSummary.Controls.Add( rq );
            valSummary.Visible = true;

        } //

        /// <summary>
        /// Converts boolean values "true" and "false" to "Yes" and "No".
        /// </summary>
        /// <param name="boolean"></param>
        /// <returns></returns>
        protected string ConvertBoolToYesNo( bool boolean )
        {
            string value = "";
            if ( boolean )
            {
                value = "Yes";
            }
            else
            {
                value = "No";
            }
            return value;
        }//
        /// <summary>
        /// Converts "Yes" and "No" values to boolean values "true" and "false".
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected bool ConvertYesNoToBool( string str )
        {
            bool value = false;
            switch ( str.ToLower() )
            {
                case "yes":
                    value = true;
                    break;
                case "si":
                    value = true;
                    break;
                case "2":
                    value = true;
                    break;
                case "no":
                    value = false;
                    break;
            }
            return value;
        }

        /// <summary>
        /// Heler method to get the value for a parameter that may come from the request querystring or the session.
        /// </summary>
        /// <param name="keyName">Key name used to insepct Request and session</param>
        /// <param name="defaultValue">Default value to use of key is not found</param>
        /// <returns>Appropriate value or the default</returns>
        protected string GetDefaultParmValue( string keyName, string defaultValue )
        {

            string selection;
            try
            {
                selection = GetRequestKeyValue( keyName, defaultValue );
                if ( selection.Length == 0 )
                {
                    if ( HttpContext.Current.Session[ keyName ] != null )
                    {
                        selection = ( string ) HttpContext.Current.Session[ keyName ].ToString();
                    }
                    else
                        selection = defaultValue;
                }
            }
            catch
            {
                selection = defaultValue;
            }

            return selection;

        } //

        /// <summary>
        /// Heler method to get the value for a parameter that may come from the request querystring or the session.
        /// </summary>
        /// <param name="keyName">Key name used to insepct Request and session</param>
        /// <param name="defaultValue">Default value to use of key is not found</param>
        /// <returns>Appropriate value or the default</returns>
        protected int GetDefaultParmValue( string keyName, int defaultValue )
        {

            int selection;
            try
            {
                if ( GetRequestKeyValue( keyName, defaultValue ) > 0 )
                {
                    selection = GetRequestKeyValue( keyName, defaultValue );
                }
                else if ( HttpContext.Current.Session[ keyName ] != null )
                {
                    selection = Convert.ToInt32( HttpContext.Current.Session[ keyName ].ToString() );
                }
                else
                    selection = defaultValue;
            }
            catch
            {
                selection = defaultValue;
            }

            return selection;

        } //

        /// <summary>
        /// Return the public version of the current MCMS url - removes MCMS specific parameters
        /// </summary>
        protected string GetPublicUrl( string url )
        {
            string publicUrl = "";

            //find common parms
            int nrmodePos = url.ToLower().IndexOf( "nrmode" );
            int urlStartPos = url.ToLower().IndexOf( "nroriginalurl" );
            int urlEndPos = url.ToLower().IndexOf( "&nrcachehint" );

            if ( urlStartPos > 0 && urlEndPos > urlStartPos )
            {
                publicUrl = url.Substring( urlStartPos + 14, urlEndPos - ( urlStartPos + 14 ) );
            }
            else
            {
                //just take everything??
                publicUrl = url;
            }

            return publicUrl;
        } //


        /// <summary>
        /// Common method for populating a form message
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="container">The HTML control that will present the message, if present</param>
        /// <param name="messageCss">Style for message</param>/// 
        protected void SetFormMessage( string message, HtmlGenericControl container, string messageCss )
        {
            container.Attributes.Add( "class", messageCss );
            container.InnerHtml += message;
            container.Style[ "display" ] = "block";
        }

        /// <summary>
        /// Determine if current user is a guest, a registered user or a content manager user
        /// </summary>
        /// <returns></returns>
        protected string GetCurrentUserid()
        {
            string userid = "";

            try
            {
                IPB.IWebUser appUser = SessionManager.GetUserFromSession( HttpContext.Current.Session, Constants.USER_REGISTER );
                if ( appUser == null )
                {
                    appUser = SessionManager.GetUserFromSession( HttpContext.Current.Session, Constants.USER_MCMS );
                    if ( appUser == null )
                    {
                        userid = "guest";
                    }
                    else
                    {
                        userid = appUser.UserName;
                    }

                }
                else
                {
                    //08-03-07 mparsons - now that we are allowing e-mail addresses as a userid, we may want something different
                    //									- determine where used
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
        /// Determine if current user is a logged in (registered) user 
        /// </summary>
        /// <returns></returns>
        protected bool IsUserAuthenticated()
        {
            bool isUserAuthenticated = false;
            try
            {
                IPB.IWebUser appUser = SessionManager.GetUserFromSession( HttpContext.Current.Session, Constants.USER_REGISTER );
                if ( appUser == null )
                {
                    isUserAuthenticated = false;
                }
                else
                {
                    isUserAuthenticated = true;
                }
            }
            catch
            {

            }

            return isUserAuthenticated;

        } //

        /// <summary>
        /// Check if the current page is one that shoud be secure (i.e. use SSL)
        /// </summary>
        /// <returns></returns>
		//protected bool IsASecurePage()
		//{
		//	bool IsASecurePage = false;

		//	try
		//	{
		//		string templatePath = HttpContext.Current.Request.Path;
		//		string template = templatePath.Substring( templatePath.LastIndexOf( "/" ) + 1 );

		//		string securePages = UtilityManager.GetAppKeyValue( "securePages" );
		//		if ( securePages.IndexOf( template ) > -1 )
		//			IsASecurePage = true;

		//	}
		//	catch
		//	{
		//		//allow default of false on exception
		//	}
		//	return IsASecurePage;
		//}

        /// <summary>
        /// Determine if the current page is the live view or a content management view 
        /// </summary>
        /// <returns></returns>
        protected bool IsPageLive()
        {
            try
            {
                string nrmode = HttpContext.Current.Request.QueryString[ "NRMODE" ].ToString().ToLower();

                if ( nrmode == "unpublished" || nrmode == "update" )
                {
                    return false;

                }
                else
                {
                    return true;

                }
            }
            catch ( System.NullReferenceException nrex )
            {	//ignore as means nrmode not found
                return true;

            }
            catch ( Exception ex )
            {
                this.LogError( ex, thisBaseClassName + ".IsPageLive exception" );
                return true;
            }

        } //


        protected string GetPathwayPrefix()
        {
            string resPrefix = "";
            string sectionName = UtilityManager.getPathType();
            if ( sectionName == "residents" )
            {
                resPrefix = "";
            }
            else if ( sectionName == "business" )
            {
                resPrefix = "biz";

            }
            else
            {
                //just use section name
                resPrefix = sectionName;
            }

            return resPrefix;
        }//

        /// <summary>
        /// Short term method to "re-use" residents resources by retrieveing and changing "residents" to "advisors"
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <returns></returns>
        protected string GetAdvisorResource( ResourceManager rm, string resourceKey )
        {
            string resourceValue = UtilityManager.GetResourceValue( rm, resourceKey );
            resourceValue = resourceValue.Replace( "residents", "advisors" );

            return resourceValue;
        }//

        /// <summary>
        /// Toggle the lock state of the passed control
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="status">True to set text box as readonly</param>
        protected void SetFieldLockedStatus( TextBox textBox, bool status )
        {
            textBox.ReadOnly = status;
            if ( ( textBox.ID.ToLower().IndexOf( "phone" ) > -1 ) || ( textBox.ID.ToLower().IndexOf( "main" ) > -1 ) )
            {
                System.Web.UI.WebControls.Image i = ( System.Web.UI.WebControls.Image ) FindControl( "lockMainPhone" );
                if ( i != null ) i.Visible = status;
            }
            else if ( textBox.ID.ToLower().IndexOf( "fax" ) > -1 )
            {
                System.Web.UI.WebControls.Image i = ( System.Web.UI.WebControls.Image ) FindControl( "lockFax" );
                if ( i != null ) i.Visible = status;
            }
            else
            {
                System.Web.UI.WebControls.Image i = ( System.Web.UI.WebControls.Image ) FindControl( "lock" + textBox.ID );
                if ( i != null ) i.Visible = status;
            }

            switch ( status )
            {
                case true:
                    textBox.CssClass = "txtbox_Display";
                    break;
                case false:
                    textBox.CssClass = "";
                    break;
            }
            return;
        }

		/// <summary>
		/// Output description meta tags for this page
		/// </summary>
		/// <param name="text"></param>
		protected void SetMetaTags( string text )
		{
			SetMetaTags( text, "description" );
		}
		/// <summary>
		/// Output meta tags for this page
		/// </summary>
		/// <param name="text"></param>
		/// <param name="type">description (default), keyword </param>
        protected void SetMetaTags( string text, string type )
        {
            HtmlMeta tag = new HtmlMeta();

			if ( type != null && type.ToLower() == "keyword")
				tag.Name = "keyword";
			else
				tag.Name = "description";
            tag.Content = text;
            Page.Header.Controls.Add( tag );

        }//

        #region Phone validation
        /// <summary>
        /// Overload of IsPhoneValid for a custom validator control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void IsPhoneValid( Object sender, ServerValidateEventArgs e )
        {
            e.IsValid = IsPhoneValid( e.Value );
        }

        /// <summary>
        /// Check to make sure the phone is valid (of format ###-###-#### where 1st number is not 0 or 1)
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public bool IsPhoneValid( string phone )
        {
            string choice = UtilityManager.GetAppKeyValue( "usingPhoneNumberClassValidation", "no" );

            if ( choice.Equals( "yes" ) )
            {
                return IsPhoneValid1( phone );
            }
            else
            {
                if ( RegEx.IsMatch( phone, "([2-9]\\d{2})-(\\d{3})-(\\d{4})" ) )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool IsPhoneValid1( string phone )
        {
            bool allowPhonetics = false;
            string statusMessage = "";

            return IsPhoneValid( phone, allowPhonetics, ref statusMessage );

        } //

        /// <summary>
        /// Validate a phone number using PhoneNumber methods
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public static bool IsPhoneValid( string phone, ref string statusMessage )
        {
            bool allowPhonetics = false;
            return IsPhoneValid( phone, allowPhonetics, ref statusMessage );

        } //

        /// <summary>
        /// Validate a phone number using PhoneNumber methods
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="allowPhonetics"></param>
        /// <returns></returns>
        public static bool IsPhoneValid( string phone, bool allowPhonetics, ref string statusMessage )
        {

            if ( phone.Length > 1 )
            {
                try
                {
                    IPB.PhoneNumber phoneNbr = new IPB.PhoneNumber( phone, allowPhonetics );
                    // IsNanpValid does additional checks such as area code cannot start with 0 or 1
                    if ( phoneNbr.IsNanpValid & phoneNbr.CountryCode == 1 )
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch ( FormatException fex )
                {
                    statusMessage = fex.Message;
                    return false;
                }
            }
            else
            {
                return false;
            }
        } //


        /// <summary>
        /// Change the format of the phone # from ###-###-#### to ########## and back.
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="inputPhoneContainsPunctuation"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public string FormatPhone( string phone, bool inputPhoneContainsPunctuation, string status )
        {
            if ( ( phone.Length >= 10 ) )
            {
                if ( ( phone.IndexOf( "-" ) == -1 ) && !inputPhoneContainsPunctuation )
                {
                    status = "OK";
                    return phone.Substring( 0, 3 ) + "-" + phone.Substring( 3, 3 ) + "-" + phone.Substring( 6, 4 );
                }
                else if ( IsPhoneValid( phone ) && inputPhoneContainsPunctuation )
                {
                    status = "OK";
                    IPB.PhoneNumber ph = new IPB.PhoneNumber( phone, false );
                    return ph.ToString( "axs" );
                    //return phone.Substring(0, 3) + phone.Substring(4, 3) + phone.Substring(8, 4);
                }
                else
                {
                    status = "ERROR";
                    return "";
                }
            }
            else
            {
                status = "OK";
                return "";
            }
        }
        #endregion

        /// <summary>
        /// VALIDATE A SSN
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void IsSsnValid( Object sender, ServerValidateEventArgs e )
        {
            e.IsValid = IsSsnValid( e.Value );
        }//

        /// <summary>
        /// VALIDATE A SSN  
        /// - assumes the presence of dash dividers
        /// - doing additional checks: 
        /// 	Reference: http://ssa-custhelp.ssa.gov/cgi-bin/ssa.cfg/php/enduser/std_adp.php?p_faqid=425&p_created=972930021&p_sid=Q4u1z8qj&p_accessibility=0&p_redirect=&p_lva=&p_sp=cF9zcmNoPTEmcF9zb3J0X2J5PSZwX2dyaWRzb3J0PSZwX3Jvd19jbnQ9MTEsMTEmcF9wcm9kcz0mcF9jYXRzPTE2JnBfcHY9JnBfY3Y9MS4xNiZwX3NlYXJjaF90eXBlPWFuc3dlcnMuc2VhcmNoX25sJnBfcGFnZT0xJnBfc2VhcmNoX3RleHQ9dmFsaWQ*&p_li=&p_topview=1
        /// 	* The first 3 digits cannot be “000”, “666” or > “772”.
        /// 	* The middle two digits cannot be “00”.
        ///		* The last 4 digits cannot be “0000”.
        /// </summary>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public bool IsSsnValid( string ssn )
        {
            ssn = ssn.Trim();
            //first check if in display mode where only last 4 digits are shown
            if ( ssn.Length == 11
                 && ssn.Substring( 0, 7 ) == "XXX-XX-"
                 && IsInteger( ssn.Substring( 7, 4 ) ) )
            {
                return true;

            }
            else if ( ssn.Length == 9 && IsInteger( ssn ) )
            {
                int area = Int32.Parse( ssn.Substring( 0, 3 ) );
                int group = Int32.Parse( ssn.Substring( 3, 2 ) );
                int serialNbr = Int32.Parse( ssn.Substring( 5, 4 ) );

                if ( area == 0 || area == 666 || area > 772
                    || group == 0 || serialNbr == 0 )
                    return false;
                else
                    return true;
            }

            if ( RegEx.IsMatch( ssn, "(\\d{3})-(\\d{2})-(\\d{4})" ) )
            {
                //so at this point we have 9 digits char with 2 dashes
                if ( ssn.Length == 11 )
                {
                    int area = Int32.Parse( ssn.Substring( 0, 3 ) );
                    int group = Int32.Parse( ssn.Substring( 4, 2 ) );
                    int serialNbr = Int32.Parse( ssn.Substring( 7, 4 ) );

                    if ( area == 0 || area == 666 || area > 772
                        || group == 0 || serialNbr == 0 )
                        return false;
                    else
                        return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }//

        /// <summary>
        /// Format a SSn
        /// - this is not for validating, assumes previous validation. 
        /// </summary>
        /// <param name="ssn"></param>
        /// <param name="formatingWithPunctuation">If true, will return in common format, otherwise, just the numbers</param>
        /// <returns></returns>
        public string FormatSsn( string ssn, bool formatingWithPunctuation )
        {
            ssn = ssn.Trim();
            ssn = ssn.Replace( "-", "" );

            if ( ( ssn.Length == 9 ) )
            {
                if ( formatingWithPunctuation )
                {
                    return ssn.Substring( 0, 3 ) + "-" + ssn.Substring( 3, 2 ) + "-" + ssn.Substring( 5, 4 );
                }
                else
                {
                    return ssn;
                }
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Validate a date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void IsValidDate( Object sender, ServerValidateEventArgs args )
        {
            bool retVal = true;
            try
            {
                DateTime date = DateTime.Parse( args.Value );
            }
            catch ( FormatException ex )
            {
                retVal = false;
            }
            catch ( Exception ex )
            {
                retVal = false;
            }
            args.IsValid = retVal;
        } //IsValidDate

        /// <summary>
        /// Validate a time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void IsValidTime( Object sender, ServerValidateEventArgs args )
        {
            bool retVal = true;
            try
            {
                DateTime date = DateTime.Parse( args.Value );
            }
            catch ( FormatException ex )
            {
                retVal = false;
            }
            catch ( Exception ex )
            {
                retVal = false;
            }
            args.IsValid = retVal;
        } //IsValidTime


        /// <summary>
        /// Search for a control within a starting control - typically the current Page
        /// </summary>
        /// <param name="startingControl"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        public Control FindChildControl( Control startingControl, string controlId )
        {
            if ( startingControl != null )
            {
                Control foundControl;
                foundControl = startingControl.FindControl( controlId );
                if ( foundControl == null && startingControl.ID == controlId )
                {
                    foundControl = startingControl;
                }
                if ( foundControl != null )
                {
                    return foundControl;
                }
                foreach ( Control c in startingControl.Controls )
                {
                    foundControl = FindChildControl( c, controlId );
                    if ( foundControl != null )
                    {
                        return foundControl;
                    }
                }
            } return null;
        }
        #endregion


        #region Form Helper Property Methods - sync with BaseAppPage, if needed
        /// <summary>
        /// Determine a property value: If a value can be extracted from the placeholder string, use it, otherwise return passed value of property
        /// </summary>
        /// <param name="pageControl"></param>
        /// <param name="propName"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        protected string DetermineProperty( string pageControl, string propName, string property )
        {
            //default to current, in case not in page control
            string value = property == null ? "" : property;

            string text = ContentHelper.ExtractNameValue( pageControl, propName, ";" );
            if ( text != null && text.Trim() != "" )
            {
                value = text;
            }

            return value;
        }
        protected bool DetermineProperty( string pageControl, string propName, bool property )
        {
            bool value = property;

            string text = ContentHelper.ExtractNameValue( pageControl, propName, ";" );
            if ( text != null && text.Trim() != "" )
            {
                value = bool.Parse( text );
            }

            return value;
        }
        protected int DetermineProperty( string pageControl, string propName, int defaultValue, int property )
        {
            int value = property;

            string text = ContentHelper.ExtractNameValue( pageControl, propName, ";" );
            if ( text != null && text.Trim() != "" )
            {
                if ( int.TryParse( text, out value ) == false )
                    value = defaultValue;
            }

            return value;
        }
        #endregion


        #region === HTTP Request Methods ===
        /// <summary>
        /// Retrieve a particular parameter from the HTTP Request querystring.
        /// </summary>
        /// <param name="parameter">Parameter name to return from the Request QueryString</param>
        /// <param name="defaultValue">A default value to return in the event the requested parameter doesn't exist</param>
        /// <returns>The value for the requested parameter or the default value if the parameter was not found</returns>
        public int GetRequestKeyValue( string parameter, int defaultValue )
        {
            //centralizing
            return FormHelper.GetRequestKeyValue( parameter, defaultValue );

        } // end

        /// <summary>
        /// Retrieve a particular parameter from the HTTP Request querystring.
        /// </summary>
        /// <param name="parameter">Parameter name to return from the Request QueryString</param>
        /// <returns>The value for the requested parameter or blank if the parameter was not found</returns>
        public string GetRequestKeyValue( string parameter )
        {

            return GetRequestKeyValue( parameter, "" );

        } // end

        /// <summary>
        /// Retrieve a particular parameter from the HTTP Request querystring.
        /// </summary>
        /// <param name="parameter">Parameter name to return from the Request QueryString</param>
        /// <param name="defaultValue">A default value to return in the event the requested parameter doesn't exist</param>
        /// <returns>The value for the requested parameter or the default value if the parameter was not found</returns>
        public string GetRequestKeyValue( string parameter, string defaultValue )
        {
            //centralizing
            return FormHelper.GetRequestKeyValue( parameter, defaultValue );

        } // end
        public bool GetRequestKeyValue( string parameter, bool defaultValue )
        {
            //centralizing
            return FormHelper.GetRequestKeyValue( parameter, defaultValue );

        } // end
        #endregion

        #region === Session handler methods ===
        /// <summary>
        /// Gets an item from the session as a string.
        /// This method is menat to hide the actual session implementation. In the event that, say SQL Server is used to 
        /// handle session data, then just this method chgs, no application code
        /// </summary>
        /// <remarks>This property is explicitly thread safe.</remarks>
        public string GetSessionItem( string sessionKey )
        {
            string sessionValue = "";

            try
            {
                sessionValue = HttpContext.Current.Session[ sessionKey ].ToString();

            }
            catch
            {
                sessionValue = "";
            }
            finally
            {

            }
            return sessionValue;
        } //
        /// <summary>
        /// Gets an item from the session as a string.
        /// This method is menat to hide the actual session implementation. In the event that, say SQL Server is used to  handle session data, then just this method chgs, no application code
        /// </summary>
        /// <param name="sessionKey">Key name to retrieve from session</param> 
        /// <param name="defaultValue">Default value to use in not found in session</param> 
        /// <remarks>This property is explicitly thread safe.</remarks>
        public string GetSessionItem( string sessionKey, string defaultValue )
        {
            string sessionValue = "";

            try
            {
                sessionValue = HttpContext.Current.Session[ sessionKey ].ToString();
            }
            catch
            {
                sessionValue = defaultValue;
            }

            return sessionValue;

        } //	

        /// <summary>
        /// Gets an item from the session as a string.
        /// This method is menat to hide the actual session implementation. In the event that, say SQL Server is used to  handle session data, then just this method chgs, no application code
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <param name="defaultValue">Default value to use in not found in session</param>
        /// <returns></returns>
        public int GetSessionItem( string sessionKey, int defaultValue )
        {
            int sessionItem = -1;
            string sessionValue = "";

            try
            {
                sessionValue = HttpContext.Current.Session[ sessionKey ].ToString();
                sessionItem = Int32.Parse( sessionValue );

            }
            catch
            {
                sessionItem = defaultValue;
            }
            finally
            {

            }
            return sessionItem;
        } //

        /// <summary>
        /// Assigns a value to a session key
        /// </summary>
        /// <param name="sessionKey">String represnting the name of the session key</param>
        /// <param name="sessionValue">Value to be assigned to the session key</param>
        public void SetSessionItem( string sessionKey, string sessionValue )
        {
            HttpContext.Current.Session[ sessionKey ] = sessionValue;

        } //

        /// <summary>
        /// Use AppUser as a proxy to current definition for a user
        /// </summary>
        public AppUser CurrentUser
        {
            get
            {
                if ( _appUser == null )
                {
                    _appUser = new AppUser();
                }
                return _appUser;
            }
            set
            {
                _appUser = value;
            }
        }//

        public AppUser GetAppUser()
        {
            if ( IsUserAuthenticated() )
            {
                AppUser user =  ( AppUser ) WebUser;
                return user;
            }
            else
            {
                AppUser user = new AppUser();
                return user;
            }
        }//

        /// <summary>
        /// Gets/Sets an instance of a Registered IPB.AppUser object to/from the active user's session.
        /// </summary>
        /// <remarks>This property is explicitly thread safe.</remarks>
        public IWebUser WebUser
        {
            get
            {
                IWebUser user = SessionManager.GetUserFromSession( HttpContext.Current.Session, Constants.USER_REGISTER );

                if ( user == null )
                {
                    //what to do??
                    // redirect to login form - probably timed out!
                    //string returnUrl = "?a=TimeOut&ReturnUrl=" + this.HttpContext.Current.Request.Url.PathAndQuery;
                    //Response.Redirect( Resources.ResourceBroker.GetPath( ( int ) Navigate.LoginPage ) + returnUrl );
                    return null;
                }
                return user;

            }
            set
            {
                SessionManager.SetUserToSession( HttpContext.Current.Session, value );
            }
        }//

        #endregion

        #region Message Handling
        /// <summary>
        /// Handle condition where current user doesn't have access to the requested page/control
        /// </summary>
        /// <param name="rm">ResourceManager</param>
        protected void HandleUnauthorizedUser( ResourceManager formRm )
        {
            bool showPopup = false;
            bool usingSystemConsole = false;
            string unAuthMsgKey = "advisor_unauthorized_access_role";

            if ( UtilityManager.GetAppKeyValue( "showAuthErrorAsPopup", "no" ).Equals( "yes" ) )
                showPopup = true;
            if ( UtilityManager.GetAppKeyValue( "showAuthErrorOnConsole", "yes" ).Equals( "yes" ) )
                usingSystemConsole = true;

            if ( usingSystemConsole )
            {
                string msgString = UtilityManager.GetResourceValue( formRm, unAuthMsgKey, "You do not have access to the requested page." );
                SetConsoleErrorMessage( msgString );
            }
            else
            {
                //format message and redirect
                SetApplicationMessage( formRm, "advisor_unauthorized_access_role", showPopup );
                string redirectURL = "/vos_portal/";
                try
                {
                    redirectURL = HttpContext.Current.Request.UrlReferrer.ToString();
                }
                catch
                {
                    //ignore and use default
                }
                Response.Redirect( redirectURL );
            }

        } //

        /// <summary>
        /// Format an exception and message, and then log it
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="message">Additional message regarding the exception</param>
        public void LogError( Exception ex, string message )
        {

            string user = "";
            string sessionId = HttpContext.Current.Session.SessionID.ToString();

            string parmsString = "";
            string remoteIP = "unknown";
            try
            {
                remoteIP = HttpContext.Current.Request.ServerVariables[ "REMOTE_HOST" ];
            }
            catch
            {
                remoteIP = "unknown";
            }
            string serverName = UtilityManager.GetAppKeyValue( "serverName", "unknown" );
            string path = serverName + HttpContext.Current.Request.Path;
            string queryString = HttpContext.Current.Request.QueryString.ToString();
            string mcmsUrl = GetPublicUrl( queryString );

            if ( mcmsUrl.Length > 0 )
            {
                mcmsUrl = Server.UrlDecode( mcmsUrl );
                if ( mcmsUrl.IndexOf( "?" ) > -1 )
                {
                    parmsString = mcmsUrl.Substring( mcmsUrl.IndexOf( "?" ) + 1 );
                    mcmsUrl = mcmsUrl.Substring( 0, mcmsUrl.IndexOf( "?" ) );
                }
            }
            user = this.GetCurrentUserid();

            try
            {
                string errMsg = message +
                    "\r\nType: " + ex.GetType().ToString() +
                    "\r\nUser: " + user + "_______Session Id - " + sessionId + "____IP - " + remoteIP +
                    "\r\nException:" + ex.Message.ToString() +
                    "\r\nStack Trace:" + ex.StackTrace.ToString() +
                    "\r\nServer\\Template:" + path +
                    "\r\nUrl:" + mcmsUrl;

                if ( parmsString.Length > 0 )
                    errMsg += "\r\nParameters:" + parmsString;

                LoggingHelper.LogError( errMsg );
            }
            catch
            {
                //eat any additional exception
            }

        } //

        public void LogError( string message )
        {

            string user = "";
            string sessionId = HttpContext.Current.Session.SessionID.ToString();

            string parmsString = "";
            string remoteIP = HttpContext.Current.Request.ServerVariables[ "REMOTE_HOST" ];

            string serverName = UtilityManager.GetAppKeyValue( "serverName", HttpContext.Current.Request.ServerVariables[ "LOCAL_ADDR" ] );
            string path = serverName + HttpContext.Current.Request.Path;
            string queryString = HttpContext.Current.Request.QueryString.ToString();
            string mcmsUrl = GetPublicUrl( queryString );

            mcmsUrl = Server.UrlDecode( mcmsUrl );
            if ( mcmsUrl.IndexOf( "?" ) > -1 )
            {
                parmsString = mcmsUrl.Substring( mcmsUrl.IndexOf( "?" ) + 1 );
                mcmsUrl = mcmsUrl.Substring( 0, mcmsUrl.IndexOf( "?" ) );
            }

            user = this.GetCurrentUserid();

            try
            {
                string errMsg = message +
                    "\r\nUser: " + user + "_______Session Id - " + sessionId + "____IP - " + remoteIP +
                    "\r\nServer\\Template:" + path +
                    "\r\nUrl:" + mcmsUrl;

                if ( parmsString.Length > 0 )
                    errMsg += "\r\nParameters:" + parmsString;

                LoggingHelper.LogError( errMsg );
            }
            catch
            {
                //eat any additional exception
            }

        } //

        /// <summary>
        /// Set Application Message using passed resource manager, resource string
        /// OBSOLETE - use SetConsoleErrorMessage instead 
        /// </summary>
        /// <param name="rm">ResourceManager</param>
        /// <param name="resString">Resource string for the message</param>
        /// <param name="showPopup">True to also show message as a popup</param>
        protected void SetApplicationMessage( ResourceManager rm, string resString, bool showPopup )
        {
            //retrieve the resource string
            //08-03-05 mparsons - chgd to use the UtilityManager version to allow a default - where no actual resource exists, the steing itself will be displayed
            string msgString = UtilityManager.GetResourceValue( rm, resString, resString );
            SetApplicationMessage( msgString, showPopup );
        } //

        /// <summary>
        /// Set Application Message using passed message
        /// OBSOLETE - use SetConsoleErrorMessage instead 
        /// </summary>
        /// <param name="msgString">Message to be displayed</param>
        /// <param name="showPopup">True to also show message as a popup</param>
        protected void SetApplicationMessage( string msgString, bool showPopup )
        {
            //assume error style for now
            string css = UtilityManager.GetAppKeyValue( "errorMessageCss", "errorMessage" );

            //SetApplicationMessage( "", msgString, css, showPopup );
            SetConsoleErrorMessage( msgString );

        } //
        /// <summary>
        /// Set Application Message using passed message, uses the infoMessage style
        /// OBSOLETE - use SetConsoleInfoMessage instead 
        /// </summary>
        /// <param name="rm"></param>
        /// <param name="resString"></param>
        /// <param name="showPopup"></param>
        protected void SetApplicationInfoMessage( ResourceManager rm, string resString, bool showPopup )
        {

            //08-03-05 mparsons - chgd to use the UtilityManager version to allow a default - where no actual resource exists, the steing itself will be displayed
            string msgString = UtilityManager.GetResourceValue( rm, resString, resString );

            //assume infoMessage style 
            string css = UtilityManager.GetAppKeyValue( "infoMessageCss", "infoMessage" );
            //SetApplicationMessage( "", msgString, css, showPopup );
            SetConsoleInfoMessage( msgString );

        } //
        /// <summary>
        /// SetApplicationMessage - OBSOLETE DO NOT USE - replace with appropriate Console message calls
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msgString"></param>
        /// <param name="cssClass"></param>
        /// <param name="showPopup"></param>
        private void SetApplicationMessage( string title, string msgString, string cssClass, bool showPopup )
        {
            this.SetSessionItem( SessionManager.APPLICATION_MESSAGE, msgString );
            //
            IPB.FormMessage formMessage = new IPB.FormMessage();
            formMessage.Text = msgString;
            formMessage.Title = title;
            formMessage.CssClass = cssClass;
            formMessage.ShowPopup = showPopup;
            if ( msgString.IndexOf( "&lt;" ) > -1 || msgString.IndexOf( "</" ) > -1 )
            {
                formMessage.IsFormatted = true;
            }

            HttpContext.Current.Session[ SessionManager.FORM_MESSAGE ] = formMessage;

        } //
        /// <summary>
        /// Format javascript to display a message box on next page load
        /// </summary>
        /// <param name="Message">Display message</param>
        public void DisplayMsgBox( string Message )
        {
            System.Web.HttpContext.Current.Response.Write( "<SCRIPT  type='text/javascript' language='javascript'>" );
            System.Web.HttpContext.Current.Response.Write( "alert('" + Message + "')" );
            System.Web.HttpContext.Current.Response.Write( "</SCRIPT>" );
        }
        /// <summary>
        /// Format javascript to display a message box on next page load
        /// </summary>
        /// <param name="title">Title of message</param>
        /// <param name="Message">Display message</param>
        public void DisplayMsgBox( string title, string Message )
        {
            System.Web.HttpContext.Current.Response.Write( "<SCRIPT  type='text/javascript' language='javascript'>" );
            System.Web.HttpContext.Current.Response.Write( "alert('" + title + "\\n\\n" + Message + "')" );
            System.Web.HttpContext.Current.Response.Write( "</SCRIPT>" );
        }

        /// <summary>
        /// Determine if the requested session message exists. if found, format using passed HTML container
        /// </summary>
        /// <param name="sessionMessageName">Set to session variable containing the message</param>
        /// <param name="container">The HTML control that will present the message, if present</param>
        protected void GetSessionMessage( string sessionMessageName, HtmlGenericControl container )
        {
            string pTag = "<p>";

            IPB.FormMessage message = new IPB.FormMessage();
            message = ( IPB.FormMessage ) HttpContext.Current.Session[ sessionMessageName ];
            //message = ( IPB.FormMessage ) HttpContext.Current.Session[ SessionManager.SYSTEM_CONSOLE_MESSAGE ];
            if ( message != null )
            {
                if ( message.ShowPopup )
                {
                    this.DisplayMsgBox( message.Title, message.Text );
                }
                if ( message.CssClass.Length > 0 )
                    pTag = "<p class='" + message.CssClass + "'>";

                if ( message.IsFormatted )
                {
                    if ( message.Title.Length > 0 )
                        container.InnerHtml += message.Title;

                    if ( message.Text.StartsWith( "<p" ) )
                        container.InnerHtml += message.Text;
                    else
                        container.InnerHtml += pTag + message.Text + "</p>";
                }
                else
                {
                    if ( message.Title.Length > 0 )
                        container.InnerHtml += "<h2>" + message.Title + "</h2>";

                    container.InnerHtml += pTag + message.Text + "</p>";
                }
                container.Visible = true;

                //now clear out message
                HttpContext.Current.Session.Remove( sessionMessageName );

            }
            else
            {
                //just in case, reset
                if ( container.InnerHtml.Length < 10 )
                    container.InnerHtml = "";
            }

        }//


        /// <summary>
        /// Set a system Console Error Message
        /// </summary>
        public void SetConsoleErrorMessage( string message )
        {
            SetSessionMessage( SessionManager.SYSTEM_CONSOLE_MESSAGE, "", message, "errorMessage", false );
        }

        /// <summary>
        /// Set a system Console success Message
        /// </summary>
        public void SetConsoleSuccessMessage( string message )
        {
            SetSessionMessage( SessionManager.SYSTEM_CONSOLE_MESSAGE, "", message, "successMessage", false );
        }

        /// <summary>
        /// Set a system Console info Message
        /// </summary>
        public void SetConsoleInfoMessage( string message )
        {
            SetSessionMessage( SessionManager.SYSTEM_CONSOLE_MESSAGE, "", message, "infoMessage", false );
        }
        /// <summary>
        /// store a system message in the passed session variable using the FormMessage class
        /// </summary>
        /// <param name="sessionMessageName">Name of the session variable</param>
        /// <param name="title">Optional title</param>
        /// <param name="msgString">Message</param>
        /// <param name="cssClass">CSS Class</param>
        /// <param name="showPopup">True also display a popup dialog</param>
        public void SetSessionMessage( string sessionMessageName, string title, string msgString, string cssClass, bool showPopup )
        {

            //
            IPB.FormMessage formMessage = new IPB.FormMessage();
            formMessage.Text = msgString;
            formMessage.Title = title;
            formMessage.CssClass = cssClass;
            formMessage.ShowPopup = showPopup;
            if ( msgString.IndexOf( "&lt;" ) > -1 || msgString.IndexOf( "</" ) > -1 )
            {
                formMessage.IsFormatted = true;
            }

            try
            {
                //need to handle case where multiple messages are written to console??
                if ( HttpContext.Current.Session[ sessionMessageName ] != null )
                {
                    IPB.FormMessage existingMessage = ( IPB.FormMessage ) HttpContext.Current.Session[ sessionMessageName ];
                    if ( existingMessage.Text.Trim().Length > 0 )
                    {
                        if ( existingMessage.Title.Length > 0 )
                            formMessage.Text += "<br/>" + existingMessage.Title;
                        if ( existingMessage.Text.Length > 0 )
                            formMessage.Text += "<br/>" + existingMessage.Text;
                    }
                }
                HttpContext.Current.Session[ sessionMessageName ] = formMessage;
            }
            catch ( Exception ex )
            {
                //ignore
                this.LogError( ex, thisBaseClassName + ".SetSessionMessage: " + ex.ToString() );
            }


        } //
        #endregion

        #region Link Formatting
        protected string GetInternalLink()
        {
            //get internal link snippet using default from web.config (or other way around??)
            string link = UtilityManager.GetInternalLink();
            return link;
        } //
        protected string GetExternalLink()
        {
            //get internal link snippet using default from web.config (or other way around??)
            string link = UtilityManager.GetExternalLink();
            return link;
        } //
        #endregion


        #region === Miscellaneous helper methods: defaults, IsDatatype, etc. ===
        /// <summary>
        /// Returns passed string as an integer, if is an integer and not null/empty. 
        /// Otherwise returns the passed default value
        /// </summary>
        /// <param name="stringToTest"></param>
        /// <param name="defaultValue"></param>
        /// <returns>The string parameter as an int or the default value if the parameter is not a vlid integer</returns>
        public int AssignWithDefault( string stringToTest, int defaultValue )
        {
            int newVal;

            try
            {
                if ( IsInteger( stringToTest ) && stringToTest.Length > 0 )
                {
                    newVal = int.Parse( stringToTest );
                }
                else
                {
                    newVal = defaultValue;
                }
            }
            catch
            {

                newVal = defaultValue;
            }

            return newVal;

        } //
        /// <summary>
        /// Checks passed string, if not nullthen returns the passed string. 
        ///	Otherwise returns the passed default value
        /// </summary>
        /// <param name="stringToTest"></param>
        /// <param name="defaultValue"></param> 
        /// <returns>int</returns>
        public string AssignWithDefault( string stringToTest, string defaultValue )
        {
            string newVal;

            try
            {
                if ( stringToTest == null )
                {
                    newVal = defaultValue;
                }
                else
                {
                    newVal = stringToTest;
                }
            }
            catch
            {

                newVal = defaultValue;
            }

            return newVal;

        } //

        /// <summary>
        /// CurrencyToDecimal - handle assignment of a string containing formatted currency to a decimal
        /// </summary>
        /// <param name="strValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public decimal CurrencyToDecimal( string strValue, decimal defaultValue )
        {

            decimal decimalAmt = 0;

            try
            {
                if ( strValue == "" )
                {
                    decimalAmt = defaultValue;
                }
                else
                {
                    //remove leading $
                    string amount = strValue.Replace( "$", "" );
                    decimalAmt = decimal.Parse( amount );
                }
            }
            catch
            {

                decimalAmt = defaultValue;
            }

            return decimalAmt;

        } //
        /// <summary>
        /// IsInteger - test if passed string is an integer
        /// </summary>
        /// <param name="stringToTest"></param>
        /// <returns></returns>
        public bool IsInteger( string stringToTest )
        {
            int newVal;
            bool result = false;
            try
            {
                newVal = Int32.Parse( stringToTest );

                // If we get here, then number is an integer
                result = true;
            }
            catch
            {

                result = false;
            }
            return result;

        }

        /// <summary>
        /// IsNumeric - test if passed string is numeric
        /// </summary>
        /// <param name="stringToTest"></param>
        /// <returns></returns>
        public bool IsNumeric( string stringToTest )
        {
            double newVal;
            bool result = false;
            try
            {
                result = double.TryParse( stringToTest, NumberStyles.Any,
                    NumberFormatInfo.InvariantInfo, out newVal );
            }
            catch
            {

                result = false;
            }
            return result;

        }


        /// <summary>
        /// IsDate - test if passed string is a valid date
        /// </summary>
        /// <param name="stringToTest"></param>
        /// <returns></returns>
        public bool IsDate( string stringToTest )
        {

            DateTime newDate;
            bool result = false;
            try
            {
                newDate = System.DateTime.Parse( stringToTest );
                result = true;
            }
            catch
            {

                result = false;
            }
            return result;

        } //end

        /// <summary>
        /// Check is dataset is valid and has at least one table with at least one row
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public bool DoesDataSetHaveRows( DataSet ds )
        {

            try
            {
                if ( ds != null && ds.Tables.Count > 0 && ds.Tables[ 0 ].Rows.Count > 0 )
                    return true;
                else
                    return false;
            }
            catch
            {

                return false;
            }
        }//

        #endregion

        #region e-mail

        /// <summary>
        /// Validate e-mail actually exists ==> slow and not effective
        /// </summary>
        private bool DoesEmailExist( string address )
        {
            string statusMessage = "";
            return DoesEmailExist( address, ref statusMessage );

        } //

        private bool DoesEmailExist( string address, ref string statusMessage )
        {
            try
            {
                //string[] host = ( address.Split( '@' ) );
                //string hostname = host[ 1 ];

                //IPHostEntry IPhst = Dns.Resolve( hostname );
                //IPEndPoint endPt = new IPEndPoint( IPhst.AddressList[ 0 ], 25 );
                //Socket s = new Socket( endPt.AddressFamily,
                //                         SocketType.Stream, ProtocolType.Tcp );
                //s.Connect( endPt );

                ////Attempting to connect
                //if ( !Check_Response( s, SMTPResponse.CONNECT_SUCCESS ) )
                //{
                //    s.Close();
                //    statusMessage = "Failed unable to connect!";
                //    return false;
                //}

                ////HELLO server
                //Senddata( s, string.Format( "HELLO {0}\r\n", Dns.GetHostName() ) );
                //if ( !Check_Response( s, SMTPResponse.GENERIC_SUCCESS ) )
                //{
                //    s.Close();
                //    statusMessage = "Failed on send to host!";
                //    return false;
                //}

                ////Identify yourself
                ////Servers may resolve your domain and check whether 
                ////you are listed in BlackLists etc.
                //string emailTo = UtilityManager.GetAppKeyValue( "systemAdminEmail", "mparsons@illinoisworknet.com" );
                //Senddata( s, string.Format( "MAIL From: {0}\r\n", emailTo ) );
                //if ( !Check_Response( s, SMTPResponse.GENERIC_SUCCESS ) )
                //{
                //    s.Close();
                //    statusMessage = "Failed on send of test email!";
                //    return false;
                //}

                ////Attempt Delivery (I can use VRFY, but most 
                ////SMTP servers only disable it for security reasons)
                //Senddata( s, address );
                //if ( !Check_Response( s, SMTPResponse.GENERIC_SUCCESS ) )
                //{
                //    s.Close();
                //    //maybe this should be skipped if inconsistant
                //    statusMessage = "Failed on a delivery attempt.";
                //    return false;
                //}
            }
            catch ( System.Net.Sockets.SocketException ex )
            {
                //check if test env - can't check from latter
                statusMessage = ex.Message.ToString();
                if ( IsTestEnv() )
                {
                    statusMessage += " Note: doesn't work in a localhost environment!";
                }
                return false;
            }
            catch ( Exception ex )
            {
                statusMessage = ex.Message.ToString();
                return false;
            }
            return ( true );

        } //
        private static void Senddata( Socket s, string msg )
        {

            byte[] _msg = Encoding.ASCII.GetBytes( msg );
            s.Send( _msg, 0, _msg.Length, SocketFlags.None );
        }
        private static bool Check_Response( Socket s, SMTPResponse response_expected )
        {
            string sResponse;
            int response;
            byte[] bytes = new byte[ 1024 ];
            while ( s.Available == 0 )
            {
                System.Threading.Thread.Sleep( 100 );
            }

            s.Receive( bytes, 0, s.Available, SocketFlags.None );
            sResponse = Encoding.ASCII.GetString( bytes );
            response = Convert.ToInt32( sResponse.Substring( 0, 3 ) );
            if ( response != ( int ) response_expected )
                return false;
            return true;
        }
        #endregion
        #region Tracing/debugging
        /// <summary>
        /// IsTestEnv - determines if the current environment is a testing/development 
        /// </summary>
        /// <returns>True if localhost - implies testing</returns>
        public bool IsTestEnv()
        {
            string host = HttpContext.Current.Request.Url.Host.ToString();

            if ( host.ToLower() == "localhost" )
                return true;
            else
                return false;

        } //


        #endregion

        #region Report Helper Methods
        protected void AddColumn(GridView gv, string dataField, string headerText)
        {
            AddColumn(gv, dataField, headerText, false, true);
        }

        protected void AddColumn(GridView gv, string dataField, string headerText, bool rightJustify, bool htmlEncode)
        {
            BoundField boundField = new BoundField
            {
                DataField = dataField,
                HeaderText = headerText,
                HtmlEncode = htmlEncode
            };
            if (rightJustify)
            {
                boundField.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
            }
            gv.Columns.Add(boundField);
        }

        protected void InitReport(string fileName)
        {
            // Make sure the filename ends in .csv.  If it doesn't, then add it on.
            Regex endFileName = new Regex(@".csv$");
            if (endFileName.Matches(fileName).Count == 0)
            {
                fileName += ".csv";
            }

            // Set up the report
            Response.Clear();
            string dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            fileName = string.Format(fileName, dateTime);
            Response.ContentType = string.Format("text/csv; name=\"{0}\"", fileName);
            Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}", fileName));
            //Response.Charset = "";
            Response.ContentEncoding = Encoding.Unicode;
        }


        protected string GetWriteableValue(object o, string name, StringBuilder headerRow, bool isWritingHeader)
        {
            return GetWriteableValue(o, name, headerRow, isWritingHeader, true);
        }

        protected string GetWriteableValue(object o, string name, StringBuilder headerRow, bool isWritingHeader, bool isCleaningHtml)
        {
            if (isWritingHeader)
            {
                headerRow.Append(name + ",");
            }
            if (o == null || o == Convert.DBNull)
            {
                return ",";
            }
            else if (o.ToString().IndexOf(",") == -1)
            {
                return FixupString(o.ToString(), isCleaningHtml) + ",";
            }
            else
            {
                return "\"" + FixupString(o.ToString(), isCleaningHtml) + "\",";
            }
        }


        private string FixupString(string input, bool isCleaningHtml)
        {
            string retVal = input;
            if (isCleaningHtml)
            {
                Regex htmlRegex = new Regex(@"\<[^\>]*\>");
                retVal = htmlRegex.Replace(retVal, " ");
            }
            retVal = retVal.Replace("\"", "\"\"");
            retVal = retVal.Replace("\n", " ");
            retVal = retVal.Replace("\r", " ");

            return retVal;
        }

        public void EndReport()
        {
            Response.End();
        }

        #endregion

    } //class
}//namespace
