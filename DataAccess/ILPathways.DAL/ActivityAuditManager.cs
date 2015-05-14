using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.ApplicationBlocks.Data;

using ILPathways.Business;
using Isle.DTO;

namespace ILPathways.DAL
{
    public class ActivityAuditManager : BaseDataManager
	{
		static string className = "ActivityAuditManager";
    
        #region constants for activity log - max 25 char

        /// <summary>
        /// General activity of audit
        /// </summary>
        public const string ACTIVITY_LOG_ACTIVITY_TYPE_AUDIT = "Audit";


        #endregion

        #region Activity Logging Routines
        /// <summary>
        /// LOG an activity to the activity log
        /// </summary>
        /// <param name="activityName"></param>
        /// <param name="eventName"></param>
        /// <param name="comment"></param>
        public static int LogActivity( string activityName, string eventName, string comment )
        {
            string activityType = ACTIVITY_LOG_ACTIVITY_TYPE_AUDIT;
            return LogActivity( activityType, activityName, eventName, comment, 0, 0, 0, 0, 0 );
        }//

        /// <summary>
        /// LOG an activity to the activity log.
        /// A common convention for the foreign keys includes:
        ///		1. user (keyId1) of organization (keyId2) added/updated something (keyId3) 
        ///		2. user (keyId1) of GROUP (keyId2) added/updated something (keyId3) 
        ///		3. {0} was enrolled in {1} by {2}
        /// </summary>
        /// <param name="activityType">Activity type to log</param>
        /// <param name="activityName">Category within activity</param>
        /// <param name="eventName">event within category</param>
        /// <param name="comment">general comment for activity - often used to expand data related to one or more of the keys</param>
        /// <param name="targetUserId">FK to vos_user</param>
        /// <param name="pActivityObjectId">FK to any table</param>
        /// <param name="actionByUserId">FK to to vos_user</param>
        /// <param name="objectRelatedId">FK to any table</param>
        /// <param name="pRelatedImageUrl"></param>
        /// <param name="pRelatedTargetUrl"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIPAddress"></param>
        /// <param name="targetObjectId">FK to a target object in the activity</param>
        public static int LogActivity( string activityType, string activityName, string eventName, string comment,
            int targetUserId, int pActivityObjectId, int actionByUserId, int objectRelatedId, int targetObjectId )
        {
            string pRelatedImageUrl = "";
            string pRelatedTargetUrl = "";
            string pSessionId = "";
            string pIPAddress = "";

            return LogActivity( activityType, activityName, eventName, comment,
                            targetUserId, pActivityObjectId, actionByUserId, objectRelatedId,
                            pRelatedImageUrl, pRelatedTargetUrl, pSessionId, pIPAddress, targetObjectId, "" );


        } //


