using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using IOER.Library;
using ILPathways.Utilities;
using LogManager = ILPathways.Utilities.LoggingHelper;
using IOER.classes;
using ILPathways.Business;
using ILPathways.DAL;
using MyManager = ILPathways.DAL.TemplateManager;
using SecurityManager = Isle.BizServices.GroupServices;
using LRWarehouse.DAL;
using LDBM = LRWarehouse.DAL.DatabaseManager;

namespace IOER.Controls._Templates
{
    public partial class Search : BaseUserControl
    {
        MyManager myManager = new MyManager();

        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
        const string thisClassName = "@template";
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
        #endregion
        /// <summary>
        /// Handle Page Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void Page_Load( object sender, EventArgs e )
        {

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
            //set grid defaults (variables are in base control)
            //set sort to blank to default to results from database
            GridViewSortExpression = "";
            GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
            LastPageNumber = 0;
            //
            if ( FormSecurityName.Length > 0 )
            {
                this.FormPrivileges = SecurityManager.GetGroupObjectPrivileges( this.WebUser, FormSecurityName );
            }
            else
            {
                //no security
                this.FormPrivileges = new ApplicationRolePrivilege();
                FormPrivileges.CreatePrivilege = 1;
                FormPrivileges.ReadPrivilege = 1;
                FormPrivileges.WritePrivilege = 1;
                FormPrivileges.DeletePrivilege = 1;
            }
            //handling setting of which action buttons are available to the current user
            btnSearch.Enabled = false;

            if ( FormPrivileges.CanCreate() )
            {
                //addLink.Enabled = true;
            }

            if ( FormPrivileges.CanRead() )
            {
                btnSearch.Enabled = true;
                //determine extent of project view
                if ( FormPrivileges.ReadPrivilege == ( int ) ILPathways.Business.EPrivilegeDepth.Local )
                {

                }
                else if ( FormPrivileges.ReadPrivilege > ( int ) ILPathways.Business.EPrivilegeDepth.State )
                {

                }
            }

            //


            // Set source for form lists
            this.PopulateControls();
            //optional:
            CheckRecordRequest();
        }	// End 
        /// <summary>
        /// Check the request type for the form
        /// </summary>
        private void CheckRecordRequest()
        {

            // Check if a request was made for a specific search parms
            int id = this.GetRequestKeyValue( "id", 0 );

            if ( id > 0 )
            {
                //may set a perm property for use with the search?

            }

        }	// End 
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
                    //this.HandleNewRequest();
                    break;

