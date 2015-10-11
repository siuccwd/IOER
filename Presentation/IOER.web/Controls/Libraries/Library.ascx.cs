using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;

using Isle.BizServices;
using IOER.Library;
using ILPathways.Business;
using Services = IOER.Services;
using ILPathways.Utilities;

using ILPathways.Common;
using Item = ILPathways.Business.DataItem;
using ThisUser = LRWarehouse.Business.Patron;
using DataBaseHelper = LRWarehouse.DAL.BaseDataManager;
using IOER.Services;

namespace IOER.Controls.Libraries
{
    public partial class Library : BaseUserControl
    {
        public string libInfoString { get; set; }
        public string userGUID { get; set; }
        public string proxyId { get; set; }
        public int libraryID { get; set; }
        public string linkedCollectionID { get; set; }
        public string libraryGuid { get; set; }
        public string collectionGuid { get; set; }
        public string currentFilters;
        public string activityJSON { get; set; }

        /// <summary>
        /// Handle page load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load( object sender, EventArgs e )
        {
            //Prevent duplicate page loads - still not sure what causes this or how to fix it
            if ( Request.Url == Request.UrlReferrer && !Page.IsPostBack ) { return; }

            if ( !Page.IsPostBack )
            {
                var serializer = new JavaScriptSerializer();
                libInfoString = serializer.Serialize( GetLibrary() );

                activityJSON = serializer.Serialize(
                  new ActivityBizServices().ActivityTotals_Library(
                    libraryID,
                    DateTime.Now.AddDays( -1 * int.Parse( activityDaysAgo.Text ) ),
                    DateTime.Now
                  ).FirstOrDefault() ?? new Isle.DTO.HierarchyActivityRecord()
                );
            }
        }

        /// <summary>
        /// check for requested library or collection
        /// </summary>
        /// <returns></returns>
        public ESLibrary2Service.LibraryData GetLibrary()
        {

            //Attempt to get the user
            userGUID = IsUserAuthenticated() ? WebUser.RowId.ToString() : "";

            proxyId = IsUserAuthenticated() ? WebUser.ProxyId : "0";
            libraryID = 0;
            //If available, get the preselected Collection ID
            //It will be used in the ESLibrary.js file
            linkedCollectionID = GetCollectionId();

            //Determine what to show
            if ( Request.RawUrl.ToLower() == "/my/library" )  //User wants their Library
            {
                return GetMyLibrary();
            }

            libraryID = FormHelper.GetRouteKeyValue( Page, "libID", 0 );
            if ( libraryID > 0 )
                return GetLibrary( libraryID );
            else
            {
                libraryID = GetRequestKeyValue( "id", 0 );
                if ( libraryID > 0 )
                    return GetLibrary( libraryID );
            }


            //at this point, a library was not found
            if ( linkedCollectionID == "0" )
                PrintError( "Sorry, a valid Library ID has not been provided." );
            return null;
        }

        /// <summary>
        /// If available, get the preselected Collection ID
        /// It will be used in the ESLibrary.js file
        /// </summary>
        /// <returns></returns>
        private string GetCollectionId()
        {
            int colId = GetRequestKeyValue( "col", 0 );
            //string colId = Request.Params[ "col" ] == null ? "0" : Request.Params[ "col" ];

            colId = FormHelper.GetRouteKeyValue( Page, "colID", 0 );

            return colId.ToString();
        }

        private ESLibrary2Service.LibraryData GetMyLibrary()
        {
            if ( userGUID == "" ) //Not logged in
            {
                PrintError( "Please Login to access your Library." );
                return null;
            }

			var user = ( ThisUser ) WebUser;
            //It's probably better to replace the next line with one that gets the user's current Library ID (and use that in the GetAllLibraryData method), but that ID seems to always be 0)
            var library = new Isle.BizServices.LibraryBizService().GetMyLibrary( user );
            if ( !library.IsValid || library.Id == 0 ) //User doesn't have a library yet
            {
                if ( FormHelper.GetRequestKeyValue( "action" ) == "autoCreate" )
                {
                    string statusMessage = "";
                    new LibraryBizService().CreateDefaultLibrary( user, ref statusMessage );
                    Response.Redirect( "/My/Library", true );
                }
                else
                {
                    PrepareNewLibrary();

                    return null;
                }
            }
            var data = new ESLibrary2Service().GetAllLibraryData( user, library );
            libraryID = library.Id;
            settingsPanel.Visible = data.isMyLibrary || data.hasEditAccess;
            settingsTab.Visible = data.isMyLibrary || data.hasEditAccess;
            addResourcesPanel.Visible = data.isMyLibrary || data.hasEditAccess;
            addTab.Visible = data.isMyLibrary || data.hasEditAccess;
            joinLibraryPanel.Visible = !data.isMyLibrary;

            joinTab.Visible = !data.isMyLibrary && data.allowJoinRequest;

            //Read code tables for access levels
            if ( data.hasEditAccess )
            {
                SetupDDLs();
            }

            return data;


        } ///

        private ESLibrary2Service.LibraryData GetLibrary( int libId )
        {
            try
            {
                var data = new Services.ESLibrary2Service().GetAllLibraryData( userGUID, libId );
                settingsPanel.Visible = data.isMyLibrary || data.hasEditAccess;
                settingsTab.Visible = data.isMyLibrary || data.hasEditAccess;
                addResourcesPanel.Visible = data.isMyLibrary || data.hasEditAccess;
                addTab.Visible = data.isMyLibrary || data.hasEditAccess;
                joinLibraryPanel.Visible = !data.isMyLibrary;

                joinTab.Visible = !data.isMyLibrary && data.allowJoinRequest;

                //Read code tables for access levels
                if ( data.hasEditAccess )
                {
                    SetupDDLs();
                }

                return data;
            }
            catch ( UnauthorizedAccessException uaex )
            {
                SetConsoleErrorMessage( "You do not have access this library" );
                libraryStuff.Visible = false;
                //searchControl.Visible = false;

                var errData = new ESLibrary2Service.LibraryData();
                return errData;
            }
        }

