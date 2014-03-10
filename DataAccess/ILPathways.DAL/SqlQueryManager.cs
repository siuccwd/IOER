using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Resources;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using Microsoft.ApplicationBlocks.Data;
using ILPathways.Business;

namespace ILPathways.DAL
{
    public class SqlQueryManager : BaseDataManager
    {
        static string className = "SqlQueryManager";

        #region Constants for database procedures

        /// <summary>
        /// SqlQuery related
        /// </summary>
        const string DELETE_PROC = "SqlQueryDelete";
        const string GET_PROC = "SqlQueryGet";
        const string INSERT_PROC = "SqlQueryInsert";
        const string SELECT_PROC = "SqlQuerySelect";
        const string UPDATE_PROC = "SqlQueryUpdate";

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public SqlQueryManager()
        { }//
        #region ====== Core Methods ===============================================

        /// <summary>
        /// Delete a SqlQuery entity
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="statusMessage"></param>/// 
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
                LogError( ex, className + ".Delete() " );
                statusMessage = className + "- Unsuccessful: Delete(): " + ex.Message.ToString();

                successful = false;
            }
            return successful;
        }//

        /// <summary>
        /// Insert a SqlQuery entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static int Create( SqlQuery entity, ref string statusMessage )
        {
            string connectionString = GatewayConnection();
            int newId = 0;
            #region parameters
            SqlParameter[] sqlParameters = new SqlParameter[ 8 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Title", entity.Title );

            sqlParameters[ 1 ] = new SqlParameter( "@Description", entity.Description );

            sqlParameters[ 2 ] = new SqlParameter( "@QueryCode", entity.QueryCode );

            sqlParameters[ 3 ] = new SqlParameter( "@Category", SqlDbType.VarChar );
            sqlParameters[ 3 ].Size = 25;
            sqlParameters[ 3 ].Value = entity.Category;

            sqlParameters[ 4 ] = new SqlParameter( "@SQL", SqlDbType.VarChar );
            sqlParameters[ 4 ].Value = entity.SQL;

            sqlParameters[ 5 ] = new SqlParameter( "@OwnerId", SqlDbType.Int );
            sqlParameters[ 5 ].Value = entity.OwnerId;

            sqlParameters[ 6 ] = new SqlParameter( "@IsPublic", SqlDbType.Bit );
            sqlParameters[ 6 ].Value = entity.IsPublic;

            sqlParameters[ 7 ] = new SqlParameter( "@CreatedBy", SqlDbType.VarChar );
            sqlParameters[ 7 ].Size = 25;
            sqlParameters[ 7 ].Value = entity.CreatedBy;
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
                LogError( ex, className + "Insert() " );
                statusMessage = className + "- Unsuccessful: Insert(): " + ex.Message.ToString();
                entity.Message = statusMessage;
                entity.IsValid = false;
            }
            return newId;
        }

        /// <summary>
        /// Update a SqlQuery entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string Update( SqlQuery entity )
        {
            string connectionString = GatewayConnection();
            string message = "successful";

            #region parameters
            SqlParameter[] sqlParameters = new SqlParameter[ 9 ];
            sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
            sqlParameters[ 0 ].Value = entity.Id;

            sqlParameters[ 1 ] = new SqlParameter( "@Title", entity.Title );

            sqlParameters[ 2 ] = new SqlParameter( "@Description", entity.Description );

            sqlParameters[ 3 ] = new SqlParameter( "@QueryCode", SqlDbType.VarChar );
            sqlParameters[ 3 ].Size = 50;
            sqlParameters[ 3 ].Value = entity.QueryCode;

            sqlParameters[ 4 ] = new SqlParameter( "@Category", SqlDbType.VarChar );
            sqlParameters[ 4 ].Size = 25;
            sqlParameters[ 4 ].Value = entity.Category;

            sqlParameters[ 5 ] = new SqlParameter( "@SQL", SqlDbType.VarChar );
            sqlParameters[ 5 ].Value = entity.SQL;

            sqlParameters[ 6 ] = new SqlParameter( "@OwnerId", SqlDbType.Int );
            sqlParameters[ 6 ].Value = entity.OwnerId;

            sqlParameters[ 7 ] = new SqlParameter( "@IsPublic", SqlDbType.Bit );
            sqlParameters[ 7 ].Value = entity.IsPublic;

            sqlParameters[ 8 ] = new SqlParameter( "@LastUpdatedBy", SqlDbType.VarChar );
            sqlParameters[ 8 ].Size = 25;
            sqlParameters[ 8 ].Value = entity.LastUpdatedBy;
            #endregion
            try
            {
                SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, UPDATE_PROC, sqlParameters );

            }
            catch ( Exception ex )
            {
                LogError( ex, className + "Update()" );
                message = className + "- Unsuccessful: Update(): " + ex.Message.ToString();

                entity.Message = message;
            }
            return message;
        }
        #endregion

        #region ====== Retrieval Methods ===============================================
        /// <summary>
        /// select a single SqlQuery entity using the Id
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static SqlQuery Get( int pid )
        {
            string pQueryCode = "";

            return Get( pid, pQueryCode );

        }//

        /// <summary>
        /// select a single SqlQuery entity using the query code
        /// </summary>
        /// <param name="pQueryCode"></param>
        /// <returns></returns>
        public static SqlQuery Get( string pQueryCode )
        {
            int pid = 0;

            return Get( pid, pQueryCode );

        }//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="pQueryCode"></param>
        /// <returns></returns>
        public static SqlQuery Get( int pid, string pQueryCode )
        {
            string connectionString = GatewayConnectionRO();
            SqlQuery entity = new SqlQuery();

            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@id", pid );
            sqlParameters[ 1 ] = new SqlParameter( "@QueryCode", pQueryCode );

            try
            {
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, GET_PROC, sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.
                    while ( dr.Read() )
                    {
                        entity.Id = GetRowColumn( dr, "id", 0 );
                        entity.Title = GetRowColumn( dr, "title", "" );
                        entity.Description = GetRowColumn( dr, "description", "" );
                        entity.QueryCode = GetRowColumn( dr, "queryCode", "" );
                        entity.Category = GetRowColumn( dr, "category", "" );
                        entity.SQL = GetRowColumn( dr, "SQL", "" );
                        entity.OwnerId = GetRowColumn( dr, "ownerId", 0 );
                        entity.IsPublic = GetRowColumn( dr, "isPublic", false );
                        entity.Created = GetRowColumn( dr, "created", System.DateTime.MinValue );
                        entity.CreatedBy = GetRowColumn( dr, "createdBy", "" );
                        entity.LastUpdated = GetRowColumn( dr, "lastUpdated", System.DateTime.MinValue );
                        entity.LastUpdatedBy = GetRowColumn( dr, "lastUpdatedBy", "" );
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
                LogError( ex, className + "Get()" );

                return null;
            }
        }//

        /// <summary>
        /// Get all distinct categories
        /// - Also adds a "Select a Category" row to the returned dataset
        /// </summary>
        /// <returns></returns>
        public static DataSet SelectCategories( int pIsPublic )
        {

            try
            {
                string filter = "";
                if ( pIsPublic > -1 && pIsPublic < 2 )
                    filter = " where IsPublic = " + pIsPublic + " ";

                string sqlDefault = String.Format( "SELECT Distinct Category FROM [SqlQuery] {0} order by Category", filter );

                DataSet ds = SqlHelper.ExecuteDataset( GatewayConnectionRO(), System.Data.CommandType.Text, sqlDefault );

                if ( ds.HasErrors )
                {
                    return null;
                }

                AddEntryToTable( ds.Tables[ 0 ], 0, "Select a Category", "Category", "Category" );
                return ds;

            }
            catch ( Exception e )
            {
                LogError( e, className + ".SelectCategories() " );
                return null;
            }

        } //
        /// <summary>
        /// select SqlQuery records and returns a dataset
        /// </summary>
        /// <returns>DataSet</returns>
        public static DataSet Select()
        {
            string pQueryCode = "";
            string pCategory = "";
            int pIsPublic = 2;
            int pOwnerId = 0;

            return Select( pQueryCode, pCategory, pIsPublic, pOwnerId );

        }//

        /// <summary>
        /// select a SqlQuery entity (One or more)
        /// </summary>
        /// <param name="pQueryCode">Unique code for a query (a like filter is used in proc)</param>
        /// <param name="pCategory"></param>
        /// <param name="pIsPublic">0-not public, 1-public any other value means all</param>
        /// <param name="pOwnerId">Userid for owner of a query</param>
        /// <returns></returns>
        public static DataSet Select( string pQueryCode, string pCategory, int pIsPublic, int pOwnerId )
        {
            return Select( pQueryCode, pCategory, pIsPublic, pOwnerId, "" );

        }//

        /// <summary>
        /// select a SqlQuery entity (One or more)
        /// </summary>
        /// <param name="pQueryCode">Unique code for a query (a like filter is used in proc)</param>
        /// <param name="pCategory"></param>
        /// <param name="pIsPublic">0-not public, 1-public any other value means all</param>
        /// <param name="pOwnerId">Userid for owner of a query</param>
        /// <param name="pKeyword">Optional keyword filter</param>
        /// <returns></returns>
        public static DataSet Select( string pQueryCode, string pCategory, int pIsPublic, int pOwnerId, string pKeyword )
        {
            string connectionString = GatewayConnectionRO();  // get your connection string here

            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@QueryCode", SqlDbType.VarChar );
            sqlParameters[ 0 ].Size = 50;
            sqlParameters[ 0 ].Value = pQueryCode;

            sqlParameters[ 1 ] = new SqlParameter( "@Category", SqlDbType.VarChar );
            sqlParameters[ 1 ].Size = 25;
            sqlParameters[ 1 ].Value = pCategory;

            sqlParameters[ 2 ] = new SqlParameter( "@IsPublic", SqlDbType.SmallInt );
            sqlParameters[ 2 ].Value = pIsPublic;

            sqlParameters[ 3 ] = new SqlParameter( "@OwnerId", SqlDbType.Int );
            sqlParameters[ 3 ].Value = pOwnerId;

            sqlParameters[ 4 ] = new SqlParameter( "@Keyword", pKeyword );

            DataSet dsResult = new DataSet();
            try
            {
                dsResult = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, SELECT_PROC, sqlParameters );
                if ( dsResult.HasErrors )
                {
                    return null;
                }
                return dsResult;

            }
            catch ( Exception ex )
            {
                LogError( ex, className + "Select()" );

                return null;
            }

        }//
        #endregion
    }
}
