using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Web.Script.Serialization;
using IOER.Library;
using LRWarehouse.DAL;
using System.Data.SqlClient;
using ILPathways.Utilities;

using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace IOER
{
    public partial class DataDump : System.Web.UI.Page
    {
        string[] toWrite = new string[ 8 ];
        int[] identifier = new int[ 8 ];
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( Request.QueryString[ "mode" ] == "rebuildIOER" & Request.QueryString[ "auth" ] == "admin" )
            {
                RebuildEntireIndex();
            }
            else if ( Request.QueryString[ "mode" ] == "buildIOERJSON" & Request.QueryString[ "auth" ] == "admin" )
            {
                MakeListAsynchronously();
            }
            else if ( Request.QueryString[ "mode" ] == "rebuildWorknet" & Request.QueryString[ "auth" ] == "admin" )
            {
                RebuildWorkNetIndex();
            }
            else if ( Request.QueryString[ "mode" ] == "injectEvaluations" & Request.QueryString[ "auth" ] == "admin" )
            {
                //new ElasticSearchManager().RebuildAllEvaluations();
            }
            else if ( Request.QueryString[ "mode" ] == "replaceResource" & Request.QueryString[ "auth" ] == "admin" )
            {
                //new ElasticSearchManager().CreateOrReplaceRecord( int.Parse( Request.QueryString[ "intID" ] ) );
                //new ElasticSearchManager().RefreshResource( int.Parse( Request.QueryString[ "intID" ] ) );
                new Isle.BizServices.ResourceV2Services().RefreshResource( int.Parse( Request.QueryString[ "intID" ] ) );
            }
        }

        protected void RebuildWorkNetIndex()
        {
            //DataSet ds = DatabaseManager.DoQuery( "SELECT * FROM [workNet2013].[dbo].[Resource] order by 2" );
            DataSet ds = DatabaseManager.DoQuery( "SELECT Id, ResourceUrl, Title, [Description], isnull(DisabilityCategory,'') As DisabilityCategory, isnull(ServiceCategory,'') As WorkSupportService, isnull(Region, '') As Region, isnull(EndUser, '') As EndUser, isnull(MediaType, '') As MediaType, isnull(Keywords, '') As Keywords, ApplicationPath, Created  FROM [workNet2013].[dbo].[Resource] order by 2" );
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string newLine = System.Environment.NewLine;
            StringBuilder builder = new StringBuilder();

            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    string header = serializer.Serialize(
                        new
                        {
                            index = new
                            {
                                _index = "worknet",
                                _type = "resource",
                                _id = Get( dr, "Id" )
                            }
                        }
                    );
                    string record = serializer.Serialize(
                        new
                        {
                            id = Get( dr, "Id" ),
                            title = Get( dr, "Title" ),
                            description = Get( dr, "Description" ),
                            url = Get( dr, "ResourceUrl" ),
                            section = Get( dr, "ApplicationPath" ),
                            keywords = CSLtoStringList(
                                Get( dr, "Keywords" ) + ", " +
                                Get( dr, "mediaType" ) + ", " +
                                Get( dr, "EndUser" ) + ", " +
                                Get( dr, "DisabilityCategory" ) + ", " +
                                Get( dr, "WorkSupportService" ) + ", " +
                                Get( dr, "Region" )
                            ),
                            mediaType = CSLtoStringList( Get( dr, "MediaType" ) ),
                            endUser = CSLtoStringList( Get( dr, "EndUser" ) ),
                            disability = CSLtoStringList( Get( dr, "DisabilityCategory" ) ),
                            workSupportService = CSLtoStringList( Get( dr, "WorkSupportService" ) ),
                            region = CSLtoStringList( Get( dr, "Region" ) ),
                            created = DateTime.Parse( Get( dr, "Created" ) ).ToString( "M/d/yyyy h:mm:ss" ),
                            timestamp = long.Parse( DateTime.Parse( Get( dr, "Created" ) ).ToString( "yyyyMMddHHmmss" ) ),
                            viewsCount = ( Get( dr, "ViewsCount" ) == "" ? 0 : int.Parse( Get( dr, "ViewsCount" ) ) ),
                            sortTitle = Get( dr, "Title" ).ToLower()
                        }
                    );
                    builder.Append( header + newLine + record + newLine );
                }
                File.WriteAllText( @"C:\elasticSearchJSON\workNetJSON.json", builder.ToString() );
            }
        }

        protected string[] CSLtoStringList( string input )
        {
            return input.Trim().Split( new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries );
        }

        protected string Get( DataRow dr, string column )
        {
            return DatabaseManager.GetRowPossibleColumn( dr, column );
        }

        protected void RebuildEntireIndex()
        {
            ElasticSearchManager eManager = new ElasticSearchManager();
            DataSet ds = DatabaseManager.DoQuery( "SELECT * FROM [Resource_Index]" );
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                //eManager.DeleteEntireIndexContents();
                //eManager.BulkUpload( ds );
                //eManager.RebuildAllEvaluations(); //Ideally this would be taken care of by the database in the initial call instead
                //output.Text = "Finished at " + DateTime.Now.ToShortTimeString();
                output.Text = "No action taken - this process must be handled using external executable.";
            }
            else
            {
                output.Text = "Error: No rows found in Table.";
            }
        }

        protected void MakeListAsynchronously()
        {
            //Get Data
            DataSet ds = DatabaseManager.DoQuery( "SELECT * FROM [Resource_Index]" );

            //Break data into 8 parts
            List<DataRow>[] listOfRows = new List<DataRow>[ 8 ];
            Lister[] listers = new Lister[ 8 ];
            Task[] tasks = new Task[ 8 ];
            int listLength = int.Parse( Math.Ceiling( ( Double )( ds.Tables[ 0 ].Rows.Count / 8 ) ).ToString() ) + 1;

            int totalTracker = 0;
            for ( int i = 0; i < listOfRows.Length; i++ )
            {
                listOfRows[ i ] = new List<DataRow>();
                listers[ i ] = new Lister();
                for ( int j = 0; j < listLength; j++ )
                {
                    if ( totalTracker < ds.Tables[ 0 ].Rows.Count )
                    {
                        listOfRows[ i ].Add( ds.Tables[ 0 ].Rows[ totalTracker ] );
                        totalTracker++;
                    }
                }
                DataRow[] items = listOfRows[ i ].ToArray<DataRow>();
                listers[ i ].rows = items;
                listers[ i ].tracker = i;
                listers[ i ].toWrite = "";
            }

            File.WriteAllText( @"C:\elasticSearchJSON\lastrun.txt", "Running Async process at " + DateTime.Now.ToShortTimeString() + System.Environment.NewLine );

            //multi-threading really seems to hate keeping its mitts on its own variables, soooo...
            tasks[ 0 ] = new Task( () => listers[ 0 ].ListMaker() );
            tasks[ 0 ].Start();
            tasks[ 1 ] = new Task( () => listers[ 1 ].ListMaker() );
            tasks[ 1 ].Start();
            tasks[ 2 ] = new Task( () => listers[ 2 ].ListMaker() );
            tasks[ 2 ].Start();
            tasks[ 3 ] = new Task( () => listers[ 3 ].ListMaker() );
            tasks[ 3 ].Start();
            tasks[ 4 ] = new Task( () => listers[ 4 ].ListMaker() );
            tasks[ 4 ].Start();
            tasks[ 5 ] = new Task( () => listers[ 5 ].ListMaker() );
            tasks[ 5 ].Start();
            tasks[ 6 ] = new Task( () => listers[ 6 ].ListMaker() );
            tasks[ 6 ].Start();
            tasks[ 7 ] = new Task( () => listers[ 7 ].ListMaker() );
            tasks[ 7 ].Start();
        }

        #region subclasses

        public class Lister
        {
            public DataRow[] rows;
            public int tracker;
            public string toWrite;

            public void ListMaker()
            {
                try
                {
                  int currentLineCount = 0;
                  JavaScriptSerializer serializer = new JavaScriptSerializer();
                  ResourceJSONManager manager = new ResourceJSONManager();






                  string collectionName = "collection5";// UtilityManager.GetAppKeyValue( "elasticSearchCollection", "collection5" );








                  //File.AppendAllText( @"C:\elasticSearchJSON\lastrun" + tracker + ".txt", "Beginning Task " + tracker + " at " + DateTime.Now.ToShortTimeString() + System.Environment.NewLine );
                  //File.AppendAllText( @"C:\elasticSearchJSON\lastrun" + tracker + ".txt", "Task " + tracker + " has " + rows.Count() + " items." + System.Environment.NewLine );
                  //File.AppendAllText( @"C:\elasticSearchJSON\numbers.txt", "Tracker " + tracker + ": " + currentLineCount + System.Environment.NewLine );
                  //File.AppendAllText( @"C:\elasticSearchJSON\debug" + tracker + ".txt", "Made it to Debug 0" + System.Environment.NewLine );
                  foreach ( DataRow dr in rows )
                  {
                    //File.AppendAllText( @"C:\elasticSearchJSON\debug" + tracker + ".txt", "Made it to Debug 1" + System.Environment.NewLine );
                    LRWarehouse.Business.ResourceJSONFlat flat = manager.GetJSONFlatFromDataRow( dr );
                    LRWarehouse.Business.ResourceJSONElasticSearch resource = manager.GetJSONElasticSearchFromJSONFlat( flat );
                    //File.AppendAllText( @"C:\elasticSearchJSON\debug" + tracker + ".txt", "Made it to Debug 2" + System.Environment.NewLine );
                    //string header = string.Format( "{ \"index\": { \"_index\": \"{0}\", \"_type\": \"resource\", \"_id\": \"" + resource.versionID.ToString() + "\" } }", collectionName );
                    string header = "{ \"index\": { \"_index\": \"" + collectionName + "\", \"_type\": \"resource\", \"_id\": \"" + resource.versionID + "\" } }";
                    //File.AppendAllText( @"C:\elasticSearchJSON\debug" + tracker + ".txt", "Made it to Debug 3" + System.Environment.NewLine );
                    string jsonData = serializer.Serialize( resource );
                    //File.AppendAllText( @"C:\elasticSearchJSON\debug" + tracker + ".txt", "Made it to Debug 4" + System.Environment.NewLine );
                    toWrite = toWrite + header + Environment.NewLine + jsonData + Environment.NewLine;
                    currentLineCount++;
                    //File.AppendAllText( @"C:\elasticSearchJSON\numbers.txt", "Tracker " + tracker + ": " + currentLineCount + System.Environment.NewLine );

                    if ( currentLineCount > 499 )
                    {
                      //File.AppendAllText( @"C:\elasticSearchJSON\lastrun.txt", "Updating File " + tracker + " at " + DateTime.Now.ToShortTimeString() + System.Environment.NewLine );
                      toWrite = toWrite.Replace( @"\n\t\t\t\t\t\t\t\t\t", " " ).Replace( @"          ", " " );
                      //File.AppendAllText( @"C:\inetpub\wwwroot\vos_2010\Illinois Pathways\IllinoisPathways\IllinoisPathways\testing\json\resourceJSON" + tracker + ".json", toWrite );
                      File.AppendAllText( @"C:\elasticSearchJSON\resourceJSON" + tracker + ".json", toWrite );
                      currentLineCount = 0;
                      toWrite = "";
                      //File.AppendAllText( @"C:\elasticSearchJSON\debug" + tracker + ".txt", "Made it to Debug 5" + System.Environment.NewLine );
                    }
                  }
                  //write any remaining data
                  //File.AppendAllText( @"C:\inetpub\wwwroot\vos_2010\Illinois Pathways\IllinoisPathways\IllinoisPathways\testing\json\resourceJSON" + tracker + ".json", toWrite );
                  File.AppendAllText( @"C:\elasticSearchJSON\resourceJSON" + tracker + ".json", toWrite );
                  //File.AppendAllText( @"C:\elasticSearchJSON\debug" + tracker + ".txt", "Made it to Debug 6" + System.Environment.NewLine );
                }
                catch ( Exception ex )
                {
                  File.AppendAllText( @"C:\elasticSearchJSON\lastrun" + tracker + ".txt", "Task " + tracker + " suffered fatal error: " + ex.Message.ToString() + System.Environment.NewLine );
                }
            }
        }
        #endregion

    }
}