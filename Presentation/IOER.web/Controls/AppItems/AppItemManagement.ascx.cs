using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EmailHelper = ILPathways.Utilities.EmailManager;
using MyManager = ILPathways.DAL.AppItemManager;
using ILPathways.Business;
using ILPathways.classes;
using ILPathways.Controllers;
using BDM = ILPathways.Common.BaseDataManager;
using ILPathways.DAL;
using ILPathways.Library;
using ILPathways.Utilities;

using Obout.Ajax.UI.HTMLEditor;

namespace ILPathways.Controls.AppItems
{
    public partial class AppItemManagement : BaseUserControl
    {
        MyManager myManager;
        FileResourceController resController;

        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
        const string thisClassName = "AppItemManagement";
        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        const string formSecurityName = "/vos_portal/advisor/Controls/ApplicationItems/AppItemManagement.ascx";

        //set to blank to default to order from proc
        const string gridDefaultSortExpression = "";

        #region Properties
        /// <summary>
        /// INTERNAL PROPERTY: CurrentRecord
        /// Set initially and store in ViewState
        /// </summary>
        protected AppItem CurrentRecord
        {
            get { return ViewState[ "CurrentRecord" ] as AppItem; }
            set { ViewState[ "CurrentRecord" ] = value; }
        }

        protected DataItem CurrentFaqCategory
        {
            get { return ViewState[ "CurrentFaqCategory" ] as DataItem; }
            set { ViewState[ "CurrentFaqCategory" ] = value; }
        }

        protected string AppItemTypeRestriction
        {
            get { return appItemTypeRestriction.Text; }
            set { appItemTypeRestriction.Text = value; }
        }
        /// <summary>
        /// Store retrieve the last page number - used after updates to attempt to show the same page
        /// </summary>
        protected int LastPageNumber
        {
            get
            {
                if ( ViewState[ "LastPageNumber" ] == null )
                    ViewState[ "LastPageNumber" ] = 0;

                if ( IsInteger( ViewState[ "LastPageNumber" ].ToString() ) )
                    return Int32.Parse( ViewState[ "LastPageNumber" ].ToString() );
                else
                    return 0;
            }
            set { ViewState[ "LastPageNumber" ] = value; }
        }//
        protected int LastTotalRows
        {
            get
            {
                if ( ViewState[ "LastTotalRows" ] == null )
                    ViewState[ "LastTotalRows" ] = 0;

                if ( IsInteger( ViewState[ "LastTotalRows" ].ToString() ) )
                    return Int32.Parse( ViewState[ "LastTotalRows" ].ToString() );
                else
                    return 0;
            }
            set { ViewState[ "LastTotalRows" ] = value; }
        }//
        protected DocumentVersion CurrentDocument
        {
            get { return ViewState[ "CurrentDocument" ] as DocumentVersion; }
            set { ViewState[ "CurrentDocument" ] = value; }
        }
        #endregion

        /// <summary>
        /// Handle Page Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void Page_Load( object sender, EventArgs ex )
        {
            //get current user
            resController = new FileResourceController();

            //get form ResourceManager
            //get manager
            myManager = new MyManager();

            if ( Page.IsPostBack )
            {

            }
            else
            {
                this.InitializeForm();
            }
            if ( !Page.IsPostBack )
            {
                StreamReader input;

                input = new StreamReader( System.Web.HttpContext.Current.Server.MapPath( "/content/Content1.txt" ), System.Text.Encoding.ASCII );
                txtDescription.EditPanel.Content = input.ReadToEnd();
                input.Close();
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock( this, this.GetType(), "EditorResponse", "alert('Submitted:\\n\\n" + txtDescription.EditPanel.Content.Replace( "\"", "\\\"" ).Replace( "\n", "\\n" ).Replace( "\r", "" ).Replace( "'", "\\'" ) + "');", true );
            }

        }//

        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        private void InitializeForm()
        {
            //set grid defaults (variables are in base control)
            GridViewSortExpression = gridDefaultSortExpression;
            GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
            LastPageNumber = 0;

            CurrentFaqCategory = new DataItem();
            CurrentRecord = new AppItem();
            CurrentDocument = new DocumentVersion();

            //
            this.FormPrivileges = SecurityManager.GetGroupObjectPrivileges( this.WebUser, formSecurityName );

            //= Add attribute to btnDelete to allow client side confirm
            btnDelete.Attributes.Add( "onClick", "return confirmDelete(this);" );
            btnDeleteImage.Attributes.Add( "onClick", "return confirmDeleteImage(this);" );
            this.btnApprove.Attributes.Add( "onClick", "return confirmApprove(this);" );

            //handling setting of which action buttons are available to the current user
            this.btnNew.Enabled = false;
            this.btnSave.Enabled = false;
            this.btnDelete.Visible = false;
            this.btnSend.Visible = false;
            this.btnSendTest.Visible = false;

            if ( FormPrivileges.CanCreate() )
            {
                this.btnNew.Enabled = true;
                //assume if can create, then can update
                //- may be some case where can only update records that user owns
                btnSave.Enabled = true;

            }
            else if ( FormPrivileges.CanUpdate() )
            {
                btnSave.Enabled = true;
            }

            if ( FormPrivileges.CanDelete() )
            {
                this.btnDelete.Visible = true;
                this.btnDelete.Enabled = false;
            }

            //

            if ( FormPrivileges.WritePrivilege > ( int ) ILPathways.Business.EPrivilegeDepth.Region )
            {
                this.maxPicHeight.ReadOnly = false;
                this.maxPicWidth.ReadOnly = false;
            }


            //check for parameters included with page control
           // Posting pagePosting = CmsHttpContext.Current.Posting;
            //string pageControl = ""; // McmsHelper.GetPlaceHolder( pagePosting, "pageControl" );
            //check if there is a limitation to a particular app item type
            string typeLimit = ""; // UtilityManager.ExtractNameValue( pageControl, "AppItemType", ";" );
            if ( typeLimit.Length > 0 )
            {
                //future handle multiple in a comma separated list
                AppItemTypeRestriction = typeLimit;
                //this.SetListSelection( this.lstForm, typeLimit );
                //lstForm.Enabled = false;
            }

            // Set source for form lists
            this.PopulateControls();

            //check for an external request for a particular record
            CheckRecordRequest();
        }	// End 

        /// <summary>
        /// Check the request type for the form
        /// </summary>
        private void CheckRecordRequest()
        {

            // Check if a request was made for a specific record
            string id = this.GetRequestKeyValue( "id", "" ); ;

            if ( id.Length > 0 )
            {
                this.Get( id );
                //Hmmm if we have a specific record, should we assume only that type will be handled?
                if ( CurrentRecord.HasValidRowId() && LimitAppTypesOnExternalRequest.Text.Equals( "yes" ) )
                {
                    AppItemTypeRestriction = CurrentRecord.TypeId.ToString();
                    this.SetListSelection( this.lstForm, CurrentRecord.TypeId.ToString() );
                    lstForm.Enabled = false;

                    this.SetListSelection( this.ddlTypeId, CurrentRecord.TypeId.ToString() );
                    ddlTypeId.Enabled = false;

                    this.SetListSelection( this.ddlTypeId2, CurrentRecord.TypeId.ToString() );
                    ddlTypeId2.Enabled = false;
                }
            }
        }	// End 


        protected void Page_PreRender( object sender, EventArgs e )
        {
            //handle lost page index
            if ( pager1.CurrentIndex == 0 && LastPageNumber > 0 )
            {
                pager1.CurrentIndex = pager2.CurrentIndex = LastPageNumber;
                pager1.ItemCount = pager2.ItemCount = LastTotalRows;
            }

        }	// End 

