using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
//using System.Threading; //Web Browser needs this
using System.Web.Script.Serialization;
using System.Web.Services;

using ILPathways.Controllers;
using ILPathways.Utilities;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using ResMgr = Isle.BizServices.ResourceBizService;


namespace ILPathways.Services
{
    /// <summary>
    /// Summary description for WebDALService
    /// </summary>
    [WebService( Namespace = "http://ilsle.com/" )]
    [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
    [System.ComponentModel.ToolboxItem( false )]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WebDALService : System.Web.Services.WebService
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        string className = "WebDALService";

        #region comments
        [WebMethod]
        public string PostComment( string resourceIntID, string commentText, string userID )
        {
            Comments comment = new Comments();
            try
            {
                comment.Post( resourceIntID, commentText, userID );
            }
            catch ( Exception ex )
            {
                return serializer.Serialize( ex.ToString() );
            }
            return serializer.Serialize( comment );
        }
        [WebMethod]
        public string GetComments( string resourceIntID )
        {
            Comments comments = new Comments();
            comments.Get( resourceIntID );
            return serializer.Serialize( comments );
        }
        #endregion

        #region likes/dislikes
        /// <summary>
        /// Like a Resource or a Collection -- Used by Activity1.ascx
        /// </summary>
        /// <param name="userGUID"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [WebMethod]
        public string Like( string userGUID, string type, int id )
        {
          var utilService = new UtilityService();
          
          //Make sure the user is logged in
          var user = utilService.GetUserFromGUID( userGUID );

          bool isValid = true;
          string status = "";
          var likes = LikeResourceOrCollection( user, type, id, ref isValid, ref status );

          return utilService.ImmediateReturn( likes, isValid, status, new { id = id, type = type } );
        }
        /// <summary>
        /// Like a Resource or a Collection -- Server-side version.  Returns the number of likes for the object on success. Could probably be optimized.
        /// </summary>
        /// <param name="user">The user doing the Liking</param>
        /// <param name="type">"resource" or "collection"</param>
        /// <param name="id">Resource ID of the Resource or ID of the collection</param>
        public int LikeResourceOrCollection( Patron user, string type, int id, ref bool isValid, ref string status )
        {
          var utilService = new UtilityService();

          //Ensure the user is logged in
          if ( !user.IsValid || user.Id == 0 )
          {
            isValid = false;
            status = "Please login to Like!";
            return 0;
          }

          //Ensure the user has not already liked this thing
          if ( type == "collection" )
          {
            var libService = new Isle.BizServices.LibraryBizService();
            var test = libService.LibrarySection_GetLike( id, user.Id );
            if ( test == null || test.Id == 0 || test.IsValid == false )
            {
              //User has not liked this yet, so do the like
              libService.LibrarySectionLikeCreate( id, true, user.Id );

              status = "okay";
              isValid = true;
              return libService.LibrarySection_LikeSummary( id ).First<Business.DataItem>().Int1;
            }
            else
            {
              status = "You already liked or disliked this!";
              isValid = false;
              return libService.LibrarySection_LikeSummary( id ).First<Business.DataItem>().Int1;
            }
          }
          else if ( type == "resource" )
          {
              //resourceIntId is being passed
            //var intID = utilService.GetIntIDFromVersionID( id );
            var intID = id;
            var test = new ResourceLikeSummaryManager().GetForDisplay( intID, user.Id, ref status );
            if ( !test.YouLikeThis && !test.YouDislikeThis )
            {
              //User has not liked this yet, so do the like
              var likeManager = new ResourceLikeManager();
              var like = new ResourceLike();

              like.IsLike = true;
              like.CreatedById = user.Id;
              like.ResourceIntId = intID;

              likeManager.Create( like, ref status );

              status = "okay";
              isValid = true;
              return test.LikeCount + 1;
            }
            else
            {
              status = "You already liked or disliked this!";
              isValid = false;
              return test.LikeCount;
            }
          }
          else
          {
            isValid = false;
            status = "Invalid type specified.";
            return 0;
          }
        }
        #endregion

        #region thumbnails
        #region thumbnail methods
        /*[WebMethod]
        public string GetSearchThumbnail( string url, string intID )
        {
            string reference = "";
            GenerateThumbnails( url, intID, ref reference );
            return GetThumbnail( url, intID, "thumb", "-thumb.png", 200, 1024, 768 );
        }
        [WebMethod]
        public string GetDetailThumbnail( string url, string intID )
        {
            string reference = "";
            GenerateThumbnails( url, intID, ref reference );
            return GetThumbnail( url, intID, "large", "-large.png", 400, 1024, 768 );
        }
        [WebMethod]
        public bool CreateThumbnails( string url, string intID ) // Plan C
        {
            string reference = "";
            GenerateThumbnails( url, intID, ref reference );
            return reference.ToLower().IndexOf( "error" ) > -1;
        }
        [WebMethod]
        public void CreateThumbnailsList( List<ThumbnailInput> items ) //Plan B
        {
            string status = "";
            foreach ( ThumbnailInput item in items )
            {
                GenerateThumbnails( item.url, item.intID, ref status );
            }
        }
        [WebMethod]
        public void CreateThumbnailsListThreaded( List<ThumbnailInput> items ) // Plan A
        {
            foreach ( ThumbnailInput item in items )
            {
                System.Threading.ThreadPool.QueueUserWorkItem( new System.Threading.WaitCallback( CreateThumbnailsThreaded ), item );
            }
        }
        protected void CreateThumbnailsThreaded( Object item )
        {
            ThumbnailInput input = ( ThumbnailInput )item;
            CreateThumbnails( input.url, input.intID );
        }
        public class ThumbnailInput
        {
            public string url { get; set; }
            public string intID { get; set; }
        }

        [WebMethod]
        public void CreateTodaysThumbnails()
        {
            CreateThumbnailsForLastNDays( 2 );
        }*/

        /*[WebMethod]
        public string CreateThumbnailsForLastNDays( int days )
        {
            DateTime sinceDate = DateTime.Now.Subtract( new TimeSpan( days, 0, 0, 0, 0 ) );

            DataSet ioerDS = LRWarehouse.DAL.DatabaseManager.DoQuery( "SELECT [ResourceUrl], Id FROM [Resource] WHERE [Created] > '" + sinceDate.ToShortDateString() + "' OR [LastUpdated] > '" + sinceDate.ToShortDateString() + "' ORDER BY [LastUpdated] desc" );

            DataSet worknetDS = LRWarehouse.DAL.DatabaseManager.DoQuery( "SELECT [ResourceUrl], 'worknet-' + CONVERT(varchar, [Id]) AS Id FROM [workNet2013].[dbo].[Resource] WHERE [Created] > '" + sinceDate.ToShortDateString() + "' ORDER BY [Created] desc" ); //Some lastupdated are null

            List<ThumbnailInput> ioerStuff = new List<ThumbnailInput>();
            List<ThumbnailInput> workNetStuff = new List<ThumbnailInput>();
            if ( DatabaseManager.DoesDataSetHaveRows( ioerDS ) )
            {
                foreach ( DataRow dr in ioerDS.Tables[ 0 ].Rows )
                {
                    ThumbnailInput item = new ThumbnailInput();
                    item.url = DatabaseManager.GetRowColumn( dr, "ResourceUrl" );
                    item.intID = DatabaseManager.GetRowColumn( dr, "Id" );
                    ioerStuff.Add( item );
                }
            }
            if ( DatabaseManager.DoesDataSetHaveRows( worknetDS ) )
            {
                foreach ( DataRow dr in worknetDS.Tables[ 0 ].Rows )
                {
                    ThumbnailInput item = new ThumbnailInput();
                    item.url = DatabaseManager.GetRowColumn( dr, "ResourceUrl" );
                    item.intID = DatabaseManager.GetRowColumn( dr, "Id" );
                    workNetStuff.Add( item );
                }
            }
            if ( ioerStuff.Count > 0 )
            {
                CreateThumbnailsList( ioerStuff );
            }
            if ( workNetStuff.Count > 0 )
            {
                CreateThumbnailsList( workNetStuff );
            }

            return "IOER Records: " + ioerStuff.Count + ", workNet Records: " + workNetStuff.Count;
        }*/
        /*protected string GetThumbnail( string url, string intID, string folder, string suffix, int width, int viewWidth, int viewHeight )
        {
            //Check main cache
            if ( IsSandbox() )
            {
                suffix = "-sandbox" + suffix;
            }
            string serverImageFilePath = ContentHelper.GetAppKeyValue( "serverImageFilePath", @"\\STAGE\OER Thumbnails\" );
            try
            {
                if ( IsLocalHost() )
                {
                    string tryURL = ContentHelper.GetAppKeyValue( "cachedImagesUrl", "//ioer.ilsharedlearning.org/OERThumbs/" );
                    string fullURL = tryURL + folder + "/" + intID + suffix;
                    HttpWebRequest request = ( HttpWebRequest )WebRequest.Create( fullURL );
                    HttpWebResponse response = ( HttpWebResponse )request.GetResponse();
                    if ( response.StatusCode.ToString() == "OK" || response.StatusCode.ToString() == "200" )
                    {
                        return ( fullURL );
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    if ( File.Exists( serverImageFilePath + folder + @"\" + intID + suffix ) ) //shouldn't need server.mappath
                    {
                        return ContentHelper.GetAppKeyValue( "cachedImagesUrl", "//ioer.ilsharedlearning.org/OERThumbs/" ) + folder + "/" + intID + suffix;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
            catch
            {
                //Get thumbnail from third party provider
                return StoreThumbFromURL2PNG( url, width, viewWidth, viewHeight, intID, folder + @"\", suffix );
            }
        }*/
        /*protected string StoreThumbFromURL2PNG( string url, int width, int viewWidth, int viewHeight, string intID, string folder, string suffix )
        {
            string imageURL = URL2PNG.GetThumbnail( url, width, viewWidth, viewHeight );

            //should have check that file ends in \
            string serverImageFilePath = ContentHelper.GetAppKeyValue( "serverImageFilePath", @"\\STAGE\OER Thumbnails\" );
            if ( serverImageFilePath.Substring( serverImageFilePath.Length - 1, 1 ) != "\\" )
            {
                serverImageFilePath += "\\";
            }
            HttpWebRequest request = ( HttpWebRequest )WebRequest.Create( imageURL );
            try
            {
                HttpWebResponse response = ( HttpWebResponse )request.GetResponse();
                if ( response.StatusCode.ToString() == "OK" || response.StatusCode.ToString() == "200" )
                {
                    byte[] bytes = null;
                    using ( Stream stream = response.GetResponseStream() )
                    using ( MemoryStream ms = new MemoryStream() )
                    {
                        int count = 0;
                        do
                        {
                            byte[] buffer = new byte[ 1024 ];
                            count = stream.Read( buffer, 0, 1024 );
                            ms.Write( buffer, 0, count );
                        } while ( stream.CanRead && count > 0 );
                        bytes = ms.ToArray();
                    }

                    //Have only production servers write to staging server's thumbnail store
                    if ( !IsLocalHost() )
                    {
                        string path = ( folder + intID + suffix );
                        File.WriteAllBytes( serverImageFilePath + path, bytes ); //shouldn't need server.mappath
                    }
                    return imageURL;
                }
                else
                {
                    LoggingHelper.DoTrace( "Storage Error: Response error: ID: " + intID + ", Status code: " + response.StatusCode.ToString() );
                    return "";
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.DoTrace( "Storage Error: ID: " + intID + ", Exception: " + ex.ToString() );
                return "";
            }
        }*/

        #region convenience methods
        public bool IsLocalHost()
        {
            string host = HttpContext.Current.Request.Url.Host.ToString();
            return ( host.Contains( "localhost" ) || host.Contains( "209.175.164.200" ) );
        }

        public bool IsSandbox()
        {
            return ContentHelper.GetAppKeyValue( "envType" ) == "sandbox";
        }

        public bool IsTestServer()
        {
            string host = HttpContext.Current.Request.Url.Host.ToString();
            return ( host.Contains( "testenv" ) );
        }

        public bool IsProduction()
        {
            if ( IsLocalHost() || IsSandbox() || IsTestServer() )
            {
              return false;
            }

            return true;
        }

        /*public void SaveThumbnails( string url, int intID )
        {
            SaveThumbnails( url, intID.ToString() );
        }
        public void SaveThumbnails( string url, string intID )
        {
            string thumb = "";
            string large = "";
            SaveThumbnails( url, intID, ref thumb, ref large );
        }
        public void SaveThumbnails( string url, string intID, ref string smallThumbnailURL, ref string largeThumbnailURL )
        {
            smallThumbnailURL = GetSearchThumbnail( url, intID );
            largeThumbnailURL = GetDetailThumbnail( url, intID );
        }
        public void SaveThumbnails( string url, int intID, ref string smallThumbnailURL, ref string largeThumbnailURL )
        {
            SaveThumbnails( url, intID.ToString(), ref smallThumbnailURL, ref largeThumbnailURL );
        }
        public void GenerateThumbnails( string url, string intID, ref string status )
        {
            try
            {
                int exitCode;
                ProcessStartInfo processInfo;
                Process process;
                string arguments;
                if ( IsSandbox() )
                {
                    arguments = "\"" + url + "\"" + " " + intID + "-sandbox";
                }
                else
                {
                    arguments = "\"" + url + "\"" + " " + intID;
                }

                try
                {
                    processInfo = new ProcessStartInfo( @"\\STAGE\OER Thumbnails\thumbnailer\programs\thumb.bat", arguments ); //Should work on production and staging servers
                }
                catch ( Exception ex )
                {
                    LoggingHelper.DoTrace( "Generation Error (Phase 1): ID: " + intID + ", Exception: " + ex.ToString() );
                    status = "Error: " + ex.ToString() + " (Are you running on production/test server or not?)";
                    return;
                }

                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;
                process = Process.Start( processInfo );
                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                exitCode = process.ExitCode;

                process.Close();

                if ( error != "" )
                {
                    status = error;
                }
                else
                {
                    status = output;
                }

                status = status + " Exit Code: " + exitCode;
                //LoggingHelper.DoTrace( "Generation Message (End Status): ID: " + intID + ", Status: " + status );
            }
            catch ( Exception ex )
            {
                LoggingHelper.DoTrace( "Generation Error (Phase 2): ID: " + intID + ", Exception: " + ex.ToString() );
                status = "Error: " + ex.ToString();
            }
        }*/
        #endregion
        #endregion
        #endregion

        #region standards ratings
        [WebMethod]
        public List<StandardsData> FetchStandardsData( RatingMessage input )
        {
            List<StandardsData> results = new List<StandardsData>();

            //get resource standards
            ResourceStandardCollection collection = new ResourceStandardManager().Select( input.intID );

            Patron user = new PatronManager().GetByRowId( input.userGUID );
            ResourceEvaluationManager reManager = new ResourceEvaluationManager();
            Dictionary<string, string> ratingWords = new Dictionary<string, string> { { "1", "Very Weak" }, { "2", "Limited" }, { "3", "Strong" }, { "4", "Superior" } };

            foreach ( ResourceStandard standard in collection )
            {
                string status = "";

                StandardsData data = new StandardsData();
                data.align = standard.AlignmentTypeValue;
                if ( data.align == "" ) { data.align = "Aligns to"; }
                data.code = standard.StandardNotationCode;
                data.link = standard.StandardUrl;
                data.id = standard.StandardId;
                data.text = standard.StandardDescription;
                data.rating = "Unrated";
                data.userRated = false;

                //OLD - get standard evaluations via ResourceEvaluationManager
                DataSet ds = reManager.Resource_StandardEvaluations_Select( input.intID, user.Id, standard.StandardId, ref status );
                if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        string rating = DatabaseManager.GetRowPossibleColumn( dr, "Value" );
                        if ( rating != "" )
                        {
                            data.rating = ratingWords[ rating ];
                        }

                        string userID = DatabaseManager.GetRowPossibleColumn( dr, "CreatedById" );
                        data.userRated = ( user.Id.ToString() == userID );
                    }
                }

                results.Add( data );
            }

