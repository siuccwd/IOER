using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Utilities;

namespace IOER.Organizations
{
    public partial class Default : System.Web.UI.Page
    {
		protected void Page_Load( object sender, EventArgs e )
		{
			if ( !this.IsPostBack )
			{
				this.InitializeForm();
			}
		}

		private void InitializeForm()
		{
			try
			{
				//we don't want addThis on this page, so show literal in master
				Literal showingAddThis = ( Literal ) FormHelper.FindChildControl( Page, "litHidingAddThis" );
				if ( showingAddThis != null )
					showingAddThis.Visible = true;
			}
			catch
			{
			}
		}
    }
}