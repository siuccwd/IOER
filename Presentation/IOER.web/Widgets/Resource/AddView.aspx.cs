using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;

namespace IOER.Widgets.Resource
{
  public partial class AddView : BaseAppPage
  {
    protected void Page_Load( object sender, EventArgs e )
    {
      try
      {
        var id = int.Parse( Request.Params[ "id" ] );
        var title = Request.Params["title"];
        new Services.ActivityService().Resource_ResourceView( id, title );
      }
      catch
      {
        
      }

      Response.Redirect( Request.Params[ "next" ] );
    }
  }
}