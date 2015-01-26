using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Common;
using ILPathways.Library;
using ILPathways.Utilities;
using ILPathways.Controllers;
using ILPathways.Business;
using Isle.BizServices;

using LRWarehouse.Business;
using LRWarehouse.DAL;



namespace ILPathways.Account.controls
{
  public partial class UserProfile2 : BaseUserControl
  {
    public string avatarURL { get; set; }

    Services.UtilityService utilService = new Services.UtilityService();
    bool isValid;
    string status;

    protected void Page_Load( object sender, EventArgs e )
    {
      if ( !IsUserAuthenticated() )
      {
        Response.Redirect( "/Account/Login.aspx?nextUrl=/Account/Profile.aspx" );
      }
      if ( !IsPostBack )
      {
        InitPage();
      }
    }

    public void InitPage()
    {
      //Force a refresh of the patron object from the database
      var user = new AccountServices().Get( WebUser.Id );
      WebUser = ( Business.IWebUser ) user;

        //always check/refesh org mbrs
      OrganizationBizService.FillUserOrgsMbrs( WebUser );

      //Display some current info
      email1.Value = user.Email;
      email2.Value = user.Email;
      firstName.Value = user.FirstName;
      lastName.Value = user.LastName;
      avatarURL = user.UserProfile.ImageUrl + "?rand=" + (new Random().Next(1000000));

      DataSet ds = CodeTableBizService.Resource_CodeTableSelect( "Codes.AudienceType" );
      DatabaseManager.PopulateList( this.ddlPubRole, ds, "Id", "Title", "Select a publishing role" );

      if ( user.HasUserProfile() )
      {
          jobTitle.Value = user.UserProfile.JobTitle;
          this.SetListSelection( ddlPubRole, user.UserProfile.PublishingRoleId.ToString());

          txtProfile.Text = user.UserProfile.RoleProfile;
          if ( user.UserProfile.Organization != null && user.UserProfile.Organization.Length > 0 )
              this.lblOrg.Value = user.UserProfile.Organization;
      }

      //Clear out text boxes
      password1.Value = "";
      password2.Value = "";
    }

    public void btnUpdateAccount_Click( object sender, EventArgs e )
    {
      if ( IsUserAuthenticated() )
      {
        var user = ( Patron ) WebUser;

        //Email
        status = "";
        isValid = false;
        var inputEmail = "";
        if ( email1.Value != "" )
        {
          if ( email1.Value == email2.Value )
          {
            bool alreadyExists = false;
            inputEmail = email1.Value;
            inputEmail = utilService.ValidateEmail( inputEmail, ref isValid, ref status, ref alreadyExists );
            if ( !isValid )
            {
              SetConsoleErrorMessage( status );
              return;
            }
          }
          else
          {
            SetConsoleErrorMessage( "Email fields do not match." );
            return;
          }
        }

        //Password
        status = "";
        isValid = false;
        var inputPassword = "";
        if ( password1.Value != "" )
        {
          if ( password1.Value == password2.Value )
          {
            bool hasBonus = false;
            inputPassword = password1.Value;
            inputPassword = utilService.ValidatePassword( inputPassword, ref isValid, ref status, ref hasBonus );
            if ( !isValid )
            {
              SetConsoleErrorMessage( status );
              return;
            }
          }
          else
          {
            SetConsoleErrorMessage( "Password fields do not match." );
            return;
          }
        }

        //Name
        var inputFirstName = "";
        if ( firstName.Value != "" )
        {
          inputFirstName = utilService.ValidateName( firstName.Value, 1, "First Name", ref isValid, ref status );
          if ( !isValid )
          {
            SetConsoleErrorMessage( status );
            return;
          }
        }
        var inputLastName = "";
        if ( lastName.Value != "" )
        {
          status = "";
          isValid = false;
          inputLastName = utilService.ValidateName( lastName.Value, 1, "Last Name", ref isValid, ref status );
          if ( !isValid )
          {
            SetConsoleErrorMessage( status );
            return;
          }
        }

        //Update profile
        bool makingChanges = false;
        if ( inputEmail != "" )
        {
          user.Email = inputEmail;
          makingChanges = true;
        }
        if ( inputPassword != "" )
        {
          user.Password = Isle.BizServices.AccountServices.Encrypt( inputPassword );
          makingChanges = true;
        }
        if ( inputFirstName != "" )
        {
          user.FirstName = inputFirstName;
          makingChanges = true;
        }
        if ( inputLastName != "" )
        {
          user.LastName = inputLastName;
          makingChanges = true;
        }

        if ( makingChanges )
        {
          new Isle.BizServices.AccountServices().Update( user );
          SetConsoleSuccessMessage( "Your account was updated successfully." );

          //Reload things
          InitPage();
        }
        else
        {
          SetConsoleInfoMessage( "No changes to make!" );
        }

      }
      else
      {
        SetConsoleErrorMessage( "You must login to update your Profile." );
        Response.Redirect( "/Account/Login.aspx?nextUrl=/Account/Profile.aspx" );
      }
    }

