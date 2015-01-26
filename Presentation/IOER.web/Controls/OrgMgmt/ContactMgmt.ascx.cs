using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
using Isle.BizServices;

using LRWarehouse.Business;
using LRWarehouse.DAL;
using LDBM = LRWarehouse.DAL.DatabaseManager;

namespace ILPathways.Controls.OrgMgmt
{
    public partial class ContactMgmt : BaseUserControl
    {
        const string thisClassName = "ContactMgmt";
        MyManager myManager = new MyManager();
        Services.UtilityService utilService = new Services.UtilityService();
        string statusMessage = "";

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

        public int LocalOrgId
        {
            get { return Int32.Parse( this.currentOrgId.Text ); }
            set { this.currentOrgId.Text = value.ToString(); }
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
        /// <summary>
        /// Handle page load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        }

        protected void Page_PreRender( object sender, EventArgs e )
        {

            try
            {
  
                membersPager2.CurrentIndex = LastPageNumber;
                membersPager2.ItemCount = LastTotalRows;

                //check for change in orgId
                if ( LastOrgId == 0 )
                {
                    memberPanel.Visible = false;
                    litOrgTitle.Text = "";
                }
                else if ( LastOrgId > 0 && LastOrgId != LocalOrgId )
                {
                    //likely due to addition of new org
                    //can't add members yet, but maybe make members visible?
                    memberPanel.Visible = true;
                    addMemberPanel.Visible = false;
                    importPanel.Visible = false;
                    LocalOrgId = LastOrgId;
                    Get( LocalOrgId );
                }
            }
            catch
            {
                //no action
            }

        }//
        public void Get( int recId )
        {
            try
            {
                //get record
                Organization entity = MyManager.EFGet( recId );

                if ( entity == null || entity.IsValid == false )
                {
                    this.SetConsoleErrorMessage( "Sorry the requested record does not exist" );
                    return;

                }
                else
                {
                    litOrgTitle.Text = entity.Name;
                }

            }
            catch ( System.Exception ex )
            {
                //Action??		- display message and close form??	
                LoggingHelper.LogError( ex, thisClassName + ".Get() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );

            }

        }	// End method

        #region  ====== Members =============================================================

        protected void btnImportMembers_Click( object sender, EventArgs e )
        {
            memberPanel.Visible = false;
            memberImport.InitializeImport();
            importPanel.Visible = true;
        }

        protected void btnCloseImport_Click( object sender, EventArgs e )
        {
            memberPanel.Visible = true;
            importPanel.Visible = false;
        } //

        protected void btnAddMember_Click( object sender, EventArgs e )
        {
            memberPanel.Visible = false;
            addMemberPanel.Visible = true;
            ResetMember();
        } //
        protected void ResetMember()
        {
            lblUserId.Text = "0";
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtEmail.Text = "";
            txtConfirmEmail.Text = "";
            rblMemberType.SelectedIndex = 1;
            foreach ( ListItem item in cblOrgRoles.Items )
            {
                item.Selected = false;
            }
        } //
        protected void ShowMember( int id )
        {
            memberPanel.Visible = false;
            addMemberPanel.Visible = true;
            ResetMember();
            Patron user = AccountServices.GetUser( id );

            lblUserId.Text = user.Id.ToString();
            txtFirstName.Text = user.FirstName;
            txtLastName.Text = user.LastName;
            txtEmail.Text = user.Email;
            txtConfirmEmail.Text = user.Email;

            OrganizationMember ombr = OrganizationBizService.OrganizationMember_Get( LastOrgId, user.Id );

            this.SetListSelection( rblMemberType, ombr.OrgMemberTypeId.ToString() );
            //get roles
            MyManager.OrganizationMember_FillRoles( ombr );

            foreach ( OrganizationMemberRole role in ombr.MemberRoles )
            {
                foreach ( ListItem item in cblOrgRoles.Items )
                {
                    if ( role.RoleId.ToString() == item.Value )
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }


        } //

        protected void btnCloseAddPanel_Click( object sender, EventArgs e )
        {
            memberPanel.Visible = true;
            addMemberPanel.Visible = false;
        }

        protected void btnAddUser_Click( object sender, EventArgs e )
        {
            if ( ValidateUser() == true )
            {
                UpdateUser();

            }
        }
        protected bool UpdateUser()
        {
            bool isOk = true;
            bool isUpdate = false;
            OrganizationMember ombr = new OrganizationMember();
            string statusMessage = "";
            Patron user = new Patron();
            int id = 0;
            Int32.TryParse( lblUserId.Text, out id );

            if ( id == 0 )
            {
                user.UserName = txtEmail.Text;
                string password = "ChangeMe_" + System.DateTime.Now.Millisecond.ToString();
                user.Password = ILPathways.Utilities.UtilityManager.Encrypt( password );
            }
            else
            {
                user = AccountServices.GetUser( id );
                ombr = OrganizationBizService.OrganizationMember_Get( LastOrgId, user.Id );
                isUpdate = true;
            }
            user.FirstName = this.txtFirstName.Text.Trim();
            user.LastName = this.txtLastName.Text.Trim();
            user.Email = txtEmail.Text;

            if ( id == 0 )
            {
                id = new AccountServices().Create( user, ref statusMessage );
                if ( id > 0 )
                {
                    user.Id = id;
                    //CreateOrgMember( user );
                    UpdateOrgMember( user, ombr );

                    SetConsoleSuccessMessage( string.Format( userAddConfirmation.Text, user.FullName() ) );
                }
                else
                {
                    //??
                    SetConsoleErrorMessage( "There was a problem creating the account. System admin has been notified." );
                    EmailManager.NotifyAdmin( "Problem creating a quick account from the invitation process", "Don't know why, but: "
                        + "<br/>email: " + user.Email
                        + "<br/> Name: " + user.FullName()
                        + "<br/>Status Msg: " + statusMessage
                        );
                }
            }
            else
            {
                new AccountServices().Update( user );
                //check org mbr chg
                UpdateOrgMember( user, ombr );
                SetConsoleSuccessMessage( "Successfully updated " + user.FullName() );
            }

            //also create profile (default org),
            if ( user.OrgId == 0 )
            {
                PatronProfile prof = new PatronProfile();
                prof.UserId = user.Id;
                prof.OrganizationId = LastOrgId;
                new AccountServices().PatronProfile_Create( prof, ref statusMessage );
            }
            return isOk;
        }

        protected void NotifyNewUser( Patron user )
        {
            string statusMessage = "";
            string toEmail = user.Email;
            string bcc = UtilityManager.GetAppKeyValue( "systemAdminEmail", "mparsons@siuccwd.com" );
            string fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", "mparsons@siuccwd.com" );

            //if ( this.doingBccOnRegistration.Text == "no" )
            //    bcc = "";
            //
            bool isSecure = false;
            string eMessage = "";
            if ( UtilityManager.GetAppKeyValue( "SSLEnable", "0" ) == "1" )
                isSecure = true;
            string proxyId = new AccountServices().Create_ProxyLoginId( user.Id, "User added from org.", ref statusMessage );

            string subject = string.Format( noticeSubject.Text, WebUser.FullName() );
            eMessage = string.Format( this.noticeEmail.Text, litOrgTitle.Text, rblMemberType.SelectedItem.Text );

            string confirmUrl = string.Format( this.activateLink.Text, proxyId.ToString() );
            confirmUrl = UtilityManager.FormatAbsoluteUrl( confirmUrl, isSecure );

            string acctCreated = string.Format( this.acctCreatedMsg.Text, user.Email, "Change password on login", confirmUrl );
            eMessage += acctCreated;

            eMessage += " <p>" + WebUser.EmailSignature() + "</p>";

            EmailManager.SendEmail( toEmail, fromEmail, subject, eMessage, "", bcc );
        }

        protected void UpdateOrgMember( Patron user, OrganizationMember currentOmbr )
        {
            string statusMessage = "";
            int orgMbrId = currentOmbr.Id;
            // OrganizationMember om = new OrganizationMember();

            int memberTypeId = Int32.Parse( rblMemberType.SelectedValue );
            if ( memberTypeId != currentOmbr.OrgMemberTypeId )
            {
                currentOmbr.UserId = user.Id;
                currentOmbr.OrgId = LastOrgId;
                currentOmbr.OrgMemberTypeId = memberTypeId;
                currentOmbr.LastUpdatedById = WebUser.Id;

                if ( currentOmbr.OrgMemberTypeId == 0 )
                {
                    currentOmbr.CreatedById = WebUser.Id;
                    orgMbrId = MyManager.OrganizationMember_Create( currentOmbr, ref statusMessage );
                    currentOmbr.Id = orgMbrId;
                }
                else
                {
                    MyManager.OrganizationMember_Update( currentOmbr );
                }
            }


            if ( orgMbrId > 0 )
            {
                HandleOrgMbrRoles( currentOmbr );

            }
        } //
        private void HandleOrgMbrRoles( OrganizationMember orgMbr )
        {
            //get current roles
            //might be better to delete all and then add, otherwise we need a balance line
            MyManager.OrganizationMember_FillRoles( orgMbr );

            OrganizationMemberRole role = new OrganizationMemberRole();
            string statusMessage = "";

            //handle changes
            foreach ( OrganizationMemberRole role2 in orgMbr.MemberRoles )
            {
                bool roleFound = false;
                foreach ( ListItem item in cblOrgRoles.Items )
                {
                    if ( role2.RoleId.ToString() == item.Value )
                    {
                        if ( item.Selected == true )
                        {
                            roleFound = true;
                            break;
                        }
                    }
                }

                if ( roleFound == false )
                {
                    //delete role
                    MyManager.OrganizationMemberRole_Delete( role2.Id, ref statusMessage );
                }
            }
            //handle new
            foreach ( ListItem item in this.cblOrgRoles.Items )
            {
                if ( item.Selected )
                {
                    bool isNew = true;
                    foreach ( OrganizationMemberRole role3 in orgMbr.MemberRoles )
                    {
                        if ( role3.RoleId.ToString() == item.Value )
                        {
                            isNew = false;
                            break;
                        }
                    }

                    if ( isNew == true )
                    {
                        role = new OrganizationMemberRole();
                        role.OrgMemberId = orgMbr.Id;
                        role.RoleId = Int32.Parse( item.Value );
                        role.CreatedById = WebUser.Id;
                        if ( orgMbr.Id > 0 )
                        {
                            int id = MyManager.OrganizationMemberRole_Create( role, ref statusMessage );
                        }
                    }
                }
            }


        }

        protected bool ValidateUser()
        {
            bool isValid = true;
            if ( this.txtEmail.Text.Trim().Length < 10 )
            {
                SetConsoleErrorMessage( "Error: please enter a valid email address" );
                isValid = false;
            }
            else
            {
                bool alreadyExists = false;
                string inputEmail = txtEmail.Text;
                inputEmail = utilService.ValidateEmail( txtEmail.Text, ref isValid, ref statusMessage, ref alreadyExists );
                if ( !isValid )
                {
                    SetConsoleErrorMessage( statusMessage );
                    isValid = false;
                }
                else
                {
                    if ( txtEmail.Text.Trim().ToLower() != txtConfirmEmail.Text.Trim().ToLower() )
                    {
                        SetConsoleErrorMessage( "Error: Confirmation email does not match email" );
                    }
                }
            }
            if ( this.txtLastName.Text.Trim().Length < 2 )
            {
                SetConsoleErrorMessage( "Error: please enter a last name (at least two letters)" );
                isValid = false;
            }
            if ( this.txtFirstName.Text.Trim().Length < 1 )
            {
                SetConsoleErrorMessage( "Error: please enter a first name/Initial" );
                isValid = false;
            }
            return isValid;
        }


        protected void searchLink_Click( object sender, EventArgs e )
        {
            this.DoMembersSearch();
        }

        private void DoMembersSearch()
        {
            int selectedPageNbr = 0;
            string sortTerm = GetMembersSortTerm();
            membersPager2.ItemCount = 0;

            DoMembersSearch( selectedPageNbr, sortTerm );
        } //

        /// <summary>
        /// Conduct a search while addressing current page nbr and a sort term
        /// </summary>
        /// <param name="selectedPageNbr"></param>
        /// <param name="sortTerm"></param>
        private void DoMembersSearch( int selectedPageNbr, string sortTerm )
        {
            memberPanel.Visible = true;
            if ( selectedPageNbr == 0 )
            {
                //with custom pager, need to start at 1
                selectedPageNbr = 1;
            }
            LastPageNumber = selectedPageNbr;
            membersPager2.CurrentIndex = selectedPageNbr;

            // Set the page size for the DataGrid control based on the selection
            CheckForMembersPageSizeChange();

            int pTotalRows = 0;

            string filter = FormatMembersFilter();

            List<OrganizationMember> list = MyManager.OrganizationMember_Search( filter, sortTerm, selectedPageNbr, membersPager2.PageSize, ref pTotalRows );
            membersPager2.ItemCount = pTotalRows;

            LastTotalRows = pTotalRows;
            ddlMembersPageSizeList.Visible = false;
            if ( list == null || list.Count == 0 )
            {
                SetConsoleInfoMessage( "No records were found for the provided search criteria" );
                membersGrid.Visible = false;
                membersPager2.Visible = false;
            }
            else
            {
                membersGrid.Visible = true;
                ddlMembersPageSizeList.Visible = true;

                if ( pTotalRows > membersGrid.PageSize )
                {
                    //membersGrid.PagerSettings.Visible = true;
                    membersPager2.Visible = true;
                }
                else
                {
                    membersPager2.Visible = false;
                }

                //populate the grid
                membersGrid.DataSource = list;
                //membersGrid.PageIndex = selectedPageNbr;
                membersGrid.DataBind();

            }
        }


        protected string FormatMembersFilter()
        {
            string filter = "";
            string booleanOperator = "AND";

            filter = string.Format( " OrgId = {0} ", LastOrgId );

            if ( ddlFilterMemberType.SelectedIndex > 0 )
            {
                int mbrTypeId = Int32.Parse( this.ddlFilterMemberType.SelectedValue.ToString() );
                filter += MyManager.FormatSearchItem( filter, "OrgMemberTypeId", mbrTypeId, booleanOperator );
            }

            //keyword
            string keywordFilter = "";
            if ( txtMemberKeyword.Text.Trim().Length > 0 )
            {
                //updated to check code or title for filter
                keywordFilter = LDBM.HandleApostrophes( FormHelper.CleanText( txtMemberKeyword.Text ) );
                keywordFilter = keywordFilter.Replace( "*", "%" );
                if ( keywordFilter.IndexOf( "%" ) == -1 )
                {
                    keywordFilter = "%" + keywordFilter + "%";
                }

                string where = " (FirstName like '" + keywordFilter + "'	OR LastName like '" + keywordFilter + "') ";
                filter += MyManager.FormatSearchItem( filter, where, booleanOperator );
            }
            return filter;
        }

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
                ShowMember( ID );
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

                if ( myManager.OrganizationMember_Delete( recordId ) )
                {
                    this.SetConsoleSuccessMessage( "Successfully removed member from this organization." );
                    //OK reset list
                    DoMembersSearch();
                }
                else
                {
                    // problem
                    this.SetConsoleErrorMessage( "An unexpected issue was encountered while attempting to delete this record. System administration has been notified:<br/> " + statusMessage );
                    LoggingHelper.LogError( thisClassName + ".DeleteRecord() - Delete failed for org member id of " + recordId.ToString() + " and returned the following message:<br/>" + statusMessage, true );
                }

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".DeleteRecord() - Unexpected error encountered while attempting to delete org member id of " + recordId.ToString() );
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
            string sortTerm = GetMembersSortTerm();

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


        private string GetMembersSortTerm()
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

        protected void PopulateOrgMemberTypes( DropDownList ddl )
        {
            PopulateOrgMemberTypes( ddl, false );
        }	// End 
        protected void PopulateOrgMemberTypes( DropDownList ddl, bool selectFirst )
        {

            List<CodeItem> codes = MyManager.OrgMemberType_Select();
            if ( codes != null && codes.Count > 0 )
            {
                //Select an Member Type
                LDBM.PopulateList( ddl, codes, "Id", "Title", "Select a Member Type" );
                if ( selectFirst )
                {
                    ddl.SelectedIndex = 1;
                }

            }
        }	// End 

        protected void PopulateOrgMemberTypesList()
        {

            List<CodeItem> codes = MyManager.OrgMemberType_Select(); ;
            if ( codes != null && codes.Count > 0 )
            {
                //Select an Member Type
                LDBM.PopulateList( rblMemberType, codes, "Id", "Title", "" );
                rblMemberType.SelectedIndex = 1;

            }
        }	// End 

        #region Paging related methods
        public void memberPager_Command( object sender, CommandEventArgs e )
        {
            int currentPageIndx = Convert.ToInt32( e.CommandArgument );
            membersPager2.CurrentIndex = currentPageIndx;
            string sortTerm = GetMembersSortTerm();

            DoMembersSearch( currentPageIndx, sortTerm );
        }
        /// <summary>
        /// Initialize page size list and check for a previously set size
        /// </summary>
        private void InitializeMemberPageSizeList()
        {
            SetMembersPageSizeList();

            //Set page size based on user preferences
            int defaultPageSize = 25;
            this.membersGrid.PageSize = defaultPageSize;

            membersPager2.PageSize = defaultPageSize;
            this.SetListSelection( this.ddlMembersPageSizeList, defaultPageSize.ToString() );

        } //
        private void SetMembersPageSizeList()
        {
            CodeTableBizService.PopulateGridPageSizeList( ref this.ddlMembersPageSizeList );
        } //
        /// <summary>
        /// Check if page size preferrence has changed and update session variable if appropriate
        /// </summary>
        private void CheckForMembersPageSizeChange()
        {
            int index = ddlMembersPageSizeList.SelectedIndex;
            if ( index > 0 )
            {
                int size = Convert.ToInt32( ddlMembersPageSizeList.SelectedItem.Text );
                if ( membersGrid.PageSize != size )
                {
                    membersGrid.PageSize = size;
                    //pager1.PageSize = size;
                    membersPager2.PageSize = size;
                }
            }
        } //

        /// <summary>
        /// Handle change to page size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void MembersPageSizeList_OnSelectedIndexChanged( object sender, System.EventArgs ea )
        {
            // Set the page size for the DataGrid control based on the selection
            int index = ddlMembersPageSizeList.SelectedIndex;
            if ( index > 0 )
            {
                //need to reset to first page as current pg nbr may be out of range
                membersGrid.PageIndex = 0;
                //retain curent sort though
                string sortTerm = GetMembersSortTerm();

                DoMembersSearch( membersGrid.PageIndex, sortTerm );
            }
        }


        #endregion

        #endregion 

        private void PopulateControls()
        {
            //List<CodeItem> list = MyManager.OrgType_Select();
            //LDBM.PopulateList( this.ddlOrgType, list, "Id", "Title", "Select an org. type" );
            //InitializePageSizeList();
            //
            PopulateMemberControls();
        }

        private void PopulateMemberControls()
        {
            InitializeMemberPageSizeList();

            PopulateOrgMemberTypes( ddlFilterMemberType );

            PopulateOrgMemberTypesList();

            List<CodeItem> list = MyManager.OrgMemberRole_Select();

            LDBM.PopulateList( cblOrgRoles, list, "Id", "Title", "" );
        }
    }
}