            return results;
        }
        [WebMethod]
        public List<StandardsData> RateStandard( RatingMessage input )
        {
            ResourceEvaluationManager reManager = new ResourceEvaluationManager();
            ResourceEvaluation evaluation = new ResourceEvaluation();
            Patron user = new PatronManager().GetByRowId( input.userGUID );
            evaluation.CreatedById = user.Id;
            evaluation.ResourceIntId = input.intID;
            evaluation.StandardId = input.id;
            evaluation.Value = decimal.Parse( input.rating ) + 1;
            if ( evaluation.Value > 4 || evaluation.Value < 1 ) { return null; }
            evaluation.ScaleMin = 1;
            evaluation.ScaleMax = 4;
            string status = "";

            //TODO - does irating stay at 0-3??
            int rating = Int32.Parse( input.rating );
            int id = ResMgr.ResourceStandardEvaluation_Create( input.intID, input.id, user.Id, rating, ref status );
           // ResMgr.StandardEvaluation_Create( evaluation, ref status );

            return FetchStandardsData( input );
        }
        public class StandardsData
        {
            public string code;
            public string link;
            public string align;
            public int id;
            public string text;
            public string rating;
            public bool userRated;
        }
        public class RatingMessage
        {
            public int id;
            public string rating;
            public string userGUID;
            public int intID;
        }
        #endregion

        /*
        public void RunTest()
        {
            var t = new Thread(TestScreenshot);
            t.SetApartmentState( ApartmentState.STA );
            t.Start();
        }

        public void TestScreenshot()
        {
            TestScreenshot( "http://www.google.com/", 1024, 768 );
        }

        public void TestScreenshot( string url, int width, int height )/* Don't forget about the referenced System.Windows.Forms DLL and threading reference! */
        /*
        {

            var browser = new System.Windows.Forms.WebBrowser();
            browser.ScrollBarsEnabled = false;
            browser.ScriptErrorsSuppressed = true;
            browser.Navigate( url );

            while ( browser.ReadyState != System.Windows.Forms.WebBrowserReadyState.Complete ) { }

            browser.Width = width;
            browser.Height = height;

            var bmp = new System.Drawing.Bitmap( width, height );

            browser.DrawToBitmap( bmp, new System.Drawing.Rectangle( 0, 0, width, height ) );

            browser.Dispose();

            bmp.Save( @"C:\elasticSearchJSON\test.png", System.Drawing.Imaging.ImageFormat.Png );

        }
        */

    }

    //Pretty much everything uses the following
    public class GenericObject
    {
        protected DataSet ds;
        protected List<string> listTexts = new List<string>();
        protected List<string> listValues = new List<string>();
        protected List<string> listDescriptions = new List<string>();
        public string[] texts;
        public string[] values;
        public string[] descriptions;

        public string GetField( DataRow dr, string columnName )
        {
            return DatabaseManager.GetRowPossibleColumn( dr, columnName );
        }

        public bool HasRows( DataSet ds )
        {
            return DatabaseManager.DoesDataSetHaveRows( ds );
        }
    }

    public class Comments : GenericObject
    {
        protected List<string> listDates = new List<string>();
        public string[] dates;
        public string returnMessage;

        public void Get( string resourceIntIDRaw )
        {
            int resourceID = int.Parse( resourceIntIDRaw );
            ResourceCommentManager manager = new ResourceCommentManager();
            try
            {
                ds = manager.Select( resourceID );
                if ( !HasRows( ds ) )
                {
                    //LoggingHelper.DoTrace( 8, "No comments for: " + resourceIntIDRaw );
                    return;
                }
                //LoggingHelper.DoTrace( 8, "***FOUND comments for: " + resourceIntIDRaw );
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    listValues.Add( GetField( dr, "Id" ) );
                    listTexts.Add( GetField( dr, "CreatedBy" ) );
                    listDescriptions.Add( GetField( dr, "Comment" ) );
                    listDates.Add( DateTime.Parse( GetField( dr, "Created" ) ).ToShortDateString() );
                }

                texts = listTexts.ToArray<string>();
                values = listValues.ToArray<string>();
                descriptions = listDescriptions.ToArray<string>();
                dates = listDates.ToArray<string>();
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "Comments ws, exception" );
            }
        }

        public void Post( string resourceID, string commentText, string userID )
        {
            int resourceIntID;
            int userIntID;
            string userFullName;

            //TODO: make procs for these
            PatronManager patronManager = new PatronManager();
            Patron user = patronManager.GetByRowId( userID );

            if ( user.IsValid )
            {
                resourceIntID = int.Parse( resourceID );

                userIntID = user.Id;
                userFullName = user.FullName();
            }
            else
            {
                return;
            }

            //do bad word check
            if ( BadWordChecker.CheckForBadWords( commentText ) )
            {
                returnMessage = returnMessage + "<span class=\"commentStatus fail\"> Inappropriate content found. Please fix the inappropriate content. </span>";
            }
            else if ( commentText.Trim().Length < 15 )
            {
                returnMessage = returnMessage + "<span class=\"commentStatus fail\"> Please provide a comment of a meaningful length. </span>";
            }
            else
            {
                string statusMessage = "";
                ResourceCommentManager mgr = new ResourceCommentManager();
                //TODO - we may initially delay post of comments, pending approval
                ResourceComment entity = new ResourceComment();
                //entity.ResourceId = new Guid( resourceID );
                entity.ResourceIntId = resourceIntID;
                entity.Comment = commentText;
                entity.CreatedBy = userFullName;
                //TODO - add commenter context - from Patron table
                //entity.Commenter = user.TBD;
                //entity.IsActive = true;
                entity.CreatedById = userIntID;

                int id = mgr.Create( entity, ref statusMessage );
                if ( id > 0 )
                {
                    returnMessage = "<span class=\"commentStatus success\">Comment was posted.</span>";

                }
                else
                {
                    returnMessage = "<span class=\"commentStatus fail\">Error encountered saving comment.</span>";
                }
            }
        }
    }
}
