using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Utilities;

namespace IOER
{
  public partial class Server : System.Web.UI.Page
  {
    protected void Page_Load( object sender, EventArgs e )
    {
		string serverName = UtilityManager.GetAppKeyValue("serverName", "");
		string siteVersion = UtilityManager.GetAppKeyValue("siteVersion", "");
		ip.Text = "Server: " + serverName + ", Version: " + siteVersion;

      //ip.Text = ILPathways.Utilities.UtilityManager.GetAppKeyValue( "serverName", "" );
    }
  }
}