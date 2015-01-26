using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EFDAL = IoerContentBusinessEntities;
using ILPathways.Utilities;
using ILPathways.DAL;
using ILPathways.Business;
using ThisUser = LRWarehouse.Business.Patron;

namespace Isle.BizServices
{
    public class ActivityBizServices
    {
        private static string thisClassName = "ActivityBizServices";
       // EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();

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
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = activity;
            log.Event = activityEvent;
            log.Comment = comment;

            if ( actionByUserId > 0 )
                log.ActionByUserId = actionByUserId;
            if ( activityObjectId > 0 )
                log.ActivityObjectId = activityObjectId;

            try
            {
                ctx.ActivityLogs.Add( log );
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    return; // log.Id;
                }
                else
                {
                    //?no info on error
                    return;
                }
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
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = activityType;
            log.Activity = activity;
            log.Event = activityEvent;
            
            log.Comment = comment;

            if (actionByUserId > 0)
                log.ActionByUserId = actionByUserId;
            if (targetUserId > 0)
                log.TargetUserId = targetUserId;
            if ( relatedId > 0 )
                log.Int2 = relatedId;
            if (activityObjectId > 0)
                log.ActivityObjectId = activityObjectId;

            try
            {
                ctx.ActivityLogs.Add( log );
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    return; // log.Id;
                }
                else
                {
                    //?no info on error
                    return;
                }
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

        #region Site Activity - libraries
        public static void LibraryHit( int libraryId, ThisUser user, string action )
        {
            if ( libraryId == 0 )
                return;

            Library entity = new LibraryBizService().Get( libraryId );
            if ( entity == null || entity.Id == 0 )
                return;

            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Library";
            log.Event = (action == null || action== "") ? "Select" : action;

            log.RelatedTargetUrl = string.Format( "/Library/{0}/{1}", entity.Id, ResourceBizService.FormatFriendlyTitle( entity.Title ) );

            if ( user != null && user.Id > 0 )
                log.Comment = string.Format( "Action: {0}, Library: {1}, user: {2}", log.Event, entity.Title, user.FullName() );
            else
                log.Comment = string.Format( "Action: {0}, Library: {1},  Guest user", log.Event, entity.Title );
            //actor type - person
            log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id; 
            log.TargetUserId = 0;

            log.ActivityObjectId = libraryId;
            try
            {
                ctx.ActivityLogs.Add( log );
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    return; // log.Id;
                }
                else
                {
                    //?no info on error
                    return ;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".LibraryHit()" );
                return ;
            }
        }

