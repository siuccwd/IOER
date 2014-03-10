using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
//using AutoMapper;

using ILPathways.Business;
using ILPathways.Common;
using ILPathways.DAL;
using AcctSvce = Isle.BizServices.workNetAccountServices;
using PatronMgr = LRWarehouse.DAL.PatronManager;
//.use alias for easy change to Gateway
//using ThisUser = ILPathways.Business.AppUser;
//using ThisUserProfile = ILPathways.Business.AppUserProfile;
//using PatronMgr = ILPathways.DAL.PatronManager;

using LRWarehouse.Business;
using ThisUser = LRWarehouse.Business.Patron;
using ThisUserProfile = LRWarehouse.Business.PatronProfile;
using ResBiz = IOERBusinessEntities;

namespace Isle.BizServices
{
    public class AccountServices : ServiceHelper
    {
        string className = "AccountServices";
        PatronMgr myManager = new PatronMgr();

        #region Organzations
        public static Organization OrganizationCreate( Organization org, string pathway )
        {
            return OrganizationManager.Create( org, pathway); 
        }

        public static Organization OrganizationUpdate( Organization org )
        {
            return OrganizationManager.Update( org );
        }


        public static DataSet GetTopLevelOrganzations()
        {
            return OrganizationManager.GetTopLevelOrganzations(); ;
        }
        public static DataSet GetChildOrganizations( int parentId )
        {
            return OrganizationManager.GetChildOrganizations( parentId );;
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

        public static Organization GetOrgByName( string orgName )
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
        #endregion

        #region Authorization

        private static bool CanUserAuthor( ThisUser appUser )
        {
            bool isValid = false;

            return isValid;
        }

        private static bool CanUserOrgAuthor( ThisUser appUser )
        {
            bool isValid = false;

            return isValid;
        }
        /// <summary>
        /// Retrieve customer by User Id
        /// </summary>
        /// <param name="rowId"></param>
        /// <param name="encryptedPassword"></param>
        /// <returns></returns>
        public ThisUser GetCustomerByRowId( string rowId, ref string statusMessage )
        {

            ThisUser user = new ThisUser();
            string message = "There was an error while processing the request.";

            try
            {
                user = myManager.GetByRowId( rowId );

            }
            catch ( Exception ex )
            {
                ServiceHelper.LogError( "AccountServices.GetCustomerByRowId(): " + ex.ToString() );
                statusMessage = message;
                return null;
            }

            return user;
        } //

        #endregion
        #region  ThisUser methods
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
        public int Create( ThisUser entity, ref string statusMessage )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.Create( entity, ref statusMessage );

        }

        /// <summary>
        /// Update an User record
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string Update( ThisUser entity )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.Update( entity );

        }//
        #endregion

        #region ====== Retrieval Methods ===============================================
        /// <summary>
        /// Get User record via integer id
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public ThisUser Get( int pId )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.Get( pId );
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
            PatronMgr mgr = new PatronMgr();
            return mgr.DoesUserEmailExist( email );

        }//
        /// <summary>
        /// retrieve user by username
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ThisUser GetByUsername( string userName )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.GetByUsername( userName );

        }//
        /// <summary>
        /// Get User record via rowId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public ThisUser GetByRowId( string pRowId )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.GetByRowId( pRowId );
        }//

        /// <summary>
        /// Get User record via email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public ThisUser GetByEmail( string email )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.GetByEmail( email );

        }//

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="loginName">Can be either email address or user name</param>
        /// <param name="password"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public ThisUser Login( string loginName, string password, ref string statusMessage )
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
        public ThisUser Authorize( string userName, string password, ref string statusMessage )
        {
            ThisUser appUser = new ThisUser();
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
            //return mgr.Authorize( userName, password );

            //get user
            appUser = mgr.Authorize( userName, encryptedPassword );

            if ( appUser == null || appUser.IsValid == false )
            {
                statusMessage = "Error: Login failed - Invalid credentials";
            }

            return appUser;
        }

        public ThisUser RecoverPassword( string lookup )
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
        public List<ThisUser> Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, bool pOutputRelTables, ref int pTotalRows )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );


        }
        public DataSet UserSearch( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, bool pOutputRelTables, ref int pTotalRows )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.UserSearch( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );


        }
        public int PatronProfile_Create( ThisUserProfile entity, ref string statusMessage )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.PatronProfile_Create( entity, ref statusMessage );

        }//
        public string PatronProfile_Update( ThisUserProfile entity )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.PatronProfile_Update( entity );

        }//
        public ThisUserProfile PatronProfile_Get( int pUserId )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.PatronProfile_Get( pUserId );

        }//


        public static List<Patron> GetAllUsers()
        {

            ResBiz.ResourceEntities ctx = new ResBiz.ResourceEntities();
            List<ResBiz.Patron> eflist = ctx.Patrons
                            .OrderBy( s => s.LastName )
                            .ToList();
            List<Patron> list = new List<Patron>();
            if ( eflist.Count > 0 )
            {
                foreach ( ResBiz.Patron item in eflist )
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

            ResBiz.ResourceEntities ctx = new ResBiz.ResourceEntities();

            List<ResBiz.Patron> eflist = ctx.Patrons.OrderBy( s => s.LastName ).ToList();
            List<CodeItem> list = new List<CodeItem>();
            if ( eflist.Count > 0 )
            {
                foreach ( ResBiz.Patron item in eflist )
                {
                    ci = new CodeItem();
                    ci.Id = item.Id;
                    ci.Title = item.LastName + ", " + item.FirstName;
                    list.Add( ci );
                }
            }

            return list;

        }

        #endregion
        #endregion

        #region ====== workNet/external Methods ===============================================
        string codedPassword = "Niemand hat die Absicht, eine mauer zu errichten!";

        public ThisUser GetByWorkNetId( int pworkNetId )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.GetByWorkNetId( pworkNetId );
        }//

        public ThisUser GetByWorkNetCredentials( string loginId, string token )
        {
            return GetByExtSiteCredentials( 1, loginId, token );
        }//

        public ThisUser GetByExtSiteCredentials( int externalSiteId, string loginId, string token )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.GetByExtSiteCredentials( externalSiteId, loginId, token );
        }//
        public ThisUser LoginViaWorkNet( string loginName, string password )
        {
            AcctSvce.AccountSoapClient wsClient = new AcctSvce.AccountSoapClient();
            AcctSvce.AccountDetail acct = new AcctSvce.AccountDetail();

            string encryptedPassword = ServiceHelper.Encrypt( codedPassword );
            acct = wsClient.Login( "workNet902", codedPassword, loginName, password );

            if ( acct != null && acct.worknetId > 0 )
            {
                //now do we arbitrarily create a pathways account?
                //BO.ThisUser user = new BO.ThisUser();
                ThisUser user = new ThisUser();
                user.FirstName = acct.firstName;
                user.LastName = acct.lastName;
                user.Email = acct.email;
                user.Username = acct.userName;
                user.Password = password;
                user.IsValid = true;
                //user.worknetId = acct.worknetId;
                user.RowId = new Guid( acct.rowId );

                return user;
            }
            else
            {
                ThisUser user = new ThisUser();
                user.Message = acct.statusMessage; //"Error: Invalid Username or Password";
                user.IsValid = false;

                return user;
            }
        }
        #endregion

    }
}
