using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;

using AutoMapper;

using ILPContentFile = ILPathways.Business.ContentFile;
using IB = ILPathways.Business;
using ILPathways.Common;
using ILPathways.Utilities;
using DAL = ILPathways.DAL;
using Isle.DTO;
using dtoc = Isle.DTO.Content;

namespace IoerContentBusinessEntities
{
    public class EFContentManager
    {
        static string thisClassName = "EFContentManager";

        static IsleContentEntities ctx = new IsleContentEntities();
        static string DEFAULT_GUID = "00000000-0000-0000-0000-000000000000";
        static bool usingContentConnector = false;
        static int DefaultSortOrder = 20;
        private static bool IsUsingContentStandards()
        {
            bool usingContentStandards = UtilityManager.GetAppKeyValue( "usingContentStandards", false );

            return usingContentStandards;
        }

        #region Content =======================
        public  int ContentAdd( IB.ContentItem entity )
        {
            string statusMessage = "";
            return ContentAdd( entity, ref statusMessage );
        }
        public  int ContentAdd( IB.ContentItem entity, ref string statusMessage )
        {
            Content efEntity = new Content();

            try
            {
                Content_FromMap( entity, efEntity );
                if ( efEntity.RowId.ToString() == DEFAULT_GUID )
                    efEntity.RowId = Guid.NewGuid();

                efEntity.Created = System.DateTime.Now;
                efEntity.LastUpdated = System.DateTime.Now;
                ctx.Contents.Add( efEntity );

                // submit the change to database
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    statusMessage = "successful";
                    entity.Id = efEntity.Id;
                    return efEntity.Id;
                }
                else
                {
                    //?no info on error
                    return 0;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ContentAdd()" );
                statusMessage = ex.Message;
                return 0;
            }
        }

