using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Business;
using IOER.Controllers;
using IOER.Library;
using ILPathways.Utilities;
using Isle.BizServices;
using LRWarehouse.Business;
using BDM = LRWarehouse.DAL.BaseDataManager;

namespace IOER.Controls
{
  public partial class QuickContribute : BaseUserControl
  {
    public string mode { get; set; }
    public string orgData { get; set; }
    public string libraryData { get; set; }
    public string maxFileSize { get; set; }
    public string selectedOrgOutput { get; set; }
    public string selectedLibraryOutput { get; set; }
    public string selectedCollectionOutput { get; set; }
    public bool isDebugMode { get; set; }
    public string gooruId {get; set; }
    public string isCurriculumMode { get; set; }
    public string gooruSessionToken { get; set; }

    private int currentNodeID { get; set; }
    private bool isCurriculumNode { get; set; }
    private List<ContentStandard> contentStandards = new List<ContentStandard>();


    public string parentNodeId
    {
        get
        {
            return txtParentNodeId.Text;
        }
        set
        {
            txtParentNodeId.Text = value;
        }

    }
    public string currentParentNodeId { get; set; }
    public string postedResourceIntID { get; set; }
    public string rememberedItems 
    {
        get { return this.txtRememberedItems.Text; }
        set { this.txtRememberedItems.Text = value; }
    }

    InfoMemoryJSON memory = new InfoMemoryJSON();
    Services.UtilityService utilService = new Services.UtilityService();
    LibraryBizService libService = new LibraryBizService();
    ContentServices myManager = new ContentServices();

    /// <summary>
    /// Set value used when check form privileges
    /// </summary>
    public string FormSecurityName
    {
        get { return this.txtFormSecurityName.Text; }
        set { this.txtFormSecurityName.Text = value; }
    }

    protected void Page_Load( object sender, EventArgs e )
    {
      Startup(); //Happens regardless of postback status
      if ( IsPostBack )
      {
        SubmitForm();
      }
      else 
      {
        InitPage();
      }
    }

    public void Startup()
    {
      var user = new Patron();

      //Ensure the user is logged in and can publish
      if ( CanPublish(user) == false )
      {
          contributer.Visible = false;
          return;
      }

      user = ( Patron ) WebUser;

        // get org members where can contribute
      var orgs = OrganizationBizService.OrganizationMembersCodes_WithContentPrivileges( user.Id );
      orgData = "var orgData = " + new JavaScriptSerializer().Serialize( orgs ) + ";";
      selectedOrgOutput = "var selectedOrgID = 0";

      //Get the user's library data
      var libraries = new Services.ESLibrary2Service().GetMyLibrariesWithContribute( user );
      libraryData = "var libraryData = " + new JavaScriptSerializer().Serialize( libraries ) + ";";

      //Get Max File Size
      maxFileSize = "var maxFileSize = " + UtilityManager.GetAppKeyValue( "maxDocumentSize", "20000000" ) + ";";

      //Initialize preselected Library/Collection text
      selectedLibraryOutput = "var selectedLibraryID = 0";
      selectedCollectionOutput = "var selectedCollectionID = 0";

      //Show or hide the Debug mode checkbox
      if ( IOER.classes.SessionManager.Get( Session, "CAN_VIEW_ADMIN_MENU", "missing" ) == "yes" )
      {
        lblDebugMode.Visible = true;
      }
      else
      {
        lblDebugMode.Visible = false;
      }

      if (Request.Params["gooruId"] != null){

          gooruId = Request.Params["gooruId"]; 
          gooruSearch.Visible= true;
          try
          {
			  //HACK WARNING
            gooruSessionToken = new IOER.Pages.GooruSearch().GetSessionToken();
          }
          catch ( Exception ex )
          {
            gooruSessionToken = ex.Message;
          }
      }

      if ( string.IsNullOrWhiteSpace( postedResourceIntID ) )
      {
        postedResourceIntID = "var resourceIntID = 0;";
      }

      //Curriculum content standards things
      isCurriculumMode = "var curriculumMode = false;";
      if ( Request.Params[ "nodeId" ] != null || Request.Params[ "contentId" ] != null )
      {
        isCurriculumMode = "var curriculumMode = true;";
        isCurriculumNode = true;
        string number = Request.Params[ "nodeId" ] ?? Request.Params[ "contentId" ] ?? "0";
        currentNodeID = int.Parse( number );
      }


    }

