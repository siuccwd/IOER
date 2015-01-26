using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

using EFDAL = GatewayBusinessEntities;
using ILPathways.Business;
using ILPathways.Common;
using ILPathways.DAL;
using ILPathways.Utilities;
using ThisUser = LRWarehouse.Business.Patron;

namespace Isle.BizServices
{
    public class OrganizationBizService : ServiceHelper
    {
       // static EFDAL.GatewayContext context = new EFDAL.GatewayContext();

        #region ======= EF Core
        public static bool Organization_Delete( int id, ref string statusMessage )
        {
            bool action = false;
            try
            {
                using ( var context = new EFDAL.GatewayContext() )
                {
                    EFDAL.Organization entity = context.Organizations.SingleOrDefault( s => s.id == id );
                    if ( entity != null )
                    {
                        context.Organizations.Remove( entity );
                        context.SaveChanges();
                        action = true;
                    }
                }
            }
            catch ( Exception ex )
            {
                LogManager.LogError( ex, string.Format( "OrganizationBizService.Organization_Delete(orgid: {0})", id ) );
                statusMessage = ex.Message;
            }
            return action;
       }

        public static int Organization_Create( Organization org, ref string statusMessage )
        {

            EFDAL.Organization entity = new EFDAL.Organization();
            try
            {
                using ( var context = new EFDAL.GatewayContext() )
                {
                    //just in case
                    org.Created = System.DateTime.Now;
                    org.LastUpdatedById = org.CreatedById;
                    org.LastUpdated = System.DateTime.Now;
                    //may want to ensure not already set
                    if ( org.IsValidRowId( org.RowId ) )
                        org.RowId = org.RowId;
                    else
                        org.RowId = Guid.NewGuid();

                    Organization_FromMap( org, entity );

                    context.Organizations.Add( entity );

                    // submit the change to database
                    int count = context.SaveChanges();
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
                using ( var context = new EFDAL.GatewayContext() )
                {
                    EFDAL.Organization entity = context.Organizations.SingleOrDefault( s => s.id == org.Id );
                    if ( entity != null )
                    {
                        org.LastUpdated = System.DateTime.Now;
                        Organization_FromMap( org, entity );

                        context.SaveChanges();
                        action = true;
                    }
                }
            }
            catch ( Exception ex )
            {
                LogManager.LogError( ex, string.Format( "OrganizationBizService.Organization_Update( orgId: {0})", org.Id ) );
            }
            
            return action; 
         }

        /// <summary>
        /// Retrieve an organization
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Organization EFGet( int id )
        {
            //var questions = context.Organizations.Include("Organization_Member").Select(q => q);

            //var blogs1 = context.Organizations 
            //              .Include("Organization_Member") 
            //              .ToList(); 

            Organization org = new Organization();
            using ( var context = new EFDAL.GatewayContext() )
            {
                EFDAL.Organization entity = context.Organizations
                        .Include( "Organization_Member" )
                        .SingleOrDefault( s => s.id == id );
                if ( entity != null )
                {
                    org = Organization_ToMap( entity );
                }
            }
            return org;
        }//

        private static Organization DALGet( int id )
        {
            return OrganizationManager.Get( id );
        }//
        public static Organization EFGetByRowId( string id )
        {
            Organization org = new Organization();
            using ( var context = new EFDAL.GatewayContext() )
            {
                EFDAL.Organization entity = context.Organizations
                        .Include( "Organization_Member" )
                        .SingleOrDefault( s => s.RowId == new Guid( id ) );
                if ( entity != null )
                {
                    org = Organization_ToMap( entity );
                }
            }
            return org;
        }//

        public static void AssociateUserWithOrg( ThisUser user )
        {
            string statusMessage = "";
            //check for existing email domain, and associate
            int pos = user.Email.IndexOf( "@" );
            if ( pos > -1 )
            {
                string domain = user.Email.Substring( pos + 1 );
                Organization org = GetByEmailDomain( domain );
                if ( org != null && org.Id > 0 )
                {
                    //while normally done for a new user, should do a check to ensure not already a member
                    if ( DoesUserHaveOrgId( user, org.Id ) == false )
                    {
                        int orgMbrTypeId = OrganizationMember.MEMBERTYPE_EMPLOYEE;

                        int id = OrganizationMember_Create( org.Id, user.Id, orgMbrTypeId, user.Id, ref statusMessage );
                        //note - should add a Patron.Profile record - otherwise, will not have the primary OrgId
                        //maybe the orgId should be on the patron?, or may be not at all. Actually need a way to designate a current/primary org, as the org member does not allow designation
                        if ( user.OrgId == 0 )
                        {
                            user.UserProfile.OrganizationId = org.Id;
                            user.OrgId = org.Id;
                            if ( user.HasUserProfile() )
                            {
                                new AccountServices().PatronProfile_Update( user.UserProfile );
                            }
                            else
                            {
                                user.UserProfile.UserId = user.Id;
                                new AccountServices().PatronProfile_Create( user.UserProfile, ref statusMessage );
                            }
                        }
                        //should notify someone
                        //moot until interface, so just admin
                        SendAutoOrgMbrAddNotificationEmail( user, org, ref statusMessage );
                    }
                }

            }
		}//
        private static bool SendAutoOrgMbrAddNotificationEmail( ThisUser user, Organization org, ref string statusMessage )
        {
            bool isValid = false;
            string adminEmail = UtilityManager.GetAppKeyValue( "appAdminEmail", "mparsons@siuccwd.com" );
            string fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", "mparsons@siuccwd.com" );
            string url = "/Account/Login.aspx?nextUrl=/Admin/Org/Organizations.aspx?id=" + org.Id.ToString();
            url = UtilityManager.FormatAbsoluteUrl( url, true );

            //future send to actual admin
            //string emailList = GetApproversEmailList( libraryId, ref adminMbr );
            //adminMbr = GetFirstApprover( libraryId );

            if ( adminEmail.Length > 5 )
            {
                string eMessage = string.Format( "A new user has registered with your domain email. This user has been automatically added to the organization: <br/>Organization: {0} <br/>User: {1} <br/>Email: {2} <br/>Default role: {3}. <br/>", org.Name, user.FullName(), user.Email, "Employee" );

                eMessage += "<p>You can use the organization administration page to change the member role, or remove from your organization as needed. </p>";
                eMessage += string.Format( "<p><a href='{0}'>Follow this link to login to IOER and view the organization.</a></p>", url );


                EmailManager.SendEmail( adminEmail, fromEmail, "Confirm addition of new user to your organization", eMessage );
                isValid = true;
            }
            else
            {
                statusMessage = "Error: an account was not found for the organization administrator";
                //==> send to info@!!!

            }

            return isValid;
        }

        /// <summary>
        /// Retrieve by email domain - what if not unique?
        /// ==> could have separate orgs for high school, middle school, etc, but same domain????
        /// ==> may suggest that it should return a list and consider sending email to all
        /// ==> could use org members
        /// </summary>
        /// <param name="emailDomain"></param>
        /// <returns></returns>
        public static Organization GetByEmailDomain( string emailDomain )
        {
            Organization org = new Organization();
            using ( var context = new EFDAL.GatewayContext() )
            {
                EFDAL.Organization entity = context.Organizations
                        .Include( "Organization_Member" )
                        .SingleOrDefault( s => s.EmailDomain == emailDomain );
                if ( entity != null )
                {
                    org = Organization_ToMap( entity );
                }
            }
            return org;
        }//


        public List<Organization> Organization_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {

            return OrganizationManager.SearchAsList( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }

        public static void Organization_FromMap( Organization fromEntity, EFDAL.Organization to )
        {

            //EFDAL.Organization to = new EFDAL.Organization();
            to.id = fromEntity.Id;
            to.Name = fromEntity.Name;
            to.OrgTypeId = fromEntity.OrgTypeId;
            to.IsIsleMember = fromEntity.IsIsleMember;
            to.IsActive = fromEntity.IsActive;

            to.parentId = ( int ) fromEntity.ParentId;
            to.Address = fromEntity.Address1;
            to.Address2 = fromEntity.Address2;
            to.City = fromEntity.City;
            to.State = string.IsNullOrEmpty( fromEntity.State ) ? "IL" : fromEntity.State;
            to.Zipcode = fromEntity.Zipcode;
            to.MainPhone = fromEntity.MainPhone;
            to.MainExtension = fromEntity.MainExtension;
            to.Fax = fromEntity.Fax;
            to.EmailDomain = fromEntity.EmailDomain;
            to.WebSite = fromEntity.WebSite;
            to.K12Identifier = fromEntity.ExternalIdentifier;

            to.Created = fromEntity.Created;
            to.CreatedById = fromEntity.CreatedById;
            to.RowId = fromEntity.RowId;
            to.LastUpdated = fromEntity.LastUpdated;
            to.LastUpdatedById = fromEntity.LastUpdatedById;
           // return to;
        }

        public static Organization Organization_ToMap( EFDAL.Organization fromEntity )
        {

            Organization to = new Organization();
            to.Id = fromEntity.id;
            to.Name = fromEntity.Name;
            to.OrgTypeId = fromEntity.OrgTypeId == null ? 0 : ( int ) fromEntity.OrgTypeId;
            to.IsIsleMember = fromEntity.IsIsleMember != null ? ( bool ) fromEntity.IsIsleMember : false;
            to.IsActive = fromEntity.IsActive != null ? ( bool ) fromEntity.IsActive : false;

            if ( fromEntity.parentId != null )
                to.ParentId = ( int ) fromEntity.parentId;
            to.Address1 = string.IsNullOrEmpty( fromEntity.Address ) ? "" : fromEntity.Address;
            to.Address2 = string.IsNullOrEmpty( fromEntity.Address2 ) ? "" : fromEntity.Address2;
            to.City = string.IsNullOrEmpty( fromEntity.City ) ? "" : fromEntity.City;
            to.Zipcode = string.IsNullOrEmpty( fromEntity.Zipcode ) ? "" : fromEntity.Zipcode;
            to.State = string.IsNullOrEmpty( fromEntity.State ) ? "IL" : fromEntity.State;

            to.MainPhone = string.IsNullOrEmpty( fromEntity.MainPhone ) ? "" : fromEntity.MainPhone;
            to.MainExtension = string.IsNullOrEmpty( fromEntity.MainExtension ) ? "" : fromEntity.MainExtension;
            to.Fax = string.IsNullOrEmpty( fromEntity.Fax ) ? "" : fromEntity.Fax;

            to.EmailDomain = string.IsNullOrEmpty( fromEntity.EmailDomain ) ? "" : fromEntity.EmailDomain;
            to.WebSite = fromEntity.WebSite;
            to.ExternalIdentifier = string.IsNullOrEmpty( fromEntity.K12Identifier ) ? "" : fromEntity.K12Identifier;

            to.Created = fromEntity.Created == null ? to.DefaultDate : ( System.DateTime ) fromEntity.Created;
            if ( fromEntity.CreatedById != null )
                to.CreatedById = ( int ) fromEntity.CreatedById;
            to.RowId = fromEntity.RowId == null ? System.Guid.NewGuid() : ( System.Guid ) fromEntity.RowId;

            to.LastUpdated = fromEntity.LastUpdated == null ? to.DefaultDate : ( System.DateTime ) fromEntity.LastUpdated;
            if ( fromEntity.LastUpdatedById != null )
                to.LastUpdatedById = ( int ) fromEntity.LastUpdatedById;

            if ( fromEntity.Organization_Member != null && fromEntity.Organization_Member.Count > 0 )
            {
                to.OrgMembers = new List<ILPathways.Business.OrganizationMember>();
                foreach ( EFDAL.Organization_Member efom in fromEntity.Organization_Member )
                {
                    to.OrgMembers.Add( OrganizationMember_ToMap( efom ) );
                }
            }
            return to;
        }

        #endregion

        #region DAL Organzation methods

        public static DataSet GetTopLevelOrganzations()
        {
            return OrganizationManager.GetTopLevelOrganzations(); ;
        }
        public static DataSet GetChildOrganizations( int parentId )
        {
            return OrganizationManager.GetChildOrganizations( parentId ); ;
        }
        public static DataSet GetRTTTOrganzations()
        {
            return OrganizationManager.GetRTTTOrganzations();
        }
        /// <summary>
        /// Search for Organizations using passed parameters
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns>Dataset</returns>
        public static DataSet OrganizationSearch( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            return OrganizationManager.Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }

        /// <summary>
        /// Search for Organizations using passed parameters
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns>List<></returns>
        public static List<Organization> SearchAsList( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            //TODO - create a List<> version
            return OrganizationManager.SearchAsList( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }

        /// <summary>
        /// Get organzation for user
        /// </summary>
        /// <param name="appUser"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static Organization GetOrganization( ThisUser appUser, ref string statusMessage )
        {
            Organization entity = new Organization();
            OrganizationManager mgr = new OrganizationManager();
            if ( appUser.OrgId > 0 )
            {
                entity = OrganizationManager.Get( appUser.OrgId );
            }
            return entity;
        }

        private static Organization GetOrgByName( string orgName )
        {
            Organization entity = new Organization();
            OrganizationManager mgr = new OrganizationManager();
            entity = OrganizationManager.GetByName( orgName );
            return entity;
        }

        public static int OrganizationRequestCreate( OrganizationRequest org, ref string statusMessage )
        {
            return new OrganizationRequestManager().Create( org, ref statusMessage );
        }
        public static string OrganizationRequestUpdate( OrganizationRequest org )
        {
            return new OrganizationRequestManager().Update( org );
        }

        public static List<Library> Organization_GetLibraries( int orgId )
        {
            string filter = string.Format( " (OrgId = {0})", orgId );
            int totalRows = 0;

            return new LibraryBizService().LibrarySearchAsList( filter, "lib.Title", 1, 100, ref totalRows ); ;
        }
        #endregion

        #region === organization members
        public bool OrganizationMember_Delete( int id )
        {
            bool action = false;
            using ( var context = new EFDAL.GatewayContext() )
            {
                EFDAL.Organization_Member entity = context.Organization_Member
                                    .SingleOrDefault( s => s.Id == id );
                if ( entity != null )
                {
                    context.Organization_Member.Remove( entity );
                    context.SaveChanges();
                    action = true;
                }
            }
            return action;
 }

        /// <summary>
        /// Create an org member using a member type of external
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="userId"></param>
        /// <param name="createdById"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static int OrganizationMember_Create( int orgId, int userId, int createdById, ref string statusMessage )
        {
            int orgMbrTypeId = OrganizationMember.MEMBERTYPE_EXTERNAL;

            return OrganizationMember_Create( orgId, userId, orgMbrTypeId, createdById, ref statusMessage );

        }
        /// <summary>
        /// Create an org member
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="userId"></param>
        /// <param name="orgMbrTypeId"></param>
        /// <param name="createdById"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static int OrganizationMember_Create( int orgId, int userId, int orgMbrTypeId, int createdById, ref string statusMessage )
        {
            OrganizationMember om = new OrganizationMember();
            om.UserId = userId;
            om.OrgId = orgId;
            if ( orgMbrTypeId == 0 )
                orgMbrTypeId = OrganizationMember.MEMBERTYPE_EXTERNAL;

            om.OrgMemberTypeId = orgMbrTypeId;
            om.CreatedById = createdById;
            om.LastUpdatedById = createdById;

            return OrganizationMember_Create( om, ref statusMessage );

        }
        /// <summary>
        /// Create an org member
        /// </summary>
        /// <param name="org"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static int OrganizationMember_Create( OrganizationMember orgMbr, ref string statusMessage )
        {

            EFDAL.Organization_Member entity = new EFDAL.Organization_Member();
            using ( var context = new EFDAL.GatewayContext() )
            {
                OrganizationMember_FromMap( orgMbr, entity );

                entity.Created = System.DateTime.Now;
                entity.LastUpdatedById = orgMbr.CreatedById;
                entity.LastUpdated = System.DateTime.Now;

                context.Organization_Member.Add( entity );

                // submit the change to database
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    statusMessage = "Successful";
                    if ( orgMbr.MemberRoles != null && orgMbr.MemberRoles.Count > 0 )
                        OrganizationMemberRoles_Create( orgMbr.MemberRoles, ref statusMessage );

                    return entity.Id;
                }
                else
                {
                    statusMessage = "Error - OrganizationMember_Create failed";
                    //?no info on error
                    return 0;
                }
            }
        }
        public static bool OrganizationMember_Update( OrganizationMember orgMbr )
        {
            bool action = false;
            using ( var context = new EFDAL.GatewayContext() )
            {
                EFDAL.Organization_Member entity = context.Organization_Member.SingleOrDefault( s => s.Id == orgMbr.Id );
                if ( entity != null )
                {
                    OrganizationMember_FromMap( orgMbr, entity );
                    entity.LastUpdated = System.DateTime.Now;

                    context.SaveChanges();
                    action = true;
                }
                else
                {
                    //should there be a check for add, just in case?
                }
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
            using ( var context = new EFDAL.GatewayContext() )
            {
                EFDAL.Organization_Member entity = context.Organization_Member
                        .SingleOrDefault( s => s.Id == id );
                if ( entity != null )
                {
                    orgMbr = OrganizationMember_ToMap( entity );
                    //fill roles
                    OrganizationMember_FillRoles( orgMbr );
                }
            }
            return orgMbr;
        }//
        /// <summary>
        /// retrieve an org mbr by orgId and userId
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static OrganizationMember OrganizationMember_Get( int orgId, int userId )
        {
            OrganizationMember orgMbr = new OrganizationMember();
            using ( var context = new EFDAL.GatewayContext() )
            {
                EFDAL.Organization_Member entity = context.Organization_Member
                        .SingleOrDefault( s => s.OrgId == orgId && s.UserId == userId );
                if ( entity != null )
                {
                    orgMbr = OrganizationMember_ToMap( entity );
                    OrganizationMember_FillRoles( orgMbr );
                }
            }
            return orgMbr;
        }//

        /// <summary>
        /// Fill the organization roles for an org member
        /// </summary>
        /// <param name="orgMbr"></param>
        public static void OrganizationMember_FillRoles( OrganizationMember orgMbr )
        {
            //fill roles
            using ( var context = new EFDAL.GatewayContext() )
            {
                List<EFDAL.Organization_MemberRole> roles = context.Organization_MemberRole
                                .Include( "Codes_OrgMemberRole" )
                                .Where( s => s.OrgMemberId == orgMbr.Id )
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
                        r.OrgMemberId = ( int ) role.OrgMemberId;

                        orgMbr.MemberRoles.Add( r );
                    }
                }
            }
        }//
        public static List<OrganizationMember> OrganizationMember_GetAll( int orgId )
        {
            OrganizationMember orgMbr = new OrganizationMember();
            List<OrganizationMember> mbrs = new List<ILPathways.Business.OrganizationMember>();
            using ( var context = new EFDAL.GatewayContext() )
            {

                List<EFDAL.Organization_MemberSummary> list = context.Organization_MemberSummary
                        .Where( s => s.OrgId == orgId )
                        .ToList();

                if ( list != null && list.Count > 0 )
                {
                    foreach ( EFDAL.Organization_MemberSummary efom in list )
                    {
                        orgMbr = OrganizationMember_ToMap( efom );
                        mbrs.Add( orgMbr );
                    }
                }
            }

            return mbrs;
        }//
        public static List<OrganizationMember> OrganizationMember_GetPending( int orgId )
        {
            OrganizationMember orgMbr = new OrganizationMember();
            List<OrganizationMember> mbrs = new List<ILPathways.Business.OrganizationMember>();
            using ( var context = new EFDAL.GatewayContext() )
            {

                List<EFDAL.Organization_MemberSummary> list = context.Organization_MemberSummary
                        .Where( s => s.OrgId == orgId && s.OrgMemberTypeId == 0)
                        .ToList();

                if ( list != null && list.Count > 0 )
                {
                    foreach ( EFDAL.Organization_MemberSummary efom in list )
                    {
                        orgMbr = OrganizationMember_ToMap( efom );
                        mbrs.Add( orgMbr );
                    }
                }
            }

            return mbrs;
        }//
        public static bool DoesUserHaveOrgId( IWebUser user, int orgId )
        {
            bool hasOrg = false;
            if ( user.OrgId == orgId )
                return true;
            //always do a fill/check
            OrganizationBizService.FillUserOrgsMbrs( user );
            
            foreach ( OrganizationMember org in user.OrgMemberships )
            {
                if ( orgId == org.Id )
                {
                    hasOrg = true;
                    break;
                }
            }
            return hasOrg;
        }   //

        /// <summary>
        /// Fill all orgMbr items for a user. 
        /// If user has an orgId, check if an orgMbr exists. If not found, add an employee orgMbr
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static void FillUserOrgsMbrs( IWebUser user )
        {

            if ( user.OrgMemberships == null
                || ( user.LastOrgMbrCheckDate != null && ( user.LastOrgMbrCheckDate.Hour + 1 ) < DateTime.Now.Hour )
                )
            {
                List<OrganizationMember> orgs = OrganizationMember_GetUserOrgs( user.Id );
                if ( user.OrgId > 0 )
                {
                    //check if the user.OrgId (primary org), is in the list
                    bool hasOrg = false;
                    if ( orgs == null )
                        orgs = new List<OrganizationMember>();
                    //check if included in orgs
                    foreach ( OrganizationMember mbr in orgs )
                    {
                        if ( mbr.OrgId == user.OrgId )
                        {
                            hasOrg = true;
                            break;
                        }
                    }
                    if ( hasOrg == false )
                    {
                        //primary orgId was not in list (this should not happen!), so add it
                        OrganizationMember om = new OrganizationMember();
                        om.OrgId = user.OrgId;
                        om.UserId = user.Id;
                        //for now just default to an employee
                        om.OrgMemberTypeId = OrganizationMember.MEMBERTYPE_EMPLOYEE;
                        om.MemberRoles = new List<OrganizationMemberRole>();

                        orgs.Add( om );
                    }
                }
                user.LastOrgMbrCheckDate = DateTime.Now;
                user.OrgMemberships = orgs;
            }
            
            
        }//

        /// <summary>
        /// return true if related user is a member of an Isle approved org. Excludes students (typeId = 3)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool IsStaffMemberOfAnIsleOrg( int userId )
        {
            bool isMemberOfIsleOrg = false;
            using ( var context = new EFDAL.GatewayContext() )
            {
                List<EFDAL.Organization_MemberSummary> list = context.Organization_MemberSummary
                            .Where( s => s.IsIsleMember == true && s.UserId == userId && s.OrgMemberTypeId != 3 )
                            .ToList();
                if ( list != null && list .Count > 0)
                {
                    isMemberOfIsleOrg = true;
                }
            }
            return isMemberOfIsleOrg;
        }//

        /// <summary>
        /// return true if related user is a member of an Isle approved org (any member type)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool IsMemberOfAnIsleOrg( int userId )
        {
            bool isMemberOfIsleOrg = false;
            using ( var context = new EFDAL.GatewayContext() )
            {
                List<EFDAL.Organization_MemberSummary> list = context.Organization_MemberSummary
                            .Where( s => s.IsIsleMember == true && s.UserId == userId )
                            .ToList();
                if ( list != null && list.Count > 0 )
                {
                    isMemberOfIsleOrg = true;
                }
            }
            return isMemberOfIsleOrg;
        }//


        /// <summary>
        /// retrieve list of all orgs for user.
        /// Member roles for the org will be included
        /// ***need to ensure profile orgId is included, until properly covered
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<OrganizationMember> OrganizationMember_GetUserOrgs( int userId )
        {
            List<OrganizationMember> list = new List<OrganizationMember>();
            if ( userId == 0 )
                return list;

            string pFilter = string.Format( " UserId = {0} ", userId );
            string pOrderBy = "";
            int pStartPageIndex = 0;
            //just in case, limit list - use different method if need all for a search, etc
            int pMaximumRows = 1000;
            int pTotalRows = 0;

            list = OrganizationMember_Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, true, ref pTotalRows );

            return list;
        }

