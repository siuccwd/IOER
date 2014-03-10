using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using ILPathways.Utilities;
using ILPathways.classes;
using ILPathways.Business;
using GDAL = Isle.BizServices;

using LRWarehouse.DAL;

using MyManager = LRWarehouse.DAL.LRManager;

namespace ILPathways.Admin.mapping
{
    public partial class MapCareerClusters : ILPathways.Library.BaseAppPage
    {
        const string thisClassName = "MapCareerClusters";

        const string DELETE_SQL = "DELETE FROM [dbo].[Map.CareerCluster] WHERE id = {0} ";

        // BaseUserControl baseUC = new BaseUserControl();

        #region Properties

        public string SELECT_SQL
        {
            get { return this.selectSql.Text; }
        }
        public string INSERT_SQL
        {
            get { return this.insertSql.Text; }
        }
        public string UPDATE_SQL
        {
            get { return this.updateSql.Text; }
        }

        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string FormSecurityName
        {
            get { return this.txtFormSecurityName.Text; }
            set { this.txtFormSecurityName.Text = value; }
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
            //if ( WebUser == null )
            //{
            //    //SetConsoleErrorMessage( "You must be authenticated and authorized to use this page. <br/>Please login and try again." );
            //    return;
            //}

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

            //if ( IsTestEnv() )  //TEMP
            //{
            //    FormPrivileges.SetAdminPrivileges();

            //}
            //else 
            if ( this.IsUserAuthenticated() == false )
            {
                FormPrivileges = new ApplicationRolePrivilege();
                FormPrivileges.SetReadOnly();
                FormPrivileges.ReadPrivilege = 1;
            }
            else
            {
                // get form privileges
                FormPrivileges = GDAL.SecurityManager.GetGroupObjectPrivileges( WebUser, FormSecurityName );

            }



            //handling setting of which action buttons are available to the current user
            this.btnNew.Enabled = false;
            //|| WebUser.TopAuthorization < 3 
            if ( FormPrivileges.CanCreate() )
            {
                this.btnNew.Enabled = true;
            }
            PopulateControls();

            if ( UtilityManager.GetAppKeyValue( "isSearchActive", "no" ) == "yes" )
            {

                DoSearch();
            }


        }	// End 

        protected void PopulateClusters( DropDownList ddl )
        {

            PopulateClusters( ddl, false );
        }	// End 

        protected void PopulateClusters( DropDownList ddl, bool selectFirst )
        {

            string sql = clustersSelect.Text;
            DataSet ds = DatabaseManager.DoQuery( sql );

            if ( MyManager.DoesDataSetHaveRows( ds ) )
            {
                MyManager.PopulateList( ddl, ds, "Id", "Title", "Select a Cluster" );
                if ( selectFirst )
                {
                    ddl.SelectedIndex = 1;
                }

            }
        }	// End 

        protected void addButton_Click( object sender, EventArgs e )
        {
            try
            {
                if ( ddlClusters.SelectedIndex < 1 )
                {
                    SetConsoleErrorMessage( "Error: a Career Cluster must be selected" );
                    return;
                }
                if ( txtMapping.Text.Trim().Length < 2 )
                {
                    SetConsoleErrorMessage( "Error: a mapping filter must be entered" );
                    return;
                }
                string lrrt = DatabaseManager.HandleApostrophes( this.txtMapping.Text );
                string codeId = this.ddlClusters.SelectedValue.ToString();


                string sql = string.Format( INSERT_SQL, lrrt, codeId, WebUser.UserName );

                DatabaseManager.ExecuteSql( sql );
                SetConsoleSuccessMessage( "Successfully added the new mapping of: " + lrrt );
                DoSearch();

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".addButton_Click() - Unexpected error encountered" );
                SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }

        }


