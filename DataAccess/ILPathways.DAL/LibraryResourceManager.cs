using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class LibraryResourceManager : BaseDataManager
    {
        static string className = "LibraryResourceManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[Library.ResourceGet]";
        const string SEARCH_PROC = "[Library.ResourceSearch]";
        const string DELETE_PROC = "[Library.ResourceDelete]";
        const string INSERT_PROC = "[Library.ResourceInsert]";
        const string UPDATE_PROC = "[Library.ResourceUpdate]";

        /// <summary>
        /// Default constructor
        /// </summary>
        public LibraryResourceManager()
        { }//

        #region ====== Core Methods ===============================================
        /// <summary>
        /// Delete an Library resource by Id
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool DeleteById( int libraryResourceId, ref string statusMessage )
        {
            return Delete( libraryResourceId, 0, 0, ref statusMessage );
        }//

        /// <summary>
        /// Delete an Library resource by collection id and resource int id
        /// </summary>
        /// <param name="fromCollectionId"></param>
        /// <param name="resourceIntId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( int fromCollectionId, int resourceIntId, ref string statusMessage )
        {
            return Delete( 0, fromCollectionId, resourceIntId, ref statusMessage );
        }//

        private bool Delete( int libraryResourceId, int fromCollectionId, int resourceIntId, ref string statusMessage )
        {
            bool successful = false;
            using ( SqlConnection conn = new SqlConnection( ContentConnection() ) )
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@LibraryResourceId", libraryResourceId );
                sqlParameters[ 1 ] = new SqlParameter( "@SourceLibrarySectionId", fromCollectionId );
                sqlParameters[ 2 ] = new SqlParameter( "@ResourceIntId", resourceIntId );

                try
                {
                    SqlHelper.ExecuteNonQuery( conn, CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
                    successful = true;
                }
                catch ( Exception ex )
                {
                    LogError( ex, className + string.Format( ".Delete(libraryResourceId: {0}, fromCollectionId: {1}, resourceIntId: {2}) ", libraryResourceId, fromCollectionId, resourceIntId ) );
                    statusMessage = className + "- Unsuccessful: Delete(): " + ex.Message.ToString();
                    successful = false;
                }
            }
            return successful;
        }//

     
        private int CreateViewed( LibraryResource entity )
        {

            SqlParameter[] sqlParameters = new SqlParameter[ 6 ];

            sqlParameters[ 0 ] = new SqlParameter( "@Id", entity.Id );
            sqlParameters[ 1 ] = new SqlParameter( "@LibrarySectionId", entity.LibrarySectionId );
            sqlParameters[ 2 ] = new SqlParameter( "@ResourceIntId", entity.ResourceIntId );
            sqlParameters[ 3 ] = new SqlParameter( "@Comment", entity.Comment );
            sqlParameters[ 4 ] = new SqlParameter( "@Created", entity.Created );
            sqlParameters[ 5 ] = new SqlParameter( "@CreatedById", entity.CreatedById );


            return 0;

        }//
        /// <summary>
        /// not sure???
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private int CreatePublished( LibraryResource entity )
        {

            SqlParameter[] sqlParameters = new SqlParameter[ 6 ];

            sqlParameters[ 0 ] = new SqlParameter( "@Id", entity.Id );
            sqlParameters[ 1 ] = new SqlParameter( "@LibrarySectionId", entity.LibrarySectionId );
            sqlParameters[ 2 ] = new SqlParameter( "@ResourceIntId", entity.ResourceIntId );
            sqlParameters[ 3 ] = new SqlParameter( "@Comment", entity.Comment );
            sqlParameters[ 4 ] = new SqlParameter( "@Created", entity.Created );
            sqlParameters[ 5 ] = new SqlParameter( "@CreatedById", entity.CreatedById );


            return 0;

        }//

        /// <summary>
        /// may not need this in libraries - could be too many entries - user can like something without necessarily wanting in a libary
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private int CreateLikedType( LibraryResource entity )
        {

            SqlParameter[] sqlParameters = new SqlParameter[ 6 ];

            sqlParameters[ 0 ] = new SqlParameter( "@Id", entity.Id );
            sqlParameters[ 1 ] = new SqlParameter( "@LibrarySectionId", entity.LibrarySectionId );
            sqlParameters[ 2 ] = new SqlParameter( "@ResourceIntId", entity.ResourceIntId );
            sqlParameters[ 3 ] = new SqlParameter( "@Comment", entity.Comment );
            sqlParameters[ 4 ] = new SqlParameter( "@Created", entity.Created );
            sqlParameters[ 5 ] = new SqlParameter( "@CreatedById", entity.CreatedById );


            return 0;
        }//
        /// <summary>
        ///  Method to create LibraryResource object
        /// </summary>
        /// <param name="resourceIntId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        //public int Create( int resourceIntId, int userId, ref string statusMessage )
        //{
        //    IWebUser user = new PatronManager().Get( userId );
        //    if ( user != null )
        //    {
        //        LibraryResource entity = new LibraryResource();
        //        entity.ResourceIntId = resourceIntId;
        //        entity.CreatedById = user.Id;

        //        return Create( entity, user, ref statusMessage );
        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //}//


        /// <summary>
        ///  Method to create LibraryResource object
        ///  Note the stored proc will handle LibrarySectionId = 0 by using default section (in which case library is required)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Create( LibraryResource entity, ref string statusMessage )
        {

            int newId = 0;
            using ( SqlConnection conn = new SqlConnection( ContentConnection() ) )
            {
                try
                {
                    #region parameters
                    SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@LibraryId", entity.LibraryId );
                    sqlParameters[ 1 ] = new SqlParameter( "@LibrarySectionId", entity.LibrarySectionId );
                    sqlParameters[ 2 ] = new SqlParameter( "@ResourceIntId", entity.ResourceIntId );
                    sqlParameters[ 3 ] = new SqlParameter( "@Comment", entity.Comment );
                    sqlParameters[ 4 ] = new SqlParameter( "@CreatedById", entity.CreatedById );

                    #endregion

                    SqlDataReader dr = SqlHelper.ExecuteReader( conn, CommandType.StoredProcedure, "[Library.ResourceInsert]", sqlParameters );
                    if ( dr.HasRows )
                    {
                        dr.Read();
                        newId = int.Parse( dr[ 0 ].ToString() );
                        entity.Id = newId;
                        entity.IsValid = true;
                    }
                    dr.Close();
                    dr = null;
                    statusMessage = "successful";

                }
                catch ( Exception ex )
                {
                    //provide helpful info about failing entity
                    LogError( ex, className + string.Format( ".LibraryResource_Create() for resource: {0} and CreatedBy: {1}", entity.ResourceIntId, entity.CreatedById ) );
                    statusMessage = className + "- Unsuccessful: LibraryResource_Create(): " + ex.Message.ToString();

                    entity.Message = statusMessage;
                    entity.IsValid = false;
                }
            }
            return newId;
        }//

        public string Update( LibraryResource entity )
        {

            string message = "successful";
            using ( SqlConnection conn = new SqlConnection( ContentConnection() ) )
            {
                try
                {
                    #region parameters
                    SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@Id", entity.Id );
                    sqlParameters[ 1 ] = new SqlParameter( "@Comment", entity.Comment );

                    #endregion

                    SqlHelper.ExecuteNonQuery( ContentConnection(), UPDATE_PROC, sqlParameters );
                    message = "successful";

                }
                catch ( Exception ex )
                {
                    LogError( ex, className + string.Format( ".Update() for Id: {0}and LastUpdatedBy: {1}", entity.Id.ToString(), entity.LastUpdatedBy ) );

                    message = className + "- Unsuccessful: Update(): " + ex.Message.ToString();
                    entity.Message = message;
                    entity.IsValid = false;
                }
            }
            return message;
        }//

        /// <summary>
        /// Copy a library resource to new collection using libraryResourceId
        /// </summary>
        /// <param name="libraryResourceId"></param>
        /// <param name="toCollectionId"></param>
        /// <param name="userId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int ResourceCopyById( int libraryResourceId, int toCollectionId, int userId, ref string statusMessage )
        {
            return ResourceCopy( libraryResourceId, 0, toCollectionId, userId, ref statusMessage );
        }//

        /// <summary>
        /// Copy a library resource to new collection using resourceIntId and target collection Id
        /// 14-01-09 mparsons - removing @SourceLibrarySectionId (will physically remove later)
        /// 15-03-25 mparsons - actually removed @SourceLibrarySectionId 
        /// </summary>
        /// <param name="resourceIntId"></param>
        /// <param name="toCollectionId"></param>
        /// <param name="userId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int ResourceCopy( int resourceIntId, int toCollectionId, int userId, ref string statusMessage )
        {
            return ResourceCopy( 0, resourceIntId, toCollectionId, userId, ref statusMessage );
        }//

        private int ResourceCopy( int libraryResourceId, int resourceIntId, int toCollectionId, int userId, ref string statusMessage )
        {
            int newId = 0;
            using ( SqlConnection conn = new SqlConnection( ContentConnection() ) )
            {
                try
                {
                    #region parameters
                    SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@LibraryResourceId", libraryResourceId );
                    //sqlParameters[ 1 ] = new SqlParameter( "@SourceLibrarySectionId", fromCollectionId );
                    sqlParameters[ 1 ] = new SqlParameter( "@ResourceIntId", resourceIntId );
                    sqlParameters[ 2 ] = new SqlParameter( "@NewLibrarySectionId", toCollectionId );
                    sqlParameters[ 3 ] = new SqlParameter( "@CreatedById", userId );

                    #endregion

                    SqlDataReader dr = SqlHelper.ExecuteReader( conn, CommandType.StoredProcedure, "[Library.ResourceCopy]", sqlParameters );
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
                    if ( ex.Message.ToLower().IndexOf( "cannot insert duplicate key row" ) > -1 )
                    {
                        statusMessage = "Error - the resource already exists in the selected collection";
                    }
                    else
                    {
                        LogError( ex, className + string.Format( ".ResourceCopy() for Id: {0}, or resourceIntId: {1}, toCollectionId: {2}", libraryResourceId, resourceIntId, toCollectionId ) );
                        statusMessage = className + "- Unsuccessful: ResourceCopy(): " + ex.Message.ToString();
                    }
                }
            }
            return newId;
        }//

        public string ResourceMoveById( int libraryResourceId, int toCollectionId, int userId, ref string statusMessage )
        {
            return ResourceMove( libraryResourceId, 0, 0, toCollectionId, userId, ref statusMessage );
        }//

        /// <summary>
        /// Copy a library resource to new collection using resourceIntId and target collection Id
        /// </summary>
        /// <param name="fromCollectionId"></param>
        /// <param name="resourceIntId"></param>
        /// <param name="toCollectionId"></param>
        /// <param name="userId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public string ResourceMove( int fromCollectionId, int resourceIntId, int toCollectionId, int userId, ref string statusMessage )
        {
            return ResourceMove( 0, fromCollectionId, resourceIntId, toCollectionId, userId, ref statusMessage );
        }//

        private string ResourceMove( int libraryResourceId, int fromCollectionId, int resourceIntId, int toCollectionId, int userId, ref string statusMessage )
        {
            string message = "successful";
            using ( SqlConnection conn = new SqlConnection( ContentConnection() ) )
            {
                try
                {
                    #region parameters
                    SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@LibraryResourceId", libraryResourceId );
                    sqlParameters[ 1 ] = new SqlParameter( "@SourceLibrarySectionId", fromCollectionId );
                    sqlParameters[ 2 ] = new SqlParameter( "@ResourceIntId", resourceIntId );
                    sqlParameters[ 3 ] = new SqlParameter( "@NewLibrarySectionId", toCollectionId );
                    sqlParameters[ 4 ] = new SqlParameter( "@CreatedById", userId );

                    #endregion

                    SqlHelper.ExecuteNonQuery( conn, "[Library.ResourceMove]", sqlParameters );
                    message = "successful";

                }
                catch ( Exception ex )
                {
                    if ( ex.Message.ToLower().IndexOf( "cannot insert duplicate key row" ) > -1 )
                    {
                        statusMessage = "Error - the resource already exists in the selected collection";
                    }
                    else
                    {
                        LogError( ex, className + string.Format( ".ResourceMove() for Id: {0}, or resourceIntId: {1}, fromCollectionId: {2}, toCollectionId: {3}", libraryResourceId, resourceIntId, fromCollectionId, toCollectionId ) );
                        statusMessage = className + "- Unsuccessful: ResourceMove(): " + ex.Message.ToString();
                    }
                }
            }
            return message;
        }//

        #endregion


        #region ====== LIbrary Retrieval/Helper methods ===============================================
        public LibraryResource Get( int libResId )
        {
            string connectionString = ContentConnectionRO();
            LibraryResource entity = new LibraryResource();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", libResId );
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, GET_PROC, sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.
                    while ( dr.Read() )
                    {
                        entity = FillLazy( dr );
                    }
                }
                dr.Close();
                dr = null;
                return entity;

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".LibrarySectionGet() " );
                entity.Message = "Unsuccessful: " + className + ".LibrarySectionGet(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }
        }//
        /// <summary>
        /// determine if resource is in the default library
        /// </summary>
        /// <param name="user"></param>
        /// <param name="resourceIntId"></param>
        /// <returns></returns>
        private bool IsResourceInLibrary( IWebUser user, int resourceIntId )
        {
            try
            {
                Library lib = new LibraryManager().GetMyLibrary( user );
                int pTotalRows = 0;
                string pFilter = string.Format( "(LibraryId = {0} and lib.ResourceIntId = {1}) ", lib.Id, resourceIntId );

                DataSet ds = Search( pFilter, "", 1, 50, ref pTotalRows );
                if ( DoesDataSetHaveRows( ds ) )
                    return true;
                else
                    return false;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".IsResourceInLibrary() exception for " + user.FullName() );
                return false;
            }
        }//

        private bool IsResourceInLibrary( int libraryId, int resourceIntId )
        {
            try
            {
                int pTotalRows = 0;
                string pFilter = string.Format( "(LibraryId = {0} and lib.ResourceIntId = {1}) ", libraryId, resourceIntId );

                DataSet ds = Search( pFilter, "", 1, 50, ref pTotalRows );
                if ( DoesDataSetHaveRows( ds ) )
                    return true;
                else
                    return false;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".IsResourceInLibrary() exception for libraryId: " + libraryId.ToString() );
                return false;
            }
        }//

        /// <summary>
        /// Return list of all resources for a library
        /// </summary>
        /// <param name="libraryId"></param>
        /// <returns></returns>
        public List<LibraryResource> SelectAllResourceIdsForLibrary( int libraryId )
        {

            List<LibraryResource> list = new List<LibraryResource>();
            try
            {
                int pTotalRows = 0;
                string pFilter = string.Format( "(lib.LibraryId = {0} ) ", libraryId );

                list = SearchList( pFilter, "", 1, 10000, ref pTotalRows );

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SelectAllResourceIdsForLibrary() exception for libraryId: " + libraryId.ToString() );

            }
            return list;
        }//

        /// <summary>
        /// Return list of all resources for a library section
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public List<LibraryResource> SelectAllResourcesForSection( int sectionId )
        {

            List<LibraryResource> list = new List<LibraryResource>();
            try
            {
                int pTotalRows = 0;
				string pFilter = string.Format( "(lib.LibrarySectionId = {0} ) ", sectionId );

                list = SearchList( pFilter, "", 1, 5000, ref pTotalRows );

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SelectAllResourcesForSection() exception for sectionId: " + sectionId.ToString() );

            }
            return list;
        }//

        /// <summary>
        /// Return list of all resources for a library section
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public List<LibraryResource> SelectResourcesRequiringApproval( int libraryId )
        {

            List<LibraryResource> list = new List<LibraryResource>();
            try
            {
                int pTotalRows = 0;
                string pFilter = string.Format( "(LibraryId = {0} AND IsActive = 0 ) ", libraryId );

                list = SearchList( pFilter, "", 1, 5000, ref pTotalRows );

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SelectAllResourcesForSection() exception for sectionId: " + libraryId.ToString() );

            }
            return list;
        }//
        #endregion


        #region ====== Search methods ===============================================
        public bool IsLibraryEmpty( int libraryId )
        {

            int pTotalRows = 0;
            string filter = string.Format( "(lib.LibraryId = {0}) ", libraryId );
            DataSet ds = Search( filter, "", 1, 10, ref pTotalRows );
            if ( DoesDataSetHaveRows( ds ) == true )
                return false;
            else
                return true;
        }

        /// <summary>
        /// Get unique lists of all libraries and collections containing the resource Id
        /// </summary>
        /// <param name="resourceIntId"></param>
        /// <param name="libList">list of libraryIds containing resource</param>
        /// <param name="collList">list of sectionIds containing resource</param>
        /// <returns>true if data found</returns>
        public bool SelectAllLibrariesAndSectionsForResource( int resourceIntId,
                            ref List<int> libList,
                            ref List<int> collList )
        {

            DoTrace( 6, string.Format( className + ".SelectAllLibrariesAndSectionsForResource() : {0}", resourceIntId ) );

            bool found = false;
            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntId );
            
            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Library.UniqueLibrariesForResource]", sqlParameters );
                    if (DoesDataSetHaveRows(ds))
                    {
                        int count = ds.Tables[ 0 ].Rows.Count;
                        //libList = new int[ count ];
                        //collList = new int[ count ]; 
                        found = true;
                        int prevLibId= 0;
                        int prevColId = 0;
                        int libIdx = 0;
                        int colIdx = 0;
                        foreach ( DataRow row in ds.Tables[ 0 ].Rows )
                        {
                            int libraryId = GetRowColumn( row, "LibraryId", 0 );
                            int collectionId = GetRowColumn( row, "CollectionId", 0 );
                            if ( libraryId != prevLibId )
                            {
                                libList.Add( libraryId );
                                //libList[ libIdx ] = libraryId;
                                prevLibId = libraryId;
                                libIdx++;
                            }
                            if ( collectionId != prevColId )
                            {
                                collList.Add( collectionId );
                                //collList[ colIdx ] = collectionId;
                                prevColId = collectionId;
                                colIdx++;
                            }
                        }
                    }
                    return found;
                    
                }
                catch ( Exception ex )
                {
                    LogError( ex, className + ".SelectAllLibrariesAndSectionsForResource() " );
                    return false;

                }
            }
        }
        /// <summary>
        /// Search for Library Resource related data using passed parameters
        /// typically used with widgets where a limited number of resources are pulled for a collection
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public List<LibraryResource> SearchList( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            LibraryResource entity = new LibraryResource();
            List<LibraryResource> entities = new List<LibraryResource>();

            DataSet ds = Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
            //DataSet ds = SearchFromIoer( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
            try
            {
                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow row in ds.Tables[ 0 ].Rows )
                    {
                        entity = this.Fill( row );

                        entities.Add( entity );
                    }
                }
                return entities;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SearchList() " );
                return null;

            }
        }

        /// <summary>
        /// Do a library resource search, but doing through IOER database - for better performance.
        /// TODO
        /// - review the actual columns returned from this version
        /// - might be better to have a different DTO
        /// - always on top resources
        /// - show who added resource to library
        /// - when added to the library
        /// - include target url
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public List<LibraryResource> SearchFromIoer( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            LibraryResource entity = new LibraryResource();
            List<LibraryResource> entities = new List<LibraryResource>();


            DataSet ds = SearchViaIoer( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
            try
            {
                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        //need to customize fill, as likely for a full search
                        //entity = this.Fill( dr );
                        entity = new LibraryResource();
                        entity.IsValid = true;

                        entity.Id = GetRowColumn( dr, "LibraryResourceId", 0 );
                        if ( entity.Id == 0 )
                            entity.Id = GetRowColumn( dr, "Id", 0 );

                        entity.LibraryId = GetRowColumn( dr, "LibraryId", 0 );
                        entity.LibrarySectionId = GetRowColumn( dr, "LibrarySectionId", 0 );
                        entity.CollectionTitle = GetRowColumn( dr, "LibrarySection", "" );

                        entity.Title = GetRowPossibleColumn( dr, "Title", "" );
                        entity.Description = GetRowPossibleColumn( dr, "Description", "" );
                        entity.SortTitle = GetRowPossibleColumn( dr, "SortTitle", "" );
                        entity.ResourceUrl = GetRowPossibleColumn( dr, "ResourceUrl", "" );
                        entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
                        entity.ResourceVersionIntId = GetRowColumn( dr, "ResourceVersionIntId", 0 );
                        entity.SetImageUrls();

                        entity.Created = GetRowColumn( dr, "DateAddedToCollection", System.DateTime.MinValue );
                        entity.CreatedById = GetRowColumn( dr, "libResourceCreatedById", 0 );
                        entity.CreatedBy = GetRowPossibleColumn( dr, "CreatedBy", "" );
                        entity.CreatedByImageUrl = GetRowPossibleColumn( dr, "CreatedByImageUrl", "" );
                        entity.ProfileUrl = string.Format( "/Profile/{0}/{1}", entity.CreatedById, entity.CreatedBy.Replace( " ", "" ) );

                        entities.Add( entity );
                    }
                }
                return entities;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SearchFromIoer() " );
                return null;

            }
        }
        /// <summary>
        /// Search for Library Resource related data using passed parameters
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public DataSet SearchViaIoer( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
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

            //NOTE - using prod in ioer database - confusing, YES
            using ( SqlConnection conn = new SqlConnection( GetReadOnlyConnection() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Library.ResourceSearch]", sqlParameters );
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
                    LogError( ex, className + ".SearchViaIoer() " );
                    return null;
                }
            }
        }
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

            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Library.ResourceSearch]", sqlParameters );
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
                    LogError( ex, className + ".LibraryResource_Search() " );
                    return null;

                }
            }
        }

        private LibraryResource Fill( DataRow dr )
        {
            LibraryResource entity = FillLazy( dr );

            //library
           // entity.ResourceLibrary.IsPublic = GetRowColumn( dr, "IsPublic", false );
            entity.ResourceLibrary.IsDiscoverable = GetRowColumn( dr, "IsDiscoverable", false );
            entity.ResourceLibrary.LibraryTypeId = GetRowColumn( dr, "LibraryTypeId", 0 );
            entity.ResourceLibrary.LibraryType = GetRowColumn( dr, "LibraryType", "" );

            //section
            entity.ResourceSection.SectionTypeId = GetRowColumn( dr, "SectionTypeId", 0 );
            entity.ResourceSection.SectionType = GetRowColumn( dr, "LibrarySectionType", "" );
            entity.ResourceSection.AreContentsReadOnly = GetRowColumn( dr, "AreContentsReadOnly", false );

            return entity;
        }//

        private LibraryResource FillLazy( DataRow dr )
        {
            LibraryResource entity = new LibraryResource();
            entity.ResourceSection = new LibrarySection();

            entity.IsValid = true;

            entity.Id = GetRowColumn( dr, "LibraryResourceId", 0 );
            if ( entity.Id == 0 )
                entity.Id = GetRowColumn( dr, "Id", 0 );

            entity.LibraryId = GetRowColumn( dr, "LibraryId", 0 );
            entity.LibrarySectionId = GetRowColumn( dr, "LibrarySectionId", 0 );
            entity.CollectionTitle = GetRowColumn( dr, "LibrarySection", "" );

            entity.Title = GetRowPossibleColumn( dr, "Title", "" );
            entity.Description = GetRowPossibleColumn( dr, "Description", "" );
            entity.SortTitle = GetRowPossibleColumn( dr, "SortTitle", "" );
            entity.ResourceUrl = GetRowPossibleColumn( dr, "ResourceUrl", "" );
            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            entity.ResourceVersionIntId = GetRowColumn( dr, "ResourceVersionIntId", 0 );
            entity.SetImageUrls();

            entity.Created = GetRowColumn( dr, "DateAddedToCollection", System.DateTime.MinValue );
            entity.CreatedById = GetRowColumn( dr, "libResourceCreatedById", 0 );
            entity.CreatedBy = GetRowPossibleColumn( dr, "CreatedBy", "" );
            entity.CreatedByImageUrl = GetRowPossibleColumn( dr, "CreatedByImageUrl", "" );
            entity.ProfileUrl = string.Format( "/Profile/{0}/{1}", entity.CreatedById, entity.CreatedBy.Replace(" ","") );
            return entity;
        }//

        private LibraryResource Fill( SqlDataReader dr )
        {
            LibraryResource entity = FillLazy( dr );

            //library
            //entity.ResourceLibrary.IsPublic = GetRowColumn( dr, "IsPublic", false );
            entity.ResourceLibrary.IsDiscoverable = GetRowColumn( dr, "IsDiscoverable", false );
            entity.ResourceLibrary.LibraryTypeId = GetRowColumn( dr, "LibraryTypeId", 0 );
            entity.ResourceLibrary.LibraryType = GetRowColumn( dr, "LibraryType", "" );

            //section
            entity.ResourceSection.SectionTypeId = GetRowColumn( dr, "SectionTypeId", 0 );
            entity.ResourceSection.SectionType = GetRowColumn( dr, "LibrarySectionType", "" );
            entity.ResourceSection.AreContentsReadOnly = GetRowColumn( dr, "AreContentsReadOnly", false );

            return entity;
        }//

        private LibraryResource FillLazy( SqlDataReader dr )
        {
            LibraryResource entity = new LibraryResource();
            entity.ResourceSection = new LibrarySection();

            entity.IsValid = true;
            //handle fills from search, not just a get
            entity.Id = GetRowPossibleColumn( dr, "LibraryResourceId", 0 );
            if (entity.Id == 0) 
                entity.Id = GetRowColumn( dr, "Id", 0 );

            entity.LibrarySectionId = GetRowColumn( dr, "LibrarySectionId", 0 );

            entity.Title = GetRowPossibleColumn( dr, "Title", "" );
            entity.SortTitle = GetRowPossibleColumn( dr, "SortTitle", "" );
            entity.ResourceUrl = GetRowPossibleColumn( dr, "ResourceUrl", "" );
            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            entity.ResourceVersionIntId = GetRowColumn( dr, "ResourceVersionIntId", 0 );
            entity.SetImageUrls();

            entity.Created = GetRowColumn( dr, "DateAddedToCollection", System.DateTime.MinValue );
            entity.CreatedById = GetRowColumn( dr, "libResourceCreatedById", 0 );

            return entity;
        }//
        #endregion

    }
}