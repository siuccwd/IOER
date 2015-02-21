using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Isle.BizServices;
using Isle.DTO.Common;


namespace Isle.Factories.Paradata
{
  public class ParadataFactory
  {
    public AJAXResponse AddLikeDislike( int resourceID, string userGuid, bool isLike )
    {
      var status = "";
      var success = false;

      new ParadataService().AddLikeDislike( resourceID, userGuid, isLike, ref success, ref status );

      return new AJAXResponse() { data = null, valid = success, status = status, extra = null };
    }

    public AJAXResponse GetComments( int resourceID, string userGuid )
    {
      var userCanPost = false;
      var status = "";
      var success = false;

      var comments = new ParadataService().GetComments( resourceID, userGuid, ref userCanPost, ref success, ref status );

      return new AJAXResponse() { data = comments, valid = success, status = status, extra = userCanPost };
    }
  }
}
