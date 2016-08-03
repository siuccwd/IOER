using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using IOER.classes;
using IOER.Library;
using ILPathways.Utilities;

namespace IOER
{
    public partial class ErrorPage : BaseAppPage
    {
        protected void Page_Load( object sender, EventArgs e )
        {

			try
			{
				//we don't want addThis on this page, so show literal in master
				Literal showingAddThis = (Literal)FormHelper.FindChildControl(Page, "litHidingAddThis");
				if (showingAddThis != null)
					showingAddThis.Visible = true;
			}
			catch
			{
			}

			var error = FormHelper.GetRouteKeyValue(Page, "error", "404");
			if (error == "401")
			{
				e401Panel.Visible = true;
				if (IsUserAuthenticated()) 
					lblLoggedIn.Visible = true;
				else
					lblNotLoggedIn.Visible = true;
			}
			else if (error == "404")
			{
				string errorSourcePage = this.GetRequestKeyValue("ErrorPage", "Unknown");
				if (errorSourcePage != "Unknown")
				{
					lblInfo.Text = "Additional Information:" + "<br/>";
					lblInfo.Text += "Error originated on: " + errorSourcePage + "<br/>";
				}
				e404Panel.Visible = true;
			}
			else
			{
				HandleGeneralError();
			}
            
        }
		private void HandleGeneralError()
		{
			e404Panel.Visible = true;

			string exType = "";
			string reason = "";
			string originalUrl = "";
			string errorSourcePage = this.GetRequestKeyValue("ErrorPage", "Unknown");
			
			string url = "";
			string reqUrl = "";
			string remoteIP = "";
			string sessionId = "";
			string userid = "";

			string errorUrl = this.GetSessionItem("AppErrorUrl");
			string errorMsg = this.GetSessionItem( "AppErrorMessage" );
			try
			{
				url = Request.Url.ToString();
				reqUrl = Request.UserHostAddress;
				remoteIP = Request.ServerVariables["REMOTE_HOST"];
				if (Session.SessionID != null || Session.SessionID != "")
				{
					sessionId = Session.SessionID.ToString();
				}
				userid = SessionManager.GetCurrentUserName(Session);

				Object obj = Session["AppError"];
				if (obj == null)
				{
					exType = "none";
				}
				else
				{
					Exception ex = obj as Exception;
					exType = ex.GetType().ToString();
					reason = ex.Message.ToString();

				}
			}
			catch
			{
				//no request object - continue
			}


			if (exType == "System.ArgumentOutOfRangeException")
			{
				//Probable obsolete page
				reason = "The requested page no longer exists. If you arrived at the page in error via a saved link, then you should remove the link.";
			}

			if ( errorMsg.Length > 0 )
			{
				lblInfo.Text = "Additional Information:";
				lblInfo.Text += "<br/> " + errorMsg + "<br/>";
				
			}
			else if ( reason.Length > 0 )
			{
				lblInfo.Text = "Additional Information:";
				lblInfo.Text += "<br/>Error originated on: " + errorSourcePage + "<br/>";
				lblInfo.Text += exType + "<br/>";
				lblInfo.Text += reason + "<br/>";
			}

			string serverName = UtilityManager.GetAppKeyValue("serverName", "");
			lblInfo.Text += "<br/>Server:&nbsp;" + serverName;

			try
			{
				url = Server.UrlDecode(url);
				string parmsString = "";
				if (url.IndexOf("?") > -1)
				{
					parmsString = url.Substring(url.IndexOf("?"));
					url = url.Substring(0, url.IndexOf("?") - 1);
				}

				//log error page
				//may not want to e-mail as source condition should have already done so
				LoggingHelper.LogPageVisit(sessionId, "errorPage.aspx", url, parmsString, false, userid, "", reason, remoteIP, "");
			}
			catch
			{
				//ignore errors
			}

		}
    }
}