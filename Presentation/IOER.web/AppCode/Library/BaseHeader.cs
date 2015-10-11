using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Drawing.Text;

using ILPathways.Utilities;
using IOER.classes;
using ILPathways.Business;
using EmailHelper = ILPathways.Utilities.EmailManager;

//using DAL = workNet.DAL;
//using workNet.BusObj.Entity;

namespace IOER.Library
{
	/// <summary>
	/// Summary description for BaseHeader
	/// </summary>
	public class BaseHeader : BaseUserControl
	{

		/// <summary>
		/// public variable for dynamic addition to the menu
		/// </summary>
		public string m_loginItem;
		public string m_govLink;

		public BaseHeader()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private DataItem FormatMenuItem( ResourceManager rm, string textResString, string urlResString, string tooltipResString )
		{

			return FormatMenuItem( rm, textResString, urlResString, tooltipResString, "" );
		}//

		private DataItem FormatMenuItem( ResourceManager rm, string textResString, string urlResString, string tooltipResString, string target )
		{
			string menuItem = "";
			DataItem dataItem = new DataItem();

			string toolTip = "";

            dataItem.param1 = "<a href='" + urlResString + "' "  + toolTip + target + ">" + textResString  + "</a>";
			return dataItem;
		}//

		/// <summary>
		/// Check for secure state - using https vs. http
		/// </summary>
		protected void CheckForProperSecureState()
		{
			if (Request.IsSecureConnection == true)
			{
				if (this.IsASecurePage())
				{
					//OK ignore
				}
				else
				{
					Response.Redirect("http://" + Request.ServerVariables["HTTP_HOST"] + Request.Url.PathAndQuery);
				}
			}

		} //

		/// <summary>
		/// Check for incorrect url
		/// </summary>
		protected void CheckForProperDomain()
		{

			string host = Request.ServerVariables["HTTP_HOST"];
			//if ( host.ToLower().Equals( "localhost" ) )
			if (host.ToLower().Equals("illinoisworknet.com"))
			{
				host = "www.Illinoisworknet.com";
				string url = "http://" + host + Request.QueryString["NRORIGINALURL"];
				//this.welcomeTag.Text = url;
				//welcomeTag.Visible = true;
				Response.Redirect(url);
			}

		} //

		/// <summary>
		/// "Bookmarks" the posting in the user's bookmarks list.
		/// </summary>		
		protected void lbtnBMarks_Click(object sender, EventArgs e)
		{
			try
			{
                //string url;
                //string lStatusMessage = "";
                //string channel = CmsHttpContext.Current.Channel.DisplayName + " - ";
                //string title = GetPageTitle();
                //string extraTitle = this.GetRequestKeyValue("title");
                //if (extraTitle.Length == 0)
                //    extraTitle = this.GetRequestKeyValue("occtext");

                //if (extraTitle.Length > 0)
                //{
                //    //title += " - " + extraTitle;
                //    title = extraTitle;
                //}

                ////append channel
                //title = channel + title;
                ////check for occType
                //string occType = this.GetRequestKeyValue( "occType" );
                //if ( occType.Length > 0 )
                //    title = title + " - " + occType;
                ////check for prgType
                //string prgType = this.GetRequestKeyValue( "prgType" );
                //if ( prgType.Length > 0 )
                //    title = title + " - " + prgType;

                //url = UtilityManager.GetPublicUrl(Request.Url.PathAndQuery);
                //url = HttpUtility.UrlDecode(url);

                //DAL.DatabaseManager.BookmarkAdd(title, url, WebUser.UserID, ref lStatusMessage);

                ////Add message to SystemConsoleMessage
                //string message = SessionManager.getResourceValueByKey( lStatusMessage, Session );
                string message = "TBD";
                //string css = ApplicationManager.SUCCESS_MESSAGE_STYLE;	
                string css = UtilityManager.GetAppKeyValue( "successMessageCss", "successMessage" );
                //if ( message.ToLower().StartsWith( "error:" ) )
                //    css = ApplicationManager.ERROR_MESSAGE_STYLE;	
                //else
                //{
                //    message += " (" + title + ")";
                //}

                //this.SetSessionMessage( SessionManager.SYSTEM_CONSOLE_MESSAGE, "", message, css, false );

                this.SetSessionMessage( SessionManager.SYSTEM_CONSOLE_MESSAGE, "", message, css, false );

			}
			catch (Exception ex)
			{
				this.LogError( ex, "baseHeader:lbtnBMarks_Click exception" );
			}


		} //

	}

}
