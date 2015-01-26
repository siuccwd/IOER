using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using ILPathways.Utilities;
using EmailHelper = ILPathways.Utilities.EmailManager;
using MyManager = ILPathways.DAL.SqlQueryManager;
//using ILPathways.DAL;
using Isle.BizServices;
using ILPathways.Business;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using LDBM = LRWarehouse.DAL.DatabaseManager;

namespace ILPathways.Admin
{
    public partial class QueryMgmt : BaseAppPage
    {

        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
        const string thisClassName = "QueryMgmt";
        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        const string formSecurityName = "ILPathways.Admin.QueryMgmt";

        #region Properties
        /// <summary>
        /// INTERNAL PROPERTY: CurrentRecord
        /// Set initially and store in ViewState
        /// </summary>
        protected SqlQuery CurrentRecord
        {
            get { return ViewState[ "CurrentRecord" ] as SqlQuery; }
            set { ViewState[ "CurrentRecord" ] = value; }
        }

        /// <summary>
        /// Get/Set query category
        /// - if non-blank, then only queries in that category will be available
        /// </summary>
        public string DefaultCategory
        {
            get { return defaultCategory.Text; }
            set { defaultCategory.Text = value; }
        }
        #endregion

        /// <summary>
        /// Handle Page Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void Page_Load( object sender, EventArgs ex )
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

            //set grid defaults (variables are in base control)
            GridViewSortExpression = "";
            GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Descending;
            //
            this.FormPrivileges = SecurityManager.GetGroupObjectPrivileges( this.WebUser, formSecurityName );

            //= Add attribute to btnDelete to allow client side confirm
            btnDelete.Attributes.Add( "onClick", "return confirmDelete(this);" );

            //handling setting of which action buttons are available to the current user
            this.btnNew.Enabled = false;
            this.btnSave.Enabled = false;
            this.btnDelete.Visible = false;

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

            //
            //check for defaultCategory
            string defaultCategory = "";    // McmsHelper.GetPlaceHolderParameter( "pageControl", "category" );
            //check category

            if ( defaultCategory.Length > 3 )
            {
                DefaultCategory = defaultCategory;
                SetFormSelectList( DefaultCategory );
                ddlCategoryFilter.Visible = false;
                ddlCategory.Visible = false;
                isPublicPanel.Visible = false;

                lblDefaultCategory.Text = DefaultCategory;
                lblDefaultCategory.Visible = true;
                lblNewCategory.Visible = false;
                //txtNewCategory.Text = DefaultCategory;
                txtNewCategory.Visible = false;

                searchPanel.Visible = false;
            }

            // Set source for form lists
            this.PopulateControls();
            //this.FormatHelpItem( lblKeyword, lblKeyword_Help, true );

            if ( DefaultCategory.Length > 3 )
            {
                this.SetListSelection( this.ddlCategoryFilter, DefaultCategory );
            }
            if ( btnNew.Enabled )
                HandleNewRequest();

            //do initial search
            DoSearch();
        }	// End 


