using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Drawing;

namespace IOER.Controls.SearchV7.Themes
{
	public partial class ioer_library : SearchTheme
	{
		protected void Page_Load( object sender, EventArgs e )
		{

		}

		public override SearchConfig GetSearchConfig()
		{
			return new SearchConfig()
			{
				SearchTitle = searchTitle.Text,
				AllFieldSchemas = fieldSchemas.Text.Split( ',' ).ToList(),
				AdvancedFieldSchemas = advancedFieldSchemas.Text.Split( ',' ).ToList(),
				ThemeColorMain = ColorTranslator.FromHtml( themeColorMain.Text ),
				ThemeColorSelected = ColorTranslator.FromHtml( themeColorSelected.Text ),
				ThemeColorHeader = ColorTranslator.FromHtml( themeColorHeader.Text ),
				Sort = new SortMode() { field = sortField.Text, order = sortOrder.Text },
				ResultTagSchemas = resultTagSchemas.Text.Split( ',' ).ToList(),
				SiteId = int.Parse( siteID.Text ),
				StartAdvanced = startAdvanced.Text == "1",
				HasStandards = hasStandards.Text == "1",
				UseResourceUrl = useResourceUrl.Text == "1",
				DoAutoSearch = doAutoSearch.Text == "1",
				DoPreloadNewestSearch = doPreloadNewestSearch.Text == "1",
				ShowLibColInputs = showLibColInputs.Text == "1"
			};
		} //

		public override string GetPreloadedNewestResults()
		{
			var input = new Services.ElasticSearchService.JSONQueryV7()
			{
				sort = new Services.ElasticSearchService.SortV7() { field = "ResourceId", order = "desc" },
				text = "*",
				not = "",
				size = 20,
				start = 0
			};
			return ( string ) new Services.ElasticSearchService().DoSearchCollection7( input ).data;
		} //

	}
}