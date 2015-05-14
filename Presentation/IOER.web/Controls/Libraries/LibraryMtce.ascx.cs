using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using SD = System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using EmailHelper = ILPathways.Utilities.EmailManager;

using ILPlibrary = ILPathways.Library;
using ILPathways.Controllers;
using ILPathways.Common;
using ILPathways.Utilities;
using GDAL = Isle.BizServices;
using ILPathways.Business;
using ThisLibrary = ILPathways.Business.Library;
using MyManager = Isle.BizServices.LibraryBizService;
using ContentManager = Isle.BizServices.ContentServices;
using DataBaseHelper = LRWarehouse.DAL.BaseDataManager;

namespace ILPathways.Controls.Libraries
{
    public partial class LibraryMtce : ILPlibrary.BaseUserControl
    {
        MyManager myManager = new MyManager();
        ContentManager myContentManager  = new ContentManager();
        const string thisClassName = "LibraryMtce";


        #region Properties
        /// <summary>
        /// Set value used when check form privileges
        /// - using admin to allow setting global access
        /// - will do individual checks on gets, if not admin
        /// </summary>
        public string FormSecurityName
        {
            get { return this.formSecurityName.Text; }
            set { this.formSecurityName.Text = value; }
        }
        public bool InitializeOnRequest { get; set; }
        public int NewLibraryId
        {
            get
            {
                if ( Session[ "NewLibraryId" ] == null )
                    Session[ "NewLibraryId" ] = 0;
                return Int32.Parse( Session[ "NewLibraryId" ].ToString() );
            }
            set { Session[ "NewLibraryId" ] = value; }
        }
        //if text is needed to be populated at load and available after postback, one option can be to save in a control in the ascx file.
        public int DefaultLibraryTypeId
        {
            get { return Int32.Parse( txtDefaultLibraryTypeId.Text ); }
            set { txtDefaultLibraryTypeId.Text = value.ToString(); }
        }
        public List<CodeItem> OrgCodesList
        {
            get
            {
                if ( ViewState[ "OrgCodesList" ] == null )
                    ViewState[ "OrgCodesList" ] = new List<CodeItem>();
                return ViewState[ "OrgCodesList" ] as List<CodeItem>;
            }
            set { ViewState[ "OrgCodesList" ] = value; }
        }
        #endregion

        /// <summary>
        /// Handle Page Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void Page_Load( object sender, EventArgs ex )
        {
            if ( IsUserAuthenticated() )
            {
                WebUser = GetAppUser();
            }
            else
            {
                //set message - only do where clearly on form by itself.
                //or just disable, ==> done in populate as previously end user could see the detail
            }
            if ( Page.IsPostBack == false )
            {
                this.InitializeForm();
                if (InitializeOnRequest == false)
                    CheckRecordRequest();
                    
            }
        }//

        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        public void InitializeForm()
        {

            FormPrivileges = new ApplicationRolePrivilege();
            this.FormPrivileges = GDAL.SecurityManager.GetGroupObjectPrivileges( this.WebUser, FormSecurityName );
            if ( IsUserAuthenticated() && WebUser.UserName == "mparsons" )
            {
                showingImagePath.Text = "yes";
            }
            //= Add attribute to btnDelete to allow client side confirm
            btnDelete.Attributes.Add( "onClick", "return confirmDelete(this);" );

            //handling setting of which action buttons are available to the current user
            this.btnNew.Enabled = false;
            this.btnSave.Enabled = false;
            this.btnDelete.Visible = false;

            SetValidators( false );
            // Set source for form lists
            this.PopulateControls();

        }	// End 

        /// <summary>
        /// called from a parent control
        /// </summary>
        public void InitializeOrgLibrary( string orgName, List<CodeItem> orgCodesList )
        {
            //reset form by populating with an empty business object
            ThisLibrary entity = new ThisLibrary();
            if ( IsUserAuthenticated() )
            {
                WebUser = GetAppUser();
            }
            //do any defaults. 
            entity.IsActive = true;
            entity.Title = orgName;
            entity.Description = "";
            entity.LibraryTypeId = 2;
            ddlLibraryType.Enabled = false;
            entity.HasChanged = false;
            this.btnNew.Visible = false;
            btnSave.Enabled = true;
            detailsPanel.Visible = true;
            if ( orgCodesList != null && orgCodesList.Count > 0 )
            {
                //??necesary?
                OrgCodesList = orgCodesList;
                //already contains: Select an Organization (so set index accordingly)
                //MP ===> doesn't actually contain latter!!!! Could check first entry??
                DataBaseHelper.PopulateList( this.ddlOrgs, orgCodesList, "Id", "Title", "" );


                //15-02-02 mp - there is no select entry.
                if ( orgCodesList.Count == 2 ) 
                {
                    //ddlOrgs.SelectedIndex = 1;
                    //ddlOrgs.Enabled = false;
                }
                this.orgPanel.Visible = true;
                ddlOrgs.Visible = true;
            }
            else
            {
                SetConsoleErrorMessage( "Error: you must be associated with an organization in order to create an organization library!" );
                detailsPanel.Visible = false;
            }
            libraryName.Text = "Add new organization library";
           PopulateForm( entity );

        }	// End 
        public void SetOrgLibrary( int id )
        {
            if ( IsUserAuthenticated() )
            {
                WebUser = GetAppUser();
            }
            this.btnNew.Visible = false;
            SetValidators( true );

            Get( id );
            //for now don't allow change  of type
            ddlLibraryType.Enabled = false;

        }	// End 

