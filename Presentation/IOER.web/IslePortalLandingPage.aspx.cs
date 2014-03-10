using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Utilities;
using ILPathways.classes;
using ILPathways.Business;
using LRWarehouse.Business;
using ILPathways.Controllers;
using LDAL = LRWarehouse.DAL;
using MyManager = Isle.BizServices.AccountServices;
using CurrentUser = LRWarehouse.Business.Patron;

namespace ILPathways
{
    public partial class IslePortalLandingPage : ILPathways.Library.BaseAppPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (bool.Parse(SSOauthenticateUser.Text))
                {
                    Response.Redirect(authenticationPage.Text);
                }
                else
                {
                    Response.Redirect(displayPage.Text);
                }
            }
            catch (ThreadAbortException taex)
            {
                // This is okay, do nothing
            }
            catch (FormatException fex)
            {
                SSOauthenticateUser.Text = "true";
                Page_Load(sender, e);
            }
            catch (Exception ex)
            {
                LDAL.BaseDataManager.LogError("ILPathwaysAccount.Page_Load(): " + ex.ToString());
                Response.Redirect(displayPage.Text);
            }

        }
    }
}