        public static void LibraryHit( Library entity, ThisUser user, string action )
        {
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Library";
            log.Event = ( action == null || action == "" ) ? "Select" : action;

            log.RelatedTargetUrl = string.Format( "/Library/{0}/{1}", entity.Id, ResourceBizService.FormatFriendlyTitle( entity.Title ) );

            if ( user != null && user.Id > 0 )
                log.Comment = string.Format( "Action: {0}, Library: {1}, user: {2}", log.Event, entity.Title, user.FullName() );
            else
                log.Comment = string.Format( "Action: {0}, Library: {1},  Guest user", log.Event, entity.Title );
            //actor type - person
            log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id; 
            log.TargetUserId = 0;

            log.ActivityObjectId = entity.Id;
            try
            {
                ctx.ActivityLogs.Add( log );
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    return; // log.Id;
                }
                else
                {
                    //?no info on error
                    return;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".LibraryHit()" );
                return;
            }
        }
        public static void CollectionHit( int collectionId, ThisUser user, string action )
        {
            if ( collectionId == 0 )
                return;

            LibrarySection entity = new LibraryBizService().LibrarySectionGet( collectionId );
            if ( entity == null || entity.Id == 0 )
                return;
            if ( entity.ParentLibrary == null || entity.ParentLibrary.Id == 0 )
                return;

            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Collection";
            log.Event = ( action == null || action == "" ) ? "Select" : action;

            log.RelatedTargetUrl = string.Format( "/Library/Collection/{0}/{1}/{2}",entity.LibraryId, entity.Id, ResourceBizService.FormatFriendlyTitle( entity.Title ) );

            if ( user != null && user.Id > 0 )
                log.Comment = string.Format( "Action: {0}, Library: {1}, Collection: {2}, user: {3}", log.Event, entity.ParentLibrary.Title, entity.Title, user.FullName() );
            else
                log.Comment = string.Format( "Action: {0}, Library: {1}, Collection: {2},  Guest user", log.Event, entity.ParentLibrary.Title, entity.Title );
            //actor type - person
            log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id; ;
            log.TargetUserId = 0;

            log.ActivityObjectId = entity.Id;
            try
            {
                ctx.ActivityLogs.Add( log );
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    return; // log.Id;
                }
                else
                {
                    //?no info on error
                    return;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".CollectionHit()" );
                return;
            }
        }
        #endregion

        #region Site Activity - Content
        public void ContentHit( int contentId, IWebUser user, string action )
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
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
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
                log.Activity = "Learning List Node";

            log.Event = "Visited: " + entity.Title;
            log.RelatedTargetUrl = string.Format( "/LearningList/{0}/{1}", entity.Id, ResourceBizService.FormatFriendlyTitle( entity.Title ) );
            if ( user != null && user.Id > 0 )
                log.Comment = string.Format( "Action: {0}, Content: {1}, user: {2}", log.Event, entity.Title, user.FullName() );
            else
                log.Comment = string.Format( "Action: {0}, Content: {1},  Guest user", log.Event, entity.Title );
            //actor type - person
            log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id;
            log.TargetUserId = 0;

            log.ActivityObjectId = entity.Id;
            try
            {
                ctx.ActivityLogs.Add( log );
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    return; // log.Id;
                }
                else
                {
                    //?no info on error
                    return;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ContentHit()" );
                return;
            }
        }
        public void NodeHit( ContentItem parent, ContentItem entity, IWebUser user )
        {
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
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

            log.Event = "Visited: " + entity.Title;
            log.RelatedTargetUrl = string.Format( "/LearningList/{0}/{1}", entity.Id, ResourceBizService.FormatFriendlyTitle( entity.Title ));

            if ( user != null && user.Id > 0 )
                log.Comment = string.Format( "Action: {0}, Parent: {1}, Content: {2}, user: {3}", log.Event, parent.Title,  entity.Title, user.FullName() );
            else
                log.Comment = string.Format( "Action: {0}, Parent: {1}, Content: {2},  Guest user", log.Event, parent.Title, entity.Title );
            //actor type - person
            log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id;
            log.TargetUserId = 0;
            log.Int2 = parent.Id;
            log.ActivityObjectId = entity.Id;
            try
            {
                ctx.ActivityLogs.Add( log );
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    return; // log.Id;
                }
                else
                {
                    //?no info on error
                    return;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ContentHit()" );
                return;
            }
        }

        public void DownloadHit( ContentItem entity, IWebUser user, string action )
        {
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Content Download";

            log.Event = "Downloaded: " + entity.Title;

            if ( user != null && user.Id > 0 )
                log.Comment = string.Format( "Action: {0}, Content: {1}, user: {2}", log.Event, entity.Title, user.FullName() );
            else
                log.Comment = string.Format( "Action: {0}, Content: {1},  Guest user", log.Event, entity.Title );
            //actor type - person
            log.ActionByUserId = ( user == null || user.Id == 0 ) ? 0 : user.Id;
            log.TargetUserId = 0;
            log.Int2 = entity.ParentId;
            log.ActivityObjectId = entity.Id;
            try
            {
                ctx.ActivityLogs.Add( log );
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    return; // log.Id;
                }
                else
                {
                    //?no info on error
                    return;
                }
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
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Account";
            log.Event = "Registration";
            log.Comment = string.Format( "{0} ({1}) Registration. From IPAddress: {2}, on server: {3}", entity.FullName(), entity.Id, ipAddress, server );
            //actor type - person, system
            log.ActionByUserId = entity.Id;
            log.TargetUserId = entity.Id;
            try
            {
                ctx.ActivityLogs.Add( log );
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
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".UserRegistrationActivity()" );
                return 0;
            }
        }
        public static int UserRegistrationFromPortal( ThisUser entity, string ipAddress )
        {
            string server = ILPathways.Utilities.UtilityManager.GetAppKeyValue( "serverName", "" );
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Account";
            log.Event = "Portal SSO Registration";
            log.Comment = string.Format( "{0} ({1}) Registration. From IPAddress: {2}, on server: {3}", entity.FullName(), entity.Id, ipAddress, server );
            //actor type - person, system
            log.ActionByUserId = entity.Id;
            log.TargetUserId = entity.Id;
            try
            {
                ctx.ActivityLogs.Add( log );
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
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".UserRegistrationFromPortal()" );
                return 0;
            }
        }
        public static int UserRegistrationConfirmation( ThisUser entity )
        {
            string server = ILPathways.Utilities.UtilityManager.GetAppKeyValue( "serverName", "" );
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
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

                ctx.ActivityLogs.Add( log );
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
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Account";
            log.Event = "Authentication: " + type;
            log.Comment = string.Format( "{0} ({1}) logged in on server: {2}", entity.FullName(), entity.Id, server );
            //actor type - person, system
            log.ActionByUserId = entity.Id;
            log.TargetUserId = entity.Id;
            try
            {
                ctx.ActivityLogs.Add( log );
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
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".UserAuthentication()" );
                return 0;
            }
 }

        public static void SessionStartActivity(string comment)
        {
            SiteActivityAdd( "Session", "Start", comment, 0, 0, 0 );
        }

        public static int SiteActivityAdd( string activity, string eventType, string comment, int actionByUserId, int targetUserId, int activityObjectId )
        {
            string server = ILPathways.Utilities.UtilityManager.GetAppKeyValue( "serverName", "" );
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();

            using ( var context = new EFDAL.IsleContentEntities() )
            {
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

                    context.ActivityLogs.Add( log );

                    // submit the change to database
                    int count = context.SaveChanges();
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
                    LoggingHelper.LogError( ex, thisClassName + ".SiteActivityAdd()" );
                    return 0;
                }
            }
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
        #endregion

    }
}
