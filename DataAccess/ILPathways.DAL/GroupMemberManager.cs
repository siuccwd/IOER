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
	/// <summary>
	/// Data access manager for GroupMember
	/// </summary>
	public class GroupMemberManager : BaseDataManager
	{
		static string className = "GroupMemberManager";

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
		public GroupMemberManager()
		{ }//

		/// <summary>
		/// struct for transporting Group_MemberRelationship
		/// </summary>
		[Serializable]
		public struct Group_MemberRelationship
		{
			public int GroupId;
			public int UserId;
			public int GroupCategoryId;

			public string Status;		
			public DateTime Created;
			public string Message;

		}

		#region ====== Core Methods ===============================================
		/// <summary>
		/// Delete an GroupMember record - first use the group code to lookup the GroupMember in order to get Id
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="groupCode"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public static bool Delete( int userId, string groupCode, ref string statusMessage )
		{
			int pId = 0;
			int groupId = 0;

			GroupMember mbr = Get( pId, userId, groupId, groupCode );
			if ( mbr == null || mbr.Id == 0 )
			{
				//??
				return false;
			} else
			{
				return Delete( mbr.Id, ref statusMessage );
			}
		}//

		/// <summary>
		/// Delete an GroupMember record
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
			} catch ( Exception ex )
			{
				LogError( ex, className + ".Delete() " );
				statusMessage = className + "- Unsuccessful: Delete(): " + ex.Message.ToString();

				successful = false;
			}
			return successful;
		}//

		/// <summary>
		/// Add an GroupMember record - first use the group code to lookup the groupId
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="groupCode"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public static int Create( GroupMember entity, string groupCode, ref string statusMessage )
		{

			AppGroup group = GroupManager.GetByCode( groupCode );
			if ( group == null || group.Id == 0 )
			{
				//??
				statusMessage = className + "- Unsuccessful: Create(): Error the related AppGroup was not found (using a group code of: " + groupCode + ")";
				return 0;
			} else
			{
				entity.GroupId = group.Id;
				return Create( entity, ref statusMessage );
			}
		}//

		/// <summary>
		/// Add an GroupMember record
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public static int Create( GroupMember entity, ref string statusMessage )
		{
			string connectionString = GatewayConnection();
			int newId = 0;

			try
			{
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 7 ];
                sqlParameters[ 0 ] = new SqlParameter( "@GroupId", entity.GroupId );
                sqlParameters[ 1 ] = new SqlParameter( "@UserId", entity.UserId );
                sqlParameters[ 2 ] = new SqlParameter( "@Status", entity.Status );
                sqlParameters[ 3 ] = new SqlParameter( "@Category", entity.Category );
                sqlParameters[ 4 ] = new SqlParameter( "@IsActive", entity.IsActive );
                sqlParameters[ 5 ] = new SqlParameter( "@Comment", entity.Comment );
                sqlParameters[ 6 ] = new SqlParameter( "@CreatedById", entity.CreatedById );
                #endregion
				SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
				if ( dr.HasRows )
				{
					dr.Read();
					newId = int.Parse( dr[ 0 ].ToString() );
				}
				dr.Close();
				dr = null;
				statusMessage = "successful";

			} catch ( Exception ex )
			{
				LogError( ex, className + ".Insert() " );
				statusMessage = className + "- Unsuccessful: Insert(): " + ex.Message.ToString();
				entity.Message = statusMessage;
				entity.IsValid = false;
			}

			return newId;
		}

		/// <summary>
		/// /// Update an GroupMember record
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static string Update( GroupMember entity )
		{
			string message = "successful";

			#region parameters
			SqlParameter[] sqlParameters = new SqlParameter[ 6 ];
			sqlParameters[ 0 ] = new SqlParameter( "@ID", entity.Id);
            sqlParameters[ 1 ] = new SqlParameter( "@Status", entity.Status );
            sqlParameters[ 2 ] = new SqlParameter( "@Category", entity.Category );
            sqlParameters[ 3 ] = new SqlParameter( "@IsActive", entity.IsActive );
            sqlParameters[ 4 ] = new SqlParameter( "@Comment", entity.Comment );
            sqlParameters[ 5 ] = new SqlParameter( "@LastUpdatedById", entity.LastUpdatedById );
			
			#endregion

			try
			{
				SqlHelper.ExecuteNonQuery( GatewayConnection(), UPDATE_PROC, sqlParameters );
				message = "successful";

			} catch ( Exception ex )
			{
				LogError( ex, className + ".Update() " );
				message = className + "- Unsuccessful: Update(): " + ex.Message.ToString();
				entity.Message = message;
				entity.IsValid = false;
			}

			return message;

		}//
		#endregion

		#region ====== Retrieval Methods ===============================================
		/// <summary>
		/// Get GroupMember record by Id
		/// </summary>
		/// <param name="pId"></param>
		/// <returns></returns>
		public static GroupMember Get( int pId )
		{
			int userId = 0;
			int groupId = 0;
			string groupCode = "";

			return Get( pId, userId, groupId, groupCode );

		}//

		/// <summary>
		/// Get GroupMember record by userId and group code
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="groupCode"></param>
		/// <returns></returns>
		public static GroupMember GetByGroupCode( string groupCode )
		{
			int userId = 0;
			int pId = 0;
			int groupId = 0;

			return Get( pId, userId, groupId, groupCode );
		
		}//
		public static GroupMember GetByGroupCode( int userId, string groupCode )
		{
			int pId = 0;
			int groupId = 0;

			return Get( pId, userId, groupId, groupCode );

		}//
		/// <summary>
		/// Get GroupMember record by userId and group Id
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="groupId"></param>
		/// <returns></returns>
		public static GroupMember GetByGroupId( int userId, int groupId )
		{
			int pId = 0;
			string groupCode = "";

			return Get( pId, userId, groupId, groupCode );

		}//

		/// <summary>
		/// Get GroupMember record by Id or userId and (group Id or group code)
		/// </summary>
		/// <param name="pId"></param>
		/// <param name="userId"></param>
		/// <param name="groupId"></param>
		/// <param name="groupCode"></param>
		/// <returns></returns>
		private static GroupMember Get( int pId, int userId, int groupId, string groupCode )
		{
            string connectionString = GatewayConnectionRO();
			GroupMember entity = new GroupMember();

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

				} else
				{
					entity.Message = "Record not found";
					entity.IsValid = false;
				}
				dr.Close();
				dr = null;
				return entity;

			} catch ( Exception ex )
			{
				LogError( ex, className + ".Get() " );
				entity.Message = className + "- Unsuccessful: Get(): " + ex.ToString();
				entity.IsValid = false;
				return entity;

			}

		}//


		/// <summary>
		/// Return true if user is a member of a group for the provided group id
		/// </summary>
		/// <param name="groupId"></param>
		/// <param name="userid"></param>
		/// <returns></returns>
		public static bool IsAGroupMember( int groupId, int userid )
		{
			string groupCode = "";

			return IsAGroupMember( groupId, userid, groupCode );
		}//


		/// <summary>
		/// Return true if user is a member of the target group based on group code
		/// </summary>
		/// <param name="groupCode"></param>
		/// <param name="userid"></param>
		/// <returns></returns>
		public static bool IsAGroupMember( string groupCode, int userid )
		{
			int groupId = 0;

			return IsAGroupMember( groupId, userid, groupCode );
		}//

		/// <summary>
		/// Return true if user is a member of the target group
		/// </summary>
		/// <param name="groupId"></param>
		/// <param name="userid"></param>
		/// <param name="groupCode"></param>
		/// <param name="programCode"></param>
		/// <returns></returns>
		private static bool IsAGroupMember( int groupId, int userid, string groupCode )
		{
			bool isMember = false;
			int createdById = 0;

			try
			{
				DataSet ds = Select( groupId, userid, groupCode, createdById );

				if ( DoesDataSetHaveRows(ds) == true)
					isMember = true;

			} catch ( Exception ex )
			{
				LogError( ex, className + ".IsGroupMember(" + userid + ") exception " );
			}

			return isMember;
		}//

		/// <summary>
		/// Select GroupMember related data using passed groupId
		/// </summary>
		/// <param name="groupId"></param>
		/// <returns></returns>
		public static DataSet Select( int groupId )
		{
			int createdById = 0;
			return Select( groupId, 0, "", createdById );
		}//


		/// <summary>
		/// Select GroupMember related data using passed parameters
		/// - where userid is owner
		/// - where userid is a team member of the group (id is provided)
		/// - where userid is a team member of any group (id is zero)
		/// - where groups of a particular type are wanted (groupCode)
		/// - future may be to filter on privileges, etc.
		/// </summary>
		/// <param name="userid"></param>
		/// <param name="groupId"></param>
		/// <param name="groupCode"></param>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public static DataSet Select( int groupId, int userid, string groupCode )
		{
			int createdById = 0;
			return Select( groupId, userid, groupCode, createdById );
		}//



		/// <summary>
		/// Select GroupMember related data using passed parameters
		/// - where userid is owner
		/// - where userid is a team member of the group (id is provided)
		/// - where userid is a team member of any group (id is zero)
		/// - where groups of a particular type are wanted (groupCode)
		/// - future may be to filter on privileges, etc.
		/// </summary>
		/// <param name="userid"></param>
		/// <param name="groupId"></param>
		/// <param name="groupCode"></param>
		/// <param name="orgId"></param>
		/// <param name="createdById">Use to only retrieve customers that were created by this userid</param>
		/// <param name="programCode"></param>
		/// <returns></returns>
		public static DataSet Select( int groupId, int userid, string groupCode, int createdById)
		{
            string connectionString = GatewayConnectionRO();

			SqlParameter[] sqlParameters = new SqlParameter[ 4 ];

			sqlParameters[ 0 ] = new SqlParameter( "@GroupId", groupId );
			sqlParameters[ 1 ] = new SqlParameter( "@userId", userid );
			sqlParameters[ 2 ] = new SqlParameter( "@GroupCode", groupCode );
			sqlParameters[ 3 ] = new SqlParameter( "@CreatedById", createdById );

			DataSet ds = new DataSet();
			try
			{
				ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, SELECT_PROC, sqlParameters );

				if ( ds.HasErrors )
				{
					return null;
				}
				return ds;
			} catch ( Exception ex )
			{
				LogError( ex, className + ".Select() " );
				return null;

			}
	    }

        /// <summary>
        /// return true if user is a valid approver for the organization
        /// </summary>
        /// <param name="pOrgId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool IsUserAnOrgApprover( int pOrgId, int userId )
        {
            bool isValid = false;
            List<GroupMember> list = OrgApproversSelect( pOrgId );
            if ( list != null && list.Count > 0 )
            {
                foreach ( GroupMember item in list )
                {
                    if ( item.UserId == userId )
                    {
                        isValid = true;
                        break;
                    }
                }
            }
            return isValid;
	    }

      
        public static List<GroupMember> OrgApproversSelect( int orgId )
        {
            List<GroupMember> collection = new List<GroupMember>();

            string connectionString = GatewayConnectionRO();

            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@OrgId", orgId );

            DataSet ds = new DataSet();

            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "[AppGroup.OrgApproversSelect]", sqlParameters );

                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        GroupMember entity = Fill( dr );
                        collection.Add( entity );
                    }
                }
                return collection;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".OrgApproversSelect() " );
                return null;

            }
           

		}

        /// <summary>
        /// Select Group Members
        /// </summary>
        /// <param name="pGroupId"></param>
        /// <param name="pUserid"></param>
        /// <param name="pGroupCode"></param>
        /// <param name="pCreatedById"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static DataSet Search( int pGroupId, int pUserid, string pGroupCode, int pCreatedById, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {

            string booleanOperator = "AND";
            string filter = "";
            //construct filter
            if ( pGroupId > 0 )
                filter = FormatSearchItem( filter, "GroupId", pGroupId.ToString(), booleanOperator );
            if ( pUserid > 0 )
                filter = FormatSearchItem( filter, "ContactId", pUserid.ToString(), booleanOperator );
            if ( pGroupCode.Trim().Length > 0 )
                filter = FormatSearchItem( filter, "GroupCode", pGroupCode, booleanOperator );
            if ( pCreatedById > 0 )
                filter = FormatSearchItem( filter, "gm.CreatedById", pCreatedById.ToString(), booleanOperator );

            return Search( filter, "", pStartPageIndex, pMaximumRows, ref pTotalRows );
        }

        /// <summary>
        /// Select Group Members
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
            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", pStartPageIndex);

            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", pMaximumRows);

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
                LogError( ex, className + ".Search() " );
                return null;

            }
        }


		/// <summary>
		/// future use
		/// </summary>
		/// <param name="pFilter"></param>
		/// <param name="pOrderBy"></param>
		/// <param name="pStartPageIndex"></param>
		/// <param name="pMaximumRows"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
        private static DataSet ExternalGroupMember_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
		{
			string connectionString = GatewayConnectionRO();

			if ( pFilter.Length > 0 )
				pFilter = " Where " + pFilter;

			SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
			sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );

			sqlParameters[ 1 ] = new SqlParameter( "@OrderBy", pOrderBy );

			sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", SqlDbType.Int );
			sqlParameters[ 2 ].Value = pStartPageIndex;

			sqlParameters[ 3 ] = new SqlParameter( "@PageSize", SqlDbType.Int );
			sqlParameters[ 3 ].Value = pMaximumRows;

			sqlParameters[ 4 ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
			sqlParameters[ 4 ].Direction = ParameterDirection.Output;

			DataSet ds = new DataSet();
			try
			{
				ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "[AppGroup.ExternalGroupMember_Search]", sqlParameters );

				string rows = sqlParameters[ 4 ].Value.ToString();
				try
				{
					pTotalRows = Int32.Parse( rows );
				} catch
				{
					pTotalRows = 0;
				}

				if ( ds.HasErrors )
				{
					return null;
				}
				return ds;
			} catch ( Exception ex )
			{
				LogError( ex, className + ".ExternalGroupMember_Search() " );
				return null;

			}
		}


        #endregion

        #region ====== Helper Methods ===============================================
        /// <summary>
        /// Select Group Members
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static DataSet GroupOrgMbrSearch( string pFilter, string pSortOrder, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
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
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "[AppGroup.OrgMemberSearch]", sqlParameters );

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
                LogError( ex, className + ".GroupOrgMbrSearch() " );
                return null;

            }
        }

		#endregion

		#region ====== Helper Methods ===============================================
		/// <summary>
		/// Fill an GroupMember object from a data reader
		/// </summary>
		/// <param name="dr">SqlDataReader</param>
		/// <returns>GroupMember</returns>
		public static GroupMember Fill( SqlDataReader dr )
		{
			GroupMember entity = new GroupMember();

			entity.IsValid = true;

			entity.GroupId = GetRowColumn( dr, "groupId", 0 );
            entity.UserId = GetRowColumn( dr, "UserId", 0 );
			entity.Id = GetRowColumn( dr, "id", 0 );
			entity.Status = GetRowColumn( dr, "status", "" );
			entity.Category = GetRowColumn( dr, "category", "" );

			entity.Comment = GetRowColumn( dr, "comment", "" );

			entity.IsActive = GetRowColumn( dr, "isActive", false );
			

			entity.Created = GetRowColumn( dr, "created", System.DateTime.MinValue );
			entity.LastUpdated = GetRowColumn( dr, "lastUpdated", System.DateTime.MinValue );
			entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
			entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );

			//get associated AppUser
			//if ( entity.UserId > 0 )
            //{
            //    AppUser user = new UserManager().Get( entity.UserId );
            //    if ( user != null && user.IsValid )
            //    {
            //        entity.RelatedAppUser = user;

            //        entity.FullName = user.FullName();
            //        entity.SortName = user.SortName();

            //    }

            //} else
            //{
            //    entity.FullName = GetRowColumn( dr, "FullName", "" );
            //    entity.SortName = GetRowColumn( dr, "SortName", "" );
            //}

            entity.FullName = GetRowColumn( dr, "FullName", "" );
            entity.SortName = GetRowColumn( dr, "SortName", "" );
            entity.UserEmail = GetRowColumn( dr, "UserEmail", "" );
			

			//additional derived properties
			entity.GroupName = GetRowColumn( dr, "GroupName", "" );
			entity.GroupCode = GetRowColumn( dr, "GroupCode", "" );


			return entity;
		}//

        public static GroupMember Fill( DataRow dr )
        {
            GroupMember entity = new GroupMember();

            entity.IsValid = true;

            entity.GroupId = GetRowColumn( dr, "groupId", 0 );
            entity.UserId = GetRowColumn( dr, "UserId", 0 );
            entity.Id = GetRowColumn( dr, "id", 0 );
            entity.Status = GetRowColumn( dr, "status", "" );
            entity.Category = GetRowColumn( dr, "category", "" );

            entity.Comment = GetRowColumn( dr, "comment", "" );

            entity.IsActive = GetRowColumn( dr, "isActive", false );


            entity.Created = GetRowColumn( dr, "created", System.DateTime.MinValue );
            entity.LastUpdated = GetRowColumn( dr, "lastUpdated", System.DateTime.MinValue );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );

            //get associated AppUser
            //if ( entity.UserId > 0 )
            //{
            //    AppUser user = new UserManager().Get( entity.UserId );
            //    if ( user != null && user.IsValid )
            //    {
            //        entity.RelatedAppUser = user;

            //        entity.FullName = user.FullName();
            //        entity.SortName = user.SortName();

            //    }

            //} else
            //{
            //    entity.FullName = GetRowColumn( dr, "FullName", "" );
            //    entity.SortName = GetRowColumn( dr, "SortName", "" );
            //}

            entity.FullName = GetRowColumn( dr, "FullName", "" );
            entity.SortName = GetRowColumn( dr, "SortName", "" );
            entity.UserEmail = GetRowColumn( dr, "UserEmail", "" );


            //additional derived properties
            entity.GroupName = GetRowColumn( dr, "GroupName", "" );
            entity.GroupCode = GetRowColumn( dr, "GroupCode", "" );


            return entity;
        }//
		#endregion

	}
}