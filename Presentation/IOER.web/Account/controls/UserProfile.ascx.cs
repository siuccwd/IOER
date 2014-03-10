using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Isle.BizServices;
using MyManager = Isle.BizServices.AccountServices;
using AppUser = LRWarehouse.Business.Patron; //ILPathways.Business.AppUser;

//using CurrentUser = LRWarehouse.Business.Patron; //ILPathways.Business.AppUser;
using ILPathways.Utilities;
using ILPathways.classes;
using ILPathways.Business;
using ILPathways.Library;

using LRWarehouse.Business;
using ILPathways.Controllers;
using Rbiz = Isle.BizServices.ResourceBizService;
using LDAL = LRWarehouse.DAL;

namespace ILPathways.Account.controls
{
    /// <summary>
    /// User profile
    /// </summary>
    public partial class UserProfile : BaseUserControl
    {
        private AppUser myEntity;
        MyManager myManager = new MyManager();

        public string newPassword = "";
        #region Properties


        private bool _emailConfirmationIsRequired = false;
        public bool EmailConfirmationIsRequired
        {
            get { return _emailConfirmationIsRequired; }
            set { _emailConfirmationIsRequired = value; }
        }

        #endregion
        protected void Page_Load( object sender, EventArgs e )
        {

            if ( Page.IsPostBack == false )
            {
                this.InitializeForm();
            }

        }
        private void InitializeForm()
        {
            try 
            {
                // Set source for form lists
                PopulateControls();

                //Update a profile
                string rowId = this.GetRequestKeyValue( "g" );
                if ( rowId.Length == 36 )
                {
                    HandleConfirmation( rowId );
                }
                else if ( IsUserAuthenticated() == false )
                {
                    SetConsoleErrorMessage( "Error: Invalid request, you are not logged in." );
                    Response.Redirect( "/Account/Login.aspx" );
                } else 
                {
                    fillProfileForm();
                }
            }
            catch ( ThreadAbortException taex )
            {
                //Ignore this, it is caused by a redirect.  The redirect will go OK anyway.
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "UserProfile.InitializeForm exception:" );
            }
        }	// End 

