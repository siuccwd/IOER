using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using GDAL = Isle.BizServices;
using ILP=ILPathways.Business;
using MyMgr= Isle.BizServices.LibraryBizService;
using ResMgr = Isle.BizServices.AccountServices;
using ILPathways.Common;
using ILPathways.Library;
using ILPathways.Utilities;
using IDBM=ILPathways.DAL.DatabaseManager;
//using DBM = LRWarehouse.DAL.DatabaseManager;
using LRWarehouse.Business;

namespace ILPathways.Admin
{
    public partial class AdminStuff : BaseAppPage
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
       

        bool _canViewPrivileges = false;
        public bool CanViewPrivileges
        {
            get { return _canViewPrivileges; }
            set { _canViewPrivileges = value; }
        }

        #endregion
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( IsUserAuthenticated() == false )
            {
                SetConsoleErrorMessage( "Error: you must be authenticated in order to use this page.<br/>Please login and try again." );
                formContainer.Visible = false;
                return;
            }

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
            //formSecurityName
            this.FormPrivileges = GDAL.SecurityManager.GetGroupObjectPrivileges( this.WebUser, FormSecurityName );
            //if ( CurrentUser.TopAuthorization < 4 )
            //{
            //    FormPrivileges.SetAdminPrivileges();
            //}
            if ( FormPrivileges.CanCreate() == false)
            {
                SetConsoleErrorMessage( "Error: you do not have access to the selected page.<br/>" );
                formContainer.Visible = false;
                Response.Redirect( "/", true );
                //return;
            }
            //
            int totalRows = 0;
            //DataSet ds = mgr.Libraries_SelectWithEditAccess( WebUser.Id );
            DataSet ds = mgr.LibrarySearch( "lib.IsActive = 1", "lib.Title", 1, 1000, ref totalRows );
            IDBM.PopulateList( this.sourceLibrary, ds, "Id", "title", "Select a Source Library" );

            DataSet ds2 = mgr.Libraries_SelectWithEditAccess( WebUser.Id );
            IDBM.PopulateList( this.targetLibrary, ds2, "Id", "title", "Select a Target Library" );

            //ResMgr resMgr = new ResMgr();
            List<CodeItem> list = ResMgr.GetAllUsersAsCodes();

            PopulateList( this.ddlPersons, list, "Id", "Title", "Select a Person" );

        }
        public void PopulateList( DropDownList list, List<CodeItem> items, string dataValueField, string dataTextField, string selectTitle )
        {
            try
            {
                //clear current entries
                list.Items.Clear();

                if ( items.Count > 0 )
                {
                    int count = items.Count;
                    if ( selectTitle.Length > 0 )
                    {
                        // add select row
                        CodeItem hdr = new CodeItem();
                        hdr.Id = 0;
                        hdr.Title = selectTitle;
                        items.Insert( 0, hdr );
                    }
                    list.DataSource = items;
                    list.DataValueField = dataValueField;
                    list.DataTextField = dataTextField;
                    list.DataBind();
                    list.Enabled = true;
                    if ( selectTitle.Length > 0 )
                        list.SelectedIndex = 0;
                }
                else
                {
                    list.Items.Add( new ListItem( "No Selections Available", "" ) );
                    list.Enabled = false;
                }

            }
            catch ( Exception ex )
            {
                LogError( ex, "BaseDataManager.PopulateList( DropDownList list, DataSet ds, string " + dataValueField + ", string " + dataTextField + ", string selectTitle )" );
            }
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
                btnCopy.Enabled = false;
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
                btnCopy.Enabled = false;
            }
        }

        protected void ddlPersons_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( ddlPersons.SelectedIndex > 0 )
            {
                personId.Text = ddlPersons.SelectedValue;
                lblPerson.Text = ddlPersons.SelectedItem.Text;
                btnAddLibraryMbr.Enabled = true;
            }
            else
            {
                personId.Text = "";
                lblPerson.Text = "";
                btnAddLibraryMbr.Enabled = false;
            }
        }

        protected void targetLibrary_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( targetLibrary.SelectedIndex > 0 )
            {
                libraryId.Text = targetLibrary.SelectedValue;
                btnCopy.Enabled = false;
                btnDeleteLibrary.Enabled = false;
            }
            else
            {
                libraryId.Text = "";
                btnCopy.Enabled = false;
                btnDeleteLibrary.Enabled = false;
            }
        }

        protected void btnCopy_Click( object sender, EventArgs e )
        {
            if ( IsInteger( this.collectionId.Text ) == false )
            {
                SetConsoleErrorMessage( "Enter a valid collection id" );
                this.lblMessage.Text = "Enter a valid collection id";
                return;
            }

            if ( IsInteger( this.libraryId.Text ) == false )
            {
                SetConsoleErrorMessage( "Enter a valid library id" );
                this.lblMessage.Text = "Enter a valid library id";
                return;
            }


            string statusMessage = "";
            int newId = mgr.LibrarySectionCopy( int.Parse( collectionId.Text ), int.Parse( libraryId.Text ), WebUser.Id, ref statusMessage );
            SetConsoleSuccessMessage( statusMessage );
            this.lblMessage.Text = statusMessage;


        }


        protected void btnDeleteLibrary_Click( object sender, EventArgs e )
        {
            if ( IsInteger( this.libraryId.Text ) == false )
            {
                SetConsoleErrorMessage( "Enter/select a valid library id" );
                this.lblMessage.Text = "Enter a valid library id";
                return;
            }

            string statusMessage = "";
            bool result = mgr.LibraryDelete( int.Parse( libraryId.Text ), ref statusMessage );
            if ( result )
            {
                SetConsoleSuccessMessage( "Successfully deleted library" );
                libraryId.Text = "";
                btnCopy.Enabled = false;
                //btnDeleteLibrary.Enabled = false;
            }
            else
            {
                SetConsoleSuccessMessage( statusMessage );
            }

        }

        protected void btnAddLibraryMbr_Click( object sender, EventArgs e )
        {
            if ( IsInteger( this.personId.Text ) == false )
            {
                SetConsoleErrorMessage( "Enter a valid person id" );
                this.lblMessage.Text = "";
                return;
            }

            if ( IsInteger( this.sourceLibraryId.Text ) == false )
            {
                SetConsoleErrorMessage( "Enter a valid sourceLibraryId" );
                this.lblMessage.Text = "";
                return;
            }
            int libraryId = int.Parse( sourceLibraryId.Text );
            int userId = int.Parse( personId.Text );
            int mbrTypeId = int.Parse( ddlMemberType.SelectedValue);

            string statusMessage = "";
            if ( mgr.IsLibraryMember( libraryId, userId ) )
            {
                SetConsoleErrorMessage( "Already a libary member" );
            }
            else
            {
                if ( mgr.LibraryMember_Create( libraryId, userId, mbrTypeId, WebUser.Id, ref statusMessage ) > 0 )
                    SetConsoleSuccessMessage( statusMessage );
                else
                    SetConsoleErrorMessage( "Error encountered" );
            }

        }

    }
}