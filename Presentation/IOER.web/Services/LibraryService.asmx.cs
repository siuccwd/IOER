using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using System.Web.Script.Serialization;
using Isle.BizServices;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using ILPathways.Utilities;

using DataBaseHelper = LRWarehouse.DAL.BaseDataManager;

namespace ILPathways.Services
{
    /// <summary>
    /// Summary description for LibraryService
    /// </summary>
    [WebService( Namespace = "http://tempuri.org/" )]
    [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
    [System.ComponentModel.ToolboxItem( false )]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class LibraryService : System.Web.Services.WebService
    {
      LibraryBizService libService = new LibraryBizService();
      ElasticSearchManager esManager = new ElasticSearchManager();
      JavaScriptSerializer serializer = new JavaScriptSerializer();

      #region Library Search methods

      [WebMethod]
      public string DoLibrariesSearch( string searchType, string text, List<JSONInputFilter> filters, string userGUID, bool useSubscribedLibraries, string sort, int start )
      {
        try
        {
          //Determine which sort option to use
          var sortString = "";
          if ( sort != "" )
          {
            var sortItems = sort.Split( '|' );
            var order = (sortItems[1] == "asc" ? " ASC" : " DESC");
            switch ( sortItems[ 0 ] )
            {
              case "title":
                sortString = "lib.Title" + order;
                break;
              case "type":
                sortString = "libt.Title" + order;
                break;
              case "contact":
                sortString = "owner.SortName" + order;
                break;
              case "organization:":
                sortString = "Organization" + order;
                break;
              case "total":
                sortString = "TotalResources" + order;
                break;
              case "updated":
                sortString = "ResourceLastAddedDate" + order;
                break;
              default:
                break;
            }
          }

          //Continue
          int totalResults = 0;
          string generatedFilter = "";
          var user = new UtilityService().GetUserFromGUID( userGUID );
          if ( searchType == "libraries" )
          {
            var libraries = DoLibrariesSearch( text, filters, user, sortString, start, 100, useSubscribedLibraries, ref generatedFilter );
            var results = BuildLibrarySearchResults( libraries );

            return new UtilityService().ImmediateReturn( results, true, "okay", new { totalResults = libraries.Count() } ); //May want this to return the filter as part of the "extra" object

          }
          else if ( searchType == "collections" )
          {
            var collections = DoCollectionsSearch( text, filters, user, sortString, start, 100, useSubscribedLibraries, ref generatedFilter );
            var results = BuildCollectionSearchResults( collections );

            return new UtilityService().ImmediateReturn( results, true, "okay", new { totalResults = collections.Count() } );

          }
          else
          {
            return new UtilityService().ImmediateReturn( "", false, "Invalid Search Type", null );
          }
        }
        catch ( Exception ex )
        {
          return new UtilityService().ImmediateReturn( "", false, ex.Message, null );
        }
      }

        /// <summary>
        /// Libraries search
        /// </summary>
        /// <param name="text"></param>
        /// <param name="filters"></param>
        /// <param name="user"></param>
        /// <param name="sort"></param>
        /// <param name="start"></param>
        /// <param name="max"></param>
        /// <param name="useSubscribedLibraries"></param>
        /// <param name="generatedFilter"></param>
        /// <returns></returns>
      public List<Business.Library> DoLibrariesSearch( string text, List<JSONInputFilter> filters, Patron user, string sort, int start, int max, bool useSubscribedLibraries, ref string generatedFilter )
      {
        text = FormHelper.SanitizeUserInput( text.Trim() );

        string filter = FormatFilter( text, filters, user, useSubscribedLibraries );
        generatedFilter = filter;
        int totalRows = 0;

        //Get the library search results
        var libraries = libService.LibrarySearchAsList( filter, sort, start, max, ref totalRows );

        return libraries;
      }
        /// <summary>
        /// Collections search
        /// </summary>
        /// <param name="text"></param>
        /// <param name="filters"></param>
        /// <param name="user"></param>
        /// <param name="sort"></param>
        /// <param name="start"></param>
        /// <param name="max"></param>
        /// <param name="useSubscribedLibraries"></param>
        /// <param name="generatedFilter"></param>
        /// <returns></returns>
      public List<Business.LibrarySection> DoCollectionsSearch( string text, List<JSONInputFilter> filters, Patron user, string sort, int start, int max, bool useSubscribedLibraries, ref string generatedFilter )
      {
        text = FormHelper.SanitizeUserInput( text.Trim() );

        string filter = FormatFilter( text, filters, user, useSubscribedLibraries );
        generatedFilter = filter;
        int totalRows = 0;

        //Get the collection search results
        var collections = libService.LibrarySections_SearchAsList( filter, sort, start, max, ref totalRows );

        return collections;
      }

      #endregion
      #region Helper Methods

      /// <summary>
      /// call for an initial search
      /// </summary>
      /// <returns></returns>
      protected string FormatFilter( string text, List<JSONInputFilter> filters, Patron user, bool useSubscribedLibraries )
      {
        string booleanOperator = "AND";
        string filter = "";

        if ( useSubscribedLibraries )
        {
          filter = " ( lib.Id in (SELECT  LibraryId FROM [Library.Subscription] where UserId = " + user.Id + ") ) ";
        }

        int dateFilterID = 0;
        List<int> libraryTypeFilterIDs = new List<int>();
        int privacyFilterID = 2;

        foreach ( JSONInputFilter item in filters )
        {
          switch ( item.name )
          {
            case "dateRange":
              dateFilterID = item.ids.First<int>();
              break;
            case "libraryType":
              libraryTypeFilterIDs = item.ids;
              break;
            case "view":
              privacyFilterID = item.ids.First<int>();
              break;
            default: break;
          }
        }

        //
        FormatDatesFilter( booleanOperator, dateFilterID, ref filter );
        //
        FormatLibTypeFilter( libraryTypeFilterIDs, booleanOperator, ref filter );
        //
        FormatViewableFilter( booleanOperator, privacyFilterID, user, ref filter );

        FormatKeyword( text, booleanOperator, ref filter );

        if ( new WebDALService().IsLocalHost() )
        {
          LoggingHelper.DoTrace( 6, "sql: " + filter );
        }

        return filter;
      }	//
      protected void FormatKeyword( string text, string booleanOperator, ref string filter )
      {
        string keyword = DataBaseHelper.HandleApostrophes( FormHelper.SanitizeUserInput( text ) );
        string keywordFilter = "";
        string keywordTemplate = " (lib.Title like '{0}'  OR lib.[Description] like '{0}' OR owner.[SortName] like '{0}'  OR owner.[Organization] like '{0}' OR org.[Name] like '{0}') ";

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
      private void FormatDatesFilter( string booleanOperator, int dateFilterID, ref string filter )
      {
        DateTime endDate;
        switch ( dateFilterID )
        {
          case 1:
            endDate = System.DateTime.Now.AddDays( -7 );
            break;
          case 2:
            endDate = System.DateTime.Now.AddDays( -30 );
            break;
          case 3:
            endDate = System.DateTime.Now.AddMonths( -6 );
            break;
          case 4:
            endDate = System.DateTime.Now.AddYears( -1 );
            break;
          default:
            return;
            break;
        }

        string where = string.Format( " ResourceLastAddedDate > '{0}'", endDate.ToString( "yyyy-MM-dd" ) );
        filter += DataBaseHelper.FormatSearchItem( filter, where, booleanOperator );
      }
      private void FormatViewableFilter( string booleanOperator, int privacyFilterID, Patron user, ref string filter )
      {
        switch ( privacyFilterID )
        {
          case 1:
            return;
            break;
          case 2:
            filter += DataBaseHelper.FormatSearchItem( filter, "lib.PublicAccessLevel > 1", booleanOperator );
            break;
          case 3:
            filter += DataBaseHelper.FormatSearchItem( filter, "lib.PublicAccessLevel = 1", booleanOperator );
            break;
          case 4:
            if ( user.IsValid && user.Id > 0 )
            {
              filter += DataBaseHelper.FormatSearchItem( filter, string.Format( "lib.Id in (Select LibraryId from [Library.Member] where userid = {0} )", user.Id ), booleanOperator );
            }
            else { }
            break;
          default: break;
        }
      }
      public static void FormatLibTypeFilter( List<int> libraryTypeFilterIDs, string booleanOperator, ref string filter )
      {
        string csv = "";
        foreach ( int id in libraryTypeFilterIDs )
        {
          csv += id + ",";
        }

        if ( csv.Length > 0 )
        {
          csv = csv.Substring( 0, csv.Length - 1 );

          string where = string.Format( " (lib.LibraryTypeId in ({0})) ", csv );
          filter += DataBaseHelper.FormatSearchItem( filter, where, booleanOperator );
        }
      }

      public List<JSONLibrarySearchResult> BuildLibrarySearchResults( List<Business.Library> libraries )
      {
        var output = new List<JSONLibrarySearchResult>();

        foreach ( Business.Library item in libraries )
        {
          var lib = new JSONLibrarySearchResult();
          lib.title = item.Title;
          string encodedTitle = lib.title.Replace( " ", "_" );
          encodedTitle = encodedTitle.Replace( "'", "" );

          lib.description = item.Description;
          lib.iconURL = item.ImageUrl;
          //lib.url = "/Library/?id=" + item.Id;
          lib.url = string.Format("/Library/{0}/{1}",  item.Id,encodedTitle);
          lib.id = item.Id;

          var collections = libService.LibrarySectionsSelectList( item.Id, 1 );
          foreach ( Business.LibrarySection section in collections )
          {
            var col = new JSONLibColSearchResultItem();
            col.title = section.Title;
            string encodedColTitle = col.title.Replace( " ", "_" );
            encodedColTitle = encodedColTitle.Replace( "'", "" );

            col.description = section.Description;
            col.iconURL = section.ImageUrl;
            col.id = section.Id;
            col.url = string.Format( "/Library/Collection/{0}/{1}/{2}", item.Id, section.Id, encodedColTitle );

            lib.collections.Add( col );
          }

          output.Add( lib );
        }

        return output;
      }

      public List<JSONLibrarySearchResult> BuildCollectionSearchResults( List<Business.LibrarySection> collections )
      {
        var output = new List<JSONLibrarySearchResult>();

        foreach ( Business.LibrarySection item in collections )
        {
          var col = new JSONLibrarySearchResult();
          col.title = item.Title;
          string encodedTitle = col.title.Replace( " ", "_" );
          encodedTitle = encodedTitle.Replace( "'", "" );

          col.description = item.Description;
          col.iconURL = item.ImageUrl;
          //col.url = "/Library/?id=" + item.LibraryId + "&col=" + item.Id;

          //col.url = string.Format( "/Collection/{0}/{1}", item.Id, encodedTitle );
          col.url = string.Format( "/Library/Collection/{0}/{1}/{2}", item.LibraryId, item.Id, encodedTitle );

          col.id = item.Id;

          output.Add( col );
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
      public class JSONLibColSearchResultItem
      {
        public string title { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public string iconURL { get; set; }
        public int id { get; set; }
      }
      public class JSONLibrarySearchResult : JSONLibColSearchResultItem
      {
        public JSONLibrarySearchResult()
        {
          collections = new List<JSONLibColSearchResultItem>();
        }
        public string url { get; set; }
        public List<JSONLibColSearchResultItem> collections { get; set; }
      }

      #endregion

      /*
        #region main methods

        [WebMethod]
        public jsonLibCol UpdateLibrary( string identity, jsonLibCol input )
        {
            //Validate the User
            validUser user = ValidateUser( identity );

            //Validate the input
            try
            {
                input = ValidateJSONLibCol( input );
                if ( input == null )
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

            //Update the library
            try
            {
                Business.Library currentLibrary = user.library;  //User can only alter their own library since input's library ID is ignored
                currentLibrary.Title = input.title;
                currentLibrary.Description = input.description;
                currentLibrary.IsPublic = input.isPublic;
                currentLibrary.IsDiscoverable = input.isDiscoverable;
                libService.Update( currentLibrary );
            }
            catch
            {
                return null;
            }

            return input;
        }

        [WebMethod]
        public List<jsonLibCol> DoCollectionAction( string identity, collectionAction input )
        {
            //Validate the User
            validUser user = ValidateUser( identity );

            //Copy, Move, or Delete
            bool valid = false;
            string status = "";
            if ( input.action == "copy" )
            {
                //Protect against spoof data
                if ( !SectionsContainID( user.collections, input.targetCollection ) )
                {
                    return null;
                }

                //Do the copy
                int newID = libService.ResourceCopy( input.intID, input.targetCollection, user.user.Id, ref status );
                if ( newID != 0 )
                {
                    valid = true;
                    esManager.AddToCollection( input.intID.ToString(), input.targetCollection );
                    esManager.AddToLibrary( input.intID.ToString(), user.library.Id );
                }
            }
            else if ( input.action == "move" )
            {
                //Protect against spoof data
                if ( !SectionsContainID( user.collections, input.originCollection ) || !SectionsContainID( user.collections, input.targetCollection ) )
                {
                    return null;
                }

                //Do the move
                string message = libService.ResourceMove( input.originCollection, input.intID, input.targetCollection, user.user.Id, ref status );
                if ( message == "successful" )
                {
                    valid = true;
                    esManager.AddToCollection( input.intID.ToString(), input.targetCollection );
                    esManager.RemoveFromCollection( input.intID.ToString(), input.originCollection );
                }
            }
            else if ( input.action == "delete" )
            {
                //Protect against spoof data
                if ( !SectionsContainID( user.collections, input.originCollection ) )
                {
                    return null;
                }

                //Do the delete
                bool successful = libService.LibraryResourceDelete( input.originCollection, input.intID, ref status );
                if ( successful )
                {
                    valid = true;
                    esManager.RemoveFromCollection( input.intID.ToString(), input.originCollection );

                    //Remove it from our library (in elasticsearch) if need be
                    if ( !libService.IsResourceInLibrary( user.user, input.intID ) )
                    {
                        esManager.RemoveFromLibrary( input.intID.ToString(), user.library.Id );

                    }
                }
            }

            if ( valid )
            {
                return FetchJSONCollections( user.library.Id ); //Need to regenerate the collection list
            }
            else
            {
                return null;
            }
        }

        [WebMethod]
        public List<jsonLibCol> UpdateCollection( string identity, jsonLibCol input )
        {
            //Validate the User
            validUser user = ValidateUser( identity );

            //Validate the input
            try
            {
                input = ValidateJSONLibCol( input );
                if ( input == null )
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

            //Validate the input ID
            if ( !SectionsContainID( user.collections, input.id ) )
            {
                return null;
            }

            //Update the collection
            try
            {
                Business.LibrarySection section = libService.LibrarySectionGet( input.id );
                section.Title = input.title;
                section.Description = input.description;
                section.IsPublic = input.isPublic;
                libService.LibrarySectionUpdate( section );
            }
            catch
            {
                return null;
            }

            return FetchJSONCollections( user.library.Id ); //Need to regenerate the collection list
        }

        [WebMethod]
        public List<jsonLibCol> CreateCollection( string identity, jsonLibCol input )
        {
            //Validate the User
            validUser user = ValidateUser( identity );

            //Validate the input
            try
            {
                input = ValidateJSONLibCol( input );
                if ( input == null )
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

            //Create the Collection
            try
            {
                string status = "";
                Business.LibrarySection section = new Business.LibrarySection();
                section.Title = input.title;
                section.Description = input.description;
                section.IsPublic = input.isPublic;
                section.CreatedById = user.user.Id;
                section.LibraryId = user.library.Id;
                section.SectionTypeId = 3;
                section.IsDefaultSection = false;
                section.AreContentsReadOnly = false;
                section.ParentId = 0;
                libService.LibrarySectionCreate( section, ref status );
            }
            catch
            {
                return null;
            }

            return FetchJSONCollections( user.library.Id ); //Need to regenerate the collection list
        }

        [WebMethod]
        public List<jsonLibCol> MakeCollectionDefault( string identity, int input )
        {
            //Validate the User
            validUser user = ValidateUser( identity );

            //Validate the input
            if ( !SectionsContainID( user.collections, input ) )
            {
                return null;
            }

            //Defaultize the collection
            try
            {
                List<Business.LibrarySection> collections = libService.LibrarySectionsSelectList( user.library.Id, 2 );
                foreach ( Business.LibrarySection section in collections )
                {
                    //If the selected one is already the default, do nothing
                    if ( section.IsDefaultSection && section.Id == input ) { break; }
                    //When we find the new default, set it. The stored procedure handles unsetting the old one.
                    if ( section.Id == input )
                    {
                        section.IsDefaultSection = true;
                        libService.LibrarySectionUpdate( section );
                    }
                }
            }
            catch { return null; }
            return FetchJSONCollections( user.library.Id );
        }

        [WebMethod]
        public List<jsonLibCol> DeleteCollection( string identity, int input )
        {
            //Validate the User
            validUser user = ValidateUser( identity );

            //Validate the input
            if ( !SectionsContainID( user.collections, input ) )
            {
                return null;
            }

            //Delete the collection
            try
            {
                string status = "";
                libService.LibrarySection_Delete( input, ref status );
            }
            catch
            {
                return null;
            }

            return FetchJSONCollections( user.library.Id ); //Need to regenerate the collection list
        }

        [WebMethod]
        public string GetAvatar( string identity, string input )
        {
            validUser user = ValidateUser( identity );
            return user.library.ImageUrl;
        }
        [WebMethod]
        public string GetCollectionAvatar( string collectionId, string input )
        {
            //??????
            return "";
        }
        [WebMethod]
        public string GetSubscription( string identity, string input )
        {
            validUser user = ValidateUser( identity );

            int libraryID = int.Parse( input );

            if ( libService.IsSubcribedtoLibrary( libraryID, user.user.Id ) )
            {
                Business.ObjectSubscription membership = libService.LibrarySubscriptionGet( libraryID, user.user.Id );
                return membership.SubscriptionTypeId.ToString();
            }

            return "0";
        }

        [WebMethod]
        public string SetSubscription( string identity, jsonFollowing input )
        {
            validUser user = ValidateUser( identity );

            int libraryID = input.id;

            if ( input.type != 0 )
            {
                if ( GetSubscription( identity, input.id.ToString() ) != "0" )
                {
                    libService.LibrarySubScriptionUpdate( libraryID, user.user.Id, input.type );
                }
                else
                {
                    string status = "";
                    libService.LibrarySubScriptionCreate( libraryID, user.user.Id, input.type, ref status );
                }
            }

            return input.type.ToString();
        }

        #endregion

        #region utility methods

        protected jsonLibCol ValidateJSONLibCol( jsonLibCol input )
        {
            input.title = Cleanse( input.title );
            input.description = Cleanse( input.description );
            if ( input.title.Length >= 5 && input.description.Length >= 10 )
            {
                return input;
            }
            else
            {
                return null;
            }
        }

        protected string Cleanse( string input )
        {
            return input
                .Replace( "<", "" )
                .Replace( ">", "" )
                .Replace( "\\", "" )
                .Replace( "\"", "" );
            //.Replace( "'", "" );
        }

        public List<jsonLibCol> FetchJSONCollections( int libraryID )
        {
            List<Business.LibrarySection> collections = libService.LibrarySectionsSelectList( libraryID, 2 );
            return FetchJSONCollections( collections );
        }

        protected List<jsonLibCol> FetchJSONCollections( List<Business.LibrarySection> collections )
        {
            List<jsonLibCol> returnData = new List<jsonLibCol>();
            foreach ( Business.LibrarySection section in collections )
            {
                jsonLibCol json = new jsonLibCol();
                json.id = section.Id;
                json.title = section.Title;
                json.description = section.Description;
                json.isPublic = section.IsPublic;
                json.isDiscoverable = true;
                json.isDefaultCollection = section.IsDefaultSection;
                returnData.Add( json );
            }

            return returnData;
        }

        public validUser ValidateUser( string guid )
        {
            validUser validUser = new validUser();

            //Validate user
            Patron user = new Patron();
            user.IsValid = false;
            try
            {
                user = new AccountServices().GetByRowId( guid );
            }
            catch
            {
                return null;
            }
            if ( user.IsValid )
            {
                validUser.user = user;
            }
            else
            {
                return null;
            }

            //Get valid library/collections
            validUser.library = libService.GetMyLibrary( user );
            validUser.collections = libService.LibrarySectionsSelectList( validUser.library.Id, 2 );

            return validUser;
        }

        protected bool SectionsContainID( List<Business.LibrarySection> sections, int targetID )
        {
            bool flag = false;
            foreach ( Business.LibrarySection section in sections )
            {
                if ( section.Id == targetID )
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        #endregion

        #region subclasses

        public class validUser
        {
            //Validate the User
            public Patron user;
            public Business.Library library;
            public List<Business.LibrarySection> collections;
        }

        public class collectionAction
        {
            public int intID;
            public string action;
            public int targetCollection;
            public int originCollection;
        }

        public class jsonLibCol
        {
            public int id;
            public string title;
            public string description;
            public bool isPublic;
            public bool isDiscoverable;
            public bool isDefaultCollection;
        }

        public class jsonLibrary
        {
            public int id;
            public string title;
            public string description;
            public bool isPublic;
            public bool isDiscoverable;
        }
        public class jsonCollection
        {
            public int id;
            public string title;
            public string description;
            public bool isPublic;
            public bool IsReadOnly;
            public bool isDefaultCollection;

        }
        public class jsonFollowing
        {
            public int id;
            public int type;
        }

        #endregion*/
    }
}
