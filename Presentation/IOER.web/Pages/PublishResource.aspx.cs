using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ILPathways.Pages
{
    public partial class PublishResource : ILPathways.Library.BaseAppPage
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( this.IsUserAuthenticated() == false )
            {
                //Response.Redirect( "/Account/Login.aspx?returnURL=" + Request.Path );
            }
        }
    }
}