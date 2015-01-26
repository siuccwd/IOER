using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using ILPathways.Utilities;
//using ILPathways.DAL;
using ILPathways.Business;
using ILPathways.Common;
using ILPathways.Controllers;
using BDM = LRWarehouse.DAL.BaseDataManager;
using MyManager = Isle.BizServices.SecurityManager;
using UserManager = Isle.BizServices.AccountServices;
using GroupManager = Isle.BizServices.GroupServices;
using MyGroupController = ILPathways.Controllers.GroupsManagementController;

namespace ILPathways.Controls.GroupsManagment
{
    public partial class GroupPrivileges : GroupsManagementController
    {
        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
        const string thisClassName = "GroupPrivileges";
        MyManager mgr = new MyManager();

        #region Properties
        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string FormSecurityName
        {
            get { return this.formSecurityName.Text; }
            set { this.formSecurityName.Text = value; }
        }

        /// <summary>
        /// Get/Set CurrentGroupSessionCode - used to create a unique session variable name
        /// </summary>
        public string CurrentGroupSessionCode
        {
            get { return this._currentGroupSessionCode; }
            set { this._currentGroupSessionCode = value; }
        }
        private string _currentGroupSessionCode = GroupsManagementController.CONTROLLER_CURRENT_GROUP;

        /// <summary>
        /// INTERNAL PROPERTY: CurrentRecord
        /// Set initially and store in ViewState
        /// </summary>
        protected ApplicationGroupPrivilege CurrentRecord
        {
            get { return ViewState[ "CurrentRecord" ] as ApplicationGroupPrivilege; }
            set { ViewState[ "CurrentRecord" ] = value; }
        }
        /// <summary>
        /// ParentGroupId
        /// - used if control used separately, not in a tab/accordion
        /// </summary>
        protected int ParentGroupId
        {
            get
            {
                if ( ViewState[ "ParentGroupId" ] == null )
                    ViewState[ "ParentGroupId" ] = 0;
                return Int32.Parse( ViewState[ "ParentGroupId" ].ToString() );
            }
            set { ViewState[ "ParentGroupId" ] = value.ToString(); }
        }
        /// <summary>
        /// Store retrieve whether the parent record just changed
        /// </summary>
        protected bool DidParentRecordChangeXX
        {
            get { return bool.Parse( Session[ "sessionDidGroupIdChange" ].ToString() ); }
            set { Session[ "sessionDidGroupIdChange" ] = value; }
        }

        #endregion

        protected void Page_Load( object sender, EventArgs e )
        {
            this.InstanceGroupCode = CurrentGroupSessionCode;
            //get current user
            CurrentUser = GetAppUser();

            if ( !Page.IsPostBack )
            {
                this.InitializeForm();
            }
        }
        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        private void InitializeForm()
        {
            //
            this.FormPrivileges = MyManager.GetGroupObjectPrivileges( this.WebUser, FormSecurityName );

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
                this.btnDelete.Visible = true;
                this.btnDelete.Enabled = false;
            }

            saveNewPrivilegeButton.Visible = btnSave.Enabled;
            copyPrivilegeButton.Visible = btnSave.Enabled;
            //

            if ( btnNew.Enabled )
                HandleNewRequest();

            // Set source for form lists
            this.PopulateControls();

        }	// End 
        /// <summary>
        /// Check for a parent selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_PreRender( object sender, EventArgs e )
        {

            try
            {
                if ( CurrentGroupExists() )
                {
                    if ( CurrentGroup.Id > 0 )
                    {
                        objectPrivilegesPanel.Visible = true;
                        if ( CurrentGroup.Id.ToString() != txtGroupId.Text )
                        {
                            txtGroupId.Text = CurrentGroup.Id.ToString();
                            SetFormSelectList( CurrentGroup.Id );
                            SetObjectsList( CurrentGroup.Id );
                            ResetForm();

                        }
                        else
                        {
                            //OK, same record
                        }
                    }
                    else
                    {
                        SetFormSelectList( 9999 );
                        HandleNewRequest();
                    }
                }
                else
                {
                    //no group, lock down
                    objectPrivilegesPanel.Visible = false;
                    this.txtGroupId.Text = "0";

                }

            }
            catch
            {
                //no action
            }

        }//

