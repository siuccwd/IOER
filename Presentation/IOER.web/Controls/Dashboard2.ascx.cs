using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using LRWarehouse.Business;
using System.Web.Script.Serialization;
using ILPathways.Services;

namespace ILPathways.Controls
{
  public partial class Dashboard2 : BaseUserControl
  {
    Patron user;
    JavaScriptSerializer serializer = new JavaScriptSerializer();
    Dashboard2Service service = new Dashboard2Service();
    public string profile { get; set; }
    public string libraryThumbs { get; set; }
    public string resourceThumbs { get; set; }

    protected void Page_Load( object sender, EventArgs e )
    {
      InitPage();
    }

    public void InitPage()
    {
      //Get the user
      if ( IsUserAuthenticated() )
      {
        user = ( Patron ) WebUser;
        mainContent.Visible = true;
        errorMessage.Visible = false;
      }
      else
      {
        profile = "var profile = {};";
        libraryThumbs = "var libraryThumbs = [];";
        resourceThumbs = "var resourceThumbs = [];";
        mainContent.Visible = false;
        errorMessage.Visible = true;
        return;
      }

      bool isValid = true;
      string status = "";
      profile = "var profile = " + serializer.Serialize( service.GetUserProfile( user ) ) + ";";
      libraryThumbs = "var libraryThumbs = " + serializer.Serialize( service.GetMyLibraryThumbs( user, ref isValid, ref status ) ) + ";";
      resourceThumbs = "var resourceThumbs = " + serializer.Serialize( service.GetMyResourceThumbs( user, 10, ref isValid, ref status ) ) + ";";
    }
  }
}