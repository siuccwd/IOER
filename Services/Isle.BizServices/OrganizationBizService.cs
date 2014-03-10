using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using EFDAL = GatewayBusinessEntities;
using ILPathways.Business;
using ILPathways.Common;
using ILPathways.DAL;

namespace Isle.BizServices
{
    public class OrganizationBizService : ServiceHelper
    {
        static EFDAL.GatewayEntities1 ctx = new EFDAL.GatewayEntities1();

        #region EF Core
        public static bool Organization_Delete( int id, ref string statusMessage )
        {
            bool action = false;
            try
            {
                EFDAL.Organization entity = ctx.Organizations.SingleOrDefault( s => s.id == id );
                if ( entity != null )
                {
                    ctx.Organizations.Remove( entity );
                    ctx.SaveChanges();
                    action = true;
                }
            }
            catch ( Exception ex )
            {
                LogManager.LogError( ex, string.Format("OrganizationBizService.Organization_Delete(orgid: {0})", id) );
                statusMessage = ex.Message;
            }
            return action;
       }

        public static int Organization_Create( Organization org, ref string statusMessage )
        {

            EFDAL.Organization entity = new EFDAL.Organization();
            try
            {
                //just in case
                org.Created = System.DateTime.Now;
                org.LastUpdatedById = org.CreatedById;
                org.LastUpdated = System.DateTime.Now;
                //may want to ensure not already set
                org.RowId = Guid.NewGuid();

                entity = Organization_FromMap( org );

                ctx.Organizations.Add( entity );

                // submit the change to database
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    statusMessage = "Successful";
                    return entity.id;
                }
                else
                {
                    statusMessage = "Error - Organization_Create failed";
                    //?no info on error
                    return 0;
                }
            }
            catch ( Exception ex )
            {
                LogManager.LogError( ex, string.Format( "OrganizationBizService.Organization_Create( org: {0})", org.Name ) );
                statusMessage = ex.Message;
                return 0;
            }
        }

        public static bool Organization_Update( Organization org )
        {
            bool action = false;
            try
            {
                EFDAL.Organization entity = ctx.Organizations.SingleOrDefault( s => s.id == org.Id );
                if ( entity != null )
                {
                    org.LastUpdated = System.DateTime.Now;
                    entity = Organization_FromMap( org );
                    
                    ctx.SaveChanges();
                    action = true;
                }
            }
            catch ( Exception ex )
            {
                LogManager.LogError( ex, string.Format("OrganizationBizService.Organization_Update( orgId: {0})", org.Id) );
            }
            
            return action; 
         }


        public static Organization EFGet( int id )
        {
            //var questions = ctx.Organizations.Include("Organization_Member").Select(q => q);

            //var blogs1 = ctx.Organizations 
            //              .Include("Organization_Member") 
            //              .ToList(); 

            Organization org = new Organization();
            EFDAL.Organization entity = ctx.Organizations
                    .Include( "Organization_Member" ) 
                    .SingleOrDefault( s => s.id == id );
            if ( entity != null )
            {
                org = Organization_ToMap( entity );
            }

            return org;
        }//

        public static Organization DALGet( int id )
        {
            return OrganizationManager.Get( id );
        }//
        public static Organization EFGetByRowId( string id )
        {
            Organization org = new Organization();
            EFDAL.Organization entity = ctx.Organizations
                    .Include( "Organization_Member" )
                    .SingleOrDefault( s => s.RowId == new Guid( id ));
            if ( entity != null )
            {
                org = Organization_ToMap( entity );
            }

            return org;
        }//

