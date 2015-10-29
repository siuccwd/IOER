using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Script.Serialization;

using LRWarehouse.Business.ResourceV2;
using Isle.BizServices;
using System.Drawing;
using System.Web.Routing;


namespace IOER.Controls.SearchV6
{
  public partial class SearchV6 : System.Web.UI.UserControl
  {
    /* --- Initialization --- */
    public SearchV6()
    {
      Filters = new List<FieldDB>();
      LibraryIds = new List<int>();
      CollectionIds = new List<int>();
    }

    /* --- Managers/Helper Classes --- */
    JavaScriptSerializer serializer = new JavaScriptSerializer();

    /* --- Properties --- */
    public List<FieldDB> Filters { get; set; }
    public List<int> LibraryIds { get; set; }
    public string JSON_LibraryIds { get; set; }
    public List<int> CollectionIds { get; set; }
    public string JSON_CollectionIds { get; set; }
    public string PreselectedText { get; set; }
    public List<int> PreselectedTags { get; set; }
    public string AjaxSearchUrl { get; set; }
    public bool UseResourceUrl { get; set; }
    public bool UseStandards { get; set; }
    public string ThemeName { get; set; }
    public int SiteId { get; set; }
    public bool UseAllFiltersAndTags { get; set; }
    public string ViewMode { get; set; }
    public Color MainColor { get; set; }
    public string SortOrder { get; set; }
		public string DoAutoNewestSearch { get; set; }
		public string UsageRightsIconsJSON { get; set; }
		public string InitialResultsJSON { get; set; }

    /* --- Methods --- */
    protected void Page_Load( object sender, EventArgs e )
    {
      GetPageData();
      LoadTheme();
      GetFields();
    }

    private void GetPageData()
    {
      //TODO: web.configify this
      AjaxSearchUrl = "/Services/ElasticSearchService.asmx/DoSearchCollection7";

      //Add preselected text
      PreselectedText = Request.Params[ "text" ] ?? "";

      //Get library and/or collection IDs as applicable. Supports multiple values for either.
      LibraryIds = ServiceHelper.CommaSeparatedListToIntegerList( Request.Params[ "libraryIDs" ] ?? "" );
      JSON_LibraryIds = serializer.Serialize( LibraryIds );
			CollectionIds = ServiceHelper.CommaSeparatedListToIntegerList( Request.Params[ "collectionIDs" ] ?? ( string ) Request.RequestContext.RouteData.Values[ "collectionIDs" ] ?? "" );
      JSON_CollectionIds = serializer.Serialize( CollectionIds );

      //Determine whether or not to use standards browser
      UseStandards = bool.Parse( Request.Params[ "standards" ] ?? "true" );

      //Determine whether or not to override field selection
      UseAllFiltersAndTags = bool.Parse( Request.Params[ "allFilters" ] ?? "false" );

      //Determine preselected view mode
      ViewMode = ( Request.Params[ "viewMode" ] ?? "list" );

      //Preselected tags to use
			PreselectedTags = ServiceHelper.CommaSeparatedListToIntegerList( Request.Params[ "tagIDs" ] ?? ( string ) Request.RequestContext.RouteData.Values[ "tagIDs" ] ?? "" );

      //Determine preselected sort order if it hasn't been set by an external control
			if ( string.IsNullOrWhiteSpace( SortOrder ) )
			{
				SortOrder = ( Request.Params[ "sort" ] ?? "" );
			}
			//Skip auto newest search in some cases
			if ( string.IsNullOrWhiteSpace( DoAutoNewestSearch ) || DoAutoNewestSearch.ToLower().Trim() == "true" )
			{
				DoAutoNewestSearch = "true";
			}
			else
			{
				DoAutoNewestSearch = "false";
			}

			//Get usage rights data
			var rightsData = new ResourceV2Services().GetUsageRightsList();
			var jsonRights = new List<HelperRights>();
			foreach ( var item in rightsData.Where( m => !string.IsNullOrWhiteSpace( m.Url ) ).ToList() )
			{
				jsonRights.Add( new  HelperRights() { Url = item.Url, MiniIconUrl = item.MiniIconUrl, Title = item.Description } );
			}
			UsageRightsIconsJSON = serializer.Serialize( jsonRights );

    }

    //Get the available filters and tags for the set of selected libraries and collections, then compare them to the allowed fields for the current theme and merge as appropriate
    private void GetFields()
    {
      if ( ( LibraryIds.Count() > 0 || CollectionIds.Count() > 0 ) && !UseAllFiltersAndTags )
      {
        //Get the appropriate fields for the current selection of libraries and collections
        var limitedFields = new ResourceV2Services().GetFieldsForLibCol( LibraryIds, CollectionIds, SiteId );
        var finalFields = new List<FieldDB>();
        //For each item in the allowed filters for this theme...
        foreach ( var item in Filters )
        {
          //Find the matching limited field
          var targetField = limitedFields.Where(i => i.Id == item.Id).FirstOrDefault();
          if ( targetField != null )
          {
            //If found, add the limited field (which in turn means the tags will be limited appropriately)
            finalFields.Add( targetField );
          }
        }
        //Set the fields
        Filters = finalFields.OrderBy( m => m.SortOrder ).ToList();
      }
    }

    private void LoadTheme()
    {
      //Get theme parameter
      var theme = Request.Params[ "theme" ] ?? ThemeName;
      if ( string.IsNullOrWhiteSpace( theme ) ) { theme = "ioer"; }

      //Load theme control
      var control = LoadControl( "Themes/" + theme + ".ascx" ) as Themes.SearchTheme;
      themesBox.Controls.Add( control );

      //Get Filters
      Filters = control.GetFields().OrderBy( m => m.SortOrder ).ToList();

      //Get Site ID
      SiteId = control.GetSiteId();

      //Determine whether or not to use the resource url
      if ( Request.Params[ "directLinks" ] == null )
      {
        //If there is no URL parameter, use theme default
        UseResourceUrl = control.GetUseResourceUrl();
      }
      else
      {
        UseResourceUrl = bool.Parse( Request.Params[ "directLinks" ] ?? "true" );
      }

      //Override main color if desired
      try
      {
        var targetColor = "#" + ( Request.Params[ "mainColor" ] ?? "" );
        control.SetMainColor( targetColor == "#" ? ColorTranslator.ToHtml( control.GetMainColor() ) : targetColor );
      }
      catch { }

			//Default initial data
			InitialResultsJSON = control.GetInitialSearchDataJSON();

    }

    /* --- Helper Methods --- */
    
		/* --- Helper Classes --- */
		public class HelperRights
		{
			public string Url { get; set; }
			public string MiniIconUrl { get; set; }
			public string Title { get; set; }
		}
  }
}