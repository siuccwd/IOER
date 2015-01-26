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
      GetNewestResources();
      GetMostCommentedResources();
      GetCommunityPosts();
      GetFeaturedResources();
      if ( IsUserAuthenticated() )
      {
        GetFollowedResources();
      }
    }

    public void GetCommunityPosts()
    {
      communityPosts = new ILPathways.Services.CommunityService().GetRecentPosts( 1, 5 );
    }

    public void GetNewestResources()
    {
      var input = GetSortedInput( "intID", "desc" );
      newestResources = searchService.DoSearch3( input );
    }

    public void GetMostCommentedResources()
    {
      var input = GetSortedInput( "commentsCount", "desc" );
      mostCommentedResources = searchService.DoSearch3( input );
    }

    public void GetFeaturedResources()
    {
      var libFilter = new Services.ElasticSearchService.jsonFilter();
      libFilter.field = "libraryIDs";
      libFilter.es = "libraryIDs";
      libFilter.items.Add( new Services.ElasticSearchService.jsonFilterItem()
      {
        id = 286,
        text = "SIUC IOER"
      } );
      var colFilter = new Services.ElasticSearchService.jsonFilter();
      colFilter.field = "collectionIDs";
      colFilter.es = "collectionIDs";
      colFilter.items.Add( new Services.ElasticSearchService.jsonFilterItem()
      {
        id = 532,
        text = "Featured Resources"
      } );

      /*var libService = new Isle.BizServices.LibraryBizService();
      var myLibraries = libService.Library_SelectListWithContributeAccess( WebUser.Id );
      foreach ( LibrarySummaryDTO library in myLibraries )
      {
        var lib = new Services.ElasticSearchService.jsonFilterItem();
        lib.id = library.Id;
        lib.text = library.Title;
        libFilter.items.Add( lib );
      }*/

      var input = GetSortedInput( "intID", "desc" );
      input.narrowingOptions.idFilters.Add( libFilter );
      input.narrowingOptions.idFilters.Add( colFilter );

      featuredResources = searchService.DoSearch3( input );
    }

    public void GetFollowedResources()
    {
      var libFilter = new Services.ElasticSearchService.jsonFilter();
      libFilter.field = "libraryIDs";
      libFilter.es = "libraryIDs";

      var libService = new Isle.BizServices.LibraryBizService();
      var myLibraries = libService.Library_SelectListWithContributeAccess( WebUser.Id );
      foreach(LibrarySummaryDTO library in myLibraries){
        var lib = new Services.ElasticSearchService.jsonFilterItem();
        lib.id = library.Id;
        lib.text = library.Title;
        libFilter.items.Add( lib );
      }

      var input = GetSortedInput( "intID", "desc" );
      input.narrowingOptions.idFilters.Add( libFilter );

      followedResources = searchService.DoSearch3( input );
    }

    public Services.ElasticSearchService.JSONQuery2 GetSortedInput( string field, string order )
    {
      var input = new Services.ElasticSearchService.JSONQuery2();
      input.sort = new Services.ElasticSearchService.jsonSortingOptions() { field = field, order = order };
      input.searchText = "* -freesound -delete -bookshare -\"Smarter Balanced Assessment Consortium\"";
      input.size = 10;

      return input;
    }
  }
}