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
using ResMgr = Isle.BizServices.AccountServices;
using OrgMgr = Isle.BizServices.OrganizationBizService;
using ILPathways.Common;
using ILPathways.Library;
using ILPathways.Utilities;
using IDBM = ILPathways.DAL.DatabaseManager;
//using DBM = LRWarehouse.DAL.DatabaseManager;
using LRWarehouse.Business;

namespace ILPathways.Controls.Libraries
{
    public partial class LibraryAdminManager : BaseUserControl
    {
        MyMgr mgr = new MyMgr();

        #region Properties

        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string FormSecurityName
        {
            get { return this.formSecurityName.Text; }
            set { this.formSecurityName.Text = value; }
        }

        #endregion
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( IsUserAuthenticated() == false )
            {
                SetConsoleErrorMessage( "Error: you must be authenticated in order to use this page.<br/>Please login and try again." );
                authStyles.Text = "<style type=\"text/css\">.authStyles{display:none;}</style>";
                formContainer.Visible = false;
                return;
            }

            if ( !Page.IsPostBack )
            {
                this.InitializeForm();
            }
        }
        private void InitializeForm()
        {
            //formSecurityName
            //==> actually first check for any admin libs
            //this is prob to allow admin accss to all libs
            this.FormPrivileges = GDAL.SecurityManager.GetGroupObjectPrivileges( this.WebUser, FormSecurityName );
            if ( CurrentUser.Username == "mparsons" )
            {
                FormPrivileges.SetAdminPrivileges();
            }
            if ( FormPrivileges.CreatePrivilege >= (int) ILP.EPrivilegeDepth.Region)
            {
                //show link to load all libraries
                showAllLibraries.Visible = true;
                //SetConsoleErrorMessage( "Error: you do not have access to the selected page.<br/>" );
                //formContainer.Visible = false;
                //Response.Redirect( "/", true );
                //return;
            }
            //
            bool hasLibs = ShowAllowedLibraries();
            bool hasOrgAdmin = ShowAllowedOrgs();

            if ( hasLibs == false )
            {
                //check if has org, don't allow for perssonal until has personal
                if ( WebUser.OrgId > 0 )
                {
                    //check if org administator
                    //OrgMbrId in (select OrgContactId from [dbo].[Organization.MemberRole] where roleId in (1, 3)) '
                }
                else
                {
                    SetConsoleErrorMessage( "Note: You must be the administrator of a library, or have a user library in order to use this page.<br/>Please set up your <a href='/My/Library'>user library</a> or request administrator access to another library and then return to this page." );
                    formContainer.Visible = false;
                }

            if ( hasLibs == false )
            {
                //check if has org, don't allow for perssonal until has personal
                if ( WebUser.OrgId > 0 )
                {
                    //check if org administator
                    //OrgMbrId in (select OrgContactId from [dbo].[Organization.MemberRole] where roleId in (1, 3)) '
                }
                else
                {
                    SetConsoleErrorMessage( "Note: You must be the administrator of a library, or have a user library in order to use this page.<br/>Please set up your <a href='/My/Library'>user library</a> or request administrator access to another library and then return to this page." );
                    formContainer.Visible = false;
                }
            }
            }

        }

        protected bool ShowAllowedLibraries()
        {
            bool hasLibraries = false;
            DataSet ds = mgr.Libraries_SelectWithEditAccess( WebUser.Id );
            //DataSet ds = mgr.LibrarySearch( "lib.IsActive = 1", "lib.Title", 1, 1000, ref totalRows );
            if ( DoesDataSetHaveRows( ds ) )
            {
                hasLibraries = true;
                IDBM.PopulateList( this.sourceLibrary, ds, "Id", "title", "Select a Source Library" );
            }
            return hasLibraries;

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
                    IDBM.PopulateList( this.ddlOrgs, codes, "Id", "Title", "Select an Organization" );
                    this.orgsPanel.Visible = true;
                }
            }

            return hasOrgs;
        }
        protected void membersGrid_RowCommand( object sender, GridViewCommandEventArgs e )
        {

        }

        protected void membersGrid_PageIndexChanging( object sender, GridViewPageEventArgs e )
        {

        }

        protected void membersGrid_Sorting( object sender, GridViewSortEventArgs e )
        {

        }

        protected void newInvitationLink_Click( object sender, EventArgs e )
        {

        }

        protected void showInvitations_Click( object sender, EventArgs e )
        {

        }



        protected void searchLink_Click( object sender, EventArgs e )
        {

        }

        protected void inviteStep2Link_Click( object sender, EventArgs e )
        {

        }

        protected void inviteStep3NoAcct_Click( object sender, EventArgs e )
        {

        }


        void PopulateControls()
        {
            //get editable libraries

            int totalRows = 0;
            //DataSet ds = mgr.Libraries_SelectWithEditAccess( WebUser.Id );
            DataSet ds = mgr.LibrarySearch( "lib.IsActive = 1", "lib.Title", 1, 1000, ref totalRows );
            IDBM.PopulateList( this.sourceLibrary, ds, "Id", "title", "Select a Source Library" );

        }

        protected void sourceLibrary_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( sourceLibrary.SelectedIndex > 0 )
            {
                int libId = Int32.Parse( sourceLibrary.SelectedValue );
                sourceLibraryId.Text = libId.ToString();
                DataSet ds = mgr.LibrarySectionsSelect( libId );
                IDBM.PopulateList( this.sourceCollection, ds, "Id", "title", "Select the Source Collection" );
            }
            else
            {
                sourceCollection.DataSource = null;
                sourceCollection.DataBind();
                collectionId.Text = "";
                sourceLibraryId.Text = "";
            }
        }

        protected void sourceCollection_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( sourceCollection.SelectedIndex > 0 )
            {
                collectionId.Text = sourceCollection.SelectedValue;
            }
            else
            {
                collectionId.Text = "";

            }
        }

        protected void showAllLibraries_Click( object sender, EventArgs e )
        {

        }

    }
}