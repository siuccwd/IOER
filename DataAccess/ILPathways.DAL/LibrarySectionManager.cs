using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.ApplicationBlocks.Data;

using ILPathways.Business;
using ILPathways.Utilities;

namespace ILPathways.DAL
{
    public class LibrarySectionManager : BaseDataManager
    {
        static string thisClassName = "LibraryManager";
        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[Library.SectionGet]";
        const string SELECT_PROC = "[Library.SectionSelect]";

        const string DELETE_PROC = "[Library.SectionDelete]";
        const string INSERT_PROC = "[Library.SectionInsert]";
        const string UPDATE_PROC = "[Library.SectionUpdate]";

        public static int MY_EVALUATIONS_LIBRARY_SECTION_ID = 1;
        public static int MY_AUTHORED_LIBRARY_SECTION_ID = 2;
        public static int GENERAL_LIBRARY_SECTION_ID = 3;

        /// <summary>
        /// Default constructor
        /// </summary>
        public LibrarySectionManager()
        { }//

        #region ====== Core methods ===============================================
        public static bool Delete( int pId, ref string statusMessage )
        {
            bool successful = false;

            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
            sqlParameters[ 0 ].Value = pId;

            try
            {
                SqlHelper.ExecuteNonQuery( ContentConnection(), CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
                successful = true;
            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".LibrarySection_Delete() " );
                statusMessage = thisClassName + "- Unsuccessful: LibrarySection_Delete(): " + ex.Message.ToString();

                successful = false;
            }
            return successful;
        }//
        /// <summary>
        /// Add default section for library 
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static LibrarySection CreateDefault( int libraryId, ref string statusMessage )
        {
            Library lib = new LibraryManager().Get( libraryId );

            LibrarySection entity = new LibrarySection();
            entity.LibraryId = libraryId;
            entity.SectionTypeId = GENERAL_LIBRARY_SECTION_ID;
            entity.Title = "Default collection";
            entity.Description = "The default collection is system generated. You may change the title and description as needed.";
            entity.AreContentsReadOnly = false;
            entity.IsDefaultSection = true;
            entity.IsActive = true;
            entity.IsPublic = true;
            entity.ImageUrl = lib.ImageUrl;
            entity.RowId = Guid.NewGuid();

            if ( lib != null && lib.SeemsPopulated )
            {
                entity.PublicAccessLevel = lib.PublicAccessLevel;
                entity.OrgAccessLevel = lib.OrgAccessLevel;
            }
            else
            {
                entity.PublicAccessLevel = EObjectAccessLevel.ReadOnly;
                entity.OrgAccessLevel = EObjectAccessLevel.ContributeWithApproval;
            }
            entity.ParentId = 0;
            entity.CreatedById = lib.CreatedById;

            int id = Create( entity, ref statusMessage );
            entity.Id = id;
            if ( id > 0 )
            {

            }

            return entity;
        }//

        public static LibrarySection CreateMyAuthored( int libraryId, ref string statusMessage )
        {
            LibrarySection entity = new LibrarySection();
            entity.LibraryId = libraryId;
            entity.SectionTypeId = MY_AUTHORED_LIBRARY_SECTION_ID;
            entity.Title = "My Authored";
            entity.Description = "Collection of resources that were created by me";
            entity.AreContentsReadOnly = false;
            entity.IsDefaultSection = false;
            entity.IsPublic = true;
            entity.PublicAccessLevel = EObjectAccessLevel.ReadOnly;
            entity.OrgAccessLevel = EObjectAccessLevel.ReadOnly;
            entity.IsActive = true;
            entity.ParentId = 0;
            //hmm, either pass in id, or get from library
            Library lib = new LibraryManager().Get( libraryId );
            if ( lib != null && lib.Id > 0 )
            {
                entity.CreatedById = lib.CreatedById;

                int id = Create( entity, ref statusMessage );
                entity.Id = id;
                if ( id > 0 )
                {

                }
                return entity;
            }
            else
            {
                return null;
            }


        }//


