using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Net;
using System.IO;

namespace ILPathways.Controls
{
  public partial class ImportedSearch : System.Web.UI.UserControl
  {
    protected void Page_Load( object sender, EventArgs e )
    {
      HttpWebRequest request = ( HttpWebRequest ) WebRequest.Create( "http://localhost:2013/1/Search/Index" );
      HttpWebResponse response = ( HttpWebResponse ) request.GetResponse();
      StreamReader reader = new StreamReader( response.GetResponseStream() );
      importedContent.InnerHtml = reader.ReadToEnd();
    }
  }
}