        /// <summary>
        /// retrieve list of orgs where user has administer rights
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<OrganizationMember> OrganizationMember_GetAdminOrgs( int userId )
        {
            string pFilter = string.Format( "OrgMbrId in (select OrgMemberId from [dbo].[Organization.MemberRole]  base inner join [organization.Member] om on base.OrgMemberId = om.Id   where om.UserId = {0} AND roleId in (1, 3)) ", userId );
            string pOrderBy = "";
            int pStartPageIndex = 0;
            //just in case, limit list - use different method if need all for a search, etc
            int pMaximumRows = 1000;
            int pTotalRows = 0;

            List<OrganizationMember> list = OrganizationMember_Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, true, ref pTotalRows );

            return list;
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

            List<OrganizationMember> list = OrganizationMemberManager.SearchAsList( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );

            using ( var context = new EFDAL.GatewayContext() )
            {
                //?? get all the roles for each one, or include an eagar parameter?
                if ( includeMbrRoles && list != null && list.Count > 0 )
                {
                    foreach ( OrganizationMember item in list )
                    {
                        List<EFDAL.Organization_MemberRole> roles = context.Organization_MemberRole
                                    .Include( "Codes_OrgMemberRole" )
                                    .Where( s => s.OrgMemberId == item.Id )
                                    .ToList();
                        if ( roles.Count > 0 )
                        {
                            item.MemberRoles = new List<ILPathways.Business.OrganizationMemberRole>();
                            foreach ( EFDAL.Organization_MemberRole role in roles )
                            {
                                OrganizationMemberRole r = new OrganizationMemberRole();
                                r.Id = role.Id;
                                r.RoleId = ( int ) role.RoleId;
                                r.RoleTitle = role.Codes_OrgMemberRole.Title;
                                r.OrgMemberId = ( int ) role.OrgMemberId;

                                item.MemberRoles.Add( r );
                            }
                        }
                    }
                }
            }
            return list;
        }

