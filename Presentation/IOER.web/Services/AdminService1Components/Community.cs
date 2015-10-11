using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using LRWarehouse.Business;
using IOER.Services;
using Isle.BizServices;
using Patron = LRWarehouse.Business.Patron;

namespace IOER.Services.AdminService1Components
{
  public class Community : IManageObject
  {
    CommunityServices service = new CommunityServices();

    //Get methods
    public List<Privilege> ListPrivileges()
    {
      throw new NotImplementedException( "Not implemented yet!" );
    }
    public List<UserDTO> ListAllMembers( int manageID )
    {
      throw new NotImplementedException( "Sorry, community member listing is not implemented yet." );
    }
    public List<UserDTO> ListMembers( int manageID, int privilegeID )
    {
      throw new NotImplementedException( "Not implemented yet!" );
    }
    public List<UserDTO> ListPendingInvitations( int manageID )
    {
      throw new NotImplementedException( "Sorry, listing pending invitations is not implemented yet." );
    }

    //Set methods
    public Valid Member_UpdateMembership( int manageID, Patron user, int memberID, int privilegeID )
    {
      var result = new Valid() { id = memberID };

      //Update status
      //TODO: make something updateable
      result.extra = new { title = "Member", error = "" };
      result.text = "Successfully updated privileges.";

      return result;
    }
    public Valid Member_ApproveMembership( int manageID, Patron user, int memberID, int privilegeID, string customMessage )
    {
      var result = new Valid() { id = memberID };

      if ( service.Community_MemberAdd( manageID, memberID ) > 0 )
      {
        result.valid = true;
        result.text = "Membership approved. Click \"Membership Requests\" to refresh.";
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

      //Do the removal
      if ( service.Community_MemberDelete( manageID, memberID ) )
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