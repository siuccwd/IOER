using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;
using IOER.Controllers;
using ILPathways.Utilities;
using GroupManager = Isle.BizServices.GroupServices;
using MyManager = Isle.BizServices.ContentServices;
using ILPathways.Business;
using AppUser = LRWarehouse.Business.Patron; //ILPathways.Business.AppUser;
using AcctManager = Isle.BizServices.AccountServices;
using OrgManager = Isle.BizServices.OrganizationBizService;
using Isle.BizServices;
//prefer to prevent direct reference to LR
using LRWarehouse.Business;
using ResManager = Isle.BizServices.ResourceBizService;

namespace IOER.Controls.Content
{
    /// <summary>
    /// Handle display of a content item
    /// - has special code for handling curriculum type
    /// </summary>
    public partial class ContentDisplay : BaseUserControl
    {
        const string thisClassName = "ContentDisplay";
        MyManager myManager = new MyManager();

        #region Properties

        public int CurrentRecordID
        {
            get { return int.Parse( this.txtCurrentContentId.Text ); }
            set { this.txtCurrentContentId.Text = value.ToString(); }
        }
        public string resourceTitle;
        public string resourceSummary;
        public string redirectTarget;
		public bool showIframeContent = false;
		public string iframeContentUrl = "";

        #endregion

        /// <summary>
        /// Handle Page Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void Page_Load( object sender, EventArgs ex )
        {

            if ( !IsPostBack )
            {
                this.InitializeForm();
				CheckRecordRequest();
            }
        }//

        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        private void InitializeForm()
        {
            if ( IsUserAuthenticated() )
            {
                CurrentUser = GetAppUser();
                if ( CurrentUser.ParentOrgId == -1 )
                {

                    //TODO ********** need to use commmon method that handles org mbrs **********
                    CurrentUser.ParentOrgId = 0;
                    //get has not been attempted, do now
                    string statusMessage = string.Empty;
                    Organization org = OrgManager.GetOrganization( CurrentUser, ref statusMessage );
                    if ( org != null && org.Id > 0 )
                    {
                        CurrentUser.ParentOrgId = org.ParentId;
                    }
                    //update session
                    this.WebUser = CurrentUser;
                }
            }
        }//

		private void CheckRecordRequest()
		{
            try
            {
                string rid = this.GetRequestKeyValue( "rid", "" );

				//string _author = "";
				//string _org = "";
                //string request = Request.RawUrl;

                if ( Page.RouteData.Values.ContainsKey( "RouteID" ) )
                {
                    string _routeID = "";
                    if ( Page.RouteData.Values.Count > 0 )
                        _routeID = Page.RouteData.Values[ "RouteID" ].ToString();
                    else
                        _routeID = "";
                    if ( _routeID.Length > 0 && IsInteger( _routeID ) )
                    {
                        CurrentRecordID = Int32.Parse( _routeID );
                    }
                    else if ( _routeID.Length == 36 )
                    {
                        rid = _routeID;
                    }
                }
				//if ( request.ToLower().IndexOf( "k12/home/" ) > -1 )
				//{
				//	if ( Page.RouteData.Values.ContainsKey( "OrgName" ) )
				//	{
				//		_org = Page.RouteData.Values[ "OrgName" ].ToString();
				//		//look up district
				//	}

				//	if ( Page.RouteData.Values.ContainsKey( "Author" ) )
				//	{
				//		_author = Page.RouteData.Values[ "Author" ].ToString();
				//		//look up author, use district, determine if home page exists
				//	}
				//}
                if ( rid.Trim().Length == 36 )
                {
                    this.Get( rid );
                }
                else
                {
                    if ( CurrentRecordID == 0 )
                    {
                        CurrentRecordID = this.GetRequestKeyValue( "cidx", 0 );
                    }
                    if ( CurrentRecordID > 0 )
                        Get( CurrentRecordID );
                }
                if ( CurrentRecordID == 0 )
                    SetConsoleErrorMessage( "Invalid page request" );
            }
			catch ( System.Threading.ThreadAbortException tex )
			{
			}
            catch
            {
                SetConsoleErrorMessage( "Invalid ID specified" );
                return;
            }


        }	// End 


