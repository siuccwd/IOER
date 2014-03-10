using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using Microsoft.ApplicationBlocks.Data;
using ILPathways.Utilities;
using LRWarehouse.Business;
using System.Threading;

namespace LRWarehouse.DAL
{
    public class ElasticSearchManager : BaseDataManager
    {
        const string className = "ElasticSearchManager";
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        ResourceJSONManager jsonManager = new ResourceJSONManager();


        public ElasticSearchManager()
        {
        }

        public DataSet GetSqlDataForElasticSearch( int resourceIntId, ref string status )
        {
            return GetSqlDataForElasticSearch( resourceIntId.ToString(), ref status );
        }

        public DataSet GetSqlDataForElasticSearch( string resourceIntIdList, ref string status )
        {
            status = "successful";
            #region SqlParameters
            SqlParameter[] sqlParameter = new SqlParameter[ 1 ];
            sqlParameter[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntIdList );
            #endregion

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, "Resource_BuildElasticSearch", sqlParameter );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            catch ( Exception ex )
            {
                LogError( className + ".GetSqlDataForElasticSearch(): " + ex.ToString() );
                status = className + ".GetSqlDataForElasticSearch(): " + ex.Message;

                return null;
            }

        }

        #region Create or Replace whole Record
        public void CreateOrReplaceRecord( int resourceIntID )
        {
            ResourceJSONFlat[] resources = jsonManager.GetJSONFlatByIntID( resourceIntID );
            CreateOrReplaceRecord( resources );
        }
        public void CreateOrReplaceRecord( DataSet ds )
        {
            ResourceJSONFlat[] resources = jsonManager.GetJSONArrayFromDataSet( ds );
            CreateOrReplaceRecord( resources );
        }
        public void CreateOrReplaceRecord( ResourceJSONFlat[] flats )
        {
            foreach ( ResourceJSONFlat json in flats )
            {
                CreateOrReplaceRecord( json );
            }
        }
        public void CreateOrReplaceRecord( Resource entity )
        {
            ResourceJSONFlat flat = jsonManager.GetJSONFromResource( entity );
            ResourceJSONElasticSearch searchable = jsonManager.GetJSONElasticSearchFromJSONFlat( flat );
            CreateOrReplaceRecord( searchable );
        }
        public void CreateOrReplaceRecord( ResourceJSONFlat flat )
        {
            ResourceJSONElasticSearch searchable = jsonManager.GetJSONElasticSearchFromJSONFlat( flat );
            CreateOrReplaceRecord( searchable );
        }
        public void CreateOrReplaceRecord( ResourceJSONElasticSearch searchable )
        {
            //Publish it
            ContactServer( "PUT", serializer.Serialize( searchable ), "resource/" + searchable.versionID );
        }

        public void BulkUpload( DataSet resourceList )
        {
            List<ResourceJSONFlat> flats = new List<ResourceJSONFlat>();
            StringBuilder builder = new StringBuilder();
            string newline = Environment.NewLine;
            //string toWrite = "";
            int count = 0;
            foreach ( DataRow dr in resourceList.Tables[ 0 ].Rows )
            {
                if ( count <= 1000 )
                {
                    ResourceJSONFlat flat = jsonManager.GetJSONFlatFromDataRow( dr );
                    string header = "{ \"index\": { \"_index\": \"collection5\", \"_type\": \"resource\", \"_id\": \"" + flat.versionID + "\" } }";
                    string jsonData = serializer.Serialize( flat );
                    builder.Append( header + newline + jsonData + newline );
                    //toWrite = toWrite + header + Environment.NewLine + jsonData + Environment.NewLine;
                    count++;
                }
                if ( count == 1000 )
                {
                    int tries = 0;
                    while ( tries < 5 )
                    {
                        try
                        {
                            //ContactServer( "POST", toWrite, "_bulk" );
                            ContactServer( "POST", builder.ToString(), "_bulk" );
                            tries = 5;
                        }
                        catch
                        {
                            tries++;
                        }
                    }
                    //toWrite = "";
                    builder.Clear();
                    count = 0;
                    Thread.Sleep( 8000 );
                }
            }
            if ( count > 0 )
            {
                //ContactServer( "POST", toWrite, "_bulk" );
                ContactServer( "POST", builder.ToString(), "_bulk" );
            }

        }
        #endregion

