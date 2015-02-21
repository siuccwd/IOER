using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Script.Serialization;

using Isle.DTO.Common;


namespace Isle.Factories.Common
{
  public class Utilities
  {
    JavaScriptSerializer serializer = new JavaScriptSerializer();

    public string Serialize( object data )
    {
      return serializer.Serialize( data );
    }
    T Deserialize<T>( string data ) where T : class
    {
      return serializer.Deserialize<T>( data );
    }

    public static string RespondString( object data, bool valid, string status, object extra )
    {
      return new JavaScriptSerializer().Serialize( Respond( data, valid, status, extra ) );
    }
    public static AJAXResponse Respond( object data, bool valid, string status, object extra )
    {
      return new AJAXResponse()
      {
        data = data,
        valid = valid,
        status = status,
        extra = extra
      };
    }

    public static List<string> SplitListString( string input, string separator )
    {
      try
      {
        return input.Split( new string[] { separator }, StringSplitOptions.RemoveEmptyEntries ).ToList();
      }
      catch
      {
        return new List<string>();
      }
    }
    public static List<int> SplitListInt( string input, string separator )
    {
      try
      {
        return SplitListString( input, separator ).Select( int.Parse ).ToList();
      }
      catch
      {
        return new List<int>();
      }
    }

    public SiteParams GetSiteParams( int id )
    {
      var apiRoot = WebConfigurationManager.AppSettings[ "apiRoot" ].ToString();
      //Proper database call should replace this
      var siteData = new List<SiteParams>
      {
        new SiteParams() { siteID = 1, siteTitle = "IOER", cssThemes = new List<string>() { "ioer.css" }, hasStandardsBrowser = true, apiRoot = apiRoot },
        new SiteParams() { siteID = 2, siteTitle = "disabilityworks", cssThemes = new List<string>() { "disabilityworks.css" }, hasStandardsBrowser = false, apiRoot = apiRoot },
        new SiteParams() { siteID = 3, siteTitle = "Illinois workNet", cssThemes = new List<string>() { "worknet.css" }, hasStandardsBrowser = false, apiRoot = apiRoot },
        new SiteParams() { siteID = 4, siteTitle = "Illinois workNet Manufacturing (ATIM)", cssThemes = new List<string>() { "manufacturing.css" }, hasStandardsBrowser = true, apiRoot = apiRoot },
        new SiteParams() { siteID = 5, siteTitle = "Central Resource Site", cssThemes = new List<string>() { "neutral.css" }, hasStandardsBrowser = true, apiRoot = apiRoot }
      };

      return siteData.Where( item => item.siteID == id ).First();
    }

  }
}
