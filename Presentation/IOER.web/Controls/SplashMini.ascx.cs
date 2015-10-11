using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;

namespace IOER.Controls
{
  public partial class SplashMini : BaseUserControl
  {
    public string useNewWindow { get; set; }
    public string scriptLink { get; set; }

    protected void Page_Load( object sender, EventArgs e )
    {
      InitPage();
    }

    public void InitPage()
    {
      if ( useNewWindow == "" )
      {
        useNewWindow = "false";
        scriptLink = "<script src=\"//ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js\" type=\"text/javascript\"></script>";
      }
      if ( IsUserAuthenticated() )
      {
        loginLink.Visible = false;
        registerLink.Visible = false;
        myLibraryLink.Visible = true;
        myFollowedLink.Visible = true;
        myCreatedLink.Visible = true;
        myNetworkLink.Visible = true;
        myDashboardLink.Visible = true;
      }
      else
      {
        loginLink.Visible = true;
        registerLink.Visible = true;
        myLibraryLink.Visible = false;
        myFollowedLink.Visible = false;
        myCreatedLink.Visible = false;
        myNetworkLink.Visible = false;
        myDashboardLink.Visible = false;
      }
    }
  }
}