        #region retrieval
        private ContentItem Get()
        {
            ContentItem entity = new ContentItem();
            string rid = this.GetRequestKeyValue( "rid", "" );
            if ( rid.Trim().Length == 36 )
            {
                entity = myManager.GetByRowId( rid );
            }
            else
            {
                int id = this.GetRequestKeyValue( "cidx", 0 );
                if ( id > 0 )
                    entity = myManager.Get( id );
            }
            return entity;
        }
        /// <summary>
        /// Get - retrieve form data
        /// </summary>
        /// <param name="recId"></param>
        private void Get( int recId )
        {
            try
            {
                ContentItem entity = myManager.Get( recId );

                if ( entity == null || entity.IsValid == false )
                {
                    this.SetConsoleErrorMessage( "Sorry the requested record does not exist" );
                    return;

                }
                else
                {
					string stay = this.GetRequestKeyValue("stay", "no");
					if (entity.IsHierarchyType
						&& redirecting50ToLearningList.Text.Equals("yes")
						&& stay == "no")
					{
						Response.Redirect(string.Format("/LearningList/{0}/{1}", entity.Id, ResourceBizService.FormatFriendlyTitle(entity.Title)), true);
					}
                    PopulateForm( entity );
                }
			}
			catch (System.Threading.ThreadAbortException tex)
			{
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
                ContentItem entity = myManager.GetByRowId( recId );

                if ( entity == null || entity.IsValid == false )
                {
                    this.SetConsoleErrorMessage( "Sorry the requested record does not exist" );
                    return;

                }
                else
                {
					string stay = this.GetRequestKeyValue("stay", "no");
					if (entity.IsHierarchyType
						&& redirecting50ToLearningList.Text.Equals("yes")
						&& stay == "no")
					{
						Response.Redirect(string.Format("/LearningList/{0}/{1}", entity.Id, ResourceBizService.FormatFriendlyTitle(entity.Title)), true);
					}
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
        /// Populate the form 
        ///</summary>
        private void PopulateForm( ContentItem entity )
        {
           bool isHierarcyType = false;

            if ( (entity.IsHierarchyType && entity.TypeId != ContentItem.CURRICULUM_CONTENT_ID)
                || entity.TypeId == ContentItem.DOCUMENT_CONTENT_ID )
            {
                //if a hierarch type, get the top level in ordr to get the author info
                //the curriculum handler will take care of displaying the correct node 
                ContentItem topNode = myManager.GetTopNodeForHierarchy( entity );
                if (topNode != null && topNode.Id > 0) 
                {
                    entity = topNode;
                    isHierarcyType = true;
                }

                //int topId = myManager.GetTopIdForHierarchy( entity );
                //if ( topId > 0 )
                //{
                //    entity = myManager.Get( topId );
                //}
            }
            new ActivityBizServices().ContentHit(entity, CurrentUser);

            CurrentRecordID = entity.Id;
            resourceTitle = entity.Title;
            lblContentState.Text = "Status: " + entity.Status + "<br/>"
                            + "Privileges: " + entity.PrivilegeType + "<br/>";

            resourceSummary = entity.Summary;
            lblAuthor.Text = entity.Author;
			hlAuthor.Text = entity.Author;
			hlAuthor.NavigateUrl = string.Format( profLink.Text, entity.CreatedById, ResourceVersion.UrlFriendlyTitle( entity.Author));

            // check privileges to determine if current user/guest can view
            if ( CanView( entity ) == false )
            {
                pageContent.Text += privateContentMsg.Text;
                notAllowedPanel.Visible = true;
                return;
            }
            lblStatus.Text = entity.Status;
            if ( entity.StatusId > 0 )
            {
                string style = entity.Status.Replace( " ", "" ).ToLower();
                lblStatus.CssClass = style + "Status";
            }
            if ( entity.StatusId == ContentItem.PUBLISHED_STATUS )
            {
                statusDiv.Visible = false;
            }

            //Page Title
            Page.Title = entity.Title + " | ISLE OER";
            //OK
            detailPanel.Visible = true;

            lblPrivileges.Text = entity.PrivilegeType;
            if ( entity.ConditionsOfUse != null )
            {
                lblUsageRights.Text = entity.ConditionsOfUse;
                if ( entity.ConditionsOfUseIconUrl.Length > 0 )
                {
                    lblUsageRights.Text = string.Format( ccouImageLinkTemplate.Text, entity.ConditionsOfUseUrl, entity.ConditionsOfUseIconUrl, entity.ConditionsOfUse );

                }
            }

            if ( entity.HasResourceId() )
            {
                communityViewPanel.Visible = true;

                //14-04 24 mp - the latter should already be in the entity?
                string url = GetResourceUrl( entity );                
                if ( url.Length > 5 )
                {
                    this.hlResourceVerionLink.NavigateUrl = url;   
                }
                else
                {
                    communityViewPanel.Visible = false;
                    this.hlResourceVerionLink.NavigateUrl = "";
                    hlResourceVerionLink.Text = "Not available";
                }
            }
            else
            {
                communityViewPanel.Visible = false;
                this.hlResourceVerionLink.NavigateUrl = "";
                hlResourceVerionLink.Text = "Not available";
            }

            if ( entity.TypeId != ContentItem.DOCUMENT_CONTENT_ID )
            {
                //get attachments
                PopulateSupplements( entity );
                //get references
                PopulateReferences( entity );
            }
            //check for standards
            if ( entity.IsHierarchyType == false )
            {
                if ( entity.HasResourceId() )
                    PopulateStandards( entity );
            }

            if ( attachmentsPanel.Visible == false && referencesPanel.Visible == false  )
                supplementsPanel.Visible = false;

            buttonBox.Visible = false;
            if ( entity.StatusId == ContentItem.INPROGRESS_STATUS )
            {
                //TODO - need to handle updates. ON update, will not be publishing/
                //if has a LR RV record
                if ( entity.ResourceIntId == 0 && UserCanPublishThisResource( entity ) )
                {
                    buttonBox.Visible = true;
                    publishSection.Visible = true;
                }

            }
            else if ( entity.StatusId == ContentItem.REVISIONS_REQUIRED_STATUS )
            {
                //TODO - need to handle updates. ON update, will not be publishing/
                //if has a LR RV record
                if ( UserCanPublishThisResource( entity ) )
                {
                    buttonBox.Visible = true;
                    requestApprovalButton.Visible = true;
                }

            }
            else if ( entity.StatusId == ContentItem.SUBMITTED_STATUS )
            {

                if ( UserCanApproveThisResource( entity ) )
                {
                    buttonBox.Visible = true;
                    approveSection.Visible = true;
                }
                else
                {
                    approveSection.Visible = false;
                }
            }

            curriculumPanel.Visible = false;
            if ( entity.IsHierarchyType == true )
            {
                summaryPanel.Visible = false;
                curriculumPanel.Visible = true;
                pageContent.Visible = false;
                //curriculumContent.Text = entity.Description;
            }
            else
            if ( entity.TypeId == ContentItem.DOCUMENT_CONTENT_ID )
            {
                //check if part of a curriculum
                if ( myManager.IsNodePartOfCurriculum( entity ) )
                {
                    summaryPanel.Visible = false;
                    curriculumPanel.Visible = true;
                    pageContent.Visible = false;
                }
                else
                {
                    PopulateDocument( entity );
                }
            }
            else
            {
                curriculumPanel.Visible = false;
                if (entity.Description != null && entity.Description.Length > 5)
                    pageContent.Text = entity.Description;
                else 
                    pageContent.Text = entity.Summary;
            }
        }//


        private string GetResourceUrl( ContentItem entity )
        {
            string url = "";
             if (entity.ResourceFriendlyUrl != null)
                 url = entity.ResourceFriendlyUrl;
             else 
             {             
                ResourceVersion rv = ResManager.ResourceVersion_GetByResourceId( entity.ResourceIntId );
                if ( rv != null && rv.Id > 0 )
                {
                    url = ResourceBizService.FormatFriendlyResourceUrlByResId( rv.Title, rv.ResourceIntId );  
                }
             }

            return url;
        }//


        private bool CanView( ContentItem entity )
        {
            bool isValid = false;
            if ( IsUserAuthenticated() && WebUser.Id == entity.CreatedById )
                return true;

            //allow approvers
            if ( entity.StatusId > ContentItem.INPROGRESS_STATUS && UserCanApproveThisResource( entity ) )
                return true;

            if ( IsUserAuthenticated() && WebUser.Id == 2 )
            {
                //notAllowedPanel.Visible = true;
                //SetConsoleInfoMessage( "OVERRIDE - Normally MAY not be allowed" );
                //return true;
            }
            string allow = this.GetRequestKeyValue( "ov", "" );
            if ( allow.ToLower() == "isle" && System.DateTime.Now < new System.DateTime( 2013, 9, 15 ) )
            {
                SetConsoleInfoMessage( "OVERRIDE - Normally MAY not be allowed" );
                return true;
            }

            if ( entity.StatusId < ContentItem.PUBLISHED_STATUS )
                return false;

            //TODO - only caveat may be to allow a reviewer to see this view?
            //       they would have to be authenticated with org admin status???
            if ( entity.PrivilegeTypeId == ContentItem.PUBLIC_PRIVILEGE )
                return true;

            //else must be authenticated
            if ( IsUserAuthenticated() == false )
                return false;

            CurrentUser = GetAppUser();
            //must have an orgId, although not for the state
            if ( CurrentUser.OrgId == 0 )
                return false;


            if ( entity.PrivilegeTypeId == ContentItem.MY_ORG_PRIVILEGE
               && entity.OrgId == CurrentUser.OrgId )
                return true;


            //hmm what for district??
            //==> need to include parent with content, and also get user parent - should persist, or maybe an oppurtunity for cookies???
            //-should have a state level access, or admin check
            //start with having an org - since 

            if ( entity.PrivilegeTypeId == ContentItem.MY_REGION_PRIVILEGE )
            {
                if ( entity.OrgId == CurrentUser.OrgId
                  || entity.OrgId == CurrentUser.ParentOrgId
                  || entity.ParentOrgId == CurrentUser.ParentOrgId
                  || entity.ParentOrgId == CurrentUser.OrgId
                 )
                    return true;
            }

            return isValid;
        }//


        private void PopulateDocument( ContentItem entity )
        {

            if ( entity.RelatedDocument != null && entity.RelatedDocument.HasValidRowId() )
            {
                //if ( IsTestEnv() )
                //    supplements.Text += "<br/>(test only): " + item.RelatedDocument.FileName;

                //check if file exists on server, if not it will be downloaded
                //15-01-11 MP - need to ensure uses
                string fileUrl = FileResourceController.ValidateDocumentOnServer( entity, entity.RelatedDocument );
                if ( fileUrl.Length > 10 )
                {
                    bool isOwner = ( IsUserAuthenticated() && WebUser.Id == entity.CreatedById );

                    if ( entity.PrivilegeTypeId == ContentItem.PUBLIC_PRIVILEGE || isOwner )
                    {
                        pageContent.Text += "<p>" + string.Format( docLinkTemplate.Text, fileUrl, entity.RelatedDocument.Title ) + "</p>";
												showIframeContent = true;
												iframeContentUrl = fileUrl;
                    }
                    else if ( IsUserAuthenticated() == false )
                    {
                        pageContent.Text += string.Format( "<h2>{0}</h2>Private document, viewing is not allowed for users who are not logged in.", entity.RelatedDocument.Title );
                    }
                    else if ( entity.PrivilegeTypeId >= ContentItem.MY_ORG_PRIVILEGE && WebUser.OrgId != entity.OrgId )
                    {
                        pageContent.Text += string.Format( "<h2>{0}</h2>Private document, viewing is only allowed for members of this organization ({1}).", entity.RelatedDocument.Title, entity.Organization );
                    } //don't have a process for region/state ============================================
                    //else if ( entity.PrivilegeTypeId > ContentItem.MY_ORG_PRIVILEGE )
                    //{
                    //    pageContent.Text += string.Format( "<h2>{0}</h2>Private document, viewing is prohibited.", entity.RelatedDocument.Title );
                    //}
                    else
                    {
                        //same org, ok for now
                        pageContent.Text += "<p>" + string.Format( docLinkTemplate.Text, fileUrl, entity.RelatedDocument.Title ) + "</p>";
												showIframeContent = true;
												iframeContentUrl = fileUrl;
                    }


                }
                else
                {
                    pageContent.Text += "<br/>Sorry issue encountered locating the related document.<br/>" + entity.Message;
                }
            }
        }//


        private void PopulateSupplements( ContentItem entity )
        {
            bool isOwner = ( IsUserAuthenticated() && WebUser.Id == entity.CreatedById );

            List<ContentSupplement> list = myManager.ContentSupplementsSelectList( entity.Id );

            if ( list.Count > 0 )
            {
                attachmentsPanel.Visible = true;
                foreach ( ContentSupplement item in list )
                {
                    //check if file exists on server, if not it will be downloaded
                    //string fileUrl = ValidateDocumentOnServer( entity, item.RelatedDocument );
                    string fileUrl = FileResourceController.ValidateSupplementDocumentOnServer( item, item.RelatedDocument, entity );

                    if ( item.PrivilegeTypeId > ContentItem.PUBLIC_PRIVILEGE )
                        supplements.Text += "Privilege: " + item.PrivilegeType + "<br/>";

                    if ( item.Description.Trim().Length > 0 )
                        supplements.Text += item.Description + "<br/>";

                    if ( fileUrl.Length > 10 )
                    {

                        if ( isOwner || ( entity.PrivilegeTypeId == ContentItem.PUBLIC_PRIVILEGE || item.PrivilegeTypeId == ContentItem.PUBLIC_PRIVILEGE ) )
                        {
                            supplements.Text += "" + string.Format( docLinkTemplate.Text, fileUrl, item.Title ) + "<br/>";
                        }
                        else
                        if ( IsUserAuthenticated() == false  )
                        {
                            supplements.Text += "<br/>Private document, viewing is not allowed for users who are not logged in: " + item.Title;
                        }
                        else if ( entity.PrivilegeTypeId >= ContentItem.MY_ORG_PRIVILEGE && WebUser.OrgId != entity.OrgId )
                        {
                            pageContent.Text += string.Format( "<h2>{0}</h2>Private document, viewing is only allowed for members of this organization ({1}).", item.Title, entity.Organization );
                        } //don't have a process for region/state ============================================
                        else
                        {
                            if ( showingImageAttachmentsInline.Text.Equals( "yes" ) )
                            {
                                //would also need to check file size to ensure not excessive
                            }
                            /*if ( item.RelatedDocument.MimeType.ToLower().IndexOf("image") > -1
                                || fileUrl.ToLower().IndexOf( "jpg" ) > -1 )
                            {
                                //show actual image
                                supplements.Text += string.Format( imageLinkTemplate.Text, item.RelatedDocument.RowId ) + "<br/>";
                            }
                            else*/
                            supplements.Text += "" + string.Format( docLinkTemplate.Text, fileUrl, item.Title ) + "<br/>";

                        }
                    }
                    else
                        supplements.Text += "<br/>Sorry issue encountered locating the related document.";

                    supplements.Text += "<br/><hr><br/>";
                }

            }
        }//
        
        private void PopulateReferences( ContentItem entity )
        {
            List<ContentReference> list = myManager.ContentReferencesSelectList( entity.Id );

            if ( list.Count > 0 )
            {
                referencesPanel.Visible = true;
                foreach ( ContentReference item in list )
                {
                    if ( item.Title.Trim().Length > 0 )
                        lblReferences.Text += "<strong>" + item.Title + "</strong><br />";
                    if ( item.Author.Trim().Length > 0 )
                        lblReferences.Text += "<strong>Author:</strong> " + item.Author + "<br />";

                    if ( item.Publisher.Trim().Length > 0 )
                        lblReferences.Text += "<strong>Publisher:</strong> " + item.Publisher + "<br/>";
                    if ( item.AdditionalInfo.Trim().Length > 0 )
                        lblReferences.Text += "<strong>Additional Information:</strong> " + item.AdditionalInfo + "<br/>";
                    if ( item.ISBN.Trim().Length > 0 )
                        lblReferences.Text += "<strong>ISBN:</strong> " + item.ISBN + "<br />";

                    if ( item.ReferenceUrl.Length > 0 )
                    {
                        if ( showingRefUrl.Text.Equals( "yes" ) )
                            lblReferences.Text += "<br/><a href='" + item.ReferenceUrl + "' target='_blank'>" + item.ReferenceUrl + "</a><br />";
                        else
                            lblReferences.Text += "<br/><a href='" + item.ReferenceUrl + "' target='_blank'>Website</a><br />";
                    }

                    lblReferences.Text += "<br/><hr><br/>";
                }

            }
        }//

        private void PopulateStandards( ContentItem entity )
        {
            //Standards
            ResManager standardManager = new ResManager();
            ResourceStandardCollection standardsList = standardManager.ResourceStandards_Select( entity.ResourceIntId );

            if ( standardsList != null && standardsList.Count > 0 )
            {
                lblStandardsList.Text = "<ul class=\"associatedStandards\">";
                foreach ( ResourceStandard standard in standardsList )
                {
                    if ( standard.AlignmentTypeValue == "" )
                    {
                        standard.AlignmentTypeValue = "Aligns To";
                    }
                    if ( standard.AlignmentTypeCodeId == 0 )
                    {
                        standard.AlignmentTypeCodeId = 0;
                    }
                    lblStandardsList.Text = lblStandardsList.Text + "<li class=\"associatedStandard\">The Resource " + standard.AlignmentTypeValue + " This Standard:<br /> <a id=\"alignedStandard_" + standard.StandardId + "\" class=\"toolTipLink\" href=\"" + standard.StandardUrl + "\" target=\"_blank\" title=\"" + standard.StandardNotationCode + "|" + standard.StandardUrl + "|" + standard.StandardDescription + "\">" + standard.StandardNotationCode + "</a>";
                }
                lblStandardsList.Text = lblStandardsList.Text + "</ul>";

                standardsPanel.Visible = true;
            }

        }//

        #endregion

        #region Events

        public void PublishButton_Click( object sender, EventArgs e )
        {
            if ( UserCanPublishThisResource() )
            {
                //Response.Redirect( "/Publish.aspx" ); //Keeps targeting the iframe
                redirectTarget = Request.Url.Scheme + "://" + Request.Url.Host + ":" + Request.Url.Port + "/Publish.aspx";
                Session.Add( "authoredResourceID", CurrentRecordID.ToString() );
                Session.Add( "authoredResourceURL", Request.Url.Scheme + "://" + Request.Url.Host + ":" + Request.Url.Port + Request.RawUrl );
            }
        }
        public void RequestApprovalButton_Click( object sender, EventArgs e )
        {
            if ( UserCanPublishThisResource() )
            {
                string statusMessage = string.Empty;
                AppUser user = GetAppUser();
				if (myManager.RequestApproval(CurrentRecordID, user, ref statusMessage) == true)
                {
                    SetConsoleSuccessMessage( "An email was sent requesting a review of your resource." );
                    //refresh
                    requestApprovalButton.Visible = false;
                    Get( CurrentRecordID );
                }
                else
                    SetConsoleErrorMessage( "An error occurred attempting to initiate a review of your resource.<br/>System administration has been notified.<br/>" + statusMessage );

            }
        }

        public void ApproveButton_Click( object sender, EventArgs e )
        {
            /* set status to Published
            * set resource to active (or whatever other steps)
            * insert record in audit table
            * 
            * send approved message
            */

            string statusMessage = string.Empty;
            AppUser user = GetAppUser();

            if ( PublishingServices.HandleApprovedAction( CurrentRecordID, user, ref statusMessage ) == true )
            {
                SetConsoleSuccessMessage( "A notification of this approval was sent to the author of this resource. <br>" );
                //refresh
                approveButton.Visible = false;
                Get( CurrentRecordID );
            }
            else
            {
                SetConsoleErrorMessage( "An error occurred attempting to approve this resource.<br/>System administration has been notified.<br/>" + statusMessage );
            }

        }

        public void DeclineButton_Click( object sender, EventArgs e )
        {
            /* set status to Declined
            * set resource to inactive (in case previously published)
            * ??do we need some qualifier as to way a resource is inactive to prevent part of a cleanup
            * insert record in audit table
            * a reason will be required, include in an email and audit table
            * send email
            * 
            */
            //check that a reason was given
            txtReason.Text = FormHelper.CleanText( txtReason.Text );
            if ( txtReason.Text.Trim().Length < 20 )
            {
                SetConsoleErrorMessage( "Please provide a reason of at least 20 characters" );
            }

            string statusMessage = string.Empty;
            AppUser user = GetAppUser();

			if (myManager.HandleDeclinedAction(CurrentRecordID, txtReason.Text, user, ref statusMessage) == true)
            {
                SetConsoleSuccessMessage( "A notification of this action was sent to the author of this resource." );
                //refresh
                approveButton.Visible = false;
                Get( CurrentRecordID );
            }
            else
            {
                SetConsoleErrorMessage( "An error occurred attempting to approve this resource.<br/>System administration has been notified.<br/>" + statusMessage );
            }
        }
        #endregion



        #region Validation Methods
        /// <summary>
        /// validate form content
        /// </summary>
        /// <returns></returns>
        private bool IsFormValid()
        {
            //int recId = 0;
            bool isValid = true;

            try
            {
                Page.Validate();
                isValid = HasRequiredFields();
            }
            catch ( System.Exception ex )
            {
                //SetFormMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.ToString() );
                return false;
            }

            return isValid;
        } //

        /// <summary>
        /// verify required fields are present
        /// </summary>
        /// <returns></returns>
        private bool HasRequiredFields()
        {
            bool isValid = true;

            return isValid;
        } //

        protected bool UserCanPublishThisResource()
        {

            if ( !IsUserAuthenticated() )
            {
                return false;
            }

            //TODO - need to handle with rowId
            ContentItem entity = new ContentItem();
            if ( CurrentRecordID > 0 )
            {
                entity = myManager.Get( CurrentRecordID );
            }
            else
            {
                entity = Get();
            }

            return UserCanPublishThisResource( entity );

        }
        protected bool UserCanPublishThisResource( ContentItem entity )
        {

            if ( !IsUserAuthenticated() )
            {
                return false;
            }

            if ( entity.CreatedById == WebUser.Id )
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        protected bool UserCanApproveThisResource()
        {

            if ( !IsUserAuthenticated() )
            {
                return false;
            }

            //TODO - need to handle with rowId
            ContentItem entity = new ContentItem();
            if ( CurrentRecordID > 0 )
            {
                entity = myManager.Get( CurrentRecordID );
            }
            else
            {
                entity = Get();
            }

            return UserCanApproveThisResource( entity );

        }

        protected bool UserCanApproveThisResource( ContentItem entity )
        {

            if ( !IsUserAuthenticated() )
            {
                return false;
            }

            CurrentUser = GetAppUser();

            //for now, don't allow author to approve,even if an approver??
            if ( entity.CreatedById == CurrentUser.Id && canAuthorApproveOwnContent.Text == "no" )
            {
                return false;
            }
            /* 
             * get orgId, and parentOrgId, or specific method
             * 
             * 
             */

			return  ContentServices.IsUserOrgApprover(entity, CurrentUser.Id);
            //string statusMessage = "";
            //if ( entity.OrgId > 0 )
            //{
            //    if ( GroupManager.IsUserAnOrgApprover( entity.OrgId, CurrentUser.Id ) )
            //        return true;
            //    else
            //        return false;
            //}
            //AcctManager mgr = new AcctManager();
            //AppUser author = mgr.Get( entity.CreatedById );
            //   //if valid
            //Organization org = AcctManager.GetOrganization( author, ref statusMessage );
            //if ( org != null && org.Id > 0 )
            //{
            //    if ( GroupManager.IsUserAnOrgApprover( org.Id, CurrentUser.Id ) )
            //        return true;
            //}

            //return false;
        }
        #endregion

    }
}