        /// <summary>
        /// special cases like confirmations
        /// </summary>
        /// <param name="rowId"></param>
        private void HandleConfirmation( string rowId )
        {
            try
            {
                AppUser user = myManager.GetByRowId( rowId );
                if ( user != null && user.IsValid )
                {
                    user.IsActive = true;
                    myManager.Update( user );

                    this.WebUser = user;

                    SetConsoleSuccessMessage( "Registration Successful. <br/>Welcome to IOER, " + user.FullName() + "." );
                    ActivityBizServices.UserRegistrationConfirmation( user );
                    //Check for Redirect URL 
                    string rURL = "";
                    if ( FormHelper.GetRequestKeyValue( "nextUrl" ) != "" )
                    {
                        rURL = FormHelper.GetRequestKeyValue( "nextUrl" );
                        rURL = HttpUtility.UrlDecode( rURL );
                    }
                    //TODO - may not want to immediately redirect - ie after completing profile
                    if ( rURL.Length > 0 )
                        Response.Redirect( rURL, true );

                    fillProfileForm();
                }
                else
                {
                    SetConsoleErrorMessage( "Error: invalid account identifier provided for confirmation." );
                    Response.Redirect( "/", true );
                }
            }
            catch ( ThreadAbortException taex )
            {
                //Ignore this, it is caused by a redirect.  The redirect will go OK anyway.
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "UserProfile.HandleConfirmation exception:" );
            }
        }	// End 


        //Pre-fill a profile form with values pulled from the session's current patron
        private void fillProfileForm()
        {

            //Set the current working entity to that patron.
            myEntity = GetAppUser();
            if ( myEntity.Username.ToLower() == "mparsons" )
                btnUpload.Visible = true;

            newPassword = "New ";

            pnlUpdateControls.Visible = true;

            //Fill out the form data

            txtFirstName.Text = myEntity.FirstName;
            txtLastName.Text = myEntity.LastName;
            txtEmail.Text = myEntity.Email;
            txtEmail2.Text = myEntity.Email;

            PatronProfile entity = myManager.PatronProfile_Get( myEntity.Id );
            if ( entity.OrganizationId > 0 )
            {
                this.lblPubProfileTitle.Text = this.lblAuthorProfileTitle.Text;
                txtMyOrganization.Text = entity.Organization;
            }
            else
            {
                txtMyOrganization.Text = "None";
            }
            if ( entity.ImageUrl != null && entity.ImageUrl.Length > 10 )
            {
                noProfileImagelabel.Visible = false;
                FormatImageTag( entity.ImageUrl );
            }
            else
            {
                noProfileImagelabel.Visible = true;
                //show template
                string defaultProfileImage = UtilityManager.GetAppKeyValue( "defaultProfileImage", "" );
                if ( defaultProfileImage.Length > 10 )
                    FormatImageTag( defaultProfileImage );
            }

  
            this.SetListSelection( this.ddlPubRole, entity.PublishingRoleId.ToString() );
            this.txtJobTitle.Text = entity.JobTitle;
            this.txtProfile.Text = entity.RoleProfile;

        }

        private void FormatImageTag( string imageUrl )
        {
            int imageWidth = UtilityManager.GetAppKeyValue( "libraryImageWidth", 160 );
            currentFileName.Text = imageUrl;
            if ( showingImagePath.Text.Equals( "yes" ) )
                currentFileName.Visible = true;
            int sec = DateTime.Now.Second;
            string v = "?v=" + sec.ToString();
            string img = string.Format( txtImageTemplate.Text, imageUrl + v);
            currentImage.Text = img;
            currentImage.Visible = true;
        }//

        /// <summary>
        /// return true if a file has been entered or selected
        /// </summary>
        protected bool IsFilePresent()
        {
            bool isPresent = false;

            if ( fileUpload.HasFile || fileUpload.FileName != "" )
            {
                isPresent = true;

            }

            return isPresent;
        }//

        protected void btnUpdateProfile_Click( object sender, EventArgs e )
        {
            AppUser applicant = GetAppUser();
            if ( this.IsFormValid( applicant ) )
            {
                this.UpdateForm();
            }
        }
        /// <summary>
        /// validate form content
        /// </summary>
        /// <returns></returns>
        private bool IsFormValid( AppUser applicant )
        {
            applicant.IsValid = true;

            bool isEmailValid = true;
            StringBuilder errorBuilder = new StringBuilder( "" );

            try
            {
                txtEmail.Text = FormHelper.CleanText( txtEmail.Text );

                //Do form validation

                if ( !validateEmail() )
                {
                    applicant.IsValid = false;
                    isEmailValid = false;
                    errorBuilder.Append( " Invalid Email. <br/>" );
                }

                //TODO - if user changed email, need to verify the new one doesn't exist!!
                ValidateExistingUser( applicant, isEmailValid, ref errorBuilder );

            }
            catch ( System.Exception ex )
            {
                //SetFormMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.ToString() );
                return false;
            }

            if ( applicant.IsValid == false )
            {
                registerErrorMessage.Text = errorBuilder.ToString();
                return false;
            }
            else
            {
                return true;
            }
        } //
        private bool ValidateExistingUser( AppUser applicant, bool isEmailValid, ref StringBuilder errorBuilder )
        {
            //
            if ( !validateUpdatePassword( ref applicant, ref errorBuilder ) )
            {
                applicant.IsValid = false;
                errorBuilder.Append( " Invalid Password. <br/>" );
            }

            if ( isEmailValid && applicant.Email != txtEmail.Text )
            {
                if ( myManager.DoesUserEmailExist( txtEmail.Text ) )
                {
                    applicant.IsValid = false;
                    errorBuilder.Append( " An account with that Email address already exists. <br/>" );
                }
            }
            string message = "";
            if ( HasRequiredFields( ref message ) == false )
            {
                applicant.IsValid = false;
                errorBuilder.Append( message );
            }

            return applicant.IsValid;

        } //

        private bool HasRequiredFields( ref string message )
        {
            bool isValid = true;
            message = "";


            if ( this.ddlPubRole.SelectedIndex < 1 )
                message += "An IOER role must be selected<br/>";

            if ( this.txtLastName.Text.Trim().Length == 0 )
                message += "A last name must be entered<br/>";

            if ( this.txtFirstName.Text.Trim().Length == 0 )
                message += "A first name must be entered<br/>";

            if ( this.txtEmail.Text.Trim().Length == 0 )
                message += "An email must be entered<br/>";

            if ( message.Length > 0 )
            {
                //SetConsoleErrorMessage( message );
                isValid = false;
            }
            return isValid;
        } //
        private void UpdateForm()
        {
            //get current record
            AppUser applicant = myManager.Get( WebUser.Id );

            //Store registration info
            applicant.FirstName = FormHelper.SanitizeUserInput( txtFirstName.Text );
            applicant.LastName = FormHelper.SanitizeUserInput( txtLastName.Text );
            //?????
            //string testBanana = Request.Form[ txtFirstName.UniqueID ];

            if ( txtPassword1.Text.Length > 0 )
                applicant.Password = UtilityManager.Encrypt( txtPassword1.Text );

            applicant.Email = FormHelper.SanitizeUserInput( txtEmail.Text );

            //Write to database
            string status = myManager.Update( applicant );
            if ( status == "successful" )
            {
                UpdateProfile( applicant );
                //SessionManager.SetUserToSession( Session, applicant );
                WebUser = applicant;

                //Emit message to user
                SetConsoleSuccessMessage( "Your profile has been updated." );
                Response.Redirect( Request.RawUrl );
            }
            else
            {
                SetConsoleErrorMessage( "An error was encountered updating your profile:<br/>" + status );

            }
        }//

        private bool UpdateProfile( AppUser user )
        {
            PatronProfile entity = myManager.PatronProfile_Get( user.Id );
            bool isUpdate = false;
            bool hasOrg = false;

            if ( entity.UserId > 0 )
                isUpdate = true;

            entity.UserId = user.Id;
            string toEmail = UtilityManager.GetAppKeyValue( "contactUsMailTo", "info@siuccwd.com" );
            string subject = "";
            string body = "";

            //??why set to zero??
            //entity.OrganizationId = 0;

            if ( txtOrganizationName.Text.Trim().Length > 3 )
            {
                if ( txtOrganizationName.Text.Trim().ToLower().IndexOf( "type at " ) == -1 )
                {
                    hasOrg = true;
                    HandleOrganizationRequest( user );
                }
            }
            if ( this.ddlPubRole.SelectedIndex > 0 )
            {
                entity.PublishingRoleId = int.Parse( this.ddlPubRole.SelectedValue.ToString() );
            }
            entity.RoleProfile = FormHelper.CleanText( this.txtProfile.Text );
            entity.JobTitle = FormHelper.CleanText( txtJobTitle.Text );
            string statusMessage = "";

            // if file exists, do upload and populate the entity as needed
            if ( IsFilePresent() )
            {
                bool isValid = HandleUpload( user, entity, ref statusMessage );
                if ( isValid == false )
                {
                    SetConsoleErrorMessage( statusMessage );
                    return false;
                }
            }

            if ( isUpdate )
                statusMessage = myManager.PatronProfile_Update( entity );
            else
                myManager.PatronProfile_Create( entity, ref statusMessage );

            return true;
        }
        protected void btnUpload_Click( object sender, EventArgs e )
        {
            if ( IsFilePresent() )
            {
                string statusMessage = "";
                PatronProfile entity = myManager.PatronProfile_Get( WebUser.Id );
                bool isValid = HandleUpload( WebUser, entity, ref statusMessage );
                if ( isValid == false )
                {
                    SetConsoleErrorMessage( statusMessage );
                    return;
                }
                else
                {
                    if ( entity.UserId == WebUser.Id )
                        statusMessage = myManager.PatronProfile_Update( entity );
                    else
                        myManager.PatronProfile_Create( entity, ref statusMessage );
                    SetConsoleInfoMessage( "Upload status:  " +  statusMessage );
                    FormatImageTag( entity.ImageUrl );
                }
            }
            else
            {
                SetConsoleErrorMessage( "Error - select an image and then click the upload button." );
            }
        } //

        /// <summary>
        /// Handle upload of file
        /// </summary>
        /// <param name="user"></param>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        protected bool HandleUpload( IWebUser user, PatronProfile entity, ref string statusMessage )
        {
            bool isValid = true;
            try
            {
                int imageWidth = UtilityManager.GetAppKeyValue( "libraryImageWidth", 150 );

                string savingName = user.RowId.ToString().Replace( "-", "" ) + System.IO.Path.GetExtension( fileUpload.FileName );
                string savingFolder = FileResourceController.DetermineDocumentPath( user.Id, 0 );
                string savingURL = FileResourceController.DetermineDocumentUrl( user.Id, 0, savingName );
                entity.ImageUrl = savingURL;
                ImageStore img = new ImageStore();
                img.FileName = savingName;
                img.FileDate = DateTime.Now;

                FileResourceController.HandleImageResizingToWidth( img, fileUpload, imageWidth, imageWidth,false, true );
                FileSystemHelper.HandleDocumentCaching( savingFolder, img, true );
            }
            catch ( Exception ex )
            {
                statusMessage = ex.Message;
                LoggingHelper.LogError( ex, "Profile().HandleUpload" );
                isValid = false;
            }
            return isValid;
        }

        private void HandleOrganizationRequest( AppUser user )
        {
            string toEmail = UtilityManager.GetAppKeyValue( "contactUsMailTo", "info@siuccwd.com" );
            string subject = "";
            string body = "";
            string statusMessage = "";
            OrganizationRequest entity = new OrganizationRequest();
            entity.UserId = user.Id;

            //REQUEST for new org
            //- check if exists to determine if an add request
            if ( txtOrganizationName.Text.IndexOf( "[" ) > -1 && txtOrganizationName.Text.IndexOf( "]" ) > -1 )
            {
                entity.OrgId = ExtractOrgId( txtOrganizationName.Text );

                entity.Action = "Add to organization";
                //existing??
                //TODO - check for a org contact, and send email to them

                subject = string.Format( "IOER request to add user to organzation: {0}", txtOrganizationName.Text );
                body = string.Format( txtAddUserToOrgRequest.Text, user.FullName(), txtOrganizationName.Text );
                SetConsoleInfoMessage( "<p>NOTE: Adding a user to an organization requires approval.<br/>An email was sent to administration to address your request. You will be notified upon review.</p>" );
            }
            else
            {
                //TODO - show a form to register an org
                entity.OrganzationName = txtOrganizationName.Text;
                entity.Action = "Add to NEW organization";
                subject = string.Format( "IOER request to add new organzation: {0}", txtOrganizationName.Text );
                body = string.Format( txtNewOrgRequest.Text, user.FullName(), txtOrganizationName.Text );
                SetConsoleInfoMessage( "<p>NOTE: Adding a new organization requires approval. <br/>An email was sent to administration to address your request. You will be notified upon review.</p>" );
            }

            string cc = UtilityManager.GetAppKeyValue( "systemAdminEmail", "mparsons@siuccwd.com" );
            //int orgId = HandleSpecialOrgs( txtOrganizationName.Text );

            //could do a quick add as inactive

            //create request
            int id = MyManager.OrganizationRequestCreate( entity, ref statusMessage );

            body += "<br/>url: TBD - a direct link to a org mgmt page using id=" + id.ToString();
            body += "<br/>From: " + user.EmailSignature();
            string from = user.Email;
            EmailManager.SendEmail( toEmail, from, subject, body, cc, "" );
        }

        private int ExtractOrgId( string org )
        {
            int orgId = 0;
            org = org.Trim();
            int startPos = org.IndexOf( "[" );
            if ( startPos > 0 )
            {
                int endPos = org.IndexOf( "]", startPos );
                if ( startPos > 0 && endPos > startPos )
                {
                    string sorgId = org.Substring( startPos, endPos - startPos );
                    if ( sorgId != null )
                    {
                        sorgId = sorgId.Trim();
                        if ( IsInteger( sorgId ) )
                        {
                            orgId = int.Parse( sorgId );
                        }


                    }
                }
            }

            return orgId;
        }

        private int HandleSpecialOrgs( string testOrg )
        {
            int orgId = 0;
            //if special (temp) org, assign immediately
            if ( testOrg.IndexOf( "(" ) > -1 )
            {
                testOrg = testOrg.Substring( 0, testOrg.IndexOf( "(" ) - 1 );
            }
            testOrg = testOrg.Trim();
            if ( testOrg.ToLower().IndexOf( "bloomington feast" ) > -1 )
            {
                Organization org = MyManager.GetOrgByName( testOrg );
                orgId = org.Id;
            }

            return orgId;
        }


        #region Form validation methods


        //Validate the Password
        private bool validatePassword()
        {
            bool validPass = false;
            string passwordText1 = txtPassword1.Text;
            string passwordText2 = txtPassword2.Text;

            if ( passwordText1 != passwordText2 )
            {
                return false;
            }

            char[] passwordChars = passwordText1.ToCharArray();
            string lowerCaseChars = "abcdefghijklmnopqrstuvwxyz";
            string upperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string numberChars = "1234567890";
            string invalidChars = "<> {}[]=+-_~`,.#/|\\";
            bool hasLowerCase = false;
            bool hasUpperCase = false;
            bool hasNumber = false;
            bool hasInvalid = false;

            for ( int i = 0 ; i < passwordChars.Length ; i++ )
            {
                if ( !lowerCaseChars.Contains( passwordChars[ i ] ) )
                {
                    hasLowerCase = true;
                }
                if ( !upperCaseChars.Contains( passwordChars[ i ] ) )
                {
                    hasUpperCase = true;
                }
                if ( !numberChars.Contains( passwordChars[ i ] ) )
                {
                    hasNumber = true;
                }
                if ( invalidChars.Contains( passwordChars[ i ] ) )
                {
                    hasInvalid = true;
                }
            }
            if ( hasLowerCase && hasUpperCase && hasNumber && passwordChars.Length >= 8 & !hasInvalid )
            {
                validPass = true;
            }

            return validPass;

        }

        private bool validateUpdatePassword( ref Patron applicant, ref StringBuilder errorBuilder )
        {
            //Only do something if the user chose to update their password
            //what if the confirm password was not entered??
            if ( txtPassword1.Text.Length > 0 && txtPassword2.Text.Length > 0 )
            {
                if ( txtPassword1.Text == txtPassword2.Text )
                    return true;
                else
                {
                    errorBuilder.Append( "Passwords do not match" );
                    return false;
                }
               
            }
            //Otherwise, just keep the password
            else
            {
                return true;
            }
        }

        //Validate the Email
        private bool validateEmail()
        {
            string email = FormHelper.SanitizeUserInput( txtEmail.Text );
            string email2 = FormHelper.SanitizeUserInput( txtEmail2.Text );

            if ( email == email2 )
            {
                if ( email.LastIndexOf( '@' ) > 1 & email.LastIndexOf( '.' ) > email.LastIndexOf( '@' ) & email.Length >= 6 )
                {
                    revEmail.Validate();
                    if ( revEmail.IsValid )
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region Housekeeping
        /// <summary>
        /// Populate form controls
        /// </summary>
        private void PopulateControls()
        {
            SetPubRolesList();
        } //


        private void SetPubRolesList()
        {

            DataSet ds = Rbiz.DoQuery( this.pubRoleSql.Text );
            if (DoesDataSetHaveRows(ds))
                LDAL.DatabaseManager.PopulateList( this.ddlPubRole, ds, "Id", "Title", roleSelectTitle.Text );
        }

 
 
        #endregion

    }
}
