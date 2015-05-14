using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Isle.BizServices;
using LRWarehouse.Business.ResourceV2;
using ILPathways.Library;
using ILPathways.Services;
using System.Web.Script.Serialization;
using System.Drawing;

namespace ILPathways.Controls.SearchV6.Themes
{
  public partial class ioer_library : SearchTheme
  {
    /* --- Initialization --- */
    public ioer_library()
    {
      Fields = new ResourceV2Services().GetFieldAndTagCodeData();
      SiteId = 1;
      UseResourceUrl = false;
      MainColor = ColorTranslator.FromHtml( "#3572B8" );
    }

    /* --- Managers/Helper Classes --- */
    JavaScriptSerializer serializer = new JavaScriptSerializer();

    /* --- Properties --- */
    public string LibraryPageDataJSON { get; set; }

    /* --- Methods --- */
    protected void Page_Load( object sender, EventArgs e )
    {
      LoadLibraryData();
    }

    private void LoadLibraryData()
    {
      LibraryPageDataJSON = "null";

      //Get Library Data
      var libraryID = 0;
      int.TryParse( Request.Params[ "libraryID" ], out libraryID );
      if ( libraryID == 0 )
      {
        Fail( "That is not a valid Library ID." );
        return;
      }

      var valid = true;
      var status = "";
      var libraryData = new LibraryAJAXService().LoadLibraryPage( libraryID, ref valid, ref status );
      if ( !valid )
      {
        Fail( status );
        return;
      }

      LibraryPageDataJSON = serializer.Serialize( libraryData );
    }

    private void Fail( string message )
    {
      errorMessage.Visible = true;
      mainLibraryHeader.Visible = false;
      errorMessage.InnerHtml = message;
    }
  }
}