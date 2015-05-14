using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using LRWarehouse.Business.ResourceV2;
using Isle.BizServices;
using System.Drawing;

namespace ILPathways.Controls.SearchV6.Themes
{
  public class SearchTheme : System.Web.UI.UserControl
  {
    /* --- Initialization --- */
    public SearchTheme()
    {
      Fields = new List<FieldDB>();
    }

    /* --- Properties --- */
    public List<FieldDB> Fields { get; set; }
    public int SiteId { get; set; }
    public bool UseResourceUrl { get; set; }
    public Color MainColor { get; set; }
    public string MainColorHex { get; set; }

    /* --- Methods --- */
    public void SetFields( List<string> schemas )
    {
      var rawData = new ResourceV2Services().GetFieldAndTagCodeData();
      foreach ( var item in schemas )
      {
        var field = rawData.Where( s => s.Schema.ToLower() == item.ToLower() ).FirstOrDefault();
        if ( field != null )
        {
          Fields.Add( field );
        }
      }
    }

    public List<FieldDB> GetFields()
    {
      return Fields;
    }

    public int GetSiteId()
    {
      return SiteId;
    }

    public bool GetUseResourceUrl()
    {
      return UseResourceUrl;
    }

    public void SetMainColor( string hex )
    {
      MainColor = ColorTranslator.FromHtml( hex );
      MainColorHex = ColorTranslator.ToHtml( MainColor );
    }

    public Color GetMainColor()
    {
      return MainColor;
    }

  }
}