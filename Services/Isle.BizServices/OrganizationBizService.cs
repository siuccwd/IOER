using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

//using EFDAL = GatewayBusinessEntities;
using EFDAL = IoerContentBusinessEntities;
using ILPathways.Business;
using ILPathways.Common;
using ILPathways.DAL;
using ILPathways.Utilities;
using ThisUser = LRWarehouse.Business.Patron;
using Isle.DTO;

namespace Isle.BizServices
{
    public class OrganizationBizService : ServiceHelper
    {
       // static EFDAL.GatewayContext context = new EFDAL.GatewayContext();

        #region ======= EF Core
		/// <summary>
		/// This should be used with caution. Would delete all members, libraries, etc. We could leave off RI, so will fail if any associated rows, like members, exist.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
        public bool Organization_Delete( int id, ref string statusMessage )
        {
            bool action = false;
            try
            {
                using ( var context = new EFDAL.GatewayContext() )
                {
                    EFDAL.Organization entity = context.Organizations.SingleOrDefault( s => s.Id == id );
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

        public int Organization_Create( Organization org, ref string statusMessage )
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
                        return entity.Id;
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

		public bool Organization_SetInactive( int orgId, int updatedById )
		{
			bool action = false;
			try
			{
				using ( var context = new EFDAL.GatewayContext() )
				{
					EFDAL.Organization entity = context.Organizations.SingleOrDefault( s => s.Id == orgId );
					if ( entity != null )
					{
						entity.LastUpdated = System.DateTime.Now;
						entity.LastUpdatedById = updatedById;
						entity.IsActive = false;

						context.SaveChanges();
						action = true;

						//inactivate libraries
						new EFDAL.EFLibraryManager().Library_SetOrgLibsInactive( orgId, updatedById );
						//next?

						//perhaps some notification
					}
				}
			}
			catch ( Exception ex )
			{
				LogManager.LogError( ex, string.Format( "OrganizationBizService.Organization_SetInactive( orgId: {0})", orgId ) );
			}

			return action;
		}
        public bool Organization_Update( Organization org )
        {
            bool action = false;
            try
            {
                using ( var context = new EFDAL.GatewayContext() )
                {
                    EFDAL.Organization entity = context.Organizations.SingleOrDefault( s => s.Id == org.Id );
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
			return Get( id );
        }

        /// <summary>
        /// Retrieve an organization
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Organization Get( int id )
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
                        .SingleOrDefault( s => s.Id == id );
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

		/// <summary>
		/// Check if user has an email domain for an existing org. If found, and user is not already a member, add as an employee
		/// </summary>
		/// <param name="user"></param>
        public void AssociateUserWithOrg( ThisUser user )
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

		/// <summary>
		/// Associate user with a specific org - typically used when doing auto associations at a conference
		/// </summary>
		/// <param name="user"></param>
		/// <param name="orgId"></param>
		public void AssociateUserWithOrg(ThisUser user, int orgId)
		{
			int defaultMemberTypeId = OrganizationMember.MEMBERTYPE_EXTERNAL;
			AssociateUserWithOrg(user, orgId, defaultMemberTypeId);
		}
		/// <summary>
		/// Associate user with a specific org - typically used when doing auto associations at a conference
		/// </summary>
		/// <param name="user"></param>
		/// <param name="orgId"></param>
		/// <param name="defaultMemberTypeId">Typically will use 4-external/contractor</param>
		public void AssociateUserWithOrg(ThisUser user, int orgId, int defaultMemberTypeId)
		{
			string statusMessage = "";
			if (user == null || user.Id < 1)
				return;

			try
			{
				Organization org = Get(orgId);
				if (org != null && org.Id > 0)
				{
					//while normally done for a new user, should do a check to ensure not already a member
					if (DoesUserHaveOrgId(user, org.Id) == false)
					{

						int id = OrganizationMember_Create(org.Id, user.Id, defaultMemberTypeId, user.Id, ref statusMessage);
						//note - only handle Profile, if type is employee
						if (id > 0 
							&& user.OrgId == 0 
							&& defaultMemberTypeId == OrganizationMember.MEMBERTYPE_EMPLOYEE)
						{
							user.UserProfile.OrganizationId = org.Id;
							user.OrgId = org.Id;
							if (user.HasUserProfile())
							{
								new AccountServices().PatronProfile_Update(user.UserProfile);
							}
							else
							{
								user.UserProfile.UserId = user.Id;
								new AccountServices().PatronProfile_Create(user.UserProfile, ref statusMessage);
							}
						}
						//should notify someone ==> maybe only site admin, as again could be temp??
						//moot until interface, so just admin
						SendAutoOrgMbrAddNotificationEmail(user, org, ref statusMessage);
					}
				}
			}
			catch (Exception ex)
			{
				LogManager.LogError(ex, string.Format("OrganizationBizService.AssociateUserWithOrg(id: {0})", orgId));
			}
			
		}//
        private static bool SendAutoOrgMbrAddNotificationEmail( ThisUser user, Organization org, ref string statusMessage )
        {
            bool isValid = false;
            string adminEmail = UtilityManager.GetAppKeyValue( "appAdminEmail", "mparsons@siuccwd.com" );
            string fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", "mparsons@siuccwd.com" );
			string pathOrgAdmin = UtilityManager.GetAppKeyValue("path.OrgAdmin", "/Organizations/Organizations.aspx?id=");
			string url = "/Account/Login.aspx?nextUrl=" + pathOrgAdmin + org.Id.ToString();
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
                        .FirstOrDefault( s => s.EmailDomain == emailDomain );
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
            to.Id = fromEntity.Id;
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
			to.Zipcode4 = fromEntity.ZipCode4;
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
            to.Id = fromEntity.Id;
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
			to.ZipCode4 = string.IsNullOrEmpty( fromEntity.Zipcode4 ) ? "" : fromEntity.Zipcode4;
			
            to.State = string.IsNullOrEmpty( fromEntity.State ) ? "IL" : fromEntity.State;

            to.MainPhone = string.IsNullOrEmpty( fromEntity.MainPhone ) ? "" : fromEntity.MainPhone;
            to.MainExtension = string.IsNullOrEmpty( fromEntity.MainExtension ) ? "" : fromEntity.MainExtension;
            to.Fax = string.IsNullOrEmpty( fromEntity.Fax ) ? "" : fromEntity.Fax;

            to.EmailDomain = string.IsNullOrEmpty( fromEntity.EmailDomain ) ? "" : fromEntity.EmailDomain;
            to.WebSite = fromEntity.WebSite;
			to.LogoUrl = fromEntity.LogoUrl;
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
		public bool AddAdminUserForNewOrg( int orgId, int userId, ref string statusMessage )
		{
			bool isValid = true;
			string addAdminFailedMsg = "<h2>Attempt to add admin user to new org failed.</h2><p>OrgId: {0}<br />UserId: {1}</p><p>{2}</p>";
			int mbrId = OrganizationMember_Create( orgId, userId, 1, userId, ref statusMessage );
			if ( mbrId > 0 )
			{
				//add roles
				List<OrganizationMemberRole> roles = new List<OrganizationMemberRole>();
				OrganizationMemberRole role = new OrganizationMemberRole();
				role.OrgMemberId = mbrId;
				role.RoleId = OrganizationMember.MEMBERROLE_ADMINISTRATOR;
				role.CreatedById = userId;
				roles.Add( role );

				role = new OrganizationMemberRole();
				role.OrgMemberId = mbrId;
				role.RoleId = OrganizationMember.MEMBERROLE_CONTENT_APPROVER;
				role.CreatedById = userId;
				roles.Add( role );

				role = new OrganizationMemberRole();
				role.OrgMemberId = mbrId;
				role.RoleId = OrganizationMember.MEMBERROLE_LIBRARY_ADMIN;
				role.CreatedById = userId;
				roles.Add( role );

				role = new OrganizationMemberRole();
				role.OrgMemberId = mbrId;
				role.RoleId = OrganizationMember.MEMBERROLE_ACCOUNT_ADMIN;
				role.CreatedById = userId;
				roles.Add( role );

				OrganizationMemberRoles_Create( roles, ref statusMessage );
			}
			else
			{
				isValid = false;
				string message = string.Format( addAdminFailedMsg, orgId, userId, statusMessage );
				EmailManager.NotifyAdmin( "OrganizationMgmt.AddAdminUser - failed", message );
				statusMessage = "Sorry, the step to add you as the administrator for this new organization was not successful. System administration has been notified. ";
			}

			return isValid;
		}
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
        public int OrganizationMember_Create( int orgId, int userId, int createdById, ref string statusMessage )
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
        public int OrganizationMember_Create( int orgId, int userId, int orgMbrTypeId, int createdById, ref string statusMessage )
        {
			if (orgId < 1 || userId < 1)
				return 0;

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
        public int OrganizationMember_Create( OrganizationMember orgMbr, ref string statusMessage )
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
        public bool OrganizationMember_Update( OrganizationMember orgMbr )
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
                            .Where( s => s.IsIsleMember == true 
								&& s.UserId == userId 
								&& s.OrgMemberTypeId != 3 )
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

            string pFilter = string.Format( " IsActive = 1 AND UserId = {0} ", userId );
            string pOrderBy = "";
            int pStartPageIndex = 0;
            //just in case, limit list - use different method if need all for a search, etc
            int pMaximumRows = 1000;
            int pTotalRows = 0;

            list = OrganizationMember_Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, true, ref pTotalRows );

            return list;
        }

        /// <summary>
        /// return true if user is an administrator or account administrator for any org
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool IsAnyOrgAdministrator(int userId)
        {
            bool isAdmin = false;
            using (var context = new EFDAL.GatewayContext())
            {
                List<EFDAL.OrganizationMember_RoleIdCSV> list = context.OrganizationMember_RoleIdCSV
                            .Where(s => s.UserId == userId && (s.IsAdmin == 1 || s.IsAccountAdmin == 1))
                            .ToList();
                if (list != null && list.Count > 0)
                {
                    isAdmin = true;
                }
            }
            return isAdmin;
        }//
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
        /// Return all where user can contribute content
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<OrganizationMember> OrganizationMembers_WithContentPrivileges( int userId )
        {
            string pFilter = string.Format( "UserId = {0} AND OrgMbrId in (SELECT omr.OrgMemberId  From [Organization.MemberRole] omr where  omr.RoleId in (1,2,5) ) ", userId );
            string pOrderBy = "";
            int pStartPageIndex = 0;
            //just in case, limit list - use different method if need all for a search, etc
            int pMaximumRows = 1000;
            int pTotalRows = 0;

            OrganizationMember code = new OrganizationMember();

            List<OrganizationMember> list = OrganizationMemberManager.SearchAsList( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
            if ( list != null && list.Count > 0 )
            {
                //inject 
                code = new OrganizationMember();
                code.Id = 0;
                code.Organization = "Select Organization (optional)";
                list.Insert(0, code );
            }
            else
            {
                //nothing. interface will handle
            }
            return list;
        }

        public static List<CodeItem> OrganizationMembersCodes_WithContentPrivileges( int userId )
        {
            string pFilter = string.Format( "UserId = {0} AND OrgMbrId in (SELECT omr.OrgMemberId  From [Organization.MemberRole] omr where  omr.RoleId in (1,2,5) ) ", userId );
            string pOrderBy = "";
            int pStartPageIndex = 0;
            //just in case, limit list - use different method if need all for a search, etc
            int pMaximumRows = 1000;
            int pTotalRows = 0;

            List<CodeItem> list = new List<CodeItem>();
            CodeItem code = new CodeItem();
            DataSet ds = OrganizationMemberManager.Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                code = new CodeItem();
                code.Id = 0;
                code.Title = "Select Organization (optional)";
                code.Description = "";
                code.WarehouseTotal = 0;
                code.SortOrder = 10;
                list.Add( code );

                foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
                {
                    code = new CodeItem();
                    code.Id = GetRowColumn( dr, "OrgId", 0 );
                    code.Title = GetRowColumn( dr, "Organization", "Missing" );
                    code.Description = "";
                    code.WarehouseTotal = 0;
                    code.SortOrder = 10;
                    list.Add( code );
                } //end foreach
            }

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
        /// 
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
			to.Email = fromEntity.Email;

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


        /// <summary>
        /// Retrieve cross tab of organization member type totals
        /// </summary>
        /// <param name="includeOrphanCount">True - also show row for users with no organization affiliation</param>
        /// <returns></returns>
        public static List<HierarchyActivityRecord> OrganizationMember_Crosstab( bool includeOrphanCount)
        {

            List<HierarchyActivityRecord> list = new List<HierarchyActivityRecord>();
            HierarchyActivityRecord entity = new HierarchyActivityRecord();
            ActivityCount activityCount = new ActivityCount();

            using ( var context = new EFDAL.GatewayContext() )
            {
                List<EFDAL.Organization_MemberCrosstab> items = context.Organization_MemberCrosstab
                                    .Where( s => ( int ) s.OrgId > 0 
                                            || ( includeOrphanCount == true ) )
                                    .ToList();
                if ( items.Count > 0 )
                {
                    int cntr = 0;
                    foreach ( EFDAL.Organization_MemberCrosstab item in items )
                    {
                        cntr++;
                        entity = new HierarchyActivityRecord();

                        activityCount = new ActivityCount();
                        activityCount.Id = (int) item.OrgId;
                        activityCount.Title = item.Organization;

                        Activties_AddItem( activityCount, ( int ) item.Administrator, "administrator" );
                        Activties_AddItem( activityCount, ( int ) item.Employee, "employee" );
                        Activties_AddItem( activityCount, ( int ) item.Student, "student" );
                        Activties_AddItem( activityCount, ( int ) item.External, "external" );
                        Activties_AddItem( activityCount, ( int ) item.Other, "other" );
                        Activties_AddItem( activityCount, ( int ) item.All, "line_total" );

                        entity.Activity = activityCount;
                        //entity.ChildrenActivity.Add( activityCount );

                        list.Add( entity );
                    }
                }
            }

            return list;
        }
        private static void Activties_AddItem( ActivityCount activityCount,
                    int views, 
                    string label )
        {

            //int views = GetRowColumn( dr, title, 0 );
            var cids = new List<int>() { views };
            activityCount.Activities.Add( label, cids );
        }
        #endregion

        #region === organization member roles
        public bool OrganizationMemberRoles_Create( List<OrganizationMemberRole> roles, ref string statusMessage )
        {
            bool isValid = true;
            foreach ( OrganizationMemberRole role in roles )
            {
                //error handling???
                OrganizationMemberRole_Create( role, ref statusMessage );
            }

            return isValid;
        }
       
        public int OrganizationMemberRole_Create( OrganizationMemberRole orgMbrRole, ref string statusMessage )
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

        public bool OrganizationMemberRole_Delete( int orgMbrRoleId, ref string statusMessage )
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
            try
            {
                using (var context = new EFDAL.GatewayContext())
                {
                    //TODO - add isActive ????
                    List<EFDAL.Codes_OrgMemberType> codes = context.Codes_OrgMemberType
                                .Where(s => s.Id > 0 && s.IsActive == true)
                                .OrderBy(s => s.Id).ToList();

                    if (codes != null && codes.Count > 0)
                    {
                        foreach (EFDAL.Codes_OrgMemberType item in codes)
                        {
                            ci = new CodeItem();
                            ci.Id = item.Id;
                            ci.Title = item.Title;
                            ci.IsActive = item.IsActive != null ? (bool)item.IsActive : true;
                            list.Add(ci);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogError(ex, "OrganizationBizService.OrgMemberType_Select()");
            }
            return list;
        }//

        public static List<CodeItem> OrgMemberRole_Select()
        {
            CodeItem ci = new CodeItem();
            List<CodeItem> list = new List<CodeItem>();
            try
            {
                using (var context = new EFDAL.GatewayContext())
                {
                    List<EFDAL.Codes_OrgMemberRole> codes = context.Codes_OrgMemberRole
							.Where(s => s.IsActive == true)
							.OrderBy(s => s.Id)
							.ToList();

                    if (codes != null && codes.Count > 0)
                    {
                        foreach (EFDAL.Codes_OrgMemberRole item in codes)
                        {
                            ci = new CodeItem();
                            ci.Id = item.Id;
                            ci.Title = item.Title;
                            ci.IsActive = item.IsActive != null ? (bool)item.IsActive : true;
                            list.Add(ci);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogError(ex, "OrganizationBizService.OrgMemberRole_Select()");
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

      #region WIP methods 
        //These methods aren't truly implemented yet, but serve as placeholders
        //They don't necessarily need to be static if it helps to do them normally
        //I'm also not committed to the method/variable names below, so feel free to adjust them to fit your desired pattern/schema

        //Deny a pending membership
        //Note: userId is the ID of the user performing the denial
        //Note: customMessage is sent to the denied member, presumably to indicate why they were denied. We don't -have- to implement this part.
        //Should return a bool indicating whether or not the denial was successful, and a status message explaining any failure. 
        //The status message will be hidden from the user but findable to us for debugging purposes.
        public static bool OrganizationMember_DenyPending( int organizationId, int userId, int pendingMemberId, string customMessage, ref string status )
        {
            throw new NotImplementedException( "Sorry, denying memberships is not implemented yet." );
        }

        //Invite an existing IOER user
        public bool InviteExistingUser( int organizationId, int actingUserId, int inviteeId, int typeId, List<int> roleIds, string customMessage, ref string status )
        {
					//Check for existing
					var testUser = OrganizationMember_Get( organizationId, inviteeId );
					if ( testUser != null && testUser.Id > 0 )
					{
						status = "That user is already a member of this organization.";
						return false;
					}

					//Get the user
					var invitedUser = AccountServices.GetUser( inviteeId );
					//Setup the member object
					var member = new OrganizationMember()
					{
						UserId = invitedUser.Id,
						OrgId = organizationId,
						OrgMemberTypeId = typeId,
						LastUpdatedById = actingUserId,
						CreatedById = actingUserId
					};

					//Create the membership
					var memberID = OrganizationMember_Create( member, ref status );
					if ( memberID == 0 )
					{
						//Status should already be set by the above method
						return false;
					}
					member.Id = memberID;

					//Assign roles to the new member
					var roles = OrgMemberRole_Select();
					foreach ( var item in roleIds )
					{
						var targetRole = roles.Where( m => m.Id == item ).FirstOrDefault();
						if ( targetRole != null )
						{
							var role = new OrganizationMemberRole()
							{
								RoleId = targetRole.Id,
								OrgMemberId = member.Id,
								CreatedById = actingUserId
							};
							OrganizationMemberRole_Create( role, ref status );
						}
					}

					//Notify the member of the addition - this is hacky due to time constraints
					var actingUser = AccountServices.GetUser( actingUserId );
					var organization = Get( organizationId );
					var typeTitle = "member";
					var memberType = OrgMemberType_Select().Where( m => m.Id == typeId ).FirstOrDefault();
					if ( memberType != null )
					{
						typeTitle = memberType.Title;
					}
					string bcc = UtilityManager.GetAppKeyValue( "systemAdminEmail", "mparsons@siuccwd.com" );
					string fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", "mparsons@siuccwd.com" );
					var isSecure = false;
					string eMessage = "";
					if ( UtilityManager.GetAppKeyValue( "SSLEnable", "0" ) == "1" )
						isSecure = true;
					string proxyId = new AccountServices().Create_ProxyLoginId( invitedUser.Id, "Existing user added to org.", ref status );
					//Hacks - need to move these to somewhere less hardcoded-y when time allows
					string subject = string.Format( "{0} added you to an \"Illinois Open Educational Resources\" organization", actingUser.FullName() );
					string confirmUrl = string.Format( "/Account/Login.aspx?pg={0}&nextUrl=/Libraries", proxyId.ToString() );
					confirmUrl = UtilityManager.FormatAbsoluteUrl( confirmUrl, isSecure );
					eMessage = string.Format( "Note<br />You have been added to the following organization: <br />Organization: <strong>{0}</strong> <br />Member Type: <strong>{1}</strong>. <br /><br />" + customMessage + "<br /><br /><p>Use the link below to login to the website. You will then have access to the latter organization.</p> <a href=\"{2}\">Login to IOER</a>", organization.Name, memberType.Title, confirmUrl );
					eMessage += " <p>" + actingUser.EmailSignature() + "</p>";
					EmailManager.SendEmail( invitedUser.Email, fromEmail, subject, eMessage, "", bcc );

					//Update the user's profile if applicable
					if ( invitedUser.OrgId == 0 )
					{
						var profile = new LRWarehouse.Business.PatronProfile()
						{
							UserId = invitedUser.Id,
							OrganizationId = organizationId
						};
						if ( profile.IsValid ) // Not sure what this check actually does
						{
							new AccountServices().PatronProfile_Update( profile );
						}
						else
						{
							new AccountServices().PatronProfile_Create( profile, ref status );
						}
					}

					return true;
				}

        //Invite a non-existing user by email
        public bool InviteNewUser( int organizationId, int actingUserId, string inviteeEmail, int typeId, List<int> roleIds, string customMessage, ref string status )
        {
					var acctServices = new AccountServices();
					//Check to see if the user already exists
					var testUser = new AccountServices().GetByEmail( inviteeEmail );
					if ( testUser != null && testUser.Id > 0 )
					{
						return InviteExistingUser( organizationId, actingUserId, testUser.Id, typeId, roleIds, customMessage, ref status );
					}

          //Otherwise, create the user account
					var newUser = new LRWarehouse.Business.Patron()
					{
						FirstName = "New",
						LastName = "User",
						Email = inviteeEmail,
						CreatedById = actingUserId,
						LastUpdatedById = actingUserId,
						Password = ILPathways.Utilities.UtilityManager.Encrypt( "ChangeMe_" + System.DateTime.Now.Millisecond.ToString() )
					};
					var id = acctServices.Create( newUser, ref status );
					if ( id > 0 )
					{
						newUser.Id = id;
						return InviteExistingUser( organizationId, actingUserId, newUser.Id, typeId, roleIds, customMessage, ref status );
					}
					else
					{
						//Status should already be set
						return false;
					}
				}
        
        //Gets all organization members of a certain role
        //The current hack below accomplishes this, but is not very efficient
        public static List<OrganizationMember> OrganizationMember_GetAll( int organizationId, int roleId )
        {
          return OrganizationMember_GetAll( organizationId ).Where( m => m.OrgMemberTypeId == roleId ).ToList();
        }

      #endregion
    }
}
