using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using ILPathways.Utilities;
using Isle.BizServices;
using LRWarehouse.Business;
using ILPathways.Services;
using System.Web.Script.Serialization;

namespace ILPathways.Controls
{
  public partial class Publish3 : BaseUserControl
  {
    public string myLibrariesString { get; set; }
    List<DDLLibrary> libraries = new List<DDLLibrary>();
    LibraryBizService libService = new LibraryBizService();
    JavaScriptSerializer serializer = new JavaScriptSerializer();
    string formSecurityName = "Isle.Controls.CanPublish";
    string LRPublishAction = "";
    string RequiresApproval = "";
    int AuthoredResourceID = 0;

    protected void Page_Load( object sender, EventArgs e )
    {
      if ( !IsUserAuthenticated() )
      {
        Response.Redirect( "/Account/Login.aspx?nextUrl=" + Request.Url.PathAndQuery );
        return;
      }
      else
      {
        string allowingOpenPublishing = UtilityManager.GetAppKeyValue( "allowingOpenPublishing", "no" );
        var privileges = Isle.BizServices.SecurityManager.GetGroupObjectPrivileges( WebUser, formSecurityName );
        if ( privileges.CanCreate() == false && allowingOpenPublishing == "no" )
        {
          tagger.Visible = false;
          errorMessage.Visible = true;
          return;
        }
        else
        {
          tagger.Visible = true;
          errorMessage.Visible = false;

          if ( privileges.CanDelete() )
          {
            //Don't need this after all, probably
            //worknet_qualify.Visible = true;
          }

          if ( IsPostBack == false)
            PrefillAuthoredResourceInfo();
        }
      }

      //On publish
      if ( IsPostBack )
      {
        if ( ValidateInputs() )
        {
          PublishResource();
        }
      }

      //Regardless
      myLibrariesString = "var myLibraries = [];";
      if(IsUserAuthenticated()) 
      {
        LoadMyLibraries();
      }
    }

    void PrefillAuthoredResourceInfo()
    {
      //Check to see if the session has any data to fill
      var authoredResourceID = GetSessionItem( "authoredResourceID", 0 );
      var authoredResourceURL = GetSessionItem( "authoredResourceURL", "" );
      if ( authoredResourceID == 0 || authoredResourceURL == "" )
      {
        return;
      }

      //If so, load the item
      var contentManager = new ContentServices();
      var contentItem = contentManager.Get( authoredResourceID );
      AuthoredResourceID = authoredResourceID;
      createdContentItemId.Text = authoredResourceID.ToString();

      //Pre-populate fields
      //Creator
      txtCreator.Value = contentItem.HasOrg() ? contentItem.ContentOrg.Name : WebUser.FullName();
      //Publisher
      txtPublisher.Value = ILPathways.Utilities.UtilityManager.GetAppKeyValue( "defaultPublisher", "Illinois Shared Learning Environment" );

      if ( contentItem.IsValid )
      {
        txtURL.Value = authoredResourceURL;
        txtTitle.Value = contentItem.Title;
        txtDescription.Value = contentItem.Summary;
        usageRightsSelector.selectedValue = contentItem.ConditionsOfUseId.ToString();
        usageRightsSelector.conditionsURL = contentItem.ConditionsOfUseUrl;

        if ( contentItem.PrivilegeTypeId != Business.ContentItem.PUBLIC_PRIVILEGE )
        {
          LRPublishAction = "no";
        }
        if ( contentItem.IsOrgContent() )
        {
          RequiresApproval = "yes"; //If it requires approval, we can't publish it yet
          if ( contentItem.PrivilegeTypeId == Business.ContentItem.PUBLIC_PRIVILEGE )
          {
            LRPublishAction = "save";
          }
        }
      }

      //Remove the items from the session
      Session.Remove( "authoredResourceID" );
      Session.Remove( "authoredResourceURL" );

    }

    void LoadMyLibraries()
    {
      //var myLibraries = libService.Library_SelectListWithEditAccess( WebUser.Id );
        var myLibraries = new Services.ESLibrary2Service().GetMyLibrariesWithContribute( WebUser.Id );
      foreach ( var item in myLibraries )
      {
        var lib = new DDLLibrary()
        {
          id = item.id,
          title = item.title
        };

        foreach ( var col in item.collections )
        {
            lib.collections.Add( new DDLCollection()
            {
                id = col.id,
                title = col.title
            } );
        }
        //foreach ( var col in libService.LibrarySections_SelectListWithEditAccess( item.id, WebUser.Id ) )
        //{
        //  lib.collections.Add( new DDLCollection()
        //  {
        //    id = col.Id,
        //    title = col.Title
        //  } );
        //}

        libraries.Add( lib );
      }

      myLibrariesString = "var myLibraries = " + serializer.Serialize( libraries ) +";";
    }

