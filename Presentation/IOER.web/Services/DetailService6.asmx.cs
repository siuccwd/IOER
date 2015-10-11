 using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Serialization;

//TODO - remove direct calls to DAL and go thru biz services layer
using LRWarehouse.DAL;
using LRWarehouse.Business;

using ILPathways.Business;
using ILPathways.Utilities;
using Isle.BizServices;
using Isle.DTO;
using AccountManager = Isle.BizServices.AccountServices;
using ResBiz = Isle.BizServices.ResourceBizService;

namespace IOER.Services
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
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        UtilityService utilService = new UtilityService();

        #region Data getters
        [WebMethod( EnableSession = true )]
        public jsonDetail LoadAllResourceData( int vid, string userGUID )
        {
            var detail = new jsonDetail();

            try
            {
                //Get most of the data
                ResourceVersion version = new ResourceVersionManager().Get( vid );
                if (version.IsValid == false || version.IsActive == false)
                {
                    throw new Exception();
                }

                Patron user = GetUser( userGUID );

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
                detail.accessibilityControl = GetMVF( "accessibilityControl", detail.intID, false, "Accessibility Control" );
                detail.accessibilityFeature = GetMVF( "accessibilityFeature", detail.intID, false, "Accessibility Feature" );
                detail.accessibilityHazard = GetMVF( "accessibilityHazard", detail.intID, false, "Accessibility Hazard" );
                detail.accessRights = GetMVF( "accessRights", detail.intID, false, "Access Rights" );
                detail.language = GetMVF( "language", detail.intID, false, "Language" );
                detail.careerCluster = GetMVF( "careerCluster", detail.intID, true, "Career Cluster" );
                detail.endUser = GetMVF( "endUser", detail.intID, true, "End User" );
                detail.groupType = GetMVF( "groupType", detail.intID, true, "Group Type" );
                detail.resourceType = GetMVF( "resourceType", detail.intID, true, "Resource Type" );
                detail.mediaType = GetMVF( "mediaType", detail.intID, true, "Media Type" );
                detail.educationalUse = GetMVF( "educationalUse", detail.intID, true, "Educational Use" );
                detail.gradeLevel = GetMVF( "gradeLevel", detail.intID, true, "Grade Levels" );
                detail.k12subject = GetMVF( "subject", detail.intID, true, "K-12 Subject" );

                //Specially-formatted data
                detail.usageRights = GetUsageRights( version.Rights );
                detail.timeRequired = GetTimeRequired( version.TypicalLearningTime );

                //TODO - update the following
                //GetStandardsAndRubrics( ref detail, detail.intID, user );
                GetStandards( ref detail, user );
                GetEvaluations( ref detail, user );

                detail.subject = GetFreeText( detail.intID, "subject" );
                detail.keyword = GetFreeText( detail.intID, "keyword" );

                detail.libColInfo = GetLibColInfo( detail.intID, user );

                //Paradata
                detail.paradata = GetParadata( detail.intID, user );
                detail.comments = GetComments( detail.intID );
                bool canViewItem = true;

                ContentItem item = new ContentServices().GetForResourceDetail(version.ResourceIntId, user, ref canViewItem);
                if ( item != null && item.Id > 0 )
                {
                    FillRelatedContentDetail( detail, item );

                    if ( canViewItem == false )
                    {
                        detail.lrDocID = "";
                        detail.url = "";
                        detail.IsPrivateDocument = true;
                        //message should have been imbedded already?
                        //detail.resourceNote += item.Message;
					}
					else
					{
						//check for a condition where target url should be shown, and not link to the content page:
						//document only item, no parent
						//may also want to only do if public, as would expose doc url
						if ( item.TypeId == ContentItem.DOCUMENT_CONTENT_ID
							&& item.Message.IndexOf("standAloneContent") > -1
							&& item.PrivilegeTypeId == 1
							&& item.DocumentUrl != null && item.DocumentUrl.Length > 5)
						{
							detail.url = item.DocumentUrl;
						}
					}
                }
            }
            catch ( Exception ex )
            {
                //return null;
            }

            return detail;
        }

        private void FillRelatedContentDetail(jsonDetail detail, ContentItem item)
        {
            //now determine what useful content info to add
            //Q&D
            if ( item.Message != null && item.Message.Length > 0 )
                detail.resourceNote += "<div id='resourceMsg'>" + item.Message + "</div>";
        }

        [WebMethod]
        public jsonMVF GetMVF( string table, int intID, bool isMultiSelect, string printTitle )
        {
            var dataManager = new ResourceDataManager();
            jsonMVF list = new jsonMVF();
            try
            {
                DataSet ds = dataManager.SelectedCodes( ResourceDataManager.ResourceDataSubclassFinder.getSubclassByName( table ), intID );
                if ( ResBiz.DoesDataSetHaveRows( ds ) )
                {
                    list.name = table;
                    list.isMultiSelect = isMultiSelect;
                    list.printTitle = printTitle;
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        var item = new jsonMVFItem();
                        item.id = int.Parse( ResBiz.GetRowColumn( dr, "Id" ) );
                        item.selected = bool.Parse( ResBiz.GetRowColumn( dr, "IsSelected" ) );
                        item.title = ResBiz.GetRowColumn( dr, "Title" );
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
            string[] tables = new string[] { "accessibilityControl", "accessibilityFeature","accessibilityHazard","accessRights", "careerCluster", "educationalUse", "mediaType", "gradeLevel", "groupType", "endUser", "itemType", "language", "resourceType" };
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
                    item.id = int.Parse( ResBiz.GetRowColumn( dr, "Id" ) );
                    item.selected = false;
                    item.title = ResBiz.GetRowColumn( dr, "Title" );
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
        [WebMethod( EnableSession = true )]
        public string DoStandardRating( string userGUID, int standardID, int rating, int intID )
        {
          var user = GetUser(userGUID);
          if(user == null || user.Id == 0)
          {
            return utilService.ImmediateReturn(null, false, "invalid user", null);
          }
            string status = "";

            //create eval record
          ResBiz.ResourceStandardEvaluation_Create( intID, standardID, user.Id, rating, ref status );

          var detail = new jsonDetail() { intID = intID };
          GetStandards( ref detail, user );
          return utilService.ImmediateReturn( detail.standards, true, "okay", null );
        }

        [WebMethod( EnableSession = true )]
        public string AddToCollection( string userGUID, int libraryID, int collectionID, int intID )
        {
          string status = "";
          //Get/validate the user
          var user = GetUser( userGUID );
          if ( user.IsValid == false )
          {
              return null;
          }
          //Init library service
          var libService = new Isle.BizServices.LibraryBizService();
          //Ensure this collection exists
          var collection = libService.LibrarySectionGet( collectionID );
          if ( collection.IsValid == false )
          {
              return null;
          }
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
            //int test = libService.LibraryResourceCreate( libResource, user, ref status );
            int newId = libService.LibraryResourceCreate( libraryID, collection.Id, intID, user, ref status );
			if ( newId > -1 )
            {

              //Get the refreshed library info and return it
				//15-10-08 - if adding to a private library, will not show in the list
              var output = GetLibColInfo( intID, user );

              //14-07-07 mp - check for a status indicating approval required
              output.message = status;
              return serializer.Serialize( output );
            }
          }
          return null;
        }

        [WebMethod( EnableSession = true )]
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
                //comment.IsActive = true;

                //var commentID = 0;
                var commentID = commentManager.Create( comment, ref status );
                if ( commentID > 0 )
                {
                    //new ElasticSearchManager().AddComment( intID.ToString() );
                    //new ElasticSearchManager().RefreshResource( intID );
                    new Isle.BizServices.ResourceV2Services().RefreshResource( intID );
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
        public string FindMoreLikeThis( int intID, string text, string parameters, int minFieldMatches )
        {
            //return new ElasticSearchManager().FindMoreLikeThis( versionID, parameters.Split( ',' ), minFieldMatches, 12, 1 );
            //return new ElasticSearchManager().FindMoreLikeThis( intID, text, parameters.Split( ',' ), minFieldMatches, 12 );
					var query = new ElasticSearchService.JSONQueryV7()
					{
						text = text,
						size = 10
					};
						return (string) new ElasticSearchService().DoSearchCollection7( query ).data;
        }

				[WebMethod]
				public string FindMoreLikeThis_MultiMatch( string text )
				{
					//Temporary check until ES is updated on test environment
					var environment = UtilityManager.GetAppKeyValue( "envType", "prod" );

					//Match with OR
					object matchOr;
					if ( environment != "dev" )
					{
						matchOr = new
						{
							query = text,
							type = "cross_fields",
							fields = new List<string>() { "Title", "Description", "Keywords", "Fields.Tags" },
							@operator = "or"
						};
					}
					else
					{
						matchOr = new
						{
							query = text,
							fields = new List<string>() { "Title", "Description", "Keywords", "Fields.Tags" },
							@operator = "or"
						};
					}
					var queryOr = new
					{
						query = new
						{
							multi_match = matchOr
						},
						size = 21
					};
					var queryOrJSON = serializer.Serialize( queryOr );

					var orResults = new ElasticSearchManager().Search( queryOrJSON );

					return serializer.Serialize( UtilityService.DoReturn( orResults, true, "okay", queryOr ) );
				}

        [WebMethod( EnableSession = true )]
        public string AddLike( string userGUID, int versionID )
        {
            return serializer.Serialize( AddLikeDislike( userGUID, versionID, true ) );
        }

                
        [WebMethod( EnableSession = true )]
        public string AddDislike( string userGUID, int versionID )
        {
            return serializer.Serialize( AddLikeDislike( userGUID, versionID, false ) );
        }

        [WebMethod( EnableSession = true )]
        public string AddClickThrough( string userGUID, int versionID )
        {
            var version = new ResourceVersionManager().Get( versionID );

            new ResourceV2Services().AddResourceClickThrough( version.ResourceIntId, GetUser( userGUID ), version.Title );

            //new ResourceViewManager().Create( intID, GetUserID( userGUID ) );

            //ActivityBizServices.ResourceClickThroughHit( intID, userGUID, "" );
            //new ElasticSearchManager().AddResourceView( intID.ToString() );
            //new ElasticSearchManager().RefreshResource( intID );
            //new Isle.BizServices.ResourceV2Services().RefreshResource( intID );

            return serializer.Serialize( GetClickThroughs( version.ResourceIntId ) );
        }

        [WebMethod( EnableSession = true )]
        public string UpdateResource( int versionID, string userGUID, InputResource update )
        {
            //validate user and resource
            var userID = GetUserID( userGUID );
            if ( userID == 0 ) { return null; }
            var resourceIntID = utilService.GetIntIDFromVersionID( versionID );
            if ( resourceIntID == 0 ) { return null; }
            var resourceManager = new ResourceManager();
            var versionManager = new ResourceVersionManager();
            var resource = resourceManager.Get( resourceIntID );
            resource.Version = versionManager.Get( versionID );
            if ( resource.IsValid == false ) { return null; }

            var user = new AccountManager().Get( userID );

            //validate text inputs
            string status = "";
            update.subject = FixFreeText( update.subject );
            update.keyword = FixFreeText( update.keyword );
            bool didTitle_DescChange = false;

            //do admin-only updates
            //TODO - need to add handling for organization related resources
            //      - would be nice if don't have to duplicate code from detail page!
            //duplicating for now:
            bool canEdit = ResBiz.CanUserEditResource( resourceIntID, user.Id );
            bool canUpdate = false;
            var permissions = SecurityManager.GetGroupObjectPrivileges( user, "IOER.Pages.ResourceDetail" );

            if ( permissions.CanUpdate() == false )
            {
                LoggingHelper.DoTrace( 2, string.Format("DetailService6.UpdateResource. Unexpected state of cannot update for userId: {0}, email: {1}", user.Id, user.Email ));
            }

            if ( UtilityManager.GetAppKeyValue( "allowingOpenPublishing", "no" ) == "yes" )
                canUpdate = true;
            if ( canEdit || permissions.CanUpdate() )
                canUpdate = true;

            if ( canEdit 
                || permissions.CreatePrivilege > ( int )ILPathways.Business.EPrivilegeDepth.State )
            {

                if ( update.title != resource.Version.Title
                    || update.description != resource.Version.Description )
                    didTitle_DescChange = true;

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
            if ( canUpdate )
            {
                resource.Version.Rights = update.usageRights;
                resource.Version.TypicalLearningTime = update.timeRequired;
                resource.Version.AccessRights = update.accessRights.value;
                resource.Version.AccessRightsId = update.accessRights.id;
                versionManager.UpdateById( resource.Version );

                //check if need to sync title and desc changes for a content item
                if (didTitle_DescChange == true)
                    new ContentServices().HandleSyncResourceVersionChgs( resource.Version );

                ProcessMVF( update.gradeLevel, userID, resourceIntID, "gradeLevel" );
                ProcessMVF( update.careerCluster, userID, resourceIntID, "careerCluster" );
                ProcessMVF( update.endUser, userID, resourceIntID, "endUser" );
                ProcessMVF( update.groupType, userID, resourceIntID, "groupType" );
                ProcessMVF( update.resourceType, userID, resourceIntID, "resourceType" );
                ProcessMVF( update.mediaType, userID, resourceIntID, "mediaType" );
                ProcessMVF( update.educationalUse, userID, resourceIntID, "educationalUse" );
                ProcessMVF( update.k12subject, userID, resourceIntID, "subject" );

                ProcessMVF( update.accessibilityControl, userID, resourceIntID, "accessibilityControl" );
                ProcessMVF( update.accessibilityFeature, userID, resourceIntID, "accessibilityFeature" );
                ProcessMVF( update.accessibilityHazard, userID, resourceIntID, "accessibilityHazard" );

                new ResourceLanguageManager().Create( resourceIntID, update.language.id, update.language.value, userID, ref status );
                if ( update.itemType.id != 0 )
                {
                    new ResourceItemTypeManager().Create( resourceIntID, update.itemType.id, userID, ref status );
                }

                var standardsManager = new ResourceStandardManager();
                foreach ( InputStandard standard in update.standards )
                {
                    ResourceStandard item = new ResourceStandard();
                    item.CreatedById = userID;
                    item.ResourceIntId = resourceIntID;
                    item.StandardId = standard.id;
                    item.StandardNotationCode = standard.code;
                    item.AlignmentTypeCodeId = standard.alignment.id;
                    item.AlignmentTypeValue = standard.alignment.value;
                    standardsManager.Create( item, ref status );

                }

                AddFreeTextItems( new ResourceKeywordManager(), update.keyword, userID, resourceIntID );
                AddFreeTextItems( new ResourceSubjectManager(), update.subject, userID, resourceIntID );

                ActivityBizServices.SiteActivityAdd( "Resource", "Tag", string.Format( "Resource ID: {0} was tagged by {1}", resourceIntID, user.FullName() ), user.Id, 0, resourceIntID );

                //Update elasticsearch
                //new ElasticSearchManager().RefreshRecord( resourceIntID );
                //new ElasticSearchManager().RefreshResource( resourceIntID );
                new Isle.BizServices.ResourceV2Services().RefreshResource( resourceIntID );
            }

            //return updated resource
            return serializer.Serialize( LoadAllResourceData( versionID, userGUID ) );
        }

        [WebMethod]
        public string ReportIssue( string issue, string userGUID, int resourceID )
        {
            string status = "";
            issue = ValidateText( issue, 5, ref status );
            if ( issue != "" )
            {
                var user = GetUser( userGUID );
                if ( user.IsValid )
                {
                    //string url = "/IOER/" + resourceID + "/";
                    string url = "/Resource/" + resourceID + "/";
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

        [WebMethod( EnableSession = true )]
        public string DeactivateResource( string userGUID, int versionID )
        {
            var user = GetUser( userGUID );
            var permissions = SecurityManager.GetGroupObjectPrivileges( user, "IOER.Pages.ResourceDetail" );
            if ( permissions.CreatePrivilege > ( int ) ILPathways.Business.EPrivilegeDepth.State )
            {
                try 
                {
                    string response = "";
                    ResBiz.Resource_SetInactiveByVersionId( versionID, ref response );

                    //new ResourceVersionManager().SetActiveState( false, versionID );
                    
                    //var esManager = new ElasticSearchManager();
                    //new ElasticSearchManager().DeleteByVersionID( versionID, ref response );
                    new ResourceV2Services().DeleteResourceByVersionID( versionID );

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

        [WebMethod]
        public string RegenerateThumbnail( string userGUID, int resourceID, string url )
        {
          return utilService.RegenerateThumbnail( userGUID, resourceID, url );
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
					var useRights = new ResourceV2Services().GetUsageRights( rights );

					output.usageRightsText = useRights.Title;
					output.usageRightsValue = useRights.CodeId;
					output.usageRightsDescription = useRights.Description;
					output.usageRightsURL = rights;
					output.usageRightsIconURL = useRights.IconUrl;
					output.usageRightsMiniIconURL = useRights.MiniIconUrl;

					return output;

					/*
            var output = new jsonUsageRights();
            //DataSet ds = CodeTableManager.ConditionsOfUse_Select(); //Not enough data
            DataSet ds = ResBiz.DoQuery( "IF( SELECT COUNT(*) FROM [ConditionOfUse] WHERE [Url] = '" + rights + "' ) = 0 SELECT * FROM [ConditionOfUse] WHERE [Url] IS NULL ELSE SELECT * FROM [ConditionOfUse] WHERE [Url] = '" + rights + "'" );
            if ( ResBiz.DoesDataSetHaveRows( ds ) )
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
					*/
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
            if ( ResBiz.DoesDataSetHaveRows( ds ) )
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

        public jsonDetailLibraryInfo GetLibColInfo( int resourceID, Patron user )
        {
          var output = new jsonDetailLibraryInfo();
          LibraryContributeDTO dto = new LibraryContributeDTO();

          var libService = new Isle.BizServices.LibraryBizService();

          //Get all of the libraries I have access to
          if ( user.Id > 0 )
          {
            libService.Library_SelectListWithContributeAccess( dto, user.Id );

            if ( dto != null && dto.libraries != null && dto.libraries.Count > 0 )
            {

                //List<LibrarySummaryDTO> myLibraries = dto.libraries;

                foreach ( LibrarySummaryDTO item in dto.libraries )
                {
                    //Fill out library data
                    var library = new jsonDetailLibrary();
                    library.isPersonalLibrary = item.LibraryTypeId == 1;
                    library.isInLibrary = libService.IsResourceInLibrary( item.Id, resourceID );
                    library.id = item.Id;
                    if (item.UserNeedsApproval)
                        library.title = item.Title + " [Requires Approval]";
                    else
                        library.title = item.Title;
                    library.avatarURL = item.ImageUrl;

                    //Get collections for this library
                    //need to take into account if library is open contribute
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
          }

          //Get all of the public Libraries this Resource appears in. Also need to check for lib member
          var inLibraries = libService.GetAllLibrariesWithResource( resourceID );
          foreach ( ILPathways.Business.Library item in inLibraries )
          {
			  if ( item.PublicAccessLevelInt >= 2 || item.CreatedById == user.Id )
			  {
				  jsonDetailLibCol library = new jsonDetailLibCol();
				  library.title = item.Title;
				  library.id = item.Id;
				  library.avatarURL = ( item.ImageUrl == "defaultUrl" ? "/images/ioer_med.png" : item.ImageUrl );

				  output.libraries.Add( library );
			  }
          }

          return output;
        }

        public jsonParadata GetParadata( int intID, Patron user )
        {
          var output = new jsonParadata();

          string testStatus = "";
          var summary = new ResourceLikeSummaryManager().GetForDisplay( intID, user.Id, ref testStatus );
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
            int resourceId = utilService.GetIntIDFromVersionID( versionID );
            Patron user = GetUser( userGUID );

            if ( user.Id == 0 ) { return null; }
            var manager = new ResourceLikeManager();
            var like = new ResourceLike();

            if ( ResourceBizService.HasLikeDislike(user.Id, resourceId ))
            {
                return GetParadata( resourceId, user );
            }

            //like.IsLike = isLike;
            //like.CreatedById = user.Id;
            //like.ResourceIntId = resourceId;
            //string status = "";
            new ResourceBizService().AddLikeDislike( user.Id, resourceId, isLike );
            //manager.Create( like, ref status );

            return GetParadata( resourceId, user );
        }

        public int GetClickThroughs( int intID )
        {
            int count = ResBiz.ResourceClickThroughs(intID);
            //Need a proc for this
            //DataSet ds = ResBiz.DoQuery( "SELECT COUNT(*) AS 'Count' FROM [Resource.View] WHERE ResourceIntId = " + intID );
            //if ( ResBiz.DoesDataSetHaveRows( ds ) )
            //{
            //    count = GetInt( ds.Tables[ 0 ].Rows[ 0 ], "Count" );
            //}
            return count;
        }

        /*public void GetStandardsAndRubrics( ref jsonDetail input, int intID, Patron user )
        {
            try
            {
                ResourceRatings ratings = new ResourceEvaluationManager().GetRatingsForResource( intID, user.Id );
                input.standards = ratings.standardRatings;
                input.rubrics = ratings.rubricRatings;
            }
            catch ( Exception ex ) //need to fix this
            {
                ResourceRatings ratings = new ResourceRatings();
                input.standards = ratings.standardRatings;
                input.rubrics = ratings.rubricRatings;
            }

        }*/
        public void GetStandards ( ref jsonDetail detail, Patron user ) 
        {
          var statusMessage = "";
          //detail.standards = new ResourceEvaluationManager().GetRatingsForResource( detail.intID, user.Id ).standardRatings;
          detail.standards = ResourceBizService.ResourceStandardEvaluation_GetAll( detail.intID, user, ref statusMessage );
          var alignments = new string[] { "Aligns to", "Assesses", "Teaches", "Requires" }; //kludge
          foreach ( var item in detail.standards )
          {
            try
            {
              item.AlignmentType = alignments[ item.AlignmentTypeId ];
            }
            catch
            {
              item.AlignmentType = "Aligns to";
            }
          }
          return;
          //

          /*var status = "";
          var baseStandardData = new ResourceEvaluationManager().GetRatingsForResource( detail.intID, user.Id ).standardRatings; //Need alignment type, couldn't get it to come from database in the next call. so this will need to be temporary too.
          var standardsEvalData = ResourceBizService.GetAllStandardEvaluationsForResource( detail.intID, user, ref status );

          foreach ( var item in standardsEvalData )
          {
            var target = baseStandardData.Where( m => m.id == item.StandardId ).FirstOrDefault();
            if ( target != null )
            {
              item.AlignmentType = target.alignmentType;
              item.AlignmentTypeId = target.alignmentTypeID;
            }
          }
          detail.standards = standardsEvalData;*/
        }
        
        public void GetEvaluations( ref jsonDetail detail, Patron user )
        {
          string status = "";
          try
          {
            var data = ResourceBizService.GetAllEvaluationsForResource( detail.intID, user, ref status );
            bool already = false;
            bool can = false;
            ResourceBizService.Evaluations_UserEvaluationStatus( 1, detail.intID, user, ref already, ref can );
            detail.userAlreadyEvaluated = already;
            detail.userCanEvaluate = can;
            
            detail.evaluations = data;
          }
          catch ( Exception ex )
          {
            LoggingHelper.LogError( ex.Message.ToString() + "; Rubric Status: " + status );
          }
        }

        protected string Get( DataRow dr, string column )
        {
            return ResBiz.GetRowPossibleColumn( dr, column );
        }
        protected int GetInt( DataRow dr, string column )
        {
            try
            {
                return int.Parse( ResBiz.GetRowPossibleColumn( dr, column ) );
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
            Patron user = new Patron();

            if ( userGUID.Trim() == "" )
            {
                user.IsValid = false;
            }
            else
            {
                user = new AccountServices().GetByRowId( userGUID );
            }
            return user;
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
                manager.Create( ResourceDataManager.ResourceDataSubclassFinder.getSubclassByName( table ), intID, pair.id, pair.value, userID );
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
                k12subject = new jsonMVF();
                //standards = new List<ResourceRating>();
                standards = new List<ResourceStandardEvaluationSummary>();
                //standards = new List<ResourceStandardEvaluationSummary>();
                rubrics = new List<ResourceRating>();
                comments = new List<jsonComment>();
                usageRights = new jsonUsageRights();
                subject = new jsonFreeText();
                keyword = new jsonFreeText();
                libColInfo = new jsonDetailLibraryInfo();
                paradata = new jsonParadata();
                comments = new List<jsonComment>();
                evaluations = new List<ResourceEvaluationSummaryDTO>();
                resourceNote = "";
                IsPrivateDocument = false;
            }

            //Single value items
            public int versionID { get; set; }
            public int intID { get; set; }
            public string title { get; set; }
            public string url { get; set; }
            public string description { get; set; }
            public string resourceNote { get; set; }
            public string requires { get; set; }
            public string created { get; set; }
            public string creator { get; set; }
            public string publisher { get; set; }
            public string submitter { get; set; }
            public string isBasedOnUrl { get; set; }
            public string timeRequired { get; set; }
            public string lrDocID { get; set; }
                            
            public bool IsPrivateDocument { get; set; }
            //Multi value fields with controlled vocabulary
            public jsonMVF itemType { get; set; }
            public jsonMVF accessRights { get; set; }
            public jsonMVF accessibilityControl { get; set; }
            public jsonMVF accessibilityFeature { get; set; }
            public jsonMVF accessibilityHazard { get; set; }

            public jsonMVF language { get; set; }
            public jsonMVF careerCluster { get; set; }
            public jsonMVF endUser { get; set; }
            public jsonMVF groupType { get; set; }
            public jsonMVF resourceType { get; set; }
            public jsonMVF mediaType { get; set; }
            public jsonMVF educationalUse { get; set; }
            public jsonMVF gradeLevel { get; set; }
            public jsonMVF k12subject { get; set; }

            //Specially-formatted data
            public jsonUsageRights usageRights { get; set; }
            public List<ResourceStandardEvaluationSummary> standards { get; set; }
            //public List<ResourceRating> standards { get; set; }
            //public List<ResourceStandardEvaluationSummary> standards { get; set; }
            public List<ResourceRating> rubrics { get; set; }
            public List<ResourceEvaluationSummaryDTO> evaluations { get; set; }
            public jsonFreeText subject { get; set; }
            public jsonFreeText keyword { get; set; }
            public jsonDetailLibraryInfo libColInfo { get; set; }
            public bool userCanEvaluate { get; set; }
            public bool userAlreadyEvaluated { get; set; }

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
            public string message { get; set; }
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
                k12subject = new List<IDValuePair>();
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
            public List<IDValuePair> k12subject { get; set; }

            public List<IDValuePair> accessibilityControl { get; set; }
            public List<IDValuePair> accessibilityFeature { get; set; }
            public List<IDValuePair> accessibilityHazard { get; set; }

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
