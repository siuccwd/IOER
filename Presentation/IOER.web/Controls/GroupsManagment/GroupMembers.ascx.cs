using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Business;
using ILPathways.classes;
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
    public partial class GroupMembers : GroupsManagementController
    {
        const string thisClassName = "Controls_GroupsManagement_GroupMembers";

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
        /// Get/Set ThisContainerPaneIndex - this can be be set to a different value when added to an accordion control
        /// </summary>
        public int ThisContainerPaneIndex
        {
            get { return this._ThisContainerPaneIndex; }
            set { _ThisContainerPaneIndex = value; }
        }
        private int _ThisContainerPaneIndex = GroupsManagementController.CONTROLLER_PANE_CUSTOMERS;

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
        }//

        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        private void InitializeForm()
        {
            //set grid defaults (variables are in base control
            GridViewSortExpression = "SortName";
            GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Ascending;

            //FormPrivileges - not used yet
            this.FormPrivileges = GDAL.SecurityManager.GetGroupObjectPrivileges( CurrentUser, FormSecurityName );

            if ( CurrentUser.TopAuthorization < 3 )
            {
                FormPrivileges.SetAdminPrivileges();
            }
            RecordPrivileges = new ApplicationRolePrivilege();
            //maybe control accordian panes here?
            if ( FormPrivileges.CanCreate() )
            {
                //this.btnNew.Enabled = true;
                //btnSave.Enabled = true;

            }
            else if ( FormPrivileges.CanUpdate() )
            {
                //btnSave.Enabled = true;
            }

            if ( FormPrivileges.CanDelete() )
            {
                //this.btnDelete.Visible = true;
                //this.btnDelete.Enabled = false;
            }
            InitializePageSizeList();

            //check for a direct request
            //CheckRecordRequest();

        }	// End 
        /// <summary>
        /// Check the request type for the form ==> should be handled by container
        /// </summary>
        private void CheckRecordRequest()
        {

            int gid = this.GetRequestKeyValue( "gid", 0 );
            if ( gid > 0 )
            {
                InstanceGroupCode = "CurrentGroupMbrGrp";
                ParentGroupId = gid;
                HandleGroupChange( gid );
                DoSearch();
            }
            else
            {
                string code = this.GetRequestKeyValue( "code", "" );
                if ( code.Length > 0 )
                {
                    InstanceGroupCode = "CurrentGroupMbrGrp";
                    Group grp = GroupManager.GetByCode( code );
                    if ( grp.Id > 0 )
                    {
                        ParentGroupId = grp.Id;
                        HandleGroupChange( grp.Id );
                        DoSearch();
                    }
                }
            }

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
                        if ( CurrentGroup.Id.ToString() != txtGroupId.Text )
                        {
                            //chg of group,do search
                            this.txtGroupId.Text = CurrentGroup.Id.ToString();
                            HandleGroupChange( CurrentGroup.Id );
                            DoSearch();
                        }
                    }
                    else
                    {
                        this.txtGroupId.Text = "0";
                        menuPanel.Visible = false;
                        formGrid.DataSource = null;
                        formGrid.DataBind();
                    }
                }
                else
                {
                    this.txtGroupId.Text = "0";
                    menuPanel.Visible = false;
                    formGrid.DataSource = null;
                    formGrid.DataBind();
                }

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
        /// <param name="e"></param>
        public void FormButton_Click( Object sender, CommandEventArgs e )
        {
            LastActiveContainerIdx = ThisContainerPaneIndex;

            switch ( e.CommandName )
            {

                case "ShowTeam":
                    this.DoSearch();
                    break;
                case "AddMember":
                    this.HandleAddRequest();
                    break;

                case "RefreshTeam":
                    this.DoSearch();
                    break;
                case "Filter":
                    this.DoSearch();
                    break;
                //case "NotifyInComplete":
                //  this.GetEmailsOfInComplete();
                //  break;

            }
        }

        private void HandleGroupChange( int id )
        {
            if ( CurrentGroup != null && CurrentGroup.Id != id )
            {
                CurrentGroup = GroupManager.Get( id );
            }
            this.txtGroupId.Text = CurrentGroup.Id.ToString();



            this.btnAddIWDSMember.Visible = false;
            this.btnNotifyInComplete.Visible = false;


        }//

        /// <summary>
        /// Get members for group
        /// </summary>
        /// <param name="lwiaId"></param>
        /// <param name="orgId"></param>
        private void DoSearch()
        {
            int selectedPageNbr = 0;
            string sortTerm = GridViewSortExpression;
            if ( GridViewSortDirection == System.Web.UI.WebControls.SortDirection.Ascending )
                sortTerm = sortTerm + " ASC";
            else
                sortTerm = sortTerm + " DESC";
            pager1.ItemCount = 0;
            pager1.CurrentIndex = 1;
            pager2.ItemCount = 0;
            pager2.CurrentIndex = 1;

            DoSearch( selectedPageNbr, sortTerm );
        }//

        /// <summary>
        /// Get members for group
        /// </summary>
        /// <param name="lwiaId"></param>
        /// <param name="orgId"></param>
        private void DoSearch( int selectedPageNbr, string sortTerm )
        {
            string missingData = string.Empty;

            int groupId = CurrentGroup.Id;
            if ( groupId < 1 )
            {
                //message
                SetConsoleErrorMessage( "Error there is no current group. First select a group and then repeat your request" );
                return;
            }

            try
            {
                HandleAuthGrpSearch( groupId, selectedPageNbr, sortTerm );

            }
            catch ( System.Exception e )
            {
                //Action??		- display message and close form??	

                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Close this form and try again.<br>" + e.ToString() );
            }
        }	// End method


        private void HandleAuthGrpSearch( int groupId, int selectedPageNbr, string sortTerm )
        {
            DataSet formDataset = null;
            string missingData = string.Empty;
            int pTotalRows = 0;
            int pagesize = formGrid.PageSize;

            if ( selectedPageNbr == 0 )
            {
                //with custom pager, need to start at 1
                selectedPageNbr = 1;
            }
            //LastPageNumber = selectedPageNbr;
            pager1.CurrentIndex = pager2.CurrentIndex = selectedPageNbr;

            // Set the page size for the DataGrid control based on the selection
            CheckForPageSizeChange();

            string booleanOperator = "AND";
            string filter = "";
            pager1.Visible = true;
            //construct filter
            string pOrderBy = "";
            filter = MyManager.FormatSearchItem( filter, "GroupId", groupId.ToString(), booleanOperator );

            if ( txtFilterBy.Text.Length > 0 )
            {
                string where = " USER_LAST_NAME like '" + txtFilterBy.Text + "%'";
                filter += MyManager.FormatSearchItem( filter, where, booleanOperator );
            }

            formDataset = GroupManager.GroupMember_Search( filter, pOrderBy, selectedPageNbr, pagesize, ref pTotalRows );
            pager1.ItemCount = pager2.ItemCount = pTotalRows;
            //LastTotalRows = pTotalRows;

            if ( MyManager.DoesDataSetHaveRows( formDataset ) == false )
            {
                //this.SetConsoleInfoMessage("No customers were found for this group.");
                formGrid.DataSource = null;
                formGrid.DataBind();
                lblRecordCount.Text = "Found 0 records";
            }
            else
            {
                lblRecordCount.Text = "Found " + formDataset.Tables[ 0 ].Rows.Count.ToString() + " records";

                resultsPanel.Visible = true;
                ddlPageSizeList.Enabled = true;

                //searchPanel.Visible = false;

                DataTable dt = formDataset.Tables[ 0 ];
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

                if ( CurrentGroup != null && CurrentGroup.GroupCode == "ContentManagers" )
                {
                    formGrid.Columns[ 0 ].Visible = false;
                    formGrid.Columns[ formGrid.Columns.Count - 1 ].Visible = true;
                }

            }
        }	// End method



        /// <summary>
        /// Open search window to add a member to this group
        /// </summary>
        private void HandleAddRequest()
        {
            int groupId = GetParentId();
            if ( groupId > 0 )
            {
                GroupsManagementController.GroupUserSearchParameters searchParms = new GroupsManagementController.GroupUserSearchParameters();
                searchParms.groupId = groupId.ToString();
                searchParms.roleId = "1";
                //only look for individual users
                searchParms.childEntityType = "groupMember";


                searchParms.isSYEPSearch = false;
                searchParms.isElevateAmericaSearch = true;
                searchParms.emailNoticeCode = "";
                searchParms.showCloseButton = false;

                string title = "Illinois workNet Person Search";
                string navigateUrl = "/vos_portal/UserSearch.aspx?";

                OpenSearchForm( searchParms, title, navigateUrl );

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
        /// <param name="groupId"></param>
        /// <param name="roleId"></param>
        /// <param name="type"></param>
        /// <param name="title"></param>
        /// <param name="navigateUrl"></param>
        protected void OpenSearchForm( GroupsManagementController.GroupUserSearchParameters searchParms, string title, string navigateUrl )
        {
            Session[ GroupsManagementController.GROUPS_SESSIONVAR_USERSEARCH_PARMS ] = searchParms;

            string hostName = Request.Url.Host.ToString();
            BrowserWindow win = new BrowserWindow( hostName + navigateUrl, 800, 600, true );
            win.MenuBar = false;

            win.AttachToPageLoad( this );
        }//

        #region Group Members grid

        ///<summary>
        ///Members Grid DataBounds
        ///</summary>
        protected void formGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                LinkButton delBtn = ( LinkButton ) e.Row.FindControl( "btnDelete" );
                if ( delBtn != null && ( FormPrivileges.CanDelete() || RecordPrivileges.CanDelete() ) )
                {
                    delBtn.Attributes.Add( "onclick", "javascript:return "
                            + "confirm('Are you sure you want to remove "
                            + DataBinder.Eval( e.Row.DataItem, "FullName" )
                            + " from this group?')" );
                }
                else
                {
                    delBtn.Enabled = false;
                }
            }
        }

        ///<summary>
        ///Members Grid RowBounds
        /// Delete and Assign commands
        ///</summary>
        protected void formGrid_RowCommand( object sender, GridViewCommandEventArgs e )
        {

            LastActiveContainerIdx = ThisContainerPaneIndex;

            if ( e.CommandName == "DeleteRow" )
            {
                try
                {
                    // get the ID of the clicked row
                    int ID = Convert.ToInt32( e.CommandArgument );
                    string statusMessage = "";

                    // Delete the record 
                    if ( GroupManager.GroupMember_Delete( ID, ref statusMessage ) )
                    {
                        //successfull
                        SetConsoleSuccessMessage( "Successfully removed member from this group!" );
                        DoSearch();
                    }
                    else
                    {
                        SetConsoleErrorMessage( "Delete failed: " + statusMessage );
                    }

                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, thisClassName + ".formGrid_RowCommand().DeleteRow - Unexpected error encountered" );
                }

            } if ( e.CommandName == "SelectMember" )
            {
                try
                {
                    // get the ID of the clicked row
                    int id = Convert.ToInt32( e.CommandArgument );

                    LastSelectedGroupMemberId = id;
                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, thisClassName + ".formGrid_RowCommand().SelectMember - Unexpected error encountered" );
                }

            }
        }
        /// <summary>
        /// Handle click on a grid header - that id change the sort order
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void formGrid_Sorting( object sender, GridViewSortEventArgs e )
        {
            LastActiveContainerIdx = ThisContainerPaneIndex;

            string newSortExpression = e.SortExpression;
            string sortTerm = "";

            //check if the same field as previous sort
            if ( GridViewSortExpression.ToLower().Equals( newSortExpression.ToLower() ) )
            {
                // This sort is being applied to the same field for the second time.
                // Reverse it.
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

            DoSearch();

        }//

        ///<summary>
        ///Add pagination capabilities
        ///</summary>
        public void formGrid_PageIndexChanging( object sender, GridViewPageEventArgs e )
        {
            LastActiveContainerIdx = ThisContainerPaneIndex;

            formGrid.PageIndex = e.NewPageIndex;
            //get current sort term
            string sortTerm = GetCurrentSortTerm();

            DoSearch( formGrid.PageIndex, sortTerm );

        }//

        #endregion

        /// <summary>
        /// Find the parentId for this group 
        /// </summary>
        private int GetParentId()
        {
            int groupId = 0;


            try
            {
                if ( CurrentGroupExists() )
                {
                    groupId = CurrentGroup.Id;
                }

            }
            catch
            {
                //no action
            }

            return groupId;
        }

        #region Page size related methods
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
            DataSet ds1 = LDAL.DatabaseManager.GetCodeValues( "GridPageSize", "SortOrder" );
            LDAL.DatabaseManager.PopulateList( this.ddlPageSizeList, ds1, "StringValue", "StringValue", "Select Size" );


        } //


        private string GetCurrentSortTerm()
        {

            string sortTerm = GridViewSortExpression;
            if ( GridViewSortDirection == System.Web.UI.WebControls.SortDirection.Ascending )
                sortTerm = sortTerm + " ASC";
            else
                sortTerm = sortTerm + " DESC";

            return sortTerm;

        }
        public void pager_Command( object sender, CommandEventArgs e )
        {
            int currentPageIndx = Convert.ToInt32( e.CommandArgument );
            pager1.CurrentIndex = currentPageIndx;
            pager2.CurrentIndex = pager1.CurrentIndex;
            string sortTerm = GetCurrentSortTerm();

            DoSearch( currentPageIndx, sortTerm );

        }
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
