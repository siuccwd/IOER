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
using MyManager = Isle.BizServices.ContentServices;
using AcctManager = Isle.BizServices.AccountServices;
using GroupManager = Isle.BizServices.GroupServices;
using ILPLibrary = ILPathways.Library;
using ILPathways.Controllers;
using ILPathways.Utilities;
using GDAL = Isle.BizServices;
using LDAL = LRWarehouse.DAL;

namespace ILPathways.LRW.controls
{
    public partial class ContentSearch : ILPLibrary.BaseUserControl
    {
        const string thisClassName = "ContentSearch";
        string filterDesc = "";
        MyManager myManager = new MyManager();



        #region Properties
        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string FormSecurityName
        {
            get { return this.txtFormSecurityName.Text; }
            set { this.txtFormSecurityName.Text = value; }
        }

        protected string CustomFilter
        {
            get { return this.txtCustomFilter.Text; }
            set { this.txtCustomFilter.Text = value; }
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
        /// Get/Set if is my authored view
        /// </summary>
        public bool IsMyAuthoredView
        {
            get
            {
                return _isMyAuthoredView;
            }
            set { this._isMyAuthoredView = value; }
        }//
        private bool _isMyAuthoredView = false;

        private string _district = null;
        protected string District
        {
            get
            {
                try
                {
                    if ( _district == null )
                    {
                        if ( Page.RouteData.Values.Count > 0 )
                            _district = Page.RouteData.Values[ "DistrictName" ].ToString();
                        else
                            _district = "";
                    }
                }
                catch
                {
                    _district = "";
                }

                return _district;
            }
        }
        private string _authorSearch = null;
        protected string AuthorSearch
        {
            get
            {
                try
                {
                    if ( _authorSearch == null )
                    {
                        if ( Page.RouteData.Values.Count > 0 )
                            _authorSearch = Page.RouteData.Values[ "Author" ].ToString();
                        else
                            _authorSearch = "";
                    }
                }
                catch
                {
                    _authorSearch = "";
                }
                return _authorSearch;
            }
        }
        //protected bool IsOrgApprover
        //{
        //    get
        //    {
        //        if ( Session[ "IsOrgApprover" ] == null )
        //            Session[ "IsOrgApprover" ] = false;

        //        if ( IsInteger( Session[ "IsOrgApprover" ].ToString() ) )
        //            return bool.Parse( Session[ "IsOrgApprover" ].ToString() );
        //        else
        //            return 0;
        //    }
        //    set { Session[ "IsOrgApprover" ] = value; }
        //}//
        #endregion

        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                this.InitializeForm();
            }
           
        }

        public void InitializeForm()
        {
            //TODO - add code to determine if must by authenticated for this view
            if ( IsMyAuthoredView == true )
            {
                if ( this.IsUserAuthenticated() == false )
                {
                    //error
                    SetConsoleErrorMessage( "Invalid request, must be signed in to use this function." );
                    pnlSearch.Visible = false;
                    return;
                }
                HandleMyAuthored();
            }
            else
            {
                //public search, only published
                //no privileges
                listCreatedBy.SelectedIndex = -1;
                listCreatedBy.Enabled = false;
                FormPrivileges = new ApplicationRolePrivilege();
                FormPrivileges.SetReadOnly();
                FormPrivileges.ReadPrivilege = 1;
                FormPrivileges.CreatePrivilege = 0;
            }

            //set grid defaults (variables are in base control)
            //set sort to blank to default to results from database
            GridViewSortExpression = "";
            GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
            LastPageNumber = 0;
            LastTotalRows = 0;

            resultsPanel.Visible = false;

            // Set source for form lists
            this.PopulateControls();
            CheckRequest();
            //if ( CustomFilter.Length == 0 )
            //{
            //    //set default filters, if logged in 
                DoSearch();
            //}
        }	// End 


