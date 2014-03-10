using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using SD = System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using MyManager = Isle.BizServices.AccountServices;
using AppUser = LRWarehouse.Business.Patron; //ILPathways.Business.AppUser;

//using CurrentUser = LRWarehouse.Business.Patron; //ILPathways.Business.AppUser;
using ILPathways.Utilities;
using ILPathways.classes;
using ILPathways.Business;
using ILPathways.Library;

using LRWarehouse.Business; 
using ILPathways.Controllers;
using LDAL = LRWarehouse.DAL;
using Isle.BizServices;

namespace ILPathways.Account.controls
{
    public partial class AccountRegisterUpdateController : BaseUserControl
    {
        private AppUser myEntity;
        MyManager myManager = new MyManager();
        static int maxWidth = 150;
        bool setToMaxWidth = true;

        public string newPassword = "";
        #region Properties
        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string CurrentView
        {
            get { return this.txtCurrentView.Text; }
            set { this.txtCurrentView.Text = value; }
        }

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
            else
            {
                registerErrorMessage.Text = jValidateError.Value;
            }
        }
        private void InitializeForm()
        {
           

            // Set source for form lists
            this.PopulateControls();
            if ( CurrentView.ToLower() == "registration")
            {
                //Register a new user
                registrationPanel.Visible = true;
                containerPanel.Visible = true;
                fillRegistrationForm();
            }
            else if ( CurrentView.ToLower() == "profile" )
                {
                    containerPanel.Visible = true;
                    //Update a profile
                    string rowId = "";  // this.GetRequestKeyValue( "g" );
                    if ( rowId.Length == 36 )
                    {
                        //HandleConfirmation( rowId );
                    }
                    else
                    {
                        if ( IsUserAuthenticated() )
                        {
                            //profilePanel.Visible = true;
                            fillProfileForm();
                        }
                        else
                        {
                            SetConsoleErrorMessage( "Error: Invalid request, you are not logged in." );
                            Response.Redirect( "/Account/Login.aspx" );
                        }
                    }
                }
                else
                {

                }
        }	// End 

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
                    if ( rURL.Length > 0)
                        Response.Redirect( rURL, true );
                    //profilePanel.Visible = true;
                    fillProfileForm();
                }
            }
            catch ( ThreadAbortException taex )
            {
                //Ignore this, it is caused by a redirect.  The redirect will go OK anyway.
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "AccountRegisterUpdateController.HandleConfirmation exception:" );
            }
        }	// End 


        //Pre-fill the registration form with values pulled from another account
        private void fillRegistrationForm()
        {
            //If there is a workNet based patron to pull data from...
            if ( Session[ "workNetPatron" ] != null )
            {
                //Set the current working entity to that patron.
                AppUser worknetPatron = ( AppUser ) Session[ "workNetPatron" ];

                //Check for existing account
                //May want to change this to Username (or do Username in addition to this?)
                CurrentUser = myManager.GetByEmail( worknetPatron.Email );

                if ( CurrentUser != null && CurrentUser.Id > 0 )
                {
                    SetConsoleErrorMessage ( " Your email is already registered with Illinois Pathways. <br/>Have you already done a sync of your workNet and pathways account? ");
                    Response.Redirect( "/Account/Profile.aspx" );
                }
                else
                {
                    //Fill the values
                    txtFirstName.Text = worknetPatron.FirstName;
                    txtLastName.Text = worknetPatron.LastName;
                    txtEmail.Text = worknetPatron.Email;

                    //If we're coming from Worknet...
                    if ( worknetPatron.worknetId != 0 )
                    {
                        registerErrorMessage.Text += "You are registering your workNet account (" + worknetPatron.Email + ") with IOER.";
                        hdnWorknetId.Value = worknetPatron.worknetId.ToString();
                        txtEmail.ReadOnly = true;
                    }
                    else { hdnWorknetId.Value = ""; }
                }
            }

            if ( !IsPostBack )
            {
                //Fill the secret question drop-down list
                //ddlSecretQuestion.DataSource = LDAL.DatabaseManager.selectCodesFromTable( "[Codes.SecretQuestion]" );
                //ddlSecretQuestion.DataTextField = "Title";
                //ddlSecretQuestion.DataValueField = "Id";
                //ddlSecretQuestion.DataBind();
            }
        }

        //Pre-fill a profile form with values pulled from the session's current patron
        private void fillProfileForm()
        {
            
            //Set the current working entity to that patron.
            myEntity = GetAppUser();
            if ( myEntity.Username.ToLower() == "mparsons" )
                btnUpload.Visible = true;

            //Show/hide appropriate form elements
            lblUserName.Visible = true;
            txtUsername.Visible = false;
            newPassword = "New ";
            pnlRegisterControls.Visible = false;
            pnlUpdateControls.Visible = true;
            //pnlChangePassword.Visible = true;

            //Fill out the form data
            lblUserName.Text = myEntity.Username;

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
            if ( entity.ImageUrl!= null && entity.ImageUrl.Length > 10 )
            {
                noProfileImagelabel.Visible = false;
                FormatImageTag( entity.ImageUrl );
            }
            else
            {
                //show template
                string defaultProfileImage = UtilityManager.GetAppKeyValue( "defaultProfileImage", "" );
                if ( defaultProfileImage.Length > 10 )
                    FormatImageTag( defaultProfileImage );
            }

            this.SetListSelection( this.ddlOrganization, entity.OrganizationId.ToString() );
            this.SetListSelection( this.ddlPubRole, entity.PublishingRoleId.ToString() );
            this.txtJobTitle.Text = entity.JobTitle;
            this.txtProfile.Text = entity.RoleProfile;
            
        }

        private void FormatImageTag( string imageUrl )
        {
            int libraryImageWidth = UtilityManager.GetAppKeyValue( "libraryImageWidth", 150 );
            currentFileName.Text = imageUrl;
            if ( showingImagePath.Text.Equals( "yes" ) )
                currentFileName.Visible = true;
            int sec = DateTime.Now.Second;
            string v = "?v=" + sec.ToString();
            string img = string.Format( txtImageTemplate.Text, imageUrl + v, libraryImageWidth );
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
        //Register a new account
        protected void btnSubmitRegister_Click( object sender, EventArgs e )
        {
            Patron applicant = new Patron();

            if ( this.IsFormValid( applicant ) )
            {
                this.AddUser();
            }
        }
        //Register a new account
        protected void AddUser()
        {
            //Setup the variables
            AppUser applicant = new AppUser();
            applicant.IsValid = true;

            //Link to worknet account
            if ( hdnWorknetId.Value != "" )
            {
                applicant.worknetId = int.Parse( hdnWorknetId.Value );
            }
            else
            {
                applicant.worknetId = 0;
            }

            //Store registration info
            if (txtUsername.Text.Length > 4)
                applicant.Username = FormHelper.SanitizeUserInput( txtUsername.Text );
            else
                applicant.Username = FormHelper.SanitizeUserInput( txtEmail.Text );

            applicant.FirstName = FormHelper.SanitizeUserInput( txtFirstName.Text );
            applicant.LastName = FormHelper.SanitizeUserInput( txtLastName.Text );
            applicant.Password = UtilityManager.Encrypt( txtPassword1.Text );
            applicant.Email = FormHelper.SanitizeUserInput( txtEmail.Text );

            //applicant.SecretQuestionID = Int32.Parse( ddlSecretQuestion.SelectedValue );
            //applicant.SecretAnswer = FormHelper.SanitizeUserInput( txtSecretAnswer.Text );
            if ( EmailConfirmationIsRequired == false )
                applicant.IsActive = true;
            else
                applicant.IsActive = false;

            string statusMessage = "";

            //Write to database
            applicant.Id = myManager.Create( applicant, ref statusMessage );

            //Return successful
            if ( applicant.IsValid && applicant.Id > 0 )
            {
                if ( EmailConfirmationIsRequired == false )
                {
                    WebUser = applicant;
                    UpdateProfile( applicant );
                    //Emit message to user
                    SetConsoleSuccessMessage( "Registration Successful. Welcome to IOER, " + applicant.FirstName + " " + applicant.LastName + "." );
                    //notify on new
                    if ( sendInfoEmailOnRegistration.Text == "yes")
                        NewAcctNotification( applicant );

                    Response.Redirect( "/" );
                }
                else
                {
                    //WebUser = applicant;
                    UpdateProfile( applicant );
                    //Emit message to user
                    SetConsoleSuccessMessage( "A confirmation of your email address is required. An email was sent with a link to complete registration. Upon completing registration your account will be activated." );
                    //notify on new
                    SendConfirmationRequest( applicant.Id );

                    Response.Redirect( "/" );
                }
               
            }
            else 
            {
                SetConsoleErrorMessage( "An error was encountered attempting to create your account:<br/>" + statusMessage );
            }
        }

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

                if ( applicant.Id > 0 )
                {
                    ValidateExistingUser( applicant, isEmailValid, ref errorBuilder );
                    
                }
                else
                {
                    //Add
                    ValidateNewUser( applicant, isEmailValid, ref errorBuilder );
                    
                }
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
        private bool ValidateNewUser( AppUser applicant, bool isEmailValid, ref StringBuilder errorBuilder )
        {
            
            txtUsername.Text = FormHelper.SanitizeUserInput( this.txtUsername.Text );

            //if ( !validateUsername() )
            //{
            //    applicant.IsValid = false;
            //    errorBuilder.Append( " Invalid Username. <br/>" );
            //}
            if ( !validatePassword() )
            {
                applicant.IsValid = false;
                errorBuilder.Append( " Invalid Password. <br/>" );
            }

            if ( !cbxTOS.Checked )
            {
                applicant.IsValid = false;
                errorBuilder.Append( " You must agree to the Terms of Use to use this site. <br/>" );
            }
            string message = "";
            if ( HasRequiredFields( ref message ) == false )
            {
                applicant.IsValid = false;
                errorBuilder.Append( message );
            }

            //Check for existing email ==> only if email is valid
            if ( isEmailValid )
            {
                if ( myManager.DoesUserEmailExist( txtEmail.Text ) )
                {
                    applicant.IsValid = false;
                    errorBuilder.Append( " An account with that Email address already exists. <br/>" );
                }
            }
            //Check for existing account (don't check if email found)
            //if ( myManager.DoesUserNameExist( txtUsername.Text ) )
            //{
            //    applicant.IsValid = false;
            //    errorBuilder.Append( " An account with that user name already exists. <br/>" );
            //}

            return applicant.IsValid;
        } //
        private bool HasRequiredFields( ref string message )
        {
            bool isValid = true;
            message = "";
            //actually ok to not have org, just means limitations
            if ( this.ddlOrganization.SelectedIndex < 1 && txtIsOrgRequired.Text.Equals("yes"))
                message += "An organization must be selected<br/>";

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
            bool usingCropTool = false;
            if ( IsFilePresent() )
            {
                if ( usingCropTool )
                {
                    HandleUploadForCropping();
                }
                else
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
                        SetConsoleInfoMessage( "Upload status:  " + statusMessage );
                        FormatImageTag( entity.ImageUrl );
                    }
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

                FileResourceController.HandleImageResizingToWidth( img, fileUpload, imageWidth, imageWidth, false, true );
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

        #region  --- handle image upload and crop

        protected void HandleUploadForCropping()
        {
            Boolean FileOK = false;
            Boolean FileSaved = false;

            imagePath.Text = FileResourceController.DetermineDocumentPath( WebUser.Id, 0 ) + "\\";

            Session[ "WorkingImage" ] = fileUpload.FileName;
            workUrl.Text = FileResourceController.DetermineDocumentUrl( WebUser.Id, 0, fileUpload.FileName );
            workCroppedUrl.Text = FileResourceController.DetermineDocumentUrl( WebUser.Id, 0, "cropped_" + fileUpload.FileName );
            workResizedUrl.Text = workCroppedUrl.Text.Replace( "cropped_", "resized_" );

            String FileExtension = Path.GetExtension( Session[ "WorkingImage" ].ToString() ).ToLower();
            String[] allowedExtensions = { ".png", ".jpeg", ".jpg", ".gif" };
            for ( int i = 0 ; i < allowedExtensions.Length ; i++ )
            {
                if ( FileExtension == allowedExtensions[ i ] )
                {
                    FileOK = true;
                }
            }

            if ( FileOK )
            {
                try
                {
                    //handle image size
                    ImageStore img = new ImageStore();
                    img.ImageFileName = fileUpload.FileName;
                    //size to a manageble size for cropping
                    FileResourceController.HandleImageResizingToWidth( img, fileUpload, 600, 600, true, true, imagePath.Text );

                    //fileUpload.PostedFile.SaveAs( path + Session[ "WorkingImage" ] );
                    FileSaved = true;
                }
                catch ( Exception ex )
                {
                    SetConsoleErrorMessage( "File could not be uploaded." + ex.Message.ToString());

                    FileSaved = false;
                }
            }
            else
            {
                SetConsoleErrorMessage( "Cannot accept files of this type.");
            }

            if ( FileSaved )
            {
                pnlUpload.Visible = false;
                btnNew.Visible = true;
                pnlCrop.Visible = true;
                imgCrop.ImageUrl = workUrl.Text;// "/" + workFolder + "/" + Session[ "WorkingImage" ].ToString();
                previewImage.ImageUrl = workUrl.Text; // "/" + workFolder + "/" + Session[ "WorkingImage" ].ToString();

            }

        } //

        protected void btnCrop_Click( object sender, EventArgs e )
        {
            string imageName = Session[ "WorkingImage" ].ToString();
            int w = Convert.ToInt32( W.Value );
            int h = Convert.ToInt32( H.Value );
            int x = Convert.ToInt32( X.Value );
            int y = Convert.ToInt32( Y.Value );

            //set crop section first
            byte[] CropImage = Crop( imagePath.Text + imageName, w, h, x, y );
            using ( MemoryStream ms = new MemoryStream( CropImage, 0, CropImage.Length ) )
            {
                ms.Write( CropImage, 0, CropImage.Length );
                using ( SD.Image croppedImage = SD.Image.FromStream( ms, true ) )
                {
                    //now resize to max (or use two steps?)

                    SaveCroppedImage( imageName, croppedImage );

                    SaveResizedImage( imageName, croppedImage );
                    //croppedImage.Dispose();
                }
            }
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            //now resize the cropped image
            string imageName = Session[ "WorkingImage" ].ToString();
        }

        void SaveResizedImage( string imageName, SD.Image image )
        {
            string SaveTo = imagePath.Text + "resized_" + imageName;
            double scaleFactor = 1;
            if ( image.Width > maxWidth )
                scaleFactor = ( double ) maxWidth / ( double ) image.Width;
            else if ( image.Width < maxWidth && setToMaxWidth == true )
                scaleFactor = ( double ) maxWidth / ( double ) image.Width; //same but increases

            if ( scaleFactor != 1 )
            {
                ImageFormat imgFormat;
                imgFormat = image.RawFormat;
                int newWidth = ( int ) ( image.Width * scaleFactor );
                int newHeight = ( int ) ( image.Height * scaleFactor );

                SD.Bitmap thumbnailBitmap = new SD.Bitmap( newWidth, newHeight );
                SD.Graphics thumbnailGraph = SD.Graphics.FromImage( thumbnailBitmap );

                thumbnailGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbnailGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbnailGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;

                SD.Rectangle imageRectangle = new SD.Rectangle( 0, 0, newWidth, newHeight );

                thumbnailGraph.DrawImage( image, imageRectangle );

                //thumbnailBitmap.Save( toStream, imgFormat );
                thumbnailBitmap.Save( SaveTo, image.RawFormat );
                //thumbnailGraph.Dispose();
                //thumbnailBitmap.Dispose();
                //image.Dispose();
            }
            else
            {
                image.Save( SaveTo, image.RawFormat );
            }

            ImageStore entity = new ImageStore();
            entity.ImageFileName = imageName;
            FileStream fs = new FileStream( SaveTo, FileMode.Open, FileAccess.Read );
            entity.Bytes = fs.Length;
            byte[] data = new byte[ fs.Length ];
            fs.Read( data, 0, data.Length );
            fs.Close();
            fs.Dispose();
            entity.SetImageData( entity.Bytes, data );

            //image.Save( SaveTo, image.RawFormat );
            pnlCrop.Visible = false;
            pnlCropped.Visible = true;
            imgResized.ImageUrl = workResizedUrl.Text;   // "/" + workFolder + "/" + "resized_" + imageName;
        }
  
        void SaveCroppedImage( string imageName, SD.Image croppedImage )
        {
            string SaveTo = imagePath.Text + "cropped_" + imageName;
            croppedImage.Save( SaveTo, croppedImage.RawFormat );
            pnlCrop.Visible = false;
            pnlCropped.Visible = true;
            imgCropped.ImageUrl = workCroppedUrl.Text;// "/" + workFolder + "/" + "cropped_" + imageName;
            //croppedImage.Dispose();
        }

        static byte[] Crop( string imagePath, int Width, int Height, int X, int Y )
        {
            try
            {
                using ( SD.Image originalImage = SD.Image.FromFile( imagePath ) )
                {
                    using ( SD.Bitmap bmp = new SD.Bitmap( Width, Height ) )
                    {
                        bmp.SetResolution( originalImage.HorizontalResolution, originalImage.VerticalResolution );
                        using ( SD.Graphics Graphic = SD.Graphics.FromImage( bmp ) )
                        {
                            Graphic.SmoothingMode = SmoothingMode.AntiAlias;
                            Graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            Graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            Graphic.DrawImage( originalImage, new SD.Rectangle( 0, 0, Width, Height ), X, Y, Width, Height, SD.GraphicsUnit.Pixel );
                            MemoryStream ms = new MemoryStream();
                            bmp.Save( ms, originalImage.RawFormat );
                            return ms.GetBuffer();
                        }
                    }
                }
            }
            catch ( Exception Ex )
            {
                throw ( Ex );
            }
        }

        protected void btnNew_Click( object sender, EventArgs e )
        {
            pnlUpload.Visible = true;
            pnlCrop.Visible = false;
            pnlCropped.Visible = false;
        }

        #endregion

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
                testOrg = testOrg.Substring( 0, testOrg.IndexOf( "(" ) -1 );
            }
            testOrg = testOrg.Trim();
            if ( testOrg.ToLower().IndexOf( "bloomington feast" ) > -1 )
            {
                Organization org = MyManager.GetOrgByName( testOrg );
                orgId = org.Id;
            }

            return orgId;
        }

        /// <summary>
        /// send confirmation of new account to admin
        /// </summary>
        /// <param name="applicant"></param>
        private void NewAcctNotification( AppUser applicant )
        {
            string toEmail = UtilityManager.GetAppKeyValue( "contactUsMailTo", "info@IOER.com" );
            string subject = string.Format( "IOER - New account notification: {0}", applicant.FullName() );
            string body = string.Format( "<p>{0} created a new account. </p>", applicant.FullName() );
            //body += "<br/>url: TBD - a direct link to a user mgmt page";
            body += "<br/>From: " + applicant.EmailSignature();
            //string from = applicant.Email;
            EmailManager.SendEmail( toEmail, toEmail, subject, body, "", "" );

        }

        private void SendConfirmationRequest( int userId )
        {
            //retrieve user to get rowId
            AppUser applicant = myManager.Get( userId );

            string toEmail = applicant.Email;
            string bcc = UtilityManager.GetAppKeyValue( "systemAdminEmail", "mparsons@siuccwd.com" );
            string fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", "mparsons@siuccwd.com" );
            if ( doBccOnRegistration.Text == "no" )
                bcc = "";
            //
            bool isSecure = false;

            if ( UtilityManager.GetAppKeyValue( "SSLEnable", "0" ) == "1" )
                isSecure = true;
            string url = string.Format( activateLink.Text, applicant.RowId.ToString() );
            url = UtilityManager.FormatAbsoluteUrl( url, isSecure );

            string subject = string.Format( "IOER - registration confirmation: {0}", applicant.FullName() );

            string body = string.Format( this.confirmMessage.Text, applicant.FirstName, url );
                        

            EmailManager.SendEmail( toEmail, fromEmail, subject, body, "", bcc );

        }
        #region Form validation methods

        //Validate the UserName
        private bool validateUsername()
        {
            //Get the name
            string name = FormHelper.SanitizeUserInput( txtUsername.Text );

            //Check for invalid name length
            if ( name.Length < 6 ) { return false; }

            //Get invalid characters
            char[] invalidChars = "$%^&()+?<> {}[]=+-~`,#/|\\".ToCharArray();

            //Check for invalid characters
            for ( int i = 0 ; i < invalidChars.Length ; i++ )
            {
                if ( name.Contains(invalidChars[i]) ) 
                { 
                    return false; 
                }
            }

            //Otherwise, return true
            return true;
        }

        //Validate the Password
        private bool validatePassword()
        {
            bool validPass = false;
            string passwordText1 =  txtPassword1.Text;
            string passwordText2 =  txtPassword2.Text;

            if(passwordText1 != passwordText2) 
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

            for (int i = 0; i < passwordChars.Length; i++) {
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
                //If the old password was correctly entered
                //if ( UtilityManager.Encrypt( txtChangePassword.Text ) == myEntity.Password )
                //{
                //    if ( validatePassword() )
                //    {
                //        applicant.Password = UtilityManager.Encrypt( txtPassword1.Text );
                //        return true;
                //    }
                //    else
                //    {
                //        errorBuilder.Append( "New Password is Invalid." );
                //        return false;
                //    }
                //}
                //else
                //{
                //    errorBuilder.Append( "Old Password is Incorrect." );
                //    return false;
                //}
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
        /// <param name="recId"></param>
        private void PopulateControls()
        {
            SetOrgList();
            SetPubRolesList();
        } //


        private void SetPubRolesList()
        {

            DataSet ds = LDAL.DatabaseManager.DoQuery( this.pubRoleSql.Text);
            LDAL.DatabaseManager.PopulateList( this.ddlPubRole, ds, "Id", "Title", roleSelectTitle.Text  );
        } //

        private void SetOrgList()
        {

            DataSet ds = MyManager.GetRTTTOrganzations();
            LDAL.DatabaseManager.PopulateList( this.ddlOrganization, ds, "Id", "Name", "Select an organization" );
        }
        #endregion

    }
} 