        public List<Organization> Organization_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {

            return OrganizationManager.SearchAsList( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }

        public static EFDAL.Organization Organization_FromMap( Organization fromEntity )
        {

            EFDAL.Organization to = new EFDAL.Organization();
            to.id = fromEntity.Id;
            to.Name = fromEntity.Name;
            to.OrgTypeId = fromEntity.OrgTypeId;
            if ( fromEntity.ParentId != null )
                to.parentId = ( int ) fromEntity.ParentId;
            to.Address = fromEntity.Address1;
            to.Address2 = fromEntity.Address2;
            to.City = fromEntity.City;
            to.State = string.IsNullOrEmpty( fromEntity.State ) ? "IL" : fromEntity.State;
            to.Zipcode = fromEntity.Zipcode;
            to.MainPhone = fromEntity.MainPhone;
            to.MainExtension = fromEntity.MainExtension;
            to.Fax = fromEntity.Fax;

            to.Created = fromEntity.Created;
            to.CreatedById = fromEntity.CreatedById;
            to.RowId = fromEntity.RowId;
            to.LastUpdated = fromEntity.LastUpdated;
            to.LastUpdatedById = fromEntity.LastUpdatedById;
            return to;
        }

        public static Organization Organization_ToMap( EFDAL.Organization fromEntity )
        {

            Organization to = new Organization();
            to.Id = fromEntity.id;
            to.Name = fromEntity.Name;
            to.OrgTypeId = ( int ) fromEntity.OrgTypeId;
            if (fromEntity.parentId != null)
                to.ParentId = ( int ) fromEntity.parentId;
            to.Address1 = fromEntity.Address;
            to.Address2 = fromEntity.Address2;
            to.City = fromEntity.City;
            to.State = string.IsNullOrEmpty( fromEntity.State ) ? "IL" : fromEntity.State;
            to.Zipcode = fromEntity.Zipcode;
            to.MainPhone = fromEntity.MainPhone;
            to.MainExtension = fromEntity.MainExtension;
            to.Fax = fromEntity.Fax;

            to.Created = ( System.DateTime ) fromEntity.Created;
            if ( fromEntity.CreatedById != null )
                to.CreatedById = ( int ) fromEntity.CreatedById;
            to.RowId = fromEntity.RowId == null ? System.Guid.NewGuid() : ( System.Guid ) fromEntity.RowId;
            to.LastUpdated = ( System.DateTime ) fromEntity.LastUpdated;
            if ( fromEntity.LastUpdatedById != null )
                to.LastUpdatedById = ( int ) fromEntity.LastUpdatedById;

            if ( fromEntity.Organization_Member != null && fromEntity.Organization_Member.Count > 0)
            {
                to.OrgMembers = new List<ILPathways.Business.OrganizationMember>();
                foreach ( EFDAL.Organization_Member efom in fromEntity.Organization_Member )
                {
                    to.OrgMembers.Add( OrganizationMember_ToMap( efom ));
                }
            }
            return to;
        }

        #endregion


        #region === organization members
        public bool OrganizationMember_Delete( int id )
        {
            bool action = false;

            EFDAL.Organization_Member entity = ctx.Organization_Member.SingleOrDefault( s => s.Id == id );
            if ( entity != null )
            {
                ctx.Organization_Member.Remove( entity );
                ctx.SaveChanges();
                action = true;
            }
            return action;
        }

        public static int OrganizationMember_Create( OrganizationMember org, ref string statusMessage )
        {

            EFDAL.Organization_Member entity = new EFDAL.Organization_Member();
            entity = OrganizationMember_FromMap( org );

            entity.Created = System.DateTime.Now;
            entity.LastUpdatedById = org.CreatedById;
            entity.LastUpdated = System.DateTime.Now;

            ctx.Organization_Member.Add( entity );

            // submit the change to database
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                statusMessage = "Successful";
                return entity.Id;
            }
            else
            {
                statusMessage = "Error - OrganizationMember_Create failed";
                //?no info on error
                return 0;
            }
        }
        public static bool OrganizationMember_Update( OrganizationMember org, ref string statusMessage )
        {
            bool action = false;
            EFDAL.Organization_Member entity = ctx.Organization_Member.SingleOrDefault( s => s.Id == org.Id );
            if ( entity != null )
            {
                entity = OrganizationMember_FromMap( org );
                entity.LastUpdated = System.DateTime.Now;

                ctx.SaveChanges();
                action = true;
            }

            return action;
        }

