using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
//using AutoMapper;

using ILPathways.Business;
using ILPathways.Common;
using ILPathways.DAL;
using ILPathways.Utilities;
using AcctSvce = Isle.BizServices.workNetAccountServices;
using PatronMgr = LRWarehouse.DAL.PatronManager;
//.use alias for easy change to Gateway
//using ThisUser = ILPathways.Business.AppUser;
//using ThisUserProfile = ILPathways.Business.AppUserProfile;
//using PatronMgr = ILPathways.DAL.PatronManager;

using LRWarehouse.Business;
using Patron = LRWarehouse.Business.Patron;
using ThisUserProfile = LRWarehouse.Business.PatronProfile;
using EFDal = IOERBusinessEntities;

using Isle.DTO;

namespace Isle.BizServices
{
    public class AccountServices : ServiceHelper
    {
        static string thisClassName = "AccountServices";
        PatronMgr myManager = new PatronMgr();
        static string SessionLoginProxy = "Session Login Proxy";

        #region Authorization

        private static bool CanUserAuthor( Patron appUser )
        {
            bool isValid = true;

            return isValid;
        }

        private static bool CanUserOrgAuthor( Patron appUser )
        {
            bool isValid = true;

            return isValid;
        }

		/// <summary>
		/// return true if user is part of the site admin group
		/// 16-02-26 mp - added static version
		/// TODO - add a session variable to save repeated calls
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static bool IsUserAnAdmin(Patron user)
		{
			string siteAdminObjectName = ServiceHelper.GetAppKeyValue("siteAdminObjectName");
			//"Site.Admin"
			return SecurityManager.GetGroupObjectPrivileges(user, siteAdminObjectName).CreatePrivilege > (int)ILPathways.Business.EPrivilegeDepth.State;
		}
		public bool IsUserAdmin(Patron user)
		{
			string siteAdminObjectName = ServiceHelper.GetAppKeyValue("siteAdminObjectName");
			//"Site.Admin"
			return SecurityManager.GetGroupObjectPrivileges(user, siteAdminObjectName).CreatePrivilege > (int)ILPathways.Business.EPrivilegeDepth.State;
		}
		public bool CanAdminUserAdminAnyLibrary()
		{
			bool adminUserCanAdminAnyLibrary = ServiceHelper.GetAppKeyValue( "adminUserCanAdminAnyLibrary", false );
			return adminUserCanAdminAnyLibrary;
		}
		
		public void SetUserAdminRole(Patron appUser)
		{
			string siteAdminObjectName = ServiceHelper.GetAppKeyValue("siteAdminObjectName");
			ApplicationRolePrivilege arp = SecurityManager.GetGroupObjectPrivileges(appUser, siteAdminObjectName);
			if (arp.CreatePrivilege > (int)EPrivilegeDepth.State ) 
			{
				//NOTE: this needs to be put back in the session!
				//actually this is set at login
				//add kludge until improved
				if ("2 22 24 56".IndexOf(appUser.Id.ToString()) > -1)
					appUser.TopAuthorization = (int)EUserRole.ProgramAdministrator;
				else
					appUser.TopAuthorization = ( int ) EUserRole.ProgramStaff;
			}
			else if (arp.CreatePrivilege == (int)EPrivilegeDepth.State)
			{
				//NOTE: this needs to be put back in the session!
				//actually this is set at login
				appUser.TopAuthorization = (int)EUserRole.StateAdministrator;
			}
		}
        #endregion
        #region  Patron methods
        #region ====== Core Methods ===============================================
        /// <summary>
        /// Delete an User record using rowId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( string pRowId, ref string statusMessage )
        {
            //TODO - chg to ws call??
            PatronMgr mgr = new PatronMgr();
            return mgr.Delete( pRowId, ref statusMessage );

        }//

        /// <summary>
        /// Add an User record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int Create( Patron entity, ref string statusMessage )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.Create( entity, ref statusMessage );

        }

