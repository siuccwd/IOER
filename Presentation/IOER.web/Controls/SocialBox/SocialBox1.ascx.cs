using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Script.Serialization;

namespace ILPathways.Controls.SocialBox
{
  public partial class SocialBox1 : System.Web.UI.UserControl
  {
    public bool hideMe { get; set; }
    public string socialBoxDataString { get; set; }
    JavaScriptSerializer serializer = new JavaScriptSerializer();
    public string shareType { get; set; }
    public object shareData { get; set; }

    protected void Page_Load( object sender, EventArgs e )
    {
      DetermineVisibility();
      LoadData();
      LoadShareInterface();
    }

    private void DetermineVisibility()
    {
      //Hide the social box when it isn't wanted
      //Via URL parameter
      if ( Request.Params[ "hidesocial" ] != null )
      {
        var param = Request.Params[ "hidesocial" ];
        if ( param == "yes" || param == "true" )
        {
          socialBoxContainer.Visible = false;
        }
      }

      //Via control configuration
      if ( hideMe )
      {
        socialBoxContainer.Visible = false;
      }

      //Via URL detection (hide on all widgets)
      if ( Request.RawUrl.ToLower().IndexOf( "/widgets/" ) > -1 )
      {
        socialBoxContainer.Visible = false;
      }
    }

    private void LoadData()
    {
      var dto = new SocialBoxData();

      //dto.likeData = {};
      
      //dto.comments = {};

      dto.shareData = shareData;

      socialBoxDataString = serializer.Serialize( dto );
    }

    private void LoadShareInterface()
    {
      switch ( shareType )
      {
        case "curriculum":
          shareType_curriculum.Visible = true;
          break;
        default:
          break;
      }
    }

    public class SocialBoxData
    {
      public SocialBoxData()
      {
        likeData = new SocialBoxLikeData();
        comments = new List<SocialBoxComment>();
      }
      public SocialBoxLikeData likeData { get; set; }
      public List<SocialBoxComment> comments { get; set; }
      public object shareData { get; set; }
    }
    public class SocialBoxLikeData
    {
      public int likes { get; set; }
      public bool iLikedThis { get; set; }
    }
    public class SocialBoxComment
    {
      public int id { get; set; }
      public string title { get; set; }
      public string date { get; set; }
      public string text { get; set; }
      public string avatar { get; set; }
    }
  }
}