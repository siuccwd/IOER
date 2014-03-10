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
//using workNet.DAL;

namespace ILPathways.Admin.mapping
{
    public partial class MapResourceType : ILPathways.Library.BaseAppPage
    {
        const string thisClassName = "MapResourceType";

        const string DELETE_SQL = "DELETE FROM [dbo].[Map.ResourceType] WHERE id = {0} ";

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
            if ( FormPrivileges.CanCreate()                 )
            {
                this.btnNew.Enabled = true;
            }

            if ( UtilityManager.GetAppKeyValue( "isSearchActive", "no" ) == "yes" )
            {
                PopulateResourceTypes( ddlResourceType );

                DoSearch();
            }

        }	// End 

        protected void PopulateResourceTypes( DropDownList ddl )
        {

            string sql = resourceTypeSelect.Text;
            DataSet ds = DatabaseManager.DoQuery( sql );

            MyManager.PopulateList( ddl, ds, "Id", "Title", "Select a Resource Type" );
        }	// End 

        protected void addButton_Click( object sender, EventArgs e )
        {
            try
            {
                if ( ddlResourceType.SelectedIndex < 1 )
                {
                    SetConsoleErrorMessage( "Error: a Resource type must be selected" );
                    return;
                }
                if ( txtLR_ResourceType.Text.Trim().Length < 2 )
                {
                    SetConsoleErrorMessage( "Error: a Learning registry Resource type must be entered" );
                    return;
                }
                string lrrt = DatabaseManager.HandleApostrophes( this.txtLR_ResourceType.Text );
                string codeId = this.ddlResourceType.SelectedValue.ToString();


                string sql = string.Format( INSERT_SQL, lrrt, codeId );

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

        private void DoSearch()
        {
            string sql = SELECT_SQL;
            DataSet ds = DatabaseManager.DoQuery( sql );

            this.txtLR_ResourceType.Text = "";
            //this.txtResourceType.Text = "";
            ddlResourceType.SelectedIndex = 0;

            this.rfvCategory.Enabled = true;
            this.rfvLRValue.Enabled = true;

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
                    //todo: why is the following done??? If there is only one row, then get an exception - only row[0] exists
                    //formGrid.Rows[ 1 ].Enabled = true;

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
                System.Data.DataRowView drv = (DataRowView)e.Row.DataItem;
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
                                DataBinder.Eval( e.Row.DataItem, "Title" ) + ")')" );
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
            DropDownList ddl = ( DropDownList ) e.Row.FindControl( "gridDdlResourceType" );
            PopulateResourceTypes( ddl );

            string resType = DatabaseManager.GetRowColumn( drv, "ResourceType", "" );
            //if ( siteType.Length > 0 )
            if ( resType.Length > 0 )
            {
                this.SetListSelection( ddl, resType.ToString() );
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
            this.rfvCategory.Enabled = false;
            this.rfvLRValue.Enabled = false;

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
            this.rfvCategory.Enabled = false;
            this.rfvLRValue.Enabled = false;
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
                TextBox lrValue = formGrid.Rows[ e.RowIndex ].FindControl( "gridLRValue" ) as TextBox;
                TextBox txtCodeId = formGrid.Rows[ e.RowIndex ].FindControl( "CodeId" ) as TextBox;

                DropDownList ddl = ( DropDownList ) row.FindControl( "gridDdlResourceType" );
                if ( ddl != null && ddl.SelectedIndex > 0 )
                {
                    string lr = DatabaseManager.HandleApostrophes( lrValue.Text );
                    string codeId = ddl.SelectedValue;

                    string sql = string.Format( UPDATE_SQL, lr, codeId, id );


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
                }
                else
                {

                }

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".UpdateRecord" );

            }
        }

        #endregion
    }
}
