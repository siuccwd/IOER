using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;
using System.Web.Script.Serialization;
using Isle.BizServices;

namespace IOER.Admin
{
  public partial class IndexRefresher : BaseAppPage
  {
    JavaScriptSerializer serializer = new JavaScriptSerializer();

    protected void Page_Load( object sender, EventArgs e )
    {
      Page.Server.ScriptTimeout = 30000000;

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

      //Do update
      if ( IsPostBack )
      {
        try
        {
          if ( txtIDs.Value.Length == 0 )
          {
            throw new Exception( "No IDs entered" );
          }

					//Attempt to reindex these IDs
					var ids = serializer.Deserialize<List<int>>( "[" + txtIDs.InnerText + "]" ).Distinct().ToList();

					if ( hdnDelete.Value != "true" )
					{
						new ResourceV2Services().ImportRefreshResources( ids, false );
						SetConsoleSuccessMessage( "Resources reindexed!" );
					}
					else
					{
						new ResourceV2Services().BulkDeleteResources( ids );
						SetConsoleSuccessMessage( "Resources deleted!" );
					}
        }
        catch ( Exception ex )
        {
          SetConsoleErrorMessage( ex.Message );
        }
      }
    }
  }
}