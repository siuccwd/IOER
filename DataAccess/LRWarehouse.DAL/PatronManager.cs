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
using LRWarehouse.Business;
using LRWarehouse.DAL;
using ILPathways.Business;


namespace LRWarehouse.DAL
{
    /// <summary>
    /// Data access manager for User
    /// </summary>
    public class PatronManager : BaseDataManager
    {
        static string className = "PatronManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[PatronGet]";
        const string DELETE_PROC = "[PatronDelete]";
        const string INSERT_PROC = "[PatronInsert]";
        const string UPDATE_PROC = "[PatronUpdate]";

        const string SEARCH_PROC = "[PatronSearch]";


        /// <summary>
        /// Default constructor
        /// </summary>
        public PatronManager()
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
            string connectionString = LRWarehouse();
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

        /// <summary>
        /// Add an User record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int Create( Patron entity, ref string statusMessage )
        {
            string connectionString = LRWarehouse();
            int newId = 0;

            try
            {
                if ( entity.UserName == null || entity.UserName.Trim().Length == 0 )
                    entity.UserName = entity.Email;
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 7 ];

                sqlParameters[ 0 ] = new SqlParameter( "@UserName", entity.UserName );
                sqlParameters[ 1 ] = new SqlParameter( "@Password", entity.Password );
                sqlParameters[ 2 ] = new SqlParameter( "@FirstName", entity.FirstName);
                sqlParameters[ 3 ] = new SqlParameter( "@LastName", entity.LastName);
                sqlParameters[ 4 ] = new SqlParameter( "@Email", entity.Email);

                sqlParameters[ 5 ] = new SqlParameter( "@IsActive", entity.IsActive );
				sqlParameters[ 6 ] = new SqlParameter( "@Identifier", entity.ExternalIdentifier );
                
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = int.Parse( dr[ 0 ].ToString() );
                    entity.Id = newId;
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".Create() for Email: {0} and CreatedBy: {1}", entity.Email.ToString(), entity.CreatedBy ) );
                statusMessage = className + "- Unsuccessful: Create(): " + ex.Message.ToString();
                entity.Message = statusMessage;
                entity.IsValid = false;
            }

            return newId;
        }

        /// <summary>
        /// Update an User record
        /// NOTE: the update proc checks for an empty password. If found, it will read the current password and use in the update statement
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string Update( Patron entity )
        {
            string message = "successful";
            string connectionString = LRWarehouse();

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 6 ];
                sqlParameters[ 0 ] = new SqlParameter( "@id", entity.Id );
                sqlParameters[ 1 ] = new SqlParameter( "@Password", entity.Password );
                sqlParameters[ 2 ] = new SqlParameter( "@FirstName", entity.FirstName );
                sqlParameters[ 3 ] = new SqlParameter( "@LastName", entity.LastName );
                sqlParameters[ 4 ] = new SqlParameter( "@Email", entity.Email );
                sqlParameters[ 5 ] = new SqlParameter( "@IsActive", entity.IsActive );
                #endregion

                SqlHelper.ExecuteNonQuery( connectionString, UPDATE_PROC, sqlParameters );
                message = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".Update() for Id: {0} and LastUpdatedBy: {1}", entity.Id.ToString(), entity.LastUpdatedBy ) );
                message = className + "- Unsuccessful: Update(): " + ex.Message.ToString();
                entity.Message = message;
                entity.IsValid = false;
            }

            return message;

        }//
        #endregion

        #region ====== Retrieval Methods ===============================================
        /// <summary>
        /// Get User record via integer id
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public Patron Get( int pId )
        {
            if ( pId > 0 )
                return Get( pId, "", "", "" );
            else
                return new Patron();
        }//
        /// <summary>
        /// Get patron by username
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>

        /// <summary>
        /// Check if a username exists
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool DoesUserNameExist( string userName )
        {
            Patron entity = GetByUsername( userName );
            if ( entity.IsValid && entity.Id > 0 )
                return true;
            else
                return false;

        }//
        public Patron GetByUsername( string userName )
        {
            if ( userName.Trim().Length > 3 )
                return Get( 0, "", "", userName );
            else
                return new Patron();

        }//


        /// <summary>
        /// Get User record via rowId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public Patron GetByRowId( string pRowId )
        {
            if ( pRowId.Length == 36 && pRowId != DEFAULT_GUID )
                return Get( 0, pRowId, "", "" );
            else
                return new Patron();

        }//

        /// <summary>
        /// Check if an email already is associated with an account
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool DoesUserEmailExist( string email )
        {
            Patron entity = Get( 0, "", email, "" );
            if ( entity.IsValid && entity.Id > 0 )
                return true;
            else
                return false;

        }//
        /// <summary>
        /// Get User record via email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Patron GetByEmail( string email )
        {

            if ( email.Trim().Length > 5 && email.IndexOf( "@" ) > 0 )
                return Get( 0, "", email, "" );
            else
                return new Patron();

        }//


        private Patron Get( int id, string rowId, string email, string username )
        {
            string connectionString = LRWarehouseRO();
            Patron entity = new Patron();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", id );
                sqlParameters[ 1 ] = new SqlParameter( "@UserName", username );
                sqlParameters[ 2 ] = new SqlParameter( "@Email", email );
                sqlParameters[ 3 ] = new SqlParameter( "@RowId", rowId );

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
                entity.Message = "Unsuccessful: " + className + ".Get(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//

        public Patron GetByWorkNetId( int pworkNetId )
        {
            string connectionString = LRWarehouseRO();
            Patron entity = new Patron();

            try
            {
                //temp
                DataSet ds = DatabaseManager.DoQuery( "SELECT * FROM [Patron] WHERE [workNetId] = '" + pworkNetId + "'" );
                if ( BaseDataManager.DoesDataSetHaveRows( ds ) )
                {

                    DataRow dr = ds.Tables[ 0 ].Rows[ 0 ];
                    //entity.Id = int.Parse( PatronManager.GetRowColumn( dr, "Id" ) );
                    int id = int.Parse( PatronManager.GetRowColumn( dr, "Id" ) );
                    //for integrity, should just do a get with the latter id!!!
                    entity = Get( id );
                    //entity.Username = PatronManager.GetRowColumn( dr, "UserName" );
                    //entity.FirstName = PatronManager.GetRowColumn( dr, "FirstName" );
                    //entity.LastName = PatronManager.GetRowColumn( dr, "LastName" );
                    //entity.Email = PatronManager.GetRowColumn( dr, "Email" );
                    //entity.SecretQuestionID = long.Parse( PatronManager.GetRowColumn( dr, "SecretQuestionId" ) );
                    //entity.SecretAnswer = PatronManager.GetRowColumn( dr, "SecretAnswer" );
                    //entity.worknetId = pworkNetId;
                    //entity.IsValid = true;

                }
                else
                {
                    entity.IsValid = false;
                }
                return entity;

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".GetByWorkNetId() " );
                entity.Message = "Unsuccessful: " + className + ".GetByWorkNetId(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }
        }//

        public Patron GetByWorkNetCredentials( string loginId, string token )
        {
            return GetByExtSiteCredentials( 1, loginId, token );
        }//

        public Patron GetByExtSiteCredentials( int externalSiteId, string loginId, string token )
        {
            string connectionString = LRWarehouseRO();
            Patron entity = new Patron();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ExternalSiteId", externalSiteId );
                sqlParameters[ 1 ] = new SqlParameter( "@LoginId", loginId );
                sqlParameters[ 2 ] = new SqlParameter( "@Token", token );

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, "[Patron.GetByExtAccount]", sqlParameters );

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
                entity.Message = "Unsuccessful: " + className + ".Get(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//

        public Patron Authorize( string userName, string password )
        {
            string connectionString = LRWarehouseRO();
            Patron entity = new Patron();

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
                LogError( ex, className + ".Authorize() " );
                //entity.Message = "Unsuccessful: " + className + ".Get(): " + ex.Message.ToString();
                entity.Message = "Error: " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }
        }


        public Patron RecoverPassword( string lookup )
        {
            string connectionString = LRWarehouseRO();
            Patron entity = new Patron();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Lookup", lookup );

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, "[Patron.RecoverPassword]", sqlParameters );

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
                LogError( ex, className + ".RecoverPassword() " );
                //entity.Message = "Unsuccessful: " + className + ".Get(): " + ex.Message.ToString();
                entity.Message = "Error: " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

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
        public List<Patron> Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            List<Patron> list = new List<Patron>();
            DataSet ds = new DataSet();

            try
            {
                ds = UserSearch( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );

                if ( DoesDataSetHaveRows( ds ) == false || ds.HasErrors )
                {
                    return null;
                }
                else
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
                    {
                        Patron row = Fill( dr );

                        list.Add( row );

                    } //end foreach

                    return list;
                }
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Search() " );
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
        public DataSet UserSearch( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            string connectionString = LRWarehouseRO();
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
                LogError( ex, className + ".UserSearch() " );
                return null;

            }
        }

        /// <summary>
        /// Fill an User object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>Patron</returns>
        public Patron Fill( SqlDataReader dr )
        {
            Patron entity = FillLazy( dr );

            entity.UserProfile = PatronProfile_Get( entity.Id );
            if ( entity.UserProfile != null && entity.UserProfile.UserId > 0 )
                entity.OrgId = entity.UserProfile.OrganizationId;

            return entity;
        }//
        public Patron FillLazy( SqlDataReader dr )
        {
            Patron entity = new Patron();

            entity.IsValid = true;

            string rowId = GetRowColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
                entity.RowId = new Guid( rowId );

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.UserName = GetRowColumn( dr, "UserName", "NoUser" );
            entity.IsActive = GetRowColumn( dr, "IsActive", true );

            //Note update proc handles a null password
            entity.Password = GetRowColumn( dr, "Password", "" );
            entity.FirstName = GetRowColumn( dr, "FirstName", "Null" );
            entity.LastName = GetRowColumn( dr, "LastName", "Null" );
            entity.Email = GetRowColumn( dr, "Email", "Null" );
            //entity.SecretQuestionID = GetRowColumn( dr, "SecretQuestionId", 0 );
            //entity.SecretAnswer = GetRowColumn( dr, "SecretAnswer", "Null" );
            entity.Created = GetRowColumn( dr, "Created", DateTime.Now );
            // entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", DateTime.Now );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );

            return entity;
        }//

        public Patron Fill( DataRow dr )
        {
            Patron entity = FillLazy( dr );

            //future - could get org info, profile info
            entity.UserProfile = PatronProfile_Get( entity.Id );
            if ( entity.UserProfile != null && entity.UserProfile.UserId > 0 )
                entity.OrgId = entity.UserProfile.OrganizationId;

            return entity;
        }//

        public Patron FillLazy( DataRow dr )
        {
            Patron entity = new Patron();

            entity.IsValid = true;

            string rowId = GetRowColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
                entity.RowId = new Guid( rowId );

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.UserName = GetRowColumn( dr, "UserName", "NoUser" );
            entity.IsActive = GetRowColumn( dr, "IsActive", false );

            entity.Password = GetRowColumn( dr, "Password", "" );
            entity.FirstName = GetRowColumn( dr, "FirstName", "Null" );
            entity.LastName = GetRowColumn( dr, "LastName", "Null" );
            entity.Email = GetRowColumn( dr, "Email", "Null" );
            //entity.SecretQuestionID = GetRowColumn( dr, "SecretQuestionId", 0 );
            //entity.SecretAnswer = GetRowColumn( dr, "SecretAnswer", "Null" );
            entity.Created = GetRowColumn( dr, "Created", DateTime.Now );
            // entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", DateTime.Now );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );

            return entity;
        }//

        #endregion

        #region ====== Patron.Profile Methods ============================================

        /// <summary>
        /// Add an Profile record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int PatronProfile_Create( PatronProfile entity, ref string statusMessage )
        {
            string connectionString = LRWarehouse();
            int count = 0;

            try
            {
                //first ensure profile doesn't already exist
                PatronProfile prof = PatronProfile_Get( entity.UserId );
                if ( prof != null && prof.UserId > 0 )
                {
                    statusMessage = PatronProfile_Update( entity );
                    return 1;
                }

                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 6 ];

                sqlParameters[ 0 ] = new SqlParameter( "@UserId", entity.UserId );
                sqlParameters[ 1 ] = new SqlParameter( "@JobTitle", entity.JobTitle );
                sqlParameters[ 2 ] = new SqlParameter( "@PublishingRoleId", entity.PublishingRoleId );
                sqlParameters[ 3 ] = new SqlParameter( "@RoleProfile", entity.RoleProfile );
                sqlParameters[ 4 ] = new SqlParameter( "@OrganizationId", entity.OrganizationId );
                sqlParameters[ 5 ] = new SqlParameter( "@ImageUrl", entity.ImageUrl );
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, "[Patron.ProfileInsert]", sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    Int32.TryParse( dr[ 0 ].ToString(), out count );
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".PatronProfile_Create() for UserId: {0} and CreatedBy: {1}", entity.UserId.ToString(), entity.CreatedBy ) );
                statusMessage = className + "- Unsuccessful: PatronProfile_Create(): " + ex.Message.ToString();
                entity.Message = statusMessage;
                entity.IsValid = false;
            }

            return count;
        }

        /// <summary>
        /// Update an User record
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string PatronProfile_Update( PatronProfile entity )
        {
            string message = "successful";
            string connectionString = LRWarehouse();

            try
            {
                //first ensure profile does already exist
                PatronProfile prof = PatronProfile_Get( entity.UserId );
                if ( prof == null || prof.UserId == 0 )
                {
                    string statusMessage = "";
                    int count = PatronProfile_Create( entity, ref statusMessage );
                    return statusMessage;
                }
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 6 ];
                sqlParameters[ 0 ] = new SqlParameter( "@UserId", entity.UserId );
                sqlParameters[ 1 ] = new SqlParameter( "@JobTitle", entity.JobTitle );
                sqlParameters[ 2 ] = new SqlParameter( "@PublishingRoleId", entity.PublishingRoleId );
                sqlParameters[ 3 ] = new SqlParameter( "@RoleProfile", entity.RoleProfile );
                sqlParameters[ 4 ] = new SqlParameter( "@OrganizationId", entity.OrganizationId );
                sqlParameters[ 5 ] = new SqlParameter( "@ImageUrl", entity.ImageUrl );
                #endregion

                SqlHelper.ExecuteNonQuery( connectionString, "[Patron.ProfileUpdate]", sqlParameters );
                message = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".PatronProfile_Update() for UserId: {0} and LastUpdatedBy: {1}", entity.UserId.ToString(), entity.LastUpdatedBy ) );
                message = className + "- Unsuccessful: PatronProfile_Update(): " + ex.Message.ToString();
                entity.Message = message;
                entity.IsValid = false;
            }

            return message;

        }//
        public PatronProfile PatronProfile_Get( int pUserId )
        {
            string connectionString = LRWarehouseRO();
            PatronProfile entity = new PatronProfile();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@UserId", pUserId );

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, "[Patron.ProfileGet]", sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.
                    while ( dr.Read() )
                    {
                        entity = PatronProfile_Fill( dr );
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
                LogError( ex, className + ".PatronProfile_Get() " );
                entity.Message = "Unsuccessful: " + className + ".PatronProfile_Get(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//
        public PatronProfile PatronProfile_Fill( SqlDataReader dr )
        {
            PatronProfile entity = new PatronProfile();

            entity.IsValid = true;

            entity.UserId = GetRowColumn( dr, "UserId", 0 );
            entity.JobTitle = GetRowColumn( dr, "JobTitle", "" );

            entity.OrganizationId = GetRowColumn( dr, "OrganizationId", 0 );
            entity.Organization = GetRowColumn( dr, "Organization", "" );

            entity.PublishingRoleId = GetRowColumn( dr, "PublishingRoleId", 0 );
            entity.PublishingRole = GetRowColumn( dr, "PublishingRole", "" );
            entity.ImageUrl = GetRowColumn( dr, "ImageUrl", "" );
            entity.RoleProfile = GetRowColumn( dr, "RoleProfile", "" );
            entity.Created = GetRowColumn( dr, "Created", DateTime.Now );
            // entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", DateTime.Now );
            //entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );

            return entity;
        }//

        #endregion

        #region ====== Patron.ExternalAccount Methods ============================================
        /// <summary>
        /// Add an User record
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="externalLoginProvider"></param>
        /// <param name="loginName"></param>
        /// <param name="token"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int ExternalAccount_Create(int userId, string externalLoginProvider, string loginName, string token, ref string statusMessage)
        {
            int id = 0;
            DataSet ds = CodeTableManager.SelectCodesExternalSite(externalLoginProvider, 0, ref statusMessage);
            if (DoesDataSetHaveRows(ds))
            {
                id = GetRowColumn(ds.Tables[0].Rows[0], "Id", 0);
            }

            return ExternalAccount_Create(userId, id, loginName, token, ref statusMessage);
        }
        /// <summary>
        /// Add an User record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int ExternalAccount_Create( int userId, int externalSiteId, string loginName, string token, ref string statusMessage )
        {
            string connectionString = LRWarehouseRO();
            int newId = 0;

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
                sqlParameters[ 0 ] = new SqlParameter( "@PatronId", userId );
                sqlParameters[ 1 ] = new SqlParameter( "@ExternalSiteId", externalSiteId );
                sqlParameters[ 2 ] = new SqlParameter( "@LoginId", loginName );
                sqlParameters[ 3 ] = new SqlParameter( "@Token", token );
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, "[Patron.ExternalAccountInsert]", sqlParameters );
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
                LogError( ex, className + string.Format( ".ExternalAccount_Create() for UserId: {0} and SiteId: {1}", userId, externalSiteId ) );
                statusMessage = className + "- Unsuccessful: ExternalAccount_Create(): " + ex.Message.ToString();
            }

            return newId;
        }

        public PatronExternalAccount ExternalAccount_Get(int id, int userId, int externalSiteId, string externalLoginProvider, string token, ref string statusMessage)
        {
            string connectionString = LRWarehouse();
            statusMessage = "successful";
            PatronExternalAccount retVal = null;
            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[5];
                sqlParameters[0] = new SqlParameter("@Id", id);
                sqlParameters[1] = new SqlParameter("@PatronId", userId);
                sqlParameters[2] = new SqlParameter("@ExternalSiteId", externalSiteId);
                sqlParameters[3] = new SqlParameter("@Token", token);
                sqlParameters[4] = new SqlParameter("@ExternalLoginProvider", externalLoginProvider);
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, "[Patron.ExternalAccountGet]", sqlParameters);
                if (dr.HasRows)
                {
                    retVal = ExternalAccountFill(dr);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, className + string.Format(".ExternalAccount_Get() for Id {0}, userId {1}, ExternalSiteId {2}, Token {3}, and LoginProvider {4}",
                    id, userId, externalSiteId, token, externalLoginProvider));
                statusMessage = className + "- Unsuccessful: ExternalAccount_Get(): " + ex.Message.ToString();
            }

            return retVal;
        }

        public PatronExternalAccount ExternalAccountFill(SqlDataReader dr)
        {
            dr.Read();
            PatronExternalAccount retVal = new PatronExternalAccount
            {
                PatronId = GetRowColumn(dr, "PatronId", 0),
                Id = GetRowColumn(dr, "Id", 0),
                ExternalSiteId = GetRowColumn(dr, "ExternalSiteId", 0),
                LoginId = GetRowPossibleColumn(dr, "LoginId", ""),
                Password = GetRowPossibleColumn(dr, "Password", ""),
                Token = GetRowColumn(dr, "Token", ""),
            };

            dr.Close();
            return retVal;
        }

        public PatronExternalAccount ExternalAccountFill(DataSet ds)
        {
            DataRow dr = ds.Tables[0].Rows[0];
            PatronExternalAccount retVal = new PatronExternalAccount
            {
                PatronId = GetRowColumn(dr, "PatronId", 0),
                Id = GetRowColumn(dr, "Id", 0),
                ExternalSiteId = GetRowColumn(dr, "ExternalSiteId", 0),
                LoginId = GetRowColumn(dr, "LoginId", ""),
                Password = GetRowColumn(dr, "Password", ""),
                Token = GetRowColumn(dr, "Token", ""),
            };

            return retVal;
        }

        #endregion


    }
}