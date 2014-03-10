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
	/// Data access manager for OrganizationMemberManager
	/// </summary>
	public class OrganizationMemberManager : BaseDataManager
	{
		static string className = "OrganizationMemberManager";

		/// <summary>
		/// Base procedures
		/// </summary>
		const string GET_PROC = "OrganizationMemberGet";
		const string SELECT_PROC = "OrganizationMemberSelect";	
		const string DELETE_PROC = "OrganizationMemberDelete";
		const string INSERT_PROC = "OrganizationMemberInsert";
		const string UPDATE_PROC = "OrganizationMemberUpdate";


		/// <summary>
		/// Default constructor
		/// </summary>
		public OrganizationMemberManager()
		{ }//

		#region ====== Core Methods ===============================================

        /// <summary>
        /// Delete a OrganizationMember record 
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="userid"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static bool Delete( int orgId, int userid, ref string statusMessage )
        {
            return Delete( 0, orgId, userid, ref statusMessage );
        }

		/// <summary>
		/// Delete an OrganizationMember record
		/// </summary>
		/// <param name="id"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public static bool Delete(int id, ref string statusMessage )
		{
            return Delete( id, 0, 0, ref statusMessage );
		}//

        private static bool Delete( int id, int orgId, int userid, ref string statusMessage )
        {
            string connectionString = GatewayConnection();
            bool successful = false;

            SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Id", id );
            sqlParameters[ 0 ] = new SqlParameter( "@OrgId", orgId );
            sqlParameters[ 0 ] = new SqlParameter( "@Userid", userid );

            try
            {
                SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
                successful = true;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Delete() " );
                statusMessage = className + "- Unsuccessful: Delete(): " + ex.Message.ToString();

                successful = false;
            }
            return successful;
        }


		/// <summary>
		/// Add an OrganizationMember record
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public static int Create( OrganizationMember entity, ref string statusMessage )
		{
			string connectionString = GatewayConnection();
			int newId = 0;

			#region parameters
			SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
			sqlParameters[ 0 ] = new SqlParameter( "@OrgId", SqlDbType.Int );
			sqlParameters[ 0 ].Value = entity.OrgId;

			sqlParameters[ 1 ] = new SqlParameter( "@UserId",  entity.UserId);

            sqlParameters[ 2 ] = new SqlParameter( "@TypeId", entity.OrgMemberTypeId );

			sqlParameters[ 3 ] = new SqlParameter( "@CreatedById", entity.CreatedById);



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

			} catch ( Exception ex )
			{
				if ( ex.Message.ToLower().IndexOf( "violation of primary key constraint" ) > -1 )
				{
					statusMessage = "Error: Duplicate Record. This person is already associated with the current organization and program";
				} else
				{
					LogError( ex, className + string.Format( ".Insert() for orgId: {0} and userid: {1} and contact type: {2}", entity.OrgId.ToString(), entity.UserId.ToString(), entity.OrgMemberType ) );
					statusMessage = className + "- Unsuccessful: Insert(): " + ex.Message.ToString();
				}
				entity.Message = statusMessage;
				entity.IsValid = false;
			}

			return newId;
		}

		/// <summary>
		/// /// Update an OrganizationMember record
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static string Update( OrganizationMember entity )
		{
			string message = "successful";
			//TODO - if we are going to allow updating of primary contact, then this update won't workm, would have to do 
			//			 a delete and add

			#region parameters
			SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
            sqlParameters[ 0 ] = new SqlParameter("@Id", entity.Id);
            sqlParameters[ 1 ] = new SqlParameter( "@TypeId", entity.OrgMemberTypeId );
            sqlParameters[ 2 ] = new SqlParameter( "@CreatedById", entity.CreatedById );

			#endregion

			try
			{
				SqlHelper.ExecuteNonQuery( GatewayConnection(), UPDATE_PROC, sqlParameters );
				message = "successful";

			} catch ( Exception ex )
			{
				LogError( ex, className + string.Format(".Update() for Id: {0} and contact type: {1}",  entity.Id.ToString(), entity.OrgMemberType));
				message = className + "- Unsuccessful: Update(): " + ex.Message.ToString();
				entity.Message = message;
				entity.IsValid = false;
			}

			return message;

		}//
		#endregion


		#region ====== Retrieval Methods ===============================================
        public static OrganizationMember Get( int pId )
        {
            return Get( pId, 0, 0 );
        }
        public static OrganizationMember Get( int orgId, int userId )
        {
            return Get( 0, orgId, userId );
        }
		/// <summary>
		/// Get a OrganizationMember using the primary key: Id 
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
        private static OrganizationMember Get( int pId, int pOrgId, int pUserId )
		{

			OrganizationMember entity = new OrganizationMember();
			string connectionString = GatewayConnectionRO();

			try
			{
				SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
				sqlParameters[ 0 ] = new SqlParameter( "@Id", pId);
                sqlParameters[ 1 ] = new SqlParameter( "@OrgId",  pOrgId);
                sqlParameters[ 2 ] = new SqlParameter( "@UserId",  pUserId);

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
				LogError( ex, className + ".Get(int userId) " );
				entity.Message = className + "- Unsuccessful: Get(int userId): " + ex.ToString();
				entity.IsValid = false;

			}
			return entity;
		} //GetOrganizationMember

        public static List<OrganizationMember> SearchAsList( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            List<OrganizationMember> list = new List<OrganizationMember>();
            DataSet ds = Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
                {
                    OrganizationMember org = Fill( dr );
                    list.Add( org );
                } //end foreach
            }

            return list;
        }//

		/// <summary>
		/// Select business contacts related data using passed parameters
		/// - uses custom paging
		/// - only requested range of rows will be returned
		/// </summary>
		/// <param name="pFilter"></param>
		/// <param name="pStartPageIndex"></param>
		/// <param name="pMaximumRows"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
        public static DataSet Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
		{
            int outputCol = 4;

            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", pOrderBy );
            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", pStartPageIndex );
            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", pMaximumRows );

            sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ outputCol ].Direction = ParameterDirection.Output;

            using ( SqlConnection conn = new SqlConnection( GatewayConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Organization.MemberSearch]", sqlParameters );

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
		}

		#endregion

		#region ====== Helper Methods ===============================================
		
		/// <summary>
		/// Fill an OrganizationMember object from a DataRow
		/// </summary>
		/// <param name="dr">DataRow</param>
		/// <returns>OrganizationMember</returns>
		public static OrganizationMember Fill( DataRow dr )
		{
			OrganizationMember entity = new OrganizationMember();

            entity.Id = GetRowPossibleColumn( dr, "OrgMbrId", 0 );
            if (entity.Id == 0)
                entity.Id = GetRowPossibleColumn( dr, "Id", 0 );

			entity.IsValid = true;
			entity.OrgId = GetRowColumn( dr, "OrgId", 0 );
			entity.UserId = GetRowColumn( dr, "UserId", 0 );

            entity.OrgMemberTypeId = GetRowColumn( dr, "OrgMemberTypeId", 0 );
            entity.OrgMemberType = GetRowColumn( dr, "OrgMemberType", "" );
            entity.Organization = GetRowColumn( dr, "Organization", "Missing" );
			//entity.Comment = dr[ "Comment" ].ToString();

            entity.Created = GetRowPossibleColumn( dr, "MemberAdded", entity.DefaultDate );
            if (entity.Created == entity.DefaultDate)
                entity.Created = GetRowPossibleColumn( dr, "Created", entity.DefaultDate );
            entity.CreatedById = GetRowPossibleColumn( dr, "CreatedById", 0 );

            //entity.LastUpdated = DateTime.Parse( dr[ "LastUpdated" ].ToString() );
            //entity.LastUpdatedById = GetRowPossibleColumn( dr, "LastUpdatedById", 0 );


			return entity;
		}//

		/// <summary>
		/// Fill an OrganizationMember object from a SqlDataReader
		/// </summary>
		/// <param name="dr"></param>
		/// <returns></returns>
		public static OrganizationMember Fill( SqlDataReader dr )
		{
            OrganizationMember entity = new OrganizationMember();

            entity.Id = GetRowPossibleColumn( dr, "OrgMbrId", 0 );
            if ( entity.Id == 0 )
                entity.Id = GetRowPossibleColumn( dr, "Id", 0 );

            entity.IsValid = true;
            entity.OrgId = GetRowColumn( dr, "OrgId", 0 );
            entity.UserId = GetRowColumn( dr, "UserId", 0 );

            entity.OrgMemberTypeId = GetRowColumn( dr, "OrgMemberTypeId", 0 );
            entity.OrgMemberType = GetRowColumn( dr, "OrgMemberType", "" );
            entity.Organization = GetRowColumn( dr, "Organization", "Missing" );
            //entity.Comment = dr[ "Comment" ].ToString();

            entity.Created = GetRowPossibleColumn( dr, "MemberAdded", entity.DefaultDate );
            if ( entity.Created == entity.DefaultDate )
                entity.Created = GetRowPossibleColumn( dr, "Created", entity.DefaultDate );
            entity.CreatedById = GetRowPossibleColumn( dr, "CreatedById", 0 );

            //entity.LastUpdated = DateTime.Parse( dr[ "LastUpdated" ].ToString() );
            //entity.LastUpdatedById = GetRowPossibleColumn( dr, "LastUpdatedById", 0 );

			return entity;
		}//
		#endregion

	}
}
