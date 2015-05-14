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
    public string orgData { get; set; }
    public string selectedOrgOutput { get; set; }
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
      Startup();
    }

    public void Startup()
    {
        // get org members where can contribute
        var orgs = OrganizationBizService.OrganizationMembersCodes_WithContentPrivileges( WebUser.Id );
        orgData = "var orgData = " + new JavaScriptSerializer().Serialize( orgs ) + ";";
        selectedOrgOutput = "var selectedOrgID = 0";
        var publishForOrgId = GetSelectedOrg();

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
      var resource = new Resource();
      resource.Version = new ResourceVersion();

      //Fill objects
      resource.CreatedById = WebUser.Id;
      resource.PublishedForOrgId = 0;       //************* TBD ********
      //Title
      resource.Version.Title = txtTitle.Value;
      //Resource URL
      resource.ResourceUrl = txtURL.Value;
      resource.Version.ResourceUrl = txtURL.Value;
      //Description
      resource.Version.Description = txtDescription.Value;
      //Keywords
      foreach ( var item in serializer.Deserialize<List<string>>( hdnKeywords.Value ) )
      {
        resource.Keyword.Add( new ResourceChildItem() { CreatedById = WebUser.Id, OriginalValue = item } );
      }


      //Language
      resource.Language.Add( new ResourceChildItem() 
        { 
          CreatedById = WebUser.Id, 
          CodeId = int.Parse( ddlLanguage.GetCheckedItems().First().Value ), 
          OriginalValue = ddlLanguage.GetCheckedItems().First().Text 
        } 
      );
      //Access Rights
      resource.Version.AccessRightsId = int.Parse( ddlAccessRights.GetCheckedItems().First().Value );
      resource.Version.AccessRights = ddlAccessRights.GetCheckedItems().First().Text;
      //Usage Rights
      resource.Version.Rights = usageRightsSelector.conditionsURL;
      //Creator
      resource.Version.Creator = txtCreator.Value;
      //Publisher
      resource.Version.Publisher = txtPublisher.Value;
      //Submitter
      resource.Version.Submitter = WebUser.FullName();
      //Requirements
      resource.Version.Requirements = txtRequirements.Value;
      //Learning Standards
      if ( hdnStandards.Value != "" )
      {
        foreach ( var item in serializer.Deserialize<List<InputStandard>>( hdnStandards.Value ) )
        {
          resource.Standard.Add( new ResourceStandard()
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
      resource.ResourceType = ProcessCBXL( cbxlResourceType.GetCheckedItems() );
      //Media Type
      resource.ResourceFormat = ProcessCBXL( cbxlMediaType.GetCheckedItems() );
      //K-12 Subject
      resource.SubjectMap = ProcessCBXL( cbxlK12Subject.GetCheckedItems() );
      //Educational Use
      resource.EducationalUse = ProcessCBXL( cbxlEducationalUse.GetCheckedItems() );
      //Career Cluster
      resource.ClusterMap = ProcessCBXL( cbxlCareerCluster.GetCheckedItems() );
      //Grade Level
      resource.Gradelevel = ProcessCBXL( cbxlGradeLevel.GetCheckedItems() );
      //End User
      resource.Audience = ProcessCBXL( cbxlEndUser.GetCheckedItems() );
      //Group Type
      resource.GroupType = ProcessCBXL( cbxlGroupType.GetCheckedItems() );

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

      //TODO - add option to publish to an org   **************************
      int publishForOrgId = GetSelectedOrg();

      //var publishController = new Controllers.PublishController();
      if ( LRPublishAction == "no" )
      {
          PublishingServices.PublishToDatabase( resource, 
              publishForOrgId,
            ref success, 
            ref status, 
            ref versionID, 
            ref intID, 
            ref sortTitle );

          //log - could have been a test, or could have been another reason?
          new ActivityBizServices().PublishActivity( resource, WebUser, "Saved (skipped published)" );
      }
      else if ( LRPublishAction == "save" )
      {
        PublishingServices.PublishToDatabase( resource,
            publishForOrgId,
                ref success, 
                ref status, 
                ref versionID, 
                ref intID, 
                ref sortTitle );

        resource.Id = intID;
        resource.Version.Id = versionID;
        PublishingServices.BuildSaveLRDocument( resource, ref success, ref status );

          //not sure if this should be logged - until actually done?
        new ActivityBizServices().PublishActivity( resource, WebUser, "Saved (not published)" );
      }
      else
      {
          PublishingServices.PublishToAll( resource, 
            ref success, 
            ref status, 
            ref versionID, 
            ref intID, 
            ref sortTitle, 
            true, 
            false,
            (Patron) WebUser,
            publishForOrgId);
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
          bool isValid = ILPathways.Controllers.ContentController.HandleContentApproval( resource, AuthoredResourceID, user, ref hasApproval, ref statusMessage2 );
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
    public int GetSelectedOrg()
    {
        int orgID = 0;
        try
        {
            orgID = int.Parse( Request.Form.GetValues( "ddlOrg" )[ 0 ] );
        }
        catch ( Exception ex )
        {
            orgID = 0;

        }
        selectedOrgOutput = "var selectedOrgID = " + orgID + ";";
        return orgID;

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