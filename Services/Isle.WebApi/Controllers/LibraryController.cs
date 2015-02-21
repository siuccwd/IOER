using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Isle.DTO;
using Isle.DTO.Common;
using Isle.BizServices;

using Mgr = Isle.Factories.LibraryFactory;

namespace Isle.WebApi.Controllers
{
    public class LibraryController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public LibraryDTO Get( int id )
        {
            LibraryDTO entity = new LibraryDTO();
            entity = Mgr.Get( id );
            if ( entity == null || entity.Id == 0 )
                return new LibraryDTO();
            else
                return entity;
        }

        [HttpGet]
        public AJAXResponse AddToDefaultLibCol( int id, string userGuid )
        {
          return new Factories.LibraryFactory().AddToDefaultLibCol( id, userGuid );
        }

    }
}