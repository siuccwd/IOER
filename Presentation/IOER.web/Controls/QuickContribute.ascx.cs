using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Business;
using ILPathways.Controllers;
using ILPathways.Library;
using ILPathways.Utilities;
using Isle.BizServices;
using LRWarehouse.Business;

namespace ILPathways.Controls
{
  public partial class QuickContribute : BaseUserControl
  {
    public string mode { get; set; }
    public string libraryData { get; set; }
    public string maxFileSize { get; set; }
    public string selectedLibraryOutput { get; set; }
    public string selectedCollectionOutput { get; set; }
    public bool isDebugMode { get; set; }
    Services.UtilityService utilService = new Services.UtilityService();
    Services.PublisherService pubService = new Services.PublisherService();
    LibraryBizService libService = new LibraryBizService();

    protected void Page_Load( object sender, EventArgs e )
    {
      Startup(); //Happens regardless of postback status
      if ( IsPostBack )
      {

      }
      else
      {
        InitPage();
      }
    }

    public void Startup()
    {
      var user = new Patron();

      //Ensure the user is logged in
      if( IsUserAuthenticated() ){
        contributer.Visible = true;
        error.Visible = false;
        user = (Patron) WebUser;
      }
      else {
        contributer.Visible = false;
        error.Visible = true;
        return;
      }

      //Get the user's library data
      var libraries = new Services.ESLibrary2Service().GetMyLibraries( user );
      libraryData = "var libraryData = " + new JavaScriptSerializer().Serialize( libraries ) + ";";

      //Get Max File Size
      maxFileSize = "var maxFileSize = " + UtilityManager.GetAppKeyValue( "maxDocumentSize", "20000000" ) + ";";

      //Initialize preselected Library/Collection text
      selectedLibraryOutput = "var selectedLibraryID = 0";
      selectedCollectionOutput = "var selectedCollectionID = 0";

      //Show or hide the Debug mode checkbox
      if ( ILPathways.classes.SessionManager.Get( Session, "CAN_VIEW_ADMIN_MENU", "missing" ) == "yes" )
      {
        lblDebugMode.Visible = true;
      }
      else
      {
        lblDebugMode.Visible = false;
      }

    }

    public void InitPage()
    {
      //Pre-toggle the radio button
      if ( mode == "tag" )
      {
        rblTagWebpage.Checked = true;
        rblUploadFile.Checked = false;
      }
      else if ( mode == "upload" )
      {
        rblTagWebpage.Checked = false;
        rblUploadFile.Checked = true;
      }

    }

