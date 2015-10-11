using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

//using vos_portal.classes;
//using vos_portal.Library;
//using utilityClasses;
//using workNet.BusObj.Entity;
//??

using MyManager = ILPathways.DAL.AppItemManager;
//using MyAppEmailManager = workNet.DAL.AnnouncementEmailManager;
using IOER.classes;
using IOER.Controllers;
using IOER.Library;
using ILPathways.Utilities;
using ILPathways.Business;
using LRWarehouse.DAL;

namespace IOER.Controls.AppItems
{
    public partial class NewsItemSearch : IOER.Library.BaseUserControl
    {
        const string thisClassName = "NewsItemSearch";
        MyManager myManager = new MyManager();
       // private MyAppEmailManager myEmailTemplateMgr = new MyAppEmailManager();
        public string txtTitle;
        public string txtDescr1;
        public string published, updated;

        #region Properties
        public string NewsItemTemplateCode
        {
            get
            {
                return txtNewsItemCode.Text;
            }
            set { txtNewsItemCode.Text = value; }
        }
        public string UsingAppItemWebService
        {
            get
            {
                return usingWebService.Text;
            }
            set { usingWebService.Text = value; }
        }
        /// <summary>
        /// CurrentNewsTemplate
        /// </summary>
        public NewsEmailTemplate CurrentNewsTemplate
        {
            get
            {
                try
                {
                    if ( ViewState[ "CurrentNewsTemplate" ] == null )
                        ViewState[ "CurrentNewsTemplate" ] = new NewsEmailTemplate();

                    return ( NewsEmailTemplate ) ViewState[ "CurrentNewsTemplate" ];
                }
                catch ( NullReferenceException nex )
                {
                    return null;
                }
            }
            set { ViewState[ "CurrentNewsTemplate" ] = value; }
        }
        /// <summary>
        /// Gets/sets Category
        /// </summary>
        private string Category
        {
            get
            {
                try
                {
                    return ViewState[ "SearchCategory" ].ToString();
                }
                catch ( NullReferenceException nex )
                {
                    return "";
                }
            }
            set { ViewState[ "SearchCategory" ] = value; }
        }

        /// <summary>
        /// Gets/sets SubscribeUrl
        /// </summary>
        private string SubscribeUrl
        {
            get
            {
                try
                {
                    return ViewState[ "SubscribeUrl" ].ToString();
                }
                catch ( NullReferenceException nex )
                {
                    return "";
                }
            }
            set { ViewState[ "SubscribeUrl" ] = value; }
        }
        string SearchUrl
        {
            get
            {
                try
                {
                    return ViewState[ "SearchUrl" ].ToString();
                }
                catch ( NullReferenceException nex )
                {
                    ViewState[ "SearchUrl" ] = "";
                    return "";
                }
            }
            set { ViewState[ "SearchUrl" ] = value; }
        }
        /// <summary>
        /// Gets/sets DisplayUrl
        /// </summary>
        private string DisplayUrl
        {
            get
            {
                try
                {
                    return ViewState[ "DisplayUrl" ].ToString();
                }
                catch ( NullReferenceException nex )
                {
                    return "";
                }
            }
            set { ViewState[ "DisplayUrl" ] = value; }
        }
        string SearchTitle
        {
            get
            {
                try
                {
                    return ViewState[ "SearchTitle" ].ToString();
                }
                catch ( NullReferenceException nex )
                {
                    ViewState[ "SearchTitle" ] = "";
                    return "";
                }
            }
            set { ViewState[ "SearchTitle" ] = value; }
        }
        /// <summary>
        /// Override Category heading (pathways) label text
        /// </summary>
        public string CategoryHeading
        {
            get
            {
                return categoryHeading.Text;
            }
            set { categoryHeading.Text = value; }
        }

        /// <summary>
        /// Override Search Results Heading
        /// </summary>
        public string ResultsHeading
        {
            get
            {
                return resultsHeading.Text;
            }
            set { resultsHeading.Text = value; }
        }


        /// <summary>
        /// If "yes", then show categories panel where more than one is found
        /// </summary>
        public string AllowingCategoriesPanelDisplay
        {
            get
            {
                try
                {
                    return allowCategoriesPanelDisplay.Text;

                }
                catch ( Exception ex )
                {
                    return "no";
                }
            }
            set
            {
                allowCategoriesPanelDisplay.Text = value;
            }
        }

        #endregion

        protected void Page_Load( object sender, EventArgs e )
        {

            if ( !Page.IsPostBack )
            {
                InitializeForm();
            }
        }