        /// <summary>
        /// Add an section record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static int Create( LibrarySection entity, ref string statusMessage )
        {
            int newId = 0;
            try
            {
                #region parameters
                //temp handling of transition from IsPublic
                if ( entity.IsPublic && entity.PublicAccessLevel == 0 )
                {
                    entity.PublicAccessLevel = EObjectAccessLevel.ReadOnly;
                }
                if ( entity.IsPublic && entity.OrgAccessLevel == 0 )
                {
                    entity.OrgAccessLevel = EObjectAccessLevel.ReadOnly;
                }
                SqlParameter[] sqlParameters = new SqlParameter[ 12 ];
                sqlParameters[ 0 ] = new SqlParameter( "@LibraryId", entity.LibraryId );
                sqlParameters[ 1 ] = new SqlParameter( "@SectionTypeId", entity.SectionTypeId );
                sqlParameters[ 2 ] = new SqlParameter( "@Title", entity.Title );
                sqlParameters[ 3 ] = new SqlParameter( "@Description", entity.Description );
                sqlParameters[ 4 ] = new SqlParameter( "@IsDefaultSection", entity.IsDefaultSection );
                sqlParameters[ 5 ] = new SqlParameter( "@PublicAccessLevel", ( int ) entity.PublicAccessLevel );
                sqlParameters[ 6 ] = new SqlParameter( "@OrgAccessLevel", ( int ) entity.OrgAccessLevel );
                sqlParameters[ 7 ] = new SqlParameter( "@AreContentsReadOnly", entity.AreContentsReadOnly );
                sqlParameters[ 8 ] = new SqlParameter( "@ParentId", entity.ParentId );
                sqlParameters[ 9 ] = new SqlParameter( "@CreatedById", entity.CreatedById );
                sqlParameters[ 10 ] = new SqlParameter( "@ImageUrl", entity.ImageUrl );
                if ( entity.HasValidRowId() == false )
                    entity.RowId = Guid.NewGuid();

                sqlParameters[ 11 ] = new SqlParameter( "@RowId", entity.RowId );
                //sqlParameters[ 10 ] = new SqlParameter( "@Subject", entity.Message );
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( ContentConnection(), CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
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
                LogError( ex, thisClassName + string.Format( ".LibrarySectionCreate() for title: {0} and CreatedBy: {1}", entity.Title, entity.CreatedById ) );
                statusMessage = thisClassName + "- Unsuccessful: LibrarySectionCreate(): " + ex.Message.ToString();

                entity.Message = statusMessage;
                entity.IsValid = false;
            }

            return newId;
        }

        public static string Update( LibrarySection entity )
        {
            string message = "successful";
            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 10 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", entity.Id );
                sqlParameters[ 1 ] = new SqlParameter( "@LibraryId", entity.LibraryId );
                sqlParameters[ 2 ] = new SqlParameter( "@Title", entity.Title );
                sqlParameters[ 3 ] = new SqlParameter( "@Description", entity.Description );
                sqlParameters[ 4 ] = new SqlParameter( "@IsDefaultSection", entity.IsDefaultSection );
                sqlParameters[ 5 ] = new SqlParameter( "@PublicAccessLevel", ( int ) entity.PublicAccessLevel );
                sqlParameters[ 6 ] = new SqlParameter( "@OrgAccessLevel", ( int ) entity.OrgAccessLevel );
                sqlParameters[ 7 ] = new SqlParameter( "@AreContentsReadOnly", entity.AreContentsReadOnly );
                sqlParameters[ 8 ] = new SqlParameter( "@CreateLastUpdatedByIddById", entity.LastUpdatedById );

                sqlParameters[ 9 ] = new SqlParameter( "@ImageUrl", entity.ImageUrl );
                //sqlParameters[ 9 ] = new SqlParameter( "@Subject", entity.Message );
                #endregion

                SqlHelper.ExecuteNonQuery( ContentConnection(), UPDATE_PROC, sqlParameters );
                message = "successful";
            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + string.Format( ".LibrarySectionUpdate() for Id: {0}, title: {1} and LastUpdatedBy: {2}", entity.Id.ToString(), entity.Title, entity.LastUpdatedBy ) );

                message = thisClassName + "- Unsuccessful: LibrarySectionUpdate(): " + ex.Message.ToString();
                entity.Message = message;
                entity.IsValid = false;
            }

