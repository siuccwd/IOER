using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;
using Isle.DTO.Common;
using Isle.Factories.Common;
using System.Web.Configuration;

namespace Isle.Factories.Search
{
  public class SearchContact
  {
    public static AJAXResponse ContactServer( string method, string json, string urlAddendum )
    {
      string collection = "";
      string fullURL = "";
      try
      {
          collection = WebConfigurationManager.AppSettings[ "elasticSearchCollection" ].ToString();
        fullURL = WebConfigurationManager.AppSettings["elasticSearchURL"].ToString() + collection + urlAddendum;
        HttpWebRequest request = ( HttpWebRequest ) WebRequest.Create( fullURL );
        request.Method = method;

        if ( method == "POST" || method == "PUT" || json.Length > 0 )
        {
          request.ContentType = "application/json; charset=utf-8";
          byte[] byteData = Encoding.UTF8.GetBytes( json );
          request.ContentLength = byteData.Length;
          Stream requestStream = request.GetRequestStream();
          requestStream.Write( byteData, 0, byteData.Length );
          request.Timeout = 15000;
        }
        HttpWebResponse response = ( HttpWebResponse ) request.GetResponse();
        StreamReader reader = new StreamReader( response.GetResponseStream() );
        return Utilities.Respond( reader.ReadToEnd(), true, "okay", json );
      }
      catch ( Exception ex )
      {
        return Utilities.Respond( ex.Message, false, "Error: " + ex.Message, ex );
      }
    }
  }
}
