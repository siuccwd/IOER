using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Business;
using ILPathways.Controllers;
using ILPathways.Utilities;
using GDAL = Isle.BizServices;
using MyManager = Isle.BizServices.GroupServices;
using UserManager = Isle.BizServices.AccountServices;
using GroupManager = Isle.BizServices.GroupServices;
using MyGroupController = ILPathways.Controllers.GroupsManagementController;
//using bizService = ILPathways.GatewayServiceReference;

using Group = ILPathways.Business.AppGroup;
using LDAL = LRWarehouse.DAL;
using BDM = LRWarehouse.DAL.BaseDataManager; 

namespace ILPathways.Controls.GroupsManagment
{
    public partial class GroupDetail : GroupsManagementController
    {

        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
        const string thisClassName = "Controls_GroupsManagement_GroupDetail";

        MyGroupController myc = new MyGroupController();

        #region properties
        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string FormSecurityName
        {
            get { return this.txtFormSecurityName.Text; }
            set { this.txtFormSecurityName.Text = value; }
        }

        /// <summary>
        /// private variable for GroupType. 
        /// </summary>
        private int _defaultGroupTypeId = AppGroup.GROUP_TYPEID_GENERAL;
        /// <summary>
        /// Get/Set Default GroupType property 
        /// </summary>
        public int DefaultGroupTypeId
        {
            get { return _defaultGroupTypeId; }

            set { _defaultGroupTypeId = value; }
        } //

        /// <summary>
        /// private variable for UseDefaultGroupCode. 
        /// </summary>
        private bool _useDefaultGroupCode = false;
        /// <summary>
        /// Get/Set Default UseDefaultGroupCode property 
        /// </summary>
        public bool UsingDefaultGroupCode
        {
            get { return _useDefaultGroupCode; }

            set { _useDefaultGroupCode = value; }
        } //

        /// <summary>
        /// private variable for DefaultProgramCode. 
        /// </summary>
        private string _defaultProgramCode = "";
        /// <summary>
        /// Get/Set Default Program Code property 
        /// </summary>
        public string DefaultProgramCode
        {
            get { return _defaultProgramCode; }

            set { _defaultProgramCode = value; }
        } //


        private string _currentGroupSessionCode = "CurrentGroup";
        /// <summary>
        /// Get/Set CurrentGroupSessionCode - used to create a unique session variable name
        /// </summary>
        public string CurrentGroupSessionCode
        {
            get { return this._currentGroupSessionCode; }
            set { this._currentGroupSessionCode = value; }
        }

        /// <summary>
        /// Get/Set ThisContainerPaneIndex - this can be be set to a different value when added to an accordion control
        /// </summary>
        public int ThisContainerPaneIndex
        {
            get { return this._ThisContainerPaneIndex; }
            set { _ThisContainerPaneIndex = value; }
        }
        private int _ThisContainerPaneIndex = GroupsManagementController.CONTROLLER_PANE_DETAIL;

        /// <summary>
        /// INTERNAL PROPERTY: CurrentRecord
        /// Set initially and store in ViewState
        /// </summary>
        protected Group CurrentRecord
        {
            get { return ViewState[ "CurrentRecord" ] as Group; }
            set { ViewState[ "CurrentRecord" ] = value; }
        }

        #endregion

        /// <summary>
        /// Handle Page Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void Page_Load( object sender, EventArgs ex )
        {
            this.InstanceGroupCode = CurrentGroupSessionCode;

            if ( IsUserAuthenticated() == false )
            {
                SetConsoleErrorMessage( "You must be authorized to use this function!" );
                return;
            }
            //get current user
            CurrentUser = GetAppUser();

            //this.myc.GetGroupSuccess += new EventHandler( myc_GetGroupSuccess );
            //this.myc.GetGroupFailure += new EventFailureEventHandler(GroupDetail_GetGroupFailure);
            //this.myc.GetGroupNoComponents += new EventHandler(myc_GetGroupNoComponents);

            if ( !Page.IsPostBack )
            {
                this.InitializeForm();
            }
        }

