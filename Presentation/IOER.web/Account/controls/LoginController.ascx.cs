using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Isle.BizServices;
using MyManager = Isle.BizServices.AccountServices;
using CurrentUser = LRWarehouse.Business.Patron; //ILPathways.Business.AppUser;
using ILPathways.Utilities;
using IOER.Controllers;

using LRWarehouse.DAL;
using LRWarehouse.Business;
using ILPathways.Business;
using IOER.Library;
using IOER.classes;


namespace IOER.Account.controls
{
    public partial class LoginController : BaseUserControl //System.Web.UI.UserControl
    {
        public string errorMessage = "";
        public string ssoLoginUrl
        {
            get
            {
                string url = UtilityManager.GetAppKeyValue("ssoLoginUrl", "");
                string nextUrl = FormHelper.GetRequestKeyValue("nextUrl", "");
                if (nextUrl != null && nextUrl != string.Empty)
                {
                    url += "?nextUrl=" + nextUrl;
                }
                return url;
            }
        }
        MyManager myManager = new MyManager();
        CurrentUser currentUser = new CurrentUser();
        OrganizationBizService orgMgr = new OrganizationBizService();

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

            if ( post > -1 )
            {
                rURL = HttpContext.Current.Request.QueryString.ToString().Substring(post+8);

                //need to handle getting all parms with the nextUrl

                if ( rURL.ToLower().IndexOf( "login.aspx" ) > -1 )
                    rURL = "/";

                rURL = HttpUtility.UrlDecode( rURL );

                //string nextUrl = FormHelper.GetRequestKeyValue("nextUrl2");
                //if (nextUrl.Length > 0 && rURL.IndexOf("nextUrl2") == -1)
                //{
                //    if (rURL.IndexOf("?") == -1)
                //        rURL += "?nextUrl=" + nextUrl;
                //    else
                //        rURL += "&nextUrl=" + nextUrl;
                //}

                //check if there are additional parameters, without a ?. If so, convert first one to a ?
                if (rURL.IndexOf("?") == -1 && rURL.IndexOf("&") > -1)
                {
                    var regex = new Regex(Regex.Escape("&"));
                    rURL = regex.Replace(rURL, "?", 1);
                }

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
			currentUser = new Patron();
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
                        //if already auth, check if same user
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
						currentUser = myManager.GetByProxyRowId( rowId, ref statusMessage );

                    }
                    else
                    {
                        currentUser = myManager.GetByRowId( rowId );
                    }

                    if ( currentUser != null && currentUser.Id > 0 )
                    {
                        ActivityBizServices.UserAutoAuthentication( currentUser );
						InitializeUserSession( currentUser );

                        if ( action == "activate" )
                        {
                            ConfirmRegistration( currentUser );

                            //if no return url, should direct to the profile, getting started!
                            if ( ReturnURL.Length < 5 )
                            {
                                ReturnURL = UtilityManager.FormatAbsoluteUrl( "/Account/Profile.aspx", false );
                            }
                            SetConsoleSuccessMessage( "Your IOER account has been confirmed!<br/> Welcome to our system. Visit the <a href=\"/Help/Guide.aspx\">User Guide</a> for tips on getting started. " );

                        }
                        else if ( action == "autoactivate" )
                        {
                            ConfirmRegistration( currentUser );

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
                            if ( currentUser.IsActive == false 
                                && currentUser.Created == currentUser.LastUpdated)
                            {
                                //a new user will not have a profile, and lastupdated will equal created date
                                currentUser.IsActive = true;
                                myManager.Update( currentUser );
                                //log
                                ActivityBizServices.SiteActivityAdd( "Account","Activated via auto-login",string.Format("User {0} probably used password recovery for an non-activated account", currentUser.FullName()), currentUser.Id, 0,0 );
                            }
                        }

                        this.WebUser = currentUser;
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
			if ( user.IsActive == false )
			{
				user.IsActive = true;
				myManager.Update( user );
			}

            //check profile, may have to create?
            if ( autoCreateLibraryOnActivate.Text == "yes" )
            {
                string statusMessage = "";
                new LibraryBizService().CreateDefaultLibrary( user, ref statusMessage );
                SetConsoleSuccessMessage( libraryCreateMsg.Text );
            }
            ActivityBizServices.UserRegistrationConfirmation( user );
            //do check whether can auto associate a user with an org based on email domain
            orgMgr.AssociateUserWithOrg(user);

			AutoAddToOrgChecks(user);
	    }//


