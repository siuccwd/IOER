using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IB = ILPathways.Business;
using IOER.Library;
using LRWarehouse.Business;
using Isle.DTO;
using LRWarehouse.Business.ResourceV2;
using Isle.BizServices;
using System.Web.Script.Serialization;
using IOER.Services;
using Patron = LRWarehouse.Business.Patron;

namespace IOER.Controls.DetailV7
{
	public partial class DetailV7 : BaseUserControl
	{
		public string ErrorMessage { get; set; }
		public ResourceDB Resource { get; set; }
		public UserPermissions Permissions { get; set; }
		public Patron User { get; set; }
		public List<DetailV7Service.LibColData> UserLibraries { get; set; }
		public string UserLibrariesJSON { get; set; }
		public string ResourceJSON { get; set; }
		public string ResourceLibrariesJSON { get; set; }
		public List<RubricV2> RubricsList { get; set; }

		JavaScriptSerializer serializer = new JavaScriptSerializer();
		DetailV7Service service = new DetailV7Service();

		protected void Page_Load( object sender, EventArgs e )
		{
			Resource = new ResourceDB();
		
			//Load Resource Data
			LoadData();

			//Load User Data
			User = IsUserAuthenticated() ? (Patron) WebUser : new Patron();
			GetUserPermissions();

			//Load Data that requires Resource and User Data
			GetLibraryData();

			ResourceJSON = serializer.Serialize( Resource );
		}

		public void LoadData()
		{
			//Get Resource ID
			try
			{
				var id = int.Parse( (string) Request.Params[ "resourceID" ] ?? (string) Page.RouteData.Values[ "resourceID" ] ?? "0" );
				Resource = new ResourceV2Services().GetResourceDB( id );
				RubricsList = new Isle.BizServices.RubricV2Services().GetRubricListWithEvaluationDataForResource( 0, 0, Resource.ResourceId );
				detailPage.Visible = true;
				errorPage.Visible = false;
				deactivatedPage.Visible = false;
			}
			//Deactivated resource
			catch ( InvalidOperationException ioex )
			{
				detailPage.Visible = false;
				errorPage.Visible = false;
				deactivatedPage.Visible = true;
				//If user can reactivate, show button
				BtnReactivateResource.Visible = true;
				return;
			}
			//Everything else
			catch ( Exception ex )
			{
				detailPage.Visible = false;
				errorPage.Visible = true;
				deactivatedPage.Visible = false;
				ErrorMessage = ex.Message;
				return;
			}

		}

		public void GetUserPermissions()
		{
			var privileges = SecurityManager.GetGroupObjectPrivileges( WebUser, txtFormSecurityName.Text );
			var anyoneCanUpdate = !privileges.CanUpdate() && txtGeneralSecurity.Text == "";
			var authorLevelAccess = ResourceBizService.CanUserEditResource(Resource.ResourceId, User.Id);
			var adminLevelAccess = privileges.CreatePrivilege > ( int ) IB.EPrivilegeDepth.Region;

			Permissions = new UserPermissions()
			{
				IsLoggedIn = IsUserAuthenticated(),
				CanRead = true,
				CanUpdate = anyoneCanUpdate || authorLevelAccess || adminLevelAccess,
				CanDelete = authorLevelAccess || adminLevelAccess,
				IsIOERAdmin = adminLevelAccess
			};
		}
		public class UserPermissions
		{
			public bool IsLoggedIn { get; set; }
			public bool CanRead { get; set; }
			public bool CanUpdate { get; set; }
			public bool CanDelete { get; set; }
			public bool IsIOERAdmin { get; set; }
		}

		public void GetLibraryData()
		{
			//Get library data for this user
			UserLibraries = service.GetUserLibraryData( Resource.ResourceId );
			UserLibrariesJSON = serializer.Serialize( UserLibraries );

			//Get library data for this resource
			ResourceLibrariesJSON = serializer.Serialize( service.GetResourceLibraryData( Resource.ResourceId ) );
		}


	}
}