        /// <summary>
        /// Handle selection from form list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void lstForm_SelectedIndexChanged( object sender, System.EventArgs ea )
        {

            try
            {
                //LastActiveAccordianPane = GroupsManagementController.CONTROLLER_PANE_PRIVILEGES;
                int id = int.Parse( this.lstForm.SelectedValue );

                this.Get( id );

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
        private void Get( int recId )
        {
            try
            {
                //get record
                ApplicationGroupPrivilege entity = MyManager.AppGroupObjectPrivileges_Get( recId );

                if ( entity == null || entity.Id == 0 )
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
        private void PopulateForm( ApplicationGroupPrivilege entity )
        {
            CurrentRecord = entity;

            txtId.Text = entity.Id.ToString();
            txtGroupId.Text = entity.GroupId.ToString();

            //this.SetListSelection( ddlObject, entity.ObjectId.ToString() );

            this.SetListSelection( ddlCreatePriv, entity.CreatePrivilege.ToString() );
            this.SetListSelection( ddlReadPriv, entity.ReadPrivilege.ToString() );
            this.SetListSelection( ddlUpdatePriv, entity.WritePrivilege.ToString() );
            this.SetListSelection( ddlDeletePriv, entity.DeletePrivilege.ToString() );

            this.SetListSelection( ddlAssignPriv, entity.AssignPrivilege.ToString() );
            this.SetListSelection( ddlAppendPriv, entity.AppendPrivilege.ToString() );
            this.SetListSelection( ddlApprovePriv, entity.ApprovePrivilege.ToString() );

            if ( entity.ObjectId > 0 )
            {
                lblObjectName.Text = entity.DisplayName;
                lblObjectName.Visible = true;
                //don't allow change of object for existing privilege
                ddlObject.Enabled = false;
                ddlObject.Visible = false;

                if ( FormPrivileges.CanDelete() )
                {
                    this.btnDelete.Enabled = true;
                }
                saveNewPrivilegeButton.Enabled = true;
                copyPrivilegeButton.Enabled = true;
            }
            else
            {
                ddlObject.Enabled = true;
                ddlObject.Visible = true;
                lblObjectName.Text = "";
                lblObjectName.Visible = false;

                this.btnDelete.Enabled = false;
                saveNewPrivilegeButton.Enabled = true;
                copyPrivilegeButton.Enabled = false;
            }

            refreshObjectsButton.Visible = ddlObject.Visible;
        }//

        /// <summary>
        /// Handle a form button clicked 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        public void FormButton_Click( Object sender, CommandEventArgs ex )
        {
            //TODO - these hard-coded indices assume all panes are visible. this refers to the 4th pane, but if an auth group,only 3 panes actually show!!
           // LastActiveAccordianPane = GroupsManagementController.CONTROLLER_PANE_PRIVILEGES;

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
                case "SaveNewPrivilege":
                    if ( this.IsFormValid() )
                    {
                        this.UpdateForm();

                        this.ResetForm();
                    }
                    break;
                case "Delete":
                    this.DeleteRecord();
                    break;
                case "CopyPrivilege":
                    this.PrivilegeCopyRequest();
                    break;
                case "refreshObjects":
                    this.SetObjectsList();
                    break;

            }
        } // end 

        /// <summary>
        /// Prepare for a new record request
        /// </summary>
        private void HandleNewRequest()
        {
            this.ResetForm();

        } // end 

        #region Validation Methods
        /// <summary>
        /// validate form content
        /// </summary>
        /// <returns></returns>
        private bool IsFormValid()
        {
            int recId = 0;
            bool isValid = true;

            try
            {

                vsErrorSummary.Controls.Clear();

                if ( ddlObject.Visible == true && this.ddlObject.SelectedIndex < 1 )
                {
                    this.AddReqValidatorError( vsErrorSummary, this.rfvObject.ErrorMessage, "ddlObject", true );
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

        #endregion

        /// <summary>
        /// Copy current privilege and enable the object dropdown
        /// </summary>
        protected void PrivilegeCopyRequest()
        {
            // get current record from session
            ApplicationGroupPrivilege entity = CurrentRecord;
            entity.Id = 0;
            entity.ObjectId = 0;

            PopulateForm( entity );

        } //


        /// <summary>
        /// Handle form update
        /// </summary>
        private void UpdateForm()
        {
            int id = 0;
            string msg = "";
            string action = "";
            bool isUpdate;

            ApplicationGroupPrivilege entity = new ApplicationGroupPrivilege();

            try
            {
                id = int.Parse( this.txtId.Text );

                if ( id == 0 )
                {
                    isUpdate = false;
                    entity.Id = 0;
                    entity.GroupId = GetParentId();
                    entity.ObjectId = Int32.Parse( ddlObject.SelectedValue );
                    entity.DisplayName = ddlObject.SelectedItem.Text;
                    entity.CreatedById = WebUser.Id;
                    entity.Created = System.DateTime.Now;
                    action = "Create";

                }
                else
                {
                    // get current record from viewstate
                    entity = CurrentRecord;
                    isUpdate = true;
                    action = "Update";
                }
                /*
                 * //assign form fields
                 */

                entity.LastUpdated = System.DateTime.Now;
                entity.LastUpdatedById = WebUser.Id;

                entity.CreatePrivilege = Int32.Parse( ddlCreatePriv.SelectedValue );
                entity.ReadPrivilege = Int32.Parse( ddlReadPriv.SelectedValue );
                entity.WritePrivilege = Int32.Parse( ddlUpdatePriv.SelectedValue );
                entity.DeletePrivilege = Int32.Parse( ddlDeletePriv.SelectedValue );
                entity.AppendPrivilege = Int32.Parse( this.ddlAppendPriv.SelectedValue );
                entity.AssignPrivilege = Int32.Parse( this.ddlAssignPriv.SelectedValue );
                entity.ApprovePrivilege = Int32.Parse( this.ddlApprovePriv.SelectedValue );

                //call insert/update
                string statusMessage = "";
                int entityId = 0;
                if ( entity.Id == 0 )
                {
                    entityId = MyManager.AppGroupObjectPrivileges_Create( entity, ref statusMessage );
                    if ( entityId > 0 )
                    {
                        this.txtId.Text = entityId.ToString();
                        entity.Id = entityId;
                    }

                }
                else
                {
                    statusMessage = MyManager.AppGroupObjectPrivileges_Update( entity );
                }


                if ( statusMessage.Equals( "successful" ) )
                {

                    //update CurrentRecord and form
                    Get( entity.Id );

                    this.SetConsoleSuccessMessage( action + " was successful for: " + entity.DisplayName );
                    this.SetFormSelectList();
                    SetObjectsList();
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

                if ( MyManager.AppGroupObjectPrivileges_Delete( id, ref statusMessage ) )
                {
                    this.SetConsoleSuccessMessage( "Delete of record was successful" );
                    //reset lists as needed for removed records
                    this.SetFormSelectList();
                    SetObjectsList();
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
        /// Reset form values
        /// </summary>
        private void ResetForm()
        {
            //reset form by populating with an empty business object
            ApplicationGroupPrivilege entity = new ApplicationGroupPrivilege();
            if ( CurrentGroup != null && CurrentGroup.Id > 0 )
                entity.GroupId = CurrentGroup.Id;

            PopulateForm( entity );

            //unselect forms list
            lstForm.SelectedIndex = -1;
        }//
        /// <summary>
        /// Find the parentId for this accordian group 
        /// </summary>
        private int GetParentId()
        {
            int groupId = 0;

            //find courseId and update
            Label txtCurrentParentId = ( Label ) FindChildControl( Page, "txtCurrentParentId" );
            try
            {
                if ( txtCurrentParentId != null )
                {
                    groupId = int.Parse( txtCurrentParentId.Text.ToString() );
                }

            }
            catch
            {
                //no action
            }

            return groupId;
        }
        /// <summary>
        /// Populate form controls
        /// </summary>
        /// <param name="recId"></param>
        private void PopulateControls()
        {

            //get objects
            SetObjectsList();

            //SetFormSelectList(0);
            PopulatePrivilegeControls();
        } //

        private void SetObjectsList()
        {
            SetObjectsList( CurrentGroup.Id );

        } //

        private void SetObjectsList( int groupId )
        {
            //string sql = string.Format( this.selectAvailableObjectsSql.Text, groupId );
            //DataSet ds = DatabaseManager.DoQuery( sql );
            ////DataSet ds = WorkNetObjectManager.Select();
            //MyManager.PopulateList( ddlObject, ds, "Id", "DisplayName", "Select an Object" );

        } //

        /// <summary>
        /// populate the main form list
        /// </summary>
        private void SetFormSelectList()
        {
            int groupId = GetParentId();
            SetFormSelectList( groupId );

        } //
        /// <summary>
        /// populate the main form list
        /// </summary>
        private void SetFormSelectList( int groupId )
        {

            //get current privileges using current group
            //DataSet ds = MyManager.Select( groupId, 0 );
            //MyManager.PopulateList( lstForm, ds, "Id", "DisplayName", "Select an Existing Object" );

            //now set only to those not in current list
            //TBD
        } //

        protected bool PopulatePrivilegeControls()
        {
            //

            //get privilege levels
            List<CodeItem> list = MyManager.Codes_PrivilegeDepth_Select();

            BDM.PopulateList( this.ddlCreatePriv, list, "Id", "Title", "" );
            BDM.PopulateList( this.ddlReadPriv, list, "Id", "Title", "" );
            BDM.PopulateList( this.ddlUpdatePriv, list, "Id", "Title", "" );
            BDM.PopulateList( this.ddlDeletePriv, list, "Id", "Title", "" );
            BDM.PopulateList( this.ddlAppendPriv, list, "Id", "Title", "" );
            BDM.PopulateList( this.ddlApprovePriv, list, "Id", "Title", "" );
           
            //

            return true;
        } //
    }

}