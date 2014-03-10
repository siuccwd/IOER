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

namespace ILPathways.DAL
{
    public class LibraryManager : BaseDataManager
	{
		static string thisClassName = "LibraryManager";

        public static int LIBRARY_TYPE_PERSONAL_ID = 1;
        public static int LIBRARY_TYPE_ORGANZATION_ID = 2;

        public static int MY_EVALUATIONS_LIBRARY_SECTION_ID = 1;
        public static int MY_AUTHORED_LIBRARY_SECTION_ID = 2;
        public static int GENERAL_LIBRARY_SECTION_ID = 3;
		/// <summary>
		/// Base procedures
		/// </summary>
		const string GET_PROC = "[LibraryGet]";
        const string SEARCH_PROC = "[LibrarySearch]";
        
		const string DELETE_PROC = "[LibraryDelete]";
		const string INSERT_PROC = "[LibraryInsert]";
		const string UPDATE_PROC = "[LibraryUpdate]";


		/// <summary>
		/// Default constructor
		/// </summary>
		public LibraryManager()
		{ }//

		#region ====== Core Methods ===============================================
		/// <summary>
		/// Delete an Library record
		/// </summary>
		/// <param name="pId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
        public bool Delete( int pId, ref string statusMessage )
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
				LogError( ex, thisClassName + ".Delete() " );
				statusMessage = thisClassName + "- Unsuccessful: Delete(): " + ex.Message.ToString();