    private bool CanPublish(Patron user)
    {
        bool canPublish = false;
        if ( IsUserAuthenticated() == false )
        {
            notLoggedInError.Visible = true;
            return false;
        }

        if ( UtilityManager.GetAppKeyValue( "allowingOpenPublishing", "no" ) == "yes" )
            return true;

        user = ( Patron ) WebUser;
        //check if can publish

        if ( FormSecurityName.Length > 0 )
        {
            this.FormPrivileges = SecurityManager.GetGroupObjectPrivileges( this.WebUser, FormSecurityName );

            if ( FormPrivileges.CanCreate() == true )
            {
                canPublish = true;
            }
            else
            {
                notAuthorizedError.Visible = true;
            }
        }
        else
        {
            //another approach to allow open publishing would be to set FormSecurityName to blank
            canPublish = true;
        }

        return canPublish;
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
      
      doingLRPublish.Text = this.GetRequestKeyValue( "doingLRPublish", "yes" );

      rememberedItems = "var rememberedItems = { use: false }";
      parentNodeId = this.GetRequestKeyValue( "nodeId", "0" );
      currentParentNodeId = "var currentParentNodeId = " + parentNodeId + ";";
      postedResourceIntID = "var resourceIntID = 0;";

      DataSet ds = myManager.ContentPrivilegeCodes_Select();

      BDM.PopulateList( this.ddlContentFilePrivacyLevel, ds, "Id", "Title" );
      int contentId = this.GetRequestKeyValue( "contentId", 0 );
      if ( contentId > 0 )
      {
          PopulateContentItem( contentId );
      }
      else
      {
          string gooruId = this.GetRequestKeyValue( "gooruId", "" );
          //if (gooruId.Length == 36)
          //    PopulateGooruItem( gooruId ).Wait();
      }
    }

    //static async Task PopulateGooruItem( string gooruId )
    //{
    //    using (var client = new HttpClient())
    //    {
    //        client.BaseAddress = new Uri("http://localhost:9000/");
    //        client.DefaultRequestHeaders.Accept.Clear();
    //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    //        // New code:
    //        HttpResponseMessage response = await client.GetAsync("api/products/1");
    //        if (response.IsSuccessStatusCode)
    //        {
    //            ILPathways.Business.Library product = await response.Content.ReadAsAsync<ILPathways.Business.Library>();

    //            Console.WriteLine("{0}\t${1}\t{2}", product.Title, product.ImageUrl, product.LibraryTypeId);
    //        }
    //    }
    //}
 

    public void PopulateContentItem( int contentId )
    {
        ContentItem item = new ContentServices().Get( contentId );
        if ( item != null && item.Id > 0 )
        {
            if ( item.HasResourceId() )
            {
                SetConsoleErrorMessage( "The provided content item has already been published. Use the resource detail page to make updates." );

            }
            else
            {
                txtTitle.Value = item.Title;
                txtDescription.Value = item.Summary;
                txtURL.Value = ContentServices.FormatContentFriendlyUrl( item );
            }
        }
        else
        {

        }
   }

    public void SubmitForm()
    {
        Resource resource = new Resource();
        Patron user = new Patron();
        if ( IsUserAuthenticated() )
        {
            user = ( Patron )WebUser;
        }
        else
        {
            SetConsoleErrorMessage( "Error: Please login to Publish." );
            return;
        }

        if ( IsFormValid( user, ref resource ) )
        {
            DoUpdate( user, ref resource );
		}
	 }


