using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Isle.DTO.Common;
using Isle.Factories.Common;
using Isle.Factories.Filters;

namespace Isle.WebApi.Controllers
{
  public class FiltersController : ApiController
  {
    FiltersFactory factory = new FiltersFactory();

    // GET api/<controller>
    public AJAXResponse Get( int id = 5, bool includeDescriptions = false, bool mustHaveResources = false, bool includeCounts = false )
    {
      bool valid = false;
      string status = "";
      var data = factory.GetFilters( id, mustHaveResources, includeDescriptions, includeCounts, ref valid, ref status );

      return Utilities.Respond( data, valid, status, null );
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