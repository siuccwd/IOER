using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Script.Serialization;

using IOER.Library;
using Isle.BizServices;

namespace IOER.Controls
{
	public partial class Splash3 : BaseUserControl
	{
		JavaScriptSerializer serializer = new JavaScriptSerializer();
		Services.ElasticSearchService searchService = new Services.ElasticSearchService();

		public string followedLibraryIDs { get; set; }
		public string followedLibraries { get; set; }
		public string newestResources { get; set; }
		public string mostCommentedResources { get; set; }
		public string featuredResources { get; set; }
		public string featuredLearningLists { get; set; }
		public string featuredLibraries { get; set; }
		public string communityPosts { get; set; }

		protected void Page_Load( object sender, EventArgs e )
		{
			//If user is authenticated, try to get any followed libraries
			if ( IsUserAuthenticated() )
			{
				var myFollowedLibraryIDs = new LibraryBizService().Library_SelectLibrariesFollowing( WebUser.Id ).LibraryIds;
				followedLibraryIDs = serializer.Serialize( myFollowedLibraryIDs );
				var input = GetSortedInputV7( "ResourceId", "desc" );
				input.libraryIDs = myFollowedLibraryIDs;
				followedLibraries = ( string ) searchService.DoSearchCollection7( input ).data;
			}
			else
			{
				followedLibraryIDs = "[]";
				followedLibraries = "null";
			}

			GetNewestResourcesV7();
			GetMostCommentedResourcesV7();
			GetFeaturedResourcesV7();
			GetFeaturedLearningListsV7();
			GetFeaturedLibrariesV7();
			GetCommunityPosts();

		}

		public void GetNewestResourcesV7()
		{
			var input = GetSortedInputV7( "ResourceId", "desc" );
			input.size = 20;
			newestResources = ( string ) searchService.DoSearchCollection7( input ).data;
		}

		public void GetMostCommentedResourcesV7()
		{
			var input = GetSortedInputV7( "Paradata.Comments", "desc" );
			mostCommentedResources = ( string ) searchService.DoSearchCollection7( input ).data;
		}

		public void GetFeaturedResourcesV7()
		{
			var input = GetSortedInputV7( "ResourceId", "desc" );
			input.libraryIDs.Add( 286 );
			input.collectionIDs.Add( 532 );
			input.size = 25;
			featuredResources = ( string ) searchService.DoSearchCollection7( input ).data;
		}

		public void GetFeaturedLearningListsV7()
		{
			var input = GetSortedInputV7( "ResourceId", "desc" );
			input.libraryIDs.Add( 286 );
			input.collectionIDs.Add( 642 );
			input.size = 25;
			featuredLearningLists = ( string ) searchService.DoSearchCollection7( input ).data;
		}

		public void GetFeaturedLibrariesV7()
		{
			var input = GetSortedInputV7( "ResourceId", "desc" );
			input.libraryIDs.Add( 286 );
			input.collectionIDs.Add( 788 );
			input.size = 10;
			featuredLibraries = ( string ) searchService.DoSearchCollection7( input ).data;
		}

		public void GetCommunityPosts()
		{
			communityPosts = new IOER.Services.CommunityService().GetRecentPosts( 1, 5 );
		}

		/// <summary>
		/// Configure options for resource search
		/// </summary>
		/// <param name="field"></param>
		/// <param name="order"></param>
		/// <returns></returns>
		public Services.ElasticSearchService.JSONQueryV7 GetSortedInputV7( string field, string order )
		{
			var input = new Services.ElasticSearchService.JSONQueryV7();
			input.sort = new Services.ElasticSearchService.SortV7() { field = field, order = order };
			input.text = "*";
			input.not = " 'delete'  'freesound'  'smarter balanced' ";
			input.size = 10;
			input.start = 0;

			return input;
		}
	}
}