using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using LRWarehouse.Business.ResourceV2;
using ILPathways.Library;
using Isle.BizServices;
using System.Web.Script.Serialization;

namespace ILPathways.Controls.UberTaggerV1
{
  public partial class UberTaggerV1 : BaseUserControl
  {
    ResourceV2Services resourceV2Services = new ResourceV2Services();
    JavaScriptSerializer serializer = new JavaScriptSerializer();

    public List<FieldDB> Fields { get; set; }
    public List<UsageRights> UsageRights { get; set; }
    public string MyLibColData { get; set; }
    public ResourceDTO LoadedResourceData { get; set; }
    public string LoadedStandardsJSON { get; set; }
    public string LoadedKeywordsJSON { get; set; }
    public Dictionary<string, bool> CanUpdate { get; set; }

    protected void Page_Load( object sender, EventArgs e )
    {
      InitAlwaysFirst();

      if ( IsPostBack )
      {
        HandlePublishing();
      }
      else
      {

      }

      LoadResource();

      InitAlwaysLast();
    }

    //Stuff that runs regardless of postback
    private void InitAlwaysFirst()
    {
      //Ensure we have a user
      if ( !IsUserAuthenticated() )
      {
        Response.Redirect( "/Account/Login.aspx?nextUrl=" + Request.Url.PathAndQuery );
        return;
      }

      //Lock fields the user can't access
      CanUpdate = new Dictionary<string, bool>() //use schema for keys
      {
        { "title", false },
        { "description", false }
      };
      var permissions = SecurityManager.GetGroupObjectPrivileges( WebUser, "ILPathways.LRW.Pages.ResourceDetail" );
      if ( permissions.CreatePrivilege > ( int ) ILPathways.Business.EPrivilegeDepth.State )
      {
        CanUpdate[ "title" ] = true;
        CanUpdate[ "description" ] = true;
      }


    }
    private void InitAlwaysLast()
    {
      //Show or hide the testing mode checkbox
      lblTestingMode.Visible = ( ILPathways.classes.SessionManager.Get( Session, "CAN_VIEW_ADMIN_MENU", "missing" ) == "yes" );

      //Get Field code table data
      Fields = resourceV2Services.GetFieldAndTagCodeData();

      //Get Usage Rights code table data 
      UsageRights = resourceV2Services.GetUsageRightsList();

      //Get Library and Collection data for the user
      MyLibColData = "[]";
      var libraries = new Services.ESLibrary2Service().GetMyLibrariesWithContribute( WebUser.Id );
      var myLibrariesData = new List<DDLLibrary>();
      foreach ( var item in libraries )
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

        myLibrariesData.Add( lib );
      }
      MyLibColData = serializer.Serialize( myLibrariesData );

      //
    }

    //Load a saved Resource
    private void LoadResource()
    {
      LoadedResourceData = new ResourceDTO();
      LoadedStandardsJSON = "[]";
      LoadedKeywordsJSON = "[]";

      //Get resource ID
      var resourceID = int.Parse( Request.Params[ "resourceid" ] ?? "0" );
      //Get data if available
      LoadedResourceData = resourceV2Services.GetResourceDTO( resourceID );
      if ( resourceID == 0 || LoadedResourceData.ResourceId == 0 )
      {
        //Auto-assign values if new
        LoadedResourceData.Fields.Where( m => m.Schema == "inLanguage" ).FirstOrDefault().Tags.First().Selected = true;
        LoadedResourceData.Fields.Where( m => m.Schema == "accessRights" ).FirstOrDefault().Tags.First().Selected = true;
        LoadedResourceData.UsageRights.Id = 3;
        LoadedResourceData.UsageRights.Url = resourceV2Services.GetUsageRightsList().Where( m => m.Id == 3 ).FirstOrDefault().Url;
        CanUpdate[ "title" ] = true;
        CanUpdate[ "description" ] = true;
      }
      else
      {

        //Auto-assign values if not present
        if ( LoadedResourceData.Fields.Where( m => m.Schema == "accessRights" ).FirstOrDefault().Tags.Where( t => t.Selected ).Count() == 0 )
        {
          LoadedResourceData.Fields.Where( m => m.Schema == "accessRights" ).FirstOrDefault().Tags.First().Selected = true;
        }

        if ( LoadedResourceData.Fields.Where( m => m.Schema == "inLanguage" ).FirstOrDefault().Tags.Where( t => t.Selected ).Count() == 0 )
        {
          LoadedResourceData.Fields.Where( m => m.Schema == "inLanguage" ).FirstOrDefault().Tags.First().Selected = true;
        }
      }
      
      //Serialize data
      LoadedStandardsJSON = serializer.Serialize( LoadedResourceData.Standards );
      LoadedKeywordsJSON = serializer.Serialize( LoadedResourceData.Keywords );
      hdnResourceID.Value = LoadedResourceData.ResourceId.ToString();

      //Note error
      if ( LoadedResourceData.ResourceId == 0 && resourceID != 0 )
      {
        SetConsoleErrorMessage( "That is an invalid Resource ID. However, you can create a new resource below." );
        Response.Redirect( Request.Path ); //Start over
      }
    }

