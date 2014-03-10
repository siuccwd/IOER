using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using GDAL = Isle.BizServices;
using ILP = ILPathways.Business;
using MyMgr = Isle.BizServices.LibraryBizService;
using AccountMgr = Isle.BizServices.AccountServices;
using OrgMgr = Isle.BizServices.OrganizationBizService;
using ILPathways.Library;
using ILPathways.Common;
using ILPathways.Utilities;
using IDBM = ILPathways.DAL.DatabaseManager;
//using DBM = LRWarehouse.DAL.DatabaseManager;
using AppUser= LRWarehouse.Business.Patron;
using LRWarehouse.Business;

namespace ILPathways.Controls.Libraries
{
    public partial class LibraryAdmin : BaseUserControl
    {
        MyMgr mgr = new MyMgr();
        private string thisClassName = "LibraryAdmin";
        #region Properties

        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string FormSecurityName
        {
            get { return this.formSecurityName.Text; }
            set { this.formSecurityName.Text = value; }
        }
        public ILP.Library CurrentLibrary
        {
            get
            {
                if (ViewState["CurrentLibrary"] == null)
                    ViewState["CurrentLibrary"] = new ILP.Library();
                return ViewState["CurrentLibrary"] as ILP.Library;
            }
            set { ViewState["CurrentLibrary"] = value; }
        }
        public int CurrentLibraryId
        {
            get
            {
                if ( !IsInteger(txtCurrentLibraryId.Text) )
                    txtCurrentLibraryId.Text = "0";

                return Int32.Parse(txtCurrentLibraryId.Text);
            }
            set { txtCurrentLibraryId.Text = value.ToString(); }
        }
        public int CurrentLibraryMemberId
        {
            get
            {
                if ( !IsInteger( txtCurrentLibraryMemberId.Text ) )
                    txtCurrentLibraryId.Text = "0";

                return Int32.Parse( txtCurrentLibraryMemberId.Text );
            }
            set { txtCurrentLibraryMemberId.Text = value.ToString(); }
        }
        public List<CodeItem> OrgCodesList 
        {
            get {
                if ( ViewState[ "OrgCodesList" ] == null )
                    ViewState[ "OrgCodesList" ] = new List<CodeItem>();
                return ViewState[ "OrgCodesList" ] as List<CodeItem>; }
            set { ViewState[ "OrgCodesList" ] = value; }
        }
        
        public AppUser Invitee 
        {
            get {
                if ( ViewState[ "Invitee" ] == null )
                    ViewState[ "Invitee" ] = new AppUser();
                return ViewState[ "Invitee" ] as AppUser; 
            }
            set { ViewState[ "Invitee" ] = value; }
        }

        /// <summary>
        /// get/set current Invitation
        /// </summary>
        public ILP.LibraryInvitation Invitation 
        {
            get {
                if ( ViewState[ "Invitation" ] == null )
                    ViewState[ "Invitation" ] = new ILP.LibraryInvitation();
                return ViewState[ "Invitation" ] as ILP.LibraryInvitation; 
            }
            set { ViewState[ "Invitation" ] = value; }
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
            if ( IsUserAuthenticated() == false )
            {
                SetConsoleErrorMessage( "Error: you must be authenticated in order to use this page.<br/>Please login and try again." );
                formContainer.Visible = false;
                return;
            }

            if ( !Page.IsPostBack )
            {
                this.InitializeForm();
            }
        }
        protected void Page_PreRender( object sender, EventArgs e )
        {

            try
            {
                pager2.CurrentIndex = LastPageNumber;
                pager2.ItemCount = LastTotalRows;
            }
            catch
            {
                //no action
            }

        }//
        private void InitializeForm()
        {
            //formSecurityName
            //==> actually first check for any admin libs
            //this is prob to allow admin accss to all libs
            this.FormPrivileges = GDAL.SecurityManager.GetGroupObjectPrivileges( this.WebUser, FormSecurityName );
            if ( CurrentUser.Username == "mparsons" )
            {
                FormPrivileges.SetAdminPrivileges();
                sourceLibraryId.Visible = true;
                collectionId.Visible = true;
            }
            if ( FormPrivileges.CreatePrivilege >= ( int )ILP.EPrivilegeDepth.Region )
            {
                //show link to load all libraries
                showAllLibraries.Visible = true;
            }
            GridViewSortExpression = "";
            GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
            LastPageNumber = 0;
            InitializePageSizeList();

            //load adminable libs, if false, no libs
            bool hasLibs = ShowAllowedLibraries();
            //chck if has orgs for adding a lib
            bool hasOrgAdmin = ShowAllowedOrgs();
            if ( hasOrgAdmin == false )
            {
                newLibraryLink.Visible = false;
            }
            if ( hasLibs == false && hasOrgAdmin == false )
            {
                SetConsoleErrorMessage( "Note: You must be the administrator of a library, or have a User library in order to use this page.<br/>Please set up your <a href='/My/Library'>User library</a> or request administrator access to another library and then return to this page." );
                formContainer.Visible = false;
            }
            else
            {
                CheckRecordRequest();
            }

        }//
        /// <summary>
        /// Check the request type for the form
        /// </summary>
        private void CheckRecordRequest()
        {

            // Check if a request was made for a specific search parms
            int id = this.GetRequestKeyValue( "id", 0 );

            if ( id > 0 )
            {
                //get library and see if has access - future

            }

        }	// End 
        protected bool ShowAllowedLibraries()
        {
            bool hasLibraries = false;
            DataSet ds = mgr.Libraries_SelectWithEditAccess( WebUser.Id );
            //DataSet ds = mgr.LibrarySearch( "lib.IsActive = 1", "lib.Title", 1, 1000, ref totalRows );
            if ( DoesDataSetHaveRows( ds ) )
            {
                hasLibraries = true;
                IDBM.PopulateList( this.sourceLibrary, ds, "Id", "FormattedTitle", "Select a Library" );
                if ( ds.Tables[ 0 ].Rows.Count == 1 )
                {
                    sourceLibrary.SelectedIndex = 1;
                    sourceLibrary_SelectedIndexChanged(new object(), new EventArgs());
                }
            }
            return hasLibraries;

        }

