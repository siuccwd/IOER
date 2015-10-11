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

        [WebMethod]
        public JSONList GetCodeTableJSON( string tableName, string tableTitle, string esID )
        {
            //Get the data
            ResourceDataManager manager = new ResourceDataManager();
            ResourceDataManager.IResourceDataSubclass operatingTable = ResourceDataManager.ResourceDataSubclassFinder.getSubclassByName( tableName );
            DataSet ds = manager.GetCodetable( operatingTable );

            //Put the data in the list
            JSONList list = new JSONList();
            list.text = tableTitle;
            list.es_id = esID;
            list.items = new List<JSONListItem>();

            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    JSONListItem item = new JSONListItem();
                    item.id = int.Parse( DatabaseManager.GetRowColumn( dr, "Id" ) );
                    item.text = DatabaseManager.GetRowColumn( dr, "Title" );
                    list.items.Add( item );
                }
            }

            return list;
        }

        [WebMethod]
        public JSONFields GetCodeTables()
        {
            //assessmentType, careerCluster, educationalUse, mediaType, gradeLevel, groupType, endUser, itemType, language, resourceType
            JSONFields fields = new JSONFields();
            fields.fields = new List<JSONList>();
            fields.fields.Add( GetCodeTableJSON( "assessmentType", "Assessment Type", "assessmentType" ) );
            fields.fields.Add( GetCodeTableJSON( "careerCluster", "Career Cluster", "careerCluster" ) );
            fields.fields.Add( GetCodeTableJSON( "educationalUse", "Educational Use", "educationalUse" ) );
            fields.fields.Add( GetCodeTableJSON( "mediaType", "Media Type", "mediaType" ) );
            fields.fields.Add( GetCodeTableJSON( "gradeLevel", "Grade Level", "gradeLevel" ) );
            fields.fields.Add( GetCodeTableJSON( "groupType", "Group Type", "groupType" ) );
            fields.fields.Add( GetCodeTableJSON( "endUser", "End User", "endUser" ) );
            fields.fields.Add( GetCodeTableJSON( "itemType", "Item Type", "itemType" ) );
            fields.fields.Add( GetCodeTableJSON( "language", "Language", "language" ) );
            fields.fields.Add( GetCodeTableJSON( "resourceType", "Resource Type", "resourceType" ) );
            return fields;
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

        public class JSONFields
        {
            public List<JSONList> fields;
        }
        public class JSONList
        {
            public string es_id;
            public string text;
            public List<JSONListItem> items;
        }
        public class JSONListItem
        {
            public int id;
            public string text;
            public bool selected;
        }

        public class JSONQuery 
        {
            public string searchText;
            public Dictionary<string,int[]>[] narrowingOptions;
            public string sort;
            public int size;
            public int start;
        }

        [WebMethod]
        public string DoSearch3( JSONQuery2 query )
        {
          return DoSearch4( query, "" );
        }

        public List<JSONFilterV5> GetJSONFiltersV5()
        {
          //Get the data
          var filters = new List<JSONFilterV5>();
          var data = Isle.BizServices.CodeTableBizService.Site_SelectFilterCategories( 1 );

          foreach ( var item in data )
          {
            //Populate the filter
            var filter = new JSONFilterV5()
            {
              id = item.Id,
              title = item.Title,
              field = item.SchemaTag,
            };
            //Populate the tags in the filter
            foreach ( var tag in item.TagValues )
            {
              var newTag = new JSONTagV5()
              {
                id = tag.id,
                title = tag.title,
                selected = false
              };
              filter.tags.Add( newTag );
            }

            filters.Add( filter );
          }

          return filters;
        }
        public class JSONFilterV5
        {
          public JSONFilterV5()
          {
            tags = new List<JSONTagV5>();
          }
          public int id { get; set; }
          public string field { get; set; } //ElasticSearch index field name
          public string title { get; set; }
          public List<JSONTagV5> tags { get; set; }
        }
        public class JSONTagV5
        {
          public int id { get; set; }
          public string title { get; set; }
          public bool selected { get; set; }
        }

        [WebMethod( EnableSession = true )]
        public string DoSearchV5( JSONQueryV5 query )
        {
          //Clean up text
          query.text = query.text
                .Replace( "(", "" ).Replace( ")", "" )
                .Replace( "<", "" ).Replace( ">", "" )
                .Replace( "{", "" ).Replace( "}", "" )
                .Replace( "\\", "" );

          //Get list of all filters
          var filters = GetJSONFiltersV5();

          //Determine which fields to do full-text searches on
          var searchFields = new List<string>() { "title", "description", "url", "urlParts", "gradeLevelAliases", "standardAliases", "keywords", "creator", "publisher", "lrDocID" };
          foreach ( var filter in filters )
          {
            searchFields.Add( filter.field + ".tags" );
          }

          //Create the object that gets sent to ElasticSearch
          dynamic sendQuery = new
          {
            @bool = new
            {
              must = new List<object> 
              {
                new 
                {
                  query_string = new 
                  {
                    query = query.text,
                    use_dis_max = true,
                    default_operator = "and",
                    fields = searchFields
                  }
                },
                new 
                {
                  terms = new 
                  {
                    url = new string[] 
                    {
                      "http", "https", "ftp"
                    }
                  }
                }
              },
              should = new List<object>
              {
                new 
                {
                  terms = new 
                  {
                    title = query.text.Split(' ').Where(m => m.IndexOf("-") != 0).ToList(),
                    minimum_should_match = 1,
                    boost = 10.0
                  }
                },
                new 
                {
                  match_phrase = new 
                  {
                    title = new 
                    {
                      query = query.text,
                      slop = 5,
                      boost = 10.0
                    }
                  }
                },
                new 
                {
                  match_phrase = new 
                  {
                    description = new 
                    {
                      query = query.text,
                      slop = 2,
                      boost = 2.5
                    }
                  }
                },
                new 
                {
                  match_phrase = new 
                  {
                    keywords = new 
                    {
                      query = query.text,
                      slop = 1,
                      boost = 1.5
                    }
                  }
                }
              }
            }
          };

          //Add the filters
          foreach ( var item in query.filters )
          {
            var thing = new Dictionary<string, List<int>>();
            thing.Add( item.field + ".ids", item.items );
            sendQuery.@bool.must.Add(
              new
              {
                terms = thing
              }
            );
          }

          //Holds the final result
          dynamic jsonQ;

          //Handle sorting - Only add the object if needed
          if ( query.sort.Key != "" && query.sort.Value != "" )
          {
            jsonQ = new
            {
              sort = query.sort,
              size = query.size,
              from = query.start,
              query = sendQuery
            };
          }
          else
          {
            jsonQ = new
            {
              size = query.size,
              from = query.start,
              query = sendQuery
            };
          }

          var jsonQuery = serializer.Serialize( jsonQ );

          return new ElasticSearchManager().Search( jsonQuery, "collection6" );

        }
        public class JSONQueryV5
        {
          public JSONQueryV5() {
            text = "*";
            size = 20;
            start = 0;
            filters = new List<JSONQueryV5Filter>();
            targetFields = new List<string>();
            sort = new KeyValuePair<string, string>( "", "" );
          }
          public string text { get; set; }
          public int size { get; set; }
          public int start { get; set; }
          public List<JSONQueryV5Filter> filters { get; set; }
          public List<string> targetFields { get; set; }
          public KeyValuePair<string, string> sort { get; set; }
        }
        public class JSONQueryV5Filter
        {
          public JSONQueryV5Filter()
          {
            field = "";
            items = new List<int>();
          }
          public string field { get; set; }
          public List<int> items { get; set; }
        }

        [WebMethod( EnableSession = true )]
        public string DoSearch4( JSONQuery2 query, string targetFields )
        {
            //Clean up query text
            query.searchText = query.searchText
                .Replace( "(", "" ).Replace( ")", "" )
                .Replace( "<", "" ).Replace( ">", "" )
                .Replace( "{", "" ).Replace( "}", "" )
                .Replace( "\\", "" );

            //Text-based Narrowing options
            foreach ( jsonFilter item in query.narrowingOptions.textFilters )
            {
                foreach ( jsonFilterItem filter in item.items )
                {
                    query.searchText = filter.text + " " + query.searchText;
                }
            }

            //Setup the list of fields to do full-text searches on
            string[] searchFields = new string[] { "accessRights", "audiences", "clusters", "creator", "description", "gradeLevelAliases", "gradeLevels", "educationalUses", "groupTypes", "keywords", "languages", "mediaTypes", "notationParts", "publisher", "resourceTypes", "standardNotations", "subjects", "submitter", "title", "url", "urlParts", "usageRights" };

            if ( targetFields != "" )
            {
              searchFields = targetFields.Split( ',' );
            }

            //This will hold required items
            List<object> mustListNew = new List<object>();

            //Sorting options
            object sorting;
            if ( query.sort.field != null && query.sort.order != null )
            {
                Dictionary<string, string> sort = new Dictionary<string, string>();
                sort.Add( query.sort.field, query.sort.order );
                sorting = sort;
            }
            else
            {
                sorting = new { };
            }


            //This is the JSON that gets sent to elasticSearch
            dynamic jsonQ = new
            {
              sort = sorting,
              query = new
              {
                @bool = new
                {
                  must = new List<object>
                        {
                            new
                            {
                                query_string = new
                                {
                                    query = query.searchText,
                                    use_dis_max = true,
                                    default_operator = "and",
                                    fields = searchFields
                                }
                            },
                            new
                            {
                                terms = new
                                {
                                    url = new string[] 
                                    {
                                        "http", "https", "ftp"
                                    }
                                }
                            }
                        },
                  should = new List<object>
                        {
                            new 
                            {
                                terms = new 
                                {
                                    title = query.searchText.Split(' ').Where(m => m.IndexOf("-") != 0).ToList(),
                                    minimum_should_match = 1,
                                    boost = 10.0
                                }
                            },
                            new 
                            {
                                match_phrase = new 
                                {
                                    title = new 
                                    {
                                        query = query.searchText,
                                        slop = 5,
                                        boost = 10.0
                                    }
                                }
                            },
                            new 
                            {
                                match_phrase = new 
                                {
                                    description = new 
                                    {
                                        query = query.searchText,
                                        slop = 2,
                                        boost = 2.5
                                    }
                                }
                            },
                            new 
                            {
                                match_phrase = new 
                                {
                                    keywords = new 
                                    {
                                        query = query.searchText,
                                        slop = 1,
                                        boost = 1.5
                                    }
                                }
                            }
                        }
                }
              },
              size = query.size,
              from = query.start
            };

            IPB.IWebUser user = SessionManager.GetUserFromSession( Session );

            //ID-based Narrowing options
            foreach ( jsonFilter item in query.narrowingOptions.idFilters )
            {
                List<int> ints = new List<int>();
                Dictionary<string, int[]> finalFilter = new Dictionary<string, int[]>();
                foreach ( jsonFilterItem filter in item.items )
                {
                    ints.Add( filter.id );

                    if ( item.field.ToLower().Equals( "collectionids" ) )
                    {
                        //could be many, but not currently
                        ActivityBizServices.CollectionHit( filter.id, user, "Visit" );
                    }
                }
                finalFilter.Add( item.es, ints.ToArray() );

                object newItem = new
                {
                    terms = finalFilter
                };
                jsonQ.query.@bool.must.Add( newItem );

               
            }

            string jsonQuery = serializer.Serialize( jsonQ ).Replace( "\"sort\":{\"\":\"\"},", "" );

            ElasticSearchManager eManager = new ElasticSearchManager();
            //return eManager.DoElasticSearch( jsonQuery );
            //return new ElasticSearchManager2().Search( jsonQuery, "collection5" );
            return new ElasticSearchManager().Search( jsonQuery );
        }
        public class JSONQuery2
        {
          public JSONQuery2()
          {
            sort = new jsonSortingOptions();
            narrowingOptions = new jsonNarrowingOptions();
          }
            public string searchText { get; set; }
            public jsonSortingOptions sort { get; set; }
            public int start { get; set; }
            public int size { get; set; }
            public jsonNarrowingOptions narrowingOptions { get; set; }
        }
        public class jsonNarrowingOptions
        {
          public jsonNarrowingOptions()
          {
            idFilters = new List<jsonFilter>();
            textFilters = new List<jsonFilter>();
          }
            public List<jsonFilter> idFilters { get; set; }
            public List<jsonFilter> textFilters { get; set; }
        }
        public class jsonFilter
        {
          public jsonFilter()
          {
            items = new List<jsonFilterItem>();
          }
            public string es { get; set; }
            public string field { get; set; }
            public string title { get; set; }
            public List<jsonFilterItem> items { get; set; }
        }
        public class jsonFilterItem
        {
            public string text { get; set; }
            public int id { get; set; }
        }
        public class jsonSortingOptions
        {
            public string field { get; set; }
            public string order { get; set; }
        }

        [WebMethod]
        public string DoAPISearch( string text, List<APIFilter> filters, APIOptions options )
        {
          var searcher = new JSONQuery2();
          searcher.searchText = text;
          searcher.sort.field = options.sortField;
          searcher.sort.order = options.sortOrder;
          searcher.start = options.start;
          searcher.size = options.pageSize;

          foreach ( APIFilter inputFilter in filters )
          {
            var item = new jsonFilter();
            item.field = inputFilter.fieldName;
            item.es = inputFilter.fieldName;
            item.title = inputFilter.fieldName;

            foreach ( int id in inputFilter.tagIDs )
            {
              var filterItem = new jsonFilterItem();
              filterItem.id = id;
              item.items.Add( filterItem );
            }

            searcher.narrowingOptions.idFilters.Add( item );
          }

          return DoSearch3( searcher );
        }
        public class APIFilter
        {
          public string fieldName { get; set; }
          public List<int> tagIDs { get; set; }
        }
        public class APIOptions
        {
          public int start { get; set; }
          public int pageSize { get; set; }
          public string sortField { get; set; }
          public string sortOrder { get; set; }
        }

        [WebMethod( EnableSession = true )]
        public Services.UtilityService.GenericReturn DoSearchCollection7( JSONQueryV7 input )
        {
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

          //Setup the list of fields to do full-text searches on
          var listifiedText = input.text.Split(' ').Where(m => m.IndexOf("-") != 0).ToList();
          var positiveText = "";
          foreach ( var item in listifiedText ) { positiveText = positiveText + item + " "; }

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
            query.@bool.must.Add(
              new { nested = new { path = "Fields", query = new { @bool = new { must = new List<object> 
                { 
                  //new { match = new Dictionary<string,int>() { { "Fields.Id", item.Id } } }, 
                  new { terms = new Dictionary<string, List<int>>() { { "Fields.Ids", item.Ids } } } 
                } 
              } } } } 
            );
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
          if( input.not.Count() > 0 )
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
					var allFields = new List<string>() { "ResourceId^99", "LrDocId", "Title^15", "Title.English^10", "Title.Ngram", "Title.Raw^10", "Description", "Description.English", "Description.Ngram", "Description.Raw^2", "Url", "Url.Raw^5", "Creator^5", "Publisher^5", "Publisher.Ngram", "Publisher.English^3", "Publisher.Raw^2", "Submitter^2", "Keywords", "Keywords.English", "Keywords.Ngram", "Keywords.Raw^2", "GradeAliases", "StandardNotations", "StandardNotations.Ngram", "StandardNotations.Raw^2", "Fields.Tags^5", "Fields.Tags.Raw^10", "Title.Synonym^5", "Description.Synonym^5", "Publisher.Synonym^2", "Keywords.Synonym^2" };
					
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
        public class SortV7 {
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
