using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using LRWarehouse.DAL;
using System.Web.Script.Serialization;

namespace ILPathways.Services
{
  /// <summary>
  /// Summary description for StandardsService
  /// </summary>
  [WebService( Namespace = "http://tempuri.org/" )]
  [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
  [System.ComponentModel.ToolboxItem( false )]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  [System.Web.Script.Services.ScriptService]
  public class StandardsService : System.Web.Services.WebService
  {
    ElasticSearchManager manager = new ElasticSearchManager();
    JavaScriptSerializer serializer = new JavaScriptSerializer();

    [WebMethod]
    public string GetStandardBodies()
    {
      var query = new { query = new { term = new { levelType = "body" } } };
      var q = serializer.Serialize( query );
      return manager.Search( q, "standards", "standard" );
    }

    [WebMethod]
    public string GetDomains( int bodyID, List<string> grades )
    {
      var query = new { query = new { @bool = new { must = new List<dynamic>() } } };
      query.query.@bool.must.Add( new { term = new { bodyID = bodyID } } );
      query.query.@bool.must.Add( new { terms = new { grades = grades } } );
      var q = serializer.Serialize( query );
      return manager.Search( q, "standards", "standard" );
    }

    [WebMethod]
    public string GetDomain( int bodyID, int domainID, List<string> grades )
    {
      var query = new { query = new { @bool = new { must = new List<dynamic>() } }, sort = new { id = new { order = "asc" } } };
      query.query.@bool.must.Add( new { term = new { bodyID = bodyID } } );
      query.query.@bool.must.Add( new { terms = new { grades = grades } } );
      query.query.@bool.must.Add( new { term = new { domainID = domainID } } );
      var q = serializer.Serialize( query );
      return manager.Search( q, "standards", "standard" );
    }

    [WebMethod]
    public string GetStandards( List<string> standardIDs )
    {
      var query = new { query = new { terms = new { id = standardIDs } } };
      var q = serializer.Serialize( query );
      return manager.Search( q, "standards", "standard" );
    }

    [WebMethod]
    public string HelloWorld()
    {
      return "Hello World";
    }
  }
}
