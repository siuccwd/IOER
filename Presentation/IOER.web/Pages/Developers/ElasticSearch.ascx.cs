using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IOER.Pages.Developers
{
    public partial class ElasticSearch : DocumentationItem
    {
        public ElasticSearch()
		{
			//PageTitle = "Elastic Search";
			//UpdatedDate = DateTime.Parse( "2015/09/14" );
			
		}
        protected void Page_Load(object sender, EventArgs e)
        {
					PageTitle = litPageTitle.Text;
					UpdatedDate = DateTime.Parse( litLastUpdateDate.Text );
        }
    }
}