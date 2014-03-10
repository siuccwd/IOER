using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

//TODO - remove direct calls to DAL and go thru biz services layer
using LRWarehouse.DAL;
using LRWarehouse.Business;
using System.Web.Script.Serialization;
using System.Data;
using Isle.BizServices;
using ILPathways.Utilities;

using AccountManager = Isle.BizServices.AccountServices;
using DBM = Isle.BizServices.ResourceBizService;

namespace ILPathways.Services
{
    /// <summary>
    /// Summary description for DetailService6
    /// </summary>
    [WebService( Namespace = "http://tempuri.org/" )]
    [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
    [System.ComponentModel.ToolboxItem( false )]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class DetailService6 : System.Web.Services.WebService
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer( null );
        UtilityService utilService = new UtilityService();

        #region Data getters
        [WebMethod]
        public jsonDetail LoadAllResourceData( int vid, string userGUID )
        {
            var detail = new jsonDetail();

            try
            {
                //Get most of the data
                ResourceVersion version = new ResourceVersionManager().Get( vid );
                if ( version.IsValid == false || version.IsActive == false ) { throw new Exception(); }

                //Single value fields
                detail.versionID = vid;
                detail.intID = version.ResourceIntId;
                detail.title = version.Title;
                //OR??? detail.title = version.SortTitle;

                detail.url = version.ResourceUrl;
                detail.description = version.Description;
                detail.requires = version.Requirements;
                detail.created = version.Created.ToShortDateString();
                detail.creator = version.Creator;
                detail.publisher = version.Publisher;
                detail.submitter = version.Submitter;
                detail.isBasedOnUrl = GetIsBasedOnURL( detail.intID );
                detail.lrDocID = version.LRDocId;

                //Multi value fields with controlled vocabulary
                detail.itemType = GetMVF( "itemType", detail.intID, false, "Item Type" );
                detail.accessRights = GetMVF( "accessRights", detail.intID, false, "Access Rights" );
                detail.language = GetMVF( "language", detail.intID, false, "Language" );
                detail.careerCluster = GetMVF( "careerCluster", detail.intID, true, "Career Cluster" );
                detail.endUser = GetMVF( "endUser", detail.intID, true, "End User" );
                detail.groupType = GetMVF( "groupType", detail.intID, true, "Group Type" );
                detail.resourceType = GetMVF( "resourceType", detail.intID, true, "Resource Type" );
                detail.mediaType = GetMVF( "mediaType", detail.intID, true, "Media Type" );
                detail.educationalUse = GetMVF( "educationalUse", detail.intID, true, "Educational Use" );
                detail.gradeLevel = GetMVF( "gradeLevel", detail.intID, true, "Grade Levels" );

                //Specially-formatted data
                detail.usageRights = GetUsageRights( version.Rights );
                detail.timeRequired = GetTimeRequired( version.TypicalLearningTime );
                GetStandardsAndRubrics( ref detail, detail.intID, userGUID );
                detail.subject = GetFreeText( detail.intID, "subject" );
                detail.keyword = GetFreeText( detail.intID, "keyword" );
                detail.libColInfo = GetLibColInfo( detail.intID, userGUID );

                //Paradata
                detail.paradata = GetParadata( detail.intID, userGUID );
                detail.comments = GetComments( detail.intID );
            }
            catch ( Exception ex )
            {
                //return null;
            }

            return detail;
        }

        [WebMethod]
        public jsonMVF GetMVF( string table, int intID, bool isMultiSelect, string printTitle )
        {
            var dataManager = new ResourceDataManager();
            jsonMVF list = new jsonMVF();
            try
            {
                DataSet ds = dataManager.SelectedCodes( ResourceDataManager.ResourceDataSubclassFinder.getSubclassByName( table ), intID );
                if ( DBM.DoesDataSetHaveRows( ds ) )
                {
                    list.name = table;
                    list.isMultiSelect = isMultiSelect;
                    list.printTitle = printTitle;
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        var item = new jsonMVFItem();
                        item.id = int.Parse( DBM.GetRowColumn( dr, "Id" ) );
                        item.selected = bool.Parse( DBM.GetRowColumn( dr, "IsSelected" ) );
                        item.title = DBM.GetRowColumn( dr, "Title" );
                        list.items.Add( item );
                    }
                }
            }
            catch ( System.Threading.ThreadAbortException tae )
            {
                //ignore
            }
            catch ( Exception ex )
            {

            }
            return list;
        }

