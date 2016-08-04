using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

using System.Data;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using IPB = ILPathways.Business;
using IOER.classes;
using Isle.BizServices;
using LRWarehouse.Business.ResourceV2;
using Patron = LRWarehouse.Business.Patron;

namespace IOER.Services
{
    /// <summary>
    /// Summary description for ElasticSearchService
    /// </summary>
    [WebService( Namespace = "http://tempuri.org/" )]
    [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
    [System.ComponentModel.ToolboxItem( false )]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
	public class ElasticSearchService : System.Web.Services.WebService
	{
		JavaScriptSerializer serializer = new JavaScriptSerializer();
		string className = "ElasticSearchService";
		ResourceJSONManager jsonManager = new ResourceJSONManager();

		[WebMethod( EnableSession = true )]
		public string GetByVersionID( string versionID )
		{
			dynamic container = new Dictionary<string, object>();
			dynamic query = new Dictionary<string, object>();
			dynamic term = new Dictionary<string, object>();
			term.Add( "versionID", versionID );
			query.Add( "term", term );
			container.Add( "query", query );
			string jsonQuery = serializer.Serialize( container );
			return new ElasticSearchManager().Search( jsonQuery );
		}

		[WebMethod( EnableSession = true )]
		public void AddResourceView( string intID, string ID )
		{
			int userID = GetUserID( ID );
			int id = int.Parse( intID );

			new ResourceViewManager().Create( id, userID );
			//new ElasticSearchManager().RefreshResource( id );
			new Isle.BizServices.ResourceV2Services().RefreshResource( id );
		}

		/// <summary>
		/// Add detail view for a resource.
		/// 15-03-25 mparsons - This was not being used - apparantly we didn't want to publish to the LR.
		///                     It would be useful for activity. So from this point on, we will log as activity.
		/// </summary>
		/// <param name="intID"></param>
		/// <param name="ID"></param>
		[WebMethod( EnableSession = true )]
		public void AddDetailView( string intID, string ID )
		{
			int userID = GetUserID( ID );
			int resourceId = int.Parse( intID );

			new ResourceViewManager().CreateDetailPageView( resourceId, userID );

			//Skipping - this is done on the detail page
			//ActivityBizServices.ResourceDetailHit( resourceId, "title", userID );

			//new ElasticSearchManager().RefreshResource( id );
			new Isle.BizServices.ResourceV2Services().RefreshResource( resourceId );
		}
		private int GetUserID( string ID )
		{
			int userID = 0;
			if ( ID != "" )
			{
				try
				{
					Patron user = new PatronManager().GetByRowId( ID );
					userID = user.Id;
				}
				catch
				{
					return userID;
				}
			}
			return userID;
		}

		[WebMethod( EnableSession = true )]
		public Services.UtilityService.GenericReturn DoSearchCollection7( JSONQueryV7 input )
		{
			//Experimental - force a newest first search if the query is for a blind relevance search
			if ( input.sort.field == "" || input.sort.field == "_score" )
			{
				if ( ( input.text == "*" || input.text == "" ) && input.fields.Count() == 0 && input.allStandardIDs.Count() == 0 )
				{
					input.sort.field = "ResourceId";
					input.sort.order = "desc";
				}
			}

			//Attempt to figure out which field the user is looking for, if any
			//This needs to happen before : is stripped out
			var updatedText = input.text;
			var searchFields = GetSearchFields( ref updatedText );
			input.text = updatedText;

			//Clean up text
			var specialCharacters = new List<string>() { "=", "&&", "||", "<", ">", "!", "(", ")", "{", "}", "[", "]", "^", "~", "?", "\\", "/", ":" }; //also " and * but those can be useful
			input.text = input.text.Trim();
			foreach ( var item in specialCharacters )
			{
				input.text = input.text.Replace( item, " " );
			}
			input.text = input.text.Replace( "|", " OR " );

			if ( string.IsNullOrWhiteSpace( input.text ) )
			{
				input.text = "*";
			}
			//remove quotes from single-word queries
			if ( input.text.IndexOf( " " ) == -1 )
			{
				input.text = input.text.Replace( '"', ' ' ).Trim();
			}

			//Construct the query object
			dynamic query = new
			{
				@bool = new
				{
					must = new List<object>
              {
                new { query_string = new { query = input.text, use_dis_max = true, default_operator = "and", lenient = true, fields = searchFields } },
              },
					should = new List<object>
              {
                new { nested = new { path = "Fields", query = new { @bool = new { should = new {
                  query_string = new { query = input.text, use_dis_max = true, default_operator = "and", lenient = true, fields = new List<string>() { "Fields.Tags^5", "Fields.Tags.Raw^10" } } 
                } } } } },
                //new { match_phrase = new { Title = new { query = input.text, slop = 5, boost = 10.0 } } },
                //new { match_phrase = new { Description = new { query = input.text, slop = 2, boost = 2.5 } } },
                //new { match_phrase = new { Keywords = new { query = input.text, slop = 1, boost = 1.5 } } },
              },
					must_not = new List<object>
					{

					}
				}
			};

			//Add the filters
			foreach ( var item in input.fields )
			{
				if ( item.Id == 36 || item.Schema == "usageRights" ) //kludge
				{
					query.@bool.must.Add(
						new
						{
							terms = new
							{
								UsageRightsId = item.Ids
							}
						}
					);
				}
				else
				{
					query.@bool.must.Add(
						new
						{
							nested = new
							{
								path = "Fields",
								query = new
								{
									@bool = new
									{
										must = new List<object> 
                { 
                  //new { match = new Dictionary<string,int>() { { "Fields.Id", item.Id } } }, 
                  new { terms = new Dictionary<string, List<int>>() { { "Fields.Ids", item.Ids } } } 
                }
									}
								}
							}
						}
					);
				}
			}

			//Standards
			if ( input.allStandardIDs.Count() > 0 )
			{
				query.@bool.must.Add(
				  new { terms = new { StandardIds = input.allStandardIDs } }
				);
			}

			//Library and Collection IDs
			if ( input.libraryIDs.Count() > 0 )
			{
				query.@bool.must.Add(
				  new { terms = new { LibraryIds = input.libraryIDs } }
				);
			}
			if ( input.collectionIDs.Count() > 0 )
			{
				query.@bool.must.Add(
				  new { terms = new { CollectionIds = input.collectionIDs } }
				);
			}

			//filter things out
			if ( input.not.Count() > 0 )
			{
				query.@bool.must_not.Add(
				  new { query_string = new { query = input.not, use_dis_max = false, default_operator = "or", lenient = false, fields = new List<string>() { "Publisher", "Creator", "Submitter" } } }
				);
			}

			//Holds the final result
			dynamic jsonQuery;

			//Add sort only if sort is needed
			if ( input.sort.field != "" && input.sort.order != "" )
			{
				jsonQuery = new
				{
					sort = new Dictionary<string, object>() { { input.sort.field, new { order = input.sort.order } } },
					size = input.size,
					from = input.start,
					query = query
				};
			}
			else
			{
				jsonQuery = new
				{
					size = input.size,
					from = input.start,
					query = query
				};
			}

			if ( input.collectionIDs.Count() == 1 ) //skip cases where no collection is hit, or potential future case where multiple collections are searched for
			{
				ActivityBizServices.CollectionHit( input.collectionIDs.First(), SessionManager.GetUserFromSession( Session ), "Visit" );
			}

			var queryJSON = serializer.Serialize( jsonQuery );
			var result = new ElasticSearchManager().Search( queryJSON, "mainSearchCollection", "resource" );

			return Services.UtilityService.DoReturn( result, true, "", new { currentlyRebuildingIndex = ( ServiceHelper.GetAppKeyValue( "currentlyRebuildingIndex", "no" ) == "yes" ) } );
			//return Services.UtilityService.DoReturn( result, true, "", queryJSON );
			//return Services.UtilityService.DoReturn( result, true, "", null );
		}
		public List<string> GetSearchFields( ref string query )
		{
			query = query.ToLower().Trim();

			//All available fields, based on elasticsearch mapping
			var allFields = new List<string>() { "ResourceId^99", "LrDocId", "Title^15", "Title.English^10", "Title.Ngram", "Title.Raw^10", "Description", "Description.English", "Description.Ngram", "Description.Raw^2", "Url", "Url.Raw^5", "Creator^5", "Publisher^5", "Publisher.Ngram", "Publisher.English^3", "Publisher.Raw^2", "Submitter^2", "Keywords", "Keywords.English", "Keywords.Ngram", "Keywords.Raw^2", "GradeAliases", "StandardIds", "StandardNotations", "StandardNotations.Ngram", "StandardNotations.Raw^2", "Fields.Tags^5", "Fields.Tags.Raw^10", "Title.Synonym^5", "Description.Synonym^5", "Publisher.Synonym^2", "Keywords.Synonym^2", "UsageRightsId", "UsageRightsUrl" };

			//Determine which field the user is looking for, if any
			//Skip checks if there is no target field
			if ( query.IndexOf( ":" ) == -1 )
			{
				return allFields;
			}

			try
			{
				//Otherwise, attempt to determine the target fields
				//Note - does not handle individual filters at this time
				//First break the query up and extract the first item
				var parts = query.Split( new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries );
				//Can search for multiple targets, ie "publisher,submitter,creator"
				var targetFields = parts[ 0 ].Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries );
				//Now reassemble the query sans the first item
				parts[ 0 ] = "";
				query = string.Join( " ", parts );
				//Continue on
				var searchFields = new List<string>();
				foreach ( var item in allFields )
				{
					foreach ( var target in targetFields )
					{
						if ( item.ToLower().IndexOf( target ) == 0 )
						{
							searchFields.Add( item );
						}
					}
				}

				//Return the result, or the whole list
				if ( searchFields.Count() > 0 )
				{
					return searchFields;
				}
				else
				{
					return allFields;
				}
			}
			catch
			{
				return allFields;
			}

		}
		public class JSONQueryV7
		{
			public JSONQueryV7()
			{
				fields = new List<FieldES>();
				allStandardIDs = new List<int>();
				libraryIDs = new List<int>();
				collectionIDs = new List<int>();
				sort = new SortV7();
				not = "";
			}
			public string text { get; set; }
			public List<FieldES> fields { get; set; }
			public List<int> allStandardIDs { get; set; }
			public List<int> libraryIDs { get; set; }
			public List<int> collectionIDs { get; set; }
			public SortV7 sort { get; set; }
			public int size { get; set; }
			public int start { get; set; }
			public string not { get; set; }
		}
		public class SortV7
		{
			public SortV7()
			{
				field = "";
				order = "";
			}
			public string field { get; set; }
			public string order { get; set; }
		}
	}
}
