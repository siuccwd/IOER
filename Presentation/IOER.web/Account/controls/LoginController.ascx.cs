using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using MyManager = Isle.BizServices.AccountServices; 
using CurrentUser = LRWarehouse.Business.Patron; //ILPathways.Business.AppUser;
using ILPathways.Utilities;
using ILPathways.Controllers;

using LRWarehouse.DAL;
using LRWarehouse.Business;

using ILPathways.Library;
using ILPathways.classes;
using Isle.BizServices;

namespace ILPathways.Account.controls
{
    public partial class LoginController : BaseUserControl //System.Web.UI.UserControl
    {
        public string errorMessage = "";
        MyManager myManager = new MyManager();
        CurrentUser currentUser = new CurrentUser();
        
        //
        #region Properties
        public string ReturnURL
        {
            get { return this.txtReturnUrl.Text; }
            set { this.txtReturnUrl.Text = value; }
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
            //Check for Redirect URL and Saving in a Session 
            string rURL = "";
            if ( FormHelper.GetRequestKeyValue( "nextUrl" ) != "" )
            {
                rURL = FormHelper.GetRequestKeyValue( "nextUrl" );
                rURL = HttpUtility.UrlDecode( rURL );
                rURL = UtilityManager.FormatAbsoluteUrl(rURL, false);
                Session[ "redirectURL" ] = rURL;
                ReturnURL = rURL;
            }
            else
            {
                ReturnURL = FormHelper.GetRequestKeyValue( "ReturnURL" );
                if ( ReturnURL == "" )
                {
                    //if not login.aspx, return to current ==> necessary?, will there anyway
                    if (Request.RawUrl.ToLower().IndexOf("login.aspx") < 0)
                        ReturnURL = Request.Path + "?" + Request.QueryString;
                    else
                        ReturnURL = "/";
                }
            }
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
            string rowId = this.GetRequestKeyValue( "g" );

            if ( rowId != "" )
            {
                string action = this.GetRequestKeyValue( "a" );
                try
                {
                    CurrentUser user = myManager.GetByRowId( rowId );
                    if ( user != null && user.IsValid )
                    {
                        ActivityBizServices.UserAutoAuthentication( user );

                        if ( action == "activate" )
                        {
                            ConfirmRegistration( user );
                            
                            //if no return url, should direct to the profile, getting started!
                            if ( ReturnURL.Length < 5 )
                            {
                                ReturnURL = UtilityManager.FormatAbsoluteUrl("/Account/Profile.aspx", false);
                            }
                            SetConsoleSuccessMessage( "Your IOER account has been confirmed!<br/> Welcome to our system. Visit the <a href=\"/Help/Guide.aspx\">User Guide</a> for tips on getting started. " );

                        }
                        else if ( action == "autoactivate" )
                        {
                            ConfirmRegistration( user );
                            
                            //if no return url, should direct to the profile, getting started!
                            if ( ReturnURL.Length < 5 )
                            {
                                ReturnURL = UtilityManager.FormatAbsoluteUrl("/Account/Profile.aspx", false);
                            }
                            //asumes prior process handles the success message!
                            //SetConsoleSuccessMessage( "Your IOER account has been confirmed!<br/> Welcome to our system. Visit the <a href=\"/Help/Guide.aspx\">User Guide</a> for tips on getting started. " );

                        }

                        this.WebUser = user;
                        Response.Redirect( ReturnURL );
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
            //check profile, may have to create

            ActivityBizServices.UserRegistrationConfirmation( user );
                   
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
                    Response.Redirect( UtilityManager.FormatAbsoluteUrl("/Account/Register.aspx", false) );    
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
            string statusMessage = "";
            string userName = txtUserName.Text.Trim();
            string pw = txtPassword.Text.Trim();
            //Login locally
            currentUser = myManager.Authorize( userName, UtilityManager.Encrypt( pw ), ref statusMessage );
            if ( currentUser.IsValid )
            {
                SessionManager.SetUserToSession( Session, currentUser );
                ActivityBizServices.UserAuthentication( currentUser );
                Response.Redirect( ReturnURL );
            }
            else
            {
                errorMessage = "Error: Invalid User name or Password."; //currentUser.Message;
                loginErrorMessage.Visible = true;
            }
        }

        protected void loginLinkedInButton_Click( object sender, EventArgs e )
        {
            //SetConsoleSuccessMessage( "LinkedIn ID: " + hdnLinkedInID.Value );
            //litLinkedInLogin.Text = "false";
        }

    }// End class
}