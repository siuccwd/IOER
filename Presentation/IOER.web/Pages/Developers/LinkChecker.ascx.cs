using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IOER.Pages.Developers
{
	public partial class LinkChecker : DocumentationItem
	{
		public LinkChecker()
		{
			PageTitle = "Link Checker";
			UpdatedDate = DateTime.Parse( "2015/08/31" );
		}

		protected void Page_Load( object sender, EventArgs e )
		{

		}
	}
}