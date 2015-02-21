using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ResBiz = IOERBusinessEntities;
using Isle.DTO;
using LRWarehouse.Business;

namespace Isle.Factories
{
    public class SiteFiltersFactory
    {

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
            if ( site == null || site.Id == 0 || site.SiteTagCategories == null || site.SiteTagCategories.Count == 0 )
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
    }
}
