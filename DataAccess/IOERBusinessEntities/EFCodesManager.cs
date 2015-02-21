using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LB = LRWarehouse.Business;
using ILPathways.Utilities;
using IDTO = Isle.DTO;

namespace IOERBusinessEntities
{
    public class EFCodesManager
    {

        static ResourceContext ctx = new ResourceContext();
        public EFCodesManager() { }

        #region === Site tags and values =======================
        public static IDTO.Site Codes_Site_Get( int siteId )
        {
            IDTO.Site entity = new IDTO.Site();
            Codes_Site item = new Codes_Site();

            using ( var context = new ResourceContext() )
            {
                item = context.Codes_Site.SingleOrDefault( s => s.Id == siteId );

                if ( item != null && item.Id > 0 )
                {
                    entity.Id = item.Id;
                    entity.Title = item.Title;
                    entity.Description = item.Description == null ? "" : item.Description;
                    entity.HasStandardsBrowser = item.HasStandardsBrowser == null ? false : ( bool ) item.HasStandardsBrowser;
                    entity.CssThemes = item.CssThemes;
                    entity.ApiRoot = item.ApiRoot;

                    entity.SiteTagCategories = Codes_TagCategory_Fill( siteId );
                }
            }
            return entity;
        } 

        /// <summary>
        /// Get a Code Site by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mustHaveValues"></param>
        /// <returns></returns>
        public static LB.CodesSite Codes_Site_Get( int siteId, bool mustHaveValues )
        {
            LB.CodesSite entity = new LB.CodesSite();
            Codes_Site item = new Codes_Site();

            using ( var context = new ResourceContext() )
            {
                item = context.Codes_Site.SingleOrDefault( s => s.Id == siteId );

                if ( item != null && item.Id > 0 )
                {
                    entity.Id = item.Id;
                    entity.Title = item.Title;
                    entity.Description = item.Description == null ? "" : item.Description;
                    entity.HasStandardsBrowser = item.HasStandardsBrowser == null ? false : (bool)item.HasStandardsBrowser;
                    entity.CssThemes = item.CssThemes;
                    entity.ApiRoot = item.ApiRoot;

                    entity.SiteTagCategories = Codes_FillSite( context, item, mustHaveValues );
                }
            }
            return entity;
        } 

        /// <summary>
        /// Get a code site by title
        /// </summary>
        /// <param name="title"></param>
        /// <param name="mustHaveValues"></param>
        /// <returns></returns>
        public static LB.CodesSite Codes_Site_GetByTitle( string title, bool mustHaveValues )
        {
            LB.CodesSite entity = new LB.CodesSite();
            Codes_Site item = new Codes_Site();

            using ( var context = new ResourceContext() )
            {
                item = context.Codes_Site.SingleOrDefault( s => s.Title == title );

                if ( item != null && item.Id > 0 )
                {
                    entity.Id = item.Id;
                    entity.Title = item.Title;
                    entity.Description = item.Description == null ? "" : item.Description;

                    entity.SiteTagCategories = Codes_FillSite( context, item, mustHaveValues );
                }
            }
            return entity;
        }

