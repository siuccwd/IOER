using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Resources;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EmailHelper = ILPathways.Utilities.EmailManager;
using MyManager = Isle.BizServices.GroupServices;
using ILPathways.Business;
using IOER.classes;
using IOER.Controllers;
//using ILPathways.DAL;
using Isle.BizServices;

using IOER.Library;
using ILPathways.Utilities;
using LRDAL = LRWarehouse.DAL;

namespace IOER.Admin.Groups
{
    public partial class GroupsMgmt : IOER.Library.BaseAppPage
    {
		MyManager grpMgr = new MyManager();

        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
		const string thisClassName = "GroupsMgmt";

        /// <summary>
        /// Session variable for group-user search parameters
        /// </summary>
        public static string GROUPS_SESSIONVAR_USERSEARCH_PARMS = "group_user_search_parms";

        #region Properties

        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string FormSecurityName
        {
            get { return this.formSecurityName.Text; }
            set { this.formSecurityName.Text = value; }
        }
        private string _currentGroupSessionCode = "CurrentTabbedGroup";
        /// <summary>
        /// Get/Set CurrentGroupSessionCode - used to create a unique session variable name
        /// </summary>
        public string CurrentGroupSessionCode
        {
            get { return this._currentGroupSessionCode; }
            set { this._currentGroupSessionCode = value; }
        }

        /// <summary>
        /// Get/Set GroupsType - if blank, no preselection, and will show list
        /// </summary>
        public int DefaultGroupTypeId
        {
            get { return int.Parse(defaultGroupType.Text); }
            set { defaultGroupType.Text = value.ToString(); }
        }

        public string GroupFilter
        {
            get { return txtGroupFilter.Text; }
            set { txtGroupFilter.Text = value; }
        }

        bool _canViewPrivileges = false;
        public bool CanViewPrivileges
        {
            get { return _canViewPrivileges; }
            set { _canViewPrivileges = value; }
        }

        bool _usingDefaultGroupCode = false;
        public bool UsingDefaultGroupCode
        {
            get { return _usingDefaultGroupCode; }
            set { _usingDefaultGroupCode = value; }
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
        /// Store retrieve the last LastGroupId consumed by this control
        /// WHY??????????????????????
        /// </summary>
        protected int LastGroupId
        {
            get
            {
                try
                {
                    if ( ViewState[ "LastTabbedGroupId" ] == null )
                        ViewState[ "LastTabbedGroupId" ] = 0;
                    return Int32.Parse( ViewState[ "LastTabbedGroupId" ].ToString() );
                }
                catch
                {
                    return 0;
                }
            }
            set { ViewState[ "LastTabbedGroupId" ] = value; }
        }//
        #endregion
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( IsUserAuthenticated() == false )
            {
                SetConsoleErrorMessage( "Error: you must be authenticated in order to use this page.<br/>Please login and try again." );
                formContainer.Visible = false;
                return;
            }
            //get current user
            CurrentUser = GetAppUser();
            //this probably only needs to be set in the main grpMgr for the maintenance page!
            //this.InstanceGroupCode = CurrentGroupSessionCode;
            grpMgr.InstanceGroupCode = MyManager.CONTROLLER_CURRENT_GROUP;

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
            try
            {
                //we don't want addThis on this page, so show literal in master
                Literal showingAddThis = ( Literal ) FormHelper.FindChildControl( Page, "litHidingAddThis" );
                if ( showingAddThis != null )
                    showingAddThis.Visible = true;
            }
            catch
            {
            }
            grpMgr.LastActiveContainerIdx = -1;
            grpMgr.DidParentRecordChange = false;
            grpMgr.CurrentGroup = new AppGroup();

            //formSecurityName
            this.FormPrivileges = SecurityManager.GetGroupObjectPrivileges( this.CurrentUser, FormSecurityName );
            if ( CurrentUser.TopAuthorization < 4 )
            {
                FormPrivileges.SetAdminPrivileges();
            }
            RecordPrivileges = new ApplicationRolePrivilege();

            //maybe control accordian panes here?
            if ( FormPrivileges.CanCreate() || CurrentUser.TopAuthorization < 5 )
            {
                tabGroupDetail.Visible = true;

            }
            else if ( FormPrivileges.CanUpdate() )
            {
                tabGroupDetail.Visible = true;
            }

            if ( FormPrivileges.CanDelete() )
            {
                tabGroupDetail.Visible = true;
            }

            if ( FormPrivileges.AssignPrivilege > 0 )
            {
                //do after select!
                //tabMembers.Visible = true;
            }

            if ( CurrentUser.TopAuthorization > 5 || UsingDefaultGroupCode )
            {
                groupTypePanel.Visible = false;
            }
            this.PopulateControls();

            if ( CurrentUser.TopAuthorization > 5 || UsingDefaultGroupCode )
            {
                groupTypePanel.Visible = false;
            }

            if ( DefaultGroupTypeId > 0 )
            {
                //do immediate search??
                //SetFormSelectList( DefaultGroupTypeId );
                //???????????????
                DoSearch();
            }
            CheckRecordRequest();
     
        }	// End 

