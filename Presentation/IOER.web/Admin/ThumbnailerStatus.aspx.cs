using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Utilities;
using IOER.Library;
using Isle.BizServices;

namespace IOER.Admin
{
	public partial class ThumbnailerStatus : BaseAppPage
	{
		public string ServerName { get; set; }

		protected void Page_Load( object sender, EventArgs e )
		{
			//Check for login
			if ( !IsUserAuthenticated() )
			{
				Response.Redirect( "/Account/Login.aspx?nextUrl=" + Request.Url.PathAndQuery );
				return;
			}

			//Check for admin status
			FormPrivileges = SecurityManager.GetGroupObjectPrivileges( WebUser, "IOER.Pages.ResourceDetail" );
			if ( !FormPrivileges.CanDelete() )
			{
				Response.Redirect( "/" );
			}

			ServerName = UtilityManager.GetAppKeyValue( "serverName", "" );

		}
	}
}