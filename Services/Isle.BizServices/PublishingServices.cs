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
//using Isle.DataContracts;
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
//using Thumbnailer = LRWarehouse.DAL.ResourceThumbnailManager;

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
					//new ElasticSearchManager().RefreshResource( flat.intID ); //Kludge - need to get ID more efficiently
					new Isle.BizServices.ResourceV2Services().RefreshResource( flat.intID );

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

		/// <summary>
		/// publishing
		/// 15-04-15 mparsons - explictly passing , rather than getting from Resource object to identify where called from, and ensure set as needed. Then MAYBE will just use in object?
		/// </summary>
		/// <param name="input"></param>
		/// <param name="isSuccessful"></param>
		/// <param name="status"></param>
		/// <param name="versionID"></param>
		/// <param name="intID"></param>
		/// <param name="sortTitle"></param>
		/// <param name="updatingElasticSearch"></param>
		/// <param name="skipLRPublish"></param>
		/// <param name="user"></param>
		/// <param name="publishForOrgId">Optional will be a valid OrgId if user is publishing for an org</param>
		[Obsolete]
		public static void PublishToAll_OLD( Resource input,
			ref bool isSuccessful,
			ref string status,
			ref int versionID,
			ref int intID,
			ref string sortTitle,
			bool updatingElasticSearch,
			bool skipLRPublish,
			Patron user,
			int publishForOrgId )
		{
			bool success = true;
			string tempStatus = "";
			string lrDocID = "";
			string continueOnPublishError = UtilityManager.GetAppKeyValue( "continueOnPublishError", "yes" );
			//Publish to LR. This will give us an LR Doc ID
			if ( !skipLRPublish )
			{
				PublishToLearningRegistry2( input, 
							ref success, 
							ref tempStatus, 
							ref lrDocID );
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
					else
					{
						EmailManager.NotifyAdmin( "Error during LR Publish", "Error: " + tempStatus + "<p>The error was encountered during the LR publish. The system continued with saving to the database and elastic search. </p>" );
					}
				}
			}
			input.Version.LRDocId = lrDocID;

			//If successful, publish to Database. This will give us a Resource Version ID
			PublishToDatabase( input, publishForOrgId, ref success, ref tempStatus, ref versionID, ref intID, ref sortTitle );
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

			new ActivityBizServices().PublishActivity( input, user );

			isSuccessful = true;
			status = "okay";
			SetConsoleSuccessMessage( "Successfully published the Resource" );

			//new ResourceThumbnailManager().CreateThumbnailAsynchronously( input.Id, input.ResourceUrl, false );
			//new Thumbnailer().CreateThumbnail( input.Id, input.ResourceUrl );
			ThumbnailServices.CreateThumbnail( input.Id.ToString(), input.ResourceUrl, false );

			SendPublishNotification( user, input );
		}


		public static void PublishToLearningRegistry2( Resource input,
					ref bool successful,
					ref string status,
					ref string lrDocID )
		{
			//Create payload
			var payload = new ResourceJSONManager().GetJSONLRMIFromResource( input );

			PublishToLearningRegistry(
			  new JavaScriptSerializer().Serialize( payload ),
			  input.ResourceUrl,
			  input.Version.Submitter,
			  input.Keyword.Select( m => m.OriginalValue.Trim() ).ToList(),
			  ref successful,
			  ref status,
			  ref lrDocID
			);
		}


		/// <summary>
		/// Publish resource to LR
		/// NOTE: keep code for filling envelop in sync (better to merge!) with CreateLREnvelope AND BuildDocument
		/// </summary>
		/// <param name="payload"></param>
		/// <param name="url"></param>
		/// <param name="submitter"></param>
		/// <param name="keywords"></param>
		/// <param name="successful"></param>
		/// <param name="status"></param>
		/// <param name="lrDocID"></param>
		public static void PublishToLearningRegistry( string payload,
								  string url,
								  string submitter,
								  List<string> keywords,
								  ref bool successful,
								  ref string status,
								  ref string lrDocID )
		{
			//Create document
			lr_document doc = new lr_document();
			LoggingHelper.DoTrace( 8, " %%%%% PublishingServices.PublishToLearningRegistry - New Publish %%%%%%" );

			try
			{
				doc.resource_data_type = "metadata";
				doc.payload_placement = "inline";
				doc.doc_version = UtilityManager.GetAppKeyValue( "lr_doc_version", "0.51.0" );
				doc.payload_schema = new List<string> { "LRMI" };
				doc.resource_data = payload;
				doc.resource_locator = url;
				//keywords
				doc.keys = keywords;

				if ( !string.IsNullOrWhiteSpace( lrDocID ) )
				{
					//doing replace
					doc.replaces = new List<string> { lrDocID };
					LoggingHelper.DoTrace( 3, " %%%%% PublishingServices.PublishToLearningRegistry - doing registry update of LrDoc: " + lrDocID );
				}

				//Sign the document
				SignDocument( submitter, ref doc );

				//Build the envelope
				lr_Envelope envelope = new lr_Envelope();
				envelope.documents.Add( doc );

				//Do publish
				string node = UtilityManager.GetAppKeyValue( "learningRegistryNodePublish" );
				string clientID = UtilityManager.GetAppKeyValue( "learningRegistryUserId" );
				string clientPassword = UtilityManager.GetAppKeyValue( "learningRegistryPassword" );
				LRClient client = new LRClient( node, clientID, clientPassword );

				//LoggingHelper.DoTrace( 4, " %%%%% PublishingServices.PublishToLearningRegistry - doing Publish ");
				PublishResponse response = client.Publish( envelope );

				lrDocID = response.document_results.ElementAt( 0 ).doc_ID;
				if ( !string.IsNullOrWhiteSpace( lrDocID ) && lrDocID.Length > 10 )
				{
					status = "Successfully published. LR Doc ID: " + lrDocID;
					LoggingHelper.DoTrace( 4, " %%%%% PublishingServices.PublishToLearningRegistry - Successfully published. LR Doc ID: " + lrDocID);
					//quick read to trace document
					if ( System.DateTime.Now.Day == 10 )
					{
						//lr_document lrDoc = client.ObtainDocByID( lrDocID );
					}
				}
				else
				{
					//need to get a message from the response, or update Publish
					successful = false;
					lrDocID = "";
					if ( response != null && response.document_results != null && response.document_results.Count > 0 )
						status = "Failed to Publish: " + response.document_results[0].error;
					else
						status = "Failed to Publish: - contact system administration" ;

					LoggingHelper.DoTrace( 2, " %%%%% PublishingServices.PublishToLearningRegistry - Error. " + status );
				}
				
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "PublishingServices.PublishToLearningRegistry()" );
				successful = false;
				lrDocID = "";
				status = "Failed to Publish: " + ex.Message;
			}
}


		/// <summary>
		/// Delete a document from the registry (asynchronously)
		/// </summary>
		/// <param name="lrDocID"></param>
		/// <param name="submitter"></param>
		/// <param name="deletedDocId">If successful contains docId of the document with the delete/replacement</param>
		/// <param name="successful"></param>
		/// <param name="status"></param>
		public static void DeleteFromRegistry( string lrDocID,
					Patron user )
		{
			//,
			//		ref string deletedDocId,
			//		ref bool successful,
			//		ref string status
			//do async, as no need for end user to be notified
			System.Threading.ThreadPool.QueueUserWorkItem( delegate
			{
				DeleteFromRegistryAsync( lrDocID, user );
			} );
		}


		/// <summary>
		/// Delete a document from the registry
		/// </summary>
		/// <param name="lrDocID"></param>
		/// <param name="submitter"></param>
		/// <param name="deletedDocId">If successful contains docId of the document with the delete/replacement</param>
		/// <param name="successful"></param>
		/// <param name="status"></param>
		private static void DeleteFromRegistryAsync( string lrDocID,
					Patron user )
		{
			
			bool successful = true;
			string status = "";
			string deletedDocId = "";
			string submitter = user.FullName();

			if ( string.IsNullOrWhiteSpace( lrDocID ) )
			{
				successful = false;
				status = "An valid LR DOC ID was was not provided";
				return;
			}
			//Create document
			lr_document doc = new lr_document();
			try
			{
				doc.replaces = new List<string> { lrDocID };
				doc.payload_placement = "none";
				doc.resource_data_type = "metadata";
				doc.doc_version = UtilityManager.GetAppKeyValue( "lr_doc_version", "0.51.0" );

				//Sign the document
				SignDocument( submitter, ref doc );

				//Build the envelope
				lr_Envelope envelope = new lr_Envelope();
				envelope.documents.Add( doc );

				//Do publish
				string node = UtilityManager.GetAppKeyValue( "learningRegistryNodePublish" );
				string clientID = UtilityManager.GetAppKeyValue( "learningRegistryUserId" );
				string clientPassword = UtilityManager.GetAppKeyValue( "learningRegistryPassword" );
				LRClient client = new LRClient( node, clientID, clientPassword );

				//trace
				LoggingHelper.DoTrace( 3, "Delete document \r\r" + new JavaScriptSerializer().Serialize( envelope ) );
				PublishResponse response = client.Publish( envelope );

				//Set return values
				successful = true;
				deletedDocId = response.document_results.ElementAt( 0 ).doc_ID;
				if ( !string.IsNullOrWhiteSpace( deletedDocId ) && deletedDocId.Length > 10 )
				{
					status = submitter + " successfully deleted document from the Credential Registry. Related Doc ID: " + deletedDocId;

					new ActivityBizServices().AddActivity( "Learning Registry", "Document Delete", status, user == null ? 0 : user.Id, 0 );

					LoggingHelper.DoTrace( 3, status );
				}
				else
				{
					//need to get a message from the response
					successful = false;
					deletedDocId = "";
					if ( response != null && response.document_results != null && response.document_results.Count > 0 )
						status = "Failed to delete the registry document: " + response.document_results[ 0 ].error;
					else
						status = "Failed to delete the registry document: - contact system administration";
				}
			}
			catch ( Exception ex )
			{
				successful = false;
				status = "Failed to delete document: " + ex.Message;
			}
			if ( !successful )
			{
				EmailManager.NotifyAdmin( "Error encountered while attempting to delete LR document", status );
				status = status.Replace( "<br/>", "\\r\\n" );
				LoggingHelper.DoTrace( 3, status );
			}
		}

		public static void PublishToDatabase( LRWarehouse.Business.ResourceV2.ResourceDTO input
						, int publishForOrgId
						, int updatedById
						, List<int> selectedTags
						, ref bool successful
						, ref string status
						, ref int versionID
						, ref int intID
						, ref string sortTitle )
		{
			ResourceManager resMgr = new ResourceManager();
			bool creatingSystemResource = false;	
			try
			{
				//Resource
				if ( input.ResourceId == 0 )
				{
					var resource = new Resource()
					{
						ResourceUrl = input.Url,
						CreatedById = input.CreatedById,
					};
					intID = resMgr.Create( resource, ref status );
					if ( intID == 0 )
					{
						successful = false;
						return;
					}
					else
					{
						input.ResourceId = intID;
						creatingSystemResource = true;
					}
				}
				//Version

				//Access Rights
				var finalAccessRightsValue = "";
				var finalAccessRightsId = 2;
				var targetAccessRights = input.Fields.Where( m => m.Schema == "accessRights" ).FirstOrDefault().Tags.Where( t => t.Selected ).FirstOrDefault();
				if ( targetAccessRights != null )
				{
					//Convert new access rights ID to old access rights ID for storage in version table
					var accessRightsDBField = new ResourceV2Services().GetFieldAndTagCodeData(false).Where( m => m.Schema == "accessRights" ).FirstOrDefault();
					if ( accessRightsDBField != null )
					{
						var matchedAccessRightsDB = accessRightsDBField.Tags.Where( m => m.Id == targetAccessRights.Id ).FirstOrDefault();
						if ( matchedAccessRightsDB != null )
						{
							finalAccessRightsId = matchedAccessRightsDB.OldCodeId;
							finalAccessRightsValue = matchedAccessRightsDB.Title;
						}
					}
					//var accessRights = targetAccessRights.Tags.Where(t => t.Selected).FirstOrDefault() ?? targetAccessRights.Tags.First();
				}

				var versionManager = new ResourceVersionManager();
				var version = new ResourceVersion()
				{
					Id = versionID,
					LRDocId = input.LrDocId,
					Title = input.Title,
					Description = input.Description,
					Publisher = input.Publisher,
					Creator = input.Creator,
					Rights = input.UsageRights.Url,
					UsageRightsId = input.UsageRights.CodeId,
					AccessRights = finalAccessRightsValue,
					Modified = DateTime.Now,
					Submitter = input.Submitter,
					Created = DateTime.Now, //Still not sure if this means "resource creation" or "table record creation" date. Going with the safer of the two
					TypicalLearningTime = "", //No longer used
					IsSkeletonFromParadata = false,
					Schema = "LRMI",
					AccessRightsId = finalAccessRightsId,
					InteractivityTypeId = 0, //No longer used
					ResourceIntId = intID,
					Requirements = input.Requirements
				};
				if ( versionID == 0 )
				{
					versionID = versionManager.Create( version, ref status );
				}
				else
				{
					versionManager.UpdateById( version );
				}

				//Keywords
				var keywordManager = new ResourceKeywordManager();
				foreach ( var item in input.Keywords )
				{
					keywordManager.Create( new ResourceChildItem()
					{
						ResourceIntId = intID,
						OriginalValue = item,
						CreatedById = updatedById	//input.CreatedById
					}, ref status );
				}

				//Standards
				//note: AlignmentDegreeId is actually the usage now as in Major, Supporting, Additional
				var standardManager = new ResourceStandardManager();
				foreach ( var item in input.Standards )
				{
					standardManager.Create( new ResourceStandard()
					{
						ResourceIntId = intID,
						StandardId = item.StandardId,
						AlignmentTypeCodeId = item.AlignmentTypeId,
						UsageTypeId = item.UsageTypeId,
						AlignmentTypeValue = item.AlignmentType,
						UsageType = item.UsageType,
						CreatedById = updatedById,	//input.CreatedById,
						AlignedById = updatedById,	//input.CreatedById,
						StandardDescription = item.Description,
						StandardNotationCode = item.NotationCode
					}, ref status );
				}

				//Tags
				//SelectMany condenses resulting lists into a single list--here it condenses the lists of selected tags into a single list
				//from which the IDs are selected
				//List<int> tags = input.Fields.SelectMany( t => t.Tags.Where( s => s.Selected ) ).ToList().Select( t => t.Id ).ToList();
				//Actually, we just want to add the new tags, because EF doesn't do a duplicate check
				try
				{
					if ( selectedTags != null && selectedTags.Count > 0 )
					{
						new IOERBusinessEntities.EFResourceManager().Resource_CreateTags( selectedTags, input.ResourceId, updatedById );
					}
				}
				catch ( Exception ex )
				{
					//ServiceHelper.DoTrace(4, "********PublishingServices.PublishToDatabase. EXCEPTION ENCOUNTERED");
					//skip throw, to allow continuing!!!
					ServiceHelper.LogError( ex, "PublishingServices.PublishToDatabase. " + "Entity Framework Error: " + ex.Message + ";<br />InnerException: " + ex.InnerException.Message + ";<br />" + ex.InnerException.ToString() + ";" );
					//throw new Exception("Entity Framework Error: " + ex.Message + ";<br />InnerException: " + ex.InnerException.Message + ";<br />" + ex.InnerException.ToString() + ";");
				}

				sortTitle = versionManager.Get( versionID ).SortTitle.Replace( " ", "_" );
				successful = true;
				status = "okay";
				if ( creatingSystemResource )
				{

				}
			}
			catch ( Exception ex )
			{
				successful = false;
				status = ex.Message;
			}
		}


		[Obsolete]
		private static void PublishToDatabase( Resource input
						, int publishForOrgId
						, ref bool successful
						, ref string status
						, ref int versionID
						, ref int intID
						, ref string sortTitle )
		{
			try
			{
				string statusMessage = "";
				ResourceDataManager dataManager = new ResourceDataManager();
				ResourceManager resMgr = new ResourceManager();
				input.PublishedForOrgId = publishForOrgId;
				//Resource. Use method that just uses url. Published by will be handled here
				//intID = resMgr.Create( input, ref status );
				intID = resMgr.CreateByUrl( input.ResourceUrl, ref status );
				input.Id = intID;
				input.Version.ResourceIntId = intID;

				//published by. Probably better to do here, rather than burying it?
				if ( input.CreatedById > 0 )
				{
					resMgr.Create_ResourcePublishedBy( intID, input.CreatedById, publishForOrgId, ref statusMessage );
				}

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

		#region Obsolete LR Document methods
		/// <summary>
		/// ===> NOT USED
		/// </summary>
		/// <param name="resourceEntity"></param>
		/// <returns></returns>
		//[Obsolete]
		//private static lr_document BuildDocumentXXX( ref Resource resourceEntity )
		//{
		//	lr_document doc = new lr_document();
		//	//string resourceData = BuildPayloadLRMI_JSON( resourceEntity );
		//	//OR?
		//	var resourceData = new ResourceJSONManager().GetJSONLRMIFromResource(resourceEntity);



		//	//Required Fields
		//	doc.resource_data_type = "metadata";
		//	doc.payload_placement = "inline";
		//	doc.doc_version = UtilityManager.GetAppKeyValue("lr_doc_version", "0.51.0");
		//	doc.payload_schema = new List<string>( new string[] { "LRMI" } );

		//	//Most of the data is in here:
		//	doc.resource_data = resourceData;

		//	//Resource Locator
		//	doc.resource_locator = resourceEntity.ResourceUrl;

		//	//Submitter Informaton
		//	lr_identity identity = new lr_identity();
		//	identity.submitter_type = "agent";
		//	identity.signer = UtilityManager.GetAppKeyValue("lrIdentitySigner", "error");
		//	string defaultSubmitter = UtilityManager.GetAppKeyValue("defaultSubmitter", "IOER");
		//	if ( resourceEntity.Version.Submitter.ToLower().IndexOf( defaultSubmitter.ToLower() ) == -1 )
		//		resourceEntity.Version.Submitter = defaultSubmitter + resourceEntity.Version.Submitter;
		//	identity.submitter = resourceEntity.Version.Submitter;
			
		//	doc.identity = identity;

		//	//Keywords
		//	foreach ( ResourceChildItem keyword in resourceEntity.Keyword )
		//	{
		//		doc.keys.Add( keyword.OriginalValue.Trim() );
		//	}

		//	return doc;
		//}

		[Obsolete]
		private static string BuildPayloadLRMI_JSONXXX( Resource entity )
		{
			ResourceJSONManager jsonManager = new ResourceJSONManager();
			ResourceJSONLRMI resource = jsonManager.GetJSONLRMIFromResource( entity );

			JavaScriptSerializer serializer = new JavaScriptSerializer();
			return serializer.Serialize( resource );
		}

		[Obsolete]
		private static lr_Envelope BuildEnvelope( lr_document doc )
		{
			lr_Envelope envelope = new lr_Envelope();

			envelope.documents.Add( doc );
			return envelope;
		}

		#endregion

		public static void PublishToElasticSearchAsynchronously( int resourceId )
		{
			System.Threading.ThreadPool.QueueUserWorkItem( delegate
			{
				var successful = true;
				var status = "okay";
				PublishToElasticSearch( resourceId, ref successful, ref status );
			} );
		}

		public static void PublishToElasticSearch( int resourceId, ref bool successful, ref string status )
		{
			//Do create
			try
			{
				//new ElasticSearchManager().CreateOrReplaceRecord( resourceId );
				//new ElasticSearchManager().RefreshResource( resourceId );
				new Isle.BizServices.ResourceV2Services().RefreshResource( resourceId );
				successful = true;
				status = "okay";
			}
			catch ( Exception ex )
			{
				successful = false;
				status = ex.Message;
			}
		}

		public static void BuildSaveLRDocument( Resource input,
				ref bool successful,
				ref string status )
		{
			var envelope = CreateLREnvelope( input, ref successful, ref status );
			var pending = new PublishPending()
			{
				ResourceId = input.Id,
				ResourceVersionId = input.Version.Id,
				Reason = "Resource requires approval.",
				CreatedById = input.CreatedById,
				LREnvelope = new JavaScriptSerializer().Serialize( envelope )
			};

			new ResourceManager().PublishPending_Create( pending, ref status );

}

		/// <summary>
		/// Fill an LR Envelope
		/// NOTE: keep filling code in sync with PublishToLearningRegistry
		/// </summary>
		/// <param name="input"></param>
		/// <param name="successful"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static lr_Envelope CreateLREnvelope(Resource input, ref bool successful, ref string status)
		{
			//Create payload
			var payload = new ResourceJSONManager().GetJSONLRMIFromResource(input);
			//keywords
			List<string> keywords = new List<string>();
			foreach (ResourceChildItem word in input.Keyword)
			{
				keywords.Add(word.OriginalValue.Trim());
			}

			return CreateLREnvelope(new JavaScriptSerializer().Serialize(payload), input.ResourceUrl, input.Version.Submitter, keywords, ref successful, ref status);
		}

		/// <summary>
		/// Fill an LR Envelope
		/// NOTE: keep filling code in sync with PublishToLearningRegistry and BuildDocument
		/// </summary>
		/// <param name="payload"></param>
		/// <param name="resourceUrl"></param>
		/// <param name="submitter"></param>
		/// <param name="keywords"></param>
		/// <param name="successful"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static lr_Envelope CreateLREnvelope( string payload,
													string resourceUrl,
													string submitter,
													List<string> keywords,
													ref bool successful,
													ref string status )
		{
			//Create payload
			//var payload = new ResourceJSONManager().GetJSONLRMIFromResource( input );

			//Create document
			lr_document doc = new lr_document();
			doc.resource_data_type = "metadata";
			doc.payload_placement = "inline";
			doc.doc_version = UtilityManager.GetAppKeyValue("lr_doc_version", "0.51.0");
			doc.payload_schema = new List<string> { "LRMI" };
			doc.resource_data = payload;
			doc.resource_locator = resourceUrl;
			//keywords
			doc.keys = keywords;

			//Identity info
			//lr_identity identity = new lr_identity();
			//identity.submitter_type = "agent";
			//identity.signer = UtilityManager.GetAppKeyValue("lrIdentitySigner", "error");
			//string defaultSubmitter = UtilityManager.GetAppKeyValue("defaultSubmitter", "IOER");
			////check if default submitter already included
			//if ( submitter.ToLower().IndexOf( defaultSubmitter.ToLower() ) == -1 )
			//	submitter = defaultSubmitter + submitter;
			//identity.submitter = submitter;

			//doc.identity = identity;

			//foreach ( ResourceChildItem word in input.Keyword )
			//{
			//	doc.keys.Add( word.OriginalValue.Trim() );
			//}

			
			SignDocument( submitter, ref doc );

			//Build the envelope
			lr_Envelope envelope = new lr_Envelope();
			envelope.documents.Add( doc );

			return envelope;
		}
		private static void SignDocument( string submitter, ref lr_document doc )
		{
			//LoggingHelper.DoTrace( 3, " %%%%% PublishingServices.SignDocument for " + submitter );

			string signingKeyLocation = UtilityManager.GetAppKeyValue( "signingKeyLocation", "" );
			if ( string.IsNullOrWhiteSpace( signingKeyLocation ) )
			{
				//produce error?

				return;
			}

			//Identity info
			lr_identity identity = new lr_identity();
			identity.submitter_type = "agent";
			identity.signer = UtilityManager.GetAppKeyValue( "lrIdentitySigner", "error" );
			string defaultSubmitter = UtilityManager.GetAppKeyValue( "defaultSubmitter", "IOER" );
			//check if default submitter already included
			if ( submitter.ToLower().IndexOf( defaultSubmitter.ToLower() ) == -1 )
				submitter = defaultSubmitter + submitter;
			identity.submitter = submitter;

			doc.identity = identity;

			string publicKeyLocationId = UtilityManager.GetAppKeyValue( "publicKeyLocationId", "" );
			//try
			//{
				string PgpKeyringLocation = Path.Combine( HttpRuntime.AppDomainAppPath, signingKeyLocation );
				string keyData = File.ReadAllText( PgpKeyringLocation );
				string[] PublicKeyLocations = new string[] { string.Format("http://pgp.mit.edu:11371/pks/lookup?op=get&search={0}",publicKeyLocationId) };
				string UserID = UtilityManager.GetAppKeyValue( "signingUserId", "error" );
				string password = UtilityManager.GetAppKeyValue( "signingPassword", "error" );

				PgpSigner signer = new PgpSigner( PublicKeyLocations, keyData, UserID, password );
				doc = signer.Sign( doc );
			//}
			//catch ( Exception ex )
			//{
			//	//maybe do not want to catch, let it fail?
			//}


		}
		public static void GenerateThumbnail( int resourceID, string url )
		{
			//new Thumbnailer().CreateThumbnail( resourceID, url );
			ThumbnailServices.CreateThumbnail( resourceID.ToString(), url, false );
		}

		public static lr_document LR_GetByDocID( string lrDocID )
		{
			string node = UtilityManager.GetAppKeyValue( "learningRegistryNodePublish" );
			string clientID = UtilityManager.GetAppKeyValue( "learningRegistryUserId" );
			string clientPassword = UtilityManager.GetAppKeyValue( "learningRegistryPassword" );
			LRClient client = new LRClient( node, clientID, clientPassword );

			lr_document lrDoc = client.ObtainDocByID( lrDocID );
			//add trace code
			LoggingHelper.DoTrace( 5, "lrDoc \r\r" + new JavaScriptSerializer().Serialize( lrDoc ) );
			return lrDoc;
		}

	}
}
