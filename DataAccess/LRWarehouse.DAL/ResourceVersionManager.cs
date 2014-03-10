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
using MyEntity = LRWarehouse.Business.ResourceVersion;

namespace LRWarehouse.DAL
{
    /// <summary>
    /// Data access manager for ResourceVersion
    /// </summary>
    public class ResourceVersionManager : BaseDataManager
    {
        static string thisClassName = "ResourceVersionManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[Resource.VersionGet]";
        const string DISPLAY_PROC = "[Resource.Version_Display]";
        const string SELECT_PROC = "[Resource.VersionSelect]";
        const string DELETE_PROC = "[Resource.VersionDelete]";
        const string INSERT_PROC = "[Resource.VersionInsert]";
        const string UPDATE_PROC = "[Resource.VersionUpdate]";
        const string UPDATEByRowid_PROC = "[Resource.VersionUpdateByRowId]";

        const string SEARCH_PROC = "[Resource_Search]";
        const string SEARCH_FT_PROC = "[Resource_Search_FT]";


        /// <summary>
        /// Default constructor
        /// </summary>
        public ResourceVersionManager()
        {

        }

        #region ====== Core Methods ===============================================
        /// <summary>
        /// Delete an ResourceVersion record using rowId
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
                LogError( ex, thisClassName + ".Delete() " );
                statusMessage = thisClassName + "- Unsuccessful: Delete(): " + ex.Message.ToString();