        /// <summary>
        /// LOG an activity to the activity log.
        /// </summary>
        /// <param name="activityType"></param>
        /// <param name="activityName"></param>
        /// <param name="eventName"></param>
        /// <param name="comment"></param>
        /// <param name="targetUserId"></param>
        /// <param name="pActivityObjectId"></param>
        /// <param name="actionByUserId"></param>
        /// <param name="objectRelatedId">Optional, often the parent or related parent (like library for a resource)</param>
        /// <param name="pRelatedImageUrl"></param>
        /// <param name="pRelatedTargetUrl"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIPAddress"></param>
        /// <param name="targetObjectId">FK to a target object in the activity</param>
        /// <returns></returns>
        public static int LogActivity( string activityType, string activityName, string eventName, string comment,
                int targetUserId, int pActivityObjectId, int actionByUserId, int objectRelatedId, 
                string pRelatedImageUrl, string pRelatedTargetUrl
                ,string pSessionId, string pIPAddress
                , int targetObjectId
                , string referrer )
        {
            
            int newId = 0;

            if ( pSessionId == null || pSessionId.Length < 10 )
                pSessionId = GetCurrentSessionId();

            if ( pIPAddress == null || pIPAddress.Length < 10 )
                pIPAddress = GetUserIPAddress();


            if ( referrer == null || referrer.Length < 5 )
                referrer = GetUserReferrer();
            try
            {
                string connectionString = ContentConnection();

                //  ============================================================
                SqlParameter[] sqlParameters = new SqlParameter[ 14 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ActivityType", activityType );
                sqlParameters[ 1 ] = new SqlParameter( "@Activity", activityName );
                sqlParameters[ 2 ] = new SqlParameter( "@Event", eventName );
                sqlParameters[ 3 ] = new SqlParameter( "@Comment", comment );

                sqlParameters[ 4 ] = new SqlParameter( "@TargetUserId", SqlDbType.Int );
                sqlParameters[ 4 ].Value = targetUserId;

                sqlParameters[ 5 ] = new SqlParameter( "@ActivityObjectId", SqlDbType.Int );
                sqlParameters[ 5 ].Value = pActivityObjectId;

                sqlParameters[ 6 ] = new SqlParameter( "@ActionByUserId", SqlDbType.Int );
                sqlParameters[ 6 ].Value = actionByUserId;

                sqlParameters[ 7 ] = new SqlParameter( "@ObjectRelatedId", objectRelatedId );
                sqlParameters[ 8 ] = new SqlParameter( "@RelatedImageUrl", pRelatedImageUrl );
                sqlParameters[ 9 ] = new SqlParameter( "@RelatedTargetUrl", pRelatedTargetUrl );

                sqlParameters[ 10 ] = new SqlParameter( "@SessionId", pSessionId );
                sqlParameters[ 11 ] = new SqlParameter( "@IPAddress", pIPAddress );
                sqlParameters[ 12 ] = new SqlParameter( "@TargetObjectId", targetObjectId );
                sqlParameters[ 13 ] = new SqlParameter( "@Referrer", referrer );

                //  ==============================================================
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, "ActivityLogInsert", sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = int.Parse( dr[ 0 ].ToString() );
                }
                dr.Close();
                dr = null;

            }
            catch ( Exception e )
            {
                //log error and ignore
                LogError( e, className + ".LogActivity() Exception: \r\n " );
            }

            return newId;
        } //
        private static string GetCurrentSessionId()
        {
            string sessionId = "unknown";

            try
            {
                if ( HttpContext.Current.Session != null )
                {
                    sessionId = HttpContext.Current.Session.SessionID;
                }
            }
            catch
            {
            }
            return sessionId;
        }
        private static string GetUserReferrer()
        {
            string lRefererPage = "";
            try
            {
                if ( HttpContext.Current.Request.UrlReferrer != null )
                {
                    lRefererPage = HttpContext.Current.Request.UrlReferrer.ToString();
                    //check for link to us parm
                    //??

                    //handle refers from illinoisworknet.com 
                    if ( lRefererPage.ToLower().IndexOf( ".illinoisworknet.com" ) > -1 )
                    {
                        //may want to keep reference to determine source of this condition. 
                        //For ex. user may have let referring page get stale and so a new session was started when user returned! 

                    }
                }
            }
            catch ( Exception ex )
            {
                lRefererPage = ex.Message;
            }

            return lRefererPage;
        } //
        /// <summary>
        /// insert an activity log detail record ==> future, maybe
        /// </summary>
        /// <param name="pActivityLogId"></param>
        /// <param name="pDetailType"></param>
        /// <param name="pDetails"></param>
        /// <param name="pTargetUserId"></param>
        /// <param name="pCreatedById"></param>
        /// <returns></returns>
        private static int ActivityLogDetailInsert( int pActivityLogId, string pDetailType, int pTargetUserId, string pDetails, int pCreatedById )
        {
            int groupId = 0;
            int newId = 0;
            try
            {
                string connectionString = ContentConnection();

                //  ==============================================================================
                SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ActivityLogId", SqlDbType.Int );
                sqlParameters[ 0 ].Value = pActivityLogId;
                sqlParameters[ 1 ] = new SqlParameter( "@DetailType", pDetailType );
                sqlParameters[ 2 ] = new SqlParameter( "@TargetUserId", SqlDbType.Int );
                sqlParameters[ 2 ].Value = pTargetUserId;
                sqlParameters[ 3 ] = new SqlParameter( "@Details", pDetails );

                sqlParameters[ 4 ] = new SqlParameter( "@CreatedById", SqlDbType.Int );
                sqlParameters[ 4 ].Value = pCreatedById;


                //SqlHelper.ExecuteNonQuery( dbCon, CommandType.StoredProcedure, "ActivityLogInsert", sqlParameters );

                //  ==============================================================================
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, "ActivityLogDetailInsert", sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = int.Parse( dr[ 0 ].ToString() );
                }
                dr.Close();
                dr = null;

            }
            catch ( Exception e )
            {
                //log error and ignore
                LogError( e, className + ".ActivityLogDetailInsert() Exception: \r\n " );
            }