        #region Replace Field/Item within a Record
        public void ReplaceField( int resourceVersionID, string fieldName, string item )
        {
            ReplaceItem( resourceVersionID, fieldName, item );
        }
        public void ReplaceField( int resourceVersionID, string fieldName, int item )
        {
            ReplaceItem( resourceVersionID, fieldName, item );
        }
        public void ReplaceField( int resourceVersionID, string fieldName, string[] item )
        {
            ReplaceItem( resourceVersionID, fieldName, item );
        }
        public void ReplaceField( int resourceVersionID, string fieldName, int[] item )
        {
            ReplaceItem( resourceVersionID, fieldName, item );
        }
        private void ReplaceItem( int resourceVersionID, string fieldName, object data )
        {
            dynamic query = new Dictionary<string, object>();
            dynamic doc = new Dictionary<string, object>();
            doc.Add( fieldName, data );
            query.Add( "doc", doc );
            ContactServer( "POST", serializer.Serialize( query ), "resource/" + resourceVersionID + "/_update" );
        }
        #endregion

        #region Delete Record
        public void DeleteByIntID( int resourceIntID, ref string response )
        {
            DeleteByIntID( resourceIntID.ToString(), ref response );
        }
        public void DeleteByIntID( string resourceIntID, ref string response )
        {
            //Setup the JSON
            dynamic deleter = new Dictionary<string, object>();
            dynamic term = new Dictionary<string, object>();
            term.Add( "intID", resourceIntID );
            deleter.Add( "term", term );

            //Do the delete
            ContactServer( "DELETE", serializer.Serialize( deleter ), "resource/_query" );
        }
        public void MassDeleteByIntID( string[] resourceIntIDs, ref string response )
        {
            //Must divide the set up into groups of 1000 due to ES limit
            int max = 1000;
            int count = 0;
            List<string> listIDs = new List<string>();
            for ( int i = 0; i < resourceIntIDs.Length; i++ )
            {
                listIDs.Add( resourceIntIDs[ count ] );
                if ( listIDs.Count >= max || i == resourceIntIDs.Length - 1 )
                {
                    //Setup the JSON
                    dynamic deleter = new Dictionary<string, object>();
                    dynamic terms = new Dictionary<string, object>();
                    terms.Add( "intID", listIDs.ToArray<string>() );
                    deleter.Add( "terms", terms );

                    //Do the delete
                    ContactServer( "DELETE", serializer.Serialize( deleter ), "resource/_query" );
                    deleter.Clear();
                    terms.Clear();
                    listIDs.Clear();

                    Thread.Sleep( 2000 ); //Give ES time to process/recover, ensure no timeouts/problems
                }
                count++;
            }

        }
        public void DeleteByVersionID( int resourceVersionID, ref string response )
        {
            DeleteByVersionID( resourceVersionID.ToString(), ref response );
        }
        public void DeleteByVersionID( string resourceVersionID, ref string response )
        {
            ContactServer( "DELETE", "", "resource/" + resourceVersionID );
        }
        public void DeleteEntireIndexContents()
        {
            ContactServer( "DELETE", "{ \"match_all\": {} }", "resource/_query" );
        }
        #endregion