    public void btnUpdateAvatar_Click( object sender, EventArgs e )
    {
      if ( IsUserAuthenticated() )
      {
        var user = ( Patron ) WebUser;

        try
        {
            if ( fileAvatar.HasFile == false || fileAvatar.FileName == "" )
            {
                SetConsoleErrorMessage( "An image was not provided.<br/>Please select a valid image and try again!" );
                return;

            }

          int imageWidth = UtilityManager.GetAppKeyValue( "libraryImageWidth", 150 );
          string ext = System.IO.Path.GetExtension( fileAvatar.FileName );
          if ( ext == null || ext.Trim().Length == 0 )
          {
              SetConsoleErrorMessage( "The selected file does not have an extention type. Please select a valid image and try again!" );
              isValid = false;
              return;
          }
          string savingName = user.RowId.ToString().Replace( "-", "" ) + System.IO.Path.GetExtension( fileAvatar.FileName );
          string savingFolder = FileResourceController.DetermineDocumentPath( user.Id, 0 );
          string savingURL = FileResourceController.DetermineMyImageUrl( user.Id, savingName );
          var img = new ILPathways.Business.ImageStore();
          img.FileName = savingName;
          img.FileDate = DateTime.Now;

          FileResourceController.HandleImageResizingToWidth( img, fileAvatar, imageWidth, imageWidth, false, true );
          FileSystemHelper.HandleImageCaching( savingFolder, img, true );

          bool isNewProfile = false;
          //get the current profile
          PatronProfile profile = new AccountServices().PatronProfile_Get( user.Id );
          if ( profile == null )
          {
              profile = new PatronProfile();
              profile.UserId = user.Id;
              profile.ImageUrl = "";
              isNewProfile = true;
          }

          if ( profile.ImageUrl != savingURL )
          {
              string statusMessage = "";
              profile.ImageUrl = savingURL;

              if ( profile.UserId == 0 )
                  profile.UserId = user.Id;
              if ( isNewProfile )
                  new AccountServices().PatronProfile_Create( profile, ref statusMessage );
              else 
                  new AccountServices().PatronProfile_Update( profile );
          }

        }
        catch ( Exception ex )
        {
          status = ex.Message;
          LoggingHelper.LogError( ex, "Profile().HandleUpload" );
          SetConsoleErrorMessage( "There was an error processing your request. Please try again later." );
          isValid = false;
        }

        InitPage();
      }
    }

    public void btnUpdateProfile_Click( object sender, EventArgs e )
    {
      if ( IsUserAuthenticated() )
      {
        //Setup
          Patron user = ( Patron ) WebUser;
        status = "";
        isValid = false;
        bool makingChanges = false;

        //User Role
        if ( ddlPubRole.SelectedIndex > 0 && ddlPubRole.SelectedValue != user.UserProfile.PublishingRoleId.ToString() )
        {
          user.UserProfile.PublishingRoleId = int.Parse( ddlPubRole.SelectedValue );
          makingChanges = true;
        }

        //Job Title
        var inputJobTitle = "";
        if ( jobTitle.Value != "" )
        {
          inputJobTitle = utilService.ValidateText( jobTitle.Value, 5, "Job Title", ref isValid, ref status );
          if ( !isValid )
          {
            SetConsoleErrorMessage( status );
            return;
          }
        }

        //Job Profile
        status = "";
        isValid = false;
        var inputTxtProfile = "";
        if ( txtProfile.Text != "" )
        {
          inputTxtProfile = utilService.ValidateText( txtProfile.Text, 25, "Job Profile", ref isValid, ref status );
          if ( !isValid )
          {
            SetConsoleErrorMessage( status );
            return;
          }
        }

        //Do the updates
        
        if ( inputJobTitle != "" )
        {
          user.UserProfile.JobTitle = inputJobTitle;
          makingChanges = true;
        }

        if ( inputTxtProfile != "" )
        {
          user.UserProfile.RoleProfile = inputTxtProfile;
          makingChanges = true;
        }

        if ( makingChanges )
        {
            //just in case
            if ( user.UserProfile.UserId == 0 )
                user.UserProfile.UserId = user.Id;

          new Isle.BizServices.AccountServices().PatronProfile_Update( user.UserProfile );
          SetConsoleSuccessMessage( "Successfully updated your profile!" );

          InitPage();
        }
        else
        {
          SetConsoleInfoMessage( "No changes to make!" );
        }
      }
    }

  }
}