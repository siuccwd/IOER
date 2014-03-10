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
    /// <summary>
    /// Manage data access for Group related objects
    /// </summary>
    public class GroupManager : BaseDataManager
    {
        static string className = "GroupManager";

        #region Constants for database procedures
        /// <summary>
        /// Group / GroupUser related
        /// </summary>
        const string GET_PROC = "[AppGroupGet]";
        const string SELECT_PROC = "[AppGroupSelect]";
        const string SEARCH_PROC = "[AppGroupSearch]";
        const string DELETE_PROC = "[AppGroupDelete]";
        const string INSERT_PROC = "[AppGroupInsert]";
        const string UPDATE_PROC = "[AppGroupUpdate]";

        const string GROUP_MEMBER_GET_PROC = "GroupMemberGet";
        //don't use following
        //const string GROUP_MEMBERS_GET_PROC = "GetGroupMembers";

        const string GROUP_POOL_SELECT = "Group_PoolGroup_Select";

        #endregion

        #region Public Constants
        /// <summary>
        /// Group for Career Builder account mtce 
        /// </summary>
        public const string CAREERBUILDER_ACCOUNTS_GROUP = "CB_AccountMaintenance";

        /// <summary>
        /// Group for Career Builder account mtce 
        /// </summary>
        public const string CONTENT_MANAGERS_GROUP = "ContentManagers";

        public const string ASMT_GROUP_ACCEPT_STATUS = "Agree";
        public const string ASMT_GROUP_DENY_STATUS = "Deny";
        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public GroupManager() { }//


        #region Core Methods

        /// <summary>
        /// Delete an AppGroup record
        /// </summary>
        /// <param name="id"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static bool Delete( int id, ref string statusMessage )
        {
            string connectionString = GatewayConnection();
            bool result = true;
            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
            sqlParameters[ 0 ].Value = id;


            try
            {
                SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Delete()" );
                statusMessage = "Unsuccessful: GroupManager.Delete(): " + ex.Message.ToString();
                result = false;
            }

            return result;
        }//

        /// <summary>
        /// GroupInsert
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static int Create( AppGroup entity, ref string statusMessage )
        {
            string connectionString = GatewayConnection();
            int newId = 0;

            SqlParameter[] sqlParameters = new SqlParameter[ 9 ];
            //sqlParameters[ 0 ] = new SqlParameter( "@ApplicationId", entity.ApplicationId );
            sqlParameters[ 0 ] = new SqlParameter( "@GroupCode", entity.GroupCode );
            sqlParameters[ 1 ] = new SqlParameter( "@Title", entity.Title );
            sqlParameters[ 2 ] = new SqlParameter( "@Description", entity.Description );

            sqlParameters[ 3 ] = new SqlParameter( "@IsActive", entity.IsActive);
            sqlParameters[ 4 ] = new SqlParameter( "@GroupTypeId", entity.GroupTypeId );
            sqlParameters[ 5 ] = new SqlParameter( "@ContactId", entity.ContactId);
            sqlParameters[ 6 ] = new SqlParameter( "@OrgId", entity.OrgId );
            sqlParameters[ 7 ] = new SqlParameter( "@ParentGroupId", entity.ParentGroupId);
            sqlParameters[ 8 ] = new SqlParameter( "@CreatedById", entity.CreatedById );

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
                LogError( ex, className + ".Create() " );
                statusMessage = "Unsuccessful: " + className + ".Create(): " + ex.Message.ToString();
                entity.Message = statusMessage;
                entity.IsValid = false;
            }
            return newId;
        }//

        /// <summary>
        /// Update a AppGroup record
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string Update( AppGroup entity )
        {
            string connectionString = GatewayConnection();
            string message = "successful";

            SqlParameter[] sqlParameters = new SqlParameter[ 7 ];
            sqlParameters[ 0 ] = new SqlParameter( "@id", entity.Id );
            sqlParameters[ 1 ] = new SqlParameter( "@Title", entity.Title );
            sqlParameters[ 2 ] = new SqlParameter( "@Description", entity.Description );
            sqlParameters[ 3 ] = new SqlParameter( "@IsActive", SqlDbType.Bit );
            sqlParameters[ 3 ].Value = entity.IsActive;
            sqlParameters[ 4 ] = new SqlParameter( "@ContactId", SqlDbType.Int );
            sqlParameters[ 4 ].Value = entity.ContactId;
            sqlParameters[ 5 ] = new SqlParameter( "@GroupTypeId", entity.GroupTypeId );
            sqlParameters[ 6 ] = new SqlParameter( "@LastUpdatedById", entity.LastUpdatedById );


            try
            {
                SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, UPDATE_PROC, sqlParameters );
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Update()" );
                message = "Unsuccessful: " + className + ".Update(): " + ex.Message.ToString();

                entity.Message = message;
            }

            return message;
        } //
	
        #endregion

        #region Retrieval Methods

        /// <summary>
        /// Retrieve a group by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static AppGroup Get( int id )
        {
            string groupCode = "";
            if ( id > 0 )
            {
                return Get( id, groupCode );
            }
            else
            {
                AppGroup grp = new AppGroup();
                grp.IsValid = false;
                grp.Message = "Invalid Get request with key less than one.";
                return grp;
            }

        }//

        /// <summary>
        /// Retrieve a group by group code
        /// </summary>
        /// <param name="groupCode"></param>
        /// <returns></returns>
        /// <remarks>10-08-12 mparsons - added check for existing groupCode. If blank, returns invalid group</remarks>
        public static AppGroup GetByCode( string groupCode )
        {
            int id = 0;
            if ( groupCode.Trim().Length > 0 )
            {
                return Get( id, groupCode );
            }
            else
            {
                AppGroup grp = new AppGroup();
                grp.IsValid = false;
                grp.Message = "Invalid Get request with key (GroupCode) equal to blank.";
                return grp;
            }

        }//

        /// <summary>
        /// Get an AppGroup record
        /// </summary>
        /// <param name="id"></param>
        /// <param name="groupCode"></param>
        /// <returns></returns>
        private static AppGroup Get( int id, string groupCode )
        {
            AppGroup entity = new AppGroup();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@id", id );
                sqlParameters[ 1 ] = new SqlParameter( "@GroupCode", groupCode );

                //TODO - change procedure to handle GroupSession??
                SqlDataReader dr = SqlHelper.ExecuteReader( GatewayConnectionRO(), GET_PROC, sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.

                    while ( dr.Read() )
                    {
                        entity = Fill( dr );
                    }


                    //if parentGroupId exists, get and fill
                    if ( entity.ParentGroupId > 0 )
                    {
                        // entity.ParentGroup = Get( entity.ParentGroupId );
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
                LogError( ex, className + ".GetGroup()" );

                entity.Message = className + ".GetGroup(): " + ex.ToString();
                entity.IsValid = false;
                return entity;
                //return null;
            }

        }//		
        /// <summary>
        /// Fill an Group object from a data reader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>Group</returns>
        private static AppGroup Fill( SqlDataReader dr )
        {
            AppGroup entity = new AppGroup();

            entity.Id = GetRowColumn( dr, "Id", 0 );

            entity.GroupCode = GetRowColumn( dr, "GroupCode", "" );
            entity.GroupTypeId = GetRowColumn( dr, "GroupTypeId", 1 );
            entity.GroupType = GetRowColumn( dr, "GroupType", "" );

            entity.Title = GetRowColumn( dr, "Title", "" );
            entity.Description = GetRowColumn( dr, "Description", "" );

            entity.IsActive = GetRowColumn( dr, "isActive", false );
            entity.Created = GetRowColumn( dr, "created", System.DateTime.MinValue );
            entity.LastUpdated = GetRowColumn( dr, "lastUpdated", System.DateTime.MinValue );

            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            //entity.CreatedBy = GetRowColumn( dr, "createdBy", "" );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            //entity.LastUpdatedBy = GetRowColumn( dr, "lastUpdatedBy", "" );

            entity.ContactId = GetRowColumn( dr, "ContactId", 0 );
            entity.OrgId = GetRowColumn( dr, "OrgId", 0 );
            entity.ParentGroupId = GetRowColumn( dr, "ParentGroupId", 0 );


            return entity;
        }//	
        /// <summary>
        /// Retrieve all groups	for:
        /// - where userid is owner
        /// - where userid is a team member of the group (id is provided)
        /// - where userid is a team member of any group (id is zero)
        /// - where groups of a particular type are wanted
        /// - future may be to filter on privileges, etc.
        /// </summary>
        /// <param name="userid">userid of group creator</param>
        /// <param name="orgId">organization Id associated with a group</param>
        /// <param name="groupType">Group type or groups to retrieve</param>
        /// <returns></returns>
        public static DataSet Select( int userid, int orgId, string groupType )
        {
            int parentGroupId = 0;
            int parentOrgId = 0;

            return Select( userid, orgId, groupType, parentGroupId, parentOrgId, "" );

        }//
        /// <summary>
        /// Retrieve all groups	for:
        /// - where userid is owner
        /// - where userid is a team member of the group (id is provided)
        /// - where userid is a team member of any group (id is zero)
        /// - where groups of a particular type are wanted
        /// - future may be to filter on privileges, etc.
        /// </summary>
        /// <param name="userid">userid of group creator</param>
        /// <param name="orgId">organization Id associated with a group</param>
        /// <param name="groupType">Group type or groups to retrieve</param>
        /// <param name="parentGroupId">parent groupId associated with a group</param>
        /// <returns></returns>
        public static DataSet Select( int userid, int orgId, string groupType, int parentGroupId )
        {
            int parentOrgId = 0;

            return Select( userid, orgId, groupType, parentGroupId, parentOrgId, "" );

        }//
        /// <summary>
        /// Select groups
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="orgId"></param>
        /// <param name="groupType"></param>
        /// <param name="parentGroupId"></param>
        /// <param name="parentOrgId"></param>
        /// <returns></returns>
        public static DataSet Select( int userid, int orgId, string groupType, int parentGroupId, int parentOrgId )
        {

            return Select( userid, orgId, groupType, parentGroupId, parentOrgId, "" );

        }//
        /// <summary>
        /// Retrieve all child groups for passed parent. Additionally may want to limit where user is a member of the child group
        /// - where userid is owner
        /// - where userid is a team member of the group 
        /// </summary>
        /// <param name="parentGroupId">parent groupId associated with a group</param>
        /// <param name="userid">userid of group creator or a team member under the group</param>
        /// <returns></returns>
        public static DataSet SelectChildGroups( int parentGroupId, int userid )
        {
            int orgId = 0;
            string groupType = "";
            int parentOrgId = 0;

            return Select( userid, orgId, groupType, parentGroupId, parentOrgId, "" );
        }//

        /// <summary>
        /// Retrieve all groups	for:
        /// - where userid is owner
        /// - where userid is a team member of the group 
        /// - where groups of a particular type are wanted
        /// - where group has passed parentGroupId 
        /// - future may be to filter on privileges, etc.
        /// </summary>
        /// <param name="userid">userid of group creator or a team member under the group</param>
        /// <param name="orgId">organization Id associated with a group</param>
        /// <param name="groupType">Group type or groups to retrieve</param>
        /// <param name="parentGroupId">parent groupId associated with a group</param>
        /// <param name="parentOrgId">parent of the organization associated with a group. This allows retrieval of all groups for an lwia</param>
        /// <returns></returns>
        public static DataSet Select( int userid, int orgId, string groupType, int parentGroupId, int parentOrgId, string onlineProgramCode )
        {
            try
            {
                //TODO - modify to just call the search
                string filter = "";
                string booleanOperator = " AND ";
                int pTotalRows = 0;
                if ( userid > 0 )
                    filter += FormatSearchItem( filter, "ContactId", userid, booleanOperator );

                if ( orgId > 0 )
                    filter += FormatSearchItem( filter, "OrgId", orgId, booleanOperator );

                if ( groupType.Length > 0 )
                    filter += FormatSearchItem( filter, "codes.Title", groupType, booleanOperator );

                if ( parentGroupId > 0 )
                    filter += FormatSearchItem( filter, "ParentGroupId", parentGroupId, booleanOperator );

                return Search( filter, "Title", 1, 1000, ref pTotalRows );
               
                
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Select()" );
                return null;
            }
        }//

        /// <summary>
        /// Group dynamic search with custom paging
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static DataSet Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            string connectionString = GatewayConnectionRO();

            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", pOrderBy );

            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", SqlDbType.Int );
            sqlParameters[ 2 ].Value = pStartPageIndex;

            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", SqlDbType.Int );
            sqlParameters[ 3 ].Value = pMaximumRows;

            sqlParameters[ 4 ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ 4 ].Direction = ParameterDirection.Output;

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, SEARCH_PROC, sqlParameters );

                string rows = sqlParameters[ 4 ].Value.ToString();
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
                LogError( ex, className + ".Search() " );
                return null;

            }
        }

 
        #endregion

        #region Helper Methods
        /// <summary>
        /// Check if user is owner of passed group
        /// </summary>
        /// <param name="pGroupId"></param>
        /// <param name="pUserId"></param>
        /// <returns></returns>
        public static bool IsGroupOwner( int pGroupId, int pUserId )
        {
            bool isGroupOwner = false;

            try
            {
                AppGroup grp = Get( pGroupId );
                if ( grp != null && grp.IsValid )
                {
                    if ( grp.ContactId == pUserId )
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".IsGroupOwner() exception for group: {0} and user:{1}", pGroupId, pUserId ) );
                isGroupOwner = false;
            }

            return isGroupOwner;
        }//

        #endregion
    }
}
