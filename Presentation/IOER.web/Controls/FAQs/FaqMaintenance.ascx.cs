using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EmailHelper = ILPathways.Utilities.EmailManager;
using MyManager = ILPathways.Controllers.FaqController;
using ILPathways.Business;
using ILPathways.classes;
using BDM = ILPathways.Common;
using ILPathways.Controllers;
//using ILPathways.DAL;
using Isle.BizServices;
using ILPathways.Library;
using ILPathways.Utilities;
using LRWarehouse.Business;
using LRD = LRWarehouse.DAL;

namespace ILPathways.Controls.FAQs
{
    public partial class FaqMaintenance : BaseUserControl
    {
        MyManager myManager;
        FileResourceController resController;

        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
        const string thisClassName = "controls_FAQ_FaqMaintenance";
        /// <summary>
        /// Set value used when check form privileges
        /// </summary>


        const string gridDefaultSortExpression = "base.Created";
        #region Basic Properties
        private string formSecurityName = "/vos_portal/controls/FAQ/FaqMaintenance.ascx";
        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string FormSecurityName
        {
            get { return this.formSecurityName; }
            set { this.formSecurityName = value; }
        }
        /// <summary>
        /// INTERNAL PROPERTY: CurrentRecord
        /// Set initially and store in ViewState
        /// </summary>
        protected FaqItem CurrentRecord
        {
            get { return ViewState[ "CurrentRecord" ] as FaqItem; }
            set { ViewState[ "CurrentRecord" ] = value; }
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
        /// <summary>
        /// Store last retrieved total rows. Need to use to properly reset pager item count after none search postbacks
        /// </summary>
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
        /// <summary>
        /// save paths dataset for use in check for any changes on update
        /// </summary>
        protected DataSet CurrentPathwaysList
        {
            get
            {
                if ( ViewState[ "selectedPaths" ] != null )
                {
                    DataSet ds = ( DataSet ) ViewState[ "selectedPaths" ];
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            set { ViewState[ "selectedPaths" ] = value; }
        }
        #endregion
        #region Configuration Properties
        /// <summary>
        /// Get/Set default categories codes. If blank, allow all
        /// Format should include quotes as in: "One", "Two", as will be part of 
        /// filter: (CategoryCode in (0} )
        /// </summary>
        public string DefaultCategories
        {
            get { return defaultCategories.Text; }
            set { defaultCategories.Text = value; }
        }

        /// <summary>
        /// Get/Set default pathways. If non-blank, hide references
        /// </summary>
        public string DefaultTargetPathways
        {
            get { return defaultTargetPathways.Text; }
            set { defaultTargetPathways.Text = value; }
        }

        private bool _showingFaqCode = false;
        /// <summary>
        /// Get/Set default ShowingFaqCode. If non-blank, hide references
        /// </summary>
        public bool ShowingFaqCode
        {
            get { return _showingFaqCode; }
            set { _showingFaqCode = value; }
        }

        private bool _canAddImage = false;
        /// <summary>
        /// Get/Set default CanAddImage. 
        /// </summary>
        public bool CanAddImage
        {
            get { return _canAddImage; }
            set { _canAddImage = value; }
        }

        private bool _canAddNewStatus = false;
        /// <summary>
        /// Get/Set default CanAddImage. 
        /// </summary>
        public bool CanAddNewStatus
        {
            get { return _canAddNewStatus; }
            set { _canAddNewStatus = value; }
        }

        #endregion
        /// <summary>
        /// Handle Page Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void Page_Load( object sender, EventArgs ex )
        {

            resController = new FileResourceController();

            //get manager
            myManager = new MyManager();

            if ( Page.IsPostBack )
            {

            }
            else
            {
                this.InitializeForm();
            }
        }//

        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        private void InitializeForm()
        {
            //set grid defaults (variables are in base control
            GridViewSortExpression = gridDefaultSortExpression;
            GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Descending;
            LastPageNumber = 0;

            CurrentRecord = new FaqItem();
            //
            this.FormPrivileges = SecurityManager.GetGroupObjectPrivileges( this.WebUser, FormSecurityName );

            //= Add attribute to btnDelete to allow client side confirm
            btnDelete.Attributes.Add( "onClick", "return confirmDelete(this);" );
            btnDeleteImage.Attributes.Add( "onClick", "return confirmDeleteImage(this);" );

            //handling setting of which action buttons are available to the current user
            this.btnNew.Enabled = false;
            this.btnSave.Enabled = false;
            this.btnDelete.Visible = false;

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

            if ( FormPrivileges.WritePrivilege > ( int ) ILPathways.Business.EPrivilegeDepth.State )
            {
                this.maxPicHeight.ReadOnly = false;
                this.maxPicWidth.ReadOnly = false;
            }
            if ( DefaultTargetPathways.Length > 0 )
            {
                this.pathwaysPanel.Visible = false;
            }
            else
            {
                publishPathwaysPanel.Visible = true;
            }
            if ( CanAddNewStatus )
                newStatusPanel.Visible = true;

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
            }

        }	// End 

        /// <summary>
        /// Get - retrieve form data
        /// </summary>
        /// <param name="recId"></param>
        private void Get( string recId )
        {
            try
            {
                //get record
                FaqItem entity = myManager.Get( recId );

                if ( entity == null || entity.HasValidRowId() == false )
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
        private void PopulateForm( FaqItem entity )
        {

            //string statusMessage = "";

            detailsPanel.Visible = true;
            TabContainer1.ActiveTabIndex = 2;

            tab3.Visible = true;


            ddlCategory.AutoPostBack = true;
            ddlSubcategory.AutoPostBack = false;

            this.SetListSelection( this.ddlCategory, entity.CategoryId.ToString() );
            //handle subcat list
            if ( entity.CategoryId > 0 && entity.CategoryId != CurrentRecord.CategoryId )
            {
                //don't want to do this every postback though
                //check if categoryId equals previous one
                SetSubCategories( entity.CategoryId );
            }

            txtNewStatus.Text = "";
            this.rblIsActive.SelectedValue = this.ConvertBoolToYesNo( entity.IsActive );

            if ( entity.RowId.ToString() == Guid.NewGuid().ToString() )
                this.txtRowId.Text = "";
            else
                this.txtRowId.Text = entity.RowId.ToString();

            this.txtSequenceNbr.Text = entity.SequenceNbr.ToString();

            this.txtTitle.Text = entity.Title;

            this.txtDescription.Text = entity.Description;
            this.txtFaqCode.Text = entity.FaqCode;

            GetFaqPaths( entity );

            //ddlTypeId.SelectedIndex = -1;
            if ( entity.HasValidRowId() )
            {
                //this.SetListSelection( this.ddlFAQPathway, entity.PathwayId.ToString() );


                this.SetListSelection( this.ddlSubcategory, entity.SubcategoryId.ToString() );

                this.SetListSelection( this.ddlStatus, entity.Status );

                this.SetFieldLockedStatus( txtFaqCode, true );

                //if ( entity.ExpiryDate > System.DateTime.MinValue )
                //  this.txtExpiryDate.Text = entity.ExpiryDate.ToString();
                //this.lblHistory.Text = entity.HistoryTitle();
                this.lblHistory.Text = "Created: " + entity.CreatedByTitle();
                if ( entity.CreatedById > 0 )
                {
                    //AppUser creator = UserManager.GetUser( entity.CreatedById, ref statusMessage );
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
            }
            else
            {
                //reset

                //reset controls or state for empty object
                this.lblHistory.Text = "";
                lblCreatedHistoryDetails.Text = "";
                lblUpdatedHistory.Text = "";
                lblUpdatedHistoryDetails.Text = "";

                btnDelete.Enabled = false;

                this.SetFieldLockedStatus( txtFaqCode, false );

                //this.ddlFAQPathway.SelectedIndex = -1;
                //ddlCategory.SelectedIndex = -1;
                ddlSubcategory.SelectedIndex = -1;
                ddlStatus.SelectedIndex = -1;

                this.txtExpiryDate.Text = "";

            }

            //check for an image ==========================================
            if ( entity.ImageId > 0 )
            {
                //image details should be in the entity, but check just in case
                if ( entity.AppItemImage == null || entity.AppItemImage.Id == 0 )
                {
                    //entity.AppItemImage = ImageStoreManager.Get( entity.ImageId );
                    CurrentRecord = entity;
                }
                //populate image stuff
                //future: add a comment field to ImageStore to store image info including height and width
                lblImageId.Text = entity.AppItemImage.Id.ToString();
                this.txtImageFileName.Text = entity.AppItemImage.ImageFileName;

                imgCurrent.Visible = true;
                imgCurrent.AlternateText = entity.AppItemImage.Title;
                txtImageTitle.Text = entity.AppItemImage.Title;

                imgCurrent.ImageUrl = showPictureUrl.Text + "&id=" + entity.AppItemImage.Id.ToString();
                this.btnDeleteImage.Enabled = true;
            }
            else
            {
                imgCurrent.Visible = false;
                lblImageId.Text = "";
                txtImageTitle.Text = "";
                txtImageFileName.Text = "";
                this.btnDeleteImage.Enabled = false;
            }

            CurrentRecord = entity;

        }//


        private void GetFaqPaths( FaqItem entity )
        {
            // Load checkboxes
            //cbxFAQPathway.Items.Clear();

            //DataSet ds = myManager.FaqPathway_Select( entity.RowId );
            //CurrentPathwaysList = ds;

            //if ( ds != null && ds.Tables.Count > 0 && ds.Tables[ 0 ].Rows.Count > 0 )
            //{
            //    foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
            //    {
            //        ListItem item = new ListItem();
            //        item.Value = LRD.DatabaseManager.GetRowColumn( dr, "id", "0" );
            //        item.Text = dr[ "Pathway" ].ToString().Trim();
            //        item.Selected = LRD.DatabaseManager.GetRowColumn( dr, "HasPath", false );

            //        cbxFAQPathway.Items.Add( item );

            //    } //end foreach
            //}

        }

        protected void Category_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            try
            {
                TabContainer1.ActiveTabIndex = 2;
                if ( ddlCategory.SelectedIndex > 0 )
                {
                    int categoryId = Int32.Parse( this.ddlCategory.SelectedValue );
                    //fill subcategories for selected category
                    if ( usingMultiplePath.Text.Equals( "yes" ) )
                    {
                        SetSubCategories( categoryId );

                    }
                    //else if (  ddlFAQPathway.SelectedIndex > 0 )
                    //{
                    //  //int pathwayId = Int32.Parse( this.ddlFAQPathway.SelectedValue );
                    //  SetSubCategories( categoryId );

                    //} 
                    //else
                    //{
                    //  SetConsoleErrorMessage( "Please select a valid pathway before selecting a category" );
                    //}
                }
            }
            catch ( Exception ex )
            {
                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.Message );
                this.LogError( ex, thisClassName + ".Category_OnSelectedIndexChanged" );
            }
        }
        protected void Page_PreRender( object sender, EventArgs e )
        {

            try
            {
                pager1.CurrentIndex = pager2.CurrentIndex = LastPageNumber;
                pager1.ItemCount = pager2.ItemCount = LastTotalRows;
            }
            catch
            {
                //no action
            }

        }//

        /// <summary>
        /// Handle a form button clicked 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        public void FormButton_Click( Object sender, CommandEventArgs ex )
        {
            TabContainer1.ActiveTabIndex = 2;

            switch ( ex.CommandName )
            {
                case "New":
                    this.HandleNewRequest();
                    break;
                case "Update":
                    if ( this.IsFormValid() )
                    {
                        this.UpdateForm();
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

            }
        } // end 
        /// <summary>
        /// Check for bad words
        /// </summary>
        private void DoBadWordsCheck()
        {
            string comments = this.txtDescription.Text.Trim().ToString();

            // Check for bad words      
            if ( BadWordChecker.CheckForBadWords( comments ) )
            {

                //highlight badwords if found
                string badWords = BadWordChecker.Highlight( comments );
                //txtbxComment.Text = badWords;
                txtDescription.Text = badWords;

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
            ResetForm();

        } // end 

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

                vsErrorSummary.Controls.Clear();

                Page.Validate();
                //other edits

                //recId = int.Parse( this.Id.Text );

                // check additional required fields
                isValid = HasRequiredFields();

                if ( isValid )
                {
                    isValid = ValidateImageFields();

                }

            }
            catch ( System.Exception ex )
            {
                //SetFormMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.ToString() );
                return false;
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
        /// verify required fields are present
        /// </summary>
        /// <returns></returns>
        private bool HasRequiredFields()
        {
            // check additional required fields

            if ( HasPathways() == false )
            {
                this.AddReqValidatorError( this.vsErrorSummary, "Atleast one Pathway must be selected for an FAQ item", "txtTitle", true );
            }

            if ( this.ddlCategory.SelectedIndex < 1 )
            {
                this.AddReqValidatorError( this.vsErrorSummary, "A category must be selected for an FAQ item", "txtTitle", true );
            }


            if ( this.ddlSubcategory.SelectedIndex < 1 )
            {
                this.AddReqValidatorError( this.vsErrorSummary, "A subcategory must be selected for an FAQ item", "txtTitle", true );
            }
            if ( ddlStatus.SelectedIndex < 1 && this.txtNewStatus.Text.Trim().Length == 0 )
            {
                this.AddReqValidatorError( this.vsErrorSummary, "A Status is required", "txtTitle", true );
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
        /// ensure at least one pathway has been selected
        /// </summary>
        /// <returns></returns>
        private bool HasPathways()
        {
            bool isValid = false;
            foreach ( ListItem li in this.cbxFAQPathway.Items )
            {
                if ( li.Selected == true )
                {
                    isValid = true;
                    break;
                }
            }

            return isValid;
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

            //if ( !IsInteger( maxPicWidth.Text ) )
            //{
            //  this.AddReqValidatorError( this.vsErrorSummary, "The maximum picture width must be provided", "txtImageTitle", true );

            //}
            //if ( !IsInteger( maxPicHeight.Text ) )
            //{
            //  this.AddReqValidatorError( this.vsErrorSummary, "The maximum picture height must be provided", "txtImageTitle", true );

            //}
            //if ( vsErrorSummary.Controls.Count > 0 )
            //{
            //  return false;
            //}

            //int maxWidth = Int32.Parse( maxPicWidth.Text );
            //int maxHeight = Int32.Parse( maxPicHeight.Text );

            //if ( txtImageTitle.Text.Trim().Length == 0 )
            //{
            //  this.AddReqValidatorError( this.vsErrorSummary, "A title must be provided for the image", "txtImageTitle", false );
            //}

            //if ( !resController.DoesImageExist( imageFileUpload ) )
            ////if ( !DoesImageExist() )
            //{
            //  this.AddReqValidatorError( this.vsErrorSummary, "The image path is invalid. Please enter a valid path or use the browse button to search for and select an image.", "imageFileUpload", true );

            //} else if ( doingSizeCheck.Text.Equals( "yes" )
            //         && resController.IsImageSizeValid( imageFileUpload, maxWidth, maxHeight ) == false )
            //{
            //  this.AddReqValidatorError( this.vsErrorSummary, "The selected Image is too wide. Select an image no wider than <a href='#sampleImageSize'>the sample blue box</a>.", "imageFileUpload", false );
            //}

            //if ( !resController.IsAllowedImageMimeType( imageFileUpload.FileName ) )
            //{
            //  this.AddReqValidatorError( this.vsErrorSummary, cvlImagePathMime.ErrorMessage, "imageFileUpload", false );
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

        #endregion

        /// <summary>
        /// Handle form update
        /// </summary>
        private void UpdateForm()
        {
            string id = "";
            string msg = "";
            string action = "";
            string statusMessage = "";
            string imageStatus = "";

            FaqItem entity = new FaqItem();

            try
            {
                id = this.txtRowId.Text;

                if ( id.Length == 0 )
                {

                    entity.CreatedBy = WebUser.Email;
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

                entity.SequenceNbr = Int32.Parse( txtSequenceNbr.Text );

                entity.Title = this.txtTitle.Text;
                entity.Description = this.txtDescription.Text;

                if ( txtFaqCode.Text.Trim() == "" || txtFaqCode.Text.ToLower() == "default" )
                    entity.FaqCode = System.Guid.NewGuid().ToString();
                else
                    entity.FaqCode = this.txtFaqCode.Text;

                if ( usingMultiplePath.Text.Equals( "no" ) )
                {
                    //entity.PathwayId = int.Parse( this.ddlFAQPathway.SelectedValue.ToString() );
                }
                entity.CategoryId = int.Parse( this.ddlCategory.SelectedValue.ToString() );
                entity.SubcategoryId = int.Parse( this.ddlSubcategory.SelectedValue.ToString() );

                if ( this.txtNewStatus.Text.Length > 0 )
                    entity.Status = txtNewStatus.Text;
                else if ( ddlStatus.SelectedIndex > 0 )
                    entity.Status = this.ddlStatus.SelectedValue.ToString();
                else
                    entity.Status = "";

                //
                entity.IsActive = this.ConvertYesNoToBool( this.rblIsActive.SelectedValue );

                //if ( txtExpiryDate.Text.Length > 0 )
                //  entity.ExpiryDate = System.DateTime.Parse( txtExpiryDate.Text );

                entity.LastUpdated = System.DateTime.Now;
                entity.LastUpdatedBy = WebUser.UserName;
                entity.LastUpdatedById = WebUser.Id;		//include for future use

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
                       // imageStatus = HandleImageUpdate( entity );
                    }

                }
                else
                {
                    statusMessage = myManager.Update( entity );
                    //if ( statusMessage.ToLower().Equals( "successful" ) )
                       // imageStatus = HandleImageUpdate( entity );
                }


                if ( statusMessage.Equals( "successful" ) )
                {
                    //check dropdowns need to be refreshed
                    if ( this.txtNewStatus.Text.Length > 0 )
                        SetStatusList();

                    UpdateFaqPathways( entity, action, ref statusMessage );

                    //update CurrentRecord and form
                    Get( entity.RowId.ToString() );

                    this.SetConsoleSuccessMessage( action + " was successful for: " + entity.Title + imageStatus );

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
        /// update related faq pathways
        /// </summary>
        /// <returns></returns>
        protected bool UpdateFaqPathways( FaqItem entity, string action, ref string statusMessage )
        {
            bool isValid = true;

            try
            {
                if ( DefaultTargetPathways.Length > 0 )
                {
                    //only need to address on create
                    if ( action.ToLower().Equals( "create" ) )
                    {
                        foreach ( string newItem in DefaultTargetPathways.Split( ',' ) )
                        {
                            if ( newItem.Length > 0 )
                            {
                                int pPathwayId = Int32.Parse( newItem );
                                myManager.FaqPathway_Insert( entity.RowId, pPathwayId, ref statusMessage );
                            }
                        }
                    }

                }
                else
                {
                    DataSet ds = new DataSet();
                    //Retrieve tags list from session
                    if ( LRD.BaseDataManager.DoesDataSetHaveRows( CurrentPathwaysList ) == true )
                    {
                        ds = CurrentPathwaysList;
                        StringBuilder deletedItems = new StringBuilder( "" );
                        StringBuilder addedItems = new StringBuilder( "" );
                        int counter = 0;

                        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                        {
                            //Build the added and deleted jobs/carees string
                            if ( cbxFAQPathway.Items[ counter ].Selected
                                && LRD.DatabaseManager.GetRowColumn( dr, "HasPath", false ) == false )
                                addedItems.Append( dr[ "id" ] ).Append( "|" );

                            else if ( ( !cbxFAQPathway.Items[ counter ].Selected )
                                && LRD.DatabaseManager.GetRowColumn( dr, "HasPath", false ) == true )
                                deletedItems.Append( dr[ "id" ] ).Append( "|" );

                            counter++;
                        }

                        //Update the database with the changes
                        myManager.FaqPathway_ApplyChanges( entity.RowId, WebUser.Id, addedItems.ToString(), deletedItems.ToString() );
                    }
                    else
                    {
                        //new only
                        foreach ( ListItem li in this.cbxFAQPathway.Items )
                        {
                            if ( li.Selected == true )
                            {
                                int pPathwayId = Int32.Parse( li.Value );
                                myManager.FaqPathway_Insert( entity.RowId, pPathwayId, ref statusMessage );
                            }
                        }
                        if ( statusMessage.Length > 0 )
                            isValid = false;
                    }
                }
            }
            catch ( Exception ex )
            {
                this.LogError( ex, thisClassName + ".UpdateFaqPathways" );
                isValid = false;
            }
            return isValid;
        }

        /// <summary>
        /// insert image into database 
        /// </summary>
        /// <returns></returns>
        //protected string HandleImageUpdate( FaqItem thisItem )
        //{
        //    bool isValid = true;
        //    int imageId = 0;
        //    string statusMessage = "";
        //    string returnMessage = "";
        //    string action = "";
        //    bool isUpdate;
        //    //!!! need to check if pertinent fields have all been cleared - if so then delete??
        //    //if just the title etc, changed, then maybe we don't want to do a full update (ie the image data)??
        //    //there may a request to update the title as well
        //    if ( !IsImageFilePresent() )
        //    {
        //        // no image, skip
        //        return "";
        //    }

        //    ImageStore entity = new ImageStore();

        //    if ( this.lblImageId.Text == "" )
        //        imageId = 0;
        //    else
        //        imageId = int.Parse( this.lblImageId.Text );

        //    if ( imageId == 0 )
        //    {
        //        isUpdate = false;
        //        entity.Id = 0;
        //        entity.CreatedBy = WebUser.IdentifyingName();
        //        entity.Created = System.DateTime.Now;
        //        action = "Create";

        //    }
        //    else
        //    {
        //        entity = CurrentRecord.AppItemImage;
        //        isUpdate = true;
        //        action = "Update";
        //    }

        //    entity.Title = txtImageTitle.Text;

        //    //check if file was specified (versus updating text for existing file)
        //    if ( IsImageFilePresent() )
        //    {
        //        entity.MimeType = imageFileUpload.PostedFile.ContentType;
        //        entity.ImageFileName = imageFileUpload.FileName;

        //        int maxWidth = Int32.Parse( maxPicWidth.Text );
        //        int maxHeight = Int32.Parse( maxPicHeight.Text );

        //        ///call method to:
        //        ///- check if image is within allowed maximums
        //        ///- if not, resize image
        //        ///- entity will be updated with size and image stream data
        //        isValid = resController.HandleImageResizing( entity, imageFileUpload, maxWidth, maxHeight );
        //        // if not valid check message and return
        //        if ( !isValid )
        //        {
        //            statusMessage = entity.Message;
        //            return "";
        //        }


        //    }

        //    if ( entity.Id == 0 )
        //    {
        //        imageId = ImageStoreManager.Create( entity, ref statusMessage );
        //        if ( imageId > 0 )
        //        {
        //            this.lblImageId.Text = imageId.ToString();
        //            entity.Id = imageId;
        //            statusMessage = "Successfully saved Image!";

        //            imgCurrent.ImageUrl = "/vos_portal/showPicture.aspx?imgSrc=is&id=" + imageId.ToString();
        //            imgCurrent.AlternateText = entity.Title;
        //            imgCurrent.Visible = true;
        //        }
        //        else
        //        {
        //            lblImageId.Text = "0";
        //            statusMessage = "Image save failed: " + statusMessage;
        //            imgCurrent.ImageUrl = "";
        //            isValid = false;
        //        }
        //    }
        //    else
        //    {
        //        statusMessage = ImageStoreManager.Update( entity );
        //        if ( statusMessage.Equals( "successful" ) )
        //        {
        //            imgCurrent.AlternateText = entity.Title;
        //            imgCurrent.Visible = true;

        //            statusMessage = "Successfully updated Image!";
        //        }
        //        else
        //        {
        //            isValid = false;
        //            imgCurrent.ImageUrl = "";
        //        }

        //    }
        //    if ( !isUpdate )
        //    {
        //        // New image, so update imageId in current record
        //        thisItem.ImageId = imageId;
        //        statusMessage = myManager.Update( thisItem );
        //    }

        //    return returnMessage;

        //}//


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
                //ignore RI message (means image is associtate with another record as well)
                string riMessage = "the delete statement conflicted with the reference constraint";

                imageId = CurrentRecord.ImageId;
                //reset and update record
                CurrentRecord.ImageId = 0;
                string statusMessage = myManager.Update( CurrentRecord );

                if ( statusMessage.Equals( "successful" ) || statusMessage.IndexOf( riMessage ) > -1 )
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
                //this.ResetForm();
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + e.ToString() );
            }
        }

        /// <summary>
        /// Reset form values
        /// </summary>
        private void ResetForm()
        {
            //reset form by populating with an empty business object

            FaqItem entity = new FaqItem();
            //note, a nice to have is to remember the current category, and subcategory!!
            if ( ddlCategory.SelectedIndex > 0 )
            {
                entity.CategoryId = Int32.Parse( this.ddlCategory.SelectedValue );
            }
            //do any defaults. 
            entity.IsActive = true;
            entity.FaqCode = "default";
            entity.SequenceNbr = 0;
            PopulateForm( entity );
        }//

        #region SEARCH related methods

        /// <summary>
        /// Handle selection from form list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void SearchPathway_SelectedIndexChanged( object sender, System.EventArgs ea )
        {

            try
            {
                TabContainer1.ActiveTabIndex = 0;
                if ( ddlSearchPathway.SelectedIndex > 0 )
                {
                    int pathwayId = Int32.Parse( this.ddlSearchPathway.SelectedValue );
                    SetSearchCategories( pathwayId );

                    //do search anyway??, then set categories
                    //if ( searchingOnPathwayChange.Text.Equals( "yes" ) )
                    //  DoSearch();
                }
            }
            catch ( Exception ex )
            {
                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.Message );
                this.LogError( ex, thisClassName + ".lstForm_SelectedIndexChanged" );
            }
        } //

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
            if ( selectedPageNbr == 0 )
            {
                //with custom pager, need to start at 1
                selectedPageNbr = 1;
            }
            LastPageNumber = selectedPageNbr;
            pager1.CurrentIndex = pager2.CurrentIndex = selectedPageNbr;

            int pTotalRows = 0;
            // Set the page size for the DataGrid control based on the selection
            CheckForPageSizeChange();

            try
            {
                string filter = FormatFilter();

                ds = myManager.Search( filter, sortTerm, selectedPageNbr, pager1.PageSize, ref pTotalRows );
                //assumes min rows are returned, so pager needs to have the total rows possible
                pager1.ItemCount = pager2.ItemCount = pTotalRows;

                LastTotalRows = pTotalRows;

                if ( DoesDataSetHaveRows( ds ) == false )
                {
                    TabContainer1.ActiveTabIndex = 0;
                    resultsPanel.Visible = false;
                    if ( filter.Length > 20 )
                        SetConsoleInfoMessage( "No records were found for the provided search criteria" );
                    ddlPageSizeList.Enabled = false;
                    pager1.Visible = false;
                    pager2.Visible = false;
                }
                else
                {
                    TabContainer1.ActiveTabIndex = 1;
                    resultsPanel.Visible = true;
                    ddlPageSizeList.Enabled = true;

                    //searchPanel.Visible = false;

                    DataTable dt = ds.Tables[ 0 ];
                    DataView dv = ( ( DataTable ) dt ).DefaultView;

                    if ( pTotalRows > formGrid.PageSize )
                    {
                        //formGrid.PagerSettings.Visible = true;
                        pager1.Visible = true;
                        if ( pTotalRows > 10 )
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


        protected string FormatFilter()
        {
            string filter = " (base.IsActive = 1)";

            string booleanOperator = "AND";

            if ( pathwaysPanel.Visible && ddlSearchPathway.SelectedIndex > 0 )
            {
                //string where = string.Format( " (fqpath.PathwayId in ({0})) ", this.ddlSearchPathway.SelectedValue );
                string where = string.Format( "(base.RowId in (select FaqRowId from [Faq.FaqPathway] where PathwayId in ({0}))) ", this.ddlSearchPathway.SelectedValue );

                filter += LRD.BaseDataManager.FormatSearchItem( filter, where, booleanOperator );
            }
            else if ( DefaultTargetPathways.Length > 0 )
            {
                //string where = string.Format( " (fqpath.PathwayId in ({0})) ", DefaultTargetPathways );
                string where = string.Format( "(base.RowId in (select FaqRowId from [Faq.FaqPathway] where PathwayId in ({0}))) ", DefaultTargetPathways );
                filter += LRD.BaseDataManager.FormatSearchItem( filter, where, booleanOperator );
            }

            if ( this.ddlSearchCategory.SelectedIndex > 0 )
            {
                filter += LRD.BaseDataManager.FormatSearchItem( filter, "base.CategoryId", ddlSearchCategory.SelectedValue.ToString(), booleanOperator );
            }
            else if ( DefaultCategories.Length > 0 )
            {
                string where = string.Format( "(cat.CategoryCode in ({0}) )", DefaultCategories );
                filter += LRD.BaseDataManager.FormatSearchItem( filter, where, booleanOperator );
            }

            if ( ddlStatusSearch.SelectedIndex > 0 )
            {
                filter += LRD.BaseDataManager.FormatSearchItem( filter, "Status", ddlStatusSearch.SelectedValue.ToString(), booleanOperator );
            }

            if ( txtKeyword.Text.Trim().Length > 0 )
            {
                string keyword = LRD.BaseDataManager.HandleApostrophes( FormHelper.CleanText( txtKeyword.Text.Trim() ) );

                if ( keyword.IndexOf( "%" ) == -1 )
                    keyword = "%" + keyword + "%";

                string where = " (base.Title like '" + keyword + "'	OR base.[Description] like '" + keyword + "') ";
                filter += LRD.BaseDataManager.FormatSearchItem( filter, where, booleanOperator );
            }

            if ( this.IsTestEnv() )
                this.SetConsoleSuccessMessage( "sql: " + filter );

            return filter;
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
            //consider helper: if main sort is not the main title, add as the secondary sort
            if ( newSortExpression.ToLower() != "title" )
            {
                sortTerm += ", Title ASC";
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

        private string GetCurrentSortTerm()
        {
            string sortTerm = GridViewSortExpression;
            if ( sortTerm.Length > 0 )
            {
                if ( GridViewSortDirection == System.Web.UI.WebControls.SortDirection.Ascending )
                    sortTerm = sortTerm + " ASC";
                else
                    sortTerm = sortTerm + " DESC";
            }
            return sortTerm;

        }
        #endregion

        #region Paging related methods
        public void pager_Command( object sender, CommandEventArgs e )
        {

            int currentPageIndx = Convert.ToInt32( e.CommandArgument );
            pager1.CurrentIndex = currentPageIndx;
            pager2.CurrentIndex = pager1.CurrentIndex;
            string sortTerm = GetCurrentSortTerm();

            DoSearch( currentPageIndx, sortTerm );

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
            DataSet ds1 = LRD.DatabaseManager.GetCodeValues( "GridPageSize", "SortOrder" );
            LRD.DatabaseManager.PopulateList( this.ddlPageSizeList, ds1, "StringValue", "StringValue", "Select Size" );

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
                //need to reset to first page as current pg nbr may be out of range
                formGrid.PageIndex = 0;
                //retain curent sort though
                string sortTerm = GetCurrentSortTerm();

                DoSearch( formGrid.PageIndex, sortTerm );
                //DoSearch();
            }
        } //
        #endregion
        #region Housekeeping
        /// <summary>
        /// Populate form controls
        /// </summary>
        /// <param name="recId"></param>
        private void PopulateControls()
        {
            if ( usingMultiplePath.Text.Equals( "yes" ) )
            {
                SetFAQPathwayList();
                GetFaqPaths( new FaqItem() );
                SetSearchCategories( 0 );
                SetCategoryList();
                //SetSubcategoryList( 0, "" );
            }
            else
            {
                SetFAQPathwayList();
            }


            SetStatusList();

            //
            InitializePageSizeList();
        } //

        /// <summary>
        /// set FAQ pathway list 
        /// </summary>
        private void SetFAQPathwayList()
        {
            //this.ddlFAQPathway.Items.Clear();
            //string sql = "Select distinct string1 As FAQPathway from FaqItem where typeid = 1010 order by 1 ";
            string sql = sqlSelectDistinctPaths.Text;
            DataSet ds = LRD.DatabaseManager.DoQuery( sql );
            LRD.BaseDataManager.PopulateList( this.ddlSearchPathway, ds, "Id", "Title", "Select a Pathway" );

            //if ( usingMultiplePath.Text.Equals( "no" ) )
            //{
            //  DataSet ds1 = LRD.DatabaseManager.DoQuery( sql );
            //  LRD.BaseDataManager.PopulateList( this.ddlFAQPathway, ds1, "Id", "Title", "Select a Pathway" );
            //}
        } //

        private void SetSearchCategories( int pathwayId )
        {
            ddlSearchCategory.Items.Clear();
            string filter = "   "; //start with spaces to force use of AND
            if ( DefaultCategories.Length > 0 )
            {
                //Assume DefaultCategories is in format "One", "Two"
                string where = string.Format( "(CategoryCode in ({0}) )", DefaultCategories );
                filter = LRD.DatabaseManager.FormatSearchItem( filter, where, "AND" );
            }
            if ( pathwayId > 0 )
            {
                filter += LRD.DatabaseManager.FormatSearchItem( filter, "PathwayId", pathwayId, "AND" );
            }

            string sql = string.Format( sqlSelectSearchCategories.Text, filter );
            DataSet ds = LRD.DatabaseManager.DoQuery( sql );
            LRD.BaseDataManager.PopulateList( this.ddlSearchCategory, ds, "Id", "Category", "Select a Category" );

        } //
        /// <summary>
        /// set faq category list - all categories, not using pathway
        /// </summary>
        private void SetCategoryList()
        {
            ddlCategory.Items.Clear();
            ddlSubcategory.Items.Clear();
            string filter = "   "; //start with spaces to force use of AND
            if ( DefaultCategories.Length > 0 )
            {
                //Assume DefaultCategories is in format "One", "Two"
                string where = string.Format( "(CategoryCode in ({0}) )", DefaultCategories );
                filter = LRD.DatabaseManager.FormatSearchItem( filter, where, "AND" );
            }
            //string sql = string.Format( "Select distinct Category from FaqItem where len(Category) > 0 AND TypeId = 1010 and String1 = '{0}' Order by Category", pathway.Trim());
            string sql = string.Format( sqlCategoriesList.Text, filter );
            DataSet ds = LRD.DatabaseManager.DoQuery( sql );
            LRD.BaseDataManager.PopulateList( this.ddlCategory, ds, "Id", "Category", "Select a Category" );
        } //


        /// <summary>
        /// set faq category list - depends on the selected faq path and app item of 1010
        /// </summary>
        private void SetPathwayCategories( int pathwayId )
        {
            ddlCategory.Items.Clear();
            ddlSubcategory.Items.Clear();
            //string sql = string.Format( "Select distinct Category from FaqItem where len(Category) > 0 AND TypeId = 1010 and String1 = '{0}' Order by Category", pathway.Trim());
            string sql = string.Format( "SELECT id, [Category]  FROM [dbo].[FAQ.Category] where [PathwayId] = {0} order by 2", pathwayId.ToString() );
            DataSet ds = LRD.DatabaseManager.DoQuery( sql );
            LRD.BaseDataManager.PopulateList( this.ddlCategory, ds, "Id", "Category", "Select a Category" );
        } //

        /// <summary>
        /// set subcategory list - depends on the selected app item type and category ==> use webservice to retrieve client side
        /// </summary>
        private void SetSubCategories( int pCategoryId )
        {
            ddlSubcategory.Items.Clear();

            string SUBCATEGORY_SELECT_SQL = "SELECT [Id] ,[SubCategory] As Title FROM [dbo].[FAQ.SubCategory] where [CategoryId] = {0} order by 2 ";

            string sql = string.Format( SUBCATEGORY_SELECT_SQL, pCategoryId.ToString() );
            DataSet ds = LRD.DatabaseManager.DoQuery( sql );
            LRD.DatabaseManager.PopulateList( ddlSubcategory, ds, "Id", "Title", "Select a SubCategory" );

        } //

        private void SetStatusList()
        {
            //search uses existing
            string sql = string.Format( "SELECT distinct [Status]  FROM [dbo].[FAQ] order by 1" );
            DataSet ds1 = LRD.DatabaseManager.DoQuery( sql );
            LRD.BaseDataManager.PopulateList( this.ddlStatusSearch, ds1, "Status", "Status", "Select a Status" );

            //
            DataSet ds2 = LRD.DatabaseManager.SelectCodesTable( "[CodeTable]", "StringValue", "StringValue", "IntegerValue", " CodeName = 'FaqStatus'" );
            LRD.BaseDataManager.PopulateList( ddlStatus, ds2, "Code", "title", "Select a Status" );
        } //



        #endregion

    }

}