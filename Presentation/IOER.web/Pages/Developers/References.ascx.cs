using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IOER.Pages.Developers
{
	public partial class References : DocumentationItem
	{
		public References()
		{
			PageTitle = "References";
			UpdatedDate = DateTime.Parse( "2015/10/23" );
		}

		protected void Page_Load( object sender, EventArgs e )
		{

		}
	}
}