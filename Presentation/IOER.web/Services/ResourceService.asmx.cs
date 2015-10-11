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

			loggingData.Add( "TIMER: Initialization complete @ " + stopwatch.ElapsedMilliseconds );

			//Validate User
			var user = AccountServices.GetUserFromSession( Session );	//SessionManager.GetUserFromSession( Session ) ?? new Patron();
			if ( user.Id == 0 )
			{
				return Fail( "You must be logged in to save resources.", null );
			}

			loggingData.Add( "TIMER: User retrieved @ " + stopwatch.ElapsedMilliseconds );

			//Validate basic tagging authority
			string allowingOpenPublishing = ServiceHelper.GetAppKeyValue( "allowingOpenPublishing", "no" );
			var privileges = Isle.BizServices.SecurityManager.GetGroupObjectPrivileges( user, "Isle.Controls.CanPublish" );
			if ( !privileges.CanCreate() && allowingOpenPublishing == "no" )
			{
				return Fail( "You do not have permission to save or publish resources.", null );
			}

			loggingData.Add( "TIMER: Publish permissions checked @ " + stopwatch.ElapsedMilliseconds );

			//Validate Resource
			//URL
			input.Url = util.ValidateURL( input.Url, isNewResource, ref valid, ref status );
			if ( !valid ) { return Fail( status, null ); }

			loggingData.Add( "TIMER: URL validated @ " + stopwatch.ElapsedMilliseconds );

			//Title
			input.Title = util.ValidateText( input.Title, 3, "Title", ref valid, ref status );
			if ( !valid ) { return Fail( status, null ); }

			loggingData.Add( "TIMER: Title validated @ " + stopwatch.ElapsedMilliseconds );

			//Description
			input.Description = util.ValidateText( input.Description, 25, "Description", ref valid, ref status );
			if ( !valid ) { return Fail( status, null ); }

			loggingData.Add( "TIMER: Description validated @ " + stopwatch.ElapsedMilliseconds );

			//Keywords
			input.Keywords = input.Keywords.Distinct().ToList();
			foreach ( var item in input.Keywords )
			{
				util.ValidateText( item, 3, item, ref valid, ref status );
				if ( !valid ) { return Fail( status, null ); }
			}

			loggingData.Add( "TIMER: Keywords validated @ " + stopwatch.ElapsedMilliseconds );

			//Usage Rights
			if ( input.UsageRights.Url != "" )
			{
				input.UsageRights.Url = util.ValidateURL( input.UsageRights.Url, false, ref valid, ref status );
				if ( !valid ) { return Fail( status, null ); }
			}

			loggingData.Add( "TIMER: Usage Rights validated @ " + stopwatch.ElapsedMilliseconds );

			//Creator
			input.Creator = util.ValidateText( input.Creator, 0, "Creator", ref valid, ref status );
			if ( !valid ) { return Fail( status, null ); }

			//Publisher
			input.Publisher = util.ValidateText( input.Publisher, 0, "Publisher", ref valid, ref status );
			if ( !valid ) { return Fail( status, null ); }

			//Requirements
			input.Requirements = util.ValidateText( input.Requirements, 0, "Requirements", ref valid, ref status );
			if ( !valid ) { return Fail( status, null ); }

			loggingData.Add( "TIMER: Creator, Publisher, Requirements validated @ " + stopwatch.ElapsedMilliseconds );

			//Add automatic data
			string defaultSubmitter = UtilityManager.GetAppKeyValue( "defaultSubmitter", "ISLE OER on Behalf of " );
			input.Submitter = defaultSubmitter + user.FullName();
			input.CreatedById = user.Id;
			input.ResourceCreated = DateTime.Now.ToShortDateString();

			loggingData.Add( "TIMER: Text inputs validated @ " + stopwatch.ElapsedMilliseconds );

			//Mark input tags as selected - useful for processing later
			var finalFields = serializer.Deserialize<List<FieldDTO>>( serializer.Serialize( service.GetFieldAndTagCodeData() ) );
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

			loggingData.Add( "TIMER: Tagged fields selected @ " + stopwatch.ElapsedMilliseconds );

			//Fill in standards
			foreach ( var item in input.Standards )
			{
				var standard = new StandardDataManager().StandardItem_Get( item.StandardId );
				item.NotationCode = standard.NotationCode;
				item.Description = standard.Description;
				item.Url = standard.StandardUrl;
			}

			loggingData.Add( "TIMER: Standards added @ " + stopwatch.ElapsedMilliseconds );

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

				loggingData.Add( "TIMER: Existing resource retrieved @ " + stopwatch.ElapsedMilliseconds );

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

				loggingData.Add( "TIMER: Preliminary checks complete @ " + stopwatch.ElapsedMilliseconds );

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

				loggingData.Add( "TIMER: General update permission checks complete @ " + stopwatch.ElapsedMilliseconds );

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

				loggingData.Add( "TIMER: Special field update permission checks complete @ " + stopwatch.ElapsedMilliseconds );

				//Ensure certain fields aren't changed
				input.Url = existing.Url;
				//cannot change the createdById!!!
				input.CreatedById = existing.CreatedById;  //( existing.CreatedById == 0 ? user.Id : existing.CreatedById );
				input.Submitter = existing.Submitter;

				loggingData.Add( "TIMER: Update branch complete @ " + stopwatch.ElapsedMilliseconds );
			}
			else //Creating a new resource
			{
				loggingData.Add( "Creating a new resource" );

				//Ensure that keywords are added
				if ( input.Keywords.Count() == 0 )
				{
					return Fail( "You must add at least one keyword.", null );
				}

				loggingData.Add( "TIMER: Keyword checks complete @ " + stopwatch.ElapsedMilliseconds );

				//Separated this so it gets tested instead of skipped
				var payload = service.GetJSONLRMIPayloadFromResource( input, ref loggingData );

				loggingData.Add( "TIMER: Publish payload retrieved @ " + stopwatch.ElapsedMilliseconds );

				//If in dev, we don't want to modify the input at all and we don't want to publish
				if ( ServiceHelper.GetAppKeyValue( "envType", "prod" ) != "dev" )
				{
					//If not in dev, check for testing mode
					if ( !testingMode )
					{
						//If not in dev and not testing, then do LR publish
						PublishingServices.PublishToLearningRegistry( payload, input.Url, input.Submitter, input.Keywords, ref valid, ref status, ref lrDocID );
						if ( !valid )
						{
							return Fail( "There was an error publishing to the Learning Registry.", "" );
						}
						input.LrDocId = lrDocID;

						loggingData.Add( "TIMER: LR publish complete @ " + stopwatch.ElapsedMilliseconds );

					}
					else
					{
						//Otherwise, if not in dev but we ARE testing, then modify input
						input.Creator = "delete";
						input.Publisher = "delete";
						input.Description = "Test Data: " + input.Description;

						loggingData.Add( "TIMER: LR publish skipped @ " + stopwatch.ElapsedMilliseconds );
					}
				}

				//We do however want to set the content's special fields for a new resource:
				updateContentSpecialFields = true;
			}

			loggingData.Add( "TIMER: Processing for database begins @ " + stopwatch.ElapsedMilliseconds );

			//Regardless of new or update, save to Database
			var tags = input.Fields.SelectMany( i => i.Tags ).Where( t => t.Selected ).Select( t => t.Id ).ToList();

			loggingData.Add( "TIMER: Relevant tags selected @ " + stopwatch.ElapsedMilliseconds );

			PublishingServices.PublishToDatabase( input, input.OrganizationId, tags, ref valid, ref status, ref versionID, ref intID, ref sortTitle );
			if ( !valid )
			{
				return Fail( "There was an error publishing to the Database.", status );
			}
			input.VersionId = versionID;
			input.ResourceId = intID;
			input.UrlTitle = sortTitle;

			loggingData.Add( "TIMER: Database publish complete @ " + stopwatch.ElapsedMilliseconds );

			//Update ElasticSearch
			PublishingServices.PublishToElasticSearchAsynchronously( intID );
			loggingData.Add( "TIMER: ElasicSearch publish complete @ " + stopwatch.ElapsedMilliseconds );

			//Add to library
			if ( input.LibraryId != 0 && input.CollectionId != 0 )
			{
				try
				{
					new LibraryBizService().LibraryResourceCreate( input.CollectionId, input.ResourceId, user.Id, ref status );
				}
				catch ( Exception ex )
				{
					return Fail( "There was a problem adding the resource to the selected library and collection.", ex.Message );
				}
			}

			loggingData.Add( "TIMER: Added resource to Library @ " + stopwatch.ElapsedMilliseconds );

			//Log activity
			System.Threading.ThreadPool.QueueUserWorkItem( delegate
			{
				new ActivityBizServices().PublishActivity( new ResourceManager().Get( input.ResourceId ), user );
			} );

			loggingData.Add( "TIMER: Activity added @ " + stopwatch.ElapsedMilliseconds );

			//If there is an associated content item, set its privilege level to that of the resource
			var contentServices = new ContentServices();
			var canView = true;
			//First get by ID, for existing resource/content pairs
			loggingData.Add( "Loading content..." );
			var content = contentServices.GetForResourceDetail( input.ResourceId, ( Patron ) user, ref canView );
			if ( content != null && content.Id > 0 )
			{
				loggingData.Add( "Content loaded (first method): " + content.Id );
				loggingData.Add( "TIMER: Content loaded @ " + stopwatch.ElapsedMilliseconds );
				SetContentData( input, content, contentServices, updateContentSpecialFields, loggingData );
				loggingData.Add( "TIMER: Content set @ " + stopwatch.ElapsedMilliseconds );
			}
			//Otherwise try to get based on passed ID
			else if ( input.ContentId > 0 )
			{
				content = contentServices.Get( input.ContentId );
				loggingData.Add( "Content loaded (second method): " + content.Id );
				loggingData.Add( "TIMER: Content loaded @ " + stopwatch.ElapsedMilliseconds );

				//oops - can this result in a missed update. With a shared resource, publisher may not be the creator
				if ( content.CreatedById == user.Id || content.ResourceIntId == 0 )
				{
					SetContentData( input, content, contentServices, updateContentSpecialFields, loggingData );
					loggingData.Add( "TIMER: Content set @ " + stopwatch.ElapsedMilliseconds );

					if ( content.ResourceIntId == 0 && content.TypeId == ContentItem.CURRICULUM_CONTENT_ID )
					{
						//just 50 for now
						//start auto publish  of hierarchy
						string resourceList = "";
						new ResourceManager().InitiateDelayedPublishing( input.ContentId, input.ResourceId, user.Id, ref resourceList, ref status );

						loggingData.Add( "TIMER: Delayed publishing initiated @ " + stopwatch.ElapsedMilliseconds );

						if ( resourceList.Length > 0 )
						{
							new ActivityBizServices().AutoPublishActivity( new ResourceManager().Get( input.ResourceId ), user, resourceList );

							//this could be lengthy, do we want to handle with a scheduled task?
							if ( UtilityManager.GetAppKeyValue( "doElasticIndexUpdateWithAutoPublish" ) == "yes" )
							{
								ResourceV2Services mgr2 = new ResourceV2Services();
								mgr2.ImportRefreshResources( resourceList );
							}
						}
					}
				}
			}
			//URL publish only, so just create the thumbnail
			else
			{
				loggingData.Add( "No content found." );
				//Generate thumbnail

				loggingData.Add( "TIMER: Beginning thumbnail generation @ " + stopwatch.ElapsedMilliseconds );
				new ResourceThumbnailManager().CreateThumbnail( input.ResourceId, input.Url, true );
				loggingData.Add( "TIMER: Thumbnail finished @ " + stopwatch.ElapsedMilliseconds );
			}

			//Return
			loggingData.Add( "TIMER: All operations finished @ " + stopwatch.ElapsedMilliseconds );
			stopwatch.Stop();
			return UtilityService.DoReturn( input, true, "okay", loggingData );
		}

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

		private void SetContentData( ResourceDTO input, ContentItem content, ContentServices contentServices, bool updateContentSpecialFields, List<string> loggingData )
		{
			if ( updateContentSpecialFields )
			{
				loggingData.Add( "Updating special fields" );
				content.Title = input.Title;
				content.Summary = input.Description;
				//==> no, potential serious error
				//content.Description = input.Description;
			}
			//what if not entered, ex on a quick tag
			content.PrivilegeTypeId = input.PrivilegeId;
			content.StatusId = ContentItem.PUBLISHED_STATUS;
			content.ResourceIntId = input.ResourceId;
			contentServices.Update( content, false );
			input.ContentId = content.Id;

			loggingData.Add( "input.PrivilegeId: " + input.PrivilegeId );
			loggingData.Add( "ContentItem.PUBLIC_PRIVILEGE: " + ContentItem.PUBLIC_PRIVILEGE );
			loggingData.Add( "content.DocumentUrl: " + content.DocumentUrl );
			loggingData.Add( "content.TypeId: " + content.TypeId );
			loggingData.Add( "ContentItem.CURRICULUM_CONTENT_ID: " + ContentItem.CURRICULUM_CONTENT_ID );
			loggingData.Add( "content.ImageUrl: " + content.ImageUrl );

			//Change thumbnail to document if public and available
			var thumbURL = "/content/" + content.Id;
			loggingData.Add( "Initial thumbnail URL: " + thumbURL );
			if ( input.PrivilegeId == ContentItem.PUBLIC_PRIVILEGE && !string.IsNullOrWhiteSpace( content.DocumentUrl ) )
			{
				loggingData.Add( "Changing thumbnail URL to: " + content.DocumentUrl );
				thumbURL = content.DocumentUrl;
			}
			//If the content already has a is the top level of a learning list and already has a thumbnail, do not replace the thumbnail
			if ( content.TypeId == ContentItem.CURRICULUM_CONTENT_ID && !string.IsNullOrWhiteSpace( content.ImageUrl ) )
			{
				loggingData.Add( "Doing nothing with thumbnails" );
				//do nothing
			}
			else
			{
				loggingData.Add( "Overwriting thumbnail: " + "http://ioer.ilsharedlearning.org" + thumbURL );
				//Getting called needlessly - commenting out for now
				//new ResourceThumbnailManager().CreateThumbnail( input.ResourceId, "http://ioer.ilsharedlearning.org" + thumbURL, true );
			}

		}

	}
}
