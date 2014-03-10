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

namespace ILPathways.Services
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

        [WebMethod]
        public TestObj getTestObj( TestObj input )
        {
            input.name = input.name + "_added";
            input.age = input.age + 10;
            input.items = new string[] {"test", "test2"};
            return input;
        }

        public class TestObj
        {
            public string name = "";
            public int age = 0;
            public string[] items;
        }

        [WebMethod]
        public string GetByVersionID( string versionID )
        {
            dynamic container = new Dictionary<string, object>();
            dynamic query = new Dictionary<string, object>();
            dynamic term = new Dictionary<string, object>();
            term.Add( "versionID", versionID );
            query.Add( "term", term );
            container.Add( "query", query );
            string jsonQuery = serializer.Serialize( container );
            return new ElasticSearchManager().DoElasticSearch( jsonQuery );
        }

        [WebMethod]
        public string DoSearchWidget( string searchText, string pageSize )
        {
            HttpContext.Current.Response.AddHeader( "Access-Control-Allow-Origin", "*" );
            if ( HttpContext.Current.Request.HttpMethod == "OPTIONS" )
            {
                HttpContext.Current.Response.AddHeader( "Access-Control-Allow-Origin", "file://" );
                HttpContext.Current.Response.AddHeader( "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE" );
                HttpContext.Current.Response.AddHeader( "Access-Control-Allow-Headers", "Content-Type, Accept, x-csrf-token, Origin" );
                HttpContext.Current.Response.AddHeader( "Access-Control-Max-Age", "1728000" );
                HttpContext.Current.Response.End();
            }
            string returnText = DoSearch( searchText, "", pageSize, "0", "" );
            return returnText;
        }

        [WebMethod]
        public string DoSearchWidgetJSONP( string searchText, string pageSize, string jsoncallback )
        {
            HttpContext.Current.Response.AddHeader( "Access-Control-Allow-Origin", "*" );
            if ( HttpContext.Current.Request.HttpMethod == "OPTIONS" )
            {
                HttpContext.Current.Response.AddHeader( "Access-Control-Allow-Methods", "GET, POST, OPTIONS" );
                HttpContext.Current.Response.AddHeader( "Access-Control-Allow-Headers", "Content-Type, Accept" );
                HttpContext.Current.Response.AddHeader( "Access-Control-Max-Age", "1728000" );
                HttpContext.Current.Response.End();
            }
            string returnText = DoSearch( searchText, "", pageSize, "0", "" );
            return returnText;
        }

        [WebMethod]
        public string DoSearch( string searchText, string narrowingOptions, string pageSize, string offset, string sort )
        {
            //container - level 0
            dynamic dContainer_0 = new Dictionary<string, object>();
                //query - level 1
                dynamic dQuery_1 = new Dictionary<string, object>();
                    //filtered - level 2
                    dynamic dFiltered_2 = new Dictionary<string, object>();
                        //query - level 3
                        dynamic dQuery_3 = new Dictionary<string, object>();
                            //bool - level 4
                            dynamic dBool_4 = new Dictionary<string, object>();
                                //must - level 5
                                dynamic dMust_5 = new Dictionary<string, object>();
                                //should - level 5
                                dynamic dShould_5 = new Dictionary<string, object>();
                        //filter - level 3
                        dynamic dFilter_3 = new Dictionary<string, object>();
                            //bool - level 4
                            dynamic dFilterBool_4 = new Dictionary<string, object>();
                                //must - level 5 (array)

                                    //terms - level 6

                                        //ColumnNames - level 7 (array)
                //highlight - level 1
                dynamic dHighlight_1 = new Dictionary<string, object>();
                //sort - level 1
                dynamic dSort_1 = new Dictionary<string, object>();

            //Setup query
            if ( searchText == "" )
            {
                dynamic dMatchAll_6 = new Dictionary<string, object>();
                dMust_5.Add( "match_all", dMatchAll_6 );
                dBool_4.Add( "must", dMust_5 );
            }
            else
            {
                searchText = searchText
                    .Replace( "(", "" ).Replace( ")", "" )
                    .Replace( "<", "" ).Replace( ">", "" )
                    .Replace( "{", "" ).Replace( "}", "" )
                    .Replace( "\\", "" );

                dynamic dMultiMatch_6 = new Dictionary<string, object>();
                dMultiMatch_6.Add( "query", searchText );
                dMultiMatch_6.Add( "use_dis_max", true );
                dMultiMatch_6.Add( "default_operator", "and" );
                dMultiMatch_6.Add( "fields", new string[] {
                    "accessRights",
                    "audiences",
                    "clusters",
                    "description",
                    "gradeLevelAliases",
                    "gradeLevels",
                    "educationalUses",
                    "groupTypes",
                    "keywords",
                    "languages",
                    "mediaTypes",
                    "notationParts",
                    "publisher",
                    "resourceTypes",
                    "standardNotations",
                    "subjects",
                    "title",
                    "url",
                    "urlParts",
                    "usageRights",
                } );

                dynamic dMatchPhrase_6 = new Dictionary<string, object>();
                dynamic dDescription_7 = new Dictionary<string, object>();
                dDescription_7.Add( "query", searchText );
                dDescription_7.Add( "slop", 1 );
                dDescription_7.Add( "boost", 5 );

                dMatchPhrase_6.Add( "description", dDescription_7 );

                dMust_5.Add( "query_string", dMultiMatch_6 );
                dShould_5.Add( "match_phrase", dMatchPhrase_6 );

                dBool_4.Add( "must", dMust_5 );
                dBool_4.Add( "should", dShould_5 );
            }

            dQuery_3.Add( "bool", dBool_4 );
            dFiltered_2.Add( "query", dQuery_3 );


            //Setup filters
            string[] fieldValuePairs = narrowingOptions.Split( new string[] { "|@|" }, StringSplitOptions.RemoveEmptyEntries );
            dynamic dFiltersMust_5 = new Dictionary<string, object>[fieldValuePairs.Length];
            for ( int i = 0 ; i < fieldValuePairs.Length ; i++ )
            {
                string[] temp = fieldValuePairs[i].Split( new string[] { "|~|" }, StringSplitOptions.RemoveEmptyEntries );
                string fieldName = temp[ 0 ];
                string[] values = temp[ 1 ].ToLower().Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries );

                dynamic dColumnNames_7 = new Dictionary<string, object>();
                dColumnNames_7.Add( fieldName, values );
                dynamic dTerm_6 = new Dictionary<string, object>();
                dTerm_6.Add( "terms", dColumnNames_7 );
                dFiltersMust_5[ i ] = dTerm_6;
            }

            if ( fieldValuePairs.Length > 0 )
            {
                dFilterBool_4.Add( "must", dFiltersMust_5 );
                dFilter_3.Add( "bool", dFilterBool_4 );
                dFiltered_2.Add( "filter", dFilter_3 );
            }

            //Sorting
            if ( sort.Length > 1 )
            {
                string[] sorts = sort.Split( '|' );
                dynamic dSortColumn_2 = new Dictionary<string, object>();
                dSortColumn_2.Add( "order", sorts[ 1 ] );
                dSortColumn_2.Add( "ignore_unmapped", "true" );
                dSort_1.Add( sorts[ 0 ], dSortColumn_2 );
            }

            //add the prepared objects to the core object
            dQuery_1.Add("filtered", dFiltered_2 );
            if ( sort.Length > 1 )
            {
                dContainer_0.Add( "sort", dSort_1 );
            }
            dContainer_0.Add( "query", dQuery_1 );

            //Setup the pagination and page size
            dContainer_0.Add( "size", pageSize );
            dContainer_0.Add( "from", offset );

            string jsonQuery = serializer.Serialize( dContainer_0 );

            ElasticSearchManager eManager = new ElasticSearchManager();
            return eManager.DoElasticSearch( jsonQuery );
        }

        [WebMethod]
        public void AddResourceView( string intID, string ID )
        {
            int userID = GetUserID( ID );
            
            new ResourceViewManager().Create( int.Parse( intID ), userID );
            new ElasticSearchManager().AddResourceView( intID );
        }

        [WebMethod]
        public void AddDetailView( string intID, string ID )
        {
            int userID = GetUserID( ID );

            new ResourceViewManager().CreateDetailPageView( int.Parse( intID ), userID );
            new ElasticSearchManager().AddDetailView( intID );
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
        public Widget_Details FetchCodes()
        {
            return new ResourceService().Get_Details( 0 );
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
        public string GetRecord( int versionID )
        {
            return new ElasticSearchManager().GetByVersionID( versionID );
        }

        [WebMethod]
        public string DoSearch3( JSONQuery2 query )
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
                                match_phrase = new 
                                {
                                    description = new 
                                    {
                                        query = query.searchText,
                                        slop = 1,
                                        boost = 5
                                    }
                                }
                            }
                        }
                    }
                },
                size = query.size,
                from = query.start
            };

            //ID-based Narrowing options
            foreach ( jsonFilter item in query.narrowingOptions.idFilters )
            {
                List<int> ints = new List<int>();
                Dictionary<string, int[]> finalFilter = new Dictionary<string, int[]>();
                foreach ( jsonFilterItem filter in item.items )
                {
                    ints.Add( filter.id );
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
            return eManager.DoElasticSearch( jsonQuery );
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
        public string DoSearch2( JSONQuery query )
        {
            //Clean up query text
            query.searchText = query.searchText
                .Replace( "(", "" ).Replace( ")", "" )
                .Replace( "<", "" ).Replace( ">", "" )
                .Replace( "{", "" ).Replace( "}", "" )
                .Replace( "\\", "" );

            //Setup the list of fields to do full-text searches on
            string[] searchFields = new string[] { "accessRights", "audiences", "clusters", "description", "gradeLevelAliases", "gradeLevels", "educationalUses", "groupTypes", "keywords", "languages", "mediaTypes", "notationParts", "publisher", "resourceTypes", "standardNotations", "subjects", "title", "url", "urlParts", "usageRights" };

            //This will hold required items
            List<object> mustListNew = new List<object>();

            //Sorting options
            object sorting;
            if ( query.sort != "|" )
            {
                string[] sortParts = query.sort.Split( '|' );
                Dictionary<string, string> sort = new Dictionary<string, string>();
                sort.Add( sortParts[ 0 ], sortParts[ 1 ] );
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
                                match_phrase = new 
                                {
                                    description = new 
                                    {
                                        query = query.searchText,
                                        slop = 1,
                                        boost = 5
                                    }
                                }
                            }
                        }
                    }
                },
                size = query.size,
                from = query.start
            };

            //Narrowing options
            foreach ( Dictionary<string, int[]> item in query.narrowingOptions )
            {
                object newItem = new
                {
                    terms = item
                };
                jsonQ.query.@bool.must.Add( newItem );
            }

            string jsonQuery = serializer.Serialize( jsonQ );

            ElasticSearchManager eManager = new ElasticSearchManager();
            return eManager.DoElasticSearch( jsonQuery );

        }

    }
}