        protected void refreshLibrariesLink_Click( object sender, EventArgs e )
        {
            ShowAllowedLibraries();
        }

        protected void showAllLibraries_Click( object sender, EventArgs e )
        {
            int totalRows = 0;
            DataSet ds = mgr.LibrarySearch( "lib.IsActive = 1", "lib.Title", 1, 1000, ref totalRows );
            if ( DoesDataSetHaveRows( ds ) )
            {

                IDBM.PopulateList( this.sourceLibrary, ds, "Id", "title", "Select a Library" );
            }
        }
        protected bool ShowAllowedOrgs()
        {
            bool hasOrgs = false;
            //probably need to take into account the early state where org is created for first person, and no org member exists 
            //===> or add this to the org create proc - for fist time in
            List<ILP.OrganizationMember> list = OrgMgr.OrganizationMember_GetAdminOrgs( WebUser.Id );
            if ( list != null && list.Count > 0 )
            {
                hasOrgs = true;
                List<CodeItem> codes = new List<CodeItem>();
                if ( list.Count > 0 )
                {
                    CodeItem ci = new CodeItem();
                    foreach ( ILP.OrganizationMember item in list )
                    {
                        ci = new CodeItem();
                        ci.Id = item.Id;
                        ci.Title = item.Organization;
                        codes.Add( ci );
                    }

                    OrgCodesList = codes;
                    IDBM.PopulateList( this.ddlOrgs, codes, "Id", "Title", "Select an Organization" );
                    //this.orgsPanel.Visible = true;
                }
            }

            return hasOrgs;
        }



