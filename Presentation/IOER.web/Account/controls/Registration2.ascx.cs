using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using LRWarehouse.Business;
using AppUser = LRWarehouse.Business.Patron; // LRWarehouse.Business.Patron; //ILPathways.Business.AppUser;
using IOER.Library;
using ILPathways.Utilities;
using ILPathways.Business;
using Isle.BizServices;
using OrgMgr = Isle.BizServices.OrganizationBizService;

namespace IOER.Account.controls
{
  public partial class Registration2 : BaseUserControl
  {
    Services.UtilityService utilService = new Services.UtilityService();
    OrganizationBizService orgMgr = new OrganizationBizService();

    #region Properties
    public string ReturnURL
    {
        get { return this.txtReturnUrl.Text; }
        set { this.txtReturnUrl.Text = value; }
    }


    /// <summary>
    /// get/set current Invitation
    /// </summary>
    public LibraryInvitation Invitation
    {
        get
        {
            if ( ViewState[ "Invitation" ] == null )
                ViewState[ "Invitation" ] = new LibraryInvitation();
            return ViewState[ "Invitation" ] as LibraryInvitation;
        }
        set { ViewState[ "Invitation" ] = value; }
    }
    #endregion
    protected void Page_Load( object sender, EventArgs e )
    {
        if ( !Page.IsPostBack )
        {
            DoInitialization();
        }
    }

    private void DoInitialization( )
    {
        string inviteId = FormHelper.GetRequestKeyValue( "invite" );

        //Check for Redirect URL and Saving in a property 
        string rURL = defaultUrl.Text;
        if ( FormHelper.GetRequestKeyValue( "nextUrl" ) != "" )
        {
            rURL = FormHelper.GetRequestKeyValue( "nextUrl" );
            rURL = HttpUtility.UrlDecode( rURL );
            rURL = UtilityManager.FormatAbsoluteUrl( rURL, false );
            ReturnURL = rURL;
        }
        else
        {
            ReturnURL = rURL;
        }
        if ( inviteId.Length == 36 )
        {
            HandleInvitation( inviteId );
        }
    }

      /// <summary>
      /// handle a registration invitation
      /// </summary>
      /// <param name="inviteId"></param>
    public void HandleInvitation( string inviteId )
    {
        Invitation = new LibraryBizService().LibraryInvitation_GetByGuid( inviteId );
        if ( Invitation != null && Invitation.SeemsPopulated )
        {
            if ( Invitation.IsActive == false )
            {
                SetConsoleErrorMessage( "Error: this invitation has been already accepted and cannot be used." );
                Invitation = null;
                return;
            }
            //what to display
            if ( prefillingEmailIfFound.Text == "yes" )
            {
                this.email.Value = Invitation.TargetEmail;
                this.confirmEmail.Value = Invitation.TargetEmail;
            }
            doImmediateConfirm.Text = "yes";
            confirmMsg.Visible = false;

            //check for a next url
            if ( Invitation.StartingUrl != null && Invitation.StartingUrl.Length > 5 )
                ReturnURL = Invitation.StartingUrl;

            if ( Invitation.MessageContent != null && Invitation.MessageContent.Length > 5 )
            {
                regMessage.Text = Invitation.MessageContent;
                regMessage.Visible = true;

            }
            //need to update the invite, but should wait until the actual reg is done
        }
    }

    public void btnSubmit_Click( object sender, EventArgs e )
    {
        LoggingHelper.DoTrace( 8, "registration.btnSubmit_Click" );
      if ( IsFormValid() )
      {
          AddUser();
      }
      else
      {
          //messages already displayed
      }
    }


