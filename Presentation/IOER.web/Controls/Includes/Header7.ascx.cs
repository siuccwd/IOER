using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Threading;

using ILPathways.Library;
using ILPathways.Services;
using ILPathways.Utilities;
//using ILPathways.DAL;
using Isle.BizServices;
using LibMgr = Isle.BizServices.LibraryBizService;



namespace ILPathways.Controls.Includes
{
  public partial class Header7 : BaseUserControl
  {
    public string topNavCount { get; set; }
    private const string CAN_VIEW_ADMIN_MENU = "CAN_VIEW_ADMIN_MENU";
    private const string CAN_VIEW_LIBRARYADMIN_MENU = "CAN_VIEW_LIBRARYADMIN_MENU";


    protected void Page_Load( object sender, EventArgs e )
    {
        
      CheckForRedirect();
      InitPage();
    }

    /// <summary>
    /// This is a horrible hack that should have been handled in DNS, however we cannot do it right now without incurring
    /// additional charges from our current DNS provider, Network Solutions.  This is intended to redirect ioer.*.*
    /// (where the TLD is not "org") to ioer.ilsharedlearning.org.  Protocol (http vs. https) and port are preserved, as is the page the user is navigating to.
    /// </summary>
    private void CheckForRedirect()
    {
        //LoggingHelper.DoTrace( 2, "_______ Header7.CheckForRedirect" );
        if ( skipRedirectCheck.Text == "yes" )
            return;

      try
      {
          string hostName = Request.ServerVariables[ "HTTP_HOST" ];
          if ( hostName != null && hostName.Trim().Length > 0 )
          {
              hostName = hostName.Trim().ToLower();
              string[] domainPieces = hostName.Split( '.' );
              int lastPieceIndex = domainPieces.Count() - 1;
              string tld = "";
              string port = "";
              int portIndex = domainPieces[ lastPieceIndex ].IndexOf( ":" );
              if ( portIndex > -1 )
              {
                  tld = domainPieces[ lastPieceIndex ].Substring( 0, domainPieces[ lastPieceIndex ].IndexOf( ":" ) );
                  port = domainPieces[ lastPieceIndex ].Substring( portIndex, domainPieces[ lastPieceIndex ].Length - portIndex );
              }
              else
              {
                  tld = domainPieces[ lastPieceIndex ];
              }


              if ( ( hostName.IndexOf( "ioer" ) > -1 || hostName.IndexOf( "sandbox" ) > -1 ) && tld != "org" )
              {
                  string newHostName = domainPieces[ 0 ];
                  newHostName += ".ilsharedlearning.org";
                  if ( port.Length > 0 )
                  {
                      newHostName += port;
                  }
                  string url = Request.Url.ToString();
                  url = url.Replace( hostName, newHostName );
                  //LoggingHelper.DoTrace( 2, "$%$%$%$ CheckForRedirect. doing redirect: " + url );
                  Response.Redirect( url );
              }
          }
      }
      catch ( ThreadAbortException taex )
      {
        // Do nothing, this is okay
      }
      catch ( Exception ex )
      {
          LogError( ex, "Header7.CheckForRedirect()" );
      }
    }


    public void InitPage()
    {

        try
        {
            if ( doingSecureCheck.Text.Equals( "yes" ) )
                CheckForProperSecureState();

            string serverName = UtilityManager.GetAppKeyValue( "serverName", "" );
            string siteVersion = UtilityManager.GetAppKeyValue( "siteVersion", "" );
            var tag = "Server: " + serverName + ", Version: " + siteVersion;
            loginLink.Attributes.Add( "title", tag );
            profileLink.Attributes.Add( "title", tag );

            if ( IsUserAuthenticated() )
            {
                LoggingHelper.DoTrace( 6, "_______ Header7.InitPage - user is authenticated" );

                topNavCount = "four";
                loginLink.Visible = false;
                loginLink2.Visible = false;
                profileStuff.Visible = true;
                myIOERMenu.Visible = true;
                myIOERMenuLink.Visible = true;
                adminMenu.Visible = false;
                profileLink.InnerHtml = WebUser.FullName();

                SetAdminMenu();
                libAdminLink.Visible = CanUserAdminLibraries( WebUser.Id );
            }
            else
            {
                topNavCount = "three";
                loginLink.Visible = true;
               // loginLink2.Visible = true;
                profileStuff.Visible = false;
                myIOERMenu.Visible = false;
                myIOERMenuLink.Visible = false;
                adminMenu.Visible = false;
                string nextUrl = "/";
                if ( Request.Url.PathAndQuery.ToLower().IndexOf( "login.aspx" ) == -1
                    && Request.Url.PathAndQuery.ToLower().IndexOf( "register.aspx" ) == -1 )
                    nextUrl = Request.Url.PathAndQuery;

                loginLink.HRef = loginUrl1.Text + nextUrl;
                loginLink2.HRef = loginUrl2.Text + nextUrl;

            }
        }
        catch ( System.Threading.ThreadAbortException ex )
        {
            //ignore
        }
        catch ( Exception ex )
        {

            LoggingHelper.LogError( ex, "Header7.InitPage. Path: " + Request.Path );
        }
    }

