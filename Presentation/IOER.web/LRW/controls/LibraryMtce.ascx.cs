using System;
using System.Collections;
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
using ILPathways.Utilities;
using GDAL = Isle.BizServices;
using ILPathways.Business;
using ThisLibrary = ILPathways.Business.Library;
using MyManager = Isle.BizServices.LibraryBizService;
using ContentManager = Isle.BizServices.ContentServices;
using DataBaseHelper = LRWarehouse.DAL.BaseDataManager;

namespace ILPathways.LRW.controls
{
    public partial class LibraryMtce : ILPlibrary.BaseUserControl
    {
        MyManager myManager = new MyManager();
        ContentManager myContentManager  = new ContentManager();
        const string thisClassName = "LibraryMtce";


        #region Properties
        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string FormSecurityName
        {
            get { return this.txtFormSecurityName.Text; }
            set { this.txtFormSecurityName.Text = value; }
        }

        //if text is needed to be populated at load and available after postback, one option can be to save in a control in the ascx file.
        public int DefaultLibraryTypeId
        {
            get { return Int32.Parse( txtDefaultLibraryTypeId.Text ); }
            set { txtDefaultLibraryTypeId.Text = value.ToString(); }
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
                CurrentUser = GetAppUser();
            }
            else
            {
                //set message - only do where clearly on form by itself.
                //or just disable, ==> done in populate as previously end user could see the detail
            }
            if ( Page.IsPostBack == false )
            {
                this.InitializeForm();
            }
        }//

        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        private void InitializeForm()
        {
          
            // get form privileges - only admin, owner or curator anyway
            this.FormPrivileges = GDAL.SecurityManager.GetGroupObjectPrivileges( this.CurrentUser, FormSecurityName );

            if ( IsUserAuthenticated() && CurrentUser.UserName == "mparsons" )
            {
                FormPrivileges.SetAdminPrivileges();
                showingImagePath.Text = "yes";

            }
            //= Add attribute to btnDelete to allow client side confirm
            btnDelete.Attributes.Add( "onClick", "return confirmDelete(this);" );

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
                this.btnDelete.Visible = false;
                this.btnDelete.Enabled = true;
            }

            SetValidators( false );
            // Set source for form lists
            this.PopulateControls();

