using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using EFDAL = GatewayBusinessEntities;
using ILPathways.Business;
using ILPathways.DAL;

namespace Isle.BizServices
{
    public class GroupServices : ServiceHelper
    {
        EFDAL.GatewayEntities1 ctx = new EFDAL.GatewayEntities1();

        #region Groups
        public static bool Delete( int id, ref string statusMessage )
        {
            return GroupManager.Delete( id, ref statusMessage );
        }//
        public static int Create( AppGroup entity, ref string statusMessage )
        {
            return GroupManager.Create( entity, ref statusMessage);
        }
        public static string Update( AppGroup entity )
        {
            return GroupManager.Update( entity );
        }
        public static AppGroup Get( int id)
        {
            return GroupManager.Get( id );
        }//

        public static AppGroup GetByCode( string groupCode )
        {
            return GroupManager.GetByCode( groupCode );
        }//

         public static DataSet Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            return GroupManager.Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }
        #endregion

        #region Group Members
        public static bool GroupMember_Delete( int id, ref string statusMessage )
        {
            return GroupMemberManager.Delete( id, ref statusMessage );
        }//

        public static int GroupMember_Create( GroupMember entity, ref string statusMessage )
        {
            return GroupMemberManager.Create( entity, ref statusMessage );
        }//
        public static DataSet GroupMember_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            return GroupMemberManager.Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }//


        #endregion
        #region Org Group Members
        public static bool GroupOrgMember_Delete( int id, ref string statusMessage )
        {
            bool action = false;
            EFDAL.GatewayEntities1 ctx = new EFDAL.GatewayEntities1();
            EFDAL.AppGroup_OrgMember entity = ctx.AppGroup_OrgMember.SingleOrDefault( s => s.ID == id );
            if ( entity != null )
            {
                ctx.AppGroup_OrgMember.Remove( entity );
                ctx.SaveChanges();
                action = true;
            }
            return action;
        }//

        public static int GroupOrgMember_Create( GroupOrgMember gom, ref string statusMessage )
        {
            EFDAL.GatewayEntities1 ctx = new EFDAL.GatewayEntities1();

            //entity = OrganizationMember_FromMap( gom );
            EFDAL.AppGroup_OrgMember to = new EFDAL.AppGroup_OrgMember();
            to.GroupId = gom.GroupId;
            to.OrgId = gom.OrgId;
            to.IsActive = true;

            to.LastUpdatedById = gom.CreatedById;
            to.Created = System.DateTime.Now;
            to.LastUpdatedById = gom.CreatedById;
            to.LastUpdated = System.DateTime.Now;
            to.RowId = Guid.NewGuid();
 
            ctx.AppGroup_OrgMember.Add( to );

            // submit the change to database
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                statusMessage = "Successful";
                return to.ID;
            }
            else
            {
                statusMessage = "Error - GroupOrgMember_Create failed";
                //?no info on error
                return 0;
            }
        }//

        public static DataSet GroupOrgMember_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            return GroupMemberManager.GroupOrgMbrSearch( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }//


        #endregion

        #region Privileges
        public static ApplicationRolePrivilege GetGroupObjectPrivileges( IWebUser currentUser, string pObjectName )
        {
            return SecurityManager.GetGroupObjectPrivileges( currentUser, pObjectName );
        }//


        public static ApplicationRolePrivilege GetGroupObjectPrivileges( AppUser currentUser, string pObjectName )
        {
            return SecurityManager.GetGroupObjectPrivileges( currentUser, pObjectName );
        }//
        #endregion

        #region organization group authorization ==> Temp

        /// <summary>
        /// retrieve list of approvers for an org
        /// </summary>
        /// <param name="pOrgId"></param>
        /// <returns></returns>
        public static List<GroupMember> OrgApproversSelect( int pOrgId )
        {
            return GroupMemberManager.OrgApproversSelect( pOrgId );
        }//

        /// <summary>
        /// return true, if user can approve the passed orgId
        /// </summary>
        /// <param name="pOrgId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool IsUserAnOrgApprover( int pOrgId, int userId )
        {
            return GroupMemberManager.IsUserAnOrgApprover( pOrgId, userId );
        }//

        public static bool IsUserAnyOrgApprover( int userId )
        {
            string code = "OrgApprovers";
            return GroupMemberManager.IsAGroupMember( code, userId );
        }//

        /// <summary>
        /// TODO - list of all orgs where user is an approver - useful for searches
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<Organization> ApproverOrgsSelect( int userId )
        {
            List<Organization> list = new List<Organization>();
            return list;    //GroupMemberManager.OrgApproversSelect( userId );
        }//
        #endregion


        #region Codes
        public static DataSet CodesGroupType_Select()
        {
            DataSet ds = DatabaseManager.DoQuery( "SELECT [Id],[Title]  FROM [Gateway].[dbo].[Codes.GroupType] order by [Title]" );
            return ds;    //GroupMemberManager.OrgApproversSelect( userId );
        }//
        #endregion
    }
}