        /// <summary>
        /// Get an Org Member ===> may want to use DAL, to get full 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static OrganizationMember OrganizationMember_Get( int id )
        {
            OrganizationMember orgMbr = new OrganizationMember();
            EFDAL.Organization_Member entity = ctx.Organization_Member
                    .SingleOrDefault( s => s.Id == id );
            if ( entity != null )
            {
                orgMbr = OrganizationMember_ToMap( entity );
                //fill roles
                OrganizationMember_FillRoles( orgMbr );
            }

            return orgMbr;
        }//
        /// <summary>
        /// retrieve an org mbr by orgId and userId
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static OrganizationMember OrganizationMember_Get( int orgId, int userId)
        {
            OrganizationMember orgMbr = new OrganizationMember();
            EFDAL.Organization_Member entity = ctx.Organization_Member
                    .SingleOrDefault( s => s.OrgId == orgId && s.UserId == userId );
            if ( entity != null )
            {
                orgMbr = OrganizationMember_ToMap( entity );
                OrganizationMember_FillRoles( orgMbr );
            }

            return orgMbr;
        }//
        public static void OrganizationMember_FillRoles( OrganizationMember orgMbr )
        {
            //fill roles
            List<EFDAL.Organization_MemberRole> roles = ctx.Organization_MemberRole
                            .Include( "Codes_OrgMemberRole" )
                            .Where( s => s.OrgContactId == orgMbr.Id )
                            .ToList();
            if ( roles.Count > 0 )
            {
                orgMbr.MemberRoles = new List<ILPathways.Business.OrganizationMemberRole>();
                foreach ( EFDAL.Organization_MemberRole role in roles )
                {
                    OrganizationMemberRole r = new OrganizationMemberRole();
                    r.Id = role.Id;
                    r.RoleId = ( int ) role.RoleId;
                    r.RoleTitle = role.Codes_OrgMemberRole.Title;
                    r.OrgMemberId = ( int ) role.OrgContactId;

                    orgMbr.MemberRoles.Add( r );
                }
            }
        }//
        public static List<OrganizationMember> OrganizationMember_GetAll( int orgId )
        {
            OrganizationMember orgMbr = new OrganizationMember();
            List<OrganizationMember> mbrs = new List<ILPathways.Business.OrganizationMember>();
            List<EFDAL.Organization_Member> list = ctx.Organization_Member
                    .Where( s => s.OrgId == orgId )
                    .ToList();

            if ( list != null && list.Count > 0 )
            {
                foreach ( EFDAL.Organization_Member efom in list )
                {
                    orgMbr = OrganizationMember_ToMap( efom );
                    mbrs.Add( orgMbr );
                }
            }

            return mbrs;
        }//

