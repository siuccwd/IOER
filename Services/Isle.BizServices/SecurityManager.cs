using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using ILPathways.Business;
using ILPathways.Common;
using MyManager = ILPathways.DAL.SecurityManager;
using ILPathways.Utilities;
using ThisUser = LRWarehouse.Business.Patron;
using EFMgr = IoerContentBusinessEntities.EFSecurityManager;

namespace Isle.BizServices
{
    public class SecurityManager
    {
        
        /// <summary>
        /// Application object to use for checking for publish privileges
        /// </summary>
        public static string CAN_PUBLISH_OBJECT = "Isle.Controls.CanPublish";
        #region == check privileges
        /// <summary>
        /// Retrieve publishing privileges for user
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public static ApplicationRolePrivilege GetUserPublishingPrivileges( IWebUser currentUser )
        {
            return GetGroupObjectPrivileges( currentUser, CAN_PUBLISH_OBJECT );
        }//

        /// <summary>
        /// Retrieve the privileges for the provided user (actually their highest role) and an object. If the object name is blank, default privileges will be returned
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="pObjectName">Control name, or channel, etc.</param>
        /// <returns></returns>
        private static ApplicationRolePrivilege GetGroupObjectPrivilegesOLD( IWebUser currentUser, string pObjectName )
        {
            return MyManager.GetGroupObjectPrivilegesOLD( currentUser, pObjectName );
        }

        /// <summary>
        /// Retrieve the privileges for the provided user (actually their highest role) and an object. If the object name is blank, default privileges will be returned
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="pObjectName">Control name, or channel, etc.</param>
        /// <returns></returns>
        public static ApplicationRolePrivilege GetGroupObjectPrivileges( IWebUser currentUser, string pObjectName )
        {
            ApplicationRolePrivilege entity = new ApplicationRolePrivilege();

            if ( pObjectName == "" )
            {
                entity.SetOrgPrivileges();
                return entity;
            }

            if ( currentUser == null || currentUser.Id == 0 )
            {
                return entity;
            }

            LoggingHelper.DoTrace( 7, string.Format( "Isle.BizServices.SecurityManager.GetGroupObjectPrivileges(IWebUser currentUser). UserId: {0}, object: {1}", currentUser.Id, pObjectName ) );
            //currently only checking if not previously checked, however if an org is added since first check
           //then will miss it!!!!
            OrganizationBizService.FillUserOrgsMbrs( currentUser );
            if ( currentUser.OrgMemberships == null )
            {
                
            }

            entity = MyManager.GetUserGroupObjectPrivileges( currentUser, pObjectName );

            return entity;
        }//

        #endregion  

        #region ApplicationGroupPrivilege
        public static int AppGroupObjectPrivileges_Create( ApplicationGroupPrivilege entity, ref string statusMessage )
        {
            int id = 0;


            return id;
        }//
        public static string AppGroupObjectPrivileges_Update( ApplicationGroupPrivilege entity )
        {
            string status = "";


            return status;
        }//
        public static bool AppGroupObjectPrivileges_Delete( int id, ref string statusMessage )
        {
            bool result = false;
            if ( id < 1 )
            {
                statusMessage = "Error invalid identifier";
                return result;
            }

            return result;
        }
        public static ApplicationGroupPrivilege AppGroupObjectPrivileges_Get( int id )
        {
            ApplicationGroupPrivilege entity = new ApplicationGroupPrivilege();
            if ( id < 1 )
                return entity;

            try
            {

                return entity;
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "SecurityManager.GetAppGroupObjectPrivileges(int id: " + id.ToString() + ") " );
                return entity;
            }
        }//

        /// <summary>
        /// Get all app object privileges for the group
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static DataSet ApplicationGroupPrivilege_Select( int groupId )
        {
            return MyManager.ApplicationGroupPrivilege_Select( groupId );
        }

        /// <summary>
        /// Get application objects not currently in passed group
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static DataSet ApplicationGroupPrivilege_SelectNotInGroup( int groupId )
        {
            return MyManager.ApplicationGroupPrivilege_SelectNotInGroup( groupId );
        }

        public static List<ApplicationGroupPrivilege> ApplicationGroupPrivilegeSelect( int groupId )
        {
            return EFMgr.ApplicationGroupPrivilege_Select( groupId );
        }


        public static List<CodeItem> Codes_PrivilegeDepth_Select()
        {
            List<CodeItem> list = EFMgr.Codes_PrivilegeDepth_Select();

            return list;
        } 
        #endregion
    }
}
