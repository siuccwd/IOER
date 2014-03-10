using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using System.Web.Script.Serialization;
using ILPathways.Services;

namespace ILPathways.Controls.Community
{
  public partial class Community1 : BaseUserControl
  {
    JavaScriptSerializer serializer = new JavaScriptSerializer();
    CommunityService comService = new CommunityService();
    public string communityData { get; set; }
    public string userGUID { get; set; }
    public string minimumLength { get; set; }
    public string userID { get; set; }
    public string userIsAdmin { get; set; }
    public string loginClass { get; set; }
    int minimumMessageLength { get; set; }

    protected void Page_Load( object sender, EventArgs e )
    {
      InitPage();
    }

    protected void InitPage()
    {
      minimumMessageLength = int.Parse( ltlMinimumMessageLength.Text );
      minimumLength = "var minimumLength = " + minimumMessageLength + ";";
      userID = "var userID = 0;";
      userIsAdmin = "var userIsAdmin = false;";

      communityData = "var communityData = {};";
      userGUID = "var userGUID = \"\";";

      //Get community data
      communityData = "var communityData = " + serializer.Serialize( comService.GetJSONCommunity( 1 ) ) +";";

      //Check user status
      if ( IsUserAuthenticated() )
      {
        PostMakerContent.Visible = true;
        PostMakerLoginMessage.Visible = false;
        userGUID = "var userGUID = \"" + WebUser.RowId.ToString() + "\";";
        userID = "var userID = " + WebUser.Id + ";";
        if ( new UtilityService().isUserAdmin( WebUser ) )
        {
          userIsAdmin = "var userIsAdmin = true;";
        }
      }
      else
      {
        PostMakerContent.Visible = false;
        PostMakerLoginMessage.Visible = true;
        loginClass = "login";
      }
    }
  }
}