        /// <summary>
        /// called from a parent control
        /// </summary>
        public void InitializePersonalLibary()
        {
            //reset form by populating with an empty business object
            ThisLibrary entity = new ThisLibrary();
            //do any defaults. 
            entity.IsActive = true;
            if ( IsUserAuthenticated() )
            {
                entity.Title = WebUser.FullName() + "'s Library";
                entity.Description = WebUser.FullName();

            }
            entity.LibraryTypeId = DefaultLibraryTypeId;

            entity.HasChanged = false;
            this.btnNew.Visible = false;
            btnSave.Enabled = true;

            PopulateForm( entity );
        }//
        public void SetLibrary( int id )
        {
            SetValidators( true );
            Get( id );
        }	// End 

        private void SetValidators( bool state)
        {
            this.rfvTitle.Enabled = state;

        }	// End 


        #region retrieval
        /// <summary>
        /// Check the request type for the form
        /// </summary>
        private void CheckRecordRequest()
        {

            // Check if a request was made for a specific record
            int id = this.GetRequestKeyValue( "id", 0 );
            //if PK is a Guid, use:
            string rid = this.GetRequestKeyValue( "rid", "" );
            //if (id.Trim().Length > 36)

            if ( id > 0 )
            {
                this.Get( id );

            }
            else if ( rid.Length == 36 )
            {
                this.Get( rid );
            }
            else
            {
                //or get personal library

                if ( IsUserAuthenticated() )
                    HandleNewRequest();
                else
                {
                    detailsPanel.Visible = false;
                    this.lblLibraryDisplay.Text = "You must be logged into the website in order to create a library.";
                }
            }


        }	// End 

        /// <summary>
        /// Get - retrieve form data
        /// </summary>
        /// <param name="recId"></param>
        public void Get( int recId )
        {
            try
            {
                //get record
                ThisLibrary entity = myManager.Get( recId );

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
                this.ResetForm();
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );

            }

        }	// End method

