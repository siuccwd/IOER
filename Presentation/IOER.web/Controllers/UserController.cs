using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using AcctSvce = ILPathways.AccountServiceReference;
using wnGlossary = ILPathways.workNetGlossary;
using utilities = ILPathways.Utilities;
using BO = ILPathways.Business;
using LRWarehouse.Business;

//using PatronMgr = LRWarehouse.DAL.PatronManager;
using PatronMgr = Isle.BizServices.AccountServices;

namespace ILPathways.Controllers
{
    public class UserController
    {
        string className = "UserController";

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
        #endregion

        #region ====== Retrieval Methods ===============================================
        /// <summary>
        /// Get User record via integer id
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public Patron Get( int pId )
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
            PatronMgr mgr = new PatronMgr();
            return mgr.GetByRowId( pRowId );
        }//

        /// <summary>
        /// Get User record via email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Patron GetByEmail( string email )
        {
            PatronMgr mgr = new PatronMgr();
            return mgr.GetByEmail( email );

        }//


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

        //public Patron Authorize( string userName, string password )
        //{
        //    PatronMgr mgr = new PatronMgr();
        //    return mgr.Authorize( userName, password );
        //}

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
        //public List<Patron> Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, bool pOutputRelTables, ref int pTotalRows )
        //{
        //    PatronMgr mgr = new PatronMgr();
        //    return mgr.Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );

            
        //}


        #endregion

        #endregion

 
        #region workNet user methods

        string codedPassword = "Niemand hat die Absicht, eine mauer zu errichten!";

        public Patron LoginViaWorkNet(string loginName, string password)
        {
            AcctSvce.AccountSoapClient wsClient = new AcctSvce.AccountSoapClient();
            AcctSvce.AccountDetail acct = new AcctSvce.AccountDetail();

            string encryptedPassword = utilities.UtilityManager.Encrypt( codedPassword );
            acct = wsClient.Login( "workNet902", codedPassword, loginName, password );

            if ( acct != null && acct.worknetId > 0 )
            {
                //now do we arbitrarily create a pathways account?
                //BO.AppUser user = new BO.AppUser();
                Patron user = new Patron();
                user.FirstName = acct.firstName;
                user.LastName = acct.lastName;
                user.Email = acct.email;
                user.Username = acct.userName;
                user.Password = password;
                user.IsValid = true;
                user.worknetId = acct.worknetId;
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
        #endregion
    }
}