        [WebMethod]
        public List<BlankCBXL> GetCodeTables()
        {
            var dataManager = new ResourceDataManager();
            string[] tables = new string[] { "accessRights", "careerCluster", "educationalUse", "mediaType", "gradeLevel", "groupType", "endUser", "itemType", "language", "resourceType" };
            var list = new List<BlankCBXL>();

            var count = 0;
            foreach ( string name in tables )
            {
                //Get the data
                DataSet ds = dataManager.GetCodetable( ResourceDataManager.ResourceDataSubclassFinder.getSubclassByName( name ) );

                //Convert it to JSON
                var cbxl = new BlankCBXL();
                cbxl.name = name;
                count++;
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    var item = new jsonCBXLItem();
                    item.id = int.Parse( DBM.GetRowColumn( dr, "Id" ) );
                    item.selected = false;
                    item.title = DBM.GetRowColumn( dr, "Title" );
                    cbxl.items.Add( item );
                }
                list.Add( cbxl );
            }
            return list;
        }

        [WebMethod]
        public string GetThumbnail( int intID, string url )
        {
          return new UtilityService().GetThumbnail( intID, url );
        }
        #endregion

        #region Data setters
        [WebMethod]
        public string DoStandardRating( string userGUID, int standardID, int rating, int versionID )
        {
            int userID = GetUserID( userGUID );
            if ( userID == 0 ) { return null; }

            int intID = utilService.GetIntIDFromVersionID( versionID );

            return serializer.Serialize( new ResourceEvaluationManager().RateStandard( userID, standardID, rating, intID ) );
        }

        [WebMethod]
        public string AddToCollection( string userGUID, int libraryID, int collectionID, int intID )
        {
          string status = "";
          //Get/validate the user
          var user = GetUser( userGUID );
          if ( user.IsValid == false ) { return null; }
          //Init library service
          var libService = new Isle.BizServices.LibraryBizService();
          //Ensure this collection exists
          var collection = libService.LibrarySectionGet( collectionID );
          if ( collection.IsValid == false ) { return null; }
          //Ensure this collection belongs to this user
          var myCollections = libService.LibrarySections_SelectListWithContributeAccess( libraryID, user.Id );
          bool foundCollection = false;
          foreach ( ILPathways.Business.LibrarySection section in myCollections )
          {
            if ( section.Id == collectionID )
            {
              foundCollection = true;
              break;
            }
          }

          //If everything checks out, do the create
          if ( foundCollection )
          {
            var libResource = new ILPathways.Business.LibraryResource();
            libResource.LibrarySectionId = collection.Id;
            libResource.ResourceIntId = intID;
            libResource.CreatedById = user.Id;
            libResource.LibraryId = libraryID;
            int test = libService.LibraryResourceCreate( libResource, user, ref status );
            if ( test > -1 )
            {
              //Get the refreshed library info and return it
              var output = GetLibColInfo( intID, userGUID );
              return serializer.Serialize( output );
            }
          }
          return null;
        }

        [WebMethod]
        public string PostComment( string userGUID, string text, int versionID )
        {
            var response = new jsonCommentResponse();
            string status = "";
            int userID = GetUserID( userGUID );
            if ( userID == 0 ) { return null; }

            int intID = utilService.GetIntIDFromVersionID( versionID );
            if ( intID == 0 ) { return null; }

            text = ValidateText( text, 10, ref status );
            if ( status != "okay" )
            {
                response.validComment = false;
                response.status = "Sorry, Invalid comment: " + status;
            }

            if ( response.validComment )
            {
                var commentManager = new ResourceCommentManager();
                var comment = new ResourceComment();
                comment.ResourceIntId = intID;
                comment.Comment = text;
                comment.CreatedById = userID;
                comment.CreatedBy = new PatronManager().Get( userID ).FullName();
                comment.IsActive = true;

                //var commentID = 0;
                var commentID = commentManager.Create( comment, ref status );
                if ( commentID > 0 )
                {
                    new ElasticSearchManager().AddComment( intID.ToString() );
                    response.validComment = true;
                    response.comments = GetComments( intID );
                }
                else
                {
                    response.validComment = false;
                    response.status = status;
                }
            }

            return serializer.Serialize( response );
        }


        [WebMethod]
        public string FindMoreLikeThis( int versionID, string parameters, int minFieldMatches )
        {
            return new ElasticSearchManager().FindMoreLikeThis( versionID, parameters.Split( ',' ), minFieldMatches, 12, 1 );
        }

        [WebMethod]
        public string AddLike( string userGUID, int versionID )
        {
            return serializer.Serialize( AddLikeDislike( userGUID, versionID, true ) );
        }

        [WebMethod]
        public string AddDislike( string userGUID, int versionID )
        {
            return serializer.Serialize( AddLikeDislike( userGUID, versionID, false ) );
        }

        [WebMethod]
        public string AddClickThrough( string userGUID, int versionID )
        {
            var intID = utilService.GetIntIDFromVersionID( versionID );
            new ResourceViewManager().Create( intID, GetUserID( userGUID ) );
            new ElasticSearchManager().AddResourceView( intID.ToString() );

            return serializer.Serialize( GetClickThroughs( intID ) );
        }

        [WebMethod]
        public string UpdateResource( int versionID, string userGUID, InputResource update )
        {
            //validate user and resource
            var userID = GetUserID( userGUID );
            if ( userID == 0 ) { return null; }
            var intID = utilService.GetIntIDFromVersionID( versionID );
            if ( intID == 0 ) { return null; }
            var resourceManager = new ResourceManager();
            var versionManager = new ResourceVersionManager();
            var resource = resourceManager.Get( intID );
            resource.Version = versionManager.Get( versionID );
            if ( resource.IsValid == false ) { return null; }
            var user = new AccountManager().Get( userID );

            //validate text inputs
            string status = "";
            update.subject = FixFreeText( update.subject );
            update.keyword = FixFreeText( update.keyword );

            //do admin-only updates
            var permissions = SecurityManager.GetGroupObjectPrivileges( user, "ILPathways.LRW.Pages.ResourceDetail" );
            if ( permissions.CreatePrivilege > ( int )ILPathways.Business.EPrivilegeDepth.State )
            {
                //Only change these if they pass validation
                update.title = ValidateText( update.title, 8, ref status );
                resource.Version.Title = ( status == "okay" ? update.title : resource.Version.Title );

                update.description = ValidateText( update.description, 10, ref status );
                resource.Version.Description = ( status == "okay" ? update.description : resource.Version.Description );

                update.requires = ValidateText( update.requires, 0, ref status );
                resource.Version.Requirements = ( status == "okay" ? update.requires : resource.Version.Requirements );
            }
            else
            {
                update.title = resource.Version.Title;
                update.description = resource.Version.Description;
                update.requires = resource.Version.Requirements;
            }

            //do remaining updates
            if ( permissions.CanUpdate() )
            {
                resource.Version.Rights = update.usageRights;
                resource.Version.TypicalLearningTime = update.timeRequired;
                resource.Version.AccessRights = update.accessRights.value;
                resource.Version.AccessRightsId = update.accessRights.id;
                versionManager.UpdateById( resource.Version );

                ProcessMVF( update.gradeLevel, userID, intID, "gradeLevel" );
                ProcessMVF( update.careerCluster, userID, intID, "careerCluster" );
                ProcessMVF( update.endUser, userID, intID, "endUser" );
                ProcessMVF( update.groupType, userID, intID, "groupType" );
                ProcessMVF( update.resourceType, userID, intID, "resourceType" );
                ProcessMVF( update.mediaType, userID, intID, "mediaType" );
                ProcessMVF( update.educationalUse, userID, intID, "educationalUse" );

                new ResourceLanguageManager().Create( intID, update.language.id, update.language.value, userID, ref status );
                if ( update.itemType.id != 0 )
                {
                    new ResourceItemTypeManager().Create( intID, update.itemType.id, userID, ref status );
                }

                var standardsManager = new ResourceStandardManager();
                foreach ( InputStandard standard in update.standards )
                {
                    ResourceStandard item = new ResourceStandard();
                    item.CreatedById = userID;
                    item.ResourceIntId = intID;
                    item.StandardId = standard.id;
                    item.StandardNotationCode = standard.code;
                    item.AlignmentTypeCodeId = standard.alignment.id;
                    item.AlignmentTypeValue = standard.alignment.value;
                    standardsManager.Create( item, ref status );
                }

                AddFreeTextItems( new ResourceKeywordManager(), update.keyword, userID, intID );
                AddFreeTextItems( new ResourceSubjectManager(), update.subject, userID, intID );
            }

            //return updated resource
            return serializer.Serialize( LoadAllResourceData( versionID, userGUID ) );
        }

        [WebMethod]
        public string ReportIssue( string issue, string userGUID, int versionID )
        {
            string status = "";
            issue = ValidateText( issue, 5, ref status );
            if ( issue != "" )
            {
                var user = GetUser( userGUID );
                if ( user.IsValid )
                {
                    //string url = "http://ioer.ilsharedlearning.org/IOER/" + versionID + "/";
                    string url = "/IOER/" + versionID + "/";
                    url = UtilityManager.FormatAbsoluteUrl( url, false );
                    string toEmail = UtilityManager.GetAppKeyValue( "contactUsMailTo", "info@ilsharedlearning.org" );
                    string subject = "Reporting an issue!";
                    string body = "<p>" + issue + "</p>" +
                    "<br/>IOER: " + string.Format( "<a href='{0}'>Resource Detail url</a>", url ) +
                    "<br/>From: " + user.EmailSignature();
                    string from = user.Email;
                    EmailManager.SendEmail( toEmail, from, subject, body, "", "" );

                    return "Your report has been received. Thank you.";
                }
            }

            return "There was a problem processing your request. Please ensure you are logged in.";
        }

        [WebMethod]
        public string DeactivateResource( string userGUID, int versionID )
        {
            var user = GetUser( userGUID );
            var permissions = SecurityManager.GetGroupObjectPrivileges( user, "ILPathways.LRW.Pages.ResourceDetail" );
            if ( permissions.CreatePrivilege > ( int ) ILPathways.Business.EPrivilegeDepth.State )
            {
                try {
                    new ResourceVersionManager().SetActiveState( false, versionID );
                    string response = "";
                    var esManager = new ElasticSearchManager();
                    new ElasticSearchManager().DeleteByVersionID( versionID, ref response );

                    ActivityBizServices.SiteActivityAdd( "Resource", "Deactivate", string.Format( "Resource VersionID: {0} was deactivated by {1}", versionID, user.FullName() ), user.Id, 0, versionID );

                    return "Resource Deactivated. Refresh the Page to confirm.";
                }
                catch(Exception ex)
                {
                    return "There was a problem deactivating the Resource: " + ex.ToString();
                }
            }
            else
            {
                return "You do not have permission to deactivate Resources!";
            }
        }

        #endregion

        #region helper methods

        public string AddFreeTextItems( dynamic manager, List<string> input, int userID, int intID )
        {
            string status = "";
            foreach ( string item in input )
            {
                var word = new ResourceChildItem();
                word.CreatedById = userID;
                word.ResourceIntId = intID;
                word.OriginalValue = item;
                manager.Create( word, ref status );
            }
            return status;
        }

        public string GetIsBasedOnURL( int intID )
        {
            try
            {
                string url = new ResourceRelatedUrlManager().Select( intID ).Tables[ 0 ].Rows[ 0 ][ 0 ].ToString();
                if ( url == "http://" ) { return ""; }
                return url;
            }
            catch ( Exception ex )
            {
                return "";
            }
        }

        public jsonUsageRights GetUsageRights( string rights )
        {
            var output = new jsonUsageRights();
            //DataSet ds = CodeTableManager.ConditionsOfUse_Select(); //Not enough data
            DataSet ds = DBM.DoQuery( "IF( SELECT COUNT(*) FROM [ConditionOfUse] WHERE [Url] = '" + rights + "' ) = 0 SELECT * FROM [ConditionOfUse] WHERE [Url] IS NULL ELSE SELECT * FROM [ConditionOfUse] WHERE [Url] = '" + rights + "'" );
            if ( DBM.DoesDataSetHaveRows( ds ) )
            {
                DataRow dr = ds.Tables[ 0 ].Rows[ 0 ];
                output.usageRightsText = Get( dr, "Summary" );
                output.usageRightsValue = int.Parse( Get( dr, "Id" ) );
                output.usageRightsDescription = Get( dr, "Title" );
                output.usageRightsURL = rights;
                output.usageRightsIconURL = Get( dr, "IconUrl" );
                output.usageRightsMiniIconURL = Get( dr, "MiniIconUrl" );
            }
            return output;
        }

        public string GetTimeRequired( string timeRequired )
        {
            string result = "";
            if ( timeRequired == null || timeRequired == "" )
            {
                return "Unknown";
            }
            if ( timeRequired.IndexOf( "P" ) == 0 || timeRequired.IndexOf( "PT" ) == 0 )
            {
                return timeRequired
                    .Replace( "P", "" )
                    .Replace( "PT", "" )
                    .Replace( "D", " Days " )
                    .Replace( "H", " Hours " )
                    .Replace( "M", " Minutes" );
            }
            else
            {
                result = timeRequired;
            }
            return result;
        }

        public jsonFreeText GetFreeText( int intID, string item )
        {
            jsonFreeText output = new jsonFreeText();
            List<ResourceChildItem> items = new List<ResourceChildItem>();
            output.name = item;
            if ( item == "subject" )
            {
                //Need to make this table-driven
                output.recommended = new List<string> { "Mathematics", "English Language Arts", "Science", "Social Studies", "Arts", "World Languages", "Health", "Physical Education", "Technology" };
                items = new ResourceSubjectManager().Select( intID );
            }
            else if ( item == "keyword" )
            {
                items = new ResourceKeywordManager().Select( intID );
            }

            foreach ( ResourceChildItem word in items )
            {
                output.items.Add( word.OriginalValue );
            }

            return output;
        }

        public List<jsonComment> GetComments( int intID )
        {
            var list = new List<jsonComment>();
            DataSet ds = new ResourceCommentManager().Select( intID );
            if ( DBM.DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    var comment = new jsonComment();
                    comment.avatarURL = ""; //To be implemented once User Avatars are a thing
                    comment.commentDate = DateTime.Parse( Get( dr, "Created" ) ).ToShortDateString();
                    comment.commentID = int.Parse( Get( dr, "Id" ) );
                    comment.commentText = Get( dr, "Comment" );
                    comment.name = Get( dr, "CreatedBy" );
                    list.Add( comment );
                }
            }

            return list;
        }

        public jsonDetailLibraryInfo GetLibColInfo( int intID, string userGUID )
        {
          var output = new jsonDetailLibraryInfo();
          var user = new AccountManager().GetByRowId( userGUID );
          var libService = new Isle.BizServices.LibraryBizService();

          //Get all of the libraries I have access to
          if ( user.Id > 0 )
          {
            var myLibraries = libService.Library_SelectListWithContributeAccess( user.Id );
            foreach ( ILPathways.Business.Library item in myLibraries )
            {
              //Fill out library data
              var library = new jsonDetailLibrary();
              library.isPersonalLibrary = item.LibraryTypeId == 1;
              library.isInLibrary = libService.IsResourceInLibrary( item.Id, intID );
              library.id = item.Id;
              library.title = item.Title;
              library.avatarURL = item.ImageUrl;
              
              //Get collections for this library
              var collections = libService.LibrarySections_SelectListWithContributeAccess( item.Id, user.Id );
              foreach ( ILPathways.Business.LibrarySection col in collections )
              {
                var collection = new jsonDetailLibCol();
                collection.title = col.Title;
                collection.id = col.Id;
                collection.avatarURL = col.ImageUrl;
                if ( col.IsDefaultSection )
                {
                  library.defaultCollectionID = col.Id;
                }
                library.collections.Add( collection );
              }

              output.myLibraries.Add( library );
            }
          }

          //Get all of the Libraries this Resource appears in
          var inLibraries = libService.GetAllLibrariesWithResource( intID );
          foreach ( ILPathways.Business.Library item in inLibraries )
          {
            jsonDetailLibCol library = new jsonDetailLibCol();
            library.title = item.Title;
            library.id = item.Id;
            library.avatarURL = (item.ImageUrl == "defaultUrl" ? "/images/isle.png" : item.ImageUrl );

            output.libraries.Add( library );
          }

          return output;
        }

        public jsonParadata GetParadata( int intID, string userGUID )
        {
          var output = new jsonParadata();

          string testStatus = "";
          var summary = new ResourceLikeSummaryManager().GetForDisplay( intID, GetUserID( userGUID ), ref testStatus );
          output.likes = summary.LikeCount;
          output.dislikes = summary.DislikeCount;
          output.iLikeThis = summary.YouLikeThis;
          output.iDislikeThis = summary.YouDislikeThis;

          output.clickThroughs = GetClickThroughs( intID );
          output.favorites = new Isle.BizServices.LibraryBizService().GetAllLibrariesWithResource( intID ).Count;

          return output;
        }

        public jsonParadata AddLikeDislike( string userGUID, int versionID, bool isLike )
        {
            int intID = utilService.GetIntIDFromVersionID( versionID );
            int userID = GetUserID( userGUID );
            if ( userID == 0 ) { return null; }
            var manager = new ResourceLikeManager();
            var like = new ResourceLike();

            //Need a proc for this
            DataSet ds = DBM.DoQuery( "SELECT * FROM [Resource.Like] WHERE [ResourceIntId] = " + intID + " AND [CreatedById] = " + userID );
            if ( DBM.DoesDataSetHaveRows( ds ) )
            {
              return GetParadata( intID, userGUID );
            }

            like.IsLike = isLike;
            like.CreatedById = userID;
            like.ResourceIntId = intID;
            string status = "";

            manager.Create( like, ref status );

            return GetParadata( intID, userGUID );
        }

        public int GetClickThroughs( int intID )
        {
            int count = 0;
            //Need a proc for this
            DataSet ds = DBM.DoQuery( "SELECT COUNT(*) AS 'Count' FROM [Resource.View] WHERE ResourceIntId = " + intID );
            if ( DBM.DoesDataSetHaveRows( ds ) )
            {
                count = GetInt( ds.Tables[ 0 ].Rows[ 0 ], "Count" );
            }
            return count;
        }

        public void GetStandardsAndRubrics( ref jsonDetail input, int intID, string userGUID )
        {
            try
            {
                ResourceRatings ratings = new ResourceEvaluationManager().GetRatingsForResource( intID, GetUserID( userGUID ) );
                input.standards = ratings.standardRatings;
                input.rubrics = ratings.rubricRatings;
            }
            catch ( Exception ex ) //need to fix this
            {
                ResourceRatings ratings = new ResourceRatings();
                input.standards = ratings.standardRatings;
                input.rubrics = ratings.rubricRatings;
            }
        }
        protected string Get( DataRow dr, string column )
        {
            return DBM.GetRowPossibleColumn( dr, column );
        }
        protected int GetInt( DataRow dr, string column )
        {
            try
            {
                return int.Parse( DBM.GetRowPossibleColumn( dr, column ) );
            }
            catch
            {
                return 0;
            }
        }
        protected int GetUserID( string userGUID )
        {
            var user = new AccountManager().GetByRowId( userGUID );
            if ( user.IsValid )
            {
                return user.Id;
            }
            else
            {
                return 0;
            }
        }
        protected Patron GetUser( string userGUID )
        {
            return userGUID == "" ? new Patron() { IsValid = false } : new AccountManager().GetByRowId( userGUID );
        }
        protected string ValidateText( string text, int minimumLength, ref string status )
        {
            status = "";
            text = text.Replace( "<", "" ).Replace( ">", "" ).Replace( "\"", "'" );
            text = FormHelper.CleanText( text );
            if ( BadWordChecker.CheckForBadWords( text ) )
            {
                text = "";
                status += "Inappropriate language detected. ";
            }
            if ( text.Length < minimumLength )
            {
                text = "";
                status = status + "Too short. ";
            }
            if ( status == "" ) { status = "okay"; }
            return text;
        }

        protected List<string> FixFreeText( List<string> items )
        {
            List<string> output = new List<string>();
            string status = "";
            foreach ( string item in items )
            {
                var text = ValidateText( item, 3, ref status );
                if ( text != "" )
                {
                    output.Add( text );
                }
            }
            return output;
        }

        protected void ProcessMVF( List<IDValuePair> input, int userID, int intID, string table )
        {
            var manager = new ResourceDataManager();
            foreach ( IDValuePair pair in input )
            {
                manager.Create( ResourceDataManager.ResourceDataSubclassFinder.getSubclassByName( table ), intID, pair.id, "", userID );
            }
        }
        #endregion

        #region subclasses
        //Details
        public class BlankCBXL
        {
            public BlankCBXL() { items = new List<jsonCBXLItem>(); }
            public string name { get; set; }
            public List<jsonCBXLItem> items { get; set; }
        }
        public class jsonCBXLItem
        {
          public string title { get; set; }
          public int id { get; set; }
          public bool selected { get; set; }
        }
        public class jsonDetail
        {
            //Initialization
            public jsonDetail()
            {
                itemType = new jsonMVF();
                accessRights = new jsonMVF();
                language = new jsonMVF();
                careerCluster = new jsonMVF();
                endUser = new jsonMVF();
                groupType = new jsonMVF();
                resourceType = new jsonMVF();
                mediaType = new jsonMVF();
                educationalUse = new jsonMVF();
                gradeLevel = new jsonMVF();
                standards = new List<ResourceRating>();
                rubrics = new List<ResourceRating>();
                comments = new List<jsonComment>();
                usageRights = new jsonUsageRights();
                subject = new jsonFreeText();
                keyword = new jsonFreeText();
                libColInfo = new jsonDetailLibraryInfo();
                paradata = new jsonParadata();
                comments = new List<jsonComment>();
            }

            //Single value items
            public int versionID { get; set; }
            public int intID { get; set; }
            public string title { get; set; }
            public string url { get; set; }
            public string description { get; set; }
            public string requires { get; set; }
            public string created { get; set; }
            public string creator { get; set; }
            public string publisher { get; set; }
            public string submitter { get; set; }
            public string isBasedOnUrl { get; set; }
            public string timeRequired { get; set; }
            public string lrDocID { get; set; }

            //Multi value fields with controlled vocabulary
            public jsonMVF itemType { get; set; }
            public jsonMVF accessRights { get; set; }
            public jsonMVF language { get; set; }
            public jsonMVF careerCluster { get; set; }
            public jsonMVF endUser { get; set; }
            public jsonMVF groupType { get; set; }
            public jsonMVF resourceType { get; set; }
            public jsonMVF mediaType { get; set; }
            public jsonMVF educationalUse { get; set; }
            public jsonMVF gradeLevel { get; set; }

            //Specially-formatted data
            public jsonUsageRights usageRights { get; set; }
            public List<ResourceRating> standards { get; set; }
            public List<ResourceRating> rubrics { get; set; }
            public jsonFreeText subject { get; set; }
            public jsonFreeText keyword { get; set; }
            public jsonDetailLibraryInfo libColInfo { get; set; }

            //Paradata
            public jsonParadata paradata { get; set; }
            public List<jsonComment> comments { get; set; }

        }
        public class jsonUsageRights
        {
            public string usageRightsText { get; set; }
            public int usageRightsValue { get; set; }
            public string usageRightsDescription { get; set; }
            public string usageRightsURL { get; set; }
            public string usageRightsIconURL { get; set; }
            public string usageRightsMiniIconURL { get; set; }
        }
        public class jsonMVF
        {
            public jsonMVF()
            {
                items = new List<jsonMVFItem>();
                isMultiSelect = true;
            }
            public bool isMultiSelect { get; set; }
            public string name { get; set; }
            public string printTitle { get; set; }
            public List<jsonMVFItem> items { get; set; }
        }
        public class jsonMVFItem
        {
            public string title { get; set; }
            public int id { get; set; }
            public bool selected { get; set; }
        }
        public class jsonFreeText
        {
            public jsonFreeText()
            {
                recommended = new List<string>();
                items = new List<string>();
            }
            public string name { get; set; }
            public List<string> recommended { get; set; }
            public List<string> items { get; set; }
        }
        public class jsonComment
        {
            public string name { get; set; }
            public int commentID { get; set; }
            public string avatarURL { get; set; }
            public string commentDate { get; set; }
            public string commentText { get; set; }
        }
        public class jsonParadata
        {
            public int likes { get; set; }
            public int dislikes { get; set; }
            public int favorites { get; set; }
            public int clickThroughs { get; set; }
            public bool iLikeThis { get; set; }
            public bool iDislikeThis { get; set; }
        }
        public class jsonDetailLibraryInfo
        {
            public jsonDetailLibraryInfo()
            {
                libraries = new List<jsonDetailLibCol>();
                myLibraries = new List<jsonDetailLibrary>();
            }
            public List<jsonDetailLibCol> libraries { get; set; }
            public List<jsonDetailLibrary> myLibraries { get; set; }
        }
        public class jsonDetailLibrary : jsonDetailLibCol
        {
          public jsonDetailLibrary()
          {
            collections = new List<jsonDetailLibCol>();
          }
          public bool isPersonalLibrary { get; set; }
          public bool isInLibrary { get; set; }
          public int defaultCollectionID { get; set; }
          public List<jsonDetailLibCol> collections { get; set; }
        }
        public class jsonDetailLibCol
        {
            public int id { get; set; }
            public string title { get; set; }
            public string avatarURL { get; set; }
        }
        public class jsonCommentResponse
        {
            public jsonCommentResponse()
            {
                comments = new List<jsonComment>();
                validComment = true;
            }

            public List<jsonComment> comments { get; set; }
            public bool validComment { get; set; }
            public string status { get; set; }
        }

        public class InputResource
        {
            public InputResource()
            {
                gradeLevel = new List<IDValuePair>();
                careerCluster = new List<IDValuePair>();
                endUser = new List<IDValuePair>();
                groupType = new List<IDValuePair>();
                resourceType = new List<IDValuePair>();
                mediaType = new List<IDValuePair>();
                educationalUse = new List<IDValuePair>();
                subject = new List<string>();
                keyword = new List<string>();
                standards = new List<InputStandard>();
            }

            public string title { get; set; }
            public string description { get; set; }
            public string requires { get; set; }
            public List<IDValuePair> gradeLevel { get; set; }
            public List<IDValuePair> careerCluster { get; set; }
            public List<IDValuePair> endUser { get; set; }
            public List<IDValuePair> groupType { get; set; }
            public List<IDValuePair> resourceType { get; set; }
            public List<IDValuePair> mediaType { get; set; }
            public List<IDValuePair> educationalUse { get; set; }
            public string usageRights { get; set; }
            public string timeRequired { get; set; }
            public IDValuePair accessRights { get; set; }
            public IDValuePair language { get; set; }
            public IDValuePair itemType { get; set; }
            public List<string> subject { get; set; }
            public List<string> keyword { get; set; }
            public List<InputStandard> standards { get; set; }
        }
        public class IDValuePair
        {
            public int id { get; set; }
            public string value { get; set; }
        }
        public class InputStandard
        {
            public int id { get; set; }
            public string code { get; set; }
            public IDValuePair alignment { get; set; }
        }
        #endregion

    }
}
