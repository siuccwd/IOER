using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Isle.DTO;
using Isle.BizServices;
using ILPathways.Common;
using ILPathways.Business;

namespace IOER.Services.UserManagement
{
	public class Dummy : Management
	{
		public override List<ObjectMember> GetAllUsers( int id )
		{
			return new List<ObjectMember>();
		}

		public override List<CodeItem> GetMemberTypes( int objectID, IWebUser user )
		{
			return new List<CodeItem>();
		}

		public override bool InviteUser( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status )
		{
			status = "not implemented";
			return true;
		}

		public override bool AddExistingUser( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status )
		{
			status = "not implemented";
			return true;
		}

		//Update a user role
		public override bool UpdateUserMemberType( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status )
		{
			status = "not implemented";
			return true;
		}

		//Remove a user
		public override bool RemoveUser( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status )
		{
			status = "not implemented";
			return true;
		}


	}
}