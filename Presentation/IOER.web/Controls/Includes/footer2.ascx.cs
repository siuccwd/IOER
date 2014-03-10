using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ILPathways.Includes
{
    public partial class footer2 : System.Web.UI.UserControl
    {
        public string thisYear = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            thisYear = System.DateTime.Now.Year.ToString();
        }
    }
}
