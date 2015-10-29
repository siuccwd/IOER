using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Utilities;
using IOER.Library;
using System.Web.Script.Serialization;
//using ILPathways.DAL;
using Isle.BizServices;
using Isle.DTO;
using ILPathways.Business;
using IPB = ILPathways.Business;
using MyManager = Isle.BizServices.ResourceBizService;

using LRWarehouse.Business;

using MyLibraryManager = Isle.BizServices.LibraryBizService;

namespace IOER.LRW.controls
{
    public partial class Detail6 : BaseUserControl
    {
        public string userGUID { get; set; }
        public int resourceIntID { get; set; }
        public int resourceVersionID { get; set; }
        public string resourceText { get; set; }
        public string codeTables { get; set; }
        public bool isUserAdmin { get; set; }
        public bool isUserAuthor { get; set; }
        public bool alreadyLoaded { get; set; }

		ResourceVersion pResourceVersion = new ResourceVersion();

        protected void Page_Load( object sender, EventArgs e )
        {
          if ( !IsPostBack )
          {
            InitRecord();
            InitButtons();
          }
          alreadyLoaded = true;
        }

        protected void InitRecord()
        {
            var userGUID = "";
            if ( IsUserAuthenticated() )
            {
                userGUID = WebUser.RowId.ToString();
                //DoLibraryChecks( WebUser );
            }
            else
            {
                //check for claim
                IsWNUserAuthorized();
            }

            if ( IsValidRecord() )
            {
                GetResource( resourceVersionID );
            }
            else
            {
                content.Visible = false;
                error.Visible = true;
            }
        }
        private bool IsWNUserAuthorized()
        {
            bool isValid = false;
            try
            {
                //var identity = ( System.Security.Claims.ClaimsIdentity ) HttpContext.User.Identity;
                //if ( identity != null && identity.Claims != null )
                //{
                //    var roles = identity.Claims.Where( w => w.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                //                                    && ( w.Value == "Program Administrator" || w.Value == "Program Staff" ) )
                //                                    .ToList();
                //    if ( roles != null && roles.Count > 0 )
                //        isValid = true;
                //}
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "IsWNUserAuthorized" );
            }

            return isValid;
        }
        protected void GetResource( int resourceVersionID )
        {
            
            content.Visible = true;
            error.Visible = false;
            var serializer = new JavaScriptSerializer();
            if ( userGUID == null )  //Kludge fix for this getting dropped somehow
            {
              if ( IsUserAuthenticated() )
              {
                userGUID = WebUser.RowId.ToString();
              }
              else
              {
                userGUID = "";
              }
            }

            resourceText = "var resource = " + serializer.Serialize( new Services.DetailService6().LoadAllResourceData( resourceVersionID, userGUID ) ) + ";";
            codeTables = "var codeTables = " + serializer.Serialize( new Services.DetailService6().GetCodeTables() ) + ";";
        }

        protected bool IsValidRecord()
        {
            
            resourceVersionID = FormHelper.GetRequestKeyValue( "vid", 0 );
            if ( Page.RouteData.Values.Count > 0
             && Page.RouteData.Values.ContainsKey( "RouteVID" ) )
            {
                resourceVersionID = int.Parse( Page.RouteData.Values[ "RouteVID" ].ToString() );

            } else if ( Page.RouteData.Values.Count > 0
             && Page.RouteData.Values.ContainsKey( "RouteRID" ) )
            {
                resourceIntID = int.Parse( Page.RouteData.Values[ "RouteRID" ].ToString() );
                ResourceVersion entity = MyManager.ResourceVersion_GetByResourceId( resourceIntID );
                if ( entity != null && entity.Id > 0 )
                    resourceVersionID = entity.Id;
            }
            else if (resourceVersionID == 0)
            {
                int rId = FormHelper.GetRequestKeyValue( "rid", 0 );
                //446014
                if ( rId > 0 )
                {
                    ResourceVersion entity = MyManager.ResourceVersion_GetByResourceId( resourceIntID );
                    if ( entity != null && entity.Id > 0 )
                        resourceVersionID = entity.Id;
                }
            }

            if ( resourceVersionID > 0 )
            {
                try
                {

                    resourceIntID = 0;
					//or use display version to primarily get keywords to feed meta tags
                    pResourceVersion = MyManager.ResourceVersion_Get( resourceVersionID );
                    var isActive = false;
                    if ( pResourceVersion != null && pResourceVersion.Id > 0 )
                    {
                        resourceIntID = pResourceVersion.ResourceIntId;
                        isActive = pResourceVersion.IsActive;

                        int libId = FormHelper.GetRequestKeyValue( "libId", 0 );
                        int colId = FormHelper.GetRequestKeyValue( "colId", 0 );
                        ActivityBizServices.ResourceHit( resourceIntID, pResourceVersion.Title, libId, colId, WebUser );

						SetMetaTags( pResourceVersion.Description );
						SetMetaTags( pResourceVersion.Title, "keywords" );

                    }

                    return ( resourceVersionID != 0 && isActive );
                }
                catch ( Exception ex )
                {
                    return false;
                }
            }
            else
            {
                litResourceId.Text = resourceIntID.ToString();
            }
           
            return false;
        }
       
