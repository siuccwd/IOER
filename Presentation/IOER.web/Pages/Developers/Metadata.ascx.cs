using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IOER.Pages.Developers
{
	public partial class Metadata : DocumentationItem
	{
		public Metadata()
		{
			PageTitle = "IOER Metadata and Vocabularies";
			UpdatedDate = DateTime.Parse( "2015/07/01" );
		}

		protected void Page_Load( object sender, EventArgs e )
		{

		}
	}
}