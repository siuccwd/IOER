using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Business;
using ILPathways.classes;
using ILPathways.Controllers;
using ILPLibrary = ILPathways.Library;
using ILPathways.Utilities;
using GDAL = Isle.BizServices;

using LRWarehouse.Business;
using LLibrary = ILPathways.Business.Library;
using MyManager = Isle.BizServices.LibraryBizService;
using LRWarehouse.DAL;
using MyEventController = ILPathways.Controllers.CodeTableController;
using CodesContract = ILPathways.CodeTableServiceReference.CodesDataContract;
using DataBaseHelper = LRWarehouse.DAL.BaseDataManager;
using bizService = ILPathways.CodeTableServiceReference;

namespace ILPathways.LRW.controls
{
    /// <summary>
    /// Search libraries
    /// </summary>
    public partial class LibrariesSearch : ILPLibrary.BaseUserControl
    {
        MyManager myManager = new MyManager();
        string filterDesc = "";
        MyEventController mec = new MyEventController();

        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
        const string thisClassName = "LibrariesSearch";


        #region Properties
    
        protected string CustomFilter
        {
            get
            {
                if ( ViewState[ "CustomFilter" ] == null )
                    ViewState[ "CustomFilter" ] = "";

                return ViewState[ "CustomFilter" ].ToString();
            }
            set { ViewState[ "CustomFilter" ] = value; }
        }//

        protected string CustomTitle
        {
            get
            {
                if ( ViewState[ "CustomTitle" ] == null )
                    ViewState[ "CustomTitle" ] = "";

                return ViewState[ "CustomTitle" ].ToString();
            }
            set { ViewState[ "CustomTitle" ] = value; }
        }//

        private bool _autoSearch = true;
        /// <summary>
        /// if true a search will be done immediately on page load
        /// </summary>
        public bool AutoSearch
        {
            get
            {
                return _autoSearch;
            }
            set { _autoSearch = value; }
        }//

        /// <summary>
        /// If yes, only show libraries to which user is subscribed
        /// </summary>
        public string SubscribedLibrariesView
        {
            get
            {
                return txtSubscribedLibsView.Text;
            }
            set { txtSubscribedLibsView.Text = value; }
        }//
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

        protected int LibraryImageWidth
        {
            get
            {
                if ( ViewState[ "LibraryImageWidth" ] == null )
                    ViewState[ "LibraryImageWidth" ] = 125;

                if ( IsInteger( ViewState[ "LibraryImageWidth" ].ToString() ) )
                    return Int32.Parse( ViewState[ "LibraryImageWidth" ].ToString() );
                else
                    return 0;
            }
            set { ViewState[ "LibraryImageWidth" ] = value; }
        }//
        public string DefaultLibraryImage
        {
            get
            {
                return txtDefaultLibraryImage.Text;
            }
            set { txtDefaultLibraryImage.Text = value; }
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
                this.InitializeForm();
            }
        }

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

            resultsPanel.Visible = false;
            LibraryImageWidth = UtilityManager.GetAppKeyValue( "libraryImageWidth", 125 );
            DefaultLibraryImage = UtilityManager.GetAppKeyValue( "defaultLibraryImage", "" );

            // Set source for form lists
            this.PopulateControls();