        public void SetupDDLs()
        {
            //Read code tables for access levels
            ddlPublicAccessLevels.Items.Clear();
            ddlOrganizationAccessLevels.Items.Clear();

            var list = LibraryBizService.GetCodes_LibraryAccessLevel();
            foreach ( CodeItem item in list )
            {
                var insert = new ListItem()
                {
                    Value = item.Id.ToString(),
                    Text = item.Title
                };
                ddlPublicAccessLevels.Items.Add( insert );
                ddlOrganizationAccessLevels.Items.Add( insert );
            }

        }

        public void PrepareNewLibrary()
        {
            libraryStuff.Visible = false;
            //searchControl.Visible = false;

            noLibraryYet.Visible = true;
            error.Visible = false;
            txtTitleNew.Text = WebUser.FullName() + "'s Library";

            List<CodeItem> list = Isle.BizServices.LibraryBizService.GetCodes_LibraryAccessLevel();

            DataBaseHelper.PopulateList( this.ddlPublicAccessLevel, list, "Id", "Title", "" );
            if ( list.Count > 2 )
                ddlPublicAccessLevel.SelectedIndex = 2;
            else
                ddlPublicAccessLevel.SelectedIndex = 0;
            DataBaseHelper.PopulateList( this.ddlOrgAccessLevel, list, "Id", "Title", "" );
            if ( list.Count > 3 )
                ddlOrgAccessLevel.SelectedIndex = 3;
            else
                ddlOrgAccessLevel.SelectedIndex = 0;
        }

        public void BtnCreateLibrary_Click( object sender, EventArgs e )
        {
            //Validation
            if ( !IsUserAuthenticated() )
            {
                SetConsoleErrorMessage( "You must be logged in to create a Library." );
                return;
            }
            bool valid = true;
            string status = "";
            ValidateText( txtTitleNew.Text, 10, "Title", ref valid, ref status );
            ValidateText( txtDescriptionNew.Text, 20, "Description", ref valid, ref status );
            if ( !fileNewImage.HasFile )
            {
                valid = false;
                status = status + " You must select an image.";
            }
            if ( !valid )
            {
                SetConsoleErrorMessage( "Error: " + status );
                return;
            }
            try
            {
                ILPathways.Business.Library library = new ILPathways.Business.Library();
                library.Title = this.txtTitleNew.Text;
                library.Description = this.txtDescriptionNew.Text;
                library.ImageUrl = ""; //done later
                library.RowId = Guid.NewGuid();
                library.IsActive = true;
                //library.IsPublic = true;

                if ( ddlPublicAccessLevel.SelectedIndex >= 0 )
                    library.PublicAccessLevel = ( EObjectAccessLevel ) Int32.Parse( ddlPublicAccessLevel.SelectedValue );
                else
                    library.PublicAccessLevel = EObjectAccessLevel.ReadOnly;


                if ( ddlOrgAccessLevel.SelectedIndex >= 0 )
                    library.OrgAccessLevel = ( EObjectAccessLevel ) Int32.Parse( ddlOrgAccessLevel.SelectedValue );
                else
                    library.OrgAccessLevel = EObjectAccessLevel.ContributeWithApproval;

                //personal
                library.LibraryTypeId = 1;
                library.CreatedById = WebUser.Id;

                //Do the create
                status = "";
                var libService = new Isle.BizServices.LibraryBizService();
                int id = libService.Create( library, ref status );
                var defaultCollection = libService.LibrarySection_CreateDefault( id, ref status );

                //Add avatars
                var avatarSystem = new My.Avatar();
                avatarSystem.UpdateAvatar( fileNewImage, library, new LibrarySection() ); //Create the avatar for library
                avatarSystem.UpdateAvatar( fileNewImage, library, defaultCollection ); //Use the same avatar for the default collection
                SetConsoleSuccessMessage( completionMessage.Text );

                Response.Redirect( "/My/Library", true );
            }
            catch ( System.Threading.ThreadAbortException taex )
            {
                //ignore, from redirect
                LoggingHelper.DoTrace( 5, "%%%%% ESLibrary2.BtnCreateLibrary_Click: " + taex.Message );
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "ESLibrary2.BtnCreateLibrary_Click()" );
            }
        }

        public string ValidateText( string text, int minimum, string name, ref bool valid, ref string status )
        {
            text = FormHelper.CleanText( text );
            if ( BadWordChecker.CheckForBadWords( text ) )
            {
                valid = false;
                status = status + " Inappropriate Language found.";
            }
            if ( text.Length < minimum )
            {
                valid = false;
                status = status + " " + name + " is too short.";
            }

            return text;
        }

        public void PrintError( string message )
        {
            libraryStuff.Visible = false;
            //searchControl.Visible = false;

            error.Visible = true;
            error.InnerHtml = message;
        }

        public class jsonUsableFilters
        {
            public List<int> accessRights;
            public List<int> endUser;
            public List<int> groupType;
            public List<int> edUse;
            public List<int> careerCluster;
            public List<int> gradeLevel;
            public List<int> resourceType;
            public List<int> mediaType;
        }

        protected void quickLibrary_Click( object sender, EventArgs e )
        {
            if ( IsUserAuthenticated() )
            {
                string statusMessage = "";
                new LibraryBizService().CreateDefaultLibrary( WebUser, ref statusMessage );
                SetConsoleSuccessMessage( libraryCreateMsg.Text );
                Response.Redirect( "/My/Library" );
            }
        }

    }
}