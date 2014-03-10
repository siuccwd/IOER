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

using ILPathways.classes;
using ILPathways.Library;
using ILPathways.Utilities;


namespace ILPathways
{
    public partial class PageNotFound : BaseAppPage
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            string exType = "";
            string reason = "";
            string originalUrl = "";
            string errorSourcePage = "Error originated on: " + this.GetRequestKeyValue( "ErrorPage", "Unknown" );
            string url = "";
            string reqUrl = "";
            string remoteIP = "";
            string sessionId = "";
            string userid = "";

            string errorUrl = this.GetSessionItem( "AppErrorUrl" );
            if ( errorUrl.Length > 0 )
            {
                int endPos = 0;
                int pos = errorUrl.ToLower().IndexOf( "nroriginalurl=" );
                if ( pos > -1 )
                {
                    originalUrl = errorUrl.Substring( pos + 14 );
                    endPos = originalUrl.ToLower().IndexOf( "&" );

                    if ( endPos > -1 )
                        originalUrl = errorUrl.Substring( 0, endPos );
                }
            }

            try
            {
                url = Request.Url.ToString();
                reqUrl = Request.UserHostAddress;
                remoteIP = Request.ServerVariables[ "REMOTE_HOST" ];
                if ( Session.SessionID != null || Session.SessionID != "" )
                {
                    sessionId = Session.SessionID.ToString();
                }
                userid = SessionManager.GetCurrentUserName( Session );

                Object obj = Session[ "AppError" ];
                if ( obj == null )
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


            if ( exType == "System.ArgumentOutOfRangeException" )
            {
                //Probable obsolete page
                reason = "The requested page no longer exists. If you arrived at the page in error via a saved link, then you should remove the link.";
            }

            if ( reason.Length > 0 )
            {
                lblInfo.Text = "Additional Information:" + "<br/>";
                lblInfo.Text += errorSourcePage + "<br/>";
                lblInfo.Text += exType + "<br/>";
                lblInfo.Text += reason + "<br/>";
            }

            string serverName = UtilityManager.GetAppKeyValue( "serverName", "" );
            lblInfo.Text += "<br/>Server:&nbsp;" + serverName;

            try
            {
                url = Server.UrlDecode( url );
                string parmsString = "";
                if ( url.IndexOf( "?" ) > -1 )
                {
                    parmsString = url.Substring( url.IndexOf( "?" ) );
                    url = url.Substring( 0, url.IndexOf( "?" ) - 1 );
                }

                //log error page
                //may not want to e-mail as source condition should have already done so
                LoggingHelper.LogPageVisit( sessionId, "errorPage.aspx", url, parmsString, false, userid, "", reason, remoteIP, "" );
            }
            catch
            {
                //ignore errors
            }
        }
    }
}