        private void Get( string recId )
        {
            try
            {
                //get record
                ThisLibrary entity = myManager.GetByRowId( recId );

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
                this.ResetForm();
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );

            }

        }	// End method

        /// <summary>
        /// Populate the form 
        ///</summary>
        private void PopulateForm( ThisLibrary entity )
        {
            //CurrentRecord = entity;
            lblOrganization.Text = "";
            viewLibraryLink.Visible = false ;
            this.txtId.Text = entity.Id.ToString();
            this.txtTitle.Text = entity.Title;
            this.txtDescription.Text = entity.Description;

            this.SetListSelection( this.ddlLibraryType, entity.LibraryTypeId.ToString() );
            this.SetListSelection( this.ddlPublicAccessLevel, entity.PublicAccessLevelInt.ToString() );
            this.SetListSelection( this.ddlOrgAccessLevel, entity.OrgAccessLevelInt.ToString() );

            this.rblIsDiscoverable.SelectedValue = this.ConvertBoolToYesNo( entity.IsDiscoverable );
            this.rblIsActive.SelectedValue = this.ConvertBoolToYesNo( entity.IsActive );
            this.rblAllowJoinRequest.SelectedValue = this.ConvertBoolToYesNo( entity.AllowJoinRequest );
            
			currentFileName.Text = entity.ImageUrl;
            noLibraryImagelabel.Visible = true;

            if ( entity.Id > 0 )
            {
                //determine rights
                int CurrentLibraryMemberTypeId = 0;
                if ( entity.IsMyPersonalLibrary( WebUser.Id ) )
                {
                    CurrentLibraryMemberTypeId = LibraryMember.LIBRARY_MEMBER_TYPE_ID_ADMIN;
                }
                else
                {
                    LibraryMember lm = myManager.LibraryMember_Get( entity.Id, WebUser.Id );
                    if ( lm != null && lm.MemberTypeId > LibraryMember.LIBRARY_MEMBER_TYPE_ID_READER )
                    {
                        CurrentLibraryMemberTypeId = lm.MemberTypeId;
                    }
                }

                if ( CurrentLibraryMemberTypeId == LibraryMember.LIBRARY_MEMBER_TYPE_ID_ADMIN )
                    FormPrivileges.DeletePrivilege = 3;

                viewLibraryLink.NavigateUrl = string.Format("/Libraries/Library.aspx?id={0}", entity.Id);
                viewLibraryLink.Visible = true;
                if ( entity.OrgId > 0 )
                {
                    orgPanel.Visible = true;
                    lblOrganization.Text = entity.Organization;
                    lblOrganization.Visible = true;
                    ddlOrgs.Visible = false;
                }
                if (entity.LibraryTypeId == 1)
                    orgPanel.Visible = false;
                this.lblHistory.Text = entity.HistoryTitle();
                libraryName.Text = "Update Library: " + entity.Title;
                if ( entity.ImageUrl.Length > 10 )
                {
                    noLibraryImagelabel.Visible = false;
                    FormatImageTag( entity.ImageUrl );
                }
                else
                {
                    //show template
                    string defaultLibraryImage = UtilityManager.GetAppKeyValue( "defaultLibraryImage", "" );
                    if ( defaultLibraryImage.Length > 10 )
                        FormatImageTag( defaultLibraryImage );
                }
                ddlLibraryType.Enabled = false;
                if ( FormPrivileges.CanDelete()  )
                {
                    btnDelete.Visible = true;
                }

                this.lblLibraryDisplay.Text = entity.LibrarySummaryFormatted( "isleBox_H2" );

                //TODO - update for curators
                //Assuming that if user can get this far, has already been vetted
                //
                //    && WebUser.Id == entity.CreatedById
                if ( IsUserAuthenticated() )
                {
                    btnSave.Enabled = true;
                    btnSave.Visible = true;
                }
                else
                {
                    btnSave.Visible = false;
                    detailsPanel.Enabled = false;
                }
            }
            else
            {
                //reset controls or state for empty object
                //ex: reset a dropdownlist not handled by 
                this.lblHistory.Text = "";
                //if preset, don't allow changes
                if ( entity.LibraryTypeId > 0 )
                    ddlLibraryType.Enabled = false;

                btnDelete.Visible = false;
                detailsPanel.Enabled = true;
				currentImage.Text = "";
            }

        }//
        #endregion

        #region Events
        /// <summary>
        /// Handle a form button clicked 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        public void FormButton_Click( Object sender, CommandEventArgs ev )
        {
            switch ( ev.CommandName )
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
                case "Delete":
                    this.DeleteRecord();
                    break;
            }
        } // end 
		
        #endregion

        #region Form Actions

        /// <summary>
        /// Prepare for a new record request
        /// </summary>
        private void HandleNewRequest()
        {
            this.ResetForm();

        } // end 

        /// <summary>
        /// Reset form values
        /// </summary>
        private void ResetForm()
        {
            //reset form by populating with an empty business object
            ThisLibrary entity = new ThisLibrary();
            //do any defaults. 
            entity.IsActive = true;
            entity.PublicAccessLevel = EObjectAccessLevel.ReadOnly;
            entity.OrgAccessLevel = EObjectAccessLevel.ContributeWithApproval;

            //set default lib type, if panel hidden???
            //actually cannot create personal - or maybe can, need default type
            entity.LibraryTypeId = DefaultLibraryTypeId;

            entity.HasChanged = false;
            PopulateForm( entity );
        }//

        #region Validation Methods
        /// <summary>
        /// validate form content
        /// </summary>
        /// <returns></returns>
        private bool IsFormValid()
        {
            bool isValid = true;

            try
            {
                vsErrorSummary.Controls.Clear();

                //Page.Validate();


                //other edits

                //recId = int.Parse( this.Id.Text );

                // check additional required fields
                isValid = HasRequiredFields();
                if ( isValid )
                    isValid = DoValidations();
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
            // check required fields
            if ( this.ddlLibraryType.SelectedIndex < 1 )
            {
                SetConsoleErrorMessage( "Please select a library type" );
                isValid = false;
            }
            else
            {
                //15-02-02 mp - in the context of lib admin, there is not a select option, so there is always a select
                //                    && this.ddlOrgs.SelectedIndex < 1 
                int orgId = 0;
                if ( ddlOrgs.SelectedIndex > -1 )
                    orgId = Int32.Parse( ddlOrgs.SelectedValue );

                int libTypeId = Int32.Parse( ddlLibraryType.SelectedValue );
                if ( ddlOrgs.Visible == true
                    && orgId == 0 
                    && libTypeId == 2 )
                {
                    SetConsoleErrorMessage( "Please select the organization that will administer (own) this library" );
                    isValid = false;
                }
            }
            if ( this.txtDescription.Text.Trim().Length == 0 )
            {
                SetConsoleErrorMessage( "Please provide a library descripiton" );
                isValid = false;
            }
            if ( this.txtTitle.Text.Trim().Length == 0 )
            {
                SetConsoleErrorMessage( rfvTitle.ErrorMessage );
                isValid = false;
            }



            return isValid;
        } //
        
        private bool DoValidations()
        {
            bool isValid = true;
            int id = 0;
            Int32.TryParse( this.txtId.Text, out id );
            int maxFileSize = UtilityManager.GetAppKeyValue( "maxLibraryImageSize", 100000 );
            if ( !FileResourceController.IsFileSizeValid( fileUpload, maxFileSize ) )
            {
                SetConsoleErrorMessage( string.Format( "Error: File must be {0}KB or less.", ( maxFileSize / 1024 ) ) );
                isValid = false;

            }
            //if create
            if ( id == 0 )
            {
                if ( fileUpload.PostedFile.ContentType.IndexOf( "image/" ) != 0 )
                {
                    SetConsoleErrorMessage( "Error: You must select an image file." );
                    isValid = false;
                }
            }


            return isValid;
        } //
        private void AddReqValidatorError( ValidationSummary valSummary, string message, ref string controlName, bool setInvalid )
        {
            //            AddReqValidatorError( valSummary, message, controlName, false );
            SetConsoleErrorMessage( message );
            setInvalid = true;

        } //

        #endregion

        /// <summary>
        /// Handle form update
        /// </summary>
        private void UpdateForm()
        {
            int id = 0;
            //string msg = "";
            string action = "";
			string statusMessage = "";

            ThisLibrary entity = new ThisLibrary();

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
                    entity.LibraryTypeId = Int32.Parse( this.ddlLibraryType.SelectedItem.Value );
                    entity.RowId = Guid.NewGuid();
                }
                else
                {
                    // get db record
                    entity = myManager.Get(id);
                    action = "Update";
                }

                /* assign form fields 			 */
                entity.Title = FormHelper.SanitizeUserInput( this.txtTitle.Text );
                entity.Description = FormHelper.SanitizeUserInput( this.txtDescription.Text );

                entity.AllowJoinRequest = this.ConvertYesNoToBool( this.rblAllowJoinRequest.SelectedValue );
                entity.IsDiscoverable = this.ConvertYesNoToBool( this.rblIsDiscoverable.SelectedValue );
                entity.IsActive = this.ConvertYesNoToBool( this.rblIsActive.SelectedValue );

                if ( ddlPublicAccessLevel.SelectedIndex >= 0 )
                    entity.PublicAccessLevel = ( EObjectAccessLevel ) Int32.Parse( ddlPublicAccessLevel.SelectedValue );
                else
                    entity.PublicAccessLevel = EObjectAccessLevel.ReadOnly;
               

                if ( ddlOrgAccessLevel.SelectedIndex >= 0 )
                    entity.OrgAccessLevel = ( EObjectAccessLevel ) Int32.Parse( ddlOrgAccessLevel.SelectedValue );
                else
                    entity.OrgAccessLevel = EObjectAccessLevel.ReadOnly;

                if ( ddlOrgs.SelectedIndex > -1 )
                {
                    int orgId = Int32.Parse( ddlOrgs.SelectedValue );
                    if (orgId > 0)
                        entity.OrgId = orgId;
                }

                entity.LastUpdated = System.DateTime.Now;
                entity.LastUpdatedById = WebUser.Id;		//include for future use
                entity.LastUpdatedBy = WebUser.FullName();

				// if file exists, do upload and populate the entity as needed
                if ( IsFilePresent() )
                {
                    bool isValid = HandleUpload( entity, ref statusMessage );
                    if ( isValid == false)
                    {
                        SetConsoleErrorMessage( statusMessage );
                        return;
                    }
                }

                //call insert/update
                string actionMsg = "";
                int entityId = 0;
                if ( entity.Id == 0 )
                {
                    entityId = myManager.Create( entity, ref statusMessage );
                    if ( entityId > 0 )
                    {
                        this.txtId.Text = entityId.ToString();
                        entity.Id = entityId;
                        NewLibraryId = entityId;
                        //need to create the default collection
                        var defaultCollection = myManager.LibrarySection_CreateDefault( entityId, ref statusMessage );
                        actionMsg = "Successfully created the library and the default collection, and added you as an administrator.";
                        //add user as lib member admin
                        LibraryMember mbr = new LibraryMember();
                        mbr.ParentId = entity.Id;
                        mbr.UserId = WebUser.Id;
                        mbr.MemberTypeId = ( int ) ELibraryMemberType.Administrator;
                        mbr.SubscriptionTypeId = ( int ) ELibrarySubscriptionType.Reader;
                        mbr.CreatedById = WebUser.Id;

                        myManager.LibraryMember_Create( entity.Id,
                                        WebUser.Id,
                                        ( int ) ELibraryMemberType.Administrator,
                                        WebUser.Id, ref statusMessage );

                        PopulateForm( entity );
                        this.SetConsoleSuccessMessage( actionMsg );
                    }
                    else
                    {
                        this.SetConsoleErrorMessage( statusMessage );
                    }
                }
                else
                {
                    statusMessage = myManager.Update( entity );
                    if ( statusMessage.Equals( "successful" ) )
                    {
                        actionMsg = "Update was successful for: " + entity.Title;
                        this.SetConsoleSuccessMessage( actionMsg );
                        PopulateForm( entity );
                    } else
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
        /// Handle upload of file
        /// </summary>
        /// <param name="library"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        protected bool HandleUpload( ThisLibrary library, ref string statusMessage )
        {
            bool isValid = true;
            try
            {
                int libraryImageWidth = UtilityManager.GetAppKeyValue( "libraryImageWidth", 150 );

                string savingName = library.RowId.ToString().Replace( "-", "" ) + System.IO.Path.GetExtension( fileUpload.FileName );
                string savingFolder = FileResourceController.DetermineDocumentPath( library );
                string savingURL = FileResourceController.DetermineDocumentUrl( library, savingName );
                library.ImageUrl = savingURL;
                ImageStore img = new ImageStore();
                img.FileName = savingName;
                img.FileDate = DateTime.Now;

                FileResourceController.HandleImageResizingToWidth( img, fileUpload, libraryImageWidth, libraryImageWidth, true, true );
                FileSystemHelper.HandleDocumentCaching( savingFolder, img, true );
            }
            catch ( Exception ex )
            {
                statusMessage = ex.Message;
                LoggingHelper.LogError( ex, "LibraryMtce().HandleUpload" );
                isValid = false;
            }
            return isValid;
        }



		private void FormatImageTag( string imageUrl )
		{
            int libraryImageWidth = UtilityManager.GetAppKeyValue( "libraryImageWidth", 150 );
            currentFileName.Text = imageUrl;
            if ( showingImagePath.Text.Equals("yes"))
                currentFileName.Visible = true;
            int sec = DateTime.Now.Second;
            string v = "?v=" + sec.ToString();
            string img = string.Format( txtLibraryImageTemplate.Text, imageUrl + v, libraryImageWidth );
			currentImage.Text = img;
			currentImage.Visible = true;
		}//
		
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

        /// <summary>
        /// Delete a record
        /// </summary>
        private void DeleteRecord()
        {
            int id = 0;

            try
            {
                string statusMessage = "";
                id = int.Parse( this.txtId.Text );

                if ( myManager.Library_SetInactive( id, ref statusMessage ) )
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

        #endregion

        #region Housekeeping
        /// <summary>
        /// Populate form controls
        /// </summary>
        private void PopulateControls()
        {
            SetLibTypeList();
            SetAccessLevel();
        } //

        private void SetLibTypeList()
        {
            DataSet ds = myManager.SelectLibraryTypes();
            DataBaseHelper.PopulateList( ddlLibraryType, ds, "Id", "Title", "Select a library type" );
        }
        private void SetAccessLevel()
        {
            List<CodeItem> list = MyManager.GetCodes_LibraryAccessLevel();
            //DataSet ds = myManager.SelectLibraryTypes();
            DataBaseHelper.PopulateList( this.ddlPublicAccessLevel, list, "Id", "Title", "" );
            ddlPublicAccessLevel.SelectedIndex = 2;
            DataBaseHelper.PopulateList( this.ddlOrgAccessLevel, list, "Id", "Title", "" );
            ddlOrgAccessLevel.SelectedIndex = 3;
        }
        #endregion
    }
}
