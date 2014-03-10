using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ILPathways.Utilities;

namespace ILPathways
{
    public partial class SSLRedirect : System.Web.UI.Page
    {
        protected string redir = "";

        protected void Page_Load( object sender, EventArgs e )
        {
            //pass-thru the URL to the destination page
            redir = FormHelper.GetRequestKeyValue( "redir" );
            if ( redir == "" )
            {
                redir = System.Configuration.ConfigurationManager.AppSettings.Get( "defaultPage" );
            }
        }
    }
}