        protected void InitializeForm()
        {
            CurrentNewsTemplate = new NewsEmailTemplate();
            if ( NewsItemTemplateCode.Length == 0 )
            {
                //hide, no code found
                searchPanel.Visible = false;
                pnlResults.Visible = false;
                SetConsoleErrorMessage( "Error: a valid news code has not been provided." );
                return;
            }
            CurrentNewsTemplate = IOER.Controllers.NewsController.NewsTemlateGet( NewsItemTemplateCode );
            if ( CurrentNewsTemplate == null || CurrentNewsTemplate.Id == 0 )
            {
                //hide, no code found
                searchPanel.Visible = false;
                pnlResults.Visible = false;
                SetConsoleErrorMessage( "Sorry, this page has not been properly configured to display news yet." );
                return;
            }

            Category = CurrentNewsTemplate.Category;
            SubscribeUrl = CurrentNewsTemplate.ConfirmUrl;
            if ( SubscribeUrl != "" )
            {
                //??????
                lnkSubscribe.NavigateUrl = SubscribeUrl;
                lnkSubscribe.Visible = true;
            }
            if ( SearchTitle == "" )
                SearchTitle = "Read another news item";
            SearchUrl = CurrentNewsTemplate.SearchUrl;

            if ( Category == "" )
            {
                //try to figure based on path, or leave as unlikely
                pnlSubscribe.Visible = false;
            }
            else
            {
                NewsSubscribe.AnnouncementCategory = Category;
                //hide for now
                pnlSubscribe.Visible = true;

                //FormatCategory();
            }

            DisplayUrl = CurrentNewsTemplate.DisplayUrl;
            if ( DisplayUrl == "" )
            {
                DisplayUrl = defaultDisplayUrl.Text;
            }
            //the display url is often formatted for use in a stored proc. truncate any parameters
            if ( DisplayUrl.ToLower().IndexOf( "?" ) > -1 )
                DisplayUrl = DisplayUrl.Substring( 0, DisplayUrl.IndexOf( "?" ) ); ;

            //if ( SubscribeUrl.ToLower().IndexOf( parentUrl.ToLower() ) == -1 )
                //SubscribeUrl = parentUrl + "NewsSubscribe.htm";

            if ( CategoryHeading != "" )
            {
                Label1.Text = CategoryHeading;
            }
            if ( ResultsHeading != "" )
            {
                SearchResults.InnerHtml = ResultsHeading;
            }


            CheckRecordRequest();


        }
        protected void FormatCategory()
        {

           // string message = "";
            //string template = "( NewsItemCode in ({0}) )";
            string[] categories = Category.Split( ',' );
            //int cntr = 0;

            //foreach ( string cat in categories )
            //{
            //    cntr++;
            //    NewsEmailTemplate tmp = myEmailTemplateMgr.Get( cat.Trim() );

            //    ListItem item = new ListItem();
            //    item.Value = tmp.NewsItemCode;
            //    item.Text = tmp.Category;

            //    CBLCategory.Items.Add( item );
            //    if ( cntr == 1 )
            //    {
            //        //set defaults for now
            //        DisplayUrl = tmp.DisplayUrl;
            //        SubscribeUrl = tmp.ConfirmUrl;
            //        if ( DisplayUrl.ToLower().IndexOf( "?" ) > -1 )
            //        {
            //            DisplayUrl = DisplayUrl.Substring( 0, DisplayUrl.ToLower().IndexOf( "?" ) );
            //        }
            //    }
            //}//foreach

            //CBLCategory.Items[ 0 ].Selected = true;
            //if ( CBLCategory.Items.Count > 1 && AllowingCategoriesPanelDisplay == "yes" )
            //    categoriesPanel.Visible = true;

        }// 

        /// <summary>
        /// Check for a record request
        /// </summary>
        private void CheckRecordRequest()
        {
            string id = this.GetRequestKeyValue( "id", "" ); ;

            if ( id.Length == 36 )
            {
                searchSection.Visible = false;
                this.Get( id );
            }
            else
            {
                InitializeSearch();

            }


        }	// End 