    /// <summary>
    /// validate form
    /// If valid, the resource parameter is loaded for use in update
    /// </summary>
    /// <param name="user"></param>
    /// <param name="resource"></param>
    /// <returns></returns>
    private bool IsFormValid( Patron user, ref Resource resource)
    {

		int maxDocumentSize = UtilityManager.GetAppKeyValue("maxDocumentSize", 20000000);
      VirusScanner virScan = new VirusScanner(maxDocumentSize);
      bool canDoPublish = true; //Overall validation. May only be set to false, or remain true
      bool isValid = true; //Value manipulated by each method. May get set true/false repeatedly


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
        scanResult = virScan.Scan( fileFile.FileBytes );

        //scanResult = "no scan done";
        if ( scanResult == "Infected" )
        {
            SetConsoleErrorMessage( "The file you are uploading appears to be infected with a virus! Please remove the infection before attempting to upload again." );
            return false;
        }
        else if ( scanResult.IndexOf( "ERROR" ) > -1 )
        {
            SetConsoleErrorMessage( "An error occurred while scanning the uploaded file for viruses." );
            return false;
        }
        else if ( scanResult == "no scan done" )
        {
            SetConsoleInfoMessage( "The uploaded file was not scanned for viruses." );
        }
      
      }
      else
      {
        SetConsoleErrorMessage( "There was an error reading user input. Please try again." );
        return false;
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
      if ( isDebugMode )
      {
        description = "testdata: " + description;
      }

      //Keywords
      var rawKeywords = hdnKeywords.Value.Split( new string[] { "[separator]" }, StringSplitOptions.RemoveEmptyEntries );
      if ( rawKeywords.Length == 0 )
      {
        SetConsoleErrorMessage( "You must enter at least one keyword." );
      }
      var count = 1;
      var maxKeywords = UtilityManager.GetAppKeyValue("maxKeywords", 50); ;
      List<string> keywords = new List<string>();
      foreach ( string word in rawKeywords )
      {
        if ( count <= maxKeywords ) //Limit max keywords
        {
          var test = ValidateText( word, 3, "Keyword #" + count, ref isValid );
          if ( isValid )
          {
            keywords.Add( test );
            memory.keywords.Add( test );
          }
          //keywords.Add( ValidateText( word, 4, "Keyword #" + count, ref isValid ) );
        }
        count++;
        if ( count > maxKeywords )
        {
          SetConsoleErrorMessage( "Too many keywords. The limit is " + maxKeywords + "." );
          canDoPublish = false;
          break;
        }
      }

      //Standards
      var standards = new ResourceStandardCollection();
      var raw = hdnStandards.Value.Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries );
      if ( raw.Count() > 0 )
      {
          foreach ( var item in raw )
          {
              try
              {
                  var data = item.Split( new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries );
                  var id = int.Parse( data[ 0 ] );
                  var alignment = int.Parse( data[ 1 ] );
                  var type = int.Parse( data[ 2 ] );
                  standards.Add( new ResourceStandard() { StandardId = id, AlignmentTypeCodeId = alignment, CreatedById = user.Id } );
                  contentStandards.Add( new ContentStandard() { StandardId = id, AlignmentTypeCodeId = alignment, CreatedById = user.Id, UsageTypeId = type, ContentId = currentNodeID } );
              }
              catch { }
          }
      }
      //Usage Rights
      var rights = RightsSelector.conditionsURL;
      memory.usageRightsIndex = RightsSelector.selectedIndex;
      memory.usageRightsURL = RightsSelector.conditionsURL;

