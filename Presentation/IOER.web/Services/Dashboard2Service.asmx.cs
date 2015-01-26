using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using System.Web.Script.Serialization;
using LRWarehouse.Business;
using ILPathways.Services;
using LRWarehouse.DAL;
using Isle.BizServices;

namespace ILPathways.Services
{
  /// <summary>
  /// Summary description for Dashboard2Service
  /// </summary>
  [WebService( Namespace = "http://tempuri.org/" )]
  [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
  [System.ComponentModel.ToolboxItem( false )]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  [System.Web.Script.Services.ScriptService]
  public class Dashboard2Service : System.Web.Services.WebService
  {
    JavaScriptSerializer serializer = new JavaScriptSerializer();
    UtilityService util = new UtilityService();
    LibraryBizService libService = new LibraryBizService();

    #region Get Methods
    [WebMethod]
    public string GetUserProfile( string userGUID )
    {
      var user = util.GetUserFromGUID( userGUID );
      if ( user.Id == 0 )
      {
        return util.ImmediateReturn( "", false, "User not found.", null );
      }

      return serializer.Serialize( GetUserProfile( user ) );
    }
    public JSONUserProfile GetUserProfile( Patron user )
    {
      var ret = new JSONUserProfile();
      ret.name = user.FullName();
      ret.description = "demo profile info";
      ret.userGUID = user.RowId.ToString();
      ret.avatarURL = "/images/ioer_med.png";

      return ret;
    }

    [WebMethod]
    public string GetMyLibraryThumbs( string userGUID )
    {
      var user = util.GetUserFromGUID( userGUID );
      if ( user.Id == 0 )
      {
        return util.ImmediateReturn( "", false, "User not found.", null );
      }

      bool isValid = true;
      string status = "";

      var libData = GetMyLibraryThumbs( user, ref isValid, ref status );
      if ( !isValid )
      {
        return util.ImmediateReturn( "", false, status, null );
      }

      return serializer.Serialize( libData );
      
    }
    public List<JSONThumbnailItem> GetMyLibraryThumbs( Patron user, ref bool isValid, ref string status )
    {
      var list = new List<JSONThumbnailItem>();
      var library = libService.GetMyLibrary( user );
      if ( library == null || !library.IsValid || library.Id == 0 )
      {
        isValid = false;
        status = "You don't have a Library yet.";
        return list;
      }

      var libItem = new JSONThumbnailItem();
      libItem.imageURL = library.ImageUrl;
      libItem.link = "/Libraries/Library.aspx?id=" + library.Id;
      libItem.title = library.Title;
      list.Add( libItem );

      var cols = libService.LibrarySections_SelectListWithEditAccess( library.Id, user.Id );
      foreach ( ILPathways.Business.LibrarySection section in cols )
      {
        var col = new JSONThumbnailItem();
        col.imageURL = ( section.ImageUrl == "" ? section.AvatarURL : section.ImageUrl );
        col.link = "/Libraries/Library.aspx?id=" + library.Id + "&col=" + section.Id;
        col.title = section.Title;
        list.Add( col );
      }

      return list;
    }

    [WebMethod]
    public string GetMyResourceThumbs( string userGUID )
    {
      var user = util.GetUserFromGUID( userGUID );
      if ( user.Id == 0 )
      {
        return util.ImmediateReturn( "", false, "User not found.", null );
      }

      bool isValid = true;
      string status = "";
      return serializer.Serialize( GetMyResourceThumbs( user, 10, ref isValid, ref status ) );
    }
    public List<JSONThumbnailItem> GetMyResourceThumbs( Patron user, int count, ref bool isValid, ref string status )
    {
      var list = new List<JSONThumbnailItem>();

      return list;
    }


    #endregion
    #region Helper Methods

    public class JSONUserProfile
    {
      public string userGUID { get; set; }
      public string name { get; set; }
      public string description { get; set; }
      public string avatarURL { get; set; }
    }

    #endregion

    #region subclasses
    public class JSONThumbnailItem
    {
      public string imageURL { get; set; }
      public string title { get; set; }
      public string link { get; set; }
    }

    #endregion
  }
}
