using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using ILPathways.Common;
using ILPathways.Utilities;
using EmailHelper = ILPathways.Utilities.EmailManager;
using MyManager = Isle.BizServices.OrganizationBizService;
using ILPathways.DAL;
using ILPathways.Business;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using LDBM = LRWarehouse.DAL.DatabaseManager;

namespace ILPathways.Admin.Org
{
    public partial class Organizations : BaseAppPage
    {
        const string thisClassName = "Organizations";
        MyManager myManager = new MyManager();

        #region Properties
        public int LastOrgId
        {
            get
            {
                if ( Session[ "LastOrgId" ] == null )
                    Session[ "LastOrgId" ] = "0";

                return Int32.Parse( Session[ "LastOrgId" ].ToString() );
            }
            set { Session[ "LastOrgId" ] = value.ToString(); }
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

        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !this.IsPostBack )
            {
                this.InitializeForm();
            }
        }

        private void InitializeForm()
        {
            PopulateControls();
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
        }

        #region form grid related methods
       
        protected void SearchButton_Click( object sender, System.EventArgs e )
        {
            DoSearch();

        }
       
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
            if ( selectedPageNbr == 0 )
            {
                //with custom pager, need to start at 1
                selectedPageNbr = 1;
            }
            LastPageNumber = selectedPageNbr;
            pager1.CurrentIndex = pager2.CurrentIndex = selectedPageNbr;
            TabContainer1.ActiveTabIndex = 0;
            // Set the page size for the DataGrid control based on the selection
            CheckForPageSizeChange();

            int pTotalRows = 0;
            string filter = FormatFilter();


            
            try
            {
                List<Organization> list = myManager.Organization_Search( filter, sortTerm, selectedPageNbr, pager1.PageSize, ref pTotalRows );

                if ( list == null || list.Count ==  0)
                {
                    resultsPanel.Visible = false;
                    SetConsoleInfoMessage( "No records were found for the provided search criteria" );
                    ddlPageSizeList.Enabled = false;
                }
                else
                {
    
                    resultsPanel.Visible = true;
                    ddlPageSizeList.Enabled = true;

                    //searchPanel.Visible = false;

                    //DataTable dt = ds.Tables[ 0 ];
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
                    formGrid.DataSource = list;
                    //formGrid.PageIndex = selectedPageNbr;

                    formGrid.DataBind();

                }
            }
            catch ( System.Exception ex )
            {
                //Action??		- display message and close form??	
                LoggingHelper.LogError( ex, thisClassName + ".DoSearch() - Unexpected error encountered while attempting search. User: " + WebUser.FullName() );

                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Close this form and try again.<br>" + ex.ToString() );
            }
        }	//

        protected string FormatFilter()
        {
            string filter = "";
            string booleanOperator = "AND";

            int typeId = 0;
            if ( ddlOrgType.SelectedIndex > 0 )
            {
                //status = this.ddlStatus.SelectedValue.ToString();
                typeId = Int32.Parse( this.ddlOrgType.SelectedValue.ToString() );
                filter += MyManager.FormatSearchItem( filter, "OrgTypeId", typeId, booleanOperator );
            }

            //keyword
            string keywordFilter = "";
            if ( txtKeyword.Text.Trim().Length > 0 )
            {
                //updated to check code or title for filter
                keywordFilter = LDBM.HandleApostrophes( FormHelper.CleanText( txtKeyword.Text ) );
                keywordFilter = keywordFilter.Replace( "*", "%" );
                if ( keywordFilter.IndexOf( "%" ) == -1 )
                {
                    keywordFilter = "%" + keywordFilter + "%";
                }

                string where = " (base.Name like '" + keywordFilter + "'	OR base.City like '" + keywordFilter + "') ";
                filter += MyManager.FormatSearchItem( filter, where, booleanOperator );
            }
            //if ( this.IsTestEnv() )
            //  this.SetConsoleSuccessMessage( "sql: " + filter );
            return filter;
        }	//

        protected void formGrid_RowCommand( object sender, GridViewCommandEventArgs e )
        {
            if ( e.CommandName == "DeleteRow" )
            {


            }
            else if ( e.CommandName == "SelectRow" )
            {
                // get the ID of the clicked row
                int recordId = Convert.ToInt32( e.CommandArgument );

                if ( recordId > 0 )
                {
                    LastOrgId = recordId;
                    TabContainer1.ActiveTabIndex = 1;
                   // this.Get( recordId );
                }
            }
        }


        /// <summary>
        /// Reset selected item on sort
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void formGrid_Sorted( object sender, GridViewSortEventArgs ex )
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
                GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
                GridViewSortExpression = newSortExpression;
                sortTerm = newSortExpression + " ASC";
            }

            DoSearch( 0, sortTerm );

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
            int defaultPageSize = this.formGrid.PageSize;
            //this.formGrid.PageSize = defaultPageSize;

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
                    //Update user preference - NONE
                    //Session[ SessionManager.SYSTEM_GRID_PAGESIZE ] = ddlPageSizeList.SelectedItem.Text;
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
            }
        }

        #endregion

        private void PopulateControls()
        {
            List<CodeItem> list = MyManager.OrgType_Select();
            LDBM.PopulateList( this.ddlOrgType, list, "Id", "Title", "Select an org. type" );

            //
            //InitializePageSizeList();
        } //
    }
}