        /// <summary>
        /// Fill the tag category and tag values for the provided site
        /// </summary>
        /// <param name="context"></param>
        /// <param name="site"></param>
        /// <param name="mustHaveValues"></param>
        /// <returns></returns>
        public static List<LB.CodesSiteTagCategory> Codes_FillSite( ResourceContext context, Codes_Site site, bool mustHaveValues )
        {

            List<LB.CodesSiteTagCategory> list = new List<LB.CodesSiteTagCategory>();

            LB.CodesSiteTagCategory entity = new LB.CodesSiteTagCategory();
            using ( context )
            {
                List<Codes_SiteTagCategory> categories = context.Codes_SiteTagCategory
                                    .Include( "Codes_TagCategory" )
                                    .Where( s => s.SiteId == site.Id && s.IsActive == true )
                                    .OrderBy ( s => s.SortOrder ).ThenBy(s => s.Title)
                                    .ToList();

                if ( categories != null && categories.Count > 0 )
                {
                    foreach ( Codes_SiteTagCategory category in categories )
                    {
                        entity = new LB.CodesSiteTagCategory();

                        entity.Id = category.Id;
                        entity.CategoryId = ( int ) category.CategoryId;
                        entity.SiteId = ( int ) category.SiteId;
                        entity.Title = category.Title;
                        entity.IsActive = category.IsActive;
                        entity.SortOrder = category.SortOrder;
                        entity.TagCategory = new LB.CodesTagCategory();
                        //just incase??
                        if ( category.Codes_TagCategory == null )
                            category.Codes_TagCategory = context.Codes_TagCategory
                                .SingleOrDefault( s => s.Id == entity.CategoryId && s.IsActive == true );

                        if ( category.Codes_TagCategory != null && (bool) category.Codes_TagCategory.IsActive )
                        {
                            //save flattened versions
                            entity.Description = category.Codes_TagCategory.Description;
                            entity.SchemaTag = category.Codes_TagCategory.SchemaTag;

                            //fill TagCategory
                            entity.TagCategory.Id = category.Codes_TagCategory.Id;
                            entity.TagCategory.Title = category.Codes_TagCategory.Title;
                            entity.TagCategory.Description = category.Codes_TagCategory.Description;
                            entity.TagCategory.SortOrder = category.Codes_TagCategory.SortOrder;    // != null ? (int) category.Codes_TagCategory.SortOrder : 0;
                            entity.TagCategory.IsActive = category.Codes_TagCategory.IsActive;   // != null ? (bool) category.Codes_TagCategory.IsActive : true;
                            entity.TagCategory.SchemaTag = category.Codes_TagCategory.SchemaTag;

                            entity.TagCategory.TagValues = new List<LB.CodesTagValue>();

                            category.Codes_TagCategory.Codes_TagValue = context.Codes_TagValue
                                    .Include( "Codes_TagValueKeyword" )
                                    .Where( s => s.CategoryId == category.Codes_TagCategory.Id && s.IsActive == true )
                                    .OrderBy( s => s.SortOrder ).ThenBy( s => s.Title )
                                    .ToList();

                            if ( category.Codes_TagCategory.Codes_TagValue != null && category.Codes_TagCategory.Codes_TagValue.Count > 0 )
                            {
                                LB.CodesTagValue val = new LB.CodesTagValue();

                                foreach ( Codes_TagValue item in category.Codes_TagCategory.Codes_TagValue )
                                {
                                    val = new LB.CodesTagValue();
                                    val.Id = item.Id;
                                    val.CategoryId = item.CategoryId;
                                    val.CodeId = item.CodeId;
                                    val.Title = item.Title;
                                    val.Description = item.Description != null ? item.Description : "";
                                    val.SortOrder = item.SortOrder != null ? ( int ) item.SortOrder : 0;
                                    val.IsActive = item.IsActive != null ? ( bool ) item.IsActive : true;
                                    val.SchemaTag = item.SchemaTag != null ? item.SchemaTag : "";
                                    val.WarehouseTotal = item.WarehouseTotal != null ? ( int ) item.WarehouseTotal : 0;

                                    if ( mustHaveValues == false || val.WarehouseTotal > 0 )
                                        entity.TagCategory.TagValues.Add( val );
                                    //load any standard keywords
                                    if ( item.Codes_TagValueKeyword != null && item.Codes_TagValueKeyword.Count > 0 )
                                    {
                                        LB.CodesTagValueKeyword ckw;
                                        foreach ( Codes_TagValueKeyword kw in item.Codes_TagValueKeyword )
                                        {
                                            if ( kw.IsActive )
                                            {
                                                val.Keywords.Add( kw.Keyword );
                                                ckw = new LB.CodesTagValueKeyword();
                                                ckw.Id = kw.Id;
                                                ckw.TagValueId = kw.TagValueId;
                                                ckw.Keyword = kw.Keyword;
                                                val.TagValueKeywords.Add( ckw );
                                            }
                                        }
                                    }
                                }
                                list.Add( entity );
                            }
                            
                        }
                        else
                        {
                            //related tag category was not found?
                            //should not happen, but if does, just don't include
                        }

                    }
                }
            }
            return list;
        }