        #region Grid methods
        protected void ddlFilterCluster_OnSelectedIndexChanged( object sender, System.EventArgs ea )
        {

            int index = ddlFilterCluster.SelectedIndex;
            DoSearch( 0, "" );
        } //
        private void DoSearch()
        {
            int selectedPageNbr = 0;
            GridViewSortExpression = "";
            string sortTerm = "";   // GetCurrentSortTerm();

            DoSearch( selectedPageNbr, sortTerm );
        } //
        private void DoSearch( int selectedPageNbr, string sortTerm )
        {
            if ( selectedPageNbr == 0 )
            {
                //with custom pager, need to start at 1
                selectedPageNbr = 1;
            }
            LastPageNumber = selectedPageNbr;
            pager1.CurrentIndex = pager2.CurrentIndex = selectedPageNbr;

            // Set the page size for the DataGrid control based on the selection
            CheckForPageSizeChange();


            string sql = "";
            if ( ddlFilterCluster.SelectedIndex > 0 )
            {
                sql = string.Format( SELECT_SQL, string.Format( " where MappedClusterId = {0} ", ddlFilterCluster.SelectedValue ) );
            }
            else
            {
                sql = string.Format( SELECT_SQL, " " );
            }
            DataSet ds = DatabaseManager.DoQuery( sql );

            this.txtMapping.Text = "";
            //don't reset the selected cluster
            //ddlClusters.SelectedIndex = 0;

            this.rfvCluster.Enabled = true;
            this.rfvFilterValue.Enabled = true;

            if ( FormPrivileges.CanCreate() )
                this.addPanel.Visible = true;

            if ( MyManager.DoesDataSetHaveRows( ds ) && ds.Tables[ 0 ].TableName.ToLower() != "resultmessagetable" )
            {
                try
                {
                    if ( FormPrivileges.CanCreate() == false )
                        formGrid.AutoGenerateEditButton = false;

                    formGrid.DataSource = ds;
                    formGrid.DataBind();

                    formGrid.Visible = true;
                    gridPanel.Visible = true;
                    if ( FormPrivileges.CanDelete() == false )
                        formGrid.Columns[ 0 ].Visible = false;
                }
                catch ( Exception ex )
                {
                    SetConsoleErrorMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.Message );
                    LogError( ex, thisClassName + ".DoSearch" );
                }
                finally
                {
                    ds.Dispose();
                }

            }
            else
            {
                if ( MyManager.DoesDataSetHaveRows( ds ) && ds.Tables[ 0 ].TableName.ToLower() == "resultmessagetable" )
                {
                    string msg = DatabaseManager.GetRowColumn( ds.Tables[ 0 ].Rows[ 0 ], "RESULTS_TABLE", "" );
                    if ( msg.Length > 0 )
                        SetConsoleErrorMessage( msg );
                }

                formGrid.DataSource = null;
                formGrid.DataBind();
            }
        }
        protected void formGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                System.Data.DataRowView drv = ( DataRowView ) e.Row.DataItem;
                if ( ( e.Row.RowState & DataControlRowState.Edit ) == DataControlRowState.Edit )
                {
                    FormatEditRow( drv, e );
                }
                else
                {
                    //check if item is in use, cannot delete these records
                    LinkButton delBtn = ( LinkButton ) e.Row.FindControl( "deleteRowButton" );
                    if ( FormPrivileges.CanDelete() )
                    {
                        delBtn.Enabled = true;
                        delBtn.Attributes.Add( "onclick", "javascript:return " +
                                "confirm('Are you sure you want to delete this record (" +
                                DataBinder.Eval( e.Row.DataItem, "ClusterTitle" ) + ")')" );
                    }
                    else
                    {
                        delBtn.Visible = false;
                    }
                }
            }
        }
        /// <summary>
        /// Format a row about to be edited
        /// </summary>
        /// <param name="drv"></param>
        /// <param name="e"></param>
        protected void FormatEditRow( DataRowView drv, GridViewRowEventArgs e )
        {

            //Populate Drop-down list on Row in Data Grid, NOT the Drop-down list at the top of the control.
            DropDownList ddl = ( DropDownList ) e.Row.FindControl( "gridddlClusters" );
            PopulateClusters( ddl );

            string clusterId = DatabaseManager.GetRowColumn( drv, "MappedClusterId", "" );
            //if ( siteFormat.Length > 0 )
            if ( clusterId.Length > 0 )
            {
                this.SetListSelection( ddl, clusterId.ToString() );
            }
            else
            {
                ddl.SelectedIndex = 0;
            }

        }

        protected void formGrid_RowCommand( object sender, GridViewCommandEventArgs e )
        {
            if ( e.CommandName == "DeleteRow" )
            {
                // get the ID of the clicked row
                string id = e.CommandArgument.ToString();

                // Delete the record 
                DeleteRecordByID( id );
            }
        }


        protected void DeleteRecordByID( string id )
        {
            try
            {
                string sql = string.Format( DELETE_SQL, id );
                if ( DatabaseManager.ExecuteSql( sql ) == "Successful" )
                {
                    //OK
                    SetConsoleSuccessMessage( "Requested record was deleted" );
                    DoSearch();
                }
                else
                {

                }


            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".DeleteRecordByID() - Unexpected error encountered" );
                SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }
        }

        /// <summary>
        /// fires when edit link is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void EditRecord( object sender, GridViewEditEventArgs e )
        {

            formGrid.EditIndex = e.NewEditIndex;
            DoSearch();

            formGrid.FooterRow.Visible = false;
            this.rfvCluster.Enabled = false;
            this.rfvFilterValue.Enabled = false;

        }

        /// <summary>
        /// fires when cancel link is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void CancelRecord( object sender, GridViewCancelEditEventArgs e )
        {
            formGrid.EditIndex = -1;
            DoSearch();
            this.rfvCluster.Enabled = false;
            this.rfvFilterValue.Enabled = false;
        }

        /// <summary>
        /// fires when update link is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void UpdateRecord( object sender, GridViewUpdateEventArgs e )
        {

            try
            {
                GridViewRow row = ( GridViewRow ) formGrid.Rows[ e.RowIndex ];
                string id = formGrid.DataKeys[ e.RowIndex ].Value.ToString();

                //get values from grid 
                TextBox filterValue = formGrid.Rows[ e.RowIndex ].FindControl( "gridFilterValue" ) as TextBox;
                string filter = DatabaseManager.HandleApostrophes( filterValue.Text );

                string sql = string.Format( UPDATE_SQL, id, filter, WebUser.UserName );


                //DropDownList ddl = ( DropDownList ) row.FindControl( "gridddlClusters" );
                //if ( ddl != null && ddl.SelectedIndex > 0 )
                //{
                //    //string codeId = ddl.SelectedValue;
                //    //string sql = string.Format( UPDATE_SQL, id, codeId, filter, WebUser.UserName );

                //update the record
                string status = DatabaseManager.ExecuteSql( sql );
                if ( status.ToLower() == "successful" )
                {
                    SetConsoleSuccessMessage( "Successfully updated record" );
                    // Refresh the data
                    formGrid.EditIndex = -1;

                    DoSearch();
                }
                else
                {
                    SetConsoleErrorMessage( "Unexpected status encountered while attempting to update this record: <br/>" + status );
                }
                //}
                //else
                //{
                //    SetConsoleErrorMessage( "Error - select a cluster from the list and try again." );
                //}

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".UpdateRecord" );

            }
        }

        #endregion

        #region Paging related methods
        public void pager_Command( object sender, CommandEventArgs e )
        {

            int currentPageIndx = Convert.ToInt32( e.CommandArgument );
            pager1.CurrentIndex = currentPageIndx;
            pager2.CurrentIndex = pager1.CurrentIndex;
            //string sortTerm = GetCurrentSortTerm();

            DoSearch( currentPageIndx, "" );

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
                //string sortTerm = GetCurrentSortTerm();

                //DoSearch( 0, sortTerm );
                DoSearch( 0, "" );

            }
        } //
        #endregion
        #region Housekeeping
        private void PopulateControls()
        {
            InitializePageSizeList();

            PopulateClusters( this.ddlClusters );
            PopulateClusters( this.ddlFilterCluster, true );
        } //
        #endregion
    }
}