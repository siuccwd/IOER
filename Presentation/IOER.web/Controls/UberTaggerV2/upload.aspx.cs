using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Script.Serialization;

namespace ILPathways.Controls.UberTaggerV2
{
  public partial class upload : System.Web.UI.Page
  {
    JavaScriptSerializer serializer = new JavaScriptSerializer();

    public string LoadMessage { get; set; }

    protected void Page_Load( object sender, EventArgs e )
    {
      //If something happened
      if ( IsPostBack )
      {
        //Get data
        var info = serializer.Deserialize<UploadInfo>( hdnData.Value );

        //If we need to remove a file...
        if ( info.command == "remove" )
        {
          if ( info.contentID == 0 )
          {
            LoadMessage = GetLoadMessage( false, info.command, "Invalid File ID.", "", 0, "Content ID is 0" );
            return;
          }
          if ( info.resourceID != 0 )
          {
            LoadMessage = GetLoadMessage( false, info.command, "You can't delete a file that has already been published.", "", info.contentID, "Resource ID is " + info.resourceID );
            return;
          }
          //Handle file removal

          //Return to client
          LoadMessage = GetLoadMessage( true, info.command, "File removed.", "", info.contentID, "" );
        }
        //If we need to upload/replace a file...
        else if ( info.command == "upload" )
        {
          //Handle file upload
          if ( !fileUpload.HasFile )
          {
            LoadMessage = GetLoadMessage( false, info.command, "No file selected.", "", info.contentID, "No file detected" );
            return;
          }


          var tempURL = "http://ioer.ilsharedlearning.org/file/blah.pdf";

          //Send to client
          LoadMessage = GetLoadMessage( true, info.command, "Upload Successful", tempURL, 999, "" );
        }
        //Should not occur
        else
        {
          LoadMessage = GetLoadMessage( false, info.command, "There was an error processing the upload.", "", info.contentID, "Unknown error" );
        }
      }
      //Nothing happened, just a page load
      else
      {
        LoadMessage = GetLoadMessage( true, "load", "Ready", "", 0, "" );
      }
    }

    private string GetLoadMessage( bool valid, string command, string status, string url, int contentID, object extra )
    {
      return serializer.Serialize( new { type = "uploadMessage", command, valid = valid, status = status, url = url, contentID = contentID, extra = extra } );
    }

    public class UploadInfo
    {
      public string command { get; set; }
      public int resourceID { get; set; }
      public int contentID { get; set; }
    }
  }
}