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

using GDAL = Isle.BizServices;
using ILPathways.Business;
using ILPathways.Controllers;

using ILPathways.Library;
using ILPathways.Utilities;
using Isle.BizServices;
using LRWarehouse.DAL;
using LRWarehouse.Business;
using LDAL = LRWarehouse.DAL;
using BDM = LRWarehouse.DAL.BaseDataManager; 
using AppUser = LRWarehouse.Business.Patron; //ILPathways.Business.AppUser;

namespace ILPathways.LRW.controls
{
    public partial class Authoring2 : BaseUserControl
    {
        const string thisClassName = "Authoring2";
        public string selectedTab = "";
        public string previewID;
        public string cleanTitle;
        //public string currentConditionOfUse;
        //public string conditionsOfUse_summaries;
        //public string conditionsOfUse_urls;
        //public string conditionsOfUse_iconURLs;

        ContentServices myManager = new ContentServices();

        #region Properties
        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string FormSecurityName
        {
            get { return this.txtFormSecurityName.Text; }
            set { this.txtFormSecurityName.Text = value; }
        }
        protected ContentItem CurrentRecord
        {
            get { return ViewState[ "CurrentRecord" ] as ContentItem; }
            set { ViewState[ "CurrentRecord" ] = value; }
        }
        #endregion

        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                InitializeForm();

            }
            else if ( IsUserAuthenticated() == false )
            {
                SetConsoleErrorMessage( "Error: you are not signed in (your session may have timed out). Please sign in and try again." );
                Response.Redirect( Request.RawUrl.ToString(), true );
                return;
            }
            //regardless:
            //populateConditionsOfUseData();
            //conditionsSelector.PopulateItems();
            previewID = previewLink.Text;
            if ( string.IsNullOrEmpty( txtTitle.Text ) )
                cleanTitle = "";
            else
                cleanTitle = new PublishController().FormatFriendlyTitle( this.txtTitle.Text );
            if ( Request.Form[ "__EVENTTARGET" ] != null && Request.Form[ "__EVENTTARGET" ] == "btnRemRef" )
            {
                DeleteReference( null, null );
            }
            if ( Request.Form[ "__EVENTTARGET" ] != null && Request.Form[ "__EVENTTARGET" ] == "btnRemAtt" )
            {
                DeleteAttachment( null, null );
            }
        }//
        protected void InitializeForm()
        {
            //User Authentication
            if ( IsUserAuthenticated() )
            {

                CurrentUser = GetAppUser();
                //check if authorized, if not found, will check by orgId
                this.FormPrivileges = GDAL.SecurityManager.GetGroupObjectPrivileges( CurrentUser, FormSecurityName );
                

                //}
                if ( this.FormPrivileges.CanCreate() )
                {
                    Stage1Items.Visible = true;
                    loginMessage.Visible = false;

                    CurrentRecord = new ContentItem();
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
            else
            {
                //display message, and lock down
                Stage1Items.Visible = false;
                loginMessage.Visible = true;
            }

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
            if (rid.Trim().Length == 36)
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

                //if ( btnNew.Enabled )
                //    HandleNewRequest();
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
            ResetAttachment();
            this.ResetReference();
            SetTemplates();

            //Stage2Items.Visible = false;
            tabNav.Visible = false;
            statusDiv.Visible = false;
            //do any defaults. 
            entity.IsActive = true;
            entity.Status = "Draft";
            entity.HasChanged = false;
            templatesPanel.Visible = true;
            ddlIsOrganizationContent.Enabled = true;
            btnPublish.Visible = false;
            PopulateForm( entity );

            //reset any controls or buttons not handled in the populate method

            //unselect forms list
            //this.ddlConditionsOfUse.SelectedIndex = 2;
            selectedTab = ltlBasicTabName.Text;
        }//

        /// <summary>
        /// Populate the form 
        ///</summary>
        private void PopulateForm( ContentItem entity )
        {
            CurrentRecord = entity;

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
            this.editor.EditPanel.Content = entity.Description;

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
                templatesPanel.Visible = false;
                ddlContentType.Enabled = false;
                //Page Title
                Page.Title = entity.Title + " | ISLE OER";
                lblContentType.Text = entity.ContentType;
                lblContentType.Visible = true;
                ddlContentType.Visible = false;

                //if orgId > 0, may want to have an indication that org is the owner??
                //for now always disable. changing could be complicated
                ddlIsOrganizationContent.Enabled = false;
                if ( entity.IsOrgContentOwner )
                {
                    ddlIsOrganizationContent.SelectedIndex = 1;
                    ddlIsOrganizationContent.Enabled = false;
                    if ( entity.ResourceVersionId == 0 )
                    {
                        //never published
                        btnPublish.Visible = true;
                        btnPublish.CommandName = "PublishNew";
                    }
                    else if ( entity.ResourceVersionId > 0 && entity.StatusId == ContentItem.REVISIONS_REQUIRED_STATUS )
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
                        if ( entity.ResourceVersionId > 0 )
                        {
                            btnPublish.CommandName = "PublishUpdate";
                            this.btnPublish.Attributes.Remove( "onClick");
                            this.btnPublish.Attributes.Add( "onClick", "return confirmRePublish(this);" );
                        }
                        else
                        {
                            btnPublish.CommandName = "PublishNew";
                        }
                    }
                }

                Stage2Items.Visible = true;
                tabNav.Visible = true;
                statusDiv.Visible = true;
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

                //show the finish button
                //btnFinish.Visible = true;
                //btnFinish2.Visible = true;
                historyPanel.Visible = true;
                this.lblHistory.Text = entity.HistoryTitle();

                //get all supplements
                SetAttachmentList( entity.Id );
                //get all references
                SetReferenceList( entity.Id );
                //set preview
                ShowPreview( entity );

                if ( FormPrivileges.CanDelete() )
                {
                    btnDelete.Visible = true;
                }
                btnNew.Visible = true;

                //set after content item exists
                string documentFolder = FileResourceController.DetermineDocumentPath( entity );
                string relativePath = FileResourceController.GetRelativeUrlPath( entity );
                string baseUrl = UtilityManager.GetAppKeyValue( "path.MapContentPath", "/Content/" );

                string upload = baseUrl + relativePath + "/";

                myImmediateImageInsert.UploadFolder = upload;  // "/ContentDocs/"; //documentFolder;
                //ib1.GalleryFolders = documentFolder;
                //ib1.ManagedFolders = documentFolder;
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
                btnNew.Visible = false;
                this.btnUnPublish.Visible = false;
                this.btnSetInactive.Visible = false;

                ltlAttachmentsList.Text = "";
                this.ltlReferencesList.Text = "";
                ddlContentType.Enabled = true;
            }

        }//
        void ShowPreview( ContentItem entity )
        {

            //previewLink.Text = entity.RowId.ToString(); //able to retain value through postbacks
            previewLink.Text = entity.Id.ToString(); //able to retain value through postbacks
            previewID = previewLink.Text; //able to be inserted into html properties
            if (string.IsNullOrEmpty(txtTitle.Text) )
                cleanTitle = "";
            else
                cleanTitle = new PublishController().FormatFriendlyTitle( this.txtTitle.Text );
        }
   
        #endregion

        #region Form actions

        protected void btnSave_Click( object sender, EventArgs e )
        {
            //validate
            if ( ValidateSummary() & this.IsFormValid() )
            {
                //save
                this.UpdateForm();
                //CreateNewAuthoredResourceStarterRecord();

                //hide cancel

                //enable other sections
                Stage2Items.Visible = true;
                tabNav.Visible = true;
                statusDiv.Visible = true;
                //take the user to the next step
                selectedTab = ltlWebContentTabName.Text;

                //show the finish button
                //btnFinish.Visible = true;
                //btnFinish2.Visible = true;
            }
           
            //hide cancel

        }

        protected void btnSaveWeb_Click( object sender, EventArgs e )
        {
            btnSave_Click( sender, e );
            //selectedTab = ltlAttachmentsTabName.Text;
        }
        protected void btnSaveWebContinue_Click( object sender, EventArgs e )
        {
            btnSave_Click( sender, e );
            selectedTab = ltlAttachmentsTabName.Text;
        }
        protected void btnPublish2_Click( object sender, EventArgs e )
        {

            /*cases
             * - personal, just update status, maybe resource IsActive
             * 
             * 
             * 
             */
            if ( ValidateSummary() & this.IsFormValid() )
            {
                if ( CurrentRecord.IsOrgContentOwner == false )
                {
                    //only showing if previously published==> maybe
                    this.UpdateForm( ContentItem.PUBLISHED_STATUS );
                    btnPublish.Visible = false;
                }
                else
                {
                    if ( CurrentRecord.StatusId == ContentItem.REVISIONS_REQUIRED_STATUS )
                    {
                        //update just in case
                        this.UpdateForm();

                        string statusMessage = string.Empty;
                        AppUser user = GetAppUser();
                        if ( ContentController.RequestApproval( CurrentRecord.Id, user, ref statusMessage ) == true )
                        {
                            SetConsoleSuccessMessage( "An email was sent requesting a review of your resource." );
                            //refresh
                            Get( CurrentRecord.Id );
                            btnPublish.Visible = false;
                        }
                        else
                            SetConsoleErrorMessage( "An error occurred attempting to initiate a review of your resource.<br/>System administration has been notified.<br/>" + statusMessage );

                    }
                }
                
            }
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
        private void PublishNewResource()
        {
            //TODO - do we need a mechanism to publish as https??

            string port = "";
            if ( Request.Url.Port > 80 )
                port = ":" + Request.Url.Port.ToString();

            Session.Add( "authoredResourceID", CurrentRecord.Id.ToString() );
            if ( litPreviewUrlTemplate.Text.ToLower().IndexOf( "resourcepage.aspx" ) > -1 )
                Session.Add( "authoredResourceURL", Request.Url.Scheme + "://" + Request.Url.Host + port
                                                + string.Format( litPreviewUrlTemplate.Text, CurrentRecord.RowId.ToString() ) );
            else
                Session.Add( "authoredResourceURL", UtilityManager.FormatAbsoluteUrl( string.Format( litPreviewUrlTemplate.Text, CurrentRecord.Id, new PublishController().FormatFriendlyTitle( CurrentRecord.Title ) ), false ) );

            Response.Redirect( "/Publish.aspx?rid=" + CurrentRecord.RowId.ToString(), true );
            //redirectTarget = Request.Url.Scheme + "://" + Request.Url.Host + ":" + Request.Url.Port + "/Publish.aspx";

        } // end 

        private void SubmitPublishApproveRequest()
        {
            //update just in case
            this.UpdateForm();

            string statusMessage = string.Empty;
            AppUser user = GetAppUser();
            if ( ContentController.RequestApproval( CurrentRecord.Id, user, ref statusMessage ) == true )
            {
                SetConsoleSuccessMessage( "An email was sent requesting a review of your resource." );
                //refresh
                Get( CurrentRecord.Id );
                btnPublish.Visible = false;
            }
            else
                SetConsoleErrorMessage( "An error occurred attempting to initiate a review of your resource.<br/>System administration has been notified.<br/>" + statusMessage );

        } // end 

        private void UpdateForm()
        {
            UpdateForm(0);
        }

        private void UpdateForm( int newStatusId)
        {
            int id = 0;
            string statusMessage = "";
            string action = "";
           bool isApprovalRequired = false;    
            Patron user = ( Patron ) WebUser;
            ContentItem entity = new ContentItem();

            try
            {
                Int32.TryParse ( this.txtId.Text, out id );

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

                    if ( ddlTemplates.SelectedIndex > 0 )
                    {
                        int tempid = Int32.Parse( ddlTemplates.SelectedValue);
                        ContentItem template = myManager.Get( tempid );
                        this.editor.EditPanel.Content = template.Description;
                        //TODO - do we expect templates to have references or attachments?
                    }

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
                    // get current record 
                    entity = myManager.Get(id);
                    action = "Update";
                }
                //TODO - may want means to set entity false - maybe by approver?

                /* assign form fields 			 */
                entity.Title = this.txtTitle.Text;
                entity.Summary = this.txtDescription.Text;
                entity.Description = this.editor.EditPanel.Content;

                //if ( ddlConditionsOfUse.SelectedIndex > -1)
                if( conditionsSelector.selectedIndex > -1)
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
                } else 
                {
                    entity.StatusId = DetermineStatus( entity );
                }

                if ( newStatusId == ContentItem.INACTIVE_STATUS )
                {
                    //reset rvid
                    entity.ResourceVersionId = 0;
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
                    statusMessage = myManager.Update( entity );
                }


                if ( statusMessage.Equals( "successful" ) )
                {

                    CurrentRecord = entity;
                    //maybe, or ?????

                    if ( currentStatusId != entity.StatusId || newStatusId > 0 )
                    {
                        Get( entity.Id );
                    }
                    if ( newStatusId != ContentItem.INACTIVE_STATUS)
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

            return statusId;
        }

        protected void btnNew_Click( object sender, EventArgs e )
        {
            //reset form
            ResetForm();
        }
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

                if ( myManager.Delete( id, ref statusMessage ) )
                {
                    //TODO - need to delete LR related stuff!
                    if ( CurrentRecord.ResourceVersionId > 0 )
                    {
                        ResourceVersion entity = new ResourceVersionManager().Get( CurrentRecord.ResourceVersionId );
                        if ( entity != null && entity.Id > 0 )
                        {
                            string status = new ResourceManager().SetResourceActiveState( false, CurrentRecord.ResourceVersionId );
                            //note can have a RV id not not be published to LR. Need to check for a resource docid
                            if ( entity.LRDocId != null && entity.LRDocId.Length > 10 )
                            {
                                //post request to delete ==> this process would take care of actual delete of the Resource hierarchy
                                extraMessage = "<br/>( WELL ALMOST - NEED TO ADD CODE TO REMOVE FROM THE LR, AND DO PHYSICAL DELETE OF THE RESOURCE HIERARCHY - JEROME)";
                            }

                        }
                    }

                    this.SetConsoleSuccessMessage( "Delete of record was successful " + extraMessage );
                    this.ResetForm();
                }
                else
                {
                    this.SetConsoleErrorMessage( statusMessage );
                }

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
                    //save rvid for later
                    int rvid = CurrentRecord.ResourceVersionId;

                    this.UpdateForm( ContentItem.INACTIVE_STATUS );
                    //probably need a check that update was successful

                    //TODO - should a history record be created?

                    string extraMessage = "";
                    if ( rvid > 0 )
                    {
                        ResourceVersion entity = new ResourceVersionManager().Get( rvid );
                        if ( entity != null && entity.Id > 0 )
                        {
                            string status = new ResourceManager().SetResourceActiveState( false, rvid );
                            //note can have a RV id not not be published to LR. Need to check for a resource docid
                            if ( entity.LRDocId != null && entity.LRDocId.Length > 10 )
                            {
                                //need to call methods to remove from LR
                                //==> this process would take care of actual delete of the Resource hierarchy
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

        protected void btnFinish_Click( object sender, EventArgs e )
        {
            btnSave_Click( sender, e );
            if ( CurrentRecord.HasValidRowId())
                Response.Redirect( "/Repository/ResourcePage.aspx?rid=" + CurrentRecord.RowId.ToString() );
            else 
                Response.Redirect( "/Repository/ResourcePage.aspx?cidx=" + CurrentRecord.Id );
        }

        protected void btnNewReference_Click( object sender, EventArgs e )
        {
            ResetReference();
            selectedTab = ltlReferencesTabName.Text;
        }
        protected void ResetReference()
        {
            ContentReference entity = new ContentReference();
            entity.ReferenceUrl = "http://";
            ddlReference.SelectedIndex = -1;
            PopulateReference( entity );
        }
        protected void btnSaveReference_Click( object sender, EventArgs e )
        {
            if ( IsReferenceValid() )
            {
                UpdateReference();
            }
            selectedTab = ltlReferencesTabName.Text;
        }
        protected void btnDeleteReference_Click( object sender, EventArgs e )
        {
            int id = 0;

            try
            {
                if ( Int32.TryParse( this.txtReferenceId.Text, out id ) )
                {
                    DeleteReference( id );
                }
            }
            catch ( System.Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".btnDeleteReference_Click() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }

            selectedTab = ltlReferencesTabName.Text;
        }

        protected void btnNewAttachment_Click( object sender, EventArgs e )
        {
            ResetAttachment();
            selectedTab = ltlAttachmentsTabName.Text;
        }
        protected void ResetAttachment()
        {
            ContentSupplement entity = new ContentSupplement();
            ddlAttachment.SelectedIndex = -1;
            PopulateAttachment( entity );
        }
        protected void btnSaveAttachment_Click( object sender, EventArgs e )
        {
            if ( IsAttachmentValid() )
            {
                UpdateAttachment();
            }
            selectedTab = ltlAttachmentsTabName.Text;
        }
        protected void btnSaveAttachmentContinue_Click( object sender, EventArgs e )
        {
            if ( IsAttachmentPresent() == false )
            {
                selectedTab = ltlReferencesTabName.Text;
            }
            else 
            if ( IsAttachmentValid() )
            {
                UpdateAttachment();
                selectedTab = ltlReferencesTabName.Text;
            }
            else
            {
                selectedTab = ltlAttachmentsTabName.Text;
            }
        }
        protected void btnDeleteAttachment_Click( object sender, EventArgs e )
        {
            int id = 0;

            try
            {
                if ( Int32.TryParse( this.txtAttachmentId.Text, out id ) )
                {
                    DeleteAttachment( id );
                }
            }
            catch ( System.Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".btnDeleteAttachment_Click() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }

            selectedTab = ltlAttachmentsTabName.Text;
        }
        #endregion

        #region Attachment handling
        protected void ddlAttachment_SelectedIndexChanged( object sender, System.EventArgs ea )
        {

            try
            {
                if ( ddlAttachment.SelectedIndex > 0 )
                {
                    int id = Int32.Parse(ddlAttachment.SelectedValue.ToString());

                    ContentSupplement entity = myManager.ContentSupplementGet( id );
                    if ( entity == null || entity.Id == 0 )
                    {
                        this.SetConsoleErrorMessage( "Sorry the requested attachment record does not exist" );
                        return;
                    }
                    else
                    {

                        PopulateAttachment( entity );
                    }
                }
                else
                {
                    SetConsoleInfoMessage( "Please select a valid item from the list" );
                }
            }
            catch ( Exception ex )
            {
                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.Message );
                this.LogError( ex, thisClassName + ".ddlAttachment_SelectedIndexChanged" );
            }
        } //
        private void PopulateAttachment( ContentSupplement entity )
        {
            selectedTab = ltlAttachmentsTabName.Text;

            this.txtAttachmentId.Text = entity.Id.ToString();
            this.txtFileTitle.Text = entity.Title;
            this.txtAttachmentSummary.Text = entity.Description;
            this.txtDocumentRowId.Text = "";

            this.SetListSelection( this.ddlAttachmentPrivacyLevel, entity.PrivilegeTypeId.ToString() );

            //check for doc info and display
            if ( entity.DocumentRowId != null && entity.IsInitialGuid( entity.DocumentRowId ) == false )
            {
                currentFilePanel.Visible = true;
                this.txtDocumentRowId.Text = entity.DocumentRowId.ToString();
                DocumentVersion doc = myManager.DocumentVersionGet( entity.DocumentRowId );
                //lblDocumentLink.Text = "View " + doc.Title;
                lblDocumentLink.Text = doc.FileName;
                this.currentFileName.Text = doc.FileName;

                entity.RelatedDocument = doc;

                //check if file exists on server, if not it will be downloaded
                lblDocumentLink.NavigateUrl = ValidateDocumentOnServer( CurrentRecord, doc );
            }
            else
            {
                currentFilePanel.Visible = false;
                //shouldn't happen after completed coding
                this.txtDocumentRowId.Text = "";

                lblDocumentLink.Text = "";
                this.currentFileName.Text = "";
            }


            if ( entity.Id > 0 )
            {
                if ( FormPrivileges.CanDelete() )
                {
                    btnDeleteAttachment.Visible = true;
                }
            }
            else
            {
                btnDeleteAttachment.Visible = false;
            }
        }//
        protected void btnUpload_Click( object sender, EventArgs e )
        {
            //validate
            if ( !IsFilePresent() )
            {
                SetConsoleErrorMessage( "Please select a file before clicking upload" );
                selectedTab = ltlAttachmentsTabName.Text;
                return;
            }
            LoggingHelper.DoTrace( 5, thisClassName + " btnUpload_Click" );

            //do upload

            CurrentUser = GetAppUser();
            
            //string statusMessage = "";
            //bool isValid = HandleDocument( CurrentRecord, CurrentUser, true, ref statusMessage );

            selectedTab = ltlAttachmentsTabName.Text;
        }

        private bool IsAttachmentPresent()
        {
            bool hasData = false;
            if ( this.txtFileTitle.Text.Trim().Length > 0
                || this.txtAttachmentSummary.Text.Trim().Length > 0
                || ddlAttachmentPrivacyLevel.SelectedIndex > 0
                || IsFilePresent()
                )
                hasData = true;

            return hasData;
        }

        private bool IsAttachmentValid()
        {
            bool isValid = true;
            StringBuilder errorBuilder = new StringBuilder( "" );

            if ( this.txtFileTitle.Text.Trim().Length < 10 )
            {
                errorBuilder.Append( "A title of at least 10 characters is required for an attachment.<br/>" );
            }
            //if ( this.txtAttachmentSummary.Text.Trim().Length < 20 )
            //{
            //    errorBuilder.Append( "A description of at least 20 characters is required for the attachment summary.<br/>" );
            //}
            //
            int id = 0;
            Int32.TryParse( this.txtAttachmentId.Text, out id );
            if (id == 0 && !IsFilePresent() )
            {
                errorBuilder.Append( "A file must be selected for an attachment.<br/>" );
            }
            if ( ddlAttachmentPrivacyLevel.SelectedIndex < 1)
            {
                errorBuilder.Append( "A privacy level must be selected for an attachment.<br/>" );
            }
            if ( errorBuilder.ToString().Length > 0 )
            {
                SetConsoleErrorMessage( errorBuilder.ToString() );
                isValid = false;
            }
            return isValid;

        }
        
        /// <summary>
        /// Handle Update Attachment
        /// </summary>
        private void UpdateAttachment()
        {
            int id = 0;
            string action = "";
            bool isUpdate;
            string statusMessage = "";
            CurrentUser = GetAppUser();

            ContentSupplement entity = new ContentSupplement();

            try
            {
                bool isvalid = Int32.TryParse( this.txtAttachmentId.Text, out id );
                if ( isvalid == false )
                {
                    SetConsoleErrorMessage( "Error - invalid value for attachment identifier. Try to create a new attachment" );
                    return;
                }

                if ( id == 0 )
                {
                    isUpdate = false;
                    entity.ParentId = CurrentRecord.Id;
                    entity.CreatedBy = WebUser.FullName();
                    entity.CreatedById = WebUser.Id;
                    entity.Created = System.DateTime.Now;
                    action = "Create";
                }
                else
                {
                    // get current record
                    entity = myManager.ContentSupplementGet(id);
                    isUpdate = true;
                    action = "Update";
                }

                /* assign form fields 			 */
                entity.Title = this.txtFileTitle.Text;
                entity.Description = this.txtAttachmentSummary.Text;
                entity.PrivilegeTypeId = Int32.Parse( ddlAttachmentPrivacyLevel.SelectedValue.ToString() );
                
                entity.LastUpdated = System.DateTime.Now;
                entity.LastUpdatedBy = WebUser.FullName();
                entity.LastUpdatedById = WebUser.Id;		//include for future use

                // if file exists, check
                if ( IsFilePresent() )
                {
                    bool isValid = HandleDocument( CurrentRecord, entity, CurrentUser, false, ref statusMessage );
                    if ( isValid )
                    {
                        //will have been set in HandleDocument
                        entity.DocumentRowId = entity.RelatedDocument.RowId;
                    }
                    else
                    {
                        //problem, should have displayed a message
                        return;
                    }
                }
       
                //call insert/update
                statusMessage = "";
                if ( isUpdate == false )
                {
                    //first need to save the document (and get the document id
                    id = myManager.ContentSupplementCreate( entity, ref statusMessage );
                    if ( id > 0 )
                    {
                        this.txtAttachmentId.Text = id.ToString();
                        entity.Id = id;
                    }
                }
                else
                {
                    statusMessage = myManager.ContentSupplementUpdate( entity );
                }

                if ( statusMessage.Equals( "successful" ) )
                {
                    //update CurrentRecord and form????
                    //PopulateForm( entity );

                    this.SetConsoleSuccessMessage( action + " was successful for: " + entity.Title );
                    this.SetAttachmentList( CurrentRecord.Id);

                    this.ResetAttachment();
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
        private void DeleteAttachment( object sender, System.EventArgs e )
        {
            //Response.Write ( "You Clicked on " + Request.Form[ "__EVENTARGUMENT" ].ToString () );
            string key = Request.Form[ "__EVENTARGUMENT" ].ToString();

            int id = Int32.Parse( key );
            DeleteAttachment( id );
        }
        private void DeleteAttachment( int id )
        {
            string statusMessage = "";

            try
            {
                 if ( myManager.ContentSupplementDelete( id, ref statusMessage ) )
                    {
                        SetConsoleSuccessMessage( "Attachment was deleted" );
                        //reset lists as needed for removed records
                        this.SetAttachmentList( CurrentRecord.Id );
                        //this.();
                        //check if current matches that deleted
                        int currentId = 0;
                        if ( Int32.TryParse( this.txtAttachmentId.Text, out currentId ) )
                        {
                            if ( currentId == id )
                                this.ResetAttachment();
                        }
                    }
                    else
                    {
                        SetConsoleErrorMessage( "Error was encountered attempting to delete the attachment:<br/>" + statusMessage );
                    }
            }
            catch ( System.Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".DeleteAttachment() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }

            selectedTab = ltlAttachmentsTabName.Text;
        } 
        protected bool HandleDocument( ContentItem parentEntity, ContentSupplement attachment, Patron user, bool uploadOnly, ref string statusMessage )
        {
            bool isValid = true;
            string documentRowId = "";

            string action = "";
            bool isUpdate;
            //!!! need to check if pertinent fields have all been cleared - if so then delete??
            //if just the title etc, changed, then maybe we don't want to do a full update (ie the image data)??
            //there may a request to update the title as well
            if ( !IsFilePresent() )
            {
                // no file, skip - this only ok , if update
                statusMessage = "Error no document";
                return false;
            }

            DocumentVersion entity = new DocumentVersion();
            documentRowId = this.txtDocumentRowId.Text;

            if ( documentRowId.Length < 10
                || documentRowId == entity.DEFAULT_GUID
                || uploadOnly == true)
            {
                isUpdate = false;
                entity.RowId = new Guid();

                entity.CreatedById = user.Id;
                entity.CreatedBy = user.FullName();
                entity.Created = System.DateTime.Now;
                action = "Create";

            }
            else
            {
                
                entity = myManager.DocumentVersionGet( documentRowId );
                isUpdate = true;
                action = "Update";
            }

            entity.LastUpdatedById = user.Id;
            entity.Title = this.txtFileTitle.Text;

           
            //check if file was specified (versus updating text for existing file)
            if ( IsFilePresent() )
            {
                try
                {
                    int maxFileSize = UtilityManager.GetAppKeyValue( "maxDocumentSize", 4000000 );
                    //TODO - if an image, do a resize
                    if ( FileResourceController.IsFileSizeValid( fileUpload, maxFileSize ) == false )
                    {
                        SetConsoleErrorMessage( string.Format("Error the selected file exceeds the size limits ({0} bytes).", maxFileSize ));
                        return false;
                    }

                    entity.MimeType = fileUpload.PostedFile.ContentType;
                    entity.FileName = fileUpload.FileName;
                    entity.FileDate = System.DateTime.Now;

                    string sFileType = System.IO.Path.GetExtension( entity.FileName );
                    sFileType = sFileType.ToLower();

                    entity.FileName = System.IO.Path.ChangeExtension( entity.FileName, sFileType );

                    //probably want to fix filename to standardize
                    entity.CleanFileName();

                    //LoggingHelper.DoTrace( 5, thisClassName + " HandleDocument(). calling DetermineDocumentPath" );
                    string documentFolder = FileResourceController.DetermineDocumentPath( parentEntity );
                    // LoggingHelper.DoTrace( 5, thisClassName + " - uploading to " + documentFolder );
                    entity.URL = FileResourceController.DetermineDocumentUrl( parentEntity, entity.FileName );
                    attachment.ResourceUrl = entity.URL;

                    //try separating following to insulate from file privileges issues
                    UploadFile( documentFolder, entity.FileName );
                    //rewind for db save
                    fileUpload.PostedFile.InputStream.Position = 0;
                    Stream fs = fileUpload.PostedFile.InputStream;

                    entity.ResourceBytes = fs.Length;
                    byte[] data = new byte[ fs.Length ];
                    fs.Read( data, 0, data.Length );
                    fs.Close();
                    fs.Dispose();
                    entity.SetResourceData( entity.ResourceBytes, data );
                }
                catch ( Exception ex )
                {
                    SetConsoleErrorMessage( "Unexpected error occurred while attempting to upload your file.<br/>" + ex.Message );
                    return false;
                }
            }
            //this.currentFileName.Text = entity.FileName;

            if ( uploadOnly )
            {
                //lblDocumentLink.NavigateUrl = attachment.ResourceUrl;
                //lblDocumentLink.Text = "View: " + entity.FileName;
                //lblDocumentLink.Visible = true;

            }
            else
            {
                if ( entity.Id == 0 )
                {
                    string documentId = myManager.DocumentVersionCreate( entity, ref statusMessage );
                    if ( documentId.Length > 0 )
                    {
                        this.txtDocumentRowId.Text = documentId.ToString();
                        entity.RowId = new Guid( documentId );
                        statusMessage = "Successfully saved document!";
                       
                    }
                    else
                    {
                        txtDocumentRowId.Text = "0";
                        statusMessage = "Document save failed: " + statusMessage;
                        lblDocumentLink.Visible = false;
                        isValid = false;
                    }
                }
                else
                {
                    statusMessage = myManager.DocumentVersionUpdate( entity );
                    if ( statusMessage.Equals( "successful" ) )
                    {

                        statusMessage = "Successfully updated Image!";
                    }
                    else
                    {
                        isValid = false;
                        lblDocumentLink.Visible = false;
                    }

                }
            }

            if ( isValid )
            {
                attachment.DocumentRowId = entity.RowId;
                attachment.RelatedDocument = entity;

                ValidateDocumentOnServer( parentEntity, entity );
            }

            return isValid;

        }//

        private void UploadFile( string documentFolder, string filename )
        {
            try
            {
                FileSystemHelper.CreateDirectory( documentFolder );

                string diskFile = documentFolder + "\\" + filename;
                //string diskFile = MapPath( documentFolder ) + "\\" + entity.FileName;

                LoggingHelper.DoTrace( 5, thisClassName + " UploadFile(). doing SaveAs" );
                fileUpload.SaveAs( diskFile );

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format("UploadFile(documentFolder: {0}, filename: {1})", documentFolder, filename) );
            }
        }//

        private string ValidateDocumentOnServer( ContentItem parentEntity, DocumentVersion doc )
        {
            string fileUrl = "";

            try
            {
                string documentFolder = FileResourceController.DetermineDocumentPath( parentEntity );
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

        private bool IsUploadValid()
        {
            //int recId = 0;
            bool isValid = true;

            try
            {

                //for new document, current record is required
                if ( this.txtRowId.Text.Length == 0 || txtRowId.Text == "0")
                {

                    if ( !IsFilePresent() )
                    {
                        SetConsoleErrorMessage( "Please select a file before clicking upload" );
                        // this.AddReqValidatorError( vsPvattErrorSummary, rfvFileUpload.ErrorMessage, "fileUpload" );
                        return false;
                    }
                }
                else
                {
                    //if no existing file or new file, ignore other fields
                    if ( !IsFilePresent() )
                        return true;
                }

            }
            catch ( System.Exception ex )
            {
                //SetFormMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.ToString() );
                return false;
            }

            return isValid;
        } //
        
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


        #endregion

        #region References handling
        protected void ddlReference_SelectedIndexChanged( object sender, System.EventArgs ea )
        {

            try
            {
                if ( ddlReference.SelectedIndex > 0 )
                {
                    int id = Int32.Parse( ddlReference.SelectedValue.ToString() );

                    ContentReference entity = myManager.ContentReferenceGet( id );
                    if ( entity == null || entity.Id == 0 )
                    {
                        this.SetConsoleErrorMessage( "Sorry the requested record does not exist" );
                        return;
                    }
                    else
                    {
                        PopulateReference( entity );
                    }
                }
                else
                {
                    SetConsoleInfoMessage( "Please select a valid item from the list" );
                }
            }
            catch ( Exception ex )
            {
                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.Message );
                this.LogError( ex, thisClassName + ".ddlReference_SelectedIndexChanged" );
            }
        } //
        private void PopulateReference( ContentReference entity )
        {
            selectedTab = ltlReferencesTabName.Text;

            this.txtReferenceId.Text = entity.Id.ToString();
            this.txtAuthor.Text = entity.Author;
            this.txtPublisher.Text = entity.Publisher;
            this.txtPublicationTitle.Text = entity.Title;
            txtAdditionalInfo.Text = entity.AdditionalInfo;

            this.txtISBN.Text = entity.ISBN;
            this.txtUrl.Text = entity.ReferenceUrl;

            this.txtDocumentRowId.Text = "";

            if ( entity.Id > 0 )
            {
                if ( FormPrivileges.CanDelete() )
                {
                    btnDeleteReference.Visible = true;
                }
            }
            else
            {
                btnDeleteReference.Visible = false;
            }
        }//
        private bool IsReferenceValid()
        {
            bool isValid = true;
            StringBuilder errorBuilder = new StringBuilder( "" );

            //if ( this.txtAuthor.Text.Trim().Length < 10 )
            //{
            //    errorBuilder.Append( "An author is required for this reference.<br/>" );
            //}
            //if ( this.txtPublisher.Text.Trim().Length < 10 )
            //{
            //    errorBuilder.Append( "A Publisher is (probably) required for this reference.<br/>" );
            //}
            if ( this.txtPublicationTitle.Text.Trim().Length < 3 )
            {
                errorBuilder.Append( "A title (of at least 3 characters) is required for this resource.<br/>" );
            }
            
            if ( errorBuilder.ToString().Length > 0 )
            {
                SetConsoleErrorMessage( errorBuilder.ToString() );
                isValid = false;
            }

            if ( CheckValidURL( txtUrl.Text ) == false )
            {
                SetConsoleErrorMessage( "Error: Invalid URL" );
                isValid = false;
            }
            return isValid;

        }

        void UpdateReference()
        {
            int id = 0;
            string action = "";
            bool isUpdate;
            string statusMessage = "";
            refMessage.Text = "";

            ContentReference entity = new ContentReference();

            try
            {
                Int32.TryParse( this.txtReferenceId.Text, out id );

                if ( id == 0 )
                {
                    isUpdate = false;
                    entity.ParentId = CurrentRecord.Id;
                    entity.CreatedBy = WebUser.FullName();
                    entity.CreatedById = WebUser.Id;
                    entity.Created = System.DateTime.Now;
                    action = "Create";
                }
                else
                {
                    // get current record
                    entity = myManager.ContentReferenceGet(id);
                    isUpdate = true;
                    action = "Update";
                }

                /* assign form fields 			 */
                entity.Author = this.txtAuthor.Text;
                entity.Publisher = this.txtPublisher.Text;
                entity.Title = this.txtPublicationTitle.Text;
                entity.AdditionalInfo = this.txtAdditionalInfo.Text;
                entity.ISBN = this.txtISBN.Text;
                if ( this.txtUrl.Text.ToLower() == "http://" )
                    entity.ReferenceUrl = "";
                else
                    entity.ReferenceUrl = this.txtUrl.Text;

                entity.LastUpdated = System.DateTime.Now;
                entity.LastUpdatedBy = WebUser.FullName();
                entity.LastUpdatedById = WebUser.Id;		//include for future use

                //call insert/update
                statusMessage = "";
                if ( isUpdate == false )
                {
                    //first need to save the document (and get the document id
                    id = myManager.ContentReferenceCreate( entity, ref statusMessage );
                    if ( id > 0 )
                    {
                        this.txtReferenceId.Text = id.ToString();
                        entity.Id = id;
                    }
                }
                else
                {
                    statusMessage = myManager.ContentReferenceUpdate( entity );
                }

                if ( statusMessage.Equals( "successful" ) )
                {
                    //refMessage is only used with update panel - where SetConsoleSuccessMessage is not display
                    //refMessage.Text = action + " was successful for: " + entity.Title;
                    this.SetConsoleSuccessMessage( action + " was successful for: " + entity.Title );
                    this.SetReferenceList( CurrentRecord.Id );

                    this.ResetReference();
                }
                else
                {
                    this.SetConsoleErrorMessage( statusMessage );
                }

            }
            catch ( System.Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".UpdateReference() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }
        }
        private void DeleteReference( object sender, System.EventArgs e )
        {
            //Response.Write ( "You Clicked on " + Request.Form[ "__EVENTARGUMENT" ].ToString () );
            string key = Request.Form[ "__EVENTARGUMENT" ].ToString();

            int id = Int32.Parse( key );
            DeleteReference( id );
        }
        private void DeleteReference( int id )
        {
            //SetConsoleInfoMessage( "DELETE REFERENCE NOT IMPLEMENTED, COMING SOON" );
            string statusMessage = "";

            try
            {
                if ( myManager.ContentReferenceDelete( id, ref statusMessage ) )
                {
                    SetConsoleSuccessMessage( "Resource was deleted" );
                    //reset lists as needed for removed records
                    this.SetReferenceList( CurrentRecord.Id );
                    //may not want to reset now, if always invocated from list
                    //check if current matches that deleted
                    int currentId = 0;
                    if ( Int32.TryParse( this.txtReferenceId.Text, out currentId ) )
                    {
                        if ( currentId == id )
                            this.ResetReference();
                    }

                    
                }
                else
                {
                    SetConsoleErrorMessage( "Error was encountered attempting to delete the Resource:<br/>" + statusMessage );
                }
            }
            catch ( System.Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".btnDeleteAttachment_Click() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }

            selectedTab = ltlReferencesTabName.Text;
        } 
        #endregion

        #region Housekeeping
        /// <summary>
        /// Populate form controls
        /// </summary>
        /// <param name="recId"></param>
        private void PopulateControls()
        {
            bool hasHomeContent = false;
            string booleanOperator = "AND";
            //check if already has a home content
            string filter = "";
            string where = string.Format( "([TypeId] = 20 AND base.CreatedById = {0}) ", CurrentUser.Id );
            filter = BDM.FormatSearchItem( filter, where, booleanOperator );
            int pTotalRows = 0;
            DataSet ds = myManager.Search( filter, "", 1, 25, ref pTotalRows );
            if ( DoesDataSetHaveRows( ds ) )
                hasHomeContent = true;
            SetContentType2( hasHomeContent );
            //SetContentType();

            //check if org admin, and whether already has an org home page

            SetPrivilegeLists();
            //SetConditionsOfUse();
            SetTemplates();
        } //

        private void SetContentType()
        {
            //may only allow on first create?
            DataSet ds = myManager.ContentType_Select();
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                templatesPanel.Visible = true;
                BDM.PopulateList( this.ddlContentType, ds, "Id", "Title", "Select type" );
            }
        }
        private void SetContentType2( bool hasHomeContent )
        {
            //may only allow on first create?
            DataSet ds = myManager.ContentType_Select();
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                templatesPanel.Visible = true;
                ListItem item = new ListItem( "Select type", "0" );
                ddlContentType.Items.Add( item );

                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    string id = BDM.GetRowColumn( dr, "Id", "0" );
                    string title = BDM.GetRowColumn( dr, "Title", "-" );
                    
                    item = new ListItem( title, id );
                    if ( id == "20" && hasHomeContent )
                        item.Enabled = false;
                    else
                        item.Enabled = true;

                    ddlContentType.Items.Add( item );
                }
                ddlContentType.SelectedIndex = 0;
                //PopulateList2( this.ddlContentType, ds, "Id", "Title", "Select type" );
            }
        }

        private void SetTemplates()
        {
            //may only allow on first create?
            DataSet ds = myManager.SelectOrgTemplates( CurrentUser.OrgId );
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                templatesPanel.Visible = true;
                BDM.PopulateList( this.ddlTemplates, ds, "Id", "Title", "Select template" );
            }
        }
       
        /*private void populateConditionsOfUseData()
        {
            DataSet ds = CodeTableManager.ConditionsOfUse_Select();
            StringBuilder summaryBuilder = new StringBuilder();
            StringBuilder urlBuilder = new StringBuilder();
            StringBuilder iconURLBuilder = new StringBuilder();
            foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
            {
                //ddlConditionsOfUse.Items.Add( new ListItem( DatabaseManager.GetRowColumn( dr, "Title" ), DatabaseManager.GetRowColumn( dr, "Id" ) ) );
                summaryBuilder.Append( "\"" + DatabaseManager.GetRowColumn( dr, "Description" ) + "\"," );
                urlBuilder.Append( "\"" + DatabaseManager.GetRowColumn( dr, "Url" ) + "\"," );
                iconURLBuilder.Append( "\"" + DatabaseManager.GetRowColumn( dr, "IconUrl" ) + "\"," );
            }
            conditionsOfUse_summaries = summaryBuilder.ToString();
            conditionsOfUse_urls = urlBuilder.ToString();
            conditionsOfUse_iconURLs = iconURLBuilder.ToString();

        }*/
       
        /*private void SetConditionsOfUse()
        {
            //Conditions of Use
            DataSet ds = CodeTableManager.ConditionsOfUse_Select();
            StringBuilder summaryBuilder = new StringBuilder();
            StringBuilder urlBuilder = new StringBuilder();
            StringBuilder iconURLBuilder = new StringBuilder();
            foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
            {
                ddlConditionsOfUse.Items.Add( new ListItem( DatabaseManager.GetRowColumn( dr, "Title" ), DatabaseManager.GetRowColumn( dr, "Id" ) ) );
                //summaryBuilder.Append( "\"" + DatabaseManager.GetRowColumn( dr, "Description" ) + "\"," );
                //urlBuilder.Append( "\"" + DatabaseManager.GetRowColumn( dr, "Url" ) + "\"," );
                //iconURLBuilder.Append( "\"" + DatabaseManager.GetRowColumn( dr, "IconUrl" ) + "\"," );
            }
            //conditionsOfUse_summaries = summaryBuilder.ToString();
            //conditionsOfUse_urls = urlBuilder.ToString();
            //conditionsOfUse_iconURLs = iconURLBuilder.ToString();
            //DataSet ds = LDAL.CodeTableManager.ConditionsOfUse_Select();
            //BDM.PopulateList( this.ddlConditionsOfUse, ds, "Id", "Title", "Select rights" );
        }*/
        private void SetPrivilegeLists()
        {
            DataSet ds = myManager.ContentPrivilegeCodes_Select();
            DataSet ds2 = ds.Copy();

            BDM.PopulateList( this.ddlPrivacyLevel, ds, "Id", "Title", "Select access level" );

            BDM.PopulateList( this.ddlAttachmentPrivacyLevel, ds2, "Id", "Title", "Select >>" );
        }

        private void SetAttachmentList( int parentId )
        {
            List<ContentSupplement> list = myManager.ContentSupplementsSelectList( parentId );

            //still using ddl even though hidden
            DataSet ds = myManager.ContentSupplementsSelect( parentId );
            BDM.PopulateList( ddlAttachment, ds, "Id", "Title", "Select an attachment" );

            AddAttachmentsToList( list );

        } //
        private void AddAttachmentsToList( List<ContentSupplement> list )
        {
            ltlAttachmentsList.Text = "";
            string resourceUrl = "";
            foreach ( ContentSupplement item in list ) 
            {
                if ( item.Id == 0 )
                {
                    continue; // Prevent "select an attachment" from showing as an option
                }
                resourceUrl = "";
                if ( item.RelatedDocument != null && item.RelatedDocument.HasValidRowId() )
                {
                    //if ( IsTestEnv() )
                    //    supplements.Text += "<br/>(test only): " + item.RelatedDocument.FileName;

                    //check if file exists on server, if not it will be downloaded
                    string fileUrl = ValidateDocumentOnServer( CurrentRecord, item.RelatedDocument );
                    if ( fileUrl.Length > 10 )
                    {
                        resourceUrl = fileUrl;
                    }
                    else
                        resourceUrl = "#"; ;
                }
                string[] arguments = new string[] {
                    item.Title,
                    item.Description,
                    item.PrivilegeType,
                    item.Id.ToString(),
                    resourceUrl
                };
                ltlAttachmentsList.Text = ltlAttachmentsList.Text +
                    //String.Format( ltlAppliedAttachmentTemplate.Text, DatabaseManager.GetRowColumn( dr, "Title" ), DatabaseManager.GetRowColumn( dr, "Description" ), DatabaseManager.GetRowColumn( dr, "PrivilegeType" ) );
                    String.Format( ltlAppliedAttachmentTemplate.Text, arguments );
            }
        }
        private void SetAttachmentList2( int parentId )
        {
            DataSet ds = myManager.ContentSupplementsSelect( parentId );
            BDM.PopulateList( ddlAttachment, ds, "Id", "Title", "Select an attachment" );

            AddAttachmentsToList2( ds );

        } //

        private void AddAttachmentsToList2( DataSet ds )
        {
            ltlAttachmentsList.Text = "";
            foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
            {
                if ( DatabaseManager.GetRowColumn( dr, "Id" ) == "0" )
                {
                    continue; // Prevent "select an attachment" from showing as an option
                }
                string[] arguments = new string[] {
                    DatabaseManager.GetRowColumn( dr, "Title" ),
                    DatabaseManager.GetRowColumn( dr, "Description" ),
                    DatabaseManager.GetRowColumn( dr, "PrivilegeType" ),
                    DatabaseManager.GetRowColumn( dr, "Id" ),
                    DatabaseManager.GetRowColumn( dr, "ResourceUrl" )
                };
                ltlAttachmentsList.Text = ltlAttachmentsList.Text +
                    //String.Format( ltlAppliedAttachmentTemplate.Text, DatabaseManager.GetRowColumn( dr, "Title" ), DatabaseManager.GetRowColumn( dr, "Description" ), DatabaseManager.GetRowColumn( dr, "PrivilegeType" ) );
                    String.Format( ltlAppliedAttachmentTemplate.Text, arguments );
            }
        }


        private void SetReferenceList( int parentId )
        {
            DataSet ds = myManager.ContentReferencesSelect( parentId );
            BDM.PopulateList( ddlReference, ds, "Id", "Title", "Select a Resource" );

            AddReferencesToList( ds );

        } //
        private void AddReferencesToList( DataSet ds )
        {
            ltlReferencesList.Text = "";
            string[] targetColumnNames = ltlReferenceColumns.Text.Split(',');
            string[] targetColumnDisplays = ltlReferenceColumnsDisplay.Text.Split(',');

            foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
            {
                if ( DatabaseManager.GetRowColumn( dr, "Id" ) == "0" )
                {
                    continue; // Prevent "select an attachment" from showing as an option
                }
                ltlReferencesList.Text = ltlReferencesList.Text + "<div class=\"appliedReference\">";
                ltlReferencesList.Text = ltlReferencesList.Text + String.Format( ltlAppliedReferencesHeader.Text, DatabaseManager.GetRowColumn( dr, "Id" ) );
                for ( int i = 0 ; i < targetColumnNames.Length ; i++ )
                {
                    UseReferenceTemplate( dr, targetColumnNames[ i ], targetColumnDisplays[ i ] );
                }
                ltlReferencesList.Text = ltlReferencesList.Text + "</div>";
            }
        }

        private void UseReferenceTemplate( DataRow dr, string columnName, string displayName )
        {
            if ( DatabaseManager.GetRowColumn( dr, columnName ) != "" )
            {
                ltlReferencesList.Text = ltlReferencesList.Text +
                    String.Format( ltlAppliedReferencesTemplate.Text, displayName, DatabaseManager.GetRowColumn( dr, columnName ) );
            }
        }
        #endregion

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
            if( conditionsSelector.conditionsURL.Length < int.Parse(ltlMinUsageRightsURLLength.Text ) )
            {
                SetConsoleErrorMessage( "You must select or enter a URL in the Usage Rights section." );
                isValid = false;
            }

            return isValid;
        }

        protected bool CheckValidURL( string referenceURL )
        {
            bool isValid = true;
            if ( referenceURL.Trim().Length == 0 
              || referenceURL.ToLower() == "http://" )
                return true;

            //Check valid URL protocol
            if ( referenceURL.IndexOf( "http://" ) != 0 & referenceURL.IndexOf( "https://" ) != 0 & referenceURL.IndexOf( "ftp://" ) != 0 )
            {
                //Attempt to fix incomplete URL
                referenceURL = "http://" + referenceURL;
            }

            try
            {
                HttpWebRequest request = ( HttpWebRequest ) WebRequest.Create( referenceURL );
                HttpWebResponse response = ( HttpWebResponse ) request.GetResponse();
                if ( response.StatusCode.ToString() != "OK" & response.StatusCode.ToString() != "200" )
                {
                    isValid = false;
                }
            }
            catch ( Exception ex )
            {
                isValid = false;
            }

            return isValid;
        }

        #endregion

    }
}
