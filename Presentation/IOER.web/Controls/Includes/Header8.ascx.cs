using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using LRWarehouse.Business;
using IOER.Library;
using Isle.BizServices;
using IOER.classes;

namespace IOER.Controls.Includes
{
	public partial class Header8 : BaseUserControl
	{
		public bool IsLoggedIn { get; set; }
		public bool ShowAdminMenu { get; set; }
		public bool ShowAdminExtras { get; set; }
		//public Patron User { get; set; }
		private const string CAN_VIEW_ADMIN_MENU = "CAN_VIEW_ADMIN_MENU";

		protected void Page_Load( object sender, EventArgs e )
		{
			//Handle redirect hack
			CheckForRedirect();

			//Detect user login
			if ( IsUserAuthenticated() )
			{
				//Set logged in user
				IsLoggedIn = true;
				//User = ( Patron ) WebUser;
				CurrentUser = GetAppUser();
				//Detect site admin
				DetermineAdminMenu();

				//Detect logout command
				if ( Request.Params[ "logout" ] == "true" )
				{
					Session.Abandon();
					WebUser = null;
					Response.Redirect( "/", true );
				}

			}

		}

		private void DetermineAdminMenu()
		{
			string auth = IOER.classes.SessionManager.Get( Session, CAN_VIEW_ADMIN_MENU, "missing" );

			try
			{
				//Get auth from session
				if ( auth.Equals( "yes" ) )
				{
					ShowAdminMenu = true;
				}
				//Or figure it out now, and set it in the session
				else if ( auth.Equals( "missing" ) )
				{
					var privileges = SecurityManager.GetGroupObjectPrivileges( WebUser, this.txtAdminSecurityName.Text );
					if ( privileges.CanCreate() )
					{
						ShowAdminMenu = true;
						SessionManager.Set( Session, CAN_VIEW_ADMIN_MENU, "yes" );
					}
					else
					{
						ShowAdminMenu = false;
						ShowAdminExtras = false;
						SessionManager.Set( Session, CAN_VIEW_ADMIN_MENU, "no" );
					}
				}

				//Show extra admin tools
				//May remove this in the future
				if ( ShowAdminMenu && WebUser.UserName == "mparsons" )
				{
					ShowAdminExtras = true;
				}

			}
			catch ( System.Threading.ThreadAbortException taex )
			{
				//Do nothing, this is okay
			}
			catch ( Exception ex )
			{
				LogError( ex, "Header8.DetermineAdminMenu()" );
			}
		}

		/// <summary>
		/// This is a horrible hack that should have been handled in DNS, however we cannot do it right now without incurring additional charges from our current DNS provider, Network Solutions.  
		/// This is intended to redirect ioer.*.*
		/// (where the TLD is not "org") to ioer.ilsharedlearning.org.  Protocol (http vs. https) and port are preserved, as is the page the user is navigating to.
		/// </summary>
		private void CheckForRedirect()
		{
			if ( skipRedirectCheck.Text == "yes" )
			{
				return;
			}

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

					if ( ( hostName.IndexOf( "ioer" ) > -1 || hostName.IndexOf( "sandbox" ) > -1) && tld != "org" )
					{
						string newHostName = domainPieces[ 0 ];
						newHostName += ".ilsharedlearning.org";
						if ( port.Length > 0 )
						{
							newHostName += port;
						}
						string url = Request.Url.ToString();
						url = url.Replace( hostName, newHostName );

						Response.Redirect( url );
					}
                    if (hostName.IndexOf("www") > -1)
                    {
                        string newHostName = "ioer.ilsharedlearning.org";
                        if (port.Length > 0)
                        {
                            newHostName += port;
                        }
                        string url = Request.Url.ToString();
                        url = url.Replace(hostName, newHostName);

                        Response.Redirect(url);
                    }
				}
			}
			catch ( System.Threading.ThreadAbortException taex )
			{
				// Do nothing, this is okay
			}
			catch ( Exception ex )
			{
				LogError( ex, "Header8.CheckForRedirect()" );
			}
		}


	}
}