using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

using IB=ILPathways.Business;
using ILPathways.Utilities;
using Isle.BizServices;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using DataBaseHelper = LRWarehouse.DAL.BaseDataManager;
using Patron = LRWarehouse.Business.Patron;

namespace IOER.Services
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
      UtilityService utilService = new UtilityService();

      #region Library Search methods
        /// <summary>
        /// Do a library or collections search
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="text"></param>
        /// <param name="filters"></param>
        /// <param name="userGUID"></param>
        /// <param name="useSubscribedLibraries"></param>
        /// <param name="sort"></param>
        /// <param name="start"></param>
        /// <returns></returns>
       [WebMethod(EnableSession = true)]
      public string DoLibrariesSearch( string searchType, string text, List<JSONInputFilter> filters, string userGUID, bool useSubscribedLibraries, string sort, int start )
      {
        try
        {
          //Determine which sort option to use
            //Note added code to force org libs to show first is common cases
          var sortString = "";
          var defaultOrder = "";
          if ( searchType == "libraries" )
              defaultOrder = " libt.Title, ";
          else
              defaultOrder = " lib.LibraryTypeId DESC, ";//

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
                sortString = defaultOrder + "ResourceLastAddedDate" + order;
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
            var libraries = DoLibrariesSearch( text, filters, user, sortString, start, 120, useSubscribedLibraries, ref generatedFilter );
            var results = BuildLibrarySearchResults( libraries );

            return new UtilityService().ImmediateReturn( results, true, "okay", new { totalResults = libraries.Count() } ); //May want this to return the filter as part of the "extra" object

          }
          else if ( searchType == "collections" )
          {
            var collections = DoCollectionsSearch( text, filters, user, sortString, start, 150, useSubscribedLibraries, ref generatedFilter );
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
      public List<IB.Library> DoLibrariesSearch( string text, List<JSONInputFilter> filters, Patron user, string sort, int start, int max, bool useSubscribedLibraries, ref string generatedFilter )
      {
        text = FormHelper.CleanText( text.Trim() );

        string filter = FormatFilter( text, filters, user, useSubscribedLibraries );
        generatedFilter = filter;
        int totalRows = 0;

        //trace
        LoggingHelper.DoTrace( 5, string.Format("Library search. User: {0}, Filter: {1}", user.Id,  filter) );
        //Get the library search results
        var libraries = libService.LibrarySearchAsList( filter, sort, start, max, ref totalRows );

        ActivityBizServices.LibrariesSearchHit(filter, user, "Libraries");

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
      public List<IB.LibrarySection> DoCollectionsSearch( string text, List<JSONInputFilter> filters, Patron user, string sort, int start, int max, bool useSubscribedLibraries, ref string generatedFilter )
      {
        text = FormHelper.CleanText( text.Trim() );

        string filter = FormatFilter( text, filters, user, useSubscribedLibraries );
        generatedFilter = filter;
        int totalRows = 0;

        LoggingHelper.DoTrace( 5, string.Format( "Collection search. User: {0}, Filter: {1}", user.Id, filter ) );
        //Get the collection search results
        var collections = libService.LibrarySections_SearchAsList( filter, sort, start, max, ref totalRows );

        ActivityBizServices.LibrariesSearchHit(filter, user, "Collections");
        return collections;
      }

      #endregion
      #region =========================== Library approval Methods ============================================


      [WebMethod]
      public string GetPendingResourcesJSON( int libraryID, string userGUID )
      {
        try
        {
          var data = GetPendingResources( libraryID, userGUID );
          return utilService.ImmediateReturn( data, true, "okay", null ); 
        }
        catch(Exception ex)
        {
          return utilService.ImmediateReturn( null, false, ex.Message, null );
        }
      }

      public List<LibraryAdminPendingResourcesDTO> GetPendingResources( int libraryID, string userGUID )
      {
        var resources = new List<LibraryAdminPendingResourcesDTO>();

        //Get resources for this library, that this user can approve
        var rawData = new LibraryBizService().LibraryResource_SelectPendingResources( libraryID );

        foreach ( var item in rawData )
        {
          resources.Add( new LibraryAdminPendingResourcesDTO()
            {
              //libraryResource.Id
              id = item.Id,
              resourceIntId = item.ResourceIntId,
              submittingUserID = item.CreatedById,
              targetLibraryID = item.LibraryId,
              targetCollectionID = item.LibrarySectionId,
              submittingUserName = item.CreatedBy,
              targetCollectionTitle = item.CollectionTitle,
              submittedOnDate = item.Created.ToShortDateString(), //Not sure about this one
              title = item.Title,
              resourceURL = item.ResourceUrl,
            } 
          );
        }

        //Testing
        /*resources.Add( new LibraryAdminPendingResourcesDTO() { id = 450304, title = "Test Resource", resourceURL = "http://ioer.ilsharedlearning.org", submittedOnDate = DateTime.Now.ToShortDateString(), submittingUserName = "Nate Argo", targetCollectionTitle = "Target Collection", submittingUserID = 22, targetCollectionID = 2, targetLibraryID = 3 } );
        resources.Add( new LibraryAdminPendingResourcesDTO() { id = 450303, title = "Another Test Resource", resourceURL = "http://ioer.ilsharedlearning.org", submittedOnDate = DateTime.Now.ToShortDateString(), submittingUserName = "Nate Argo", targetCollectionTitle = "Target Collection", submittingUserID = 22, targetCollectionID = 2, targetLibraryID = 3 } );
        resources.Add( new LibraryAdminPendingResourcesDTO() { id = 450287, title = "One More Test Resource", resourceURL = "http://ioer.ilsharedlearning.org", submittedOnDate = DateTime.Now.ToShortDateString(), submittingUserName = "Mike Parsons", targetCollectionTitle = "Target Collection 3", submittingUserID = 2, targetCollectionID = 4, targetLibraryID = 3 } );*/

        return resources;
      }

      [WebMethod]
      public string SaveApprovalsJSON( int libraryID, string userGUID, List<LibraryAdminResourceApprovalsInput> approvals )
      {
        try
        {
          var data = SaveApprovals( libraryID, userGUID, approvals );
          return utilService.ImmediateReturn( data, true, "okay", null );
        }
        catch ( Exception ex )
        {
          return utilService.ImmediateReturn( null, false, ex.Message, null );
        }
      }

      public List<LibraryAdminPendingResourcesDTO> SaveApprovals( int libraryID, string userGUID, List<LibraryAdminResourceApprovalsInput> approvals )
      {
        string status ="";
        string reason = "";
        //Validate user
        var user = utilService.GetUserFromGUID(userGUID);
        if(user == null || !user.IsValid){
          return null;
        }

        //Save approvals
        foreach ( var item in approvals )
        {
          if ( item.approved )
          {
            libService.LibraryResource_Activate( item.id, user, ref status );
          }
          else
          {
              libService.LibraryResource_RejectSubmission( item.id, user, item.reason, ref status );
          }
        }

        return GetPendingResources( libraryID, userGUID );
      }

      public class LibraryAdminResourceApprovalsInput 
      {
        public int id { get; set; }
        public bool approved { get; set; }
        public string reason { get; set; }
      }

      public class LibraryAdminPendingResourcesDTO
      {
        public int id { get; set; }
        public int resourceIntId { get; set; }
        public int submittingUserID { get; set; }
        public int targetLibraryID { get; set; }
        public int targetCollectionID { get; set; }
        public string submittingUserName { get; set; }
        public string targetCollectionTitle { get; set; }
        public string submittedOnDate { get; set; }
        public string title { get; set; }
        public string resourceURL { get; set; }
      }

      /* Not sure if methods below this line (within this region) are in use or up to date anymore */

      /// <summary>
      /// Retrieve list of library resources requiring approvals
      /// </summary>
      /// <param name="text"></param>
      /// <param name="libraryId"></param>
      /// <param name="collectionId"></param>
      /// <param name="sort">necessary?, maybe by created date</param>
      /// <param name="start"></param>
      /// <returns></returns>
      [WebMethod]
      public string DoPendingApprovalsSearch( string text, string libraryId, string collectionId, string sort, int start, int pageSize )
      {
          try
          {
              //Determine which sort option to use
              //Note added code to force org libs to show first is common cases
              var sortString = "lib.DateAddedToCollection DESC, lr.Title";

              if ( sort != "" )
              {
                  var sortItems = sort.Split( '|' );
                  var order = ( sortItems[ 1 ] == "asc" ? " ASC" : " DESC" );
                  switch ( sortItems[ 0 ] )
                  {
                      case "title":
                          sortString = "lr.Title" + order;
                          break;
                      case "contact":
                          sortString = "lib.libResourceCreatedById" + order;
                          break;
                      case "created":
                          sortString = "lib.DateAddedToCollection" + order;
                          break;
                      default:
                          break;
                  }
              }
              if ( pageSize < 5 ) pageSize = 20;
              //Continue
              int totalResults = 0;
              int totalRows = 0;
              //var user = new UtilityService().GetUserFromGUID( userGUID );

              var resources = DoPendingApprovalsSearch2( text, libraryId, collectionId, sortString, start, pageSize, ref totalRows );
              var results = BuildPendingApprovalsResults( resources );

              return new UtilityService().ImmediateReturn( results, true, "okay", new { totalResults = resources.Count() } ); //May want this to return the filter as part of the "extra" object


          }
          catch ( Exception ex )
          {
              return new UtilityService().ImmediateReturn( "", false, ex.Message, null );
          }
      }
      /// <summary>
      /// Do actual resource pending approvals search
      /// </summary>
      /// <param name="text"></param>
      /// <param name="filters"></param>
      /// <param name="user">May not need</param>
      /// <param name="libraryId"></param>
      /// <param name="collectionId"></param>
      /// <param name="sort"></param>
      /// <param name="start"></param>
      /// <param name="max"></param>
      /// <param name="generatedFilter"></param>
      /// <returns></returns>
      public List<IB.LibraryResource> DoPendingApprovalsSearch2( string text, string libraryId, string collectionId, string sort, int start, int pageSize, ref int totalRows )
      {
          text = FormHelper.CleanText( text.Trim() );

          string filter = FormatPendingFilter( text, libraryId, collectionId);
          
          //Get the library search results
          var resources = libService.LibraryResource_SearchList( filter, sort, start, pageSize, ref totalRows );

          return resources;
      }
      public List<JSONLibResourcesSearchResult> BuildPendingApprovalsResults( List<IB.LibraryResource> resources )
      {
          var output = new List<JSONLibResourcesSearchResult>();

          foreach ( IB.LibraryResource item in resources )
          {
              var res = new JSONLibResourcesSearchResult();
              res.title = item.Title;
              res.description = "TBD";
              string encodedTitle = UtilityManager.UrlFriendlyTitle( res.title );

              res.resourceUrl = item.ResourceUrl;
              res.resourceId = item.ResourceIntId;
              res.imageUrl = ResourceBizService.GetResourceThumbnailImageUrl( item.ResourceUrl, item.ResourceIntId );
              res.collectionId = item.LibrarySectionId;
              res.author = string.Format( "TBD: {0}", item.CreatedById );

              output.Add( res );
          }

          return output;
      }
      protected string FormatPendingFilter( string text, string libraryId, string collectionId )
      {
          string booleanOperator = "AND";
          string filter = "";
          if ( libraryId.Length > 0 )
              filter = string.Format( "(LibraryId = {0} and lib.IsActive = 0)", libraryId );
          else
              filter = string.Format( "(LibrarySectionId = {0} and lib.IsActive = 0)", collectionId ); 

         // FormatKeyword( text, booleanOperator, ref filter );

          if ( new WebDALService().IsLocalHost() )
          {
              LoggingHelper.DoTrace( 6, "sql: " + filter );
          }

          return filter;
      }	//
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

          //this filter is only used where the current library search view is 'my followed libraries'
          //but probably ok to include explicit mbrs
        if ( useSubscribedLibraries )
        {
          filter = " ( lib.Id in (SELECT  LibraryId FROM [Library.Subscription] where UserId = " + user.Id + ")) ";
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
        string keyword = DataBaseHelper.HandleApostrophes( FormHelper.CleanText( text ) );
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
          //note these filters are only available to logged in users anyway
        switch ( privacyFilterID )
        {
          case 1: //all
            //should include all public, and if auth, all libraries where a member
            if ( user.IsValid && user.Id > 0 )
            {
                filter += DataBaseHelper.FormatSearchItem( filter, string.Format( "(lib.PublicAccessLevel > 1) OR (lib.CreatedById = {0}) OR  (lib.Id in (Select LibraryId from [Library.Member] where userid = {0} ))", user.Id ), booleanOperator );
            }
            else
            {
                filter += DataBaseHelper.FormatSearchItem( filter, "lib.PublicAccessLevel > 1", booleanOperator );
            }
            break;
          case 2: //all public
            filter += DataBaseHelper.FormatSearchItem( filter, "lib.PublicAccessLevel > 1", booleanOperator );
                //should include member libraries as well??
            //if ( user.IsValid && user.Id > 0 )
            //{
            //    filter += DataBaseHelper.FormatSearchItem( filter, string.Format( "(lib.CreatedById = {0}) OR  (lib.Id in (Select LibraryId from [Library.Member] where userid = {0} ))", user.Id ), booleanOperator );
            //}
            break;
          case 3: //only private - Not used. Seems wrong anyway
            filter += DataBaseHelper.FormatSearchItem( filter, "lib.PublicAccessLevel = 1", booleanOperator );
            break;
          case 4:   //where a member
            if ( user.IsValid && user.Id > 0 )
            {
                filter += DataBaseHelper.FormatSearchItem( filter, string.Format( "(lib.CreatedById = {0}) OR  (lib.Id in (Select LibraryId from [Library.Member] where userid = {0} ))", user.Id ), booleanOperator );
            }
            else { }
            break;
          default:
                //should always get where member or creator
            if ( user.IsValid && user.Id > 0 )
            {
                filter += DataBaseHelper.FormatSearchItem( filter, string.Format( "(lib.CreatedById = {0}) OR  (lib.Id in (Select LibraryId from [Library.Member] where userid = {0} ))", user.Id ), booleanOperator );
            }
              break;
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

      public List<JSONLibrarySearchResult> BuildLibrarySearchResults( List<IB.Library> libraries )
      {
        var output = new List<JSONLibrarySearchResult>();

        foreach ( IB.Library item in libraries )
        {
          var lib = new JSONLibrarySearchResult();
          lib.title = item.Title;
          string encodedTitle = UtilityManager.UrlFriendlyTitle( lib.title );

          lib.description = item.Description;
          lib.iconURL = item.ImageUrl;
          if ( item.Organization != null && item.Organization.Length > 2 )
          {
              lib.organization = item.Organization;
          }
          else
              lib.organization = "";

          //lib.url = "/Library/?id=" + item.Id;
          lib.url = string.Format("/Library/{0}/{1}",  item.Id,encodedTitle);
          lib.id = item.Id;

          var collections = libService.LibrarySectionsSelectList( item.Id, 1 );
          foreach ( IB.LibrarySection section in collections )
          {
            var col = new JSONLibColSearchResultItem();
            col.title = section.Title;
            string encodedColTitle = UtilityManager.UrlFriendlyTitle( col.title );

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


      public List<JSONLibrarySearchResult> BuildCollectionSearchResults( List<IB.LibrarySection> collections )
      {
        var output = new List<JSONLibrarySearchResult>();

        foreach ( IB.LibrarySection item in collections )
        {
          var col = new JSONLibrarySearchResult();
          col.title = item.Title;
          string encodedTitle = UtilityManager.UrlFriendlyTitle( col.title );

          col.description = item.Description;
          col.iconURL = item.ImageUrl;
          col.libraryTitle = item.LibraryTitle;
          col.libraryUrl = string.Format( "/Library/{0}/{1}", item.LibraryId, UtilityManager.UrlFriendlyTitle( item.LibraryTitle ) );
          col.organization = "";
          if ( item.ParentLibrary != null && item.ParentLibrary.OrgId > 0 )
          {
              //TODO - get organization
              //col.organization = item.Organization;
              col.organization = item.ParentLibrary.Organization;
          }
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
        public string libraryTitle { get; set; }
        public string libraryUrl { get; set; }
        public string organization { get; set; }
        public List<JSONLibColSearchResultItem> collections { get; set; }
      }

      public class JSONLibResourcesSearchResult 
      {
          public JSONLibResourcesSearchResult()
          {
          }
          public string title { get; set; }
          public string description { get; set; }
          public string imageUrl { get; set; }
          public string resourceUrl { get; set; }
          public int resourceId { get; set; }
          public int collectionId { get; set; }

          public string author { get; set; }
          
      }

      #endregion

    }
}