        protected void sourceLibrary_SelectedIndexChanged( object sender, EventArgs e )
        {
            CurrentLibraryMemberId = 0;

            if ( sourceLibrary.SelectedIndex > 0 )
            {
                //reset action areas
                membersGrid.DataSource = null;
                membersGrid.DataBind();
                HideSections();

                CurrentLibraryId = Int32.Parse( sourceLibrary.SelectedValue );
                CurrentLibrary = mgr.Get(CurrentLibraryId);
                pageTitle.Text = "Library Administration - " + CurrentLibrary.Title;
                //need to also check if is my personal library, or is an org admin
                if ( CurrentLibrary.IsMyPersonalLibrary( WebUser.Id ) )
                {
                    CurrentLibraryMemberId = ILP.LibraryMember.LIBRARY_MEMBER_TYPE_ID_ADMIN;
                }
                else
                {
                    ILP.LibraryMember lm = mgr.LibraryMember_Get( CurrentLibraryId, WebUser.Id );
                    if ( lm != null && lm.MemberTypeId > 2 )
                    {
                        CurrentLibraryMemberId = lm.MemberTypeId;
                    } else 
                    {
                        //not a Member, do an org check 
                        if ( CurrentLibrary.OrgId > 0 )
                        {
                            ILP.OrganizationMember orgMbr = OrgMgr.OrganizationMember_Get( CurrentLibrary.OrgId, WebUser.Id );
                            if ( orgMbr != null && orgMbr.SeemsPopulated )
                            {
                                if ( orgMbr.HasAdministratorRole() 
                                  || orgMbr.HasLibraryAdministratorRole ())
                                {
                                    CurrentLibraryMemberId = ILP.LibraryMember.LIBRARY_MEMBER_TYPE_ID_ADMIN;
                                }
                            }
                        }
                        else if ( FormPrivileges.CreatePrivilege >= ( int ) ILP.EPrivilegeDepth.Region )
                        {
                            CurrentLibraryMemberId = ILP.LibraryMember.LIBRARY_MEMBER_TYPE_ID_ADMIN;
                        }
                    }
                    
                }

                if ( CurrentLibraryMemberId == 0 )
                {
                    //no apparant access
                    SetLibraryOptionsState( false );
                    SetConsoleErrorMessage( "Hmmm - you don't appear to have administrative access to this library." );
                    CurrentLibraryId = 0;
                    CurrentLibrary = null;
                    sourceLibrary.SelectedIndex = -1;
                    return;
                }

                sourceLibraryId.Text = CurrentLibraryId.ToString();
                litCurrentLibrary.Text = sourceLibrary.SelectedItem.Text;

                if ( FormPrivileges.CreatePrivilege < ( int )ILP.EPrivilegeDepth.State )
                {
                    DataSet ds = mgr.LibrarySections_SelectWithEditAccess( CurrentLibraryId, WebUser.Id );
                    IDBM.PopulateList( this.sourceCollection, ds, "Id", "title", "Select Collection" );
                }
                else
                {
                    DataSet ds = mgr.LibrarySectionsSelect( CurrentLibraryId );
                    IDBM.PopulateList( this.sourceCollection, ds, "Id", "title", "Select Collection" );
                }
                SetLibraryOptionsState( true );
            }
            else
            {
                CurrentLibrary = new ILP.Library();
                CurrentLibraryId = 0;
                pageTitle.Text = "Library Administration";
                litCurrentLibrary.Text = " none";
                HideSections();
                startPanel.Visible = true;
                SetLibraryOptionsState( false );
                sourceCollection.DataSource = null;
                sourceCollection.DataBind();
                collectionId.Text = "";
                sourceLibraryId.Text = "";
            }
        } //

        protected void SetLibraryOptionsState( bool state )
        {
            editLibraryLink.Visible = state;
            libMbrsLink.Visible = state;
            libInviteLink.Visible = state;

        } //

        protected void sourceCollection_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( sourceCollection.SelectedIndex > 0 )
            {
                collectionId.Text = sourceCollection.SelectedValue;
                colOptionsPanel.Enabled = true;
                litCurrentCollection.Text = sourceCollection.SelectedItem.Text;
            }
            else
            {
                litCurrentCollection.Text = " none";
                collectionId.Text = "";
                colOptionsPanel.Enabled = false;
            }
        }

        protected void newLibraryLink_Click( object sender, EventArgs e )
        {
            HideSections();
            //ensure orglist did not get lost
            //use 2 as the select ... row should have been added
            if ( OrgCodesList == null || OrgCodesList.Count < 2 )
            {
                if ( ShowAllowedOrgs() == false)  
                {
                    //problems?
                    SetConsoleErrorMessage( "Error: - no organizations were found - not allowed to create an organization library." );
                    return;
                }
            }
            pageTitle.Text = "Library Administration";
            LibraryPanel.Visible = true;
            LibraryMtce1.InitializeOrgLibary( "Enter library name", OrgCodesList );
            refreshLibrariesLink.Visible = true;
        }

        protected void editLibraryLink_Click( object sender, EventArgs e )
        {
            HideSections();
            LibraryPanel.Visible = true;
            if ( sourceLibrary.SelectedIndex > 0 )
            {
                int libId = Int32.Parse( sourceLibrary.SelectedValue );
                LibraryMtce1.SetOrgLibrary( libId );
            }
        }
        protected void libMbrsLink_Click( object sender, EventArgs e )
        {
            HideSections();
            membersPanel.Visible = true;
            if ( sourceLibrary.SelectedIndex > 0 )
            {
                DoMembersSearch();
            }
            //
        }
        protected void HideSections()
        {
            startPanel.Visible = false;
            LibraryPanel.Visible = false;
            collectionPanel.Visible = false;
            membersPanel.Visible = false;
            invitationsPanel.Visible = false;
            followersPanel.Visible = false;

            rfvEmail.Enabled = false;
        } //
        protected void libInviteLink_Click( object sender, EventArgs e )
        {
            HideSections();
            invitationsPanel.Visible = true;
            newInvitationLink_Click( sender, e );
            
        }

