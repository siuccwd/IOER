using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Isle.BizServices;
using System.Web.Script.Serialization;

namespace IOER.Help
{
	public partial class HelpList : System.Web.UI.UserControl
	{
		public List<HelpListItem> Data { get; set; }
		public HelpListItem HelpItem { get; set; }
		public string Json { get; set; }

		protected void Page_Load( object sender, EventArgs e )
		{
			//get help sections from a SharePoint list
			var query = "$select=Title,TabName,Tagline,Description,DownloadsCSV,MediaCSV,LinksCSV,SortOrder,IsActive&$orderby=SortOrder&$filter=(IsActive eq 1)";
			Data = new WorkNetListFactoryServices().GetListFromList<HelpListItem>( "miniApps_ioerguide", query );
			foreach ( var item in Data )
			{
				item.ParseCSVData();
			}
			string tabName = (string) ( Page.RouteData.Values[ "tab" ] ?? Request.Params[ "tab" ] ?? "" );
			HelpItem = Data.Where( m => m.TabName == tabName ).FirstOrDefault() ?? Data.First();
			//Json = new JavaScriptSerializer().Serialize( Data );
		}

		public class HelpListItem
		{
			public HelpListItem()
			{
				Downloads = new List<HelpListItemPart>();
				Media = new List<HelpListItemPart>();
				Links = new List<HelpListItemPart>();
			}
			public string Title { get; set; }
			public string TabName { get; set; }
			public string Tagline { get; set; }
			public string Description { get; set; }
			public string DownloadsCSV { get; set; }
			public string MediaCSV { get; set; }
			public string LinksCSV { get; set; }
			public List<HelpListItemPart> Downloads { get; set; }
			public List<HelpListItemPart> Media { get; set; }
			public List<HelpListItemPart> Links { get; set; }

			public void ParseCSVData()
			{
				Downloads = ParseCSV( DownloadsCSV );
				Media = ParseCSV( MediaCSV );
				Links = ParseCSV( LinksCSV );
			}

			private List<HelpListItemPart> ParseCSV( string raw )
			{
				var result = new List<HelpListItemPart>();
				try
				{
					var items = raw.Split( new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries );
					foreach ( var set in items )
					{
						var data = set.Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries );
						result.Add( new HelpListItemPart()
						{
							Title = data[ 0 ],
							Proportion = data.Length == 3 ? data[1] : "",
							Url = (data.Length == 3 ? data[2] : data[1]).Replace("http://", "https://")
						} );
					}
				}
				catch { }
				return result;
			}
		}

		public class HelpListItemPart
		{
			public string Title { get; set; }
			public string Url { get; set; }
			public string Proportion { get; set; }
		}
	}
}