    protected bool ValidateInputs()
    {
      var utilService = new UtilityService();
      bool valid = true;
      string status = "";
      /* Required */
      //Title
      txtTitle.Value = utilService.ValidateText( txtTitle.Value, 10, "Title", ref valid, ref status );
      if ( !valid )
      {
        SetConsoleErrorMessage( status );
        return false;
      }

      //URL
      txtURL.Value = utilService.ValidateURL( txtURL.Value, true, ref valid, ref status );
      if ( !valid )
      {
        SetConsoleErrorMessage( status );
        return false;
      }

      //Description
      txtDescription.Value = utilService.ValidateText( txtDescription.Value, 25, "Description", ref valid, ref status );
      if ( !valid )
      {
        SetConsoleErrorMessage( status );
        return false;
      }

      //Keywords
      foreach ( var item in serializer.Deserialize<List<string>>( hdnKeywords.Value ) )
      {
        utilService.ValidateText( item, 3, "Keyword", ref valid, ref status );
        if ( !valid )
        {
          SetConsoleErrorMessage( status );
          return false;
        }
      }

      //Language
      if ( ddlLanguage.GetCheckedItems().Count() == 0 )
      {
        SetConsoleErrorMessage( "You must select a Language" );
        return false;
      }

      //Access Rights
      if ( ddlAccessRights.GetCheckedItems().Count() == 0 )
      {
        SetConsoleErrorMessage( "You must select Access Rights" );
        return false;
      }

      //Usage Rights
      usageRightsSelector.conditionsURL = utilService.ValidateURL( usageRightsSelector.conditionsURL, false, ref valid, ref status );
      if ( !valid )
      {
        SetConsoleErrorMessage( status );
        return false;
      }

      //Resource Type
      if ( cbxlResourceType.GetCheckedItems().Count() == 0 )
      {
        SetConsoleErrorMessage( "You must select a Resource Type" );
        return false;
      }

      //Media Type
      if ( cbxlMediaType.GetCheckedItems().Count() == 0 )
      {
        SetConsoleErrorMessage( "You must select a Media Type" );
        return false;
      }

      /* Non-required */
      //Creator
      if(txtCreator.Value.Length > 0){
        txtCreator.Value = utilService.ValidateText( txtCreator.Value, 0, "Creator", ref valid, ref status );
        if ( !valid )
        {
          SetConsoleErrorMessage( status );
          return false;
        }
      }

      //Publisher
      if ( txtPublisher.Value.Length > 0 )
      {
        txtPublisher.Value = utilService.ValidateText( txtPublisher.Value, 0, "Creator", ref valid, ref status );
        if ( !valid )
        {
          SetConsoleErrorMessage( status );
          return false;
        }
      }

      //Requirements
      if ( txtRequirements.Value.Length > 0 )
      {
        txtRequirements.Value = utilService.ValidateText( txtRequirements.Value, 0, "Creator", ref valid, ref status );
        if ( !valid )
        {
          SetConsoleErrorMessage( status );
          return false;
        }
      }


      return valid;
    }

