using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using ILPathways.Utilities;
using LRWarehouse.DAL;
using System.Web.Script.Serialization;

namespace ILPathways.Services
{
  /// <summary>
  /// Summary description for PublisherService
  /// </summary>
  [WebService( Namespace = "http://tempuri.org/" )]
  [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
  [System.ComponentModel.ToolboxItem( false )]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  [System.Web.Script.Services.ScriptService]
  public class PublisherService : System.Web.Services.WebService
  {

  }
}
