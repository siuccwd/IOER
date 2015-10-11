using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Resources;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;
using ILPathways.Utilities;
using EmailHelper = ILPathways.Utilities.EmailManager;
using MyManager = ILPathways.DAL.SqlQueryManager;
using MyDataManager = LRWarehouse.DAL.DatabaseManager;
using IOER.classes;
//using ILPathways.DAL;
using Isle.BizServices;
using ILPathways.Business;
using LRWarehouse.Business;
using LRWarehouse.DAL;

namespace IOER.Admin
{
    public partial class Query : BaseAppPage
    {

        public string queryTitleText;
        public string queryDescText;

        const string formName = "ILPathways.Admin.Query";

        /// <summary>
        /// INTERNAL PROPERTY: CurrentGroup
        /// Set initially and store in ViewState
        /// </summary>
        protected SqlQuery CurrentRecord
        {
            get { return ViewState[ "CurrentRecord" ] as SqlQuery; }
            set { ViewState[ "CurrentRecord" ] = value; }
        }

        /// <summary>
        /// Handle page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load( object sender, System.EventArgs e )
        {

            if ( IsUserAuthenticated() == false )
            {
                //hide all
                //should be handled at a  higher level
                SetConsoleErrorMessage( "Error - must be authenticated" );

            }
            else
            {
                // Put user code to initialize the page here
                rowCount.Text = "0";
                if ( !Page.IsPostBack )
                {
                    this.InitializeForm();
                    CheckRecordRequest();

                }

            }
        } //

        /// <summary>
        /// Check for another object (because page may be re-used
        /// </summary>
        protected void Page_PreRender( object sender, System.EventArgs e )
        {
            //check for passed sql or object
            SqlQuery query = new SqlQuery();
            //string sql = "";

            query = ( SqlQuery ) Session[ SessionManager.SESSION_WORKNETQUERY ];
            if ( query != null )
            {
                //now clear out last sql
                Session.Remove( SessionManager.SESSION_WORKNETQUERY );
                //if authenticated and authorized display sql and execute it??
                this.queryPanel.Visible = true;
                PopulateForm( query, true );
 
            }
        } //

        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        private void InitializeForm()
        {
            //formSecurityName
			this.FormPrivileges = SecurityManager.GetGroupObjectPrivileges(WebUser, txtFormSecurityName.Text);

            if ( FormPrivileges.HasStateUpdate() )
            {
                this.queryPanel.Visible = true;
                this.queryPanel.Enabled = true;
                workNetAdminReturnLink.Visible = true;
                newQueryLink.Visible = true;
            }
            else
            {
                //read only view
                if ( showingTitleToAll.Text.Equals( "yes" ) )
                {
                    this.queryPanel.Visible = true;
                    searchPanel.Visible = false;
                    newQueryLink.Visible = false;
                    DisplayButton.Visible = false;
                    txtQueryTitle.Visible = true;
                    lblDescription.Visible = true;
                    txtQuery.Visible = false;
                }
                else
                {
                    searchPanel.Visible = queryPanel.Visible;
                    newQueryLink.Visible = queryPanel.Visible;
                    DisplayButton.Visible = queryPanel.Visible;
                    txtQuery.Visible = queryPanel.Visible;
                }
            }

            // Set source for form lists
            this.PopulateControls();
            //

            //check for passed sql or object
            SqlQuery query = new SqlQuery();
            string sql = "";

            query = ( SqlQuery ) Session[ SessionManager.SESSION_WORKNETQUERY ];
            if ( query != null )
            {
                //now clear out last sql
                Session.Remove( SessionManager.SESSION_WORKNETQUERY );
                //if authenticated and authorized display sql and execute it??
                this.queryPanel.Visible = true;
                PopulateForm( query, true );

            }
            else
            {
                sql = SessionManager.Get( Session, SessionManager.SESSION_WORKNETQUERY_SQL );
                if ( sql.Length > 0 )
                {

                    //now clear out last sql
                    Session.Remove( SessionManager.SESSION_WORKNETQUERY_SQL );
                    query.SQL = sql;

                    //if authenticated and authorized display sql and execute it??
                    this.queryPanel.Visible = true;
                    PopulateForm( query, true );

                }
            }

        }	// End 