    public void btnSubmit_Click( object sender, EventArgs e )
    {
      int maxDocumentSize = UtilityManager.GetAppKeyValue("maxDocumentSize", 20000000);
      VirusScanner virScan = new VirusScanner(maxDocumentSize);
      bool canDoPublish = true; //Overall validation. May only be set to false, or remain true
      bool isValid = true; //Value manipulated by each method. May get set true/false repeatedly
      Patron user = new Patron();
      if( IsUserAuthenticated() )
      {
        user = (Patron) WebUser;
      }
      else 
      {
        SetConsoleErrorMessage("Error: Please login to Publish.");
        return;
      }

      //Mode
      if ( rblTagWebpage.Checked )
      {
        mode = "tag";
      }
      else if ( rblUploadFile.Checked )
      {
        mode = "upload";
        // Scan the file first!  If it's infected, there's NO NEED to do any additional processing!
        string scanResult = "OK";
        scanResult = virScan.Scan(fileFile.FileBytes);
        if (scanResult == "Infected")
        {
            SetConsoleErrorMessage("The file you are uploading appears to be infected with a virus! Please remove the infection before attempting to upload again.");
            return;
        }
        else if (scanResult.IndexOf("ERROR") > -1)
        {
            SetConsoleErrorMessage("An error occurred while scanning the uploaded file for viruses.");
            return;
        }
        else if (scanResult == "no scan done")
        {
            SetConsoleInfoMessage("The uploaded file was not scanned for viruses.");
        }
      }
      else
      {
        SetConsoleErrorMessage( "There was an error reading user input. Please try again." );
        return;
      }

      //Debugging
      isDebugMode = cbxDebug.Checked;

      //URL
      string publishingURL = GetPublishingURL( ref isValid );
      if ( !isValid )
      {
        canDoPublish = false;
      }

      //Title
      string title = ValidateText( txtTitle.Value, 5, "Title", ref canDoPublish );

      //Description
      string description = ValidateText( txtDescription.Value, 10, "Description", ref canDoPublish );

      //Keywords
      var rawKeywords = hdnKeywords.Value.Split( new string[] { "[separator]" }, StringSplitOptions.RemoveEmptyEntries );
      if ( rawKeywords.Length == 0 )
      {
        SetConsoleErrorMessage( "You must enter at least one keyword." );
      }
      var count = 1;
      var maxKeywords = 25;
      List<string> keywords = new List<string>();
      foreach ( string word in rawKeywords )
      {
        if ( count <= maxKeywords ) //Limit max keywords
        {
          keywords.Add( ValidateText( word, 4, "Keyword #" + count, ref isValid ) );
        }
        count++;
        if ( count > maxKeywords )
        {
          SetConsoleErrorMessage( "Too many keywords. The limit is " + maxKeywords + "." );
          canDoPublish = false;
          break;
        }
      }

      //Usage Rights
      var rights = RightsSelector.conditionsURL;

      //Tags
      var subjects = ValidateCBXL( cbxlSubject.GetCheckedItems(), 1, "Subjects", ref canDoPublish );
      var gradeLevels = ValidateCBXL( cbxlGradeLevel.GetCheckedItems(), 1, "Grade Level", ref canDoPublish );
      //var careerClusters = ValidateCBXL( cbxlCareerCluster.GetCheckedItems(), 1, "Career Cluster", ref canDoPublish ); //Not required
      var careerClusters = cbxlCareerCluster.GetCheckedItems();
      
      //Pull Library/Collection data out of the form
      var selectedLibrary = 0;
      var selectedCollection = 0;
      GetSelectedLibraryAndCollection( ref selectedLibrary, ref selectedCollection );

      //If the submission has survived all of that, do the publish
      if ( !canDoPublish )
      {
        SetConsoleErrorMessage( "Invalid Submission. Please make the necessary corrections." );
        return;
      }
      else
      {
        //Publish the Resource

        //Setup the input
        var resource = ConstructResource(
          user,
          publishingURL,
          title,
          description,
          keywords,
          rights,
          subjects,
          gradeLevels,
          careerClusters
        );

        //Do the publish
        bool successfulPublish = false;
        string publishStatus = "";
        int versionID = 0;
        int intID = 0;
        string sortTitle = "";
        new Controllers.PublishController().PublishToAll(resource, ref successfulPublish, ref publishStatus, ref versionID, ref intID, ref sortTitle, isDebugMode );

        //Add to the Selected Library and Collection
        if ( successfulPublish )
        {
            //TODO ==================
            //update the content item for resourceVersionId and contents rights id/url
            if ( createdContentItemId.Text != null && IsInteger( createdContentItemId.Text ) )
            {
                int contentId = Int32.Parse( createdContentItemId.Text );
                if ( contentId > 0 )
                {
                    ContentItem item = new ContentServices().Get( contentId );
                    if ( item != null && item.SeemsPopulated )
                    {
                        item.ResourceVersionId = resource.Version.Id;
                        item.StatusId = ContentItem.PUBLISHED_STATUS;
                        item.UseRightsUrl = rights;
                        item.IsPublished = true;

                        string statusMsg = new ContentServices().UpdateAfterQuickPub( item );
                        if ( item.DocumentRowId != null && item.IsValidRowId( item.DocumentRowId ) )
                        {
                            statusMsg = new ContentServices().DocumentVersion_SetToPublished( item.DocumentRowId.ToString() );
                        }
                    }
                }
            }
          //Add to library if available
          if ( selectedLibrary > 0 && selectedCollection > 0 )
          {
            string libStatus = "";
            var test = libService.LibraryResourceCreate( selectedCollection, intID, user.Id, ref libStatus );
            if ( test == 0 )
            {
              SetConsoleErrorMessage( "Error adding the Resource to that Library/Collection: " + libStatus );
            }
          }

          //Link to detail page
          SetConsoleSuccessMessage( "View your new Resource <a target=\"_blank\" href=\"/IOER/" + versionID + "/" + sortTitle + "\">here</a>. <b>Note:</b> Thumbnails take a few moments to generate--you may not immediately see the thumbnail for your new Resource." );

          //Reset the page
          txtURL.Value = "";
          txtTitle.Value = "";
          txtDescription.Value = "";
          hdnKeywords.Value = "";
          RightsSelector.selectedValue = "3";
          cbxlSubject.list.ClearSelection();
          cbxlGradeLevel.list.ClearSelection();
          cbxlCareerCluster.list.ClearSelection();
          selectedLibraryOutput = "var selectedLibraryID = 0";
          selectedCollectionOutput = "var selectedCollectionID = 0";
        }
        else
        {
          SetConsoleErrorMessage( "Error publishing your Resource: " + publishStatus );
          if ( mode == "tag" )
          {
            txtURL.Value = publishingURL; //Trying to force-restore this value
          }
        }
      }
    }

