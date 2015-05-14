using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using System.Web.Script.Serialization;
using Isle.BizServices;
using Isle.DTO;

namespace ILPathways.Controls
{
  public partial class Splash2 : BaseUserControl
  {
    public string newestResources { get; set; }
    public string followedResources { get; set; }
    public string mostCommentedResources { get; set; }
    public string communityPosts { get; set; }
    public string featuredResources { get; set; }
    Services.ElasticSearchService searchService = new Services.ElasticSearchService();

    protected void Page_Load( object sender, EventArgs e )
    {      
      newestResources = "[]";
      followedResources = "[]";
      mostCommentedResources = "[]";
      featuredResources = "[]";
      GetNewestResourcesV7();
      GetMostCommentedResourcesV7();
      GetCommunityPosts();
      GetFeaturedResourcesV7();
      if ( IsUserAuthenticated() )
      {
        GetFollowedResourcesV7();
      }
    }

    public void GetCommunityPosts()
    {
      communityPosts = new ILPathways.Services.CommunityService().GetRecentPosts( 1, 5 );
    }


    public void GetNewestResourcesV7()
    {
      var input = GetSortedInputV7( "ResourceId", "desc" );
      newestResources = ( string ) searchService.DoSearchCollection7( input ).data;
    }

    public void GetMostCommentedResourcesV7()
    {
      var input = GetSortedInputV7( "Paradata.Comments", "desc" );
      mostCommentedResources = ( string ) searchService.DoSearchCollection7( input ).data;
    }

    public void GetFollowedResourcesV7()
    {
      var libService = new Isle.BizServices.LibraryBizService();
      var myLibraries = libService.Library_SelectListWithContributeAccess( WebUser.Id );
      var input = GetSortedInputV7( "ResourceId", "desc" );

      foreach ( LibrarySummaryDTO library in myLibraries )
      {
        input.libraryIDs.Add( library.Id );
      }

      newestResources = ( string ) searchService.DoSearchCollection7( input ).data;
    }

    public void GetFeaturedResourcesV7()
    {
      var input = GetSortedInputV7( "ResourceId", "desc" );
      input.libraryIDs.Add( 286 );
      input.collectionIDs.Add( 532 );
      input.size = 25;
      featuredResources = ( string ) searchService.DoSearchCollection7( input ).data;
    }

    public Services.ElasticSearchService.JSONQueryV7 GetSortedInputV7( string field, string order )
    {
      var input = new Services.ElasticSearchService.JSONQueryV7();
      input.sort = new Services.ElasticSearchService.SortV7() { field = field, order = order };
      input.text = "*";
      input.not = " 'delete'  'freesound'  'bookshare'  'smarter balanced' ";
      input.size = 10;
      input.start = 0;

      return input;
    }
  }
}