        /// <summary>
        /// Update content
        /// Optionally may also renumber children
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public  bool ContentUpdate( IB.ContentItem entity )
        {
            bool isValid = false;
            try
            {
                using ( var context = new IsleContentContext() )
                {
                    Content efEntity = context.Contents.SingleOrDefault( s => s.Id == entity.Id );
                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        Content_FromMap( entity, efEntity );
                        efEntity.LastUpdated = System.DateTime.Now;

                        context.SaveChanges();

                        isValid = true;
                        if ( entity.RenumberingChildren == true )
                        {
                            //not sure if this is the most likely path to do this update!
                            Content_RenumberHierarchyChildren( context, efEntity );
                            entity.RenumberingChildren = false;
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ContentUpdate()" );
                return false;
            }
            return isValid;
        }

        public  bool Content_Delete( int id, ref string statusMessage )
        {
            bool isSuccessful = false;
            try
            {
                statusMessage = "";
                using ( var context = new IsleContentContext() )
                {
                    Content item = context.Contents.SingleOrDefault( s => s.Id == id );
                    
                    if ( item != null && item.Id > 0 )
                    {
                        //will need to delete resource
                        //14-12-04 MP - added code at the services level to check this!
                        //15-01-28 MP - note in a cascade delete, the resources will need to be handled!!
                        int resourceId = item.ResourceIntId == null ? 0 : ( int ) item.ResourceIntId;

                        //if ( usingContentConnector == false )
                        //{
                            context.Contents.Remove( item );
                            context.SaveChanges();
                            isSuccessful = true;

                            //TODO - need to check for and delete a related resource
                            //delete doc version
                            if ( item.DocumentRowId != null
                                && item.DocumentRowId.ToString().Length == 36
                                && item.DocumentRowId.ToString() != DEFAULT_GUID )
                                Document_Version_Delete( item.DocumentRowId, ref statusMessage );
                        //} 
                        //else 
                        //{
                        //    //check for where a content connector child
                        //    if ( ContentConnector_DeleteChild( item.Id, ref statusMessage ) )
                        //    {

                        //        context.Contents.Remove( item );
                        //        context.SaveChanges();
                        //        isSuccessful = true;

                        //        //delete doc version
                        //        if ( item.DocumentRowId != null
                        //            && item.DocumentRowId.ToString().Length == 36
                        //            && item.DocumentRowId.ToString() != DEFAULT_GUID )
                        //            Document_Version_Delete( item.DocumentRowId, ref statusMessage );
                        //    }
                        //    else
                        //    {
                        //        statusMessage = "An issue was encountered removing records related to the requested record. Please notify system administration if this problem reoccurs.";
                        //    }
                        //}

                        // =========== delete child nodes ==========================
                        List<Content> eflist = context.Contents
                            .Where( s => s.ParentId == id )
                            .OrderBy( s => s.Id )
                            .ToList();

                        if ( eflist != null && eflist.Count > 0 )
                        {
                            foreach ( Content efom in eflist )
                            {
                                Content_Delete( efom.Id, ref statusMessage );
                                //if resource exists, need to set inactive
                                //if ( efom.ResourceIntId != null && efom.ResourceIntId > 0 )
                                //{
                                //    Isle.BizServices.ResourceBizService.Resource_SetInactive( ( int ) efom.ResourceIntId, ref statusMessage );
                                //}
                            }
                            statusMessage = string.Format("Also removed all child items ({0})",eflist.Count);
                        }
        
                    }
                }
            }
            catch ( Exception ex )
            {
                isSuccessful = false;
                statusMessage = ex.Message;
            }
            return isSuccessful;
        }

        /// <summary>
        /// Get a Content
        /// - lazy loading
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public static IB.ContentItem Content_Get( int contentId )
        {
            return Content_Get( contentId, false );
        }

        /// <summary>
        /// Get a Content
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="doingEagerLoad">If true, will populate child components like standards</param>
        /// <returns></returns>
        public static IB.ContentItem Content_Get( int contentId, bool doingEagerLoad )
        {

            using ( var context = new IsleContentContext() )
            {
                return Content_Get( context, contentId, doingEagerLoad );
            }
        }

        /// <summary>
        /// Get a Content
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="doingEagerLoad">If true, will populate child components like standards</param>
        /// <returns></returns>
        public static IB.ContentItem Content_Get( IsleContentContext context, int contentId, bool doingEagerLoad )
        {
            IB.ContentItem entity = new IB.ContentItem();
            //using ( var context = new IsleContentContext() )
            //{

            //                            .Include( "Codes_ContentStatus" )
            ContentSummaryView item = context.ContentSummaryViews
                            .SingleOrDefault( s => s.ContentId == contentId );

                if ( item != null && item.ContentId > 0 )
                {
                    entity = ContentSummary_ToMap( item, doingEagerLoad ); 
                }
            //}
            return entity;
        }

        /// <summary>
        /// Retrieve content using related resource Id
        /// - need to plan for conversion to use resourceIntId!!
        /// </summary>
        /// <param name="resVerId"></param>
        /// <returns></returns>
        private static IB.ContentItem Content_GetByResourceVersionId( int resVerId )
        {
            IB.ContentItem entity = new IB.ContentItem();
            using ( var context = new IsleContentContext() )
            {
                Content item = context.Contents.SingleOrDefault( s => s.ResourceVersionId == resVerId );

                if ( item != null && item.Id > 0 )
                {
                    entity = Content_ToMap( item, true, true, false );
                }
            }
            return entity;
        }

        public static IB.ContentItem Content_GetByResourceId( int pResourceIntId )
        {
            IB.ContentItem entity = new IB.ContentItem();
            using ( var context = new IsleContentContext() )
            {
                Content item = context.Contents.SingleOrDefault( s => s.ResourceIntId == pResourceIntId );

                if ( item != null && item.Id > 0 )
                {
                    entity = Content_ToMap( item, true, true, false );
                }
            }
            return entity;
        }
        #endregion 

        #region === ISBE Content =======================
        public IB.ContentItem CreateIsbeHierarchy( int contentId, int modules )
        {
            IB.ContentItem curr = Content_Get( contentId );
            return CreateIsbeHierarchy( curr, modules );
        }
        public IB.ContentItem CreateIsbeHierarchy( IB.ContentItem curr, int modules )
        {
            
            curr.ChildItems = new List<IB.ContentItem>();

            IB.ContentItem mod = new IB.ContentItem();
            
            //scope
            mod = CreateIsbeModule( curr, 0, "Scope and Sequence", 10 );
            curr.ChildItems.Add( mod );
            //family letter
            mod = CreateIsbeModule( curr, 0, "Family Letter", 30 );
            curr.ChildItems.Add( mod );

            int order = 30;
            if ( modules > 0  )
            {
                for ( int i = 1 ; i < modules + 1 ; i++ )
                {
                    mod.ParentId = curr.Id;
                    mod.SortOrder = order;
                    order += DefaultSortOrder;
                    mod.TypeId = IB.ContentItem.MODULE_CONTENT_ID;

                    mod = CreateIsbeModule( curr, i, "", order );
                    curr.ChildItems.Add( mod );
                }
            }

            return curr;
        }//
        public IB.ContentItem CreateIsbeModule( IB.ContentItem content, int moduleNbr, string title, int sortOrder )
        {
            IB.ContentItem mod = new IB.ContentItem();
            if (title == "")
                title = "Unit " + moduleNbr.ToString();

            mod.ParentId = content.Id;
            mod.TypeId = IB.ContentItem.MODULE_CONTENT_ID;
            mod.SortOrder = sortOrder;
            mod.Title = title;
            mod.Description = title + " for curriculum " + content.Title;
            mod.Summary = title + " for curriculum " + content.Title;
            mod.IsOrgContentOwner = content.IsOrgContentOwner;
            mod.StatusId = content.StatusId;
            mod.PrivilegeTypeId = content.PrivilegeTypeId;
            mod.ConditionsOfUseId = content.ConditionsOfUseId;
            mod.IsActive = true;
            //mod.IsPublished = false;
            mod.IsOrgContentOwner = content.IsOrgContentOwner;
            mod.OrgId = content.OrgId;
            mod.ResourceIntId = 0;
            mod.Created = System.DateTime.Now;
            mod.LastUpdated = mod.Created;
            mod.CreatedById = content.CreatedById;
            mod.LastUpdatedById = mod.CreatedById;
            mod.RowId = Guid.NewGuid();
            mod.UseRightsUrl = content.UseRightsUrl;

            //ADD
            int modId = ContentAdd( mod );
            if ( moduleNbr > 0 )
            {
                //add unit map
                Content_CreateChild( mod, IB.ContentItem.UNIT_CONTENT_ID, 0, "Unit Map", 10 );
                //add family letter
                Content_CreateChild( mod, IB.ContentItem.UNIT_CONTENT_ID, 0, "Family Letter", 30 );
                //add assessments
                Content_CreateChild( mod, IB.ContentItem.UNIT_CONTENT_ID, 0, "Assessments", 50 );
                //add n lessons
                Content_CreateChild( mod, IB.ContentItem.UNIT_CONTENT_ID, 1, "Lesson", 70 );

                Content_CreateChild( mod, IB.ContentItem.UNIT_CONTENT_ID, 2, "Lesson", 90 );
            }
            return mod;
        }
        #endregion

        #region === Hierarchical Content =======================
        /// <summary>
        /// Create a starting hierarchy
        /// </summary>
        /// <param name="modules"></param>
        /// <param name="units"></param>
        /// <param name="lessons"></param>
        /// <param name="activities"></param>
        /// <returns></returns>
        public IB.ContentItem CreateHierarchy( int contentId, int modules, int units, int lessons, int activities )
        {
            IB.ContentItem curr = Content_Get( contentId );
            curr.ChildItems = new List<IB.ContentItem>();

            IB.ContentItem mod = new IB.ContentItem();
            int order = 10;
            //first assumption, assume have to start with some modules and units
            if ( modules > 0 && units > 0 )
            {
                for ( int i = 1 ; i < modules + 1 ; i++ )
                {
                    mod.ParentId = contentId;
                    mod.SortOrder = order;
                    order += 10;
                    mod.TypeId = IB.ContentItem.MODULE_CONTENT_ID;
                    mod.Title = "Module " + i.ToString();
                    mod.Description = "Module " + i.ToString() + " for curriculum " + curr.Title;
                    mod.Summary = "Module " + i.ToString() + " for curriculum " + curr.Title;
                    mod.IsOrgContentOwner = curr.IsOrgContentOwner;
                    mod.StatusId = curr.StatusId;
                    mod.PrivilegeTypeId = curr.PrivilegeTypeId;
                    mod.ConditionsOfUseId = curr.ConditionsOfUseId;
                    mod.IsActive = true;
                   // mod.IsPublished = false;
                    mod.IsOrgContentOwner = curr.IsOrgContentOwner;
                    mod.OrgId = curr.OrgId;
                    mod.ResourceIntId = 0;
                    mod.Created = System.DateTime.Now;
                    mod.LastUpdated = mod.Created;
                    mod.CreatedById = curr.CreatedById;
                    mod.LastUpdatedById = mod.CreatedById;
                    mod.RowId = Guid.NewGuid();
                    mod.UseRightsUrl = curr.UseRightsUrl;

                    int modId = ContentAdd( mod );
                    if ( units > 0 )
                        Content_CreateChildren( mod, mod.TypeId + 2, units, lessons, activities );

                    curr.ChildItems.Add( mod );
                }
            }

            return curr;


        }//


        private void Content_CreateChildren( IB.ContentItem parentEntity, int levelId, int units, int lessons, int activities )
        {
            IB.ContentItem child = new IB.ContentItem();
            string parentTitle = "Parent";
            string childTitle = "Child";
            int childReq = 3;
            if ( levelId == IB.ContentItem.UNIT_CONTENT_ID )
            {
                childReq = units;
                parentTitle = "Module";
                childTitle = "Unit";
            }
            else if ( levelId == IB.ContentItem.LESSON_CONTENT_ID )
            {
                childReq = lessons;
                parentTitle = "Unit";
                childTitle = "Lesson";
            }
            else if ( levelId == IB.ContentItem.ACTIVITY_CONTENT_ID )
            {
                childReq = activities;
                parentTitle = "Lesson";
                childTitle = "Actitity";
            }
            else
            {
                //error , exit?
                return;
            }

            int order = 10;
            //first assumption, assume have to start with some modules and units
            if ( childReq > 0 )
            {
                for ( int i = 1 ; i < childReq + 1 ; i++ )
                {
                    child.ParentId = parentEntity.Id;
                    child.SortOrder = order;
                    order += 5;
                    child.TypeId = levelId;
                    child.Title = parentEntity.Title + " - " + childTitle + " " + i.ToString();
                    child.Description = parentEntity.Title + " - " + childTitle + " " + i.ToString() + " TBD";
                    child.Summary = parentEntity.Title + " - " + childTitle + " " + i.ToString() + " TBD";
                    child.IsOrgContentOwner = parentEntity.IsOrgContentOwner;
                    child.StatusId = parentEntity.StatusId;
                    child.PrivilegeTypeId = parentEntity.PrivilegeTypeId;
                    child.ConditionsOfUseId = parentEntity.ConditionsOfUseId;
                    child.IsActive = true;
                   // child.IsPublished = false;
                    child.IsOrgContentOwner = parentEntity.IsOrgContentOwner;
                    child.OrgId = parentEntity.OrgId;
                    child.ResourceIntId = 0;
                    child.Created = System.DateTime.Now;
                    child.LastUpdated = child.Created;
                    child.CreatedById = parentEntity.CreatedById;
                    child.LastUpdatedById = child.CreatedById;
                    child.RowId = Guid.NewGuid();
                    child.UseRightsUrl = parentEntity.UseRightsUrl;

                    int childId = ContentAdd( child );
                    if ( childReq > 0 )
                        Content_CreateChildren( child, child.TypeId + 2, units, lessons, activities );

                    parentEntity.ChildItems.Add( child );
                }
            }

 }//


        private IB.ContentItem Content_CreateChild( IB.ContentItem parentEntity, int levelId, int childNbr, string childTitle, int order )
        {
            IB.ContentItem child = new IB.ContentItem();
            string childSuffix = "";
            if ( childNbr > 0 )
                childSuffix = " " + childNbr.ToString();

            child.ParentId = parentEntity.Id;
            child.SortOrder = order;
            
            child.TypeId = levelId;
            child.Title = parentEntity.Title + " - " + childTitle + childSuffix;
            child.Description = parentEntity.Title + " - " + childTitle + childSuffix + " TBD";
            child.Summary = parentEntity.Title + " - " + childTitle + childSuffix + " TBD";
            child.IsOrgContentOwner = parentEntity.IsOrgContentOwner;
            child.StatusId = parentEntity.StatusId;
            child.PrivilegeTypeId = parentEntity.PrivilegeTypeId;
            child.ConditionsOfUseId = parentEntity.ConditionsOfUseId;
            child.ResourceIntId = 0;
            child.IsActive = true;
            //child.IsPublished = false;
            child.IsOrgContentOwner = parentEntity.IsOrgContentOwner;
            child.OrgId = parentEntity.OrgId;
            child.ResourceIntId = 0;
            child.Created = System.DateTime.Now;
            child.LastUpdated = child.Created;
            child.CreatedById = parentEntity.CreatedById;
            child.LastUpdatedById = child.CreatedById;
            child.RowId = Guid.NewGuid();
            child.UseRightsUrl = parentEntity.UseRightsUrl;

            int childId = ContentAdd( child );
            parentEntity.ChildItems.Add( child );

            return child;
       }//

       
        /// <summary>
        /// Get a very thin hierarchy outline with caching
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="publishedOnly"></param>
        /// <param name="allowCaching"></param>
        /// <returns></returns>
        public static ContentNode Content_GetHierarchyOutline( int contentId, bool publishedOnly, bool allowCaching )
        {
            int cacheHours = UtilityManager.GetAppKeyValue( "curriculumCacheHours", 1 );
            int cacheHoursElapsed = cacheHours * -1;

            //just cache Curriculumn for now
            string key = "";

            ContentNode node = new ContentNode();
            using ( var context = new IsleContentContext() )
            {
                Content entity = context.Contents
                    .SingleOrDefault( s => s.Id == contentId );

                if ( entity == null || entity.Id == 0 )
                    return node;

                if ( entity.TypeId == IB.ContentItem.CURRICULUM_CONTENT_ID )
                {
                    key = "NodeOutline" + entity.Id.ToString();

                    if ( HttpRuntime.Cache[ key ] != null && allowCaching )
                    {
                        var cache = ( CachedContent ) HttpRuntime.Cache[ key ];
                        try
                        {
                            if ( cache.lastUpdated > DateTime.Now.AddHours( cacheHoursElapsed ) )
                            {
                                LoggingHelper.DoTrace( 2, string.Format( "===Content_GetHierarchyOutline === Using cached version of curriculum, Id: {0}, {1}", entity.Id, entity.Title ) );
                                return cache.nodeOutline;
                            }
                        }
                        catch ( Exception ex )
                        {
                            LoggingHelper.DoTrace( 2, "===Content_GetHierarchyOutline === exception " + ex.Message );
                        }
                    }
                    else
                    {
                        LoggingHelper.DoTrace( 2, string.Format( "****** Content_GetHierarchyOutline === Retrieving full version of curriculum, Id: {0}, {1}", entity.Id, entity.Title ) );
                    }
                }
                node.Id = entity.Id;
                node.TypeId = entity.TypeId == null ? 10 : ( int ) entity.TypeId;
                node.SortOrder = entity.SortOrder == null ? DefaultSortOrder : ( int ) entity.SortOrder;
                node.Title = entity.Title;
                node.Description = entity.Description;

                Content_GetHierarchyOutline( context, node, publishedOnly );

                //Cache the output
                if ( key.Length > 0 )
                {
                    var newCache = new CachedContent()
                    {
                        nodeOutline = node,
                        lastUpdated = DateTime.Now
                    };

                    if ( HttpContext.Current.Cache[ key ] != null )
                    {
                        HttpRuntime.Cache.Remove( key );
                        HttpRuntime.Cache.Insert( key, newCache );

                        LoggingHelper.DoTrace( 2, string.Format( "===Content_GetHierarchyOutline $$$ Updating cached version of curriculum, Id: {0}, {1}", entity.Id, entity.Title ) );

                    }
                    else
                    {
                        LoggingHelper.DoTrace( 2, string.Format( "===Content_GetHierarchyOutline ****** Inserting new cached version of curriculum, Id: {0}, {1}", entity.Id, entity.Title ) );
                        //HttpContext.Current.Cache.Insert( key, newCache );

                        System.Web.HttpRuntime.Cache.Insert( key, newCache, null, DateTime.Now.AddHours( cacheHours ), TimeSpan.Zero );
                    }
                }
            }



            return node;
        }//


        /// <summary>
        /// Get a very thin hierarchy 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="node"></param>
        /// <param name="publishedOnly"></param>
        public static void Content_GetHierarchyOutline( IsleContentContext context, ContentNode node, bool publishedOnly )
        {
            //ContentNode node = new ContentNode();
            ContentNode child = new ContentNode();

 
            //get first level
            node.ChildNodes = new List<ContentNode>();

            
            List<Content> eflist = context.Contents
                            .Where( s => s.ParentId == node.Id 
                                && s.TypeId != IB.ContentItem.DOCUMENT_CONTENT_ID 
                                && s.TypeId != IB.ContentItem.EXTERNAL_URL_CONTENT_ID )
                            .OrderBy( s => s.SortOrder )
                            .ToList();

            if ( eflist != null && eflist.Count > 0 )
            {
                foreach ( Content efom in eflist )
                {
                    child = new ContentNode();
                    child.Id = efom.Id;
                    child.ParentId = (int) efom.ParentId;
                    child.TypeId = UtilityManager.Assign(efom.TypeId, IB.ContentItem.MODULE_CONTENT_ID);
                    child.SortOrder = efom.SortOrder == null ? DefaultSortOrder : ( int ) efom.SortOrder;
                    child.Title = efom.Title;
                    child.Description = efom.Description;
                    if ( efom.StatusId == IB.ContentItem.PUBLISHED_STATUS )
                        child.IsPublished = true;
                    else
                        child.IsPublished = false;
                   // child.IsPublished = UtilityManager.Assign( efom.IsPublished, false );

                    if ( publishedOnly == false || child.IsPublished )
                    {
                        //get this items children
                        child.ChildNodes = ContentNode_FillChildren( context, child, publishedOnly );

                        node.ChildNodes.Add( child );
                    }
                }
            }
         
            //return node;
        }
        private static List<ContentNode> ContentNode_FillChildren( IsleContentContext context,
                        ContentNode entity,
                        bool publishedOnly )
        {

            List<ContentNode> items = new List<ContentNode>();
            ContentNode child = new ContentNode();

            List<Content> eflist = context.Contents
                            .Where(s => s.ParentId == entity.Id
                                && s.TypeId != IB.ContentItem.DOCUMENT_CONTENT_ID
                                && s.TypeId != IB.ContentItem.EXTERNAL_URL_CONTENT_ID )
                            .OrderBy( s => s.SortOrder )
                            .ToList();

            if ( eflist != null && eflist.Count > 0 )
            {
                foreach ( Content efom in eflist )
                {
                    child = new ContentNode();
                    child.Id = efom.Id;
                    child.ParentId = ( int ) efom.ParentId;
                    child.TypeId = UtilityManager.Assign(efom.TypeId, IB.ContentItem.UNIT_CONTENT_ID);
                    child.SortOrder = efom.SortOrder == null ? DefaultSortOrder : ( int ) efom.SortOrder;
                    child.Title = efom.Title;
                    child.Description = efom.Description;
                    if ( efom.StatusId == IB.ContentItem.PUBLISHED_STATUS )
                        child.IsPublished = true;
                    else
                        child.IsPublished = false;
                    //child.IsPublished = UtilityManager.Assign( efom.IsPublished, false );

                    if ( publishedOnly == false || child.IsPublished )
                    {
                        //get this items children
                        child.ChildNodes = ContentNode_FillChildren( context, child, publishedOnly );

                        items.Add( child );
                    }
                }
            }

            return items;
        }
        /// <summary>
        /// Get all related content in a hierarchy like a curriculum
        /// Will generally do lazy loading, don't need all detail for hierarchy
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="publishedOnly">If true, only published nodes will be included</param>
        /// <returns></returns>
        public static IB.ContentItem Content_GetHierarchyOutlineFAT( int contentId,
                        bool publishedOnly )
        {

            IB.ContentItem entity = Content_Get( contentId, false );
            if ( entity == null || entity.Id == 0 )
                return entity;

            //get first level
            entity.ChildItems = new List<IB.ContentItem>();

            using ( var context = new IsleContentContext() )
            {
                List<Content> eflist = context.Contents
                                .Where( s => s.ParentId == contentId )
                                .OrderBy( s => s.SortOrder )
                                .ToList();

                if ( eflist != null && eflist.Count > 0 )
                {
                    foreach ( Content efom in eflist )
                    {
                        IB.ContentItem child = Content_ToMap( efom, false, false, false );
                        if ( publishedOnly == false || child.IsPublished )
                        {
                            //get this items children
                            child.ChildItems = Content_FillOutlineChildrenFAT( context, child, false, publishedOnly );

                            entity.ChildItems.Add( child );
                        }
                    }
                }
            }
            return entity;
 }

        private static List<IB.ContentItem> Content_FillOutlineChildrenFAT( IsleContentContext context, 
                        IB.ContentItem entity, 
                        bool doingEagerLoad,
                        bool publishedOnly )
        {
          
            List<IB.ContentItem> items = new List<IB.ContentItem>();

            List<Content> eflist = context.Contents
                            .Where( s => s.ParentId == entity.Id )
                            .OrderBy( s => s.SortOrder )
                            .ToList();

            if ( eflist != null && eflist.Count > 0 )
            {
                foreach ( Content efom in eflist )
                {
                    //TODO - MP- this should be as thin as possible
                    IB.ContentItem child = Content_ToMap( efom, doingEagerLoad, false, doingEagerLoad );
                    if ( publishedOnly == false || child.IsPublished )
                    {
                        //get this items children
                        child.ChildItems = Content_FillOutlineChildrenFAT( context, child, doingEagerLoad, publishedOnly );

                        items.Add( child );
                    }
                }
            }
         
            return items;
        }


        private static void Content_FromMap( IB.ContentItem fromEntity, Content to )
        {
           // Content to = new Content();
            to.Id = fromEntity.Id;
            to.RowId = fromEntity.RowId;

            to.Title = fromEntity.Title;
            to.Description = fromEntity.Description;
            to.Summary = fromEntity.Summary != null ? fromEntity.Summary : "";
            
            to.SortOrder = (int) fromEntity.SortOrder;
            if ( fromEntity.ParentId > 0 )
                to.ParentId = fromEntity.ParentId;
            else
                to.ParentId = null;

            to.IsActive = ( bool ) fromEntity.IsActive;

            if (fromEntity.TypeId > 10)
                to.TypeId = fromEntity.TypeId;
            else
                to.TypeId = 10;

            if ( fromEntity.StatusId == 0 )
                fromEntity.StatusId = 2;
            to.StatusId = fromEntity.StatusId;
            if ( fromEntity.StatusId == IB.ContentItem.PUBLISHED_STATUS )
                to.IsPublished = true;
            else
                to.IsPublished = false; 
            //to.IsPublished = ( bool ) fromEntity.IsPublished;

            if ( fromEntity.PrivilegeTypeId == 0 )
                fromEntity.PrivilegeTypeId = 1;
            to.PrivilegeTypeId = fromEntity.PrivilegeTypeId;

            if ( fromEntity.ConditionsOfUseId == 0 )
                fromEntity.ConditionsOfUseId = 1;
            to.ConditionsOfUseId = fromEntity.ConditionsOfUseId;

            if ( fromEntity.UseRightsUrl != null && fromEntity.UseRightsUrl.Trim().Length > 0 )
                to.UseRightsUrl = fromEntity.UseRightsUrl;
            else
                to.UseRightsUrl = null;

            if ( fromEntity.ResourceIntId > 0 )
                to.ResourceIntId = fromEntity.ResourceIntId;
            //if ( fromEntity.ResourceVersionId > 0 )
            //    to.ResourceVersionId = fromEntity.ResourceVersionId;

            if ( fromEntity.ImageUrl != null && fromEntity.ImageUrl.Trim().Length > 0 )
                to.ImageUrl = fromEntity.ImageUrl;
            else
                to.ImageUrl = null;

            if ( fromEntity.DocumentUrl != null && fromEntity.DocumentUrl.Trim().Length > 0 )
                to.DocumentUrl = fromEntity.DocumentUrl;
            else
                to.DocumentUrl = null;
            if ( fromEntity.DocumentRowId != null && fromEntity.IsValidRowId(fromEntity.DocumentRowId) )
                to.DocumentRowId = fromEntity.DocumentRowId;
            else
                to.DocumentRowId = null;

            to.IsOrgContentOwner = fromEntity.IsOrgContentOwner;

            if ( fromEntity.OrgId > 0 )
                to.OrgId = fromEntity.OrgId;
            else
                to.OrgId = null;

            to.Timeframe = fromEntity.Timeframe != null ? fromEntity.Timeframe : "";
            if ( fromEntity.Approved != null && fromEntity.Approved > fromEntity.DefaultDate )
                to.Approved = ( System.DateTime ) fromEntity.Approved;
            else
                to.Approved = null;

            if ( fromEntity.ApprovedById > 0 )
                to.ApprovedById = fromEntity.ApprovedById;
            else
                to.ApprovedById = null;

            if ( fromEntity.Created != null )
                to.Created = fromEntity.Created;
            to.CreatedById = fromEntity.CreatedById;
            if ( fromEntity.LastUpdated != null )
                to.LastUpdated = fromEntity.LastUpdated;
            to.LastUpdatedById = fromEntity.LastUpdatedById;
           // return to;
    }

   

        private static IB.ContentItem Content_ToMap( Content fromEntity, bool doingEagerLoad, bool loadingStandards, bool loadingDocument )
        {
            IB.ContentItem to = new IB.ContentItem();

            try
            {
                if ( IsUsingContentStandards() )
                    to.UsingContentStandards = true;

                to.IsValid = true;
                to.Id = fromEntity.Id;
                to.RowId = fromEntity.RowId;

                to.Title = fromEntity.Title != null ? fromEntity.Title : "";
                to.Description = fromEntity.Description != null ? fromEntity.Description : "";
                to.Summary = fromEntity.Summary != null ? fromEntity.Summary : "";
                to.SortOrder = fromEntity.SortOrder != null ? ( int ) fromEntity.SortOrder : DefaultSortOrder;
                to.ParentId = fromEntity.ParentId != null ? ( int ) fromEntity.ParentId : 0;

                //if ( fromEntity.IsPublished != null )
                //    to.IsPublished = ( bool ) fromEntity.IsPublished;
                to.IsActive = fromEntity.IsActive == null ? true : ( bool ) fromEntity.IsActive;

                to.TypeId = fromEntity.TypeId != null ? ( int ) fromEntity.TypeId : 0;
                to.ContentType = fromEntity.ContentType != null 
                              && fromEntity.ContentType.Title != null ? fromEntity.ContentType.Title : "";
                if ( to.ContentType == "" )
                {
                    to.ContentType = ContentType_Get( to.TypeId );
                }
                to.StatusId = fromEntity.StatusId != null ? ( int ) fromEntity.StatusId : 0;
                to.Status = fromEntity.Codes_ContentStatus != null ? fromEntity.Codes_ContentStatus.Title : "";
                if (to.Status == "")
                    to.Status = to.GetStatusTitle( to.StatusId );

                to.PrivilegeTypeId = fromEntity.PrivilegeTypeId != null ? ( int ) fromEntity.PrivilegeTypeId : 1;
                to.PrivilegeType = fromEntity.Codes_ContentPrivilege != null ? fromEntity.Codes_ContentPrivilege.Title : "";

                to.ConditionsOfUseId = fromEntity.ConditionsOfUseId != null ? ( int ) fromEntity.ConditionsOfUseId : 0;
                if ( to.ConditionsOfUseId > 0  )
                {
                    try
                    {
                        LR_ConditionOfUse_Select cou = ConditionOfUse_Get( to.ConditionsOfUseId );
                        //no FK to conditions of use, the caller needs to resolve?
                        to.ConditionsOfUse = cou.Summary != null ? cou.Summary : "";

                        //need to check for custom - or get both and let interface handle
                        //- if custom exists, then ConditionsOfUseUrl will have been set to be the same
                        to.ConditionsOfUseUrl = cou.Url != null ? cou.Url : "";
                        to.ConditionsOfUseIconUrl = cou.IconUrl != null ? cou.IconUrl : "";
                    }
                    catch ( Exception ex )
                    {
                        LoggingHelper.LogError( ex, thisClassName + ".Content_ToMap() for to.ConditionsOfUseId" );
                    }
                }

                to.UseRightsUrl = fromEntity.UseRightsUrl != null ? fromEntity.UseRightsUrl : "";

                //to.ResourceVersionId = fromEntity.ResourceVersionId != null ? ( int ) fromEntity.ResourceVersionId : 0;
                to.ResourceIntId = fromEntity.ResourceIntId != null ? ( int ) fromEntity.ResourceIntId : 0;

                if ( fromEntity.ImageUrl != null && fromEntity.ImageUrl.Trim().Length > 0 )
                    to.ImageUrl = fromEntity.ImageUrl;
                else
                    to.ImageUrl = "";

                to.DocumentUrl = fromEntity.DocumentUrl != null ? fromEntity.DocumentUrl : "";
                if ( fromEntity.DocumentRowId != null && to.IsValidRowId( fromEntity.DocumentRowId ) )
                    to.DocumentRowId = ( Guid ) fromEntity.DocumentRowId;

                if ( doingEagerLoad || loadingDocument 
                    && (to.DocumentUrl != null && to.IsValidRowId( to.DocumentRowId ) ))
                {
                    //do we want this - very heavy, should be cached on file system anyway!
                    //need min of file path
                    to.RelatedDocument = Document_Version_Get( to.DocumentRowId );
                }

                to.IsOrgContentOwner = fromEntity.IsOrgContentOwner != null ? ( bool ) fromEntity.IsOrgContentOwner : false;

                to.OrgId = fromEntity.OrgId != null ? ( int ) fromEntity.OrgId : 0;
                if ( to.OrgId > 0 && doingEagerLoad )
                {
                    //this also should be cached, as unlikely to change for a child item
                    Gateway_OrgSummary org = Gateway_OrgSummary_Get( to.OrgId );

                    if ( org != null && org.id > 0 )
                    {
                        to.Organization = org.Name;
                        to.ParentOrgId = org.parentId != null ? ( int ) org.parentId : 0;
                        to.ParentOrganization = org.ParentOrganization != null ? org.ParentOrganization : "";
                    }
                }
                to.Timeframe = fromEntity.Timeframe != null ? fromEntity.Timeframe : "";
                
                //And/OR get content standards
                if ( loadingStandards )
                {
                    //get standards==> only do if no content standards -= transitioning

                    if ( to.UsingContentStandards )
                    {
                        to.ContentStandards = Fill_ContentStandards( to.Id );
                    }
                    else if ( to.ResourceIntId > 0 )
                    {
                        to.Standards = FillResourceStandards( to.Id, to.ResourceIntId );
                    }
                }
             

                to.Approved = fromEntity.Approved != null ? ( System.DateTime ) fromEntity.Approved : to.DefaultDate;
                to.ApprovedById = fromEntity.ApprovedById != null ? ( int ) fromEntity.ApprovedById : 0;

                to.Created = fromEntity.Created != null ? ( System.DateTime ) fromEntity.Created : to.DefaultDate;
                to.CreatedById = fromEntity.CreatedById != null ? ( int ) fromEntity.CreatedById : 0;
                to.LastUpdated = fromEntity.LastUpdated != null ? ( System.DateTime ) fromEntity.LastUpdated : to.DefaultDate;
                to.LastUpdatedById = fromEntity.LastUpdatedById != null ? ( int ) fromEntity.LastUpdatedById : 0;
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".Content_ToMap( Content fromEntity )" );
            }
            return to;
        }
        private static IB.ContentItem ContentSummary_ToMap( ContentSummaryView fromEntity, bool doingEagerLoad )
        {
            IB.ContentItem to = new IB.ContentItem();

            try
            {
                if ( IsUsingContentStandards() )
                    to.UsingContentStandards = true;

                to.IsValid = true;
                to.Id = fromEntity.ContentId;
                to.RowId = fromEntity.ContentRowId;

                to.Title = fromEntity.Title != null ? fromEntity.Title : "";
                to.Description = fromEntity.Description != null ? fromEntity.Description : "";
                to.Summary = fromEntity.Summary != null ? fromEntity.Summary : "";
                to.SortOrder = fromEntity.SortOrder != null ? ( int ) fromEntity.SortOrder : DefaultSortOrder;
                to.ParentId = fromEntity.ParentId;

                //to.IsPublished = fromEntity.IsPublished;
                to.IsActive = fromEntity.IsActive == null ? true : ( bool )fromEntity.IsActive;

                to.TypeId = fromEntity.TypeId != null ? ( int )fromEntity.TypeId : 0;
                to.ContentType = fromEntity.ContentType != null
                              && fromEntity.ContentType != null ? fromEntity.ContentType : "";
             
                to.StatusId = fromEntity.StatusId != null ? ( int )fromEntity.StatusId : 0;
                to.Status = fromEntity.ContentStatus != null ? fromEntity.ContentStatus : "";

                to.PrivilegeTypeId = fromEntity.PrivilegeTypeId != null ? ( int )fromEntity.PrivilegeTypeId : 0;
                to.PrivilegeType = fromEntity.ContentPrivilege != null ? fromEntity.ContentPrivilege : "";

                to.ConditionsOfUseId = fromEntity.ConditionsOfUseId;
                to.ConditionsOfUse = fromEntity.Summary != null ? fromEntity.Summary : "";
                to.ConditionsOfUseUrl = fromEntity.ConditionsOfUseUrl != null ? fromEntity.ConditionsOfUseUrl : "";
                to.ConditionsOfUseIconUrl = fromEntity.ConditionsOfUseIconUrl != null ? fromEntity.ConditionsOfUseIconUrl : "";

                to.UseRightsUrl = fromEntity.UseRightsUrl != null ? fromEntity.UseRightsUrl : "";

                //to.ResourceVersionId = fromEntity.ResourceVersionId != null ? ( int ) fromEntity.ResourceVersionId : 0;
                to.ResourceIntId = fromEntity.ResourceIntId != null ? ( int )fromEntity.ResourceIntId : 0;

                if ( fromEntity.ImageUrl != null && fromEntity.ImageUrl.Trim().Length > 0 )
                    to.ImageUrl = fromEntity.ImageUrl;
                else
                    to.ImageUrl = "";

                to.DocumentUrl = fromEntity.DocumentUrl != null ? fromEntity.DocumentUrl : "";
                if ( fromEntity.DocumentRowId != null && to.IsValidRowId( fromEntity.DocumentRowId ) )
                    to.DocumentRowId = ( Guid )fromEntity.DocumentRowId;

                if ( to.DocumentUrl != null && to.IsValidRowId( to.DocumentRowId ) )
                {
                    to.RelatedDocument = Document_Version_Get( to.DocumentRowId );
                }

                to.IsOrgContentOwner = fromEntity.IsOrgContentOwner != null ? ( bool )fromEntity.IsOrgContentOwner : false;

                to.OrgId = fromEntity.OrgId;
                if ( to.OrgId > 0 && doingEagerLoad )
                {
                    Gateway_OrgSummary org = Gateway_OrgSummary_Get( to.OrgId );

                    if ( org != null && org.id > 0 )
                    {
                        to.Organization = org.Name;
                        to.ParentOrgId = org.parentId != null ? ( int )org.parentId : 0;
                        to.ParentOrganization = org.ParentOrganization != null ? org.ParentOrganization : "";
                    }
                }
                to.Timeframe = fromEntity.Timeframe != null ? fromEntity.Timeframe : "";

                //And/OR get content standards
                if ( doingEagerLoad )
                {
                    //get standards==> only do if no content standards -= transitioning
                    if ( to.UsingContentStandards )
                    {
                        to.ContentStandards = Fill_ContentStandards( to.Id );
                    }
                    else if ( to.ResourceIntId > 0 )
                    {
                        to.Standards = FillResourceStandards( to.Id, to.ResourceIntId );
                    }
                }


                to.Approved = fromEntity.Approved != null ? ( System.DateTime )fromEntity.Approved : to.DefaultDate;
                to.ApprovedById = fromEntity.ApprovedById != null ? ( int )fromEntity.ApprovedById : 0;

                to.Created = fromEntity.Created != null ? ( System.DateTime )fromEntity.Created : to.DefaultDate;
                to.CreatedBy = fromEntity.Author;
                to.CreatedById = fromEntity.CreatedById != null ? ( int )fromEntity.CreatedById : 0;
                to.LastUpdated = fromEntity.LastUpdated != null ? ( System.DateTime )fromEntity.LastUpdated : to.DefaultDate;
                to.LastUpdatedById = fromEntity.LastUpdatedById != null ? ( int )fromEntity.LastUpdatedById : 0;
                to.LastUpdatedBy = fromEntity.LastUpdatedBy;
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ContentSummary_ToMap( Content fromEntity )" );
            }
            return to;
        }

        /// <summary>
        /// Renumber (update sortOrder) for all children items
        /// </summary>
        /// <param name="contentId"></param>
        public static bool Content_RenumberHierarchy( int contentId )
        {
            bool isValid = true;
            try
            {
                using ( var context = new IsleContentContext() )
                {
                    Content efEntity = context.Contents.SingleOrDefault( s => s.Id == contentId );
                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        Content_RenumberHierarchyChildren( context, efEntity );
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".Content_RenumberHierarchy()" );
                return false;
            }
            return isValid;
        }//

        private static void Content_RenumberHierarchyChildren( IsleContentContext context, Content entity )
        {
            int sortOrder = DefaultSortOrder;
            List<IB.ContentItem> items = new List<IB.ContentItem>();

            List<Content> eflist = context.Contents
                            .Where( s => s.ParentId == entity.Id )
                            .OrderBy( s => s.SortOrder )
                            .ToList();

            if ( eflist != null && eflist.Count > 0 )
            {
                foreach ( Content efom in eflist )
                {
                    //special case - leave zero values alone
                    if ( efom.SortOrder> 0 && efom.SortOrder != sortOrder )
                    {
                        //only update if has not changed
                        efom.SortOrder = sortOrder;
                        context.SaveChanges();
                    }
                    //get this items children
                    Content_RenumberHierarchyChildren( context, efom );

                    sortOrder += DefaultSortOrder;
                }
            }
            
        }

      

        /// <summary>
        /// Get content item for node and all related content resources
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="doCompleteFill">If true, fill all appicable child items</param>
        /// <returns></returns>
        public static IB.ContentItem Content_GetHierarchyNode( NodeRequest request )
        {
            int cacheHours = UtilityManager.GetAppKeyValue( "curriculumCacheHours", 1 );
            bool cachingAllNodes = UtilityManager.GetAppKeyValue( "cacheAllNodes", false );
            int cacheHoursElapsed = cacheHours * -1;

            using ( var context = new IsleContentContext() )
            {
                IB.ContentItem entity = Content_Get( context, request.ContentId, true );
                if ( entity == null || entity.Id == 0 )
                    return entity;

                //just cache Curriculumn for now
                //NOTE - the big issue is the standards!
                string key = "";

                if ( entity.TypeId == IB.ContentItem.CURRICULUM_CONTENT_ID 
                    || ( cachingAllNodes  && entity.TypeId > IB.ContentItem.CURRICULUM_CONTENT_ID) )
                {
                    key = "ContentNodeCache" + entity.Id.ToString();

                    if ( HttpRuntime.Cache[ key ] != null && request.AllowCaching )
                    {
                        var cache = ( CachedContent ) HttpRuntime.Cache[ key ];
                        try
                        {
                            if ( cache.lastUpdated > DateTime.Now.AddHours( cacheHoursElapsed ) )
                            {
                                LoggingHelper.DoTrace( 2, string.Format( "===Content_GetHierarchyNode === Using cached version of node, Id: {0}, {1}, {2}", entity.Id, entity.Title, entity.TypeId ) );
                                return cache.Item;
                            }
                        }
                        catch ( Exception ex )
                        {
                            LoggingHelper.DoTrace( 2, "===Content_GetHierarchyNode === exception " + ex.Message );
                        }
                    }
                    else
                    {
                        LoggingHelper.DoTrace( 2, string.Format( "****** Content_GetHierarchyNode === Retrieving full version of curriculum, Id: {0}, {1}", entity.Id, entity.Title ) );
                    }
                }
            

                Content_FillHierarchyNode( context, entity, true );

                //Cache the output
                if ( key.Length > 0 )
                {
                    var newCache = new CachedContent()
                    {
                        Item = entity,
                        lastUpdated = DateTime.Now
                    };

                    if ( HttpContext.Current.Cache[ key ] != null )
                    {
                        HttpRuntime.Cache.Remove( key );
                        HttpRuntime.Cache.Insert( key, newCache );

                        //HttpContext.Current.Cache.Remove( key );
                        //HttpContext.Current.Cache.Insert( key,  newCache );

                        LoggingHelper.DoTrace( 2, string.Format( "===Content_GetHierarchyNode $$$ Updating cached version of curriculum, Id: {0}, {1}", entity.Id, entity.Title ) );

                    }
                    else
                    {
                        LoggingHelper.DoTrace( 2, string.Format( "===Content_GetHierarchyNode ****** Inserting new cached version of curriculum, Id: {0}, {1}", entity.Id, entity.Title ) );
                        //HttpContext.Current.Cache.Insert( key, newCache );

                        System.Web.HttpRuntime.Cache.Insert( key, newCache, null, DateTime.Now.AddHours( cacheHours ), TimeSpan.Zero );
                    }
                }
                return entity;
            }
        }


        /// <summary>
        /// Get content item for node and all related content resources
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="doCompleteFill">If true, fill all appicable child items</param>
        /// <returns></returns>
        public static void Content_FillHierarchyNode( IsleContentContext context, IB.ContentItem entity, bool doCompleteFill )
        {

            //LoggingHelper.DoTrace( 8, string.Format( "Content_GetHierarchyNode. Id: {0}, parentId: {1}, Node: {2}, type: {3}", entity.Id, entity.ParentId, entity.Title, entity.TypeId ) );
             //get standards (check if already done)
            //14-04-11 MP - now attempting to fill standards in Get, so no need to check here!
            //if ( entity.ResourceIntId > 0
            //    && ( entity.Standards == null || entity.Standards.Count == 0) )
            //    entity.Standards = FillResourceStandards( entity.ResourceIntId );
            entity.ChildrenStandards = new List<IB.ContentResourceStandard>();
            entity.ContentChildrenStandards = new List<IB.Content_StandardSummary>();

            //now get all direct child content items 
            entity.ChildItems = new List<IB.ContentItem>();

            List<Content> eflist = context.Contents
                            .Where( s => s.ParentId == entity.Id
                                && ( s.TypeId == IB.ContentItem.DOCUMENT_CONTENT_ID
                                    || s.TypeId == IB.ContentItem.EXTERNAL_URL_CONTENT_ID )
                                    )
                            .OrderBy( s => s.SortOrder ).ThenBy( s => s.Title )
                            .ToList();

            if ( eflist != null && eflist.Count > 0 )
            {
                foreach ( Content efom in eflist )
                {
                    //14-12-16 mp - no reason to do complete fill for a document item - review and minimize
                    IB.ContentItem child = Content_ToMap( efom, false, true, false ); //??????
                    if ( child.IsDocumentType 
                        &&  child.SortOrder == IB.ContentItem.FEATURED_CONTENT_SORT_ORDER )
                    {
                        //set now, but need to remove later dependent on privileges
                        entity.AutoPreviewUrl = child.DocumentUrl;
                    }

                    entity.ChildItems.Add( child );

                    if ( child.HasStandards )
                    {
                        //entity.ChildrenStandards.AddRange( child.Standards );
                        Content_MergeStandards( child, entity );
                    }
                }
            }
         


            if ( entity.IsHierarchyType  )
            {
                //get standards from all children and merge
                //need to skip docs, as already done
                List<Content> childList = context.Contents
                            .Where( s => s.ParentId == entity.Id && s.TypeId > IB.ContentItem.CURRICULUM_CONTENT_ID )
                            .OrderBy( s => s.SortOrder )
                            .ToList();

                if ( childList != null && childList.Count > 0 )
                {
                    foreach ( Content nodeChild in childList )
                    {
                        //LoggingHelper.DoTrace( 8, string.Format( "======================== Child Node: {0}, type: {1}", nodeChild.Title, nodeChild.TypeId ) );
                        //TODO - mapping needs to be thinner here!!
                        IB.ContentItem childNode = Content_ToMap( nodeChild, false, true, false );

                        Content_FillHierarchyNode( context, childNode, true );
                        //don't add
                        //entity.ChildItems.Add( childNode );

                        if ( childNode.HasStandards || childNode.HasChildStandards )
                        {
                            //entity.ChildrenStandards.AddRange( child.Standards );
                            Content_MergeStandards( childNode, entity );
                        }

                    }
                }
            }

        }//


        private static void GetDistinctCurriculumStandards( IsleContentContext context, IB.ContentItem entity )
        {
            IB.Content_StandardSummary item = new IB.Content_StandardSummary();
            List<IB.Content_StandardSummary> list = new List<IB.Content_StandardSummary>();

            //get standards from all children and merge
            //need to skip docs, as already done
            List<Curriculum_DistinctStandards> childList = context.Curriculum_DistinctStandards
                        .Where( s => s.CurriculumId == entity.Id )
                        .OrderBy( s => s.UsageTypeId ).ThenBy(s => s.NotationCode)
                        .ToList();

            if ( childList != null && childList.Count > 0 )
            {
                foreach ( Curriculum_DistinctStandards efom in childList )
                {

                    item = new IB.Content_StandardSummary();
                    item.ContentId = efom.CurriculumId;
                    item.Id = efom.ContentStandardId;
                    item.StandardId = efom.StandardId;
                    item.StandardUrl = efom.StandardUrl;
                    item.NotationCode = efom.NotationCode;
                    item.Description = efom.Description;

                    //if ( efom.Created != null )
                    //    item.Created = ( System.DateTime ) efom.Created;
                    //item.CreatedById = efom.CreatedById;

                    //if ( efom.LastUpdated != null )
                    //    item.LastUpdated = ( System.DateTime ) efom.LastUpdated;
                    //item.LastUpdatedById = efom.LastUpdatedById;

                    item.AlignmentType = efom.AlignmentType;
                    //item.AlignmentTypeCodeId = efom.AlignmentTypeCodeId;
                    item.UsageTypeId = efom.UsageTypeId;
                    item.StandardUsage = efom.StandardUsage;

                    list.Add( item );

                    //if ( childNode.HasStandards || childNode.HasChildStandards )
                    //{
                    //    //entity.ChildrenStandards.AddRange( child.Standards );
                    //    Content_MergeStandards( childNode, entity );
                    //}

                }
            }
        }//

        
        private static void Content_MergeStandards( IB.ContentItem childEntity, IB.ContentItem parentEntity)
        {

            if ( childEntity.HasStandards == false && childEntity.HasChildStandards == false )
                return;

            List<IB.Content_StandardSummary> joinList = parentEntity.ContentChildrenStandards.ToList();

            //check child entity standards, and add to parent if do not already exist
            foreach ( IB.Content_StandardSummary s in childEntity.ContentStandards )
              {
                 var existing = joinList.FirstOrDefault(x => x.StandardId == s.StandardId && x.UsageTypeId == s.UsageTypeId);
                 if (existing != null)
                 {
                     //not sure what the following does, or if still necessary
                     //add contentId to list - not right, fix in future **************************
                    existing.ContentItemIds.Add( s.ContentId);
                     //have to update item in joinlist - how?
                 }
                 else
                 {
                    joinList.Add(s);
                 }
              }

            foreach ( IB.Content_StandardSummary s in childEntity.ContentChildrenStandards )
              {
                  var existing = joinList.FirstOrDefault( x => x.StandardId == s.StandardId && x.UsageTypeId == s.UsageTypeId );
                  if ( existing != null )
                  {
                      //add contentId to list
                      existing.ContentItemIds.Add( s.ContentId );
                  }
                  else
                  {
                      joinList.Add( s );
                  }
              }

            parentEntity.ContentChildrenStandards = joinList;
        } //

        /// <summary>
        /// soon to be obsolete!!??
        /// </summary>
        /// <param name="childEntity"></param>
        /// <param name="parentEntity"></param>
        [Obsolete]
        private static void Content_MergeResourceStandards( IB.ContentItem childEntity, IB.ContentItem parentEntity )
        {

            if ( childEntity.HasStandards == false && childEntity.HasChildStandards == false )
                return;

            List<IB.ContentResourceStandard> joinList = parentEntity.ChildrenStandards.ToList();

            foreach ( IB.ContentResourceStandard s in childEntity.Standards )
            {
                var existing = joinList.FirstOrDefault( x => x.StandardId == s.StandardId );
                if ( existing != null )
                {
                    //add contentId to list - not right, fix in future **************************
                    existing.ContentItemIds.Add( s.ContentId );
                    //have to update item in joinlist - how?
                }
                else
                {
                    joinList.Add( s );
                }
            }

            foreach ( IB.ContentResourceStandard s in childEntity.ChildrenStandards )
            {
                var existing = joinList.FirstOrDefault( x => x.StandardId == s.StandardId );
                if ( existing != null )
                {
                    //add contentId to list
                    existing.ContentItemIds.Add( s.ContentId );
                }
                else
                {
                    joinList.Add( s );
                }
            }

            parentEntity.ChildrenStandards = joinList;
        } //

        private static IB.ContentItem Content_ToMap( ContentConnector_ChildSummary fromEntity, bool doingCompleteFill )
        {
            IB.ContentItem to = new IB.ContentItem();
            try
            {
                if ( IsUsingContentStandards() )
                    to.UsingContentStandards = true;

                to.IsValid = true;
                to.Id = fromEntity.ContentId;
                to.RowId = fromEntity.ContentRowId;

                to.Title = fromEntity.Title != null ? fromEntity.Title : "";
                to.Description = fromEntity.Description != null ? fromEntity.Description : "";
                to.Summary = fromEntity.Summary != null ? fromEntity.Summary : "";
                to.SortOrder = fromEntity.SortOrder;
                to.ParentId = fromEntity.ParentId;

                //to.IsPublished = ( bool ) fromEntity.IsPublished;
                to.IsActive = ( bool ) fromEntity.IsActive;

                to.TypeId = fromEntity.TypeId != null ? ( int ) fromEntity.TypeId : 0;
                to.ContentType = fromEntity.ContentType != null ? fromEntity.ContentType : "";
                if ( to.ContentType == "" )
                {
                    to.ContentType = ContentType_Get( to.TypeId );
                }
                to.StatusId = fromEntity.StatusId != null ? ( int ) fromEntity.StatusId : 0;
                to.Status = fromEntity.ContentStatus != null ? fromEntity.ContentStatus : "";


                to.PrivilegeTypeId = fromEntity.PrivilegeTypeId != null ? ( int ) fromEntity.PrivilegeTypeId : 0;
                to.PrivilegeType = fromEntity.ContentPrivilege != null ? fromEntity.ContentPrivilege : "";

                to.ConditionsOfUseId = fromEntity.ConditionsOfUseId;
                if ( to.ConditionsOfUseId > 0 )
                {
                    LR_ConditionOfUse_Select cou = ConditionOfUse_Get( to.ConditionsOfUseId );
                    //no FK to conditions of use, the caller needs to resolve?
                    to.ConditionsOfUse = cou.Summary != null ? cou.Summary : "";

                    //need to check for custom - or get both and let interface handle
                    //- if custom exists, then ConditionsOfUseUrl will have been set to be the same
                    to.ConditionsOfUseUrl = cou.Url != null ? cou.Url : "";
                    to.ConditionsOfUseIconUrl = cou.IconUrl != null ? cou.IconUrl : "";
                }

                to.UseRightsUrl = fromEntity.UseRightsUrl != null ? fromEntity.UseRightsUrl : "";

                //to.ResourceVersionId = fromEntity.ResourceVersionId;
                to.ResourceIntId = fromEntity.ResourceIntId != null ? ( int ) fromEntity.ResourceIntId : 0;
                to.ResourceUrl = fromEntity.ResourceUrl != null ? fromEntity.ResourceUrl : "";

                to.DocumentUrl = fromEntity.DocumentUrl != null ? fromEntity.DocumentUrl : "";
                if ( fromEntity.DocumentRowId != null )
                    to.DocumentRowId = ( Guid ) fromEntity.DocumentRowId;
                if ( to.DocumentUrl != null && to.IsValidRowId( to.DocumentRowId ) )
                {
                    to.RelatedDocument = Document_Version_Get( to.DocumentRowId );
                }

                to.IsOrgContentOwner = fromEntity.IsOrgContentOwner;

                to.OrgId = fromEntity.OrgId;
                if ( to.OrgId > 0 )
                {
                    Gateway_OrgSummary org = Gateway_OrgSummary_Get( to.OrgId );

                    if ( org != null && org.id > 0 )
                    {
                        to.Organization = org.Name;

                        to.ParentOrgId = org.parentId != null ? ( int ) org.parentId : 0;
                        to.ParentOrganization = org.ParentOrganization != null ? org.ParentOrganization : "";
                    }
                }
                if ( doingCompleteFill )
                {
                    //get standards
                    if ( to.UsingContentStandards )
                    {
                        to.ContentStandards = Fill_ContentStandards( to.Id );
                    }
                    else if ( to.ResourceIntId > 0 )
                    {
                        to.Standards = FillResourceStandards( to.Id, to.ResourceIntId );
                    }
                }

                to.Approved = fromEntity.Approved != null ? ( System.DateTime ) fromEntity.Approved : to.DefaultDate;
                to.ApprovedById = fromEntity.ApprovedById != null ? ( int ) fromEntity.ApprovedById : 0;

                to.Created = fromEntity.Created != null ? ( System.DateTime ) fromEntity.Created : to.DefaultDate;
                to.CreatedById = fromEntity.CreatedById != null ? ( int ) fromEntity.CreatedById : 0;
                to.LastUpdated = fromEntity.LastUpdated != null ? ( System.DateTime ) fromEntity.LastUpdated : to.DefaultDate;
                to.LastUpdatedById = fromEntity.LastUpdatedById != null ? ( int ) fromEntity.LastUpdatedById : 0;
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "Content_ToMap( ContentConnector_ChildSummary fromEntity )" );
            }
            return to;
        }

