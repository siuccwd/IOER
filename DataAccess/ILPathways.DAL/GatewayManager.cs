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
    public class GatewayManager : BaseDataManager
    {
        static string className = "GatewayManager";

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
        public GatewayManager()
        { }//

        #region ====== Core AppUser Methods ===============================================
        /// <summary>
        /// Delete an User record using rowId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( string pRowId, ref string statusMessage )
        {
            string connectionString = GatewayConnection();
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
            string connectionString = GatewayConnection();
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
        public string Create( AppUser entity, ref string statusMessage )
        {
            string connectionString = GatewayConnection();
            string newId = "";

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
                    newId = dr[ 0 ].ToString();
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
        public string Update( AppUser entity )
        {
            string message = "successful";
            string connectionString = GatewayConnection();

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

        /// <summary>
        /// Get User record via rowId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public AppUser Get( string pRowId )
        {
            string connectionString = GatewayConnectionRO();
            AppUser entity = new AppUser();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@RowId", pRowId );

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, GET_PROC, sqlParameters );

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

        }//
        public AppUser Get( int pId )
        {
            string connectionString = GatewayConnectionRO();
            AppUser entity = new AppUser();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, GET_PROC, sqlParameters );

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

        }//

        public AppUser Authorize( string userName, string password )
        {
            string connectionString = GatewayConnectionRO();
            AppUser entity = new AppUser();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@UserName", userName );
                sqlParameters[ 1 ] = new SqlParameter( "@Password", password );

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, "[PatronAuthorize]", sqlParameters );

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

        /// <summary>
        /// Select User related data using passed parameters
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public DataSet Select( int pId, string parm2 )
        {
            string connectionString = GatewayConnectionRO();

            //replace following with actual nbr of parameters and do assignments
            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );
            sqlParameters[ 1 ] = new SqlParameter( "@parm2", parm2 );

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, SELECT_PROC, sqlParameters );

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
        public DataSet Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, bool pOutputRelTables, ref int pTotalRows )
        {
            string connectionString = GatewayConnectionRO();
            int outputCol = 5;

            SqlParameter[] sqlParameters = new SqlParameter[ 6 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", pOrderBy );

            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", SqlDbType.Int );
            sqlParameters[ 2 ].Value = pStartPageIndex;

            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", SqlDbType.Int );
            sqlParameters[ 3 ].Value = pMaximumRows;
            sqlParameters[ 4 ] = new SqlParameter( "@OutputRelTables", pOutputRelTables );

            sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ outputCol ].Direction = ParameterDirection.Output;

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, SEARCH_PROC, sqlParameters );
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

        /// <summary>
        /// Fill an User object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>AppUser</returns>
        public AppUser Fill( SqlDataReader dr, bool includeRelatedData )
        {
            AppUser entity = new AppUser();

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

        #region ====== external accounts Methods ===============================================

        public AppUser DoesExternalAccountExist( string userName, string password )
        {
            string connectionString = GatewayConnectionRO();
            AppUser entity = new AppUser();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@UserName", userName );
                sqlParameters[ 1 ] = new SqlParameter( "@Password", password );

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, "[PatronAuthorize]", sqlParameters );

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

        #endregion


        #region App Groups
        /// <summary>
        /// Delete an AppGroup record
        /// </summary>
        /// <param name="id"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static bool AppGroupDelete( int id, ref string statusMessage )
        {
            string connectionString = GatewayConnection();
            bool result = true;
            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
            sqlParameters[ 0 ].Value = id;


            try
            {
                SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, "AppGroupDelete", sqlParameters );
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".AppGroupDelete()" );
                statusMessage = "Unsuccessful: GroupManager.AppGroupDelete(): " + ex.Message.ToString();
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
        public static int AppGroupCreate( AppGroup entity, ref string statusMessage )
        {
            string connectionString = GatewayConnection();
            int newId = 0;

            SqlParameter[] sqlParameters = new SqlParameter[ 7 ];
            //sqlParameters[ 0 ] = new SqlParameter( "@ApplicationId", entity.ApplicationId );
            sqlParameters[ 0 ] = new SqlParameter( "@GroupCode", entity.GroupCode );
            sqlParameters[ 1 ] = new SqlParameter( "@Title", entity.Title);
            sqlParameters[ 2 ] = new SqlParameter( "@Description",  entity.Description);

            sqlParameters[ 3 ] = new SqlParameter( "@IsActive", SqlDbType.Bit );
            sqlParameters[ 3 ].Value = entity.IsActive;
            
            sqlParameters[ 4 ] = new SqlParameter( "@ContactId", SqlDbType.Int );
            sqlParameters[ 4 ].Value = entity.ContactId;

            sqlParameters[ 5 ] = new SqlParameter( "@ParentGroupId", SqlDbType.Int );
            sqlParameters[ 5 ].Value = entity.ParentGroupId;

            sqlParameters[ 6 ] = new SqlParameter( "@CreatedById", entity.CreatedById);



            try
            {
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, "GroupInsert", sqlParameters );
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
                LogError( ex, className + ".AppGroupCreate() " );
                statusMessage = "Unsuccessful: " + className + ".AppGroupCreate(): " + ex.Message.ToString();
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
        public static string AppGroupUpdate( AppGroup entity )
        {
            string connectionString = GatewayConnection();
            string message = "successful";

            SqlParameter[] sqlParameters = new SqlParameter[ 7 ];
            sqlParameters[ 0 ] = new SqlParameter( "@id", entity.Id);
            sqlParameters[ 1 ] = new SqlParameter( "@Title",  entity.Title);
            sqlParameters[ 2 ] = new SqlParameter( "@Description", entity.Description);
            sqlParameters[ 3 ] = new SqlParameter( "@IsActive", SqlDbType.Bit );
            sqlParameters[ 3 ].Value = entity.IsActive;
            sqlParameters[ 4 ] = new SqlParameter( "@ContactId", SqlDbType.Int );
            sqlParameters[ 4 ].Value = entity.ContactId;
            sqlParameters[ 5 ] = new SqlParameter( "@ParentGroupId", SqlDbType.Int );
            sqlParameters[ 5 ].Value = entity.ParentGroupId;

            sqlParameters[ 6 ] = new SqlParameter( "@LastUpdatedById", entity.LastUpdatedById);


            try
            {
                SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, "AppGroupUpdate", sqlParameters );
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".AppGroupUpdate()" );
                message = "Unsuccessful: " + className + ".AppGroupUpdate(): " + ex.Message.ToString();

                entity.Message = message;
            }

            return message;
        } //

        /// <summary>
        /// Retrieve a group by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static AppGroup AppGroupGet( int id )
        {
            string groupCode = "";
            if ( id > 0 )
            {
                return AppGroupGet( id, groupCode );
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
        public static AppGroup AppGroupGetByCode( string groupCode )
        {
            int id = 0;
            if ( groupCode.Trim().Length > 0 )
            {
                return AppGroupGet( id, groupCode );
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
        private static AppGroup AppGroupGet( int id, string groupCode )
        {
            AppGroup entity = new AppGroup();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@id", id );
                sqlParameters[ 1 ] = new SqlParameter( "@GroupCode", groupCode );

                //TODO - change procedure to handle GroupSession??
                SqlDataReader dr = SqlHelper.ExecuteReader( GatewayConnection(), "AppGroupGet", sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.

                    while ( dr.Read() )
                    {
                        entity = AppGroupFill( dr );
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
        private static AppGroup AppGroupFill( SqlDataReader dr )
        {
            AppGroup entity = new AppGroup();

            entity.Id = GetRowColumn( dr, "id", 0 );

            entity.GroupCode = dr[ "GroupCode" ].ToString();
            entity.Title = dr[ "Title" ].ToString();
            entity.Description = GetRowColumn( dr, "description", "" );

            entity.IsActive = GetRowColumn( dr, "isActive", false );
            entity.Created = GetRowColumn( dr, "created", System.DateTime.MinValue );
            entity.LastUpdated = GetRowColumn( dr, "lastUpdated", System.DateTime.MinValue );

            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            //entity.CreatedBy = GetRowColumn( dr, "createdBy", "" );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            //entity.LastUpdatedBy = GetRowColumn( dr, "lastUpdatedBy", "" );

            entity.ContactId = GetRowColumn( dr, "ContactId", 0 );
            entity.ParentGroupId = GetRowColumn( dr, "ParentGroupId", 0 );


            return entity;
        }//		
        #endregion

    }
}