using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Drawing;

using LRWarehouse.Business.ResourceV2;
using Isle.BizServices;


namespace IOER.Controls.SearchV7
{
	public partial class SearchV7 : System.Web.UI.UserControl
	{
		/* --- Initialization --- */
		public SearchV7()
		{
			Theme = new Themes.uber();
			Config = new Themes.SearchConfig();
			Filters = new List<FieldDB>();
			LibraryIds = new List<int>();
			CollectionIds = new List<int>();
		}

		/* --- Managers/Helper Classes --- */
		JavaScriptSerializer serializer = new JavaScriptSerializer();
		ResourceV2Services resourceService = new ResourceV2Services();

		/* --- Initialization --- */
		public string LoadTheme { get; set; }

		/* --- Properties --- */
		public Themes.SearchTheme Theme { get; set; }
		public Themes.SearchConfig Config { get; set; }
		public List<FieldDB> Filters { get; set; }
		public string UsageRightsIconsJSON { get; set; }
		public string InitialResultsJSON { get; set; }
		public List<int> LibraryIds { get; set; }
		public List<int> CollectionIds { get; set; }
		public string PreselectedText { get; set; }
		public List<int> PreselectedTagIDs { get; set; }
		public bool UseAllFiltersAndTags { get; set; }

		/* --- Page_Load --- */
		protected void Page_Load( object sender, EventArgs e )
		{
			//Get inputs
			GetInputs();

			//Get config
			GetConfig();

			//Get filters
			Filters = GetFilters();

			//Load additional data
			LoadData();

		} //End Page_Load

		//Get inputs and initialize stuff
		public void GetInputs()
		{
			//Theme
			var targetTheme = string.IsNullOrWhiteSpace( LoadTheme ) ? GetParam( "theme", "ioer" ) : LoadTheme;
			//Hack
			if ( targetTheme == "wn_search" ) { targetTheme = "worknet"; }
			//End Hack

			//Theme
			Theme = LoadControl( "Themes/" + targetTheme + ".ascx" ) as Themes.SearchTheme;
			themeBox.Controls.Add( Theme );

			//Library and Collection IDs
			LibraryIds = ServiceHelper.CommaSeparatedListToIntegerList( GetParam( "libraryIDs", "" ) );
			CollectionIds = ServiceHelper.CommaSeparatedListToIntegerList( GetParam( "collectionIDs", "" ) );

			//Others
			PreselectedText = GetParam( "text", "" );
			PreselectedTagIDs = ServiceHelper.CommaSeparatedListToIntegerList( GetParam( "tagIDs", "" ) );
			UseAllFiltersAndTags = GetParam( "allFields", "false" ).ToLower() == "true";
		}

		//Get theme and additional tweaks
		public void GetConfig()
		{
			//Get main configuration
			Config = Theme.GetSearchConfig();

			//Override colors via parameters
			Config.ThemeColorMain = ColorTranslator.FromHtml( "#" + GetParam( "mainColor", ColorTranslator.ToHtml( Config.ThemeColorMain ).Replace( "#", "" ) ) );
			Config.ThemeColorHeader = ColorTranslator.FromHtml( "#" + GetParam( "headerColor", ColorTranslator.ToHtml( Config.ThemeColorHeader ).Replace( "#", "" ) ) );
			Config.ThemeColorSelected = ColorTranslator.FromHtml( "#" + GetParam( "selectedColor", ColorTranslator.ToHtml( Config.ThemeColorSelected ).Replace( "#", "" ) ) );

			//Override other things
			Config.SearchTitle = GetParam( "title", Config.SearchTitle );
			Config.HasStandards = GetParam( "standards", ( Config.HasStandards ? "true" : "false" ) ) == "true" ? true : false;
			Config.UseResourceUrl = GetParam( "directLinks", ( Config.UseResourceUrl ? "true" : "false" ) ) == "true" ? true : false;
		}

		//Load helper data
		public void LoadData()
		{
			//Usage Rights JSON
			var rightsData = new ResourceV2Services().GetUsageRightsList();
			var jsonRights = new List<HelperRights>();
			foreach ( var item in rightsData.Where( m => !string.IsNullOrWhiteSpace( m.Url ) ).ToList() )
			{
				jsonRights.Add( new HelperRights() { Url = item.Url, MiniIconUrl = item.MiniIconUrl, Title = item.Description } );
			}
			UsageRightsIconsJSON = serializer.Serialize( jsonRights );

			//Auto newest search JSOn
			if ( Config.DoPreloadNewestSearch )
			{
				InitialResultsJSON = Theme.GetPreloadedNewestResults();
			}
			else
			{
				InitialResultsJSON = "{}";
			}
		} //

		//Determine what filters to use
		public List<FieldDB> GetFilters()
		{
			var starterList = new List<FieldDB>();

			//Get starting set of filters
			if ( UseAllFiltersAndTags )
			{
				starterList = resourceService.GetFieldAndTagCodeData();
			}
			else
			{
				starterList = resourceService.GetFieldsForLibCol( LibraryIds, CollectionIds, Config.SiteId );
			}

			//Trim down to just the allowed ones
			var allowedFields = Config.AllFieldSchemas;
			var finalFields = new List<FieldDB>();

			//Uber
			if ( allowedFields.Count() == 0 )
			{
				finalFields = starterList;
			}
			//Everything else
			else
			{
				foreach ( var item in allowedFields )
				{
					var targetField = starterList.Where( m => m.Schema == item ).FirstOrDefault();
					if ( targetField != null )
					{
						finalFields.Add( targetField );
					}
				}
			}

			finalFields = finalFields.OrderBy( m => m.SortOrder ).ToList();

			return finalFields;

		} //

		#region Helper Methods and Classes
		public string GetParam(string name, string fallback)
		{
			var requestData = Request.Params[ name ];
			var routeData = (string) Page.RouteData.Values[ name ];
			if( !string.IsNullOrWhiteSpace( requestData ) )
			{
				return requestData;
			}
			else if ( !string.IsNullOrWhiteSpace( routeData ) )
			{
				return routeData;
			}
			else
			{
				return fallback;
			}
		}

		public class HelperRights
		{
			public string Url { get; set; }
			public string MiniIconUrl { get; set; }
			public string Title { get; set; }
		}

		#endregion
	} //End class

}