                case "Search":
                    if ( this.IsSearchValid() )
                    {
                        this.DoSearch();
                    }
                    break;

            }
        } // end 


        #endregion

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
                ds = myManager.Search( filter, sortTerm, selectedPageNbr, pager1.PageSize, ref pTotalRows );

                //assumes min rows are returned, so pager needs to have the total rows possible
                pager1.ItemCount = pager2.ItemCount = pTotalRows;

                LastTotalRows = pTotalRows;

                if ( MyManager.DoesDataSetHaveRows( ds ) == false )
                {
                    resultsPanel.Visible = false;
                    SetConsoleErrorMessage( "No records were found for the provided search criteria" );
                    ddlPageSizeList.Enabled = false;
                    pager1.Visible = false;
                    pager2.Visible = false;
                }
                else
                {

                    resultsPanel.Visible = true;
                    ddlPageSizeList.Enabled = true;

                    //searchPanel.Visible = false;

                    DataTable dt = ds.Tables[ 0 ];
                    //DataView dv = ( ( DataTable ) dt ).DefaultView;
                    //if ( sortTerm.Length > 0 )
                    //    dv.Sort = sortTerm;

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
                    formGrid.DataSource = dt;
                    //formGrid.PageIndex = selectedPageNbr;

                    formGrid.DataBind();

                }
            }
            catch ( System.Exception ex )
            {
                //Action??		- display message and close form??	
                LogManager.LogError( ex, thisClassName + ".DoSearch() - Unexpected error encountered while attempting search."  );

                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Close this form and try again.<br>" + ex.ToString() );
            }
        }	//


        protected string FormatFilter()
        {
            string filter = "";

            string booleanOperator = "AND";

            if ( txtKeyword.Text.Trim().Length > 0 )
            {
                string keyword = MyManager.HandleApostrophes( FormHelper.CleanText( txtKeyword.Text.Trim() ) );

                if ( keyword.IndexOf( "%" ) == -1 )
                    keyword = "%" + keyword + "%";

                string where = " (prj.Title like '" + keyword + "'	OR prj.[Description] like '" + keyword + "') ";
                filter += MyManager.FormatSearchItem( filter, where, booleanOperator );
            }

            //string status = "";
            int statusId = 0;
            if ( ddlStatus.SelectedIndex > 0 )
            {
                //status = this.ddlStatus.SelectedValue.ToString();
                statusId = Int32.Parse( this.ddlStatus.SelectedValue.ToString() );
                filter += MyManager.FormatSearchItem( filter, "StatusId", statusId, booleanOperator );
            }

            //if ( this.IsTestEnv() )
            //  this.SetConsoleSuccessMessage( "sql: " + filter );

            return filter;
        }	//
        protected void formGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            string url = Request.Url.ToString();

            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                //select
                if ( openingDetailInNewWindow.Text.Equals( "yes" ) )
                {
                    LinkButton slb = ( LinkButton ) e.Row.FindControl( "selectButton" );
                    slb.PostBackUrl = url + "?id=" + DataBinder.Eval( e.Row.DataItem, "projectId" );

                    //slb.Attributes.Add( "onclick", "javascript:return " +
                    //		"confirm('Are you sure you want to select this project " +
                    //		DataBinder.Eval( e.Row.DataItem, "projectId" ) + "')" );
                }

                //add delete confirmation
                LinkButton dlb = ( LinkButton ) e.Row.FindControl( "deleteRowButton" );
                //check if user can delete
                if ( FormPrivileges.CanDelete() )
                {
                    dlb.Attributes.Add( "onclick", "javascript:return " +
                            "confirm('Are you sure you want to delete this project " +
                            DataBinder.Eval( e.Row.DataItem, "projectId" ) + "')" );
                }
                else
                {
                    dlb.Enabled = false;
                }
            }
        }//

        protected void formGrid_RowCommand( object sender, GridViewCommandEventArgs e )
        {
            if ( e.CommandName == "DeleteRow" )
            {
                // get the ID of the clicked row
                int ID = Convert.ToInt32( e.CommandArgument );

                // Delete the record 
                DeleteRecord( ID );

            }
            else if ( e.CommandName == "SelectRow" )
            {
                pager1.CurrentIndex = pager2.CurrentIndex = LastPageNumber;
                pager1.ItemCount = pager2.ItemCount = LastTotalRows;

                // get the ID of the clicked row
                int ID = Convert.ToInt32( e.CommandArgument );

                // show the record 
                ShowRecord( ID );
            }
        }

        /// <summary>
        /// show selected row
        /// </summary>
        /// <param name="projectId"></param>
        protected void ShowRecord( int projectId )
        {
            try
            {
                string url = this.projectDetailUrl.Text + "?id=" + projectId.ToString();
                if ( openingDetailInNewWindow.Text == "yes" )
                {
                    url = UtilityManager.FormatAbsoluteUrl( url, true );
                    BrowserWindow win = new BrowserWindow( url, 1024, 800 );
                    win.MenuBar = true;
                    win.Resizable = true;
                    win.ScrollBars = true;
                    win.AttachToPageLoad( this );

                }
                else
                {
                    Response.Redirect( url, true );
                }
            }
            catch ( System.Threading.ThreadAbortException ex )
            {
                //UtilityManager.DoTrace( 3, "includes_header4.Page_Load - System.Threading.ThreadAbortException. Google Search      section Path = " + Request.Path );

            }
            catch ( Exception ex )
            {
                LogManager.LogError( ex, thisClassName + ".ShowRecord() - Unexpected error encountered while attempting to delete project id of " + projectId.ToString() );
                this.SetConsoleErrorMessage( "An unexpected error was encountered while attempting to delete this record. System administration has been notified:<br/> " + ex.Message.ToString() );
            }
        }
        /// <summary>
        /// handle row delete
        /// </summary>
        /// <param name="projectId"></param>
        protected void DeleteRecord( int projectId )
        {
            try
            {
                string statusMessage = "";

                if ( myManager.Delete( projectId, ref statusMessage ) )
                {
                    this.SetConsoleSuccessMessage( "Delete was successful" );
                    //OK reset list
                    string sortTerm = GetCurrentSortTerm();

                    DoSearch( pager1.CurrentIndex, sortTerm );
                    //DoSearch();
                }
                else
                {
                    // problem
                    this.SetConsoleErrorMessage( "An unexpected issue was encountered while attempting to delete this record. System administration has been notified:<br/> " + statusMessage );
                    LogManager.LogError( thisClassName + ".DeleteRecord() - Delete failed for project id of " + projectId.ToString() + " and returned the following message:<br/>" + statusMessage, true );
                }

            }
            catch ( Exception ex )
            {
                LogManager.LogError( ex, thisClassName + ".DeleteRecord() - Unexpected error encountered while attempting to delete project id of " + projectId.ToString() );
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

            //consider helper: if main sort is not the main title, add as the secondary sort
            //if ( newSortExpression.ToLower() != "title" )
            //{
            //  sortTerm += ", Title ASC";
            //}

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
            DataSet ds1 = LDBM.GetCodeValues( "GridPageSize", "SortOrder" );
            LDBM.PopulateList( this.ddlPageSizeList, ds1, "StringValue", "StringValue", "Select Size" );

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

            DataSet ds = LDBM.SelectCodesTable( "[Codes.ProjectStatus]", "Code", "title", "Code", "" );
            MyManager.PopulateList( ddlStatus, ds, "Code", "title", "Select a Status type" );

            DataSet ds1 = myManager.Select( 0, "tbd");
            MyManager.PopulateList( ddlOrgList, ds1, "OrgId", "Name", "Select a project organization" );

            //
            InitializePageSizeList();
        } //

        #endregion


    }

}