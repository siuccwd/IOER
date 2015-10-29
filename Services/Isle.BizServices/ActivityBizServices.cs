using System;
using System.Collections.Generic;
using System.Web;

using ILPathways.Business;
using ILPathways.DAL;
using ILPathways.Utilities;
using Isle.DTO;
using EFDAL = IoerContentBusinessEntities;
using IPB = ILPathways.Business;
using LRW = LRWarehouse.Business;
using ThisUser = LRWarehouse.Business.Patron;

namespace Isle.BizServices
{
    public class ActivityBizServices
    {
        private static string thisClassName = "ActivityBizServices";
        // EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
       

        #region activity reporting
        
        public List<HierarchyActivityRecord> ActivityTotals_Library( int libraryId, DateTime startDate, DateTime endDate )
        {
            return  ActivityTotals_Library( libraryId, startDate, endDate, false );
        }

        public List<HierarchyActivityRecord> ActivityTotals_Library( int libraryId, DateTime startDate, DateTime endDate, bool includePersonLibraries )
        {
            List<HierarchyActivityRecord> list = new List<HierarchyActivityRecord>();

            list = ActivityAuditManager.ActivityTotals_Library( libraryId, startDate, endDate, includePersonLibraries );

            return list;
        }
        [Obsolete]
        private List<LibraryActivitySummary> ActivityLibraryTotals( int libraryId, DateTime startDate, DateTime endDate )
        {
            List<LibraryActivitySummary> list = new List<LibraryActivitySummary>();
            list = ActivityAuditManager.ActivityLibraryTotals( libraryId, startDate, endDate );

            return list;
        }


        public List<HierarchyActivityRecord> ActivityTotals_LearningLists( int objectId, DateTime startDate, DateTime endDate )
        {
            List<HierarchyActivityRecord> list = new List<HierarchyActivityRecord>();
            bool removeEmptyNodes = true;
            list = ActivityAuditManager.ActivityTotals_LearningLists( objectId, startDate, endDate, removeEmptyNodes );

            return list;
        }


        public List<HierarchyActivityRecord> ActivityTotals_Accounts( DateTime startDate, DateTime endDate )
        {
            List<HierarchyActivityRecord> list = new List<HierarchyActivityRecord>();

            list = ActivityAuditManager.ActivityTotals_Accounts( startDate, endDate );

            return list;
        }

        #endregion


        /// <summary>
        /// General purpose create of a site activity
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="activityEvent"></param>
        /// <param name="comment">A formatted user friendly description of the activity</param>
        /// <param name="actionByUserId">Optional userId of person initiating the activity</param>
        /// <param name="activityObjectId"></param>
        public void AddActivity( string activity, string activityEvent, string comment, int actionByUserId, int activityObjectId )
        {
            //EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = activity;
            log.Event = activityEvent;
            log.Comment = comment;
            log.SessionId = GetCurrentSessionId();

            if ( actionByUserId > 0 )
                log.ActionByUserId = actionByUserId;
            if ( activityObjectId > 0 )
                log.ActivityObjectId = activityObjectId;

            
            try
            {
                SiteActivityAdd( log );
                //ctx.ActivityLogs.Add( log );
                //int count = ctx.SaveChanges();
                //if ( count > 0 )
                //{
                //    return; // log.Id;
                //}
                //else
                //{
                //    //?no info on error
                //    return;
                //}
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".AddActivity()" );
                return;
            }
        }