      //Tags
      var subjects = ValidateCBXL( cbxlSubject.GetCheckedItems(), 1, "Subjects", ref canDoPublish );
      var gradeLevels = ValidateCBXL( cbxlGradeLevel.GetCheckedItems(), 1, "Grade Level", ref canDoPublish );
      //var careerClusters = ValidateCBXL( cbxlCareerCluster.GetCheckedItems(), 1, "Career Cluster", ref canDoPublish ); //Not required
      var careerClusters = cbxlCareerCluster.GetCheckedItems();


      //If the submission has survived all of that, do the publish
      if ( !canDoPublish )
      {
        SetConsoleErrorMessage( "Invalid Submission. Please make the necessary corrections." );
        return false;
      }  
      else 
      {
        //Setup the input
        resource = ConstructResource(
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
          
        if ( standards.Count() > 0 )
        {
          resource.Standard = standards;
        }
      }
		return isValid;
    }

    /// <summary>
    /// Determine publishing Url
    /// - if a file, and not public, need to use the content item url
    /// </summary>
    /// <param name="isValid"></param>
    /// <returns></returns>
    public string GetPublishingURL( ref bool isValid )
    {
      if( mode == "upload" )
      {
        if ( fileFile.HasFile ) //Doing file upload & publish
        {
          //Do file upload, with needed checks
          bool isFileOkay = true;
          string status = "";
          var publishingURL = UtilityManager.GetAppKeyValue("siteRoot", "http://ioer.ilsharedlearning.org") 
                        + CreateDocumentItem( fileFile, (Patron) WebUser, ref isFileOkay, ref status );
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

    /// <summary>
    /// For file resources, create the document record, and parent content item
    /// </summary>
    /// <param name="uploader"></param>
    /// <param name="user"></param>
    /// <param name="isValid"></param>
    /// <param name="status"></param>
    /// <returns>Url to the document</returns>
    public string CreateDocumentItem( FileUpload uploader, Patron user, ref bool isValid, ref string status )
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
        item.StatusId = ContentItem.SUBMITTED_STATUS;//set to 3, just in case of a failure before publish,then set after actual publish

        //check if have context of a parent
        int parentId = 0;
        Int32.TryParse( parentNodeId, out parentId );
        ContentItem parentItem = new ContentItem();
        if ( parentId > 0 )
        {
            parentItem = new ContentServices().Get( parentId );
        }
        //how to get value from rights selector ==> will be done later
        item.ConditionsOfUseId = 1;
        int contentId = FileResourceController.CreateContentItemWithFileOnly( uploader, parentItem, item, ref status );
        if ( contentId > 0 )
        {
            //= ========= save content id in ordr to be able to get back to contentId in order to update it!
            createdContentItemId.Text = contentId.ToString();
            isValid = true;
            url = item.DocumentUrl;
        }
        else
            isValid = false;

      //Return the URL to the file
        return url;
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
      MemorizeSelectedItems( subject, gradeLevel, careerCluster );
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

    /// <summary>
    /// do form update
    /// NOTE: the resource parameter has been populated in the IsFormUpdate method
    /// </summary>
    /// <param name="user"></param>
    /// <param name="resource"></param>
    public void DoUpdate( Patron user, ref Resource resource )
    {
  
        if ( resource == null || resource.ResourceUrl == null || resource.ResourceUrl.Trim().Length < 5 )
        {
            SetConsoleErrorMessage( "Wierd error - resource appears empty???" );
            return;
        }

      int accessRightsId = Int32.Parse( ddlContentFilePrivacyLevel.SelectedValue );
      memory.privacyID = int.Parse( ddlContentFilePrivacyLevel.SelectedValue );

      //Pull Library/Collection data out of the form
      var selectedLibrary = 0;
      var selectedCollection = 0;
      GetSelectedLibraryAndCollection( ref selectedLibrary, ref selectedCollection );
      var publishForOrgId = GetSelectedOrg();

        //Do the publish
        bool successfulPublish = false;
        string publishStatus = "";
        int versionID = 0;
        int resourceIntID = 0;
        string sortTitle = "";
        bool skipLRPublish = isDebugMode;
        bool updatingElasticSearch = true;

        if ( doingLRPublish.Text == "no" )
            skipLRPublish = true;

        if ( accessRightsId != ContentItem.PUBLIC_PRIVILEGE )
        {
            //don't do LR if not public
            skipLRPublish = true;
            updatingElasticSearch = false;
        }
        
        //**** add cheat. If the CI status is not published, the page in the image will indicate a private page
        int contentId = this.GetRequestKeyValue( "contentId", 0 );
        if ( contentId > 0 )
        {
            string statusMsg = new ContentServices().UpdateAfterPublish( contentId );
        }

        //TBD - will populate once org list added
        resource.PublishedForOrgId= 0;

        PublishingServices.PublishToAll( resource
                    , ref successfulPublish
                    , ref publishStatus
                    , ref versionID
                    , ref resourceIntID
                    , ref sortTitle 
                    , updatingElasticSearch
                    , skipLRPublish
                    , user
                    , publishForOrgId );

        //Add to the Selected Library and Collection
        if ( successfulPublish )
        {

          int docContentId = 0;
            //TODO ==================
            //update the content item for resourceId and contents rights id/url
            if ( createdContentItemId.Text != null && IsInteger( createdContentItemId.Text ) )
            {
                //do again, just so not forgotten if cheat goes away
                docContentId = Int32.Parse( createdContentItemId.Text );
                if ( docContentId > 0 )
                {
                    int parentId = 0;
                    Int32.TryParse( parentNodeId, out parentId );

                    ContentItem item = new ContentServices().Get( docContentId );
                    //check type
                    if ( item != null && item.SeemsPopulated )
                    {
                        if ( item.TypeId == ContentItem.DOCUMENT_CONTENT_ID )
                        {
                            //if a document type
                            //item.ResourceVersionId = resource.Version.Id;
                            item.ResourceIntId = resource.Id;
                            // 14-11-03 mparsons - connector no longer used, set parentId
                            item.ParentId = parentId;

                            item.StatusId = ContentItem.PUBLISHED_STATUS;
                            item.UseRightsUrl = resource.Version.Rights;
                            //item.IsPublished = true;
                            item.PrivilegeTypeId = accessRightsId;
                            string statusMsg = new ContentServices().UpdateAfterQuickPub( item );
                            if ( item.DocumentRowId != null && item.IsValidRowId( item.DocumentRowId ) )
                            {
                                statusMsg = new ContentServices().DocumentVersion_SetToPublished( item.DocumentRowId.ToString() );
                            }
                        }

                        if ( parentId > 0 )
                        {
                            //create connector
                            //TODO - may want direct parent sometimes?
                            // 14-11-03 mparsons - connector no longer used
                            //new ContentServices().ContentConnectorAdd( parentId, item.Id, WebUser.Id );

                        }
                    }
                }
            }

            //var siteRoot = UtilityManager.GetAppKeyValue( "siteRoot", "http://ioer.ilsharedlearning.org" );
             //&& resource.ResourceUrl.ToLower().IndexOf("/content/") > -1
             // && resource.ResourceUrl.ToLower().IndexOf( siteRoot.ToLower() ) > -1 
            //check if a content item was published
            if ( mode == "tag" && contentId > 0 )
            {
                //prob should be calling handle approvals?
                bool hasApproval = false;
                string statusMessage2 = "";
				bool isValid = new ContentServices().HandleContentApproval(resource, contentId, user, ref hasApproval, ref statusMessage2);
                if ( hasApproval )
                {
                    SetConsoleSuccessMessage( "<p>NOTE: Approval is required, an email has been sent requesting a review of this resource.</p>" );
                }


                //ContentItem item = new ContentServices().Get( contentId );
                //item.ResourceVersionId = resource.Version.Id;
                //item.ResourceIntId = resource.Id;

                //item.StatusId = ContentItem.PUBLISHED_STATUS;
                //item.UseRightsUrl = resource.Version.Rights;
                //item.IsPublished = true;
                //string statusMsg = new ContentServices().UpdateAfterQuickPub( item );
                
            }

          //Add to library if available
          if ( selectedLibrary > 0 && selectedCollection > 0 )
          {
            string libStatus = "";
            //var test = libService.LibraryResourceCreate( selectedCollection, resourceIntID, user.Id, ref libStatus );
            var test = libService.LibraryResourceCreate( selectedLibrary, selectedCollection, resourceIntID, user, ref libStatus );
            if ( test == 0 )
            {
              SetConsoleErrorMessage( "Error adding the Resource to that Library/Collection: " + libStatus );
            }
            else
            {
              memory.libraryID = selectedLibrary;
              memory.collectionID = selectedCollection;
            }
          }

          //Publish content standards if necessary
			//whoa is this pushing res standards back to content?
          if ( isCurriculumNode )
          {
			//contentStandards.ForEach( m => m.ContentId = docContentId );
			  //new CurriculumServices().ContentStandard_Add( item.Id, user.Id, contentStandards );
          }

          //Link to detail page
          SetConsoleSuccessMessage( "View your new Resource <a target=\"_blank\" href=\"/Resource/" + resourceIntID + "/" + sortTitle + "\">here</a>. <b>Note:</b> Thumbnails take a few moments to generate--you may not immediately see the thumbnail for your new Resource." );

         postedResourceIntID = "var resourceIntID = " + resourceIntID + ";";
         currentParentNodeId = "var currentParentNodeId = " + parentNodeId + ";";

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
          rememberedItems = "var rememberedItems = " + new JavaScriptSerializer().Serialize( memory ) + ";";
        }
        else
        {
          SetConsoleErrorMessage( "Error publishing your Resource: " + publishStatus );
          if ( mode == "tag" )
          {
            txtURL.Value = resource.ResourceUrl; //Trying to force-restore this value
          }
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
        if ( !libService.LibrarySection_DoesUserHaveContributeAccess( testLibraryID, testCollectionID, WebUser.Id ) )
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

    public void MemorizeSelectedItems( List<ListItem> subject, List<ListItem> gradeLevel, List<ListItem> careerCluster )
    {
      /*
      var holder = new InfoMemoryJSON()
      {
        subjectIDs = MemorizeItems( subject ),
        gradeLevelIDs = MemorizeItems( gradeLevel ),
        careerClusterIDs = MemorizeItems( careerCluster )
      };

      rememberedItems = "var rememberedItems = " + new JavaScriptSerializer().Serialize( holder ) + ";";
      */
      memory.subjectIDs = MemorizeItems( subject );
      memory.gradeLevelIDs = MemorizeItems( gradeLevel );
      memory.careerClusterIDs = MemorizeItems( careerCluster );
    }
    public List<int> MemorizeItems( List<ListItem> items )
    {
      var output = new List<int>();
      foreach ( ListItem item in items )
      {
        if ( item.Selected )
        {
          output.Add( int.Parse( item.Value ) );
        }
      }

      return output;
    }

    //Subclass used to hold onto selected info
    public class InfoMemoryJSON
    {
      public InfoMemoryJSON()
      {
        subjectIDs = new List<int>();
        gradeLevelIDs = new List<int>();
        careerClusterIDs = new List<int>();
        keywords = new List<string>();
        use = true;
      }
      public bool use { get; set; }
      public List<int> subjectIDs { get; set; }
      public List<int> gradeLevelIDs { get; set; }
      public List<int> careerClusterIDs { get; set; }
      public List<string> keywords { get; set; }
      public int libraryID { get; set; }
      public int collectionID { get; set; }
      public int usageRightsIndex { get; set; }
      public string usageRightsURL { get; set; }
      public int privacyID { get; set; }
    }
  }
}