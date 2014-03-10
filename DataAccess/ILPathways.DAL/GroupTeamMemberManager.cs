using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class GroupTeamMemberManager : BaseDataManager
    {
        static string className = "GroupTeamMemberManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[AppGroup.MemberGet]";
        const string SELECT_PROC = "[AppGroup.MemberSelect]";
        const string SEARCH_PROC = "[AppGroup.MemberSearch]";
        const string DELETE_PROC = "[AppGroup.MemberDelete]";
        const string INSERT_PROC = "[AppGroup.MemberInsert]";
        const string UPDATE_PROC = "[AppGroup.MemberUpdate]";


        /// <summary>
        /// Default constructor
        /// </summary>
        public GroupTeamMemberManager()
        { }//

        #region ====== Core Methods ===============================================
        /// <summary>
        /// Delete an GroupTeamMember record
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static bool Delete( int pId, ref string statusMessage )
        {
            string connectionString = GatewayConnection();
            bool successful = false;

            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
            sqlParameters[ 0 ].Value = pId;

            try
            {
                SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
                successful = true;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Delete() exception " );
                statusMessage = className + "- Unsuccessful: Delete(): " + ex.Message.ToString();

                successful = false;
            }
            return successful;
        }//

        /// <summary>
        /// Add an GroupTeamMember record
        /// </summary>
        /// <param name="entity">GroupTeamMember</param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static int Insert( GroupTeamMember entity, ref string statusMessage )
        {
            string connectionString = GatewayConnection();
            int newId = 0;

            #region parameters
            SqlParameter[] sqlParameters = new SqlParameter[ 13 ];
            sqlParameters[ 0 ] = new SqlParameter( "@GroupId", SqlDbType.Int );
            sqlParameters[ 0 ].Value = entity.GroupId;

            sqlParameters[ 1 ] = new SqlParameter( "@UserId", entity.UserId);

            sqlParameters[ 2 ] = new SqlParameter( "@CreatePrivilege", SqlDbType.Int );
            sqlParameters[ 2 ].Value = entity.GroupPrivilege.CreatePrivilege;

            sqlParameters[ 3 ] = new SqlParameter( "@ReadPrivilege", SqlDbType.Int );
            sqlParameters[ 3 ].Value = entity.GroupPrivilege.ReadPrivilege;

            sqlParameters[ 4 ] = new SqlParameter( "@WritePrivilege", SqlDbType.Int );
            sqlParameters[ 4 ].Value = entity.GroupPrivilege.WritePrivilege;

            sqlParameters[ 5 ] = new SqlParameter( "@DeletePrivilege", SqlDbType.Int );
            sqlParameters[ 5 ].Value = entity.GroupPrivilege.DeletePrivilege;

            sqlParameters[ 6 ] = new SqlParameter( "@AppendPrivilege", SqlDbType.Int );
            sqlParameters[ 6 ].Value = entity.GroupPrivilege.AppendPrivilege;

            sqlParameters[ 7 ] = new SqlParameter( "@AppendToPrivilege", SqlDbType.Int );
            sqlParameters[ 7 ].Value = entity.GroupPrivilege.AppendToPrivilege;

            sqlParameters[ 8 ] = new SqlParameter( "@AssignPrivilege", SqlDbType.Int );
            sqlParameters[ 8 ].Value = entity.GroupPrivilege.AssignPrivilege;

            sqlParameters[ 9 ] = new SqlParameter( "@ApprovePrivilege", SqlDbType.Int );
            sqlParameters[ 9 ].Value = entity.GroupPrivilege.ApprovePrivilege;

            sqlParameters[ 10 ] = new SqlParameter( "@SharePrivilege", SqlDbType.Int );
            sqlParameters[ 10 ].Value = entity.GroupPrivilege.SharePrivilege;

            sqlParameters[ 11 ] = new SqlParameter( "@CreatedbyId", SqlDbType.Int );
            sqlParameters[ 11 ].Value = entity.CreatedById;
            sqlParameters[ 12 ] = new SqlParameter( "@RowId", entity.RowId.ToString() );
            #endregion

            try
            {
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();

                    newId = int.Parse( dr[ 0 ].ToString() );
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Insert() " );
                statusMessage = className + "- Unsuccessful: Insert(): " + ex.Message.ToString();
                entity.Message = statusMessage;
                entity.IsValid = false;
            }

            return newId;
        }

        /// <summary>
        /// /// Update an GroupTeamMember record
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string Update( GroupTeamMember entity )
        {
            string message = "successful";

            #region parameters
            SqlParameter[] sqlParameters = new SqlParameter[ 11 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Id", SqlDbType.Int );
            sqlParameters[ 0 ].Value = entity.Id;

            sqlParameters[ 1 ] = new SqlParameter( "@CreatePrivilege", SqlDbType.Int );
            sqlParameters[ 1 ].Value = entity.GroupPrivilege.CreatePrivilege;

            sqlParameters[ 2 ] = new SqlParameter( "@ReadPrivilege", SqlDbType.Int );
            sqlParameters[ 2 ].Value = entity.GroupPrivilege.ReadPrivilege;

            sqlParameters[ 3 ] = new SqlParameter( "@WritePrivilege", SqlDbType.Int );
            sqlParameters[ 3 ].Value = entity.GroupPrivilege.WritePrivilege;

            sqlParameters[ 4 ] = new SqlParameter( "@DeletePrivilege", SqlDbType.Int );
            sqlParameters[ 4 ].Value = entity.GroupPrivilege.DeletePrivilege;

            sqlParameters[ 5 ] = new SqlParameter( "@AppendPrivilege", SqlDbType.Int );
            sqlParameters[ 5 ].Value = entity.GroupPrivilege.AppendPrivilege;

            sqlParameters[ 6 ] = new SqlParameter( "@AppendToPrivilege", SqlDbType.Int );
            sqlParameters[ 6 ].Value = entity.GroupPrivilege.AppendToPrivilege;

            sqlParameters[ 7 ] = new SqlParameter( "@AssignPrivilege", SqlDbType.Int );
            sqlParameters[ 7 ].Value = entity.GroupPrivilege.AssignPrivilege;

            sqlParameters[ 8 ] = new SqlParameter( "@ApprovePrivilege", SqlDbType.Int );
            sqlParameters[ 8 ].Value = entity.GroupPrivilege.ApprovePrivilege;

            sqlParameters[ 9 ] = new SqlParameter( "@SharePrivilege", SqlDbType.Int );
            sqlParameters[ 9 ].Value = entity.GroupPrivilege.SharePrivilege;

            sqlParameters[ 10 ] = new SqlParameter( "@LastUpdatedById", SqlDbType.Int );
            sqlParameters[ 10 ].Value = entity.LastUpdatedById;
            #endregion

            try
            {
                SqlHelper.ExecuteNonQuery( GatewayConnection(), UPDATE_PROC, sqlParameters );
                message = "successful";

            }
            catch ( Exception ex )
            {
                LogError( className + ".Update(): " + ex.ToString() );
                message = className + "- Unsuccessful: Update(): " + ex.Message.ToString();
                entity.Message = message;
                entity.IsValid = false;
            }

            return message;

        }//
        #endregion

        #region ====== Retrieval Methods ===============================================
        /// <summary>
        /// Get GroupTeamMember record by userId and group code
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="groupCode"></param>
        /// <returns></returns>
        public static GroupTeamMember GetByGroupCode( int userId, string groupCode )
        {
            int pId = 0;
            int groupId = 0;

            return Get( pId, userId, groupId, groupCode );

        }//
        /// <summary>
        /// Get GroupTeamMember record bu Id
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public static GroupTeamMember Get( int pId )
        {
            int pGroupId = 0;
            int pUserId = 0;
            string groupCode = "";

            return Get( pId, pUserId, pGroupId, groupCode );

        }//

        /// <summary>
        /// Get GroupTeamMember record by Id or contactId and (group Id or group code)
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="contactId"></param>
        /// <param name="groupId"></param>
        /// <param name="groupCode"></param>
        /// <returns></returns>
        private static GroupTeamMember Get( int pId, int userId, int groupId, string groupCode )
        {
            string connectionString = GatewayConnectionRO();
            GroupTeamMember entity = new GroupTeamMember();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );
                sqlParameters[ 1 ] = new SqlParameter( "@UserId", userId );
                sqlParameters[ 2 ] = new SqlParameter( "@GroupId", groupId );
                sqlParameters[ 3 ] = new SqlParameter( "@GroupCode", groupCode );

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, GET_PROC, sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.
                    while ( dr.Read() )
                    {
                        entity = Fill( dr );
                    }

                }
                else
                {
                    entity.Message = "Record not found";
                    entity.IsValid = false;
                }
                dr.Close();
                dr = null;
                return entity;

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Get() " );
                entity.Message = className + "- Unsuccessful: Get(): " + ex.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//

        /// <summary>
        /// Return true if user is a team member of the target group based on group code
        /// </summary>
        /// <param name="pGroupCode"></param>
        /// <param name="pUserId"></param>
        /// <returns></returns>
        public static bool IsATeamMember( string pGroupCode, int pUserId )
        {
            bool isMember = false;
            int pGroupId = 0;
            try
            {
                return IsATeamMember( pGroupId, pUserId, pGroupCode );

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".IsTeamMember() exception for group code: {0} and user:{1}", pGroupCode, pUserId ) );
                isMember = false;
            }

            return isMember;
        }//

        /// <summary>
        /// Check if user is team member of passed group Id
        /// </summary>
        /// <param name="pGroupId"></param>
        /// <param name="pUserId"></param>
        /// <returns></returns>
        public static bool IsATeamMember( int pGroupId, int pUserId )
        {
            return IsATeamMember( pGroupId, pUserId, "" );
        }//

        /// <summary>
        /// Check if user is team member of passed group
        /// </summary>
        /// <param name="pGroupId"></param>
        /// <param name="pUserId"></param>
        /// <returns></returns>
        public static bool IsATeamMember( int pGroupId, int pUserId, string pGroupCode )
        {
            bool isMember = false;

            try
            {
                DataSet ds = Select( pGroupId, pUserId, "", pGroupCode );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    //just one row
                    isMember = true;
                }
                else
                {
                    //Hmmm should we arbitrarily check whether user is owner or leave up to the caller to decide?
                    //probably better if we don't ASSUME here
                }
            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".IsTeamMember() exception for group: {0} and user:{1}", pGroupId, pUserId ) );
                isMember = false;
            }

            return isMember;
        }//


        /// <summary>
        /// Retrieve all members of the passed group code
        /// </summary>
        /// <param name="pGroupCode"></param>
        /// <returns></returns>
        public static DataSet Select( string pGroupCode )
        {
            int pUserId = 0;
            int pGroupId = 0;
            string pGroupType = "";

            return Select( pGroupId, pUserId, pGroupType, pGroupCode );
        }//
        /// <summary>
        /// Retrieve all members of the passed group Id
        /// </summary>
        /// <param name="pGroupId"></param>
        /// <returns></returns>
        public static DataSet SelectByGroupId( int pGroupId )
        {
            int pUserId = 0;
            string pGroupCode = "";
            string pGroupType = "";

            return Select( pGroupId, pUserId, pGroupType, pGroupCode );
        }//
        public static DataSet Select( int pUserId, string pGroupType )
        {

            int pGroupId = 0;
            string pGroupCode = "";

            return Select( pGroupId, pUserId, pGroupType, pGroupCode );
        }//

        public static DataSet Select( int pGroupId, int pUserId, string pGroupType )
        {
            string pGroupCode = "";

            return Select( pGroupId, pUserId, pGroupType, pGroupCode );

        }//

        /// <summary>
        /// Select GroupTeamMember related data using passed parameters
        /// </summary>
        /// <param name="pGroupId"></param>
        /// <param name="pUserId"></param>
        /// <param name="pGroupType"></param>
        /// <returns></returns>
        public static DataSet Select( int pGroupId, int pUserId, string pGroupType, string pGroupCode )
        {
            int pStartPageIndex = 1;
            int pMaximumRows = 1000;
            int pTotalRows = 0;
            string booleanOperator = "AND";
            string filter = "";
            //construct filter
            if ( pGroupId > 0 )
                filter = FormatSearchItem( filter, "GroupId", pGroupId.ToString(), booleanOperator );
            if ( pUserId > 0 )
                filter = FormatSearchItem( filter, "ContactId", pUserId.ToString(), booleanOperator );
            if ( pGroupCode.Trim().Length > 0 )
                filter = FormatSearchItem( filter, "grp.GroupCode", pGroupCode, booleanOperator );
            if ( pGroupType.Length > 0 )
                filter = FormatSearchItem( filter, "grp.GroupType", pGroupCode, booleanOperator );

            return Search( filter, "", pStartPageIndex, pMaximumRows, ref pTotalRows );
        }
        /// <summary>
        /// Group Team Members search
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static DataSet Search( string pFilter, string pSortOrder, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            string connectionString = GatewayConnectionRO();

            if ( pFilter.Length > 0 )
                pFilter = " Where " + pFilter;

            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", pSortOrder );
            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", pStartPageIndex );

            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", pMaximumRows );

            sqlParameters[ 4 ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ 4 ].Direction = ParameterDirection.Output;

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, SEARCH_PROC, sqlParameters );

                string rows = sqlParameters[ 3 ].Value.ToString();
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
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SelectCustomers() " );
                return null;

            }
        }

        #endregion

        #region ====== Helper Methods ===============================================
        /// <summary>
        /// Fill an GroupTeamMember object from a data reader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>GroupTeamMember</returns>
        public static GroupTeamMember Fill( SqlDataReader dr )
        {
            GroupTeamMember entity = new GroupTeamMember();

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.GroupId = GetRowColumn( dr, "GroupId", 0 );
            entity.UserId = GetRowColumn( dr, "UserId", 0 );

            entity.GroupPrivilege.CreatePrivilege = GetRowColumn( dr, "createPrivilege", 0 );
            entity.GroupPrivilege.ReadPrivilege = GetRowColumn( dr, "readPrivilege", 0 );
            entity.GroupPrivilege.WritePrivilege = GetRowColumn( dr, "writePrivilege", 0 );
            entity.GroupPrivilege.DeletePrivilege = GetRowColumn( dr, "deletePrivilege", 0 );
            entity.GroupPrivilege.AppendPrivilege = GetRowColumn( dr, "appendPrivilege", 0 );
            entity.GroupPrivilege.AppendToPrivilege = GetRowColumn( dr, "appendToPrivilege", 0 );
            entity.GroupPrivilege.AssignPrivilege = GetRowColumn( dr, "assignPrivilege", 0 );
            entity.GroupPrivilege.ApprovePrivilege = GetRowColumn( dr, "approvePrivilege", 0 );
            entity.GroupPrivilege.SharePrivilege = GetRowColumn( dr, "sharePrivilege", 0 );

            entity.Created = GetRowColumn( dr, "created", System.DateTime.Now );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.CreatedBy = GetRowColumn( dr, "CreatedBy", "" );

            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            entity.LastUpdatedBy = GetRowColumn( dr, "LastUpdatedBy", "" );

            return entity;
        }//


        #endregion

    }
}