        #region Append Field within a Record
        public void AppendField( int resourceVersionID, string fieldName, string item, bool checkExists )
        {
            AppendItem( resourceVersionID, fieldName, item, checkExists );
        }
        public void AppendField( int resourceVersionID, string fieldName, int item, bool checkExists )
        {
            AppendItem( resourceVersionID, fieldName, item, checkExists );
        }
        public void AppendField( int resourceVersionID, string fieldName, string[] item, bool checkExists )
        {
            AppendItem( resourceVersionID, fieldName, item, checkExists );
        }
        public void AppendField( int resourceVersionID, string fieldName, int[] item, bool checkExists )
        {
            AppendItem( resourceVersionID, fieldName, item, checkExists );
        }
        private void AppendItem( int resourceVersionID, string fieldName, object data, bool checkExists )
        {
            dynamic query = new Dictionary<string, object>();
            dynamic parameters = new Dictionary<string, object>();
            parameters.Add( "info", data );
            if ( checkExists )
            {
                query.Add( "script", "ctx._source." + fieldName + ".contains(info) ? (ctx.op = 'none') : (ctx._source." + fieldName + " += info)" );
            }
            else
            {
                query.Add( "script", "ctx._source." + fieldName + " += info;" );
            }
            query.Add( "params", parameters );
            ContactServer( "POST", serializer.Serialize( query ).Replace( @"\u0027", "'" ), "resource/" + resourceVersionID + "/_update" );
        }
        private void AppendFieldByIntID( string intID, string fieldName, object data, bool checkExists )
        {
            List<int> versionIDs = GetVersionIDFromIntID( int.Parse( intID ) );
            foreach ( int versionID in versionIDs )
            {
                AppendItem( versionID, fieldName, data, checkExists );
                Thread.Sleep( 1000 );
            }
        }
        private void TrimItem( int resourceVersionID, string fieldName, object data )
        {
            string removeString = "";
            if ( data is int )
            {
                removeString = ".remove((Object) " + data + ")";
            }
            else
            {
                removeString = ".remove('" + data + "')";
            }
            var outerQuery = new
            {
                query = new
                {
                    script = "ctx._source." + fieldName + removeString
                }
            };
            ContactServer( "POST", serializer.Serialize( outerQuery ), "resource/" + resourceVersionID + "/_update" );
        }
        private void TrimField( string intID, string fieldName, object data )
        {
            List<int> versionIDs = GetVersionIDFromIntID( int.Parse( intID ) );
            foreach ( int versionID in versionIDs )
            {
                TrimItem( versionID, fieldName, data );
                Thread.Sleep( 1000 );
            }
        }
        #endregion