        #region  Members =============================================================

        private void DoMembersSearch()
        {
            int selectedPageNbr = 0;
            string sortTerm = GetCurrentSortTerm();
            pager2.ItemCount = 0;

            DoMembersSearch( selectedPageNbr, sortTerm );
        } //

        /// <summary>
        /// Conduct a search while addressing current page nbr and a sort term
        /// </summary>
        /// <param name="selectedPageNbr"></param>
        /// <param name="sortTerm"></param>
        private void DoMembersSearch( int selectedPageNbr, string sortTerm )
        {
            DataSet ds = null;
            if ( selectedPageNbr == 0 )
            {
                //with custom pager, need to start at 1
                selectedPageNbr = 1;
            }
            LastPageNumber = selectedPageNbr;
            pager2.CurrentIndex = selectedPageNbr;

            // Set the page size for the DataGrid control based on the selection
            CheckForPageSizeChange();

            int pTotalRows = 0;

            
            //Q and Dirty search
            int libId = Int32.Parse( sourceLibrary.SelectedValue );
            //string filter = FormatFilter();
            string filter = string.Format(" LibraryId = {0} ", libId);
            List<ILP.LibraryMember> list = mgr.LibraryMember_Search( filter, sortTerm, selectedPageNbr, pager2.PageSize, ref pTotalRows );
            pager2.ItemCount = pTotalRows;

            LastTotalRows = pTotalRows;
            ddlPageSizeList.Visible = false;
            if ( list == null || list.Count == 0 )
            {
                SetConsoleInfoMessage( "No records were found for the provided search criteria" );
                membersGrid.Visible = false;
                pager2.Visible = false;
            }
            else
            {
                membersGrid.Visible = true;
                ddlPageSizeList.Visible = true;

                if ( pTotalRows > membersGrid.PageSize )
                {
                    //membersGrid.PagerSettings.Visible = true;
                    pager2.Visible = true;
                }
                else
                {
                    pager2.Visible = false;
                }

                //populate the grid
                membersGrid.DataSource = list;
                //membersGrid.PageIndex = selectedPageNbr;
                membersGrid.DataBind();
            }
            
        }
     

        protected void colMbrsLink_Click( object sender, EventArgs e )
        {

        }
        protected void pendingMembers_Click( object sender, EventArgs e )
        {

        }

        protected void searchLink_Click( object sender, EventArgs e )
        {

        }

        protected void membersGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                //System.Data.DataRowView drv = ( DataRowView ) e.Row.DataItem;
                ILP.LibraryMember drv = ( ILP.LibraryMember ) e.Row.DataItem;
                if ( ( e.Row.RowState & DataControlRowState.Edit ) == DataControlRowState.Edit )
                {
                    FormatEditRow( drv, e );
                }
                else
                {
                    //check if item is in use, cannot delete these records
                    //LinkButton delBtn = ( LinkButton ) e.Row.FindControl( "deleteRowButton" );
                    //if ( FormPrivileges.CanDelete() )
                    //{
                    //    delBtn.Enabled = true;
                    //    delBtn.Attributes.Add( "onclick", "javascript:return " +
                    //            "confirm('Are you sure you want to delete this record (" +
                    //            DataBinder.Eval( e.Row.DataItem, "ClusterTitle" ) + ")')" );
                    //}
                    //else
                    //{
                    //    delBtn.Visible = false;
                    //}
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
        }
        /// <summary>
        /// Format a row about to be edited
        /// </summary>
        /// <param name="drv"></param>
        /// <param name="e"></param>
        protected void FormatEditRow( ILP.LibraryMember drv, GridViewRowEventArgs e )
        {

            //Populate Drop-down list on Row in Data Grid, NOT the Drop-down list at the top of the control.
            DropDownList ddl = ( DropDownList ) e.Row.FindControl( "gridDdlTypes" );
            PopulateMemberTypes( ddl );

            //string mbrTypeId = GetRowColumn( drv, "MemberTypeId", "" );
            string mbrTypeId = drv.MemberTypeId.ToString();
            //if ( siteFormat.Length > 0 )
            if ( mbrTypeId.Length > 0 )
            {
                this.SetListSelection( ddl, mbrTypeId.ToString() );
            }
            else
            {
                ddl.SelectedIndex = 0;
            }

        }
        public static string GetRowColumn( DataRowView row, string column, string defaultValue )
        {
            string colValue = "";

            try
            {
                colValue = row[ column ].ToString();

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                colValue = defaultValue;
            }
            return colValue;

        } // end method
        /// <summary>
        /// handle row commands
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void membersGrid_RowCommand( object sender, GridViewCommandEventArgs e )
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
                //pager1.CurrentIndex = pager2.CurrentIndex = LastPageNumber;
                //pager1.ItemCount = pager2.ItemCount = LastTotalRows;

