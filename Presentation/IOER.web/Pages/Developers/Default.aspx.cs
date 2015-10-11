using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Utilities;
using IOER.Library;

namespace IOER.Pages.Developers
{
	public partial class Default : BaseAppPage
	{
		public DocumentationItem Data { get; set; }

		protected void Page_Load( object sender, EventArgs e )
		{
			var pageName = FormHelper.GetRouteKeyValue( Page, "page", "index" );

			Data = new DocumentationItem();
			try
			{
				Data = (DocumentationItem) Page.LoadControl( "~/pages/developers/" + pageName + ".ascx" );
			}
			catch (Exception ex)
			{
				Data = ( DocumentationItem ) Page.LoadControl( "~/pages/developers/index.ascx" );
				LoggingHelper.DoTrace( 5, "ILPathways.Pages.Developers.Page_Load: " +ex.Message );
			}

			dataContainer.Controls.Add( Data );

			try
			{
				//we don't want addThis on this page, so show literal in master
				Literal showingAddThis = (Literal)FormHelper.FindChildControl(Page, "litHidingAddThis");
				if (showingAddThis != null)
					showingAddThis.Visible = true;
			}
			catch
			{
			}
		}
	}
}