        #region Paradata Helper Methods
        public void AddView( int resourceVersionID ) //Temporary, for compatability
        {
            AppendField( resourceVersionID, "viewsCount", 1, false );
        }
        public void AddResourceView( string intID )
        {
            AppendFieldByIntID( intID, "resourceViews", 1, false );
        }
        public void AddDetailView( string intID )
        {
            AppendFieldByIntID( intID, "detailViews", 1, false );
        }
        public void AddLike( string intID )
        {
            AddLikeDislike( intID, 1 );
        }
        public void AddDislike( string intID )
        {
            AddLikeDislike( intID, -1 );
        }
        public void AddComment( string intID )
        {
            AppendFieldByIntID( intID, "commentsCount", 1, false );
        }
        public void AddEvaluation( string intID )
        {
            AppendFieldByIntID( intID, "evaluationCount", 1, false );
        }
        public void RefreshLibraryCollectionTotals( int intID, List<int> libraryIDs, List<int> collectionIDs )
        {
          var versions = GetVersionIDFromIntID( intID );
          foreach(int i in versions){
            ReplaceField( i, "libraryIDs", libraryIDs.ToArray<int>() );
            ReplaceField( i, "collectionIDs", collectionIDs.ToArray<int>() );
            ReplaceField( i, "favorites", libraryIDs.Count );
            Thread.Sleep( 1000 );
          }
        }
        public void AddFavoriteLibraryCollection( string intID, int libraryID, int collectionID )
        {
            var outerQuery = new
            {
                query = new
                {
                    script = "ctx._source.favorites += 1; ctx._source.libraryIDs += " + libraryID + "; ctx._source.collectionIDs += " + collectionID + ";"
                }
            };

            List<int> versionIDs = GetVersionIDFromIntID( int.Parse( intID ) );
            foreach ( int versionID in versionIDs )
            {
                string debug = serializer.Serialize( outerQuery );
                ContactServer( "POST", serializer.Serialize( outerQuery ), "resource/" + versionID + "/_update" );
                Thread.Sleep( 1000 );
            }
        }
        public void RemoveFavoriteLibraryCollection( string intID, int libraryID, int collectionID )
        {
            var outerQuery = new
            {
                query = new
                {
                    script = "ctx._source.favorites += -1; ctx._source.libraryIDs.remove((Object) " + libraryID + "); ctx._source.collectionIDs.remove((Object) " + collectionID + ");"
                }
            };

            List<int> versionIDs = GetVersionIDFromIntID( int.Parse( intID ) );
            foreach ( int versionID in versionIDs )
            {
                string debug = serializer.Serialize( outerQuery );
                ContactServer( "POST", serializer.Serialize( outerQuery ), "resource/" + versionID + "/_update" );
                Thread.Sleep( 1000 );
            }
        }
        public void AddToCollection( string intID, int collectionID )
        {
            AppendFieldByIntID( intID, "collectionIDs", collectionID, true );
        }
        public void RemoveFromCollection( string intID, int collectionID )
        {
            TrimField( intID, "collectionIDs", collectionID );
        }
        public void AddToLibrary( string intID, int libraryID )
        {
            AppendFieldByIntID( intID, "libraryIDs", libraryID, true );
        }
        public void RemoveFromLibrary( string intID, int libraryID )
        {
            TrimField( intID, "libraryIDs", libraryID );
        }
        public void RemoveFavorite( string intID )
        {
            AppendFieldByIntID( intID, "favorites", -1, false );
        }
        public void SetEvaluationScoreTotal( int resourceIntID, decimal score )
        {
            List<int> resourceVersionIDs = GetVersionIDFromIntID( resourceIntID );
            foreach ( int versionID in resourceVersionIDs )
            {
                ReplaceItem( versionID, "evaluationScoreTotal", score );
                Thread.Sleep( 1000 );
            }
        }
        private void AddLikeDislike( string intID, int value )
        {
            AppendFieldByIntID( intID, "likesSummary", value, false );
            if ( value > 0 )
            {
                AppendFieldByIntID( intID, "likeCount", value, false );
            }
            else if ( value < 0 )
            {
                AppendFieldByIntID( intID, "dislikeCount", value * -1, false );
            }
        }
        public double GetResourceEvaluationScore( int intID )
        {
            double totalScore = 0.0;
            string status = "";
            double rubricScore = 0.0;
            double standardsScore = 0.0;
            int rubricCounter = 0;
            int standardsCounter = 0;
            double overallScore = 0.0;

            ResourceEvaluationManager rManager = new ResourceEvaluationManager();
            DataSet ds = rManager.Select( intID, 0, 0, 0, ref status );
            if ( DoesDataSetHaveRows( ds ) )
            {
                //Tally up the raw info
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    double value = 0.0;
                    if ( GetRowPossibleColumn( dr, "Value" ) == null || GetRowPossibleColumn( dr, "Value" ).ToLower() == "null" )
                    {
                        value = 0.0;
                    }
                    else
                    {
                        try
                        {
                            value = double.Parse( GetRowPossibleColumn( dr, "Value" ) );
                            if ( value > 3.0 )
                            {
                                value = 3.0;
                            }
                            //Compensate for the +1 that happens when a score is inserted, which compensates for the nullifying of scores of 0
                            value = value - 1;
                        }
                        catch ( Exception ex )
                        {
                            LoggingHelper.DoTrace( ex.ToString() + " Value: " + GetRowPossibleColumn( dr, "Value" ) + " EndValue" );
                        }
                    }
                    if ( GetRowPossibleColumn( dr, "RubricId" ) != "" )
                    {
                        rubricScore = rubricScore + value;
                        rubricCounter++;
                    }
                    if ( GetRowPossibleColumn( dr, "StandardId" ) != "" )
                    {
                        standardsScore = standardsScore + value;
                        standardsCounter++;
                    }
                }

                //Do the averaging
                if ( rubricCounter > 0 )
                {
                    rubricScore = rubricScore / rubricCounter;
                }
                if ( standardsCounter > 0 )
                {
                    standardsScore = standardsScore / standardsCounter;
                }

                //Calculate the final score
                overallScore = rubricScore + standardsScore / 2;

                return overallScore;
            }

