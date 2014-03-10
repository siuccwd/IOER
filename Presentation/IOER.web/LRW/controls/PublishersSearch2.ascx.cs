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

using ILPathways.Library;
using ILPathways.Utilities;
using LogManager = ILPathways.Utilities.LoggingHelper;
using ILPathways.classes;
using ILPathways.Business;
//using ILPathways.DAL;
using MyManager = LRWarehouse.DAL.LRManager;
using LRWarehouse.DAL;

namespace ILPathways.LRW.controls
{
    public partial class PublishersSearch2 : BaseUserControl
    {
        MyManager myManager = new MyManager();

        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
        const string thisClassName = "PublishersSearch";
        #region Properties
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
        bool _initializeOnLoad = true;
        public bool InitializingOnLoad
        {
            get
            {
                return _initializeOnLoad;
            }
            set { _initializeOnLoad = value; }
        }//
        #endregion
        /// <summary>
        /// Handle Page Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void Page_Load( object sender, EventArgs e )
        {

            if ( !Page.IsPostBack )
            {
                if ( InitializingOnLoad )
                    this.InitializeForm();
            }
        }//


        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        public void InitializeForm()
        {
            //set grid defaults (variables are in base control)
            //set sort to blank to default to results from database
            GridViewSortExpression = "";
            GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
            LastPageNumber = 0;
            //

            //handling setting of which action buttons are available to the current user
            btnSearch.Enabled = true;
            if ( this.includeCreator.Text.Equals( "yes" )
                || IsUserAuthenticated() && WebUser.UserName.ToLower().Equals( "mparsons" ) )
            {
                //searchOptionsPanel.Visible = true;
            }

            // Set source for form lists
            this.PopulateControls();
            if ( doingAutoSearch.Text.Equals( "yes" ) )
            {
                GridViewSortExpression = defaultSearchOrderBy.Text;
                DoSearch( 1, defaultSearchOrderBy.Text );
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

        protected void btnSearch_Click( object sender, EventArgs e )
        {
            if ( this.IsSearchValid() )
            {
                this.DoSearch();
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

            // Set the page size for the DataGrid control based on the selection
            CheckForPageSizeChange();

            int pTotalRows = 0;
            string filter = "";

            try
            {
                if ( searchOptions.SelectedIndex == 1 )
                {
                    filter = FormatFilter( "Creator" ); //
                    ds = myManager.CreatorSearch( filter, sortTerm, selectedPageNbr, pager1.PageSize, ref pTotalRows );
                }
                else
                {
                    filter = FormatFilter( "Publisher" );
                    ds = myManager.PublisherSearch( filter, sortTerm, selectedPageNbr, pager1.PageSize, ref pTotalRows );
                }

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
                    DataView dv = ( ( DataTable )dt ).DefaultView;
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
                    formGrid.DataSource = dv;
                    //formGrid.PageIndex = selectedPageNbr;

                    formGrid.DataBind();

                }
            }
            catch ( System.Exception ex )
            {
                //Action??		- display message and close form??	
                LogManager.LogError( ex, thisClassName + ".DoSearch() - Unexpected error encountered while attempting search. " );

                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Close this form and try again.<br>" + ex.ToString() );
            }
        }	//


        protected string FormatFilter( string keywordTerm)
        {
            string filter = "";

            string booleanOperator = "AND";

            if ( this.txtKeyword.Text.Trim().Length > 0 )
            {
                string keyword = MyManager.HandleApostrophes( FormHelper.SanitizeUserInput( txtKeyword.Text.Trim() ) );
                //extract the resource count
                int lastPos = keyword.LastIndexOf( ")" );
                if ( lastPos > 5 )
                {
                    int startPos = keyword.LastIndexOf( "(" );
                    if ( startPos < lastPos )
                        keyword = keyword.Substring( 0, startPos ).Trim();

                }
                keyword = keyword.Replace( "*", "%" );
                if ( keyword.IndexOf( "%" ) == -1 )
                {
                    if ( keyword.Length < 3 )
                        keyword = keyword + "%";
                    else
                        keyword = "%" + keyword + "%";
                }

                string where = " (" + keywordTerm + " like '" + keyword + "'	) ";
                filter += MyManager.FormatSearchItem( filter, where, booleanOperator );
              
            }



            if ( this.IsTestEnv() )
                this.SetConsoleSuccessMessage( "sql: " + filter );

            return filter;
        }	//
        protected void formGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {

            if ( e.Row.RowType == DataControlRowType.DataRow )
            {

                Label lblPub = ( Label )e.Row.FindControl( "lblPublisher" );
                string pub = DataBinder.Eval( e.Row.DataItem, "Publisher" ).ToString();
                if ( lblPub != null && pub != null && pub.ToLower() != "unknown" )
                {
                    lblPub.Text = string.Format( this.publisherDisplayTemplate.Text, pub );
                    lblPub.Visible = true;
                }
                else
                {
                    lblPub.Visible = false;
                }
            }
        }//

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
            if ( newSortExpression.ToLower() != "publisher" )
            {
                sortTerm += ", publisher ASC";
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
            DataSet ds1 = DatabaseManager.GetCodeValues( "GridPageSize", "SortOrder" );
            DatabaseManager.PopulateList( this.ddlPageSizeList, ds1, "StringValue", "StringValue", "Select Size" );

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
            InitializePageSizeList();
        }


        #endregion


    }

}