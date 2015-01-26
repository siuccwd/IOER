using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Isle.BizServices;

using LRWarehouse.Business;
using ILPathways.Services.AdminService1Components;
using JSON = ILPathways.Services.UtilityService.GenericReturn;


namespace ILPathways.Services
{
  /// <summary>
  /// Summary description for AdminService1
  /// </summary>
  [WebService( Namespace = "http://tempuri.org/" )]
  [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
  [System.ComponentModel.ToolboxItem( false )]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  [System.Web.Script.Services.ScriptService]
  public class AdminService1 : System.Web.Services.WebService
  {
    //Utility Service
    UtilityService utilService = new UtilityService();
    //Management object service
    IManageObject service;
    //Current user
    Patron user;
    //Status to avoid a bunch of ref variables
    string mainStatus;

    #region Get methods

    //Get privilege items for an object
    [WebMethod]
    public JSON ListPrivileges( string type )
    {
      try 
      {
        //Setup
        if ( !Initialize( type, false ) )
        {
          return Fail( mainStatus );
        }

        //Get privilege data
        var privileges = service.ListPrivileges();

        return Reply( privileges, true, "okay", type );
      }
      catch ( Exception ex )
      {
        return Reply( ex.StackTrace, false, ex.Message, null );
      }
    }

    //List all users for an object
    [WebMethod]
    public JSON ListUsers( string type, int manageID )
    {
      try
      {
        //Setup
        if ( !Initialize( type, false ) )
        {
          return Fail( mainStatus );
        }

        //Get the members
        var users = service.ListAllMembers( manageID );
        if ( users == null )
        {
          return Fail( "Error getting users" );
        }

        return Reply( users, true, "okay", type );
      }
      catch ( Exception ex )
      {
        return Reply( ex.StackTrace, false, ex.Message, null );
      }
    }

    //List pending members for an object
    [WebMethod]
    public JSON ListPendingMembers( string type, int manageID )
    {
      try 
      {
        //Setup
        if ( !Initialize( type, false ) )
        {
          return Fail( mainStatus );
        }

        //Get pending members
        var users = service.ListMembers( manageID, 0 );
        if ( users == null )
        {
          return Fail( "Error getting members" );
        }

        return Reply( users, true, "okay", type );
      }
      catch ( Exception ex )
      {
        return Reply( ex.StackTrace, false, ex.Message, null );
      }
    }

    //List Pending Invitations
    [WebMethod]
    public JSON ListPendingInvitations( string type, int manageID )
    {
      try 
      {
        //Setup
        if ( !Initialize( type, false ) )
        {
          return Fail( mainStatus );
        }

        //Get pending members
        var users = service.ListPendingInvitations( manageID );
        if ( users == null )
        {
          return Fail( "Error getting invitations" );
        }

        return Reply( users, true, "okay", type );
      }
      catch ( Exception ex )
      {
        return Reply( ex.StackTrace, false, ex.Message, null );
      }
    }

    #endregion

    #region Set methods
    //Update members' privileges
    [WebMethod( EnableSession = true )]
    public JSON Members_UpdateMemberships( string type, int manageID, List<int> memberIDs, int privilegeID )
    {
      try
      {
        //Setup
        if ( !Initialize( type, true ) )
        {
          return Fail( mainStatus );
        }

        //Attempt to save changes for each user
        //var data = service.Members_SaveChanges( manageID, user, memberIDs, privilegeID );
        var data = new List<Valid>();
        foreach ( var id in memberIDs )
        {
          try
          {
            data.Add( service.Member_UpdateMembership( manageID, user, id, privilegeID ) );
          }
          catch ( Exception ex )
          {
            data.Add( new Valid()
            {
              id = id,
              text = "There was an error performing this update.",
              valid = false,
              extra = new { title = "", error = ex.Message }
            } );
          }
        }

        return Reply( data, true, "okay", type );
      }
      catch ( Exception ex )
      {
        return Reply( ex.StackTrace, false, ex.Message, null );
      }
    }

    //Invite new members
    [WebMethod( EnableSession = true )]
    public JSON Members_Invite( string type, int manageID, List<string> emails, int privilegeID, string customMessage )
    {
      try
      {
        //Setup
        if ( !Initialize( type, true ) )
        {
          return Fail( mainStatus );
        }

        //Validate message text
        bool valid = true;
        string status = "";
        customMessage = utilService.ValidateText( customMessage, 0, "Custom Message", ref valid, ref status );
        if ( !valid )
        {
          return Fail( status );
        }

        //Validate emails
        var addedEmails = new List<string>();
        var validEmails = new List<string>();
        var invitedEmails = new List<Valid>();
        var invalidEmails = new List<Valid>();
        foreach ( var email in emails )
        {
          if ( string.IsNullOrWhiteSpace( email ) ) { continue; }
          if ( addedEmails.Contains( email ) ) { continue; }
          else { addedEmails.Add( email ); }
          valid = true;
          status = "";
          bool alreadyExists = false;
          utilService.ValidateEmail( email, ref valid, ref status, ref alreadyExists );
          if ( valid )
          {
            validEmails.Add( email );
          }
          else
          {
            //Reject invalid emails but include them in the returned data
            invalidEmails.Add( new Valid()
            {
              id = 0,
              text = email + " is not a valid email. Reason: " + status,
              valid = false
            } );
          }
        }

        //Attempt to invite valid emails
        var acctServ = new AccountServices();
        foreach ( var email in validEmails )
        {
          //Check to see if users exist
          var existingUser = acctServ.GetByEmail( email );
          if ( existingUser == null || existingUser.Id == 0 )
          {
            invitedEmails.Add( service.Member_InviteNewUser( manageID, user, email, privilegeID, customMessage ) );
          }
          else
          {
            invitedEmails.Add( service.Member_InviteExistingUser( manageID, user, existingUser, privilegeID, customMessage ) );
          }
        }

        //Result is a list of all submitted emails with appropriate statuses for each
        var emailData = invitedEmails.Concat( invalidEmails );
        status = emailData.Where( m => m.valid ).Count() + " of " + emailData.Count() + " invitations successful.";

        //Fetch the updated list of invitees
        var updatedData = ListPendingInvitations( type, manageID );
        if ( updatedData.valid )
        {
          return Reply( updatedData.data, true, status, emailData );
        }
        else
        {
          return Reply( updatedData.data, false, status + "\n\nThere was an error retrieving the list of pending invitations: " + updatedData.status, emailData );
        }

      }
      catch ( Exception ex )
      {
        return Reply( ex.StackTrace, false, ex.Message, null );
      }
    }

    //Revoke existing memberships
    [WebMethod( EnableSession = true )]
    public JSON Members_RevokeMemberships( string type, int manageID, List<int> memberIDs )
    {
      try
      {
        //Setup
        if ( !Initialize( type, true ) )
        {
          return Fail( mainStatus );
        }

        //Attempt to remove each member
        var data = new List<Valid>();

        foreach ( var id in memberIDs )
        {
          try
          {
            data.Add( service.Member_RevokeMembership( manageID, user, id ) );
          }
          catch ( Exception ex )
          {
            data.Add( new Valid()
            {
              id = id,
              text = "There was an error removing this user.",
              valid = false,
              extra = new { title = "", error = ex.Message }
            } );
          }
        }

        return Reply( data, true, "okay", type );
      }
      catch ( Exception ex )
      {
        return Reply( ex.StackTrace, false, ex.Message, null );
      }

    }

    //Approve pending memberships
    [WebMethod( EnableSession = true )]
    public JSON Members_ApproveMemberships( string type, int manageID, List<int> memberIDs, int privilegeID, string customMessage )
    {
      try
      {
        //Setup
        if ( !Initialize( type, true ) )
        {
          return Fail( mainStatus );
        }

        //Validate message text
        bool valid = true;
        string status = "";
        customMessage = utilService.ValidateText( customMessage, 0, "Custom Message", ref valid, ref status );
        if ( !valid )
        {
          return Fail( status );
        }

        //Attempt to do approvals
        var data = new List<Valid>();
        foreach ( var id in memberIDs )
        {
          try
          {
            //Need to do something with customMessage?
            data.Add( service.Member_ApproveMembership( manageID, user, id, privilegeID, customMessage ) );
          }
          catch ( Exception ex )
          {
            data.Add( new Valid()
            {
              id = id,
              valid = false,
              text = "There was an error approving this membership.",
              extra = new { error = ex.Message }
            } );
          }
        }

        return Reply( data, true, "okay", type );
      }
      catch ( Exception ex )
      {
        return Reply( ex.StackTrace, false, ex.Message, null );
      }
    }

    //Deny pending memberships
    [WebMethod( EnableSession = true )]
    public JSON Members_DenyMemberships( string type, int manageID, List<int> memberIDs, string customMessage )
    {
      try
      {
        //Setup
        if ( !Initialize( type, true ) )
        {
          return Fail( mainStatus );
        }

        //Validate message text
        bool valid = true;
        string status = "";
        customMessage = utilService.ValidateText( customMessage, 0, "Custom Message", ref valid, ref status );
        if ( !valid )
        {
          return Fail( status );
        }

        var data = new List<Valid>();
        foreach ( var id in memberIDs )
        {
          try
          {
            data.Add( service.Member_DenyMembership( manageID, user, id, customMessage ) );
          }
          catch ( Exception ex )
          {
            data.Add( new Valid()
            {
              id = id,
              valid = false,
              text = "There was an error approving this membership.",
              extra = new { error = ex.Message }
            } );
          }
        }

        return Reply( data, true, "okay", type );
      }
      catch ( Exception ex )
      {
        return Reply( ex.StackTrace, false, ex.Message, null );
      }

    }

    #endregion

    #region Helper methods

    //Setup boilerplate objects
    private bool Initialize( string type, bool validateUser )
    {
      //Get the management object
      service = ManageObjectConstructor.GetInstance( type );
      if ( service == null )
      {
        mainStatus = "Invalid Type";
        return false;
      }

      if ( validateUser )
      {
        //Validate the user
        user = ( Patron ) Session[ "user" ];
        if ( user == null )
        {
          mainStatus = "You must be logged in to do that.";
          return false;
        }
      }

      return true;
    }

    //Reply with standardized JSON response message
    private UtilityService.GenericReturn Reply( object data, bool valid, string status, object extra )
    {
      return UtilityService.DoReturn( data, valid, status, extra );
    }
    private UtilityService.GenericReturn Fail( string status )
    {
      return Reply( null, false, status, null );
    }

    #endregion

    #region Helper classes
    #endregion
  }
}