            return totalScore;
        }
        public Dictionary<int, double> GetAllEvaluations()
        {
            Dictionary<int, double> items = new Dictionary<int, double>();
            DataSet ds = DatabaseManager.DoQuery( "SELECT DISTINCT [ResourceIntId] FROM [Resource.Evaluation]" );
            if ( DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    int resourceIntID = int.Parse( GetRowColumn( dr, "ResourceIntId" ) );
                    double totalScore = GetResourceEvaluationScore( resourceIntID );
                    items.Add( resourceIntID, totalScore );
                }
            }

            return items;
        }
        public void RebuildAllEvaluations()
        {
            Dictionary<int, double> items = GetAllEvaluations();
            foreach ( KeyValuePair<int, double> item in items )
            {
                SetEvaluationScoreTotal( item.Key, ( ( decimal )item.Value ) / 3 );
            }
        }
        #endregion

        public string DoElasticSearch( string jsonQuery )
        {
            try
            {
                return ContactServer( "POST", jsonQuery, "_search/" );
            }
            catch ( Exception ex )
            {
                return ex.ToString();
            }
        }

        public string GetByVersionID( int versionID )
        {
            return ContactServer( "GET", "", "resource/" + versionID.ToString() );
        }

        public string FindMoreLikeThis( int versionID, string[] parameters, int minFieldMatches, int size, int start )
        {
            string url = "";
            foreach ( string item in parameters )
            {
                url = "," + item;
            }
            url = url.Substring( 1 );
            string uberURL = "resource/" + versionID +
                "/_mlt?mlt_fields=" + url +
                "&min_doc_freq=1&min_term_freq=" + minFieldMatches +
                "&search_size=" + size +
                "&search_from=" + start;
            return ContactServer( "GET", "", uberURL );
        }

        protected string ContactServer( string method, string json, string urlAddendum )
        {
            int maxAttempts = 4;
            bool success = false;
            int attemptNbr = 0;
            while ( attemptNbr < maxAttempts && !success )
            {
                try
                {
                    string collection = ContentHelper.GetAppKeyValue( "elasticSearchCollection" );
                    string wsUrl = UtilityManager.GetAppKeyValue( "elasticSearchUrl", "http://172.22.115.34:9200/" ) + collection + urlAddendum;
                    //string wsUrl = "http://localhost:9200/" + collection + urlAddendum;
                    HttpWebRequest request = ( HttpWebRequest )WebRequest.Create( wsUrl );
                    request.Method = method;

                    if ( method == "POST" || method == "PUT" || json.Length > 0 )
                    {
                        request.ContentType = "application/json; charset=utf-8";
                        byte[] byteData = Encoding.UTF8.GetBytes( json );
                        request.ContentLength = byteData.Length;
                        Stream requestStream = request.GetRequestStream();
                        requestStream.Write( byteData, 0, byteData.Length );
                        request.Timeout = 15000;
                    }
                    HttpWebResponse response = ( HttpWebResponse )request.GetResponse();
                    success = true;
                    //Console.WriteLine("Server contact successful");

                    StreamReader reader = new StreamReader( response.GetResponseStream() );
                    return reader.ReadToEnd();
                }
                catch ( TimeoutException tex )
                {
                    LogToStaging( "Timeout Encountered at " + DateTime.Now + ": " + tex.ToString(), true );
                    return "Timeout Error: " + tex.ToString();
                }
                catch ( Exception ex )
                {
                    LoggingHelper.DoTrace( "ElasticSearch Exception encountered: " + ex.ToString() );
                    attemptNbr++;
                    return "General Error: " + ex.ToString();
                }
            }
            return null;
        }

        protected void LogToStaging( string text, bool trace )
        {
            if ( ContentHelper.GetAppKeyValue( "envType" ) != "dev" )
            {
                if ( trace )
                {
                    LoggingHelper.DoTrace( "ElasticSearch Text Logged: " + text );
                }
                File.AppendAllText( @"\\STAGE\savedDocs\" + "errors.txt", text + "\r\n\r\n" );
            }
        }

        protected List<int> GetVersionIDFromIntID( int intID )
        {
            //Need a proc for this
            DataSet ds = DatabaseManager.DoQuery( "SELECT [Id] FROM [Resource.Version] WHERE [ResourceIntId] = " + intID );
            List<int> values = new List<int>();
            if ( DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    values.Add( int.Parse( DatabaseManager.GetRowColumn( dr, "Id" ) ) );
                }
            }

            return values;
        }

    }
}