        /// <summary>
        /// Handle selection from form list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void lstForm_SelectedIndexChanged( object sender, System.EventArgs ea )
        {

            try
            {
                int typeId = Int32.Parse( this.lstForm.SelectedValue );

                SetSearchCategoryList( typeId );
                SetSearchStatusList( typeId, "" );
                //this.Get( id );
                //DoSearch();

            }
            catch ( Exception ex )
            {
                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.Message );
                this.LogError( ex, thisClassName + ".lstForm_SelectedIndexChanged" );
            }
        } //
        protected void ddlSearchCategory_SelectedIndexChanged( object sender, System.EventArgs ea )
        {

            try
            {
                string category = this.ddlSearchCategory.SelectedValue;
                int typeId = 0;
                if ( lstForm.SelectedIndex > 0 )
                    typeId = Int32.Parse( this.lstForm.SelectedValue );

                SetSearchStatusList( typeId, category );
                //this.Get( id );
                //DoSearch();

            }
            catch ( Exception ex )
            {
                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.Message );
                this.LogError( ex, thisClassName + ".lstForm_SelectedIndexChanged" );
            }
        } //
        /// <summary>
        /// Get - retrieve form data
        /// </summary>
        /// <param name="recId"></param>
        private void Get( string recId )
        {
            try
            {
                //get record
                AppItem entity = myManager.Get( recId );

                if ( entity == null || entity.RowId.ToString().Length == 0 )
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
                this.ResetForm();
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );

            }

        }	// End method


        /// <summary>
        /// Populate the form 
        ///</summary>
        private void PopulateForm( AppItem entity )
        {

            //check for a change in type
            if ( CurrentRecord.TypeId != entity.TypeId )
            {
                SetCategoryList( entity.TypeId );
                SetStatusList( entity.TypeId );
            }

            //string statusMessage = "";

            detailsPanel.Visible = true;
            TabContainer1.ActiveTabIndex = 2;
            ddlCategory.AutoPostBack = true;
            txtNewCategory.Visible = true;
            hlDisplayStory.Visible = false;
            faqSubcategoryPanel.Visible = false;
            this.subcategoryPanel.Visible = true;

            rfvDescription.Enabled = true;
            this.txtSubcategoryDesc.Text = "";
            rfvSubcatFaqDesc.Enabled = false;
            this.lblServerDocumentLink.Text = "";
            this.lblServerDocumentLink.Visible = false;
            this.currentFileName.Text = "";

            tab3.Visible = true;
            int typeId = 0;

            //handle wierdness on get after update
            if ( ddlTypeId.SelectedIndex > 0 )
            {
                typeId = int.Parse( this.ddlTypeId.SelectedValue.ToString() );
            }
            if ( typeId == entity.TypeId )
            {
                ddlTypeId.Enabled = false;
            }
            else
            {
                //ddlTypeId.Enabled = true;
                this.SetListSelection( this.ddlTypeId, "id", entity.TypeId.ToString() );
                ddlTypeId.Enabled = false;
            }

            if ( entity.TypeId == AppItem.AnnouncementItemType )
            {
                if ( entity.String2 == null || entity.String2 == "" )
                    entity.String2 = "0";

                maxPicWidth.Text = this.annItemPicWidth.Text;
                maxPicHeight.Text = this.annItemPicHeight.Text;

            }
            else if ( entity.TypeId == AppItem.NewsItemType )		// **News **
            {
                txtNewCategory.Visible = false;
                this.subcategoryPanel.Visible = false;
                ddlCategory.AutoPostBack = false;
            }

            txtString3.Text = entity.String3;

            if ( entity.TypeId == AppItem.FAQItemType )		//==== FAQ ===============
            {
                faqSubcategoryPanel.Visible = true;
                rfvSubcatFaqDesc.Enabled = true;
                this.txtSubcategoryDesc.Text = entity.FaqSubcategory.Description;
                ddlCategory.AutoPostBack = true;
                ddlSubcategory.AutoPostBack = true;
                //set current faq to match contents of record
                CurrentFaqCategory.Id = entity.FaqSubcategory.Id;
                CurrentFaqCategory.param1 = entity.FaqSubcategory.param1;
                CurrentFaqCategory.Title = entity.FaqSubcategory.Title;
                CurrentFaqCategory.Description = entity.FaqSubcategory.Description;
                //set as not changed
                CurrentFaqCategory.HasChanged = false;

                //handle subcat list
                if ( entity.Category.Length > 0 )
                {
                    //don't want to do this every postback though
                    SetFaqSubcategoryList( entity.Category );
                }
            }
            else
            {
                if ( entity.Category.Length > 0 )
                    SetSubcategoryList( CurrentRecord.TypeId, entity.Category );
            }

            txtNewCategory.Text = "";
            txtNewSubcategory.Text = "";
            txtNewStatus.Text = "";

            if ( entity.RowId.ToString() == "00000000-0000-0000-0000-000000000000" )
                this.txtRowId.Text = "";
            else
                this.txtRowId.Text = entity.RowId.ToString();

            this.txtVersionNbr.Text = entity.VersionNbr.ToString();
            this.txtSequenceNbr.Text = entity.SequenceNbr.ToString();

            this.txtTitle.Text = entity.Title;

            this.txtDescription.EditPanel.Content = entity.Description;
            lblDescText.Text = entity.Description;
            this.txtAppItemCode.Text = entity.AppItemCode;

            //if (entity.ParentRowId.ToString().Length > 0)
            //this.SetListSelection( this.ddlParentRowId, entity.ParentRowId.ToString() );

            startDatePanel.Visible = false;
            approvalPanel.Visible = false;
            rfvStartDate.Enabled = false;

            //ddlTypeId.SelectedIndex = -1;
            if ( entity.HasValidRowId() )
            {
                if ( AppItemTypeRestriction.Length > 0 )
                {
                    //lock
                    //OK if the list has already been limited
                    //ddlTypeId.Enabled = false;
                }
                this.SetListSelection( this.ddlCategory, entity.Category );

                this.SetListSelection( this.ddlSubcategory, entity.Subcategory );

                this.SetListSelection( this.ddlStatus, entity.Status );

                this.SetFieldLockedStatus( txtAppItemCode, true );
                //show image if applicable
                if ( itemsNOTUsingImages.Text.IndexOf( entity.TypeId.ToString() ) > -1 )
                    imagePanel.Visible = false;
                else
                    imagePanel.Visible = true;

                //show approval if applicable
                if ( itemsRequiringApproval.Text.IndexOf( entity.TypeId.ToString() ) > -1 )
                {
                    approvalPanel.Visible = true;
                    string approval = "";
                    if ( entity.Approved > System.DateTime.MinValue )
                    {
                        approval = entity.Approved.ToString();
                    }
                    if ( entity.ApprovedBy.Length > 0 )
                        approval += " - " + entity.ApprovedBy;

                    this.txtApproved.Text = approval;
                    if ( entity.Status != "Published" )
                    {
                        btnApprove.Visible = true;
                        if ( FormPrivileges.CanApprove() )
                        {
                            //show approve button
                            btnApprove.Enabled = true;
                            btnApprove.Text = "Approve";
                            btnApprove.CssClass = "approveButton";
                            //btnApprove.Width = 150;
                            //btnApprove.BackColor = System.Drawing.Color.Green;
                            //btnApprove.ForeColor = System.Drawing.Color.White;
                        }
                        else
                        {
                            //show as disabled - to clarify
                            btnApprove.Enabled = false;
                            btnApprove.Text = "Approve disabled";
                            btnApprove.Width = 200;
                            btnApprove.BackColor = System.Drawing.Color.WhiteSmoke;
                        }
                    }
                    else
                    {
                        btnApprove.Enabled = false;
                        btnApprove.Text = "Unapprove (future use)";
                        btnApprove.CssClass = "unapproveButton";
                        //btnApprove.Width = 200;
                        //btnApprove.BackColor = System.Drawing.Color.WhiteSmoke;
                    }

                }

                //only show dates where applicable
                if ( itemsAllowingStartEndDates.Text.IndexOf( entity.TypeId.ToString() ) > -1 )
                {
                    startDatePanel.Visible = true;
                    rfvStartDate.Enabled = true;

                    if ( entity.StartDate > System.DateTime.MinValue )
                        this.txtStartDate.Text = entity.StartDate.ToString();

                    if ( entity.EndDate > System.DateTime.MinValue )
                        this.txtEndDate.Text = entity.EndDate.ToString();

                    if ( entity.ExpiryDate > System.DateTime.MinValue )
                        this.txtExpiryDate.Text = entity.ExpiryDate.ToString();
                }

            }
            else
            {
                //reset
                if ( AppItemTypeRestriction.Length > 0 )
                {
                    //lock
                    //OK if the list has already been limited
                    //this.SetListSelection( this.ddlTypeId, AppItemTypeRestriction );

                    //ddlTypeId.Enabled = false;
                }
                //reset controls or state for empty object
                this.SetFieldLockedStatus( txtAppItemCode, false );

                ddlCategory.SelectedIndex = -1;
                ddlSubcategory.SelectedIndex = -1;
                ddlStatus.SelectedIndex = -1;
                this.txtStartDate.Text = "";
                this.txtEndDate.Text = "";
                this.txtExpiryDate.Text = "";
                btnApprove.Visible = false;
            }

            this.rblIsActive.SelectedValue = this.ConvertBoolToYesNo( entity.IsActive );


            //check for an image ==========================================
            if ( entity.ImageId > 0 )
            {
                ////image details should be in the entity, but check just in case
                //if ( entity.AppItemImage == null || entity.AppItemImage.Id == 0 )
                //{
                //    entity.AppItemImage = ImageStoreManager.Get( entity.ImageId );
                //    CurrentRecord = entity;
                //}
                //populate image stuff
                //future: add a comment field to ImageStore to store image info including height and width
                lblImageId.Text = entity.AppItemImage.Id.ToString();
                this.txtImageFileName.Text = entity.AppItemImage.ImageFileName;

                imgCurrent.Visible = true;
                imgCurrent.AlternateText = entity.AppItemImage.Title;
                txtImageTitle.Text = entity.AppItemImage.Title;

                imgCurrent.ImageUrl = showPictureUrl.Text + "&id=" + entity.AppItemImage.Id.ToString();
                previewImageLink.NavigateUrl = showPictureUrl.Text + "&id=" + entity.AppItemImage.Id.ToString();
                previewImageLink.Visible = true;
                this.btnDeleteImage.Enabled = true;
            }
            else
            {
                imgCurrent.Visible = false;
                lblImageId.Text = "";
                txtImageTitle.Text = "";
                txtImageFileName.Text = "";
                previewImageLink.Visible = false;
                this.btnDeleteImage.Enabled = false;
            }

            //check for a document ==========================================
            if ( entity.DocumentRowId != null && !entity.IsInitialGuid( entity.DocumentRowId ) )
            {
                HandleDocumentPopulate( entity );

                //this.btnDeleteDocument.Enabled = true;
            }
            else
            {
                lblDocumentLink.Visible = false;
                txtDocumentRowId.Text = "";
                txtFileTitle.Text = "";
                //this.btnDeleteDocument.Enabled = false;
            }

            if ( entity.Title.Length > 0 )
            {
                //this.lblHistory.Text = entity.HistoryTitle();
                this.lblHistory.Text = "Created: " + entity.CreatedByTitle();
                if ( entity.CreatedById > 0 )
                {
                    //AppUser creator = UserManager.get.GetUser( entity.CreatedById, ref statusMessage );
                    //lblHistory.ToolTip = creator.SummaryAsHtml();
                    //lblCreatedHistoryDetails.Text = creator.SummaryAsHtml();
                }
                this.lblUpdatedHistory.Text = "Last Updated: " + entity.UpdatedByTitle();
                if ( entity.LastUpdatedById > 0 )
                {
                    //AppUser updUser = UserManager.GetUser( entity.LastUpdatedById, ref statusMessage );
                    //lblUpdatedHistoryDetails.Text = updUser.SummaryAsHtml();
                }

                if ( FormPrivileges.CanDelete() )
                {
                    btnDelete.Enabled = true;
                }
                if ( ( FormPrivileges.CanUpdate() || FormPrivileges.CanCreate() )
                          && ( IsEmailableAnnouncement( entity ) || WebUser.TopAuthorization < 4 ) )
                {
                    btnSend.Visible = false;
                    btnSendTest.Visible = true;
                }
                else
                {
                    btnSend.Visible = false;
                    btnSendTest.Visible = false;
                }
            }
            else
            {
                //reset controls or state for empty object
                //ex: reset a dropdownlist not handled by 
                this.lblHistory.Text = "";
                lblCreatedHistoryDetails.Text = "";
                lblUpdatedHistory.Text = "";
                lblUpdatedHistoryDetails.Text = "";

                btnDelete.Enabled = false;
                btnSend.Visible = false;
                btnSendTest.Visible = false;
            }

            // ensure CurrentRecord has all updates to entity
            CurrentRecord = entity;
        }//

        private bool IsEmailableAnnouncement( AppItem entity )
        {
            bool retVal = false;
            //string filter = "Category = '" + DatabaseManager.HandleApostrophes( entity.Category ) + "'";
            //string message = "successful";
            //DataSet ds = new AnnouncementEmailManager().Select( filter, ref message );
            //if ( entity.Status.ToLower() == "published" && DoesDataSetHaveRows( ds ) )
            //{
            //    retVal = true;
            //}

            return retVal;
        }//

        private void HandleDocumentPopulate( AppItem entity )
        {

            //document details should be in the entity, but check just in case
            //if ( entity.RelatedDocument == null || entity.RelatedDocument.HasInitialRowId() )
            //{
            //    DocumentVersion doc = DocumentStoreManager.Get( entity.DocumentRowId );
            //    entity.RelatedDocument = doc;

            //}

            ////??? not sure if should use?
            //CurrentDocument = entity.RelatedDocument;
            //string url = CacheDocumentOnServer( entity, entity.RelatedDocument );

            ////populate document stuff

            //this.txtDocumentRowId.Text = entity.RelatedDocument.RowId.ToString();
            //this.txtFileTitle.Text = entity.RelatedDocument.Title;
            //lblDocumentLink.Text = "View (from database): " + entity.RelatedDocument.FileName;
            //lblDocumentLink.NavigateUrl = showDocumentUrl.Text + entity.RelatedDocument.RowId.ToString();
            //lblDocumentLink.Visible = true;

        }//
        /// <summary>
        /// probably want to make generic for reuse
        /// </summary>
        /// <param name="version"></param>
        /// <param name="doc"></param>
        private string CacheDocumentOnServer( AppItem version, DocumentVersion doc )
        {
            string url = "";
            string filePath = "";
            string fileUrl = "";
            try
            {
                //file path will differ for a success letter
                if ( doc.FileName.Length > 0 )
                {
                    
                    filePath = FileSystemHelper.GetCacheOutputPath( doc.RowId.ToString() );
                    fileUrl = FileSystemHelper.GetCacheOutputUrl( doc.RowId.ToString() );
                    
                    url = FileSystemHelper.HandleDocumentCaching( filePath, doc.FileName, fileUrl, doc.RowId.ToString() );

                    lblServerDocumentLink.NavigateUrl = url;
                    lblServerDocumentLink.Text = "View (from server): " + doc.FileName;
                    lblServerDocumentLink.Visible = true;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".CacheDocumentOnServer() - Unexpected error encountered while retrieving document" );

                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }
            //
            return url;
        }//

        private void HandleStoryTargetPopulate( AppItem entity )
        {
           
        }//
        protected void FormatLogonAs()
        {

        }//
        protected void Category_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            try
            {
                if ( ddlCategory.SelectedIndex > 0 )
                {
                    string category = this.ddlCategory.SelectedValue;
                    //fill subcategories for selected category
                    if ( CurrentRecord.TypeId == AppItem.FAQItemType )
                    {
                        SetFaqSubcategoryList( category );
                        //if changing category, then reset subcategory description 
                        this.txtSubcategoryDesc.Text = "";
                    }
                    else
                    {
                        SetSubcategoryList( CurrentRecord.TypeId, category );
                    }

                }
            }
            catch ( Exception ex )
            {
                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.Message );
                this.LogError( ex, thisClassName + ".Category_OnSelectedIndexChanged" );
            }
        }

        /// <summary>
        /// Currently only for an FAQ. If subcategory changes, need to get the related description 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Subcategory_OnSelectedIndexChanged( object sender, EventArgs e )
        {
           
        }

        /// <summary>
        /// Handle a form button clicked 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        public void FormButton_Click( Object sender, CommandEventArgs ex )
        {

            switch ( ex.CommandName )
            {
                case "New":
                    this.HandleNewRequest();
                    break;
                case "Initialize":
                    this.HandleNewRequest();
                    break;
                case "Update":
                    if ( this.IsFormValid() )
                    {
                        this.UpdateForm( false );
                    }
                    break;
                case "Approve":
                    if ( this.IsFormValid() )
                    {
                        this.UpdateForm( true );
                    }
                    break;

                case "BadWordCheck":
                    DoBadWordsCheck();
                    break;
                case "Delete":
                    this.DeleteRecord();
                    break;
                case "Search":
                    if ( this.IsSearchValid() )
                    {
                        this.DoSearch();
                    }
                    break;
                case "DeleteImage":
                    this.DeleteImage();
                    break;

                case "SendTest":
                    this.SendTest();
                    break;
                case "Send":
                    this.SendNewsEmails();
                    break;
            }
        } // end 
        protected void SwitchUser( string userName )
        {


        }
        /// <summary>
        /// Check for bad words
        /// </summary>
        private void DoBadWordsCheck()
        {
            bool isValid = true;
            string comments = this.txtDescription.EditPanel.Content.Trim().ToString();

            // Check for bad words      
            if ( BadWordChecker.CheckForBadWords( comments ) )
            {
                isValid = false;

                //highlight badwords if found
                string badWords = BadWordChecker.Highlight( comments );
                //txtbxComment.Text = badWords;
                txtDescription.EditPanel.Content = badWords;

                SetConsoleErrorMessage( "Bad words were found and replaced by #$!@. Please remove and try again!" );

            }

        } // end 

        /// <summary>
        /// Prepare for a new record request
        /// </summary>
        private void HandleNewRequest()
        {
            tab2.Visible = true;
            tab3.Visible = true;
            TabContainer1.ActiveTabIndex = 2;

            int typeId = int.Parse( this.ddlTypeId2.SelectedValue.ToString() );
            if ( typeId > 0 )
            {
                this.ResetForm();
            }
            else
            {
                SetConsoleErrorMessage( "You must select the application item type for a new record." );
            }
        } // end 

        #region Validation Methods
        /// <summary>
        /// validate form content
        /// </summary>
        /// <returns></returns>
        private bool IsFormValid()
        {
           // int recId = 0;
            bool isValid = true;

            try
            {

                vsErrorSummary.Controls.Clear();

                Page.Validate();
                //handle links formatting
                int typeId = int.Parse( this.ddlTypeId.SelectedValue.ToString() );
                if ( typeId != AppItem.NewsItemType )
                {
                    this.txtDescription.EditPanel.Content = ContentHelper.FormatLinks(txtDescription.EditPanel.Content, "img", true );
                }
                //other edits

                //recId = int.Parse( this.Id.Text );

                // check additional required fields
                isValid = HasRequiredFields();

                if ( isValid )
                {
                    isValid = ValidateImageFields();

                    if ( typeId == AppItem.AnnouncementItemType )
                    {
                        // ===========================================================================
                    }
                    else if ( typeId == AppItem.FAQItemType )
                    {
                        if ( this.ddlCategory.SelectedIndex < 1 )
                        {
                            this.AddReqValidatorError( this.vsErrorSummary, "A category must be selected for an FAQ item", "txtRelatedUrl", true );
                        }
                    }
                }//

                if ( isValid )
                {
                    isValid = ValidateDocumentFields();
                }
            }
            catch ( System.Exception ex )
            {
                //SetFormMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.ToString() );
                return false;
            }

            if ( vsErrorSummary.Controls.Count > 0 || isValid == false )
            {
                return false;
            }
            else
            {
                return true;
            }
        } //

        /// <summary>
        /// verify required fields are present
        /// </summary>
        /// <returns></returns>
        private bool HasRequiredFields()
        {
            // check additional required fields


            if ( ddlTypeId.SelectedIndex < 1 )
            {
                this.AddReqValidatorError( this.vsErrorSummary, "An Item Type is required", "txtTitle", true );
                ddlTypeId.Enabled = true;
            }

            if ( ddlCategory.SelectedIndex < 1 && this.txtNewCategory.Text.Trim().Length == 0 )
            {
                this.AddReqValidatorError( this.vsErrorSummary, "A Category is required", "txtTitle", true );
            }
            if ( ddlStatus.SelectedIndex < 1 && this.txtNewStatus.Text.Trim().Length == 0 )
            {
                this.AddReqValidatorError( this.vsErrorSummary, "A Status is required", "txtTitle", true );
            }

            //if doing a check on a control that doesn't, have a validator defined, use the following (true results in message being added to the validation summary
            //if ( this.ddlUserOrg.SelectedIndex < 1 )
            //{
            //  this.AddReqValidatorError( vsErrorSummary, "Some error message", "ddlUserOrg", true );
            //}

            if ( vsErrorSummary.Controls.Count > 0 )
            {
                return false;
            }
            else
            {
                return true;
            }
        } //


        /// <summary>
        /// Image related validation
        /// </summary>
        /// <returns></returns>
        private bool ValidateImageFields()
        {
            //if no existing file or new file, ignore other fields
            if ( !IsImageFilePresent() )
                return true;

            if ( !IsInteger( maxPicWidth.Text ) )
            {
                this.AddReqValidatorError( this.vsErrorSummary, "The maximum picture width must be provided", "txtImageTitle", true );

            }
            if ( !IsInteger( maxPicHeight.Text ) )
            {
                this.AddReqValidatorError( this.vsErrorSummary, "The maximum picture height must be provided", "txtImageTitle", true );

            }
            if ( vsErrorSummary.Controls.Count > 0 )
            {
                return false;
            }

            int maxWidth = Int32.Parse( maxPicWidth.Text );
            int maxHeight = Int32.Parse( maxPicHeight.Text );

            if ( txtImageTitle.Text.Trim().Length == 0 )
            {
                this.AddReqValidatorError( this.vsErrorSummary, "A title must be provided for the image", "txtImageTitle", false );
            }

            if ( !resController.DoesImageExist( imageFileUpload ) )
            //if ( !DoesImageExist() )
            {
                this.AddReqValidatorError( this.vsErrorSummary, "The image path is invalid. Please enter a valid path or use the browse button to search for and select an image.", "imageFileUpload", true );

            }
            else if ( doingSizeCheck.Text.Equals( "yes" )
                           && resController.IsImageSizeValid( imageFileUpload, maxWidth, maxHeight ) == false )
            {
                this.AddReqValidatorError( this.vsErrorSummary, "The selected Image is too wide. Select an image no wider than <a href='#sampleImageSize'>the sample blue box</a>.", "imageFileUpload", false );
            }

            if ( !resController.IsAllowedImageMimeType( imageFileUpload.FileName ) )
            {
                this.AddReqValidatorError( this.vsErrorSummary, cvlImagePathMime.ErrorMessage, "imageFileUpload", false );
            }

            if ( vsErrorSummary.Controls.Count > 0 )
            {
                return false;
            }
            else
            {
                return true;
            }
        } //

        /// <summary>
        /// validate that the file mime type is valid for this process
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        protected void cvlImagePathMime_ServerValidate( object source, ServerValidateEventArgs args )
        {
            bool isValid = true;

            if ( IsImageFilePresent() )
            {
                isValid = resController.IsAllowedImageMimeType( imageFileUpload.FileName ); ;
            }

            args.IsValid = isValid;
        }

        /// <summary>
        /// return true if a file has been entered or selected
        /// </summary>
        protected bool IsImageFilePresent()
        {
            bool isPresent = false;

            if ( imageFileUpload.HasFile || imageFileUpload.FileName != "" )
            {
                isPresent = true;

            }

            return isPresent;
        }//

        private bool ValidateDocumentFields()
        {
            bool isValid = true;
            //if a file is present, then title is required
            if ( IsFilePresent() )
            {
                if ( this.txtFileTitle.Text.Trim().Length == 0 )
                {
                    SetConsoleErrorMessage( "Error - please provide a meaningful title for the document being uploaded." );
                    isValid = false;
                }
            }
            else
            {
                //check for title but no file 
                if ( this.txtFileTitle.Text.Trim().Length > 0 && this.currentFileName.Text.Length == 0 )
                {
                    SetConsoleErrorMessage( "Error - a document title was entered, but there is no document file to upload. Please provide a document or clear out the document title field." );
                    isValid = false;
                }
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
            //future, check if already uploaded (and saved to temp location

            return isPresent;
        }//
        #endregion

        /// <summary>
        /// Handle form update
        /// </summary>
        private void UpdateForm( bool doingApproval )
        {
            string id = "";
            //string msg = "";
            string action = "";
            string statusMessage = "";
            string imageStatus = "";

            AppItem entity = new AppItem();

            try
            {
                id = this.txtRowId.Text;

                if ( id.Length == 0 )
                {

                    entity.CreatedBy = WebUser.UserName;
                    entity.CreatedById = WebUser.Id;		//include for future use
                    entity.Created = System.DateTime.Now;
                    action = "Create";
                    // anything else??
                    entity.FaqSubcategory = new DataItem();
                }
                else
                {
                    // get current record from viewstate
                    entity = CurrentRecord;
                    action = "Update";
                }
                //check if image needs to be handled
                if ( IsImageFilePresent() || txtImageTitle.Text.Length > 0 )
                {
                    //could be new or an update or just a title update
                    //if ( entity.AppItemImage == null )

                    //successful = HandleImageUpdate( ref statusMessage );
                }

                /* assign form fields 			 */
                entity.VersionNbr = Int32.Parse( txtVersionNbr.Text );
                entity.SequenceNbr = Int32.Parse( txtSequenceNbr.Text );

                entity.Title = FormHelper.CleanText( this.txtTitle.Text );
                entity.TypeId = int.Parse( this.ddlTypeId.SelectedValue.ToString() );


                entity.Description = this.txtDescription.EditPanel.Content;

                if ( txtAppItemCode.Text.Trim() == "" || txtAppItemCode.Text.ToLower() == "default" )
                    entity.AppItemCode = System.Guid.NewGuid().ToString();
                else
                    entity.AppItemCode = FormHelper.CleanText( this.txtAppItemCode.Text );

                if ( this.txtNewCategory.Text.Length > 0 )
                    entity.Category = FormHelper.CleanText( txtNewCategory.Text );
                else if ( ddlCategory.SelectedIndex > 0 )
                    entity.Category = this.ddlCategory.SelectedValue.ToString();
                else
                    entity.Category = "";

                if ( this.txtNewSubcategory.Text.Length > 0 )
                    entity.Subcategory = FormHelper.CleanText( txtNewSubcategory.Text );
                else if ( ddlSubcategory.SelectedIndex > 0 )
                    entity.Subcategory = this.ddlSubcategory.SelectedValue.ToString();
                else
                    entity.Subcategory = "";

                if ( this.txtNewStatus.Text.Length > 0 )
                    entity.Status = FormHelper.CleanText( txtNewStatus.Text );
                else if ( ddlStatus.SelectedIndex > 0 )
                    entity.Status = this.ddlStatus.SelectedValue.ToString();
                else
                    entity.Status = "";
                //

           
                entity.String3 = FormHelper.CleanText( txtString3.Text );
                //
                entity.IsActive = this.ConvertYesNoToBool( this.rblIsActive.SelectedValue );

                if ( doingApproval )
                {
                    action = "Approval";
                    entity.Approved = System.DateTime.Now;
                    entity.ApprovedById = WebUser.Id;
                    entity.ApprovedBy = WebUser.FullName();
                    entity.Status = "Published";
                    entity.IsActive = true;
                }

                //only show dates where applicable
                if ( itemsAllowingStartEndDates.Text.IndexOf( entity.TypeId.ToString() ) > -1 )
                {
                    if ( txtStartDate.Text.Length > 0 )
                        entity.StartDate = System.DateTime.Parse( txtStartDate.Text );
                    if ( txtEndDate.Text.Length > 0 )
                        entity.EndDate = System.DateTime.Parse( txtEndDate.Text );
                    if ( txtExpiryDate.Text.Length > 0 )
                        entity.ExpiryDate = System.DateTime.Parse( txtExpiryDate.Text );
                }
                /*
                        entity.ParentRowId = this.ddlParentRowId.Text;

                */

                entity.LastUpdated = System.DateTime.Now;
                entity.LastUpdatedBy = WebUser.UserName;
                entity.LastUpdatedById = WebUser.Id;		//include for future use

                // ========== document ============================================================
                //TODO - will need to detect whether the file title has changed 
                // check if new and file exists
                bool isDocSaveValid = HandleDocument( entity, ref statusMessage );
                if ( isDocSaveValid )
                {
                    //the followng should have been set in HandleDocument
                    string check = entity.DocumentRowId.ToString();
                }
                else
                {
                    //problem, continue????
                    SetConsoleErrorMessage( "Error: unexpected condition encountered while storing the document in the database:<br/>" + statusMessage );

                }

                // If status is published and the approved date has never been set, set it.  This has the effect of setting an approved date
                // on any published item that does not already have an approved date.  Note: An uninitialized (read: NULL) DateTime from the
                // database results in a DateTime in C# of '0001-01-01 00:00:00' which is why the dual test for null and for dates < 2000-01-01.
                if ( entity.Status.ToLower() == "published" && ( entity.Approved == null || entity.Approved < new DateTime( 2000, 1, 1 ) ) )
                {
                    entity.Approved = DateTime.Now;
                }

                //call insert/update
                string entityId = "";
                if ( id.Length == 0 )
                {
                    entityId = myManager.Create( entity, ref statusMessage );
                    if ( entityId.Length > 0 )
                    {
                        this.txtRowId.Text = entityId;
                        entity.RowId = new Guid( entityId );

                        //probably don't want to insert image until entity is properly created??
                        imageStatus = HandleImageUpdate( entity );
                    }

                }
                else
                {
                    statusMessage = myManager.Update( entity );
                    if ( statusMessage.ToLower().Equals( "successful" ) )
                        imageStatus = HandleImageUpdate( entity );
                }


                if ( statusMessage.Equals( "successful" ) )
                {
                    //check dropdowns need to be refreshed
                    if ( this.txtNewCategory.Text.Length > 0 )
                        SetCategoryList( entity.TypeId );

                    if ( this.txtNewSubcategory.Text.Length > 0 )
                        SetSubcategoryList( entity.TypeId, "" );

                    if ( this.txtNewStatus.Text.Length > 0 )
                        SetStatusList( entity.TypeId );

                    //update CurrentRecord and form
                    Get( entity.RowId.ToString() );
                    //PopulateForm( entity );

                    this.SetConsoleSuccessMessage( action + " was successful for: " + entity.Title + imageStatus );

                }
                else
                {
                    this.SetConsoleErrorMessage( statusMessage );
                }
                //reset pager
                pager1.CurrentIndex = pager2.CurrentIndex = LastPageNumber;
                pager1.ItemCount = pager2.ItemCount = LastTotalRows;

            }
            catch ( System.Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".UpdateForm() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }
        }

        protected string FormatMessage( AppItem item, bool isTest )
        {
            //string status = "";
            string message = "";
            //DataSet ds = new AnnouncementEmailManager().Select( "Category = '" + BDM.HandleApostrophes( item.Category ) + "'", ref status );
            //if ( DoesDataSetHaveRows( ds ) )
            //{
            //    DataRow dr = ds.Tables[ 0 ].Rows[ 0 ];
            //    message = MyManager.GetRowColumn( dr, "Template", "" );
            //    string displayUrl = MyManager.GetRowColumn( dr, "DisplayUrl", "" );
            //    displayUrl = displayUrl.Replace( "@RowId", item.RowId.ToString() );

            //    string article = string.Format( "<h1><a href='{0}'>{1}</a></h1>{2}", displayUrl, item.Title, item.Description );

            //    message = message.Replace( "@Items", article );
            //    message = message.Replace( "@ItemDescriptions", article );

            //    string relativeUrl = MyManager.GetRowColumn( dr, "UnsubscribeUrl", "" );
            //    string host = Request.Url.Host;
            //    string absoluteUrl = UtilityManager.FormatAbsoluteUrl( relativeUrl, host, false );
            //    AppItemAnnouncementSubscription subscription = new AppItemAnnouncementSubscription();
            //    if ( isTest )
            //    {
            //        subscription = new AppItemAnnouncementSubscriptionManager().Get( item.Category, 0, this.WebUser.Email );
            //        absoluteUrl = absoluteUrl.Replace( "@Option3", subscription.RowId.ToString() );
            //    }


            //    //string url = string.Format( txtUnsubscribeUrl.Text, absoluteUrl );
            //    message = message.Replace( "@UnsubscribeUrl", string.Format( "<a href=\"{0}\">Unsubscribe from {1}</a>", absoluteUrl, MyManager.GetRowColumn( dr, "Title", "" ) ) );


            //    relativeUrl = MyManager.GetRowColumn( dr, "ConfirmUrl", "" ) + "?RowId=@Option3";
            //    host = Request.Url.Host;
            //    absoluteUrl = UtilityManager.FormatAbsoluteUrl( relativeUrl, host, false );
            //    if ( isTest )
            //    {
            //        absoluteUrl = absoluteUrl.Replace( "@Option3", subscription.RowId.ToString() );
            //    }
            //    message = message.Replace( "@PreferencesUrl", string.Format( "<a href=\"{0}\">Manage Preferences</a>", absoluteUrl ) );

            //    // message = message.Replace("@UnsubscribeUrl", MyManager.GetRowColumn(dr, "UnsubscribeUrl", ""));
            //}
            //else
            //{
            //    message = "No rows were returned in the dataset";
            //    //just send body. Future: check for images, etc.
            //    string host = HttpContext.Current.Request.ServerVariables[ "HTTP_HOST" ];
            //    message = item.Summary( host );
            //}
            return message;
        }

        protected void SendNewsEmails()
        {
            //string status = "";
            AppItem entity = CurrentRecord;
            SqlQuery query = SqlQueryManager.Get( "ImmediateNewsSubscribers2" );
            if ( query.IsValid && query.Id > 0 )
            {
                //EmailNoticeJob job = new EmailNoticeJob();
                //job.NoticeCode = "";
                //job.SqlId = 0;
                //job.Status = "Waiting";
                //job.Sql = query.SQL.Replace( "[Category]", entity.Category );
                //job.Subject = entity.Title;

                //DataSet ds = new AnnouncementEmailManager().Select( "Category = '" + entity.Category + "'", ref status );

                //if ( DoesDataSetHaveRows( ds ) )
                //{
                //    //format the AppItem to be sent by the queue
                //    job.Message = FormatMessage( entity, false );
                //    DataRow dr = ds.Tables[ 0 ].Rows[ 0 ];
                //    int result = utilityClasses.Managers.EmailManager.JobQueueEnqueue( job, IsTestEnv() );
                //    string msg = "";
                //    if ( job.JobClass == "I" )
                //    {
                //        msg = string.Format( "E-mails have been sent ({0})", result );
                //    }
                //    else
                //    {
                //        msg = string.Format( "E-mail Job ({0}) has been queued.", result );
                //    }
                //    SetConsoleSuccessMessage( msg );
                //    LoggingHelper.DoTrace( 1, thisClassName + ".SendNewsEmails: " + msg );
                //}
                //else
                //{
                //    SetConsoleErrorMessage( "Could not find email template for " + entity.Category + ". No E-mail sent." );
                //}
            }
            else
            {
                SetConsoleErrorMessage( "Error: Missing the required SQL entry (ImmediateNewsSubscribers2) required to submit this request." );
            }
        }

        protected void SendTest()
        {
            try
            {
                AppItem entity = CurrentRecord;
                EmailManager.SendTestEmail( this.WebUser, "info@illinoisworknet.com", entity.Title, FormatMessage( entity, true ) );
                SetConsoleSuccessMessage( "Test e-mail sent" );

                if ( IsEmailableAnnouncement( entity ) )
                    btnSend.Visible = true;

            }
            catch ( Exception ex )
            {
                SetConsoleErrorMessage( "Error sending test e-mail: " + ex.Message );
            }
        }
        /// <summary>
        /// Determine an FAQCategory needs to be created or updated
        /// </summary>
        /// <returns></returns>
        protected void HandleFaqCategoryPersistance( AppItem entity )
        {
            //need to detect if need to add or update an FaqCategory record
            //if 
          
        }//

        /// <summary>
        /// insert document into database 
        /// </summary>
        /// <returns></returns>
        protected bool HandleDocument( AppItem version, ref string statusMessage )
        {
            bool isValid = true;
            string documentRowId = "";

            string action = "";
            //!!! need to check if pertinent fields have all been cleared - if so then delete??
            //if just the title etc, changed, then maybe we don't want to do a full update (ie the image data)??
            //there may a request to update the title as well
            if ( !IsFilePresent() )
            {
                if ( this.txtFileTitle.Text.Trim().Length == 0 )
                {
                    // no file, skip - this only ok , if update
                    statusMessage = "no document";
                    return true;
                }
                else if ( this.CurrentDocument.HasValidRowId() && CurrentDocument.Title == txtFileTitle.Text )
                {
                    statusMessage = "no change to document title";
                    return true;
                }
            }

            DocumentVersion entity = new DocumentVersion();
            documentRowId = this.txtDocumentRowId.Text;

            if ( documentRowId.Length == 0 )
            {
                entity.RowId = new Guid();

                entity.CreatedById = WebUser.Id;
                entity.CreatedBy = WebUser.UserName;
                entity.Created = System.DateTime.Now;
                action = "Create";

            }
            else
            {
                //document alreay saved previously - check for changes
                entity = version.RelatedDocument;
                action = "Update";
            }

            entity.LastUpdatedById = WebUser.Id;
            entity.Title = FormHelper.CleanText( this.txtFileTitle.Text );

            if ( IsFilePresent() )
            {
                entity.MimeType = fileUpload.PostedFile.ContentType;
                //probably want to fix filename to standardize
                entity.FileName = fileUpload.FileName;
                entity.FileName = entity.FileName.Replace( " ", "_" );
                entity.FileDate = System.DateTime.Now;

                //default to pending (since saving immediately). 
                //will set to somthing else when whole record is stored
                //==> look into using a transaction!!!!
                entity.Status = "Pending";
                string sFileType = System.IO.Path.GetExtension( entity.FileName );
                sFileType = sFileType.ToLower();
                //don't have rowID yet!!
                entity.URL = FileSystemHelper.GetCacheOutputUrl() + entity.FileName;

                //string documentFolder = FileSystemHelper.GetCacheOutputPath();
                //utilityClasses.FileSystemHelper.CreateDirectory( documentFolder );
                //string diskFile = documentFolder + "\\" + entity.FileName;

                //fileUpload.SaveAs( diskFile );

                fileUpload.PostedFile.InputStream.Position = 0;
                Stream fs = fileUpload.PostedFile.InputStream;

                entity.ResourceBytes = fs.Length;
                byte[] data = new byte[ fs.Length ];
                fs.Read( data, 0, data.Length );
                fs.Close();
                fs.Dispose();
                entity.SetResourceData( entity.ResourceBytes, data );
            }

            this.currentFileName.Text = entity.FileName;
            if ( entity.HasInitialRowId() )
            {
                //documentRowId = DocumentStoreManager.Create( entity, ref statusMessage );
                //if ( documentRowId.Length > 10 )
                //{
                //    this.txtDocumentRowId.Text = documentRowId;
                //    entity.RowId = new Guid( documentRowId );
                //    version.DocumentRowId = entity.RowId;
                //    statusMessage = "Successfully saved document!";

                //}
                //else
                //{
                //    txtDocumentRowId.Text = "0";
                //    statusMessage = "Document save failed: " + statusMessage;
                //    isValid = false;
                //}
            }
            else
            {
                //statusMessage = DocumentStoreManager.Update( entity );
                //if ( statusMessage.Equals( "successful" ) )
                //{
                //    statusMessage = "Successfully updated document!";
                //}
                //else
                //{
                //    isValid = false;

                //}

            }
            version.RelatedDocument = entity;
            CurrentDocument = entity;
            CacheDocumentOnServer( version, entity );
            return isValid;

        }//


        /// <summary>
        /// insert image into database 
        /// </summary>
        /// <returns></returns>
        protected string HandleImageUpdate( AppItem thisItem )
        {
            bool isValid = true;
            int imageId = 0;
            string statusMessage = "";
            string returnMessage = "";
            string action = "";
            bool isUpdate;
            //!!! need to check if pertinent fields have all been cleared - if so then delete??
            //if just the title etc, changed, then maybe we don't want to do a full update (ie the image data)??
            //there may a request to update the title as well
            if ( !IsImageFilePresent() )
            {
                // no image, skip
                return "";
            }

            ImageStore entity = new ImageStore();

            if ( this.lblImageId.Text == "" )
                imageId = 0;
            else
                imageId = int.Parse( this.lblImageId.Text );

            if ( imageId == 0 )
            {
                isUpdate = false;
                entity.Id = 0;
                entity.CreatedBy = WebUser.UserName;
                entity.Created = System.DateTime.Now;
                action = "Create";

            }
            else
            {
                entity = CurrentRecord.AppItemImage;
                isUpdate = true;
                action = "Update";
            }

            entity.Title = FormHelper.CleanText( txtImageTitle.Text );

            //check if file was specified (versus updating text for existing file)
            if ( IsImageFilePresent() )
            {
                entity.MimeType = imageFileUpload.PostedFile.ContentType;
                entity.ImageFileName = imageFileUpload.FileName;

                int maxWidth = Int32.Parse( maxPicWidth.Text );
                int maxHeight = Int32.Parse( maxPicHeight.Text );

                ///call method to:
                ///- check if image is within allowed maximums
                ///- if not, resize image
                ///- entity will be updated with size and image stream data
				isValid = FileResourceController.HandleImageResizing( entity, imageFileUpload, maxWidth, maxHeight );
                // if not valid check message and return
                if ( !isValid )
                {
                    statusMessage = entity.Message;
                    return "Error on image save: " + statusMessage;
                }


            }

            if ( entity.Id == 0 )
            {
                //imageId = ImageStoreManager.Create( entity, ref statusMessage );
                //if ( imageId > 0 )
                //{
                //    this.lblImageId.Text = imageId.ToString();
                //    entity.Id = imageId;
                //    statusMessage = "Successfully saved Image!";

                //    imgCurrent.ImageUrl = "/vos_portal/showPicture.aspx?imgSrc=is&id=" + imageId.ToString();
                //    imgCurrent.AlternateText = entity.Title;
                //    imgCurrent.Visible = true;
                //}
                //else
                //{
                //    lblImageId.Text = "0";
                //    statusMessage = "Image save failed: " + statusMessage;
                //    imgCurrent.ImageUrl = "";
                //    isValid = false;
                //}
            }
            else
            {
                //statusMessage = ImageStoreManager.Update( entity );
                //if ( statusMessage.Equals( "successful" ) )
                //{
                //    imgCurrent.AlternateText = entity.Title;
                //    imgCurrent.Visible = true;

                //    statusMessage = "Successfully updated Image!";
                //}
                //else
                //{
                //    isValid = false;
                //    imgCurrent.ImageUrl = "";
                //}

            }
            if ( !isUpdate )
            {
                // New image, so update imageId in current record
                thisItem.ImageId = imageId;
                statusMessage = myManager.Update( thisItem );
            }

            return returnMessage;

        }//


        /// <summary>
        /// Delete a record
        /// </summary>
        private void DeleteRecord()
        {

            try
            {
                //string statusMessage = "";
                string rowId = this.txtRowId.Text;

                DeleteRecord( rowId );
            }
            catch ( Exception ex )
            {

            }
        }//


        /// <summary>
        /// Delete a record
        /// </summary>
        private void DeleteRecord( string id )
        {

            try
            {
                string statusMessage = "";

                if ( myManager.Delete( id, ref statusMessage ) )
                {
                    this.SetConsoleSuccessMessage( "Delete of record was successful" );

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
        }//

        /// <summary>
        /// Delete a Image
        /// - first remove from recor
        /// - then delete from database (for RI purposes)
        /// </summary>
        private void DeleteImage()
        {
            int imageId = 0;

            try
            {

                imageId = CurrentRecord.ImageId;
                //reset and update record
                CurrentRecord.ImageId = 0;
                string statusMessage = myManager.Update( CurrentRecord );

                if ( statusMessage.Equals( "successful" ) )
                {
                    //if ( ImageStoreManager.Delete( imageId, ref statusMessage ) )
                    //{
                    //    this.SetConsoleSuccessMessage( "Image was sucessfully deleted and removed from this record" );

                    //    //update CurrentRecord and form
                    //    //PopulateForm( CurrentRecord );
                    //    Get( CurrentRecord.RowId.ToString() );

                    //}
                    //else
                    //{
                    //    this.SetConsoleErrorMessage( statusMessage );
                    //}
                }
                else
                {
                    this.SetConsoleErrorMessage( statusMessage );
                }

            }
            catch ( System.Exception e )
            {
                LoggingHelper.LogError( e, thisClassName + ".DeleteImage() - Unexpected error encountered" );
                this.ResetForm();
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + e.ToString() );
            }
        }

        /// <summary>
        /// Reset form values
        /// </summary>
        private void ResetForm()
        {
            //reset form by populating with an empty business object
            faqSubcategoryPanel.Visible = false;
            this.subcategoryPanel.Visible = true;

            AppItem entity = new AppItem();
            CurrentDocument = new DocumentVersion();

            entity.FaqSubcategory = new DataItem();
            this.ddlSubcategory.Items.Clear();
            ddlCategory.AutoPostBack = true;

            //do any defaults. 
            entity.IsActive = true;
            entity.AppItemCode = "default";

            if ( ddlTypeId2.SelectedIndex > 0 )
            {
                entity.TypeId = int.Parse( this.ddlTypeId2.SelectedValue.ToString() );

                ddlSubcategory.AutoPostBack = false;
                if ( entity.TypeId > 0 )
                {
                    if ( entity.TypeId == AppItem.AnnouncementItemType )
                    {
                        entity.String2 = "0";

                    }
                    else if ( entity.TypeId == AppItem.FAQItemType )
                    {
                        this.ddlSubcategory.Items.Clear();
                    }
          
                }
            }
            PopulateForm( entity );

            //reset any controls or buttons not handled in the populate method

            //unselect forms list
            //lstForm.SelectedIndex = -1;
        }//

        #region form grid related methods
        /// <summary>
        /// Verify the search parameters are valid, or complete before continuing
        /// </summary>
        protected bool IsSearchValid()
        {
            bool isValid = true;

            return isValid;
        } //

        /// <summary>
        /// Conduct a search and populate the form grid
        /// </summary>
        private void DoSearch()
        {
            int selectedPageNbr = 0;
            string sortTerm = GetCurrentSortTerm();
            pager1.ItemCount = 0;
            pager2.ItemCount = 0;

            DoSearch( selectedPageNbr, sortTerm );
        } //

        /// <summary>
        /// Conduct a search while addressing current page nbr and a sort term
        /// </summary>
        /// <param name="selectedPageNbr"></param>
        /// <param name="sortTerm"></param>
        private void DoSearch( int selectedPageNbr, string sortTerm )
        {
            DataSet ds = null;
            ArrayList list = new ArrayList();
            //string recordLink = "";
            //string target = "";
            bool usingSearch = true;
            string filter = "";
            string booleanOperator = "AND";
            int pTotalRows = 0;

            if ( selectedPageNbr == 0 )
            {
                //with custom pager, need to start at 1
                selectedPageNbr = 1;
            }
            LastPageNumber = selectedPageNbr;
            pager1.CurrentIndex = pager2.CurrentIndex = selectedPageNbr;

            // Set the page size for the DataGrid control based on the selection
            CheckForPageSizeChange();

          //  string url = CmsHttpContext.Current.Posting.Url;

            //!! add specific search code

            try
            {
                int pTypeId = 0;
                string category = "";
                string status = "";
                string keyword = FormHelper.CleanText( this.txtKeyword.Text );

                if ( usingSearch )
                {
                    if ( lstForm.SelectedIndex > 0 )
                    {
                        pTypeId = int.Parse( this.lstForm.SelectedValue );
                        filter = BDM.FormatSearchItem( filter, "TypeId", pTypeId, booleanOperator );
                    }
                    else if ( AppItemTypeRestriction.Length > 0 )
                    {
                        if ( AppItemTypeRestriction.Length > 0 )
                            filter = BDM.FormatSearchItem( filter, "(TypeId in (" + AppItemTypeRestriction + ") )", booleanOperator );
                    }

                    if ( this.ddlSearchCategory.SelectedIndex > 0 )
                    {
                        filter += BDM.FormatSearchItem( filter, "Category", DatabaseManager.HandleApostrophes( this.ddlSearchCategory.SelectedValue.ToString() ), booleanOperator );
                    }
                    if ( ddlStatusSearch.SelectedIndex > 0 )
                    {
                        filter += BDM.FormatSearchItem( filter, "Status", this.ddlStatusSearch.SelectedValue.ToString(), booleanOperator );
                    }
                    if ( keyword.Length > 0 )
                    {
                        if ( keyword.IndexOf( "%" ) == -1 )
                            keyword = "%" + keyword + "%";
                        string where = " (AppItem.Title like '" + keyword + "' "
                                                + " OR AppItem.[Description] like '" + keyword + "' "
                                                + " OR AppItem.[AppItemCode] like '" + keyword + "') ";
                        filter += BaseDataManager.FormatSearchItem( filter, where, booleanOperator );
                    }
                    if ( IsTestEnv() )
                    {
                        SetConsoleInfoMessage( "sql:<br>" + filter );
                    }

                    ds = myManager.Search( filter, sortTerm, selectedPageNbr, pager1.PageSize, ref pTotalRows );
                    pager1.ItemCount = pager2.ItemCount = pTotalRows;
                }
                else
                {

                    if ( lstForm.SelectedIndex > 0 )
                    {
                        pTypeId = int.Parse( this.lstForm.SelectedValue );
                    }

                    if ( keyword.Length > 0 )
                    {
                        if ( keyword.IndexOf( "%" ) == -1 )
                            keyword = "%" + keyword + "%";
                    }

                    if ( this.ddlSearchCategory.SelectedIndex > 0 )
                    {
                        category = this.ddlSearchCategory.SelectedValue.ToString();
                    }

                    if ( ddlStatusSearch.SelectedIndex > 0 )
                    {
                        status = this.ddlStatusSearch.SelectedValue.ToString();
                    }


                    ds = myManager.Select( pTypeId, category, "", status, "", "", keyword );
                    pTotalRows = ds.Tables[ 0 ].Rows.Count;
                }
                LastTotalRows = pTotalRows;
                if ( ds == null || ds.Tables[ 0 ].Rows.Count < 1 )
                {
                    resultsPanel.Visible = false;
                    SetConsoleErrorMessage( "No records were found for the provided search criteria" );
                    TabContainer1.ActiveTabIndex = 0;
                    tab2.Visible = false;
                    ddlPageSizeList.Enabled = false;
                    pager1.Visible = false;
                    pager2.Visible = false;
                }
                else
                {
                    ddlPageSizeList.Enabled = true;

                    TabContainer1.ActiveTabIndex = 1;
                    tab2.Visible = true;
                    resultsPanel.Visible = true;
                    //searchPanel.Visible = false;
                    //populate the grid

                    DataTable dt;
                    dt = ds.Tables[ 0 ];
                    DataView dv = ( ( DataTable ) dt ).DefaultView;
                    if ( sortTerm.Length > 0 )
                        dv.Sort = sortTerm;

                    if ( pTotalRows > formGrid.PageSize )
                    {
                        //formGrid.PagerSettings.Visible = true;
                        pager1.Visible = true;
                        pager2.Visible = true;
                    }
                    else
                    {
                        pager1.Visible = false;
                        pager2.Visible = false;
                    }


                    //populate the grid
                    formGrid.DataSource = dv;
                    //formGrid.PageIndex = selectedPageNbr;
                    formGrid.DataBind();

                }
            }
            catch ( System.Exception ex )
            {
                //Action??		- display message and close form??	

                SetConsoleErrorMessage( "Unexpected error encountered<br>Close this form and try again.<br>" + ex.ToString() );
            }
        }	//

        protected void formGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {

            if ( e.Row.RowType == DataControlRowType.DataRow )
            {

                //add delete confirmation
                LinkButton dlb = ( LinkButton ) e.Row.FindControl( "deleteRowButton" );
                //check if user can delete
                if ( FormPrivileges.CanDelete() )
                {
                    string title = DataBinder.Eval( e.Row.DataItem, "title" ).ToString();
                    title = HandleJavaScriptApostrophes( title );

                    dlb.Attributes.Add( "onclick", "javascript:return " +
                            "confirm('Are you sure you want to delete this record: " + title + "')" );

                    //dlb.Attributes.Add( "onclick", "javascript:return " +
                    //    "confirm('Are you sure you want to delete this record: " +
                    //    DataBinder.Eval( e.Row.DataItem, "title" ) + "')" );
                }
                else
                {
                    dlb.Enabled = false;
                }
            }
        }//
        public static string HandleJavaScriptApostrophes( string strValue )
        {
            if ( strValue.IndexOf( "'" ) > 0 )
            {
                strValue = strValue.Replace( "'", "`" );
            }

            return strValue;
        }
        protected void formGrid_RowCommand( object sender, GridViewCommandEventArgs e )
        {
            try
            {
                if ( e.CommandName == "DeleteRow" )
                {
                    // get the ID of the clicked row
                    string rowId = e.CommandArgument.ToString();

                    // Delete the record 
                    DeleteRecord( rowId );

                }
                else if ( e.CommandName == "SelectRow" )
                {
                    pager1.CurrentIndex = pager2.CurrentIndex = LastPageNumber;
                    pager1.ItemCount = pager2.ItemCount = LastTotalRows;

                    // get the ID of the clicked row
                    string rowId = e.CommandArgument.ToString();

                    // show the record 
                    if ( rowId.Length > 0 )
                    {
                        this.Get( rowId );
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".formGrid_RowCommand() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }
        }

        /// <summary>
        /// Reset selected item on sort
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void formGrid_Sorted( object sender, GridViewSortEventArgs ex )
        {
            //clear selected index
            formGrid.SelectedIndex = -1;

        }	//

        /// <summary>
        /// Handle click on a grid header - that id change the sort order
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void formGrid_Sorting( object sender, GridViewSortEventArgs e )
        {
            string newSortExpression = e.SortExpression;
            string sortTerm = "";

            //check if the same field as previous sort
            if ( GridViewSortExpression.ToLower().Equals( newSortExpression.ToLower() ) )
            {
                // This sort is being applied to the same field for the second time so Reverse it.
                if ( GridViewSortDirection == System.Web.UI.WebControls.SortDirection.Ascending )
                {
                    GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Descending;
                    sortTerm = newSortExpression + " DESC";
                }
                else
                {
                    GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
                    sortTerm = newSortExpression + " ASC";
                }
            }
            else
            {
                GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
                GridViewSortExpression = newSortExpression;
                sortTerm = newSortExpression + " ASC";
            }

            DoSearch( 1, sortTerm );

        }//


        ///<summary>
        ///Add pagination capabilities
        ///</summary>
        public void formGrid_PageIndexChanging( object sender, GridViewPageEventArgs e )
        {
            formGrid.PageIndex = e.NewPageIndex;
            //get current sort term
            string sortTerm = GetCurrentSortTerm();

            DoSearch( formGrid.PageIndex, sortTerm );
        }//
        #endregion


        /// <summary>
        /// Populate form controls
        /// </summary>
        /// <param name="recId"></param>
        private void PopulateControls()
        {

            SetFormSelectList();
   
            SetCategoryList( 0 );
            SetSearchCategoryList( 0 );
            SetSubcategoryList( 0, "" );
            SetStatusList( 0 );
            SetSearchStatusList( 0, "" );
         
            InitializePageSizeList();
        } //
        /// <summary>
        /// populate the main form list
        /// </summary>
        private void SetFormSelectList()
        {
            string sql = "SELECT Id, Title FROM [Vos_Edit_DB].[dbo].[AppItemType] ";
            string orderBy = " Order By Title";

            if ( AppItemTypeRestriction.Length > 0 )
            {
                sql += " where Id in  (" + AppItemTypeRestriction + ") " + orderBy;
            }
            else
            {
                sql += orderBy;
            }

            DataSet ds = DatabaseManager.DoQuery( sql );
            BaseDataManager.PopulateList( lstForm, ds, "Id", "Title", "Select an AppItem Type record" );

            BaseDataManager.PopulateList( ddlTypeId, ds, "Id", "Title" );

            BaseDataManager.PopulateList( ddlTypeId2, ds, "Id", "Title" );


            //DataSet ds = myManager.AppItemTypeSelect();
            //string dataValueField = "Id";
            //string dataTextField = "Title";
            //string selectTitle = "Select an AppItem Type record";

            //if ( ds != null && ds.Tables.Count > 0 )
            //{
            //  // add select row
            //  BaseDataManager.AddEntryToTable( ds.Tables[ 0 ], 0, "Select an AppItem Type record", "Id", "Title" );
            //  DataView dv = ds.Tables[ 0 ].DefaultView;
            //  if ( AppItemTypeRestriction.Length > 0 )
            //  {
            //    dv.RowFilter = "Id in  (" + AppItemTypeRestriction + ")";
            //  }
            //  lstForm.DataSource = dv;
            //  lstForm.DataValueField = dataValueField;
            //  lstForm.DataTextField = dataTextField;
            //  lstForm.DataBind();

            //  ddlTypeId.DataSource = dv;
            //  ddlTypeId.DataValueField = dataValueField;
            //  ddlTypeId.DataTextField = dataTextField;
            //  ddlTypeId.DataBind();

            //  ddlTypeId2.DataSource = dv;
            //  ddlTypeId2.DataValueField = dataValueField;
            //  ddlTypeId2.DataTextField = dataTextField;
            //  ddlTypeId2.DataBind();
            //}


        } //

        private void SetSearchCategoryList( int typeId )
        {
            ddlSearchCategory.Items.Clear();
            DataSet ds1 = myManager.SelectTypeCategories( typeId );
            BaseDataManager.PopulateList( this.ddlSearchCategory, ds1, "Category", "Category", "Select a Category" );

        } //

        /// <summary>
        /// set category list - depends on the selected app item type
        /// </summary>
        private void SetCategoryList( int typeId )
        {
            ddlCategory.Items.Clear();
            DataSet ds;
            //if ( typeId == AppItem.NewsItemType )
            //{
            //    //string filter = "";
            //    //string message = "";
            //    //ds = new AnnouncementEmailManager().Select( filter, ref message );
            //}
            //else
            //{
                ds = myManager.SelectTypeCategories( typeId );
           // }
            BaseDataManager.PopulateList( this.ddlCategory, ds, "Category", "Category", "Select a Category" );

            if ( typeId > 0 && ds != null && ds.Tables.Count > 0 && ds.Tables[ 0 ].Rows.Count > 1 )
            {
                //???may want to only show subcats related to categories for type??
            }



        } //

        /// <summary>
        /// set subcategory list - depends on the selected app item type and category ==> use webservice to retrieve client side
        /// </summary>
        private void SetSubcategoryList( int typeId, string pCategory )
        {
            DataSet ds = myManager.SelectTypeSubcategories( typeId, DatabaseManager.HandleApostrophes( pCategory ) );
            BaseDataManager.PopulateList( this.ddlSubcategory, ds, "SubCategory", "SubCategory", "Select a Sub-Category" );

        } //


        /// <summary>
        /// set subcategory list - depends on the selected app item type and category ==> use webservice to retrieve client side
        /// </summary>
        private void SetFaqSubcategoryList( string pCategory )
        {


        } //
        private void SetSearchStatusList( int typeId, string pCategory )
        {
            DataSet ds;
            if ( typeId > 0 || pCategory.Length > 0 )
            {
                string sql = string.Format( statusSql.Text, typeId, pCategory );
                if ( IsTestEnv() )
                    LoggingHelper.DoTrace( 5, "SetSearchStatusList. sql: " + sql );
                //DataSet ds1 = myManager.SelectTypeStatus( typeId );
                ds = DatabaseManager.DoQuery( sql );
            }
            else
            {
                ds = myManager.SelectTypeStatus( typeId );
            }
            BaseDataManager.PopulateList( this.ddlStatusSearch, ds, "Status", "Status", "Select a Status" );
        } //
        private void SetStatusList( int typeId )
        {
            //will different item types have different possible status values
            DataSet ds = myManager.SelectTypeStatus( typeId );
            BaseDataManager.PopulateList( this.ddlStatus, ds, "Status", "Status", "Select a Status" );

        } //

        #region Page size related methods
        public void pager_Command( object sender, CommandEventArgs e )
        {

            int currentPageIndx = Convert.ToInt32( e.CommandArgument );
            pager1.CurrentIndex = currentPageIndx;
            pager2.CurrentIndex = pager1.CurrentIndex;
            string sortTerm = GetCurrentSortTerm();

            DoSearch( currentPageIndx, sortTerm );

        }
        private string GetCurrentSortTerm()
        {
            string sortTerm = "";
            if ( GridViewSortExpression.Length > 0 )
            {
                sortTerm = GridViewSortExpression;
                if ( GridViewSortDirection == System.Web.UI.WebControls.SortDirection.Ascending )
                    sortTerm = sortTerm + " ASC";
                else
                    sortTerm = sortTerm + " DESC";
            }
            return sortTerm;

        }
        /// <summary>
        /// Initialize page size list and check for a previously set size
        /// </summary>
        private void InitializePageSizeList()
        {
            SetPageSizeList();

            //Set page size based on user preferences
            int defaultPageSize = SessionManager.Get( Session, SessionManager.SYSTEM_GRID_PAGESIZE, 25 );
            this.formGrid.PageSize = defaultPageSize;
            pager1.PageSize = defaultPageSize;
            pager2.PageSize = defaultPageSize;
            this.SetListSelection( this.ddlPageSizeList, defaultPageSize.ToString() );

        } //
        private void SetPageSizeList()
        {
            DataSet ds1 = DatabaseManager.GetCodeValues( "GridPageSize", "SortOrder" );
            DatabaseManager.PopulateList( this.ddlPageSizeList, ds1, "StringValue", "StringValue", "Select Size" );

        } //
        /// <summary>
        /// Check if page size preferrence has changed and update session variable if appropriate
        /// </summary>
        private void CheckForPageSizeChange()
        {
            int index = ddlPageSizeList.SelectedIndex;
            if ( index > 0 )
            {
                int size = Convert.ToInt32( ddlPageSizeList.SelectedItem.Text );
                if ( formGrid.PageSize != size )
                {
                    formGrid.PageSize = size;
                    pager1.PageSize = size;
                    pager2.PageSize = size;
                    //Update user preference
                    Session[ SessionManager.SYSTEM_GRID_PAGESIZE ] = ddlPageSizeList.SelectedItem.Text;
                }
            }

        } //

        /// <summary>
        /// Handle change to page size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void PageSizeList_OnSelectedIndexChanged( object sender, System.EventArgs ea )
        {
            // Set the page size for the DataGrid control based on the selection
            int index = ddlPageSizeList.SelectedIndex;
            if ( index > 0 )
            {
                formGrid.PageIndex = 0;
                DoSearch();
            }
        } //
        #endregion

    }

}