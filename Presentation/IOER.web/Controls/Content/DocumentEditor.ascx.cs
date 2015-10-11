using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using AjaxControlToolkit;
using Obout.Ajax.UI.TreeView;
using GDAL = Isle.BizServices;
using ILPathways.Business;
using ILPathways.Common;
using IOER.Controllers;

using IOER.Library;
using ILPathways.Utilities;
using Isle.BizServices;
using LRWarehouse.DAL;
using LRWarehouse.Business;
using LDAL = LRWarehouse.DAL;
using BDM = LRWarehouse.DAL.BaseDataManager;
using AppUser = LRWarehouse.Business.Patron; 

namespace IOER.Controls.Content
{
    /// <summary>
    /// handle editing of existing documents
    /// </summary>
    public partial class DocumentEditor : BaseUserControl
    {
        const string thisClassName = "DocumentEditor";
        public string userProxyId { get; set; }
        public string previewID;
        public string cleanTitle;

        ContentServices myManager = new ContentServices();
		ContentSearchServices mySearchManager = new ContentSearchServices();

        #region Properties
        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string FormSecurityName
        {
            get { return this.txtFormSecurityName.Text; }
            set { this.txtFormSecurityName.Text = value; }
        }
        /// <summary>
        /// Get/Set current Content item Id
        /// </summary>
        protected int CurrentRecordId
        {
            get 
            { 
                int id = 0;
                Int32.TryParse( txtId.Text, out id);
                return id;
            }
            set { txtId.Text = value.ToString(); }
        }
        #endregion

        protected void Page_Load( object sender, EventArgs e )
        {
            if ( IsUserAuthenticated() == false )
            {
                Stage1Items.Visible = false;
                loginMessage.Visible = true;

                SetConsoleErrorMessage( "Error: you are not signed in (your session may have timed out). Please sign in and try again." );
                Response.Redirect( Request.RawUrl.ToString(), true );
                return;
            }

            CurrentUser = GetAppUser();
            userProxyId = "var userProxyId = \"" + CurrentUser.ProxyId.ToString() + "\";";

            if ( !IsPostBack )
            {
                InitializeForm();

            }


            //regardless:
            //populateConditionsOfUseData();
            //conditionsSelector.PopulateItems();
            //previewID = previewLink.Text;
            if ( string.IsNullOrEmpty( txtTitle.Text ) )
                cleanTitle = "";
            else
                cleanTitle = ResourceBizService.FormatFriendlyTitle( this.txtTitle.Text );
  

        }//
        protected void InitializeForm()
        {

            //check if authorized, if not found, will check by orgId
            this.FormPrivileges = GDAL.SecurityManager.GetGroupObjectPrivileges( CurrentUser, FormSecurityName );

            //}
            if ( this.FormPrivileges.CanCreate() )
            {
                Stage1Items.Visible = true;
                loginMessage.Visible = false;
            }
            else
            {
                //display message, and lock down
                Stage1Items.Visible = false;
                loginMessage.Visible = true;
            }

            //= Add attribute to btnDelete to allow client side confirm
            btnDelete.Attributes.Add( "onClick", "return confirmDelete(this);" );
            btnUnPublish.Attributes.Add( "onClick", "return confirmUnPublish(this);" );
            this.btnSetInactive.Attributes.Add( "onClick", "return confirmSetInactive(this);" );
            this.btnPublish.Attributes.Add( "onClick", "return confirmPublish(this);" );

            PopulateControls();
            CheckRecordRequest();

        }
        #region retrieval
        /// <summary>
        /// Check the request type for the form
        /// </summary>
        private void CheckRecordRequest()
        {

            // Check if a request was made for a specific record
            // generally should use rowId for better privacy
            int id = this.GetRequestKeyValue( "cidx", 0 );
            //if PK is a Guid, use:
            string rid = this.GetRequestKeyValue( "rid", "" );
            if ( rid.Trim().Length == 36 )
            {
                this.Get( rid );
            }
            else if ( id > 0 )
            {
                this.Get( id );
            }
            else
            {
                //

            }


        }	// End 