        protected void CheckRequest()
        {
            if ( AuthorSearch.Length > 0 )
            {
                CustomFilter = string.Format("auth.Fullname = '{0}' ", AuthorSearch);
            }
            else if ( District.Length > 0 )
            {
                CustomFilter = string.Format( "base.Organization = '{0}' ", District );
            }
        }	// End 


        protected void HandleMyAuthored()
        {
            filtersPanel.Visible = true;
            createResourceLinkPanel.Visible = true;

            CurrentUser = GetAppUser();
            if ( CurrentUser.ParentOrgId == -1 )
            {
                CurrentUser.ParentOrgId = 0;
                //get has not been attempted, do now
                string statusMessage = string.Empty;
                Organization org = AcctManager.GetOrganization( CurrentUser, ref statusMessage );
                if ( org != null && org.Id > 0 )
                {
                    CurrentUser.ParentOrgId = org.ParentId;
                }
                //update session
                this.WebUser = CurrentUser;
            }

            // get form privileges TODO are there any for basic search?, if not don't call
            FormPrivileges = GDAL.SecurityManager.GetGroupObjectPrivileges( CurrentUser, FormSecurityName );
            if ( CurrentUser.UserName == "mparsons" )
            {
                FormPrivileges.SetAdminPrivileges();
            }

            ApplicationRolePrivilege authPriv = GDAL.SecurityManager.GetGroupObjectPrivileges( CurrentUser, txtAuthorSecurityName.Text );
            if ( authPriv.CanCreate() )
            {
                authoringPanel.Visible = true;
                if ( authPriv.WritePrivilege > ( int ) ILPathways.Business.EPrivilegeDepth.Region )
                    FormPrivileges.SetAdminPrivileges();
            }


            if ( GroupManager.IsUserAnyOrgApprover( CurrentUser.Id ) )
            {
                approversPanel.Visible = true;
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
                    this.DoSearch();
                    break;
            }
        } // end 

        protected void approversOptionsList_SelectedIndexChanged( Object sender, EventArgs e )
        {
            searchSummaryDesc.Text = "";

            if ( approversOptionsList.SelectedIndex == 0 )
            {
                filtersPanel.Visible = true;
                dateFiltersPanel.Visible = true;
            }
            else
            {
                //approver view
                filtersPanel.Visible = false;
                dateFiltersPanel.Visible = false;

                DoSearch();
            }

        }
        protected void DoCustomSearch( string type, string parm )
        {
            lblCustomTitle.Text = CustomTitle = string.Format( "{0}: {1}", type, parm );
            CustomHdrPanel.Visible = true;
            //SearchType = 2;

            DoSearch();
        }	// End 

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

