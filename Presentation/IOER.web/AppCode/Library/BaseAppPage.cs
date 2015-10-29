using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
//using System.Net.Sockets;
//using System.Net;
using System.Reflection;
using System.Resources;
using System.Text;
//using System.Web.Mail;
using RegEx = System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using ILPathways.Utilities;
using IOER.classes;
using EmailHelper = ILPathways.Utilities.EmailManager;
using IPB = ILPathways.Business;
using AppUser = LRWarehouse.Business.Patron; //ILPathways.Business.AppUser;


namespace IOER.Library
{
    /// <summary>
    /// Summary description for BaseAppPage
    /// </summary>
    public class BaseAppPage : System.Web.UI.Page
    {
        private const string thisBaseClassName = "BaseAppPage";
        protected AppUser _appUser = new AppUser();

        protected string mCurrentUrl = "";
        ResourceManager formRm;
        #region Properties

        /// <summary>
        /// INTERNAL PROPERTY: FormPrivileges
        /// Set initially and store in ViewState
        /// </summary>
        protected IPB.ApplicationRolePrivilege FormPrivileges
        {
            get {
                if (ViewState["formPrivileges"] == null)
                    ViewState["formPrivileges"] = new IPB.ApplicationRolePrivilege();

                return ViewState[ "formPrivileges" ] as IPB.ApplicationRolePrivilege; 
            }
            set { ViewState[ "formPrivileges" ] = value; }
        }