                successful = false;
            }
            return successful;
        }//

        /// <summary>
        /// Add an ResourceVersion record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int Create( MyEntity entity, ref string statusMessage )
        {
            string connectionString = LRWarehouse();
            //string newId = "";
            int newId = 0;
            string rowId = "";

            try
            {

                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[18];
                sqlParameters[0] = new SqlParameter("@ResourceId", SqlDbType.UniqueIdentifier);
                sqlParameters[0].Value = entity.ResourceId;
                sqlParameters[ 1 ] = new SqlParameter( "@DocId", entity.LRDocId);
                sqlParameters[ 2 ] = new SqlParameter( "@Title", entity.Title);
                sqlParameters[ 3 ] = new SqlParameter( "@Description", entity.Description);
                sqlParameters[ 4 ] = new SqlParameter( "@Publisher", entity.Publisher);
                sqlParameters[ 5 ] = new SqlParameter( "@Creator",  entity.Creator);
                sqlParameters[ 6 ] = new SqlParameter( "@Rights", entity.Rights);
                sqlParameters[ 7 ] = new SqlParameter( "@AccessRights", entity.AccessRights );
                sqlParameters[ 8 ] = new SqlParameter( "@Modified", SqlDbType.DateTime );
                System.DateTime date;
                if ( entity.Modified > entity.DefaultDate )
                    date = entity.Modified;
                else
                    date = DateTime.Now;
                sqlParameters[ 8 ].Value = date;
                sqlParameters[ 9 ] = new SqlParameter( "@Submitter", SqlDbType.VarChar );
                sqlParameters[ 9 ].Size = 100;
                sqlParameters[ 9 ].Value = entity.Submitter;
                sqlParameters[ 10 ] = new SqlParameter( "@Created", SqlDbType.DateTime );
                if ( entity.Created > entity.DefaultDate )
                    date = entity.Created;
                else
                    date = DateTime.Now;
                sqlParameters[ 10 ].Value = date;   // DateTime.Now;// entity.Created;
                sqlParameters[ 11 ] = new SqlParameter( "@TypicalLearningTime", SqlDbType.VarChar );
                sqlParameters[ 11 ].Size = 50;
                sqlParameters[ 11 ].Value = entity.TypicalLearningTime;
                sqlParameters[ 12 ] = new SqlParameter( "@IsSkeletonFromParadata", SqlDbType.Bit );
                sqlParameters[ 12 ].Value = entity.IsSkeletonFromParadata;
                sqlParameters[ 13 ] = new SqlParameter( "@Schema", entity.Schema );
                sqlParameters[ 14 ] = new SqlParameter( "@AccessRightsId", entity.AccessRightsId );
                sqlParameters[ 15 ] = new SqlParameter( "@InteractivityTypeId", entity.InteractivityTypeId );
                sqlParameters[ 16 ] = new SqlParameter( "@ResourceIntId", entity.ResourceIntId );
                sqlParameters[ 17 ] = new SqlParameter( "@Requirements", entity.Requirements );
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = int.Parse( dr[ 0 ].ToString() );
                    entity.Id = newId;
                    if ( dr.VisibleFieldCount > 1 )
                    {
                        rowId = dr[ 1 ].ToString();
                    }
                    //newId = dr[ 0 ].ToString();
                    //TODO - retrieve int Id, do temp get, then change procs!!
                    //MyEntity rv = Get( newId );
                    //entity.Id = rv.Id;
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                throw ex;
            }

            return newId;
        }

        /// <summary>
        /// Update an ResourceVersion record
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string UpdateById( MyEntity entity )
        {
            string message = "successful";
            using ( SqlConnection conn = new SqlConnection( this.ConnString ) )
            {

                try
                {

                    #region parameters
                    SqlParameter[] sqlParameters = new SqlParameter[ 9 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@Id", entity.Id );

                    sqlParameters[ 1 ] = new SqlParameter( "@Title", entity.Title );
                    sqlParameters[ 2 ] = new SqlParameter( "@Description", entity.Description );
                    sqlParameters[ 3 ] = new SqlParameter( "@Rights", entity.Rights );
                    sqlParameters[ 4 ] = new SqlParameter( "@AccessRightsId", entity.AccessRightsId );

                    sqlParameters[ 5 ] = new SqlParameter( "@TypicalLearningTime", entity.TypicalLearningTime );
                    sqlParameters[ 6 ] = new SqlParameter( "@Schema", entity.Schema );
                    //? how to publish an update to interactivity type
                    sqlParameters[ 7 ] = new SqlParameter( "@InteractivityTypeId", entity.InteractivityTypeId );

                    sqlParameters[ 8 ] = new SqlParameter( "@Requirements", entity.Requirements );

                    #endregion

                    SqlHelper.ExecuteNonQuery( conn, UPDATE_PROC, sqlParameters );
                    message = "successful";

                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + string.Format( ".Update() for Id: {0} and Title: {1}", entity.Id, entity.Title ) );
                    throw ex;
                }
            }

            return message;

        }//

        /// <summary>
        /// update by RowId - deprecated
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private string UpdateByRowId( MyEntity entity )
        {
            string message = "successful";
            string connectionString = LRWarehouse();

            try
            {

                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 10 ];
                sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
                sqlParameters[ 0 ].Value = entity.RowId;
                sqlParameters[ 1 ] = new SqlParameter( "@Title", entity.Title );
                sqlParameters[ 2 ] = new SqlParameter( "@Description", entity.Description );
                sqlParameters[ 3 ] = new SqlParameter( "@Rights", entity.Rights );
                sqlParameters[ 4 ] = new SqlParameter( "@AccessRights", entity.AccessRights );
                sqlParameters[ 5 ] = new SqlParameter( "@AccessRightsId", entity.AccessRightsId );

                sqlParameters[ 6 ] = new SqlParameter( "@TypicalLearningTime", entity.TypicalLearningTime );
                sqlParameters[ 7 ] = new SqlParameter( "@Schema", entity.Schema );
                //? how to publish an update to interactivity type
                sqlParameters[ 8 ] = new SqlParameter( "@InteractivityTypeId", entity.InteractivityTypeId );
                sqlParameters[ 9 ] = new SqlParameter( "@Modified", entity.Modified );

                #endregion

                SqlHelper.ExecuteNonQuery( connectionString, UPDATEByRowid_PROC, sqlParameters );
                message = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + string.Format( ".Update() for RowId: {0} and Title: {1}", entity.RowId.ToString(), entity.Title ) );
                throw ex;
            }

            return message;

        }//

        public string Update_LrDocId( MyEntity entity )
        {
            string message = "successful";
            string connectionString = LRWarehouse();

            try
            {

                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", entity.Id );
                sqlParameters[ 1 ] = new SqlParameter( "@DocId", entity.LRDocId );

                #endregion

                SqlHelper.ExecuteNonQuery( connectionString, "[Resource.VersionUpdateDocId]", sqlParameters );
                message = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + string.Format( ".Update_LrDocId() for Id: {0} and Title: {1}", entity.Id.ToString(), entity.Title ) );
                throw ex;
            }

            return message;

        }//

        /// <summary>
        /// Imports ResourceVersion from LR.  Handles both Insert and Update.  Insulated from standard Insert and Update methods.
        /// </summary>
        /// <param name="resourceVersion"></param>
        /// <returns>status</returns>
        public string Import( MyEntity resourceVersion )
        {
            string status = "successful";

            try
            {
                SqlParameter[] parameters = new SqlParameter[ 19 ];
                parameters[ 0 ] = new SqlParameter( "@RowId", resourceVersion.RowId );
                parameters[ 1 ] = new SqlParameter( "@ResourceId", resourceVersion.ResourceId );
                parameters[ 2 ] = new SqlParameter( "@DocId", resourceVersion.LRDocId );
                parameters[ 3 ] = new SqlParameter( "@Title", resourceVersion.Title );
                parameters[ 4 ] = new SqlParameter( "@Description", resourceVersion.Description );
                parameters[ 5 ] = new SqlParameter( "@Publisher", resourceVersion.Publisher );
                parameters[ 6 ] = new SqlParameter( "@Creator", resourceVersion.Creator );
                parameters[ 7 ] = new SqlParameter( "@Rights", resourceVersion.Rights );
                parameters[ 8 ] = new SqlParameter( "@AccessRights", resourceVersion.AccessRights );
                parameters[ 9 ] = new SqlParameter( "@Modified", resourceVersion.Modified );
                parameters[ 10 ] = new SqlParameter( "@Submitter", resourceVersion.Submitter );
                parameters[ 11 ] = new SqlParameter( "@Created", resourceVersion.Created );
                parameters[ 12 ] = new SqlParameter( "@TypicalLearningTime", resourceVersion.TypicalLearningTime );
                parameters[ 13 ] = new SqlParameter( "@IsSkeletonFromParadata", resourceVersion.IsSkeletonFromParadata );
                parameters[ 14 ] = new SqlParameter( "@Schema", resourceVersion.Schema );
                parameters[ 15 ] = new SqlParameter( "@AccessRightsId", resourceVersion.AccessRightsId );
                parameters[ 16 ] = new SqlParameter( "@InteractivityTypeId", resourceVersion.InteractivityTypeId );
                parameters[ 17 ] = new SqlParameter( "@InteractivityType", resourceVersion.InteractivityType );
                parameters[ 18 ] = new SqlParameter( "@ResourceIntId", resourceVersion.ResourceIntId );

                SqlHelper.ExecuteNonQuery( ConnString, CommandType.StoredProcedure, "[Resource.Version_Import]", parameters );
            }
            catch ( Exception ex )
            {
                LogError( "ResourceVersionManager.Import(): " + ex.ToString() );
                status = ex.ToString();
            }

            return status;
        }

        /// <summary>
        /// Toogle the active state of a resource version
        /// </summary>
        /// <param name="isActive"></param>
        /// <param name="resourceVersionId"></param>
        /// <returns></returns>
        public string SetActiveState( bool isActive, int resourceVersionId )
        {
            string status = "successful";
            try
            {
                SqlParameter[] arParms = new SqlParameter[ 2 ];
                arParms[ 0 ] = new SqlParameter( "@Id", resourceVersionId );
                arParms[ 1 ] = new SqlParameter( "@IsActive", isActive );

                SqlHelper.ExecuteNonQuery( ConnString, CommandType.StoredProcedure, "ResourceVersion_SetActiveState", arParms );
            }
            catch ( Exception ex )
            {
                LogError( thisClassName + ".SetResourceActiveState(): " + ex.ToString() );
                status = ex.Message;
            }
            return status;
        }

        #endregion

        #region ====== Retrieval Methods ===============================================

        /// <summary>
        /// Get ResourceVersion record via Id
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public MyEntity Get( int pId )
        {
            string connectionString = LRWarehouseRO();
            MyEntity entity = new MyEntity();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, "[Resource.VersionGetById]", sqlParameters );

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
                LogError( ex, thisClassName + ".Get(int pId) " );
                entity.Message = "Unsuccessful: " + thisClassName + ".Get(int pId): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//
        /// <summary>
        /// Retrieve by resourceIntId - will get most recent active RV
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public MyEntity GetByResourceId( int resourceId )
        {
            LRManager searchMgr = new LRManager();
            MyEntity entity = new MyEntity();
            try
            {
                string filter = string.Format( "(lr.[ResourceIntId] = {0} )", resourceId );
                int pTotalRows = 0;
                //order by msut be in select, and date is not, so just go with the flow, assuming dups will be removed in future
                string orderBy = "";    // "lr.Modified DESC";

                DataSet ds = searchMgr.Search( filter, orderBy, 1, 100, false, ref pTotalRows );

                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        entity = Fill( dr, false );
                        //only return one
                        break;
                    }
                }
                else
                {
                    entity.Message = "Record not found";
                    entity.IsValid = false;
                }

                ds.Dispose();
                ds = null;
                return entity;

            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".GetByResourceId(string resourceId):<br/> " + resourceId );
                return entity;

            }

        }//

        /// <summary>
        /// Get RB by url - it is possible to have multiple versions, so returning a list. The caller will have to handle.
        /// </summary>
        /// <param name="pUrl"></param>
        /// <returns></returns>
        public List<MyEntity> GetByUrl( string pUrl )
        {
            List<MyEntity> collection = new List<MyEntity>();
            LRManager searchMgr = new LRManager();
            try
            {
                string filter = string.Format( "(lr.[ResourceUrl] = '{0}')", pUrl );
                int pTotalRows = 0;
                string orderBy = "ResourceVersionIntId DESC ";
                DataSet ds = searchMgr.Search( filter, "", 1, 100, false, ref pTotalRows );

                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        MyEntity entity = Fill( dr, false );
                        collection.Add( entity );
                    }
                }

                return collection;

            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".GetByUrl(string pUrl):<br/> " + pUrl );
                return collection;

            }

        }//

        public MyEntity Get( string pRowId )
        {
            Guid id = new Guid( pRowId );
            return Get( id );
        }
        /// <summary>
        /// Get ResourceVersion record via rowId
        /// ===? should be obsolete, making private
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        private MyEntity Get( Guid pRowId )
        {
            string connectionString = LRWarehouseRO();
            MyEntity entity = new MyEntity();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@RowId", pRowId );
                //sqlParameters[ 1 ] = new SqlParameter( "@Id", 0 );
                //sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
                //sqlParameters[ 0 ].Value = new Guid( pRowId );

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
                LogError( ex, thisClassName + ".Get(string pRowId) " );
                entity.Message = "Unsuccessful: " + thisClassName + ".Get(string pId): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//

        /// <summary>
        /// Get ResourceVersion record and related data for a detailed display ==> OBSOLETE?
        /// ==> only referenced in ResourceBizService, but latter not in use
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public MyEntity Display( int pId )
        {
            string connectionString = LRWarehouseRO();
            MyEntity entity = new MyEntity();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );

                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, DISPLAY_PROC, sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.
                    while ( dr.Read() )
                    {
                        entity = Fill( dr, true );
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
                LogError( ex, thisClassName + ".Display() " );
                entity.Message = "Unsuccessful: " + thisClassName + ".Display(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//

        /// <summary>
        /// Select ResourceVersion related data using passed parameters
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public DataSet Select( string filter )
        {
            if ( filter.Length > 0 && filter.ToLower().IndexOf( "where" ) != 0 )
            {
                filter = "WHERE " + filter;
            }
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Filter", filter );
                DataSet ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, "[Resource.VersionSelect]", sqlParameters );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }

        /// <summary>
        /// ===> See LRManager.Search
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pOutputRelTables"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        private DataSet Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, bool pOutputRelTables, ref int pTotalRows )
        {
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

            using ( SqlConnection conn = new SqlConnection( LRWarehouseRO() ) )
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
            }	//Connection will autmatically be closed here always
        }

        #endregion

        #region ====== Helper Methods ===============================================
        /// <summary>
        /// Fill an ResourceVersion object from a DataRow
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="includeRelatedData"></param>
        /// <returns></returns>
        public MyEntity Fill( DataRow dr, bool includeRelatedData )
        {
            MyEntity entity = new MyEntity();

            entity.IsValid = true;

            string rowId = GetRowPossibleColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
                entity.RowId = new Guid( rowId );

            entity.Id = GetRowPossibleColumn( dr, "Id", 0 );
            if (entity.Id == 0)
                entity.Id = GetRowColumn( dr, "ResourceVersionIntId", 0 );

            //NEW - get integer version of resource id
            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );

            entity.Title = GetRowColumn( dr, "Title", "missing" );
            entity.Description = GetRowColumn( dr, "Description", "" );

            entity.IsActive = GetRowPossibleColumn( dr, "IsActive", true );
            //get parent url
            entity.ResourceUrl = GetRowColumn( dr, "ResourceUrl", "" );
            entity.ResourceIsActive = GetRowPossibleColumn( dr, "ResourceIsActive", true );

            entity.LRDocId = GetRowColumn( dr, "DocId", "" );
            entity.Publisher = GetRowColumn( dr, "Publisher", "" );
            entity.Creator = GetRowColumn( dr, "Creator", "" );
            entity.Submitter = GetRowColumn( dr, "Submitter", "" );
            entity.TypicalLearningTime = GetRowColumn( dr, "TypicalLearningTime", "" );

            entity.Rights = GetRowColumn( dr, "Rights", "" );
            entity.AccessRights = GetRowColumn( dr, "AccessRights", "" );
            entity.AccessRightsId = GetRowColumn( dr, "AccessRightsId", 0 );

            entity.InteractivityTypeId = GetRowColumn( dr, "InteractivityTypeId", 0 );
            entity.InteractivityType = GetRowColumn( dr, "InteractivityType", "" );

            entity.Modified = GetRowColumn( dr, "Modified", entity.DefaultDate );
            entity.Created = GetRowColumn( dr, "Created", entity.DefaultDate );
            entity.Imported = GetRowColumn( dr, "Imported", entity.DefaultDate );

            entity.SortTitle = GetRowColumn( dr, "SortTitle", "" );
            entity.Schema = GetRowColumn( dr, "Schema", "" );
            entity.Requirements = GetRowColumn( dr, "Requirements", "" );
            if ( includeRelatedData == true )
            {

                entity.Subjects = GetRowColumn( dr, "Subjects", "" );
                entity.EducationLevels = GetRowColumn( dr, "EducationLevels", "" );
                entity.Keywords = GetRowColumn( dr, "Keywords", "" );
                entity.LanguageList = GetRowColumn( dr, "LanguageList", "" );
                entity.ResourceTypesList = GetRowColumn( dr, "ResourceTypesList", "" );
                entity.AudienceList = GetRowColumn( dr, "AudienceList", "" );
                if ( entity.ResourceTypesList.Length > 0 )
                {
                    entity.ResourceTypesList = entity.ResourceTypesList.Replace( "&lt;", "<" );
                    entity.ResourceTypesList = entity.ResourceTypesList.Replace( "&gt;", ">" );
                }
            }

            return entity;
        }//


        /// <summary>
        /// Fill an ResourceVersion object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>MyEntity</returns>
        public MyEntity Fill( SqlDataReader dr, bool includeRelatedData )
        {
            MyEntity entity = new MyEntity();

            entity.IsValid = true;

            string rowId = GetRowPossibleColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
                entity.RowId = new Guid( rowId );

            entity.Id = GetRowPossibleColumn( dr, "Id", 0 );
            if ( entity.Id == 0 )
                entity.Id = GetRowColumn( dr, "ResourceVersionIntId", 0 );

            //NEW - get integer version of resource id
            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );

            entity.Title = GetRowColumn( dr, "Title", "missing" );
            entity.Description = GetRowColumn( dr, "Description", "" );

            entity.IsActive = GetRowPossibleColumn( dr, "IsActive", true );
            //get parent url
            entity.ResourceUrl = GetRowColumn( dr, "ResourceUrl", "" );
            entity.ResourceIsActive = GetRowPossibleColumn( dr, "ResourceIsActive", true );

            entity.LRDocId = GetRowColumn( dr, "DocId", "" );
            entity.Publisher = GetRowColumn( dr, "Publisher", "" );
            entity.Creator = GetRowColumn( dr, "Creator", "" );
            entity.Submitter = GetRowColumn( dr, "Submitter", "" );
            entity.TypicalLearningTime = GetRowColumn( dr, "TypicalLearningTime", "" );

            entity.Rights = GetRowColumn( dr, "Rights", "" );
            entity.AccessRights = GetRowColumn( dr, "AccessRights", "" );
            entity.AccessRightsId = GetRowColumn( dr, "AccessRightsId", 0 );

            entity.InteractivityTypeId = GetRowColumn( dr, "InteractivityTypeId", 0 );
            entity.InteractivityType = GetRowColumn( dr, "InteractivityType", "" );

            entity.Modified = GetRowColumn( dr, "Modified", entity.DefaultDate );
            entity.Created = GetRowColumn( dr, "Created", entity.DefaultDate );
            entity.Imported = GetRowColumn( dr, "Imported", entity.DefaultDate );

            entity.SortTitle = GetRowColumn( dr, "SortTitle", "" );
            entity.Schema = GetRowColumn( dr, "Schema", "" );
            entity.Requirements = GetRowColumn( dr, "Requirements", "" );
            if ( includeRelatedData == true )
            {

                entity.Subjects = GetRowColumn( dr, "Subjects", "" );
                entity.EducationLevels = GetRowColumn( dr, "EducationLevels", "" );
                entity.Keywords = GetRowColumn( dr, "Keywords", "" );
                entity.LanguageList = GetRowColumn( dr, "LanguageList", "" );
                entity.ResourceTypesList = GetRowColumn( dr, "ResourceTypesList", "" );
                entity.AudienceList = GetRowColumn( dr, "AudienceList", "" );
                if ( entity.ResourceTypesList.Length > 0 )
                {
                    entity.ResourceTypesList = entity.ResourceTypesList.Replace( "&lt;", "<" );
                    entity.ResourceTypesList = entity.ResourceTypesList.Replace( "&gt;", ">" );
                }
            }

            return entity;
        }//

        public int ConvertInteractivityTypeToId( string resourceId, string interactivityType )
        {
            int retVal = 0;

            try
            {
                SqlParameter[] parameters = new SqlParameter[ 2 ];
                parameters[ 0 ] = new SqlParameter( "@ResourceId", resourceId );
                parameters[ 1 ] = new SqlParameter( "@InteractivityType", interactivityType );

                DataSet ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, "[ConvertInteractivityTypeToId]", parameters );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    retVal = GetRowColumn( ds.Tables[ 0 ].Rows[ 0 ], "InteractivityTypeId", 0 );
                }
            }
            catch ( Exception ex )
            {
                LogError( "ResourceVersionManager.ConvertInteractivityTypeToId(): " + ex.ToString() );
            }

            return retVal;
        }
        #endregion

    }
}