        private void InitializeSearch()
        {
            GridViewSortExpression = "Created";
            GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Descending;

            //hide and show message?
            detailsPanel.Visible = false;

            PopulateControls();

            if ( UsingAppItemWebService.Equals( "yes" ) )
                this.searchOptions.SelectedIndex = 0;
            else
                this.searchOptions.SelectedIndex = 1;
            if ( IsTestEnv() )
            {
                localPanel1.Visible = true;
                //categoriesPanel.Visible = true;
            }

            DoSearch();
        }	// End 
        /// <summary>
        /// Get - retrieve form data
        /// </summary>
        /// <param name="recId"></param>
        private void Get( string recId )
        {
            try
            {
                //get record
                AppItem entity = new AppItem();
                if ( UsingAppItemWebService.Equals( "no" ) )
                {
                    //                    entity = new AppItemManager().Get( recId );
                    SetConsoleErrorMessage( "Error - direct database retrieve is not supported!" );
                }
                else
                {
                    entity = IOER.Controllers.AppItemController.AppItemGet( recId );
                }

                if ( entity == null || entity.HasValidRowId() == false )
                {
                    this.SetConsoleErrorMessage( "Sorry the requested item does not exist" );
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
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
                detailsPanel.Visible = false;
            }

        }	// End method


        /// <summary>
        /// Populate the form 
        ///</summary>
        private void PopulateForm( AppItem entity )
        {

            detailsPanel.Visible = true;
            lblDocumentLink.Text = "";
            lblDocumentLink.Visible = false;

            txtTitle = entity.Title;
            try
            {
                Label txtPageTitle = ( Label ) FindChildControl( Page, "pageCustomTitle" );
                Page.Master.Page.Title = "Illinois Open Educational Resources - " + entity.Title;
                if ( txtPageTitle != null )
                    txtPageTitle.Text = entity.Title;

            }
            catch ( Exception ex )
            {
                //log and ignore title 
                //LoggingHelper.LogError( ex, thisClassName + ".PopulateForm() - Unexpected error encountered" );
            }

            //assign description after handling custom rendering for items like glossary items, videos, links opening in a new window
            //OR should this be really done when item is saved????
            //txtDescr1 = UtilityManager.HandleCustomTextRendering( formRM, entity.Description );
            txtDescr1 = entity.Description;


            if ( SearchUrl != null && SearchUrl.Length > 0 )
            {
                ReadAnotherItem1.Text = ReadAnotherItem2.Text = string.Format( "<a href='{0}'>{1}</a>", SearchUrl, SearchTitle );
            }


            //check for doc info and display
            //not sure of location - may require author to provide a name placeholder in the html. If not found, then display at the bottom of the text
            if ( entity.DocumentRowId != null && !entity.IsInitialGuid( entity.DocumentRowId ) )
            {
                //===> NOT HANDLING docs
                //HandleDocumentPopulate( entity.DocumentRowId );
            }

            //check for an image ==========================================
            //===> NOT HANDLING IMAGES
            if ( entity.ImageId > 0 )
            {
                //image details should be in the entity, but check just in case
                if ( entity.AppItemImage == null || entity.AppItemImage.Id == 0 )
                {
                    //entity.AppItemImage = ImageStoreManager.Get( entity.ImageId );
                    //TODO - format
                }

            }
            else
            {
                //imgCurrent.Visible = false;
            }

            try
            {
                published = entity.Approved.ToString( "MM-dd-yyyy" );
                updated = entity.LastUpdated.ToString( "MM-dd-yyyy" );
                if ( published != updated )
                {
                    History.Visible = true;
                }
                else
                {
                    History.Visible = false;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( thisClassName + ".PopulateForm(): " + ex.ToString() );
            }
        }//

        protected void FormButton_Click( object sender, CommandEventArgs e )
        {
            switch ( e.CommandName )
            {
                case "Search":
                    DoSearch();
                    break;
                case "ResultsPerPage":
                    DoSearch();
                    break;
                default:
                    break;
            }
        }

        protected void DoSearch()
        {

            int selectedPageNbr = 0;
            string sortTerm = GetCurrentSortTerm();
            pager2.ItemCount = 0;
            DoSearch( selectedPageNbr, sortTerm );
        }

        protected void DoSearch( int selectedPageNbr, string sortTerm )
        {
            DataSet ds = null;

            bool usedWSForSearch = true;
            lblResults.Text = "";

            if ( selectedPageNbr == 0 )
            {
                selectedPageNbr = 1;
            }

            pager2.CurrentIndex = selectedPageNbr;

            CheckForPageSizeChange();

            int pTotalRows = 0;
            string filterMessage = "";
            string filter = FormatFilter( ref filterMessage );
            if ( filterMessage.Length > 0 )
            {
                SetConsoleErrorMessage( filterMessage );

                pnlResults.Visible = false;
                //ddlPageSizeList.Enabled = false;
                pager2.Visible = false;
                formGrid.DataSource = null;
                formGrid.DataBind();
                return;
            }

            if ( searchOptions.SelectedIndex == 1 )
            {
                usedWSForSearch = false;
                //ds = myManager.Search( filter, sortTerm, selectedPageNbr, pager2.PageSize, ref pTotalRows );
            }
            else
            {
                usedWSForSearch = true;
                ds = IOER.Controllers.AppItemController.AppItemWebServiceNewsSearch( filter, sortTerm, selectedPageNbr, pager2.PageSize, ref pTotalRows );

            }

            pager2.ItemCount = pTotalRows;

            if ( MyManager.DoesDataSetHaveRows( ds ) && ds.Tables[ 0 ].TableName != "ErrorMessage" )
            {
                pnlResults.Visible = true;
                lblResults.Text = string.Format( "{0} items found", pTotalRows );

                DataTable dt = ds.Tables[ 0 ];
                DataView dv = ( ( DataTable ) dt ).DefaultView;
                if ( sortTerm.Length > 0 )
                    dv.Sort = sortTerm;

                if ( pTotalRows > formGrid.PageSize )
                {
                    pager2.Visible = true;
                }
                else
                {
                    pager2.Visible = false;
                }

                //populate the grid
                formGrid.DataSource = dv;
                formGrid.DataBind();
            }
            else
            {
                pnlResults.Visible = false;
                if ( usedWSForSearch == true )
                    lblResults.Text = "No items found using News service";
                else
                    lblResults.Text = "No items found in database";

                ddlPageSizeList.Enabled = false;
                pager2.Visible = false;
                formGrid.DataSource = null;
                formGrid.DataBind();
            }
        }
        protected string FormatFilter( ref string filterMessage )
        {
            string filter = " (TypeId= " + AppItem.NewsItemType + " ) ";
            string booleanOperator = "AND";


            string status = " (Status='Published') ";
            if ( webOnlyPublishedStatus.Text.Trim().Length > 0 )
            {
                status = " (Status='Published' 	OR Status = '" + webOnlyPublishedStatus.Text + "') ";
            }
            filter += MyManager.FormatSearchItem( filter, status, booleanOperator );

            if ( txtSearch.Text.Trim().Length > 0 )
            {
                string keyword = MyManager.HandleApostrophes( FormHelper.CleanText( txtSearch.Text.Trim() ) );

                if ( keyword.IndexOf( "%" ) == -1 )
                    keyword = "%" + keyword + "%";

                string where = " (Title like '" + keyword + "'	OR [Description] like '" + keyword + "') ";
                filter += MyManager.FormatSearchItem( filter, where, booleanOperator );
            }

            //categories
            string filter2 = "";
            foreach ( ListItem li in CBLCategory.Items )
            {
                if ( li.Selected )
                {
                    string where = string.Format( "Category = '{0}'", li.Text );
                    filter2 += MyManager.FormatSearchItem( filter2, where, "OR" );
                }
            }
            if ( filter2.Length > 0 )
            {
                filter += MyManager.FormatSearchItem( filter, "(" + filter2 + ")", booleanOperator );
            }
            else if ( CurrentNewsTemplate.Category.Length > 0 )
            {
                filter += MyManager.FormatSearchItem( filter, "(" + string.Format( "Category = '{0}'", CurrentNewsTemplate.Category ) + ")", booleanOperator );
            } else
            {
                filterMessage = "You must select at least one pathway. Please select a pathway and try again.";
            }
            if ( this.IsTestEnv() )
                this.SetConsoleSuccessMessage( "sql: " + filter );

            return filter;
        }	//

        public string FormatTitle( string title, string rowId )
        {
            string formatted = string.Format( DataItemTemplate.Text, DisplayUrl, rowId, title );

            return formatted;
        }
        public string FormatDate( string date )
        {
            if ( this.IsDate( date ) )
            {
                string formatted = string.Format( dateTemplate.Text, System.DateTime.Parse( date ).ToString( dateFormat.Text ) );

                return formatted;
            }
            else
            {
                return "";
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

        private void PopulateControls()
        {
            InitializePageSizeList();
        } //

        #region Paging related methods
        protected void Pager_Command( object sender, CommandEventArgs e )
        {
            int currentPageIndx = Convert.ToInt32( e.CommandArgument );
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
            pager2.PageSize = defaultPageSize;

            this.SetListSelection( this.ddlPageSizeList, defaultPageSize.ToString() );

        } //

        private void SetPageSizeList()
        {
            DataSet ds1 = DatabaseManager.GetCodeValues( "GridPageSize", "SortOrder" );
            MyManager.PopulateList( this.ddlPageSizeList, ds1, "StringValue", "StringValue", "Select Size" );

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
                    //pager1.PageSize = size;
                    pager2.PageSize = size;
                    //Update user preference
                    Session[ SessionManager.SYSTEM_GRID_PAGESIZE ] = ddlPageSizeList.SelectedItem.Text;
                }
            }

        } //
        #endregion
        protected void BtnSelectAll_Click( object sender, EventArgs e )
        {
            foreach ( ListItem li in CBLCategory.Items )
            {
                li.Selected = true;
            }
        }
        protected void BtnReset_Click( object sender, EventArgs e )
        {
            foreach ( ListItem li in CBLCategory.Items )
            {
                li.Selected = false;
            }
        }
    }

}