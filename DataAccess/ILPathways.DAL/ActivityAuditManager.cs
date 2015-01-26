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
            return LogActivity( activityType, activityName, eventName, comment, 0, 0, 0, 0 );
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
        /// <param name="keyId2">FK to any table</param>
        /// <param name="actionByUserId">FK to to vos_user</param>
        /// <param name="keyId4">FK to any table</param>
        public static int LogActivity( string activityType, string activityName, string eventName, string comment,
            int targetUserId, int keyId2, int actionByUserId, int keyId4 )
        {
            int orgId = 0;
            int projectId = 0;
            int groupId = 0;
            return LogActivity( activityType, activityName, eventName, comment,
                            targetUserId, keyId2, actionByUserId, keyId4, orgId, projectId );


        } //



        public static int LogActivity( string activityType, string activityName, string eventName, string comment,
            int targetUserId, int keyId2, int actionByUserId, int keyId4, int orgId, int projectId )
        {
            int groupId = 0;
            int newId = 0;
            try
            {
                string connectionString = ContentConnection();

                //  ==============================================================================
                SqlParameter[] sqlParameters = new SqlParameter[ 8 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ActivityType", activityType );
                sqlParameters[ 1 ] = new SqlParameter( "@Activity", activityName );
                sqlParameters[ 2 ] = new SqlParameter( "@Event", eventName );
                sqlParameters[ 3 ] = new SqlParameter( "@Comment", comment );

                sqlParameters[ 4 ] = new SqlParameter( "@TargetUserId", SqlDbType.Int );
                sqlParameters[ 4 ].Value = targetUserId;

                sqlParameters[ 5 ] = new SqlParameter( "@ActivityObjectId", SqlDbType.Int );
                sqlParameters[ 5 ].Value = keyId2;

                sqlParameters[ 6 ] = new SqlParameter( "@ActionByUserId", SqlDbType.Int );
                sqlParameters[ 6 ].Value = actionByUserId;

                sqlParameters[ 7 ] = new SqlParameter( "@Integer2", keyId4 );
                //future (near) use 
                //sqlParameters[ 8 ] = new SqlParameter( "@OrgId", orgId );
                //sqlParameters[ 9 ] = new SqlParameter( "@GroupId", groupId );


                //  ==============================================================================
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
    }
}
