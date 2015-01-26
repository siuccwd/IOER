using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using LRWarehouse.Business;
using ILPathways.Services;
using Isle.BizServices;

namespace ILPathways.Services.AdminService1Components
{
  public class LearningList : IManageObject
  {
    CurriculumServices service = new CurriculumServices();

    //Get methods
    public List<Privilege> ListPrivileges()
    {
      var data = CurriculumServices.GetCodes_ContentPartnerType();
      var results = new List<Privilege>();
      foreach ( var item in data )
      {
        results.Add( new Privilege()
        {
          id = item.Id,
          text = item.Title
        } );
      }
      return results;
      //throw new NotImplementedException( "Not implemented yet!" );
    }
    public List<UserDTO> ListAllMembers( int manageID )
    {
      var data = service.Learninglist_AllUsers( manageID );
      return GetUserDTOs( data );
    }
    public List<UserDTO> ListMembers( int manageID, int privilegeID )
    {
      //Need a more efficent way to get this
      var data = service.Learninglist_AllUsers( manageID ).Where( m => m.MemberTypeId == privilegeID ).ToList();
      return GetUserDTOs( data );
    }
    private List<UserDTO> GetUserDTOs( List<Isle.DTO.ObjectMember> data )
    {
      var results = new List<UserDTO>();
      foreach ( var item in data )
      {
        results.Add( new UserDTO()
        {
          id = item.UserId,
          name = item.MemberFullName,
          privileges = item.MemberType,
          privilegesID = item.MemberTypeId,
          date = item.Created.ToShortDateString(),
          imageURL = item.MemberImageUrl
        } );
      }
      return results;
    }
    public List<UserDTO> ListPendingInvitations( int manageID )
    {
      throw new NotImplementedException( "Sorry, listing pending invitations is not implemented yet." );
    }

    //Set methods
    public Valid Member_UpdateMembership( int manageID, Patron user, int memberID, int privilegeID )
    {
      //Do update
      var result = GetUpdatedMembership( manageID, memberID, privilegeID, user.Id );

      //Update status
      var code = ContentServices.GetCodes_ContentPartnerType().Where(m => m.Id == privilegeID).FirstOrDefault().Title;
      result.extra = new { title = code, error = "" };
      result.text = "Successfully updated privileges.";

      return result;
    }
    public Valid Member_ApproveMembership( int manageID, Patron user, int memberID, int privilegeID, string customMessage )
    {
      //Do update
      var result = GetUpdatedMembership( manageID, memberID, privilegeID, user.Id );

      //Update status
      result.text = "Membership approved. Click \"Membership Requests\" to refresh.";

      return result;
    }
    private Valid GetUpdatedMembership( int manageID, int memberID, int privilegeID, int userID )
    {
      var result = new Valid() { id = memberID };

      //Get the member
      var member = ContentServices.ContentPartner_Get( manageID, memberID );

      //Do the update
      member.PartnerTypeId = privilegeID;

      //Save
      if ( service.ContentPartner_Update( member ) )
      {
        result.valid = true;
      }
      else
      {
        throw new InvalidOperationException( "There was an error performing this action." );
      }
      return result;
    }
    public Valid Member_RevokeMembership( int manageID, Patron user, int memberID )
    {
      var status = "";
      var result = new Valid() { id = memberID };

      //Get the membership record
      var member = CurriculumServices.ContentPartner_Get( manageID, memberID );

      //Do the removal
      if ( service.ContentPartner_Delete( member.Id, ref status ) )
      {
        result.valid = true;
        result.text = "Member deleted. Click \"Current Members\" to refresh.";
        result.extra = new { error = "" };
      }
      else
      {
        throw new InvalidOperationException( status );
      }

      return result;
    }
    public Valid Member_DenyMembership( int manageID, Patron user, int memberID, string customMessage )
    {
      throw new NotImplementedException( "Not implemented yet!" );
    }
    public Valid Member_InviteExistingUser( int manageID, Patron user, Patron invitee, int privilegeID, string customMessage )
    {
      var result = new Valid() { id = invitee.Id };
      //service.InviteExistingUser(manageID, user, invitee, privilegeID, customMessage);

      //Temporary
      result.valid = false;
      result.text = "Sorry, this feature isn't implemented yet.";
      //End Temporary

      return result;
    }
    public Valid Member_InviteNewUser( int manageID, Patron user, string validatedEmail, int privilegeID, string customMessage )
    {
      var result = new Valid() { id = 0 }; //Keep this 0
      //service.InviteNewUser(manageID, user, validatedEmail, invitee, privilegeID, customMessage);

      //Temporary
      result.valid = false;
      result.text = "Sorry, this feature isn't implemented yet.";
      //End Temporary
      
      return result;
    }

  }
}