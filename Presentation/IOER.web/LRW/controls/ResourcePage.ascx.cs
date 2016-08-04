using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using ILPathways.Controllers;
using ILPathways.Utilities;
using GroupManager = Isle.BizServices.GroupServices;
using MyManager = Isle.BizServices.ContentServices;
using ILPathways.Business;
using AppUser = LRWarehouse.Business.Patron; //ILPathways.Business.AppUser;
using AcctManager = Isle.BizServices.AccountServices;

//prefer to prevent direct reference to LR
using LRWarehouse.Business;
using ResManager = Isle.BizServices.ResourceBizService;

namespace ILPathways.LRW.controls
{
    public partial class ResourcePage : BaseUserControl
    {
        const string thisClassName = "ResourcePage";
        MyManager myManager = new MyManager();

        #region Properties
        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string FormSecurityName
        {
            get { return this.txtFormSecurityName.Text; }
            set { this.txtFormSecurityName.Text = value; }
        }
        public int CurrentRecordID
        {
            get { return int.Parse( this.txtCurrentContentId.Text ); }
            set { this.txtCurrentContentId.Text = value.ToString(); }
        }
        public string resourceTitle;
        public string resourceSummary;
        public string resourceFrameSource;
        public string redirectTarget;

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
                    CurrentUser.ParentOrgId = 0;
                    //get has not been attempted, do now
                    string statusMessage = string.Empty;
                    Organization org = AcctManager.GetOrganization( CurrentUser, ref statusMessage );
                    if ( org != null && org.Id > 0 )
                    {
                        CurrentUser.ParentOrgId = org.ParentId;
                    }
                    //update session
                    this.WebUser = CurrentUser;
                }
            }

            try
            {
                string rid = this.GetRequestKeyValue( "rid", "" );

                string _author = "";
                string _org = "";
                string request = Request.RawUrl;

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
                if ( request.ToLower().IndexOf( "k12/home/" ) > -1 )
                {
                    if ( Page.RouteData.Values.ContainsKey( "OrgName" ) )
                    {
                        _org = Page.RouteData.Values[ "OrgName" ].ToString();
                        //look up district
                    }

                    if ( Page.RouteData.Values.ContainsKey( "Author" ) )
                    {
                        _author = Page.RouteData.Values[ "Author" ].ToString();
                        //look up author, use district, determine if home page exists
                    }
                }
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
        /// Populate the form 
        ///</summary>
        private void PopulateForm( ContentItem entity )
        {

            CurrentRecordID = entity.Id;
            resourceTitle = entity.Title;
            lblContentState.Text = "Status: " + entity.Status + "<br/>"
                            + "Privileges: " + entity.PrivilegeType + "<br/>";

            resourceSummary = entity.Summary;
            lblAuthor.Text = entity.Author;


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
            lblUsageRights.Text = entity.ConditionsOfUse;
            if ( entity.ConditionsOfUseIconUrl.Length > 0 )
            {
                lblUsageRights.Text = string.Format( ccouImageLinkTemplate.Text, entity.ConditionsOfUseUrl,  entity.ConditionsOfUseIconUrl, entity.ConditionsOfUse );

            }

            pageContent.Text = entity.Description;

            resourceFrameSource = CurrentRecordID.ToString();
            if ( entity.ResourceVersionId > 0 )
            {
                communityViewPanel.Visible = true;
                ResourceVersion rv = ResManager.ResourceVersion_Get( entity.ResourceVersionId );
                string url = new PublishController().FormatFriendlyResourceUrl( rv );
                this.hlResourceVerionLink.NavigateUrl = url;    // string.Format( this.rvLink.Text, entity.ResourceVersionId );

            }
            else
            {
                communityViewPanel.Visible = false;
                this.hlResourceVerionLink.NavigateUrl = "";
                hlResourceVerionLink.Text = "Not available";
            }


            //get attachments
            PopulateSupplements( entity );
            //get references
            PopulateReferences( entity );
            //check for standards
            if ( entity.ResourceVersionId > 0)
                PopulateStandards( entity );

            if ( attachmentsPanel.Visible == false && referencesPanel.Visible == false && this.standardsPanel.Visible == false )
                supplementsPanel.Visible = false;

            buttonBox.Visible = false;
            if ( entity.StatusId == ContentItem.INPROGRESS_STATUS )
            {
                //TODO - need to handle updates. ON update, will not be publishing/
                //if has a LR RV record
                if ( entity.ResourceVersionId == 0 && UserCanPublishThisResource( entity ) )
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
            //       they would have to be authnticated with org admin status???
            if ( entity.PrivilegeTypeId == ContentItem.PUBLIC_PRIVILEGE )
                return true;

            //else must be authenticated
            if ( IsUserAuthenticated() == false)
                return false;

            CurrentUser = GetAppUser();
            //must have an orgId, although not for the state
            if ( CurrentUser.OrgId == 0 )
                return false;


            if (  entity.PrivilegeTypeId == ContentItem.MY_ORG_PRIVILEGE
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


        private void PopulateSupplements( ContentItem entity )
        {
            List<ContentSupplement> list = myManager.ContentSupplementsSelectList( entity.Id );

            if ( list.Count > 0 )
            {
                attachmentsPanel.Visible = true;
                foreach ( ContentSupplement item in list ) 
                {
                    //supplements.Text += item.Title;
                    if ( item .PrivilegeTypeId > ContentItem.PUBLIC_PRIVILEGE)
                        supplements.Text +=  "Privilege: " + item.PrivilegeType + "<br/>";

                    if ( item.Description.Trim().Length > 0)
                        supplements.Text += item.Description + "<br/>";
                   
                    if ( IsUserAuthenticated() == false &&
                        ( entity.PrivilegeTypeId != 1 || item.PrivilegeTypeId != 1 ) )
                    {

                        supplements.Text += "<br/>Private document, viewing is prohibited: " + item.Title;

                    }
                    else
                    {
                        if ( item.RelatedDocument != null && item.RelatedDocument.HasValidRowId() )
                        {
                            //if ( IsTestEnv() )
                            //    supplements.Text += "<br/>(test only): " + item.RelatedDocument.FileName;

                            //check if file exists on server, if not it will be downloaded
                            string fileUrl = ValidateDocumentOnServer( entity, item.RelatedDocument );
                            if ( fileUrl.Length > 10 )
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
                            else
                                supplements.Text += "<br/>Sorry issue encountered locating the related document.";
                        }
                    }

                    supplements.Text += "<br/><hr><br/>";
                }

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
                        if (showingRefUrl.Text.Equals("yes"))
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
            ResourceStandardCollection standardsList = standardManager.SelectResourceStandardsByVersion( entity.ResourceVersionId );

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
                if ( ContentController.RequestApproval( CurrentRecordID, user, ref statusMessage ) == true )
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

            if ( ContentController.HandleApprovedAction( CurrentRecordID, user, ref statusMessage ) == true )
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

            if ( ContentController.HandleDeclinedAction( CurrentRecordID, txtReason.Text, user, ref statusMessage ) == true )
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

            return ContentController.IsUserOrgApprover( entity, CurrentUser.Id );
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