        public static List<IB.ContentResourceStandard> FillResourceStandards( int contentId, int resourceId )
        {
            List<IB.ContentResourceStandard> list = new List<IB.ContentResourceStandard>();

            using ( var context = new IsleContentContext() )
            {
                List<Resource_Standard> eflist = context.Resource_Standard
                                .Where( s => s.ResourceIntId == resourceId )
                                .OrderBy( s => s.NotationCode )
                                .ToList();

                if ( eflist != null && eflist.Count > 0 )
                {
                    foreach ( Resource_Standard efom in eflist )
                    {
                        IB.ContentResourceStandard child = new IB.ContentResourceStandard();
                        child.ContentId = contentId;
                        //doesn't make sense here, only in an aggregator context. Or maybe should be here - will test
                        child.ContentItemIds.Add( contentId );

                        child.ResourceStandardId = efom.ResourceStandardId;
                        child.ResourceIntId = efom.ResourceIntId;
                        //child.ResourceVersionIntId = efom.ResourceVersionIntId;
                        child.ResourceTitle = efom.ResourceTitle;
                        child.ResourceSortTitle = efom.ResourceSortTitle;

                        child.StandardId = efom.StandardId;
                        child.StandardUrl = efom.StandardUrl;
                        child.NotationCode = efom.NotationCode;
                        child.Description = efom.Description;
                        child.AlignedById = efom.AlignedById;

                        child.AlignmentTypeCodeId = efom.AlignmentTypeCodeId;
                        child.AlignmentType = efom.AlignmentType;

                        child.AlignmentDegreeId = efom.AlignmentDegreeId;
                        child.AlignmentDegree = efom.AlignmentDegree;
                        if ( efom.Created != null )
                            child.Created = ( System.DateTime ) efom.Created;
                        child.AlignmentDegree = efom.AlignmentDegree;

                        list.Add( child );
                    }
                }
            }
            return list;

        }
        private static List<IB.ContentResourceStandard> FillResourceStandardsByRvId( int contentId, int resourceVersionId )
        {
            List<IB.ContentResourceStandard> list = new List<IB.ContentResourceStandard>();

            using ( var context = new IsleContentContext() )
            {
                List<Resource_Standard> eflist = context.Resource_Standard
                                .Where( s => s.ResourceVersionIntId == resourceVersionId )
                                .OrderBy( s => s.NotationCode )
                                .ToList();

                if ( eflist != null && eflist.Count > 0 )
                {
                    foreach ( Resource_Standard efom in eflist )
                    {
                        IB.ContentResourceStandard child = new IB.ContentResourceStandard();
                        child.ContentId = contentId;
                        child.ResourceStandardId = efom.ResourceStandardId;
                        child.ResourceIntId = efom.ResourceIntId;
                        //child.ResourceVersionIntId = efom.ResourceVersionIntId;
                        child.ResourceTitle = efom.ResourceTitle;
                        child.ResourceSortTitle = efom.ResourceSortTitle;

                        child.StandardId = efom.StandardId;
                        child.StandardUrl = efom.StandardUrl;
                        child.NotationCode = efom.NotationCode;
                        child.Description = efom.Description;
                        child.AlignedById = efom.AlignedById;

                        child.AlignmentTypeCodeId = efom.AlignmentTypeCodeId;
                        child.AlignmentType = efom.AlignmentType;

                        child.AlignmentDegreeId = efom.AlignmentDegreeId;
                        child.AlignmentDegree = efom.AlignmentDegree;
                        if ( efom.Created != null )
                            child.Created = ( System.DateTime ) efom.Created;
                        child.AlignmentDegree = efom.AlignmentDegree;

                        list.Add( child );
                    }
                }
            }
            return list;

        }

