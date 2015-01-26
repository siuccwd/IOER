using System;
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
    /// Data access manager for LibrarySubScription
    /// </summary>
    public class LibrarySubScriptionManager : BaseDataManager
    {
        static string thisClassName = "LibrarySubScriptionManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[Library.SubscriptionGet]";
        const string SELECT_PROC = "[Library.SubscriptionSelect]";
        const string DELETE_PROC = "[Library.SubscriptionDelete]";
        const string INSERT_PROC = "[Library.SubscriptionInsert]";
        const string UPDATE_PROC = "[Library.SubscriptionUpdate]";


        /// <summary>
        /// Default constructor
        /// </summary>
        public LibrarySubScriptionManager()
        { }//

        #region ====== Core Methods ===============================================
        /// <summary>
        /// Delete a Library.Subscription record
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( int pId, ref string statusMessage )
        {
            bool successful = false;
            using ( SqlConnection conn = new SqlConnection( ContentConnection() ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@Id", pId);
					sqlParameters[ 1 ] = new SqlParameter( "@LibraryId", 0 );
					sqlParameters[ 2 ] = new SqlParameter( "@UserId", 0 );

                    SqlHelper.ExecuteNonQuery( conn, CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
                    successful = true;
                }
                catch ( Exception ex )
                {
					LogError( ex, thisClassName + ".Delete(int pId) " );
					statusMessage = thisClassName + "- Unsuccessful: Delete(int pId): " + ex.Message.ToString();

                    successful = false;
                }
            }
            return successful;

        }//

		public bool Delete( int pLibraryId, int pUserId, ref string statusMessage )
		{
			bool successful = false;
			using ( SqlConnection conn = new SqlConnection( ContentConnection() ) )
			{
				try
				{
					SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
					sqlParameters[ 0 ] = new SqlParameter( "@Id", 0 );
					sqlParameters[ 1 ] = new SqlParameter( "@LibraryId", pLibraryId );
					sqlParameters[ 2 ] = new SqlParameter( "@UserId", pUserId );

					SqlHelper.ExecuteNonQuery( conn, CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
					successful = true;
				}
				catch ( Exception ex )
				{
					LogError( ex, thisClassName + ".Delete(int pLibraryId, int pUserId) " );
					statusMessage = thisClassName + "- Unsuccessful: Delete(int pLibraryId, int pUserId): " + ex.Message.ToString();

					successful = false;
				}
			}
			return successful;

		}//
		 
        /// <summary>
        /// Add a Library.Subscription record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int Create( int libraryId, int userId, int subscriptionTypeId, ref string statusMessage )
        {
            int newId = 0;
            using ( SqlConnection conn = new SqlConnection( ContentConnection() ) )
            {
                try
                {
                    #region parameters
                    SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@LibraryId", libraryId );
                    sqlParameters[ 1 ] = new SqlParameter( "@UserId", userId );
                    sqlParameters[ 2 ] = new SqlParameter( "@SubscriptionTypeId", subscriptionTypeId );

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
                    LogError( ex, thisClassName + string.Format( ".Create() for LibraryId: {0} and userId: {1}", libraryId, userId ) );
                    statusMessage = thisClassName + "- Unsuccessful: Create(): " + ex.Message.ToString();

                }
            }
            return newId;
        }

        /// <summary>
        /// Update a Library.Subscription record
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string Update( int id, int subscriptionTypeId )
        {
            string message = "successful";
            using ( SqlConnection conn = new SqlConnection( ContentConnection() ) )
            {
                try
                {
                    #region parameters
                    SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@id", id);
                    sqlParameters[ 1 ] = new SqlParameter( "@SubscriptionTypeId", subscriptionTypeId );

                    #endregion

                    SqlHelper.ExecuteNonQuery( conn, UPDATE_PROC, sqlParameters );
                    message = "successful";

                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + string.Format( ".Update() for Id: {0} ", id ) );
                    message = thisClassName + "- Unsuccessful: Update(): " + ex.Message.ToString();
                }
            }
            return message;

        }//
       
        /// <summary>
        /// Get Library.Subscription record
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public ObjectSubscription Get( int pId )
        {
            return Get( pId, 0, 0);

        }//
        /// <summary>
        /// Get Library.Subscription record
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ObjectSubscription Get( int libraryId, int userId )
        {
            return Get( 0, libraryId, userId );
        }//
        private ObjectSubscription Get( int pId, int pLibraryId, int pUserId )
        {
            ObjectSubscription entity = new ObjectSubscription();
            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );
                    sqlParameters[ 1 ] = new SqlParameter( "@LibraryId", pLibraryId );
                    sqlParameters[ 2 ] = new SqlParameter( "@UserId", pUserId );

                    SqlDataReader dr = SqlHelper.ExecuteReader( conn, GET_PROC, sqlParameters );

                    if ( dr.HasRows )
                    {
                        // it should return only one record.
                        while ( dr.Read() )
                        {
                            entity = Fill( dr );
                            entity.SubscriptionCategory = "LibrarySubcription";
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

        #endregion

        #region ====== Library collection subscription Methods ===============================================
        /// <summary>
        /// Delete a Library.SectionSubscription record
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool CollectionSubcriptionDelete( int pId, ref string statusMessage )
        {
            bool successful = false;
            using ( SqlConnection conn = new SqlConnection( ContentConnection() ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );

                    SqlHelper.ExecuteNonQuery( conn, CommandType.StoredProcedure, "[Library.SectionSubscriptionDelete]", sqlParameters );
                    successful = true;
                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".CollectionSubcriptionDelete() " );
                    statusMessage = thisClassName + "- Unsuccessful: CollectionSubcriptionDelete(): " + ex.Message.ToString();

                    successful = false;
                }
            }
            return successful;
        }//

        
        /// <summary>
        /// Add a Library.SectionSubscription record
        /// </summary>
        /// <param name="sectionId"></param>
        /// <param name="userId"></param>
        /// <param name="typeId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int CollectionSubcriptionCreate( int sectionId, int userId, int typeId, ref string statusMessage )
        {
            int newId = 0;
            using ( SqlConnection conn = new SqlConnection( ContentConnection() ) )
            {
                try
                {

                    #region parameters
                    SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@SectionId", sectionId );
                    sqlParameters[ 1 ] = new SqlParameter( "@UserId", userId );
                    sqlParameters[ 2 ] = new SqlParameter( "@SubscriptionTypeId", typeId );

                    #endregion

                    SqlDataReader dr = SqlHelper.ExecuteReader( conn, CommandType.StoredProcedure, "[Library.SectionSubscriptionInsert]", sqlParameters );
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
                    LogError( ex, thisClassName + string.Format( ".CollectionSubcriptionCreate() for LibraryId: {0} and userId: {1}", sectionId, userId ) );
                    statusMessage = thisClassName + "- Unsuccessful: CollectionSubcriptionCreate(): " + ex.Message.ToString();

                }
            }
            return newId;
        }

        /// <summary>
        /// Update a Library.SectionSubscription record
        /// </summary>
        /// <param name="id"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public string CollectionSubcriptionUpdate( int id, int typeId )
        {
            string message = "successful";
            using ( SqlConnection conn = new SqlConnection( ContentConnection() ) )
            {
                try
                {

                    #region parameters
                    SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@id", id );
                    sqlParameters[ 1 ] = new SqlParameter( "@SubscriptionTypeId", typeId );
                    #endregion

                    SqlHelper.ExecuteNonQuery( conn, "[Library.SectionSubscriptionUpdate]", sqlParameters );
                    message = "successful";

                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + string.Format( ".CollectionSubcriptionUpdate() for Id: {0} ", id ) );
                    message = thisClassName + "- Unsuccessful: CollectionSubcriptionUpdate(): " + ex.Message.ToString();
                }
            }
            return message;

        }//

        /// <summary>
        /// Get Library.SectionSubscription record
        /// </summary>
        /// <param name="pCollectionSubcriptionId"></param>
        /// <returns></returns>
        public ObjectSubscription CollectionSubcriptionGet( int pCollectionSubcriptionId )
        {
            return CollectionSubcriptionGet( pCollectionSubcriptionId, 0, 0 );

        }//
        /// <summary>
        /// Get Library.SectionSubscription record
        /// </summary>
        /// <param name="pCollectionId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ObjectSubscription CollectionSubcriptionGet( int pCollectionId, int userId )
        {
            return CollectionSubcriptionGet( 0, pCollectionId, userId );
        }//
        private ObjectSubscription CollectionSubcriptionGet( int pId, int pCollectionId, int pUserId )
        {
            ObjectSubscription entity = new ObjectSubscription();
            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@id", pId );
                    sqlParameters[ 1 ] = new SqlParameter( "@SectionId", pCollectionId );
                    sqlParameters[ 2 ] = new SqlParameter( "@UserId", pUserId );

                    SqlDataReader dr = SqlHelper.ExecuteReader( conn, "[Library.SectionSubscriptionGet]", sqlParameters );

                    if ( dr.HasRows )
                    {
                        // it should return only one record.
                        while ( dr.Read() )
                        {
                            entity = Fill( dr );
                            entity.SubscriptionCategory = "CollectionSubcription";
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
                    entity.Message = "Unsuccessful: " + thisClassName + ".CollectionSubcriptionGet(): " + ex.Message.ToString();
                    entity.IsValid = false;
                    return entity;

                }
            }

        }//

        #endregion

        #region ====== Helper Methods ===============================================
        /// <summary>
        /// Fill a Library.Membership object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>ObjectSubscription</returns>
        public ObjectSubscription Fill( SqlDataReader dr )
        {
            ObjectSubscription entity = new ObjectSubscription();

            entity.IsValid = true;

            entity.Id = GetRowColumn( dr, "Id", 0 );

            entity.ParentId = GetRowPossibleColumn( dr, "ParentId", 0 );
            if (entity.ParentId == 0)
                entity.ParentId = GetRowPossibleColumn( dr, "LibraryId", 0 );
            if ( entity.ParentId == 0 )
                entity.ParentId = GetRowColumn( dr, "SectionId", 0 );

            entity.CreatedById = GetRowColumn( dr, "UserId", 0 );
            entity.SubscriptionTypeId = GetRowColumn( dr, "SubscriptionTypeId", 0 );

            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );

            return entity;
        }//

        #endregion

    }
}