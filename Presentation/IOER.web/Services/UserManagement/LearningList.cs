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
	public class LearningList : Management
	{
		//Get all users
		public override List<ObjectMember> GetAllUsers( int id )
		{
			return CurriculumServices.Learninglist_AllUsers( id );
		}

		//Get member types
		public override List<CodeItem> GetMemberTypes( int objectID, IWebUser user )
		{
			var list = new List<CodeItem>();
			if (user != null && user.Id > 0)
			{
				//get associated role
				ContentPartner cp = ContentServices.ContentPartner_Get( objectID, user.Id );
                if ( cp != null && cp.UserId > 0 )
                {
                    list = ContentServices.GetCodes_ContentPartnerType( cp.PartnerTypeId );
                }
                else
                {
                    //user doesn't have a type. Probably an admin user, as others should not get access
                    list = ContentServices.GetCodes_ContentPartnerType( 3 );
                }
			}
			return list;
		}

		//Invite a new user
		public override bool InviteUser( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status )
		{
			//Check to see if the actingUser has authority to invite a new user
			bool skippingCheck = true;
			if ( skippingCheck )
			{
				//If so, do the add
				int id = new CurriculumServices().Content_AddNewPartner( input.ObjectId, input.TargetUserEmail, input.TargetFirstName, input.TargetLastName, input.TargetMemberTypeId, actingUser.Id, input.Message, ref status );
				//Return the result
				return id > 0;
			}
			else
			{
				//Return an error message
				status = "You do not have permission to invite a new user.";
				return false;
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
            status = "Successful";
            //Check to see if the actingUser has authority to update user role to selected role (ie ensure contributors can't add admins)
            ContentPartner cp = ContentServices.ContentPartner_Get(input.ObjectId, actingUser.Id);
            if ( cp == null || cp.PartnerTypeId < input.TargetMemberTypeId)
            {
                status = "You are Not allowed to set this role.";
                return false;
            }

			//Check to see if the user is modifying their own role - may need special handling for this

			//Ensure that the user's action won't leave the object without an owner/admin
            //==> prob do a check in the remove method?
			//NA: we don't want the last admin to be able to make themselves a moderator

			//Do the update
            if (new ContentServices().ContentPartner_Update(input.ObjectId, input.TargetUserId, input.TargetMemberTypeId, actingUser.Id, ref status) == false)
            {
                if (string.IsNullOrWhiteSpace(status))
                    status = "Sorry update seems to have failed.";

                return false;
            }

			//Return the result
            return true;
		}

		//Remove a user
		public override bool RemoveUser( UserManagementService.UserManagementInput input, IWebUser actingUser, ref string status )
		{
            status = "Successful";

			//Check to see if the actingUser has authority to remove the selected user (ie moderator can't remove admins)
            ContentPartner cp = ContentServices.ContentPartner_Get(input.ObjectId, actingUser.Id);
            if (cp == null || cp.PartnerTypeId < 3)
            {
                status = "You are Not allowed to remove a user.";
                return false;
            }
			//Check to see if the user is removing themself - may need special handling for this
            //actually don't care, only care if removing admin
            ContentPartner entity = ContentServices.ContentPartner_Get(input.ObjectId, input.TargetUserId);

			//Ensure that the user's action won't leave the object without an owner/admin
            if (entity.PartnerTypeId == 4)
            {
                List<ContentPartner> list = ContentServices.ContentPartner_GetAll(input.ObjectId, 4);
                if (list.Count < 2)
                {
                    status = "There is only one administrator for this list. <br/>You are not allowed to remove the last admininstrator. ";
                    return false;
                }
            }
			//Do the update
            if (new ContentServices().ContentPartner_Delete(entity.Id, ref status) == false)
            {
                if (string.IsNullOrWhiteSpace(status))
                    status = "Sorry the remove seems to have failed.";

                return false;
            }

            //Return the result
            return true;
		}


	}
}