            //optional:
            CheckRecordRequest();

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
                CurrentUser = GetAppUser();
                entity.Title = "My Personal library: " + CurrentUser.FullName();
                entity.Description = "Personal library for " + CurrentUser.FullName();

            }
            entity.LibraryTypeId = DefaultLibraryTypeId;

            entity.HasChanged = false;
            this.btnNew.Visible = false;
            btnSave.Enabled = true;

            PopulateForm( entity );

        }	// End 

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
        private void Get( int recId )
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

            this.txtId.Text = entity.Id.ToString();
            this.txtTitle.Text = entity.Title;
            this.txtDescription.Text = entity.Description;
            this.txtDocumentRowId.Text = entity.DocumentRowId.ToString();

            this.SetListSelection( this.ddlLibraryType, entity.LibraryTypeId.ToString() );
            this.rblIsPublic.SelectedValue = this.ConvertBoolToYesNo( entity.IsPublic );
            this.rblIsDiscoverable.SelectedValue = this.ConvertBoolToYesNo( entity.IsDiscoverable );
            this.rblIsActive.SelectedValue = this.ConvertBoolToYesNo( entity.IsActive );
			currentFileName.Text = entity.ImageUrl;
            noLibraryImagelabel.Visible = true;

            if ( entity.Id > 0 )
            {
                this.lblHistory.Text = entity.HistoryTitle();
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
                if ( FormPrivileges.CanDelete() 
                    && entity.LibraryTypeId != ThisLibrary.PERSONAL_LIBRARY_ID )
                {
                    btnDelete.Visible = true;
                }

                this.lblLibraryDisplay.Text = entity.LibrarySummaryFormatted( "isleBox_H2" );

                //TODO - update for curators
                if ( IsUserAuthenticated()
                    && CurrentUser.Id == entity.CreatedById )
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
                if ( entity.LibraryTypeId == ThisLibrary.PERSONAL_LIBRARY_ID )
                    ddlLibraryType.Enabled = false;
                else
                    ddlLibraryType.Enabled = true;
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

                Page.Validate();


                //other edits

                //recId = int.Parse( this.Id.Text );

                // check additional required fields
                isValid = HasRequiredFields();

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
            rfvTitle.Validate();
            if ( rfvTitle.IsValid == false )
            {
                //NOTE: this may not be necessary anylonger if validator messages are properly picked up by all uplevel browsers!!!!
                this.AddReqValidatorError( vsErrorSummary, rfvTitle.ErrorMessage, "txtTitle" );
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
                    entity.CreatedById = CurrentUser.Id;		//include for future use
                    entity.CreatedBy = CurrentUser.FullName();
                    entity.Created = System.DateTime.Now;
                    action = "Create";
                    entity.LibraryTypeId = Int32.Parse( this.ddlLibraryType.SelectedItem.Value );

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

                entity.IsPublic = this.ConvertYesNoToBool( this.rblIsPublic.SelectedValue );
                entity.IsDiscoverable = this.ConvertYesNoToBool( this.rblIsDiscoverable.SelectedValue );
                entity.IsActive = this.ConvertYesNoToBool( this.rblIsActive.SelectedValue );
 
                entity.LastUpdated = System.DateTime.Now;
                entity.LastUpdatedById = CurrentUser.Id;		//include for future use
                entity.LastUpdatedBy = CurrentUser.FullName();

				// if file exists, check
                if ( IsFilePresent() )
                {
                    bool isValid = HandleDocument( entity, false, ref statusMessage );
                    if ( isValid )
                    {
                        //will have been set in HandleDocument
                        //not storing in db yet
                        entity.DocumentRowId = entity.RelatedDocument.RowId;
                    }
                    else
                    {
                        //problem, should have displayed a message
                        return;
                    }
                }
                else
                {
                    entity.ImageUrl = currentFileName.Text;
                }
                //call insert/update

                int entityId = 0;
                if ( entity.Id == 0 )
                {
                    entityId = myManager.Create( entity, ref statusMessage );
                    if ( entityId > 0 )
                    {
                        this.txtId.Text = entityId.ToString();
                        entity.Id = entityId;
                    }

                }
                else
                {
                    statusMessage = myManager.Update( entity );
                }


                if ( statusMessage.Equals( "successful" ) )
                {

                    //update CurrentRecord and form
                    PopulateForm( entity );

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

		protected bool HandleDocument( ThisLibrary parentEntity, bool uploadOnly, ref string statusMessage )
		{
			bool isValid = true;
			string documentRowId = "";

			string action = "";
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
                || uploadOnly == true )
            {
                entity.RowId = new Guid();

                entity.CreatedById = CurrentUser.Id;
                entity.CreatedBy = CurrentUser.FullName();
                entity.Created = System.DateTime.Now;
                action = "Create";

            }
            else
            {

                entity = myContentManager.DocumentVersionGet( documentRowId );
                action = "Update";
            }

            entity.LastUpdatedById = CurrentUser.Id;
            entity.Title = this.txtFileTitle.Text;

			string filename = "";
			//check if file was specified (versus updating text for existing file)
			if ( IsFilePresent() )
			{
				try
				{
                    
					int maxFileSize = UtilityManager.GetAppKeyValue( "maxLibraryImageSize", 100000 );
                    int libraryImageWidth = UtilityManager.GetAppKeyValue( "libraryImageWidth", 150 );
					//TODO - if an image, do a resize
					if ( FileResourceController.IsFileSizeValid( fileUpload, maxFileSize ) == false )
					{
						SetConsoleErrorMessage( string.Format( "Error the selected file exceeds the size limits ({0} bytes).", maxFileSize ) );
						return false;
					}

					filename = fileUpload.FileName;
                    entity.MimeType = fileUpload.PostedFile.ContentType;
                    //Need a filesize check
                    //probably want to fix filename to standardize
                    entity.FileName = filename;
                    entity.FileDate = System.DateTime.Now;

					string sFileType = System.IO.Path.GetExtension( filename );
                    string justName = System.IO.Path.GetFileNameWithoutExtension( filename );
					sFileType = sFileType.ToLower();

                    //filename = System.IO.Path.ChangeExtension( filename, sFileType );

                    ////setup parameters for new file name

                    //filename = filename.Replace( " ", "_" );
                    //filename = filename.Replace( "'", "" );
                    //filename = filename.Replace( "/", "_" );
                    //filename = filename.Replace( "\\", "_" );
                    //filename = filename.Replace( "#", "_" );
                    //use the current user rowId, so as to overwrite existing
                    filename = CurrentUser.RowId.ToString().Replace( "-", "" ) + sFileType;
					//LoggingHelper.DoTrace( 5, thisClassName + " HandleDocument(). calling DetermineDocumentPath" );
					string documentFolder = FileResourceController.DetermineDocumentPath( parentEntity );
					// LoggingHelper.DoTrace( 5, thisClassName + " - uploading to " + documentFolder );
					string url = FileResourceController.DetermineDocumentUrl( parentEntity, filename );
					parentEntity.ImageUrl = url;
					
					//LoggingHelper.DoTrace( 5, thisClassName + " HandleDocument(). calling CreateDirectory" );


					//handle image size
					ImageStore img = new ImageStore();
					img.ImageFileName = filename;
					//TODO - add target h and w to config
					//note, also this method doesn't handle too tall thin images yet.
                   // FileResourceController.HandleImageResizing( img, fileUpload, libraryImageWidth, libraryImageWidth );
                    FileResourceController.HandleImageResizingToWidth( img, fileUpload, libraryImageWidth, libraryImageWidth, true, true );

					ValidateDocumentOnServer( parentEntity, img );
                    FormatImageTag( url );
					//try separating following to insulate from file privileges issues
					//UploadFile( documentFolder, filename );

					//rewind for db save

                    if ( img.Bytes > 0 )
                    {
                        entity.ResourceBytes = img.Bytes;
                        entity.SetResourceData( entity.ResourceBytes, img.ResourceData );
                    }
                    else
                    {
                        //the following is left, but is wrong as stream is 
                        fileUpload.PostedFile.InputStream.Position = 0;
                        Stream fs = fileUpload.PostedFile.InputStream;

                        entity.ResourceBytes = fs.Length;
                        byte[] data = new byte[ fs.Length ];
                        fs.Read( data, 0, data.Length );
                        fs.Close();
                        fs.Dispose();
                        entity.SetResourceData( entity.ResourceBytes, data );
                    }
				}
				catch ( Exception ex )
				{
					SetConsoleErrorMessage( "Unexpected error occurred while attempting to upload your file.<br/>" + ex.Message );
					return false;
				}
			}
			this.currentFileName.Text = entity.FileName;

			if ( uploadOnly )
			{
				//lblDocumentLink.NavigateUrl = attachment.ResourceUrl;
				//lblDocumentLink.Text = "View: " + entity.FileName;
				//lblDocumentLink.Visible = true;

			}
			else
			{
				entity.FileName = filename;
                if ( entity.RowId == null || entity.HasValidRowId() == false )
                {
                    string documentId = myContentManager.DocumentVersionCreate( entity, ref statusMessage );
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
                        //lblDocumentLink.Visible = false;
                        isValid = false;
                    }
                }
                else
                {
                    statusMessage = myContentManager.DocumentVersionUpdate( entity );
                    if ( statusMessage.Equals( "successful" ) )
                    {

                        statusMessage = "Successfully updated Image!";
                    }
                    else
                    {
                        isValid = false;
                       // lblDocumentLink.Visible = false;
                    }

                }
			}

			if ( isValid )
			{
				//attachment.DocumentRowId = entity.RowId;
				//attachment.RelatedDocument = entity;

				//ValidateDocumentOnServer( parentEntity, entity );
			}

			return isValid;
		}//

		private string ValidateDocumentOnServer( ThisLibrary parentEntity, ImageStore doc )
		{
			string fileUrl = "";

			try
			{
				string documentFolder = FileResourceController.DetermineDocumentPath( parentEntity );
				string message = FileSystemHelper.HandleDocumentCaching( documentFolder, doc, true );
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
		private void UploadFile( string documentFolder, string filename )
		{
			try
			{
				FileSystemHelper.CreateDirectory( documentFolder );

				string diskFile = documentFolder + "\\" + filename;
				//string diskFile = MapPath( documentFolder ) + "\\" + entity.FileName;

				LoggingHelper.DoTrace( 5, thisClassName + " HandleDocument(). doing SaveAs" );
				fileUpload.SaveAs( diskFile );

			}
			catch ( Exception ex )
			{

			}
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

                if ( myManager.LibraryDelete( id, ref statusMessage ) )
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
        /// <param name="recId"></param>
        private void PopulateControls()
        {
            SetLibTypeList();

        } //

        private void SetLibTypeList()
        {

            DataSet ds = myManager.SelectLibraryTypes();
            DataBaseHelper.PopulateList( ddlLibraryType, ds, "Id", "Title", "Select a library type" );
        }
        #endregion
    }
}