        protected void InitButtons()
        {
            btnStartUpdateMode.Visible = false;
            reportProblemContainer.Visible = false;
            btnReportProblem.Visible = false;
            btnFinishUpdate.Visible = false;
            btnDeactivateResource.Visible = false;
            btnCancelChanges.Visible = false;
            btnRegenerateThumbnail.Visible = false;
            btnUbertag.Visible = false;

            if ( IsUserAuthenticated() )
            {
                reportProblemContainer.Visible = true;

                //get general privileges
                FormPrivileges = SecurityManager.GetGroupObjectPrivileges( WebUser, txtFormSecurityName.Text );
                if ( FormPrivileges.CanUpdate() == false &&  txtGeneralSecurity.Text == "" )
                {
                    //anyone can update
                    FormPrivileges.SetOrgPrivileges();
                }

                isUserAuthor = false;
                //check if was the author, (or, next, if from same org)
                //ObjectMember mbr = MyManager.GetResourceAccess( resourceIntID, WebUser.Id );
                //if ( mbr.MemberTypeId > 2 )
                //{
                //    //the following is not implemented yet. Will enable an author to update title and description
                //    isUserAuthor = true;
                //    isUserAdmin = true;
                //    //be sure to not lower the privilegs
                //    if ( FormPrivileges.CreatePrivilege < ( int ) EPrivilegeDepth.Region )
                //    {
                //        //set to region, not sure if enough?
                //        FormPrivileges.SetBasicPrivileges(3);
                //    }
                //}
                bool canEdit = MyManager.CanUserEditResource( resourceIntID, WebUser.Id );
                if ( canEdit )
                {
                    //Will enable an author to update title and description
                    isUserAuthor = true;
                    //first seeing if setting as an admin is OK, or if gives to much privilege
                    isUserAdmin = true;
                    //be sure to not lower the privilegs
                    if ( FormPrivileges.CreatePrivilege < ( int ) EPrivilegeDepth.Region )
                    {
                        //set to region, not sure if enough?
                        FormPrivileges.SetBasicPrivileges( 3 );
                    }
                }

                if ( FormPrivileges.CanUpdate() )
                {
					btnUbertag.Visible = true;
					btnUbertag.Attributes[ "onclick" ] = "window.location.href = '/tagger?theme=ioer&mode=tag&resourceID=" + resourceIntID + "'";

                    btnStartUpdateMode.Visible = true;
                    btnFinishUpdate.Visible = true;
                    btnReportProblem.Visible = true;
                    btnCancelChanges.Visible = true;
                }

				if ( canEdit )
				{
					btnDeactivateResource.Visible = true;
				}

                if ( FormPrivileges.CreatePrivilege > ( int )EPrivilegeDepth.Region )
                {
                   
                    btnDeactivateResource.Visible = true;
                    btnReActivateResource.Visible = true;
					if ( pResourceVersion != null && pResourceVersion.Id > 0
						&& string.IsNullOrWhiteSpace( pResourceVersion.ResourceImageUrl ) == true )
					{
						btnRegenerateThumbnail.Visible = true;
					}
                   
                    litResourceId.Visible = true;
                    isUserAdmin = true;
                }
            }
        }



        protected void btnReActivateResource_Click( object sender, EventArgs e )
        {
            if ( IsUserAuthenticated() )
            {
                userGUID = WebUser.RowId.ToString();
                //DoLibraryChecks( WebUser );
            }

            //set active and then retrieve
            string statusMessage = "";
            int resourceIntID = 0;
            if ( Int32.TryParse( litResourceId.Text, out resourceIntID ) )
            {
                if ( resourceIntID > 0 )
                {
                    ResourceVersion entity = MyManager.ResourceVersion_GetByResourceId( resourceIntID, false );
                    if ( entity != null && entity.Id > 0 )
                        resourceVersionID = entity.Id;

                    if ( MyManager.ResourceVersion_SetActive( resourceVersionID, ref statusMessage ) )
                    {
                        //retrieve 
                        GetResource( resourceVersionID );
                    }
                }
                else
                {
                    SetConsoleErrorMessage( "Error a valid resource id was not found " );
                }
            }
            else
            {
                SetConsoleErrorMessage( "Error a valid resource version id was not found " );
            }
        }
      
    }
}