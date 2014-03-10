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

using ILPathways.Business;
using ILPathways.Common;
using ILPathways.Library;
using ILPathways.Utilities;
using GDAL = Isle.BizServices;

using MyManager = Isle.BizServices.OrganizationBizService;
using SecurityManager = Isle.BizServices.GroupServices;
using BDM = LRWarehouse.DAL.BaseDataManager;

namespace ILPathways.Controls.OrgMgmt
{
    /// <summary>
    /// manage organizations
    /// </summary>
    public partial class OrganizationMgmt : BaseUserControl
    {
        MyManager myManager = new MyManager();
        const string thisClassName = "OrganizationMgmt";


        #region Properties
        public bool InitializeOnRequest { get; set; }

        public int LocalOrgId
        {
            get { return Int32.Parse( this.currentOrgId.Text ); }
            set { this.currentOrgId.Text = value.ToString(); }
        }

        public int LastOrgId
        {
            get {
                if (Session[ "LastOrgId" ] == null)
                    Session[ "LastOrgId" ] = "0";

                return Int32.Parse( Session[ "LastOrgId" ].ToString() );
            }
            set { Session[ "LastOrgId" ] = value.ToString(); }
        }
        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string FormSecurityName
        {
            get { return this.formSecurityName.Text; }
            set { this.formSecurityName.Text = value; }
        }
        #endregion

        /// <summary>
        /// Handle Page Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void Page_Load( object sender, EventArgs ex )
        {
            if ( !IsUserAuthenticated() )
            {
                SetConsoleErrorMessage( "Error: you must be authenticated to view the organization management page." );
                Response.Redirect( "/", true );
            }
            WebUser = GetAppUser();

            if ( Page.IsPostBack == false )
            {
                this.InitializeForm();
            }
        }//

        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        public void InitializeForm()
        {

            if ( FormSecurityName.Length > 0 )
            {
                this.FormPrivileges = SecurityManager.GetGroupObjectPrivileges( this.WebUser, FormSecurityName );
            }
            else
            {
                //no security
                this.FormPrivileges = new ApplicationRolePrivilege();
                FormPrivileges.CreatePrivilege = 0;
                FormPrivileges.ReadPrivilege = 1;
                FormPrivileges.WritePrivilege = 0;
                FormPrivileges.DeletePrivilege = 0;
            }

            //= Add attribute to btnDelete to allow client side confirm
            btnDelete.Attributes.Add( "onClick", "return confirmDelete(this);" );

            //handling setting of which action buttons are available to the current user
            this.btnNew.Enabled = false;
            this.btnSave.Enabled = false;
            this.btnDelete.Visible = false;
            if ( FormPrivileges.CreatePrivilege > 1 )
            {
                this.btnSave.Enabled = true;
                btnNew.Visible = true;
                btnNew.Enabled = true;
            }
            // Set source for form lists
            this.PopulateControls();

            if ( InitializeOnRequest == false )
                CheckRecordRequest();
        }	// End 

