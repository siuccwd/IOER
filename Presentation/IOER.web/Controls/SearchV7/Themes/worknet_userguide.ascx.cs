using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using LRWarehouse.Business.ResourceV2;
using System.Web.Script.Serialization;
using System.Drawing;

namespace IOER.Controls.SearchV7.Themes
{
	public partial class worknet_userguide : SearchTheme
	{
		/* --- Properties --- */
		public string JSONImageData { get; set; }

		protected void Page_Load( object sender, EventArgs e )
		{
			GetImageData();
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

		private void GetImageData()
		{
			//Correlate images to IDs
			var field = new Isle.BizServices.ResourceV2Services().GetFieldAndTagCodeData().Where( f => f.Schema == "educationalRole" ).FirstOrDefault();
			var targets = ltlTagList.Text.Split( ',' );
			if ( field != null )
			{
				var imageData = new List<object>();
				foreach ( var item in targets )
				{
					var tag = field.Tags.Where( t => t.Schema.ToLower() == item ).FirstOrDefault();
					imageData.Add( new
					{
						Id = tag.Id,
						Title = tag.Title,
						File = item
					} );
				}

				JSONImageData = new JavaScriptSerializer().Serialize( imageData );
			}
		}

	}
}