        #region form actions
        /// <summary>
        /// Get - retrieve form data
        /// </summary>
        /// <param name="recId"></param>
        private void Get( int recId )
        {
            try
            {
                string pQueryCode = "";
                //get record
                SqlQuery entity = MyManager.Get( recId, pQueryCode );

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
        private void PopulateForm( SqlQuery entity )
        {
            CurrentRecord = entity;
            TabContainer1.ActiveTabIndex = 1;
            this.txtId.Text = entity.Id.ToString();
            this.txtTitle.Text = entity.Title;
            this.txtDescription.Text = entity.Description;
            this.txtQueryCode.Text = entity.QueryCode;
            if ( entity.Category.Length > 0 )
                this.SetListSelection( this.ddlCategory, entity.Category );
            else
                this.ddlCategory.SelectedIndex = 0;

            this.txtNewCategory.Text = "";
            this.txtSQL.Text = entity.SQL;

            this.ddlOwnerId.Text = entity.OwnerId.ToString();

            this.rblIsPublic.SelectedValue = this.ConvertBoolToYesNo( entity.IsPublic );

            if ( entity.Id > 0 )
            {
                this.SetFieldLockedStatus( txtQueryCode, true );
                this.lblHistory.Text = entity.HistoryTitle();
                PrepareQueryLink( entity );

                if ( FormPrivileges.CanUpdate() )
                {
                    btnSave.Enabled = true;
                }

                if ( FormPrivileges.CanDelete() )
                {
                    btnDelete.Enabled = true;
                }
                btnCopy.Enabled = true;

                if ( entity.IsPublic == false && WebUser.TopAuthorization > 2 )
                {
                    //could be only creator or special query that only admin can change
                    btnSave.Enabled = false;
                    btnDelete.Enabled = false;
                }
            }
            else
            {
                this.SetFieldLockedStatus( txtQueryCode, false );
                //reset controls or state for empty object
                this.lblHistory.Text = "";
                sqlPreviewLink.Visible = false;
                btnDelete.Enabled = false;
                btnCopy.Enabled = false;
            }

            btnDelete.Visible = btnDelete.Enabled;
        }//

        private void PrepareQueryLink( SqlQuery query )
        {
            string url = UtilityManager.GetAppKeyValue( "path.RunQuery", "/Admin/Queries/Query.aspx" );
            //Session[ SessionManager.SESSION_WORKNETQUERY ] = query;
            url += "?id=" + query.Id.ToString();

            sqlPreviewLink.NavigateUrl = url;
            sqlPreviewLink.Visible = true;

        } // end 


        /// <summary>
        /// Handle a form button clicked 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        public void FormButton_Click( Object sender, CommandEventArgs ex )
        {
            TabContainer1.ActiveTabIndex = 1;
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
                case "Copy":
                    this.CopyRecord();
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
                //other edits

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
            rfvTitle.Validate();
            if ( rfvTitle.IsValid == false )
            {
                //NOTE: this may not be necessary anylonger if validator messages are properly picked up by all uplevel browsers!!!!
                this.AddReqValidatorError( vsErrorSummary, rfvTitle.ErrorMessage, "txtTitle" );
            }

            //either a category needs to be selected or the text field must have a value
            if ( DefaultCategory.Length == 0 )
            {
                if ( ddlCategory.SelectedIndex > 0 )
                {
                    txtNewCategory.Text = "";
                }
                else
                {
                    if ( txtNewCategory.Text.Trim().Length == 0 )
                    {
                        this.AddReqValidatorError( this.vsErrorSummary, "You must either select a category or enter a new one", "txtNewCategory", true );
                    }
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

            SqlQuery entity = new SqlQuery();

            try
            {
                id = int.Parse( this.txtId.Text );

                if ( id == 0 )
                {
                    entity.Id = 0;
                    entity.CreatedBy = WebUser.Email;
                    entity.Created = System.DateTime.Now;
                    action = "Create";

                }
                else
                {
                    // get current record from viewstate
                    entity = CurrentRecord;
                    action = "Update";
                }
                /*
                 * //assign form fields
                 */
                entity.Title = this.txtTitle.Text;
                entity.Description = this.txtDescription.Text;

                if ( txtQueryCode.Text.ToLower() == "default" )
                    entity.QueryCode = System.Guid.NewGuid().ToString();
                else
                    entity.QueryCode = this.txtQueryCode.Text;

                if ( DefaultCategory.Length > 0 )
                {
                    entity.Category = DefaultCategory;
                }
                else
                {
                    if ( ddlCategory.SelectedIndex > 0 )
                        entity.Category = this.ddlCategory.SelectedValue;
                    else
                        entity.Category = this.txtNewCategory.Text;
                }
                entity.SQL = this.txtSQL.Text;
                //entity.OwnerId = Int32.Parse( ddlOwnerId.Text );
                entity.IsPublic = this.ConvertYesNoToBool( rblIsPublic.SelectedValue );

                entity.LastUpdated = System.DateTime.Now;
                entity.LastUpdatedBy = WebUser.Email;

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
                        if ( statusMessage.Length == 0 )
                            statusMessage = "successful";
                    }

                }
                else
                {
                    statusMessage = MyManager.Update( entity );
                }


                if ( statusMessage.Equals( "successful" ) )
                {
                    if ( txtNewCategory.Text.Length > 0 )
                    {
                        SetCategoryList();
                    }

                    //update CurrentRecord and form
                    PopulateForm( entity );

                    this.SetConsoleSuccessMessage( action + " was successful for: " + entity.Title );

                    this.SetFormSelectList();
                    if ( DefaultCategory.Length > 0 )
                        DoSearch();

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
                    this.SetFormSelectList();
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

        private void CopyRecord()
        {
            // base on CurrentRecord
            SqlQuery entity = CurrentRecord;
            entity.Id = 0;
            entity.Title = "";
            entity.QueryCode = "Default";

            PopulateForm( entity );

        }//
        /// <summary>
        /// Reset form values
        /// </summary>
        private void ResetForm()
        {
            //reset form by populating with an empty business object
            SqlQuery entity = new SqlQuery();
            //do any defaults. 
            entity.IsActive = true;
            entity.IsPublic = true;
            entity.QueryCode = "default";
            PopulateForm( entity );

            //unselect forms list
            lstForm.SelectedIndex = -1;
        }//
        #endregion


        /// <summary>
        /// Populate form controls
        /// </summary>
        /// <param name="recId"></param>
        private void PopulateControls()
        {
            SetFormSelectList( "" );

            SetCategoryList();
            InitializePageSizeList();
        } //

        /// <summary>
        /// populate the main form list
        /// </summary>
        private void SetFormSelectList()
        {
            string category = "";

            if ( ddlCategoryFilter.SelectedIndex > 0 )
                category = ddlCategoryFilter.SelectedValue;

            SetFormSelectList( category );

        } //
        /// <summary>
        /// Set contents of form list by category
        /// </summary>
        private void SetFormSelectList( string category )
        {
            if ( formListPanel.Visible == true )
            {
                string pQueryCode = "";
                int pIsPublic = 2;
                int pOwnerId = 0;

                DataSet ds = MyManager.Select( pQueryCode, category, pIsPublic, pOwnerId );
                MyManager.PopulateList( lstForm, ds, "Id", "Title", "Select a query record" );
            }

        } //

        private void SetCategoryList()
        {
            int isPublicFilter = 2;

            //category is just a select distinct at this time
            //Note the select method adds the Select row!
            DataSet ds = MyManager.SelectCategories( isPublicFilter );

            ddlCategory.DataSource = ds;
            ddlCategory.DataValueField = "Category";
            this.ddlCategory.DataTextField = "Category";
            ddlCategory.DataBind();

            ddlCategoryFilter.DataSource = ds;
            ddlCategoryFilter.DataValueField = "Category";
            this.ddlCategoryFilter.DataTextField = "Category";
            ddlCategoryFilter.DataBind();

        } //


        private void SetCategoryList( int isPublicFilter )
        {

            //category is just a select distinct at this time
            //Note the select method adds the Select row!
            DataSet ds = MyManager.SelectCategories( isPublicFilter );

            ddlCategoryFilter.DataSource = ds;
            ddlCategoryFilter.DataValueField = "Category";
            this.ddlCategoryFilter.DataTextField = "Category";
            ddlCategoryFilter.DataBind();

        } //
        #region form grid related methods
        protected void rbIsPublicFilter_SelectedIndexChanged( object sender, System.EventArgs e )
        {

            try
            {
                int isPublicFilter = Int32.Parse( this.rbIsPublicFilter.SelectedValue );
                //reset categories
                SetCategoryList( isPublicFilter );
                if ( formListPanel.Visible == true )
                {
                    SetFormSelectList();
                }
                DoSearch();

            }
            catch ( Exception ex )
            {
                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.Message );
                this.LogError( ex, thisClassName + ".IsActiveFilter_SelectedIndexChanged" );
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
                //let SetFormSelectList handle this event
                if ( formListPanel.Visible == true )
                {
                    SetFormSelectList();
                }
                DoSearch();
                TabContainer1.ActiveTabIndex = 0;
            }
            catch ( Exception ex )
            {
                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.Message );
                this.LogError( ex, thisClassName + ".ddlCategoryFilter_SelectedIndexChanged" );
            }
        } //
        protected void SearchButton_Click( object sender, System.EventArgs e )
        {
            DoSearch();

        }
        /// <summary>
        /// Handle selection from form list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void lstForm_SelectedIndexChanged( object sender, System.EventArgs ea )
        {

            try
            {
                int id = int.Parse( this.lstForm.SelectedValue );

                this.Get( id );

            }
            catch ( Exception ex )
            {
                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.Message );
                this.LogError( ex, thisClassName + ".lstForm_SelectedIndexChanged" );
            }
        } //
        /// <summary>
        /// Conduct a search and populate the form grid
        /// </summary>
        private void DoSearch()
        {
            int selectedPageNbr = 0;
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
            TabContainer1.ActiveTabIndex = 0;
            // Set the page size for the DataGrid control based on the selection
            CheckForPageSizeChange();

            int pTotalRows = 0;
           // string lang = "";
            string category = "";
            int isPublicFilter = 2;
            //check for active filter
            if ( rbIsPublicFilter.SelectedIndex > -1 )
            {
                isPublicFilter = Int32.Parse( this.rbIsPublicFilter.SelectedValue );
            }

            if ( DefaultCategory.Length > 0 )
            {
                category = DefaultCategory;
            }
            else
            {
                if ( ddlCategoryFilter.SelectedIndex > 0 )
                    category = ddlCategoryFilter.SelectedValue;
            }
            //keyword
            string keywordFilter = "";
            if ( txtKeyword.Text.Trim().Length > 0 )
            {
                //updated to check code or title for filter
                keywordFilter = MyManager.HandleApostrophes( FormHelper.CleanText( txtKeyword.Text ) );
                keywordFilter = keywordFilter.Replace( "*", "%" );
                if ( keywordFilter.IndexOf( "%" ) == -1 )
                {
                    keywordFilter = "%" + keywordFilter + "%";
                }
            }
            try
            {
                ds = MyManager.Select( "", category, isPublicFilter, 0, keywordFilter );

                if ( MyManager.DoesDataSetHaveRows( ds ) == false )
                {
                    resultsPanel.Visible = false;
                    SetConsoleInfoMessage( "No records were found for the provided search criteria" );
                    ddlPageSizeList.Enabled = false;

                }
                else
                {
                    pTotalRows = ds.Tables[ 0 ].Rows.Count;

                    resultsPanel.Visible = true;
                    ddlPageSizeList.Enabled = true;

                    //searchPanel.Visible = false;

                    DataTable dt = ds.Tables[ 0 ];
                    DataView dv = ( ( DataTable ) dt ).DefaultView;
                    if ( sortTerm.Length > 0 )
                        dv.Sort = sortTerm;

                    if ( pTotalRows > formGrid.PageSize )
                    {
                        formGrid.PagerSettings.Visible = true;
                    }


                    //populate the grid
                    formGrid.DataSource = dv;
                    formGrid.PageIndex = selectedPageNbr;

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
                    this.Get( recordId );
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
        } //
        #endregion
    }

}