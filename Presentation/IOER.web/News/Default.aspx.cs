using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IOER.LRW.News
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load( object sender, EventArgs e )
        {

        }

        protected void Page_PreRender( object sender, EventArgs e )
        {
            if ( customPageTitle.Text.Length > 0 )
                this.Page.Title = customPageTitle.Text;

        }
    }
}