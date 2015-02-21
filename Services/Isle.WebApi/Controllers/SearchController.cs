using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Isle.Factories.Common;
using Isle.Factories.Search;
using Isle.DTO.Common;

namespace Isle.WebApi.Controllers
{
  public class SearchController : ApiController
  {
    // GET api/<controller>
    public AJAXResponse Get( int id = 5, string searchText = "", string ids = "", int libraryID = 0, int collectionID = 0, string standardIDs = "", int pageSize = 20, int pageStart = 0, string sortMode = "|" )
    {
      var idList = new List<int>();
      var standardIDList = new List<int>();
      if ( ids != null && ids.Length > 0 )
      {
        idList = ids.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( int.Parse ).ToList<int>();
      }
      if ( standardIDs != null && standardIDs.Length > 0 )
      {
        standardIDList = standardIDs.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( int.Parse ).ToList<int>();
      }
      var sortParts = sortMode.Split( '|' );

      return SearchFactory.DoSearch( id, searchText, idList, libraryID, collectionID, standardIDList, pageSize, pageStart, sortParts[ 0 ], sortParts[ 1 ] );
    }

    // POST api/<controller>
    public void Post( [FromBody]string value )
    {
    }

    // PUT api/<controller>/5
    public void Put( int id, [FromBody]string value )
    {
    }

    // DELETE api/<controller>/5
    public void Delete( int id )
    {
    }
  }
}