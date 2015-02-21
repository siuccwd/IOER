using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Isle.BizServices;
using Isle.DTO.Common;
using Isle.Factories.Paradata;

namespace Isle.WebApi.Controllers
{
    public class ParadataController : ApiController
    {
      [HttpGet]
      [ActionName("AddLike")]
      public AJAXResponse AddLike( int id, string userGuid )
      {
        return new ParadataFactory().AddLikeDislike( id, userGuid, true );
      }

      [HttpGet]
      [ActionName("GetComments")]
      public AJAXResponse GetComments( int id, string userGuid )
      {
        return new ParadataFactory().GetComments( id, userGuid );
      }

    }
}