        protected void Page_PreRender( object sender, EventArgs e )
        {
            try
            {
                //check for change in orgId
                if ( LastOrgId > 0 )
                {
                    if ( LastOrgId != LocalOrgId )
                    {
                        //will need to check privileges for update, delete
                        Get( LastOrgId );
                    }
                }
                else
                {
                    //ResetForm();
                }
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
            int id = this.GetRequestKeyValue( "id", 0 );
            //if PK is a Guid, use:
            string rid = this.GetRequestKeyValue( "rid", "" );

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
                if ( FormPrivileges.CreatePrivilege < 2 )
                {
                    //detailPanel.Visible = false;
                    //SetConsoleErrorMessage( "Error - no organization was selected, and you do not have privileges to create a new organization." );
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
                Organization entity = MyManager.EFGet( recId );

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
                Organization entity = MyManager.EFGetByRowId( recId );

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
        private void PopulateForm( Organization entity )
        {
            LocalOrgId = entity.Id;

            this.txtId.Text = entity.Id.ToString();
            this.txtName.Text = entity.Name;
            this.txtAddress1.Text = entity.Address1;
            this.txtAddress2.Text = entity.Address2;
            this.txtCity.Text = entity.City;
            this.txtZipcode.Text = entity.Zipcode;
            this.txtZipcodePlus4.Text = entity.ZipCode4;
            this.txtMainPhone.Text = entity.DisplayPhone( entity.MainPhone );
            this.txtMainExtension.Text = entity.MainExtension; 
            this.txtFax.Text = entity.Fax;

            this.SetListSelection( this.ddlOrgTypeId, entity.OrgTypeId.ToString() );
            this.SetListSelection( this.ddlState, entity.State );

            this.rblIsActive.SelectedValue = this.ConvertBoolToYesNo( entity.IsActive );


            if ( entity.Id > 0 )
            {
                //Members?

                //don't allow change to org type?
                if ( IsUserAuthenticated() )
                {
                    btnSave.Enabled = true;
                    btnSave.Visible = true;
                }
                else
                {
                    btnSave.Visible = false;
                    this.detailPanel.Enabled = false;
                }
            }
            else
            {
                //reset controls or state for empty object

                btnDelete.Visible = false;
                detailPanel.Enabled = true;
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
            LastOrgId = 0;
            this.ResetForm();

        } // end 

        /// <summary>
        /// Reset form values
        /// </summary>
        private void ResetForm()
        {
            //reset form by populating with an empty business object
            Organization entity = new Organization();
            //do any defaults. 
            entity.IsActive = true;
            entity.State = "IL";


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
            // check required fields - do in reverse order when using set console
  
            if ( this.ddlState.SelectedIndex < 1 )
            {
                SetConsoleErrorMessage( "Please select a state" );
                isValid = false;
            }
            if ( this.txtZipcodePlus4.Text.Trim().Length == 0 )
            {
                SetConsoleErrorMessage( "Please enter the zipcode plus 4" );
                isValid = false;
            }
            if ( this.txtZipcode.Text.Trim().Length == 0 )
            {
                SetConsoleErrorMessage( "Please enter a zipcode" );
                isValid = false;
            }
            if ( this.txtCity.Text.Trim().Length == 0 )
            {
                SetConsoleErrorMessage( "Please enter a city" );
                isValid = false;
            }
            if ( this.txtAddress1.Text.Trim().Length == 0 )
            {
                SetConsoleErrorMessage( "Please enter an address" );
                isValid = false;
            }
            if ( this.ddlOrgTypeId.SelectedIndex < 1 )
            {
                SetConsoleErrorMessage( "Please select an organization type" );
                isValid = false;
            }
            if ( this.txtName.Text.Trim().Length == 0 )
            {
                SetConsoleErrorMessage( rfvTitle.ErrorMessage );
                isValid = false;
            }



            return isValid;
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

            Organization entity = new Organization();

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
                    entity.RowId = Guid.NewGuid();
                }
                else
                {
                    // get db record
                    entity = MyManager.EFGet( id );
                    action = "Update";
                }

                /* assign form fields 			 */
                entity.Name = FormHelper.SanitizeUserInput( this.txtName.Text );
                entity.Address1 = FormHelper.SanitizeUserInput( this.txtAddress1.Text );
                entity.Address2 = FormHelper.SanitizeUserInput( this.txtAddress2.Text );
                entity.City = FormHelper.SanitizeUserInput( this.txtCity.Text );
                entity.ZipCode = FormHelper.SanitizeUserInput( this.txtZipcode.Text );
                entity.ZipCode4 = FormHelper.SanitizeUserInput( this.txtZipcodePlus4.Text );
                entity.MainPhone = FormHelper.SanitizeUserInput( this.txtMainPhone.Text );
                entity.MainExtension = FormHelper.SanitizeUserInput( this.txtMainExtension.Text );
                entity.State = this.ddlState.SelectedValue;

                entity.OrgTypeId = Int32.Parse( this.ddlOrgTypeId.SelectedValue );

                entity.IsActive = this.ConvertYesNoToBool( this.rblIsActive.SelectedValue );

                entity.LastUpdated = System.DateTime.Now;
                entity.LastUpdatedById = WebUser.Id;		//include for future use
                entity.LastUpdatedBy = WebUser.FullName();


                //call insert/update
                string actionMsg = "";
                int entityId = 0;
                if ( entity.Id == 0 )
                {
                    entityId = MyManager.Organization_Create( entity, ref statusMessage );
                    if ( entityId > 0 )
                    {
                        this.txtId.Text = entityId.ToString();
                        entity.Id = entityId;

                        PopulateForm( entity );
                        this.SetConsoleSuccessMessage( "Successfully created the organization" );
                    }
                    else
                    {
                        this.SetConsoleErrorMessage( statusMessage );
                    }
                }
                else
                {
                    if ( MyManager.Organization_Update( entity ))
                    {
                        actionMsg = "Update was successful for: " + entity.Name;
                        this.SetConsoleSuccessMessage( actionMsg );
                        PopulateForm( entity );
                    }
                    else
                        this.SetConsoleErrorMessage( "Update seems to have failed. System administration has been notified." );
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

                if ( MyManager.Organization_Delete( id, ref statusMessage ) )
                {
                    this.SetConsoleSuccessMessage( "Delete of record was successful" );
                    this.ResetForm();
                }
                else
                {
                    this.SetConsoleErrorMessage( statusMessage == null ? "Unexpected error encountered, system administration has been notified." : statusMessage );
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
            List<CodeItem> list = MyManager.OrgType_Select();
            BDM.PopulateList( this.ddlOrgTypeId, list, "Id", "Title", "Select an org. type" );

            List<CodeItem> states = MyManager.States_Select();
            BDM.PopulateList( this.ddlState, states, "Title", "Description", "Select a state" );

            //
            //InitializePageSizeList();
        } //

        #endregion

    }
}