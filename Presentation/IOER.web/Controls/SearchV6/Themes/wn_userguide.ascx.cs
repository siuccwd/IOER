using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using LRWarehouse.Business.ResourceV2;
using System.Web.Script.Serialization;
using System.Drawing;

namespace IOER.Controls.SearchV6.Themes
{
  public partial class wn_userguide : SearchTheme
  {
    /* --- Initialization --- */
    public wn_userguide()
    {
      //var fields = ltlFieldList.Text.Split( ',' ).ToList();
      //Temporary
      var fields = "workNetArea,guidanceScenario,educationalRole,resources".Split( ',' ).ToList();
      SetFields( fields );
      SiteId = 3;
      UseResourceUrl = true;
      MainColor = ColorTranslator.FromHtml( "#B74900" );
    }

    /* --- Properties --- */
    public string JSONImageData { get; set; }

    /* --- Methods --- */
    protected void Page_Load( object sender, EventArgs e )
    {
      GetImageData();
    }

    private void GetImageData()
    {
      //Correlate images to IDs
      var field = Fields.Where( f => f.Schema == "educationalRole" ).FirstOrDefault();
      var targets = ltlTagList.Text.Split( ',' );
      if ( field != null )
      {
        var imageData = new List<object>();
        foreach ( var item in targets )
        {
          var tag = field.Tags.Where( t => t.Schema.ToLower() == item ).FirstOrDefault();
          imageData.Add(new {
            Id = tag.Id,
            Title = tag.Title,
            File = item
          });
        }

        JSONImageData = new JavaScriptSerializer().Serialize( imageData );
      }
    }
  }
}