    private bool IsFormValid()
    {
        bool isValid = true;
        bool canRegister = true;
        
        string status = "";
        bool emailAlreadyExists = false;

        //Validate email
        if ( this.email.Value.Trim().Length == 0 )
            SetConsoleErrorMessage( "An email must be entered<br/>" );
        else
        {
            var email1 = utilService.ValidateEmail( email.Value, ref isValid, ref status, ref emailAlreadyExists );
            if ( emailAlreadyExists )
            {
                SetConsoleErrorMessage( "Email already exists. Use <a href='/Account/ForgotPassword.aspx'>password recovery</a> if you have forgotten the password." );
                return false;
            }

            var email2 = confirmEmail.Value;
            if ( email1 != email2 )
            {
                SetConsoleErrorMessage( "Emails do not match." );
                canRegister = false;
            }
            if ( !isValid )
            {
                SetConsoleErrorMessage( "Email is invalid." );
                canRegister = false;
            }
        }

        //Validate password
        bool hasBonus = false;
        var password1 = utilService.ValidatePassword( password.Value, ref isValid, ref status, ref hasBonus );
        var password2 = confirmPassword.Value;
        if ( password1 != password2 )
        {
            SetConsoleErrorMessage( "Passwords do not match." );
            canRegister = false;
        }
        if ( !isValid )
        {
            SetConsoleErrorMessage( "Password is invalid." );
            canRegister = false;
        }

        //First Name
        if ( this.firstName.Value.Trim().Length == 0 )
            SetConsoleErrorMessage( "A first name must be entered<br/>" );
        else
        {
            var fName = utilService.ValidateName( firstName.Value, 1, "First Name", ref isValid, ref status );
            if ( !isValid )
            {
                SetConsoleErrorMessage( status );
                canRegister = false;
            }
        }

        //Last Name
        if ( this.lastName.Value.Trim().Length == 0 )
            SetConsoleErrorMessage( "A last name must be entered<br/>" );
        else
        {
            var lName = utilService.ValidateName( lastName.Value, 1, "Last Name", ref isValid, ref status );
            if ( !isValid )
            {
                SetConsoleErrorMessage( status );
                canRegister = false;
            }
        }
        return canRegister;
    }


