using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILPathways.Business;
using ILPathways.Common;
using MyManager = ILPathways.DAL.SecurityManager;
using ILPathways.Utilities;
using ThisUser = LRWarehouse.Business.Patron;
using EFMgr = GatewayBusinessEntities.EFSecurityManager;

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
        public static ApplicationRolePrivilege GetGroupObjectPrivilegesOLD( IWebUser currentUser, string pObjectName )
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

            LoggingHelper.DoTrace( 5, string.Format( "Isle.BizServices.SecurityManager.GetGroupObjectPrivileges(IWebUser currentUser). UserId: {0}, object: {1}", currentUser.Id, pObjectName ) );
            //currently only checking if not previously checked, however if an org is added since first check
           //then will miss it!!!!
            OrganizationBizService.FillUserOrgsMbrs( currentUser );
            if ( currentUser.OrgMemberships == null )
            {
                
            }

            entity = MyManager.GetUserGroupObjectPrivileges( currentUser, pObjectName );

            return entity;
        }//

        public static ApplicationRolePrivilege GetGroupObjectPrivilegesXXX( ThisUser currentUser, string pObjectName )
        {
            LoggingHelper.DoTrace( 2, string.Format( "Isle.BizServices.SecurityManager.GetGroupObjectPrivileges(ThisUser currentUser). UserId: {0}, object: {1}", currentUser.Id, pObjectName ) );
            ApplicationRolePrivilege entity = new ApplicationRolePrivilege();

            if ( currentUser.OrgMemberships == null )
            {
                List<OrganizationMember> orgs = OrganizationBizService.OrganizationMember_GetUserOrgs( currentUser.Id );
                if ( currentUser.OrgId > 0 )
                {
                    bool hasOrg = false;
                    if ( orgs == null )
                        orgs = new List<OrganizationMember>();
                    //check if included in orgs
                    foreach ( OrganizationMember mbr in orgs )
                    {
                        if ( mbr.OrgId == currentUser.OrgId )
                        {
                            hasOrg = true;
                            break;
                        }
                    }
                    if ( hasOrg == false )
                    {
                        OrganizationMember om = new OrganizationMember();
                        om.OrgId = currentUser.OrgId;
                        om.UserId = currentUser.Id;
                        om.OrgMemberTypeId = OrganizationMember.MEMBERTYPE_EMPLOYEE;
                        om.MemberRoles = new List<OrganizationMemberRole>();

                        orgs.Add( om );
                    }
                }
                //this would have to be added back to session to be useful (ie session persisted!)
                currentUser.OrgMemberships = orgs;
            }

            entity =  MyManager.GetUserGroupObjectPrivileges( currentUser, pObjectName );

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
        public static List<CodeItem> Codes_PrivilegeDepth_Select()
        {
            List<CodeItem> list = EFMgr.Codes_PrivilegeDepth_Select();

            return list;
        } 
        #endregion
    }
}
