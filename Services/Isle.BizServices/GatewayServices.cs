using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using IoerContentBusinessEntities;

using IPDAL = ILPathways.DAL;
using DBM = ILPathways.DAL.DatabaseManager;
using ILPathways.Utilities;

namespace Isle.BizServices
{
    public class GatewayServices
    {
		IsleContentContext ctx = new IsleContentContext();

        #region Groups
        public int CreateGroup( AppGroup entity )
        {
            ctx.AppGroups.Add( entity );

            // submit the change to database
            // how to get the last id inserted??
            //should be returned/populated in entity
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                AppGroup appGroup = GetLastAppGroup();
                return appGroup.Id;
            }
            else
            {
                //?no info on error
                return 0;
            }
        }


        public bool Update( AppGroup entity )
        {
            bool action = true;
            //??????
            AppGroup AppGroup = ctx.AppGroups.Single( s => s.Id == entity.Id );
            if ( AppGroup != null )
            {
                //AutoMapper!
                AppGroup.Title = entity.Title;
                AppGroup.Description = entity.Description;
                AppGroup.GroupCode = entity.GroupCode;
				AppGroup.ContactId = entity.ContactId;
				;
                //AppGroup.ApplicationId = entity.ApplicationId;
                AppGroup.GroupTypeId = entity.GroupTypeId;
                AppGroup.IsActive = entity.IsActive;
                AppGroup.OrgId = entity.OrgId;
                AppGroup.ParentGroupId = entity.ParentGroupId;

                AppGroup.LastUpdated = System.DateTime.Now;
                AppGroup.LastUpdatedById = entity.LastUpdatedById;


                // submit the change to database
                ctx.SaveChanges();
            }

            return action;
        }
        public bool Delete( int id )
        {
            bool action = true;
            //??????
            AppGroup entity = ctx.AppGroups.Find( id );
            ctx.AppGroups.Remove( entity );
            ctx.SaveChanges();
            //OR
            AppGroup AppGroup = ctx.AppGroups.Single( s => s.Id == id );
            if ( AppGroup != null )
            {
                ctx.AppGroups.Remove( entity );
                ctx.SaveChanges();
            }

            return action;
        }


        public IQueryable<AppGroup> GetGroups()
        {
            return ctx.AppGroups;
        }

        public List<AppGroup> GetAllGroups()
        {
            //return ctx.AppGroups.Where( s => s.Id > 0 ).ToList();
            return ctx.AppGroups.Where( s => s.IsActive == true ).OrderBy( s => s.Title ).ToList();
        }

        public AppGroup GetLastAppGroup()
        {
            AppGroup entity = new AppGroup();
            List<AppGroup> list = ctx.AppGroups.OrderByDescending( s => s.Id ).ToList();
            if ( list.Count > 0 )
                entity = list[ 0 ];

            return entity;

        }

        #endregion     

        /// <summary>
        /// Retrieve database connection string for SqlQuery table
        /// </summary>
        /// <returns>Connection string</returns>
        public static string GetGatewayConnectionString()
        {
            return IPDAL.BaseDataManager.GatewayConnectionRO();

        }//

        /// <summary>
        /// execute dynamic sql against the content db
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataSet DoQuery( string sql )
        {
            string conn = GetGatewayConnectionString();
            return DBM.DoQuery( sql, conn );
        }

        #region Site Activity - sessions/visits
        public static void LogSessionStart( string sessionId, string serverName, string comment, string remoteIP, string referrer )
        {
            bool showingTrace = false;
            GatewayContext ctx = new GatewayContext();
            AppVisitLog log = new AppVisitLog();
            log.CreatedDate = System.DateTime.Now;
            log.SessionId = sessionId;
            log.URL = "Session Started";
			if ( remoteIP.Length > 25 )
				remoteIP = remoteIP.Substring( 0, 25 );
            log.RemoteIP = remoteIP;
			if ( log.RemoteIP.Length > 25 )
				log.RemoteIP = log.RemoteIP.Substring( 0, 25 );

            log.ServerName = serverName;
			if (string.IsNullOrWhiteSpace(comment) == false && comment.Length > 1500)
				comment = comment.Substring(0, 1500);
            log.Comment = comment;
            log.Application = "IOER";
			if ( string.IsNullOrWhiteSpace( referrer ) == false && referrer.Length > 1000 )
				referrer = referrer.Substring( 0, 1000 );
            log.Referrer = referrer;

            try
            {
                ctx.AppVisitLogs.Add( log );
                int count = ctx.SaveChanges();

				if ( showingTrace )
					LoggingHelper.DoTrace( 1, string.Format( "session start: sId-{0}; Server-{1}; Comment-{2}; remoteIP-{3} ", sessionId, serverName, comment, remoteIP ) );
            }
            catch ( Exception e )
            {
                //log error and ignore
                System.DateTime visitDate = System.DateTime.Now;
                LoggingHelper.DoTrace( 1, string.Format( "session start: sId-{0}; Server-{1}; Comment-{2}; remoteIP-{3} ", sessionId, serverName, comment, remoteIP ) );
               

            }
        } //


        
        #endregion
    }
}