            if ( SubscribedLibrariesView == "yes" )
            {
                //not allowed, do we just show all??
                if ( IsUserAuthenticated() == false )
                {
                    SetConsoleErrorMessage( "Error: you must be logged in to the website in order to view your followed libraries.<br/>Defaulting to the public view of libraries." );
                }
                else
                {
                    CustomFilter = string.Format( subscribedLibrariesFilter.Text, WebUser.Id );
                    //which filters should be hidden??
                    //which additional functionality to allow?
                    //check if user has any favourites?
                    if ( myManager.HasLibrarySubScriptions( WebUser.Id ) == false )
                    {
                        SetConsoleInfoMessage( txtNoFavsMessage.Text );
                        AutoSearch = false;
                    }
                }
            }
            if ( AutoSearch )
                DoSearch();

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
                        //SearchType = 1;
                        this.DoSearch();
                    }
                    break;

            }
        } // end 
        
        /// <summary>
        /// Conduct a search and populate the form grid
        /// </summary>
        private void DoSearch()
        {
            int selectedPageNbr = 0;
            GridViewSortExpression = "";
            string sortTerm = GetCurrentSortTerm();

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
            this.searchSummaryDesc.Text = "";
            DateTime startTime = System.DateTime.Now;
            //??? initially fake out with default dot net paging
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
            int currentPageSize = 0;
            string filter = "";

            formGrid.DataSource = null;
            formGrid.DataBind();

            if ( CustomTitle.Length > 0 )
            {

            }

            try
            {
                filter = FormatFilter();

                currentPageSize = formGrid.PageSize;

                ds = myManager.LibrarySearch( filter, sortTerm, selectedPageNbr, pager1.PageSize, ref pTotalRows );

                pager1.ItemCount = pager2.ItemCount = pTotalRows;
                LastTotalRows = pTotalRows;

                searchSummaryDesc.Text = filterDesc;

                if ( DataBaseHelper.DoesDataSetHaveRows( ds ) == false )
                {
                    if ( Page.IsPostBack == true )
                    {
                        resultsPanel.Visible = false;
                        SetConsoleInfoMessage( "No records were found for the provided search criteria" );
                        ddlPageSizeList.Enabled = false;
                        pager1.Visible = false;
                        pager2.Visible = false;
                        formGrid.DataSource = null;
                        formGrid.DataBind();
                    }
                }
                else
                {
                    resultsPanel.Visible = true;
                    ddlPageSizeList.Enabled = true;

                    DataTable dt = ds.Tables[ 0 ];
                    //DataView dv = ( ( DataTable ) dt ).DefaultView;
                    //if ( sortTerm.Length > 0 )
                    //    dv.Sort = sortTerm;

                    if ( pTotalRows > currentPageSize )
                    {
                        //formGrid.PagerSettings.Visible = true;
                        pager1.Visible = pager2.Visible = true;
                    }
                    else
                    {
                        pager1.Visible = pager2.Visible = false;
                    }


                    //populate the grid
                    formGrid.Visible = true;
                    formGrid.DataSource = dt;
                    formGrid.DataBind();


                }

            }
            catch ( System.Exception ex )
            {
                //Action??		- display message and close form??	
                LoggingHelper.LogError( ex, thisClassName + ".DoSearch() - Unexpected error encountered while attempting search. " );
                if ( ex.Message.ToLower().IndexOf( "timeout period elapsed prior" ) > -1 )
                {
                    this.SetConsoleErrorMessage( "Sorry your query was taking too long and the timeout period has elaspsed<br>You could try again and use some for the checkbox values to improve the search capability.<br>" );
                    pager1.Visible = false;
                    pager2.Visible = false;
                }
                else
                {
                    this.SetConsoleErrorMessage( "Unexpected error encountered<br>Close this form and try again.<br>" + ex.ToString() );
                }
            }
            DateTime endTime = System.DateTime.Now;
            TimeSpan duration = endTime.Subtract( startTime );

            if ( this.IsTestEnv() )
            {
                this.SetConsoleInfoMessage( "duration: " + duration.TotalSeconds.ToString() );
            }

        }	//

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
        /// call for an initial search
        /// </summary>
        /// <returns></returns>
        protected string FormatFilter()
        {

            string booleanOperator = "AND";
            string filter = "";
            //filter = "(lib.IsDiscoverable = 1) ";
            if ( CustomFilter.Length > 0 )
            {
                //skip for now, checking below==> address with future custom filters
                filter += DataBaseHelper.FormatSearchItem( filter, CustomFilter, booleanOperator );
            }
            
            filterDesc = "";


            //
            FormatDatesFilter( booleanOperator, ref filter );
            //
            FormatLibTypeFilter( this.cbxlLibraryType, booleanOperator, ref filter, ref filterDesc );
            //
            FormatViewableFilter( booleanOperator, ref filter );

            FormatKeyword( txtKeyword, booleanOperator, ref filter );

            if ( this.IsTestEnv() || GetRequestKeyValue( "debug", "" ) == "yes" )
            {
                this.SetConsoleSuccessMessage( "sql: " + filter );
                LoggingHelper.DoTrace( 6, "sql: " + filter );
            }
            return filter;
        }	//

        protected void FormatKeyword( TextBox textBox, string booleanOperator, ref string filter )
        {
            string keyword = DataBaseHelper.HandleApostrophes( FormHelper.SanitizeUserInput( textBox.Text.Trim() ) );
            string keywordFilter = "";

            if ( keyword.Length > 0 )
            {
                filterDesc = filterDesc + "<div class='searchSection isleBox'>" + keyword + "</div>";
                keyword = keyword.Replace( "*", "%" );
                if ( keyword.IndexOf( "," ) > -1 )
                {
                    string[] phrases = keyword.Split( new char[] { ',' } );
                    foreach ( string phrase in phrases )
                    {
                        string next = phrase.Trim();
                        if ( next.IndexOf( "%" ) == -1 )
                            next = "%" + next + "%";
                        string where = string.Format( this.keywordTemplate.Text, next );
                        keywordFilter += DataBaseHelper.FormatSearchItem( keywordFilter, where, "OR" );
                    }
                }
                else
                {
                    if ( keyword.IndexOf( "%" ) == -1 )
                        keyword = "%" + keyword + "%";

                    keywordFilter = string.Format( keywordTemplate.Text, keyword );

                }

                if ( keywordFilter.Length > 0 )
                    filter += DataBaseHelper.FormatSearchItem( filter, keywordFilter, booleanOperator );
            }
        }	//
        private void FormatDatesFilter( string booleanOperator, ref string filter )
        {


            DateTime endDate;
            if ( rblIDateCreated.SelectedIndex == 0 )
            {

                endDate = System.DateTime.Now.AddDays( -7 );
            }
            else if ( rblIDateCreated.SelectedIndex == 1 )
            {
                endDate = System.DateTime.Now.AddDays( -30 );
            }
            else if ( rblIDateCreated.SelectedIndex == 2 )
            {
                endDate = System.DateTime.Now.AddMonths( -6 );
            }
            else if ( rblIDateCreated.SelectedIndex == 3 )
            {
                endDate = System.DateTime.Now.AddYears( -1 );
            }
            else
            {
                return;
            }

            string where = string.Format( " ResourceLastAddedDate > '{0}'", endDate.ToString( "yyyy-MM-dd" ) );
            filter += DataBaseHelper.FormatSearchItem( filter, where, booleanOperator );
            string selDesc = string.Format( " Added after {0}", endDate.ToString( "yyyy-MM-dd" ) );
            filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
        }
        private void FormatViewableFilter( string booleanOperator, ref string filter )
        {

            if ( rblViewable.SelectedIndex == 0 )
            {
                //all, do nothing
            }
            else if ( rblViewable.SelectedIndex == 1 )
            {
                filter += DataBaseHelper.FormatSearchItem( filter, "lib.PublicAccessLevel > 1", booleanOperator );
                filterDesc = filterDesc + "<div class='searchSection isleBox'>Public libraries only</div>";
            }
            else if ( rblViewable.SelectedIndex == 2 )
            {
                filter += DataBaseHelper.FormatSearchItem( filter, "lib.PublicAccessLevel = 1", booleanOperator );
                filterDesc = filterDesc + "<div class='searchSection isleBox'>Private libraries only</div>";
            }
            else if ( rblViewable.SelectedIndex == 3 )
            {
                if ( IsUserAuthenticated() )
                {
                    filter += DataBaseHelper.FormatSearchItem( filter, string.Format( "lib.Id in (Select LibraryId from [Library.Member] where userid = {0} )", WebUser.Id), booleanOperator );
                    filterDesc = filterDesc + "<div class='searchSection isleBox'>I am Library Member</div>";
                }
                else
                {
                    SetConsoleInfoMessage( "Note: you must be logged in to use the option: <i>Where Library Member</i>" );
                }
            }
            else
            {
                return;
            }
        }

        public static void FormatLibTypeFilter( CheckBoxList cbxl, string booleanOperator, ref string filter, ref string filterDesc )
        {
            string csv = "";
            string selDesc = "";
            string comma = "";
            foreach ( ListItem li in cbxl.Items )
            {
                if ( li.Selected )
                {
                    string item = SearchController.ExtractParens( li.Text );
                    csv += li.Value + ",";

                    selDesc += comma + item;
                    comma = ", ";
                }
            }
            if ( csv.Length > 0 )
            {
                csv = csv.Substring( 0, csv.Length - 1 );

                string where = string.Format( " (lib.LibraryTypeId in ({0})) ", csv );
                filter += DataBaseHelper.FormatSearchItem( filter, where, booleanOperator );
                filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
            }
        }

        /// <summary>
        /// not used yet
        /// </summary>
        /// <param name="cbxl"></param>
        /// <param name="booleanOperator"></param>
        /// <param name="filter"></param>
        /// <param name="filterDesc"></param>
        private void FormatHasStandardsFilter( CheckBoxList cbxl, string booleanOperator, ref string filter, ref string filterDesc )
        {
            //future: any math or any lit
            //also could build the cbxl with totals
            string selDesc = "Has any CCSS standard";

            //should only be one for now
            bool isSelected = false;
            foreach ( ListItem li in cbxl.Items )
            {
                if ( li.Selected )
                {
                    isSelected = true;
                }
            }
            if ( isSelected )
            {
                string where = "lr.id in (select ResourceIntId from  (SELECT [ResourceIntId], count(*) As RecCount FROM [dbo].[Resource.Standard] group by [ResourceIntId] ) As ResStandards )";
                filter += DataBaseHelper.FormatSearchItem( filter, where, booleanOperator );
                filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
            }
        }
        /// <summary>
        /// no longer used
        /// </summary>
        /// <returns></returns>
        protected void formGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {

            if ( e.Row.RowType == DataControlRowType.DataRow )
            {

            }
        }//
        public string FormatLibraryImage( string imageUrl )
        {
            string imageTag = "";

            if ( imageUrl.Trim().Length < 15 || imageUrl.Trim() == "defaultUrl" )
            {
                if ( DefaultLibraryImage.Length > 10 )
                    imageTag = string.Format( txtLibraryImageTemplate.Text, DefaultLibraryImage, txtImgageWidth.Text, txtLowOpacity.Text );
            }
            else
            {
                imageTag = string.Format( txtLibraryImageTemplate.Text, imageUrl, txtImgageWidth.Text, "" );
            }

            return imageTag;
        }
        /// <summary>
        /// Reset selected item on sort
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void formGrid_Sorted( object sender, EventArgs ex )
        {
            //clear selected index
            formGrid.SelectedIndex = -1;

        }	//
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
                GridViewSortExpression = newSortExpression;
                //handle assumptions: if by last updated or total resources, assume desc
                if ( "TotalResources ResourceLastAddedDate".IndexOf( newSortExpression ) > -1 )
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
            if ( newSortExpression.ToLower().IndexOf("lib.title" ) == -1)
            {
                sortTerm += ", lib.Title ASC";
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
            pager2.CurrentIndex = currentPageIndx;
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

                DoSearch( 0, sortTerm );

            }
        } //
        #endregion
        #region Housekeeping
        /// <summary>
        /// Populate form controls
        /// </summary>
        private void PopulateControls()
        {
            //
            InitializePageSizeList();
            PopulateLibType();

        } //

        void PopulateLibType()
        {
            DataSet ds = myManager.SelectLibraryTypes();
            ResourceCheckBoxFiller.ConstructListItems( "title", ref this.cbxlLibraryType, ds, false, "custom" );

        }



        #endregion

    }

}
