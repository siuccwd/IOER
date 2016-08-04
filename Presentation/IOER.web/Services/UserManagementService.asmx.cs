using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using IOER.classes;
using IOER.Library;
using ILPathways.Common;
using Isle.BizServices;
using System.Web.Script.Serialization;
using IPB = ILPathways.Business;
using Isle.DTO;
using LRWarehouse.Business;
using Patron = LRWarehouse.Business.Patron;

namespace IOER.Services
{
	/// <summary>
	/// Summary description for UserManagementService
	/// </summary>
	[WebService( Namespace = "http://tempuri.org/" )]
	[WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
	[System.ComponentModel.ToolboxItem( false )]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[System.Web.Script.Services.ScriptService]
	public class UserManagementService : System.Web.Services.WebService
	{
		JavaScriptSerializer serializer = new JavaScriptSerializer();
		UserManagement.Management manager = new UserManagement.Dummy();

		private void GetManager( string type )
		{
			switch ( type.ToLower() )
			{
				case "learninglist":
					manager = new UserManagement.LearningList();
					break;
				case "library":
					manager = new UserManagement.Library();
					break;
				case "organization":
					manager = new UserManagement.Organization();
					break;
				case "community":
					manager = new UserManagement.Dummy();
					break;
				default:
					manager = new UserManagement.Dummy();
					break;
			}
		}

		//Get a list of roles that the current object type has, limited to what the user can assign or manipulate for a specific object ID
		[WebMethod( EnableSession = true )]
		public string AjaxGetMemberTypes( UserManagementInput input )
		{
			try
			{
				var roles = GetMemberTypes( input );
				return Stringify( roles, null, true );
			}
			catch ( Exception ex )
			{
				return Fail( "There was a problem getting the roles for this object.", ex );
			}
		}
		public List<CodeItem> GetMemberTypes( UserManagementInput input )
		{
			GetManager( input.ObjectType );
			var user = GetUser();
			return manager.GetMemberTypes( input.ObjectId, user );
		}
		//

		//Get a list of titles that the current object type has, if any
		[WebMethod( EnableSession = true )]
		public string AjaxGetMemberRoles( UserManagementInput input )
		{
			try
			{
				var titles = GetMemberRoles( input );
				return Stringify( titles, null, true );
			}
			catch ( Exception ex )
			{
				return Fail( "There was a problem getting the titles for this object.", ex );
			}
		}
		public List<CodeItem> GetMemberRoles( UserManagementInput input )
		{
			GetManager( input.ObjectType );
			var user = GetUser();
			return manager.GetMemberRoles( input.ObjectId, user );
		}

		//Get a list of users associated with an object
		[WebMethod( EnableSession = true )]
		public string AjaxGetUsers( UserManagementInput input )
		{
			try
			{
				var result = GetUsers( input );
				return Stringify( result, null, true );
			}
			catch ( Exception ex )
			{
				return Fail( "There was a problem getting the list of users.", ex );
			}
		}
		public List<ObjectMember> GetUsers( UserManagementInput input )
		{
			GetManager( input.ObjectType );
			var users =  manager.GetAllUsers( input.ObjectId );
			users = users.OrderBy( m => m.LastName ).ToList();
			return users;
		}
		//

		//Get a list of invited/pending users
		[WebMethod( EnableSession = true )]
		public string AjaxGetPendingUsers( UserManagementInput input )
		{
			try
			{
				var result = GetPendingUsers( input );
				return Stringify( result, null, true );
			}
			catch ( Exception ex )
			{
				return Fail( "There was a problem getting the list of invited users", ex );
			}
		}
		public List<ObjectMember> GetPendingUsers( UserManagementInput input )
		{
			GetManager( input.ObjectType );
			return manager.GetPendingUsers( input.ObjectId );
		}
		//

		//Check to ensure an email is not taken
		[WebMethod]
		public string AjaxCanInviteUser( UserManagementInput input )
		{
			var status = "There was a problem checking for this user.";
			try
			{
				var alreadyExists = false;
				var result = CanInviteUser( input, ref status, ref alreadyExists );
				return Stringify( UtilityService.DoReturn( result, true, status, alreadyExists ), null, false );
			}
			catch ( Exception ex )
			{
				return Fail( status, ex );
			}
		}
		public bool CanInviteUser( UserManagementInput input, ref string status, ref bool alreadyExists )
		{
			GetManager(input.ObjectType);
			var user = GetUser();
			return manager.CanInviteUser( input, user, ref status, ref alreadyExists );
		}
		//

		//Invite a user to an object
		[WebMethod( EnableSession = true )]
		public string AjaxInviteUser( UserManagementInput input )
		{
			var status = "There was a problem inviting the user to this object.";
			try
			{
				var result = InviteUser( input, ref status );
				return Stringify( result, null, true );
			}
			catch ( Exception ex )
			{
				return Fail( status, ex );
			}
		}

		[WebMethod( EnableSession = true )]
		public string AjaxInviteOrgUser( UserManagementInput input )
		{
			var status = "There was a problem inviting the user to this object.";
			try
			{
				input.IsNameRequired = true;
				var result = InviteUser( input, ref status );
				return Stringify( result, null, true );
			}
			catch ( Exception ex )
			{
				return Fail( status, ex );
			}
		}
		public bool InviteUser(UserManagementInput input, ref string status)
		{
			GetManager( input.ObjectType );
			var user = GetUser();
			
			//Validate email
			var valid = true;
			var exists = false;
			status = "";
			if ( input.IsNameRequired )
			{
				if ( string.IsNullOrWhiteSpace( input.TargetFirstName ) )
					status = "First name must be entered\r\n";
				if ( string.IsNullOrWhiteSpace( input.TargetLastName ) )
					status += "Last name must be entered\r\n";
			}
			//validation should have occurred on the client
			input.TargetUserEmail = new UtilityService().ValidateEmail( input.TargetUserEmail, ref valid, ref status, ref exists );
			if ( !valid )
			{
				status += "Invalid email address.";
				//throw new InvalidOperationException( "Invalid email address" );
			}
			if ( status.Length > 6 && exists == false)
			{
				throw new InvalidOperationException( status );
			}
			return manager.InviteUser( input, user, ref status );
		}
		//

		//Add an existing user directly (no invite) - not sure if there's a use case for this
		[WebMethod( EnableSession = true )]
		public string AjaxAddExistingUser( UserManagementInput input )
		{
			var status = "There was a problem adding this user.";
			try
			{
				var result = AddExistingUser( input, ref status );
				return GetUsersOrFail( input, result, status );
			}
			catch ( Exception ex )
			{
				return Fail( status, ex );
			}
		}
		public bool AddExistingUser( UserManagementInput input, ref string status )
		{
			//Add user
			GetManager( input.ObjectType );
			var user = GetUser();
			return manager.AddExistingUser( input, user, ref status );
		}
		//

		//Update a user's role
		[WebMethod( EnableSession = true )]
		public string AjaxUpdateUserMemberType( UserManagementInput input )
		{
			var status = "There was a problem updating this user's role.";
			try
			{
				var result = UpdateUserMemberType( input, ref status );
				return GetUsersOrFail( input, result, status );
			}
			catch ( Exception ex )
			{
				return Fail( status, ex );
			}
		}
		public bool UpdateUserMemberType( UserManagementInput input, ref string status )
		{
			//Update user
			GetManager( input.ObjectType );
			var user = GetUser();
			return manager.UpdateUserMemberType( input, user, ref status );
		}
		//

		//Remove a user
		[WebMethod( EnableSession = true )]
		public string AjaxRemoveUser( UserManagementInput input )
		{
			var status = "There was a problem removing this user.";
			try
			{
				var result = RemoveUser( input, ref status );
				return GetUsersOrFail( input, result, status );
			}
			catch ( Exception ex )
			{
				return Fail( status, ex );
			}
		}
		public bool RemoveUser( UserManagementInput input, ref string status )
		{
			//Remove user
			GetManager( input.ObjectType );
			var user = GetUser();
			return manager.RemoveUser( input, user, ref status );
		}
		//

		#region helper methods

		//Return list of users if successful; else return error object
		private string GetUsersOrFail( UserManagementInput input, bool successful, string status )
		{
			if ( successful )
			{
				return AjaxGetUsers( input );
			}
			else
			{
				throw new Exception( "There was an error performing that action" );
			}
		}

		//Helper method to get a non-null user object
		public IPB.IWebUser GetUser()
		{
			return SessionManager.GetUserFromSession( Session ) ?? new Patron();
		}

		//Helper method for errors
		private string Fail( string friendlyMessage, Exception ex )
		{
			return serializer.Serialize( UtilityService.DoReturn( null, false, friendlyMessage, ex.Message ) );
		}

		//Helper method for returning data
		private string Stringify( object data, object extra, bool wrapWithGenericMessage )
		{
			if ( wrapWithGenericMessage )
			{
				return serializer.Serialize( UtilityService.DoReturn( data, true, "okay", extra ) );
			}
			else
			{
				return serializer.Serialize( data );
			}
		}

		#endregion

		//Helper class to make inputs easier
		public class UserManagementInput
		{
			public UserManagementInput()
			{
				TargetMemberRoleIds = new List<int>();
			}
			public int ObjectId { get; set; } //Target object ID
			public string ObjectType { get; set; } //e.g, learninglist, library, etc.
			public string TargetFirstName { get; set; }
			public string TargetLastName { get; set; }
			public int TargetUserId { get; set; } //User being operated on
			public string TargetUserEmail { get; set; } //Email being operated on
			public int TargetMemberTypeId { get; set; } //Member Type involved
			public string Message { get; set; } //Message to send to user being invited/removed/etc
			public List<int> TargetMemberRoleIds { get; set; } //Applicable member roles
			public bool IsNameRequired { get; set; }
		}
	}
}
