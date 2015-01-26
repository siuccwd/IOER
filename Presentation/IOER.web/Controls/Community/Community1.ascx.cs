using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using ILP = ILPathways.Business;
using ILPathways.Services;
using ILPathways.Utilities;
using Isle.BizServices;
using LRWarehouse.Business;

namespace ILPathways.Controls.Community
{
  public partial class Community1 : BaseUserControl
  {
    JavaScriptSerializer serializer = new JavaScriptSerializer();
    CommunityService comService = new CommunityService();
    public int communityID { get; set; }
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

        //default to 1
      communityID = GetRequestKeyValue( "id", 1 );
      communityData = "var communityData = {};";
      userGUID = "var userGUID = \"\";";

      //Determine what to show
      if ( Page.RouteData.Values.Count > 0
          && Page.RouteData.Values.ContainsKey( "RouteID" ) )
      {
          communityID = int.Parse( Page.RouteData.Values[ "RouteID" ].ToString() );

      }

      var currentUser = new Patron();
      //Check user status
      if ( IsUserAuthenticated() )
      {
        currentUser = ( Patron ) WebUser;
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

      //Get community data
      communityData = "var communityData = " + serializer.Serialize( comService.GetJSONCommunity( currentUser, communityID ) ) + ";";

       //get all communities
      string template = "<li><a href='/Community/{0}/{1}'>{2}</a></li>";
      string communities = "";
      List<ILP.Community> list = new CommunityServices().Community_SelectAll();
      foreach ( ILP.Community item in list )
      {
          communities += string.Format( template, item.Id, UtilityManager.UrlFriendlyTitle( item.Title), item.Title );
      }
      if ( communities.Length > 1 )
          txtCommunities.Text = "<ul>" + communities + "</ul>";
    }
  }
}