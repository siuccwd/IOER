using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;
using IOER.Services;
using ILPathways.Utilities;
using DAL = ILPathways.DAL;
using Isle.BizServices;
using LibMgr = Isle.BizServices.LibraryBizService;

namespace IOER.Controls.Includes
{
    public partial class Header6 : BaseUserControl
    {
        private const string CAN_VIEW_ADMIN_MENU = "CAN_VIEW_ADMIN_MENU";
        private const string CAN_VIEW_LIBRARYADMIN_MENU = "CAN_VIEW_LIBRARYADMIN_MENU";

        protected void Page_Load( object sender, EventArgs e )
        {
            CheckForRedirect();
            Init();
        }

        /// <summary>
        /// This is a horrible hack that should have been handled in DNS, however we cannot do it right now without incurring
        /// additional charges from our current DNS provider, Network Solutions.  This is intended to redirect ioer.*.*
        /// (where the TLD is not "org") to ioer.ilsharedlearning.org.  Protocol (http vs. https) and port are preserved, as is the page the user is navigating to.
        /// </summary>
        private void CheckForRedirect()
        {
            string hostName = Request.ServerVariables["HTTP_HOST"].ToLower();
            string[] domainPieces = hostName.Split('.');
            int lastPieceIndex = domainPieces.Count() - 1;
            string tld = "";
            string port = "";
            int portIndex = domainPieces[lastPieceIndex].IndexOf(":");
            if (portIndex > -1)
            {
                tld = domainPieces[lastPieceIndex].Substring(0, domainPieces[lastPieceIndex].IndexOf(":"));
                port = domainPieces[lastPieceIndex].Substring(portIndex, domainPieces[lastPieceIndex].Length - portIndex);
            }
            else
            {
                tld = domainPieces[lastPieceIndex];
            }

            try
            {
                if ((hostName.IndexOf("ioer") > -1 || hostName.IndexOf("sandbox") > -1) && tld != "org")
                {
                    string newHostName = domainPieces[0];
                    newHostName += ".ilsharedlearning.org";
                    if (port.Length > 0)
                    {
                        newHostName += port;
                    }
                    string url = Request.Url.ToString();
                    url = url.Replace(hostName, newHostName);
                    Response.Redirect(url);
                }
            }
            catch (ThreadAbortException taex)
            {
                // Do nothing, this is okay
            }
            catch (Exception ex)
            {
                DAL.BaseDataManager.LogError("header6.CheckForRedirect(): " + ex.Message);
            }
        }

        public void Init()
        {
            if ( IsUserAuthenticated() )
            {
                CurrentUser = GetAppUser();
                SetAuthenticatedUserMenu();
            }
            else
            {
                SetGuestUserMenu();
            }
            SetEnvironmentText();
        }

        protected void SetAuthenticatedUserMenu()
        {
            loginLink.Visible = false;
            registerLink.Visible = false;
            logoutLink.Visible = true;
            profileLink.Visible = true;
            myIsle.Visible = true;
            loginTitle.Text = "Account";
            lblLoggedInAs.Text = "Logged in as " + WebUser.FullName();

            string serverName = UtilityManager.GetAppKeyValue( "serverName", "" );
            string siteVersion = UtilityManager.GetAppKeyValue( "siteVersion", "" );
            lblLoggedInAs.ToolTip = "Server: " + serverName;    // +" v: " + siteVersion;

            if ( CanUserAdminLibraries( WebUser.Id) )
                libAdminLink.Visible = true;

            loginNavTop.Attributes.Add( "data-href", "/Account/Profile.aspx" );
            SetAdminMenu();
        }
        protected bool CanUserAdminLibraries( int userId)
        {
            bool canView = false;
            //check if previously done
            string auth = IOER.classes.SessionManager.Get( Session, CAN_VIEW_LIBRARYADMIN_MENU, "missing" );
            if ( auth.Equals( "yes" ) )
            {
                canView = true;
            }
            else if ( auth.Equals( "missing" ) )
            {
                canView = new LibMgr().Library_CanUserAdministerLibraries( userId );
                if (canView )
                    IOER.classes.SessionManager.Set( Session, CAN_VIEW_LIBRARYADMIN_MENU, "yes" );
                else
                    IOER.classes.SessionManager.Set( Session, CAN_VIEW_LIBRARYADMIN_MENU, "no" );
            }


            return canView;
        }
        protected void SetGuestUserMenu()
        {
            loginLink.Visible = true;
            registerLink.Visible = true;
            logoutLink.Visible = false;
            profileLink.Visible = false;
            myIsle.Visible = false;
            loginTitle.Text = "Login";
            lblLoggedInAs.Text = "Guest";
            string serverName = UtilityManager.GetAppKeyValue( "serverName", "" );
            string siteVersion = UtilityManager.GetAppKeyValue( "siteVersion", "" );
            lblLoggedInAs.ToolTip = "Server: " + serverName + " v: " + siteVersion;

            adminMenu.Visible = false;
            loginNavTop.Attributes.Add( "data-href", "/Account/Login.aspx?nextUrl=" + Request.Url.PathAndQuery );
            loginLink.HRef = "/Account/Login.aspx?nextUrl=" + Request.Url.PathAndQuery;
        }

        protected void SetAdminMenu()
        {
            //check if previously done
            string auth = IOER.classes.SessionManager.Get( Session, CAN_VIEW_ADMIN_MENU, "missing" );
            if ( auth.Equals( "yes" ) )
            {
                adminMenu.Visible = true;
            }
            else if ( auth.Equals( "missing" ) )
            {
                this.RecordPrivileges = SecurityManager.GetGroupObjectPrivileges( this.WebUser, this.txtAdminSecurityName.Text );
                if ( RecordPrivileges.CanCreate() )
                {
                    adminMenu.Visible = true;
                    IOER.classes.SessionManager.Set( Session, CAN_VIEW_ADMIN_MENU, "yes" );
                }
                else
                {
                    adminMenu.Visible = false;
                    IOER.classes.SessionManager.Set( Session, CAN_VIEW_ADMIN_MENU, "no" );
                }
            }
            if ( adminMenu.Visible == true )
            {
                if ( WebUser.UserName == "mparsons" )
                    mpMenu.Visible = true;
            }
        }

        protected void SetEnvironmentText()
        {
            WebDALService webDAL = new WebDALService();
            if ( webDAL.IsLocalHost() )
            {
                environmentText.Text = " (Local Host)";
            }
            else if ( webDAL.IsSandbox() )
            {
                environmentText.Text = " Sandbox";
            }
            else
            {
                environmentText.Text = "";
            }

        }

        public void logoutButton_Click(object sender, EventArgs e)
        {
            Session.Abandon();

            WebUser = null;
            Response.Redirect( "/", true );
        }
    }
}