        /// <summary>
        /// Return true if node is part of a hierarchy
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool IsNodePartOfHierarchy( IB.ContentItem entity )
        {
            bool isPartOfHierarchy = false; 
            int parentId = 0;
            if ( entity.TypeId >= IB.ContentItem.CURRICULUM_CONTENT_ID 
              && entity.TypeId <= IB.ContentItem.ASSESSMENT_CONTENT_ID )
            {
                return true;
            }
            else if ( entity.TypeId != IB.ContentItem.DOCUMENT_CONTENT_ID
                   && entity.TypeId != IB.ContentItem.EXTERNAL_URL_CONTENT_ID )
            {
                return false;
            }

            if ( entity.ParentId == 0 )
                if ( usingContentConnector )
                    parentId = ContentConnector_GetParent( entity );
            else
            {
                //if has parent, just use it for now.
                parentId = entity.ParentId;
            }

            int currId = 0;
            //loop until a hierarchy type is found
            using ( var context = new IsleContentContext() )
            {
                int typeId = 0;
                while ( isPartOfHierarchy == false )
                {
                    Content parent = context.Contents
                                .SingleOrDefault( s => s.Id == parentId );

                    if ( parent != null && parent.Id > 0 )
                    {
                        parentId = parent.ParentId == null ? 0 : ( int ) parent.ParentId;
                        typeId = parent.TypeId == null ? 0 : ( int ) parent.TypeId;
                        if ( typeId >= IB.ContentItem.CURRICULUM_CONTENT_ID
                          && typeId <= IB.ContentItem.LESSON_CONTENT_ID )
                        {
                            isPartOfHierarchy = true;
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if ( currId > 0 )
                    return true;
            }
            return isPartOfHierarchy;
        }

        /// <summary>
        /// Return the top node of a hierarchy
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static IB.ContentItem GetTopNode(IB.ContentItem entity)
        {
            int topNodeId = 0;
            int parentId = 0;
            IB.ContentItem topNode = new IB.ContentItem();
            Content parent = new Content();

            if ( entity.TypeId > IB.ContentItem.CURRICULUM_CONTENT_ID 
              || entity.TypeId == IB.ContentItem.DOCUMENT_CONTENT_ID
              || entity.TypeId == IB.ContentItem.EXTERNAL_URL_CONTENT_ID)
            {
                parentId = entity.ParentId;
            }
                //obsolete
            else if ( entity.TypeId == IB.ContentItem.DOCUMENT_CONTENT_ID
                   || entity.TypeId == IB.ContentItem.EXTERNAL_URL_CONTENT_ID )
            {
                if ( usingContentConnector )
                {
                    //for a document, will need to get the lesson as well!
                    //start with connector - initially will only be one, but may need to handle in future
                    parentId = ContentConnector_GetParent( entity );
                }
            }

            //loop until a hierarchy type is found
            using ( var context = new IsleContentContext() )
            {
                int typeId = 0;
                while ( parentId > 0 )
                {
                    parent = context.Contents
                       .SingleOrDefault( s => s.Id == parentId );

                    if ( parent != null && parent.Id > 0 )
                    {
                        parentId = parent.ParentId == null ? 0 : ( int ) parent.ParentId;
                        typeId = parent.TypeId == null ? 0 : ( int ) parent.TypeId;

                        //need to handle any hierarchy top, not just curriculum
                        //so retain most recent parent
                        topNodeId = parent.Id;

                        if ( typeId == IB.ContentItem.CURRICULUM_CONTENT_ID )
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (topNodeId > 0)
                topNode = Content_ToMap(parent, false, false, false);


            return topNode;
        }

        /// <summary>
        /// Get the full curriculum for the related node
        /// </summary>
        /// <param name="entity"></param>
        //public static CurriculumDTO GetCurriculumForNode( IB.ContentItem entity )
        //{
        //    CurriculumDTO dto = new CurriculumDTO();

        //    //IB.ContentItem item = new IB.ContentItem();
        //    int parentId = 0;
        //    dto.StartingNodeId = 0;

        //    if ( entity.TypeId > IB.ContentItem.CURRICULUM_CONTENT_ID )
        //    {
        //        parentId = entity.ParentId;
        //        dto.StartingNodeId = entity.Id;
        //    }
        //    else if ( entity.TypeId == IB.ContentItem.DOCUMENT_CONTENT_ID )
        //    {
        //        //for a document, will need to get the lesson as well!
        //        //start with connector - initially will only be one, but may need to handle in future
        //        parentId = ContentConnector_GetParent( entity );
        //        //hmmm, the starting node for a doc is probably, the connector parent (typically a lesson), so may not have to do later
        //        dto.StartingNodeId = parentId;
        //    }

        //    int currId = 0;
        //    using ( var context = new IsleContentContext() )
        //    {
        //        int typeId = 0;
        //        while ( parentId > 0 )
        //        {
        //            Content parent = context.Contents
        //               .SingleOrDefault( s => s.Id == parentId );

        //            if ( parent != null && parent.Id > 0 )
        //            {
        //                parentId = parent.ParentId == null ? 0 : ( int ) parent.ParentId;
        //                typeId = parent.TypeId == null ? 0 : ( int ) parent.TypeId;
        //                if ( typeId == IB.ContentItem.CURRICULUM_CONTENT_ID )
        //                {
        //                    currId = parent.Id;
        //                    break;
        //                }
        //                else if ( entity.TypeId == IB.ContentItem.DOCUMENT_CONTENT_ID
        //                     && typeId == IB.ContentItem.LESSON_CONTENT_ID )
        //                {
        //                    dto.StartingNodeId = parent.Id;
        //                }
        //            }
        //            else
        //            {
        //                break;
        //            }
        //        }
        //        if ( currId > 0 )
        //            dto.Curriculum = Content_GetHierarchyOutline( currId, false );
        //    }
        //    return dto;
        //}
        #endregion

        #region === Download Content =======================
        /// <summary>
        /// Retrieve a node for purposes of download - doing lazy fill
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="includingChildren"></param>
        /// <param name="doCompleteFill"></param>
        /// <returns></returns>
        public static IB.ContentItem GetHierarchyForDownload( int contentId, bool includingChildren, bool doCompleteFill )
        {
            using ( var context = new IsleContentContext() )
            {
                IB.ContentItem entity = Content_Get( context, contentId, false );
                if ( entity == null || entity.Id == 0 )
                    return entity;

                FillHierarchyForDownload( context, entity, includingChildren, false );

                return entity;
            }
        }

        private static void FillHierarchyForDownload( IsleContentContext context, IB.ContentItem entity, bool includingChildren, bool doCompleteFill )
        {

            //now get all content items connected thru the content connector
            //document types (40) or reference url types (41)
            entity.ChildItems = new List<IB.ContentItem>();

            //if ( usingContentConnector == false )
            //{
            List<Content_ChildResourceSummary> eflist = context.Content_ChildResourceSummary
                            .Where( s => s.ParentId == entity.Id
                                && ( s.TypeId == IB.ContentItem.DOCUMENT_CONTENT_ID
                                  || s.TypeId == IB.ContentItem.EXTERNAL_URL_CONTENT_ID )
                                 )
                            .OrderBy( s => s.SortOrder ).ThenBy( s => s.Title )
                            .ToList();
            IB.ContentItem to = new IB.ContentItem();
            //IB.NodeResource to = new IB.NodeResource();

            if ( eflist != null && eflist.Count > 0 )
            {
                foreach ( Content_ChildResourceSummary fromEntity in eflist )
                {
                    try
                    {

                        to = new IB.ContentItem();

                        to.Id = fromEntity.ContentId;
                        to.ParentId = fromEntity.ParentId != null ? ( int ) fromEntity.ParentId : 0;
                        //just in case, use the orgId from parent
                        to.OrgId = entity.OrgId;

                        to.TypeId = fromEntity.TypeId != null ? ( int ) fromEntity.TypeId : 0;
                        to.ContentType = fromEntity.ContentType != null ? fromEntity.ContentType : "Document";
                        to.SortOrder = fromEntity.SortOrder != null ? ( int ) fromEntity.SortOrder : DefaultSortOrder;

                        to.Title = fromEntity.Title != null ? fromEntity.Title : "";
                        to.Description = fromEntity.Description != null ? fromEntity.Description : "";
                        to.StatusId = fromEntity.StatusId != null ? ( int ) fromEntity.StatusId : 2;
                        to.PrivilegeTypeId = fromEntity.PrivilegeTypeId != null ? ( int ) fromEntity.PrivilegeTypeId : 1;

                        to.DocumentUrl = fromEntity.DocumentUrl != null ? fromEntity.DocumentUrl : "";

                        if ( fromEntity.DocumentRowId != null )
                            to.DocumentRowId = ( Guid ) fromEntity.DocumentRowId;
                        to.MimeType = fromEntity.MimeType != null ? fromEntity.MimeType : "";
                        to.FileName = fromEntity.FileName != null ? fromEntity.FileName : "";
                        to.FilePath = fromEntity.FilePath != null ? fromEntity.FilePath : "";

                        bool isValidResource = true;
                        bool updatingDoc = false;
                        if ( to.TypeId == IB.ContentItem.DOCUMENT_CONTENT_ID )
                        {
                            //to.DocumentPath = fromEntity.DocumentPath != null ? fromEntity.DocumentPath : "";         
                            //need the parent context/url
                            if ( to.FilePath == null || to.FilePath.Trim().Length < 5 )
                            {
                                to.FilePath = FileSystemHelper.SetFilePathFromUrl( to.DocumentUrl, to.FileName );
                                if ( to.FilePath != null && to.FilePath.Trim().Length > 5 )
                                    updatingDoc = true;
                            }
                            //not really needed
                            to.DocumentPath = to.FileLocation();

                            //do sync check
                            //better to do here to have consistant data going out!
                            if ( FileSystemHelper.DoesFileExist( to.FilePath, to.FileName ) == false )
                            {
                                //need to get full doc, and cache
                                //to.RelatedDocument = Document_Version_Get( to.DocumentRowId );
                                IB.DocumentVersion doc = Document_Version_Get( to.DocumentRowId );
                                string message = FileSystemHelper.HandleDocumentCaching( to.FilePath, doc, true );
                                if ( message.Length > 0 )
                                {
                                    //problem. May want to mark it,or exclude so downstream doesn't try to handle it??
                                    isValidResource = false;
                                    to.DocumentPath = "";
                                    LoggingHelper.LogError( thisClassName + string.Format( ".FillHierarchyForDownload( Content fromEntity ({0} ). INCONSISTENCY: Unable to cache document:DocEntityId: {1}", entity.Id, fromEntity.ContentId ), true );
                                }
                                else
                                {
                                    updatingDoc = true;
                                }
                            }

                            if ( updatingDoc )
                            {
                                //update, just the path, but the helper method handles the three referenced properties
                                IB.DocumentVersion doc = new IB.DocumentVersion();
                                doc.RowId = to.DocumentRowId;
                                doc.FilePath = to.FilePath;
                                doc.URL = to.DocumentUrl;
                                doc.FileName = to.FileName;

                                //note ensure have full record - may want to just do a subset to make sure
                                string msg = new DAL.DocumentStoreManager().UpdateFileInfo( doc );
                            }
                        }

                        if ( isValidResource )
                            entity.ChildItems.Add( to );
                    }
                    catch ( Exception ex )
                    {
                        LoggingHelper.LogError( ex, thisClassName + string.Format( ".FillHierarchyForDownload( Content fromEntity ({0} ). Issue handling: DocEntityId: {1}", entity.Id, fromEntity.ContentId ) );
                    }
                }
            }
   

            if ( entity.IsHierarchyType && includingChildren )
            {
                //get any child nodes
                List<Content> childList = context.Contents
                            .Where( s => s.ParentId == entity.Id && s.TypeId > IB.ContentItem.EXTERNAL_URL_CONTENT_ID )
                            .OrderBy( s => s.SortOrder )
                            .ToList();

                if ( childList != null && childList.Count > 0 )
                {
                    foreach ( Content nodeChild in childList )
                    {
                        IB.ContentItem childNode = Content_ToMap( nodeChild, true, true, true );
                        FillHierarchyForDownload( context, childNode, includingChildren, true );
                        // add?
                        entity.ChildItems.Add( childNode );

                    }
                }
            }

        }//

        private static void FillHierarchyForDownloadOLD( IsleContentContext context, IB.ContentItem entity, bool includingChildren, bool doCompleteFill )
        {

            //now get all content items connected thru the content connector
            //document types (40) or reference url types (41)
            entity.ChildItems = new List<IB.ContentItem>();
            //if ( usingContentConnector == false )
            //{
            List<Content> eflist = context.Contents
                            .Where( s => s.ParentId == entity.Id
                                && ( s.TypeId == IB.ContentItem.DOCUMENT_CONTENT_ID
                                  || s.TypeId == IB.ContentItem.EXTERNAL_URL_CONTENT_ID )
                                 )
                            .OrderBy( s => s.SortOrder ).ThenBy( s => s.Title )
                            .ToList();

            if ( eflist != null && eflist.Count > 0 )
            {
                foreach ( Content efom in eflist )
                {
                    IB.ContentItem child = Content_DownloadToMap( efom, doCompleteFill );

                    entity.ChildItems.Add( child );
                }
            }


            if ( entity.IsHierarchyType && includingChildren )
            {
                //get standards from all children
                List<Content> childList = context.Contents
                            .Where( s => s.ParentId == entity.Id )
                            .OrderBy( s => s.SortOrder )
                            .ToList();

                if ( childList != null && childList.Count > 0 )
                {
                    foreach ( Content nodeChild in childList )
                    {
                        //LoggingHelper.DoTrace( 8, string.Format( "======================== Child Node: {0}, type: {1}", nodeChild.Title, nodeChild.TypeId ) );
                        IB.ContentItem childNode = Content_ToMap( nodeChild, true, true, true );
                        FillHierarchyForDownload( context, childNode, includingChildren, true );
                        // add?
                        entity.ChildItems.Add( childNode );

                    }
                }
            }

        }//
        private static IB.ContentItem Content_DownloadToMap( Content fromEntity, bool doingEagerLoad )
        {
            bool loadingStandards = doingEagerLoad;
            bool loadingDocument = true;
            return Content_ToMap( fromEntity, doingEagerLoad, loadingStandards, loadingDocument );

        }
      
        #endregion

        #region === Content.Partner ===
        public int ContentPartner_Add( IB.ContentPartner entity )
        {
            string statusMessage = "";
            return ContentPartner_Add( entity, ref statusMessage );
        }
        public int ContentPartner_Add( IB.ContentPartner entity, ref string statusMessage )
        {
            Content_Partner efEntity = new Content_Partner();

            try
            {
                using ( var context = new IsleContentContext() )
                {
                    efEntity.ContentId = entity.ContentId;
                    efEntity.PartnerTypeId = entity.PartnerTypeId;
                    efEntity.UserId = entity.UserId;

                    efEntity.CreatedById = entity.CreatedById;
                    efEntity.LastUpdatedById = entity.LastUpdatedById;
                    efEntity.Created = System.DateTime.Now;
                    efEntity.LastUpdated = System.DateTime.Now;
                    ctx.Content_Partner.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        statusMessage = "successful";
                        entity.Id = efEntity.Id;
                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error
                        return 0;
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ContentPartner_Add()" );
                statusMessage = ex.Message;
                return 0;
            }
        }

        /// <summary>
        /// Update ContentPartner
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool ContentPartner_Update( IB.ContentPartner entity )
        {
            bool isValid = false;
            try
            {
                using ( var context = new IsleContentContext() )
                {
                    Content_Partner efEntity = context.Content_Partner.SingleOrDefault( s => s.Id == entity.Id );
                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        //can only update type
                        efEntity.PartnerTypeId = entity.PartnerTypeId;
                        efEntity.LastUpdated = System.DateTime.Now;
                        efEntity.LastUpdatedById = entity.LastUpdatedById;

                        int cnt = context.SaveChanges();

                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ContentPartner_Update()" );
                return false;
            }
            return isValid;
        }
        public bool ContentPartner_Delete( int id, ref string statusMessage )
        {
            bool isSuccessful = false;
            try
            {
                statusMessage = "";
                using ( var context = new IsleContentContext() )
                {
                    Content_Partner item = context.Content_Partner.SingleOrDefault( s => s.Id == id );

                    if ( item != null && item.Id > 0 )
                    {
                        context.Content_Partner.Remove( item );
                        context.SaveChanges();
                        isSuccessful = true;
                    }
                    else
                    {
                        statusMessage = "Error - the requested record was not found.";
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ContentPartner_Delete()" );
                isSuccessful = false;
                statusMessage = ex.Message;
            }
            return isSuccessful;
        }

        public static List<IB.ContentPartner> Content_GetContentPartner( int contentId )
        {
            IB.ContentPartner entity = new IB.ContentPartner();
            List<IB.ContentPartner> list = new List<IB.ContentPartner>();

            //need to handle where a node under a curriculum
            //==> actually the caller should be responsible for this. Should not assume here??
            using ( var context = new IsleContentContext() )
            {
                List<Content_Partner> mbrs = context.Content_Partner
                                    .Where( s => s.ContentId == contentId && s.PartnerTypeId > 0 )
                                    .ToList();

                if ( mbrs.Count > 0 )
                {
                    foreach ( Content_Partner item in mbrs )
                    {
                        entity = new IB.ContentPartner();
                        entity.Id = item.Id;
                        entity.UserId = item.UserId;
                        entity.ContentId = item.ContentId;
                        entity.CreatedById = (int)item.CreatedById;
                        entity.Created = item.Created;

                        list.Add( entity );

                    }
                }

            }
            return list;
        }
        public static IB.ContentPartner Content_GetContentPartner( int contentId, int userId )
        {
            IB.ContentPartner entity = new IB.ContentPartner();

            //need to handle where a node under a curriculum
            //==> actually the caller should be responsible for this. Should not assume here??
            using ( var context = new IsleContentContext() )
            {
                Content_Partner mbr = context.Content_Partner
                              .SingleOrDefault( s => s.ContentId == contentId && s.UserId == userId );

                if ( mbr != null && mbr.Id > 0 )
                {
                    entity = new IB.ContentPartner();
                    entity.Id = mbr.Id;
                    entity.UserId = mbr.UserId;
                    entity.ContentId = mbr.ContentId;
                    entity.PartnerTypeId = mbr.PartnerTypeId;
                    entity.CreatedById = ( int )mbr.CreatedById;
                    entity.Created = mbr.Created;
                }
                else
                {
                    //check if author. Should only relate to queries, not updates
                    Content item = context.Contents
                            .SingleOrDefault( s => s.Id == contentId );
                    if ( item != null && item.Id > 0 )
                    {
                        if (item.CreatedById == userId || item.LastUpdatedById == userId)
                        {
                            entity = new IB.ContentPartner();
                            entity.Id = 0;
                            entity.UserId = userId;
                            entity.ContentId = contentId;
                            entity.PartnerTypeId = 4;
                            entity.CreatedById = userId;
                            entity.Created = DateTime.Now;
                        }
                    }
                }
            }
            return entity;
        }

        public static List<LearningListNode> SelectUserLearningLists( int userId )
        {
            return SelectLearningLists( userId, 1 );
        }

        public static List<LearningListNode> SelectUserEditableLearningLists(int userId)
        {
            return SelectLearningLists( userId, 2 );
        }

        private static List<LearningListNode> SelectLearningLists( int userId, int minPrivilege )
        {
            List<LearningListNode> list = new List<LearningListNode>();
            LearningListNode entity = new LearningListNode();
            using ( var context = new IsleContentContext() )
            {
                List<Content_LearningListSummary> mbrs = context.Content_LearningListSummary
                                    .Where( s => s.PartnerUserId == userId && s.PartnerTypeId >= minPrivilege )
                                    .OrderBy( s => s.Title )
                                    .ToList();

                if ( mbrs.Count > 0 )
                {
                    foreach ( Content_LearningListSummary item in mbrs )
                    {
                        entity = new LearningListNode();
                        entity.Id = item.Id;
                        entity.Title = item.Title;
                        entity.Description = item.Summary;
                        entity.ImageUrl = item.ImageUrl;
                        //entity.IsPublished = item.IsPublished == null ? false : ( bool ) item.IsPublished;
                        if ( item.StatusId == IB.ContentItem.PUBLISHED_STATUS )
                            entity.IsPublished = true;
                        else
                            entity.IsPublished = false;

                        entity.PartnerTypeId = item.PartnerTypeId;
                        entity.PartnerType = item.PartnerType;
                        entity.IsUserAuthor = item.PartnerType == "Author";
                        entity.CreatedById = ( int ) item.CreatedById;
                        entity.Created = ( DateTime ) item.Created;
                        if ( entity.PartnerTypeId > 1 )
                            entity.CanUserEdit = true;
                        else
                            entity.CanUserEdit = false;

                        list.Add( entity );

                    }
                }

            }

            return list;
        }

        public static List<ObjectMember> Learninglist_SelectUsers( int contentId )
        {
            List<ObjectMember> list = new List<ObjectMember>();
            ObjectMember entity = new ObjectMember();
            using ( var context = new IsleContentContext() )
            {
                List<LearningList_MembersSummary> mbrs = context.LearningList_MembersSummary
                                    .Where( s => s.ContentId == contentId )
                                    .OrderBy( s => s.LastName ).ThenBy( s => s.FirstName )
                                    .ToList();

                if ( mbrs.Count > 0 )
                {
                    foreach ( LearningList_MembersSummary item in mbrs )
                    {
                        entity = new ObjectMember();
                        entity.ObjectId = item.ContentId;
                        entity.UserId = item.UserId;
                        entity.MemberTypeId = item.PartnerTypeId;
                        entity.MemberType = item.PartnerType;

                        entity.FirstName = item.FirstName;
                        entity.LastName = item.LastName;
                        entity.Email = item.Email;
                        entity.Organization = item.Organization;
                        entity.OrgId = item.OrgId == null ? 0 : ( int ) item.OrgId;

                        entity.MemberImageUrl = item.ImageUrl;
                        entity.MemberHomeUrl = string.Format( "/Profile/{0}/{1}", entity.UserId, entity.MemberFullName.Replace(" ", "_") );

                        entity.Created = ( DateTime ) item.PartnerCreated;
                        entity.LastUpdated = ( DateTime ) item.PartnerLastUpdated;

                        list.Add( entity );

                    }
                }

            }

            return list;
        }

        #endregion
        #region content for download
        public static IB.ContentItem Content_DownloadNode( int contentId, bool activeOnly )
        {
            IB.ContentItem entity = new IB.ContentItem();
            //may want to control by status, but need to make it clear to author, and ensure it is not reset arbitrarily
            string allowedList = UtilityManager.GetAppKeyValue( "allowedContentItemsStatusForDownload", " 5 " );

            using ( var context = new IsleContentContext() )
            {

                Content item = context.Contents
                            .SingleOrDefault( s => s.Id == contentId 
                                        && s.IsActive == true
                                        && s.StatusId == IB.ContentItem.PUBLISHED_STATUS );

                if ( item != null && item.Id > 0 )
                {
                    entity.Id = item.Id;
                    entity.Title = item.Title;
                    entity.Description = item.Description;

                    //get any document children

                    //get next level
                    entity.ChildItems = new List<IB.ContentItem>();

                    List<Content> eflist = context.Contents
                                    .Where( s => s.ParentId == contentId )
                                    .OrderBy( s => s.SortOrder )
                                    .ToList();

                    if ( eflist != null && eflist.Count > 0 )
                    {
                        foreach ( Content efom in eflist )
                        {
                            IB.ContentItem child = Content_ToMap( efom, false, false, false );
                            //get this items children
                            child.ChildItems = Content_FillOutlineChildrenFAT( context, child, false, true );

                            entity.ChildItems.Add( child );
                        }
                    }
                   
                }
            }


            return entity;
        }


        #endregion

        #region content standard
        public static int ContentStandard_Add( IB.ContentStandard standard )
        {
            return ContentStandard_Add( standard.ContentId, standard.StandardId, standard.AlignmentTypeCodeId, standard.UsageTypeId, standard .CreatedById);
        }
        public static int ContentStandard_Add( int contentId, int standardId, int alignmentTypeCodeId, int usageTypeId, int createdById )
        {
            Content_Standard efEntity = new Content_Standard();
            int newId = 0;
            try
            {
                efEntity.ContentId = contentId;
                efEntity.StandardId = standardId;
                efEntity.AlignmentTypeCodeId = alignmentTypeCodeId;
                efEntity.UsageTypeId = usageTypeId;
                efEntity.CreatedById = createdById;
                efEntity.Created = System.DateTime.Now;
                efEntity.LastUpdatedById = createdById;
                efEntity.LastUpdated = System.DateTime.Now;

                ctx.Content_Standard.Add( efEntity );

                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    newId = efEntity.Id;
                }
                else
                {
                    //?no info on error
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ContentStandard_Add()" );
            }

            return newId;
        }
        public static bool ContentStandard_Update( int id, int alignmentTypeCodeId, int usageTypeId, int lastUpdatedById, ref string  statusMessage )
        {
            bool isValid = false;
            try
            {
                using ( var context = new IsleContentContext() )
                {
                    Content_Standard efEntity = context.Content_Standard.SingleOrDefault( s => s.Id == id );
                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        efEntity.AlignmentTypeCodeId = alignmentTypeCodeId;
                        efEntity.UsageTypeId = usageTypeId;
                        efEntity.LastUpdatedById = lastUpdatedById;
                        efEntity.LastUpdated = System.DateTime.Now;

                        int cnt = context.SaveChanges();
                        if ( cnt > 0 )
                            isValid = true;
                    }
                }

            }
            catch ( Exception ex )
            {
                statusMessage = "Error: " + ex.Message;
                LoggingHelper.LogError( ex, thisClassName + ".ContentStandard_Add()" );
            }

            return isValid;
        }
        public static bool ContentStandard_Delete( int id, ref string statusMessage )
        {
            bool isSuccessful = false;
            try
            {
                statusMessage = "";
                using ( var context = new IsleContentContext() )
                {
                    Content_Standard item = context.Content_Standard.SingleOrDefault( s => s.Id == id );

                    if ( item != null && item.Id > 0 )
                    {
                        context.Content_Standard.Remove( item );
                        context.SaveChanges();
                        isSuccessful = true;
                    }
                }
            }
            catch ( Exception ex )
            {
                isSuccessful = false;
                statusMessage = ex.Message;
            }
            return isSuccessful;
        }

        public static List<IB.Content_StandardSummary> Fill_ContentStandards( int contentId )
        {
            List<IB.Content_StandardSummary> list = new List<IB.Content_StandardSummary>();
            IB.Content_StandardSummary item = new IB.Content_StandardSummary();
            using ( var context = new IsleContentContext() )
            {
                List<ContentStandard_Summary> eflist = context.ContentStandard_Summary
                        .Where( s => s.ContentId == contentId )
                            .OrderBy( s => s.UsageTypeId ).ThenBy(s => s.NotationCode)
                            .ToList();

                if ( eflist != null && eflist.Count > 0 )
                {
                    foreach ( ContentStandard_Summary efom in eflist )
                    {
                        item = new IB.Content_StandardSummary();
                        item.ContentId = efom.ContentId;
                        item.Id = efom.ContentStandardId;
                        item.StandardId = efom.StandardId;
                        item.StandardUrl = efom.StandardUrl;
                        item.NotationCode = efom.NotationCode;
                        item.Description = efom.Description;

                        if ( efom.Created != null )
                            item.Created = ( System.DateTime ) efom.Created;
                        item.CreatedById = efom.CreatedById;

                        if ( efom.LastUpdated != null )
                            item.LastUpdated = ( System.DateTime ) efom.LastUpdated;
                        item.LastUpdatedById = efom.LastUpdatedById;

                        item.AlignmentType = efom.AlignmentType;
                        item.AlignmentTypeCodeId = efom.AlignmentTypeCodeId;
                        item.UsageTypeId = efom.UsageTypeId;
                        item.StandardUsage = efom.StandardUsage;

                        list.Add( item );
                    }
                }
            }

            return list;
        }

        #endregion

        #region content tag
        //public static int ContentTag_Add( IB.ContentTag entity )
        //{
        //    return ContentTag_Add( entity.ContentId, entity.TagValueId, entity.CreatedById );
        //}
        public static int ContentTag_Add( int contentId, int tagValueId, int createdById )
        {
            Content_Tag efEntity = new Content_Tag();
            int newId = 0;
            try
            {
                efEntity.ContentId = contentId;
                efEntity.TagValueId = tagValueId;
                efEntity.CreatedById = createdById;
                efEntity.Created = System.DateTime.Now;

                ctx.Content_Tag.Add( efEntity );

                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    newId = efEntity.Id;
                }
                else
                {
                    //?no info on error
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ContentTag_Add()" );
            }

            return newId;
        }
      
        public static bool ContentTag_Delete( int id, ref string statusMessage )
        {
            bool isSuccessful = false;
            try
            {
                statusMessage = "";
                using ( var context = new IsleContentContext() )
                {
                    Content_Tag item = context.Content_Tag.SingleOrDefault( s => s.Id == id );

                    if ( item != null && item.Id > 0 )
                    {
                        context.Content_Tag.Remove( item );
                        context.SaveChanges();
                        isSuccessful = true;
                    }
                }
            }
            catch ( Exception ex )
            {
                isSuccessful = false;
                statusMessage = ex.Message;
            }
            return isSuccessful;
        }

        public static bool ContentTag_Delete( int contentId, int tagValueId, ref string statusMessage )
        {
            bool isSuccessful = false;
            try
            {
                statusMessage = "";
                using ( var context = new IsleContentContext() )
                {
                    Content_Tag item = context.Content_Tag.SingleOrDefault( s => s.ContentId == contentId && s.TagValueId == tagValueId );

                    if ( item != null && item.Id > 0 )
                    {
                        context.Content_Tag.Remove( item );
                        context.SaveChanges();
                        isSuccessful = true;
                    }
                }
            }
            catch ( Exception ex )
            {
                isSuccessful = false;
                statusMessage = ex.Message;
            }
            return isSuccessful;
        }
        public static List<dtoc.Content_TagSummary> Fill_ContentTags( int contentId )
        {
            List<dtoc.Content_TagSummary> list = new List<dtoc.Content_TagSummary>();
            dtoc.Content_TagSummary item = new dtoc.Content_TagSummary();

            using ( var context = new IsleContentContext() )
            {
                List<Content_TagSummary> eflist = context.Content_TagSummary
                        .Where( s => s.ContentId == contentId )
                            .OrderBy( s => s.CategoryTitle ).ThenBy( s => s.TagTitle )
                            .ToList();

                if ( eflist != null && eflist.Count > 0 )
                {
                    foreach ( Content_TagSummary efom in eflist )
                    {
                        item = new dtoc.Content_TagSummary();
                        item.ContentId = efom.ContentId;
                        item.ContentTagId = efom.ContentTagId;
                        item.TagValueId = efom.TagValueId;
                        item.CodeId = efom.CodeId;
                        item.CategoryId = efom.CategoryId;
                        item.CategoryTitle = efom.CategoryTitle;

                        if ( efom.Created != null )
                            item.Created = ( System.DateTime )efom.Created;
                        if ( efom.CreatedById != null )
                        item.CreatedById = (int)efom.CreatedById;

                        list.Add( item );
                    }
                }
            }

            return list;
        }

        #endregion
        #region content subscriptions
        public int ContentSubscription_Add( int contentId, int userId, int typeId, ref string statusMessage )
        {
            Content_Subscription efEntity = new Content_Subscription();
            int newId = 0;
            statusMessage = "";
            try
            {
                efEntity.ContentId = contentId;
                efEntity.UserId = userId;
                efEntity.SubscriptionTypeId = typeId;
                
                efEntity.Created = System.DateTime.Now;
                efEntity.LastUpdated = System.DateTime.Now;

                ctx.Content_Subscription.Add( efEntity );

                // submit the change to database
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    newId = efEntity.Id;
                }
                else
                {
                    //?no info on error
                }
            }
            catch ( Exception ex )
            {
                statusMessage = ex.Message;
                LoggingHelper.LogError( ex, thisClassName + ".ContentSubscription_Add()" );
            }

            return newId;
        }
        public bool ContentSubscription_Update( int id, int typeId, ref string statusMessage )
        {
            bool isValid = false;
            statusMessage = "";
            using ( var context = new IsleContentContext() )
            {
                Content_Subscription efEntity = context.Content_Subscription.SingleOrDefault( s => s.Id == id );
                try
                {
                    efEntity.SubscriptionTypeId = typeId;
                    efEntity.LastUpdated = System.DateTime.Now;

                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        isValid = true;
                    }
                    else
                    {
                        //?no info on error
                        //could be that type did not change?
                    }
                }
                catch ( Exception ex )
                {
                    statusMessage = ex.Message;
                    LoggingHelper.LogError( ex, thisClassName + ".ContentSubscription_Update()" );
                }
            }
            return isValid;
        }
        public bool ContentSubscription_Delete( int id, ref string statusMessage )
        {
            bool isValid = false;
            statusMessage = "";
            using ( var context = new IsleContentContext() )
            {
                Content_Subscription efEntity = context.Content_Subscription.SingleOrDefault( s => s.Id == id );
                try
                {
                    context.Content_Subscription.Remove( efEntity );
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        isValid = true;
                    }
                    else
                    {
                        //?no info on error
                        //could be that type did not change?
                    }
                }
                catch ( Exception ex )
                {
                    statusMessage = ex.Message;
                    LoggingHelper.LogError( ex, thisClassName + ".ContentSubscription_Delete()" );
                }
            }
            return isValid;
        }

        public IB.ObjectSubscription ContentSubscription_Get( int contentId, int userId )
        {
            IB.ObjectSubscription entity = new IB.ObjectSubscription();
            using ( var context = new IsleContentContext() )
            {
                Content_Subscription efEntity = context.Content_Subscription.SingleOrDefault( s => s.ContentId == contentId && s.UserId == userId );
                if ( efEntity != null && efEntity.Id > 0 )
                {
                    entity.Id = efEntity.Id;
                    entity.ParentId = efEntity.ContentId;
                    entity.SubscriptionTypeId = (int)efEntity.SubscriptionTypeId;
                    entity.UserId = efEntity.UserId;
                    entity.Created = (DateTime) efEntity.Created;
                    entity.LastUpdated = ( DateTime ) efEntity.LastUpdated;
                }
            }

            return entity;
        }
        #endregion

        #region content history
        public bool ContentHistory_Update( int id, string message, ref string statusMessage )
        {
            bool isValid = false;
            statusMessage = "";
            using ( var context = new IsleContentContext() )
            {
                Content_History efEntity = context.Content_History.SingleOrDefault( s => s.Id == id );
                try
                {
                    efEntity.Description = message;
                    //efEntity.LastUpdated = System.DateTime.Now;

                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        isValid = true;
                    }
                    else
                    {
                        //?no info on error
                        //could be that entity did not change?
                    }
                }
                catch ( Exception ex )
                {
                    statusMessage = ex.Message;
                    LoggingHelper.LogError( ex, thisClassName + ".ContentHistory_Update()" );
                }
            }
            return isValid;
        }
        public List<CommentDTO> ContentHistory_Select( int contentId, string actionType )
        {
            List<CommentDTO> list = new List<CommentDTO>();
            CommentDTO entity = new CommentDTO();
            
            try
            {
                using ( var context = new IsleContentContext() )
                {
                    List<Content_History> efList = context.Content_History
                        .Where( s => s.ContentId == contentId && s.Action == actionType )
                                    .OrderByDescending( o => o.Created)
                                    .ToList();

                    if ( efList != null && efList.Count > 0 )
                    {
                        foreach ( Content_History item in efList )
                        {
                            entity = new CommentDTO();
                            entity.Text = item.Description;
                            entity.Date = item.Created.ToShortDateString();

                            list.Add( entity );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ContentHistory_Select()" );
            }

            return list;
        }
  
        #endregion


        #region content connector
        public int ContentConnectorAdd( int parentId, int childId, int createdById )
        {
            int newId = 0;
            //Content_Connector efEntity = new Content_Connector();

            //try
            //{
            //    efEntity.ParentId = parentId;
            //    efEntity.ChildId = childId;
            //    //TODO -could do a max of current, and add 5?
            //    efEntity.SortOrder = DefaultSortOrder;
            //    efEntity.CreatedById = createdById;
            //    efEntity.Created = System.DateTime.Now;

            //    ctx.Content_Connector.Add( efEntity );

            //    // submit the change to database
            //    int count = ctx.SaveChanges();
            //    if ( count > 0 )
            //    {
            //        newId = efEntity.Id;
            //    }
            //    else
            //    {
            //        //?no info on error
            //    }
            //}
            //catch ( Exception ex )
            //{
            //    LoggingHelper.LogError( ex, thisClassName + ".ContentConnectorAdd()" );
            //}

            return newId;
        }
        public bool ContentConnector_Delete( int id, ref string statusMessage )
        {
            bool isSuccessful = false;
            try
            {
                //Content_Connector item = ctx.Content_Connector.SingleOrDefault( s => s.Id == id );

                //if ( item != null && item.ParentId > 0 )
                //{
                //    ctx.Content_Connector.Remove( item );
                //    ctx.SaveChanges();
                //    isSuccessful = true;

                //    //now delete child content
                //    Content_Delete( item.ChildId, ref statusMessage );
                //}
            }
            catch ( Exception ex )
            {
                isSuccessful = false;
                statusMessage = ex.Message;
            }
            return isSuccessful;
        }
        public bool ContentConnector_Delete( int parentId, int childId, ref string statusMessage )
        {
            bool isSuccessful = false;
            try
            {
                //Content_Connector item = ctx.Content_Connector.SingleOrDefault( s => s.ParentId == parentId && s.ChildId == childId );

                //if ( item != null && item.ParentId > 0 )
                //{
                //    ctx.Content_Connector.Remove( item );
                //    ctx.SaveChanges();
                //    isSuccessful = true;

                //    //now delete child content
                //    Content_Delete( childId, ref statusMessage );
                //}
            }
            catch ( Exception ex )
            {
                isSuccessful = false;
                statusMessage = ex.Message;
            }
            return isSuccessful;
        }
        ///// <summary>
        ///// delete a connector where the child CI is being deleted
        ///// </summary>
        ///// <param name="parentId"></param>
        ///// <param name="childId"></param>
        ///// <returns></returns>
        public static bool ContentConnector_DeleteChild( int childId, ref string statusMessage )
        {
            bool isSuccessful = true;
            try
            {
                using ( var context = new IsleContentContext() )
                {
                    //List<Content_Connector> eflist = context.Content_Connector
                    //                .Where( s => s.ChildId == childId )
                    //                .ToList();

                    //if ( eflist != null && eflist.Count > 0 )
                    //{
                    //    foreach ( Content_Connector efom in eflist )
                    //    {
                    //        context.Content_Connector.Remove( efom );
                    //        context.SaveChanges(); ;
                    //    }
                    //}
                }
            }
            catch ( Exception ex )
            {
                isSuccessful = false;
                statusMessage = ex.Message;
            }
            return isSuccessful;
        }

        private static int ContentConnector_GetParent( IB.ContentItem entity )
        {

            int parentId = 0;
            using ( var context = new IsleContentContext() )
            {
                //Content_Connector parent = context.Content_Connector
                //        .SingleOrDefault( s => s.ChildId == entity.Id );

                //if ( parent != null && parent.Id > 0 )
                //{
                //    parentId = parent.ParentId;
                //}
            }

            return parentId;
        }


        #endregion

        #region     ContentFile
        //public static int ContentFile_Create( ILPContentFile fromEntity, ref string statusMessage )
        //{

        //    ContentFile entity = entity = ILPContentFile_FromMap( fromEntity );
        //    entity.IsActive = true;
        //    entity.Created = System.DateTime.Now;
        //    entity.LastUpdated = System.DateTime.Now;

        //    ctx.ContentFiles.Add( entity );

        //    // submit the change to database
        //    int count = ctx.SaveChanges();
        //    if ( count > 0 )
        //    {
        //        statusMessage = "Successful";
        //        return entity.Id;
        //    }
        //    else
        //    {
        //        statusMessage = "Error - ContentFile_Create failed";
        //        //?no info on error
        //        return 0;
        //    }
        //}

        //public static bool ContentFile_UpdateVersionId( int contentFileId, int versionId )
        //{

        //    bool action = false;

        //    ContentFile entity = ctx.ContentFiles.SingleOrDefault( s => s.Id == contentFileId );
        //    if ( entity != null )
        //    {
        //        entity.ResourceVersionId = contentFileId;
        //        entity.LastUpdated = System.DateTime.Now;
        //        ctx.SaveChanges();

        //        action = true;
        //    }

        //    return action;
        //}

        //public static ContentFile ILPContentFile_FromMap( ILPContentFile fromEntity )
        //{
        //    Mapper.CreateMap<ILPContentFile, ContentFile>();

        //    ContentFile toEntity = Mapper.Map<ILPContentFile, ContentFile>( fromEntity );

        //    return toEntity;
        //}

        //public static ILPContentFile ContentFile_ToMap( ContentFile fromEntity )
        //{

        //    Mapper.CreateMap<ContentFile, ILPContentFile>();
        //    ILPContentFile toEntity = Mapper.Map<ContentFile, ILPContentFile>( fromEntity );

        //    return toEntity;
        //}
        #endregion

        #region Helpers =======================
        /// <summary>
        /// Get a Gateway_OrgSummary
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IB.DocumentVersion Document_Version_Get( Guid id )
        {
            Document_Version efEntity = new Document_Version();
            IB.DocumentVersion entity = new IB.DocumentVersion();
            using ( var context = new IsleContentContext() )
            {
                efEntity = context.Document_Version.SingleOrDefault( s => s.RowId == id );
                if ( efEntity != null && entity.IsValidRowId( efEntity.RowId ) )
                {
                    entity.RowId = efEntity.RowId;
                    entity.Title = efEntity.Title != null ? efEntity.Title : "";
                    entity.Summary = efEntity.Summary != null ? efEntity.Summary : "";
                    entity.Status = efEntity.Status != null ? efEntity.Status : "";
                    entity.FileName = efEntity.FileName != null ? efEntity.FileName : "";
                    entity.FilePath = efEntity.FilePath != null ? efEntity.FilePath : "";
                    entity.MimeType = efEntity.MimeType != null ? efEntity.MimeType : "";

                    entity.FileDate = efEntity.FileDate != null ? ( System.DateTime ) efEntity.FileDate : entity.DefaultDate;

                    if ( efEntity.Bytes != null )
                    {
                        entity.ResourceBytes = ( long ) efEntity.Bytes;
                        if ( entity.ResourceBytes > 0 )
                        {
                            entity.SetResourceData( entity.ResourceBytes, efEntity.Data );
                        }
                    }
                    entity.ResourceUrl = efEntity.Url != null ? efEntity.Url : "";

                    entity.Created = efEntity.Created != null ? ( System.DateTime ) efEntity.Created : entity.DefaultDate;
                    entity.CreatedById = efEntity.CreatedById;
                    entity.LastUpdated = efEntity.LastUpdated != null ? ( System.DateTime ) efEntity.LastUpdated : entity.DefaultDate;
                    entity.LastUpdatedById = efEntity.LastUpdatedById;
                }
            }
            return entity;
        }

        public bool Document_Version_Delete( Guid? id, ref string statusMessage )
        {
            bool isSuccessful = false;
            try
            {
                statusMessage = "";
                using ( var context = new IsleContentContext() )
                {
                    Document_Version efEntity = context.Document_Version.SingleOrDefault( s => s.RowId == id );

                    if ( efEntity != null && efEntity.FileName.Length > 0 )
                    {
                        context.Document_Version.Remove( efEntity );
                        context.SaveChanges();
                        isSuccessful = true;
                    }
                }
            }
            catch ( Exception ex )
            {
                isSuccessful = false;
                statusMessage = ex.Message;
            }
            return isSuccessful;
        }
        /// <summary>
        /// Get a Gateway_OrgSummary
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Gateway_OrgSummary Gateway_OrgSummary_Get( int id )
        {
            Gateway_OrgSummary entity = new Gateway_OrgSummary();
            using ( var context = new IsleContentContext() )
            {
                entity = context.Gateway_OrgSummary.SingleOrDefault( s => s.id == id );
            }
            return entity;
        }


        /// <summary>
        /// Get a ConditionOfUse
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static LR_ConditionOfUse_Select ConditionOfUse_Get( int id )
        {
            LR_ConditionOfUse_Select entity = new LR_ConditionOfUse_Select();
            using ( var context = new IsleContentContext() )
            {
                entity = context.LR_ConditionOfUse_Select.SingleOrDefault( s => s.Id == id );
            }
            return entity;
        }
        public static string ContentType_Get( int id )
        {
            string contentType = "Content";
            ContentType entity = new ContentType();
            using ( var context = new IsleContentContext() )
            {
                entity = context.ContentTypes.SingleOrDefault( s => s.Id == id );
                if ( entity != null && entity.Id > 0 )
                    contentType = entity.Title;
            }
            return contentType;
        }

        public static List<CodeItem> ContentType_GetActive()
        {
            List<CodeItem> list = new List<CodeItem>();
            CodeItem ci = new CodeItem();

            ContentType entity = new ContentType();
            using ( var context = new IsleContentContext() )
            {
                List<ContentType> eflist = context.ContentTypes
                                    .Where( s => s.IsActive == true ).ToList();
                if ( eflist.Count > 0 )
                {
                    foreach ( ContentType item in eflist )
                    {
                        ci = new CodeItem();
                        ci.Id = item.Id;
                        ci.Title = item.Title;
                        list.Add( ci );
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// only return content types that can be top level items (module is pending)
        /// </summary>
        /// <returns></returns>
        public static List<CodeItem> ContentType_GetAllowedTopLevel()
        {
            List<CodeItem> list = new List<CodeItem>();
            CodeItem ci = new CodeItem();
            string allowedList = UtilityManager.GetAppKeyValue( "allowedNewContentItems", " 10 50 " );
            ContentType entity = new ContentType();
            using ( var context = new IsleContentContext() )
            {
                List<ContentType> eflist = context.ContentTypes
                                    .Where( s => s.IsActive == true )
                                    .ToList();
                if ( eflist.Count > 0 )
                {
                    foreach ( ContentType item in eflist )
                    {
                        if ( allowedList.IndexOf( item.Id.ToString() ) > -1 )
                        {
                            ci = new CodeItem();
                            ci.Id = item.Id;
                            ci.Title = item.Title;
                            list.Add( ci );
                        }
                    }
                }
            }
            return list;
        }
        #endregion
    }

    public class CachedContent
    {
        public CachedContent()
        {
            lastUpdated = DateTime.Now;
        }
        public DateTime lastUpdated { get; set; }
        public IB.ContentItem Item { get; set; }
        public ContentNode nodeOutline { get; set; }

    }
}
