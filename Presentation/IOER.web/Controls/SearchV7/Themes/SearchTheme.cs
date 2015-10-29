using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Drawing;

namespace IOER.Controls.SearchV7.Themes
{
	public class SearchTheme : System.Web.UI.UserControl
	{

		public virtual SearchConfig GetSearchConfig()
		{
			return new SearchConfig();
		}

		public virtual string GetPreloadedNewestResults()
		{
			return "{}";
		}

	} // End class

	//Configuration options
	public class SearchConfig
	{
		public SearchConfig()
		{
			SearchTitle = "IOER Resource Search";
			AllFieldSchemas = new List<string>();
			AdvancedFieldSchemas = new List<string>();
			ThemeColorMain = new Color();
			ThemeColorSelected = new Color();
			ThemeColorHeader = new Color();
			Text = "";
			Sort = new SortMode() { field = "_score", order = "desc" };
			PageSize = 20;
			ViewMode = "list";
			ResultTagSchemas = new List<string>();
			StartAdvanced = false;
			HasStandards = true;
			UseResourceUrl = false;
			DoAutoSearch = true;
			DoPreloadNewestSearch = false;
			ShowLibColInputs = true;
			SiteId = 0;
		}
		public string SearchTitle { get; set; }
		public List<string> AllFieldSchemas { get; set; }
		public List<string> AdvancedFieldSchemas { get; set; }
		public Color ThemeColorMain { get; set; }
		public Color ThemeColorSelected { get; set; }
		public Color ThemeColorHeader { get; set; }
		public string Text { get; set; }
		public SortMode Sort { get; set; }
		public int PageSize { get; set; }
		public string ViewMode { get; set; }
		public List<string> ResultTagSchemas { get; set; }
		public bool StartAdvanced { get; set; }
		public bool HasStandards { get; set; }
		public bool UseResourceUrl { get; set; }
		public bool DoAutoSearch { get; set; }
		public bool DoPreloadNewestSearch { get; set; }
		public bool ShowLibColInputs { get; set; }
		public int SiteId { get; set; }
	}

	public class SortMode
	{
		public string field { get; set; }
		public string order { get; set; }
	}


}