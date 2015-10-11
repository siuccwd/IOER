using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using System.Web.Script.Serialization;

using IOER.classes;
using LRWarehouse.Business;
using Isle.BizServices;
using Isle.DTO;

namespace IOER.Services
{
	/// <summary>
	/// Summary description for ContentSearch2Service
	/// </summary>
	[WebService( Namespace = "http://tempuri.org/" )]
	[WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
	[System.ComponentModel.ToolboxItem( false )]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[System.Web.Script.Services.ScriptService]
	public class ContentSearch2Service : System.Web.Services.WebService
	{
		JavaScriptSerializer serializer = new JavaScriptSerializer();
		UtilityService usrv = new UtilityService();

		[WebMethod(EnableSession=true)]
		public string DoSearchJSON( ContentSearchQuery query )
		{
			try {
				var total = 0;
				var message = "";
				var results = DoSearch(query, ref total, ref message);
				return serializer.Serialize(UtilityService.DoReturn(results, true, "okay", new { TotalResults = total, Message = message }));
			}
			catch(Exception ex){
				return serializer.Serialize( UtilityService.DoReturn( null, false, "There was an error performing the search. Please refresh the page and try again.", new { Error = ex.Message } ) );
			}
		}
		public List<ContentSearchResult> DoSearch( ContentSearchQuery query, ref int total, ref string message )
		{
			//Check for user, to see if anything might be editable
			//var user = SessionManager.GetUserFromSession( Session ) ?? new Patron();
			var user = usrv.GetUserFromSession();
			var results = new List<ContentSearchResult>();
			//Do search
			results = new ContentSearchServices().Search( query, user, ref total );

			message = query.Message; //May change this implementation soon(tm)
			

			return results;
		}

	}
}
