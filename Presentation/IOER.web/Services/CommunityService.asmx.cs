using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using Isle.BizServices;
using ILPathways.Business;
using System.Web.Script.Serialization;
using LRWarehouse.Business;

using comService = Isle.BizServices.CommunityServices;

namespace ILPathways.Services
{
  /// <summary>
  /// Summary description for CommunityService
  /// </summary>
  [WebService( Namespace = "http://tempuri.org/" )]
  [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
  [System.ComponentModel.ToolboxItem( false )]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  [System.Web.Script.Services.ScriptService]
  public class CommunityService : System.Web.Services.WebService
  {
    JavaScriptSerializer serializer = new JavaScriptSerializer();
    AccountServices accountServices = new Isle.BizServices.AccountServices();
    UtilityService utilService = new UtilityService();
    List<Patron> foundUsers = new List<Patron>();

    #region Get Methods

    [WebMethod]
    public string GetCommunity( int id )
    {
      var community = GetJSONCommunity( id );
      if ( community == null || community.id == 0 )
      {
        return utilService.ImmediateReturn( "", false, "Community not found.", null );
      }
      else
      {
        return utilService.ImmediateReturn( community, true, "okay", null );
      }
    }
    public JSONCommunity GetJSONCommunity( int id )
    {
      var output = new JSONCommunity();
      var com = comService.Community_Get( id );

      output.id = com.Id;
      output.title = com.Title;
      output.description = com.Description;
      output.created = com.Created.ToShortDateString();
      output.image = com.ImageUrl;

      foreach ( CommunityPosting posting in com.Postings )
      {
        if ( posting.RelatedPostingId == 0 ) //Top-level post
        {
          var post = BuildPost( posting );

          foreach ( CommunityPosting nextPosting in com.Postings ) //Get children of this top level post
          {
            if ( nextPosting.RelatedPostingId == posting.Id )
            {
              var childPost = BuildPost( nextPosting );
              post.responses.Add( childPost );
            }
          }
          post.responses.Reverse();

          output.postings.Add( post );
        }
      }

      return output;
    }

    protected JSONCommunityPosting BuildPost( CommunityPosting posting )
    {
      var post = new JSONCommunityPosting();
      post.communityID = posting.CommunityId;
      post.id = posting.Id;
      post.parentID = posting.RelatedPostingId;
      post.text = posting.Message;
      post.created = posting.Created.ToShortDateString();
      post.createdByID = posting.CreatedById;
      
      //Patron efficiency
      bool found = false;
      foreach ( Patron user in foundUsers )
      {
        if ( user.Id == posting.CreatedById )
        {
          found = true;
          setupUserInfo( user, ref post );
          break;
        }
      }
      if ( !found )
      {
        var user = accountServices.Get( posting.CreatedById );
        setupUserInfo( user, ref post );
        foundUsers.Add( user );
      }
      return post;
    }

    private void setupUserInfo( Patron user, ref JSONCommunityPosting post )
    {
      post.posterAvatar = user.UserProfile.ImageUrl;
      if ( post.posterAvatar == "" )
      {
        post.posterAvatar = "/images/defaultProfileImg.jpg";
      }
      post.poster = user.FullName();
    }

    #endregion

    #region Interactive Methods

    [WebMethod]
    public string PostMessage( string userGUID, int communityID, int parentID, string text )
    {
      var user = utilService.GetUserFromGUID( userGUID );

      bool isValid = false;
      string status = "";
      JSONCommunity community = PostMessage( user, communityID, parentID, 25, text, ref isValid, ref status );

      return utilService.ImmediateReturn( community, isValid, status, null );
    }
    public JSONCommunity PostMessage( Patron user, int communityID, int parentID, int minimumLength, string text, ref bool isValid, ref string status )
    {
      var output = new JSONCommunity();

      //Validate user
      if ( user.Id == 0 || !user.IsValid )
      {
        isValid = false;
        status = "You must login to post!";
        return output;
      }

      //Validate text
      text = utilService.ValidateText( text, minimumLength, "Message", ref isValid, ref status );
      if ( !isValid )
      {
        return output;
      }

      //Create the posting
      comService.PostingAdd( communityID, text, user.Id, parentID );

      return GetJSONCommunity( communityID );
    }

    [WebMethod]
    public string DeletePost( string userGUID, int postID, int communityID )
    {
      var user = utilService.GetUserFromGUID( userGUID );
      bool isValid = false;
      string status = "";

      JSONCommunity community = DeletePost( user, postID, communityID, ref isValid, ref status );

      return utilService.ImmediateReturn( community, isValid, status, null );
    }
    public JSONCommunity DeletePost( Patron user, int postID, int communityID, ref bool isValid, ref string status )
    {
      var output = new JSONCommunity();

      //Validate user
      if ( user.Id == 0 || !user.IsValid )
      {
        isValid = false;
        status = "You must login to delete!";
        return output;
      }

      //Validate that the user can delete the post in question
      var targetPost = comService.Posting_Get( postID );

      if ( targetPost.CreatedById == user.Id || utilService.isUserAdmin( user ) )
      {
        //Delete the post
        comService.Posting_Delete( postID );
      }
      else
      {
        isValid = false;
        status = "You are not authorized to delete that!";
        return output;
      }

      isValid = true;
      status = "okay";

      return GetJSONCommunity( communityID );
    }

    #endregion

    #region Subclasses

    public class JSONCommunity
    {
      public JSONCommunity()
      {
        postings = new List<JSONCommunityPosting>();
      }
      public int id { get; set; }
      public string title { get; set; }
      public string description { get; set; }
      public string created { get; set; }
      public string image { get; set; }
      public List<JSONCommunityPosting> postings { get; set; }
    }
    public class JSONCommunityPosting
    {
      public JSONCommunityPosting()
      {
        responses = new List<JSONCommunityPosting>();
      }
      public int communityID { get; set; }
      public int id { get; set; }
      public int parentID { get; set; }
      public int createdByID { get; set; }
      public string poster { get; set; }
      public string posterAvatar { get; set; }
      public string text { get; set; }
      public string created { get; set; }
      public string image { get; set; }
      public List<JSONCommunityPosting> responses { get; set; }
    }

    #endregion
  }
}
