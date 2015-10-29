using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Obout.Ajax.UI.TreeView;
using GDAL = Isle.BizServices;
using ILPathways.Business;
using ILPathways.Common;
using IOER.Controllers;

using IOER.Library;
using ILPathways.Utilities;
using Isle.BizServices;
using Isle.DTO;
using LRWarehouse.DAL;
using LRWarehouse.Business;
using BDM = LRWarehouse.DAL.BaseDataManager;
using AppUser = LRWarehouse.Business.Patron; //ILPathways.Business.AppUser;



namespace IOER.Controls.Content
{
    public partial class Authoring : BaseUserControl
    {
        const string thisClassName = "Authoring2";
        public string selectedTab = "";
        public string previewID;
        public string cleanTitle;
        public string userProxyId { get; set; }

        //public string currentConditionOfUse;
        //public string conditionsOfUse_summaries;
        //public string conditionsOfUse_urls;
        //public string conditionsOfUse_iconURLs;

        ContentServices myManager = new ContentServices();
		ContentSearchServices mySearchManager = new ContentSearchServices();
        CurriculumServices crcManager = new CurriculumServices();
        

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
        /// Get/Set current Content item
        /// </summary>
        protected ContentItem CurrentRecord
        {
            get { return ViewState[ "CurrentRecord" ] as ContentItem; }
            set { ViewState[ "CurrentRecord" ] = value; }
        }
        #endregion

