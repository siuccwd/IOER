using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

using ILPathways.Business;
using ILPathways.Common;
using ILPathways.Utilities;
using Isle.DataContracts;
using LearningRegistry;
using LearningRegistry.RDDD;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using AcctManager = Isle.BizServices.AccountServices;
using DBM = ILPathways.DAL.DatabaseManager;
using DocManager = ILPathways.DAL.DocumentStoreManager;
using EFDAL = IoerContentBusinessEntities;
using EFManager = IoerContentBusinessEntities.EFContentManager;
using GroupManager = Isle.BizServices.GroupServices;
using MyManager = Isle.BizServices.ContentServices;
//using MyManager = ILPathways.DAL.ContentManager;
using OrgManager = Isle.BizServices.OrganizationBizService;
using ResourceManager = LRWarehouse.DAL.ResourceManager;
using ThisUser = LRWarehouse.Business.Patron;
using Thumbnailer = LRWarehouse.DAL.ResourceThumbnailManager;

namespace Isle.BizServices
{
    public class PublishingServices : ServiceHelper
    {
        static string thisClassName = "PublishingServices";
        MyManager myMgr = new MyManager();
        //static ServiceHelper helper = new ServiceHelper();

        public PublishingServices()
		{ }//

        #region  --- content approvals ---
        /// <summary>
        /// Handle request to approve a content record
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="contentId"></param>
        /// <param name="user"></param>
        /// /// <param name="hasApproval">return true if approval was required</param>
        /// <param name="statusMessage"></param>
        /// <returns>true if ok, false if errors</returns>
        public static bool HandleContentApproval( Resource resource, int contentId, ThisUser author, ref bool hasApproval, ref string statusMessage )
        {
            // - get record, check if on behalf is org
            //     if not, set to published?
            // - 
            bool isValid = true;
            hasApproval = false;
            MyManager mgr = new MyManager();
            ContentItem entity = mgr.Get( contentId );
            if ( entity == null || entity.Id == 0 )
            {
                //invalid, return, log error?
                statusMessage = "Invalid request, record was not found.";
                return false;
            }

            //entity.ResourceVersionId = resource.Version.Id;
            entity.ResourceIntId = resource.Id;

            if ( entity.IsOrgContent() == false )
            {
                entity.StatusId = ContentItem.PUBLISHED_STATUS;
                mgr.Update( entity );

                //TODO - anything else??
                return true;
            }
            hasApproval = true;
            //get approvers for org
            SubmitApprovalRequest( entity );

            //update status
            entity.StatusId = ContentItem.SUBMITTED_STATUS;
            mgr.Update( entity );
            //set resource to inactive
            string status = new ResourceManager().SetResourceActiveState( resource.Id, false );

            //add record to audit table
            string msg = string.Format( "Request by {0} for approval of resource id: {1}", author.FullName(), entity.Id );

            mgr.ContentHistory_Create( contentId, "Approval Request - new", msg, author.Id, ref statusMessage );

            return isValid;
        }//

