using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Isle.BizServices;
using MyManager = Isle.BizServices.AccountServices; 
using CurrentUser = LRWarehouse.Business.Patron; //ILPathways.Business.AppUser;
using ILPathways.Utilities;
using ILPathways.Controllers;

using LRWarehouse.DAL;
using LRWarehouse.Business;
using ILPathways.Business;
using ILPathways.Library;
using ILPathways.classes;


namespace ILPathways.Account.controls
{
    public partial class LoginController : BaseUserControl //System.Web.UI.UserControl
    {
        public string errorMessage = "";
        MyManager myManager = new MyManager();
        CurrentUser currentUser = new CurrentUser();

        public string lockedUserId = "initial";
        //
        #region Properties
        public string ReturnURL
        {
            get { return this.txtReturnUrl.Text; }
            set { this.txtReturnUrl.Text = value; }
        }
        public DateTime LastLoginLockDate
        {
            get 
            {
                DateTime lastLockDate = new DateTime( 2000, 1, 1 );
                if ( Session[ lockedUserId + "_LastLoginLockDate" ] != null )
                {
                    lastLockDate = DateTime.Parse( Session[ lockedUserId + "_LastLoginLockDate" ].ToString() );
                }
                return lastLockDate; 
            }
            set { Session[ lockedUserId + "_LastLoginLockDate" ] = value; }
        }
        public int LoginAttempts
        {
            get {
                int attempts = 0;
                if ( Session[ lockedUserId + "_LoginAttempts" ] != null )
                {
                    Int32.TryParse( Session[ lockedUserId + "_LoginAttempts" ].ToString(), out attempts );
                }
                return attempts; 
            }
            set { Session[ lockedUserId + "_LoginAttempts" ] = value.ToString(); }
        }
        #endregion

        protected void Page_Load( object sender, EventArgs e )
        {
 
            //litLinkedInLogin.Text = "true";
            loginErrorMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                InitializeForm();
                //check for quick login
			    HandleQuickLogin();
            }
        }
        private void InitializeForm()
        {
            //LoggingHelper.DoTrace( 4, "LoginController.InitializeForm" );
            //Check for Redirect URL and Saving in a Session 
            string rURL = "";
            int post = HttpContext.Current.Request.QueryString.ToString().IndexOf("nextUrl");
            //if ( FormHelper.GetRequestKeyValue( "nextUrl" ) != "" )
            if ( post > -1 )
            {
                rURL = HttpContext.Current.Request.QueryString.ToString().Substring(post+8);

                //need to handle getting all parms with the nextUrl
                //rURL = FormHelper.GetRequestKeyValue( "nextUrl" );
                //if ( rURL.ToLower().IndexOf( "?" ) > -1 )
                //    rURL = rURL.Substring( 0, rURL.IndexOf( "?" ) );

                if ( rURL.ToLower().IndexOf( "login.aspx" ) > -1 )
                    rURL = "/";

                rURL = HttpUtility.UrlDecode( rURL );
                rURL = UtilityManager.FormatAbsoluteUrl( rURL, false );
                Session[ "redirectURL" ] = rURL;
                ReturnURL = rURL;
            }
            else
            {
                ReturnURL = FormHelper.GetRequestKeyValue( "ReturnURL" );
                if ( ReturnURL == "" )
                {
                    //if not login.aspx, return to current ==> necessary?, will there anyway
                    if ( Request.RawUrl.ToLower().IndexOf( "login.aspx" ) < 0 )
                        ReturnURL = Request.Path + "?" + Request.QueryString;
                    else
                        ReturnURL = "/";
                }
            }
            //force nextUrl to be http:
            if (forcingNextUrlAsHttp.Text == "yes" 
             && ReturnURL.ToLower().IndexOf("profile.aspx") == -1)
                ReturnURL = UtilityManager.FormatAbsoluteUrl( ReturnURL, false);
        }//