        protected void Page_Load( object sender, EventArgs e )
        {
            if ( IsUserAuthenticated() == false )
            {
                Stage1Items.Visible = false;
                loginMessage.Visible = true;

                SetConsoleErrorMessage( "Error: you are not signed in (your session may have timed out). Please sign in and try again." );
                Response.Redirect( "/Account/Login.aspx?nextUrl=/My/Author.aspx", true );
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
            previewID = previewLink.Text;
            if ( string.IsNullOrEmpty( txtTitle.Text ) )
                cleanTitle = "";
            else
                cleanTitle = ResourceBizService.FormatFriendlyTitle( this.txtTitle.Text );
            if ( Request.Form[ "__EVENTTARGET" ] != null && Request.Form[ "__EVENTTARGET" ] == "btnRemRef" )
            {
                DeleteReference( null, null );
            }
            if ( Request.Form[ "__EVENTTARGET" ] != null && Request.Form[ "__EVENTTARGET" ] == "btnRemAtt" )
            {
                DeleteAttachment( null, null );
            }
            if ( Request.Form[ "__EVENTTARGET" ] != null && Request.Form[ "__EVENTTARGET" ] == "btnRemNodeAtt" )
            {
                DeleteNodeAttachment( null, null );
            }
            
        }//
        protected void InitializeForm()
        {

            //check if authorized, if not found, will check by orgId
            this.FormPrivileges = GDAL.SecurityManager.GetGroupObjectPrivileges( CurrentUser, FormSecurityName );

            noNodePanel.Visible = true;
            nodePanel.Visible = false;
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
            if ( WebUser.UserName == "mparsons" )
            {
                nodeKeyPanel.Visible = true;
            }
            //= Add attribute to btnDelete to allow client side confirm
            btnDelete.Attributes.Add( "onClick", "return confirmDelete(this);" );
            btnUnPublish.Attributes.Add( "onClick", "return confirmUnPublish(this);" );
            this.btnSetInactive.Attributes.Add( "onClick", "return confirmSetInactive(this);" );
            this.btnPublish.Attributes.Add( "onClick", "return confirmPublish(this);" );

            PopulateControls();
            CheckRecordRequest();
  
        }
        protected void Page_PreRender( object sender, EventArgs e )
        {
            LoggingHelper.DoTrace( 6, thisClassName + "Page_PreRender" );
            //for tracing
            try
            {
                
            }
            catch
            {
                //no action
            }

        }//
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

            Stage2Items.Visible = false;
            
            tabNav.Visible = false;
            statusDiv.Visible = false;
            //do any defaults. 
            entity.IsActive = true;
            entity.Status = "Draft";
            entity.HasChanged = false;
           // templatesPanel.Visible = true;
            //ddlIsOrganizationContent.Enabled = true;
			ddlOrganization.Enabled = true;
            btnPublish.Visible = false;

            noNodePanel.Visible = true;
            nodePanel.Visible = false;

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
            LoggingHelper.DoTrace( 6, thisClassName + ".PopulateForm(): " + entity.Id );
            //TODO - determine if user has access to this record
            //
            bool isReadOnly = false;
            if ( entity.Id > 0 )
            {
                if ( FormPrivileges.WritePrivilege > ( int ) ILPathways.Business.EPrivilegeDepth.Region
                    || entity.CreatedById == WebUser.Id 
					|| WebUser.TopAuthorization < 4)
                {
                    //OK
                }
                else 
                {
                    ContentPartner partner = ContentServices.ContentPartner_Get( entity.Id, WebUser.Id);
                    if (partner != null && partner.PartnerTypeId > 0) 
                    {
                        //can access, now is readonly or editable?
                        if ( partner.PartnerTypeId < 2 )
                        {
                            isReadOnly = true;
                            //this will have to bubble down to nodes
                        }
                    }
                    else
                    {
                        //not author, set readonly, or reject?
                        containerPanel.Visible = false;
                        SetConsoleErrorMessage( "Only the original author or designated collaborators may make updates to this content!" );
                    }
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

			this.SetListSelection( this.ddlOrganization, entity.OrgId.ToString() );

            //txtConditionsOfUse.Text = entity.UseRightsUrl;
            conditionsSelector.conditionsURL = entity.UseRightsUrl;

            //this.rblIsActive.SelectedValue = this.ConvertBoolToYesNo( entity.IsActive );

            if ( entity.Id > 0 )
            {
                //not sure if will allow template selection for existing record
                templatesPanel.Visible = false;
                ddlContentType.Visible = false;
                //Page Title
                Page.Title = entity.Title + " | ISLE OER";
                lblContentType.Text = entity.ContentType;
                lblContentType.Visible = true;
                

                //if orgId > 0, may want to have an indication that org is the owner??
                //for now always disable. changing could be complicated
                ddlIsOrganizationContent.Enabled = false;
				//not sure we want to allowing changing?
				if ( canUserChgOrg .Text == "no")
					ddlOrganization.Enabled = false;
				//IsOrgContentOwner should now be based on presence of orgId
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
					ddlOrganization.SelectedIndex = 0;
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
                btnNew.Visible = true;
                //set after content item exists
                FileResourceController.PathParts parts = FileResourceController.DetermineDocumentPathUsingParentItem( entity );
                string documentFolder = parts.filePath;
                string relativePath = parts.url;

                //string documentFolder = FileResourceController.DetermineDocumentPath( entity );
                //string relativePath = FileResourceController.GetRelativeUrlPath( entity );
                string baseUrl = UtilityManager.GetAppKeyValue( "path.MapContentPath", "/ContentDocs/" );

                string upload = baseUrl + relativePath + "/";

                //myImmediateImageInsert.UploadFolder = upload;  // "/ContentDocs/"; //documentFolder;

                //?? ==> n
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

                historyPanel.Visible = true;
                this.lblHistory.Text = entity.HistoryTitle();
                HandleTabsVisibility();

                //============== handle different types
                if ( entity.TypeId == ContentItem.DOCUMENT_CONTENT_ID )
                {
                    PopulateDocumentContentType( entity );
                }
                else if ( entity.IsHierarchyType )
                {
                    PopulateCurriculumContentType( entity );
                }
                else
                {
                    Stage2Items.Visible = true;
                    HandleOtherContentType( entity );
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

				ddlIsOrganizationContent.Enabled = true;
				ddlOrganization.Enabled = true;
                btnDelete.Visible = false;
                btnNew.Visible = false;
                this.btnUnPublish.Visible = false;
                this.btnSetInactive.Visible = false;

                ltlAttachmentsList.Text = "";
                this.ltlReferencesList.Text = "";
                ddlContentType.Enabled = true;

                FileContentItem.Visible = false;
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
                string fileUrl = FileResourceController.ValidateDocumentOnServer( entity, entity.RelatedDocument );
                if ( fileUrl.Length > 10 )
                {

                    contentFileName.Text = entity.RelatedDocument.FileName;
                    contentFileDescription.Text = entity.Summary;
                    linkContentFile.NavigateUrl = fileUrl;
                    
                }
                else
                    contentFileName.Text = "Sorry issue encountered locating the related document.<br/>" + entity.Message;
            }

        }//
        void PopulateCurriculumContentType( ContentItem entity )
        {
            try
            {
                entity = crcManager.GetCurriculumOutlineForEditOld( entity.Id );

                ContentNode nodes = crcManager.GetCurriculumOutlineForEdit( entity.Id );
                //hmmm, if an actual hierarch type, only want to offer outline create once
                //so if has children:
                //  - not curriculm - use tree route ==> suggests cannot create a new hierarchy under curr?
                if ( entity.HasChildItems 
					|| entity.TypeId != ContentItem.CURRICULUM_CONTENT_ID )
                {
                    PopulateTree( entity );

                    if ( entity.TypeId != ContentItem.CURRICULUM_CONTENT_ID )
                        this.PopulateNode( entity.Id );
                    else
                    {
                        newsPanel.Visible = true;
                    }
                }
                else
                {
                    curriculumIntroPanel.Visible = true;
                    treePanel.Visible = false;
                    nodePanel.Visible = false;
                    noNodePanel.Visible = false;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".PopulateCurriculumContentType() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }
        }//
        void HandleOtherContentType( ContentItem entity )
        {

            //get all supplements
            SetAttachmentList( entity.Id );
            //get all references
            SetReferenceList( entity.Id );
         

            //ib1.GalleryFolders = documentFolder;
            //ib1.ManagedFolders = documentFolder;
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
   protected void btnSaveWeb_Click( object sender, EventArgs e )
        {
            btnSave_Click( sender, e );
            //selectedTab = ltlAttachmentsTabName.Text;
        }
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

                //enable other sections
                HandleTabsVisibility();
               
                statusDiv.Visible = true;
                

                //show the finish button
                //btnFinish.Visible = true;
                //btnFinish2.Visible = true;
            }

            //hide cancel
      }


        protected void HandleTabsVisibility()
        {
            //take the user to the next step
            selectedTab = ltlWebContentTabName.Text;

            if ( CurrentRecord.TypeId == ContentItem.DOCUMENT_CONTENT_ID )
            {
                FileContentItem.Visible = true;
                tabNavContentFile.Visible = true;
                tabNav.Visible = false;
                //==> overide
                selectedTab = ltlFileContentTabName.Text;
            }
            else if ( CurrentRecord.IsHierarchyType )
            {
                Stage2Items.Visible = true;
                tabNav.Visible = false;
                attachmentsPanel.Visible = false;
                referencesPanel.Visible = false;
                tabNavCurriculum.Visible = true;
                CurriculumItem.Visible = true;
                selectedTab = ltlCurriculumTabName.Text;
            }
            else
            {
                Stage2Items.Visible = true;
                tabNav.Visible = true;
                referencesPanel.Visible = true;
                this.attachmentsPanel.Visible = true;
            }
        }

     
        protected void btnSaveWebContinue_Click( object sender, EventArgs e )
        {
            btnSave_Click( sender, e );
            if ( CurrentRecord.TypeId == ContentItem.DOCUMENT_CONTENT_ID )
                selectedTab = ltlAttachmentsTabName.Text;
            else if ( CurrentRecord.TypeId >= ContentItem.CURRICULUM_CONTENT_ID
                && CurrentRecord.TypeId < ContentItem.ACTIVITY_CONTENT_ID )
                selectedTab = ltlCurriculumTabName.Text;
            else 
                selectedTab = ltlAttachmentsTabName.Text;
        }
		/// <summary>
		/// note should not be used
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Obsolete]
		protected void btnPublish2_Click(object sender, EventArgs e)
		{

			/*cases
			 * - personal, just update status, maybe resource IsActive
			 * 
			 * 
			 * 
			 */
			if (ValidateSummary() & this.IsFormValid())
			{
				if (CurrentRecord.IsOrgContentOwner == false)
				{
					//only showing if previously published==> maybe
					this.UpdateForm(ContentItem.PUBLISHED_STATUS);
					btnPublish.Visible = false;
				}
				else
				{
					if (CurrentRecord.StatusId == ContentItem.REVISIONS_REQUIRED_STATUS)
					{
						//update just in case
						this.UpdateForm();

						string statusMessage = string.Empty;
						AppUser user = GetAppUser();
						if (myManager.RequestApproval(CurrentRecord.Id, user, ref statusMessage) == true)
						{
							SetConsoleSuccessMessage("An email was sent requesting a review of your resource.");
							//refresh
							Get(CurrentRecord.Id);
							btnPublish.Visible = false;
						}
						else
							SetConsoleErrorMessage("An error occurred attempting to initiate a review of your resource.<br/>System administration has been notified.<br/>" + statusMessage);

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
                Session.Add( "authoredResourceURL", UtilityManager.FormatAbsoluteUrl( string.Format( litPreviewUrlTemplate.Text, CurrentRecord.Id, ResourceBizService.FormatFriendlyTitle( CurrentRecord.Title ) ), false ) );

            Response.Redirect( string.Format(litContentPublishUrl.Text, CurrentRecord.RowId.ToString()), true );
            //redirectTarget = Request.Url.Scheme + "://" + Request.Url.Host + ":" + Request.Url.Port + "/Publish.aspx";

        } // end 

        private void SubmitPublishApproveRequest()
        {
            //update just in case
            this.UpdateForm();

            string statusMessage = string.Empty;
            AppUser user = GetAppUser();
			if (myManager.RequestApproval(CurrentRecord.Id, user, ref statusMessage) == true)
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

                    if ( ddlTemplates.SelectedIndex > 0 )
                    {
                        int tempid = Int32.Parse( ddlTemplates.SelectedValue );
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

					//entity.OrgId = user.OrgId;

					//if not allowing this to chg, should move to the create specific section?
					//15-10-13 noved it
					if ( ddlOrganization.SelectedIndex == 0 )
					{
						entity.IsOrgContentOwner = false;
						entity.OrgId = 0;
					}
					else
					{
						entity.IsOrgContentOwner = true;
						entity.OrgId = int.Parse( this.ddlOrganization.SelectedValue.ToString() );
					}

					//if ( ddlIsOrganizationContent.SelectedIndex == 0 )
					//	entity.IsOrgContentOwner = false;
					//else
					//	entity.IsOrgContentOwner = true;
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
                entity.Description = this.editor.EditPanel.Content;

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
					//create and add author as admin content partner
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
                    LoggingHelper.DoTrace( 6, thisClassName + "UpdateForm - after update" );
                }


                if ( statusMessage.Equals( "successful" ) )
                {

                    CurrentRecord = entity;
                    //maybe, or ?????

                    if ( currentStatusId != entity.StatusId || newStatusId > 0 )
                    {
                        LoggingHelper.DoTrace( 6, thisClassName + "UpdateForm - doing get" );
                        Get( entity.Id );
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
            else if ( entity.StatusId > ContentItem.INPROGRESS_STATUS && entity.StatusId < ContentItem.PUBLISHED_STATUS )
                statusId = entity.StatusId;
            else if ( entity.StatusId == ContentItem.INACTIVE_STATUS )
                statusId = ContentItem.INPROGRESS_STATUS;       //TODO - do we want to arbitrarily set inactive to active, or require overt action by user?
            else
            {
                //is published. If approval is required, set to in progress, and what else
                //may need something to allow auto approve after initial approve, or at some point in time

                if (entity.TypeId == ContentItem.DOCUMENT_CONTENT_ID || entity.IsHierarchyType)
                {
                    //leave as is
                }
                else
                {
                    if (entity.IsOrgContent())
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
                    if (CurrentRecord.HasResourceId())
                    {
                        new ResourceBizService().Resource_SetInactive( CurrentRecord.ResourceIntId, ref statusMessage );
                        //ResourceVersion entity = new ResourceVersionManager().GetByResourceId( CurrentRecord.ResourceIntId );
                        //if ( entity != null && entity.Id > 0 )
                        //{
                        //    try
                        //    {
                        //        new ResourceVersionManager().SetActiveState( false, entity.Id );
                        //        string response = "";
                        //        var esManager = new ElasticSearchManager();
                        //        //new ElasticSearchManager().DeleteByVersionID( CurrentRecord.ResourceVersionId, ref response );
                        //        new ElasticSearchManager().DeleteResource( CurrentRecord.ResourceIntId );
                        //        extraMessage = "Resource Deactivated";
                        //    }
                        //    catch ( Exception ex )
                        //    {
                        //        extraMessage = "There was a problem deactivating the Resource: " + ex.ToString();
                        //    }


                        //    //note can have a RV id not not be published to LR. Need to check for a resource docid
                        //    if ( entity.LRDocId != null && entity.LRDocId.Length > 10 )
                        //    {
                        //        //post request to delete ==> this process would take care of actual delete of the Resource hierarchy
                        //        if (IsTestEnv()) 
                        //            extraMessage = "<br/>( WELL ALMOST - NEED TO ADD CODE TO REMOVE FROM THE LR, AND DO PHYSICAL DELETE OF THE RESOURCE HIERARCHY - JEROME)";
                        //    }

                        //}
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
            string statusMessage = "";

            if ( id > 0 )
            {
                //validate just in case
                if ( ValidateSummary() & this.IsFormValid() )
                {
                    //save rvid for later
                    int resid = CurrentRecord.ResourceIntId;

                    this.UpdateForm( ContentItem.INACTIVE_STATUS );
                    //probably need a check that update was successful

                    //TODO - should a history record be created?

                    string extraMessage = "";
                    if ( resid > 0 )
                    {
                        new ResourceBizService().Resource_SetInactive( resid, ref statusMessage );

                        //ResourceVersion entity = new ResourceVersionManager().GetByResourceId( resid );
                        //if ( entity != null && entity.Id > 0 )
                        //{
                        //    string status = new ResourceManager().SetResourceActiveState( entity.ResourceIntId, false );
                        //    //note can have a RV id not not be published to LR. Need to check for a resource docid
                        //    if ( entity.LRDocId != null && entity.LRDocId.Length > 10 )
                        //    {
                        //        //need to call methods to remove from LR
                        //        //==> this process would take care of actual delete of the Resource hierarchy
                        //        if ( IsTestEnv() )
                        //            extraMessage = "<br/>( WELL ALMOST - NEED TO ADD CODE TO REMOVE FROM THE LR, AND DO PHYSICAL DELETE OF THE RESOURCE HIERARCHY - JEROME)";
                        //    }

                        //}
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
            if ( CurrentRecord.HasValidRowId() )
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
                    int id = Int32.Parse( ddlAttachment.SelectedValue.ToString() );

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
                lblDocumentLink.NavigateUrl = FileResourceController.ValidateDocumentOnServer( CurrentRecord, doc );
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

            // Scan the file first.  If there's a virus, don't allow the upload!
            string scanResult = "OK";
            VirusScanner virScan = new VirusScanner();
            scanResult = virScan.Scan( fileUpload.FileBytes );

            if (scanResult.IndexOf("ERROR") > -1)
            {
                errorBuilder.Append("An error occurred while scanning the uploaded file for viruses.");
                isValid = false;
            }
            else if (scanResult == "Infected")
            {
                errorBuilder.Append("The file you are uploading appears to be infected with a virus! Please remove the infection before attempting to upload again.");
                isValid = false;
            }
            else if (scanResult == "no scan done")
            {
                SetConsoleInfoMessage("The uploaded file was not scanned for viruses.");
            }

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
            if ( id == 0 && !IsFilePresent() )
            {
                errorBuilder.Append( "A file must be selected for an attachment.<br/>" );
            }
            if ( ddlAttachmentPrivacyLevel.SelectedIndex < 1 )
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
                    entity = myManager.ContentSupplementGet( id );
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
                    this.SetAttachmentList( CurrentRecord.Id );

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

        /// <summary>
        /// Handle document content
        /// </summary>
        /// <param name="parentEntity"></param>
        /// <param name="attachment"></param>
        /// <param name="user"></param>
        /// <param name="uploadOnly"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
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
                || documentRowId == ContentItem.DEFAULT_GUID
                || uploadOnly == true )
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
                        SetConsoleErrorMessage( string.Format( "Error the selected file exceeds the size limits ({0} bytes).", maxFileSize ) );
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
                    //string documentFolder = FileResourceController.DetermineDocumentPath( parentEntity );
                    FileResourceController.PathParts parts = FileResourceController.DetermineDocumentPathUsingParentItem( parentEntity );
                    //entity.FilePath = parts.filePath;
					entity.FilePath = FileResourceController.FormatPartsFilePath( parts );
					string url = FileResourceController.FormatPartsRelativeUrl( parts );

                    //entity.URL = parts.url;
					entity.ResourceUrl = url + "/" + entity.FileName;

                    // LoggingHelper.DoTrace( 5, thisClassName + " - uploading to " + documentFolder );
                    //entity.URL = FileResourceController.DetermineDocumentUrl( parentEntity, entity.FileName );
					attachment.ResourceUrl = entity.ResourceUrl;

                    //try separating following to insulate from file privileges issues
					FileResourceController.UploadFile( fileUpload, entity.FilePath, entity.FileName );
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

                FileResourceController.ValidateDocumentOnServer( parentEntity, entity );
            }

            return isValid;

        }//

		//private void UploadFile( FileUpload uploadCtrl, string documentFolder, string filename )
		//{
		//	try
		//	{
		//		FileSystemHelper.CreateDirectory( documentFolder );

		//		string diskFile = documentFolder + "\\" + filename;
		//		//string diskFile = MapPath( documentFolder ) + "\\" + entity.FileName;

		//		LoggingHelper.DoTrace( 5, thisClassName + " UploadFile(). doing SaveAs" );
		//		uploadCtrl.SaveAs( diskFile );

		//	}
		//	catch ( Exception ex )
		//	{
		//		LoggingHelper.LogError( ex, thisClassName + string.Format( "UploadFile(documentFolder: {0}, filename: {1})", documentFolder, filename ) );
		//	}
		//}//

        //private string ValidateDocumentOnServer( ContentItem parentEntity, DocumentVersion doc )
        //{
        //    string fileUrl = "";

        //    try
        //    {
        //        string documentFolder = FileResourceController.DetermineDocumentPath( parentEntity );
        //        string message = FileSystemHelper.HandleDocumentCaching( documentFolder, doc );
        //        if ( message == "" )
        //        {
        //            //blank returned message means ok
        //            fileUrl = FileResourceController.DetermineDocumentUrl( parentEntity, doc.FileName );
        //        }
        //        else
        //        {
        //            //error, should return a message
        //            this.SetConsoleErrorMessage( message );
        //        }
        //    }
        //    catch ( Exception ex )
        //    {
        //        LoggingHelper.LogError( ex, thisClassName + ".ValidateDocumentOnServer() - Unexpected error encountered while retrieving document" );

        //        this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
        //    }
        //    return fileUrl;
        //}//

        private bool IsUploadValid()
        {
            //int recId = 0;
            bool isValid = true;

            try
            {

                //for new document, current record is required
                if ( this.txtRowId.Text.Length == 0 || txtRowId.Text == "0" )
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
            //bool isPresent = false;

            //if ( fileUpload.HasFile || fileUpload.FileName != "" )
            //{
            //    isPresent = true;

            //}

            return IsFilePresent( fileUpload );
        }//

        protected bool IsFilePresent( FileUpload fileControl)
        {
            bool isPresent = false;

            if ( fileControl.HasFile || fileControl.FileName != "" )
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
                    entity = myManager.ContentReferenceGet( id );
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

        #region === content type of document ========================

        protected void btnContentFileUpload_Click( object sender, EventArgs e )
        {
            selectedTab = this.ltlFileContentTabName.Text;
            string statusMessage = "";
            //validate
            if ( contentFileUpload.HasFile == false || contentFileUpload.FileName == "" )
            {
                SetConsoleErrorMessage( "Please select a file before clicking upload" );

                return;
            }
            //do upload
            //????
            CurrentUser = GetAppUser();
            
            DocumentVersion docVersion = new DocumentVersion();
            string documentRowId = this.txtDocumentRowId2.Text;
            if ( documentRowId.IndexOf("00000000-") > -1
                || documentRowId.Length != 36
                || docVersion.IsValidRowId( new Guid( documentRowId ) ) == false )
            {
                //not normal but may be necessary for workaround
                docVersion.CreatedById = WebUser.Id;
                docVersion.Created = System.DateTime.Now;

                FileResourceController.CreateDocument( contentFileUpload, docVersion, CurrentRecord.OrgId, ref statusMessage );

                CurrentRecord.DocumentUrl = docVersion.URL;
                CurrentRecord.DocumentRowId = docVersion.RowId;
                CurrentRecord.LastUpdated = System.DateTime.Now;
                CurrentRecord.LastUpdatedById = WebUser.Id;		//include for future use
                CurrentRecord.LastUpdatedBy = WebUser.FullName();
                //change status???
                int currentStatusId = CurrentRecord.StatusId;
                CurrentRecord.StatusId = DetermineStatus( CurrentRecord );
                statusMessage = myManager.Update( CurrentRecord );
                if ( statusMessage.Equals( "successful" ) )
                {
                    this.SetConsoleSuccessMessage( "Successfully added file!" );
                    contentFileName.Text = CurrentRecord.RelatedDocument.FileName;
                    contentFileDescription.Text = CurrentRecord.Summary;
                    linkContentFile.NavigateUrl = CurrentRecord.DocumentUrl;
                }
                else
                {
                    this.SetConsoleErrorMessage( "Error encountered attempting to update this item: " + statusMessage );
                }

                return;
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
                    return ;
                }

                docVersion.MimeType = contentFileUpload.PostedFile.ContentType;
                //keeping the same file name
                //docVersion.FileName = contentFileUpload.FileName;
                docVersion.FileDate = System.DateTime.Now;

                string sFileType = System.IO.Path.GetExtension( docVersion.FileName );
                sFileType = sFileType.ToLower();

                docVersion.FileName = System.IO.Path.ChangeExtension( docVersion.FileName, sFileType );

                //probably want to fix filename to standardize
                //should already be clean, so skip to avoid unexpected name changes
                //docVersion.CleanFileName();

                //LoggingHelper.DoTrace( 5, thisClassName + " HandleDocument(). calling DetermineDocumentPath" );
                FileResourceController.PathParts parts = FileResourceController.DetermineDocumentPathUsingParentItem( CurrentRecord );

                docVersion.URL = FileResourceController.FormatPartsFullUrl( parts, docVersion.FileName );
                //docVersion.URL = FileResourceController.DetermineDocumentUrl( CurrentRecord, docVersion.FileName );

                //don't update url, just incase
                CurrentRecord.DocumentUrl = docVersion.URL;

                //try separating following to insulate from file privileges issues
				FileResourceController.UploadFile( contentFileUpload, parts.filePath, docVersion.FileName );
                //rewind for db save
                contentFileUpload.PostedFile.InputStream.Position = 0;
                Stream fs = contentFileUpload.PostedFile.InputStream;

                docVersion.ResourceBytes = fs.Length;
                byte[] data = new byte[ fs.Length ];
                fs.Read( data, 0, data.Length );
                fs.Close();
                fs.Dispose();
                docVersion.SetResourceData( docVersion.ResourceBytes, data );

                statusMessage = myManager.DocumentVersionUpdate( docVersion );
                if ( statusMessage.Equals( "successful" ) )
                {

                    linkContentFile.Visible = true;

                    //update CI
                    ContentItem entity = myManager.Get( CurrentRecord.Id );
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
			//SetNodeContentType();

			LoadOrganizationDDL();
            //check if org admin, and whether already has an org home page

            SetPrivilegeLists();
            //SetConditionsOfUse();
            SetTemplates();
        } //

		private void SetNodeContentType()
        {
            //may only allow on first create?
            List<CodeItem> list = myManager.ContentType_ActiveList();
            //
            BDM.PopulateList( this.ddlNodeContentType, list, "Id", "Title", "" );

        }
        private void SetOrgContent()
        {
            //may only allow on first create?
            List<CodeItem> list = myManager.ContentType_ActiveList();
            //
            BDM.PopulateList( this.ddlNodeContentType, list, "Id", "Title", "" );

        }
		private void LoadOrganizationDDL()
		{
			
			ddlOrganization.Items.Add( new ListItem()
			{
				Value = "0",
				Text = "No organization"
			} );
			if ( WebUser.OrgMemberships == null )
			{
				addToOrg.Visible = false;
			}
			else
			{
				foreach ( var item in WebUser.OrgMemberships )
				{
					ddlOrganization.Items.Add( new ListItem()
					{
						Value = item.Id.ToString(),
						Text = item.Organization
					} );
				}
			}
		
		}

        private void SetContentType2( bool hasHomeContent )
        {

            List<CodeItem> list2 = myManager.ContentType_TopLevelActiveList();
            BDM.PopulateList( this.ddlContentType, list2, "Id", "Title", "Select type" );

            if (list2.Count == 2)
            {
                ddlContentType.SelectedIndex = 1;
            }
            //may only allow on first create?
            //DataSet ds = myManager.ContentType_Select();
            //if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            //{
            //    //templatesPanel.Visible = true;
            //    ListItem item = new ListItem( "Select type", "0" );
            //    ddlContentType.Items.Add( item );

            //    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
            //    {
            //        string id = BDM.GetRowColumn( dr, "Id", "0" );
            //        string title = BDM.GetRowColumn( dr, "Title", "-" );

            //        item = new ListItem( title, id );
            //        if ( id == "20" && hasHomeContent )
            //            item.Enabled = false;
            //        else
            //            item.Enabled = true;

            //        ddlContentType.Items.Add( item );
            //    }
            //    ddlContentType.SelectedIndex = 0;
            //PopulateList2( this.ddlContentType, ds, "Id", "Title", "Select type" );
            //
        }
        

        private void SetTemplates()
        {
            //may only allow on first create?
            DataSet ds = myManager.SelectOrgTemplates( CurrentUser.OrgId );
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                //templatesPanel.Visible = true;
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
            BDM.PopulateList( this.ddlNodePrivacyLevel, ds, "Id", "Title");
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
                    string fileUrl = FileResourceController.ValidateDocumentOnServer( CurrentRecord, item.RelatedDocument );
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
                        String.Format( ltlAppliedAttachmentTemplate.Text, arguments );
                    //String.Format( ltlAppliedAttachmentTemplate.Text, DatabaseManager.GetRowColumn( dr, "Title" ), DatabaseManager.GetRowColumn( dr, "Description" ), DatabaseManager.GetRowColumn( dr, "PrivilegeType" ) );
                    
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
            string[] targetColumnNames = ltlReferenceColumns.Text.Split( ',' );
            string[] targetColumnDisplays = ltlReferenceColumnsDisplay.Text.Split( ',' );

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

    
        #region ============== curriculum ==================================
		protected void btnCreateHierarchy_Click( object sender, EventArgs e )
        {
            int modules = 10;
            int units = 3;
            int lessons = 3;
            int activities = 0;

            Int32.TryParse( ddlModules.SelectedValue, out modules );
            Int32.TryParse( ddlUnits.SelectedValue, out units );
            Int32.TryParse( ddlLessons.SelectedValue, out lessons );
            Int32.TryParse( ddlActivities.SelectedValue, out activities );

            ContentItem hier = myManager.CreateHierarchy( CurrentRecord.Id, modules, units, lessons, activities );
            if ( hier != null && hier.Id > 0 && hier.HasChildItems )
            {
                CurrentRecord.ChildItems = hier.ChildItems;
                curriculumIntroPanel.Visible = false;
                treePanel.Visible = true;
                noNodePanel.Visible = true;
                selectedTab = ltlCurriculumTabName.Text;

                PopulateTree( CurrentRecord.Id );
            }
            else
            {
                //error?
            }
  }
        private void PopulateTree( int contentId )
        {
            ContentItem entity = crcManager.GetCurriculumOutlineForEditOld( contentId );
            this.PopulateTree( entity );

            //ContentNode nodes = crcManager.GetCurriculumOutlineForEdit( contentId );
            //this.PopulateTree( nodes );
       
        }
        private void PopulateTree( ContentNode entity )
        {
            this.OBTreeview.Nodes.Clear();
            //get top level
            
            bool showAll = true;

            foreach ( ContentNode child in entity.ChildNodes )
            {
                Node node = new Node();

                if ( child.IsPublished )
                    node.Text = child.Title;
                else
                    node.Text = child.Title + " (not published )";

                node.Value = child.Id.ToString();
                this.OBTreeview.Nodes.Add( node );

                if ( showAll )
                {
                    this.PopulateChildren( node, child );
                }
            }
        }
        private void PopulateChildren( Node parent, ContentNode entity )
        {
            foreach ( ContentNode child in entity.ChildNodes )
            {
                Node node = new Node();

                if ( child.IsPublished )
                    node.Text = child.Title;
                else
                    node.Text = child.Title + " (not published )";

                node.Value = child.Id.ToString();

                parent.ChildNodes.Add( node );
                if ( child.ChildNodes != null && child.ChildNodes.Count > 0 )
                    this.PopulateChildren( node, child );

            }


        }
        private void PopulateTree( ContentItem entity )
        {
            this.OBTreeview.Nodes.Clear();
            //get top level

            bool showAll = true;

            foreach ( ContentItem child in entity.ChildItems )
            {
                Node node = new Node();

                if ( child.StatusId == 5 )
                    node.Text = child.Title;
                else
                    node.Text = child.Title + " (" + child.Status + ")";

                node.Value = child.Id.ToString();
                this.OBTreeview.Nodes.Add( node );

                if ( showAll )
                {
                    this.PopulateChildren( node, child );
                }
            }
        }
        private void PopulateChildren( Node parent, ContentItem entity )
        {
            foreach ( ContentItem child in entity.ChildItems )
            {
                Node node = new Node();

                if ( child.StatusId == 5 )
                    node.Text = child.Title;
                else
                    node.Text = child.Title + " (" + child.Status + ")";

                node.Value = child.Id.ToString();

                parent.ChildNodes.Add( node );
                if ( child.ChildItems != null && child.ChildItems.Count > 0 )
                    this.PopulateChildren( node, child );

            }


        }

        /// <summary>
        /// handle  click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OBTreeview_SelectedTreeNodeChanged( object sender, NodeEventArgs e )
        {
            nodeId.Text = e.Node.Value;
            int id = 0;
            selectedTab = ltlCurriculumTabName.Text;

            if ( Int32.TryParse( e.Node.Value, out id ) )
            {
                PopulateNode( id );
            }
        }

        private void PopulateNode( int id )
        {
            lblFileList.Text = "";
            noNodePanel.Visible = false;
            nodePanel.Visible = true;

            ContentItem entity = new CurriculumServices().GetCurriculumNodeForEdit( id, CurrentUser );
            //key info
            nodeId.Text = entity.Id.ToString();
            this.parentNodeId.Text = entity.ParentId.ToString();
            this.nodeSortOrder.Text = entity.SortOrder.ToString();
            contentTypeId.Text = entity.TypeId.ToString();
            lblNodeStatus.Text = entity.Status;
            lblResourceId.Text = entity.ResourceIntId.ToString();

            txtNodeTitle.Text = entity.Title;
            txtNodeDescription.Text = entity.Description;
            lblNodeContentType.Text = entity.ContentType;
            txtTimeframe.Text = entity.Timeframe;

            this.SetListSelection( this.ddlNodePrivacyLevel, entity.PrivilegeTypeId.ToString() );

            nodeNavSection.Text = string.Format( litNodeViewTemplate.Text, entity.RowId.ToString(), entity.Id, ResourceBizService.FormatFriendlyTitle( entity.Title ), entity.ContentType );
            if ( entity.HasResourceId() == false )
            {
                nodeNavSection.Text += "&nbsp;" +  string.Format( litNodePublishTemplate.Text, entity.Id, entity.ContentType );
            }
            else
            {
                nodeNavSection.Text += string.Format( litNodeViewTags.Text, entity.ResourceIntId, ResourceBizService.FormatFriendlyTitle( entity.Title ), entity.ContentType );
            }
            nodeNavSection.Visible = true;

            litFrame.Text = string.Format( "<iframe id=\"contributeFrame\" runat=\"server\" src=\"/Contribute/?mode=upload&nodeId={0}&hh=temp&doingLRPublish=no\" class='contributeIframe'  ></iframe>", id.ToString() );

            //set buttons
            btnUpdateNode.Visible = true;
            nodeButtons.Visible = true;
            if ( entity.TypeId < ContentItem.ACTIVITY_CONTENT_ID )
            {
                string childNode = "child node";
                if ( entity.TypeId == ContentItem.MODULE_CONTENT_ID )
                    childNode = "Unit";
                else if ( entity.TypeId == ContentItem.UNIT_CONTENT_ID )
                    childNode = "Lesson";
                else if ( entity.TypeId == ContentItem.LESSON_CONTENT_ID )
                    childNode = "Activity";
                //how to add assessments?

                this.btnAddChildNode.Text = string.Format( "Add {0} below this {1}", childNode, entity.ContentType );
                btnAddChildNode.Visible = true;
            }
            else
                btnAddChildNode.Visible = false;

            this.btnUpdateNode.Text = string.Format( "Save {0}", entity.ContentType );
            this.btnDeleteNode.Text = string.Format( "Delete {0}", entity.ContentType );

            this.btnInsertNode.Text = string.Format("Insert {0} before this {0}", entity.ContentType);
            this.btnAppendNode.Text = string.Format( "Add {0} after this {0}", entity.ContentType );

            if (btnDeleteNode.Enabled == true)
                btnDeleteNode.Visible = true;
            txtAutoDocumentRowId.Text = "";
            autodocPanel.Visible = false;
            if ( entity.IsValidRowId( entity.DocumentRowId ) )
            {
                txtAutoDocumentRowId.Text = entity.DocumentRowId.ToString();
                if ( entity.RelatedDocument == null )
                    entity.RelatedDocument = myManager.DocumentVersionGet( entity.DocumentRowId );

                if ( entity.RelatedDocument != null )
                {
                    autodocPanel.Visible = true;
                    autodocFileName.Text = entity.RelatedDocument.FileName;
                    autodocLink.NavigateUrl = FileResourceController.ValidateDocumentOnServer( entity, entity.RelatedDocument );
                }
            }
            if ( entity.HasChildItems )
            {
                foreach ( ContentItem item in entity.ChildItems )
                {
                    if ( item.TypeId == ContentItem.DOCUMENT_CONTENT_ID )
                        PopulateDocument( item );
                }
            }

            lblStandardsList.Text = "";

            if ( entity.HasChildStandards )
            {
                foreach ( Content_StandardSummary item in entity.ContentStandards )
                {
                    PopulateStandard( item );
                }
            }
        }
        private string GetNodeLabel( int typeId )
        {
            string title = "Child Node";
            if ( typeId == ContentItem.MODULE_CONTENT_ID )
                title = "Module";
            else if ( typeId == ContentItem.UNIT_CONTENT_ID )
                title = "Unit";
            else if ( typeId == ContentItem.LESSON_CONTENT_ID )
                title = "Lesson";
            else if ( typeId == ContentItem.ACTIVITY_CONTENT_ID )
                title = "Activity";
            else if ( typeId == ContentItem.ASSESSMENT_CONTENT_ID )
                title = "Assessment";

            return title;
        }
        private void PopulateDocument( ContentItem entity )
        {
            string docUrl = "";
            if ( entity.DocumentUrl != null && entity.IsValidRowId( entity.DocumentRowId ))
            {
                //if ( IsTestEnv() )
                //    supplements.Text += "<br/>(test only): " + item.RelatedDocument.FileName;

                //check if file exists on server, if not it will be downloaded
                string fileUrl = FileResourceController.ValidateDocumentOnServer( entity, entity.RelatedDocument );
                if ( fileUrl.Length > 10 )
                {

                    if ( IsUserAuthenticated() == false && entity.PrivilegeTypeId != 1 )
                    {
                        //lblFileList.Text += string.Format( "<h2>{0}</h2>Private document, viewing is prohibited.", entity.RelatedDocument.Title );
                        docUrl = "#"; ;
                    }
                    else
                    {
                        //lblFileList.Text += "<p>" + string.Format( docLinkTemplate.Text, fileUrl, entity.RelatedDocument.Title ) + "</p>";
                        docUrl = fileUrl;
                    }
                }
                else
                {
                    lblFileList.Text += "<br/>Sorry issue encountered locating the related document.";
                    docUrl = "#"; ;
                }

                string[] arguments = new string[] {
                    entity.Title,
                    entity.PrivilegeType,
                    entity.RowId.ToString(),
                    entity.Id.ToString(),
                    docUrl,
                    entity.ResourceFriendlyUrl
                };
                lblFileList.Text = lblFileList.Text + String.Format( ltlNodeAttachmentTemplate.Text, arguments );
            }
        }//

        private void PopulateStandard( Content_StandardSummary entity )
        {

            string[] arguments = new string[] {
                    entity.NotationCode,
                    entity.Description,
                    entity.AlignmentType,
                    entity.StandardUsage
                    ,entity.Id.ToString()
                };
            lblStandardsList.Text = lblStandardsList.Text + String.Format( standardsTemplate.Text, arguments );
        }//

        /// <summary>
        /// insert a node before current:
        /// - retain the parentnodeId
        /// - get sort order and subtract 2
        /// - retain content type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnInsertNode_Click( object sender, EventArgs e )
        {

            ResetNode();
            SetSortOrder( -2 );
          //  SetConsoleErrorMessage( "Inserting nodes is not implemented yet." );
        }
        /// <summary>
        /// Add node after current:
        /// - retain the parentnodeId
        /// - get sort order and add 2
        /// - retain content type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnAppendNode_Click( object sender, EventArgs e )
        {
            ResetNode();

            SetSortOrder( 3 );
           // SetConsoleErrorMessage( "Appending nodes is not implemented yet." );
        }

        protected void btnAddChildNode_Click( object sender, EventArgs e )
        {
            this.parentNodeId.Text = nodeId.Text;
            int typeId= Int32.Parse( this.contentTypeId.Text );
            typeId += 2;
            contentTypeId.Text = typeId.ToString();
            ResetNode();
            this.btnUpdateNode.Text = string.Format( "Save {0}", GetNodeLabel( typeId ) );

            nodeSortOrder.Text = "5";
           // SetConsoleErrorMessage( "Adding child nodes is not implemented yet." );
        }


        protected void btnDeleteNode_Click( object sender, EventArgs e )
        {
            selectedTab = ltlCurriculumTabName.Text;
            if (this.btnDeleteNode.Enabled == false)
                SetConsoleErrorMessage( "Deleting nodes is not implemented yet." );

            int id = 0;
            try
            {
                string statusMessage = "";
                string extraMessage = "";
                id = int.Parse( this.nodeId.Text );

                if ( myManager.Delete( id, ref statusMessage ) )
                {
                    //not sure about cascading deletes


                    if ( CurrentRecord.HasResourceId() == true)
                    {
                        ResourceVersion entity = new ResourceVersionManager().GetByResourceId( CurrentRecord.ResourceIntId );
                        if ( entity != null && entity.Id > 0 )
                        {
                            try
                            {
                                new ResourceVersionManager().SetActiveState( false, entity.Id);
                                string response = "";
                                var esManager = new ElasticSearchManager();
                                //new ElasticSearchManager().DeleteByVersionID( CurrentRecord.ResourceVersionId, ref response );
                                new ElasticSearchManager().DeleteResource( CurrentRecord.ResourceIntId );
                                extraMessage = "Resource Deactivated";

                                ActivityBizServices.SiteActivityAdd( "Resource", "Deactivate", string.Format( "Resource ID: {0} was deactivated by {1} from author tool.", CurrentRecord.ResourceIntId, WebUser.FullName() ), WebUser.Id, 0, CurrentRecord.ResourceIntId );
                            }
                            catch ( Exception ex )
                            {
                                extraMessage = "There was a problem deactivating the Resource: " + ex.ToString();
                            }


                            //string status = new ResourceManager().SetResourceActiveState( false, CurrentRecord.ResourceVersionId );
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
                    
                    this.ResetNode();
                    PopulateTree( CurrentRecord.Id );
                    noNodePanel.Visible = true;
                    nodePanel.Visible = false;
                }
                else
                {
                    this.SetConsoleErrorMessage( statusMessage );
                }

            }
            catch ( System.Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".btnDeleteNode_Click() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }
        }

        protected void ResetNode()
        {
            selectedTab = ltlCurriculumTabName.Text;

            nodeId.Text = "0";
            ContentItem entity = new ContentItem();
            txtNodeTitle.Text = entity.Title;
            txtNodeDescription.Text = entity.Description;
            //May leave content type for some guidance, or maybe pass current
            //lblNodeContentType.Text = entity.ContentType;
            litFrame.Text = "";
            lblFileList.Text = "";
            lblNodeStatus.Text = "";
            nodeNavSection.Visible = false;
            nodeNavSection.Text = "";
            txtTimeframe.Text = "";
            //set buttons
            btnDeleteNode.Visible = false;
            nodeButtons.Visible = false;
         }

        /// <summary>
        /// Update sort order
        /// </summary>
        /// <param name="increment">Can be negative</param>
        protected void SetSortOrder( int increment)
        {
            if ( IsInteger( nodeSortOrder.Text ) )
            {
                int current = Int32.Parse( nodeSortOrder.Text );
                current = current + increment;
                //should check for negatives 
                nodeSortOrder.Text = current.ToString();
            }
            else
            {

                nodeSortOrder.Text = "10";
            }
        }

        protected void btnUpdateNode_Click( object sender, EventArgs e )
        {

            selectedTab = ltlCurriculumTabName.Text;

            //validate
            if ( ValidateNode()  )
            {
                //save
                this.UpdateNode();
               //stay put for now

            }
        }

        protected bool ValidateNode()
        {
            bool isValid = true;

            //Title
            txtNodeTitle.Text = txtNodeTitle.Text.Trim().Replace( "<", "&lt;" ).Replace( ">", "&gt;" );
            if ( txtNodeTitle.Text.Length < int.Parse( ltlMinTxtTitleLength.Text ) )
            {
                SetConsoleErrorMessage( "You must enter a Title of meaningful length." );
                isValid = false;
            }

            //Description
            txtNodeDescription.Text = txtNodeDescription.Text.Replace( "<", "&lt;" ).Replace( ">", "&gt;" );
            if ( txtNodeDescription.Text.Length < int.Parse( ltlMinTxtDescriptionLength.Text ) )
            {
                SetConsoleErrorMessage( "You must enter a Description of meaningful length." );
                isValid = false;
            }
            if ( ddlNodePrivacyLevel.SelectedIndex < 1 )
            {
                SetConsoleErrorMessage( "You must select a privacy setting (Who can access this resource?)." );
                isValid = false;
            }
            return isValid;
        }

        private void UpdateNode()
        {
            int id = 0;
            int typeId = 10;
            int sortOrder = 10;
            string statusMessage = "";
            string action = "";
            ContentItem entity = new ContentItem();
            int parentId = 0;
            if ( Int32.TryParse( this.parentNodeId.Text, out parentId ) == false )
            {
                SetConsoleErrorMessage( "ERROR: a parentId has not been set for a new record - strange.>br/>Try selecting the parent node again, and then start a new item." );
                return;
            }
            try
            {
                Int32.TryParse( this.nodeId.Text, out id );


                if ( id == 0 )
                {
                    entity.Id = 0;
                    entity.ParentId = parentId;
                    Int32.TryParse( this.contentTypeId.Text, out typeId );
                    Int32.TryParse( this.nodeSortOrder.Text, out sortOrder );

                    entity.CreatedById = WebUser.Id;		
                    entity.CreatedBy = WebUser.FullName();
                    entity.Created = System.DateTime.Now;

                    entity.SortOrder = sortOrder;
                    action = "Create";
                    entity.IsActive = true;
                    //TBD
                    //once we have multiple buttons, use parm to indicate levels
                    entity.TypeId = typeId;

                    entity.IsOrgContentOwner = CurrentRecord.IsOrgContentOwner;
                    entity.StatusId = ContentItem.INPROGRESS_STATUS;
                    entity.PrivilegeTypeId = CurrentRecord.PrivilegeTypeId;
                    entity.ConditionsOfUseId = CurrentRecord.ConditionsOfUseId;

                    //entity.IsPublished = false;
                    entity.IsOrgContentOwner = CurrentRecord.IsOrgContentOwner;
                    entity.OrgId = CurrentRecord.OrgId;
                    //entity.ResourceVersionId = 0;

                    entity.RowId = Guid.NewGuid();
                    entity.UseRightsUrl = CurrentRecord.UseRightsUrl;

                }
                else
                {
                    // get current record 
                    //don't really want full hierarchy here????????
                    entity = myManager.Get( id );
                    action = "Update";
                }
                
                /* assign form fields 			 */
                entity.Title = this.txtNodeTitle.Text;
                entity.Summary = this.txtNodeDescription.Text;
                entity.Description = this.txtNodeDescription.Text;
                entity.PrivilegeTypeId = int.Parse( this.ddlNodePrivacyLevel.SelectedValue.ToString() );
                entity.Timeframe = txtTimeframe.Text;

                entity.LastUpdated = System.DateTime.Now;
                entity.LastUpdatedById = WebUser.Id;	
                entity.LastUpdatedBy = WebUser.FullName();	//include for future use

                if ( IsFilePresent( autodocUpload ) )
                {
                    bool isValid = HandleUpload( entity );
                    if ( isValid )
                    {
                        //will have been set in HandleDocument
                    }
                    else
                    {
                        //problem, should have displayed a message
                        return;
                    }
                }
                //call insert/update
                int entityId = 0;
                if ( entity.Id == 0 )
                {
                    entityId = myManager.Create_ef( entity, ref statusMessage );
                    if ( entityId > 0 )
                    {
                        entity.Id = entityId;
                        this.nodeId.Text = entityId.ToString();
                        //reget the whole record, for rowId
                        //entity = myManager.Get( entityId );
                    }
                }
                else
                {
                    statusMessage = myManager.Update( entity );
                }


                if ( statusMessage.Equals( "successful" ) )
                {
                    //get new/updated standards
                    //this could actually have a separate save
                    HandleStandardsUpdate( entity.Id );

                    //refresh tree
                    PopulateTree( CurrentRecord.Id );
                    PopulateNode( entity.Id );
                    this.SetConsoleSuccessMessage( action + " was successful for: " + entity.Title );
                    if ( action == "Create" )
                    {
                        litFrame.Text = string.Format( "<iframe id=\"contributeFrame\" runat=\"server\" src=\"/Contribute/?mode=upload&nodeId={0}&hh=temp&doingLRPublish=no\" width='400' height='600' ></iframe>", entity.Id.ToString() );
                    }
                }
                else
                {
                    this.SetConsoleErrorMessage( statusMessage );
                }

            }
            catch ( System.Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".UpdateNode() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }
        }
        protected bool HandleUpload( ContentItem entity )
        {
            bool isValid = true;
            string statusMessage = "";

            DocumentVersion docVersion = new DocumentVersion();
            //or from entity?
            if ( entity.IsValidRowId( entity.DocumentRowId ) )
            {
                //create the doc version, and add the entity
                docVersion.CreatedById = WebUser.Id;
                docVersion.Created = System.DateTime.Now;
                docVersion.Title = entity.Title;
                //should be using org rowid!
                //bool isOk = FileResourceController.CreateDocument( autodocUpload, docVersion, entity.OrgId, ref statusMessage );
                //if ( isOk )
                //{

                //    entity.DocumentUrl = docVersion.URL;
                //    entity.DocumentRowId = docVersion.RowId;

                //    txtAutoDocumentRowId.Text = docVersion.RowId.ToString();
                //    autodocFileName.Text = docVersion.FileName;
                //    autodocLink.NavigateUrl = entity.DocumentUrl;

                //}
                //return isOk;
                //=============================
            }
            else
            {

                docVersion = myManager.DocumentVersionGet( entity.DocumentRowId );
                //if replacing, may want to delete old file name
                //or will we want to attempt a default file name based on the current node?
            }
            docVersion.LastUpdatedById = CurrentUser.Id;

            try
            {
                bool isOk = FileResourceController.CreateDocument( autodocUpload, docVersion, entity.OrgId, ref statusMessage );

                if ( isOk )
                {

                    autodocPanel.Visible = true;
                    entity.DocumentUrl = docVersion.URL;
                    entity.DocumentRowId = docVersion.RowId;

                    txtAutoDocumentRowId.Text = docVersion.RowId.ToString();
                    autodocFileName.Text = docVersion.FileName;
                    autodocLink.NavigateUrl = entity.DocumentUrl;

                }
                else
                {
                    linkContentFile.Visible = false;
                }
            }
            catch ( Exception ex )
            {
                SetConsoleErrorMessage( "Unexpected error occurred while attempting to upload your file.<br/>" + ex.Message );
                LoggingHelper.LogError( ex, thisClassName + "HandleUpload" );
                return false;
            }

            return isValid;
        }


        protected void btnPublishNode_Click( object sender, EventArgs e )
        {

            //steps
            //- try to get tags from nearest parent, not just curriculum
            //- will not do LR, just to database and ES
        }


        private void DeleteNodeAttachment( object sender, System.EventArgs e )
        {
            //Response.Write ( "You Clicked on " + Request.Form[ "__EVENTARGUMENT" ].ToString () );
            string key = Request.Form[ "__EVENTARGUMENT" ].ToString();

            int id = Int32.Parse( key );
            DeleteNodeAttachment( id );
        }
        private void DeleteNodeAttachment( int id )
        {
            string statusMessage = "";
            selectedTab = ltlCurriculumTabName.Text;
            int nodeId = int.Parse( this.nodeId.Text );
            //get the item for later processing
            ContentItem item = myManager.Get( id );

            try
            {
				//  if ( myManager.ContentConnectorDelete( nodeId, id, ref statusMessage ) )
				if ( myManager.Delete( id, ref statusMessage ) )
                {
                    RemoveServerFile( item );

                    //reset mode as needed for removed records
                    SetConsoleSuccessMessage( "Document was removed from this section, and deleted from the site." );
                    PopulateNode( nodeId );
                    
                }
                else
                {
                    SetConsoleErrorMessage( "Error was encountered attempting to delete the attachment:<br/>" + statusMessage );
                }
            }
            catch ( System.Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".DeleteNodeAttachment() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }

        }
        private void RemoveServerFile( ContentItem entity )
        {
            if ( entity.DocumentUrl != null && entity.IsValidRowId( entity.DocumentRowId ))
            {
                string documentFolder = entity.RelatedDocument.FilePath;    // FileResourceController.DetermineDocumentPath( entity );
                if ( FileSystemHelper.DeleteDocumentFromServer( documentFolder, entity.RelatedDocument ) == false)
                {
                    //just notify admin
                    LoggingHelper.LogError( thisClassName + "RemoveServerFile. Apparantly the request to remove a file from the server failed?<br/>" + entity.DocumentUrl, true );
                    
                }
            }

        }
        #endregion

        protected void btnAutodocUpload_Click( object sender, EventArgs e )
        {
            //validate
            if ( autodocUpload.HasFile == false || autodocUpload.FileName == "" )
            {
                SetConsoleErrorMessage( "Please select a file before clicking upload" );
                selectedTab = ltlCurriculumTabName.Text;
                return;

            }

            LoggingHelper.DoTrace( 6, thisClassName + " btnAutodocUpload_Click" );

            //do upload
        }

        protected void btnCreateIsbeCurriculum_Click( object sender, EventArgs e )
        {
            int modules = 3;
            Int32.TryParse( ddlModules.SelectedValue, out modules );

            ContentItem hier = CurriculumServices.CreateIsbeHierarchy( CurrentRecord.Id, modules );
            if ( hier != null && hier.Id > 0 && hier.HasChildItems )
            {
                CurrentRecord.ChildItems = hier.ChildItems;
                curriculumIntroPanel.Visible = false;
                treePanel.Visible = true;
                noNodePanel.Visible = true;
                selectedTab = ltlCurriculumTabName.Text;

                PopulateTree( CurrentRecord.Id );
            }
            else
            {
                //error?
            }
        }

        protected void HandleStandardsUpdate( int contentId )
        {
            List<JSONStandard> list = GetPublishedStandards();

            if ( list != null && list.Count > 0 )
            {
                ContentStandard cs = new ContentStandard();
                List<ContentStandard> standards = new List<ContentStandard>();
                foreach ( JSONStandard js in list )
                {
                    cs = new ContentStandard();
                    cs.ContentId = contentId;
                    cs.StandardId = js.standardID;
                    cs.AlignmentTypeCodeId = js.alignmentTypeID;
                    cs.UsageTypeId = js.usageTypeID;
                    cs.CreatedById = WebUser.Id;

                    standards.Add( cs );
                }

                int added = new CurriculumServices().ContentStandard_Add( contentId, WebUser.Id, standards );
                //check if addedd equals expected
                if ( added != standards.Count )
                {
                    SetConsoleErrorMessage( string.Format( "Oh Oh - the number of standards to be added is: {0}, but he actual number of standards added was: {1}", standards.Count, added ));
                }
            }
        } //

        protected List<JSONStandard> GetPublishedStandards()
        {
          try
          {
            return new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<List<JSONStandard>>( hdnStandards.Value );
          }
          catch ( Exception ex )
          {
            LoggingHelper.LogError( ex, "Error deserializing Standards", false );
            return new List<JSONStandard>();
          }
        }
        protected class JSONStandard
        {
          public int standardID { get; set; }
          public string code { get; set; }
          public int alignmentTypeID { get; set; }
          public int usageTypeID { get; set; }
        }

        protected void newsSaveButton_Click( object sender, EventArgs e )
        {
            string message = this.newsEditor.EditPanel.Content;
            int id = 0;
            Int32.TryParse( this.newsId.Text, out id );
            if ( id == 0 )
            {
                id = new CurriculumServices().Curriculum_AddHistory( CurrentRecord.Id, message, WebUser.Id );
                SetConsoleSuccessMessage( "Added cuuriculum new item." );
                this.newsId.Text = id.ToString();
            }
            else
            {
                new CurriculumServices().Curriculum_UpdateHistory( id, message, WebUser.Id );
                SetConsoleSuccessMessage( "Updated cuuriculum new item." );
            }
            
        }

        protected void addNewNewsButton_Click( object sender, EventArgs e )
        {
            this.newsId.Text = "0";
            this.newsEditor.EditPanel.Content = "";
        }
       
    }
}
