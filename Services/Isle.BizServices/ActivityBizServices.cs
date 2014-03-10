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
        #endregion

        #region Site Activity - business
        public static int UserRegistration( ThisUser entity )
        {
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Account";
            log.Event = "Registration";
            log.Comment = string.Format( "{0} ({1}) Registration", entity.FullName(), entity.Id );
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
        public static int UserRegistrationConfirmation( ThisUser entity )
        {
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Account";
            log.Event = "Confirmation";
            log.Comment = string.Format("{0} ({1}) Registration Confirmation", entity.FullName(), entity.Id);
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
        private static int UserAuthentication( ThisUser entity, string type )
        {
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = "Account";
            log.Event = "Authentication: " + type;
            log.Comment = string.Format( "{0} ({1}) logged in", entity.FullName(), entity.Id );
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

        public static int SiteActivityAdd( string activity, string eventType, string comment, int actionByUserId, int targetUserId, int activityObjectId )
        {
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            EFDAL.ActivityLog log = new EFDAL.ActivityLog();
            log.CreatedDate = System.DateTime.Now;
            log.ActivityType = "Audit";
            log.Activity = activity;
            log.Event = eventType;
            log.Comment = comment;
            //actor type - person, system
            log.ActionByUserId = actionByUserId;
            log.TargetUserId = targetUserId;
            log.ActivityObjectId = activityObjectId;

            try
            {
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
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".SiteActivityAdd()" );
                return 0;
            }
        }
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
        public int UserAddsLibraryComment(int userId,int libraryId)
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
