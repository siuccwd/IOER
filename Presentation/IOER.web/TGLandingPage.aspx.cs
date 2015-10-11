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

namespace IOER
{
    public partial class TGLandingPage : IOER.Library.BaseAppPage
    {
        string Subject
        {
            get { return Session["subject"].ToString(); }
            set { Session["Subject"] = value; }
        }
        string GradeLevels
        {
            get { return Session["gradeLevels"].ToString(); }
            set { Session["gradeLevels"] = value; }
        }
        string Standards
        {
            get { return Session["standards"].ToString(); }
            set { Session["standards"] = value; }
        }
        int MapId
        {
            get
            {
                int i = 0;
                int.TryParse(Session["mapId"].ToString(), out i);
                return i;
            }
            set { Session["mapId"] = value; }
        }
        string MapName
        {
            get { return Session["mapName"].ToString(); }
            set { Session["mapName"] = value; }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            Subject="";
            GradeLevels="";
            Standards="";
            MapId=0;
            MapName="";

            string paramValue = FormHelper.GetRequestKeyValue("sub", "");
            if (paramValue != string.Empty)
            {
                Subject = paramValue;
            }
            paramValue = FormHelper.GetRequestKeyValue("gl", "");
            if (paramValue != string.Empty)
            {
                GradeLevels = paramValue;
            }
            paramValue = FormHelper.GetRequestKeyValue("std", "");
            if (paramValue != string.Empty)
            {
                Standards = paramValue;
            }
            paramValue = FormHelper.GetRequestKeyValue("lmid", "0");
            if (paramValue != "0" && IsInteger(paramValue))
            {
                MapId = int.Parse(paramValue);
            }
            paramValue=FormHelper.GetRequestKeyValue("lmn","");
            if (paramValue != string.Empty)
            {
                MapName = paramValue;
            }

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
                LDAL.BaseDataManager.LogError("TGLandingPage.Page_Load(): " + ex.ToString());
                Response.Redirect(displayPage.Text);
            }

        }
    }
}