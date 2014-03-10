using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;

namespace ILPathways
{
    public partial class ResourceDetail : BaseAppPage
    {
        private void Page_PreInit( object sender, EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( GetRequestKeyValue( "ver", "" ) == "new" )
                {
                    this.MasterPageFile = "/Masters/Responsive.Master";
                }
            }
        }
        protected void Page_Load( object sender, EventArgs e )
        {
            
        }//

    }
}