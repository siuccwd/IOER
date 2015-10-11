using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;


namespace Isle.BizServices
{
	public class WorkNetListFactoryServices
	{
		public string GetJSONFromList( string listName, string query )
		{
			string host = ServiceHelper.GetAppKeyValue("sptHost", "www2.illinoisworknet.com");
			//Setup the API call
			var url = Uri.EscapeUriString( "http://{host}/_api/web/lists/getByTitle('{listName}')/items?{query}"
				.Replace("{host}", host) 
				.Replace( "{listName}", listName )
				.Replace( "{query}", query ) );
			//Create the request object
			var request = ( HttpWebRequest ) HttpWebRequest.Create( url );
			request.Method = "GET";
			request.Accept = "application/json;odata=verbose";
			//Get the response
			var response = ( HttpWebResponse ) request.GetResponse();
			//Read the response
			var reader = new StreamReader( response.GetResponseStream(), Encoding.UTF8 );
			var reply = reader.ReadToEnd();
			//Close the streams
			response.Close();
			reader.Close();
			//Return the JSON
			return reply;
		}

		public T GetFromList<T>( string listName, string query )
		{
			return new JavaScriptSerializer().Deserialize<T>( GetJSONFromList( listName, query ) );
		}

		public List<T> GetListFromList<T>( string listName, string query )
		{
			return new JavaScriptSerializer().Deserialize<ResponseObject<T>>( GetJSONFromList( listName, query ) ).d.Results;
		}

		public class ResultsObject<T>
		{
			public ResultsObject()
			{
				Results = new List<T>();
			}
			public List<T> Results { get; set; }
		}

		public class ResponseObject<T>
		{
			public ResponseObject()
			{
				d = new ResultsObject<T>();
			}
			public ResultsObject<T> d { get; set; }
		}
	}
}
