using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.IO;
using System.Web.Script.Services;

using ILPathways.Business;
using LRWarehouse.DAL;
using LRWarehouse.Business;
using LRWarehouse.Business.ResourceV2;
//using ILPathways.DAL;
using Isle.BizServices;
using IOER.classes;
using ILPathways.Utilities;
using IOER.Controllers;


using DatabaseManager = LRWarehouse.DAL.DatabaseManager;
using PatronManager = LRWarehouse.DAL.PatronManager;

namespace IOER.Services
{
	/// <summary>
	/// Summary description for ResourceService
	/// </summary>
	[WebService( Namespace = "http://tempuri.org/" )]
	[WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
	[System.ComponentModel.ToolboxItem( false )]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[System.Web.Script.Services.ScriptService]
	public class ResourceService : System.Web.Services.WebService
	{
		JavaScriptSerializer serializer = new JavaScriptSerializer();

		[WebMethod( EnableSession = true )]
		public string AjaxSaveResource( ResourceDTO input, bool testingMode )
		{
			try
			{
				var result = SaveResource( input, testingMode );
				return serializer.Serialize( result );
			}
			catch ( Exception ex )
			{
				return serializer.Serialize( Fail( "There was an error while attempting to publish the Resource. Please try again later.", ex.ToString() ) );
			}
		}
		public UtilityService.GenericReturn SaveResource( ResourceDTO input, bool testingMode )
		{
			//Determine where the slow parts are
			var stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();

			var service = new ResourceV2Services();
			var util = new UtilityService();
			var valid = true;
			var status = "";
			var versionID = input.VersionId;
			var intID = input.ResourceId;
			var sortTitle = input.UrlTitle;
			var lrDocID = input.LrDocId;
			var isNewResource = input.ResourceId == 0;
			var updateContentSpecialFields = false;
			var loggingData = new List<string>();
			string message = "";

			var errors = new List<string>();
			var succesfulLRPublish = false;
			var successfulDBPublish = false;
			var successfulESPublish = false;
			var successfulLibColAdd = false;

			//loggingData.Add( "TIMER: Initialization complete @ " + stopwatch.ElapsedMilliseconds );

			//Validate User
			var user = AccountServices.GetUserFromSession( Session );	//SessionManager.GetUserFromSession( Session ) ?? new Patron();
			if ( user.Id == 0 )
			{
				return Fail( "You must be logged in to save resources.", null );
			}

			//loggingData.Add( "TIMER: User retrieved @ " + stopwatch.ElapsedMilliseconds );

			//Validate basic tagging authority
			string allowingOpenPublishing = ServiceHelper.GetAppKeyValue( "allowingOpenPublishing", "no" );
			var privileges = Isle.BizServices.SecurityManager.GetGroupObjectPrivileges( user, "Isle.Controls.CanPublish" );
			if ( !privileges.CanCreate() && allowingOpenPublishing == "no" )
			{
				return Fail( "You do not have permission to save or publish resources.", null );
			}

			//loggingData.Add( "TIMER: Publish permissions checked @ " + stopwatch.ElapsedMilliseconds );

			//Validate Resource
			//URL
			//Strip trailing slash
			if ( input.Url.Last<char>() == '/' )
			{
				input.Url = input.Url.Substring( 0, input.Url.Length - 2 );
			}
			input.Url = util.ValidateURL( input.Url, isNewResource, ref valid, ref status );
			if ( !valid ) { return Fail( status, null ); }

			//loggingData.Add( "TIMER: URL validated @ " + stopwatch.ElapsedMilliseconds );

			//Title
			input.Title = util.ValidateText( input.Title, 3, "Title", ref valid, ref status );
			if ( !valid ) { return Fail( status, null ); }

			//loggingData.Add( "TIMER: Title validated @ " + stopwatch.ElapsedMilliseconds );

			//Description
			input.Description = util.ValidateText( input.Description, 25, "Description", ref valid, ref status );
			if ( !valid ) { return Fail( status, null ); }

			//loggingData.Add( "TIMER: Description validated @ " + stopwatch.ElapsedMilliseconds );

			//Keywords
			input.Keywords = input.Keywords.Distinct().ToList();
			foreach ( var item in input.Keywords )
			{
				util.ValidateText( item, 3, item, ref valid, ref status );
				if ( !valid ) { return Fail( status, null ); }
			}

			//loggingData.Add( "TIMER: Keywords validated @ " + stopwatch.ElapsedMilliseconds );

			//Usage Rights
			if ( input.UsageRights.CodeId == ContentItem.READ_THE_FINE_PRINT_CCOU )
			{
				//url required
				//Usage Rights URL
				if ( input.UsageRights.Url.Length == 0 )
				{
					status = "A URL must be entered when selecting Usage Rights of 'Read the Fine Print'";
					return Fail( status, null );
				}
				else
				{
					input.UsageRights.Url = util.ValidateURL( input.UsageRights.Url, false, ref valid, ref status );
					if ( !valid ) { return Fail( status, null ); }
				}
				//If the usage rights URL matches a known one from the code table, set it to that instead of custom - need a less hacky solution to the trailing slash issue though
				var codeRights = service.GetUsageRightsList().Where( m => !m.Custom && !m.Unknown && m.Url.ToLower() == (input.UsageRights.Url.ToLower() + "/") ).FirstOrDefault();
				if ( codeRights != null )
				{
					input.UsageRights.CodeId = codeRights.CodeId;
					input.UsageRights.Url = codeRights.Url;
					input.UsageRights.Custom = false;
				}
			}
			//if ( input.UsageRights.Url != "" )
			//{
			//	input.UsageRights.Url = util.ValidateURL( input.UsageRights.Url, false, ref valid, ref status );
			//	if ( !valid ) { return Fail( status, null ); }
			//}

			//loggingData.Add( "TIMER: Usage Rights validated @ " + stopwatch.ElapsedMilliseconds );

			//Creator
			input.Creator = util.ValidateText( input.Creator, 0, "Creator", ref valid, ref status );
			if ( !valid ) { return Fail( status, null ); }

			//Publisher
			input.Publisher = util.ValidateText( input.Publisher, 0, "Publisher", ref valid, ref status );
			if ( !valid ) { return Fail( status, null ); }

			//Requirements
			input.Requirements = util.ValidateText( input.Requirements, 0, "Requirements", ref valid, ref status );
			if ( !valid ) { return Fail( status, null ); }

			//loggingData.Add( "TIMER: Creator, Publisher, Requirements validated @ " + stopwatch.ElapsedMilliseconds );

			//Add automatic data
			string defaultSubmitter = UtilityManager.GetAppKeyValue( "defaultSubmitter", "ISLE OER on Behalf of " );
			input.Submitter = defaultSubmitter + user.FullName();
			input.CreatedById = user.Id;
			input.ResourceCreated = DateTime.Now.ToShortDateString();

			//loggingData.Add( "TIMER: Text inputs validated @ " + stopwatch.ElapsedMilliseconds );

			//Mark input tags as selected - useful for processing later
			var finalFields = serializer.Deserialize<List<FieldDTO>>( serializer.Serialize( service.GetFieldAndTagCodeData(false) ) );
			foreach ( var field in input.Fields )
			{
				var matchedField = finalFields.Where( f => f.Schema == field.Schema ).FirstOrDefault();
				foreach ( var tag in field.Tags )
				{
					tag.Selected = true;
					var matchedTag = matchedField.Tags.Where( t => t.Id == tag.Id ).FirstOrDefault();
					matchedTag.Selected = true;
				}
			}
			input.Fields = finalFields;

			//loggingData.Add( "TIMER: Tagged fields selected @ " + stopwatch.ElapsedMilliseconds );

			//Fill in standards
			foreach ( var item in input.Standards )
			{
				var standard = new StandardDataManager().StandardItem_Get( item.StandardId );
				item.NotationCode = standard.NotationCode;
				item.Description = standard.Description;
				item.Url = standard.StandardUrl;

				//any standard from tagger is direct
				item.IsDirectStandard = true;
			}

			//loggingData.Add( "TIMER: Standards added @ " + stopwatch.ElapsedMilliseconds );

			//Save
			if ( input.ResourceId > 0 ) //Updating existing resource
			{
				loggingData.Add( "Updating existing resource (ID " + input.ResourceId + ")" );

				//Get existing resource
				var existing = service.GetResourceDTO( input.ResourceId );
				if ( existing == null || existing.ResourceId == 0 )
				{
					return Fail( "Error getting the existing resource", "service.GetResourceDTO() did not return a valid resource" );
				}

				versionID = existing.VersionId;
				lrDocID = existing.LrDocId;
				sortTitle = existing.UrlTitle;

				//loggingData.Add( "TIMER: Existing resource retrieved @ " + stopwatch.ElapsedMilliseconds );

				//Protect against spoofed resource IDs
				if ( existing.Url != input.Url )
				{
					return Fail( "The target resource URL does not match the current resource URL.", "Input ResourceId does not match Existing ResourceId" );
				}

				//Check for existing or new keywords
				if ( input.Keywords.Count() == 0 && existing.Keywords.Count() == 0 )
				{
					return Fail( "You must add at least one keyword.", null );
				}

				//loggingData.Add( "TIMER: Preliminary checks complete @ " + stopwatch.ElapsedMilliseconds );

				//Check permissions to make sure the user can update this resource
				var openPublishing = UtilityManager.GetAppKeyValue( "allowingOpenPublishing", "no" ) == "yes";
				var canEdit = ResourceBizService.CanUserEditResource( input.ResourceId, user.Id );
				var permissions = SecurityManager.GetGroupObjectPrivileges( user, "IOER.Pages.ResourceDetail" );
				if ( !openPublishing )
				{
					if ( !permissions.CanUpdate() || !canEdit )
					{
						return Fail( "You do not have permission to update this resource.", null );
					}
				}

				//loggingData.Add( "TIMER: General update permission checks complete @ " + stopwatch.ElapsedMilliseconds );

				//Check to see if the user can update special fields
				if ( canEdit || permissions.CreatePrivilege > ( int ) ILPathways.Business.EPrivilegeDepth.State )
				{
					var didTitleOrDescriptionChange = ( input.Title != existing.Title || input.Description != existing.Description );
					updateContentSpecialFields = true;
				}
				else //Otherwise, overwrite the input
				{
					input.Title = existing.Title;
					input.Description = existing.Description;
					input.Requirements = existing.Requirements;
				}

				//loggingData.Add( "TIMER: Special field update permission checks complete @ " + stopwatch.ElapsedMilliseconds );

				//Ensure certain fields aren't changed
				input.Url = existing.Url;
				//cannot change the createdById!!!
				//Yes, but need to use current user for updates
				input.CreatedById = existing.CreatedById;  //( existing.CreatedById == 0 ? user.Id : existing.CreatedById );
				input.Submitter = existing.Submitter;

				//loggingData.Add( "TIMER: Update branch complete @ " + stopwatch.ElapsedMilliseconds );
			}
			else //Creating a new resource
			{
				loggingData.Add( "Creating a new resource" );

				//Ensure that keywords are added
				if ( input.Keywords.Count() == 0 )
				{
					return Fail( "You must add at least one keyword.", null );
				}

				//loggingData.Add( "TIMER: Keyword checks complete @ " + stopwatch.ElapsedMilliseconds );

				//Separated this so it gets tested instead of skipped
				//var payload = service.GetJSONLRMIPayloadFromResource( input, ref loggingData );
                var payload = service.GetJsonLdLrmiPayloadFromResource(input, false, ref loggingData);

				//loggingData.Add( "TIMER: Publish payload retrieved @ " + stopwatch.ElapsedMilliseconds );

				//If testing, change the data
				if ( testingMode )
				{
					input.Creator = "delete";
					input.Publisher = "delete";
					input.Description = "Test Data: " + input.Description;
				}

				//If in production and not testing, do LR publish
				if (ServiceHelper.GetAppKeyValue("doingLRPublish", "yes") == "yes" && !testingMode)
				{
					PublishingServices.PublishToLearningRegistry( payload, 
								input.Url, 
								input.Submitter, 
								input.Keywords, 
								ref valid, 
								ref status, 
								ref lrDocID );
					if ( !valid )
					{
						//return Fail( "There was an error publishing to the Learning Registry.", status );
						errors.Add( "There was an error publishing to the Learning Registry: " + status );
					}
					else
					{
						input.LrDocId = lrDocID;
						succesfulLRPublish = true;
					}

					//loggingData.Add( "TIMER: LR publish complete @ " + stopwatch.ElapsedMilliseconds );

				}
				else
				{
					succesfulLRPublish = true;
					LoggingHelper.DoTrace( 4, "%%%%%% LR publish skipped for " + input.Url );
					loggingData.Add( "TIMER: LR publish skipped @ " + stopwatch.ElapsedMilliseconds );
				}

				//We do however want to set the content's special fields for a new resource:
				updateContentSpecialFields = true;
			}

			//loggingData.Add( "TIMER: Processing for database begins @ " + stopwatch.ElapsedMilliseconds );

			//Regardless of new or update, save to Database
			var tags = input.Fields.SelectMany( i => i.Tags )
					.Where( t => t.Selected )
					.Select( t => t.Id ).ToList();

			//loggingData.Add( "TIMER: Relevant tags selected @ " + stopwatch.ElapsedMilliseconds );

			PublishingServices.PublishToDatabase( input, 
						input.OrganizationId, 
						user.Id, 
						tags, 
						ref valid, 
						ref status, 
						ref versionID, 
						ref intID, 
						ref sortTitle );
			if ( !valid )
			{
				return Fail( "There was an error publishing to the Database.", status );
			}
			input.VersionId = versionID;
			input.ResourceId = intID;
			input.UrlTitle = sortTitle;
			successfulDBPublish = true;

			//loggingData.Add( "TIMER: Database publish complete @ " + stopwatch.ElapsedMilliseconds );

			//Update ElasticSearch
			PublishingServices.PublishToElasticSearchAsynchronously( intID );
			successfulESPublish = true;
			//loggingData.Add( "TIMER: ElasicSearch publish complete @ " + stopwatch.ElapsedMilliseconds );

			//Add to library
			//TODO - this means elastic has to be called again to be updated after adding resource to the library.
			if ( input.LibraryIds.Count() != 0 && input.CollectionIds.Count() != 0 )
			{
				try
				{
					foreach ( var item in input.CollectionIds )
					{
						new LibraryBizService().LibraryResourceCreate( item, input.ResourceId, user, ref status );
					}
					successfulLibColAdd = true;
				}
				catch ( Exception ex )
				{
					return Fail( "There was a problem adding the resource to the selected library and collection.", ex.Message );
				}
			}
			else
			{
				successfulLibColAdd = true;
			}

			//loggingData.Add( "TIMER: Added resource to Library @ " + stopwatch.ElapsedMilliseconds );

			//Log activity
			System.Threading.ThreadPool.QueueUserWorkItem( delegate
			{
				new ActivityBizServices().PublishActivity( new ResourceManager().Get( input.ResourceId ), user );
			} );

			//loggingData.Add( "TIMER: Activity added @ " + stopwatch.ElapsedMilliseconds );

			//If there is an associated content item, set its privilege level to that of the resource
			var contentServices = new ContentServices();
			var canView = true;

			//First get by ID, for existing resource/content pairs
			loggingData.Add( "Loading content..." );
			var content = contentServices.GetForResourceDetail( input.ResourceId, ( Patron ) user, ref canView );
			if ( content != null && content.Id > 0 )
			{
				loggingData.Add( "Content loaded (first method): " + content.Id );
				//loggingData.Add( "TIMER: Content loaded @ " + stopwatch.ElapsedMilliseconds );
				SetContentData( input, content, contentServices, updateContentSpecialFields, false, loggingData );
				//loggingData.Add( "TIMER: Content set @ " + stopwatch.ElapsedMilliseconds );
			}
			//Otherwise try to get based on passed ID
			else if ( input.ContentId > 0 )
			{
				//this would imply only for a new resource, or res id would exist
				HandleRelatedContent( input, contentServices, user, updateContentSpecialFields, loggingData );
			}
			//URL publish only, so just create the thumbnail
			else
			{
				loggingData.Add( "No content found." );
				//Generate thumbnail

				//loggingData.Add( "TIMER: Beginning thumbnail generation @ " + stopwatch.ElapsedMilliseconds );
				//new ResourceThumbnailManager().CreateThumbnail( input.ResourceId, input.Url, true );
				loggingData.Add( "Creating thumbnail: " + input.ResourceId.ToString() + " - " + input.Url );
				ThumbnailServices.CreateThumbnail( input.ResourceId.ToString(), input.Url, true );
				//loggingData.Add( "TIMER: Thumbnail finished @ " + stopwatch.ElapsedMilliseconds );
			}

			//Return
			//loggingData.Add( "TIMER: All operations finished @ " + stopwatch.ElapsedMilliseconds );
			stopwatch.Stop();
			//Return compacted data
			var temp = input.Fields.SelectMany( m => m.Tags.Where( t => t.Selected ).Select( s => s.Id ) ).ToList<int>();
			input.Fields.Clear();
			
			if ( !succesfulLRPublish || !successfulDBPublish || !successfulESPublish || ( !successfulLibColAdd && input.LibraryIds.Count > 0 ) )
			{
				var errorList = string.Join( System.Environment.NewLine, errors );
				LoggingHelper.LogError( "Error publishing resource " + input.ResourceId + ": " + errorList );
			}

			return UtilityService.DoReturn( input, true, "okay", new { loggingData = loggingData, selected = temp } );
		}

		/// <summary>
		/// Handle updates where this is a NEW resource and is a content item 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="contentServices"></param>
		/// <param name="user"></param>
		/// <param name="updateContentSpecialFields"></param>
		/// <param name="loggingData"></param>
		private void HandleRelatedContent( ResourceDTO input,
					ContentServices contentServices,
					Patron user,
					bool updateContentSpecialFields,
					List<string> loggingData )
		{
			var content = contentServices.Get( input.ContentId );
			string status = "";
			loggingData.Add( "Content loaded (second method): " + content.Id );
			//loggingData.Add( "TIMER: Content loaded @ " + stopwatch.ElapsedMilliseconds );

			//Auto-add it to the SIUC collection of learning lists
			//may need to use a different, arbitrary user ID
			if ( content.TypeId == ContentItem.CURRICULUM_CONTENT_ID 
				|| content.TypeId == ContentItem.LEARNING_SET_CONTENT_ID )
			{
				int learningListCollectionId = UtilityManager.GetAppKeyValue( "learningListCollectionId", 0 );
				if ( learningListCollectionId > 0 )
					new LibraryBizService().LibraryResourceCreate( learningListCollectionId, input.ResourceId, user.Id, ref status );
			}

			SetContentData( input, content, contentServices, updateContentSpecialFields, true, loggingData );

			if ( content.TypeId == ContentItem.CURRICULUM_CONTENT_ID
				|| content.TypeId == ContentItem.LEARNING_SET_CONTENT_ID
				|| content.IsHierarchyType )
			{
				loggingData.Add( "Starting InitiateDelayedPublishing" );
				//just 50 for now
				//start auto publish  of hierarchy
				string resourceList = "";
				bool successful = new ResourceManager().InitiateDelayedPublishing( input.ContentId, input.ResourceId, user.Id, ref resourceList, ref status );

				//loggingData.Add( "TIMER: Delayed publishing initiated @ " + stopwatch.ElapsedMilliseconds );

				if ( successful && resourceList.Length > 0 )
				//if ( resourceList.Length > 0 )
				{
					//now add to other parts
					ResourceV2Services mgr2 = new ResourceV2Services();
					string statusMessage = "";
					//do the thumbs
					int thumbCntr = mgr2.AddThumbsForDelayedResources( content.Id, ref statusMessage );

					//now update elastic
					//this could be lengthy, do we want to handle with a scheduled task? NO - using async instead
					if ( UtilityManager.GetAppKeyValue( "doElasticIndexUpdateWithAutoPublish" ) == "yes" )
					{
						//ResourceV2Services mgr2 = new ResourceV2Services();
						System.Threading.ThreadPool.QueueUserWorkItem( delegate
						{
							int cntr = mgr2.AddDelayedResourcesToElastic( content.Id, ref statusMessage );
						} );
					}

					//Log activity
					System.Threading.ThreadPool.QueueUserWorkItem( delegate
					{
						new ActivityBizServices().AutoPublishActivity( new ResourceManager().Get( input.ResourceId ), user, resourceList );
					} );
				}
				else
				{
					//SetConsoleErrorMessage( "InitiateDelayedPublishing failed, or didn't return any resources.<br> : resourceList" );
					LoggingHelper.LogError( string.Format( "Unexpected condition - no related content was found under a learning list/hierarchy item. Id: {0}, typeId: {1}", content.Id, content.TypeId ), true );
					//return;
				}
			}
		}

		private void SetContentData( ResourceDTO input, ContentItem content, ContentServices contentServices, bool updateContentSpecialFields, bool isAdd, List<string> loggingData )
		{
			//Not sure if this needs some kind of check
			//Or a save() method of some sort
			content.PrivilegeTypeId = input.PrivilegeId;

			//Is org ID being saved somewhere? If not, input.OrganizationId should be used
			//NOTE: this is not being saved for some reason - commenting out for now
			/* var associatedOrg = OrganizationBizService.Get( input.OrganizationId );
			if ( associatedOrg != null && associatedOrg.Id > 0 ) //Might need to include user membership check?
			{
				content.ContentOrganization = associatedOrg;
			}
			*/

			if ( updateContentSpecialFields )
			{
				loggingData.Add( "Updating special fields" );
				content.Title = input.Title;
				content.Summary = input.Description;
				//==> no, potential serious error
				//content.Description = input.Description;
				
			}
			if ( isAdd 
				&& input.UsageRights.CodeId != content.UsageRightsId )
			{
				//should only allow if this is a new publish, can't do arbitrarily
				//PLUS seems to be setting to incorrect value
				content.UsageRightsId = input.UsageRights.CodeId;
				content.UsageRightsUrl = input.UsageRights.Url;
			}
			//Should we only update content if an add????
			if (isAdd)
			{
				//what if not entered, ex on a quick tag
				content.PrivilegeTypeId = input.PrivilegeId;
				content.StatusId = ContentItem.PUBLISHED_STATUS;
				content.ResourceIntId = input.ResourceId;
				contentServices.Update(content, false);

				input.ContentId = content.Id;
			}


			loggingData.Add( "input.PrivilegeId: " + input.PrivilegeId );
			loggingData.Add( "ContentItem.PUBLIC_PRIVILEGE: " + ContentItem.PUBLIC_PRIVILEGE );
			loggingData.Add( "content.DocumentUrl: " + content.DocumentUrl );
			loggingData.Add( "content.TypeId: " + content.TypeId );
			loggingData.Add( "content.ImageUrl: " + content.ImageUrl );

			//could do a check here to allow reverting an imageUrl???
			//or do for all types
			//if ( content.TypeId == ContentItem.CURRICULUM_CONTENT_ID )
			if ( string.IsNullOrWhiteSpace( content.ImageUrl ) == false )
			{
				//check for image - should check if resource already has - won't yet - phase one code
				if ( string.IsNullOrWhiteSpace( input.ThumbnailUrl ) )
				{

				}
				if ( string.IsNullOrWhiteSpace( content.ImageUrl ) == false
					&& content.ImageUrl != input.ThumbnailUrl )
				{
					new ResourceBizService().UpdateImageUrl( input.ResourceId, content.ImageUrl );
				}
			}

			//Change thumbnail to document if public and available
			//incorrect thumbURL ==> but not used anyway
			var thumbURL = "/content/" + content.Id;
			loggingData.Add( "Initial thumbnail URL: " + thumbURL );
			if ( input.PrivilegeId == ContentItem.PUBLIC_PRIVILEGE && !string.IsNullOrWhiteSpace( content.DocumentUrl ) )
			{
				loggingData.Add( "Changing thumbnail URL to: " + content.DocumentUrl );
				thumbURL = content.DocumentUrl;
				var id = content.ResourceIntId == 0 ? input.ResourceId : content.ResourceIntId;
				if ( id == 0 )
				{
					LoggingHelper.LogError( "Error creating thumbnail for Content ID " + content.Id + ". No Resource ID available at the time the thumbnail method was called" );
				}
				else
				{
					ThumbnailServices.CreateThumbnail( id.ToString(), thumbURL, false );
				}
			}
			//If the content already is the top level of a learning list and already has a thumbnail, do not replace the thumbnail
			if ( (content.TypeId == ContentItem.CURRICULUM_CONTENT_ID || content.TypeId == ContentItem.LEARNING_SET_CONTENT_ID) 
				&& !string.IsNullOrWhiteSpace( content.ImageUrl ) )
			{
				loggingData.Add( "Doing nothing with thumbnails" );
				//do nothing
			}
			else
			{
				//???for a new resource based on a content item (without an image, it appears that no thumb image will be created?
				loggingData.Add( "Overwriting thumbnail: " + "http://ioer.ilsharedlearning.org" + thumbURL );
				//Getting called needlessly - commenting out for now
				//new ResourceThumbnailManager().CreateThumbnail( input.ResourceId, "http://ioer.ilsharedlearning.org" + thumbURL, true );
			}

			//Actually save the changes
			//==> no only on add!
			if ( content.Id > 0 )
			{
				//contentServices.Update( content );
			}
		}

		#region Content Document things - used for tagger upload handling
		//Create a Document Item
		public ContentItem CreateContentDocument( byte[] fileBytes, string fileName, string mimeType, Patron user, int resourceID, int contentID, ref bool valid, ref string status )
		{
			var url = "";
			var result = new ContentItem()
			{
				Id = contentID,
				Title = "Uploaded File",
				FileName = fileName,
				MimeType = mimeType,
				TypeId = ContentItem.DOCUMENT_CONTENT_ID,
				Summary = "File uploaded by " + user.FullName() + " (" + user.Id + ") on " + DateTime.Now.ToLongDateString(),
				CreatedById = user.Id,
				OrgId = user.OrgId,
				PrivilegeTypeId = ContentItem.PUBLIC_PRIVILEGE,
				StatusId = ContentItem.SUBMITTED_STATUS //Set to 3, just in case of a failure before publish,then set after actual publish
			};

			//Check file size
			var maxFileSize = UtilityManager.GetAppKeyValue( "maxDocumentSize", 30000000 );
			if ( fileBytes.Length > maxFileSize )
			{
				valid = false;
				status = "Uploaded file is too large. Maximum file size is " + Math.Floor( ( maxFileSize / 1024f ) / 1024f ) + " megabytes.";
				return result;
			}

			//Scan the file
			new VirusScanner( maxFileSize ).Scan( fileBytes, ref valid, ref status );
			if ( !valid )
			{
				return result;
			}

			//Handle user replacing an uploaded file for a resource that was already tagged
			if ( resourceID > 0 )
			{
				if ( ResourceBizService.CanUserEditResource( resourceID, user.Id ) )
				{
					result.ResourceIntId = resourceID;
				}
				else
				{
					valid = false;
					status = "You don't have access to update that resource's file.";
					return result;
				}
			}
			if ( contentID > 0 )
			{
				var existing = new ContentServices().Get( contentID );
				if ( existing.CreatedById != user.Id && !ContentServices.DoesUserHaveContentEditAccess( contentID, user.Id ) )
				{
					valid = false;
					status = "You don't have access to update that resource's file.";
					return result;
		}
				else
				{
					result.Id = contentID;
				}
			}

			//Attempt to get content by resource ID 
			if ( resourceID > 0 && contentID == 0 )
			{
				var canView = true;
				//NOTE - may not want to use this, could be multiple content items (but not yet possible for files - yet). Will return first
				var existingContent = new ContentServices().GetForResourceDetail( resourceID, user, ref canView );

				if ( existingContent != null && existingContent.Id > 0 )
				{
					result.Id = existingContent.Id;
				}
			}

			//Upsert the file
			if ( contentID > 0 )
			{
				//Replace existing file
				valid = new FileResourceController().ReplaceDocument( fileBytes, fileName, mimeType, contentID, user.Id, ref status );
			}
			else
			{
				//Create the file
				var newID = FileResourceController.CreateContentItemWithFileOnly( fileName, mimeType, fileBytes, new ContentItem(), result, ref status );
				if ( newID > 0 )
				{
					result.Id = newID;
					valid = true;
				}
				else
				{
					valid = false;
					//status = "There was a problem uploading the file. Please try again later.";
				}
			}

			//Set URL - may not be necessary
			if ( string.IsNullOrWhiteSpace( result.DocumentUrl ) )
			{
				result.DocumentUrl = "/content/" + result.Id;
			}

			return result;

		} //

		//Delete a content document item
		public void DeleteContentDocument( int contentID, Patron user, ref bool valid, ref string status )
		{
			//Get content
			var service = new ContentServices();
			var content = service.Get( contentID );
			if ( content == null || content.Id == 0 )
			{
				valid = false;
				status = "Invalid Content ID";
				return;
			}

			//Ensure the content hasn't already been published
			if ( content.ResourceIntId > 0 )
			{
				valid = false;
				status = "You can't delete a file that has already been published.";
				return;
			}

			//Ensure the user has the right to delete the file
			//May need to add organization related handling here
			if ( content.CreatedById != user.Id )
			{
				valid = false;
				status = "You don't have access to delete that file.";
				return;
			}

			//Do the delete
			valid = service.Delete( contentID, user.Id, ref status );

			//Return

		} //

		//DTO
		public class UploadInfo
		{
			public string command { get; set; }
			public int resourceID { get; set; }
			public int contentID { get; set; }
			public string contentURL { get; set; }
			public string fileName { get; set; }
			public object extraData { get; set; }
			public bool valid { get; set; }
			public string status { get; set; }
		} //

		#endregion

		public UtilityService.GenericReturn Fail( string message, string exception )
		{
			return Fail( message, exception, new List<string>() );
		}

		public UtilityService.GenericReturn Fail( string message, string exception, List<string> loggingData )
		{
			if ( !string.IsNullOrWhiteSpace( exception ) )
			{
				LoggingHelper.LogError( "Tagger Error: " + message + " | Exception Data: " + exception, true );
			}
			return UtilityService.DoReturn( loggingData, false, message, exception );
		}

	}
}
