using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using Isle.BizServices;
using LRWarehouse.Business;
using GenericReturn = ILPathways.Services.UtilityService.GenericReturn;

namespace ILPathways.Services
{
  /// <summary>
  /// Summary description for LibraryAJAXService
  /// </summary>
  [WebService( Namespace = "http://tempuri.org/" )]
  [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
  [System.ComponentModel.ToolboxItem( false )]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  [System.Web.Script.Services.ScriptService]
  public class LibraryAJAXService : System.Web.Services.WebService
  {
    LibraryBizService svcLibrary = new LibraryBizService();
    UtilityService svcUtility = new UtilityService();
    private bool valid = true;
    private string status = "";

    [WebMethod]
    public string HelloWorld()
    {
      return "Hello World";
    }

    [WebMethod]
    public GenericReturn LoadLibraryPage( int id )
    {
      var data = LoadLibraryPage( id, ref valid, ref status );
      return new GenericReturn() { data = data, valid = valid, status = status, extra = null };
    }
    public LibraryPageDTO LoadLibraryPage( int id, ref bool valid, ref string status )
    {
      //Holds output
      var dto = new LibraryPageDTO();
      //Get main library data
      var libraryData = svcLibrary.Get( id );
      //Validate library data
      if ( libraryData == null || libraryData.Id == 0 )
      {
        valid = false;
        status = "Unable to retrieve Library";
        return null;
      }

      //Get User data
      var user = svcUtility.GetUser( false );

      //

      /* This area handles the currently-displayed Library */
      //Ensure that the library is public or that the user has access to it
      if ( libraryData.PublicAccessLevel <= Business.EObjectAccessLevel.ByRequestOnly && 
        !svcLibrary.Library_DoesUserHaveContributeAccess(id, user.Id) )
      {
        valid = false;
        status = "You don't have permission to access that Library";
        return null;
      }

      //Get collections the user has edit access to
      var myCollections = svcLibrary.LibrarySections_SelectListWithContributeAccess( id, user.Id );
      //Get public collections
      var publicCollections = svcLibrary.LibrarySectionsSelectList( id, 1 );
      //Merge the two lists while avoiding duplicates
      var publicCollectionIDs = publicCollections.Select(i => i.Id).ToList();
      var collections = publicCollections.Concat( myCollections.Where( m => !publicCollectionIDs.Contains( m.Id ) ) ).ToList();
      
      //Populate the currently-displayed library
      dto.CurrentLibrary = PopulateLibraryDTO( libraryData, user, collections, true );

      //
      
      /* This area handles the libraries for which the user has contribute access in order to allow copying resources */
      var myLibraries = svcLibrary.Libraries_SelectListWithContributeAccess( user.Id );
      foreach ( var item in myLibraries )
      {
        dto.MyLibraries.Add(
          PopulateLibraryDTO(item, user, svcLibrary.LibrarySections_SelectListWithContributeAccess(item.Id, user.Id), false)
        );
      }

      //

      return dto;
    }

    #region Helper Methods
    private LibraryDTO PopulateLibraryDTO( Business.Library input, Patron user, List<Business.LibrarySection> collections, bool includeComments )
    {
      var output = new LibraryDTO()
      {
        Id = input.Id,
        Title = input.Title,
        Description = input.Description,
        ImageUrl = input.ImageUrl,
        CanEdit = input.CanEdit,
        PublicAccessLevel = input.PublicAccessLevelInt,
        OrgAccessLevel = input.OrgAccessLevelInt,
        Comments = includeComments ? PopulateCommentDTOs( svcLibrary.LibraryComment_Select( input.Id ) ) : new List<CommentDTO>()
      };

      if ( collections != null )
      {
        foreach ( var item in collections )
        {
          output.Collections.Add( new CollectionDTO()
          {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            ImageUrl = item.ImageUrl,
            CanEdit = item.CanEdit,
            PublicAccessLevel = ( int ) item.PublicAccessLevel,
            OrgAccessLevel = ( int ) item.OrgAccessLevel,
            Comments = includeComments ? PopulateCommentDTOs( svcLibrary.LibrarySectionComment_Select( item.Id ) ) : new List<CommentDTO>()
          } );
        }
      }

      return output;
    }

    private List<CommentDTO> PopulateCommentDTOs( List<Business.ObjectComment> input )
    {
      var output = new List<CommentDTO>();
      if ( input != null )
      {
        foreach ( var item in input )
        {
          output.Add( new CommentDTO()
          {
            Id = item.Id,
            Commenter = item.CreatedBy,
            Comment = item.Message,
            Date = item.Created.ToShortDateString()
          } );
        }
      }
      return output;
    }
    #endregion

    #region DTO Classes
    //DTO sent once when the page loads. Contains all data needed to load the page
    public class LibraryPageDTO
    {
      public LibraryPageDTO()
      {
        CurrentLibrary = new LibraryDTO();
        MyLibraries = new List<LibraryDTO>();
      }
      //The library currently being viewed
      public LibraryDTO CurrentLibrary { get; set; }
      //A list of libraries the user has edit access to. Will not be as thoroughly filled out
      public List<LibraryDTO> MyLibraries { get; set; }

    }

    //Base class for Library and Collection DTOs
    public class LibColBase 
    {
      public LibColBase()
      {
        Comments = new List<CommentDTO>();
      }
      public int Id { get; set; }
      public string Title { get; set; }
      public string Description { get; set; }
      public string ImageUrl { get; set; }
      //Indicates the user's access level to this Library/Collection
      public bool CanEdit { get; set; }
      //Indicates the access level setting for this Library/Collection
      public int PublicAccessLevel { get; set; }
      //Indicates the organization access level setting for this Library/Collection
      public int OrgAccessLevel { get; set; }
      //A list of comments on this Library/Collection
      public List<CommentDTO> Comments { get; set; }
    }

    //DTO sent when library is updated
    public class LibraryDTO : LibColBase
    {
      public LibraryDTO()
      {
        Collections = new List<CollectionDTO>();
      }
      //List of collections this Library has
      public List<CollectionDTO> Collections { get; set; }
    }

    //DTO sent when collection is updated
    public class CollectionDTO : LibColBase
    { 

    }

    //Simple comment data
    public class CommentDTO
    {
      public int Id { get; set; }
      public string Commenter { get; set; }
      public string Comment { get; set; }
      public string Date { get; set; }
    }
    #endregion
  }
}