        public static void OrganizationMember_FromMap( OrganizationMember fromEntity, EFDAL.Organization_Member to )
        {

            //EFDAL.Organization_Member to = new EFDAL.Organization_Member();
            to.Id = fromEntity.Id;
            to.OrgId = fromEntity.OrgId;
            to.UserId = fromEntity.UserId;
            to.OrgMemberTypeId = fromEntity.OrgMemberTypeId;

            to.Created = fromEntity.Created;
            to.CreatedById = fromEntity.CreatedById;
            to.LastUpdated = fromEntity.LastUpdated;
            to.LastUpdatedById = fromEntity.LastUpdatedById;
            //return to;
        }

        public static OrganizationMember OrganizationMember_ToMap( EFDAL.Organization_Member fromEntity )
        {
            OrganizationMember to = new OrganizationMember();
            to.Id = fromEntity.Id;
            to.OrgId = fromEntity.OrgId;
            to.UserId = fromEntity.UserId;
            to.OrgMemberTypeId = ( int ) fromEntity.OrgMemberTypeId;

            to.Created = fromEntity.Created;
            if ( fromEntity.CreatedById != null )
                to.CreatedById = ( int ) fromEntity.CreatedById;
            
            to.LastUpdated = fromEntity.LastUpdated == null ? to.DefaultDate : ( System.DateTime ) fromEntity.LastUpdated;
            if ( fromEntity.LastUpdatedById != null )
                to.LastUpdatedById = ( int ) fromEntity.LastUpdatedById;

            return to;
        }

