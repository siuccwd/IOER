using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using Isle.DataContracts;
using Isle.BizServices;
using ThisUser = LRWarehouse.Business.Patron;
using ILPathways.Business;

namespace Isle.RESTServices
{
    //[AspNetCompatibilityRequirements( RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed )]
    public class LibraryService : ILibraryService
    {
        LibraryBizService bizService = new LibraryBizService();
        int defaultPageSize = 25;
        //leave as blank and user proc default
        string defaultSortOrder = "";
        public LibraryResourceSearchResponse Search( LibraryResourceSearchRequest request )
        {
            //TODO - probably would force a libraryId
            if ( request.Filter.IndexOf( "LibraryId" ) < 0 && request.LibraryId > 0)
            {
                if ( request.Filter.Length > 2)
                    request.Filter = string.Format( " (lib.LibraryId =  {0}) AND ", request.LibraryId ) + request.Filter;
                else
                    request.Filter = string.Format( " lib.LibraryId =  {0}", request.LibraryId );
            }

            return bizService.ResourceSearch( request );
        }

        public LibraryResourceSearchResponse MyLibrary( string userName, string pageNbr, string filter )
        {
            string booleanOperator = "AND";
            AccountServices acctSrv = new AccountServices();
            ThisUser user;
            if ( userName.IndexOf("@") > -1)
                user = acctSrv.GetByEmail( userName );
            else
                user = acctSrv.GetByUsername( userName );

            if ( user == null || user.Id == 0 )
            {
                LibraryResourceSearchResponse response = new LibraryResourceSearchResponse();

                response.Error.Message = "Account not found ";
                response.Status = StatusEnumDataContract.Failure;
                return response;
            }

            Library lib = bizService.GetMyLibrary( user );

            LibraryResourceSearchRequest request = new LibraryResourceSearchRequest();
            request.LibraryId = 0;
            request.Filter = string.Format( " lib.LibraryId =  {0}", lib.Id );

            request.Filter += LibraryBizService.FormatSearchItem( request.Filter, "((IsPublic = 1 OR IsDiscoverable = 1) AND IsCollectionPublic = 1)", booleanOperator );
            //initially assume filter is just keywords, not fully formatted
            string where = "";
            FormatKeyword( filter, booleanOperator, ref where );
            if ( where.Length > 5 )
                request.Filter += " AND " + where;

            request.SortOrder = defaultSortOrder;
            request.StartingPageNbr = ServiceHelper.StringToInt( pageNbr, 1 );
            request.PageSize = defaultPageSize;

            return bizService.ResourceSearch( request );
        }

        /// <summary>
        /// Retrieve library resources - only publically available
        /// </summary>
        /// <param name="libraryId"></param>
        /// <returns>Collection of resources in the requested library</returns>
        public LibraryResourceSearchResponse LibraryList( string libraryId )
        {
           
            return Library( libraryId, "1", "" );
        }

        public LibraryResourceSearchResponse Library( string libraryId, string pageNbr, string filter )
        {
            string booleanOperator = "AND";

            LibraryResourceSearchRequest request = new LibraryResourceSearchRequest();
            request.LibraryId = 0;
            request.Filter = string.Format( " lib.LibraryId =  {0}", libraryId );

            request.Filter += LibraryBizService.FormatSearchItem( request.Filter, "((IsPublic = 1 OR IsDiscoverable = 1) AND IsCollectionPublic = 1)", booleanOperator );
            //initially assume filter is just keywords, not fully formatted
            string where = "";
            FormatKeyword( filter, booleanOperator, ref where );
            if ( where.Length > 5 )
                request.Filter += " AND " + where;

            request.SortOrder = defaultSortOrder;
            request.StartingPageNbr = ServiceHelper.StringToInt( pageNbr, 1 );
            request.PageSize = defaultPageSize;

            return bizService.ResourceSearch( request );
        }

        /// <summary>
        /// Retrieve library resources - only publically available
        /// </summary>
        /// <param name="libraryId"></param>
        /// <returns>Collection of resources in the requested library</returns>
        public LibraryResourceSearchResponse LibraryCollections( string libraryId )
        {
            //NOT USED **********
            string booleanOperator = "AND";
            LibraryResourceSearchRequest request = new LibraryResourceSearchRequest();
            request.LibraryId = 0;
            request.Filter = string.Format( " lib.LibraryId =  {0}", libraryId );
            request.Filter += LibraryBizService.FormatSearchItem( request.Filter, "((IsPublic = 1 OR IsDiscoverable = 1) AND IsCollectionPublic = 1)", booleanOperator );

            request.SortOrder = "";
            request.StartingPageNbr = 1;
            request.PageSize = 50;

            return bizService.ResourceSearch( request );
        }