            return newId;
        } //
        #endregion

        #region Activity searching - perhaps by Union of identified events
        /// <summary>
        /// Retrieve recent activity
        /// ex User adds resource to a collection
        /// User - Actor
        /// Resource - object
        /// Collection - Target
        /// </summary>
        /// <param name="forDays"></param>
        /// <param name="pMaximumRows"></param>
        /// <returns></returns>
        public static List<ObjectActivity> ObjectActivity_RecentList( int forDays, int userId, int pMaximumRows )
        {
            List<ObjectActivity> list = new List<ObjectActivity>();
            ObjectActivity item = new ObjectActivity();
            
            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@HistoryDays", forDays );
            sqlParameters[ 1 ] = new SqlParameter( "@UserId", userId );

            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Activity.UnionAll]", sqlParameters );

                    if ( ds.HasErrors )
                    {
                        return null;
                    }
                    else
                    {
                        int cntr = 0;
                        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                        {
                            item = Fill( dr );
                            list.Add( item );
                            cntr++;
                            if ( cntr == pMaximumRows )
                                break;
                        }
                    }

                }
                catch ( Exception ex )
                {
                    LogError( ex, "ActivityAuditManager.ObjectActivity_RecentList() " );
                    return null;

                }
            }

            return list;


        }//

        public static List<ObjectActivity> ObjectActivity_MyFollowingSummary( int forDays, int userId, int pMaximumRows )
        {
            List<ObjectActivity> list = new List<ObjectActivity>();
            ObjectActivity item = new ObjectActivity();

            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@HistoryDays", forDays );
            sqlParameters[ 1 ] = new SqlParameter( "@UserId", userId );

            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Activity.MyFollowingSummary]", sqlParameters );

                    if ( ds.HasErrors )
                    {
                        return null;
                    }
                    else
                    {
                        int cntr = 0;
                        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                        {
                            item = Fill( dr );
                            list.Add( item );
                            cntr++;
                            if ( cntr == pMaximumRows )
                                break;
                        }
                    }

                }
                catch ( Exception ex )
                {
                    LogError( ex, "ActivityAuditManager.ObjectActivity_Search() " );
                    return null;

                }
            }

            return list;


        }//

        public static List<ObjectActivity> ObjectActivity_OrgSummary( int forDays, int orgId, int userId, int pMaximumRows )
        {
            List<ObjectActivity> list = new List<ObjectActivity>();
            ObjectActivity item = new ObjectActivity();

            SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
            sqlParameters[ 0 ] = new SqlParameter( "@HistoryDays", forDays );
            sqlParameters[ 1 ] = new SqlParameter( "@OrgId", orgId );
            sqlParameters[ 2 ] = new SqlParameter( "@UserId", userId );

            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Activity.OrgAll]", sqlParameters );

                    if ( ds.HasErrors )
                    {
                        return null;
                    }
                    else
                    {
                        int cntr = 0;
                        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                        {
                            item = Fill( dr );
                            list.Add( item );
                            cntr++;
                            if ( cntr == pMaximumRows )
                                break;
                        }
                    }

                }
                catch ( Exception ex )
                {
                    LogError( ex, "ActivityAuditManager.ObjectActivity_OrgSummary() " );
                    return null;

                }
            }

            return list;


        }//

        public static List<ObjectActivity> ObjectActivity_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            List<ObjectActivity> list = new List<ObjectActivity>();
            ObjectActivity item = new ObjectActivity();
            int outputCol = 4;
            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", pOrderBy );
            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", pStartPageIndex );
            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", pMaximumRows );

            sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ outputCol ].Direction = ParameterDirection.Output;

            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "ObjectActivity_Search", sqlParameters );
                    //get output paramter
                    string rows = sqlParameters[ outputCol ].Value.ToString();
                    try
                    {
                        pTotalRows = Int32.Parse( rows );
                    }
                    catch
                    {
                        pTotalRows = 0;
                    }


                    if ( ds.HasErrors )
                    {
                        return null;
                    }
                    else
                    {
                        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                        {
                            item = Fill( dr );
                            list.Add( item );
                        }
                    }

                }
                catch ( Exception ex )
                {
                    LogError( ex, "ActivityAuditManager.ObjectActivity_Search() " );
                    return null;

                }
            }

            return list;


        }//
        public static ObjectActivity Fill( DataRow dr )
        {
            ObjectActivity item = new ObjectActivity();

            item.IsValid = true;
            //no actual id yet, 
            item.Id = GetRowPossibleColumn( dr, "Id", 0 );

            item.ActorId = GetRowColumn( dr, "ActorId", 0 );
            item.Actor = GetRowPossibleColumn( dr, "Actor", "Missing" );
            item.ActorImageUrl = GetRowPossibleColumn( dr, "ActorImageUrl", "" );
            item.ActorUrl = GetRowPossibleColumn( dr, "ActorUrl", "" );
            item.IsMyAction = GetRowPossibleColumn( dr, "IsMyAction", false );

            item.Action = GetRowPossibleColumn( dr, "Action", "" );
            item.Activity = GetRowPossibleColumn( dr, "Activity", "" );

            //Object 
            item.ObjectId = GetRowColumn( dr, "ObjectId", 0 );
            item.ObjectTitle = GetRowColumn( dr, "ObjectTitle", "" );
            item.ObjectType = GetRowColumn( dr, "ObjectType", "" );
            item.ObjectText = GetRowColumn( dr, "ObjectText", "" );
            item.ObjectUrl = GetRowPossibleColumn( dr, "ObjectUrl", "" );
            item.ObjectImageUrl = GetRowPossibleColumn( dr, "ObjectImageUrl", "" );
            item.ObjectCount = GetRowColumn( dr, "ObjectCount", 0 );
            item.ObjectCount2 = GetRowColumn( dr, "ObjectCount2", 0 );
            item.HasObject = GetRowColumn( dr, "HasObject", false);

            item.TargetObjectId = GetRowColumn( dr, "TargetObjectId", 0 );
            item.TargetType = GetRowColumn( dr, "TargetType", "" );
            item.TargetTitle = GetRowPossibleColumn( dr, "TargetTitle", "" );
            item.TargetText = GetRowPossibleColumn( dr, "TargetText", "" );
            item.TargetUrl = GetRowPossibleColumn( dr, "TargetUrl", "" );
            item.TargetImageUrl = GetRowPossibleColumn( dr, "TargetImageUrl", "" );


            item.Created = GetRowColumn( dr, "Created", item.DefaultDate );
            item.ActivityDay = GetRowColumn( dr, "ActivityDay", item.DefaultDate );

            return item;
        }//

        #endregion 

        #region activity totals
        public static List<LibraryActivitySummary> ActivityLibraryTotals( int libraryId, DateTime startDate, DateTime endDate )
        {
            List<LibraryActivitySummary> list = new List<LibraryActivitySummary>();
            LibraryActivitySummary entity = new LibraryActivitySummary();
            CollectionActivitySummary col = new CollectionActivitySummary();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@LibraryId", libraryId );
                sqlParameters[ 1 ] = new SqlParameter( "@StartDate", startDate );
                sqlParameters[ 2 ] = new SqlParameter( "@EndDate", endDate );

                using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
                {
                    DataSet ds = new DataSet();
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Activity.LibraryTotals]", sqlParameters );

                    if ( ds.HasErrors )
                    {
                        return list;
                    }

                    if ( DoesDataSetHaveRows( ds ) )
                    {
                        
                        int prevLibId = 0;
                        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                        {
                            int libId = GetRowColumn( dr, "LibraryId", 0 );

                            if ( libId != prevLibId )
                            {
                                if (entity.LibraryId > 0)
                                    list.Add( entity );

                                entity = new LibraryActivitySummary();
                                entity.LibraryId = GetRowColumn( dr, "LibraryId", 0 );
                                entity.Library = GetRowColumn( dr, "Library", "missing" );
                                entity.LibraryViews = GetRowColumn( dr, "LibraryViews", 0 );
                                entity.ResourceViews = GetRowColumn( dr, "ResourceViews", 0 );

                                entity.Collections = new List<CollectionActivitySummary>();
                                prevLibId = libId;
                            }

                            col = new CollectionActivitySummary();
                            col.CollectionId = GetRowColumn( dr, "CollectionId", 0 );
                            col.Collection = GetRowColumn( dr, "Collection", "missing" );
                            col.CollectionViews = GetRowColumn( dr, "CollectionViews", 0 );
        
                            entity.Collections.Add( col );

                        }

                        //add the last one
                        if ( entity.LibraryId > 0 )
                            list.Add( entity );
                    }
                }

                
                return list;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ActivityLibraryTotals() " );
                return null;

            }

        }//

        /// <summary>
        /// get library and collection totals
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static List<HierarchyActivityRecord> ActivityTotals_Library( int libraryId, DateTime startDate, DateTime endDate )
        {
            return ActivityTotals_Library( libraryId, startDate, endDate, false);
        }//

        /// <summary>
        /// get library and collection totals
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// /// <param name="includePersonLibraries">Set to true to include personal libraries in the output. Note: if a libraryId is included, this will always be set to true.</param>
        /// <returns></returns>
        public static List<HierarchyActivityRecord> ActivityTotals_Library( int libraryId, DateTime startDate, DateTime endDate, bool includePersonLibraries )
        {
            List<HierarchyActivityRecord> list = new List<HierarchyActivityRecord>();
            HierarchyActivityRecord entity = new HierarchyActivityRecord();
            ActivityCount activityCount = new ActivityCount();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
                sqlParameters[ 0 ] = new SqlParameter( "@StartDate", startDate );
                sqlParameters[ 1 ] = new SqlParameter( "@EndDate", endDate );
                sqlParameters[ 2 ] = new SqlParameter( "@LibraryId", libraryId );
                sqlParameters[ 3 ] = new SqlParameter( "@IncludePersonLibraries", includePersonLibraries );

                using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
                {
                    DataSet ds = new DataSet();
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Activity.LibraryTotals]", sqlParameters );

                    if ( ds.HasErrors )
                    {
                        return list;
                    }

                    if ( DoesDataSetHaveRows( ds ) )
                    {

                        int prevLibId = 0;
                        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                        {
                            int libId = GetRowColumn( dr, "LibraryId", 0 );

                            if ( libId != prevLibId )
                            {
                                if ( entity.ChildrenActivity.Count > 0 )
                                    list.Add( entity );

                                entity = new HierarchyActivityRecord();
                                activityCount = new ActivityCount();
                                activityCount.Id = GetRowColumn( dr, "LibraryId", 0 );
                                activityCount.Title = GetRowColumn( dr, "Library", "missing" );

                                //activities = new List<ActivityCount>();
                                
                                int libraryViews = GetRowColumn( dr, "LibraryViews", 0 );
                                int resourceViews = GetRowColumn( dr, "ResourceViews", 0 );

                                prevLibId = libId;

                                var ids = new List<int>(){libraryViews};
                                var rids = new List<int>(){resourceViews};

                                activityCount.Activities.Add( "library_views", ids );
                                activityCount.Activities.Add( "resource_views", rids );

                                entity.Activity = activityCount;

                            }

                            activityCount = new ActivityCount();
                            activityCount.Id = GetRowColumn( dr, "CollectionId", 0 );
                            activityCount.Title = GetRowColumn( dr, "Collection", "missing" );
                            int views = GetRowColumn( dr, "CollectionViews", 0 );
                            var cids = new List<int>() { views };
                            activityCount.Activities.Add( "collection_views", cids );

                            entity.ChildrenActivity.Add( activityCount );

                        }

                        //add the last one
                        if ( entity.Activity != null && entity.Activity.Id > 0 )
                            list.Add( entity );
                    }
                }
                /*
                Dictionary<int, StudentName> students = new Dictionary<int, StudentName>()
                {
                    { 111, new StudentName {FirstName="Sachin", LastName="Karnik", ID=211}},
                    { 112, new StudentName {FirstName="Dina", LastName="Salimzianova", ID=317}},
                    { 113, new StudentName {FirstName="Andy", LastName="Ruth", ID=198}}
                };
                */
                return list;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ActivityTotals_Library() " );
                return null;
            }
        }//


        public static List<HierarchyActivityRecord> ActivityTotals_LearningLists( int objectId, DateTime startDate, DateTime endDate, bool removeEmptyNodes )
        {
            List<HierarchyActivityRecord> list = new List<HierarchyActivityRecord>();
            HierarchyActivityRecord entity = new HierarchyActivityRecord();
            ActivityCount activityCount = new ActivityCount();
            
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
                sqlParameters[ 0 ] = new SqlParameter( "@StartDate", startDate );
                sqlParameters[ 1 ] = new SqlParameter( "@EndDate", endDate );
                sqlParameters[ 2 ] = new SqlParameter( "@ObjectId", objectId );
                sqlParameters[ 3 ] = new SqlParameter( "@RemoveEmptyNodes", removeEmptyNodes );

                using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
                {
                    DataSet ds = new DataSet();
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Activity.LearningListsTotals]", sqlParameters );

                    if ( ds.HasErrors )
                    {
                        return list;
                    }

                    if ( DoesDataSetHaveRows( ds ) )
                    {

                        int prevObjId = 0;
                        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                        {
                            int objId = GetRowColumn( dr, "ObjectId", 0 );

                            if ( objId != prevObjId )
                            {
                                if ( entity.ChildrenActivity.Count > 0 )
                                    list.Add( entity );

                                entity = new HierarchyActivityRecord();
                                activityCount = new ActivityCount();
                                activityCount.Id = GetRowColumn( dr, "ObjectId", 0 );
                                activityCount.Title = GetRowColumn( dr, "Title", "missing" );

                                //activities = new List<ActivityCount>();

                                int objectViews = GetRowColumn( dr, "ObjectViews", 0 );
                                int totalViews = GetRowColumn( dr, "TotalViews", 0 );
                                int resourceViews = GetRowColumn( dr, "ResourceViews", 0 );
                                int parentDownloads = GetRowColumn( dr, "Downloads", 0 );
                                int totalDownloads = GetRowColumn( dr, "TotalDownloads", 0 );

                                prevObjId = objId;

                                var ids = new List<int>() { objectViews };
                                var tvids = new List<int>() { totalViews };
                                var rids = new List<int>() { resourceViews };
                                var pdids = new List<int>() { parentDownloads };
                                var tdids = new List<int>() { totalDownloads };

                                activityCount.Activities.Add( "object_views", ids );
                                activityCount.Activities.Add( "total_views", tvids );
                                activityCount.Activities.Add( "parent_downloads", pdids );
                                activityCount.Activities.Add( "total_downloads", tdids );
                                activityCount.Activities.Add( "resource_views", rids );

                                entity.Activity = activityCount;

                            }

                            activityCount = new ActivityCount();
                            activityCount.Id = GetRowColumn( dr, "ChildId", 0 );
                            activityCount.Title = GetRowColumn( dr, "ChildTitle", "missing" );
                            int views = GetRowColumn( dr, "ChildViews", 0 );
                            var cids = new List<int>() { views };

                            int downloads = GetRowColumn( dr, "ChildDownloads", 0 );
                            var cdids = new List<int>() { downloads };

                            activityCount.Activities.Add( "child_views", cids );
                            activityCount.Activities.Add( "child_downloads", cdids );

                            entity.ChildrenActivity.Add( activityCount );

                        }

                        //add the last one
                        if ( entity.Activity != null && entity.Activity.Id > 0 )
                            list.Add( entity );
                    }
                }
                /*
                Dictionary<int, StudentName> students = new Dictionary<int, StudentName>()
                {
                    { 111, new StudentName {FirstName="Sachin", LastName="Karnik", ID=211}},
                    { 112, new StudentName {FirstName="Dina", LastName="Salimzianova", ID=317}},
                    { 113, new StudentName {FirstName="Andy", LastName="Ruth", ID=198}}
                };
                */
                return list;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ActivityTotals_LearningLists() " );
                return null;
            }
        }//


        public static List<HierarchyActivityRecord> ActivityTotals_Accounts( DateTime startDate, DateTime endDate)
        {
            List<HierarchyActivityRecord> list = new List<HierarchyActivityRecord>();
            HierarchyActivityRecord entity = new HierarchyActivityRecord();
            ActivityCount activityCount = new ActivityCount();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@StartDate", startDate );
                sqlParameters[ 1 ] = new SqlParameter( "@EndDate", endDate );

                using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
                {
                    DataSet ds = new DataSet();
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Activity.AccountEventTotals]", sqlParameters );

                    if ( ds.HasErrors )
                    {
                        return list;
                    }

                    if ( DoesDataSetHaveRows( ds ) )
                    {
                        int cntr = 0;
                        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                        {
                            cntr++;
                            entity = new HierarchyActivityRecord();

                            activityCount = new ActivityCount();
                            activityCount.Id = cntr;
                            activityCount.Title = GetRowColumn( dr, "ActivityDay", "Huh" );

                            Activties_AddItem (activityCount,  dr, "Sessions", "sessions");
                            Activties_AddItem( activityCount, dr, "Auto_Login", "auto_login" );
                            Activties_AddItem( activityCount, dr, "Login", "login" );
                            Activties_AddItem( activityCount, dr, "Portal_SSO", "portal_sso" );
                            Activties_AddItem( activityCount, dr, "Portal_SSO_Registration", "portal_sso_registration" );
                            Activties_AddItem( activityCount, dr, "Registration", "registration" );
                            Activties_AddItem( activityCount, dr, "Account_Confirmation", "account_confirmation" );

                            entity.Activity = activityCount;
                            //entity.ChildrenActivity.Add( activityCount );

                            list.Add( entity );

                        }

                    }
                }
             
                return list;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ActivityTotals_Accounts() " );
                return null;
            }
        }//
        private static void Activties_AddItem(ActivityCount activityCount, DataRow dr, string title, string label)
        {
            
            int views = GetRowColumn( dr, title, 0 );
            var cids = new List<int>() { views };
            activityCount.Activities.Add( label, cids );
        }
        #endregion
    }
}