				successful = false;
			}
			return successful;
		}//

        /// <summary>
        /// Add a personal Library 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public Library CreateMyLibrary( IWebUser user, ref string statusMessage )
        {
            Library entity = new Library();
            entity.Title = user.FullName() + "' - Personal library";
            entity.Description = "Personal library for " + user.FullName();
            entity.LibraryTypeId = LIBRARY_TYPE_PERSONAL_ID;
            entity.IsActive = true;
            entity.IsDiscoverable = true;
            entity.IsPublic = true;
            entity.PublicAccessLevel = EObjectAccessLevel.ReadOnly;
            entity.OrgAccessLevel = EObjectAccessLevel.ContributeWithApproval;
            entity.CreatedById = user.Id;
           

            int libId = Create( entity, ref statusMessage );
            entity.Id = libId;
            if ( libId > 0 )
            {

            }
            //would it be logical to also create the default section??
            //the caller may want to do the default insert?
            //perhaps create, and store in library. Current section, or default

            return entity;
		}//

		/// <summary>
		/// Add an Library record
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
        public int Create( Library entity, ref string statusMessage )
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
                    if (entity.LibraryTypeId == LIBRARY_TYPE_PERSONAL_ID)
                        entity.OrgAccessLevel = EObjectAccessLevel.ReadOnly;
                    else
                        entity.OrgAccessLevel = EObjectAccessLevel.ContributeWithApproval;
                }
                if ( entity.PublicAccessLevel > 0 )
                    entity.IsDiscoverable = true;

                SqlParameter[] sqlParameters = new SqlParameter[ 10 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Title", entity.Title );
                sqlParameters[ 1 ] = new SqlParameter( "@Description", entity.Description );
                sqlParameters[ 2 ] = new SqlParameter( "@LibraryTypeId", entity.LibraryTypeId );
                sqlParameters[ 3 ] = new SqlParameter( "@IsDiscoverable", entity.IsDiscoverable );
                sqlParameters[ 4 ] = new SqlParameter( "@PublicAccessLevel", ( int )entity.PublicAccessLevel );
                sqlParameters[ 5 ] = new SqlParameter( "@OrgAccessLevel", ( int )entity.OrgAccessLevel );
                sqlParameters[ 6 ] = new SqlParameter( "@CreatedById", entity.CreatedById );
                sqlParameters[ 7 ] = new SqlParameter( "@ImageUrl", entity.ImageUrl );
                sqlParameters[ 8 ] = new SqlParameter( "@OrgId", entity.OrgId );
                if (entity.HasValidRowId() == false)
                    entity.RowId = Guid.NewGuid();

                sqlParameters[ 9 ] = new SqlParameter( "@RowId", entity.RowId );


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
			    LogError( ex, thisClassName + string.Format( ".Create() for title: {0} and CreatedBy: {1}", entity.Title, entity.CreatedById ) );	
				statusMessage = thisClassName + "- Unsuccessful: Create(): " + ex.Message.ToString();
				
				entity.Message = statusMessage;
				entity.IsValid = false;
			}

			return newId;
		}

		/// <summary>
		/// Update an Library record
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
        public string Update( Library entity )
		{
			string message = "successful";

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
                    if ( entity.LibraryTypeId == LIBRARY_TYPE_PERSONAL_ID )
                        entity.OrgAccessLevel = EObjectAccessLevel.ReadOnly;
                    else
                        entity.OrgAccessLevel = EObjectAccessLevel.ContributeWithApproval;
                }
                SqlParameter[] sqlParameters = new SqlParameter[ 8 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", entity.Id );
                sqlParameters[ 1 ] = new SqlParameter( "@Title", entity.Title );
                sqlParameters[ 2 ] = new SqlParameter( "@Description", entity.Description );
                sqlParameters[ 3 ] = new SqlParameter( "@IsDiscoverable", entity.IsDiscoverable );
                sqlParameters[ 4 ] = new SqlParameter( "@PublicAccessLevel", ( int )entity.PublicAccessLevel );
                sqlParameters[ 5 ] = new SqlParameter( "@OrgAccessLevel", ( int )entity.OrgAccessLevel );
                sqlParameters[ 6 ] = new SqlParameter( "@LastUpdatedById", entity.LastUpdatedById );
                sqlParameters[ 7 ] = new SqlParameter( "@ImageUrl", entity.ImageUrl );

				#endregion

				SqlHelper.ExecuteNonQuery( ContentConnection(), UPDATE_PROC, sqlParameters );
				message = "successful";

			}
			catch ( Exception ex )
			{
				LogError( ex, thisClassName + string.Format( ".Update() for Id: {0}, title: {1} and LastUpdatedBy: {2}", entity.Id.ToString(), entity.Title, entity.LastUpdatedBy ) );

				message = thisClassName + "- Unsuccessful: Update(): " + ex.Message.ToString();
				entity.Message = message;
				entity.IsValid = false;
			}

			return message;

		}//
		#endregion

		#region ====== Retrieval Methods ===============================================
		/// <summary>
		/// Get Library record
		/// </summary>
		/// <param name="pId"></param>
		/// <returns></returns>
        public Library Get( int pId )
		{
		    return Get( pId, "" );
		}//
        public Library GetByRowId( string pRowId )
        {
            return Get( 0, pRowId );

        }//

        private Library Get( int pId, string pRowId )
        {
            string connectionString = ContentConnectionRO();
            Library entity = new Library();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@id", pId);
                sqlParameters[ 1 ] = new SqlParameter( "@RowId", pRowId );


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
                LogError( ex, thisClassName + ".Get() " );
                entity.Message = "Unsuccessful: " + thisClassName + ".Get(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//

        public Library GetMyLibrary( IWebUser user )
        {
            return GetMyLibrary( user, false );
		}//

        public Library GetMyLibrary( IWebUser user, bool createIfMissing )
        {
            string connectionString = ContentConnectionRO();
            Library entity = new Library();
            string statusMessage = "";
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@CreatedById", user.Id );


                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, "[Library.GetMyLibrary]", sqlParameters );

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
                        entity = CreateMyLibrary( user, ref statusMessage );
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
                LogError( ex, thisClassName + ".Get() " );
                entity.Message = "Unsuccessful: " + thisClassName + ".Get(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//
        public Library GetMyOrgLibrary( IWebUser user, bool createIfMissing )
        {
            string connectionString = ContentConnectionRO();
            Library entity = new Library();
            string statusMessage = "";
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@OrgId", user.OrgId );

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, "[Library.GetMyOrgLibrary]", sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record, maybe multiple in the future??
                    while ( dr.Read() )
                    {
                        entity = Fill( dr );
                    }
                }
                else
                {
                    if ( createIfMissing )
                    {
                        entity = CreateMyLibrary( user, ref statusMessage );
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
                LogError( ex, thisClassName + ".Get() " );
                entity.Message = "Unsuccessful: " + thisClassName + ".Get(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }
  }//

       
        /// <summary>
        /// Determine if user has edit access to the library
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="pUserid"></param>
        /// <returns></returns>
        public bool DoesUserHaveEditAccess( int libraryId, int pUserid )
        {
            bool canAccess = false;
            List<Library> list = Library_SelectListWithEditAccess( libraryId, pUserid );

            //List<Library> list = Library_SelectListWithEditAccess( pUserid );
            if ( list == null || list.Count == 0 )
                return canAccess;
            else
            {
                foreach ( Library l in list )
                {
                    if ( l.Id == libraryId) {
                        canAccess = true;
                        break;
                    }
                }
            }
            return canAccess;
        }//


        public List<Library> Library_SelectListWithEditAccess( int pUserid )
        {
            return Library_SelectListWithEditAccess( 0, pUserid );
        }//

        /// <summary>
        /// Select all libraries as LIST where user has edit access. Includes:
        /// - personal
        /// - libraries where has curate access
        /// - collections where has curate access
        /// - org libraries with implicit access
        /// </summary>
        /// <param name="pLibraryId"></param>
        /// <param name="pUserid"></param>
        /// <returns></returns>
        public List<Library> Library_SelectListWithEditAccess( int libraryId, int pUserid )
        {
            List<Library> list = new List<Library>();
            try
            {
                DataSet ds = Library_SelectWithEditAccess( pUserid, libraryId );

                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        Library entity = Fill( dr );
                        list.Add( entity );
                    }
                }
                return list;
            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".Library_SelectListWithEditAccess() " );
                return null;

            }
        }//
		/// <summary>
		/// Select all libraries where user has edit access. Includes:
        /// - personal
        /// - libraries where has curate access
        /// - collections where has curate access
        /// - org libraries with implicit access
		/// </summary>
        /// <param name="pUserid"></param>
		/// <returns></returns>
        public DataSet Library_SelectWithEditAccess( int pUserid, int libraryId )
		{

			SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Userid",pUserid);
            sqlParameters[ 1 ] = new SqlParameter( "@libraryId", libraryId );

            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Library.SelectCanEdit]", sqlParameters );

                    if ( ds.HasErrors )
                    {
                        return null;
                    }
                    return ds;
                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".SelectLibrariesWithEditAccess() " );
                    return null;

                }
            }
		}


        public bool DoesUserHaveContributeAccess( int libraryId, int pUserid )
        {
            bool canAccess = false;
            List<Library> list = SelectListWithContributeAccess( pUserid, libraryId );

            //List<Library> list = Library_SelectListWithEditAccess( pUserid );
            if ( list == null || list.Count == 0 )
                return canAccess;
            else
            {
                foreach ( Library l in list )
                {
                    if ( l.Id == libraryId )
                    {
                        canAccess = true;
                        break;
                    }
                }
            }
            return canAccess;
        }//

        public List<Library> SelectListWithContributeAccess( int pUserid )
        {
            return SelectListWithContributeAccess( pUserid, 0 );
        }//
        /// <summary>
        /// Select all libraries as LIST where user has contribute access. Includes:
        /// - personal
        /// - libraries where has curate access
        /// - collections where has curate access
        /// - org libraries with implicit access
        /// </summary>
        /// <param name="pLibraryId"></param>
        /// <param name="pUserid"></param>
        /// <returns></returns>
        public List<Library> SelectListWithContributeAccess( int pUserid, int libraryId )
        {
            List<Library> list = new List<Library>();
            try
            {
                DataSet ds = SelectWithContributeAccess( pUserid, libraryId );

                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        Library entity = Fill( dr );
                        list.Add( entity );
                    }
                }
                return list;
            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".SelectListWithContributeAccess() " );
                return null;

            }
        }//
        /// <summary>
        /// Select all libraries where user has contribute access. Includes:
        /// - personal
        /// - libraries where has curate access
        /// - collections where has curate access
        /// - org libraries with implicit access
        /// </summary>
        /// <param name="pUserid"></param>
        /// <returns></returns>
        public DataSet SelectWithContributeAccess( int pUserid, int libraryId )
        {

            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Userid", pUserid );
            sqlParameters[ 1 ] = new SqlParameter( "@libraryId", libraryId );

            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Library.SelectCanContribute]", sqlParameters );

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
        /// Retrieve all libraries containing the resource
        /// </summary>
        /// <param name="resourceIntId"></param>
        /// <returns></returns>
        public List<Library> AllLibrariesWithResource( int resourceIntId )
        {
            List<Library> collection = new List<Library>();
            try
            {
                int pTotalRows = 0;
                string pFilter = string.Format( "(lib.Id in (select LibraryId from [Library.SectionResourceSummary] res where res.ResourceIntId = {0})) ", resourceIntId );

                DataSet ds = Search( pFilter, "", 1, 100, ref pTotalRows );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        Library entity = Fill( dr );
                        collection.Add( entity );
                    }
                }
                return collection;
            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".AllLibrariesWithResource() exception for resourceIntId: " + resourceIntId.ToString() );
                return collection;
            }
        }//

        /// <summary>
        /// Search for Library related data using passed parameters
        /// - returns List
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public List<Library> SearchAsList( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            List<Library> list = new List<Library>();
            DataSet ds = Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
            if ( DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow row in ds.Tables[ 0 ].Rows )
                {
                    Library lib = Fill( row );
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
        public DataSet Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
		{

            int outputCol = 4;
			SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
			sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", pOrderBy );
            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", pStartPageIndex );
			sqlParameters[ 3 ] = new SqlParameter( "@PageSize", pMaximumRows);

            sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ outputCol ].Direction = ParameterDirection.Output;

            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
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

		/// <summary>
		/// Fill an Library object from a SqlDataReader
		/// </summary>
		/// <param name="dr">SqlDataReader</param>
		/// <returns>Library</returns>
        public Library Fill( SqlDataReader dr )
		{
			Library entity = new Library();

			entity.IsValid = true;

			entity.Id = GetRowColumn( dr, "Id", 0 );
			entity.Title = GetRowColumn( dr, "Title", "missing" );
            entity.Description = GetRowColumn( dr, "Description", "missing" );
            
            entity.IsDiscoverable = GetRowColumn( dr, "IsDiscoverable", false );
			entity.IsActive = GetRowColumn( dr, "IsActive", false );
            entity.ImageUrl = GetRowColumn( dr, "ImageUrl", "" );

            entity.LibraryTypeId = GetRowColumn( dr, "LibraryTypeId", 0 );
            entity.LibraryType = GetRowColumn( dr, "LibraryType", "" );
            //entity.IsPublic = GetRowColumn( dr, "IsPublic", false );
            entity.PublicAccessLevel = ( EObjectAccessLevel )GetRowColumn( dr, "PublicAccessLevel", 0 );
            entity.OrgAccessLevel = ( EObjectAccessLevel )GetRowColumn( dr, "OrgAccessLevel", 0 );
            if ( entity.PublicAccessLevel == 0 )
                entity.IsPublic = false;
            else
                entity.IsPublic = true;

            entity.OrgId = GetRowColumn( dr, "OrgId", 0 );
            entity.Organization = GetRowPossibleColumn( dr, "Organization", "" );

			entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
			entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
			entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
			entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            string rowId = GetRowPossibleColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
            {
                entity.RowId = new Guid( rowId );
            }

            //rowId = GetRowPossibleColumn( dr, "DocumentRowId", "" );
            //if ( rowId.Length > 35 )
            //{
            //    entity.DocumentRowId = new Guid( rowId );
            //    entity.RelatedDocument = DocumentStoreManager.Get( rowId );
            //}
			return entity;
		}//
        public Library Fill( DataRow dr )
        {
            Library entity = new Library();

            entity.IsValid = true;

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.Title = GetRowColumn( dr, "Title", "missing" );
            entity.Description = GetRowColumn( dr, "Description", "missing" );
        

            entity.IsDiscoverable = GetRowColumn( dr, "IsDiscoverable", false );
            entity.IsActive = GetRowPossibleColumn( dr, "IsActive", true );
            entity.ImageUrl = GetRowColumn( dr, "ImageUrl", "" );

            entity.LibraryTypeId = GetRowColumn( dr, "LibraryTypeId", 0 );
            entity.LibraryType = GetRowColumn( dr, "LibraryType", "" );

            //entity.IsPublic = GetRowColumn( dr, "IsPublic", false );
            entity.PublicAccessLevel = ( EObjectAccessLevel )GetRowColumn( dr, "PublicAccessLevel", 0 );
            entity.OrgAccessLevel = ( EObjectAccessLevel )GetRowColumn( dr, "OrgAccessLevel", 0 );
            if ( entity.PublicAccessLevel == 0 )
                entity.IsPublic = false;
            else
                entity.IsPublic = true;
 

            entity.OrgId = GetRowColumn( dr, "OrgId", 0 );
            entity.Organization = GetRowPossibleColumn( dr, "Organization", "" );

            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            string rowId = GetRowPossibleColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
            {
                entity.RowId = new Guid( rowId );
            }

            return entity;
        }//

        #endregion

        #region ====== Library.Member ===============================================
        /// <summary>
        ///  Search for library members
        /// </summary>
        public List<LibraryMember> LibraryMember_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
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
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "LibraryMember_Search", sqlParameters );
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
                    LogError( ex, thisClassName + ".Search() " );
                    return null;

                }
            }

            return list;


        }//

        #endregion


        #region ====== Types ===============================================
        public DataSet SelectLibraryTypes()
        {
            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( ContentConnectionRO(), CommandType.StoredProcedure, "[Library.TypeSelect]" );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".SelectLibraryTypes() " );
                return null;

            }
        }
        public DataSet SelectLibrarySectionTypes()
        {
            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( ContentConnectionRO(), CommandType.StoredProcedure, "[Library.SectionTypeSelect]" );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".SelectLibrarySectionTypes() " );
                return null;

            }
        }

        public DataSet SelectSubscriptionTypes()
        {
            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( ContentConnectionRO(), CommandType.StoredProcedure, "[Codes.SubscriptionTypeSelect]" );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".SelectSubscriptionTypes() " );
                return null;

            }
        }
        #endregion

        #region ====== Comments ===============================================
        /// <summary>
        ///  Method to create LibraryComment object
        /// </summary>
        public int CreateComment( ObjectComment entity, ref string statusMessage )
        {
            int newId = 0;
            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];

                sqlParameters[ 0 ] = new SqlParameter( "@LibraryId", entity.ParentId );
                sqlParameters[ 1 ] = new SqlParameter( "@Comment", entity.Comment );
                sqlParameters[ 2 ] = new SqlParameter( "@CreatedById", entity.CreatedById );
                #endregion
                SqlDataReader dr = SqlHelper.ExecuteReader( ContentConnection(), CommandType.StoredProcedure, "[Library.CommentInsert]", sqlParameters );
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
                LogError( ex, thisClassName + string.Format( ".CreateComment() for ParentId: {0} and CreatedBy: {1}", entity.ParentId, entity.CreatedById ) );
                statusMessage = thisClassName + "- Unsuccessful: Create(): " + ex.Message.ToString();

                entity.Message = statusMessage;
                entity.IsValid = false;
            }

            return newId;


        }//
        public int CreateComment( int libraryId, string comment, int createdById )
        {
            int newId = 0;
            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];

                sqlParameters[ 0 ] = new SqlParameter( "@LibraryId", libraryId );
                sqlParameters[ 1 ] = new SqlParameter( "@Comment", comment );
                sqlParameters[ 2 ] = new SqlParameter( "@CreatedById", createdById );
                #endregion
                SqlDataReader dr = SqlHelper.ExecuteReader( ContentConnection(), CommandType.StoredProcedure, "[Library.CommentInsert]", sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = int.Parse( dr[ 0 ].ToString() );
                }
                dr.Close();
                dr = null;

            }
            catch ( Exception ex )
            {
                //provide helpful info about failing entity
                LogError( ex, thisClassName + string.Format( ".CreateComment() for libraryId: {0} and CreatedBy: {1}", libraryId, createdById ) );

            }

            return newId;


        }//
        public List<ObjectComment> SelectLibraryComments( int libraryId )
        {
            List<ObjectComment> collection = new List<ObjectComment>();
            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@libraryId", libraryId );

                    DataSet ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Library.CommentSelect]", sqlParameters );
                    if ( DoesDataSetHaveRows( ds ) )
                    {
                        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                        {
                            ObjectComment entity = new ObjectComment();

                            entity.IsValid = true;

                            entity.Id = GetRowColumn( dr, "Id", 0 );
                            entity.ParentId = GetRowColumn( dr, "LibraryId", 0 );
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
                    LogError( ex, thisClassName + ".SelectLibraryComments() exception for libraryId: " + libraryId.ToString() );
                    return collection;
                }
            }
        }//
        #endregion

        #region ====== Likes ===============================================
        /// <summary>
        ///  Method to check if user has an entry for LibraryLike
        /// </summary>
        public ObjectLike GetLike( int libraryId, int userId )
        {
            ObjectLike entity = new ObjectLike();
            entity.HasLikeEntry = false;
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];

                sqlParameters[ 0 ] = new SqlParameter( "@LibraryId", libraryId );
                sqlParameters[ 1 ] = new SqlParameter( "@UserId", userId );


                SqlDataReader dr = SqlHelper.ExecuteReader( ContentConnection(), CommandType.StoredProcedure, "[Library.LikeGet]", sqlParameters );
                if ( dr.HasRows )
                {
                    while ( dr.Read() )
                    {
                        entity.Id = GetRowColumn( dr, "Id", 0 );
                        entity.ParentId = GetRowColumn( dr, "LibraryId", 0 );
                        entity.HasLikeEntry = true;
                        entity.IsLike = GetRowColumn( dr, "IsLike", false );
                        entity.Created = GetRowColumn (dr, "Created", entity.DefaultDate);
                        entity.CreatedById = GetRowColumn( dr, "CreatedById", userId );
                    }

                }

            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + string.Format( ".GetLike() for libraryId: {0} and userId: {1}", libraryId, userId ) );
            }

            return entity;

        }//
        public List<DataItem> LikeSummary( int libraryId )
        {
            List<DataItem> collection = new List<DataItem>();
            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@libraryId", libraryId );

                    DataSet ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Library.LikeSummary]", sqlParameters );
                    if ( DoesDataSetHaveRows( ds ) )
                    {
                        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                        {
                            DataItem entity = new DataItem();
                            entity.ParentId = GetRowColumn( dr, "libraryId", 0 );

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
                    LogError( ex, thisClassName + ".LikeSummary() exception for libraryId: " + libraryId.ToString() );
                    return collection;
                }
            }
        }//
        #endregion

        #region ====== other ===============================================
        /// <summary>
        /// Return list of values for passed code table for which resources actually exist in the referenced library
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="pCodeTableSuffix"></param>
        /// <returns></returns>
        public List<DataItem> AvailableFiltersForLibrary( int libraryId, string pCodeTableSuffix )
        {
            string pCodeTable = string.Format("dbo.[Codes.{0}]", pCodeTableSuffix);
            string pResourceChildTable = string.Format( "[Resource.{0}]", pCodeTableSuffix );
            string resForeignKeyName = string.Format( "{0}Id", pCodeTableSuffix );

            return AvailableFiltersForLIbraryOrSection( libraryId, 0, pCodeTable, pResourceChildTable, resForeignKeyName );

        }
        public List<DataItem> AvailableFiltersForLibrary( int libraryId, string pCodeTable, string pResourceChildTable, string resForeignKeyName )
        {
            return AvailableFiltersForLIbraryOrSection( libraryId, 0, pCodeTable, pResourceChildTable, resForeignKeyName );
            
        }

        /// <summary>
        /// Return list of values for passed code table for which resources actually exist in the referenced collection
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="pCodeTableSuffix"></param>
        /// <returns></returns>
        public List<DataItem> AvailableFiltersForSection( int sectionId, string pCodeTableSuffix )
        {
            string pCodeTable = string.Format( "dbo.[Codes.{0}]", pCodeTableSuffix );
            string pResourceChildTable = string.Format( "[Resource.{0}]", pCodeTableSuffix );
            string resForeignKeyName = string.Format( "{0}Id", pCodeTableSuffix );

            return AvailableFiltersForLIbraryOrSection( 0, sectionId, pCodeTable, pResourceChildTable, resForeignKeyName );

        }
        public List<DataItem> AvailableFiltersForSection( int sectionId, string pCodeTable, string pResourceChildTable, string resForeignKeyName )
        {
            return AvailableFiltersForLIbraryOrSection( 0, sectionId, pCodeTable, pResourceChildTable, resForeignKeyName );
        }

        private List<DataItem> AvailableFiltersForLIbraryOrSection( int libraryId, int sectionId, string pCodeTable, string pResourceChildTable, string resForeignKeyName )
        {
            List<DataItem> collection = new List<DataItem>();
            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@CodeTable", pCodeTable );
                    sqlParameters[ 1 ] = new SqlParameter( "@ResourceChildTable", pResourceChildTable );
                    sqlParameters[ 2 ] = new SqlParameter( "@FKey", resForeignKeyName );
                    sqlParameters[ 3 ] = new SqlParameter( "@libraryId", libraryId );
                    sqlParameters[ 4 ] = new SqlParameter( "@LibrarySectionId", sectionId );


                    DataSet ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[LibrarySearch_SelectUsedValuesForFilter]", sqlParameters );
                    if ( DoesDataSetHaveRows( ds ) )
                    {
                        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                        {
                            DataItem entity = new DataItem();

                            entity.IsValid = true;

                            entity.Id = GetRowColumn( dr, "Id", 0 );
                            entity.Title = GetRowColumn( dr, "Title", "" );
                            collection.Add( entity );
                        }
                    }
                    return collection;
                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".AvailableFiltersForLibrary() exception for libraryId: " + libraryId.ToString() );
                    return collection;
                }
            }
        }

        #endregion


    }
}