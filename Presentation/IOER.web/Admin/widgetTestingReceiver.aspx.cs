using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Script.Serialization;

namespace ILPathways.Admin
{
    public partial class widgetTestingReceiver : System.Web.UI.Page
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            string data = Request.QueryString[ 1 ];
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            jsonRequest req = serializer.Deserialize<jsonRequest>( data );
            Services.ElasticSearchService search = new Services.ElasticSearchService();
            string result = search.DoSearchWidget( req.searchText, req.pageSize );
            output.Text = "isleSearch.updateResults(" + result + ");";
        }
    }
    public class jsonRequest
    {
        public string searchText;
        public string pageSize;
        public string jsoncallback;
    }

}