using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

using ILPathways.Common;
using Isle.DTO;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using LDBM = LRWarehouse.DAL.DatabaseManager;
using ResBiz = IOERBusinessEntities;

namespace Isle.BizServices
{
    public class CodeTableBizService : ServiceHelper
    {

        #region ==== Site filter methods ====
        public static CodesSite Site_SelectFilters( string siteName, bool mustHaveValues )
        {
            CodesSite site = ResBiz.EFCodesManager.Codes_Site_GetByTitle( siteName, mustHaveValues );

            return site;
        } //
        public static CodesSite Site_SelectFilters( int siteId, bool mustHaveValues )
        {
            CodesSite site = ResBiz.EFCodesManager.Codes_Site_Get( siteId, mustHaveValues );

            return site;
        } //

        /// <summary>
        /// Get a site,and list of filter categories
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public static Isle.DTO.Site Site_Get( int siteId )
        {
            return ResBiz.EFCodesManager.Codes_Site_Get( siteId );

        } //
        /// <summary>
        /// Get all filter categories for a site
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public static List<Isle.DTO.SiteTagCategory> Site_SelectFilterCategories( int siteId )
        {
            return ResBiz.EFCodesManager.Codes_TagCategory_Fill( siteId );

        } //

        #endregion

        #region === Site filter methods for WebApi ===========================================
        public static SiteFiltersDTO Site_SelectAsDto( string siteName, bool mustHaveValues )
        {
            CodesSite site = ResBiz.EFCodesManager.Codes_Site_GetByTitle( siteName, mustHaveValues );

            return TransformSite( site );
        } //
        public static SiteFiltersDTO Site_SelectAsDto( int siteId, bool mustHaveValues )
        {
            CodesSite site = ResBiz.EFCodesManager.Codes_Site_Get( siteId, mustHaveValues );

            return TransformSite( site );
        } //
        public static SiteFiltersDTO TransformSite( CodesSite site )
        {
            SiteFiltersDTO siteFilters = new SiteFiltersDTO();
            if ( site == null || site.Id == 0 || site.SiteTagCategories == null || site.SiteTagCategories.Count == 0)
            {
                siteFilters.IsValid = false;
                siteFilters.Message = "No data";
                return siteFilters;
            }

            siteFilters.Id = site.Id;
            siteFilters.IsValid = true;
            siteFilters.SiteName = site.Title;
            siteFilters.FiltersCount = site.SiteTagCategories.Count;
            siteFilters.FilterList = new List<SiteFiltersTagsDTO>();
            SiteFiltersTagsDTO filter;
            foreach ( CodesSiteTagCategory tag in site.SiteTagCategories )
            {
                filter = new SiteFiltersTagsDTO();
                filter.Id = tag.Id;
                filter.SiteId = tag.SiteId;
                filter.CategoryId = tag.CategoryId;
                filter.Title = tag.Title;
                filter.Description = tag.Description;
                filter.SchemaTag = tag.SchemaTag;
                filter.SortOrder = tag.SortOrder;

                if ( tag.TagCategory.TagValues != null && tag.TagCategory.TagValues.Count > 0 )
                {
                    filter.FilterValues = new List<SiteFilterValueDTO>();
                    SiteFilterValueDTO fv;
                    foreach ( CodesTagValue val in tag.TagCategory.TagValues )
                    {
                        fv = new SiteFilterValueDTO();
                        fv.Id = val.Id;
                        fv.CategoryId = val.CategoryId;
                        fv.CodeId = val.CodeId;
                        fv.Title = val.Title;
                        fv.Description = val.Description;
                        fv.SortOrder = val.SortOrder;
                        fv.SchemaTag = val.SchemaTag;
                        fv.WarehouseTotal = val.WarehouseTotal;

                        filter.FilterValues.Add( fv );
                    }

                    siteFilters.FilterList.Add( filter );
                }

            }

            return siteFilters;

        }
        #endregion

        #region === generic handling of resource code tables ===
        /// <summary>
        /// Return values for a code table, optionally specify where to return all rows or only those with total used > 0
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="mustHaveValues">If true, only return rows with total</param>
        /// <returns></returns>
        public static List<CodeItem> Resource_CodeTableSelectList( string tableName, bool mustHaveValues )
        {
            return Resource_CodeTableSelectLists( tableName, mustHaveValues );
        } //
        /// <summary>
        /// Return all values for a code table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static List<CodeItem> Resource_CodeTableSelectList( string tableName )
        {
            return Resource_CodeTableSelectLists( tableName, false );
        } //
        private static List<CodeItem> Resource_CodeTableSelectLists( string tableName, bool mustHaveValues )
        {
            List<CodeItem> list = new List<CodeItem>();
            CodeItem code = new CodeItem();
            DataSet ds = Resource_CodeTableSelect( tableName );
            if ( DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow row in ds.Tables[ 0 ].Rows )
                {
                    code = new CodeItem();
                    code.Id = GetRowColumn(row, "Id", 0);
                    code.Title = GetRowColumn( row, "Title", "Missing" );
                    code.Description = GetRowColumn( row, "Description", "Missing" );
                    code.WarehouseTotal = GetRowColumn( row, "WarehouseTotal", 0 );
                    code.SortOrder = GetRowPossibleColumn( row, "SortOrder", 10 );

                    if ( mustHaveValues == false || code.WarehouseTotal > 0 )
                        list.Add( code );
                }
            }
            return list;

        } //
        public static DataSet Resource_CodeTableSelect( string tableName )
        {
            string sql = string.Format( "SELECT [Id],[Title]  ,[Description] ,isnull([WarehouseTotal],0) As [WarehouseTotal]  FROM [dbo].[{0}] where IsActive= 1 order by title", tableName);
            DataSet ds = LDBM.DoQuery( sql );
            return ds;

        } //
        #endregion

        #region == Mapping searches =====================
        /// <summary>
        /// Map Career Cluster Search
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static DataSet MapCareerCluster_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {

            return CodeTableManager.MapCareerCluster_Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }

        /// <summary>
        /// Map K12 Subjectr Search
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static DataSet MapK12Subject_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {

            return CodeTableManager.MapK12Subject_Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }

        #endregion

        
        public static void PopulateGridPageSizeList( ref DropDownList list )
        {
            DataSet ds = LDBM.GetCodeValues( "GridPageSize", "SortOrder" );
            LDBM.PopulateList( list, ds, "StringValue", "StringValue", "Select Size" );
        } //


    }
}