        //void myc_GetGroupSuccess( object sender, EventArgs e )
        //{
        //    LoggingHelper.DoTrace( 6, thisClassName + "myc_GetGroupSuccess" );
        //    if ( myc.GroupContract != null && myc.GroupContract.Id > 0 )
        //    {
        //        PopulateForm( myc.GroupContract );
        //    }
        //}//
        //void myc_GetGroupNoComponents( object sender, EventArgs e )
        //{
        //    throw new NotImplementedException();
        //}

        //void GroupDetail_GetGroupFailure( object sender, EventFailureEventArgs e )
        //{
        //    throw new NotImplementedException();
        //}


        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        private void InitializeForm()
        {
            CurrentRecord = new Group();

            //
            this.FormPrivileges = GDAL.SecurityManager.GetGroupObjectPrivileges( CurrentUser, FormSecurityName );

            //= Add attribute to btnDelete to allow client side confirm
            btnDelete.Attributes.Add( "onClick", "return confirmDelete(this);" );

            //handling setting of which action buttons are available to the current user
            this.btnNew.Enabled = false;
            this.btnSave.Enabled = false;
            this.btnDelete.Visible = false;

            this.btnUpdateOwner.Visible = false;

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

            if ( WebUser.TopAuthorization < 4
                || FormPrivileges.AssignPrivilege > ( int ) ILPathways.Business.EPrivilegeDepth.Region )
            {
                //this.btnUpdateOwner.Visible = true;
                //this.btnUpdateOwner.Enabled = false;
                FormPrivileges.AssignPrivilege = ( int ) ILPathways.Business.EPrivilegeDepth.State;
                btnCopyGroup.Visible = true;
            }


            //??
            groupTypePanel.Visible = true;

            //
            groupCode.Text = "default";
            if ( UsingDefaultGroupCode )
            {
                this.groupCodePanel.Visible = false;
                this.groupCode.Visible = false;
                this.lockGroupCode.Visible = false;
            }
            else
            {
                this.groupCodePanel.Visible = true;
            }

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
                        if ( CurrentGroup.Id.ToString() != txtId.Text )
                        {
                            //this.Get( CurrentGroup.Id );
                            PopulateForm( CurrentGroup );
                        }
                        else
                        {
                            //OK
                        }
                    }
                    else
                    {
                        this.txtId.Text = "0";
                    }
                }

