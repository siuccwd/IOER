using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;

namespace IOER.Organizations.controls.Reports
{
    public partial class BaseActivityFilter : BaseUserControl
    {

        public string StartDate
        {
            get { return txtStartDate.Text; }
            set { txtStartDate.Text = value; }
        }
        public string EndDate
        {
            get { return txtEndDate.Text; }
            set { txtEndDate.Text = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}