        /// <summary>
        /// General purpose create of a site activity
        /// </summary>
        /// <param name="activityType"></param>
        /// <param name="activity"></param>
        /// <param name="activityEvent"></param>
        /// <param name="comment">A formatted user friendly description of the activity</param>
        /// <param name="actionByUserId"></param>
        /// <param name="targetUserId"></param>
        /// <param name="activityObjectId"></param>
        /// <param name="relatedId"></param>
        public void AddActivity( string activityType, string activity, string activityEvent, string comment, int actionByUserId, int targetUserId, int activityObjectId, int relatedId )
        {
            //EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = activityType;
            log.Activity = activity;
            log.Event = activityEvent;
            
            log.Comment = comment;
            log.SessionId = GetCurrentSessionId();

            if (actionByUserId > 0)
                log.ActionByUserId = actionByUserId;
            if (targetUserId > 0)
                log.TargetUserId = targetUserId;
            if ( relatedId > 0 )
                log.ObjectRelatedId = relatedId;
            if (activityObjectId > 0)
                log.ActivityObjectId = activityObjectId;

            try
            {
                SiteActivityAdd( log );
                //ctx.ActivityLogs.Add( log );
                //int count = ctx.SaveChanges();
                //if ( count > 0 )
                //{
                //    return; // log.Id;
                //}
                //else
                //{
                //    //?no info on error
                //    return;
                //}
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".AddActivity()" );
                return;
            }
        }

        #region social network activity
        /// <summary>
        /// Return collection of site ObjectActivies for requested days in the past for guest user.
        /// No more than the value for pMaximumRows will be returned
        /// </summary>
        /// <param name="forDays"></param>
        /// <param name="pMaximumRows"></param>
        /// <returns></returns>
        public static List<ObjectActivity> ObjectActivity_RecentList( int forDays, int pMaximumRows )
        {
            return ActivityAuditManager.ObjectActivity_RecentList( forDays, 0, pMaximumRows );
        }
        /// <summary>
        /// Return collection of site ObjectActivies for requested days in the past, where have authenticated user.
        /// No more than the value for pMaximumRows will be returned
        /// </summary>
        /// <param name="forDays"></param>
        /// <param name="userId"></param>
        /// <param name="pMaximumRows"></param>
        /// <returns></returns>
        public static List<ObjectActivity> ObjectActivity_RecentList( int forDays, int userId, int pMaximumRows )
        {
            return ActivityAuditManager.ObjectActivity_RecentList( forDays, userId, pMaximumRows );
        }

        /// <summary>
        /// Return collection of ObjectActivies related to libraries and collections that the user is following, for requested days in the past. 
        /// No more than the value for pMaximumRows will be returned
        /// </summary>
        /// <param name="forDays"></param>
        /// <param name="userId"></param>
        /// <param name="pMaximumRows"></param>
        /// <returns></returns>
        public static List<ObjectActivity> ObjectActivity_MyFollowingSummary( int forDays, int userId, int pMaximumRows )
        {

            return ActivityAuditManager.ObjectActivity_MyFollowingSummary( forDays, userId, pMaximumRows );
        }

        public static List<ObjectActivity> ObjectActivity_OrgSummary( int forDays, int orgId, int userId, int pMaximumRows )
        {

            return ActivityAuditManager.ObjectActivity_OrgSummary( forDays, orgId, userId, pMaximumRows );
        }
        #endregion

        #region Site Activity - publishing
        public void PublishActivity( LRW.Resource resource, IPB.IWebUser user )
        {
            PublishActivity( resource, user, "Publish" );
        }

		/// <summary>
		/// Iniated auto publish for components of a hierarchy
		/// </summary>
		/// <param name="resource"></param>
		/// <param name="user"></param>
		/// <param name="resourceList">List of auto published resources</param>
		public void AutoPublishActivity( LRW.Resource resource, IPB.IWebUser user, string resourceList )
		{
			if ( resource == null || resource.Id == 0 )
				return;

			try
			{
				string title = ResourceBizService.FormatFriendlyTitle( resource.Version.Title );

				EFDAL.ActivityLog log = new EFDAL.ActivityLog();
				log.CreatedDate = System.DateTime.Now;
				log.ActivityType = "Audit";
				log.Activity = "Resource";
				log.Event = "Auto-publish Hierarchy";

				log.RelatedTargetUrl = string.Format( "/Resource/{0}/{1}", resource.Id, ResourceBizService.FormatFriendlyTitle( resource.Version.Title ) );

				log.Comment = string.Format( "{0} published resource: {1} (Id:{2}) which resulted in an auto-publish: {3}", user.FullName(), title, resource.Id, resourceList );
				//actor type - person
				log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id;
				log.TargetUserId = 0;

				log.ActivityObjectId = resource.Id;

				log.SessionId = GetCurrentSessionId();
				SiteActivityAdd( log );

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".PublishActivity()" );
				return;
			}
		}
        public void PublishActivity( LRW.Resource resource, IPB.IWebUser user, string eventType )
        {
            if ( resource == null || resource.Id == 0 )
                return;

            try
            {
                string title = ResourceBizService.FormatFriendlyTitle( resource.Version.Title );

                EFDAL.ActivityLog log = new EFDAL.ActivityLog();
                log.CreatedDate = System.DateTime.Now;
                log.ActivityType = "Audit";
                log.Activity = "Resource";
                log.Event = eventType;

                log.RelatedTargetUrl = string.Format( "/Resource/{0}/{1}", resource.Id, ResourceBizService.FormatFriendlyTitle( resource.Version.Title ) );

                log.Comment = string.Format( "{0} published resource: {1} (resource Id:{2})", user.FullName(), title, resource.Id );
                //actor type - person
                log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id;
                log.TargetUserId = 0;

                log.ActivityObjectId = resource.Id;

                log.SessionId = GetCurrentSessionId();
                SiteActivityAdd( log );

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".PublishActivity()" );
                return;
            }
        } //

        #endregion
                    
        #region Site Activity - libraries
        public static void LibrariesSearchHit(string filter, ThisUser user, string type)
        {

            //EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Library";
            log.Event = type + " Search";

            log.RelatedTargetUrl = "";

            if (user != null && user.Id > 0)
                log.Comment = string.Format("Action: {0}, user: {1}, Filter: {2}", log.Event, user.FullName(), filter);
            else
                log.Comment = string.Format("Action: {0},  Guest user, Filter: {1}", log.Event, filter);
            //actor type - person
            log.ActionByUserId = (user == null || user.Id == 0) ? 0 : user.Id;
            log.TargetUserId = 0;
            log.ActivityObjectId = 0;
            log.SessionId = GetCurrentSessionId();

            try
            {
                SiteActivityAdd(log);
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(ex, thisClassName + ".LibrariesSearchHit()");
                return;
            }
        }
        public static void LibraryHit( int libraryId, ThisUser user, string action )
        {
            if ( libraryId == 0 )
                return;

            Library entity = new LibraryBizService().Get( libraryId );
            if ( entity == null || entity.Id == 0 )
                return;

            //EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Library";
            log.Event = (action == null || action== "") ? "Visit" : action;

            log.RelatedTargetUrl = string.Format( "/Library/{0}/{1}", entity.Id, ResourceBizService.FormatFriendlyTitle( entity.Title ) );

            if ( user != null && user.Id > 0 )
                log.Comment = string.Format( "Action: {0}, Library: {1}, user: {2}", log.Event, entity.Title, user.FullName() );
            else
                log.Comment = string.Format( "Action: {0}, Library: {1},  Guest user", log.Event, entity.Title );
            //actor type - person
            log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id; 
            log.TargetUserId = 0;

            log.ActivityObjectId = libraryId;
            log.SessionId = GetCurrentSessionId();

            try
            {
                SiteActivityAdd( log );
                //ctx.ActivityLogs.Add( log );
                //int count = ctx.SaveChanges();
                //if ( count > 0 )
                //{
                //    return; // log.Id;
                //}
                //else
                //{
                //    //?no info on error
                //    return;
                //}
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".LibraryHit()" );
                return ;
            }
        }

        public static void LibraryHit( Library entity, ThisUser user, string action )
        {
            //EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Library";
            log.Event = ( action == null || action == "" || action.ToLower() == "select" ) ? "Visit" : action;

            log.RelatedTargetUrl = string.Format( "/Library/{0}/{1}", entity.Id, ResourceBizService.FormatFriendlyTitle( entity.Title ) );

            if ( user != null && user.Id > 0 )
                log.Comment = string.Format( "Action: {0}, Library: {1}, user: {2}", log.Event, entity.Title, user.FullName() );
            else
                log.Comment = string.Format( "Action: {0}, Library: {1},  Guest user", log.Event, entity.Title );
            //actor type - person
            log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id; 
            log.TargetUserId = 0;

            log.ActivityObjectId = entity.Id;
            log.SessionId = GetCurrentSessionId();
            try
            {
                SiteActivityAdd( log );
                //ctx.ActivityLogs.Add( log );
                //int count = ctx.SaveChanges();
                //if ( count > 0 )
                //{
                //    return; // log.Id;
                //}
                //else
                //{
                //    //?no info on error
                //    return;
                //}
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".LibraryHit()" );
                return;
            }
        }
        
        public static void CollectionHit( int collectionId, IPB.IWebUser user, string action )
        {
            if ( collectionId == 0 )
                return;

            LibrarySection entity = new LibraryBizService().LibrarySectionGet( collectionId );
            if ( entity == null || entity.Id == 0 )
                return;
            if ( entity.ParentLibrary == null || entity.ParentLibrary.Id == 0 )
                return;

            //EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Collection";
            log.Event = ( action == null || action == "" || action.ToLower() == "select") ? "Visit" : action;

            log.RelatedTargetUrl = string.Format( "/Library/Collection/{0}/{1}/{2}",entity.LibraryId, entity.Id, ResourceBizService.FormatFriendlyTitle( entity.Title ) );

            if ( user != null && user.Id > 0 )
                log.Comment = string.Format( "Action: {0}, Library: {1}, Collection: {2}, user: {3}", log.Event, entity.ParentLibrary.Title, entity.Title, user.FullName() );
            else
                log.Comment = string.Format( "Action: {0}, Library: {1}, Collection: {2},  Guest user", log.Event, entity.ParentLibrary.Title, entity.Title );
            //actor type - person
            log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id; ;
            log.TargetUserId = 0;

            log.ActivityObjectId = entity.Id;
            log.ObjectRelatedId = entity.LibraryId;

            log.SessionId = GetCurrentSessionId();
            try
            {
                SiteActivityAdd( log );
                //ctx.ActivityLogs.Add( log );
                //int count = ctx.SaveChanges();
                //if ( count > 0 )
                //{
                //    return; // log.Id;
                //}
                //else
                //{
                //    //?no info on error
                //    return;
                //}
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".CollectionHit()" );
                return;
            }
        }
        public static void LibResourceCopy( int fromLibraryId, int toCollectionId, int resourceId, IPB.IWebUser user, string eventType )
        {
            if ( toCollectionId == 0 )
                return;

            Library lib = new LibraryBizService().Get( fromLibraryId );
            if ( lib == null || lib.Id == 0 )
                return;

            LibrarySection entity = new LibraryBizService().LibrarySectionGet( toCollectionId );
            if ( entity == null || entity.Id == 0 )
                return;
            if ( entity.ParentLibrary == null || entity.ParentLibrary.Id == 0 )
                return;

            LRW.ResourceVersion res = ResourceBizService.ResourceVersion_GetByResourceId( resourceId );

            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Collection";
            log.Event = ( eventType == null || eventType == "" ) ? "Resource Copy" : eventType;

            // log.RelatedTargetUrl = string.Format( "/Library/Collection/{0}/{1}/{2}", entity.LibraryId, entity.Id, ResourceBizService.FormatFriendlyTitle( entity.Title ) );
            if ( user != null && user.Id > 0 )
                log.Comment = string.Format( "Copied resource: {0}, from Library: {1}, to Library/Collection: {2}/{3}, by user: {4}", res.Title, lib.Title, entity.ParentLibrary.Title, entity.Title, user.FullName() );
            else
            {
                //can't happen!
            }
            
            //actor type - person
            log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id; ;
            log.TargetUserId = 0;

            //TODO - actually need a related object here or actually two for source lib and target collection
            log.ActivityObjectId = resourceId;
            log.ObjectRelatedId = fromLibraryId;
            log.TargetObjectId = toCollectionId;

            try
            {
                SiteActivityAdd( log );

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".LibResourceActivity()" );
                return;
            }
        }


        public static void LibResourceActivity( int collectionId, int resourceId, IPB.IWebUser user, string eventType )
        {
            LibResourceActivity( collectionId, resourceId, user, eventType, "" );
        }

       
        public static void LibResourceActivity( int fromCollectionId, int toCollectionId, int resourceId, IPB.IWebUser user, string eventType )
        {
            LibResourceActivity( fromCollectionId, resourceId, user, eventType, "" );
        }

        public static void LibResourceActivity( int collectionId, int resourceId, IPB.IWebUser user, string eventType, string comment )
        {
            if ( collectionId == 0 )
                return;

            LibrarySection entity = new LibraryBizService().LibrarySectionGet( collectionId );
            if ( entity == null || entity.Id == 0 )
                return;
            if ( entity.ParentLibrary == null || entity.ParentLibrary.Id == 0 )
                return;

            LRW.ResourceVersion res = ResourceBizService.ResourceVersion_GetByResourceId( resourceId );

            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Collection";
            log.Event = ( eventType == null || eventType == "" ) ? "Visit" : eventType;

           // log.RelatedTargetUrl = string.Format( "/Library/Collection/{0}/{1}/{2}", entity.LibraryId, entity.Id, ResourceBizService.FormatFriendlyTitle( entity.Title ) );
            if ( eventType == "Delete" )
            {
                log.Comment = string.Format( "Deleted resource: {0}, from Library: {1}, Collection: {2}, user: {3}", res.Title, entity.ParentLibrary.Title, entity.Title, user.FullName() );
            }
            else
            {
                if ( user != null && user.Id > 0 )
                    log.Comment = string.Format( "Action: {0}, Library: {1}, Collection: {2}, Resource: {3}, by user: {4}", log.Event, entity.ParentLibrary.Title, entity.Title, res.Title, user.FullName() );
                else
                    log.Comment = string.Format( "Action: {0}, Library: {1}, Collection: {2}, Resource: {3}, by Guest user", log.Event, entity.ParentLibrary.Title, entity.Title, res.Title );
            }

            //actor type - person
            log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id; ;
            log.TargetUserId = 0;

            //TODO - actually need a related object here
            log.ActivityObjectId = resourceId;
            //note that for resource views, the ObjectRelatedId is libraryId!!!

            log.ObjectRelatedId = entity.Id;
            
            log.TargetObjectId = entity.LibraryId;
            
            try
            {
                SiteActivityAdd( log );
         
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".LibResourceActivity()" );
                return;
            }
        }
        #endregion

        #region Site Activity - Content
        public static void ContentSearchHit(string filter, ThisUser user)
        {
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Content";
            log.Event = "Search";

            log.RelatedTargetUrl = "";

            if (user != null && user.Id > 0)
                log.Comment = string.Format("Action: {0}, user: {1}, Filter: {2}", log.Event, user.FullName(), filter);
            else
                log.Comment = string.Format("Action: {0},  Guest user, Filter: {1}", log.Event, filter);
            //actor type - person
            log.ActionByUserId = (user == null || user.Id == 0) ? 0 : user.Id;
            log.TargetUserId = 0;
            log.ActivityObjectId = 0;
            log.SessionId = GetCurrentSessionId();

            try
            {
                SiteActivityAdd(log);
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(ex, thisClassName + ".ContentSearchHit()");
                return;
            }
        }
        public void ContentHit( int contentId, IWebUser user )
        {
            if ( contentId == 0 )
                return;

            ContentItem entity = new ContentServices().Get( contentId );
            if ( entity == null || entity.Id == 0 )
                return;

            ContentHit( entity, user );
        }

        public void ContentHit( ContentItem entity, IWebUser user )
        {
            //EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            if ( entity.TypeId < ContentItem.DOCUMENT_CONTENT_ID ) 
                log.Activity = "Content";
            else if ( entity.TypeId == ContentItem.DOCUMENT_CONTENT_ID )
                log.Activity = "Content Document";
            else if ( entity.TypeId == ContentItem.EXTERNAL_URL_CONTENT_ID )
                log.Activity = "Content Web Reference";
            else if ( entity.TypeId == ContentItem.CURRICULUM_CONTENT_ID )
                log.Activity = "Learning List";
            else
                log.Activity = "Node: Child Node";

            log.Event = "Visit";  // +entity.Title;
            log.RelatedTargetUrl = string.Format( "/LearningList/{0}/{1}", entity.Id, ResourceBizService.FormatFriendlyTitle( entity.Title ) );
            if ( user != null && user.Id > 0 )
                log.Comment = string.Format( "Action: {0}, Content: {1}, user: {2}", log.Event, entity.Title, user.FullName() );
            else
                log.Comment = string.Format( "Action: {0}, Content: {1},  Guest user", log.Event, entity.Title );
            //actor type - person
            log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id;
            log.TargetUserId = 0;

            log.ActivityObjectId = entity.Id;
            log.ObjectRelatedId = entity.ParentId;
            log.SessionId = GetCurrentSessionId();
            try
            {
                SiteActivityAdd( log );

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ContentHit()" );
                return;
            }
        }
        public void NodeHit( ContentItem parent, ContentItem entity, IWebUser user )
        {
            //EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            if ( entity.TypeId < ContentItem.DOCUMENT_CONTENT_ID )
                log.Activity = "Node: Content";
            else if ( entity.TypeId == ContentItem.DOCUMENT_CONTENT_ID )
                log.Activity = "Node: Content Document";
            else if ( entity.TypeId == ContentItem.EXTERNAL_URL_CONTENT_ID )
                log.Activity = "Node: Content Web Reference";
            else
                log.Activity = "Node: Child Node";

            log.Event = "Visit";  // +entity.Title;
            log.RelatedTargetUrl = string.Format( "/LearningList/{0}/{1}", entity.Id, ResourceBizService.FormatFriendlyTitle( entity.Title ));

            if ( user != null && user.Id > 0 )
                log.Comment = string.Format( "Action: {0}, Parent: {1}, Content: {2}, user: {3}", log.Event, parent.Title,  entity.Title, user.FullName() );
            else
                log.Comment = string.Format( "Action: {0}, Parent: {1}, Content: {2},  Guest user", log.Event, parent.Title, entity.Title );
            //actor type - person
            log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id;
            log.TargetUserId = 0;
            log.ObjectRelatedId = parent.Id;
            log.ActivityObjectId = entity.Id;
            log.SessionId = GetCurrentSessionId();
            try
            {
                SiteActivityAdd( log );
                //ctx.ActivityLogs.Add( log );
                //int count = ctx.SaveChanges();
                //if ( count > 0 )
                //{
                //    return; // log.Id;
                //}
                //else
                //{
                //    //?no info on error
                //    return;
                //}
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".NodeHit()" );
                return;
            }
        }

        public static void LearningList_DocumentHit( int topNodeId, int nodeId, int contentId, IWebUser user )
        {
            //topNodeId would equal nodeId, if at the top level

            //ContentItem topNode = new CurriculumServices().Get( topNodeId );
            //if ( topNode == null || topNode.Id == 0 )
            //    return;

            ContentItem parent = new CurriculumServices().Get( nodeId );
            if ( parent == null || parent.Id == 0 )
                return;

			ContentItem entity = new CurriculumServices().Get( contentId );
            if ( entity == null || entity.Id == 0 )
                return;

            DocumentHit( topNodeId, parent, entity, user );
        }

        private static void DocumentHit( int topNodeId, ContentItem parent, ContentItem entity, IWebUser user )
        {
            //EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            if ( entity.TypeId < ContentItem.DOCUMENT_CONTENT_ID )
                log.Activity = "Node: Content";
            else if ( entity.TypeId == ContentItem.DOCUMENT_CONTENT_ID )
                log.Activity = "Node: Content Document";
            else if ( entity.TypeId == ContentItem.EXTERNAL_URL_CONTENT_ID )
                log.Activity = "Node: Content Web Reference";
            else
                log.Activity = "Node: Child Node";

            log.Event = "Visit";  
            //log.RelatedTargetUrl = string.Format( "/Content/{0}/{1}", entity.ResourceIntId, ResourceBizService.FormatFriendlyTitle( entity.Title ) );
            //NOTE: don't use direct url, in case protected
            //log.RelatedTargetUrl = entity.DocumentUrl;

            if ( user != null && user.Id > 0 )
                log.Comment = string.Format( "Action: {0}, Parent: {1}, Content: {2}, user: {3}", log.Event, parent.Title, entity.Title, user.FullName() );
            else
                log.Comment = string.Format( "Action: {0}, Parent: {1}, Content: {2},  Guest user", log.Event, parent.Title, entity.Title );

            //actor type - person
            log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id;
            log.TargetUserId = 0;
            log.ObjectRelatedId = parent.Id;
            log.ActivityObjectId = entity.Id;
            log.TargetObjectId = topNodeId;

            log.SessionId = GetCurrentSessionId();
            try
            {
                SiteActivityAdd( log );
     
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".DocumentHit()" );
                return;
            }
        }

        public void DownloadHit( ContentItem entity, IWebUser user, string action )
        {
            //EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            if (entity.TypeId == ContentItem.CURRICULUM_CONTENT_ID)
                log.Activity = "Learning List";
            else 
                log.Activity = "Content";

            log.Event = "Download";   // +entity.Title;

            if ( user != null && user.Id > 0 )
                log.Comment = string.Format( "Action: {0}, Content: {1}, user: {2}", log.Event, entity.Title, user.FullName() );
            else
                log.Comment = string.Format( "Action: {0}, Content: {1},  Guest user", log.Event, entity.Title );

            //actor type - person
            log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id;
            log.TargetUserId = 0;
            log.ObjectRelatedId = entity.ParentId;
            log.ActivityObjectId = entity.Id;
            log.SessionId = GetCurrentSessionId();
            try
            {
                SiteActivityAdd( log );
                //ctx.ActivityLogs.Add( log );
                //int count = ctx.SaveChanges();
                //if ( count > 0 )
                //{
                //    return; // log.Id;
                //}
                //else
                //{
                //    //?no info on error
                //    return;
                //}
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".DownloadHit()" );
                return;
            }
        }
        #endregion
        #region Site Activity - account
        public static int UserRegistration( ThisUser entity, string ipAddress )
        {
            string server = ILPathways.Utilities.UtilityManager.GetAppKeyValue( "serverName", "" );
            //EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Account";
            log.Event = "Registration";
            log.Comment = string.Format( "{0} ({1}) Registration. From IPAddress: {2}, on server: {3}", entity.FullName(), entity.Id, ipAddress, server );
            //actor type - person, system
            log.ActionByUserId = entity.Id;
            log.TargetUserId = entity.Id;
            log.SessionId = GetCurrentSessionId();
            try
            {
                return SiteActivityAdd( log );
                //ctx.ActivityLogs.Add( log );
                //int count = ctx.SaveChanges();
                //if ( count > 0 )
                //{
                //    return; // log.Id;
                //}
                //else
                //{
                //    //?no info on error
                //    return;
                //}
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".UserRegistrationActivity()" );
                return 0;
            }
        }
        public static int UserRegistrationFromPortal( ThisUser entity, string ipAddress )
        {
            string server = ILPathways.Utilities.UtilityManager.GetAppKeyValue( "serverName", "" );
            //EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Account";
            log.Event = "Portal SSO Registration";
            log.Comment = string.Format( "{0} ({1}) Portal SSO Registration. From IPAddress: {2}, on server: {3}", entity.FullName(), entity.Id, ipAddress, server );
            //actor type - person, system
            log.ActionByUserId = entity.Id;
            log.TargetUserId = entity.Id;
            log.SessionId = GetCurrentSessionId();
            try
            {
                return SiteActivityAdd( log );
                //ctx.ActivityLogs.Add( log );
                //int count = ctx.SaveChanges();
                //if ( count > 0 )
                //{
                //    return; // log.Id;
                //}
                //else
                //{
                //    //?no info on error
                //    return;
                //}
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".UserRegistrationFromPortal()" );
                return 0;
            }
        }
        public static int UserRegistrationConfirmation( ThisUser entity )
        {
            string server = ILPathways.Utilities.UtilityManager.GetAppKeyValue( "serverName", "" );
            //EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
           
            try
            {
                log.CreatedDate = System.DateTime.Now;
                log.ActivityType = "Audit";
                log.Activity = "Account";
                log.Event = "Confirmation";
                log.Comment = string.Format( "{0} ({1}) Registration Confirmation, on server: {2}", entity.FullName(), entity.Id, server );
                //actor type - person, system
                log.ActionByUserId = entity.Id;
                log.TargetUserId = entity.Id;

                log.SessionId = GetCurrentSessionId();

                return SiteActivityAdd( log );

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".UserRegistrationConfirmation()" );
                return 0;
            }
        }
        public static int UserAutoAuthentication( ThisUser entity )
        {
            return UserAuthentication( entity, "auto" );
        }
        public static int UserAuthentication( ThisUser entity )
        {
            return UserAuthentication( entity, "login" );
        }
        public static int UserPortalAuthentication( ThisUser entity )
        {
            return UserAuthentication( entity, "Portal SSO" );
        }
        private static int UserAuthentication( ThisUser entity, string type )
        {

            string server = ILPathways.Utilities.UtilityManager.GetAppKeyValue( "serverName", "" );
            //EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Account";
            log.Event = "Authentication: " + type;
            log.Comment = string.Format( "{0} ({1}) logged in ({2}) on server: {3}", entity.FullName(), entity.Id, type, server );
            //actor type - person, system
            log.ActionByUserId = entity.Id;
            log.TargetUserId = entity.Id;
            log.SessionId = GetCurrentSessionId();
            try
            {
                return SiteActivityAdd( log );

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".UserAuthentication()" );
                return 0;
            }
 }

        public static void SessionStartActivity(string comment, string sessionId, string ipAddress, string referrer)
        {
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            bool isBot = false;
            string server = ILPathways.Utilities.UtilityManager.GetAppKeyValue( "serverName", "" );
            try
            {
                if ( comment == null ) comment = "";

                log.CreatedDate = System.DateTime.Now;
                log.ActivityType = "Audit";
                log.Activity = "Session";
                log.Event = "Start";
                log.Comment = comment + string.Format( " (on server: {0})", server );
                log.Comment += GetUserAgent(ref isBot);

                log.SessionId = sessionId;
                log.IPAddress = ipAddress;
                log.Referrer = referrer;

                SiteActivityAdd( log );

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".SessionStartActivity()" );
                return;
            }

        }

        public static int SiteActivityAdd( string activity, string eventType, string comment
                    , int actionByUserId, int targetUserId, int activityObjectId )
        {
            return SiteActivityAdd( activity, eventType, comment, actionByUserId, targetUserId, activityObjectId, "", "" );
        }

        public static int SiteActivityAdd( string activity, string eventType, string comment
                    , int actionByUserId, int targetUserId, int activityObjectId
                    , string sessionId, string ipAddress)
        {
            string server = ILPathways.Utilities.UtilityManager.GetAppKeyValue( "serverName", "" );
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            if ( sessionId == null || sessionId.Length < 10 )
                sessionId = HttpContext.Current.Session.SessionID;

            if ( ipAddress == null || ipAddress.Length < 10 )
                ipAddress = GetUserIPAddress();

            try
            {
                log.CreatedDate = System.DateTime.Now;
                log.ActivityType = "Audit";
                log.Activity = activity;
                log.Event = eventType;
                log.Comment = comment + string.Format( " (on server: {0})", server );
                //actor type - person, system
                if ( actionByUserId > 0 )
                    log.ActionByUserId = actionByUserId;
                if ( targetUserId > 0 )
                    log.TargetUserId = targetUserId;
                if ( activityObjectId > 0 )
                    log.ActivityObjectId = activityObjectId;

                log.SessionId = sessionId;
                log.IPAddress = ipAddress;

                return SiteActivityAdd( log );

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".SiteActivityAdd()" );
                return 0;
            }
        } //

        #endregion
        #region Resource activity
        public void ResourceHit( int resourceId, string title, IWebUser user )
        {
            if ( resourceId == 0 )
                return;

            ResourceHit( resourceId, title, 0, 0,  user );
        }
        public static void ResourceClickThroughHit( int resourceId, string userGUID, string title )
        {
            if ( resourceId == 0 )
                return;
            ThisUser user = new ThisUser();
            string statusMessage = "";
            if ( userGUID != null && userGUID.Length == 36 )
            {
                user = new AccountServices().GetByRowId( userGUID, ref statusMessage );
            }

            if ( title == null || title.Trim().Length == 0 )
            {
                //first try to change the caller to provider title
                //than if really needed, get the resource - prefer to avoid unnecessary I/O
                title = "resource title missing";
                LRW.ResourceVersion rv = ResourceBizService.ResourceVersion_GetByResourceId( resourceId );
                if ( rv != null && rv.Title != null )
                {
                    title = rv.Title;
                }
            }
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            
            log.Activity = "Resource";
            log.Event = "Click Through";
           
            try
            {

                log.RelatedTargetUrl = string.Format( "/Resource/{0}/{1}", resourceId, title );
                if ( user != null && user.Id > 0 )
                    log.Comment = string.Format( "Action: {0}, Content: {1}, user: {2}", log.Event, title, user.FullName() );
                else
                    log.Comment = string.Format( "Guest user. Resource: {0} Action: {0}, Content: {1},  ", log.Event, title );

                //actor type - person
                log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id;
                log.TargetUserId = 0;

                log.ActivityObjectId = resourceId;

                SiteActivityAdd( log );
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ResourceClickThroughHit()" );
                return;
            }
        }

        public static void ResourceHit( int resourceId, string title, int libraryId, int collectionId,  IWebUser user )
        {
            
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            string suffix = "";
            if ( libraryId == 0 &&  collectionId == 0 )
            {
                log.Activity = "Resource";
                log.Event = "Visit";
            }
            else
            {
                Library lib = new LibraryBizService().Get( libraryId );
                if ( lib == null || lib.Id == 0 )
                    return;

                log.Activity = "Library";
                log.Event = "View Resource";
                suffix = string.Format( " from library: " + lib.Title );
                log.ObjectRelatedId = libraryId;
				if ( collectionId == 0 )
				{
					//try to get a collectionId using lib and resource. Will return first, if multiple
					collectionId = EFDAL.EFLibraryManager.GetCollectionForLibraryResource( libraryId, resourceId );
				}
                if ( collectionId > 0 )
                {
                    log.TargetObjectId = collectionId;
                    LibrarySection entity = new LibraryBizService().LibrarySectionGet( collectionId );
                    if ( entity != null && entity.Id > 0 )
                    {
                        suffix += string.Format( " / Collection: " + entity.Title );
                    }
                }
            }

            try
            {

                log.RelatedTargetUrl = string.Format( "/Resource/{0}/{1}", resourceId, ResourceBizService.FormatFriendlyTitle( title ) );
                if ( user != null && user.Id > 0 )
                    log.Comment = string.Format( "Action: {0}, Content: {1}, user: {2}", log.Event, title, user.FullName() );
                else
                    log.Comment = string.Format( "Guest user viewed resource: {0} Action: {0}, Content: {1},  ", log.Event, title );

                log.Comment += suffix;

                //actor type - person
                log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id;
                log.TargetUserId = 0;

                log.ActivityObjectId = resourceId;
            
                SiteActivityAdd( log );
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ResourceHit()" );
                return;
            }
        }
        #endregion 

        public static int SiteActivityAdd( EFDAL.ActivityLog log )
        {
            int count = 0;
            string truncateMsg = "";
            bool isBot = false;
            string server = ILPathways.Utilities.UtilityManager.GetAppKeyValue( "serverName", "" );

            string agent = GetUserAgent( ref isBot );

            if ( log.RelatedTargetUrl == null ) log.RelatedTargetUrl = "";
            if ( log.RelatedImageUrl == null ) log.RelatedImageUrl = "";
            if ( log.Referrer == null ) log.Referrer = "";
            if ( log.Comment == null ) log.Comment = "";
			if ( log.SessionId == null || log.SessionId.Length < 10 )
                log.SessionId = GetCurrentSessionId();

            if ( log.IPAddress == null || log.IPAddress.Length < 10 )
                log.IPAddress = GetUserIPAddress();
			if ( log.IPAddress.Length > 50 )
				log.IPAddress = log.IPAddress.Substring( 0, 50 );

            //================================
			if ( isBot )
			{
				ServiceHelper.DoBotTrace( 6, string.Format( ".SiteActivityAdd Skipping Bot: activity. Agent: {0}, Activity: {1}, Event: {2}, \r\nRelatedTargetUrl: {3}", agent, log.Activity, log.Event, log.RelatedTargetUrl ) );
				//should this be added with isBot attribute for referencing when crawled?
				return 0;
			}
			//================================
            if (IsADuplicateRequest(log.Comment))
                return 0;

            StoreLastRequest(log.Comment);

            //----------------------------------------------
            if ( log.Referrer == null || log.Referrer.Trim().Length < 5 )
            {
                string referrer = GetUserReferrer();
                log.Referrer = referrer;
            }
            if ( log.Referrer.Length > 1000 )
            {
                truncateMsg += string.Format("Referrer overflow: {0}; ",log.Referrer.Length);
                log.Referrer = log.Referrer.Substring( 0, 1000 );
            }


            if ( log.RelatedTargetUrl != null && log.RelatedTargetUrl.Length > 500 )
            {
                truncateMsg += string.Format( "RelatedTargetUrl overflow: {0}; ", log.RelatedTargetUrl.Length );
                log.RelatedTargetUrl = log.RelatedTargetUrl.Substring( 0, 500 );
            }
            if ( log.RelatedImageUrl != null && log.RelatedImageUrl.Length > 500 )
            {
                truncateMsg += string.Format( "RelatedImageUrl overflow: {0}; ", log.RelatedImageUrl.Length );
                log.RelatedImageUrl = log.RelatedImageUrl.Substring( 0, 500 );
            }
            //if ( log.Referrer.Length > 0 )
            //    log.Comment += ", Referrer: " + log.Referrer;

            //log.Comment += GetUserAgent();

            if ( log.Comment != null && log.Comment.Length > 1000 ) 
            {
                truncateMsg += string.Format( "Comment overflow: {0}; ", log.Comment.Length );
                log.Comment = log.Comment.Substring( 0, 1000 );
            }

            //the following should not be necessary but getting null related exceptions
            if ( log.TargetUserId == null )
                log.TargetUserId = 0;
            if ( log.ActionByUserId == null )
                log.ActionByUserId = 0;
            if ( log.ActivityObjectId == null )
                log.ActivityObjectId = 0;
            if ( log.ObjectRelatedId == null )
                log.ObjectRelatedId = 0;
            if ( log.TargetObjectId == null )
                log.TargetObjectId = 0;


            //using ( var context = new EFDAL.IsleContentEntities() )
            using ( var context = new EFDAL.IsleContentContext() )
            {
                try
                {
                    log.CreatedDate = System.DateTime.Now;
                    if (log.ActivityType == null || log.ActivityType.Length < 5)
                        log.ActivityType = "Audit";

                    context.ActivityLogs.Add( log );

                    // submit the change to database
                    count = context.SaveChanges();

                    if ( truncateMsg.Length > 0 )
                    {
                        string msg = string.Format( "ActivityId: {0}, Message: {1}", log.Id, truncateMsg );

                        ServiceHelper.NotifyAdmin( "ActivityLog Field Overflow", truncateMsg );
                    }
                    if ( count > 0 )
                    {
                        return log.Id;
                    }
                    else
                    {
                        //?no info on error
                        return 0;
                    }
                }
                catch ( Exception ex )
                {
                    
                    LoggingHelper.LogError( ex, thisClassName + ".SiteActivityAdd(EFDAL.ActivityLog) ==> trying via proc\n\r" + ex.StackTrace.ToString() );
                    //call stored proc as backup!

                    count = ActivityAuditManager.LogActivity( log.ActivityType, 
                        log.Activity,
                        log.Event, log.Comment, 
                        log.TargetUserId == null ? 0 : ( int ) log.TargetUserId,
                        log.ActivityObjectId == null ? 0 : ( int ) log.ActivityObjectId,
                        log.ActionByUserId == null ? 0 : ( int ) log.ActionByUserId,
                        log.ObjectRelatedId == null ? 0 : ( int ) log.ObjectRelatedId,
                        log.RelatedImageUrl, 
                        log.RelatedTargetUrl, 
                        log.SessionId, 
                        log.IPAddress,
                        log.TargetObjectId == null ? 0 : ( int ) log.TargetObjectId,
                        log.Referrer );

                    return count;
                }
            }
 } //
        public static void StoreLastRequest( string actionComment)
        {
            string sessionKey = GetCurrentSessionId() + "_lastHit";

            try
            {
                if (HttpContext.Current.Session != null)
                {
                    HttpContext.Current.Session[sessionKey] = actionComment;
                }
            }
            catch
            {
            }
            
        } //

        public static bool IsADuplicateRequest(string actionComment)
        {
            string sessionKey = GetCurrentSessionId() + "_lastHit";
            bool isDup = false;
            try
            {
                if (HttpContext.Current.Session != null)
                {
                    string lastAction = HttpContext.Current.Session[sessionKey].ToString();
                    if (lastAction.ToLower() == actionComment.ToLower())
                    {
                        ServiceHelper.DoTrace(7, "ActivityBizServices. Duplicate action: " + actionComment);
                        return true;
                    }
                }
            }
            catch
            {

            }
            return isDup;
        }
        public static string GetCurrentSessionId()
        {
            string sessionId = "unknown";

            try
            {
                if ( HttpContext.Current.Session != null )
                {
                    sessionId = HttpContext.Current.Session.SessionID;
                }
            }
            catch
            {
            }
            return sessionId;
        }

        private static string GetUserIPAddress()
        {
            string ip = "unknown";
            try
            {
                ip = HttpContext.Current.Request.ServerVariables[ "HTTP_X_FORWARDED_FOR" ];
                if ( ip == null || ip == "" || ip.ToLower() == "unknown" )
                {
                    ip = HttpContext.Current.Request.ServerVariables[ "REMOTE_ADDR" ];
                }
            }
            catch ( Exception ex )
            {

            }

            return ip;
        } //
        private static string GetUserReferrer()
        {
            string lRefererPage = "";
            try
            {
                if ( HttpContext.Current.Request.UrlReferrer != null )
                {
                    lRefererPage = HttpContext.Current.Request.UrlReferrer.ToString();
                    //check for link to us parm
                    //??

                    //handle refers from illinoisworknet.com 
                    if ( lRefererPage.ToLower().IndexOf( ".illinoisworknet.com" ) > -1 )
                    {
                        //may want to keep reference to determine source of this condition. 
                        //For ex. user may have let referring page get stale and so a new session was started when user returned! 

                    }
                }
            }
            catch ( Exception ex )
            {
                lRefererPage = "unknown";// ex.Message;
            }

            return lRefererPage;
        } //
        private static string GetUserAgent(ref bool isBot)
        {
            string agent = "";
            try
            {
                if ( HttpContext.Current.Request.UserAgent != null )
                {
                    agent = HttpContext.Current.Request.UserAgent;
                }

                isBot = false;
                if ( agent.ToLower().IndexOf( "bot" ) > -1
                    || agent.ToLower().IndexOf( "spider" ) > -1
                    || agent.ToLower().IndexOf( "slurp" ) > -1
                    || agent.ToLower().IndexOf( "crawl" ) > -1
                    )
                    isBot = true;
                if ( isBot )
                {
                    //what should happen? Skip completely? Should add attribute to track
                    //user agent may NOT be available in this context
                }
            }
            catch ( Exception ex )
            {
                //agent = ex.Message;
            }

            return agent;
        } //
        public int AddActivityTest()
        {
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "type";
            log.Activity = "Activity"; 
            log.Event = "event";

            //actor type - person, system
            log.ActionByUserId = 1;
            log.ActivityObjectId = 1;
            log.TargetUserId = 1;
            log.SessionId = GetCurrentSessionId();

            ctx.ActivityLogs.Add( log );

            // submit the change to database
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                return log.Id;
            }
            else
            {
                //?no info on error
                return 0;
            }
        }

        [Obsolete]
        private int UserAddsLibraryComment(int userId,int libraryId)
        {
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Social";
            log.Activity = "Library";
            log.Event = "Comment";

            //actor type - person, system
            log.ActionByUserId = userId;
            log.ActivityObjectId = libraryId;
            log.Comment = "User added comment to library";

            ctx.ActivityLogs.Add( log );

            // submit the change to database
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                return log.Id;
            }
            else
            {
                //?no info on error
                return 0;
            }
        }


    }
}