        /// <summary>
        /// retrieve list of orgs where user has administer rights
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<OrganizationMember> OrganizationMember_GetAdminOrgs( int userId )
        {
            string pFilter = string.Format("OrgMbrId in (select OrgContactId from [dbo].[Organization.MemberRole]  base inner join [organization.Member] om on base.OrgContactId = om.Id   where om.UserId = {0} AND roleId in (1, 3)) ", userId);
            string pOrderBy = "";
            int pStartPageIndex = 0;
            //just in case, limit list - use different method if need all for a search, etc
            int pMaximumRows = 1000;
            int pTotalRows = 0;
            return OrganizationMember_Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, true, ref pTotalRows );
        }

        /// <summary>
        /// Organization Member search - does not include roles in returned list
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static List<OrganizationMember> OrganizationMember_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            return OrganizationMember_Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, false, ref pTotalRows );
        }

        public static List<OrganizationMember> OrganizationMember_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, bool includeMbrRoles, ref int pTotalRows )
        {

            //return OrganizationManager.SearchAsList( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );

            List<OrganizationMember> list = OrganizationMemberManager.SearchAsList( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
            //?? get all the roles for each one, or include an eagar parameter?
            if ( includeMbrRoles && list != null && list.Count > 0 )
            {
                foreach ( OrganizationMember item in list )
                {
                    List<EFDAL.Organization_MemberRole> roles = ctx.Organization_MemberRole
                                .Include("Codes_OrgMemberRole")
                                .Where( s => s.OrgContactId == item.Id )
                                .ToList();
                    if ( roles.Count > 0 )
                    {
                        item.MemberRoles = new List<ILPathways.Business.OrganizationMemberRole>();
                        foreach ( EFDAL.Organization_MemberRole role in roles )
                        {
                            OrganizationMemberRole r = new OrganizationMemberRole();
                            r.Id = role.Id;
                            r.RoleId =(int) role.RoleId;
                            r.RoleTitle = role.Codes_OrgMemberRole.Title;
                            r.OrgMemberId = (int) role.OrgContactId;

                            item.MemberRoles.Add( r );
                        }
                    }
                }
            }
            return list;
        }

        public static EFDAL.Organization_Member OrganizationMember_FromMap( OrganizationMember fromEntity )
        {

            EFDAL.Organization_Member to = new EFDAL.Organization_Member();
            to.Id = fromEntity.Id;
            to.OrgId = fromEntity.OrgId;
            to.UserId = fromEntity.UserId;
            to.OrgMemberTypeId = fromEntity.OrgMemberTypeId;

            to.Created = fromEntity.Created;
            to.CreatedById = fromEntity.CreatedById;
            to.LastUpdated = fromEntity.LastUpdated;
            to.LastUpdatedById = fromEntity.LastUpdatedById;
            return to;
        }

        public static OrganizationMember OrganizationMember_ToMap( EFDAL.Organization_Member fromEntity )
        {
            OrganizationMember to = new OrganizationMember();
            to.Id = fromEntity.Id;
            to.OrgId = fromEntity.OrgId;
            to.UserId = fromEntity.UserId;
            to.OrgMemberTypeId = (int) fromEntity.OrgMemberTypeId;

            to.Created = fromEntity.Created;
            to.CreatedById = ( int ) fromEntity.CreatedById;
            to.LastUpdated = ( System.DateTime ) fromEntity.LastUpdated;
            to.LastUpdatedById = ( int ) fromEntity.LastUpdatedById;

            return to;
        }

        #endregion


        #region retrieval
        /// <summary>
        /// used by search service and others
        /// </summary>
        /// <returns></returns>
         public static string OrganzationConnection()
         {
            return OrganizationManager.GatewayConnectionRO();
        }//

        /// <summary>
        /// return array of orgs based on prefix
        /// </summary>
        /// <param name="prefixText"></param>
        /// <returns></returns>
        public static string[] OrganzationsAutoComplete( string prefixText )
        {
            return OrganizationManager.OrganzationsAutoComplete( prefixText );

        }//
        #endregion


        #region === codes
        public static List<CodeItem> OrgType_Select()
        {
            CodeItem ci = new CodeItem();
            List<CodeItem> list = new List<CodeItem>();
            List<EFDAL.Codes_OrgType> codes = ctx.Codes_OrgType.OrderBy( s => s.Id).ToList();

            if ( codes != null && codes.Count > 0 )
            {
                foreach ( EFDAL.Codes_OrgType item in codes )
                {
                    ci = new CodeItem();
                    ci.Id = item.Id;
                    ci.Title = item.Title;
                    list.Add( ci );
                }
            }

            return list;
        }//
        public static List<CodeItem> States_Select()
        {
            CodeItem ci = new CodeItem();
            List<CodeItem> list = new List<CodeItem>();
            List<EFDAL.Codes_State> codes = ctx.Codes_State.OrderBy( s => s.StateCode ).ToList();

            if ( codes != null && codes.Count > 0 )
            {
                foreach ( EFDAL.Codes_State item in codes )
                {
                    ci = new CodeItem();
                    ci.Id = item.Id;
                    ci.Title = item.StateCode;
                    ci.Description = item.State;
                    list.Add( ci );
                }
            }

            return list;
        }//
        #endregion
    }
}
