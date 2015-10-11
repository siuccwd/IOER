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

namespace IOER.Services
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
    //List<Patron> foundUsers = new List<Patron>();

    #region Get Methods

    [WebMethod]
    public string GetCommunity( string userGUID, int id )
    {
      var user = utilService.GetUserFromGUID( userGUID );
      var community = GetJSONCommunity( user, id );
      if ( community == null || community.id == 0 )
      {
        return utilService.ImmediateReturn( "", false, "Community not found.", null );
      }
      else
      {
        return utilService.ImmediateReturn( community, true, "okay", null );
      }
    }

    public JSONCommunity GetJSONCommunity( Patron user, int id )
    {
        var output = new JSONCommunity();
        //mp - updated target method to return user name and avatar
        var com = comService.Community_Get( id );

        output.id = com.Id;
        output.title = com.Title;
        output.description = com.Description;
        output.created = com.Created.ToShortDateString();
        output.image = com.ImageUrl;

        output.userIsFollowing = comService.Community_MemberIsMember( id, user.Id );

        foreach ( CommunityPosting posting in com.Postings )
        {
            if ( posting.RelatedPostingId == 0 ) //Top-level post
            {
                //build parent
                var post = BuildPost( posting );
                if ( posting.ChildPostings != null && posting.ChildPostings.Count > 0 )
                {
                    foreach ( CommunityPosting nextPosting in posting.ChildPostings ) //Get children of this top level post
                    {
                        var childPost = BuildPost( nextPosting );
                        post.responses.Add( childPost );
                    }
                }
                //post.responses.Reverse();

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
        post.postType = posting.PostingType;
        post.postTypeID = posting.PostingTypeId;

        post.poster = posting.UserFullName;
        post.posterAvatar = posting.UserImageUrl;
        if ( post.posterAvatar == "" )
        {
            post.posterAvatar = "/images/defaultProfileImg.jpg";
        }

        return post;
    }

    [WebMethod]
    public string GetRecentPosts( int communityID, int numberOfPosts )
    {
      var output = GetRecentPostsJSON( communityID, numberOfPosts );

      return utilService.ImmediateReturn( output, true, "okay", null );
    }
    public List<JSONCommunityPosting> GetRecentPostsJSON( int communityID, int numberOfPosts )
    {
      var data = comService.Community_Get( communityID, numberOfPosts );
      var output = new List<JSONCommunityPosting>();

      foreach ( CommunityPosting posting in data.Postings )
      {
        output.Add( BuildPost( posting ) );
      }

      return output;
    }

    private void setupUserInfo2( Patron user, ref JSONCommunityPosting post )
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
    [WebMethod]
    public string PostMessageFromTimeline( string userGUID, int communityID, int parentID, string text, bool isMyTimeline )
    {
      bool isValid = false;
      string status = "";
      var user = utilService.GetUserFromGUID( userGUID );

      PostMessageSilently( user, communityID, parentID, 25, text, ref isValid, ref status );
      if ( !isValid )
      {
        return utilService.ImmediateReturn( "", false, status, null );
      }

      var item = new IOER.Controls.Activity1();

      //Determine what to show
      var activities = new List<ObjectActivity>();
      if ( isMyTimeline )  //User wants their network
      {
        activities = ActivityBizServices.ObjectActivity_MyFollowingSummary( item.historyDays, user.Id, item.maxPosts );
      }
      else
      {
        activities = ActivityBizServices.ObjectActivity_RecentList( item.historyDays, user.Id, item.maxPosts );
      }

      item.LoadActivities( activities );
      return utilService.ImmediateReturn( item.eventRanges, isValid, status, null );

    }
    public JSONCommunity PostMessage( Patron user, int communityID, int parentID, int minimumLength, string text, ref bool isValid, ref string status )
    {
      var output = new JSONCommunity();

      PostMessageSilently( user, communityID, parentID, minimumLength, text, ref isValid, ref status );
      if ( !isValid )
      {
        return output;
      }

      return GetJSONCommunity( user, communityID );
    }
    public void PostMessageSilently( Patron user, int communityID, int parentID, int minimumLength, string text, ref bool isValid, ref string status )
    {
      //Validate user
      if ( user.Id == 0 || !user.IsValid )
      {
        isValid = false;
        status = "You must login to post!";
        return;
      }
      bool allowingHtmlPosts = ServiceHelper.GetAppKeyValue("allowingHtmlPosts", false);
      //Validate text
      text = utilService.ValidateText( text, minimumLength, "Message", allowingHtmlPosts, ref isValid, ref status );
      if ( !isValid )
      {
        return;
      }
		//need to handle images - or with css
	  if ( text.ToLower().IndexOf( "style=\"width: 558px;\"" ) > -1 )
	  {
		  text = text.Replace( "width: 558px", "width: 90%" );
	  }
      //Create the posting
      comService.PostingAdd( communityID, text, user.Id, parentID );
      isValid = true;
      status = "okay";

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

      return GetJSONCommunity( user, communityID );
    }

    [WebMethod]
    public string LockThread( string userGUID, int postID, int communityID )
    {
      var user = utilService.GetUserFromGUID( userGUID );
      bool isValid = false;
      string status = "";

      JSONCommunity community = LockThread( user, postID, communityID, ref isValid, ref status );

      return utilService.ImmediateReturn( community, isValid, status, null );
    }
    public JSONCommunity LockThread( Patron user, int postID, int communityID, ref bool isValid, ref string status )
    {
      var output = new JSONCommunity();

      //Validate user
      if ( user.Id == 0 || !user.IsValid )
      {
        isValid = false;
        status = "You must login to lock threads!";
        return output;
      }

      //Validate that the user can delete the post in question
      var targetPost = comService.Posting_Get( postID );

      if ( targetPost.CreatedById == user.Id || utilService.isUserAdmin( user ) )
      {
        //Lock the thread
        
      }
      else
      {
        isValid = false;
        status = "You are not authorized to lock that!";
        return output;
      }

      isValid = true;
      status = "okay";

      return GetJSONCommunity( user, communityID );

    }


    [WebMethod]
    public string ToggleFollowing( string userGUID, int communityID )
    {
      var user = utilService.GetUserFromGUID( userGUID );
      bool isValid = false;
      string status = "";

      bool isFollowingNow = ToggleFollowing(user, communityID, ref isValid, ref status);

      return utilService.ImmediateReturn( isFollowingNow, isValid, status, null );
    }
    public bool ToggleFollowing( Patron user, int communityID, ref bool isValid, ref string status )
    {
      var output = false;

      //Validate user
      if ( user.Id == 0 || !user.IsValid )
      {
        isValid = false;
        status = "You must login to follow!";
        return output;
      }

      //Toggle the following
      if ( comService.Community_MemberIsMember( communityID, user.Id ) )
      {
        new comService().Community_MemberDelete( communityID, user.Id );
        output = false;
      }
      else
      {
        new comService().Community_MemberAdd( communityID, user.Id );
        output = true;
      }

      isValid = true;
      status = "okay";
      return output;
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
      public bool userIsFollowing { get; set; }
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
      public int postTypeID { get; set; }
      public string postType { get; set; }
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
