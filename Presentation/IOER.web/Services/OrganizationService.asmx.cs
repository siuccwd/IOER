using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

using ILPathways.Business;
using ILPathways.Utilities;
using Isle.BizServices;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using DataBaseHelper = LRWarehouse.DAL.BaseDataManager;

namespace IOER.Services
{
    /// <summary>
    /// Summary description for OrganizationService
    /// </summary>
    [WebService( Namespace = "http://tempuri.org/" )]
    [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
    [System.ComponentModel.ToolboxItem( false )]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class OrganizationService : System.Web.Services.WebService
    {

        OrganizationBizService orgService = new OrganizationBizService();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        [WebMethod]
        public string DoSearch( string text, List<JSONInputFilter> filters, string userGUID, bool useAssociatedOrgs, string sort, int selectedPageNbr )
        {
            try
            {
                //Determine which sort option to use
                var sortString = " org.Name, ";

                if ( sort != "" )
                {
                    var sortItems = sort.Split( '|' );
                    var order = ( sortItems[ 1 ] == "asc" ? " ASC" : " DESC" );
                    switch ( sortItems[ 0 ] )
                    {
                        case "title":
                            sortString = "org.Name" + order;
                            break;
                        case "type":
                            sortString = "org.Name" + order;
                            break;
                        case "contact":
                            sortString = "owner.SortName" + order;
                            break;
                        case "organization:":
                            sortString = "Organization" + order;
                            break;

                        default:
                            break;
                    }
                }

                //Continue
                int totalResults = 0;
                int pageSize = 100;
                string generatedFilter = "";
                var user = new UtilityService().GetUserFromGUID( userGUID );

                var orgs = DoSearch( text, filters, user, sortString, selectedPageNbr, pageSize, useAssociatedOrgs, ref generatedFilter );
                    var results = BuildSearchResults( orgs );

                    return new UtilityService().ImmediateReturn( results, true, "okay", new { totalResults = orgs.Count() } ); //May want this to return the filter as part of the "extra" object

          

            }
            catch ( Exception ex )
            {
                return new UtilityService().ImmediateReturn( "", false, ex.Message, null );
            }
        }


        /// <summary>
        /// orgs search
        /// </summary>
        /// <param name="text"></param>
        /// <param name="filters"></param>
        /// <param name="user"></param>
        /// <param name="sortTerm"></param>
        /// <param name="selectedPageNbr"></param>
        /// <param name="pageSize"></param>
        /// <param name="useAssociatedOrgs"></param>
        /// <param name="generatedFilter"></param>
        /// <returns></returns>
        public List<Organization> DoSearch( string text, List<JSONInputFilter> filters, Patron user, string sortTerm, int selectedPageNbr, int pageSize, bool useAssociatedOrgs, ref string generatedFilter )
        {
            text = FormHelper.CleanText( text.Trim() );

            string filter = FormatFilter( text, filters, user, useAssociatedOrgs );
            generatedFilter = filter;
            int totalRows = 0;
            
            //Get the search results
            List<Organization> orgs = orgService.Organization_Search( filter, sortTerm, selectedPageNbr, pageSize, ref totalRows );

            return orgs;
        }

        #region Helper Methods

        /// <summary>
        /// call for an initial search
        /// </summary>
        /// <returns></returns>
        protected string FormatFilter( string text, List<JSONInputFilter> filters, Patron user, bool useAssociatedOrgs )
        {
            string booleanOperator = "AND";
            string filter = "";

            if ( useAssociatedOrgs )
            {
                filter = string.Format( " ( base.Id in (SELECT  OrgId FROM [Organization.Member] where UserId = {0}) ) ", user.Id ); ;
            }

            int dateFilterID = 0;
            List<int> orgTypeFilterIDs = new List<int>();
            int privacyFilterID = 2;

            foreach ( JSONInputFilter item in filters )
            {
                switch ( item.name )
                {
                    case "orgType":
                        orgTypeFilterIDs = item.ids;
                        break;
                    case "view":
                        privacyFilterID = item.ids.First<int>();
                        break;
                    default: break;
                }
            }

            //
            FormatOrgTypeFilter( orgTypeFilterIDs, booleanOperator, ref filter );
            //
            //FormatViewableFilter( booleanOperator, privacyFilterID, user, ref filter );

            FormatKeyword( text, booleanOperator, ref filter );

            if ( new WebDALService().IsLocalHost() )
            {
                LoggingHelper.DoTrace( 6, "sql: " + filter );
            }

            return filter;
        }	//
        protected void FormatKeyword( string text, string booleanOperator, ref string filter )
        {
            string keyword = DataBaseHelper.HandleApostrophes( FormHelper.CleanText( text ) );
            string keywordFilter = "";
            string keywordTemplate = " (base.Name like '{0}' OR base.[City] like '{0}' ";

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
                        keywordFilter += DataBaseHelper.FormatSearchItem( keywordFilter, where, "OR" );
                    }
                }
                else
                {
                    if ( keyword.IndexOf( "%" ) == -1 )
                    {
                        keyword = "%" + keyword + "%";
                    }

                    keywordFilter = string.Format( keywordTemplate, keyword );
                }

                if ( keywordFilter.Length > 0 )
                {
                    filter += DataBaseHelper.FormatSearchItem( filter, keywordFilter, booleanOperator );
                }
            }
        }	//
        
        private void FormatViewableFilter( string booleanOperator, int privacyFilterID, Patron user, ref string filter )
        {
            //      TODO
            switch ( privacyFilterID )
            {
                case 1:
                    break;
                case 2:
                    filter += DataBaseHelper.FormatSearchItem( filter, ".orgPublicAccessLevel > 1", booleanOperator );
                    break;
                case 3:
                    filter += DataBaseHelper.FormatSearchItem( filter, ".orgPublicAccessLevel = 1", booleanOperator );
                    break;
                case 4:
                    if ( user.IsValid && user.Id > 0 )
                    {
                        filter += DataBaseHelper.FormatSearchItem( filter, string.Format( "(base.CreatedById = {0}) OR  (base.Id in (Select LibraryId from [Library.Member] where userid = {0} ))", user.Id ), booleanOperator );
                    }
                    else { }
                    break;
                default:
                    //should always get where member or creator
                    if ( user.IsValid && user.Id > 0 )
                    {
                        filter += DataBaseHelper.FormatSearchItem( filter, string.Format( "(.orgCreatedById = {0}) OR  (.orgId in (Select LibraryId from [Library.Member] where userid = {0} ))", user.Id ), booleanOperator );
                    }
                    break;
            }
        }
        public static void FormatOrgTypeFilter( List<int> orgTypeFilterIDs, string booleanOperator, ref string filter )
        {
            string csv = "";
            foreach ( int id in orgTypeFilterIDs )
            {
                csv += id + ",";
            }

            if ( csv.Length > 0 )
            {
                csv = csv.Substring( 0, csv.Length - 1 );

                string where = string.Format( " (OrgTypeId in ({0})) ", csv );
                filter += DataBaseHelper.FormatSearchItem( filter, where, booleanOperator );
            }
        }

        public List<JSONOrganizationSearchResult> BuildSearchResults( List<Organization> orgs )
        {
            var output = new List<JSONOrganizationSearchResult>();

            foreach ( Organization item in orgs )
            {
                var org = new JSONOrganizationSearchResult();
                org.name = item.Name;
                string encodedTitle = UtilityManager.UrlFriendlyTitle( org.name );

                org.city = item.City;
                org.iconURL = item.LogoUrl;

                //org.url = "/Organization/?id=" + item.Id;
                org.url = string.Format( "/Organizations/{0}/{1}", item.Id, encodedTitle );
                org.id = item.Id;



                output.Add( org );
            }

            return output;
        }

        #endregion
        #region Subclasses

        public class JSONInputFilter
        {
            public string name;
            public List<int> ids { get; set; }
        }
        public class JSONSearchResultItem
        {
            public string name { get; set; }
            public string city { get; set; }
            public string url { get; set; }
            public string iconURL { get; set; }
            public int id { get; set; }
        }
        public class JSONOrganizationSearchResult : JSONSearchResultItem
        {
            public JSONOrganizationSearchResult()
            {
            }
            public string url { get; set; }
            public string organizationTitle { get; set; }
            public string organizationUrl { get; set; }


        }


        #endregion
    }
}