        public static OrganizationMember OrganizationMember_ToMap( EFDAL.Organization_MemberSummary fromEntity )
        {
            OrganizationMember to = new OrganizationMember();
            to.Id = fromEntity.OrgMbrId;
            to.OrgId = fromEntity.OrgId;
            to.UserId = fromEntity.UserId;
            to.FirstName = fromEntity.FirstName;
            to.LastName = fromEntity.LastName;
            to.ImageUrl = fromEntity.ImageUrl;
            to.UserProfileUrl = fromEntity.UserProfileUrl;

            to.OrgMemberTypeId = ( int ) fromEntity.OrgMemberTypeId;

            to.Created = fromEntity.MemberAdded;
            if ( fromEntity.CreatedById != null )
                to.CreatedById = ( int ) fromEntity.CreatedById;

            to.LastUpdated = fromEntity.LastUpdated == null ? to.DefaultDate : ( System.DateTime ) fromEntity.LastUpdated;
            if ( fromEntity.LastUpdatedById != null )
                to.LastUpdatedById = ( int ) fromEntity.LastUpdatedById;

            return to;
        }

        #endregion

        #region === organization member roles
        public static bool OrganizationMemberRoles_Create( List<OrganizationMemberRole> roles, ref string statusMessage )
        {
            bool isValid = true;
            foreach ( OrganizationMemberRole role in roles )
            {
                //error handling???
                OrganizationMemberRole_Create( role, ref statusMessage );
            }

            return isValid;
        }
       
