using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Utilities;
using IOER.classes;
using ILPathways.Business;
using LRWarehouse.Business;
using IOER.Controllers;
using LDAL = LRWarehouse.DAL;
using MyManager = Isle.BizServices.AccountServices;
using CurrentUser = LRWarehouse.Business.Patron;


namespace IOER.secure
{
    public partial class IsleSSO : IOER.Library.BaseAppPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if not using admin/login2.aspx template, will need to do the following:
            //this should be handled by the secure template now, so hide, or make configurable
            string host = Request.ServerVariables["HTTP_HOST"];
            if (Request.IsSecureConnection == false)
            {
                if (UtilityManager.GetAppKeyValue("SSLEnable", "0") == "1")
                {

                    host = host.Replace(":80", "");

                    //do redirect
                    Response.Redirect("https://" + host + Request.Url.PathAndQuery);
                }
            }
            else
            {
                //is secure
                if (UtilityManager.GetAppKeyValue("SSLEnable", "0") == "0")
                {
                    //should not be https
                    host = host.Replace(":80", "");
                    Response.Redirect("http://" + host + Request.Url.PathAndQuery);
                }
            }
        }


    }
}