        /// <summary>
        /// Update an User record
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string Update( Patron entity )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.Update( entity );

        }//


        public int PatronProfile_Create( ThisUserProfile entity, ref string statusMessage )
        {
            if ( entity.UserId > 0 )
            {
                PatronMgr mgr = new PatronMgr();
                return mgr.PatronProfile_Create( entity, ref statusMessage );
            }
            else
            {
                statusMessage = "Error - no userId was found";
                return 0;
            }

        }//
        public string PatronProfile_Update( ThisUserProfile entity )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.PatronProfile_Update( entity );

        }//
        public string PatronProfile_UpdateImage( ThisUserProfile entity )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.PatronProfile_Update( entity );

        }//
        #endregion

        #region ====== Retrieval Methods ===============================================
        /// <summary>
        /// Get User record and profile via integer id
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public Patron Get( int pId )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.Get( pId );
        }//

        /// <summary>
        /// Get user and profile
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public static Patron GetUser( int pId )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.Get( pId );
		}

		public static Patron Account_GetByExternalIdentifier( string identifier )
		{
			return EFDal.AccountManager.Account_GetByExternalIdentifier( identifier );
        }//

        /// <summary>
        /// Check if a username exists
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool DoesUserNameExist( string userName )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.DoesUserNameExist( userName );
        }//

        /// <summary>
        /// Check if an email already is associated with an account
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool DoesUserEmailExist( string email )
        {
			if ( string.IsNullOrWhiteSpace( email ) )
				return false;

            PatronMgr mgr = new PatronMgr();
            return mgr.DoesUserEmailExist( email );

        }//
        /// <summary>
        /// retrieve user by username
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public Patron GetByUsername( string userName )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.GetByUsername( userName );

        }//
        /// <summary>
        /// Get User record via rowId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public Patron GetByRowId( string pRowId )
        {
            if ( pRowId == null || pRowId.Trim().Length != 36 )
                return new Patron() { IsValid = false };

            PatronMgr mgr = new PatronMgr();
            return mgr.GetByRowId( pRowId );
        }//

        /// <summary>
        /// Retrieve by User RowId - called from UserDataService
        /// </summary>
        /// <param name="rowId"></param>
        /// <param name="encryptedPassword"></param>
        /// <returns></returns>
        public Patron GetByRowId( string rowId, ref string statusMessage )
        {
            Patron user = new Patron();
            string message = "There was an error while processing the request.";

            try
            {
                user = myManager.GetByRowId( rowId );
				if (user.Id == 0 || !user.IsValid)
				{
					user.IsValid = false;
					user.Id = 0;
				}
            }
            catch ( Exception ex )
            {
                ServiceHelper.LogError( "AccountServices.GetByRowId(rowId, ref string statusMessage): " + ex.ToString() );
                statusMessage = message;
                return null;
            }

            return user;
        } //

        /// <summary>
        /// Get user using the temp proxyId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public Patron GetByProxyRowId( string proxyId, ref string statusMessage  )
        {
            if ( proxyId == null || proxyId.Trim().Length != 36 )
            {
                statusMessage = "Error: invalaid request";
                return new Patron() { IsValid = false };
            }

            Patron user = GetUserFromProxy( proxyId, ref statusMessage );
            return user;
        }//

        /// <summary>
        /// Get User record via email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Patron GetByEmail( string email )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.GetByEmail( email.Trim() );

        }//

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="loginName">Can be either email address or user name</param>
        /// <param name="password"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public Patron Login( string loginName, string password, ref string statusMessage )
        {
            return Authorize( loginName, password, ref statusMessage );
        }//

        /// <summary>
        /// Authorize user
        /// </summary>
        /// <param name="userName">Can be either email address or user name</param>
        /// <param name="password"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public Patron Authorize( string userName, string password, ref string statusMessage )
        {
            Patron appUser = new Patron();
            appUser.IsValid = false;

            if ( userName == null || userName.Length < 4
                || password == null || password.Length < 7
                )
            {
                statusMessage = "Some required fields are empty.";
                return appUser;
            }
            string encryptedPassword = "";
            if ( password.Length > 40 )
            {
                //already encrypted
                encryptedPassword = password;
            }
            else
            {
                encryptedPassword = ServiceHelper.Encrypt( password );
            }
            PatronMgr mgr = new PatronMgr();

            //get user
            appUser = mgr.Authorize( userName, encryptedPassword );

            if ( appUser == null || appUser.IsValid == false )
            {
                statusMessage = "Error: Login failed - Invalid credentials";
            }
            else
            {
                //add proxy
                appUser.ProxyId = Create_SessionProxyLoginId( appUser.Id, ref statusMessage );
				//handle admin users
				SetUserAdminRole(appUser);
            }

            return appUser;
        }

        public Patron RecoverPassword( string lookup )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.RecoverPassword( lookup );
        }

        /// <summary>
        /// Search for User related data using passed parameters
        /// - uses custom paging
        /// - only requested range of rows will be returned
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public List<Patron> Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, bool pOutputRelTables, ref int pTotalRows )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );


        }
        public DataSet UserSearch( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, bool pOutputRelTables, ref int pTotalRows )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.UserSearch( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );


        }

        public ThisUserProfile PatronProfile_Get( int pUserId )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.PatronProfile_Get( pUserId );

        }//


        public static List<Patron> GetAllUsers()
        {

            EFDal.ResourceEntities ctx = new EFDal.ResourceEntities();
            List<EFDal.Patron> eflist = ctx.Patrons
                            .OrderBy( s => s.LastName )
                            .ToList();
            List<Patron> list = new List<Patron>();
            if ( eflist.Count > 0 )
            {
                foreach ( EFDal.Patron item in eflist )
                {
                    Patron e = new Patron();
                    e.Id = item.Id;
                    e.FirstName = item.FirstName;
                    e.LastName = item.LastName;
                    list.Add( e );
                }
            }

            return list;

        }
        public static List<CodeItem> GetAllUsersAsCodes()
        {
            CodeItem ci = new CodeItem();

            EFDal.ResourceEntities ctx = new EFDal.ResourceEntities();

            List<EFDal.Patron> eflist = ctx.Patrons.OrderBy( s => s.LastName ).ToList();
            List<CodeItem> list = new List<CodeItem>();
            if ( eflist.Count > 0 )
            {
                foreach ( EFDal.Patron item in eflist )
                {
                    ci = new CodeItem();
                    ci.Id = item.Id;
					ci.Title = item.LastName == null ? "" : item.LastName + ", " + item.FirstName;
                    list.Add( ci );
                }
            }

            return list;

        }

		/// <summary>
		/// Determine if current user is a logged in (registered) user 
		/// </summary>
		/// <returns></returns>
		public static bool IsUserAuthenticated()
		{
			bool isUserAuthenticated = false;
			try
			{
				Patron appUser = GetUserFromSession();
				isUserAuthenticated =  IsUserAuthenticated( appUser );
			}
			catch
			{

			}

			return isUserAuthenticated;
		} //
		public static bool IsUserAuthenticated( Patron appUser )
		{
			bool isUserAuthenticated = false;
			try
			{
				if ( appUser == null || appUser.Id == 0 || appUser.IsValid == false )
				{
					isUserAuthenticated = false;
				}
				else
				{
					isUserAuthenticated = true;
				}
			}
			catch
			{

			}

			return isUserAuthenticated;
		} //
		public static Patron GetUserFromSession()
		{
			if ( HttpContext.Current.Session != null )
			{
				return GetUserFromSession( HttpContext.Current.Session );
			}
			else
				return null;
		} //

		public static Patron GetUserFromSession( HttpSessionState session )
		{
			Patron user = new Patron();
			try
			{ 		//Get the user
				user = ( Patron ) session[ Constants.USER_REGISTER ];

				if ( user.Id == 0 || !user.IsValid )
				{
					user.IsValid = false;
					user.Id = 0;
				}
			}
			catch
			{
				user = new Patron();
				user.IsValid = false;
			}
			return user;
		}
        #endregion
        #endregion

        #region patron - proxy login methods
        public string Create_SessionProxyLoginId( int userId, ref string statusMessage )
        {
            return Create_ProxyLoginId( userId, SessionLoginProxy, 1, ref statusMessage );

        }
        public string Create_RegistrationConfirmProxyLoginId( int userId, ref string statusMessage )
        {
            int expiryDays = ServiceHelper.GetAppKeyValue( "registrationConfExpiryDays", 5 );
            return Create_ProxyLoginId( userId, "Registration Immediate Confirmation", expiryDays, ref statusMessage );

        }
        public string Create_ForgotPasswordProxyLoginId( int userId, ref string statusMessage )
        {
            int expiryDays = ServiceHelper.GetAppKeyValue( "forgotPasswordExiryDays", 1 );
            return Create_ProxyLoginId( userId, "Forgot Password", expiryDays, ref statusMessage );
            
        }
        public string Create_3rdPartyAddProxyLoginId( int userId, ref string statusMessage )
        {
            int expiryDays = ServiceHelper.GetAppKeyValue( "user3rdPartyAddExpiryDays", 14 );
            return Create_ProxyLoginId( userId, "User added from org.", expiryDays, ref statusMessage );

        }
        public string Create_3rdPartyAddProxyLoginId( int userId, string proxyType, ref string statusMessage )
        {
            int expiryDays = ServiceHelper.GetAppKeyValue( "user3rdPartyAddExpiryDays", 14 );
            return Create_ProxyLoginId( userId, proxyType, expiryDays, ref statusMessage );

        }
        /// <summary>
        /// General proxy type - usually a week to handle weekends
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="proxyType"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public string Create_ProxyLoginId( int userId, string proxyType, ref string statusMessage )
        {
            int expiryDays = ServiceHelper.GetAppKeyValue( "proxyLoginExiryDays", 5 );
            return Create_ProxyLoginId( userId, proxyType, expiryDays, ref statusMessage );

        }

        public string Create_SSOProxyLoginId(int userId, ref string statusMessage)
        {
            return Create_ProxyLoginId(userId, "SSO", 1, ref statusMessage);
        }

        /// <summary>
        /// Create a proxy guid for use in auto login
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="proxyType"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
		public string Create_ProxyLoginId( int userId, string proxyType, int expiryDays, ref string statusMessage )
        {
            EFDal.System_GenerateLoginId efEntity = new EFDal.System_GenerateLoginId();
            string proxyId = "";
            try
            {
                using ( var context = new EFDal.ResourceContext() )
                {
                    efEntity.UserId = userId;
                    efEntity.ProxyId = Guid.NewGuid();
                    efEntity.Created = System.DateTime.Now;
                    if ( proxyType == SessionLoginProxy )
                    {
                        //expire at midnight - not really good for night owls
                        //efEntity.ExpiryDate = new System.DateTime( DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59 );
                        efEntity.ExpiryDate = System.DateTime.Now.AddDays( expiryDays );
                    }
                    else
                        efEntity.ExpiryDate = System.DateTime.Now.AddDays( expiryDays );

                    efEntity.IsActive = true;
                    efEntity.ProxyType = proxyType;

                    context.System_GenerateLoginId.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        statusMessage = "Successful";
                        int id = efEntity.Id;
                        return efEntity.ProxyId.ToString();
                    }
                    else
                    {
                        //?no info on error
                        return proxyId;
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".Create_ProxyLoginId()" );
                statusMessage = ex.Message;
                return proxyId;
            }
        }

        /// <summary>
        /// Retrieve userId for a proxy.
        /// Even if the proxy has been used, the userid is returned - do can check if multiple links were used for the same user/email
        /// </summary>
        /// <param name="rowId"></param>
        /// <param name="isProxyValid"></param>
        /// <param name="doingCheckOnly">If true, a valid proxy will not be inactivated</param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int GetUserIdFromProxy( string rowId, bool doingCheckOnly, ref bool isProxyValid, ref string statusMessage )
        {
            int userId = 0;
            isProxyValid = false;

            using ( var context = new EFDal.ResourceContext() )
            {
                Guid id = new Guid( rowId );

                EFDal.System_GenerateLoginId proxy = context.System_GenerateLoginId.FirstOrDefault( s => s.ProxyId == id );
                if ( proxy != null && proxy.Id > 0 )
                {
                    if ( proxy.IsActive == false )
                    {
                        statusMessage = "Error: invalid request, previously used.";
                        userId = proxy.UserId;
                    }
                    else if ( proxy.ExpiryDate != null && proxy.ExpiryDate < System.DateTime.Now )
                    {
                        statusMessage = "Error: the request has expired.";
                        userId = proxy.UserId;
                    }
                    else
                    {
                        isProxyValid = true;
                        userId = proxy.UserId;
                        if ( doingCheckOnly == false )
                        {
                            //now set inactive (or delete??) ==> assuming only reason for get is to do the login
                            InactivateProxy( rowId, ref statusMessage );
                        }
                    }

                }
                else
                {
                    statusMessage = "Error: invalid request (temp id not found).";
                }
            }

            return userId;

        }
        public Patron GetUserFromProxy( string rowId, ref string statusMessage )
        {
            Patron user = new Patron();
            if ( rowId == null || rowId.Trim().Length != 36 )
            {
                statusMessage = "Error: invalid proxy id.";
                return user;
            }
            using ( var context = new EFDal.ResourceContext() )
            {
                Guid id = new Guid( rowId );

                EFDal.System_GenerateLoginId proxy = context.System_GenerateLoginId.FirstOrDefault( s => s.ProxyId == id );
                if ( proxy != null && proxy.Id > 0 )
                {
                    if ( proxy.IsActive == false )
                    {
                        statusMessage = "Error: invalid request, this temporary login key has been previously used.";
                    }
                    else if ( proxy.ExpiryDate != null && proxy.ExpiryDate < System.DateTime.Now )
                    {
                        statusMessage = "Error: this temporary login key has expired.";
                    }
                    else
                    {
                        user = new AccountServices().Get( proxy.UserId );
                        //now set inactive (or delete??)
                        InactivateProxy( rowId, ref statusMessage );
                    }

                }
                else
                {
                    statusMessage = "Error: invalid request (temporary login key  not found).";
                }
            }

            return user;

        }
        public bool InactivateProxy( string rowId, ref string statusMessage )
        {
            bool isValid = true;
            using ( var context = new EFDal.ResourceContext() )
            {
                Guid id = new Guid( rowId );

                EFDal.System_GenerateLoginId proxy = context.System_GenerateLoginId.FirstOrDefault( s => s.ProxyId == id );
                if ( proxy != null && proxy.Id > 0 )
                {
                    proxy.IsActive = false;
                    proxy.AccessDate = System.DateTime.Now;

                    context.SaveChanges();
                }
            }

            return isValid;

        }
        #endregion
        #region === Dashboard methods

        public static DashboardDTO GetMyDashboard( int forUserId )
        {
            Patron user = GetUser( forUserId );

            return GetDashboard( user, forUserId, 0 );
        }
        public static DashboardDTO GetMyDashboard( Patron user )
        {
            return GetDashboard( user, user.Id, 0 );
        }
        public static DashboardDTO GetMyDashboard( Patron user, int maxResources )
        {
            return GetDashboard( user, user.Id, maxResources );
        }
        private static DashboardDTO GetDashboard( int forUserId, int requestedByUserId )
        {
            Patron user = GetUser( forUserId );
            return GetDashboard( user, requestedByUserId, 0 );
        }
       
        public static DashboardDTO GetDashboard( Patron user, int requestedByUserId, int maxResources )
        {
            DashboardDTO dto = new DashboardDTO();
            if ( user.Id == requestedByUserId )
                dto.isMyDashboard = true;

			if ( maxResources > 0 )
				dto.maxResources = maxResources;
			else
				dto.maxResources = GetAppKeyValue( "maxDashboardResources", 8 );

            dto.userId = user.Id;
            dto.name = user.FullName();
            dto.avatarUrl = user.ImageUrl;
            dto.description = user.UserProfile.RoleProfile;
            dto.jobTitle = user.UserProfile.JobTitle;
            dto.role = user.UserProfile.PublishingRole;
            dto.organization = user.UserProfile.Organization;
            //future link to organization page

            //library
            LibraryBizService ls = new LibraryBizService();
            ls.Library_FillDashboard( dto, user, requestedByUserId );

            //my resources
            EFDal.EFResourceManager.Resource_FillDashboard( dto );

            //my followed lib resources

            //my org/library membership lib resources


            return dto;
        }

        public static void FillDashboard( DashboardDTO dto )
        {
            if ( dto.userId == 0 )
            {
                dto.message = "Error - a userid is required";
                return;
            }
            Patron user = GetUser( dto.userId );

            FillDashboard( dto, user, dto.userId, 0 );
        }

        public static void FillDashboard( DashboardDTO dto, Patron user )
        {
            //Patron user = GetUser( forUserId );

            FillDashboard( dto, user, user.Id, 0 );
        }
        public static void FillDashboard( DashboardDTO dto, int forUserId, int requestedByUserId )
        {
            Patron user = GetUser( forUserId );
            FillDashboard( dto, user, requestedByUserId, 0 );
        }
        public static void FillDashboard( DashboardDTO dto, Patron user, int requestedByUserId, int maxResources )
        {
            //should be initialized, but just in case
            if (dto == null)
                dto = new DashboardDTO();

            if ( user.Id == requestedByUserId )
                dto.isMyDashboard = true;

            if ( maxResources > 0 && dto.maxResources == 0)
                dto.maxResources = maxResources;
			else if ( dto.maxResources == 0 )
				dto.maxResources = GetAppKeyValue( "maxDashboardResources", 8 );

            dto.userId = user.Id;
            dto.name = user.FullName();
            dto.avatarUrl = user.ImageUrl;
            dto.description = user.UserProfile.RoleProfile;
            dto.jobTitle = user.UserProfile.JobTitle;
            dto.role = user.UserProfile.PublishingRole;
            dto.organization = user.UserProfile.Organization;
            //future link to organization page

            //library
            LibraryBizService ls = new LibraryBizService();
            ls.Library_FillDashboard( dto, user, requestedByUserId );

            //my resources
            EFDal.EFResourceManager.Resource_FillDashboard( dto );

            //my followed lib resources

            //my org/library membership lib resources

        }

        #endregion
        #region === Person Following =====
        public static int Person_FollowingAdd( int pFollowingUserId, int FollowedByUserId )
        {
            return EFDal.AccountManager.PersonFollowing_Add( pFollowingUserId, FollowedByUserId );
        }//
        public static bool Person_FollowingDelete( int pFollowingUserId, int FollowedByUserId )
        {
            return EFDal.AccountManager.PersonFollowing_Delete( pFollowingUserId, FollowedByUserId );
        }//
        public static bool Person_FollowingIsMember( int pFollowingUserId, int FollowedByUserId )
        {
            return EFDal.AccountManager.PersonFollowing_IsMember( pFollowingUserId, FollowedByUserId );
        }//
        #endregion

        #region ====== workNet/external Methods ===============================================
        string codedPassword = "Niemand hat die Absicht, eine mauer zu errichten!";

        public Patron GetByWorkNetId( int pworkNetId )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.GetByWorkNetId( pworkNetId );
        }//

        public Patron GetByWorkNetCredentials( string loginId, string token )
        {
            return GetByExtSiteCredentials( 1, loginId, token );
        }//

        public Patron GetByExtSiteCredentials( int externalSiteId, string loginId, string token )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.GetByExtSiteCredentials( externalSiteId, loginId, token );
        }//

		/// <summary>
		/// Login with workNet credentials
		/// This using a web service on old workNet, and is obsolete
		/// </summary>
		/// <param name="loginName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
        [Obsolete]
		public Patron LoginViaWorkNet( string loginName, string password )
        {
            AcctSvce.AccountSoapClient wsClient = new AcctSvce.AccountSoapClient();
            AcctSvce.AccountDetail acct = new AcctSvce.AccountDetail();

            string encryptedPassword = ServiceHelper.Encrypt( codedPassword );
            acct = wsClient.Login( "workNet902", codedPassword, loginName, password );

            if ( acct != null && acct.worknetId > 0 )
            {
                //now do we arbitrarily create a pathways account?
                //BO.Patron user = new BO.Patron();
                Patron user = new Patron();
                user.FirstName = acct.firstName;
                user.LastName = acct.lastName;
                user.Email = acct.email;
                user.UserName = acct.userName;
                user.Password = password;
                user.IsValid = true;
                //user.worknetId = acct.worknetId;
                user.RowId = new Guid( acct.rowId );

                return user;
            }
            else
            {
                Patron user = new Patron();
                user.Message = acct.statusMessage; //"Error: Invalid Username or Password";
                user.IsValid = false;

                return user;
            }
        }

		/// <summary>
		/// get a external account
		/// </summary>
		/// <param name="id"></param>
		/// <param name="userId"></param>
		/// <param name="externalSiteId"></param>
		/// <param name="externalLoginProvider"></param>
		/// <param name="token"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public PatronExternalAccount ExternalAccount_Get( int id, int userId, int externalSiteId, string externalLoginProvider, string token, ref string statusMessage )
		{
			PatronExternalAccount acct = myManager.ExternalAccount_Get( id, userId, externalSiteId, externalLoginProvider, token, ref statusMessage );

			return acct;
		}

		/// <summary>
		/// Create an external account record
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="externalSiteId"></param>
		/// <param name="loginName"></param>
		/// <param name="token"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int ExternalAccount_Create( int userId, int externalSiteId, string loginName, string token, ref string statusMessage )
		{
			return myManager.ExternalAccount_Create( userId, externalSiteId, loginName, token, ref statusMessage );
		}

		public int ExternalAccount_Create( int userId, string externalLoginProvider, string loginName, string token, ref string statusMessage )
		{
			return myManager.ExternalAccount_Create( userId, externalLoginProvider, loginName, token, ref statusMessage );
		}
        #endregion

    }
}
