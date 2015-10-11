using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Routing;

namespace IOER
{
	public partial class LearningListsSearch : System.Web.UI.Page
	{
		protected void Page_Load( object sender, EventArgs e )
		{
			Title = ( ( string ) RouteData.Values[ "title" ] ) ?? "IOER Resource Search";
			searchController.ThemeName = ( ( string ) RouteData.Values[ "theme" ] ) ?? "ioer";
		}
	}
}