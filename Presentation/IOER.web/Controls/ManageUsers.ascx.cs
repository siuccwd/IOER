using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;
using ILPathways.Business;
using LRWarehouse.Business;
using System.Web.Script.Serialization;
using IOER.Services;
using ILPathways.Common;
using Isle.BizServices;
using Isle.DTO;
using Patron = LRWarehouse.Business.Patron;

namespace IOER.Controls
{
	public partial class ManageUsers : BaseUserControl
	{
		//Properties
		public int ObjectId { get; set; }
		public string ObjectTitle { get; set; }
		public string ObjectTypeTitle { get; set; }
		public string ObjectType { get; set; } //e.g., "library", "learninglist", etc
		public List<CodeItem> MemberTypes { get; set; }
		public List<CodeItem> MemberRoles { get; set; }

		//JSON properties
		public string JsonMemberTypes { get; set; } //list of valid roles for this object, that the user is able to assign or manipulate

		//Variables
		JavaScriptSerializer serializer = new JavaScriptSerializer();
		UserManagementService service = new UserManagementService();

		//Page Load
		protected void Page_Load( object sender, EventArgs e )
		{
			//NOTE: Should checks to ensure the user can manage other users happen at this stage or earlier (i.e., in the control that is loading this control)?
			//Validate inputs
			//First, the user
			if ( IsUserAuthenticated() == false )
			{
				SetError( "You must login to manage items." );
				return;
			}
			//get current user
			CurrentUser = GetAppUser();
			//Now the object type
			if ( string.IsNullOrWhiteSpace( ObjectType ) )
			{
				SetError( "You must choose a type of item to manage." );
				return;
			}
			//And the object itself
			if ( ObjectId == 0 )
			{
				SetError( "You must choose an item to manage." );
				return;
			}

			//Next we figure out what roles the user can assign or manipulate
			MemberTypes = service.GetMemberTypes( new UserManagementService.UserManagementInput() { ObjectType = ObjectType, ObjectId = ObjectId } );
			MemberRoles = service.GetMemberRoles( new UserManagementService.UserManagementInput() { ObjectType = ObjectType, ObjectId = ObjectId } );
			JsonMemberTypes = serializer.Serialize( MemberTypes );

			//Add a type parameter to the div
			userManagerPanel.Attributes.Add( "data-type", ObjectType );
		}

		//Set an error message and hide the management panel
		private void SetError( string message )
		{
			userManagerError.Visible = true;
			userManagerPanel.Visible = false;
			userManagerErrorMessage.InnerHtml = message;
		}

	}


}