        /// <summary>
        /// Retrieve resources for a collection
        /// - needs to be publically available
        /// </summary>
        /// <param name="collectionId"></param>
        /// <returns></returns>
        public LibraryResourceSearchResponse Collection( string collectionId, string pageNbr, string filter )
        {
            string booleanOperator = "AND";
            LibraryResourceSearchRequest request = new LibraryResourceSearchRequest();
            request.LibraryId = 0;
            request.Filter = string.Format( " lib.LibrarySectionId =  {0}", collectionId );

            request.Filter += LibraryBizService.FormatSearchItem( request.Filter, "((IsPublic = 1 OR IsDiscoverable = 1) AND IsCollectionPublic = 1)", booleanOperator );

            //initially assume filter is just keywords, not fully formatted
            string where = "";
            FormatKeyword( filter, booleanOperator, ref where );
            if ( where.Length > 5 )
                request.Filter += " AND " + where;

            request.SortOrder = defaultSortOrder;
            request.StartingPageNbr = ServiceHelper.StringToInt( pageNbr, 1 );
            request.PageSize = defaultPageSize;

            return bizService.ResourceSearch( request );
        }

        /// <summary>
        /// Retrieve resources for a collection - requires credentials
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="emailAddress">NOTE: handles either username or email!</param>
        /// <param name="password">This may be an SSO token of some sort in the future</param>
        /// <returns></returns>
        public LibraryResourceSearchResponse Collection( string collectionId, string emailAddress, string password, string pageNbr, string filter )
        {
            //first validate user has is valid and can access the collection
            AccountServices acctSrv = new AccountServices();
            string statusMessage = "";

            ThisUser user =  acctSrv.Authorize( emailAddress, password, ref statusMessage ) ;

            if ( user == null || user.Id == 0 )
            {
                LibraryResourceSearchResponse response = new LibraryResourceSearchResponse();
                
                response.Error.Message = "Invalid account credentials ";
                response.Status = StatusEnumDataContract.Failure;
                return response;
            }
            //get section to ensure valid, and also library info
            
            //verify user can access collection
            //if can't directly, can still get public resources
            string booleanOperator = "AND";
            LibraryResourceSearchRequest request = new LibraryResourceSearchRequest();
            request.LibraryId = 0;
            request.Filter = string.Format( " lib.LibrarySectionId =  {0}", collectionId );
            //may be case in future that user could have access to a collection even if not owner - could use subscription?
            string where = string.Format("(((IsPublic = 1 OR IsDiscoverable = 1) AND IsCollectionPublic = 1) OR ( LibraryCreatedById = {0}))",user.Id);

            request.Filter += LibraryBizService.FormatSearchItem( request.Filter, where, booleanOperator );
            //initially assume filter is just keywords, not fully formatted
            where = "";
            FormatKeyword( filter, booleanOperator, ref where );
            if ( where.Length > 5 )
                request.Filter += " AND " + where;

            request.SortOrder = defaultSortOrder;
            request.StartingPageNbr = ServiceHelper.StringToInt( pageNbr, 1 );
            request.PageSize = defaultPageSize;

            return bizService.ResourceSearch( request );
        }

        protected void FormatKeyword( string input, string booleanOperator, ref string filter )
        {
            string keywordTemplate =  "(lr.Title like '{0}'  OR lr.[Description] like '{0}')";
            string keyword = LibraryBizService.HandleApostrophes( LibraryBizService.CleanText( input.Trim() ) );
            string keywordFilter = "";

            if ( keyword.Length > 0 )
            {
                keyword = keyword.Replace( "*", "%" );
                if ( keyword.IndexOf( "," ) > -1 )
                {
                    string[] phrases = keyword.Split( new char[] { ',' } );
                    foreach ( string phrase in phrases )
                    {
                        string next = phrase.Trim();
                        if ( next.IndexOf( "%" ) == -1 )
                            next = "%" + next + "%";
                        string where = string.Format( keywordTemplate, next );
                        keywordFilter += LibraryBizService.FormatSearchItem( keywordFilter, where, "OR" );
                    }
                }
                else
                {
                    if ( keyword.IndexOf( "%" ) == -1 )
                        keyword = "%" + keyword + "%";

                    keywordFilter = string.Format( keywordTemplate, keyword );

                }

                if ( keywordFilter.Length > 0 )
                    filter += LibraryBizService.FormatSearchItem( filter, keywordFilter, booleanOperator );
            }
        }	//
        public LibraryResourceSearchResponse Test()
        {
            LibraryResourceSearchRequest request = new LibraryResourceSearchRequest();
            request.LibraryId = 1;
            request.Filter = " lib.LibraryId = 1 ";
            request.SortOrder = "";
            request.StartingPageNbr = 1;
            request.PageSize = 25;

            return bizService.ResourceSearch( request );
        }
    }
}