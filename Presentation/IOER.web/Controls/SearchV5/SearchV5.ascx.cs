using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Script.Serialization;

namespace IOER.Controls.SearchV5
{
  public partial class SearchV5 : System.Web.UI.UserControl
  {
    public string filtersJSON { get; set; }

    protected void Page_Load( object sender, EventArgs e )
    {
      //filtersJSON = new JavaScriptSerializer().Serialize( new Services.ElasticSearchService().FetchCodes().lists );
      filtersJSON = new JavaScriptSerializer().Serialize( new Services.ElasticSearchService().GetJSONFiltersV5() );
    }
  }
}