        /// <summary>
        /// INTERNAL PROPERTY: RecordPrivileges
        /// Used where privileges are checked and assigned at a record level rather than only at the control level
        /// </summary>
        public IPB.ApplicationRolePrivilege RecordPrivileges
        {
            get
            {
                if (ViewState["recordPrivileges"] == null)
                    ViewState["recordPrivileges"] = new IPB.ApplicationRolePrivilege();

                return ViewState["recordPrivileges"] as IPB.ApplicationRolePrivilege;
            }
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
        protected bool CanCreate
        {
            get { return Boolean.Parse( ViewState[ "CanCreate" ].ToString() ); }
            set { ViewState[ "CanCreate" ] = value; }
        }

        /// <summary>
        /// INTERNAL PROPERTY: CanUpdate - is current user allowed to create a record
        /// </summary>
        protected bool CanUpdate
        {
            get { return Boolean.Parse( ViewState[ "CanUpdate" ].ToString() ); }
            set { ViewState[ "CanUpdate" ] = value; }
        }


        private string _metaKeywords;
        private string _metaDescription;
        /// <summary>
        /// Gets or sets the Meta Keywords tag for the page
        /// </summary>
        public string PageMetaKeywords
        {
            get
            {
                return _metaKeywords;
            }
            set
            {
                // strip out any excessive white-space, newlines and linefeeds
                _metaKeywords = RegEx.Regex.Replace( value, "\\s+", " " );
            }
        }

        /// <summary>
        /// Gets or sets the Meta Description tag for the page
        /// </summary>
        public string PageMetaDescription
        {
            get
            {
                return _metaDescription;
            }
            set
            {
                // strip out any excessive white-space, newlines and linefeeds
                _metaDescription = RegEx.Regex.Replace( value, "\\s+", " " );
            }
        }
        //private bool m_bskipThisPage;
        //public bool SkippingThisPage
        //{
        //  get { return m_bskipThisPage; }
        //  set { m_bskipThisPage = value; }
        //}
        #endregion

        /// <summary>
        /// Add an event handler to Init event for the control
        /// so we can execute code when a server control (page)
        /// that inherits from this base class is initialized.
        /// </summary>
        public BaseAppPage()
        {
            Init += new EventHandler( BasePage_Init );
        }


        /// <summary>
        /// Whenever a page that uses this base class is initialized
        /// add meta keywords and descriptions if available
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BasePage_Init( object sender, EventArgs e )
        {

            if ( !String.IsNullOrEmpty( PageMetaKeywords ) )
            {
                HtmlMeta tag = new HtmlMeta();
                tag.Name = "keywords";
                tag.Content = PageMetaKeywords;
                Header.Controls.Add( tag );
            }

            if ( !String.IsNullOrEmpty( PageMetaDescription ) )
            {
                HtmlMeta tag = new HtmlMeta();
                tag.Name = "description";
                tag.Content = PageMetaDescription;
                Header.Controls.Add( tag );
            }
        }

        #region Methods that are cloned in the BaseUserControl
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


        protected void LogVisit()
        {
            LogVisit( "" );
        }
        protected void LogVisit( string comment )
        {
            LogVisit( comment, "title" );
        }

        /// <summary>
        /// Log the visit to this page
        /// </summary>
        /// <remarks>Remember to keep in sync with the baseHeader.LogVisit and BaseUserControl.LogVisit method</remarks>
        protected void LogVisit( string comment, string titlePlaceholder )
        {
            string userid = "";
            string mcmsUrl = "";
            string parmsString = "";

            try
            {
                string sessionId = Session.SessionID.ToString();
                string template = Request.Path;
                string remoteIP = Request.ServerVariables[ "REMOTE_HOST" ];

                //08-03-21 mparsons - server name is now added separating in the LogManager method
                string serverName = "";	//UtilityManager.GetAppKeyValue( "serverName", Request.ServerVariables[ "LOCAL_ADDR" ] );
                string pageTitle = GetPageTitle( titlePlaceholder );
                if ( pageTitle.ToLower() == "unknown" )
                {
                    if ( comment.Length > 0 )
                        pageTitle = comment;
                }

                string path = serverName + template;
                if ( FormHelper.IsValidRequestString() == false )
                {
                    this.LogError( thisBaseClassName + ".LogVisit: Potential invalid request string encountered!" );
                }

                if ( Request.QueryString.Count > 0 )
                {
                    mcmsUrl = GetPublicUrl( Request.QueryString.ToString() );

                    mcmsUrl = Server.UrlDecode( mcmsUrl );
                }

                if ( comment.ToLower().Equals( "externallink" ) || comment.ToLower().Equals( "landingpage" ) )
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
                // is this a partner?


                //serid = this.GetCurrentUserid();

                LoggingHelper.LogPageVisit( sessionId, path, mcmsUrl, parmsString, Page.IsPostBack, userid, "", comment, remoteIP, "n/a" );

            }
            catch ( System.Threading.ThreadAbortException abex )
            {
                //ignore this one, probably due to a redirect

            }
            catch ( Exception ex )
            {
                //ignore errors
                this.LogError( ex, "isle.Library.BaseAppPage.LogVisit: " + ex.ToString() );
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
            //string defaultTitle = "";
            //string separator = "";

            pageTitle = ""; //or default to ???

            try
            {


                //string siteSectionName = UtilityManager.getPathType();
                //if ( siteSectionName == "residents" )
                //{
                //    siteSectionName = "Job Seeker";
                //}
                //else
                //{
                //    siteSectionName = "Business";
                //}


                //string siteHomePath = UtilityManager.GetAppKeyValue( "mcmsRootChannel", "vos_portal" );
                //string usingChannel = UtilityManager.GetAppKeyValue( "pageTitle.UseChannel", "no" );

                //string cp = CmsHttpContext.Current.Channel.Path.ToString();

                //string fullPath = cmsContext.Channel.Path;

                //if ( usingChannel.ToLower() == "yes" )
                //{

                //    parentChannel = UtilityManager.getPathChannel();

                //    pageTitle = defaultTitle + parentChannel;
                //    separator = " - ";
                //}

                //string title = McmsHelper.GetPlaceHolder( posting, placeholderTitleName );
                //if ( title.Length > 0 )
                //{
                //    pageTitle += separator + title;
                //}
            }
            catch ( System.NullReferenceException nex )
            {
                //ignore - probably no placeholder

            }
            catch ( Exception ex )
            {
                //if there is no placeholder or any other error, leave with default title
                this.LogError( ex, "BaseAppPage.GetPageTitle exception" );
            }
            if ( pageTitle.Length == 0 )
                pageTitle = "Unknown";

            return pageTitle;


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
        /// Determine if current user is a guest, a registered user or a content manager user
        /// </summary>
        /// <returns></returns>
        
        /// <summary>
        /// Determine if current user is a logged in (registered) user 
        /// </summary>
        /// <returns></returns>
        protected bool IsUserAuthenticated()
        {
            bool isUserAuthenticated = false;

            try
            {
                IPB.IWebUser appUser = SessionManager.GetUserFromSession( Session, Constants.USER_REGISTER );
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

        protected string GetPathwayPrefix()
        {
            string resPrefix = "";
            string sectionName = UtilityManager.getPathType();
            if ( sectionName == "residents" )
            {
                resPrefix = "";
            }
            else if ( sectionName == "advisors" )
            {
                resPrefix = "advisors";
            }
            else
            {
                resPrefix = "biz";
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


        #region Form Helper Methods - sync with BaseUserControl
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
                case "no":
                    value = false;
                    break;
            }
            return value;
        }

        /// <summary>
        /// LoadPageControl - loads a user control into the passed container
        /// </summary>
        /// <param name="userControl">Path to user control being loaded</param>
        /// <param name="container">HtmlTableCell to hold the control</param>
        /// <param name="controlID">Name to assign to the control</param>
        protected void LoadPageControl( string userControl, HtmlTableCell container, string controlID )
        {

            System.Web.UI.Control mainControl = new System.Web.UI.Control();

            try
            {
                mainControl = Page.LoadControl( userControl );
                mainControl.ID = controlID;

                container.Controls.Add( mainControl );

            }
            catch
            {
                throw;
            }

        }	// end 
        /// <summary>
        /// LoadPageControl - loads a user control into the passed container
        /// </summary>
        /// <param name="userControl">Path to user control being loaded</param>
        /// <param name="container">PlaceHolder to hold the control</param>
        /// <param name="controlID">Name to assign to the control</param>
        protected void LoadPageControl( string userControl, PlaceHolder container, string controlID )
        {

            System.Web.UI.Control mainControl = new System.Web.UI.Control();

            try
            {
                mainControl = Page.LoadControl( userControl );
                mainControl.ID = controlID;

                container.Controls.Add( mainControl );

            }
            catch
            {
                throw;
            }

        }	// end 
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
                if ( Request.QueryString[ keyName ] != null )
                {
                    selection = Request.QueryString[ keyName ];
                }
                else if ( Session[ keyName ] != null )
                {
                    selection = ( string ) Session[ keyName ].ToString();
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
        /// Helper method to get the value for a parameter that may come from the request querystring or the session.
        /// </summary>
        /// <param name="keyName">Key name used to insepct Request and session</param>
        /// <param name="defaultValue">Default value to use of key is not found</param>
        /// <returns>Appropriate value or the default</returns>
        protected int GetDefaultParmValue( string keyName, int defaultValue )
        {

            int selection;
            try
            {
                if ( Request.QueryString[ keyName ] != null )
                {
                    selection = Convert.ToInt32( Request.QueryString[ keyName ] );
                }
                else if ( Session[ keyName ] != null )
                {
                    selection = Convert.ToInt32( Session[ keyName ].ToString() );
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
        /// Common method to set the selected value in a list using a key name. The key name is used to first do a look up on the request query string, then the session.
        /// </summary>
        /// <param name="list">A DropDownList</param>
        /// <param name="keyName">Key name to use when inspecting the request and session objects, if not one of the latter then assume searching for its value</param>
        protected void SetListSelection( DropDownList list, string keyName )
        {

            string selection = "";
            if ( Request.QueryString[ keyName ] != null )
            {
                selection = Request.QueryString[ keyName ];
            }
            else if ( Session[ keyName ] != null )
            {
                selection = Session[ keyName ].ToString();
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
                else if ( list.Items.FindByText( selection.ToString() ) != null )
                {
                    list.SelectedIndex = -1;
                    list.Items.FindByText( selection.ToString() ).Selected = true;
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

            string selection = "";
            if ( Request.QueryString[ keyName ] != null )
            {
                selection = Request.QueryString[ keyName ];
            }
            else if ( Session[ keyName ] != null )
            {
                selection = Session[ keyName ].ToString();
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

            string selection = "";
            if ( Request.QueryString[ keyName ] != null )
            {
                selection = Request.QueryString[ keyName ];
            }
            else if ( Session[ keyName ] != null )
            {
                selection = Session[ keyName ].ToString();
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

        /// <summary>
        /// Remove the passed tag from the HTML string
        /// </summary>
        /// <param name="text">HTML text from placeholder</param>
        /// <param name="tag">Tag to be removed</param>
        /// <param name="endtag">End tag that matches tag to be removed</param>
        /// <param name="recursecompare">Do a recursive compare?</param>
        /// <param name="deleteallcontent">True means delete all content from tag</param>
        /// <returns></returns>
        protected string RemoveTag( string text, string tag, string endtag, string recursecompare, bool deleteallcontent )
        {
            int start = text.ToLower().IndexOf( tag.ToLower() );
            int startEndOfTag = -1;
            int end = -1;
            string completeTag = "";

            while ( start >= 0 )
            {
                startEndOfTag = text.ToLower().IndexOf( ">", start );
                if ( startEndOfTag != -1 )
                    completeTag = text.Substring( start, startEndOfTag - start + 1 );
                else
                    completeTag = tag;

                end = FindTag( text, endtag, start, recursecompare );

                if ( deleteallcontent == false )
                {
                    if ( end > start )
                        text = text.Remove( end, endtag.Length );

                    text = text.Remove( start, completeTag.Length );
                    start = text.ToLower().IndexOf( tag.ToLower() );
                }
                else
                {
                    if ( end > start )
                        text = text.Remove( start, ( end - start ) + endtag.Length );
                    else
                        text = text.Remove( start, completeTag.Length );

                    start = text.ToLower().IndexOf( tag.ToLower() );
                }
            } //end while
            return text;
        }//
        /// <summary>
        /// Find a tag in the HTML text
        /// </summary>
        /// <param name="text">input text</param>
        /// <param name="tag">Html tag</param>
        /// <param name="start">starting postion for search</param>
        /// <param name="recursecompare"></param>
        /// <returns></returns>
        private int FindTag( string text, string tag, int start, string recursecompare )
        {
            int pos = text.ToLower().IndexOf( tag.ToLower(), start );
            int check1 = text.ToLower().IndexOf( recursecompare.ToLower() + " ", start + 1 );
            int check2 = text.ToLower().IndexOf( recursecompare.ToLower() + ">", start + 1 );
            int check;
            if ( check1 == -1 && check2 == -1 )
                check = -1;
            else if ( check1 == -1 && check2 != -1 )
                check = check2;
            else if ( check1 != -1 && check2 == -1 )
                check = check1;
            else
                check = ( check1 < check2 ) ? check1 : check2;

            if ( ( check >= 0 ) & ( check < pos ) )
            {
                check = FindTag( text, tag, check, recursecompare );
                pos = text.ToLower().IndexOf( tag.ToLower(), check + 1 );
            }
            return pos;
        }//

        /// <summary>
        ///	Loops through the controls on the page and adds attibutes to the help hyperlinks.	
        /// </summary>
        /// <remarks>Created on 06-10-05 by twright</remarks>
        /// <remarks>ASP:Hyperlinks must have an id with a prefix of 'contextHelp'</remarks>
        protected void FormatHelpLinksStyle( Control parent )
        {
            try
            {
                foreach ( Control c in parent.Controls )
                {
                    //LoggingHelper.DoTrace( 9, " @@@FormatHelpLinksStyle. Control Type: " + c.GetType().ToString() );
                    if ( c.GetType().ToString().Equals( "System.Web.UI.WebControls.HyperLink" ) )
                    {
                        if ( GetControlId( c ).Length > 10 )
                        {
                            if ( c.ID.Substring( 0, 11 ) == "contextHelp" )
                            {
                                ( ( HyperLink ) c ).Attributes.Add( "onBlur", "ChangeColor(this.id,'Off')" );
                                ( ( HyperLink ) c ).Attributes.Add( "onFocus", "ChangeColor(this.id,'On')" );
                                ( ( HyperLink ) c ).Attributes.Add( "style", "border-width:0px;border-style:Solid;" );
                            }
                        }
                    }
                    else
                    {
                        //check for nested containers and do recursive loop
                        if ( c.Controls.Count > 0 )
                        {
                            FormatHelpLinksStyle( c );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                //report error and continue
                this.LogError( ex, "BaseAppPage.FormatHelpLinksStyle exception encountered" );
            }
        }//
        private string GetControlId( Control c )
        {
            string controlId = "";
            try
            {
                controlId = c.ID.ToString();
            }
            catch
            {
                controlId = "";
            }
            return controlId;
        }//
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


        /// <summary>
        /// Retrieve a particular parameter from the HTTP Request querystring.
        /// </summary>
        /// <param name="parameter">Parameter name to return from the Request QueryString</param>
        /// <returns>The value for the requested parameter or blank if the parameter was not found</returns>
        public string GetRequestFormValue( string parameter )
        {
            return GetRequestFormValue( parameter, "" );

        } // end

        public string GetRequestFormValue( string parameter, string defaultValue )
        {
            string request = Request.Form.Get( parameter );

            if ( request == null )
            {
                return defaultValue;
            }
            else
            {
                if ( request.IndexOf( "';" ) > -1 )
                {
                    request = request.Substring( 0, request.IndexOf( "';" ) );
                }
                if ( request.IndexOf( ";" ) > -1 )
                {
                    request = request.Substring( 0, request.IndexOf( ";" ) );
                }
                return request;
            }

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
                sessionValue = Session[ sessionKey ].ToString();

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
                sessionValue = Session[ sessionKey ].ToString();
            }
            catch
            {
                sessionValue = defaultValue;
            }

            return sessionValue;

        } //	
        /// <summary>
        /// Assigns a value to a session key
        /// </summary>
        /// <param name="sessionKey">String represnting the name of the session key</param>
        /// <param name="sessionValue">Value to be assigned to the session key</param>
        public void SetSessionItem( string sessionKey, string sessionValue )
        {
            Session[ sessionKey ] = sessionValue;

        } //
        public string GetSessionId()
        {
            string id = "";

            try
            {
                id = Session.SessionID.ToString();

            }
            catch
            {
                id = "";
            }
            finally
            {

            }
            return id;
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
                AppUser user = ( AppUser ) WebUser;
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
        protected IPB.IWebUser WebUser
        {
            get
            {
                IPB.IWebUser user = SessionManager.GetUserFromSession( Session, Constants.USER_REGISTER );

                if ( user == null )
                {
                    //what to do??
                    // redirect to login form - probably timed out!
                    //string returnUrl = "?a=TimeOut&ReturnUrl=" + this.Request.Url.PathAndQuery;
                    //Response.Redirect( Resources.ResourceBroker.GetPath( ( int ) Navigate.LoginPage ) + returnUrl );
                    return null;
                }
                return user;

            }
            set
            {
                SessionManager.SetUserToSession( Session, value );
            }
        }//
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
        #endregion
        #region === Miscellaneous helper methods: defaults, IsDatatype, etc. ===
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


        #endregion

        #region === Application Keys Methods ===
        //***Defined for a generic process for handling form messages
        //Dependent AppKeys:
        // path.AppBasePath - base path (ex /StartHere

        /// <summary>
        /// Gets the value of the application base path from web.config. Returns blanks if not found.
        /// AppKey = "path.AppBasePath"
        /// </summary>
        /// <remarks>This property is explicitly thread safe.</remarks>
        public string GetBasePath()
        {
            string basePath = UtilityManager.GetAppKeyValue( "app.BasePath" );

            return basePath;
        } //

        #endregion

        #region Application Message Handling - BEING MADE OBSOLETE

        /// <summary>
        /// Set Application Message using passed resource manager, resource string and placeholder 
        ///  - OBSOLETE DO NOT USE - replace with appropriate Console message calls
        /// </summary>
        /// <param name="rm">ResourceManager</param>
        /// <param name="resString">Resource string for the message</param>
        /// <param name="msgPlaceholder">PlaceHolder where message control will be loaded</param>
        protected void SetApplicationMessage( ResourceManager rm, string resString, PlaceHolder msgPlaceholder )
        {
            //08-03-05 mparsons - chgd to use the UtilityManager version to allow a default - where no actual resource exists, the steing itself will be displayed
            string msgString = UtilityManager.GetResourceValue( rm, resString, resString );
            SetApplicationMessage( msgString, msgPlaceholder );
        } //

        /// <summary>
        /// Set Application Message using passed resource manager, resource string and placeholder 
        ///  - OBSOLETE DO NOT USE - replace with appropriate Console message calls
        /// </summary>
        /// <param name="msgString">Message to be displayed</param>
        /// <param name="msgPlaceholder">PlaceHolder where message control will be loaded</param>
        protected void SetApplicationMessage( string msgString, PlaceHolder msgPlaceholder )
        {

            //this.SetSessionItem( SessionManager.APPLICATION_MESSAGE, msgString );
            //
            //assume error style for now
            string css = UtilityManager.GetAppKeyValue( "errorMessageCss", "errorMessage" );

            IPB.FormMessage formMessage = new IPB.FormMessage();
            formMessage.Text = msgString;
            formMessage.Title = "Application Error";
            formMessage.CssClass = css;
            formMessage.ShowPopup = false;
            if ( msgString.IndexOf( "&lt;" ) > -1 || msgString.IndexOf( "</" ) > -1 )
            {
                formMessage.IsFormatted = true;
            }

            Session[ SessionManager.FORM_MESSAGE ] = formMessage;
            //
            //msgPlaceholder.Controls.Add( LoadControl( "/vos_portal/advisor/Controls/AppMessage.ascx" ) );

        } //
        /// <summary>
        /// Set Application Message using passed resource manager, and resource string 
        /// /// OBSOLETE - use SetConsoleErrorMessage instead 
        /// </summary>
        /// <param name="rm">ResourceManager</param>
        /// <param name="msgString">Resource string for the message</param>
        /// <param name="showPopup">True to also show message as a popup</param>
        protected void SetApplicationMessage( ResourceManager rm, string resString, bool showPopup )
        {

            //08-03-05 mparsons - chgd to use the UtilityManager version to allow a default - where no actual resource exists, the stRing itself will be displayed
            string msgString = UtilityManager.GetResourceValue( rm, resString, resString );

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

            //08-03-05 mparsons - chgd to use the UtilityManager version to allow a default - where no actual resource exists, the string itself will be displayed
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
            //this.SetSessionItem( SessionManager.APPLICATION_MESSAGE, msgString );
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

            Session[ SessionManager.FORM_MESSAGE ] = formMessage;

        } //

        /// <summary>
        /// Determine if an application message needs to be formatted. if so format using passed HTML container
        /// </summary>
        /// <param name="usingFormMessage">Set to Yes to use the FormMessage entity</param>
        /// <param name="container">The HTML control that will present the message, if present</param>
        protected void HandleAppMessage( string usingFormMessage, HtmlGenericControl container )
        {
            //bool usingFormMessage = true;
            if ( usingFormMessage == "yes" )
            {
                IPB.FormMessage message = new IPB.FormMessage();
                message = ( IPB.FormMessage ) Session[ SessionManager.FORM_MESSAGE ];
                if ( message != null )
                {

                    if ( message.ShowPopup )
                    {
                        //this.DisplayMsgBox( message.Title, message.Text );
                    }

                    //if (message.CssClass.Length > 0)
                    //  container. = message.CssClass;

                    if ( message.IsFormatted )
                    {
                        if ( message.Title.Length > 0 )
                            container.InnerHtml += message.Title;

                        if ( message.Text.StartsWith( "<p" ) )
                            container.InnerHtml += message.Text;
                        else
                            container.InnerHtml += "<p>" + message.Text + "</p>";
                    }
                    else
                    {
                        if ( message.Title.Length > 0 )
                            container.InnerHtml += "<h2>" + message.Title + "</h2>";

                        container.InnerHtml += "<p>" + message.Text + "</p>";
                    }
                    container.Visible = true;

                    //now clear out message
                    Session.Remove( SessionManager.FORM_MESSAGE );

                }
                else
                {
                    //just in case, reset
                    if ( container.InnerHtml.Length < 10 )
                        container.InnerHtml = "";
                }
            }
            else
            {
                string appMessage = this.GetSessionItem( SessionManager.APPLICATION_MESSAGE, "" );

                if ( appMessage.Length > 0 )
                {
                    container.InnerHtml = appMessage;
                    container.Visible = true;
                    //now clear out message
                    Session.Remove( SessionManager.APPLICATION_MESSAGE );
                }
                else
                {
                    //just in case, reset
                    container.InnerHtml = "";
                }

            }

        }//

        protected void HandleAppMessage( string usingFormMessage, Label container )
        {
            //bool usingFormMessage = true;
            if ( usingFormMessage == "yes" )
            {
                IPB.FormMessage message = new IPB.FormMessage();
                message = ( IPB.FormMessage ) Session[ SessionManager.FORM_MESSAGE ];
                if ( message != null )
                {

                    if ( message.ShowPopup )
                    {
                        this.DisplayMsgBox( message.Title, message.Text );
                    }

                    if ( message.CssClass.Length > 0 )
                        container.CssClass = message.CssClass;

                    if ( message.IsFormatted )
                    {
                        if ( message.Title.Length > 0 )
                            container.Text += message.Title;

                        if ( message.Text.StartsWith( "<p" ) )
                            container.Text += message.Text;
                        else
                            container.Text += "<p>" + message.Text + "</p>";
                    }
                    else
                    {
                        if ( message.Title.Length > 0 )
                            container.Text += "<h2>" + message.Title + "</h2>";

                        container.Text += "<p>" + message.Text + "</p>";
                    }
                    container.Visible = true;

                    //now clear out message
                    Session.Remove( SessionManager.FORM_MESSAGE );

                }
                else
                {
                    //just in case, reset
                    if ( container.Text.Length < 10 )
                        container.Text = "";
                }
            }
            else
            {
                string appMessage = this.GetSessionItem( SessionManager.APPLICATION_MESSAGE, "" );

                if ( appMessage.Length > 0 )
                {
                    container.Text = appMessage;
                    container.Visible = true;
                    //now clear out message
                    Session.Remove( SessionManager.APPLICATION_MESSAGE );
                }
                else
                {
                    //just in case, reset
                    if ( container.Text.Length < 10 )
                        container.Text = "";
                }

            }

        }//


        #endregion

        #region Message and Error Handling

        /// <summary>
        /// Format an exception and message, and then log it
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="message">Additional message regarding the exception</param>
        public void LogError( Exception ex, string message )
        {

            string user = "";
            string sessionId = Session.SessionID.ToString();

            string parmsString = "";
            string remoteIP = Request.ServerVariables[ "REMOTE_HOST" ];

            string serverName = UtilityManager.GetAppKeyValue( "serverName", Request.ServerVariables[ "LOCAL_ADDR" ] );
            string path = serverName + Request.Path;
            string queryString = Request.QueryString.ToString();
            string mcmsUrl = GetPublicUrl( queryString );

            mcmsUrl = Server.UrlDecode( mcmsUrl );
            if ( mcmsUrl.IndexOf( "?" ) > -1 )
            {
                parmsString = mcmsUrl.Substring( mcmsUrl.IndexOf( "?" ) + 1 );
                mcmsUrl = mcmsUrl.Substring( 0, mcmsUrl.IndexOf( "?" ) );
            }

            //user = this.GetCurrentUserid();

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
            string sessionId = Session.SessionID.ToString();

            string parmsString = "";
            string remoteIP = Request.ServerVariables[ "REMOTE_HOST" ];

            string serverName = UtilityManager.GetAppKeyValue( "serverName", Request.ServerVariables[ "LOCAL_ADDR" ] );
            string path = serverName + Request.Path;
            string queryString = Request.QueryString.ToString();
            string mcmsUrl = GetPublicUrl( queryString );

            mcmsUrl = Server.UrlDecode( mcmsUrl );
            if ( mcmsUrl.IndexOf( "?" ) > -1 )
            {
                parmsString = mcmsUrl.Substring( mcmsUrl.IndexOf( "?" ) + 1 );
                mcmsUrl = mcmsUrl.Substring( 0, mcmsUrl.IndexOf( "?" ) );
            }

            //user = this.GetCurrentUserid();

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
        /// Format javascript to display a message box on next page load
        /// </summary>
        /// <param name="Message">Display message</param>
        public void DisplayMsgBox( string message )
        {
            string newMessage = UnformatHtml( message );

            System.Web.HttpContext.Current.Response.Write( "<SCRIPT  type='text/javascript' language='javascript'>" );
            System.Web.HttpContext.Current.Response.Write( "alert('" + newMessage + "')" );
            System.Web.HttpContext.Current.Response.Write( "</SCRIPT>" );
        }
        /// <summary>
        /// Format javascript to display a message box on next page load
        /// </summary>
        /// <param name="title">Title of message</param>
        /// <param name="Message">Display message</param>
        public void DisplayMsgBox( string title, string message )
        {
            if ( title.Length == 0 )
                title = "Note";
            string newMessage = UnformatHtml( message );
            string newtitle = UnformatHtml( title );

            System.Web.HttpContext.Current.Response.Write( "<SCRIPT  type='text/javascript' language='javascript'>" );
            System.Web.HttpContext.Current.Response.Write( "alert('" + newtitle + "\\n\\n" + newMessage + "')" );
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
            message = ( IPB.FormMessage ) Session[ sessionMessageName ];
            //message = ( IPB.FormMessage ) Session[ SessionManager.SYSTEM_CONSOLE_MESSAGE ];
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
                Session.Remove( sessionMessageName );

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
        protected void SetSessionMessage( string sessionMessageName, string title, string msgString, string cssClass, bool showPopup )
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
                if ( Session[ sessionMessageName ] != null )
                {
                    IPB.FormMessage existingMessage = ( IPB.FormMessage ) Session[ sessionMessageName ];
                    if ( existingMessage.Text.Trim().Length > 0 )
                    {
                        if ( existingMessage.Title.Length > 0 )
                            formMessage.Text += "<br/>" + existingMessage.Title;
                        if ( existingMessage.Text.Length > 0 )
                            formMessage.Text += "<br/>" + existingMessage.Text;
                    }
                }
            }
            catch
            {
                //ignore
            }
            Session[ sessionMessageName ] = formMessage;

        } //

        public string UnformatHtml( string message )
        {
            string newMessage;
            newMessage = message.Replace( "<p>", "\\n" );
            newMessage = newMessage.Replace( "</p>", "" );
            newMessage = newMessage.Replace( "<br>", "\\n" );
            newMessage = newMessage.Replace( "<br/>", "\\n" );
            newMessage = newMessage.Replace( "<br />", "\\n" );
            newMessage = newMessage.Replace( "<h2>", "" );
            newMessage = newMessage.Replace( "</h2>", "\\n" );

            return newMessage;
        }
        #endregion

        #region Tracing/debugging
        /// <summary>
        /// IsTestEnv - determines if the current environment is a testing/development 
        /// </summary>
        /// <returns>True if localhost - implies testing</returns>
        protected bool IsTestEnv()
        {
            string host = Request.Url.Host.ToString();

            if ( host.ToLower() == "localhost" )
                return true;
            else
                return false;

        } //
        #endregion



    } //class
}//namespace