        /// <summary>
        /// Check the request type for the form
        /// </summary>
        private void CheckRecordRequest()
        {

            // Check if a request was made for a specific record
            int id = this.GetRequestKeyValue( "id", 0 );
            string gcode = this.GetRequestKeyValue( "cd", "" );

            if ( id > 0 )
            {
                //txtCurrentParentId.Text = id.ToString();
                grpMgr.DidParentRecordChange = true;
                this.Get( id );

            }
            else if ( gcode.Length > 0 )
            {
                //txtCurrentParentId.Text = id.ToString();
                grpMgr.DidParentRecordChange = true;
                this.Get( gcode );

            }
            else
            {

            }


        }	// End 
        /// <summary>
        /// Determine which accordian pane should be shown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_PreRender( object sender, EventArgs e )
        {
            if ( grpMgr.LastActiveContainerIdx > -1 )
            {
                formContainer.ActiveTabIndex = grpMgr.LastActiveContainerIdx;
            }
            //check for a change to the parentId (ex after a new record)
            try
            {

                if ( grpMgr.CurrentGroup != null && grpMgr.CurrentGroup.IsValidEntity() )
                {
                    if ( grpMgr.CurrentGroup.Id != LastGroupId )
                    {
                        LastGroupId = grpMgr.CurrentGroup.Id;
                        //get record????
                        Get( LastGroupId );
                    }

                    //may depend on privileges???
                    if ( CurrentUser.TopAuthorization <= ( int ) ILPathways.Business.EUserRole.ProgramStaff
                        || FormPrivileges.ReadPrivilege >= ( int ) ILPathways.Business.EPrivilegeDepth.State )
                        this.tabMembers.Visible = true;

                }
                else
                {
                    this.tabMembers.Visible = false;
                    this.tabPrivileges.Visible = false;
                }


            }
            catch
            {
            }
        } //
        /// <summary>
        /// Get - retrieve form data
        /// </summary>
        /// <param name="groupCode"></param>
        private void Get( string groupCode )
        {
            grpMgr.CurrentGroup = MyManager.GetByCode( groupCode );
            //HandleGroupRequest();
        } //

        /// <summary>
        /// Get - retrieve form data
        /// </summary>
        /// <param name="recId"></param>
        private void Get( int recId )
        {
            grpMgr.CurrentGroup = MyManager.Get( recId );
            try
            {
                //detailPane.ToolTip = CurrentGroup.GroupName;

                HyperLink lnk = ( HyperLink ) this.FindControl( "ctl00_MainContent_ctl00_ctl00_detailPaneHeaderLink" );
                if ( lnk != null )
                {
                    lnk.Text = "Group: " + grpMgr.CurrentGroup.Title;
                }
            }
            catch ( Exception ex )
            {
                //ignore
                SetConsoleErrorMessage( "error: " + ex.ToString() );
            }

            //HandleGroupRequest();
        } //
        /// <summary>
        /// Handle selection of a group type - will populate related groups
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void GroupType_SelectedIndexChanged( object sender, System.EventArgs e )
        {
            try
            {
                //string type = this.ddlGroupTypeList.SelectedValue;
                //SetGroupListByType( type );
                if ( this.ddlGroupTypeList.SelectedIndex > 0 )
                {
                    DoSearch();
                }

            }
            catch ( Exception ex )
            {
                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.Message );
                this.LogError( ex, thisClassName + ".GroupType_SelectedIndexChanged" );
            }
        } //

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
            if ( selectedPageNbr == 0 )
            {
                //with custom pager, need to start at 1
                selectedPageNbr = 1;
            }
            LastPageNumber = selectedPageNbr;
            pager1.CurrentIndex = pager2.CurrentIndex = selectedPageNbr;

            // Set the page size for the DataGrid control based on the selection
            CheckForPageSizeChange();

            int pTotalRows = 0;
            string filter = FormatFilter();