    public string GetPublishingURL( ref bool isValid )
    {
      if( mode == "upload" )
      {
        if ( fileFile.HasFile ) //Doing file upload & publish
        {
          //Do file upload, with needed checks
          bool isFileOkay = true;
          string status = "";
          var publishingURL = GetURLFromUploadedFile( fileFile, (Patron) WebUser, ref isFileOkay, ref status );
          if ( !isFileOkay )
          {
            SetConsoleErrorMessage( "Error: " + status );
            isValid = false;
            return null;
          }

          return publishingURL;
        }
        else
        {
          SetConsoleErrorMessage( "Error: Neither URL nor File values detected" );
          isValid = false;
          return null;
        }
      }
      else if ( mode == "tag" ) //Doing URL publish
      {
        //validate the URL
        bool isURLOkay = true;
        string status = "";
        string output = utilService.ValidateURL( txtURL.Value, true, ref isURLOkay, ref status );
        if ( !isURLOkay )
        {
          SetConsoleErrorMessage( "Error: " + status );
          isValid = false;
          return null;
        }

        isValid = true;
        return output;
      }
      else
      {
        SetConsoleErrorMessage( "There was an error reading user input. Please try again." );
        isValid = false;
        return null;
      }
    }

    public void GetSelectedLibraryAndCollection( ref int libraryID, ref int collectionID )
    {
      int testLibraryID = 0;
      int testCollectionID = 0;
      try
      {
        testLibraryID = int.Parse( Request.Form.GetValues( "ddlLibrary" )[ 0 ] );
        testCollectionID = int.Parse( Request.Form.GetValues( "ddlCollection" )[ 0 ] );
        if ( !libService.LibrarySection_DoesUserHaveEditAccess( testLibraryID, testCollectionID, WebUser.Id ) )
        {
          libraryID = 0;
          collectionID = 0;
        }
        else
        {
          libraryID = testLibraryID;
          collectionID = testCollectionID;
        }
      }
      catch ( Exception ex ) 
      {
        libraryID = 0;
        collectionID = 0;
      }
      selectedLibraryOutput = "var selectedLibraryID = " + libraryID + ";";
      selectedCollectionOutput = "var selectedCollectionID = " + collectionID + ";";
    }