		protected void AutoAddToOrgChecks(CurrentUser user)
		{
			try
			{
				//check for temp auto associations
				if (autoAddToOrg.Text.Length > 0)
				{
					int orgId = 0;
					//future could have multiples with semi-colon separated groups
					string[] addOptions = autoAddToOrg.Text.Split(',');
					if (addOptions.Length == 1)
					{
						//assume orgId
						if (Int32.TryParse(addOptions[0], out orgId))
						{
							orgMgr.AssociateUserWithOrg(user, orgId);
						}
					}
					else if (addOptions.Length == 2)
					{
						//assume date, orgId
						DateTime activeDate = new DateTime(2015, 9, 1);
						string date = addOptions[0];
						if (DateTime.TryParse(date, out activeDate)) ;
						if (Int32.TryParse(addOptions[1], out orgId))
						{
							if (activeDate.ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"))
								orgMgr.AssociateUserWithOrg(user, orgId);
						}
					}
					else
					{
						//ignore as not handled yet
					}
				}
			}
			catch (Exception ex)
			{
				//just ignonre for now
			}
	    }//


        protected void loginWorknetButton_Click( object sender, EventArgs e )
        {
            //Login via WorkNet and complete account details if necessary

			CurrentUser wnUser = myManager.LoginViaWorkNet(txtUserName.Text, txtPassword.Text);
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
				InitializeUserSession( currentUser );

				DoSpecialChecks();
				//
				DoPasscodeConfirmationCheck(pw);

                LoginAttempts = 0;
                Session.Remove( lockedUserId + "_LastLoginLockDate" );
                Session.Remove( lockedUserId + "_LoginAttempts" );

                LoggingHelper.DoTrace( 5, "@@@@@@ Login successfull, redirecting to: " + ReturnURL );
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

		protected void InitializeUserSession(Patron user)
		{
			string statusMessage = "";
			//add proxy
			currentUser.ProxyId = new AccountServices().Create_SessionProxyLoginId( currentUser.Id, ref statusMessage );

			if ( new AccountServices().IsUserAdmin( currentUser ) )
				currentUser.TopAuthorization = 2;

			SessionManager.SetUserToSession( Session, currentUser );
			ActivityBizServices.UserAuthentication( currentUser );

			FormsAuthentication.SetAuthCookie( currentUser.Email, false );

        }

		protected void DoSpecialChecks()
		{
			//check for New User, or Pending User
			if (currentUser.LastName == "User"
				&& ( currentUser.FirstName == "New" || currentUser.FirstName == "Pending" ) )
			{
				SetConsoleSuccessMessage( "NOTE: Be sure to update your profile, and change your password to something secure." );
				//arbitrarily force user to profile
				//if ( ReturnURL.ToLower().IndexOf( "/default.aspx" ) > -1 )
					ReturnURL = "/Account/Profile.aspx";
			}
        }

		protected void DoPasscodeConfirmationCheck(string password)
		{
			try
			{
				if ( passCodeConfirmation.Text.Length > 0 && passCodeConfirmation.Text == password )
				{
					if ( passCodeConfirmationDate.Text == DateTime.Now.ToString( "yyyy-MM-dd" ) )
					{
						///fake out confirmation to get library, etc
						//need to ensure only done once!
						//check profile, may have to create?
						if ( autoCreateLibraryOnActivate.Text == "yes" )
						{
							LibraryBizService mgr = new LibraryBizService();
							string statusMessage = "";
							//get will create lib if doesn't exist
							ILPathways.Business.Library lib = mgr.GetMyLibrary( currentUser, true );
							//mgr.CreateDefaultLibrary( currentUser, ref statusMessage );
							SetConsoleSuccessMessage( libraryCreateMsg.Text );
						}
						ActivityBizServices.UserRegistrationConfirmation( currentUser );
						//do check whether can auto associate a user with an org based on email domain
						orgMgr.AssociateUserWithOrg( currentUser );

						SetConsoleSuccessMessage( "NOTE: Be sure to update your profile, and change your password to something secure." );
						//if return is just to the home, set to the profile page
						if ( ReturnURL.ToLower().IndexOf( "/default.aspx" ) > -1 )
							ReturnURL = "/Account/Profile.aspx";
					}
				}
			}
			catch ( Exception ex )
			{

			}
        }

        protected void loginLinkedInButton_Click( object sender, EventArgs e )
        {
            //SetConsoleSuccessMessage( "LinkedIn ID: " + hdnLinkedInID.Value );
            //litLinkedInLogin.Text = "false";
        }

    }// End class
}