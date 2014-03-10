using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ILPathways.Library;
using ILPathways.Utilities;
namespace ILPathways.My
{
    public partial class ComingSoon : BaseAppPage
    {
        public string MyTitle = "Coming Soon";
        protected void Page_Load( object sender, EventArgs e )
        {
            string parm = this.GetRequestKeyValue( "stype", "" );
            if (parm.Length > 0)
                MyTitle = parm;
        }
    }
}