            //add check in case of time out
            if ( this.IsUserAuthenticated() == false )
            {
                if ( IsMyAuthoredView == true )
                {
                    //error
                    SetConsoleErrorMessage( "Invalid request, must be signed in to use the My Authored view." );
                    pnlSearch.Visible = false;
                    return;
                }
            }
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
                lblCustomTitle.Text = CustomTitle;
                CustomHdrPanel.Visible = true;
            }

            try
            {
                if ( this.approversPanel.Visible == true && approversOptionsList .SelectedIndex == 1)
                    filter = FormatApproverFilter();
                else
                    filter = this.FormatFilter();

                currentPageSize = formGrid.PageSize;

                ds = myManager.Search( filter, sortTerm, selectedPageNbr, pager1.PageSize, ref pTotalRows );

                pager1.ItemCount = pager2.ItemCount = pTotalRows;
                LastTotalRows = pTotalRows;

                searchSummaryDesc.Text = filterDesc;

                if ( LDAL.DatabaseManager.DoesDataSetHaveRows( ds ) == false )
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
                    DataView dv = ( ( DataTable ) dt ).DefaultView;
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
                    formGrid.DataSource = dv;
                    formGrid.DataBind();

                    if ( IsUserAuthenticated() == false || authoringPanel.Visible == false)
                        formGrid.Columns[ 0 ].Visible = false;
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
        /// call for an initial search
        /// </summary>
        /// <returns></returns>
        protected string FormatFilter()
        {

            string filter = "";
            string booleanOperator = "AND";
            filterDesc = "";

            //if only current author
            if ( IsUserAuthenticated() )
            {
                
                FormatOrgFilters( booleanOperator, ref filter );
            }
            else
            {
                //restrictions?
                //only public ==> may want to use IsActive, so as not to show work in progress
                filter = txtPublicFilter.Text;
            }
            //date filters
            FormatDatesFilter( booleanOperator, ref filter );

            if ( CustomFilter.Length > 0 )
            {
                //skip for now, checking below==> address with future custom filters
                filter = CustomFilter;
            }
            else
            {
                //if ( IsPersonalLibrary )
                //    filter = string.Format( "(LibraryTypeId = 1 and LibraryCreatedById = {0}) ", CurrentUser.Id );
            }

            //
            FormatKeyword( txtKeyword, booleanOperator, ref filter );

            if ( this.IsTestEnv() || GetRequestKeyValue( "debug", "" ) == "yes" )
            {
                this.SetConsoleSuccessMessage( "sql: " + filter );
                LoggingHelper.DoTrace( 6, "sql: " + filter );
            }
            return filter;
 }	//

        protected string FormatApproverFilter()
        {

            string filter = "";
            string booleanOperator = "AND";
            filterDesc = "";
            string where = "";
            string selDesc = "";

            CurrentUser = GetAppUser();
            if ( CurrentUser.OrgId > 0 )
            {
                where = string.Format( "(base.StatusId = 3 " +
                                            "AND ( base.OrgId = {0} OR base.ParentOrgId = {0} ) )  ", CurrentUser.OrgId );
                selDesc = "Approvals in my district";
            }
            else
            {
                SetConsoleErrorMessage( "You are not associated with an organization, approver is not possible" );
                where = string.Format( "(base.OrgId = {0} OR auth.OrganizationId = {0}) ", -1 );
            }
            if ( where.Trim().Length > 5 )
            {
                filter += LDAL.DatabaseManager.FormatSearchItem( filter, where, booleanOperator );
                filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
            }
            else if ( selDesc.Trim().Length > 5 )
            {
                filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
            }

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
            string keyword = LDAL.DatabaseManager.HandleApostrophes( FormHelper.SanitizeUserInput( textBox.Text.Trim() ) );
            string keywordFilter = "";

            if ( keyword.Length > 0 )
            {
                //filterDesc = filterDesc + "<div class='searchSection isleBox'>" + keyword + "</div>";
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
                        keywordFilter += LDAL.DatabaseManager.FormatSearchItem( keywordFilter, where, "OR" );
                    }
                }
                else
                {
                    if ( keyword.IndexOf( "%" ) == -1 )
                        keyword = "%" + keyword + "%";

                    keywordFilter = string.Format( keywordTemplate.Text, keyword );

                }

                if ( keywordFilter.Length > 0 )
                    filter += LDAL.DatabaseManager.FormatSearchItem( filter, keywordFilter, booleanOperator );
            }
        }	//

        private void FormatOrgFilters( string booleanOperator, ref string filter )
        {
            //filter = "(base.PrivilegeTypeId = 1 AND base.IsActive = 1) ";
            //need to allow approver to see submitted status
            //might be better to have a status filter, and hide when n/a
            string where = "";
            string selDesc = "";
            CurrentUser = GetAppUser();

            if ( listCreatedBy.SelectedIndex == 0 )
            {
                where = string.Format( "(base.CreatedById = {0}) ", CurrentUser.Id );
                selDesc = "Created By Me";
            }
            else if ( listCreatedBy.SelectedIndex == 1 )
            {
                //my org
                if ( CurrentUser.OrgId > 0 )
                {
                    where = string.Format( "(base.CreatedById = {0}) ", CurrentUser.Id );
                    where += string.Format( "OR ((base.OrgId = {0} OR auth.OrganizationId = {0}) AND base.StatusId = 5) ", CurrentUser.OrgId );
                    selDesc = "Created by my organization";
                }
                else
                {
                    SetConsoleInfoMessage( "You are not associated with an organization, so this search option will not return results" );
                    where = string.Format( "(base.OrgId = {0} OR auth.OrganizationId = {0}) ", -1 );
                }
            }
            else if ( listCreatedBy.SelectedIndex == 2)
            {
                //my district
                if ( CurrentUser.OrgId > 0 )
                {
                    where = string.Format( "(base.CreatedById = {0}) ", CurrentUser.Id );
                    where += string.Format( "OR (base.StatusId = 5 " +
                                                "AND ( (base.OrgId = {0} OR base.ParentOrgId = {0} OR auth.OrganizationId = {0})   ", CurrentUser.OrgId );
                    if ( CurrentUser.ParentOrgId > 0)
                        where += string.Format( "OR (base.OrgId = {0} OR base.ParentOrgId = {0} OR auth.OrganizationId = {0}) ) )", CurrentUser.ParentOrgId );
                    else
                        where += ") )";

                    selDesc = "Created By my district";
                }
                else
                {
                    SetConsoleErrorMessage( "You are not associated with an organization/district, so this search option will not return results" );
                    where = string.Format( "(base.OrgId = {0} OR auth.OrganizationId = {0}) ", -1 );
                }
            }
            else
            {
                //all 
                selDesc = "";
                if ( IsMyAuthoredView == true )
                {
                    if ( CurrentUser.OrgId > 0 )
                    {
                        where = string.Format( "(base.CreatedById = {0}) ", CurrentUser.Id );
                        where += string.Format( "OR (base.StatusId = 5 " +
                                                    "AND ( (base.OrgId = {0} OR base.ParentOrgId = {0} OR auth.OrganizationId = {0})   ", CurrentUser.OrgId );
                        if ( CurrentUser.ParentOrgId > 0 )
                            where += string.Format( "OR (base.OrgId = {0} OR base.ParentOrgId = {0} OR auth.OrganizationId = {0}) ) )", CurrentUser.ParentOrgId );
                        else
                            where += ") )";
                        //or all others that are public and published
                        where += " OR (base.StatusId = 5 AND base.PrivilegeTypeId = 1 )";
                    }
                    else
                    {
                        where = "(base.StatusId = 5 AND base.PrivilegeTypeId = 1 ) ";
                    }
                }
                else
                {
                    where = "(base.StatusId = 5 AND base.PrivilegeTypeId = 1 ) ";
                }
            }
            if ( where.Trim().Length > 0 )
            {
                filter += LDAL.DatabaseManager.FormatSearchItem( filter, where, booleanOperator );
                if ( selDesc.Trim().Length > 5 )
                    filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
            }
            else if ( selDesc.Trim().Length  > 5)
            {
                filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
            }
        }

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
                endDate = System.DateTime.Now.AddDays( -180 );
            }
            else
            {
                return;
            }

            string where = string.Format( " base.LastUpdated > '{0}'", endDate.ToString( "yyyy-MM-dd" ) );
            filter += LDAL.DatabaseManager.FormatSearchItem( filter, where, booleanOperator );
            string selDesc = string.Format( " Modified > {0}", endDate.ToString( "yyyy-MM-dd" ) );
            filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
        }

        /// <summary>
        /// no longer used
        /// </summary>
        /// <returns></returns>
        protected void formGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {

            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                try
                {
                    bool allowingEdit = false;
                    if ( FormPrivileges.WritePrivilege > ( int ) ILPathways.Business.EPrivilegeDepth.Region )
                        allowingEdit = true;

                    if ( IsMyAuthoredView == true)
                    {
                        //must be authenticated
                        Label createdById = ( Label ) e.Row.FindControl( "lblCreatedById" );
                        if ( (createdById != null && createdById.Text == CurrentUser.Id.ToString())
                            || allowingEdit )
                        {
                            //editLink
                            HyperLink actionsLink = ( HyperLink ) e.Row.FindControl( "editLink" );
                            if ( actionsLink != null )
                                actionsLink.Visible = true;
                        }
                    }
                    else
                    {
                        if ( IsUserAuthenticated() )
                        {
                            if ( allowingEdit )
                            {
                                HyperLink actionsLink = ( HyperLink ) e.Row.FindControl( "editLink" );
                                if ( actionsLink != null )
                                    actionsLink.Visible = true;
                            }
                            else if ( approversOptionsList.SelectedIndex == 1 )
                            {
                                //better to do this via sql rather than many lookups!

                                string orgId = ( ( DataRowView ) e.Row.DataItem )[ "OrgId" ].ToString();
                                Label createdById = ( Label ) e.Row.FindControl( "lblCreatedById" );
                            }
                        }
                    }
                }
                catch
                {

                }
            }
        }//

        public string CleanTitle( string text )
        {
            if (string.IsNullOrEmpty(text) )
                return "";
            else
                return new PublishController().FormatFriendlyTitle( text );
        }//

        public string CleanDescription( string description, int characters )
        {
            if ( description.Trim().Length < characters )
                return description;

            string sentence = "";
            string suffix = readMoreTemplate.Text;
            if ( characters > 0 )
            {
                sentence = description.Substring( 0, ( description.Length >= characters ? characters : description.Length ) );
                sentence = HandlePartialDesc( sentence );
            }
            else
            {
                sentence = description;
                suffix = "";
            }


            if ( sentence.StartsWith( "&nbsp;" ) )
                sentence = sentence.Substring( 5, sentence.Length - 6 );
            sentence = sentence.Replace( "&nbsp;", " " );
            sentence = sentence.Replace( "&lt;p&gt;", "" );
            sentence = sentence.Replace( "&lt;/p&gt;", "" );
            sentence = sentence.Replace( "&amp;nbsp;", " " );

            return sentence + suffix;
            // }
        }
        private string HandlePartialDesc( string sentence )
        {
            //need to handle special tags to ensure end tags are included
            sentence = sentence.Replace( "<br>", " " );
            sentence = sentence.Replace( "<br/>", " " );

            sentence = StripTag( sentence, "em" );
            sentence = StripTag( sentence, "span" );
            sentence = StripTag( sentence, "b" );
            sentence = StripTag( sentence, "p" );
            sentence = StripTag( sentence, "div" );
            sentence = StripTag( sentence, "font" );

            //TODO - handle embedded anchors - may have a method in utilityManager already - also XHTML page
            // - also consider doing all stripping of tags before getting the first 100 chars


            if ( sentence.ToLower().IndexOf( "<img" ) > -1 )
            {
                //remove img
                int pos1 = sentence.ToLower().IndexOf( "<img" );
                int pos2 = sentence.ToLower().IndexOf( ">", pos1 );
                if ( pos2 > pos1 )
                {
                    //extract image
                    sentence = sentence.Substring( 0, pos1 - 1 ) + " " + sentence.Substring( pos2 );
                }
                else
                {
                    //tag is incomplete, only include up to tag start 
                    sentence = sentence.Substring( 0, pos1 - 1 );
                }
            }

            // back up to last blank char
            int pos3 = sentence.LastIndexOf( " " );
            if ( pos3 > 50 )
                sentence = sentence.Substring( 0, pos3 );
            return sentence;
        }
        private string StripTag( string text, string tag )
        {
            string result = text.Replace( "<" + tag + ">", "" );
            result = result.Replace( "</" + tag + ">", "" );

            return result;
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
                GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
                GridViewSortExpression = newSortExpression;
                sortTerm = newSortExpression + " ASC";
            }
            if ( newSortExpression.ToLower() != "title" )
            {
                sortTerm += ", Title ASC";
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
            DataSet ds1 = LDAL.DatabaseManager.GetCodeValues( "GridPageSize", "SortOrder" );
            LDAL.DatabaseManager.PopulateList( this.ddlPageSizeList, ds1, "StringValue", "StringValue", "Select Size" );

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

        } //

        #endregion

    }
}