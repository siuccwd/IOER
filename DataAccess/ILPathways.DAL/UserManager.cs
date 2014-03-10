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
using MyEntity = ILPathways.Business.AppUser;


namespace ILPathways.DAL
{
    /// <summary>
    /// Data access manager for User
    /// </summary>
    public class UserManager : BaseDataManager
    {
        static string className = "UserManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[User_Get]";
        const string SELECT_PROC = "[UserSelect]";
        const string DELETE_PROC = "[UserDelete]";
        const string INSERT_PROC = "[UserInsert]";
        const string UPDATE_PROC = "[UserUpdate]";

        const string SEARCH_PROC = "[User_Search]";


        /// <summary>
        /// Default constructor
        /// </summary>
        public UserManager()
        { }//

        #region ====== Core Methods ===============================================
        /// <summary>
        /// Delete an User record using rowId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( string pRowId, ref string statusMessage )
        {
            string connectionString = ILPathwaysConnection();
            bool successful;


            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
                sqlParameters[ 0 ].Value = new Guid( pRowId );

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
        public bool Delete( int pId, ref string statusMessage )
        {
            string connectionString = ILPathwaysConnection();
            bool successful;


            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", pId);
                //TBD
                //SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
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
        /// Add an User record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int Create( MyEntity entity, ref string statusMessage )
        {
            string connectionString = ILPathwaysConnection();
            int newId = 0;

            try
            {

                #region parameters
                //replace following with actual nbr of parameters and do assignments
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
                sqlParameters[ 0 ].Value = entity.Id;

                //...


                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = Int32.Parse( dr[ 0 ].ToString());
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".Create() for RowId: {0} and CreatedBy: {1}", entity.RowId.ToString(), entity.CreatedBy ) );
                statusMessage = className + "- Unsuccessful: Create(): " + ex.Message.ToString();
                entity.Message = statusMessage;
                entity.IsValid = false;
            }

            return newId;
        }

        /// <summary>
        /// Update an User record
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string Update( MyEntity entity )
        {
            string message = "successful";
            string connectionString = ILPathwaysConnection();

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

                SqlHelper.ExecuteNonQuery( connectionString, UPDATE_PROC, sqlParameters );
                message = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".Update() for RowId: {0} and LastUpdatedBy: {1}", entity.RowId.ToString(), entity.LastUpdatedBy ) );
                //ex:
                //LogError( ex, className + string.Format( ".Update() for orgId: {0} and userid: {1} and programId: {2} and contact type: {3}", entity.OrgId.ToString(), entity.UserId.ToString(), entity.ProgramId.ToString(), entity.ContactType ) );
                message = className + "- Unsuccessful: Update(): " + ex.Message.ToString();
                entity.Message = message;
                entity.IsValid = false;
            }

            return message;

        }//
        #endregion

        #region ====== Retrieval Methods ===============================================
        /// <summary>
        /// Get User record via rowId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public MyEntity Get( string pRowId )
        {
            
            MyEntity entity = new MyEntity();
            using ( SqlConnection conn = new SqlConnection( ILPathwaysConnectionRO() ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@RowId", pRowId );

                    SqlDataReader dr = SqlHelper.ExecuteReader( conn, GET_PROC, sqlParameters );

                    if ( dr.HasRows )
                    {
                        // it should return only one record.
                        while ( dr.Read() )
                        {
                            entity = Fill( dr, false );
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
                    entity.Message = "Unsuccessful: " + className + ".Get(): " + ex.Message.ToString();
                    entity.IsValid = false;
                    return entity;

                }
            }

        }//
        public MyEntity Get( int pId )
        {
            
            MyEntity entity = new MyEntity();
            using ( SqlConnection conn = new SqlConnection( ILPathwaysConnectionRO() ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );

                    SqlDataReader dr = SqlHelper.ExecuteReader( conn, GET_PROC, sqlParameters );

                    if ( dr.HasRows )
                    {
                        // it should return only one record.
                        while ( dr.Read() )
                        {
                            entity = Fill( dr, false );
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
                    entity.Message = "Unsuccessful: " + className + ".Get(): " + ex.Message.ToString();
                    entity.IsValid = false;
                    return entity;

                }
            }

        }//

        public MyEntity Authorize( string userName, string password )
        {

            MyEntity entity = new MyEntity();
            using ( SqlConnection conn = new SqlConnection( ILPathwaysConnectionRO() ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@UserName", userName );
                    sqlParameters[ 1 ] = new SqlParameter( "@Password", password );

                    SqlDataReader dr = SqlHelper.ExecuteReader( conn, "[PatronAuthorize]", sqlParameters );

                    if ( dr.HasRows )
                    {
                        // it should return only one record.
                        while ( dr.Read() )
                        {
                            entity = Fill( dr, false );
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
                    //entity.Message = "Unsuccessful: " + className + ".Get(): " + ex.Message.ToString();
                    entity.Message = "Error: " + ex.Message.ToString();
                    entity.IsValid = false;
                    return entity;

                }
            }
        }


        /// <summary>
        /// Select User related data using passed parameters
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public DataSet Select( )
        {
            int pId = 0;
            string parm2 = "";
            return Select( pId, parm2 );
        }


        /// <summary>
        /// Select User related data using passed parameters
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public DataSet Select( int pId, string parm2 )
        {
            

            //replace following with actual nbr of parameters and do assignments
            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );
            sqlParameters[ 1 ] = new SqlParameter( "@parm2", parm2 );
            using ( SqlConnection conn = new SqlConnection( ILPathwaysConnectionRO() ) )
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
                    LogError( ex, className + ".Select() " );
                    return null;

                }
            }
        }

        /// <summary>
        /// Search for User related data using passed parameters
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

            SqlParameter[] sqlParameters = new SqlParameter[ 6 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", pOrderBy );

            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", SqlDbType.Int );
            sqlParameters[ 2 ].Value = pStartPageIndex;

            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", SqlDbType.Int );
            sqlParameters[ 3 ].Value = pMaximumRows;

            sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ outputCol ].Direction = ParameterDirection.Output;
            using ( SqlConnection conn = new SqlConnection( ILPathwaysConnectionRO() ) )
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
                    LogError( ex, className + ".Search() " );
                    return null;

                }
            }
        }

        #endregion

        #region ====== Helper Methods ===============================================
        /// <summary>
        /// Fill an User object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>MyEntity</returns>
        public MyEntity Fill( SqlDataReader dr, bool includeRelatedData )
        {
            MyEntity entity = new MyEntity();

            entity.IsValid = true;

            string rowId = GetRowColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
                entity.RowId = new Guid( rowId );

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.Username = GetRowColumn( dr, "UserName", "NoUser" );
            entity.FirstName = GetRowColumn( dr, "FirstName", "Null" );
            entity.LastName = GetRowColumn( dr, "LastName", "Null" );
            entity.Email = GetRowColumn( dr, "Email", "Null" );
            entity.SecretQuestionID = GetRowColumn( dr, "SecretQuestionId", 0 );
            entity.SecretAnswer = GetRowColumn( dr, "SecretAnswer", "Null" );
            entity.Created = GetRowColumn( dr, "Created", DateTime.Now );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", DateTime.Now );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );

            return entity;
        }//

        #endregion

    }
}