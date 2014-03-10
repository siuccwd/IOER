using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ILPathways.Utilities;
using ILPathways.Library;

namespace ILPathways.testing
{
    public partial class TestingEicarVirus : BaseAppPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                btnSubmit_Click(sender, e);
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            VirusScanner scanner = new VirusScanner();
            string eicar = "X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";
            byte[] array = System.Text.Encoding.ASCII.GetBytes(eicar);

            string status = scanner.Scan(array);
            if (status == "OK")
            {
                SetConsoleSuccessMessage("No virus found!");
            }
            else if (status == "no scan done")
            {
                SetConsoleInfoMessage("No scan done on file.");
            }
            else
            {
                SetConsoleErrorMessage(status);
            }
        }
    }
}