            try
            {
                ds = MyManager.Search( filter, sortTerm, selectedPageNbr, pager1.PageSize, ref pTotalRows );
                //assumes min rows are returned, so pager needs to have the total rows possible
                pager1.ItemCount = pager2.ItemCount = pTotalRows;

                LastTotalRows = pTotalRows;

                if ( MyManager.DoesDataSetHaveRows( ds ) == false )
                {
                    resultsPanel.Visible = false;
                    if ( Page.IsPostBack )	//only show if not an initial auto search??
                        SetConsoleErrorMessage( "Sorry, No records were found for the provided search criteria" );
                    ddlPageSizeList.Enabled = false;
                    pager1.Visible = false;
                    pager2.Visible = false;
                }
                else
                {

                    resultsPanel.Visible = true;
                    ddlPageSizeList.Enabled = true;

                    DataTable dt = ds.Tables[ 0 ];
                    DataView dv = ( ( DataTable ) dt ).DefaultView;
                    //if ( sortTerm.Length > 0 )
                    //  dv.Sort = sortTerm;

                    if ( pTotalRows > formGrid.PageSize )
                    {
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
                    formGrid.DataBind();

                }
            }
            catch ( System.Exception ex )
            {
                //Action??		- display message and close form??	
                LoggingHelper.LogError( ex, thisClassName + ".DoSearch() - Unexpected error encountered while attempting search. " );

                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Close this form and try again.<br>" + ex.ToString() );
            }
        }	//

        protected string FormatFilter()
        {
            string filter = "";

            string booleanOperator = "AND";

            if ( ddlGroupTypeList.SelectedIndex > 0 )
            {
                filter += LRDAL.BaseDataManager.FormatSearchItem( filter, "GroupTypeId", ddlGroupTypeList.SelectedValue.ToString(), booleanOperator );

            }
            else if ( DefaultGroupTypeId > 0 && groupTypePanel.Visible == false )
            {
                filter += LRDAL.BaseDataManager.FormatSearchItem( filter, "GroupTypeId", DefaultGroupTypeId, booleanOperator );
            }

            if ( GroupFilter.Length > 0 )
            {
                filter += LRDAL.BaseDataManager.FormatSearchItem( filter, GroupFilter, booleanOperator );
            }


            if ( txtKeyword.Text.Trim().Length > 0 )
            {
                string keyword = MyManager.HandleApostrophes( FormHelper.CleanText( txtKeyword.Text.Trim() ) );

                if ( keyword.IndexOf( "%" ) == -1 )
                    keyword = "%" + keyword + "%";

                string where = " (groupOwner.LastName like '" + keyword + "' OR groupOwner.FirstName like '" + keyword
                                + "' OR Title like '" + keyword
                                + "' OR GroupCode like '" + keyword + "') ";
                filter += LRDAL.BaseDataManager.FormatSearchItem( filter, where, booleanOperator );
            }

            //TODO - add search for members

            //string status = "";
            //int statusId = 0;
            //if ( ddlStatus.SelectedIndex > 0 )
            //{
            //  //status = this.ddlStatus.SelectedValue.ToString();
            //  statusId = Int32.Parse( this.ddlStatus.SelectedValue.ToString() );
            //  filter += LRDAL.BaseDataManager.FormatSearchItem( filter, "StatusId", statusId, booleanOperator );
            //}

            //if ( IsPublicPage == true )
            //{
            //  filter += LRDAL.BaseDataManager.FormatSearchItem( filter, publicFilter.Text, booleanOperator );
            //}
            //if ( CurrentUser.TopAuthorization < 4 && filter.Length == 0 )
            //  filter += LRDAL.BaseDataManager.FormatSearchItem( filter, this.defaultFilter.Text, booleanOperator );

            if ( this.IsTestEnv() )
            {
                this.SetConsoleSuccessMessage( "sql: " + filter );
            }

            return filter;
        }	//
        protected void formGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {

            if ( e.Row.RowType == DataControlRowType.DataRow )
            {

                //add delete confirmation
                //LinkButton dlb = ( LinkButton ) e.Row.FindControl( "deleteRowButton" );
                ////check if user can delete
                //if ( FormPrivileges.CanDelete() )
                //{
                //  dlb.Attributes.Add( "onclick", "javascript:return " +
                //      "confirm('Are you sure you want to delete this project " +
                //      DataBinder.Eval( e.Row.DataItem, "RowId" ) + "')" );
                //} else
                //{
                //  dlb.Enabled = false;
                //}
            }
        }//

