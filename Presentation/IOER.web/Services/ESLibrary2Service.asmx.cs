using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

using IB = ILPathways.Business;
using ILPathways.Utilities;
using Isle.BizServices;
using Isle.DTO;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using AccountManager = Isle.BizServices.AccountServices;
using DataBaseHelper = LRWarehouse.DAL.BaseDataManager;
using CodesService = Isle.BizServices.CodeTableBizService;
using LibraryBizService = Isle.BizServices.LibraryBizService;
using ThisUser = LRWarehouse.Business.Patron;

namespace IOER.Services
{
    /// <summary>
    /// Summary description for ESLibrary2Service
    /// </summary>
    [WebService( Namespace = "http://tempuri.org/" )]
    [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
    [System.ComponentModel.ToolboxItem( false )]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class ESLibrary2Service : System.Web.Services.WebService
    {
      LibraryBizService libService = new LibraryBizService();
      AccountServices accountService = new AccountServices();
      JavaScriptSerializer serializer = new JavaScriptSerializer();

      [WebMethod( EnableSession = true )]
      public string GetAllLibraryInfo( string userGUID, int libraryID )
      {
          LibraryData output = GetAllLibraryData( userGUID, libraryID );

          //Do the serialization and return
          return serializer.Serialize( output );
      }
      
      /* Get All Data */
      public LibraryData GetAllLibraryData( string userGUID, int libraryID )
      {
        var user = GetUser( userGUID );            
        return GetAllLibraryData( user, libraryID );
      }
      public LibraryData GetAllLibraryData( ThisUser user, int libraryID )
      {
        var library = libService.Get( libraryID );
        return GetAllLibraryData( user, library );
      }
      public LibraryData GetAllLibraryData( string userGUID, IB.Library library )
      {
        var user = GetUser( userGUID );
        return GetAllLibraryData( user, library );
      }
      public LibraryData GetAllLibraryData( ThisUser user, IB.Library library )
      {
        var output = new LibraryData();
        output.library = GetLibrary( user, library );
        output.collections = GetCollections( user, library.Id );
        output.isMyLibrary = ( user.Id == output.library.createdByID );

        output.hasEditAccess = libService.Library_DoesUserHaveEditAccess( library.Id, user.Id );
        if ( output.hasEditAccess == false )
            output.hasContribute = libService.Library_DoesUserHaveContributeAccess( library.Id, user.Id );
        else
            output.hasContribute = output.hasEditAccess;

        output.allowJoinRequest = ( user.Id != output.library.createdByID ) && ( output.library.allowJoinRequest == true );
          //check if already a member
        var memberCheck = libService.LibraryMember_Get( library.Id, user.Id );
        if ( memberCheck.Id > 0 && memberCheck.MemberTypeId > 0 )
        {
            //prob better to show membership??
            output.allowJoinRequest = false;
        }

        output.myLibraries = GetMyLibraries( user );
        ActivityBizServices.LibraryHit( library, user, "Visit" );

        return output;
      }
    

      #region GET methods 
      /* Get a Library */
      [WebMethod( EnableSession = true )]
      public string GetLibrary( string userGUID, int libraryID )
      {
        var user = GetUser( userGUID );
        return serializer.Serialize( GetLibrary( user, libraryID ) );
      }
      public JSONLibrary GetLibrary( ThisUser user, int libraryID )
      {
        var library = libService.Get( libraryID );
        return GetLibrary( user, library );
      }
      public JSONLibrary GetLibrary( ThisUser user, IB.Library library )
      {
        return GetLibrary( user, library, true, true );
      }
      public JSONLibrary GetLibrary( ThisUser user, IB.Library library, bool doSubscriptionCheck, bool getFilters )
      {
        var output = new JSONLibrary();
        //Also, I'm not sure if it's better to use currentLibraryId, or just compare CreatedById.
        if ( library.PublicAccessLevelInt > 1 
            || libService.Library_DoesUserHaveContributeAccess( library.Id, user.Id ) )
        {
          output.id = library.Id;
          output.avatarURL = library.ImageUrl;
          if ( output.avatarURL == "" )
            output.avatarURL = "/images/ioer_med.png";

          output.title = library.Title;
          output.description = library.Description;
          output.currentPublicAccessLevel = ( int ) library.PublicAccessLevel;
          output.currentOrganizationAccessLevel = ( int ) library.OrgAccessLevel;
          output.allowJoinRequest = library.AllowJoinRequest;
          output.orgId = library.OrgId;
          output.Organization = library.Organization;
          output.paradata = GetParadata( user, library, null, true );
          output.createdByID = library.CreatedById;
          if ( getFilters )
          {
            output.filters = GetFiltersNew( library.Id, 0 );
          }
            //don't log as typically just collecting libraries for dropdown lists
          //ActivityBizServices.LibraryHit( library.Id, user.Id, "Get" );
        }
        else
        {
          throw new UnauthorizedAccessException( "You don't have access to that Library." );
        }

        return output;
      }


      /* Get Libraries I have Edit Access to */
      [WebMethod]
      public string GetMyLibraries( string userGUID )
      {
        var user = GetUser( userGUID );
        return serializer.Serialize( GetMyLibraries( user ) );
      }
      public List<JSONLibrary> GetMyLibraries( ThisUser user )
      {
          var libraries = libService.Libraries_SelectListWithContributeAccess( user.Id );

          //var libraries = libService.Library_SelectListWithContributeAccess( user.Id );
        return GetMyLibraries( user, libraries );
      }
      public List<JSONLibrary> GetMyLibraries( ThisUser user, List<ILPathways.Business.Library> libraries )
      {
        var output = new List<JSONLibrary>();
        foreach ( ILPathways.Business.Library library in libraries )
        {
          var lib = GetLibrary( user, library, false, false );
            //????????????????
          var libCollections = libService.LibrarySections_SelectListWithContributeAccess( library.Id, user.Id );
          lib.collections = GetCollections( user, libCollections, false, false, false );
          
          output.Add( lib );
        }

        return output;
      }

      public List<JSONLibrary> GetMyLibrariesWithContribute( int userId )
      {
          ThisUser user = AccountServices.GetUser( userId );
          return GetMyLibrariesWithContribute( user ); ;
      }
      public List<JSONLibrary> GetMyLibrariesWithContribute( ThisUser user )
      {
          List<LibrarySummaryDTO> libraries = libService.Library_SelectListWithContributeAccess( user.Id );

          var output = new List<JSONLibrary>();
          var lib = new JSONLibrary();
          foreach ( LibrarySummaryDTO library in libraries )
          {
              //var lib = FillContributeLibrary( user, library );
              lib = new JSONLibrary();
              lib.id = library.Id;
              lib.isPersonalLibrary = library.LibraryTypeId == 1;

              lib.avatarURL = library.ImageUrl;
              if ( lib.avatarURL == "" )
                  lib.avatarURL = "/images/ioer_med.png";

              lib.title = library.Title;
              if ( library.UserNeedsApproval )
                  lib.title = library.Title + " [Requires Approval]";
              else
                  lib.title = library.Title;
              //lib.description = library.Description;
              lib.currentPublicAccessLevel = library.PublicAccessLevel;
              lib.currentOrganizationAccessLevel = library.OrgAccessLevel;

              lib.createdByID = library.CreatedById;

              //????????????????
              var libCollections = libService.LibrarySections_SelectListWithContributeAccess( library.Id, user.Id );
              lib.collections = GetCollections( user, libCollections, false, false, false );

              output.Add( lib );
          }

          return output;
      }
      
      /* Get Collections */
      [WebMethod]
      public string GetCollections( string userGUID, int libraryID )
      {
        var user = GetUser( userGUID );
        return serializer.Serialize( GetCollections( user, libraryID ) );
      }
      public List<JSONCollection> GetCollections( ThisUser user, int libraryID )
      {
        var collections = libService.LibrarySectionsSelectList( libraryID, 1 ); //Get the list of public collections
        if ( user.Id > 0 )
        { //If the user is logged in, get the list of collections for which they have edit access
          var authorizedCollections = libService.LibrarySections_SelectListWithEditAccess( libraryID, user.Id );
          foreach ( IB.LibrarySection collection in authorizedCollections )
          {
            bool add = true;
            foreach ( IB.LibrarySection collection2 in collections )
            {
              if ( collection.Id == collection2.Id ) //duplicate checking
              {
                add = false;
              }
            }

            if ( add ) //If a collection exists in the list of edit-access collections but not in the list of public ones, add it
            {
              collections.Add( collection );
            }
          }
        }
        return GetCollections( user, collections );
      }
	  public List<JSONCollection> GetCollections(ThisUser user, List<IB.LibrarySection> collections)
      {
        return GetCollections( user, collections, false );
      }
	  public List<JSONCollection> GetCollections(ThisUser user, List<IB.LibrarySection> collections, bool includeLibraryTitle)
      {
        return GetCollections( user, collections, includeLibraryTitle, true, true );
      }
	  public List<JSONCollection> GetCollections(ThisUser user, List<IB.LibrarySection> collections, bool includeLibraryTitle, bool doSubscriptionCheck, bool getFilters)
      {
        var output = new List<JSONCollection>();
		foreach (IB.LibrarySection section in collections)
        {
          var col = new JSONCollection();
          col.id = section.Id;
          col.avatarURL = ( section.ImageUrl == "" ? "/images/ioer_med.png" : section.ImageUrl );
          col.title = ( includeLibraryTitle ? section.LibraryTitle + ": " : "" ) + section.Title;
          col.description = section.Description;
          col.currentPublicAccessLevel = ( int ) section.PublicAccessLevel;
          col.currentOrganizationAccessLevel = ( int ) section.OrgAccessLevel;
          col.isDefaultCollection = section.IsDefaultSection;
          col.paradata = GetParadata( user, null, section, doSubscriptionCheck );          
          if ( getFilters )
          {
            col.filters = GetFiltersNew( 0, col.id );
          }
          output.Add( col );
        }

        return output;

      }

      /* Get My Collections */
      [WebMethod]
      public string GetMyCollections( string userGUID, int libraryID )
      {
        var user = GetUser( userGUID );
        return serializer.Serialize( GetMyCollections( user, libraryID ) );
      }
      public List<JSONCollection> GetMyCollections( ThisUser user, int libraryID )
      {
        var collections = libService.LibrarySections_SelectListWithEditAccess( libraryID, user.Id );
        return GetMyCollections( user, collections );
      }
	  public List<JSONCollection> GetMyCollections(ThisUser user, List<IB.LibrarySection> collections)
      {
        return GetCollections( user, collections, false, false, false );
      }

      /* Get Available Filters */
      public List<JSONFilter> GetFiltersNew( int libraryID, int collectionID )
      {
        var output = new List<JSONFilter>();
        var data = new ResourceV2Services().GetFieldsForLibCol( new List<int>() { libraryID }, new List<int>() { collectionID }, 1 );

        foreach ( var item in data )
        {
          output.Add( new JSONFilter()
          {
            id = item.Id,
            name = item.Schema,
            ids = item.Tags.Select( t => t.Id ).ToList()
          } );
        }

        return output;
      }
      public List<JSONFilter> GetFilters( int libraryID, int collectionID )
      {
        var maps = new string[][] {
          new string[] { "AccessRights", "accessRights" },
          new string[] { "IntendedAudience", "endUser" },
          new string[] { "GradeLevel", "gradeLevel" },
          new string[] { "ResourceType", "resourceType" },
          new string[] { "ResourceFormat", "mediaType" },
          new string[] { "GroupType", "groupType" },
          new string[] { "CareerCluster", "careerCluster" },
          new string[] { "EducationalUse", "educationalUse" },
        };
        //          new string[] { "Language", "language" }
        var output = new List<JSONFilter>();

        foreach ( string[] item in maps )
        {
          var filter = new JSONFilter();
          filter.name = item[1];
          filter.ids = GetFilters( libraryID, collectionID, item[0] );
          output.Add( filter );
      }

        return output;
      }

      public List<int> GetFilters( int libraryID, int collectionID, string field )
      {
        var results = new List<IB.DataItem>();
        var output = new List<int>();
        if ( collectionID == 0 )
        {
          results = libService.AvailableFiltersForLibrary( libraryID, field );
        }
          else
        {
            results = libService.AvailableFiltersForCollection( collectionID, field );
        }

		foreach (IB.DataItem item in results)
        {
            output.Add( item.Id );
        }

        return output;
      }


      [WebMethod]
      public string FillCollectionWidget( int libraryID, string collectionName, int maxResources, bool usingTargetUrl )
      {
          string keyword = "";
          return serializer.Serialize( FillCollectionWidget( libraryID, collectionName
                    , maxResources
                    , usingTargetUrl
                    , keyword ) );
      }
      public string FillCollectionWidget( int libraryID, string collectionName, int maxResources, bool usingTargetUrl, string keyword )
      {
          return "in progress";
      }
      #endregion

      #region Update Methods
      /* Post a Comment */
      [WebMethod( EnableSession = true )]
      public string PostComment( string userGUID, int libraryID, int collectionID, string text )
      {
        var user = GetUser( userGUID );
        if ( !user.IsValid ) { return ""; }

        var status = "";
        text = ValidateText( text, 10, ref status );

        if ( status != "okay" )
        {
          return status;
        }

        if ( collectionID == 0 )
        {
          libService.LibraryCommentCreate( libraryID, text, user.Id );
          ActivityBizServices.LibraryHit( libraryID, user, "Library Comment" );
        }
        else
        {
          libService.LibrarySectionCommentCreate( collectionID, text, user.Id );
          ActivityBizServices.CollectionHit( collectionID, user, "Collection Comment" );
        }

        return serializer.Serialize( GetAllLibraryData( user, libraryID ) );
      }

      /* Update Following Option */
      [WebMethod( EnableSession = true )]
      public string UpdateFollowingOption( string userGUID, int libraryID, bool isLibrary, int collectionID, int typeID )
      {
        var user = GetUser( userGUID );
        if ( !user.IsValid ) { return ""; }
        string status = "";
        string action = "";

        //If Library
        if ( collectionID == 0 || isLibrary )
        {
          var maybeSub = libService.LibrarySubscriptionGet( libraryID, user.Id );
          if ( maybeSub.IsValid )
          {
            if ( typeID == 0 )
            {
              libService.LibrarySubscriptionDelete( maybeSub.Id, ref status );
              action = "Library Unfollow";
            }
            else
            {
              libService.LibrarySubScriptionUpdate( maybeSub.Id, typeID );
              action = "Updated Library Follower";
            }
          }
          else
          {
            if ( typeID != 0 )
            {
              libService.LibrarySubScriptionCreate( libraryID, user.Id, typeID, ref status );
              action = "New Library Follower";
            }
          }
        }
        else
        {
          //Collection
          var maybeSub = libService.CollectionSubscriptionGet( collectionID, user.Id );
          if ( maybeSub != null && maybeSub.Id > 0 )
          {
            if ( typeID == 0 )
            {
              libService.CollectionSubscriptionDelete( maybeSub.Id, ref status );
              action = "Collection Unfollow";
            }
            else
            {
              libService.CollectionSubScriptionUpdate( maybeSub.Id, typeID );
              action = "Updated Collection Follower";
            }
          }
          else
          {
            if ( typeID != 0 )
            {
              libService.CollectionSubscriptionCreate( collectionID, user.Id, typeID, ref status );
              action = "New Collection Follower";
              ActivityBizServices.CollectionHit( collectionID, user, "Collection Follower" );
            }
          }
        }

        ActivityBizServices.LibraryHit( libraryID, user, action );

        return serializer.Serialize( GetAllLibraryData( user, libraryID ) );
      }

      [WebMethod( EnableSession = true )]
      public string ActionCopy( string userGUID, int libraryID, int toCollection, int intID )
      {
        //Check user
        var user = GetUser( userGUID );
        if ( !user.IsValid ) 
        { 
          return "Invalid user"; 
        }

        //Check user can edit the collection to copy to
        var collection = libService.LibrarySectionGet( toCollection );
        if ( !libService.LibrarySection_DoesUserHaveContributeAccess( collection.LibraryId, collection.Id, user.Id ) ) 
        {
          return "Sorry, you don't have permission to do that.";
        }

        //Do the copy
        string status = "";
        var id = libService.ResourceCopy( intID, libraryID, toCollection, user, ref status );
        if ( id == 0 ) 
        {   
            return "Sorry, there was a problem copying the resource."; 
        }
        else 
        {
            ActivityBizServices.LibraryHit( libraryID, user, "LibraryResource Copy" );
            ActivityBizServices.CollectionHit( toCollection, user, "Collection Resource Copied from: " + libraryID.ToString() );
            return serializer.Serialize( GetAllLibraryData( user, libraryID ) );
        }
      }

      [WebMethod( EnableSession = true )]
      public string ActionMove( string userGUID, int libraryID, int fromCollection, int toCollection, int intID )
      {
        try
        {
        //Check user
        var user = GetUser( userGUID );
        if ( !user.IsValid ) 
        { 
          return ""; 
        }

        //Check user can edit the collection to move from
          //var fromCol = libService.LibrarySectionGet( fromCollection );
          if ( !libService.LibrarySection_DoesUserHaveEditAccess( libraryID, fromCollection, user.Id ) )
        { 
          return ""; 
        }
        
        //Check user can edit the collection to move to, regardless of whether or not it's the source library
          var toCol = libService.LibrarySectionGet( toCollection );
          if ( !libService.LibrarySection_DoesUserHaveEditAccess( toCol.LibraryId, toCollection, user.Id ) )
        { 
          return ""; 
        }

        //Do the move
        string status = "";
        var id = libService.ResourceMove( fromCollection, intID, toCollection, user.Id, ref status );
        if ( id != "successful" ) { return ""; }
        else
        {
            ActivityBizServices.LibraryHit( libraryID, user, "Resource Move" );
          return serializer.Serialize( GetAllLibraryData( user, libraryID ) );
        }
      }
        catch ( Exception ex )
        {
          return ex.Message;
        }
      }

      [WebMethod( EnableSession = true )]
      public string ActionDelete( string userGUID, int libraryID, int fromCollection, int intID )
      {
        //Check user
        var user = GetUser( userGUID );
        if ( !user.IsValid ) 
        { 
          return ""; 
        }

        //Check user can edit the collection to delete from
        var fromCol = libService.LibrarySectionGet( fromCollection );
        if ( !libService.LibrarySection_DoesUserHaveEditAccess( libraryID, fromCol.Id, user.Id ) )
        { 
          return ""; 
        }

        string status = "";
        var result = libService.LibraryResourceDelete( fromCollection, intID, user, ref status );
        if ( result == false ) 
        { 
          return ""; 
        }
        else
        {

          return serializer.Serialize( GetAllLibraryData( user, libraryID ) );
        }
      }

      [WebMethod( EnableSession = true )]
      public string UpdateLibCol( string userGUID, int collectionID, string title, string description, bool isLibrary, int publicAccessLevel, int organizationAccessLevel, bool makeDefault, int libraryID )
      {
        var user = GetUser( userGUID );
        if ( !user.IsValid ) 
        { 
            return ""; 
        }

        var library = libService.Get( libraryID );
        if ( !libService.Library_DoesUserHaveEditAccess( libraryID, user.Id ) )
        {
          return "";
        }

        var status = "";
        title = ValidateText( title, 3, ref status );
        if ( status != "okay" ) { 
            return ""; 
        }
        description = ValidateText( description, 5, ref status );
        if ( status != "okay" ) { 
            return ""; 
        }

        if ( organizationAccessLevel < publicAccessLevel )
        {
          organizationAccessLevel = publicAccessLevel;
        }

        if ( isLibrary )
        {
          library.Title = title;
          library.Description = description;
          library.PublicAccessLevel = ( ILPathways.Business.EObjectAccessLevel ) publicAccessLevel;
          library.OrgAccessLevel = ( ILPathways.Business.EObjectAccessLevel ) organizationAccessLevel;
          library.LastUpdatedById = user.Id;

          libService.Update( library );
        }
        else
        {
          var collection = libService.LibrarySectionGet( collectionID );
          if ( collection.ParentLibrary.Id != libraryID )
          {
            return "";
          }

          collection.Title = title;
          collection.Description = description;
          collection.PublicAccessLevel = ( ILPathways.Business.EObjectAccessLevel ) publicAccessLevel;
          collection.OrgAccessLevel = ( ILPathways.Business.EObjectAccessLevel ) organizationAccessLevel;
          collection.LastUpdatedById = user.Id;

          if ( makeDefault ) { collection.IsDefaultSection = true; } //Don't want to allow unsetting default

          libService.LibrarySectionUpdate( collection );
        }

        return serializer.Serialize( GetAllLibraryData( user, library ) );
      }

      [WebMethod( EnableSession = true )]
      public string CreateCollection( string userGUID, int collectionID, string title, string description, bool isLibrary, int publicAccessLevel, int organizationAccessLevel, bool makeDefault, int libraryID )
      {
        var user = GetUser( userGUID );
        if ( !user.IsValid ) { return ""; }

        var library = libService.Get( libraryID );
        if ( !libService.Library_DoesUserHaveEditAccess( libraryID, user.Id ) )
        {
          return "";
        }

        var status = "";
        title = ValidateText( title, 3, ref status );
        if ( status != "okay" ) { return ""; }
        description = ValidateText( description, 5, ref status );
        if ( status != "okay" ) { return ""; }

        var collection = new IB.LibrarySection();
        collection.CreatedById = user.Id;
        collection.LibraryId = library.Id;
        collection.LibraryTitle = library.Title;
        collection.Title = title;
        collection.Description = description;
        collection.PublicAccessLevel = ( ILPathways.Business.EObjectAccessLevel ) publicAccessLevel;
        collection.OrgAccessLevel = ( ILPathways.Business.EObjectAccessLevel ) organizationAccessLevel;
        collection.IsDefaultSection = makeDefault;

        collection.AreContentsReadOnly = false;
        collection.SectionTypeId = 3;
        collection.ParentId = 0;
        collection.ImageUrl = "";

        libService.LibrarySectionCreate( collection, ref status );

        return serializer.Serialize( GetAllLibraryData( user, library ) );
      }

      [WebMethod]
      public string DeleteCollection( string userGUID, int libraryID, int collectionID )
      {
        //Get user
        var user = GetUser( userGUID );
        if ( !user.IsValid ) 
        { 
          return ""; 
        }

        //Get collection and ensure it's safe to delete it
        var collection = libService.LibrarySectionGet( collectionID );
        if(
          !collection.IsValid ||
          collection.Id == 0 ||
          collection.IsDefaultSection ||
          !libService.LibrarySection_DoesUserHaveEditAccess( libraryID, collectionID, user.Id )
        )
        {
          return ""; 
        }

        //Do the delete
        string status = "";
        
        libService.LibrarySection_Delete( collectionID, ref status );
        ActivityBizServices.LibraryHit( libraryID, user, "Delete Collection" );
        //Return the updated data
        return serializer.Serialize( GetAllLibraryData( user, libraryID ) );
      }

      /* Add Like/Dislike */
      [WebMethod( EnableSession = true )]
      public string AddLikeDislike( string userGUID, int libraryID, int collectionID, bool isLike )
      {
        //Get User
        var user = GetUser( userGUID );
        if ( user == null || user.Id == 0 || libraryID == 0 )
        {
          return "";
        }

        //Add paradata
        if ( collectionID == 0 )
        {
          //Check for existing
          IB.ObjectLike like = libService.Library_GetLike( libraryID, user.Id );
          if ( like == null || !like.HasLikeEntry )
          {
            libService.LibraryLikeCreate( libraryID, isLike, user.Id );
            ActivityBizServices.LibraryHit( libraryID, user, "Library Like" );
          }
        }
        else
        {
          IB.ObjectLike like = libService.LibrarySection_GetLike( collectionID, user.Id );
          if ( like == null || !like.HasLikeEntry )
          {
            libService.LibrarySectionLikeCreate( collectionID, isLike, user.Id );
            ActivityBizServices.CollectionHit( collectionID, user, "Collection Like" );
          }
        }

        //return updated data
        return serializer.Serialize( GetAllLibraryData( user, libraryID ) );
      }

      /* Request to Join a Library */
      [WebMethod( EnableSession = true )]
      public string RequestJoin( string userGUID, int libraryID, string message )
      {
        //Note: the extra parameter at the end of the ImmediateReturn is in this case used to indicate whether or not to replace the join request area with the text of the message returned.
        bool isValid = false;
        string status = "";
        var utilService = new UtilityService();
        message = utilService.ValidateText( message, 25, "Message", ref isValid, ref status );
        if ( !isValid )
        {
          return utilService.ImmediateReturn( "", false, status, false );
        }

        if ( userGUID == "" )
        {
          return utilService.ImmediateReturn( "", false, "Please login to request access.", true );
        }

        //Get User
        var user = GetUser( userGUID );
        if ( !user.IsValid || libraryID == 0 )
        {
          return utilService.ImmediateReturn( "", false, "Invalid User or Library", true );
        }

        //Check to see if they already have access
        var memberCheck = libService.LibraryMember_Get( libraryID, user.Id );
        if( memberCheck.Id > 0 && memberCheck.MemberTypeId > 0 )
        {
          return utilService.ImmediateReturn( "", false, "You are already a member of this Library.", true );
        }

        //Check to see if they already requested access
        if ( memberCheck.Id > 0 && memberCheck.MemberTypeId == 0 )
        {
          return utilService.ImmediateReturn( "", false, "You have already requested access to this Library.", true );
        }

        //Request access
        int mbrId = libService.LibraryMember_Create( libraryID, user.Id, 0, user.Id, ref status );
        if ( mbrId > 0 )
        {
            ActivityBizServices.LibraryHit( libraryID, user, "Request to Join Library" );

            //send email to library admin.
            if ( libService.SendLibraryJoinRequestEmail( libraryID, user, message, ref status ) == false )
            {
              return utilService.ImmediateReturn( "", false, "You have requested access to this Library. However, there was an issue sending the email to the library administrator: " + status, false );
            }
        }
        //Return result
        return utilService.ImmediateReturn( "You have requested access to this Library. The administrator(s) will determine whether or not to grant you access.", true, "", true );

      }

      #endregion

      #region Helper Methods

      /* Get a User */
      protected ThisUser GetUser( string userGUID )
      {
        return userGUID == "" ? new ThisUser() { IsValid = false, Id = 0 } : new AccountManager().GetByRowId( userGUID );
      }

      protected string ValidateText( string text, int minimumLength, ref string status )
      {
        status = "";
        text = text.Replace( "<", "" ).Replace( ">", "" ).Replace( "\"", "'" );
        text = FormHelper.CleanText( text );
        if ( BadWordChecker.CheckForBadWords( text ) )
        {
          text = "";
          status += "Inappropriate language detected. ";
        }
        if ( text.Length < minimumLength )
        {
          text = "";
          status = status + "Too short. ";
        }
        if ( status == "" ) { status = "okay"; }
        return text;
      }

	  protected List<JSONComment> GetComments(IB.Library library, IB.LibrarySection collection)
      {
        var output = new List<JSONComment>();
		var comments = new List<IB.ObjectComment>();
        if ( collection == null )
        {
          //Get comments for a library
          comments = libService.LibraryComment_Select( library.Id );
        }
        else
        {
          //Get comments for a collection
          comments = libService.LibrarySectionComment_Select( collection.Id );
        }
		foreach (IB.ObjectComment comment in comments)
        {
          var item = new JSONComment();
          item.date = comment.Created.ToShortDateString();
          item.name = comment.CreatedBy;
          item.text = comment.Comment;
          item.id = comment.Id;
          output.Add( item );
        }

        return output;
      }

	  public JSONParadata GetParadata(ThisUser user, IB.Library library, IB.LibrarySection collection, bool doSubscriptionCheck)
      {
        var output = new JSONParadata();

        if ( collection == null || collection.Id == 0 )
        {
          //Likes and dislikes
          try
          {
			  var data = libService.Library_LikeSummary(library.Id).First<IB.DataItem>();
            output.likes = data.Int1;
            output.dislikes = data.Int2;
            if ( library.Id > 0 && user.Id > 0 )
            {
				IB.ObjectLike like = libService.Library_GetLike(library.Id, user.Id);
                if ( like != null && like.HasLikeEntry )
                {
                    output.iLikeThis = like.IsLike;
                    output.iDislikeThis = !like.IsLike;
                }
            }
          }
          catch ( Exception ex ) { } //Fails if there is an empty result set

          //Comments
          output.comments = GetComments( library, null );

          //Subscriptions
          if ( doSubscriptionCheck && user.Id > 0 )
          {
            output.following = libService.LibrarySubscriptionGet( library.Id, user.Id ).SubscriptionTypeId;
          }
        }
        else
        {
          //Likes and dislikes
          try
          {
			  var data = libService.LibrarySection_LikeSummary(collection.Id).First<IB.DataItem>();
            output.likes = data.Int1;
            output.dislikes = data.Int2;
            if ( collection.Id > 0 && user.Id > 0 )
            {
				IB.ObjectLike like = libService.LibrarySection_GetLike(collection.Id, user.Id);
                if ( like != null && like.HasLikeEntry )
                {
                    output.iLikeThis = like.IsLike;
                    output.iDislikeThis = !like.IsLike;
                }
            }
          }
          catch ( Exception ex ) { }

          //Comments
          output.comments = GetComments( null, collection );

          //Subscriptions
          if ( doSubscriptionCheck && user.Id > 0 )
          {
            output.following = libService.CollectionSubscriptionGet( collection.Id, user.Id ).SubscriptionTypeId;
          }
        }

        return output;
      }

      #endregion

      #region subclasses
      public class LibraryData
      {
        public LibraryData()
        {
          library = new JSONLibrary();
          collections = new List<JSONCollection>();
          myLibraries = new List<JSONLibrary>();
        }
        public bool isMyLibrary { get; set; }
        public bool hasEditAccess { get; set; }
        public bool hasContribute { get; set; }
        public bool allowJoinRequest { get; set; }

        public JSONLibrary library { get; set; }
        public List<JSONCollection> collections { get; set; }
        public List<JSONLibrary> myLibraries { get; set; }
      }
      public class JSONLibColBase
      {
        public JSONLibColBase()
        {
          paradata = new JSONParadata();
          filters = new List<JSONFilter>();
        }
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string avatarURL { get; set; }
        public int currentPublicAccessLevel { get; set; }
        public int currentOrganizationAccessLevel { get; set; }
        public int orgId { get; set; }
        public string Organization { get; set; }
        public JSONParadata paradata { get; set; }
        public List<JSONFilter> filters { get; set; }
      }
      public class JSONLibrary : JSONLibColBase
      {
        public JSONLibrary()
        {
          collections = new List<JSONCollection>();
        }
        public bool allowJoinRequest { get; set; }
        public bool isPersonalLibrary { get; set; }
          
        public List<JSONCollection> collections { get; set; }
        [System.Web.Script.Serialization.ScriptIgnore]
        public int createdByID { get; set; } //Not serialized
      }
      public class JSONCollection : JSONLibColBase
      {
        public bool isDefaultCollection { get; set; }
      }
      public class JSONParadata
      {
        public JSONParadata()
        {
          comments = new List<JSONComment>();
        }
        public bool iLikeThis { get; set; }
        public bool iDislikeThis { get; set; }
        public int likes { get; set; }
        public int dislikes { get; set; }
        public List<JSONComment> comments { get; set; }
        public int following { get; set; }
      }
      public class JSONComment
      {
        public int id { get; set; }
        public string name { get; set; }
        public string text { get; set; }
        public string date { get; set; }
      }
      public class JSONFilter
      {
        public string name { get; set; }
        public int id { get; set; }
        public List<int> ids { get; set; }
      }


      //Subclasses
      public class WidgetLibraryBase
      {
          public string title { get; set; }
          public string link { get; set; }
      }
      public class WidgetLibCol : WidgetLibraryBase
      {
          public WidgetLibCol()
          {
              items = new List<object>();
              collections = new List<WidgetResource>();
          }
          public bool showHeader { get; set; }
          public bool showCollections { get; set; }
          public List<object> items { get; set; }
          public List<WidgetResource> collections { get; set; }
      }

      public class WidgetResource : WidgetLibraryBase
      {
          public string thumbURL { get; set; }
      }
      #endregion

  }
}
