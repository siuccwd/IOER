using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Drawing;

namespace IOER.Controls.SearchV7.Themes
{
	public partial class uber : SearchTheme
	{
		protected void Page_Load( object sender, EventArgs e )
		{

		}

		public override SearchConfig GetSearchConfig()
		{
			return new SearchConfig()
			{
				SearchTitle = searchTitle.Text,
				AllFieldSchemas = new List<string>(),
				AdvancedFieldSchemas = new List<string>(),
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

	}
}