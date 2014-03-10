using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Utilities;
using ILPathways.Library;
using System.Web.Script.Serialization;
using ILPathways.DAL;
using ILPathways.Business;
using IPB = ILPathways.Business;
using LRWarehouse.DAL;
using LRWarehouse.Business;

using MyLibraryManager = Isle.BizServices.LibraryBizService;

namespace ILPathways.LRW.controls
{
    public partial class Detail6 : BaseUserControl
    {
        public string userGUID { get; set; }
        public int resourceIntID { get; set; }
        public int resourceVersionID { get; set; }
        public string resourceText { get; set; }
        public string codeTables { get; set; }
        public bool isUserAdmin { get; set; }
        public bool alreadyLoaded { get; set; }

        protected void Page_Load( object sender, EventArgs e )
        {
          if ( IsPostBack )
          {

          }
          else
          {
            InitRecord();
            InitUser();
            InitButtons();
          }
          alreadyLoaded = true;
        }

        protected void InitRecord()
        {
            if ( IsValidRecord() )
            {
                var userGUID = "";
                if ( IsUserAuthenticated() )
                {
                    userGUID = WebUser.RowId.ToString();
                    //DoLibraryChecks( WebUser );
                }
                content.Visible = true;
                error.Visible = false;
                var serializer = new JavaScriptSerializer();
                resourceText = "var resource = " + serializer.Serialize( new Services.DetailService6().LoadAllResourceData( resourceVersionID, userGUID ) ) + ";";
                codeTables = "var codeTables = " + serializer.Serialize( new Services.DetailService6().GetCodeTables() ) + ";";
            }
            else
            {
                content.Visible = false;
                error.Visible = true;
            }
        }
        protected bool IsValidRecord()
        {
            resourceVersionID = FormHelper.GetRequestKeyValue( "vid", 0 );
            if ( Page.RouteData.Values.Count > 0 )
            {
                resourceVersionID = int.Parse( Page.RouteData.Values[ "RouteVID" ].ToString() );
            }
            else if (resourceVersionID == 0)
            {
                int rId = FormHelper.GetRequestKeyValue( "rid", 0 );
                //446014
                if ( rId > 0 )
                {
                    ResourceVersion entity = new ResourceVersionManager().GetByResourceId( rId );
                    if ( entity != null && entity.Id > 0 )
                        resourceVersionID = entity.Id;
                }
            }

            if (resourceVersionID > 0)
            {
                try
                {
                    
                    resourceIntID = 0;
                    ResourceVersion entity = new ResourceVersionManager().Get( resourceVersionID );
                    var isActive = false;
                    if ( entity != null && entity.Id > 0 )
                    {
                        resourceIntID = entity.ResourceIntId;
                        isActive = entity.IsActive;
                    }

                    return ( resourceVersionID != 0 && isActive );
                }
                catch ( Exception ex )
                {
                    return false;
                }
            }
           
            return false;
        }
        protected void InitUser()
        {
            isUserAdmin = false;
            if ( IsUserAuthenticated() )
            {
                userGUID = WebUser.RowId.ToString();
            }
            else
            {
                userGUID = "";
            }
        }
        protected void InitButtons()
        {
            btnStartUpdateMode.Visible = false;
            reportProblemContainer.Visible = false;
            btnReportProblem.Visible = false;
            btnFinishUpdate.Visible = false;
            btnDeactivateResource.Visible = false;
            btnCancelChanges.Visible = false;

            if ( IsUserAuthenticated() )
            {
                FormPrivileges = SecurityManager.GetGroupObjectPrivileges( WebUser, txtFormSecurityName.Text );
                if ( FormPrivileges.CanUpdate() )
                {
                    btnStartUpdateMode.Visible = true;
                    btnFinishUpdate.Visible = true;
                    btnReportProblem.Visible = true;
                    reportProblemContainer.Visible = true;
                    btnCancelChanges.Visible = true;
                }

                if ( FormPrivileges.CreatePrivilege > ( int )EPrivilegeDepth.State )
                {
                    btnDeactivateResource.Visible = true;
                    isUserAdmin = true;
                }
            }
        }
      
    }
}