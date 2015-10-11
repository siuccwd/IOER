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
using Patron = ILPathways.Business.AppUser;
using PatronProfile = ILPathways.Business.PatronProfile;	//.AppUserProfile;
using ILPathways.Business;


namespace ILPathways.DAL
{
    /// <summary>
    /// Data access manager for User
    /// </summary>
    public class AccountManager : BaseDataManager
    {
        static string className = "AccountManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[PatronGet]";
        const string SELECT_PROC = "[PatronSelect]";
        const string DELETE_PROC = "[PatronDelete]";
        const string INSERT_PROC = "[PatronInsert]";
        const string UPDATE_PROC = "[PatronUpdate]";

        const string SEARCH_PROC = "[PatronSearch]";


        /// <summary>
        /// Default constructor
        /// </summary>
        public AccountManager()
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

        /// <summary>
        /// Add an User record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public string Create( Patron entity, ref string statusMessage )
        {
            string connectionString = GatewayConnection();
            string newId = "";

            try
            {

                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 8 ];

                sqlParameters[ 0 ] = new SqlParameter( "@UserName", SqlDbType.VarChar );
                sqlParameters[ 0 ].Value = entity.Username;
                sqlParameters[ 1 ] = new SqlParameter( "@Password", SqlDbType.VarChar );
                sqlParameters[ 1 ].Value = entity.Password;
                sqlParameters[ 2 ] = new SqlParameter( "@FirstName", SqlDbType.VarChar );
                sqlParameters[ 2 ].Value = entity.FirstName;
                sqlParameters[ 3 ] = new SqlParameter( "@LastName", SqlDbType.VarChar );
                sqlParameters[ 3 ].Value = entity.LastName;
                sqlParameters[ 4 ] = new SqlParameter( "@Email", SqlDbType.VarChar );
                sqlParameters[ 4 ].Value = entity.Email;
                sqlParameters[ 5 ] = new SqlParameter( "@SecretQuestionId", SqlDbType.Int );
                sqlParameters[ 5 ].Value = entity.SecretQuestionID;
                sqlParameters[ 6 ] = new SqlParameter( "@SecretAnswer", SqlDbType.VarChar );
                sqlParameters[ 6 ].Value = entity.SecretAnswer;
                sqlParameters[ 7 ] = new SqlParameter( "@workNetId", SqlDbType.Int );
                sqlParameters[ 7 ].Value = 0;



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
        public string Update( Patron entity )
        {
            string message = "successful";
            string connectionString = GatewayConnection();

            try
            {

                #region parameters
                //replace following with actual nbr of parameters and do assignments
                SqlParameter[] sqlParameters = new SqlParameter[ 9 ];
                sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
                sqlParameters[ 0 ].Value = entity.Id;
                sqlParameters[ 1 ] = new SqlParameter( "@UserName", SqlDbType.VarChar );
                sqlParameters[ 1 ].Value = entity.Username;
                sqlParameters[ 2 ] = new SqlParameter( "@Password", SqlDbType.VarChar );
                sqlParameters[ 2 ].Value = entity.Password;
                sqlParameters[ 3 ] = new SqlParameter( "@FirstName", SqlDbType.VarChar );
                sqlParameters[ 3 ].Value = entity.FirstName;
                sqlParameters[ 4 ] = new SqlParameter( "@LastName", SqlDbType.VarChar );
                sqlParameters[ 4 ].Value = entity.LastName;
                sqlParameters[ 5 ] = new SqlParameter( "@Email", SqlDbType.VarChar );
                sqlParameters[ 5 ].Value = entity.Email;
                sqlParameters[ 6 ] = new SqlParameter( "@SecretQuestionId", SqlDbType.Int );
                sqlParameters[ 6 ].Value = entity.SecretQuestionID;
                sqlParameters[ 7 ] = new SqlParameter( "@SecretAnswer", SqlDbType.VarChar );
                sqlParameters[ 7 ].Value = entity.SecretAnswer;
                sqlParameters[ 8 ] = new SqlParameter( "@workNetId", SqlDbType.Int );
                sqlParameters[ 8 ].Value = 0;

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
            return Get( pId, "", "", "" );
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
            return Get( 0, "", "", userName );

        }//
        /// <summary>
        /// Get User record via rowId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public Patron GetByRowId( string pRowId )
        {
            return Get( 0, pRowId, "", "" );

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
            return Get( 0, "", email, "" );

        }//


        private Patron Get( int id, string rowId, string email, string username )
        {
            string connectionString = GatewayConnectionRO();
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
            string connectionString = GatewayConnectionRO();
            Patron entity = new Patron();

            try
            {
                //temp
                DataSet ds = DatabaseManager.DoQuery( "SELECT * FROM [Patron] WHERE [workNetId] = '" + pworkNetId + "'" );
                if ( BaseDataManager.DoesDataSetHaveRows( ds ) )
                {

                    DataRow dr = ds.Tables[ 0 ].Rows[ 0 ];
                    //entity.Id = int.Parse( AccountManager.GetRowColumn( dr, "Id" ) );
                    int id = int.Parse( AccountManager.GetRowColumn( dr, "Id" ) );
                    //for integrity, should just do a get with the latter id!!!
                    entity = Get( id );
                    //entity.Username = AccountManager.GetRowColumn( dr, "UserName" );
                    //entity.FirstName = AccountManager.GetRowColumn( dr, "FirstName" );
                    //entity.LastName = AccountManager.GetRowColumn( dr, "LastName" );
                    //entity.Email = AccountManager.GetRowColumn( dr, "Email" );
                    //entity.SecretQuestionID = long.Parse( AccountManager.GetRowColumn( dr, "SecretQuestionId" ) );
                    //entity.SecretAnswer = AccountManager.GetRowColumn( dr, "SecretAnswer" );
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
            string connectionString = GatewayConnectionRO();
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
            string connectionString = GatewayConnectionRO();
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
            string connectionString = GatewayConnectionRO();
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
            string connectionString = GatewayConnectionRO();
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
            Patron entity = new Patron();

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
            // entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", DateTime.Now );
            //entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );

            return entity;
        }//

        public Patron Fill( DataRow dr )
        {
            Patron entity = FillLazy( dr );

            //future - could get org info, profile info

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
            entity.Username = GetRowColumn( dr, "UserName", "NoUser" );
            entity.FirstName = GetRowColumn( dr, "FirstName", "Null" );
            entity.LastName = GetRowColumn( dr, "LastName", "Null" );
            entity.Email = GetRowColumn( dr, "Email", "Null" );
            entity.SecretQuestionID = GetRowColumn( dr, "SecretQuestionId", 0 );
            entity.SecretAnswer = GetRowColumn( dr, "SecretAnswer", "Null" );
            entity.Created = GetRowColumn( dr, "Created", DateTime.Now );
            // entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", DateTime.Now );
            //entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );

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
            string connectionString = GatewayConnection();
            int count = 0;

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 5 ];

                sqlParameters[ 0 ] = new SqlParameter( "@UserId", entity.UserId );
                sqlParameters[ 1 ] = new SqlParameter( "@JobTitle", entity.JobTitle );
                sqlParameters[ 2 ] = new SqlParameter( "@PublishingRoleId", entity.PublishingRoleId );
                sqlParameters[ 3 ] = new SqlParameter( "@RoleProfile", entity.RoleProfile );
                sqlParameters[ 4 ] = new SqlParameter( "@OrganizationId", entity.OrganizationId );
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
            string connectionString = GatewayConnection();

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
                sqlParameters[ 0 ] = new SqlParameter( "@UserId", entity.UserId );
                sqlParameters[ 1 ] = new SqlParameter( "@JobTitle", entity.JobTitle );
                sqlParameters[ 2 ] = new SqlParameter( "@PublishingRoleId", entity.PublishingRoleId );
                sqlParameters[ 3 ] = new SqlParameter( "@RoleProfile", entity.RoleProfile );
                sqlParameters[ 4 ] = new SqlParameter( "@OrganizationId", entity.OrganizationId );

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
            string connectionString = GatewayConnectionRO();
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
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int ExternalAccount_Create( int userId, int externalSiteId, string loginName, string token, ref string statusMessage )
        {
            string connectionString = GatewayConnectionRO();
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

        #endregion


    }
}