                // get the ID of the clicked row
                int ID = Convert.ToInt32( e.CommandArgument );

                // show the record 
                //ShowRecord( ID );
            }
        }
        /// <summary>
        /// handle row delete
        /// </summary>
        /// <param name="recordId"></param>
        protected void DeleteRecord( int recordId )
        {
            try
            {
                string statusMessage = "";

                if ( mgr.LibraryMember_Delete( recordId ) )
                {
                    this.SetConsoleSuccessMessage( "Successfully removed member from this library." );
                    //OK reset list
                    string sortTerm = "";   // GetCurrentSortTerm();

                    DoMembersSearch();
                }
                else
                {
                    // problem
                    this.SetConsoleErrorMessage( "An unexpected issue was encountered while attempting to delete this record. System administration has been notified:<br/> " + statusMessage );
                    LoggingHelper.LogError(  "LibraryAdmin.DeleteRecord() - Delete failed for library member id of " + recordId.ToString() + " and returned the following message:<br/>" + statusMessage, true );
                }

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "LibraryAdmin.DeleteRecord() - Unexpected error encountered while attempting to delete library member id of " + recordId.ToString() );
                this.SetConsoleErrorMessage( "An unexpected error was encountered while attempting to delete this record. System administration has been notified:<br/> " + ex.Message.ToString() );
            }
        }
        /// <summary>
        /// on page change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void membersGrid_PageIndexChanging( object sender, GridViewPageEventArgs e )
        {
            membersGrid.PageIndex = e.NewPageIndex;
            //get current sort term
            string sortTerm = GetCurrentSortTerm();

            DoMembersSearch( membersGrid.PageIndex, sortTerm );
        }

        /// <summary>
        /// Checks selected sort column and determines if new sort or a change in the direction of the sort
        /// </summary>
        protected void membersGrid_Sorting( object sender, GridViewSortEventArgs e )
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
            if ( newSortExpression.ToLower() != "SortName" )
            {
                sortTerm += ", SortName ASC";
            }

            DoMembersSearch( 1, sortTerm );

        }
        /// <summary>
        /// fires when edit link is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void EditRecord( object sender, GridViewEditEventArgs e )
        {

            membersGrid.EditIndex = e.NewEditIndex;
            DoMembersSearch();

            membersGrid.FooterRow.Visible = false;
            //this.rfvGridTypes.Enabled = false;


        }

        /// <summary>
        /// fires when cancel link is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void CancelRecord( object sender, GridViewCancelEditEventArgs e )
        {
            membersGrid.EditIndex = -1;
            DoMembersSearch();
           //this.rfvgridTypes.Enabled = false;
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
                GridViewRow row = ( GridViewRow ) membersGrid.Rows[ e.RowIndex ];
                int id = Int32.Parse(membersGrid.DataKeys[ e.RowIndex ].Value.ToString());

                //get values from grid 
                 DropDownList ddl = ( DropDownList ) row.FindControl( "gridDdlTypes" );
                 if ( ddl != null && ddl.SelectedIndex > 0 )
                 {
                     //update the record
                     bool isOk = mgr.LibraryMember_Update( id, Int32.Parse( ddl.SelectedValue ), WebUser.Id );

                     if ( isOk )
                     {
                         SetConsoleSuccessMessage( "Successfully updated record" );
                         // Refresh the data
                         membersGrid.EditIndex = -1;

                         DoMembersSearch();
                     }
                     else
                     {
                         SetConsoleErrorMessage( "Update appears to have failed without a reason code" );
                     }
                 }

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".UpdateRecord" );

            }
        }
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
        protected void PopulateMemberTypes( DropDownList ddl )
        {
            PopulateMemberTypes( ddl, false );
        }	// End 

        protected void PopulateMemberTypes( DropDownList ddl, bool selectFirst )
        {

            List<CodeItem> codes = MyMgr.GetCodes_LibraryMemberType(); ;
            if ( codes != null && codes.Count > 0 )
            {
                //Select an Member Type
                IDBM.PopulateList( ddl, codes, "Id", "Title", "" );
                if ( selectFirst )
                {
                    ddl.SelectedIndex = 1;
                }

            }
        }	// End 


        #region Paging related methods
        public void pager_Command( object sender, CommandEventArgs e )
        {
            int currentPageIndx = Convert.ToInt32( e.CommandArgument );
            pager2.CurrentIndex = currentPageIndx;
            string sortTerm = GetCurrentSortTerm();

            DoMembersSearch( currentPageIndx, sortTerm );
        }
        /// <summary>
        /// Initialize page size list and check for a previously set size
        /// </summary>
        private void InitializePageSizeList()
        {
            SetPageSizeList();

            //Set page size based on user preferences
            int defaultPageSize = 25;
            this.membersGrid.PageSize = defaultPageSize;
            
            pager2.PageSize = defaultPageSize;
            this.SetListSelection( this.ddlPageSizeList, defaultPageSize.ToString() );

        } //
        private void SetPageSizeList()
        {
            GDAL.CodeTableBizService.PopulateGridPageSizeList(ref this.ddlPageSizeList);
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
                if ( membersGrid.PageSize != size )
                {
                    membersGrid.PageSize = size;
                    //pager1.PageSize = size;
                    pager2.PageSize = size;
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
                membersGrid.PageIndex = 0;
                //retain curent sort though
                string sortTerm = GetCurrentSortTerm();

                DoMembersSearch( membersGrid.PageIndex, sortTerm );
            }
        } //
        #endregion
         #endregion 
        #region  ============== invitations =====================

        protected void newInvitationLink_Click( object sender, EventArgs e )
        {
            Invitee = new AppUser();
            Invitation = new ILP.LibraryInvitation();
            invitePanelStart.Visible = true;
            txtEmail.Text = "";
            ddlMemberType.SelectedIndex = 0;
            rblAddToOrg.SelectedIndex = 0;
            txtFirstName.Text = "";
            txtLastName.Text = "";
            rfvEmail.Enabled = true;
            rblAddToOrg.Visible = true;

            finshInvitePanel.Visible = false;
            invitePanel2.Visible = false;
            userFoundMsg.Visible = false;
            notFoundMsg.Visible = false;
            inviteStep6Final.Enabled = true;
        }//

        protected void inviteStep2Link_Click( object sender, EventArgs e )
        {
            invitePanelStart.Visible = false;
            invitePanel2.Visible = true;
            inviteStep3NoAcct.Visible = false;
            inviteStep4HasAcct.Visible = false;
            createAcctPanel.Visible = false;

            //look up user
            if ( txtEmail.Text != null && txtEmail.Text.Trim().Length > 5 )
            {
                bool sameOrg = false;
                Invitee = new AccountMgr().GetByEmail(txtEmail.Text.Trim());
                if(Invitee != null && Invitee.Id > 0) 
                {
                    //check if a member already
                    ILP.LibraryMember mbr = mgr.LibraryMember_Get( CurrentLibraryId, Invitee.Id );
                    if ( mbr != null && mbr.SeemsPopulated )
                    {
                        SetConsoleErrorMessage( string.Format("Note: {0} is already a member {1} of this library", Invitee.FullName(), mbr.MemberType)) ;
                        invitePanelStart.Visible = true;
                        invitePanel2.Visible = false;
                        //NICE TO HAVE: do org role here
                        return;
                    }

                    //if library has an org, offer to add to org
                    if ( CurrentLibrary.OrgId > 0) 
                    {
                        ILP.OrganizationMember orgMbr = OrgMgr.OrganizationMember_Get( CurrentLibrary.OrgId, Invitee.Id );
                        if ( orgMbr != null && orgMbr.SeemsPopulated )
                        {
                            //don't attempt org mbr roles here
                      
                            sameOrg = true;
                            rblAddToOrg.Visible = false;
                            //could  check for org mmember but n/a for now
                            //===> could have a default orgId, but not be an org admin!!
                           
                            messagePanel.Visible = true;
                            invitePanelStart.Visible = false;
                        } 
                        else 
                        {
                            rblAddToOrg.Visible = true;
                            invitePanel2.Visible = true;
                            invitePanelStart.Visible = false;
                            inviteStep4HasAcct.Visible = true;
                            userFoundMsg.Visible = true;
                            userFoundMsg.Text = string.Format(userFoundOrgMessage.Text, Invitee.FullName());
                        }
                    } else 
                    {
                        //personal so no offer, skip org
                        messagePanel.Visible = true;
                        invitePanelStart.Visible = false;
                    }
                } else 
                {
                    //not found show next
                    Invitee = new AppUser();
                    Invitee.Email = txtEmail.Text.Trim();
                    
                    invitePanel2.Visible = true;
                    inviteStep3NoAcct.Visible = true;
                    createAcctPanel.Visible = true;
                    invitePanelStart.Visible = false;
                    notFoundMsg.Visible = true;
                }
            }
            else
            {
                SetConsoleErrorMessage("Error - Please enter a valid email addess and try again.");
            }
        }//

        protected void inviteStep3NoAcct_Click( object sender, EventArgs e )
        {
            invitePanel2.Visible = false;
            messagePanel.Visible = true;
            //no special actions, unless we want to just create an acct using email?
        }

        protected void inviteStep4HasAcct_Click( object sender, EventArgs e )
        {
            //no special actions
            invitePanel2.Visible = false;
            messagePanel.Visible = true;
        }

        protected void inviteStep5CreateAcct_Click( object sender, EventArgs e )
        {

            bool isValid = true;
            //validate names
            if ( this.txtLastName.Text.Trim().Length < 2 )
            {
                SetConsoleErrorMessage( "Error: please enter a last name (at least two letters)" );
                isValid = false;
            }
            if ( this.txtFirstName.Text.Trim().Length < 2 )
            {
                SetConsoleErrorMessage( "Error: please enter a first name (at least two letters)" );
                isValid = false;
            }
            if ( isValid == false )
                return;

            //=======================================================================================
            //may want to hold off incase not submitted
            Invitee.FirstName = this.txtFirstName.Text.Trim();
            Invitee.LastName = this.txtLastName.Text.Trim();
            string password = "ChangeMe_" + System.DateTime.Now.Millisecond.ToString();
            Invitee.TempProperty1 = password;
            Invitee.Password = ILPathways.Utilities.UtilityManager.Encrypt( password );
            Invitee.Username = txtEmail.Text;
            Invitee.Email = txtEmail.Text;

            invitePanel2.Visible = false;
            messagePanel.Visible = true;

        }


        protected void inviteStep6Final_Click( object sender, EventArgs e )
        {
            messagePanel.Visible = false;
            bool isNewAcct = false;
            int orgId = 0;
            int orgRoleId = 0;
            //need to check for user clicking finish twice

            string statusMessage = "";
            if ( Invitee == null || Invitee.Email == null || Invitee.Email.Trim().Length < 10 )
            {
                SetConsoleErrorMessage("Error: a valid invitation was not created. ");
                return;
            }

            invitePanel2.Visible = false;
            finshInvitePanel.Visible = true;
     
            if ( Invitee.Id == 0 && (Invitee.FirstName != null && Invitee.FirstName.Length > 1 ))
            {
                int id = new AccountMgr().Create( Invitee, ref statusMessage );
                if ( id > 0 )
                {
                    isNewAcct = true;
                    //do get to retrieve rowId
                    Invitee = new AccountMgr().Get(id);
                }
                else
                {
                    //??
                    SetConsoleErrorMessage( "There was a problem creating the account. System admin has been notified." );
                    EmailManager.NotifyAdmin("Problem creating a quick account from the invitation process", "Don't know why, but: "
                        + "<br/>email: " + Invitee.Email
                        + "<br/> Name: " + Invitee.FullName()
                        + "<br/>Status Msg: " + statusMessage
                        );
                }
            }//
            Invitation.CreatedById = WebUser.Id;
            Invitation.Created = System.DateTime.Now;
            Invitation.LastUpdated = System.DateTime.Now;
            Invitation.LastUpdatedById = WebUser.Id;

            if ( rblAddToOrg.SelectedIndex > 0 )
            {
                //assume to use same as library, could be a personal. Maybe check up front
                if ( CurrentLibrary.OrgId > 0 )
                {
                    Invitation.AddToOrgId = CurrentLibrary.OrgId;
                }
                else if ( WebUser.OrgId > 0 )
                {
                    Invitation.AddToOrgId = WebUser.OrgId;
                }
                else
                {
                    //no org, just skip
                }
                Invitation.AddAsOrgMemberTypeId = Int32.Parse( rblAddToOrg.SelectedValue );
            }

            string toEmail = Invitee.Email;
            string bcc = UtilityManager.GetAppKeyValue( "systemAdminEmail", "mparsons@siuccwd.com" );
            string fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", "mparsons@siuccwd.com" );
            if ( this.doingBccOnRegistration.Text == "no" )
                bcc = "";
            //
            bool isSecure = false;
            string eMessage = "";
            if ( UtilityManager.GetAppKeyValue( "SSLEnable", "0" ) == "1" )
                isSecure = true;

            string subject = string.Format( inviteSubject.Text, WebUser.FullName() );
            string libUrl = string.Format( this.libraryLink.Text, CurrentLibrary.Id );
            libUrl = UtilityManager.FormatAbsoluteUrl( libUrl, false );
            string libMessage = string.Format(this.visitLibraryMsg.Text, libUrl, CurrentLibrary.Title);

            eMessage = string.Format( this.inviteEmail.Text, CurrentLibrary.Title, ddlMemberType.SelectedItem.Text );


            if ( Invitee.Id == 0 )
            {
                //action: request user to create an account
                //first create the library invite, and attach the rowId
                //initialize invite (only necessary if no acct)
                Invitation.ParentId = CurrentLibraryId;
                Invitation.InvitationType = "Library";
                Invitation.LibMemberTypeId = Int32.Parse( ddlMemberType.SelectedValue );
                Invitation.StartingUrl = string.Format( this.libraryLink.Text, CurrentLibraryId ); 
                Invitation.TargetEmail = Invitee.Email;
                Invitation.IsActive = true;
                Invitation.TargetUserId = Invitee.Id;
                Invitation.Subject = subject;
                string gettingStarted = string.Format(invitationMessageContent.Text
                    , UtilityManager.FormatAbsoluteUrl( libUrl, false)
                    , UtilityManager.FormatAbsoluteUrl( gettingStartedLink.Text, false)
                    , UtilityManager.FormatAbsoluteUrl( librariesHome.Text, false ) );
                Invitation.MessageContent = invitationMessageContent.Text;

                Invitation.ExpiryDate = System.DateTime.Now.AddDays( 7 );
                Invitation.RowId = Guid.NewGuid();


                int inviteId = mgr.LibraryInvitation_Create( Invitation, ref statusMessage );

                string url = string.Format( this.registerLink.Text, Invitation.RowId.ToString() );
                url = UtilityManager.FormatAbsoluteUrl( url, isSecure );


                string msg = string.Format( this.doRegisterMsg.Text, url );
                eMessage += msg;

            }
            else
            {
                if ( isNewAcct )
                {
                    //action: provide confirm url to ???. 
                    //Should they be added to lib before confirm?
                    string confirmUrl = string.Format( this.activateLink.Text, Invitee.RowId.ToString() );
                    confirmUrl = UtilityManager.FormatAbsoluteUrl( confirmUrl, isSecure );

                    string acctCreated = string.Format( this.acctCreatedMsg.Text, Invitee.Email, "Change password on login", confirmUrl );
                    eMessage += acctCreated;
                }
                else
                {
                    //add user as lib member 
                    mgr.LibraryMember_Create( CurrentLibraryId,
                                    Invitee.Id,
                                    Int32.Parse( ddlMemberType.SelectedValue ),
                                    WebUser.Id, ref statusMessage );

                    string contributeUrl = string.Format( contributeLink.Text, Invitee.RowId.ToString() );
                    contributeUrl = UtilityManager.FormatAbsoluteUrl( contributeUrl, isSecure );
                    eMessage += contributeUrl;
                }

                if ( Invitation.AddToOrgId > 0 )
                {
                    CreateOrgMember( Invitee, Invitation );
                }
            }
            eMessage += libMessage;

            if ( this.txtMessage.EditPanel.Content.Trim().Length > 10 )
            {
                eMessage += " <p>" + txtMessage.EditPanel.Content + "</p>";
            }

            EmailManager.SendEmail( toEmail, fromEmail, subject, eMessage, "", bcc );
            SetConsoleSuccessMessage( "Successfully sent the invitation" );
            finshInvitePanel.Visible = true;
           
        }

        protected void CreateOrgMember( AppUser user, ILP.LibraryInvitation invite )
        {
            string statusMessage = "";
            ILP.OrganizationMember om = new ILP.OrganizationMember();
            om.UserId = user.Id;
            om.OrgId = invite.AddToOrgId;
            om.OrgMemberTypeId = invite.AddAsOrgMemberTypeId;
            om.CreatedById = invite.CreatedById;
            om.LastUpdatedById = invite.CreatedById;
            int id = OrgMgr.OrganizationMember_Create( om, ref statusMessage );

            //also create profile (default org), if not already have orgId
            if ( user.OrgId == 0 )
            {
                PatronProfile prof = new PatronProfile();
                prof.UserId = user.Id;
                prof.OrganizationId = invite.AddToOrgId;
                new AccountMgr().PatronProfile_Create( prof, ref statusMessage );
            }
        }
        protected void showInvitations_Click( object sender, EventArgs e )
        {
            pendingInvitesPanel.Visible = true;
        }

        /// <summary>
        /// collection invites
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void colInviteLink_Click( object sender, EventArgs e )
        {

        }
        
        #endregion


    }
}