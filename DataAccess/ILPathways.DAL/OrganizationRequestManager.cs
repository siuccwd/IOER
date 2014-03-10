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
using MyEntity = ILPathways.Business.OrganizationRequest;

namespace ILPathways.DAL
{
    /// <summary>
    /// Data access manager for OrganizationRequest
    /// </summary>
    public class OrganizationRequestManager : BaseDataManager
    {
        static string thisClassName = "OrganizationRequestManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[OrganizationRequestGet]";
        const string SEARCH_PROC = "[OrganizationRequestSearch]";
        const string DELETE_PROC = "[OrganizationRequestDelete]";
        const string INSERT_PROC = "[OrganizationRequestInsert]";
        const string UPDATE_PROC = "[OrganizationRequestUpdate]";


        /// <summary>
        /// Default constructor
        /// </summary>
        public OrganizationRequestManager()
        { }//

        #region ====== Core Methods ===============================================
        /// <summary>
        /// Delete a OrganizationRequest record
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( int pId, ref string statusMessage )
        {
            bool successful = false;
            using ( SqlConnection conn = new SqlConnection( GatewayConnection() ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );

                    SqlHelper.ExecuteNonQuery( conn, CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
                    successful = true;
                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".Delete() " );
                    statusMessage = thisClassName + "- Unsuccessful: Delete(): " + ex.Message.ToString();

                    successful = false;
                }
            }
            return successful;
        }//

        /// <summary>
        /// Add a OrganizationRequest record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int Create( MyEntity entity, ref string statusMessage )
        {
            int newId = 0;
            using ( SqlConnection conn = new SqlConnection( GatewayConnection() ) )
            {
                try
                {

                    #region parameters
                    SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@UserId", entity.UserId );
                    sqlParameters[ 1 ] = new SqlParameter( "@OrgId", entity.OrgId );
                    sqlParameters[ 2 ] = new SqlParameter( "@OrganzationName", entity.OrganzationName );
                    sqlParameters[ 3 ] = new SqlParameter( "@Action", entity.Action );

                    #endregion

                    SqlDataReader dr = SqlHelper.ExecuteReader( conn, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
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
                    LogError( ex, thisClassName + string.Format( ".Create() for Id: {0} and UserId: {1} and CreatedBy: {2}", entity.Id.ToString(), entity.UserId.ToString(), entity.CreatedBy ) );
                   
                    statusMessage = thisClassName + "- Unsuccessful: Create(): " + ex.Message.ToString();

                    entity.Message = statusMessage;
                    entity.IsValid = false;
                }
            }
            return newId;
        }

        /// <summary>
        /// Update a OrganizationRequest record
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string Update( MyEntity entity )
        {
            string message = "successful";
            using ( SqlConnection conn = new SqlConnection( GatewayConnection() ) )
            {
                try
                {

                    #region parameters
                    SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@id", entity.Id );
                    sqlParameters[ 1 ] = new SqlParameter( "@Action", entity.Action );
                    sqlParameters[ 2 ] = new SqlParameter( "@IsActive", entity.IsActive );
                    sqlParameters[ 3 ] = new SqlParameter( "@LastUpdatedById", entity.LastUpdatedById );
                    #endregion

                    SqlHelper.ExecuteNonQuery( conn, UPDATE_PROC, sqlParameters );
                    message = "successful";

                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + string.Format( ".Update() for Id: {0} and Action: {1} and LastUpdatedBy: {2}", entity.Id.ToString(), entity.Action.ToString(), entity.LastUpdatedBy ) );

                    message = thisClassName + "- Unsuccessful: Update(): " + ex.Message.ToString();
                    entity.Message = message;
                    entity.IsValid = false;
                }
            }
            return message;

        }//
        #endregion

        #region ====== Retrieval Methods ===============================================
        /// <summary>
        /// Get OrganizationRequest record
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public MyEntity Get( int pId )
        {
            MyEntity entity = new MyEntity();
            using ( SqlConnection conn = new SqlConnection( GatewayConnectionRO() ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
                    sqlParameters[ 0 ].Value = pId;


                    SqlDataReader dr = SqlHelper.ExecuteReader( conn, GET_PROC, sqlParameters );

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
                    LogError( ex, thisClassName + ".Get() " );
                    entity.Message = "Unsuccessful: " + thisClassName + ".Get(): " + ex.Message.ToString();
                    entity.IsValid = false;
                    return entity;

                }
            }
  }//

        /// <summary>
        /// Search for OrganizationRequest related data using passed parameters
        /// - uses custom paging
        /// - returns collection
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public List<MyEntity> SearchList( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {

            List<MyEntity> collection = new List<MyEntity>();

            DataSet ds = new DataSet();
            try
            {
                ds = Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );

                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        MyEntity entity = Fill( dr );
                        collection.Add( entity );
                    }
                }
                return collection;
            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".SearchList() " );
                return null;

            }
        }//

        /// <summary>
        /// Search for OrganizationRequest related data using passed parameters
        /// - uses custom paging
        /// - only requested range of rows will be returned
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public DataSet Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {

            int outputCol = 4;
            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", pOrderBy );

            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", SqlDbType.Int );
            sqlParameters[ 2 ].Value = pStartPageIndex;

            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", SqlDbType.Int );
            sqlParameters[ 3 ].Value = pMaximumRows;

            sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ outputCol ].Direction = ParameterDirection.Output;

            using ( SqlConnection conn = new SqlConnection( GatewayConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, SEARCH_PROC, sqlParameters );
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
                    return ds;
                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".Search() " );
                    return null;
                }
            }
        }

        #endregion

        #region ====== Helper Methods ===============================================
        /// <summary>
        /// Fill a OrganizationRequest object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>MyEntity</returns>
        public MyEntity Fill( SqlDataReader dr )
        {
            MyEntity entity = new MyEntity();

            entity.IsValid = true;

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.UserId = GetRowColumn( dr, "UserId", 0 );
            entity.OrgId = GetRowColumn( dr, "OrgId", 0 );
            //todo - get org if orgId has value
            entity.OrganzationName = GetRowColumn( dr, "OrganzationName", "" );
            entity.Action = GetRowColumn( dr, "Action", "" );
            entity.IsActive = GetRowColumn( dr, "IsActive", false );
            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );

            return entity;
        }//

        public MyEntity Fill( DataRow dr )
        {
            MyEntity entity = new MyEntity();

            entity.IsValid = true;

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.UserId = GetRowColumn( dr, "UserId", 0 );
            entity.OrgId = GetRowColumn( dr, "OrgId", 0 );
            //todo - get org if orgId has value
            entity.OrganzationName = GetRowColumn( dr, "OrganzationName", "" );
            entity.Action = GetRowColumn( dr, "Action", "" );
            entity.IsActive = GetRowColumn( dr, "IsActive", false );
            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );

            return entity;
        }//
        #endregion

    }
}