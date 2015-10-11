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
	public abstract class Management
	{
		#region Abstract methods

		//TODO: change this to use paging
		public abstract List<ObjectMember> GetAllUsers( int objectID );

		public abstract List<CodeItem> GetMemberTypes( int objectID, IWebUser user );

		public abstract bool InviteUser( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status );

		public abstract bool AddExistingUser( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status );

		public abstract bool UpdateUserMemberType( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status );

		public abstract bool RemoveUser( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status );

		#endregion
		#region Methods with default implementations

		public virtual List<ObjectMember> GetPendingUsers( int objectID )
		{
			return new List<ObjectMember>();
		}

		public virtual List<CodeItem> GetMemberRoles( int objectID, IWebUser actingUser )
		{
			//Most things do not have titles, just roles
			return new List<CodeItem>();
		}

		public virtual bool CanInviteUser( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status, ref bool alreadyExists )
		{
			var valid = true;
			input.TargetUserEmail = new UtilityService().ValidateEmail( input.TargetUserEmail, ref valid, ref status, ref alreadyExists );
			if ( !valid )
			{
				return false;
			}
			//The above method performs an existence check but the below method may do it better? Not sure.
			//bool exists = new AccountServices().DoesUserEmailExist( input.TargetUserEmail );

			return true;
		}

		#endregion
	}
}