                //check if groupId exists in parent and has changed
                //Label txtCurrentParentId = ( Label ) FindChildControl( Page, "txtCurrentParentId" );
                //if ( txtCurrentParentId != null )
                //{
                //    int id = int.Parse( txtCurrentParentId.Text.ToString() );
                //    if ( id > 0 && DidParentRecordChange )
                //    {
                //        this.Get( id );
                //    }
                //}

            }
            catch
            {
                //no action
            }

        }//

        /// <summary>
        /// Get - retrieve form data
        /// </summary>
        /// <param name="recId"></param>
        private void Get( int recId )
        {
            try
            {
                //get record
                Group entity = MyManager.Get( recId );

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
        //private void PopulateForm( bizService.GroupDataContract gc )
        //{
        //    Group entity = myc.MapGroup( gc );

        //    PopulateForm( entity );
        //}	// End method


        /// <summary>
        /// Populate the form 
        ///</summary>
        private void PopulateForm( Group entity )
        {
            if ( entity.ContactId > 0 )
            {
                //entity.PrimaryContact = new UserManager().Get( entity.ContactId );
                //contactPanel.Visible = true;
                //lblContact.Text = entity.PrimaryContact.FullName();
                //lblContactOrg.Text = entity.PrimaryContact.OrganizationName;
            }
            else
            {
                contactPanel.Visible = false;
                lblContact.Text = "";
                lblContactOrg.Text = "";
            }

            CurrentGroup = entity;

            this.txtId.Text = entity.Id.ToString();
            this.groupName.Text = entity.Title;
            this.groupCode.Text = entity.GroupCode;

            this.txtDescription.Text = entity.Description;
            this.rbIsActive.SelectedValue = this.ConvertBoolToYesNo( entity.IsActive );

            //this.rbHasPrivateCredentials.SelectedValue = this.ConvertBoolToYesNo( entity.HasPrivateCredentials );

            //TODO populate lists

            //this.contactId.Text = entity.ContactId.ToString();
            //this.orgId.Text = entity.OrgId.ToString();

            this.parentGroupId.Text = entity.ParentGroupId.ToString();

            if ( entity.Id > 0 )
            {
                this.SetListSelection( ddlGroupType, entity.GroupTypeId.ToString() );
                if ( entity.GroupCode.IndexOf( "????" ) == -1 )
                    this.SetFieldLockedStatus( groupCode, true );

                this.lblHistory.Text = entity.HistoryTitle();
                //btnUpdateOwner.Enabled = true;

                if ( FormPrivileges.CanDelete() )
                {
                    btnDelete.Enabled = true;
                }
            }
            else
            {
                ddlGroupType.SelectedIndex = -1;
                btnUpdateOwner.Enabled = false;
                //reset controls or state for empty object
                this.SetFieldLockedStatus( groupCode, false );
                this.lblHistory.Text = "";

                btnDelete.Enabled = false;
            }

        }//

        /// <summary>
        /// Handle a form button clicked 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        public void FormButton_Click( Object sender, CommandEventArgs ex )
        {
            LastActiveContainerIdx = ThisContainerPaneIndex;
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
                case "Delete":
                    this.DeleteRecord();
                    break;
                //case "UpdateOwner":
                //    this.UpdateGroupOwner();
                //    break;
                case "CopyGroup":
                    this.CopyGroup();
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
            //int recId = 0;
            bool isValid = true;

            try
            {

                vsErrorSummary.Controls.Clear();

                Page.Validate();

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
            if ( this.groupName.Text.Trim().Length == 0 )
            {
                this.AddReqValidatorError( vsErrorSummary, this.rfvGroupName.ErrorMessage, "groupName", false );
            }
            if ( this.groupCode.Text.Trim().Length == 0 )
            {
                this.AddReqValidatorError( vsErrorSummary, this.rfvGroupCode.ErrorMessage, "groupCode", false );
            }
            if ( this.txtDescription.Text.Trim().Length == 0 )
            {
                this.AddReqValidatorError( vsErrorSummary, this.rfvDescription.ErrorMessage, "txtDescription", false );
            }
            //
            if ( groupTypePanel.Visible )
            {
                if ( this.ddlGroupType.SelectedIndex < 1 )
                {
                    this.AddReqValidatorError( vsErrorSummary, this.rfvGroupType.ErrorMessage, "ddlGroupType", true );
                }
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

            Group entity = new Group();

            try
            {
                id = int.Parse( this.txtId.Text );

                if ( id == 0 )
                {
                    entity.Id = 0;
                    entity.CreatedBy = CurrentUser.IdentifyingName();
                    entity.Created = System.DateTime.Now;
                    action = "Create";

                    entity.ContactId = CurrentUser.Id;
                    //probably should not do arbitrarily
                    entity.OrgId = CurrentUser.OrgId;
                    if ( groupTypePanel.Visible == false)
                        entity.GroupTypeId = DefaultGroupTypeId;

                }
                else
                {
                    // get current record from viewstate
                    entity = CurrentGroup;
                    action = "Update";
                }
                /*
                 * //assign form fields
                 */

                entity.LastUpdated = System.DateTime.Now;
                entity.LastUpdatedBy = CurrentUser.IdentifyingName();

                entity.GroupName = this.groupName.Text;
                if ( groupCode.Text.ToLower() == "default" )
                    entity.GroupCode = System.Guid.NewGuid().ToString();
                else
                    entity.GroupCode = this.groupCode.Text;

                if ( groupTypePanel.Visible )
                    entity.GroupTypeId = int.Parse( this.ddlGroupType.SelectedValue.ToString() );

                entity.Description = this.txtDescription.Text;
                entity.IsActive = this.ConvertYesNoToBool( this.rbIsActive.SelectedValue );
               // entity.HasPrivateCredentials = this.ConvertYesNoToBool( this.rbHasPrivateCredentials.SelectedValue );

                //TODO - add ability specify owner - using  person search
                //entity.ContactId = Int32.Parse( contactId );


                //entity.ParentGroupId = Int32.Parse( parentGroupId );


                //call insert/update
                string statusMessage = "";
                int entityId = 0;
                if ( entity.Id == 0 )
                {
                    entityId = MyManager.Create( entity, ref statusMessage );
                    if ( entityId > 0 )
                    {
                        this.txtId.Text = entityId.ToString();
                        entity.Id = entityId;
                        this.UpdateParentId( txtId.Text );
                    }

                }
                else
                {
                    statusMessage = MyManager.Update( entity );
                }

                if ( statusMessage.Equals( "successful" ) )
                {
                    //update CurrentGroup and form
                    PopulateForm( entity );

                    this.SetConsoleSuccessMessage( action + " was successful for: " + entity.GroupName );

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
        /// Copy current group and privileges
        /// - can't copy privileges until the group is saved
        /// - can't save until know group code - could do a popup to prompt for group code!
        /// ==> OR leave code as writable - pass indicator to not set to read only
        /// - need to enable changing group owner!
        /// </summary>
        private void CopyGroup()
        {
            //int id = 0;
            //string msg = "";

            Group entity = CurrentGroup;
            int prevGroupId = CurrentGroup.Id;
            string prevGroupCode = CurrentGroup.GroupCode;
            try
            {
                entity.Id = 0;
                entity.CreatedBy = CurrentUser.IdentifyingName();
                entity.Created = System.DateTime.Now;

                entity.LastUpdated = System.DateTime.Now;
                entity.LastUpdatedBy = CurrentUser.IdentifyingName();

                entity.GroupName = this.groupName.Text + " **COPIED**";
                //set to default and then overrider
                entity.GroupCode = System.Guid.NewGuid().ToString();

                if ( groupTypePanel.Visible )
                    entity.GroupTypeId = int.Parse( this.ddlGroupType.SelectedValue.ToString() );
                else
                    entity.GroupTypeId = DefaultGroupTypeId;

                entity.Description = this.txtDescription.Text + " **COPIED**";
                entity.IsActive = this.ConvertYesNoToBool( this.rbIsActive.SelectedValue );
               // entity.HasPrivateCredentials = this.ConvertYesNoToBool( this.rbHasPrivateCredentials.SelectedValue );

                entity.ContactId = WebUser.Id;
                entity.OrgId = WebUser.OrgId;
                //??????????????????? - not sure if should maintain from source??
                entity.ParentGroupId = 0;


                //call insert/update
                string statusMessage = "";
                int entityId = MyManager.Create( entity, ref statusMessage );
                if ( entityId > 0 )
                {
                    this.txtId.Text = entityId.ToString();
                    entity.Id = entityId;
                    this.UpdateParentId( txtId.Text );
                }

                if ( statusMessage.Equals( "successful" ) )
                {
                    //TODO - copy privileges from source group	- prevGroupId
                    //WorkNetGroupPrivilegeManager.CopyGroupPrivileges( prevGroupId, entityId );
                    entity.GroupCode = prevGroupCode + "????";
                    this.SetFieldLockedStatus( groupCode, false );

                    //update CurrentGroup and form
                    PopulateForm( entity );

                    this.SetConsoleSuccessMessage( "Copy was successful. Now update: appropriate group code, title, and description" );

                }
                else
                {
                    this.SetConsoleErrorMessage( statusMessage );
                }

            }
            catch ( System.Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".CopyGroup() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }
        }
        /// <summary>
        /// Find and update the parentId for this accordian group 
        /// </summary>
        private void UpdateParentId( string newId )
        {
            //TODO - remove once completely using controller

            //find courseId and update
            Label txtCurrentParentId = ( Label ) FindChildControl( Page, "txtCurrentParentId" );
            try
            {
                if ( txtCurrentParentId != null )
                {
                    txtCurrentParentId.Text = newId;
                }

            }
            catch
            {
                //no action
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

                if ( MyManager.Delete( id, ref statusMessage ) )
                {
                    this.SetConsoleSuccessMessage( "Delete of record was successful" );
                    //reset lists as needed for removed records
                    this.UpdateParentId( "0" );
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
        /// Open search window to add/update the group owner
        /// </summary>
        private void UpdateGroupOwner()
        {
            //TODO - update to use the person search like site mgmt
            int groupId = CurrentGroup.Id;
            if ( groupId > 0 )
            {
                GroupsManagementController.GroupUserSearchParameters searchParms = new GroupsManagementController.GroupUserSearchParameters();
                searchParms.groupId = CurrentGroup.Id.ToString();
                //only look for wfp users
                searchParms.roleId = "2";
                //type: groupTeamMember or eagroupTeamMember or groupOwner
                searchParms.childEntityType = "groupOwner";
                //only look for users in my work lwia
                //searchParms.searchLwia = this.WebUser.WorkLwiaId.ToString();

                searchParms.isSYEPSearch = false;
                searchParms.isElevateAmericaSearch = false;
                searchParms.showCloseButton = false;
                //set default or custom eNotice. If blank, no notice will be sent
                searchParms.emailNoticeCode = "";
                searchParms.searchTitle = "New Group Owner Search";
                string navigateUrl = "/vos_portal/UserSearch.aspx?";

                OpenSearchForm( searchParms, navigateUrl );


                //--------------------------------------------------------
                //string groupID = groupId.ToString();

                //only look for wfp users
                //string roleId = "2";
                //only look for users in my work lwia
                //string myWorkLwia = this.WebUser.WorkLwiaId.ToString();
                //type: groupMember or groupTeamMember
                //string type = "groupOwner";

                //Telerik.WebControls.RadWindow newwindow = new Telerik.WebControls.RadWindow();
                //newwindow.ID = "search2";
                //newwindow.Title = "Illinois workNet Person Search";
                //newwindow.NavigateUrl = "/vos_portal/UserSearch.aspx?Id=" + groupID
                //            + "&roleId=" + roleId
                //            + "&lwia=" + myWorkLwia
                //            + "&type=" + type;

                //newwindow.Width = System.Web.UI.WebControls.Unit.Pixel( 700 );
                //newwindow.Height = System.Web.UI.WebControls.Unit.Pixel( 500 );
                //newwindow.VisibleOnPageLoad = true;
                //RadWindowManager1.Windows.Add( newwindow );
            }
            else
            {
                //message
                SetConsoleErrorMessage( "Error there is no current group. First select a group before requesting an Add" );

            }
        }//

        /// <summary>
        /// OpenSearchForm
        /// </summary>
        /// <param name="searchParms"></param>
        /// <param name="title"></param>
        /// <param name="navigateUrl"></param>
        protected void OpenSearchForm( GroupsManagementController.GroupUserSearchParameters searchParms, string navigateUrl )
        {
            Session[ GroupsManagementController.GROUPS_SESSIONVAR_USERSEARCH_PARMS ] = searchParms;

            string hostName = Request.Url.Host.ToString();
            BrowserWindow win = new BrowserWindow( hostName + navigateUrl, 800, 600, true );
            win.MenuBar = false;

            win.AttachToPageLoad( this );
        }//
        /// <summary>
        /// Reset form values
        /// </summary>
        private void ResetForm()
        {
            CurrentGroup = new Group();
            //reset form by populating with an empty business object
            Group entity = new Group();
            //do any defaults. 
            entity.IsActive = true;
            entity.GroupCode = "default";

            PopulateForm( entity );

        }//

        /// <summary>
        /// Populate form controls
        /// </summary>
        /// <param name="recId"></param>
        private void PopulateControls()
        {

            SetGroupTypeList();
        } //
        /// <summary>
        /// Populate the group type list
        /// </summary>
        private void SetGroupTypeList()
        {

            DataSet ds = GroupManager.CodesGroupType_Select();
            BDM.PopulateList( ddlGroupType, ds, "Id", "Title", "Select a Group Type" );

        } //




    }	
}
