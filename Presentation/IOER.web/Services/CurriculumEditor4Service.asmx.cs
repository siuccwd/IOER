using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using ILPathways.Business;
using Isle.BizServices;
using System.Web.Script.Serialization;

namespace IOER.Services
{
	/// <summary>
	/// Summary description for CurriculumEditor4Service
	/// </summary>
	[WebService( Namespace = "http://tempuri.org/" )]
	[WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
	[System.ComponentModel.ToolboxItem( false )]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[System.Web.Script.Services.ScriptService]
	public class CurriculumEditor4Service : System.Web.Services.WebService
	{
		#region Services
		CurriculumServices curriculumService = new CurriculumServices();
		JavaScriptSerializer serializer = new JavaScriptSerializer();
		#endregion

		/// <summary>
		/// Get a JSON style tree of a curriculum
		/// </summary>
		/// <param name="nodeID"></param>
		/// <returns></returns>
		[WebMethod( EnableSession = true )]
		public string GetJsonTree( int nodeID )
		{
			var raw = curriculumService.GetCurriculumOutlineForEdit( nodeID );
			return serializer.Serialize( raw );
		}
		//

	}
}
