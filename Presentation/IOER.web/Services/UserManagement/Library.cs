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
	public class Library : Management
	{
		//Get all users
		public override List<ObjectMember> GetAllUsers( int id )
		{
			var data = new LibraryBizService().LibraryMembers_GetAll( id, 0 );
			return ConvertUsers( data, id );
		}

		//Get member types
		public override List<CodeItem> GetMemberTypes( int objectID, IWebUser user )
		{
			throw new NotImplementedException();
		}

		//Invite a new user
		public override bool InviteUser( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status )
		{
			throw new NotImplementedException();
		}

		//Add an existing user directly
		public override bool AddExistingUser( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status )
		{
			throw new NotImplementedException();
		}

		//Update a user role
		public override bool UpdateUserMemberType( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status )
		{
			throw new NotImplementedException();
		}

		//Remove a user
		public override bool RemoveUser( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status )
		{
			throw new NotImplementedException();
		}


		#region DTO Transmogrification
		private List<ObjectMember> ConvertUsers( List<LibraryMember> input, int objectID )
		{
			var output = new List<ObjectMember>();

			foreach ( var item in input )
			{
				output.Add( new ObjectMember()
				{
					Id = item.Id,
					ObjectId = objectID,
					Organization = item.Organization,
					OrgId = item.OrganizationId,
					UserId = item.Id,
					MemberTypeId = item.MemberTypeId,
					MemberType = item.MemberType,
					Created = item.Created,
					LastUpdated = item.LastUpdated,
					FirstName = item.MemberName,
					Email = item.MemberEmail,
					MemberImageUrl = item.MemberImageUrl,
					MemberHomeUrl = item.MemberHomeUrl
				} );
			}

			return output;
		}

		#endregion
	}
}