    public string ValidateText( string input, int minLength, string title, ref bool canDoPublish )
    {
      bool isValid = true;
      string status = "";
      string output = utilService.ValidateText( input, minLength, title, ref isValid, ref status );
      if ( !isValid )
      {
        canDoPublish = false;
        SetConsoleErrorMessage( "Error: " + status );
        return "";
      }
      return output;
    }

    public List<ListItem> ValidateCBXL( List<ListItem> input, int minLength, string title, ref bool canDoPublish )
    {
      bool isValid = true;
      string status = "";
      utilService.ValidateCBXL( input, minLength, title, ref isValid, ref status );
      if ( !isValid )
      {
        canDoPublish = false;
        SetConsoleErrorMessage( "Error: " + status );
      }
      return input;
    }

    public Resource ConstructResource( Patron user, string url, string title, string description, List<string> keywords, string rights, List<ListItem> subject, List<ListItem> gradeLevel, List<ListItem> careerCluster )
    {
      var resource = new Resource();
      resource.Version = new ResourceVersion();
      //Created by ID
      resource.CreatedById = user.Id;
      resource.Version.CreatedById = user.Id;

      //URL
      resource.ResourceUrl = url;
      resource.Version.ResourceUrl = url;

      //Version Data
      resource.Version.Title = title;
      resource.Version.Description = description;
      resource.Version.Rights = rights;
      resource.Version.Submitter = user.FullName();
      if ( cbxDebug.Checked )
      {
        resource.Version.Creator = "delete";
        resource.Version.Publisher = "delete";
      }
      else
      {
        var host = ( mode == "upload" ? "ISLE OER" : new Uri( url ).Host );
        resource.Version.Creator = ( mode == "upload" ? user.FullName() : host );
        resource.Version.Publisher = host;
      }
      if ( mode == "upload" )
      {
        resource.Version.Created = DateTime.Now;
      }

      //Keywords
      foreach ( string word in keywords )
      {
        var output = new ResourceChildItem();
        output.OriginalValue = word;
        output.CreatedById = user.Id;
        resource.Keyword.Add( output );
      }

      //Tags
      resource.SubjectMap = ConstructList( subject );
      resource.Gradelevel = ConstructList( gradeLevel );
      resource.ClusterMap = ConstructList( careerCluster );
      resource.Language = new List<ResourceChildItem> { new ResourceChildItem() { OriginalValue = "English", CodeId = 1, CreatedById = user.Id } };

      return resource;
    }

    public List<ResourceChildItem> ConstructList( List<ListItem> tags )
    {
      var output = new List<ResourceChildItem>();
      foreach ( ListItem tag in tags )
      {
        var item = new ResourceChildItem();
        item.CodeId = int.Parse( tag.Value );
        item.OriginalValue = tag.Text;
        output.Add( item );
      }
      return output;
    }

    public string GetURLFromUploadedFile( FileUpload uploader, Patron user, ref bool isValid, ref string status )
    {
      //Do checks and upload file
        string url = "";

        ContentItem item = new ContentItem();
        item.Title = this.txtTitle.Value;
        item.TypeId = ContentItem.DOCUMENT_CONTENT_ID;
        item.Summary = this.txtDescription.Value;
        item.CreatedById = user.Id;
        item.OrgId = user.OrgId;
        item.PrivilegeTypeId = ContentItem.PUBLIC_PRIVILEGE;
        item.StatusId = ContentItem.SUBMITTED_STATUS;//set to 3, just incase of a failure before publish,then set after actual publish

        //how to get value from rights selector ==> will be done later
        item.ConditionsOfUseId = 1;
        int contentId = FileResourceController.CreateContentItemWithFileOnly( uploader, item, ref status );
        if ( contentId > 0 )
        {
            //TODO = ========= need to be able to get back to contentId in order to update it!
            createdContentItemId.Text = contentId.ToString();
            isValid = true;
            url = item.DocumentUrl;
        }
        else
            isValid = false;

      //Return the URL to the file
        return url;
    }
  }
}