    void PublishResource()
    {
      //Setup objects
      var output = new Resource();
      output.Version = new ResourceVersion();

      //Fill objects

      //Title
      output.Version.Title = txtTitle.Value;
      //Resource URL
      output.ResourceUrl = txtURL.Value;
      output.Version.ResourceUrl = txtURL.Value;
      //Description
      output.Version.Description = txtDescription.Value;
      //Keywords
      foreach ( var item in serializer.Deserialize<List<string>>( hdnKeywords.Value ) )
      {
        output.Keyword.Add( new ResourceChildItem() { CreatedById = WebUser.Id, OriginalValue = item } );
      }


      //Language
      output.Language.Add( new ResourceChildItem() 
        { 
          CreatedById = WebUser.Id, 
          CodeId = int.Parse( ddlLanguage.GetCheckedItems().First().Value ), 
          OriginalValue = ddlLanguage.GetCheckedItems().First().Text 
        } 
      );
      //Access Rights
      output.Version.AccessRightsId = int.Parse( ddlAccessRights.GetCheckedItems().First().Value );
      output.Version.AccessRights = ddlAccessRights.GetCheckedItems().First().Text;
      //Usage Rights
      output.Version.Rights = usageRightsSelector.conditionsURL;
      //Creator
      output.Version.Creator = txtCreator.Value;
      //Publisher
      output.Version.Publisher = txtPublisher.Value;
      //Submitter
      output.Version.Submitter = WebUser.FullName();
      //Requirements
      output.Version.Requirements = txtRequirements.Value;
      //Learning Standards
      if ( hdnStandards.Value != "" )
      {
        foreach ( var item in serializer.Deserialize<List<InputStandard>>( hdnStandards.Value ) )
        {
          output.Standard.Add( new ResourceStandard()
            {
              CreatedById = WebUser.Id,
              StandardId = item.id,
              AlignmentTypeCodeId = item.alignment,
              StandardNotationCode = item.code,
              AlignedById = WebUser.Id
            }
          );
        }
      }
      //Resource Type
      output.ResourceType = ProcessCBXL( cbxlResourceType.GetCheckedItems() );
      //Media Type
      output.ResourceFormat = ProcessCBXL( cbxlMediaType.GetCheckedItems() );
      //K-12 Subject
      output.SubjectMap = ProcessCBXL( cbxlK12Subject.GetCheckedItems() );
      //Educational Use
      output.EducationalUse = ProcessCBXL( cbxlEducationalUse.GetCheckedItems() );
      //Career Cluster
      output.ClusterMap = ProcessCBXL( cbxlCareerCluster.GetCheckedItems() );
      //Grade Level
      output.Gradelevel = ProcessCBXL( cbxlGradeLevel.GetCheckedItems() );
      //End User
      output.Audience = ProcessCBXL( cbxlEndUser.GetCheckedItems() );
      //Group Type
      output.GroupType = ProcessCBXL( cbxlGroupType.GetCheckedItems() );

      bool success = true;
      string status = "";
      int versionID = 0;
      int intID = 0;
      string sortTitle = "";

      //**** add cheat. If the CI status is not published, the page in the image will indicate a private page
      Int32.TryParse( createdContentItemId.Text, out AuthoredResourceID );
      if ( AuthoredResourceID > 0 )
      {
          string statusMsg = new ContentServices().UpdateAfterPublish( AuthoredResourceID );
      }


      var publishController = new Controllers.PublishController();
      if ( LRPublishAction == "no" )
      {
        publishController.PublishToDatabase( output, ref success, ref status, ref versionID, ref intID, ref sortTitle );
      }
      else if ( LRPublishAction == "save" )
      {
        publishController.PublishToDatabase( output, ref success, ref status, ref versionID, ref intID, ref sortTitle );
        output.Id = intID;
        output.Version.Id = versionID;
        publishController.BuildSaveLRDocument( output, ref success, ref status );
      }
      else
      {
        publishController.PublishToAll( output, ref success, ref status, ref versionID, ref intID, ref sortTitle, true, false );
      }

      if ( success )
      {
        SetConsoleSuccessMessage( "Your Resource has been Published. <a href=\"/Resource/" + intID + "/" + sortTitle + "\">Click Here to view your Resource</a>." );
        Patron user = GetAppUser();

        //Add to library
        if ( Request.Params[ "ddlLibrary" ] != "0" && Request.Params["ddlCollection"] != "0" )
        {
          var libraryID = int.Parse( Request.Params[ "ddlLibrary" ] );
          var collectionID = int.Parse( Request.Params[ "ddlCollection" ] );

          //new Isle.BizServices.LibraryBizService().LibraryResourceCreate( collectionID, intID, WebUser.Id, ref status );
          new Isle.BizServices.LibraryBizService().LibraryResourceCreate( libraryID, collectionID, intID, user, ref status );
        }

        if ( AuthoredResourceID > 0 )
        {
          bool hasApproval = false;
          string statusMessage2 = "";
          bool isValid = ILPathways.Controllers.ContentController.HandleContentApproval( output, AuthoredResourceID, user, ref hasApproval, ref statusMessage2 );
          if ( hasApproval )
          {
            SetConsoleSuccessMessage("<p>NOTE: Approval is required, an email has been sent requesting a review of this resource.</p>");
          }
        }
      }
      else
      {
        SetConsoleErrorMessage( status );
      }
    }

    protected List<ResourceChildItem> ProcessCBXL( List<ListItem> input )
    {
      var output = new List<ResourceChildItem>();
      foreach ( var item in input )
      {
        output.Add( new ResourceChildItem() { CreatedById = WebUser.Id, CodeId = int.Parse(item.Value), OriginalValue = item.Text } );
      }
      return output;
    }

    public class InputStandard
    {
      public int id { get; set; }
      public int alignment { get; set; }
      public string code { get; set; }
    }

    public class DDLItem
    {
      public int id { get; set; }
      public string title { get; set; }
    }

    public class DDLLibrary : DDLItem
    {
      public DDLLibrary()
      {
        collections = new List<DDLCollection>();
      }
      public List<DDLCollection> collections { get; set; }
    }
    public class DDLCollection : DDLItem
    {
    }

  }
}