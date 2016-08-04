using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using EmailHelper = ILPathways.Utilities.EmailManager;

using ILPlibrary = ILPathways.Library;
using ILPathways.Utilities;
using GDAL = Isle.BizServices;
using ThisEntity = ILPathways.Business.LibrarySection;
using MyManager = Isle.BizServices.LibraryBizService;
using DataBaseHelper = LRWarehouse.DAL.BaseDataManager;

namespace ILPathways.LRW.controls
{
    public partial class LibraryCollectionMtce : ILPlibrary.BaseUserControl
    {
        MyManager myManager = new MyManager();
        const string thisClassName = "LibraryCollectionMtce";
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
        public int DefaultSectionTypeId
        {
            get { return Int32.Parse( this.txtDefaultSectionTypeId.Text ); }
            set { txtDefaultSectionTypeId.Text = value.ToString(); }
        }
        public int CurrentLibraryId
        {
            get { return Int32.Parse( this.txtCurrentLibraryId.Text ); }
            set { txtCurrentLibraryId.Text = value.ToString(); }
        }
        public string LibraryTitle
        {
            get { return this.libraryName.Text; }
            set { libraryName.Text = value; }
        }

        #endregion

        /// <summary>
        /// Handle Page Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void Page_Load( object sender, EventArgs ex )
        {

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
            this.FormPrivileges = GDAL.SecurityManager.GetGroupObjectPrivileges( this.WebUser, FormSecurityName );

            if ( IsUserAuthenticated() && WebUser.UserName == "mparsons" )
            {
                FormPrivileges.SetAdminPrivileges();

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

        
        public void SetCollection( int id )
        {
            SetValidators( true );
            Get( id );
        }	// End 

        private void SetValidators( bool state )
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
            

            if ( id > 0 )
            {
                this.Get( id );

            }
            else
            {
                //or get personal library

                if ( btnNew.Enabled )
                    HandleNewRequest();
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
                ThisEntity entity = myManager.LibrarySectionGet( recId );

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
        private void PopulateForm( ThisEntity entity )
        {

            this.txtId.Text = entity.Id.ToString();
            this.txtLibraryId.Text = entity.LibraryId.ToString();
            if ( entity.LibraryId > 0 )
                CurrentLibraryId = entity.LibraryId;

            if ( entity.LibraryTitle.Length > 1)
                this.libraryName.Text = entity.LibraryTitle;

            this.txtTitle.Text = entity.Title;
            this.txtDescription.Text = entity.Description;

            this.SetListSelection( this.ddlSectionType, entity.SectionTypeId.ToString() );

            this.rblIsPublic.SelectedValue = this.ConvertBoolToYesNo( entity.IsPublic );
            this.rblIsDefaultSection.SelectedValue = this.ConvertBoolToYesNo( entity.IsDefaultSection );
            this.rblAreContentsReadOnly.SelectedValue = this.ConvertBoolToYesNo( entity.AreContentsReadOnly );


            if ( entity.Id > 0 )
            {
                this.lblHistory.Text = entity.HistoryTitle();
                ddlSectionType.Enabled = false;
                if ( entity.IsDefaultSection )
                {
                    //disable, cannot unset, only select as default
                    rblIsDefaultSection.Enabled = false;
                }
                if ( entity.AreContentsReadOnly )
                {
                    btnDelete.Visible = false;
                    this.btnSave.Visible = false;
                }

                else
                {
                    if ( FormPrivileges.CanDelete() && entity.IsDefaultSection == false )
                    {
                        btnDelete.Visible = true;
                    }

                    if ( IsUserAuthenticated() && WebUser.Id == entity.CreatedById )
                    {
                        btnSave.Enabled = true;
                        btnSave.Visible = true;
                    }
                    else
                    {
                        btnSave.Visible = false;
                        detailsPanel.Enabled = false;
                        btnDelete.Visible = false;
                    }
                }
            }
            else
            {
                //reset controls or state for empty object
                //ex: reset a dropdownlist not handled by 
                this.lblHistory.Text = "";
               
                btnDelete.Visible = false;
                this.btnSave.Visible = true;
                rblIsDefaultSection.Enabled = true;
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
            ThisEntity entity = new ThisEntity();
            //do any defaults. 
            entity.IsActive = true;
            entity.LibraryId = CurrentLibraryId;

            entity.SectionTypeId = DefaultSectionTypeId;
            entity.AreContentsReadOnly = false;
            entity.IsDefaultSection = false;
            entity.IsPublic = true;

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

            ThisEntity entity = new ThisEntity();

            try
            {

                id = int.Parse( this.txtId.Text );

                if ( id == 0 )
                {
                    entity.Id = 0;
                    entity.CreatedById = WebUser.Id;		//include for future use
                    entity.CreatedBy = WebUser.FullName();
                    entity.Created = System.DateTime.Now;
                    action = "Create";
                    entity.SectionTypeId = this.DefaultSectionTypeId;
                }
                else
                {
                    // get db record
                    entity = myManager.LibrarySectionGet( id );
                    action = "Update";
                }

                /* assign form fields 			 */
                entity.Title = FormHelper.SanitizeUserInput( this.txtTitle.Text );
                entity.Description = FormHelper.SanitizeUserInput( this.txtDescription.Text );

                entity.IsPublic = this.ConvertYesNoToBool( this.rblIsPublic.SelectedValue );
                entity.AreContentsReadOnly = this.ConvertYesNoToBool( this.rblAreContentsReadOnly.SelectedValue );
                entity.IsDefaultSection = this.ConvertYesNoToBool( this.rblIsDefaultSection.SelectedValue );

                entity.LastUpdated = System.DateTime.Now;
                entity.LastUpdatedById = WebUser.Id;		//include for future use
                entity.LastUpdatedBy = WebUser.FullName();


                //call insert/update
                string statusMessage = "";
                int entityId = 0;
                if ( entity.Id == 0 )
                {
                    entityId = myManager.LibrarySectionCreate( entity, ref statusMessage );
                    if ( entityId > 0 )
                    {
                        this.txtId.Text = entityId.ToString();
                        entity.Id = entityId;
                    }

                }
                else
                {
                    statusMessage = myManager.LibrarySectionUpdate( entity );
                }


                if ( statusMessage.Equals( "successful" ) )
                {

                    //update CurrentRecord and form
                    PopulateForm( entity );

                    this.SetConsoleSuccessMessage( action + " was successful for: " + entity.Title );
                    //do a notify so parent can refresh!
                    Session[ "" ] = "";
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
        /// Delete a record
        /// </summary>
        private void DeleteRecord()
        {
            int id = 0;

            try
            {
                string statusMessage = "";
                id = int.Parse( this.txtId.Text );

                if ( myManager.LibrarySection_Delete( id, ref statusMessage ) )
                {
                    this.SetConsoleSuccessMessage( "Delete of collection was successful" );
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

            DataSet ds = myManager.SelectLibrarySectionTypes();
            DataBaseHelper.PopulateList( this.ddlSectionType, ds, "Id", "Title", "Select a section type" );
        }
        #endregion
    }
}