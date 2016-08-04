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
	public class Organization : Management
	{
		//Get all users
		public override List<ObjectMember> GetAllUsers( int id )
		{
			var users = OrganizationBizService.OrganizationMember_GetAll( id );
			var results = GetObjectMembers( users, true );

			return results;
		}

		//Get invited/pending users
		public override List<ObjectMember> GetPendingUsers( int objectID )
		{
			var users = OrganizationBizService.OrganizationMember_GetPending( objectID );
			var results = GetObjectMembers( users, false );

			return results;
		}

		//Get member types
		public override List<CodeItem> GetMemberTypes( int objectID, IWebUser user )
		{
			var list = new List<CodeItem>();
			if ( user != null && user.Id > 0 )
			{
				//Need to update this to only return the types that the user is allowed to give to other users
				return OrganizationBizService.OrgMemberType_Select();
			}
			return list;
		}

		//Get member roles
		public override List<CodeItem> GetMemberRoles( int objectID, IWebUser actingUser )
		{
			return OrganizationBizService.OrgMemberRole_Select();
		}

		//Invite a new user
		public override bool InviteUser( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status )
		{
			//Check to see if the actingUser has authority to invite a new user
			OrganizationMember actingOrgMbr = OrganizationBizService.OrganizationMember_Get( input.ObjectId, actingUser.Id );
			if ( actingOrgMbr.IsAdministration()
				|| actingOrgMbr.HasAdministratorRole()
				|| actingOrgMbr.HasAccountAdministratorRole() )
			{

			}
			else
			{
				status = "You are not authorized to invite users.";
				return false;
			}

			
			//Check to see if the user exists
			var targetUser = new AccountServices().GetByEmail(input.TargetUserEmail);

			//If so, add them
			if ( targetUser != null && targetUser.Id > 0 )
			{
				var result = new OrganizationBizService().InviteExistingUser( input.ObjectId, actingUser.Id, targetUser.Id, input.TargetMemberTypeId, input.TargetMemberRoleIds, input.Message, ref status );
				return result;
			}
			//Otherwise, invite new
			else
			{
				var result = new OrganizationBizService().InviteNewUser( input.ObjectId, 
					actingUser.Id, 
					input.TargetUserEmail, 
					input.TargetFirstName, input.TargetLastName, 
					input.TargetMemberTypeId, 
					input.TargetMemberRoleIds, 
					input.Message, ref status );
				return result;
			}
		
		}

		//Add an existing user directly
		public override bool AddExistingUser( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status )
		{
			//Check to see if the actingUser has authority to add the existing user (will be conveyed by input.TargetUserId)

			//Do the add

			//Return the result
			throw new NotImplementedException();
		}

		//Update a user role
		public override bool UpdateUserMemberType( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status )
		{
			//Data
			var orgService = new OrganizationBizService();
			var targetUser = OrganizationBizService.OrganizationMember_Get( input.ObjectId, input.TargetUserId );
			OrganizationBizService.OrganizationMember_FillRoles( targetUser );

			//Check to see if the actingUser has authority to update target user role to selected role (ie ensure contributors can't add admins)
			OrganizationMember actingOrgMbr = OrganizationBizService.OrganizationMember_Get( input.ObjectId, actingUser.Id );
			if ( actingOrgMbr.IsAdministration()
				|| actingOrgMbr.HasAdministratorRole()
				|| actingOrgMbr.HasAccountAdministratorRole()
				|| actingUser.TopAuthorization < 5 )
			{

			}
			else
			{
				status = "You are not authorized to update users.";
				return false;
			}
			//Check to see if the user is modifying their own role - may need special handling for this

			//Ensure that the user's action won't leave the object without an owner/admin
			//==> prob do a check in the remove method?
			//NA: we don't want the last admin to be able to make themselves a moderator

			//Do the update
			//Update Member Type
			targetUser.OrgMemberTypeId = input.TargetMemberTypeId;
			orgService.OrganizationMember_Update( targetUser );
			
			//Update Member Roles
			var roles = OrganizationBizService.OrgMemberRole_Select();
			//For each role in the code table...
			foreach ( var item in roles )
			{
				//See if the member already has it
				var targetRole = targetUser.MemberRoles.Where( m => m.RoleId == item.Id ).FirstOrDefault();
				var hasRole = targetRole != null;
				//See if the member should have it
				var shouldHaveRole = input.TargetMemberRoleIds.Contains( item.Id );

				//If the user has it and should have it, do nothing
				//If the user doesn't have it and shouldn't have it, do nothing
				//If the user doesn't have it, but should have it, add it
				if ( !hasRole && shouldHaveRole )
				{
					var role = new OrganizationMemberRole()
					{
						OrgMemberId = targetUser.Id,
						RoleId = item.Id,
						CreatedById = actingUser.Id,
					};
					var rowID = orgService.OrganizationMemberRole_Create( role, ref status );
					if ( rowID == 0 )
					{
						//Status already set
						return false;
					}
				}
				//If the user has it and shouldn't have it, remove it
				if ( hasRole && !shouldHaveRole )
				{
					var success = orgService.OrganizationMemberRole_Delete( targetRole.Id, ref status );
					if ( !success )
					{
						//Status already set
						return false;
					}
				}
			}

			//Return the result
			return true;
		}

		//Remove a user
		public override bool RemoveUser( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status )
		{
			var orgService = new OrganizationBizService();
			var targetUser = OrganizationBizService.OrganizationMember_Get( input.ObjectId, input.TargetUserId );
			OrganizationMember actingOrgMbr = OrganizationBizService.OrganizationMember_Get( input.ObjectId, actingUser.Id );

			//Check to see if the actingUser has authority to remove the selected user (ie moderator can't remove admins)
			if ( actingOrgMbr.IsAdministration() 
				|| actingOrgMbr.HasAdministratorRole()
				|| actingOrgMbr.HasAccountAdministratorRole()
				|| actingUser.TopAuthorization < 5)
			//if ( actingUserLevel > targetUser.OrgMemberTypeId )
			{
				//Check to see if the user is removing themself - may need special handling for this
				//actually don't care, only care if removing admin

				//Ensure that the user's action won't leave the object without an owner/admin

				//Do the update
				orgService.OrganizationMember_Delete( input.ObjectId, input.TargetUserId );
			}
			else
			{
				status = "You are not authorized to remove that user.";
				return false;
			}
			//Return the result
			return true;
		}

		//Helper method to convert object types
		private List<ObjectMember> GetObjectMembers( List<OrganizationMember> input, bool fillRoles )
		{
			var results = new List<ObjectMember>();
			foreach ( var item in input )
			{
				//Fill out member data
				var result = new ObjectMember()
				{
					Id = item.Id,
					UserId = item.UserId,
					FirstName = item.FirstName,
					LastName = item.LastName,
					Email = item.Email,
					MemberImageUrl = item.ImageUrl,
					MemberType = item.OrgMemberType,
					MemberTypeId = item.OrgMemberTypeId,
					Organization = item.Organization,
					MemberRoles = item.Roles,
					LastLoginDate = item.LastLoginDate
				};

				//Add roles
				if ( fillRoles )
				{
					OrganizationBizService.OrganizationMember_FillRoles( item );
					foreach ( var role in item.MemberRoles )
					{
						result.Roles.Add( new CodeItem()
						{
							Id = role.RoleId,
							Title = role.RoleTitle,
							IsActive = true
						} );
					}
				}

				//Add the result
				results.Add( result );
			}

			return results;
		}


	}
}