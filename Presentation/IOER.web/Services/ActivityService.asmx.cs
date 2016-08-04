using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using Isle.BizServices;
using LRWarehouse.Business;
using Patron = LRWarehouse.Business.Patron;

namespace IOER.Services
{
  /// <summary>
  /// Summary description for ActivityService
  /// </summary>
  [WebService( Namespace = "http://tempuri.org/" )]
  [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
  [System.ComponentModel.ToolboxItem( false )]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  [System.Web.Script.Services.ScriptService]
  public class ActivityService : System.Web.Services.WebService
  {

    [WebMethod(EnableSession=true)]
    public void LearningList_DocumentHit( int curriculumID, int nodeID, int contentID )
    {
	  var user = AccountServices.GetUserFromSession( Session );
      ActivityBizServices.LearningList_DocumentHit( curriculumID, nodeID, contentID, user );
    }

	  /// <summary>
	/// called from Widgets.Resource/AddView and ?
	  /// </summary>
	  /// <param name="resourceID"></param>
	  /// <param name="title"></param>
    [WebMethod(EnableSession=true)]
    public void Resource_ResourceView(int resourceID, string title)
    {
		var user = AccountServices.GetUserFromSession( Session );
      new ResourceV2Services().AddResourceClickThrough( resourceID, user, title );
    }
  }
}
