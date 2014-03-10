using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using System.Web.Script.Serialization;
using Isle.BizServices;

namespace ILPathways.Controls
{
  public partial class Splash2 : BaseUserControl
  {
    public string newestResources { get; set; }
    public string followedResources { get; set; }
    public string mostCommentedResources { get; set; }
    Services.ElasticSearchService searchService = new Services.ElasticSearchService();

    protected void Page_Load( object sender, EventArgs e )
    {
      newestResources = "[]";
      followedResources = "[]";
      mostCommentedResources = "[]";
      GetNewestResources();
      GetMostCommentedResources();
      if ( IsUserAuthenticated() )
      {
        GetFollowedResources();
      }
    }

    public void GetNewestResources()
    {
      var input = GetSortedInput( "timestamp", "desc" );
      newestResources = searchService.DoSearch3( input );
    }

    public void GetMostCommentedResources()
    {
      var input = GetSortedInput( "commentsCount", "desc" );
      mostCommentedResources = searchService.DoSearch3( input );
    }

    public void GetFollowedResources()
    {
      var libFilter = new Services.ElasticSearchService.jsonFilter();
      libFilter.field = "libraryIDs";
      libFilter.es = "libraryIDs";

      var libService = new Isle.BizServices.LibraryBizService();
      var myLibraries = libService.Library_SelectListWithContributeAccess( WebUser.Id );
      foreach(ILPathways.Business.Library library in myLibraries){
        var lib = new Services.ElasticSearchService.jsonFilterItem();
        lib.id = library.Id;
        lib.text = library.Title;
        libFilter.items.Add( lib );
      }

      var input = GetSortedInput( "timestamp", "desc" );
      input.narrowingOptions.idFilters.Add( libFilter );

      followedResources = searchService.DoSearch3( input );
    }

    public Services.ElasticSearchService.JSONQuery2 GetSortedInput( string field, string order )
    {
      var input = new Services.ElasticSearchService.JSONQuery2();
      input.sort = new Services.ElasticSearchService.jsonSortingOptions() { field = field, order = order };
      input.searchText = "* -freesound -delete -bookshare";
      input.size = 10;

      return input;
    }
  }
}