        protected void formGrid_RowCommand( object sender, GridViewCommandEventArgs e )
        {
            if ( e.CommandName == "DeleteRow" )
            {
                // get the ID of the clicked row
                string id = e.CommandArgument.ToString();

                // Delete the record 
                DeleteRecord( id );

            }
            else if ( e.CommandName == "SelectRow" )
            {
                pager1.CurrentIndex = pager2.CurrentIndex = LastPageNumber;
                pager1.ItemCount = pager2.ItemCount = LastTotalRows;

                // get the ID of the clicked row
                string id = e.CommandArgument.ToString();

                // show the record 
                ShowRecord( id );
            }
            else if ( e.CommandName == "ActionRow" )
            {
                pager1.CurrentIndex = pager2.CurrentIndex = LastPageNumber;
                pager1.ItemCount = pager2.ItemCount = LastTotalRows;

                // get the ID of the clicked row
                string id = e.CommandArgument.ToString();

                // show the record 
                ShowRecord( id );
            }
        }

        /// <summary>
        /// show selected row
        /// </summary>
        /// <param name="record id"></param>
        protected void ShowRecord( string id )
        {
            try
            {
                //string statusMessage = "";
                //string url = this.detailUrl.Text + "?id=" + id.ToString();
                //Response.Redirect( url, true );

            }
            catch ( System.Threading.ThreadAbortException ex )
            {
                //UtilityManager.DoTrace( 3, "includes_header4.Page_Load - System.Threading.ThreadAbortException. Google Search      section Path = " + Request.Path );
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ShowRecord() - Unexpected error encountered while attempting to retrieve contract Id of " + id.ToString() );
                this.SetConsoleErrorMessage( "An unexpected error was encountered while attempting to delete this record. System administration has been notified:<br/> " + ex.Message.ToString() );
            }
        }
        /// <summary>
        /// handle row delete
        /// </summary>
        /// <param name="record id"></param>
        protected void DeleteRecord( string id )
        {
            try
            {
                //string statusMessage = "";

                //if ( myManager.Delete( id, ref statusMessage ) )
                //{
                //  this.SetConsoleSuccessMessage( "Delete was successful" );
                //  //OK reset list
                //  string sortTerm = GetCurrentSortTerm();

                //  DoSearch( pager1.CurrentIndex, sortTerm );
                //  //DoSearch();
                //} else
                //{
                //  // problem
                //  this.SetConsoleErrorMessage( "An unexpected issue was encountered while attempting to delete this record. System administration has been notified:<br/> " + statusMessage );
                //  UtilityManager.logError( thisClassName + ".DeleteRecord() - Delete failed for contract id of " + id.ToString() + " and returned the following message:<br/>" + statusMessage, true );
                //}

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".DeleteRecord() - Unexpected error encountered while attempting to delete contract Id of " + id.ToString() );
                this.SetConsoleErrorMessage( "An unexpected error was encountered while attempting to delete this record. System administration has been notified:<br/> " + ex.Message.ToString() );
            }
        }

        /// <summary>
        /// Checks selected sort column and determines if new sort or a change in the direction of the sort
        /// </summary>
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

            //if desc by ContractStartDate, use ContractEndDate
            if ( newSortExpression.ToLower() == "contractstartdate" && GridViewSortDirection == System.Web.UI.WebControls.SortDirection.Descending )
            {
                sortTerm = " ContractEndDate DESC";
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
            DataSet ds1 = LRDAL.DatabaseManager.GetCodeValues( "GridPageSize", "SortOrder" );
            LRDAL.DatabaseManager.PopulateList( this.ddlPageSizeList, ds1, "StringValue", "StringValue", "Select Size" );

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
        private void PopulateControls()
        {
            SetGroupTypeList();
            //only if doing auto search
            SetGroupList();
            //
            InitializePageSizeList();
        } //
        /// <summary>
        /// Populate the group type list
        /// </summary>
        private void SetGroupTypeList()
        {

            //DataSet ds = DatabaseManager.GetCodeValues( "en", "GroupType", "SortOrder" );
            //MyManager.PopulateList( ddlGroupTypeList, ds, "StringValue", "StringValue", "Select a Group Type" );

        } //

        private void SetGroupList()
        {
            DoSearch();

        } //

        private void SetGroupListByType( string groupType )
        {
            //===> change to just call the search
            DoSearch();

        } //

        private void SetGroupList( string filter )
        {
            DoSearch();

        } //
        #endregion
    }
}