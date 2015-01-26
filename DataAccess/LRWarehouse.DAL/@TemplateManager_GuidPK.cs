using System;

using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using MyEntity = LRWarehouse.Business.Resource;

/*
 * USE this template for tables where the PK is a uniqueidentifier
 * =======================================================================
Instructions
- change all Template to the target table/entity name
- change all MyEntity to the target table/entity name
- use Code Generator to create data access methods
- copy and paste into related methods
  
NOTES
- Methods are no longer static. the calling code will have to first instantiate the data manager before using any methods
- re: RowId - this template contains Delete and Get using both Id and RowId. Usually only one of the latter is required, 
  so delete the other one

*/

namespace LRWarehouse.DAL
{/// <summary>
    /// Data access manager for Template
    /// </summary>
    public class TemplateManager_GuidPK : BaseDataManager
    {
        const string thisClassName = "TemplateManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[TemplateGet]";
        const string SELECT_PROC = "[TemplateSelect]";
        const string DELETE_PROC = "[TemplateDelete]";
        const string INSERT_PROC = "[TemplateInsert]";
        const string UPDATE_PROC = "[TemplateUpdate]";


        /// <summary>
        /// Default constructor
        /// </summary>
        public TemplateManager_GuidPK()
        {
            //base constructor sets common connection strings
        }//

        #region ====== Core Methods ===============================================
        /// <summary>
        /// Delete an Template record using rowId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( string pRowId, ref string statusMessage )
        {
            bool successful = false;
            using ( SqlConnection conn = new SqlConnection( this.ConnString ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
                    sqlParameters[ 0 ].Value = new Guid( pRowId );

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
        /// Add an Template record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public string Create( MyEntity entity, ref string statusMessage )
        {
            string newId = "";
            using ( SqlConnection conn = new SqlConnection( this.ConnString ) )
            {
                try
                {

                    #region parameters
                    //replace following with actual nbr of parameters and do assignments
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
                    sqlParameters[ 0 ].Value = entity.Id;

                    //...


                    #endregion

                    SqlDataReader dr = SqlHelper.ExecuteReader( conn, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
                    if ( dr.HasRows )
                    {
                        dr.Read();
                        newId = dr[ 0 ].ToString();
                    }
                    dr.Close();
                    dr = null;
                    statusMessage = "successful";

                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + string.Format( ".Create() for Id: {0} ", entity.Id.ToString() ) );
                    statusMessage = thisClassName + "- Unsuccessful: Create(): " + ex.Message.ToString();
                   
                }
            }
            return newId;
        }

        /// <summary>
        /// Update an Template record
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string Update( MyEntity entity )
        {
            string message = "successful";
            using ( SqlConnection conn = new SqlConnection( this.ConnString ) )
            {
                try
                {

                    #region parameters
                    //replace following with actual nbr of parameters and do assignments
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
                    sqlParameters[ 0 ].Size = 16;
                    sqlParameters[ 0 ].Value = entity.RowId;

                    //...

                    #endregion

                    SqlHelper.ExecuteNonQuery( conn, UPDATE_PROC, sqlParameters );
                    message = "successful";

                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + string.Format( ".Update() for RowId: {0} ", entity.RowId.ToString() ) );
                    //ex:
                    //LogError( ex, thisClassName + string.Format( ".Update() for orgId: {0} and userid: {1} and programId: {2} and contact type: {3}", entity.OrgId.ToString(), entity.UserId.ToString(), entity.ProgramId.ToString(), entity.ContactType ) );
                    message = thisClassName + "- Unsuccessful: Update(): " + ex.Message.ToString();
                    //entity.Message = message;
                    //entity.IsValid = false;
                }
            }
            return message;

        }//
        #endregion

        #region ====== Retrieval Methods ===============================================
        /// <summary>
        /// Get Template record via rowId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public MyEntity Get( string pRowId )
        {
            MyEntity entity = new MyEntity();
            using ( SqlConnection conn = new SqlConnection( ReadOnlyConnString ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
                    sqlParameters[ 0 ].Value = new Guid( pRowId );

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
        /// Select Template related data using passed parameters
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public DataSet Select( int pId, string parm2 )
        {

            //replace following with actual nbr of parameters and do assignments
            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );
            sqlParameters[ 1 ] = new SqlParameter( "@parm2", parm2 );
            using ( SqlConnection conn = new SqlConnection( ReadOnlyConnString ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, SELECT_PROC, sqlParameters );

                    if ( ds.HasErrors )
                    {
                        return null;
                    }
                    return ds;
                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".Select() " );
                    return null;

                }
            }
        }

        /// <summary>
        /// Search for Template related data using passed parameters
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
            //***** delete if not using custom paging *******
            string searchProcedure = "TBD";
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
            using ( SqlConnection conn = new SqlConnection( ReadOnlyConnString ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, searchProcedure, sqlParameters );

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
        /// Fill an Template object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>MyEntity</returns>
        public MyEntity Fill( SqlDataReader dr )
        {
            MyEntity entity = new MyEntity();

            entity.IsValid = true;
            string rowId = GetRowColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
                entity.RowId = new Guid( rowId );

            //entity.Title = GetRowColumn( dr, "Title", "missing" );
            //...
            entity.IsActive = GetRowColumn( dr, "IsActive", false );
            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            //Optional:		entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.CreatedBy = GetRowColumn( dr, "CreatedBy", "" );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
            //Optional:		entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            entity.LastUpdatedBy = GetRowColumn( dr, "LastUpdatedBy", "" );


            return entity;
        }//

        #endregion

    }
}