        /// <summary>
        /// Get - retrieve form data
        /// </summary>
        /// <param name="recId"></param>
        private void Get( int recId )
        {
            try
            {
                //get record
                ContentItem entity = myManager.Get( recId );

                if ( entity == null || entity.IsValid == false )
                {
                    this.SetConsoleErrorMessage( "Sorry the requested record does not exist" );
                    return;

                }
                else
                {
                    PopulateForm( entity );
                }

            }
            catch ( System.Exception ex )
            {
                //Action??		- display message and close form??	
                LoggingHelper.LogError( ex, thisClassName + ".Get() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );

            }

        }	// End method

        private void Get( string recId )
        {
            try
            {
                //get record
                ContentItem entity = myManager.GetByRowId( recId );

                if ( entity == null || entity.IsValid == false )
                {
                    this.SetConsoleErrorMessage( "Sorry the requested record does not exist" );
                    return;

                }
                else
                {
                    PopulateForm( entity );
                }

            }
            catch ( System.Exception ex )
            {
                //Action??		- display message and close form??	
                LoggingHelper.LogError( ex, thisClassName + ".Get() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );

            }

        }	// End method


        /// <summary>
        /// Reset form values
        /// </summary>
        private void ResetForm()
        {
            //reset form by populating with an empty business object
            ContentItem entity = new ContentItem();
 
            //do any defaults. 
            entity.IsActive = true;
            entity.Status = "Draft";
            entity.HasChanged = false;
            // templatesPanel.Visible = true;
            ddlIsOrganizationContent.Enabled = true;
            btnPublish.Visible = false;

            PopulateForm( entity );

            //reset any controls or buttons not handled in the populate method

            //unselect forms list

        }//

        /// <summary>
        /// Populate the form 
        ///</summary>
        private void PopulateForm( ContentItem entity )
        {
      //      CurrentRecord = entity;
            CurrentRecordId = entity.Id;
            LoggingHelper.DoTrace( 6, thisClassName + ".PopulateForm(): " + entity.Id );
            //TODO - determine if user has access to this record
            //
            if ( entity.Id > 0 )
            {
                if ( FormPrivileges.WritePrivilege > ( int ) ILPathways.Business.EPrivilegeDepth.Region )
                {
                    //carte blanche
                }
                else if ( entity.CreatedById != WebUser.Id )
                {
                    //not author, set readonly, or reject?
                    containerPanel.Visible = false;
                    SetConsoleErrorMessage( "Only the original author may make updates to this content!" );
                }

            }
            this.txtId.Text = entity.Id.ToString();
            this.txtTitle.Text = entity.Title;
            this.txtDescription.Text = entity.Summary;

            //TODO - show publish button if status = 2
            //     - may need ability to take out of production
            //       - what happens on a change to LR version??
            lblStatus.Text = entity.Status;
            if ( entity.StatusId > 0 )
            {
                string style = entity.Status.Replace( " ", "" ).ToLower();
                lblStatus.CssClass = "status " + style + "Status";
            }
            this.SetListSelection( this.ddlPrivacyLevel, entity.PrivilegeTypeId.ToString() );
            //this.SetListSelection( this.ddlConditionsOfUse, entity.ConditionsOfUseId.ToString() );
            conditionsSelector.selectedValue = entity.ConditionsOfUseId.ToString();
            this.SetListSelection( this.ddlContentType, entity.TypeId.ToString() );

            //txtConditionsOfUse.Text = entity.UseRightsUrl;
            conditionsSelector.conditionsURL = entity.UseRightsUrl;

            //this.rblIsActive.SelectedValue = this.ConvertBoolToYesNo( entity.IsActive );

            if ( entity.Id > 0 )
            {
                //not sure if will allow template selection for existing record
                ddlContentType.Visible = false;
                //Page Title
                Page.Title = entity.Title + " | ISLE OER";
                lblContentType.Text = entity.ContentType;
                lblContentType.Visible = true;


                //if orgId > 0, may want to have an indication that org is the owner??
                //for now always disable. changing could be complicated
                ddlIsOrganizationContent.Enabled = false;
                if ( entity.IsOrgContentOwner )
                {
                    ddlIsOrganizationContent.SelectedIndex = 1;
                    ddlIsOrganizationContent.Enabled = false;
                    if ( entity.HasResourceId() == false )
                    {
                        //never published
                        btnPublish.Visible = true;
                        btnPublish.CommandName = "PublishNew";
                    }
                    else if ( entity.HasResourceId() == true
                        && entity.StatusId == ContentItem.REVISIONS_REQUIRED_STATUS )
                    {
                        btnPublish.Visible = true;
                        btnPublish.CommandName = "PublishSubmit";
                    }
                }
                else
                {
                    ddlIsOrganizationContent.SelectedIndex = 0;
                    if ( entity.StatusId < ContentItem.PUBLISHED_STATUS )
                    {
                        btnPublish.Visible = true;
                        if ( entity.HasResourceId() == true )
                        {
                            btnPublish.CommandName = "PublishUpdate";
                            this.btnPublish.Attributes.Remove( "onClick" );
                            this.btnPublish.Attributes.Add( "onClick", "return confirmRePublish(this);" );
                        }
                        else
                        {
                            btnPublish.CommandName = "PublishNew";
                        }
                    }
                }


                if ( FormPrivileges.CanDelete() )
                {
                    btnDelete.Visible = true;
                }
                //set after content item exists
                //actaully have existing at this point, so use different approach
                //FileResourceController.PathParts parts = FileResourceController.DetermineDocumentPathUsingParentItem( entity );
                //string documentFolder = parts.filePath;
                //string relativePath = parts.url;

                //string documentFolder = FileResourceController.DetermineDocumentPathUsingParentItem( entity );
                //string relativePath = FileResourceController.GetRelativeUrlPath( entity );
                //string baseUrl = UtilityManager.GetAppKeyValue( "path.MapContentPath", "/ContentDocs/" );

                //string upload = baseUrl + relativePath + "/";

                
                //?? ==> n
                if ( entity.StatusId == ContentItem.PUBLISHED_STATUS )
                {
                    this.btnUnPublish.Visible = true;
                    this.btnSetInactive.Visible = true;
                }
                else if ( entity.StatusId == ContentItem.INPROGRESS_STATUS
                       || entity.StatusId == ContentItem.REVISIONS_REQUIRED_STATUS )
                {
                    this.btnSetInactive.Visible = true;
                }

                historyPanel.Visible = true;
                this.lblHistory.Text = entity.HistoryTitle();
                

                //============== handle different types
                if ( entity.TypeId == ContentItem.DOCUMENT_CONTENT_ID )
                {
                    PopulateDocumentContentType( entity );
                }
          
                else
                {
                    SetConsoleErrorMessage( "Unexpected content type encountered - cannot display" );
                }

                //set preview
                ShowPreview( entity );

            }
            else
            {
                //reset controls or state for empty object
                //ex: reset a dropdownlist not handled by 
                historyPanel.Visible = false;
                this.lblHistory.Text = "";
                lblContentType.Visible = false;
                ddlContentType.Visible = true;

                btnDelete.Visible = false;
                this.btnUnPublish.Visible = false;
                this.btnSetInactive.Visible = false;

                ddlContentType.Enabled = true;

            //    FileContentItem.Visible = false;
            }

        }//
        void PopulateDocumentContentType( ContentItem entity )
        {
            LoggingHelper.DoTrace( 6, thisClassName + ".PopulateDocumentContentType(): " + entity.Id );
            btnSave.Text = "Save";
            //note only handling basic file update, not republishing!

            if ( entity.RelatedDocument != null && entity.RelatedDocument.HasValidRowId() )
            {
                txtDocumentRowId2.Text = entity.DocumentRowId.ToString();
                fileContentUrl.Text = entity.DocumentUrl;

                //check if file exists on server, if not it will be downloaded
                string fileUrl = ValidateDocumentOnServer( entity, entity.RelatedDocument );
                if ( fileUrl.Length > 10 )
                {

                    contentFileName.Text = entity.RelatedDocument.FileName;
                    contentFileDescription.Text = entity.Summary;
                    linkContentFile.NavigateUrl = fileUrl;

                }
                else
                    contentFileName.Text = "Sorry issue encountered locating the related document.";
            }

        }//

        protected void btnContentFileUpload_Click( object sender, EventArgs e )
        {
            //validate
            if ( contentFileUpload.HasFile == false || contentFileUpload.FileName == "" )
            {
                SetConsoleErrorMessage( "Please select a file before clicking upload" );

                return;
            }
            HandleUpload();
        }//

        protected void HandleUpload()
        {
            
            string statusMessage = "";

            ContentItem entity = myManager.Get( CurrentRecordId );

            DocumentVersion docVersion = new DocumentVersion();
            string documentRowId = this.txtDocumentRowId2.Text;
            if ( documentRowId.IndexOf( "00000000-" ) > -1
                || documentRowId.Length != 36
                || docVersion.IsValidRowId( new Guid( documentRowId ) ) == false )
            {
                //not normal but may be necessary for workaround
                docVersion.CreatedById = WebUser.Id;
                docVersion.Created = System.DateTime.Now;
                //should be using org rowid!
                FileResourceController.CreateDocument( contentFileUpload, docVersion, entity.OrgId, ref statusMessage );

                int currentStatusId = entity.StatusId;
                //get latest and then update
               // CurrentRecord = myManager.Get( CurrentRecord.Id );

                entity.DocumentUrl = docVersion.URL;
                entity.DocumentRowId = docVersion.RowId;
                entity.LastUpdated = System.DateTime.Now;
                entity.LastUpdatedById = WebUser.Id;		//include for future use
                entity.LastUpdatedBy = WebUser.FullName();
                //change status???

                entity.StatusId = DetermineStatus( entity );
                statusMessage = myManager.Update( entity );
                if ( statusMessage.Equals( "successful" ) )
                {
                    this.SetConsoleSuccessMessage( "Successfully added file!" );
                    contentFileName.Text = entity.RelatedDocument.FileName;
                    contentFileDescription.Text = entity.Summary;
                    linkContentFile.NavigateUrl = entity.DocumentUrl;
                }
                else
                {
                    this.SetConsoleErrorMessage( "Error encountered attempting to update this item: " + statusMessage );
                }

                return;
                //=============================
            }
            else
            {

                docVersion = myManager.DocumentVersionGet( documentRowId );
            }
            docVersion.LastUpdatedById = CurrentUser.Id;

            try
            {
                int maxFileSize = UtilityManager.GetAppKeyValue( "maxDocumentSize", 4000000 );
                //TODO - if an image, do a resize
                if ( FileResourceController.IsFileSizeValid( contentFileUpload, maxFileSize ) == false )
                {
                    SetConsoleErrorMessage( string.Format( "Error the selected file exceeds the size limits ({0} bytes).", maxFileSize ) );
                    return;
                }

                //NOTE: now calling ReplaceDocument so file path and url don't change
                new FileResourceController().ReplaceDocument( contentFileUpload, entity, docVersion, CurrentUser.Id, ref statusMessage );

                //docVersion.MimeType = contentFileUpload.PostedFile.ContentType;
                //keeping the same file name
                //docVersion.FileName = contentFileUpload.FileName;
                //docVersion.FileDate = System.DateTime.Now;

                //string sFileType = System.IO.Path.GetExtension( docVersion.FileName );
                //sFileType = sFileType.ToLower();

                //docVersion.FileName = System.IO.Path.ChangeExtension( docVersion.FileName, sFileType );

                //probably want to fix filename to standardize
                //should already be clean, so skip to avoid unexpected name changes
                //docVersion.CleanFileName();

                //LoggingHelper.DoTrace( 5, thisClassName + " HandleDocument(). calling DetermineDocumentPath" );

                


                //string documentFolder = FileResourceController.DetermineDocumentPath( entity );
                ////
                //// LoggingHelper.DoTrace( 5, thisClassName + " - uploading to " + documentFolder );
                //docVersion.URL = FileResourceController.DetermineDocumentUrl( entity, docVersion.FileName );

                ////try separating following to insulate from file privileges issues
                //UploadFile( contentFileUpload, documentFolder, docVersion.FileName );
                ////rewind for db save
                //contentFileUpload.PostedFile.InputStream.Position = 0;
                //Stream fs = contentFileUpload.PostedFile.InputStream;

                //docVersion.ResourceBytes = fs.Length;
                //byte[] data = new byte[ fs.Length ];
                //fs.Read( data, 0, data.Length );
                //fs.Close();
                //fs.Dispose();
                //docVersion.SetResourceData( docVersion.ResourceBytes, data );

                //statusMessage = myManager.DocumentVersionUpdate( docVersion );
                if ( statusMessage.Equals( "successful" ) )
                {

                    linkContentFile.Visible = true;

                    //update CI
                    //must not change!
                    //entity.DocumentUrl = docVersion.URL;
                    entity.LastUpdated = System.DateTime.Now;
                    entity.LastUpdatedById = WebUser.Id;		//include for future use
                    entity.LastUpdatedBy = WebUser.FullName();
                    //change status???
                    int currentStatusId = entity.StatusId;
                    entity.StatusId = DetermineStatus( entity );
                    statusMessage = myManager.Update( entity );
                    if ( statusMessage.Equals( "successful" ) )
                    {
                        this.SetConsoleSuccessMessage( "Successfully updated file!" );

                        contentFileName.Text = entity.RelatedDocument.FileName;
                        contentFileDescription.Text = entity.Summary;
                        linkContentFile.NavigateUrl = entity.DocumentUrl;

                    }
                    else
                    {
                        this.SetConsoleErrorMessage( "Error encountered attempting to update this item: " + statusMessage );
                    }

                }
                else
                {
                    linkContentFile.Visible = false;
                }
            }
            catch ( Exception ex )
            {
                SetConsoleErrorMessage( "Unexpected error occurred while attempting to upload your file.<br/>" + ex.Message );
                LoggingHelper.LogError( ex, thisClassName + "btnContentFileUpload_Click()" );
                return;
            }
        }

        private void UploadFile( FileUpload uploadCtrl, string documentFolder, string filename )
        {
            try
            {
                FileSystemHelper.CreateDirectory( documentFolder );

                string diskFile = documentFolder + "\\" + filename;
                //string diskFile = MapPath( documentFolder ) + "\\" + entity.FileName;

                LoggingHelper.DoTrace( 5, thisClassName + " UploadFile(). doing SaveAs" );
                uploadCtrl.SaveAs( diskFile );

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format( "UploadFile(documentFolder: {0}, filename: {1})", documentFolder, filename ) );
            }
        }//

        private string ValidateDocumentOnServer( ContentItem parentEntity, DocumentVersion doc )
        {
            string fileUrl = "";
            string documentFolder = "";
            try
            {
                //should use filePath and fileName from doc
                if ( doc.FileLocation().Length > 0 )
                {
                    documentFolder = doc.FilePath;
                }
                else
                {

                    //string documentFolder = FileResourceController.DetermineDocumentPath( parentEntity );
                    FileResourceController.PathParts parts = FileResourceController.DetermineDocumentPathUsingParentItem( parentEntity );
                    documentFolder = parts.filePath;
                }
                string message = FileSystemHelper.HandleDocumentCaching( documentFolder, doc );
                if ( message == "" )
                {
                    //blank returned message means ok
                    fileUrl = FileResourceController.DetermineDocumentUrl( parentEntity, doc.FileName );
                }
                else
                {
                    //error, should return a message
                    this.SetConsoleErrorMessage( message );
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ValidateDocumentOnServer() - Unexpected error encountered while retrieving document" );

                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }
            return fileUrl;
        }//

        void HandleOtherContentType( ContentItem entity )
        {


        }//

        void ShowPreview( ContentItem entity )
        {

            //previewLink.Text = entity.RowId.ToString(); //able to retain value through postbacks
            previewLink.Text = entity.Id.ToString(); //able to retain value through postbacks
            previewID = previewLink.Text; //able to be inserted into html properties
            if ( string.IsNullOrEmpty( txtTitle.Text ) )
                cleanTitle = "";
            else
                cleanTitle = ResourceBizService.FormatFriendlyTitle( this.txtTitle.Text );
        }

        #endregion

        #region Form actions
  
        protected void btnSave_Click( object sender, EventArgs e )
        {
            LoggingHelper.DoTrace( 6, thisClassName + "btnSaveWeb_Click" );
            //validate
            if ( ValidateSummary() & this.IsFormValid() )
            {
                //save
                this.UpdateForm();
                //CreateNewAuthoredResourceStarterRecord();

                //hide cancel




                //show the finish button
                //btnFinish.Visible = true;
                //btnFinish2.Visible = true;
            }

            //hide cancel
        }

        protected void btnPublish_Click( Object sender, CommandEventArgs ev )
        {
            if ( ValidateSummary() & this.IsFormValid() )
            {
                switch ( ev.CommandName )
                {
                    case "PublishNew":
                        this.PublishNewResource();
                        break;
                    case "PublishSubmit":
                        SubmitPublishApproveRequest();
                        break;
                    case "PublishUpdate":
                        //only showing if previously published==> maybe
                        this.UpdateForm( ContentItem.PUBLISHED_STATUS );
                        btnPublish.Visible = false;
                        break;
                }
            }

        } // end 

        #region validation

        private bool IsFormValid()
        {
            bool isValid = true;

            try
            {

                Page.Validate();


                //TBD

                //recId = int.Parse( this.Id.Text );

                // check additional required fields
                // isValid = HasRequiredFields();

            }
            catch ( System.Exception ex )
            {
                //SetFormMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.ToString() );
                return false;
            }

            return isValid;
        } //

        protected bool ValidateSummary()
        {
            bool isValid = true;

            //Title
            txtTitle.Text = txtTitle.Text.Trim().Replace( "<", "&lt;" ).Replace( ">", "&gt;" );
            if ( txtTitle.Text.Length < int.Parse( ltlMinTxtTitleLength.Text ) )
            {
                SetConsoleErrorMessage( "You must enter a Title of meaningful length." );
                isValid = false;
            }

            //Description
            txtDescription.Text = txtDescription.Text.Replace( "<", "&lt;" ).Replace( ">", "&gt;" );
            if ( txtDescription.Text.Length < int.Parse( ltlMinTxtDescriptionLength.Text ) )
            {
                SetConsoleErrorMessage( "You must enter a Description of meaningful length." );
                isValid = false;
            }

            //Usage Rights
            //if ( txtConditionsOfUse.Text.Length < int.Parse( ltlMinUsageRightsURLLength.Text ) )
            if ( conditionsSelector.conditionsURL.Length < int.Parse( ltlMinUsageRightsURLLength.Text ) )
            {
                SetConsoleErrorMessage( "You must select or enter a URL in the Usage Rights section." );
                isValid = false;
            }
            if ( ddlPrivacyLevel.SelectedIndex < 1 )
            {
                SetConsoleErrorMessage( "You must select a privacy setting (Who can access this resource?)." );
                isValid = false;
            }

            return isValid;
        }

        #endregion
        private void PublishNewResource()
        {
            //TODO - do we need a mechanism to publish as https??
            ContentItem entity = myManager.Get( CurrentRecordId );

            string port = "";
            if ( Request.Url.Port > 80 )
                port = ":" + Request.Url.Port.ToString();

            Session.Add( "authoredResourceID", CurrentRecordId.ToString() );
            if ( litPreviewUrlTemplate.Text.ToLower().IndexOf( "resourcepage.aspx" ) > -1 )
                Session.Add( "authoredResourceURL", Request.Url.Scheme + "://" + Request.Url.Host + port
                                                + string.Format( litPreviewUrlTemplate.Text, entity.RowId.ToString() ) );
            else
                Session.Add( "authoredResourceURL", UtilityManager.FormatAbsoluteUrl( string.Format( litPreviewUrlTemplate.Text, entity.Id, ResourceBizService.FormatFriendlyTitle( entity.Title ) ), false ) );

            Response.Redirect( "/Publish.aspx?rid=" + entity.RowId.ToString(), true );
            //redirectTarget = Request.Url.Scheme + "://" + Request.Url.Host + ":" + Request.Url.Port + "/Publish.aspx";

        } // end 

        private void SubmitPublishApproveRequest()
        {
            //update just in case
            this.UpdateForm();

            string statusMessage = string.Empty;
            AppUser user = GetAppUser();
			if (new ContentServices().RequestApproval(CurrentRecordId, user, ref statusMessage) == true)
            {
                SetConsoleSuccessMessage( "An email was sent requesting a review of your resource." );
                //refresh
                Get( CurrentRecordId );
                btnPublish.Visible = false;
            }
            else
                SetConsoleErrorMessage( "An error occurred attempting to initiate a review of your resource.<br/>System administration has been notified.<br/>" + statusMessage );

        } // end 

        private void UpdateForm()
        {
            UpdateForm( 0 );
        }

        private void UpdateForm( int newStatusId )
        {

            LoggingHelper.DoTrace( 6, thisClassName + "UpdateForm" );
            int id = 0;
            string statusMessage = "";
            string action = "";
            bool isApprovalRequired = false;
            Patron user = ( Patron ) WebUser;
            ContentItem entity = new ContentItem();

            try
            {
                Int32.TryParse( this.txtId.Text, out id );

                if ( id == 0 )
                {
                    entity.Id = 0;
                    entity.CreatedById = WebUser.Id;		//include for future use
                    entity.CreatedBy = WebUser.FullName();
                    entity.Created = System.DateTime.Now;
                    action = "Create";
                    entity.IsActive = true;
                    // requires approval. Definitely on create, not sure about updates
                    if ( ddlIsOrganizationContent.SelectedIndex == 0 )
                        isApprovalRequired = false;
                    else
                        isApprovalRequired = true;

     

                    if ( this.ddlContentType.SelectedIndex > 0 )
                    {
                        int cid = Int32.Parse( ddlContentType.SelectedValue );
                        entity.TypeId = cid;
                    }
                    else
                    {
                        entity.TypeId = ContentItem.GENERAL_CONTENT_ID;
                    }
                }
                else
                {
                    LoggingHelper.DoTrace( 6, thisClassName + "UpdateForm - get: id=" + id.ToString() );
                    // get current record 
                    entity = myManager.Get( id );
                    action = "Update";
                }
                //TODO - may want means to set entity false - maybe by approver?

                /* assign form fields 			 */
                entity.Title = this.txtTitle.Text;
                entity.Summary = this.txtDescription.Text;

                //if ( ddlConditionsOfUse.SelectedIndex > -1)
                if ( conditionsSelector.selectedIndex > -1 )
                {
                    //should be requried but no value
                    //entity.ConditionsOfUseId = int.Parse( this.ddlConditionsOfUse.SelectedValue.ToString() );
                    entity.ConditionsOfUseId = int.Parse( conditionsSelector.GetSelectedValue() );
                    //entity.ConditionsOfUseId = int.Parse( Request.Form[ ddlConditionsOfUse.UniqueID ] ); //finally figured this one out. but other issues prevent it from being useful
                    //currentConditionOfUse = entity.ConditionsOfUseId.ToString();

                    //entity.UseRightsUrl = txtConditionsOfUse.Text;
                    entity.UseRightsUrl = conditionsSelector.conditionsURL;
                }

                entity.PrivilegeTypeId = int.Parse( this.ddlPrivacyLevel.SelectedValue.ToString() );
                entity.OrgId = user.OrgId;

                if ( ddlIsOrganizationContent.SelectedIndex == 0 )
                    entity.IsOrgContentOwner = false;
                else
                    entity.IsOrgContentOwner = true;

                //status
                int currentStatusId = entity.StatusId;
                if ( newStatusId > 0 )
                {
                    entity.StatusId = newStatusId;
                }
                else
                {
                    entity.StatusId = DetermineStatus( entity );
                }

                if ( newStatusId == ContentItem.INACTIVE_STATUS )
                {
                    //reset rvid
                    entity.ResourceIntId = 0;
                }

                entity.LastUpdated = System.DateTime.Now;
                entity.LastUpdatedById = WebUser.Id;		//include for future use
                entity.LastUpdatedBy = WebUser.FullName();

                //call insert/update
                int entityId = 0;
                if ( entity.Id == 0 )
                {
                    entityId = myManager.Create( entity, ref statusMessage );
                    if ( entityId > 0 )
                    {
                        this.txtId.Text = entityId.ToString();
                        //reget the whole record, for rowId
                        entity = myManager.Get( entityId );
                        ShowPreview( entity );
                    }
                }
                else
                {
                    LoggingHelper.DoTrace( 6, thisClassName + "UpdateForm - before update" );
                    statusMessage = myManager.Update( entity );
                    
                }


                if ( statusMessage.Equals( "successful" ) )
                {
                    if ( entity.StatusId == ContentItem.PUBLISHED_STATUS )
                        lblStatus.Text = "Published";


                    //probably should do a document check just in case, didn't read instructions
                    if ( contentFileUpload.HasFile == true )
                    {

                    }
                    //CurrentRecord = entity;
                    //maybe, or ?????

                    if ( currentStatusId != entity.StatusId || newStatusId > 0 )
                    {
                        LoggingHelper.DoTrace( 6, thisClassName + "UpdateForm - skipping get" );
                        //Get( entity.Id );
                    }
                    if ( newStatusId != ContentItem.INACTIVE_STATUS )
                        this.SetConsoleSuccessMessage( action + " was successful for: " + entity.Title );
                }
                else
                {
                    this.SetConsoleErrorMessage( statusMessage );
                }

            }
            catch ( System.Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".UpdateForm() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }
        }
        /// <summary>
        /// Set the status of the current item
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected int DetermineStatus( ContentItem entity )
        {
            /*initial status = 2
             * case current
             * 2 to 2
             * 3 (submitted) -> leave?, warn?
             * 4 (declined) -> could be 2, need to remember context
             */
            int statusId = ContentItem.INPROGRESS_STATUS;

            //if not published, leave a status as is
            if ( entity.StatusId == ContentItem.DRAFT_STATUS )
                statusId = ContentItem.INPROGRESS_STATUS;
            else if ( entity.StatusId < ContentItem.PUBLISHED_STATUS )
                statusId = entity.StatusId;
            else if ( entity.StatusId == ContentItem.INACTIVE_STATUS )
                statusId = ContentItem.INPROGRESS_STATUS;       //TODO - do we want to arbitrarily set inactive to active, or require overt action by user?
            else
            {
                //is published. If approval is required, set to in progress, and what else
                //may need something to allow auto approve after initial approve, or at some point in time

                if ( entity.TypeId == ContentItem.DOCUMENT_CONTENT_ID || entity.IsHierarchyType )
                {
                    //leave as is
                }
                else
                {
                    if ( entity.IsOrgContent() )
                    {
                        statusId = ContentItem.INPROGRESS_STATUS;
                        //popup message
                    }
                    else
                    {
                        //no action. Probably don't want to arbitrarily change status. Provide an overt action
                    }
                }
            }

            return statusId;
        } //

        protected void btnDelete_Click( object sender, EventArgs e )
        {
            //delete
            //reset
            //disable?
            //may want to force click of new, and terms??
            int id = 0;
            try
            {
                string statusMessage = "";
                string extraMessage = "";
                id = int.Parse( this.txtId.Text );
                ContentItem ci = myManager.Get( id );

                if ( myManager.Delete( id, ref statusMessage ) )
                {
                    //TODO - need to delete LR related stuff!
                    if ( ci.HasResourceId() )
                    {
                        ResourceVersion entity = ResourceBizService.ResourceVersion_GetByResourceId( ci.ResourceIntId );
                        if ( entity != null && entity.Id > 0 )
                        {
                            try
                            {
                                new ResourceBizService().Resource_SetInactive( entity.ResourceIntId, ref statusMessage );
                                extraMessage = "Resource Deactivated";
                            }
                            catch ( Exception ex )
                            {
                                extraMessage = "There was a problem deactivating the Resource: " + ex.ToString();
                            }

                            //note can have a RV id not not be published to LR. Need to check for a resource docid
                            if ( entity.LRDocId != null && entity.LRDocId.Length > 10 )
                            {
                                //post request to delete ==> this process would take care of actual delete of the Resource hierarchy
                                if ( IsTestEnv() )
                                    extraMessage = "<br/>( WELL ALMOST - NEED TO ADD CODE TO REMOVE FROM THE LR, AND DO PHYSICAL DELETE OF THE RESOURCE HIERARCHY - JEROME)";
                            }

                        }
                    }

                    this.SetConsoleSuccessMessage( "Delete of record was successful " + extraMessage );

                    Response.Redirect( "/My/Authored", true );
                    //this.ResetForm();

                }
                else
                {
                    this.SetConsoleErrorMessage( statusMessage );
                }

            }
            catch ( System.Threading.ThreadAbortException tex )
            {
                //ignore this on redirect
            }
            catch ( System.Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".Delete() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }

        }
        protected void btnUnPublish_Click( object sender, EventArgs e )
        {
            //just set status back to inprogress
            //validate just in case
            if ( ValidateSummary() & this.IsFormValid() )
            {
                //save
                this.UpdateForm( ContentItem.INPROGRESS_STATUS );
            }
        }
        protected void btnSetInactive_Click( object sender, EventArgs e )
        {
            int id = 0;
            Int32.TryParse( this.txtId.Text, out id );

            if ( id > 0 )
            {
                //validate just in case
                if ( ValidateSummary() & this.IsFormValid() )
                {
                    ContentItem ci = myManager.Get( id );

                    this.UpdateForm( ContentItem.INACTIVE_STATUS );
                    //probably need a check that update was successful

                    //TODO - should a history record be created?

                    string extraMessage = "";
                    if ( ci.HasResourceId() )
                    {
                        ResourceVersion entity = ResourceBizService.ResourceVersion_GetByResourceId( ci.ResourceIntId );
                        if ( entity != null && entity.Id > 0 )
                        {
                            new ResourceBizService().Resource_SetInactive( entity.ResourceIntId, ref extraMessage );
                            //note can have a RV id not not be published to LR. Need to check for a resource docid
                            if ( entity.LRDocId != null && entity.LRDocId.Length > 10 )
                            {
                                //need to call methods to remove from LR
                                //==> this process would take care of actual delete of the Resource hierarchy
                                if ( IsTestEnv() )
                                extraMessage = "<br/>( WELL ALMOST - NEED TO ADD CODE TO REMOVE FROM THE LR, AND DO PHYSICAL DELETE OF THE RESOURCE HIERARCHY - JEROME)";
                            }

                        }
                    }
                    this.SetConsoleSuccessMessage( "Record was set to inactive. " + extraMessage );
                }
            }
        }//

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            //relevent?
            //only for new
        }

        #endregion

        #region Housekeeping
        /// <summary>
        /// Populate form controls
        /// </summary>
        private void PopulateControls()
        {
            bool hasHomeContent = false;
            string booleanOperator = "AND";
            //check if already has a home content
            string filter = "";
            string where = string.Format( "([TypeId] = 20 AND base.CreatedById = {0}) ", WebUser.Id );
            filter = BDM.FormatSearchItem( filter, where, booleanOperator );
            int pTotalRows = 0;
			DataSet ds = mySearchManager.Search( filter, "", 1, 25, ref pTotalRows );
            if ( DoesDataSetHaveRows( ds ) )
                hasHomeContent = true;
            SetContentType2( hasHomeContent );
            //SetContentType();

            //check if org admin, and whether already has an org home page

            SetPrivilegeLists();
            //SetConditionsOfUse();
          
        } //

        private void SetContentType()
        {
            //may only allow on first create?
            List<CodeItem> list = myManager.ContentType_ActiveList();
            
        }

        private void SetContentType2( bool hasHomeContent )
        {

            List<CodeItem> list2 = myManager.ContentType_TopLevelActiveList();
            BDM.PopulateList( this.ddlContentType, list2, "Id", "Title", "Select type" );

        }

        private void SetPrivilegeLists()
        {
            DataSet ds = myManager.ContentPrivilegeCodes_Select();
            DataSet ds2 = ds.Copy();

            BDM.PopulateList( this.ddlPrivacyLevel, ds, "Id", "Title", "Select access level" );

        }

   
        #endregion

        protected void btnTestPostback_Command( object sender, CommandEventArgs e )
        {

        }
    }
}