        public static int OrganizationMemberRole_Create( OrganizationMemberRole orgMbrRole, ref string statusMessage )
        {
            if ( orgMbrRole.RoleId == 0 || orgMbrRole.OrgMemberId == 0 )
            {
                statusMessage = "Error - incomplete organization member role";
                return 0;
            }

            EFDAL.Organization_MemberRole entity = new EFDAL.Organization_MemberRole();
            using ( var context = new EFDAL.GatewayContext() )
            {
                entity.OrgMemberId = orgMbrRole.OrgMemberId;
                entity.RoleId = orgMbrRole.RoleId;
                entity.CreatedById = orgMbrRole.CreatedById;
                entity.Created = System.DateTime.Now;

                context.Organization_MemberRole.Add( entity );

                // submit the change to database
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    statusMessage = "Successful";
                    orgMbrRole.Id = entity.Id;
                    return entity.Id;
                }
                else
                {
                    statusMessage = "Error - OrganizationMemberRole_Create failed";
                    //?no info on error
                    return 0;
                }
            }
        }

        public static bool OrganizationMemberRole_Delete( int orgMbrRoleId, ref string statusMessage )
        {
            bool isValid = false;
            if ( orgMbrRoleId == 0  )
            {
                statusMessage = "Error - invalid organization member role id";
                return false;
            }

            using ( var context = new EFDAL.GatewayContext() )
            {
                EFDAL.Organization_MemberRole entity = context.Organization_MemberRole
                        .SingleOrDefault( s => s.Id == orgMbrRoleId );
                if ( entity != null )
                {
                    context.Organization_MemberRole.Remove( entity );
                    context.SaveChanges();
                    isValid = true;
                }
            }

            return isValid;
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

        public static bool DoesUserHavePublishPrivileges( ThisUser user )
        {
            //
            ApplicationRolePrivilege privileges = SecurityManager.GetGroupObjectPrivileges( user, SecurityManager.CAN_PUBLISH_OBJECT );
            if ( privileges.CanCreate() == true )
                return true;
            else
                return false;
        }
        #endregion


        #region === codes
        public static List<CodeItem> OrgType_Select()
        {
            CodeItem ci = new CodeItem();
            List<CodeItem> list = new List<CodeItem>();
            using ( var context = new EFDAL.GatewayContext() )
            {
                List<EFDAL.Codes_OrgType> codes = context.Codes_OrgType.OrderBy( s => s.Id ).ToList();

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
            }
            return list;
        }//
        /// <summary>
        /// Select organization member types and return as CodeItem list
        /// </summary>
        /// <returns></returns>
        public static List<CodeItem> OrgMemberType_Select()
        {
            CodeItem ci = new CodeItem();
            List<CodeItem> list = new List<CodeItem>();
            using ( var context = new EFDAL.GatewayContext() )
            {
                //TODO - add isActive
                List<EFDAL.Codes_OrgMemberType> codes = context.Codes_OrgMemberType
                            .Where( s => s.Id > 0 )
                            .OrderBy( s => s.Id ).ToList();

                if ( codes != null && codes.Count > 0 )
                {
                    foreach ( EFDAL.Codes_OrgMemberType item in codes )
                    {
                        ci = new CodeItem();
                        ci.Id = item.Id;
                        ci.Title = item.Title;
                        list.Add( ci );
                    }
                }
            }
            return list;
        }//

        public static List<CodeItem> OrgMemberRole_Select()
        {
            CodeItem ci = new CodeItem();
            List<CodeItem> list = new List<CodeItem>();
            using ( var context = new EFDAL.GatewayContext() )
            {
                List<EFDAL.Codes_OrgMemberRole> codes = context.Codes_OrgMemberRole.OrderBy( s => s.Id ).ToList();

                if ( codes != null && codes.Count > 0 )
                {
                    foreach ( EFDAL.Codes_OrgMemberRole item in codes )
                    {
                        ci = new CodeItem();
                        ci.Id = item.Id;
                        ci.Title = item.Title;
                        list.Add( ci );
                    }
                }
            }
            return list;
        }//

        public static List<CodeItem> States_Select()
        {
            CodeItem ci = new CodeItem();
            List<CodeItem> list = new List<CodeItem>();
            using ( var context = new EFDAL.GatewayContext() )
            {
                List<EFDAL.Codes_State> codes = context.Codes_State.OrderBy( s => s.StateCode ).ToList();

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
            }
            return list;
        }//
        #endregion
    }
}