        /// <summary>
        /// Check the request type for the form
        /// </summary>
        private void CheckRecordRequest()
        {

            // Check if a request was made for a specific record
            int id = this.GetRequestKeyValue( "id", 0 ); ;
            string code = this.GetRequestKeyValue( "cd", "" );
            string execute = this.GetRequestKeyValue( "x", "no" );
            bool executeSql = execute == "yes" ? true : false;

            if ( id > 0 )
            {
                this.PopulateForm( id, executeSql );

            }
            else if ( code.Length > 0 )
            {
                this.PopulateForm( code, executeSql );
            }


        }	// End 


        protected void IsPublicFilter_SelectedIndexChanged( object sender, System.EventArgs e )
        {

            try
            {
                int isPublicFilter = Int32.Parse( this.rbIsPublicFilter.SelectedValue );
                //reset categories
                SetCategoryList( isPublicFilter );

                SetFormSelectList();

            }
            catch ( Exception ex )
            {
                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.Message );
                this.LogError( ex, formName + ".IsPublicFilter_SelectedIndexChanged" );
            }
        } //

        /// <summary>
        /// Handle selection from category list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlCategoryFilter_SelectedIndexChanged( object sender, System.EventArgs e )
        {

            try
            {
                SetFormSelectList();

            }
            catch ( Exception ex )
            {
                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.Message );
                this.LogError( ex, formName + ".ddlCategoryFilter_SelectedIndexChanged" );
            }
        } //

        /// <summary>
        /// Handle selection from form list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lstForm_SelectedIndexChanged( object sender, System.EventArgs e )
        {

            try
            {
                int id = int.Parse( this.lstForm.SelectedValue );

                if ( id > 0 )
                    PopulateForm( id, false );

            }
            catch ( Exception ex )
            {
                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.Message );
                this.LogError( ex, formName + ".lstForm_SelectedIndexChanged" );
            }
        } //

        /// <summary>
        /// do a get by ID and populate
        /// </summary>
        /// <param name="id"></param>
        protected void PopulateForm( int id, bool executeSql )
        {
            //bool executeSql = false;
            try
            {

                //get query
                CurrentRecord = MyManager.Get( id );

                if ( CurrentRecord == null )
                {
                    this.SetConsoleErrorMessage( "Error request record was not found" );

                    return;

                }
                else
                {

                    PopulateForm( CurrentRecord, executeSql );

                }

            }
            catch ( System.Exception e )
            {
                //Action??		- display message and close form??	
                LoggingHelper.LogError( e, formName + ".PopulateForm( int id )() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + e.ToString() );

            }
        } //

        /// <summary>
        /// do a get by query code and populate
        /// </summary>
        /// <param name="queryCode"></param>
        protected void PopulateForm( string queryCode, bool executeSql )
        {
            //bool executeSql = false;
            try
            {

                //get query
                CurrentRecord = MyManager.Get( queryCode );

                if ( CurrentRecord == null )
                {
                    this.SetConsoleErrorMessage( "Error request record was not found" );

                    return;

                }
                else
                {

                    PopulateForm( CurrentRecord, executeSql );

                }

            }
            catch ( System.Exception e )
            {
                //Action??		- display message and close form??	
                LoggingHelper.LogError( e, formName + ".PopulateForm( string queryCode )() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + e.ToString() );

            }
        } //

        protected void PopulateForm( SqlQuery query, bool executeSql )
        {
            string sql = query.SQL;
            //check for common substitutions
            sql = sql.Replace( "@CurrentUserId", this.WebUser.Id.ToString() );
            sql = sql.Replace( "@CurrentUser", this.WebUser.Id.ToString() );

            int grpId = this.GetRequestKeyValue( "grpId", 0 );

            if ( grpId > 0 )
            {
                sql = sql.Replace( "@CurrentGroupId", grpId.ToString() );
                sql = sql.Replace( "@GroupId", grpId.ToString() );
            }
            this.txtQuery.Text = sql;
            this.txtId.Text = query.Id.ToString();
            queryTitleText = query.Title;

            this.txtQueryTitle.Text = query.Title;

            queryDescText = query.Description;
            lblDescription.Text = query.Description;

            if ( executeSql )
            {
                ExecuteSql();
            }

        }	// End 


        protected void DisplayButton_Click( object sender, System.EventArgs e )
        {


            ExecuteSql();

        }

        protected void ExecuteSql()
        {
            grid.CurrentPageIndex = 0;

            if ( txtQuery.Text.Length > 0 )
            {
                ExportButton.Visible = true;
                string sql = txtQuery.Text;
                //sql = sql.Replace( "@CurrentUserId", WebUser.Id.ToString() );

                DataSet ds = MyDataManager.DoQuery( sql );

                DisplayResults( ds );
            }
        }

        protected void ExportButton_Click( object sender, System.EventArgs e )
        {

            if ( txtQuery.Text.Length > 0 )
            {
                DataSet ds = MyDataManager.DoQuery( txtQuery.Text );
                this.resultsPanel.Visible = true;

                if ( ds == null )
                {
                    rowCount.Text = "No rows returned";
                }
                else
                {
                    DataTable dt = ds.Tables[ 0 ];
                    MyDataManager.ExportCSV( dt, Response, true );
                }
            }
        }
        ///<exclude/>
        private void DisplayResults( DataSet ds )
        {

            this.resultsPanel.Visible = true;
            if ( MyDataManager.DoesDataSetHaveRows(ds) == false )
            {
                rowCount.Text = "No rows returned";
            }
            else
            {

                if ( ds.Tables[ 0 ].Rows.Count > grid.PageSize )
                {
                    grid.AllowPaging = true;

                    grid.PagerStyle.Visible = true;
                    rowCount.Text = ds.Tables[ 0 ].Rows.Count.ToString() + " records found - showing " + grid.PageSize + " per page";
                }
                else
                {
                    rowCount.Text = ds.Tables[ 0 ].Rows.Count.ToString() + " records found.";
                    grid.PagerStyle.Visible = false;
                }
                ExportButton.Visible = true;

            }

            grid.DataSource = ds;
            grid.DataBind();

        } //

        /// <summary>
        /// Add pagination capabilities
        /// </summary>
        public void doPagination( object sender, DataGridPageChangedEventArgs e )
        {

            grid.CurrentPageIndex = e.NewPageIndex;

            DataSet ds = MyDataManager.DoQuery( txtQuery.Text );

            DisplayResults( ds );
        }
        protected void PageSizeList_SelectedIndexChanged( object sender, System.EventArgs e )
        {
            // Set the page size for the DataGrid control based on the
            // user's selection.
            grid.PageSize = Convert.ToInt32( pageSizeList.SelectedItem.Text );

            //Update user preference
            //this.currentUser.ItemsInList = Convert.ToInt32(pageSizeList.SelectedItem.Text);

            // Rebind the data to refresh the DataGrid control. 
            grid.CurrentPageIndex = 0;
            if ( txtQuery.Text.Length > 10 )
                DisplayButton_Click( sender, e );

        }

        /// <summary>
        /// formats a DataView in CSV format and then streams to the browser
        /// </summary>
        /// <param name="dv"></param>
        /// <param name="tempFilename"></param>
        protected void ExportDataTableAsCsv( DataView dv, string tempFilename )
        {
            DataTable dt = dv.Table;

            ExportDataTableAsCsv( dt, tempFilename );

        }
        /// <summary>
        /// ExportDataTableAsCsv - formats a DataTable in CSV format and then streams to the browser
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="tempFilename">Name of temporary file</param>
        protected void ExportDataTableAsCsv( DataTable dt, string tempFilename )
        {
            string datePrefix = System.DateTime.Today.ToString( "u" ).Substring( 0, 10 );
            string logFile = UtilityManager.GetAppKeyValue( "path.ReportsOutputPath", "C:\\VOS_LOGS.txt" );

            //string outputFile = logFile.Replace( "[date]", datePrefix );
            //outputFile = outputFile.Replace( "App_TraceLog.txt", tempFilename );
            string outputFile = logFile + datePrefix + "_" + tempFilename;
            //
            string filename = "export.csv";

            string csvFilename = this.DataTableAsCsv( dt, outputFile, false );

            Response.ContentType = "application/octet-stream";
            Response.AddHeader( "Content-Disposition", "attachment; filename=" + filename + "" );

            //Response.WriteFile(Server.MapPath(csvFilename));
            Response.WriteFile( csvFilename );
            Response.End();
            // Delete the newly created file.
            //TODO: - this line is not actually executed - need scheduled clean ups?
            //File.Delete(Server.MapPath(csvFilename));
        }

        /// <summary>
        /// DataTableAsCsv - formats a DataTable in csv format
        /// 								 The code first loops through the columns of the data table to export the names of all the data columns. 
        /// 								 Then in next loop the code iterates over each data row to export all the values in the table. 
        ///									 This method creates a temporary file on the server. This temporary file will need to
        ///									 be manually deleted at a later time.
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="tempFilename">Name of temporary file</param>
        /// <param name="doingMapPath">If true use Server.MapPath(</param>/// 
        /// <returns>Name of temp file created on the server</returns>
        protected string DataTableAsCsv( DataTable dt, string tempFilename, bool doingMapPath )
        {

            string strColumn = "";
            string strCorrected = "";
            StreamWriter sw;
            string serverFilename = ""; ;

            if ( doingMapPath )
            {
                serverFilename = "~/" + tempFilename;
                // Create the CSV file to which grid data will be exported.
                sw = new StreamWriter( Server.MapPath( serverFilename ), false );
            }
            else
            {
                serverFilename = tempFilename;
                sw = new StreamWriter( serverFilename, false );
            }

            // First we will write the headers.
            int intCount = dt.Columns.Count;
            for ( int i = 0 ; i < intCount ; i++ )
            {
                sw.Write( dt.Columns[ i ].ToString() );
                if ( i < intCount - 1 )
                {
                    sw.Write( "," );
                }
            }
            sw.Write( sw.NewLine );
            // Now write all the rows.
            foreach ( DataRow dr in dt.Rows )
            {
                for ( int i = 0 ; i < intCount ; i++ )
                {
                    if ( !Convert.IsDBNull( dr[ i ] ) )
                    {
                        strColumn = dr[ i ].ToString();

                        strCorrected = strColumn.Replace( "\"", "\'" );

                        sw.Write( "\"" + strCorrected + "\"" );
                    }
                    else
                    {
                        sw.Write( "" );
                    }
                    if ( i < intCount - 1 )
                    {
                        sw.Write( "," );
                    }
                }
                sw.Write( sw.NewLine );
            }
            sw.Close();


            return serverFilename;

        } //
        /// <summary>
        /// Populate form controls
        /// </summary>
        /// <param name="recId"></param>
        private void PopulateControls()
        {

            SetFormSelectList( "" );

            SetCategoryList();

        } //

        /// <summary>
        /// Set contents of form list
        /// </summary>
        private void SetFormSelectList()
        {
            string category = "";
            int isPublicFilter = 2;
            //check for active filter
            if ( rbIsPublicFilter.SelectedIndex > -1 )
            {
                isPublicFilter = Int32.Parse( this.rbIsPublicFilter.SelectedValue );
            }

            if ( ddlCategoryFilter.SelectedIndex > 0 )
                category = ddlCategoryFilter.SelectedValue;

            SetFormSelectList( isPublicFilter, category );
        } //

        /// <summary>
        /// Set contents of form list by category
        /// </summary>
        private void SetFormSelectList( string category )
        {
            int isPublicFilter = 2;
            SetFormSelectList( isPublicFilter, category );

        } //

        /// <summary>
        /// Set contents of form list by category
        /// </summary>
        private void SetFormSelectList( int isPublicFilter, string category )
        {
            int pOwnerid = 0;
            DataSet ds = MyManager.Select( "", category, isPublicFilter, pOwnerid );
			if ( BaseDataManager.DoesDataSetHaveRows( ds ) )
			{
				MyManager.AddEntryToTable( ds.Tables[ 0 ], 0, "Select a query", "id", "Title" );

				lstForm.DataSource = ds;
				lstForm.DataValueField = "id";
				this.lstForm.DataTextField = "Title";
				lstForm.DataBind();
			}

        } //

        private void SetCategoryList()
        {
            int isPublicFilter = 2;
            //check for active filter
            if ( rbIsPublicFilter.SelectedIndex > -1 )
            {
                //int isPublicSelect = this.ConvertYesNoToBool( this.rbIsActiveFilter.SelectedValue );
                isPublicFilter = Int32.Parse( this.rbIsPublicFilter.SelectedValue );
            }
            SetCategoryList( isPublicFilter );
        } //
        private void SetCategoryList( int isPublicFilter )
        {
            //category is just a select distinct at this time
            //Note the select method adds the Select row!
            DataSet ds = MyManager.SelectCategories( isPublicFilter );

            //ddlCategory.DataSource = ds;
            //ddlCategory.DataValueField = "Category";
            //this.ddlCategory.DataTextField = "Category";
            //ddlCategory.DataBind();

            ddlCategoryFilter.DataSource = ds;
            ddlCategoryFilter.DataValueField = "Category";
            this.ddlCategoryFilter.DataTextField = "Category";
            ddlCategoryFilter.DataBind();

        } //
    }



}