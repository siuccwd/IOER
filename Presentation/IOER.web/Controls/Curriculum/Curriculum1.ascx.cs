using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Ionic.Zip;

using Isle.BizServices;
using AcctManager = Isle.BizServices.AccountServices;
using OrgManager = Isle.BizServices.OrganizationBizService;
using ILPathways.Controllers;
using ILPathways.Library;
using ILPathways.Business;
using ILPathways.Utilities;
using LRB = LRWarehouse.Business;

namespace ILPathways.Controls.Curriculum
{

    public partial class Curriculum1 : BaseUserControl
    {
        public string nodes { get; set; }
        public int CurrentRecordID { get; set; }
        public string userGUID { get; set; }
        public string startingNodeId { get; set; }

        protected void Page_Load( object sender, EventArgs e )
        {
            this.InitializeForm();

            SocialBox.shareData = new { id = CurrentRecordID };
        }

        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        private void InitializeForm()
        {
            userGUID = "var userGUID =\"\";";
            startingNodeId = "var startingNodeId = 0;";
            nodes = "var nodes = { data: null, isValid: false, status: \"There was an error loading the Curriculum\", extra: null };";

            CurrentRecordID = 0;
            if ( IsUserAuthenticated() )
            {
                CurrentUser = GetAppUser();
                userGUID = "var userGUID = \"" + WebUser.RowId.ToString() + "\";";

                if ( CurrentUser.ParentOrgId == -1 )
                {
                    CurrentUser.ParentOrgId = 0;
                    //get has not been attempted, do now
                    string statusMessage = string.Empty;
                    Organization org = OrgManager.GetOrganization( CurrentUser, ref statusMessage );
                    if ( org != null && org.Id > 0 )
                    {
                        CurrentUser.ParentOrgId = org.ParentId;
                    }
                    //update session
                    this.WebUser = CurrentUser;
                }
            }

            //hide share box if displaying as a widget
            if ( Request.RawUrl.ToLower().IndexOf( "/widgets/" ) > -1 )
            {
              shareBox.Visible = false;
            }

            try
            {
                string rid = this.GetRequestKeyValue( "rid", "" );
                
                string _author = "";
                string _org = "";
                string request = Request.RawUrl;

                if ( Page.RouteData.Values.ContainsKey( "RouteID" ) )
                {
                    string _routeID = "";
                    if ( Page.RouteData.Values.Count > 0 )
                        _routeID = Page.RouteData.Values[ "RouteID" ].ToString();
                    else
                        _routeID = "";
                    if ( _routeID.Length > 0 && IsInteger( _routeID ) )
                    {
                        CurrentRecordID = Int32.Parse( _routeID );
                    }
                    else if ( _routeID.Length == 36 )
                    {
                        rid = _routeID;
                    }
                }
                //not handling rowId yet
                //if ( rid.Trim().Length == 36 )
                //{
                //    this.Get( rid );
                //}
                //else
                //{
                if ( CurrentRecordID == 0 )
                {
                    CurrentRecordID = this.GetRequestKeyValue( "cidx", 0 );
                }

                if ( CurrentRecordID > 0 )
                {
                    //bool reloadingNode = this.GetRequestKeyValue( "reloadNode", false );
                    //if ( reloadingNode )
                    //{
                    //    //reload node, then redirect back

                    //   // new ILPathways.Services.DisplayJSONNode( "", CurrentRecordID, false );
                    //}
                    startingNodeId = "var startingNodeId = " + CurrentRecordID.ToString() + ";";
                   // startingNodeId = CurrentRecordID.ToString();

                    ContentServices mbr = new ContentServices();
                    ContentItem entity = mbr.Get( CurrentRecordID );
                    if ( entity.TypeId != ContentItem.CURRICULUM_CONTENT_ID )
                    {
                        //check for actual curriculum
                        int topId = mbr.GetTopIdForHierarchy( entity );
                        if ( topId > 0 )
                        {
                            CurrentRecordID = topId;
                        }
                    }
                    //var targetID = 0;
                    //int.TryParse( Request.Params[ "id" ], out targetID );
                    nodes = "var nodes = " + new ILPathways.Services.CurriculumService().GetTree( CurrentRecordID ) + ";"; //2176, 2207

                    //Get( CurrentRecordID );

                }
                //}
                if ( CurrentRecordID == 0 )
                {
                    //parent should handle this
                    //SetConsoleErrorMessage( "Invalid page request" );
                }
            }
            catch( Exception ex )
            {
                nodes = "var nodes = " + new Services.UtilityService().ImmediateReturn( null, false, ex.Message, null ) + ";";
                //parent should handle this
                //SetConsoleErrorMessage( "Invalid ID specified" );
                return;
            }


        }	// End 

       
    }
}