    //Handle publishing
    private void HandlePublishing()
    {
      //Determine publish or update
      var resourceID = int.Parse( hdnResourceID.Value );
      bool updateMode = resourceID > 0;

      //Get data holders
      var form = Request.Form;
      var keys = Request.Form.AllKeys;
      var resource = resourceV2Services.GetResourceDTO( resourceID );
      var errors = new List<string>();
      var util = new Services.UtilityService();
      var valid = true;
      var status = "";
      
      //Configure special fields (keywords, standards) in the event of error
      hdnKeywords.Value = hdnKeywords.Value;
      hdnStandards.Value = hdnStandards.Value;

      //Validate/Get data
      //Get fields first to precheck checkboxes in the event of text validation or other error
      var tagsRaw = keys.Where(m => m.IndexOf("cbx_") == 0).ToList();
      hdnTags.Value = serializer.Serialize( tagsRaw );
      var selectedTags = new List<int>();
      foreach ( var item in tagsRaw )
      {
        //Get the Tag ID
        var tag = int.Parse( item.Replace( "cbx_", "" ) );
        selectedTags.Add( tag );
        //In the field that contains this tag...                                                    ...Set the "selected" bool of this tag to true
        resource.Fields.Where( f => f.Tags.Where( t => t.Id == tag ).Count() == 1 ).FirstOrDefault().Tags.Where( t => t.Id == tag ).FirstOrDefault().Selected = true;
      }
      //Required tags
      if ( !FieldHasASelectedTag( resource.Fields, "learningResourceType" ) )
      {
        errors.Add( "You must select at least one Resource Type" );
      }
      if ( !FieldHasASelectedTag( resource.Fields, "mediaType" ) )
      {
        errors.Add( "You must select at least one Media Type" );
      }

      //Get drop-down list data
      //Usage Rights
      resource.UsageRights.Id = int.Parse( form[ "ddlUsageRights" ] );
      //Language
      var languageID = int.Parse( form[ "ddlLanguage" ] );
      resource.Fields.Where( m => m.Schema == "inLanguage" ).FirstOrDefault().Tags.Where( t => t.Id == languageID ).FirstOrDefault().Selected = true;
      if ( !selectedTags.Contains( languageID ) ) { selectedTags.Add( languageID ); }
      //Access Rights
      var accessRightsID = int.Parse( form[ "ddlAccessRights" ] );
      resource.Fields.Where( m => m.Schema == "accessRights" ).FirstOrDefault().Tags.Where( t => t.Id == accessRightsID ).FirstOrDefault().Selected = true;
      if ( !selectedTags.Contains( accessRightsID ) ) { selectedTags.Add( int.Parse( form[ "ddlAccessRights" ] ) ); }
      

      //Get Keywords
      var newKeywords = serializer.Deserialize<List<string>>( hdnKeywords.Value );
      foreach ( var item in newKeywords )
      {
        valid = true;
        status = "";
        util.ValidateText( item, 3, "Keyword \"" + item + "\"", ref valid, ref status );
        if ( valid )
        {
          resource.Keywords.Add( item );
        }
        else
        {
          errors.Add( status );
        }
      }
      if ( resource.Keywords.Count() == 0 ) { errors.Add( "You must add at least one Keyword." ); }

      //Get Standards
      resource.Standards = serializer.Deserialize<List<StandardsDTO>>( hdnStandards.Value );

      //Now get text inputs
      //URL
      if ( resourceID == 0 )
      {
        resource.Url = util.ValidateURL( form[ "txtURL" ], true, ref valid, ref status );
        if ( !valid ) { errors.Add( status ); }
        valid = true;
        status = "";
      }

      //Usage Rights
      resource.UsageRights.Url = util.ValidateURL( form[ "txtUsageRightsURL" ], false, ref valid, ref status );
      if ( !valid ) { errors.Add( status ); }
      valid = true;
      status = "";

      //Title
      if ( CanUpdate[ "title" ] )
      {
        resource.Title = ValidateText( util, form[ "txtTitle" ], 10, "Resource Title", ref errors );
      }

      //Description
      if ( CanUpdate[ "description" ] )
      {
        resource.Description = ValidateText( util, form[ "txtDescription" ], 25, "Description", ref errors );
      }

      //Creator
      resource.Creator = ValidateText( util, form[ "txtCreator" ], 0, "Creator", ref errors );

      //Publisher
      resource.Publisher = ValidateText( util, form[ "txtPublisher" ], 0, "Publisher", ref errors );
      if ( string.IsNullOrWhiteSpace( resource.Publisher ) )
      {
        if ( resource.Url.IndexOf( "ilsharedlearning" ) > -1 )
        {
          resource.Publisher = "ISLE OER";
        }
        else
        {
          try
          {
            resource.Publisher = new Uri( resource.Url ).Host;
          }
          catch { }
        }
      }

      //Submitter
      resource.Submitter = "ISLE OER on Behalf of " + WebUser.FullName();

      //Requirements
      resource.Requirements = ValidateText( util, form[ "txtRequirements" ], 0, "Technical/Equipment Requirements", ref errors );

      //If there are any errors, display them and return
      if ( errors.Count() > 0 )
      {
        foreach ( var item in errors )
        {
          SetConsoleErrorMessage( item );
        }
        return;
      }

      //Otherwise, publish
      //Publish
      valid = true;
      status = "";
      var version = ResourceBizService.ResourceVersion_GetByResourceId( resourceID );
      int versionID = version.Id;
      string sortTitle = version.SortTitle;
      string lrDocId = version.LRDocId;

      //If in testing mode, skip LR Publish
      if ( cbx_testingMode.Checked )
      {
        resource.Creator = "delete";
        resource.Publisher = "delete";
        resource.Description = "Test Data: " + resource.Description;
      }
      else
      {
        if ( resource.LrDocId != "" ) //Publish to LR only if we haven't done so yet
        {
          //Otherwise, do LR Publish
          PublishingServices.PublishToLearningRegistry( resourceV2Services.GetJSONLRMIPayloadFromResource( resource ), resource.Url, resource.Submitter, resource.Keywords, ref valid, ref status, ref lrDocId );
          if ( !valid )
          {
            SetConsoleErrorMessage( status );
            return;
          }
          resource.LrDocId = lrDocId;
        }
      }
        //TODO - add option to publish to an org
      int publishForOrgId = 0;

      //Publish to Database
      PublishingServices.PublishToDatabase( resource
                , publishForOrgId
                , selectedTags
                , ref valid
                , ref status
                , ref versionID
                , ref resourceID
                , ref sortTitle );
      if ( !valid )
      {
        SetConsoleErrorMessage( "There was an error adding the Resource to the database." );
        SetConsoleErrorMessage( "<div style=\"display: none;\">" + status + "</div>" );
        return;
      }
      resource.ResourceId = resourceID;
      resource.VersionId = versionID;
      resource.UrlTitle = sortTitle;
      valid = true;
      status = "";

      //Publish to ElasticSearch
      PublishingServices.PublishToElasticSearch( resourceID, ref valid, ref status );
      if ( !valid )
      {
        SetConsoleErrorMessage( "There was an error adding the Resource to the search." );
        SetConsoleErrorMessage( "<div style=\"display: none;\">" + status + "</div>" );
      }

      //Add to library
      try
      {
        var libraryId = int.Parse( form[ "ddlLibrary" ] );
        var collectionID = int.Parse( form[ "ddlCollection" ] );
        if ( libraryId != 0 && collectionID != 0 )
        {
          new Isle.BizServices.LibraryBizService().LibraryResourceCreate( libraryId, collectionID, resourceID, WebUser.Id, ref status );
        }
      }
      catch( Exception ex )
      {
        SetConsoleErrorMessage( "There was an error adding the Resource to your Library." );
        SetConsoleErrorMessage( "<div style=\"display: none;\">" + ex.Message + "</div>" );
      }

      //Thumbnail
      try
      {
        PublishingServices.GenerateThumbnail( resourceID, resource.Url );
      }
      catch ( Exception ex )
      {
        SetConsoleErrorMessage( "There was an error generating the Thumbnail" );
        SetConsoleErrorMessage( "<div style=\"display: none;\">" + ex.Message + "</div>" );
      }

      //Finish
      if ( updateMode )
      {
        SetConsoleSuccessMessage( "Your changes have been saved. <a href= \"/Resource/" + resourceID + "/" + sortTitle + "\">Click Here to view your Resource.</a>" );
      }
      else
      {
        SetConsoleSuccessMessage( "Your Resource has been published! <a href= \"/Resource/" + resourceID + "/" + sortTitle + "\">Click Here to view your Resource.</a>" );
      }
    }

    private bool FieldHasASelectedTag( List<FieldDTO> fields, string schema )
    {
      try
      {
        return fields.Where( f => f.Schema == schema ).FirstOrDefault().Tags.Where( t => t.Selected ).Count() > 0;
      }
      catch { return false; }
    }
    private string ValidateText( Services.UtilityService util, string text, int minimumLength, string title, ref List<string> errors )
    {
      var valid = true;
      var status = "";
      text = util.ValidateText( text, minimumLength, title, ref valid, ref status );
      if ( !valid ) { errors.Add( status ); }
      return text;
    }
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