    private bool AddUser()
    {
        LoggingHelper.DoTrace( 8, "registration.AddUser" );
        bool isValid = true;
        string status = "";
        var newUser = new Patron();

        newUser.UserName = email.Value.Trim();
        //default username to email prefix - will need a uniqueness check later
        //or just do 
        int pos = newUser.UserName.IndexOf( "@" );
        if ( pos > -1 )
        {
            //newUser.Username = newUser.Username.Substring( 0, pos );
        }
        newUser.FirstName = firstName.Value.Trim();
        newUser.LastName = lastName.Value.Trim();
        newUser.Email = email.Value.Trim();
        if ( doImmediateConfirm.Text == "no" )
            newUser.IsActive = false;
        else
            newUser.IsActive = true;
        newUser.Password = UtilityManager.Encrypt( password.Value );

        var newID = new Isle.BizServices.AccountServices().Create( newUser, ref status );
        if ( newID == 0 )
        {
            SetConsoleErrorMessage( "There was a problem creating your account. Please try again." );
            return false;
        }
        else
        {
            newUser.Id = newID;
            string ipAddress = this.GetUserIPAddress();
            ActivityBizServices.UserRegistration( newUser, ipAddress );

            if ( doImmediateConfirm.Text == "no" )
            {
                //Emit message to user
                SetConsoleSuccessMessage( registerSuccessMsg.Text );
                //notify on new
                SendConfirmationRequest( newID );

                Response.Redirect( UtilityManager.FormatAbsoluteUrl( ReturnURL, false ) );
            }
            else
            {

                //check for url and do login 
                //easier to use the login page

                bool isSecure = false;
                Patron user = new AccountServices().Get( newID );
                this.WebUser = user;

                if ( Invitation != null && Invitation.SeemsPopulated )
                {
                    Invitation.TargetUserId = newID;
                    CompleteInvitation( Invitation, newUser );

                    Response.Redirect( UtilityManager.FormatAbsoluteUrl( ReturnURL, false ), true );
                }
                else
                {
                    //not sure why doing proxy redirect??
                    if ( UtilityManager.GetAppKeyValue( "SSLEnable", "0" ) == "1" )
                        isSecure = true;
                    string proxyId = new AccountServices().Create_RegistrationConfirmProxyLoginId( newID, ref status );

                    string url = string.Format( autoActivateLink.Text, proxyId );
                    url = UtilityManager.FormatAbsoluteUrl( url, isSecure );

                    string nextUrl = "";
                    if ( ReturnURL != null && ReturnURL.Length > 5 )
                        nextUrl = "&" + ReturnURL;
                    url += nextUrl;
                    SetConsoleSuccessMessage( "Welcome to Illinois Open Educational Resources.<br/> Visit the <a href=\"/Help/Guide.aspx\">User Guide</a> for tips on getting started." );

                    Response.Redirect( UtilityManager.FormatAbsoluteUrl( url, false ), true );
                }

                
            }
        }
        return isValid;
    }
    private string GetUserIPAddress()
    {
        string ip = "";
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

    private void CompleteInvitation( LibraryInvitation entity, AppUser newUser )
    {
        string statusMessage = "";
        //update received - notify sender??
        entity.ResponseDate = System.DateTime.Now;
        string welcomeMsg = "Welcome to Illinois Open Educational Resources.<br/> Visit the <a href=\"/Help/Guide.aspx\">User Guide</a> for tips on getting started." ;
        if ( entity.LibMemberTypeId > 0 )
        {
            //add user as lib member 
            int id = new LibraryBizService().LibraryMember_Create( entity.ParentId,
                            entity.TargetUserId,
                            entity.LibMemberTypeId,
                            entity.CreatedById, ref statusMessage );
            if ( id > 0 )
            {
                if ( entity.MessageContent != null && entity.MessageContent.Length > 5 )
                    welcomeMsg += entity.MessageContent;
                welcomeMsg += "- added to invited library";
            }
        }

        if ( Invitation.AddToOrgId > 0 )
        {
            OrganizationMember om = new OrganizationMember();
            om.UserId = entity.TargetUserId;
            om.OrgId = entity.AddToOrgId;
            om.OrgMemberTypeId = entity.AddAsOrgMemberTypeId;
            om.CreatedById = entity.CreatedById;
            om.LastUpdatedById = entity.CreatedById;

            int omid = orgMgr.OrganizationMember_Create(om, ref statusMessage);
            if ( omid > 0 )
            {
                welcomeMsg += "- added to invited organization";

                //add profile
                PatronProfile prof = new PatronProfile();
                prof.UserId = newUser.Id;
                prof.OrganizationId = entity.AddToOrgId;
                new AccountServices().PatronProfile_Create( prof, ref statusMessage );

                //check for roles
                if ( Invitation.OrgMbrRoles != null && Invitation.OrgMbrRoles.Length > 0 )
                {
                    HandleOrgMbrRoles( omid, Invitation );
                }
            }
            else
            {
                LoggingHelper.LogError( "Registration2.CompleteInvitation() Failed to add user as org mbr. Reason: " + statusMessage, true );
            }

        }
        Invitation.ResponseDate = System.DateTime.Now;
        Invitation.IsActive = false;
        bool update = new LibraryBizService().LibraryInvitation_Update( Invitation );
        SetConsoleSuccessMessage( welcomeMsg );
    }
    private void HandleOrgMbrRoles( int orgMbrId, LibraryInvitation invitation )
    {
        OrganizationMemberRole role = new OrganizationMemberRole();
        string statusMessage = "";
        string[] roles = invitation.OrgMbrRoles.Split( ',' );

        foreach ( string item in roles )
        {
            role = new OrganizationMemberRole();
            role.OrgMemberId = orgMbrId;
            role.RoleId = Int32.Parse( item );
            role.CreatedById = invitation.CreatedById;
            if ( orgMbrId > 0 )
            {
                int id = orgMgr.OrganizationMemberRole_Create( role, ref statusMessage );
                if ( id > 0 )
                    LoggingHelper.DoTrace( 5, "Registration2.HandleOrgMbrRoles. Success add new org roleId of " + item );
                else
                {
                    LoggingHelper.DoTrace( 5, "Registration2.HandleOrgMbrRoles. ERROR do not add requested org role: " + item + ", reason: " + statusMessage );
                    LoggingHelper.LogError( string.Format("Registration2.HandleOrgMbrRoles() Failed to add org mbr role: {0} for user: {1}, org mbrId: {2}, org: {3}. Reason: {4}", item, WebUser.Email, orgMbrId, invitation.AddToOrgId, statusMessage), true );
                }
            }
        }

    }

    private void SendConfirmationRequest( int userId )
    {
        //retrieve user to get rowId
        AppUser applicant = new Isle.BizServices.AccountServices().Get( userId );
        string statusMessage = "";



        string toEmail = applicant.Email;
        string bcc = UtilityManager.GetAppKeyValue( "systemAdminEmail", "mparsons@siuccwd.com" );
        string fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", "mparsons@siuccwd.com" );
        if ( this.doingBccOnRegistration.Text == "no" )
            bcc = "";
        //
        bool isSecure = false;

        if ( UtilityManager.GetAppKeyValue( "SSLEnable", "0" ) == "1" )
            isSecure = true;
        string proxyId = new AccountServices().Create_ProxyLoginId( applicant.Id, "Registration Confirmation", ref statusMessage );
        string url = string.Format( activateLink.Text, proxyId );
        url = UtilityManager.FormatAbsoluteUrl( url, isSecure );

        string subject = string.Format( "IOER - registration confirmation: {0}", applicant.FullName() );

        string body = string.Format( this.confirmMessage.Text, applicant.FirstName, url );


        EmailManager.SendEmail( toEmail, fromEmail, subject, body, "", bcc );

    }
  }
}