        /// <summary>
        /// Request approval for a content item
        /// - resource tagging, and LR publishing (or caching) would have already occured
        /// - typically used where a previous submission was denied or author has made updates to an previously approved content item
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="author"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static bool RequestApproval( int contentId, ThisUser author, ref string statusMessage )
        {
            //TODO  - should resource be set inactive

            bool isValid = true;

            MyManager mgr = new MyManager();
            try
            {
                ContentItem entity = mgr.Get( contentId );
                if ( entity == null || entity.Id == 0 )
                {
                    //invalid, return, log error?
                    statusMessage = "Invalid request, record was not found.";
                    return false;
                }
                //get approvers for org
                SubmitApprovalRequest( entity );

                //update status
                entity.StatusId = ContentItem.SUBMITTED_STATUS;
                mgr.Update( entity );
                //set resource to inactive
                string status = new ResourceManager().SetResourceActiveState( entity.ResourceIntId, false );

                //add record to audit table
                string msg = string.Format( "Request by {0} for approval of resource id: {1}", author.FullName(), entity.Id );

                mgr.ContentHistory_Create( entity.Id, "Approval Request - existing", msg, author.Id, ref statusMessage );
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".RequestApproval(contentId: {0}, author: {1})", contentId, author.FullName() ) );
                isValid = false;
                statusMessage = ex.Message;
            }
            return isValid;
        }


        public static void SubmitApprovalRequest( ContentItem entity )
        {
            //get administrators for org, and parent
            //format and send emails
            string statusMessage = "";
            AcctManager mgr = new AcctManager();
            ThisUser author = mgr.Get( entity.CreatedById );
            string note = "";
            string toEmail = "";
            string bccEmail = "";
            //if valid
            Organization org = OrgManager.GetOrganization( author, ref statusMessage );
            if ( org != null && org.Id > 0 )
            {
                //get list of administrators
                List<GroupMember> list = GroupManager.OrgApproversSelect( org.Id );
                if ( list != null && list.Count > 0 )
                {
                    foreach ( GroupMember item in list )
                    {
                        if ( item.UserEmail.Length > 5 )
                            toEmail += item.UserEmail + ",";
                    }
                }
                else
                {
                    //if no approvers, send to info
                    toEmail = UtilityManager.GetAppKeyValue( "contactUsMailTo", "DoNotReply@ilsharedlearning.org" );
                    note = "<br/>NOTE: no organization approvers were found for this organization: " + org.Name;
                }
                string url = ContentServices.FormatContentFriendlyUrl( entity );

                string urlTitle = string.Format( "<a href='{0}'>{1}</a>", url, entity.Title );
                if ( System.DateTime.Now < new System.DateTime( 2013, 7, 1 ) )
                    bccEmail = UtilityManager.GetAppKeyValue( "appAdminEmail", "DoNotReply@ilsharedlearning.org" );

                string subject = string.Format( "Isle request to approve an education resource from: {0}", author.FullName() );
                string body = string.Format( "<p>{0} from {1} is requesting approval on an education resource.</p>", author.FullName(), org.Name );

                //could include the override parm in the link, and just go to the display?
                //or, approver will need to see all content, even private?


                body += "<br/>url:&nbsp;" + urlTitle;
                body += "<br/><br/>From: " + author.EmailSignature();
                string from = author.Email;
                EmailManager.SendEmail( toEmail, from, subject, body, author.Email, bccEmail );
            }
        }

        /// <summary>
        /// Handle action of content approved
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="approver"></param>
        /// <param name="statusMessage"></param>
        public static bool HandleApprovedAction( int contentId, ThisUser approver, ref string statusMessage )
        {
            string errorMessage = "";
            ContentItem entity = new MyManager().Get( contentId );
            if ( entity != null && entity.Id > 0 )
                return HandleApprovedAction( entity, approver, ref statusMessage, ref errorMessage );
            else
            {
                statusMessage = "Error - unable to retrieve the requested resource";
                return false;
            }
        }

        /// <summary>
        /// Handle action of content approved
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="approver"></param>
        /// <param name="statusMessage"></param>
        public static bool HandleApprovedAction( ContentItem entity, ThisUser approver
                        , ref string statusMessage
                        , ref string errorMessage )
        {
            bool isValid = true;
            string bccEmail = "";

            statusMessage = string.Empty;
            MyManager mgr = new MyManager();
            entity.StatusId = ContentItem.PUBLISHED_STATUS;
            entity.ApprovedById = approver.Id;
            entity.Approved = System.DateTime.Now;
            mgr.Update( entity );

            //set resource to active
            string status = new ResourceManager().SetResourceActiveState( entity.ResourceIntId, true );

            //======= published cached LR data
            if ( entity.PrivilegeTypeId == ContentItem.PUBLIC_PRIVILEGE )
            {
                //new BaseUserControl().SetConsoleInfoMessage( "WARNING - HAVE NOT IMPLEMENTED CODE TO READ CACHED LR DATA AND DO ACTUAL LR PUBLISH" );

                if ( PublishSavedEnvelope( entity.ResourceIntId, ref statusMessage ) == false )
                {
                    errorMessage = "WARNING - the publish to the learning registry failed. System administration has been notified.";
                    EmailManager.NotifyAdmin( "Publish of pending resource failed", string.Format( "The attempt to publish a saved resource was unsuccessful. Content id: {0}, Res IntId: {1}, Message: {2}", entity.Id, entity.ResourceIntId, statusMessage ) );
                }
            }

            //add record to audit table
            string msg = string.Format( "Resource: {0}, author: {1}, was approved by {2}", entity.Title, entity.Author, approver.FullName() );

            mgr.ContentHistory_Create( entity.Id, "Content Approved", msg, approver.Id, ref statusMessage );

            //send email
            AcctManager amgr = new AcctManager();
            ThisUser author = amgr.Get( entity.CreatedById );

            string url = ContentServices.FormatContentFriendlyUrl( entity );

            string urlTitle = string.Format( "<a href='{0}'>{1}</a>", url, entity.Title );
            string toEmail = author.Email;
            if ( System.DateTime.Now < new System.DateTime( 2013, 7, 1 ) )
                bccEmail = UtilityManager.GetAppKeyValue( "appAdminEmail", "info@illinoisworknet.com" );

            string subject = "Isle: APPROVED education resource";

            string body = string.Format( "<p>{0} has approved your education resource. It is now available on the website (based on the defined privilege settings).</p>", approver.FullName() );
            body += "<br/>url:&nbsp;" + urlTitle;
            body += "<br/>From: " + approver.EmailSignature();
            string from = approver.Email;
            EmailManager.SendEmail( toEmail, from, subject, body, approver.Email, bccEmail );

            return isValid;
        }

        public static bool PublishSavedEnvelope( int resourceIntId, ref string statusMessage )
        {
            bool isValid = true;
            string LRDocID = "";
            ResourceManager manager = new ResourceManager();

            try
            {
                PublishPending entity = manager.PublishPending_GetByResId( resourceIntId );
                if ( entity != null && entity.Id > 0 )
                {
                    //Publish to LR
                    lr_Envelope envelope = new JavaScriptSerializer().Deserialize<lr_Envelope>( entity.LREnvelope );
                    PublishLREnvelope( envelope, ref statusMessage, ref LRDocID );

                    //Publish to ElasticSearch
                    ResourceJSONFlat flat = new ResourceJSONManager().GetJSONFlatByIntID( resourceIntId )[ 0 ];
                    //new ElasticSearchManager().CreateOrReplaceRecord( flat );
                    new ElasticSearchManager().RefreshResource( flat.intID ); //Kludge - need to get ID more efficiently

                    //Set status messages
                    SetConsoleSuccessMessage( "Successful Publish of saved resource" );
                    LoggingHelper.DoTrace( 5, "Successful Publish of saved resource - LR Document ID:<br />" + LRDocID );

                    //Update entity
                    entity.IsPublished = true;
                    entity.PublishedDate = DateTime.Now;
                    manager.PublishPending_Update( entity );
                    ResourceVersionManager rvManager = new ResourceVersionManager();
                    ResourceVersion version = rvManager.Get( entity.ResourceVersionId );
                    if ( version != null && version.Id > 0 )
                    {
                        version.LRDocId = LRDocID;
                        rvManager.Update_LrDocId( version );
                    }
                    else
                    {
                        isValid = false;
                        statusMessage = string.Format( "Error - unable to retrieve the resource version record (in order to update the LR docId: {0}).", LRDocID );
                        SetConsoleErrorMessage( statusMessage );
                    }
                }
                else
                {
                    statusMessage = "Error - unable to retrieve the requested publish-pending resource";
                    SetConsoleErrorMessage( statusMessage );
                    isValid = false;
                }
            }
            catch ( Exception ex )
            {
                SetConsoleErrorMessage( "Error Publishing to Learning Registry: " + ex.Message );
                statusMessage = statusMessage + "Publish of saved resource Failed: " + ex.ToString();
                isValid = false;
            }
            return isValid;
        }

        public static void PublishLREnvelope( lr_Envelope envelope, ref string statusMessage, ref string lrDocID )
        {
            string node = UtilityManager.GetAppKeyValue( "learningRegistryNodePublish" );
            string clientID = UtilityManager.GetAppKeyValue( "learningRegistryUserId" );
            string clientPassword = UtilityManager.GetAppKeyValue( "learningRegistryPassword" ); 

            //string clientID = "info@siuccwd.com";
            //string clientPassword = "in5t@ll3r";

            LRClient client = new LRClient( node, clientID, clientPassword );
            try
            {
                PublishResponse response = client.Publish( envelope );

                SetConsoleSuccessMessage( "Successful Publish!<br />Learning Registry Document ID:<br />" + response.document_results.ElementAt( 0 ).doc_ID );
                statusMessage = statusMessage + "Successful Publish!";
                lrDocID = response.document_results.ElementAt( 0 ).doc_ID;
            }
            catch ( Exception ex )
            {
                SetConsoleErrorMessage( "Publish Failed: " + ex.Message.ToString() );
            }
        }

        #endregion

        public static void PublishToAll( Resource input,
            ref bool isSuccessful,
            ref string status,
            ref int versionID,
            ref int intID,
            ref string sortTitle,
            bool updatingElasticSearch,
            bool skipLRPublish,
            Patron user )
        {
            bool success = true;
            string tempStatus = "";
            string lrDocID = "";
            string continueOnPublishError = UtilityManager.GetAppKeyValue( "continueOnPublishError", "yes" );
            //Publish to LR. This will give us an LR Doc ID
            if ( !skipLRPublish )
            {
                PublishToLearningRegistry( input, ref success, ref tempStatus, ref lrDocID );
                if ( !success && !IsLocalHost() )
                {
                    if ( continueOnPublishError == "no" )
                    {
                        isSuccessful = false;
                        SetConsoleErrorMessage( "Error: " + tempStatus );
                        status = status + " " + tempStatus + " ";
                        versionID = 0;
                        return;
                    }
                    else {
                        EmailManager.NotifyAdmin( "Error during LR Publish", "Error: " + tempStatus + "<p>The error was encountered during the LR publish. The system continued with saving to the database and elastic search. </p>" );
                    }
                }
            }
            input.Version.LRDocId = lrDocID;

            //If successful, publish to Database. This will give us a Resource Version ID
            PublishToDatabase( input, ref success, ref tempStatus, ref versionID, ref intID, ref sortTitle );
            if ( !success )
            {
                isSuccessful = false;
                SetConsoleErrorMessage( "Error: " + tempStatus );
                status = status + " " + tempStatus + " ";
                versionID = 0;
                return;
            }
            input.Version.Id = versionID;
            input.Version.ResourceIntId = intID;
            input.Id = intID;

            //If successful, publish to ElasticSearch
            if ( updatingElasticSearch )
            {
                PublishToElasticSearch( input.Id, ref success, ref tempStatus );
                if ( !success )
                {
                    isSuccessful = false;
                    SetConsoleErrorMessage( "Error: " + tempStatus );
                    status = status + " " + tempStatus + " ";
                    versionID = 0;
                    return;
                }
            }
            isSuccessful = true;
            status = "okay";
            SetConsoleSuccessMessage( "Successfully published the Resource" );

            //new ResourceThumbnailManager().CreateThumbnailAsynchronously( input.Id, input.ResourceUrl, false );
            new Thumbnailer().CreateThumbnail( input.Id, input.ResourceUrl );

            SendPublishNotification( user, input );
        }


        public static void PublishToLearningRegistry( Resource input, 
                    ref bool successful, 
                    ref string status, 
                    ref string lrDocID )
        {
            //Create payload
            var payload = new ResourceJSONManager().GetJSONLRMIFromResource( input );

            //Create document
            lr_document doc = new lr_document();
            doc.resource_data_type = "metadata";
            doc.payload_placement = "inline";
            doc.payload_schema = new List<string> { "LRMI" };
            doc.resource_data = payload;
            doc.resource_locator = input.ResourceUrl;

            //Identity info
            lr_identity identity = new lr_identity();
            identity.submitter_type = "agent";
            identity.submitter = "ISLE OER on Behalf of " + input.Version.Submitter;
            identity.signer = "ISLE OER";
            doc.identity = identity;

            //keywords
            foreach ( ResourceChildItem word in input.Keyword )
            {
                doc.keys.Add( word.OriginalValue.Trim() );
            }

            //Sign the document
            string PgpKeyringLocation = Path.Combine( HttpRuntime.AppDomainAppPath, "App_Data/lrpriv.asc" );
            string keyData = File.ReadAllText( PgpKeyringLocation );
            string[] PublicKeyLocations = new string[] { "http://pgp.mit.edu:11371/pks/lookup?op=get&search=0x6ce0837335049763" };
            string UserID = "ISLEOER (Data Signing Key) <info@siuccwd.com>";
            string password = "89k7SMteVzPUY";
            PgpSigner signer = new PgpSigner( PublicKeyLocations, keyData, UserID, password );
            doc = signer.Sign( doc );

            //Build the envelope
            lr_Envelope envelope = new lr_Envelope();
            envelope.documents.Add( doc );

            //Do publish
            string node = UtilityManager.GetAppKeyValue( "learningRegistryNodePublish" ); //"https://node01.public.learningregistry.net", "http://sandbox.learningregistry.org/"
            string clientID = "info@siuccwd.com";
            string clientPassword = "in5t@ll3r";
            LRClient client = new LRClient( node, clientID, clientPassword );
            try
            {
                //Do publish
                PublishResponse response = client.Publish( envelope );

                //Set return values
                successful = true;
                lrDocID = response.document_results.ElementAt( 0 ).doc_ID;
                status = "Successfully published. LR Doc ID: " + lrDocID;
            }
            catch ( Exception ex )
            {
                successful = false;
                lrDocID = "";
                status = "Failed to Publish: " + ex.Message;
            }
        }


        public static void PublishToDatabase( Resource input
                        , ref bool successful
                        , ref string status
                        , ref int versionID
                        , ref int intID
                        , ref string sortTitle )
        {
            try
            {
                ResourceDataManager dataManager = new ResourceDataManager();

                //Resource
                intID = new ResourceManager().Create( input, ref status );
                input.Id = intID;
                input.Version.ResourceIntId = intID;

                //Version
                var versionManager = new ResourceVersionManager();
                versionID = versionManager.Create( input.Version, ref status );
                input.Version.Id = versionID;

                //Tags
                var tags = new Dictionary<List<ResourceChildItem>, string>
                {
                  { input.ClusterMap, "careerCluster" },
                  { input.EducationalUse, "educationalUse" },
                  { input.ResourceFormat, "mediaType" },
                  { input.Gradelevel, "gradeLevel" },
                  { input.GroupType, "groupType" },
                  { input.Audience, "endUser" },
                  { input.ItemType, "itemType" },
                  { input.Language, "language" },
                  { input.ResourceType, "resourceType" },
                  { input.SubjectMap, "subject" },
                  { input.relatedURL, "originalVersionURL" }
                };
                foreach ( KeyValuePair<List<ResourceChildItem>, string> entry in tags )
                {
                    CreateMVFs( dataManager, entry.Key, findClass( entry.Value ), intID, input.CreatedById );
                }

                //Keywords
                var keywordManager = new ResourceKeywordManager();
                foreach ( ResourceChildItem word in input.Keyword )
                {
                    word.ResourceIntId = intID;
                    keywordManager.Create( word, ref status );
                }

                //Standards
                var standardManager = new ResourceStandardManager();
                foreach ( ResourceStandard standard in input.Standard )
                {
                    standard.ResourceIntId = intID;
                    standardManager.Create( standard, ref status );
                }

                sortTitle = versionManager.Get( versionID ).SortTitle.Replace( " ", "_" );
                successful = true;
                status = "okay";
            }
            catch ( Exception ex )
            {
                successful = false;
                status = ex.Message;
                return;
            }
        }

    

    #region Helper Methods
        public static void SendPublishNotification( Patron user, Resource resource )
        {
            string toEmail = UtilityManager.GetAppKeyValue( "contactUsMailTo", "info@ilsharedlearning.org" );
            string cc = UtilityManager.GetAppKeyValue( "onPublishCC", "" );
            string bcc = UtilityManager.GetAppKeyValue( "appAdminEmail", "mparsons@siuccwd.com" );
            string fromEmail = user.Email;

            string subject = string.Format( "IOER - New publish notification from: {0}", user.FullName() );
            string body = string.Format( "<p>{0} published a new resource to IOER. </p>", user.FullName() );
            if ( resource.Version != null )
            {
                body += "<br/>Resource: " + resource.Version.Title;
                body += "<br/>" + resource.Version.Description;
            }
            //body += "<br/>Target Url: <span>" + resource.ResourceUrl + "</span>   ";
            body += "<br/><br/>Target Url: " + string.Format( "<a href='{0}'>{1}</a>", resource.ResourceUrl, resource.ResourceUrl );

            //string url = UtilityManager.FormatAbsoluteUrl( string.Format( "/ResourceDetail.aspx?vid={0}", resource.Version.Id ), true );
            //string title = FormatFriendlyTitle( resource.Version.Title );
            string url2 = ResourceBizService.FormatFriendlyResourceUrl( resource.Version );


            body += "<br/><br/>Detail Url: " + string.Format( "<a href='{0}'>View published resource</a>", url2 );

            body += "<br/>From: " + user.EmailSignature();
            //string from = applicant.Email;
            EmailManager.SendEmail( toEmail, fromEmail, subject, body, cc, bcc );
        }

        private static void CreateMVFs( ResourceDataManager dataManager, List<ResourceChildItem> input, ResourceDataManager.IResourceDataSubclass className, int intID, int createdByID )
        {
            foreach ( ResourceChildItem item in input )
            {
                dataManager.Create( className, intID, item.CodeId, item.OriginalValue, createdByID );
            }
        }

        private static ResourceDataManager.IResourceDataSubclass findClass( string tableName )
        {
            return ResourceDataManager.ResourceDataSubclassFinder.getSubclassByName( tableName );
        }
    #endregion

        #region LR Document methods

        public static lr_document BuildDocument( ref Resource resourceEntity )
        {
            string resourceData = BuildPayloadLRMI_JSON( resourceEntity );

            lr_document doc = new lr_document();

            //Required Fields
            doc.resource_data_type = "metadata";
            doc.payload_placement = "inline";
            doc.payload_schema = new List<string>( new string[] { "LRMI" } );

            //Most of the data is in here:
            doc.resource_data = resourceData;

            //Resource Locator
            doc.resource_locator = resourceEntity.ResourceUrl;

            //Submitter Informaton
            lr_identity identity = new lr_identity();
            identity.submitter_type = "agent";
            identity.submitter = "ISLE OER on Behalf of " + resourceEntity.Version.Submitter;
            identity.signer = "ISLE OER";
            doc.identity = identity;

            //Keywords
            foreach ( ResourceChildItem keyword in resourceEntity.Keyword )
            {
                doc.keys.Add( keyword.OriginalValue.Trim() );
            }

            return doc;
        }

        public static string BuildPayloadLRMI_JSON( Resource entity )
        {
            ResourceJSONManager jsonManager = new ResourceJSONManager();
            ResourceJSONLRMI resource = jsonManager.GetJSONLRMIFromResource( entity );

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize( resource );
        }

        public static lr_Envelope BuildEnvelope( lr_document doc )
        {
            lr_Envelope envelope = new lr_Envelope();

            envelope.documents.Add( doc );
            return envelope;
        }

        #endregion

        public static void PublishToElasticSearch( int resourceId, ref bool successful, ref string status )
        {
            //Do create
            try
            {
                //new ElasticSearchManager().CreateOrReplaceRecord( resourceId );
                new ElasticSearchManager().RefreshResource( resourceId );
                successful = true;
                status = "okay";
            }
            catch ( Exception ex )
            {
                successful = false;
                status = ex.Message;
            }
        }
    }
}