            return message;
        }

        #endregion

        #region ====== Retrieval Methods ===============================================

        public static LibrarySection GetLibraryDefaultSection( int libraryId )
        {
            return GetLibrarySection_Default( libraryId, false );
        }//

        public static LibrarySection GetLibrarySection_Default( int libraryId, bool createIfMissing )
        {
            string connectionString = ContentConnectionRO();
            LibrarySection entity = new LibrarySection();
            string statusMessage = "";
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@LibraryId", libraryId );
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, "[Library.SectionGetDefault]", sqlParameters );

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
                    if ( createIfMissing )
                    {
                        entity = CreateDefault( libraryId, ref statusMessage );
                    }
                    else
                    {
                        entity.Message = "Record not found";
                        entity.IsValid = false;
                    }
                }
                dr.Close();
                dr = null;
                return entity;

            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".GetLibraryDefaultSection() " );
                entity.Message = "Unsuccessful: " + thisClassName + ".GetLibraryDefaultSection(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }
        }//

        public static LibrarySection GetLibrarySection_MyAuthored( IWebUser user )
        {
            Library entity = new LibraryManager().GetMyLibrary( user, true );
            if ( entity != null && entity.Id > 0 )
                return GetLibrarySection_MyAuthored( entity.Id );
            else
                return null;
        }//

        public static LibrarySection GetLibrarySection_MyAuthored( int libraryId )
        {
            return GetLibrarySection_MyAuthored( libraryId, false );
        }//

        /// <summary>
        /// NOT IMPLEMENTED FULLY!
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="createIfMissing"></param>
        /// <returns></returns>
        public static LibrarySection GetLibrarySection_MyAuthored( int libraryId, bool createIfMissing )
        {
            string connectionString = ContentConnectionRO();
            LibrarySection entity = new LibrarySection();
            string statusMessage = "";
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@LibraryId", libraryId );
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, "[Library.SectionGetDefault]", sqlParameters );

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
                    if ( createIfMissing )
                    {
                        entity = CreateMyAuthored( libraryId, ref statusMessage );
                    }
                    else
                    {
                        entity.Message = "Record not found";
                        entity.IsValid = false;
                    }
                }
                dr.Close();
                dr = null;
                return entity;

            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".GetLibraryDefaultSection() " );
                entity.Message = "Unsuccessful: " + thisClassName + ".GetLibraryDefaultSection(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//

        public static LibrarySection Get( int sectionId )
        {
            return Get( sectionId, "" );
        }//
        public static LibrarySection GetByGuid( string guid)
        {
            return Get( 0, guid );
        }//

        private static LibrarySection Get( int sectionId, string guid )
        {
            string connectionString = ContentConnectionRO();
            LibrarySection entity = new LibrarySection();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", sectionId );
                sqlParameters[ 1 ] = new SqlParameter( "@RowId", guid );
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, GET_PROC, sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.
                    while ( dr.Read() )
                    {
                        entity = Fill( dr );
                    }
                }
                dr.Close();
                dr = null;
                return entity;

            }
            catch ( Exception ex )
            {
				LogError( ex, thisClassName + string.Format( ".LibrarySectionGet() sectionId/guid: {0}/{1} ", sectionId, guid ) );
                entity.Message = "Unsuccessful: " + thisClassName + ".LibrarySectionGet(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }
        }//

        /// <summary>
        /// LibrarySections Select List
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="pShowingAll">0-only non-public; 1-Only public, 2-all</param>
        /// <returns></returns>
        public static List<LibrarySection> LibrarySectionsSelectList( int libraryId, int pShowingAll )
        {

            List<LibrarySection> collection = new List<LibrarySection>();

            DataSet ds = new DataSet();
            try
            {
                ds = LibrarySectionsSelect( libraryId, pShowingAll );

                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        LibrarySection entity = FillLazy( dr );
                        collection.Add( entity );
                    }
                }
                return collection;
            }
            catch ( Exception ex )
            {
				LogError( ex, thisClassName + string.Format( ".LibrarySectionsSelectList() libraryId: {0} ", libraryId ) );
                return null;

            }
        }

        /// <summary>
        /// Select sections for a Library 
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pShowingAll">0-only non-public; 1-Only public, 2-all</param>
        /// <returns></returns>
        public static DataSet LibrarySectionsSelect( int libraryId, int pShowingAll )
        {
            string connectionString = ContentConnectionRO();

            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@LibraryId", libraryId );
            sqlParameters[ 1 ] = new SqlParameter( "@ShowingAll", pShowingAll );
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
                LogError( ex, thisClassName + ".LibrarySectionsSelect() " );
                return null;

            }
        }
        //=================================================================================================
        public static bool DoesUserHaveEditAccess( int libraryId, int sectionId, int pUserid )
        {
            bool canAccess = false;

            List<LibrarySection> list = SelectListWithEditAccess( libraryId, pUserid );
            if ( list == null || list.Count == 0 )
                return canAccess;
            else
            {
                foreach ( LibrarySection l in list )
                {
                    if ( l.Id == sectionId )
                    {
                        canAccess = true;
                        break;
                    }
                }
            }
            return canAccess;
        }//

        /// <summary>
        /// Select all collections as List in library where user has edit access. Includes:
        /// - personal
        /// - libraries where has curate access
        /// - collections where has curate access
        /// - org libraries with implicit access
        /// </summary>
        /// <param name="pLibraryId"></param>
        /// <param name="pUserid"></param>
        /// <returns></returns>
        public static List<LibrarySection> SelectListWithEditAccess( int pLibraryId, int pUserid )
        {
            List<LibrarySection> collection = new List<LibrarySection>();

            try
            {
                DataSet ds = SelectWithEditAccess( pLibraryId, pUserid );

                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        LibrarySection entity = FillLazy( dr );
                        collection.Add( entity );
                    }
                }
                return collection;
            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".SelectListWithEditAccess() " );
                return null;

            }
        }//

        /// <summary>
        /// Select all collections in library where user has edit access. Includes:
        /// - personal
        /// - libraries where has curate access
        /// - collections where has curate access
        /// - org libraries with implicit access
        /// </summary>
        /// <param name="pLibraryId"></param>
        /// <param name="pUserid"></param>
        /// <returns></returns>
        public static DataSet SelectWithEditAccess( int pLibraryId, int pUserid )
        {

            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@LibraryId", pLibraryId );
            sqlParameters[ 1 ] = new SqlParameter( "@Userid", pUserid );

            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Library.SectionSelectCanEdit]", sqlParameters );

                    if ( ds.HasErrors )
                    {
                        return null;
                    }
                    return ds;
                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".SelectWithEditAccess() " );
                    return null;

                }
            }
        }

        //=================================================================================================
        public static bool DoesUserHaveContributeAccess( int libraryId, int sectionId, int pUserid )
        {
            bool canAccess = false;

            List<LibrarySection> list = SelectListWithContributeAccess( libraryId, pUserid );
            if ( list == null || list.Count == 0 )
                return canAccess;
            else
            {
                foreach ( LibrarySection l in list )
                {
                    if ( l.Id == sectionId )
                    {
                        canAccess = true;
                        break;
                    }
                }
            }
            return canAccess;
        }//

        /// <summary>
        /// Select all collections as List in library where user has edit access. Includes:
        /// - personal
        /// - libraries where has curate access
        /// - collections where has curate access
        /// - org libraries with implicit access
        /// </summary>
        /// <param name="pLibraryId"></param>
        /// <param name="pUserid"></param>
        /// <returns></returns>
        public static List<LibrarySection> SelectListWithContributeAccess( int pLibraryId, int pUserid )
        {
            List<LibrarySection> collection = new List<LibrarySection>();

            try
            {
                DataSet ds = SelectWithContributeAccess( pLibraryId, pUserid );

                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        LibrarySection entity = FillLazy( dr );
                        collection.Add( entity );
                    }
                }
                return collection;
            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".SelectListWithContributeAccess() " );
                return null;

            }
        }//

        /// <summary>
        /// Select all collections in library where user has contribute access. Includes:
        /// - personal
        /// - libraries where has curate access
        /// - collections where has curate access
        /// - org libraries with implicit access
        /// </summary>
        /// <param name="pLibraryId"></param>
        /// <param name="pUserid"></param>
        /// <returns></returns>
        public static DataSet SelectWithContributeAccess( int pLibraryId, int pUserid )
        {

            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@LibraryId", pLibraryId );
            sqlParameters[ 1 ] = new SqlParameter( "@Userid", pUserid );

            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Library.SectionSelectCanContribute]", sqlParameters );

                    if ( ds.HasErrors )
                    {
                        return null;
                    }
                    return ds;
                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".SelectWithContributeAccess() " );
                    return null;

                }
            }
        }

        /// <summary>
        /// Search for collection related data using passed parameters
        /// - returns List
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static List<LibrarySection> SearchAsList( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            List<LibrarySection> list = new List<LibrarySection>();
            DataSet ds = Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
            if ( DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow row in ds.Tables[ 0 ].Rows )
                {
                    LibrarySection lib = Fill( row );
                    list.Add( lib );
                }
            }

            return list;
        }//

        /// <summary>
        /// Search for Library related data using passed parameters
        /// - uses custom paging
        /// - only requested range of rows will be returned
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
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

            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Library.SectionSearch]", sqlParameters );
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

        public static LibrarySection Fill( SqlDataReader dr )
        {
            LibrarySection entity = FillLazy( dr );

            entity.ParentLibrary = new LibraryManager().Get(entity.LibraryId);
            return entity;
        }//

        public static LibrarySection FillLazy( SqlDataReader dr )
        {
            LibrarySection entity = new LibrarySection();

            entity.IsValid = true;

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.LibraryId = GetRowColumn( dr, "LibraryId", 0 );
            entity.LibraryTitle = GetRowColumn( dr, "LibraryTitle", "" );
            entity.SectionTypeId = GetRowColumn( dr, "SectionTypeId", 0 );
            entity.SectionType = GetRowColumn( dr, "SectionType", "" );
            entity.Title = GetRowColumn( dr, "Title", "" );
			entity.FriendlyTitle = UtilityManager.UrlFriendlyTitle( entity.Title );
            entity.Description = GetRowColumn( dr, "Description", "" );

            entity.AreContentsReadOnly = GetRowColumn( dr, "AreContentsReadOnly", false );
            entity.IsDefaultSection = GetRowColumn( dr, "IsDefaultSection", false );
            //entity.IsPublic = GetRowColumn( dr, "IsPublic", false );
            entity.PublicAccessLevel = ( EObjectAccessLevel )GetRowColumn( dr, "PublicAccessLevel", 0 );
            entity.OrgAccessLevel = ( EObjectAccessLevel )GetRowColumn( dr, "OrgAccessLevel", 0 );
            if ( entity.PublicAccessLevel == 0 )
                entity.IsPublic = false;
            else
                entity.IsPublic = true;
            entity.ImageUrl = GetRowColumn( dr, "ImageUrl", "" );
            //entity.ParentId = GetRowPossibleColumn( dr, "ParentId", 0 );

            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.CreatedBy = GetRowPossibleColumn( dr, "CreatedBy", "" );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            entity.LastUpdatedBy = GetRowPossibleColumn( dr, "LastUpdatedBy", "" );
            string rowId = GetRowPossibleColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
            {
                entity.RowId = new Guid( rowId );
            }
            return entity;
        }//
        public static LibrarySection Fill( DataRow dr )
        {
            LibrarySection entity = FillLazy( dr );

            entity.ParentLibrary = new LibraryManager().Get( entity.LibraryId );
            return entity;
        }//

        public static LibrarySection FillLazy( DataRow dr )
        {
            LibrarySection entity = new LibrarySection();

            entity.IsValid = true;

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.LibraryId = GetRowColumn( dr, "LibraryId", 0 );
            entity.LibraryTitle = GetRowColumn( dr, "LibraryTitle", "" );
            entity.SectionTypeId = GetRowColumn( dr, "SectionTypeId", 0 );
            entity.SectionType = GetRowColumn( dr, "SectionType", "" );
            entity.Title = GetRowColumn( dr, "Title", "" );
			entity.FriendlyTitle = UtilityManager.UrlFriendlyTitle( entity.Title );
            entity.Description = GetRowColumn( dr, "Description", "" );

            entity.AreContentsReadOnly = GetRowColumn( dr, "AreContentsReadOnly", false );
            entity.IsDefaultSection = GetRowColumn( dr, "IsDefaultSection", false );
            //entity.IsPublic = GetRowColumn( dr, "IsPublic", false );
            entity.PublicAccessLevel = ( EObjectAccessLevel )GetRowColumn( dr, "PublicAccessLevel", 0 );
            entity.OrgAccessLevel = ( EObjectAccessLevel )GetRowColumn( dr, "OrgAccessLevel", 0 );
            if ( entity.PublicAccessLevel == 0 )
                entity.IsPublic = false;
            else
                entity.IsPublic = true;
            entity.ImageUrl = GetRowColumn( dr, "ImageUrl", "" );
            //entity.ParentId = GetRowPossibleColumn( dr, "ParentId", 0 );

            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.CreatedBy = GetRowPossibleColumn( dr, "CreatedBy", "" );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            entity.LastUpdatedBy = GetRowPossibleColumn( dr, "LastUpdatedBy", "" );

            string rowId = GetRowPossibleColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
            {
                entity.RowId = new Guid( rowId );
            }
            return entity;
        }//

        #endregion
        #region ====== Library.SectionMember ===============================================
        /// <summary>
        ///  Search for library section members
        /// </summary>
        public static List<LibraryMember> SectionMember_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            int newId = 0;
            List<LibraryMember> list = new List<LibraryMember>();
            LibraryMember item = new LibraryMember();
            int outputCol = 4;
            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", pOrderBy );
            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", pStartPageIndex );
            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", pMaximumRows );

            sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ outputCol ].Direction = ParameterDirection.Output;

            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "LibrarySectionMember_Search", sqlParameters );
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
                    else
                    {
                        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                        {
                            item = new LibraryMember();
                            item.Id = GetRowColumn( dr, "Id", 0 );
                            item.ParentId = GetRowColumn( dr, "LibraryId", 0 );
                            item.MemberTypeId = GetRowColumn( dr, "MemberTypeId", 0 );
                            item.MemberType = GetRowPossibleColumn( dr, "MemberType", "None" );

                            item.UserId = GetRowColumn( dr, "UserId", 0 );
                            item.MemberSortName = GetRowPossibleColumn( dr, "MemberSortName", "Missing" );
                            item.MemberName = GetRowPossibleColumn( dr, "MemberName", "Missing" );

                            item.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
                            item.CreatedById = GetRowPossibleColumn( dr, "CreatedById", 0 );
                            item.LastUpdated = GetRowPossibleColumn( dr, "LastUpdated", item.DefaultDate );
                            item.LastUpdatedById = GetRowPossibleColumn( dr, "LastUpdatedById", 0 );

                            string rowId = GetRowPossibleColumn( dr, "RowId", "" );
                            if ( rowId.Length > 35 )
                            {
                                item.RowId = new Guid( rowId );
                            }
                            list.Add( item );
                        }
                    }

                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".Member_Search() " );
                    return null;

                }
            }

            return list;


        }//

        #endregion
        #region ====== Comments ===============================================
        public static List<ObjectComment> SelectCollectionComments( int sectionId )
        {
            List<ObjectComment> collection = new List<ObjectComment>();
            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@sectionId", sectionId );

                    DataSet ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Library.SectionCommentSelect]", sqlParameters );
                    if ( DoesDataSetHaveRows( ds ) )
                    {
                        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                        {
                            ObjectComment entity = new ObjectComment();

                            entity.IsValid = true;

                            entity.Id = GetRowColumn( dr, "Id", 0 );
                            entity.ParentId = GetRowColumn( dr, "SectionId", 0 );
                            entity.Comment = GetRowColumn( dr, "Comment", "" );
                            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
                            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
                            entity.CreatedBy = GetRowColumn( dr, "FullName", "unknown" );
                            entity.LastName = GetRowPossibleColumn( dr, "LastName", "unknown" );
                            collection.Add( entity );
                        }
                    }
                    return collection;
                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".SelectLibraryComments() exception for sectionId: " + sectionId.ToString() );
                    return collection;
                }
            }
        }//
        #endregion
        #region ====== Likes ===============================================
        /// <summary>
        ///  Method to check if user has an entry for LibrarySectionLike
        /// </summary>
        public static ObjectLike GetLike( int sectionId, int userId )
        {
            ObjectLike entity = new ObjectLike();
            entity.HasLikeEntry = false;
            if ( userId == 0 || sectionId == 0 )
                return entity;
            try
            {
                using ( SqlConnection connection = new SqlConnection( ContentConnection() ) )
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 2 ];

                    sqlParameters[ 0 ] = new SqlParameter( "@SectionId", sectionId );
                    sqlParameters[ 1 ] = new SqlParameter( "@UserId", userId );


                    SqlDataReader dr = SqlHelper.ExecuteReader( connection, CommandType.StoredProcedure, "[Library.SectionLikeGet]", sqlParameters );
                    if ( dr.HasRows )
                    {
                        while ( dr.Read() )
                        {
                            entity.Id = GetRowColumn( dr, "Id", 0 );
                            entity.ParentId = GetRowColumn( dr, "SectionId", 0 );
                            entity.HasLikeEntry = true;
                            entity.IsLike = GetRowColumn( dr, "IsLike", false );
                            entity.Created = GetRowColumn( dr, "Created", entity.DefaultDate );
                            entity.CreatedById = GetRowColumn( dr, "CreatedById", userId );
                        }
                    }
                    dr.Close();
                    dr = null;
                }
               
            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + string.Format( ".GetLike() for sectionId: {0} and userId: {1}", sectionId, userId ) );
            }
            return entity;
        }//

        public static List<DataItem> LikeSummary( int sectionId )
        {
            List<DataItem> collection = new List<DataItem>();
            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@sectionId", sectionId );

                    DataSet ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Library.SectionLikeSummary]", sqlParameters );
                    if ( DoesDataSetHaveRows( ds ) )
                    {
                        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                        {
                            DataItem entity = new DataItem();
                            entity.ParentId = GetRowColumn( dr, "SectionId", 0 );

                            entity.Int1 = GetRowColumn( dr, "Likes", 0 );
                            entity.Int2 = GetRowColumn( dr, "Dislikes", 0 );
                            entity.Int3 = GetRowColumn( dr, "Total", 0 );

                            collection.Add( entity );
                        }
                    }
                    return collection;
                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".LikeSummary() exception for sectionId: " + sectionId.ToString() );
                    return collection;
                }
            }
        }//
        #endregion

    }
}