        /// <summary>
        /// get tag categories for provided siteId
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="mustHaveValues"></param>
        /// <returns></returns>
        public static List<IDTO.SiteTagCategory> Codes_TagCategory_Fill( int siteId )
        {

            List<IDTO.SiteTagCategory> list = new List<IDTO.SiteTagCategory>();

            IDTO.SiteTagCategory entity = new IDTO.SiteTagCategory();
            //just in case, 
            if ( siteId == 0 )
                siteId = 1;     //?????????????

            using ( var context = new ResourceContext() )
            {
                using ( context )
                {
                    List<Codes_SiteTagCategory_Summary> categories = context.Codes_SiteTagCategory_Summary
                                        .Where( s => s.SiteId == siteId  )
                                        .OrderBy( s => s.Site_SortOrder ).ThenBy( s => s.TagCategory_SortOrder ).ThenBy( s => s.Title )
                                        .ToList();

                    if ( categories != null && categories.Count > 0 )
                    {
                        foreach ( Codes_SiteTagCategory_Summary category in categories )
                        {
                            entity = new IDTO.SiteTagCategory();


                            entity.Id = category.Id;
                            entity.CategoryId = ( int ) category.CategoryId;
                            entity.SiteId = ( int ) category.SiteId;
                            entity.Title = category.Title;
                            entity.Description = category.Description;
                            entity.SchemaTag = category.SchemaTag;
                            entity.IsActive = true;
                            if (category.Site_SortOrder == 10)
                                entity.SortOrder = category.TagCategory_SortOrder;
                            else
                                entity.SortOrder = category.Site_SortOrder;

                            list.Add( entity );
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Retrieve a TagValue by categoryId and title
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static LB.CodesTagValue Codes_TagValue_Get( int categoryId, string title )
        {
            LB.CodesTagValue entity = new LB.CodesTagValue();
            Codes_TagValue item = new Codes_TagValue();
            if ( title == "Rights & Advocacy" )
                title = "Rights and Advocacy";

            using ( var context = new ResourceContext() )
            {
                item = context.Codes_TagValue.SingleOrDefault( s => s.CategoryId == categoryId && s.Title == title );

                if ( item != null && item.Id > 0 )
                {
                    entity.Id = item.Id;
                    entity.CodeId = item.CodeId;
                    entity.Title = item.Title;
                    entity.Description = item.Description == null ? "" : item.Description;

                }
                else
                {
                    //if ( categoryId == 10 )
                    //{
                    //    LoggingHelper.DoTrace( 2, string.Format( "###### EFCodesManagerCodes_TagValue_Get. Did not find categoryId: {0}, tag title: {1}. Retrying with employment, cat = 12", categoryId, title ) );
                    //    entity = Codes_TagValue_Get( 12, title );
                    //}
                    //else
                        LoggingHelper.DoTrace( 2, string.Format( "EFCodesManagerCodes_TagValue_Get. Did not find categoryId: {0}, tag title: {1}", categoryId, title ) );
                }
            }
            return entity;
        }
                
        public static LB.CodesTagValue Codes_TagValue_Get( int id )
        {
            LB.CodesTagValue entity = new LB.CodesTagValue();
            Codes_TagValue item = new Codes_TagValue();

            using ( var context = new ResourceContext() )
            {
                item = context.Codes_TagValue.SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    entity.Id = item.Id;
                    entity.CodeId = item.CodeId;
                    entity.Title = item.Title;
                    entity.Description = item.Description == null ? "" : item.Description;

                }
                else
                {
                    LoggingHelper.DoTrace( 2, string.Format( "EFCodesManagerCodes_TagValue_Get. Did not find Id: {0}", id ) );
                }
            }
            return entity;
        }
        public static Codes_TagValue Codes_TagValue_Get( ResourceContext context, int id )
        {
            Codes_TagValue item = new Codes_TagValue();

           // using ( var context = new ResourceContext() )
           // {
                item = context.Codes_TagValue
                    .Include( "Codes_TagCategory" )
                    .SingleOrDefault( s => s.Id == id );

                if ( item == null || item.Id == 0 )
                {
                    LoggingHelper.DoTrace( 2, string.Format( "EFCodesManagerCodes_TagValue_Get. Did not find Id: {0}", id ) );
                }
                else
                {
                    if ( item.Codes_TagCategory == null | item.Codes_TagCategory.Id == 0 )
                    {
                        item.Codes_TagCategory = Codes_TagCategory_Get( context, item.CategoryId );
                    }
                }
            //}
            return item;
        }


        public static Codes_TagCategory Codes_TagCategory_Get( ResourceContext context, int id )
        {
            Codes_TagCategory item = new Codes_TagCategory();

            // using ( var context = new ResourceContext() )
            // {
            item = context.Codes_TagCategory.SingleOrDefault( s => s.Id == id );

            if ( item == null || item.Id == 0 )
            {
                LoggingHelper.DoTrace( 2, string.Format( "Codes_TagCategory. Did not find Id: {0}", id ) );
            }
            //}
            return item;
        }
        #endregion

        #region ConditionOfUse =======================


        /// <summary>
        /// Get a ConditionOfUse
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static LB.ConditionOfUse ConditionOfUse_Get( int id )
        {
            LB.ConditionOfUse entity = new LB.ConditionOfUse();
            using ( var context = new ResourceContext() )
            {
                ConditionOfUse item = context.ConditionOfUses.SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    entity = ConditionOfUse_ToMap( item );
                   
                }
            }
            return entity;
        }


        public static LB.ConditionOfUse ConditionOfUse_ToMap( ConditionOfUse fromEntity )
        {
            LB.ConditionOfUse to = new LB.ConditionOfUse();
            to.Id = fromEntity.Id;
            to.Title = fromEntity.Title;
            to.Description = fromEntity.Description;
            to.Summary = fromEntity.Summary;
            to.IsActive = (bool) fromEntity.IsActive;
            to.Url = fromEntity.Url;
            to.IconUrl = fromEntity.IconUrl;
            to.Summary = fromEntity.MiniIconUrl;

            to.SortOrder = fromEntity.SortOrderAuthoring != null ? ( int ) fromEntity.SortOrderAuthoring : 0;
            to.WarehouseTotal = fromEntity.WarehouseTotal != null ? ( int ) fromEntity.WarehouseTotal : 0;


            return to;
        }

        #endregion


    }
}