		/// <summary>
		/// Handle Quick login - check if credentials were supplied in the url. 
		/// As well check for other conditions include:
		/// - Logon-as another user
		/// - Referer check
		/// </summary>
		/// <remarks>
		///</remarks>
		private void HandleQuickLogin()
		{
            bool isProxy = false;
            CurrentUser user = new Patron();
            string statusMessage = "";

            string rowId = this.GetRequestKeyValue( "g" );
            if ( rowId == "" )
            {
                //check for proxy guid
                rowId = this.GetRequestKeyValue( "pg" );
                if ( rowId.Length == 36 )
                {
                    isProxy = true;
                    if ( IsUserAuthenticated() )
                    {
                        //if alrady auth, check if same user
                        bool isProxyValid = false;
                        int userId = new AccountServices().GetUserIdFromProxy( rowId, true, ref isProxyValid, ref statusMessage );
                        if ( userId == WebUser.Id )
                        {
                            //same user, skip auth, and do redirect
                            //no message for now
                            Response.Redirect( ReturnURL );
                        }
                        else
                        {
                            //diff user
                            // - as check was (doingCheckOnly = true) a check only, the proxy is left as is, so the following code can handle as needed
                        }
                    }
                }
            }

            if ( rowId != "" )
            {

                string action = this.GetRequestKeyValue( "a" );
                try
                {

                    if ( isProxy )
                    {
                        user = myManager.GetByProxyRowId( rowId, ref statusMessage );

                    }
                    else
                    {
                        user = myManager.GetByRowId( rowId );
                    }

                    if ( user != null && user.Id > 0 )
                    {
                        ActivityBizServices.UserAutoAuthentication( user );
                        //add proxy
                        user.ProxyId = new AccountServices().Create_SessionProxyLoginId( user.Id, ref statusMessage );

                        FormsAuthentication.SetAuthCookie( user.UserName, false );

                        if ( action == "activate" )
                        {
                            ConfirmRegistration( user );

                            //if no return url, should direct to the profile, getting started!
                            if ( ReturnURL.Length < 5 )
                            {
                                ReturnURL = UtilityManager.FormatAbsoluteUrl( "/Account/Profile.aspx", false );
                            }
                            SetConsoleSuccessMessage( "Your IOER account has been confirmed!<br/> Welcome to our system. Visit the <a href=\"/Help/Guide.aspx\">User Guide</a> for tips on getting started. " );

                        }
                        else if ( action == "autoactivate" )
                        {
                            ConfirmRegistration( user );

                            //if no return url, should direct to the profile, getting started!
                            if ( ReturnURL.Length < 5 )
                            {
                                ReturnURL = UtilityManager.FormatAbsoluteUrl( "/Account/Profile.aspx", false );
                            }
                            //asumes prior process handles the success message!
                            //SetConsoleSuccessMessage( "Your IOER account has been confirmed!<br/> Welcome to our system. Visit the <a href=\"/Help/Guide.aspx\">User Guide</a> for tips on getting started. " );

                        }
                        else
                        {
                            //just in case check if active
                            //hmm, need to ensure a user who was purposefully set inactive, cannot just recover password!
                            if ( user.IsActive == false 
                                && user.Created == user.LastUpdated)
                            {
                                //a new user will not have a profile, and lastupdated will equal created date
                                user.IsActive = true;
                                myManager.Update( user );
                                //log
                                ActivityBizServices.SiteActivityAdd( "Account","Activated via auto-login",string.Format("User {0} probably used password recovery for an non-activated account", user.FullName()), user.Id, 0,0 );
                            }
                        }

                        this.WebUser = user;
                        Response.Redirect( ReturnURL );
                    }
                    else
                    {
                        if ( statusMessage.Length > 0 )
                        {
                            SetConsoleErrorMessage( statusMessage );
                        }
                        else
                        {
                            //user not found, no message necessary, don't want to encourage fiddling
                        }
                    }
                }
                catch ( ThreadAbortException taex )
                {
                    //Ignore this, it is caused by a redirect.  The redirect will go OK anyway.
                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, "Login.HandleQuickLogin exception:" );
                }
            } 
        }//

        protected void ConfirmRegistration( CurrentUser user )
        {
            user.IsActive = true;
            myManager.Update( user );
            //check profile, may have to create?
            if ( autoCreateLibraryOnActivate.Text == "yes" )
            {
                string statusMessage = "";
                new LibraryBizService().CreateDefaultLibrary( user, ref statusMessage );
                SetConsoleSuccessMessage( libraryCreateMsg.Text );
            }
            ActivityBizServices.UserRegistrationConfirmation( user );
            //do check whether can auto associate a user with an org based on email domain
            OrganizationBizService.AssociateUserWithOrg( user );
	    }//


        protected void loginWorknetButton_Click( object sender, EventArgs e )
        {
            //Login via WorkNet and complete account details if necessary
            UserController myUserController = new UserController();
            CurrentUser wnUser = myUserController.LoginViaWorkNet( txtUserName.Text, txtPassword.Text );
            if ( wnUser.IsValid )
            {
                currentUser = myManager.GetByWorkNetId( wnUser.worknetId );
                //OR - future
                //currentUser = myManager.GetByWorkNetCredentials( wnUser.Username, wnUser.RowId.ToString() );
                if ( currentUser != null && currentUser.Id > 0 )
                {
                    SessionManager.SetUserToSession( Session, currentUser );

                    //Then the account is linked, so, go to the return URL
                    Response.Redirect( ReturnURL ); 
                }
                else
                {
                    //Direct the user to register the account.
                    //??need something in session to populate
                    //should an account just be created, and allow user to change all, including userId?
                    Session.Add( "workNetPatron", wnUser );
                    Response.Redirect( UtilityManager.FormatAbsoluteUrl( "/Account/Register.aspx", false ) );
                }

            }
            else
            {
                errorMessage = "Error: Invalid User Name or Password.";
                loginErrorMessage.Visible = true;
            }
        }

        protected void loginPathwaysButton_Click( object sender, EventArgs e )
        {
            int maxAttempts = UtilityManager.GetAppKeyValue( "NUMBER_OF_LOGIN_ATTEMPTS_ALLOWED", 5 );
            int lockOutMinutes = UtilityManager.GetAppKeyValue( "LOCKOUT_TIMESPAN_MINUTES", 10 );

            string statusMessage = "";
            string userName = txtUserName.Text.Trim();
            string pw = txtPassword.Text.Trim();
            lockedUserId = userName;
            if ( LoginAttempts > maxAttempts) 
            {
                if ( DateTime.Now.Subtract( LastLoginLockDate ).Minutes < lockOutMinutes )
                {
                    SetConsoleErrorMessage( string.Format("You have exceeded the allowed number of login attempts for this account. The account will be locked for {0} minutes", lockOutMinutes) );
                    return;
                }
                else
                {
                    LoginAttempts = 0;
                    Session.Remove( lockedUserId + "_LastLoginLockDate" );
                }
            }
            LoginAttempts ++;
            //Login locally
            currentUser = myManager.Authorize( userName, UtilityManager.Encrypt( pw ), ref statusMessage );
            if ( currentUser.IsValid )
            {
                SessionManager.SetUserToSession( Session, currentUser );
                ActivityBizServices.UserAuthentication( currentUser );

                FormsAuthentication.SetAuthCookie( userName, false );

                LoginAttempts = 0;
                Session.Remove( lockedUserId + "_LastLoginLockDate" );
                Session.Remove( lockedUserId + "_LoginAttempts" );

                LoggingHelper.DoTrace( 2, "@@@@@@ Login successfull, redirecting to: " + ReturnURL );
                Response.Redirect( ReturnURL );
            }
            else
            {
                errorMessage = "Error: Invalid User name or Password."; //currentUser.Message;
                loginErrorMessage.Visible = true;
                if ( LoginAttempts >= maxAttempts )
                {
                    LastLoginLockDate = DateTime.Now;
                }
            }
        }

        protected void loginLinkedInButton_Click( object sender, EventArgs e )
        {
            //SetConsoleSuccessMessage( "LinkedIn ID: " + hdnLinkedInID.Value );
            //litLinkedInLogin.Text = "false";
        }

    }// End class
}