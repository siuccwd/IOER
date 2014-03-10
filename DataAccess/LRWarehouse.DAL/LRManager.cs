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

using MyEntity = LRWarehouse.Business.Resource;
using LRWarehouse.Business;

namespace LRWarehouse.DAL
{
    /// <summary>
    /// Data access manager for Resource
    /// </summary>
    public class LRManager : BaseDataManager
    {
        static string className = "LRManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string SEARCH_PROC = "[Resource_Search]";
        const string SEARCH_FT_PROC = "[Resource_Search_FT]";

        /// <summary>
        /// Default constructor
        /// </summary>
        public LRManager()
        { }//

        #region ====== Core Methods ===============================================
        //see ResourceManager for all CRUD (other than search) methods
        #endregion

        #region ====== Retrieval Methods ===============================================
        
        /// <summary>
        /// Search for Resource related data using passed parameters
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
            bool pOutputRelTables = false;
            return Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, pOutputRelTables, ref pTotalRows );
        }

        /// <summary>
        /// Search for Resource related data using passed parameters
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
                    string rows = sqlParameters[ 5 ].Value.ToString();
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
                    throw ex;
                    //LogError( ex, className + ".Search() " );
                    //return null;

                }
            }
        }

        public DataSet Search_FullText( string pFilter, string pKeywords, string pOrderBy, int pStartPageIndex, int pMaximumRows, bool pOutputRelTables, ref int pTotalRows )
        {
            int outputCol = 6;

            using ( SqlConnection conn = new SqlConnection( LRWarehouseRO() ) )
            {
                
                DataSet ds = new DataSet();
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 7 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
                    sqlParameters[ 1 ] = new SqlParameter( "@Keywords", pKeywords );
                    sqlParameters[ 2 ] = new SqlParameter( "@SortOrder", pOrderBy );

                    sqlParameters[ 3 ] = new SqlParameter( "@StartPageIndex", pStartPageIndex );

                    sqlParameters[ 4 ] = new SqlParameter( "@PageSize", pMaximumRows );
                    sqlParameters[ 5 ] = new SqlParameter( "@OutputRelTables", pOutputRelTables );

                    sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
                    sqlParameters[ outputCol ].Direction = ParameterDirection.Output;

                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, SEARCH_FT_PROC, sqlParameters  );
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
                    LogError( ex, className + ".Search_FullText() " );
                    return null;

                }
            }
        }

        public List<ResourceVersion> SearchToList( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, bool pOutputRelTables, ref int pTotalRows )
        {
            //List<MyEntity> list = default( List<MyEntity> );
            List<ResourceVersion> list = new List<ResourceVersion>();


            int outputCol = 5;

            SqlParameter[] sqlParameters = new SqlParameter[ 6 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", pOrderBy );

            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", pStartPageIndex );
            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", pMaximumRows );

            sqlParameters[ 4 ] = new SqlParameter( "@OutputRelTables", pOutputRelTables );

            sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ outputCol ].Direction = ParameterDirection.Output;

            using ( SqlConnection conn = new SqlConnection( LRWarehouseRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, SEARCH_PROC, sqlParameters );

                    if ( DoesDataSetHaveRows( ds ) == false || ds.HasErrors )
                    {
                        return null;
                    }
                    else
                    {
                        string rows = sqlParameters[ outputCol ].Value.ToString();
                        try
                        {
                            pTotalRows = Int32.Parse( rows );
                        }
                        catch
                        {
                            pTotalRows = 0;
                        }
                        ResourceVersionManager rvmgr = new ResourceVersionManager();
                        foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
                        {
                            ResourceVersion row = rvmgr.Fill( dr, true );

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
        }

        #endregion

        #region ====== Search Related Methods ===============================================
        public DataSet CreatorSearch( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            //***** delete if not using custom paging *******
            string connectionString = GetReadOnlyConnection();
            string searchProcedure = "[CreatorSearch]";
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
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, searchProcedure, sqlParameters );
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
                LogError( ex, className + ".PublisherSearch() " );
                return null;

            }
        }
        public DataSet ResourceType_SearchCounts( string pFilter )
        {

            string connectionString = LRWarehouseRO();

            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "ResourceType_SearchCounts", sqlParameters );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ResourceType_SearchCounts() " );
                return null;

            }
        }

        public DataSet ResourceEducationLevel_SearchCounts( string pFilter )
        {

            string connectionString = LRWarehouseRO();

            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "ResourceEducationLevel_SearchCounts", sqlParameters );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ResourceType_SearchCounts() " );
                return null;

            }
        }
        #endregion


        #region ====== Publisher methods ===============================================
        /// <summary>
        /// Search for Publisher related data using passed parameters
        /// - uses custom paging
        /// - only requested range of rows will be returned
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public DataSet PublisherSearch( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            //***** delete if not using custom paging *******
            string connectionString = GetReadOnlyConnection();
            string searchProcedure = "PublisherSearch";
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
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, searchProcedure, sqlParameters );
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
                LogError( ex, className + ".PublisherSearch() " );
                return null;

            }
        }
        #endregion

        #region ILpathways clusters and pathways 

        /// <summary>
        /// Retrieve dataset of all ILPathways career clusters
        /// </summary>
        /// <returns></returns>
        public DataSet ILPathwaysClusterSelect()
        {
            return ILPathwaysClusterSelect( 0, "" );
        }//
        public DataSet ILPathwaysClusterSelect( string ilPathwayChannel )
        {
            return ILPathwaysClusterSelect( 0, ilPathwayChannel );
        }//
        /// <summary>
        /// Retrieve dataset of all ILPathways career clusters
        /// </summary>
        /// <param name="clusterId"></param>
        /// <param name="ilPathwayChannel"></param>
        /// <returns></returns>
        public DataSet ILPathwaysClusterSelect( int clusterId, string ilPathwayChannel )
        {
            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ClusterId", clusterId );
                sqlParameters[ 1 ] = new SqlParameter( "@IlPathwayChannel", ilPathwayChannel );

                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), "[CareerClusterSelect]", sqlParameters );
                if ( ds.HasErrors )
                {
                    ds = null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".ILPathwaysClusterSelect({0}): ", clusterId ) );
                return null;
            }
        }//
        #endregion
        #region ====== Code Table Methods ===============================================
        public DataSet ResourceType_Select()
        {

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), CommandType.StoredProcedure, "Codes_ResourceType_Select" );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ResourceType_Select() " );
                return null;

            }
        }
        /// <summary>
        /// Retrieve Education level code table
        /// </summary>
        /// <returns></returns>
        public DataSet EducationLevel_Select()
        {

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), CommandType.StoredProcedure, "[Codes_EducationLevel_Select]" );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".EducationLevel_Select() " );
                return null;

            }
        }


        #endregion


 
    }
}
