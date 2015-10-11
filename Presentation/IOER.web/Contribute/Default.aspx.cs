using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ILPathways.Utilities;

namespace IOER.Contribute
{
  public partial class Default : System.Web.UI.Page
  {
    protected void Page_Load( object sender, EventArgs e )
    {
      if ( Request.Params[ "mode" ] == "tag" )
      {
        pnlIntro.Visible = false;
        contributer.Visible = true;
        contributer.mode = "tag";
      }
      else if ( Request.Params[ "mode" ] == "upload" )
      {
        pnlIntro.Visible = false;
        contributer.Visible = true;
        contributer.mode = "upload";
      }
      else
      {
        pnlIntro.Visible = true;
        contributer.Visible = false;
      }
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