using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using LRWarehouse.Business;

namespace ILPathways.Services.AdminService1Components
{
  public interface IManageObject
  {
    #region Get methods
    //Get id/value pairs of privileges (code table style) for an object
    List<Privilege> ListPrivileges();
    //List all members for an object
    List<UserDTO> ListAllMembers( int manageID );
    //List a certain type of members
    List<UserDTO> ListMembers( int manageID, int privilegeID );
    //List pending invitations
    List<UserDTO> ListPendingInvitations( int manageID );
    #endregion

    #region Set methods
    //Update existing membership
    Valid Member_UpdateMembership( int manageID, Patron user, int memberID, int privilegeID );
    //Approve pending membership
    Valid Member_ApproveMembership( int manageID, Patron user, int memberID, int privilegeID, string customMessage );
    //Revoke existing memberships
    Valid Member_RevokeMembership( int manageID, Patron user, int memberID );
    //Deny pending memberships
    Valid Member_DenyMembership( int manageID, Patron user, int memberID, string customMessage );
    //Invite new members that already have accounts
    Valid Member_InviteExistingUser( int manageID, Patron user, Patron invitee, int privilegeID, string customMessage );
    //Invite new members that don't have accounts
    Valid Member_InviteNewUser( int manageID, Patron user, string validatedEmail, int privilegeID, string customMessage );
    #endregion
  }

  //ID-text pairs of privilege options, used for filling out and selecting from drop-down lists
  public class Privilege
  {
    public int id { get; set; }
    public string text { get; set; }
  }

  //"Valid" object. Used to convey information about a given operation's result for -each- item, rather than the operations as a whole
  //e.g. if an operation is attempted on 5 items and 2 error out, their respective Value objects should reflect what happened in each case
  //This allows an operation to be carried out for all valid items while conveying information about each individual operation/item back to the user regardless of success or failure
  //id is the id of the relevant object (usually a user ID). This is used to tie the Valid object back to something that exists in the user interface (e.g. to display a status for each user in a list)
  //valid is whether or not the operation was successful for this item (e.g. were the privileges for this specific user updated?)
  //text is any relevant textual information to convey to the user (e.g. "someone@site.com was invited successfully" or "blah@.@e is not a valid email address")
  //extra is a utility to send back any sort of object that the client may need for special use cases
  public class Valid
  {
    public int id { get; set; }
    public bool valid { get; set; }
    public string text { get; set; }
    public object extra { get; set; }
  }

  //User DTO to be returned by the methods that list users
  public class UserDTO
  {
    public int id { get; set; }
    public string name { get; set; }
    public string privileges { get; set; }
    public int privilegesID { get; set; }
    public string date { get; set; }
    public string imageURL { get; set; }
  }


  //Class to create the instance of the requested management object
  public static class ManageObjectConstructor
  {
    public static IManageObject GetInstance( string name )
    {
      //Store available management objects here
      List<Match> names = new List<Match>() 
      {
        new Match() { name = "organization", className = "Organization" },
        new Match() { name = "library", className = "Library" },
        new Match() { name = "community", className = "Community" },
        new Match() { name = "learninglist", className = "LearningList" }
      };

      //Find a match without needing to expose the class name directly to the client
      var service = names.Where( m => m.name == name.ToLower() ).FirstOrDefault();
      if(service == null)
      {
        return null;
      }

      //Create an instance of the target class
      try
      {
        var path = "ILPathways.Services.AdminService1Components." + service.className;

        return ( IManageObject ) Activator.CreateInstance( null, path ).Unwrap();
      }
      catch
      {
        return null;
      }
    }
    //Cleaner than dealing with tuples or keyvaluepairs
    private class Match
    {
      public string name { get; set; }
      public string className { get; set; }
    }
  }

}