    protected void SetAdminMenu()
    {
        //LoggingHelper.DoTrace( 2, "_______ Header7.SetAdminMenu" );
      //check if previously done
      string auth = ILPathways.classes.SessionManager.Get( Session, CAN_VIEW_ADMIN_MENU, "missing" );

      try
      {
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
                  ILPathways.classes.SessionManager.Set( Session, CAN_VIEW_ADMIN_MENU, "yes" );
              }
              else
              {
                  adminMenu.Visible = false;
                  ILPathways.classes.SessionManager.Set( Session, CAN_VIEW_ADMIN_MENU, "no" );
              }
          }
          if ( adminMenu.Visible == true )
          {
              topNavCount = "five";

              if ( WebUser.UserName == "mparsons" )
                  mpMenu.Visible = true;
          }
      }
      catch ( ThreadAbortException taex )
      {
          // Do nothing, this is okay
      }
      catch ( Exception ex )
      {
          LogError( ex, "Header7.SetAdminMenu()" );
      }
    }

    protected bool CanUserAdminLibraries( int userId )
    {
        //LoggingHelper.DoTrace( 2, "_______ Header7.CanUserAdminLibraries" );
      bool canView = false;
      //check if previously done
      string auth = ILPathways.classes.SessionManager.Get( Session, CAN_VIEW_LIBRARYADMIN_MENU, "missing" );
      if ( auth.Equals( "yes" ) )
      {
        canView = true;
      }
      else if ( auth.Equals( "missing" ) )
      {
        canView = new LibMgr().Library_CanUserAdministerLibraries( userId );
        if ( canView )
          ILPathways.classes.SessionManager.Set( Session, CAN_VIEW_LIBRARYADMIN_MENU, "yes" );
        else
          ILPathways.classes.SessionManager.Set( Session, CAN_VIEW_LIBRARYADMIN_MENU, "no" );
      }


      return canView;
    }


    public void logoutButton_Click( object sender, EventArgs e )
    {
      Session.Abandon();

      WebUser = null;
      Response.Redirect( "/", true );
    }


    /// <summary>
    /// Check for secure state - using https vs. http
    /// </summary>
    protected void CheckForProperSecureState()
    {
        if ( Request.IsSecureConnection == true )
        {
            if ( this.IsASecurePage() )
            {
                //OK ignore
            }
            else
            {
                Response.Redirect( "http://" + Request.ServerVariables[ "HTTP_HOST" ] + Request.Url.PathAndQuery );
            }
        }

    } //
    /// <summary>
    /// Check if the current page is one that shoud be secure (i.e. use SSL)
    /// </summary>
    /// <returns></returns>
    protected bool IsASecurePage()
    {
        bool IsASecurePage = false;

        try
        {
            string template = Request.Path;
            //string template = templatePath.Substring( templatePath.LastIndexOf( "/" ) + 1 );

            string securePages = UtilityManager.GetAppKeyValue( "securePages" );
            if ( securePages.ToLower().IndexOf( template.ToLower() ) > -1 )
                IsASecurePage = true;

        }
        catch
        {
            //allow default of false on exception
        }
        return IsASecurePage;
    }
  }
}