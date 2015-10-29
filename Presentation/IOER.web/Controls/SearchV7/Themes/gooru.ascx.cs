using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Net.Http;
using System.Text;
using System.Drawing;
using ILPathways.Utilities;
using System.Web.Script.Serialization;

namespace IOER.Controls.SearchV7.Themes
{
	public partial class gooru : SearchTheme
	{

		public string GooruSessionToken { get; set; }

		protected void Page_Load( object sender, EventArgs e )
		{
			GooruSessionToken = GetGooruSessionToken();
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

		private string GetGooruSessionToken()
		{
			if ( Session[ "gooruSessionId" ] != null )
			{
				return Session[ "gooruSessionId" ] as string;
			}
			else
			{
				var url = UtilityManager.GetAppKeyValue( "gooruApiUrl", "http://www.goorulearning.org/gooruapi/rest/v2/account/loginas/anonymous?apiKey=960a9175-eaa7-453f-ba03-ecd07e1f1afc" );

				//Make a POST request to refresh the access token
				var refresher = new HttpClient();
				var content = new StringContent( "{}", UnicodeEncoding.UTF8, "application/json" );
				var response = refresher.PostAsync( url, content ).Result;
				var responseData = response.Content.ReadAsStringAsync().Result;
				var token = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>( responseData )[ "token" ] as string;
				Session.Remove( "gooruSessionId" );
				Session.Add( "gooruSessionId", token );

				return token;
			}
		}

	}
}