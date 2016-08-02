using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web;
using System.IO;
using System.Net.Http;

using LRWarehouse.Business;
using ILPathways.Business;
using Isle.BizServices;
using LRWarehouse.Business.ResourceV2;
using ILPathways.Common;
using ILPathways.Utilities;

namespace Isle.BizServices
{
	public class ContentAttachmentServices
	{
		List<UsageRights> usageRightsCodes = new List<UsageRights>();
		List<CodeItem> privilegeCodes = new List<CodeItem>();
		ContentServices contentServices = new ContentServices();
		CurriculumServices curriculumServices = new CurriculumServices();

		//Get a list of attachments for a given node 
		public List<AttachmentDto> GetAttachments( int nodeID, ref bool valid, ref string status )
		{
			//Validate permission
			var access = GetValidatedUserForContent( nodeID );
			HandleStandardResponse( access, false, ref valid, ref status );
			if ( !valid )
			{
				return null;
			}

			//Get the codes
			usageRightsCodes = new ResourceV2Services().GetUsageRightsList();
			privilegeCodes = contentServices.ContentPrivilegeCodes_SelectList();

			//Get the list
			var list = new List<AttachmentDto>();
			foreach ( var item in access.Content.ChildItems.OrderBy( m => m.SortOrder ).ThenBy( m => m.Id ).ToList() )
			{
				list.Add( GetAttachmentDtoFromContentItem( item ) );
			}

			return list;
		}
		//

		//Get a single attachment
		public AttachmentDto GetAttachment( int nodeID, int contentID, ref bool valid, ref string status )
		{
			//Validate permission
			var access = GetValidatedUserForContent( nodeID );
			HandleStandardResponse( access, false, ref valid, ref status );
			if ( !valid )
			{
				return null;
			}

			//Do the get
			var content = contentServices.Get( contentID );
			if ( content == null || content.Id == 0 )
			{
				valid = false;
				status = "Error: invalid content ID";
				return null;
			}

			//Convert
			usageRightsCodes = new ResourceV2Services().GetUsageRightsList();
			privilegeCodes = contentServices.ContentPrivilegeCodes_SelectList();

			valid = true;
			status = "okay";
			return GetAttachmentDtoFromContentItem( content );
		}

		//Move an item to a new place in the order
		public List<AttachmentDto> SetSortOrder( int nodeID, int attachmentID, int targetOrder, ref bool valid, ref string status )
		{
			//Validate permission
			var access = GetValidatedUserForContent( nodeID );
			HandleStandardResponse( access, false, ref valid, ref status );
			if ( !valid )
			{
				return null;
			}

			//Get the list
			var list = access.Content.ChildItems.OrderBy( m => m.SortOrder ).ThenBy( m => m.Title ).ToList();

			//Do the move
			try
			{
				//Ensure the attachment to move is actually part of this content's children
				var toMove = list.FirstOrDefault( m => m.Id == attachmentID );
				if ( toMove == null )
				{
					valid = false;
					status = "Invalid attachment";
					return null;
				}

				//Move the attachment to just before or just after the target, depending on which direction the attachment is moving
				var targetToInsertNear = list.ElementAt( targetOrder - 1 );
				toMove.SortOrder = targetToInsertNear.SortOrder + ( toMove.SortOrder > targetToInsertNear.SortOrder ? -1 : 1 ); //If moving up, -1 else +1

				//Now that the list is ordered, shift things around in the database
				ResetSortOrders( list );

				//Set ref values
				return GetAttachments( nodeID, ref valid, ref status );
			}
			catch ( Exception ex )
			{
				valid = false;
				status = ex.Message;
				return null;
			}

		}
		//

		//Delete an attachment
		public void DeleteAttachment( int nodeID, int attachmentID, ref bool valid, ref string status )
		{
			//Validate permission
			var access = GetValidatedUserForContent( nodeID );
			HandleStandardResponse( access, false, ref valid, ref status );
			if ( !valid )
			{
				return;
			}

			//Do the delete
			try
			{
				//Ensure the content item is actually a part of the list of children for the node ID for which the user has been authenticated
				if ( access.Content.ChildItems.FirstOrDefault( m => m.Id == attachmentID ) != null )
				{
					valid = contentServices.Delete( attachmentID, access.User.Id, ref status );
				}
				else
				{
					status = "Error: That attachment is not a member of the current content's attachments.";
					valid = false;
				}
			}
			catch ( Exception e )
			{
				valid = false;
				status = e.Message;
			}
		}
		//

		//Update an attachment
		public AttachmentDto UpdateAttachment( int nodeID, AttachmentDto attachment, ref bool valid, ref string status )
		{
			//Validate permission
			var access = GetValidatedUserForContent( nodeID );
			HandleStandardResponse( access, false, ref valid, ref status );
			if ( !valid )
			{
				return null;
			}

			//Inputs should already be validated

			//Do the update
			try
			{
				//Ensure the content item is actually a part of the list of children for the node ID for which the user has been authenticated
				if ( access.Content.ChildItems.FirstOrDefault( m => m.Id == attachment.ContentId ) != null )
				{
					var existing = access.Content.ChildItems.FirstOrDefault( m => m.Id == attachment.ContentId );
					if( existing == null )
					{
						valid = false;
						status = "Error: invalid attachment ID";
						return null;
					}

					//Handle basic stuff
					existing.Title = attachment.Title;
					existing.Summary = attachment.Summary;
					existing.PrivilegeTypeId = attachment.PrivilegeTypeId;
					existing.ConditionsOfUseId = attachment.UsageRights.CodeId;
					existing.ConditionsOfUseUrl = attachment.UsageRights.Url;

					//Handle keywords
					foreach ( var item in attachment.Keywords )
					{
						var contentKeyword = new ContentKeyword()
						{
							ContentId = existing.Id,
							Created = DateTime.Now,
							CreatedById = access.User.Id,
							Keyword = item
						};
						existing.ContentKeywords.Add( contentKeyword );
					}

					//Handle Standards
					var existingStandards = CurriculumServices.ContentStandard_Select( existing.Id );
					var newStandards = new List<ContentStandard>();
					foreach ( var standard in attachment.Standards )
					{
						var matchedStandard = existingStandards.FirstOrDefault( m => m.StandardId == standard.StandardId );

						if ( matchedStandard == null )
						{
							newStandards.Add( new ContentStandard()
							{
								StandardId = standard.StandardId,
								AlignmentTypeCodeId = standard.AlignmentTypeId,
								UsageTypeId = standard.UsageTypeId,
								Created = DateTime.Now,
								ContentId = attachment.ContentId
							} );
						}
						else
						{
							curriculumServices.ContentStandard_Update( matchedStandard.StandardRecordId, standard.AlignmentTypeId, standard.UsageTypeId, access.User.Id, ref status );
						}
					}
					curriculumServices.ContentStandard_Add( nodeID, access.User.Id, newStandards );

					//Do the update
					contentServices.Update( existing );

					//Return a fresh attachment
					return GetAttachment( nodeID, existing.Id, ref valid, ref status );
				}
				else
				{
					status = "Error: That attachment is not a member of the current content's attachments.";
					valid = false;
					return null;
				}
			}
			catch ( Exception e )
			{
				valid = false;
				status = e.Message;
				return null;
			}

		}
		//

		//Remove attachment standard
		public void RemoveAttachmentStandard( int nodeID, int contentID, int recordID, ref bool valid, ref string status )
		{
			//Validate permission
			var access = GetValidatedUserForContent( nodeID );
			HandleStandardResponse( access, false, ref valid, ref status );
			if ( !valid )
			{
				return;
			}

			//Do the delete
			if ( access.Content.ChildItems.FirstOrDefault( m => m.Id == contentID ) != null )
			{
				valid = contentServices.ContentStandard_Delete( contentID, access.User.Id, recordID, ref status );
			}
		}
		//

		//Add a new attachment via URL
		public AttachmentDto AddUrl( int nodeID, string url, ref bool valid, ref string status )
		{
			//Validate permission
			var access = GetValidatedUserForContent( nodeID );
			HandleStandardResponse( access, false, ref valid, ref status );
			if ( !valid )
			{
				return null;
			}

			//Construct the attachment
			var content = new ContentItem()
			{
				Title = "Web URL: " + url,
				FileName = url,
				CreatedById = access.User.Id,
				Created = DateTime.Now,
				DocumentUrl = url,
				ParentId = nodeID,
				TypeId = 41, //URL
				SortOrder = 5,
				StatusId = ContentItem.INPROGRESS_STATUS
			};

			//Create the attachment
			var contentID = contentServices.Create_ef( content, ref status );
			if ( contentID == 0 )
			{
				valid = false;
				return null;
			}

			//Update Sort Orders
			var list = curriculumServices.GetCurriculumNodeForEdit( access.Content.Id, access.User ).ChildItems;
			ResetSortOrders( list );

			//Return a full representation of the attachment
			return GetAttachment( nodeID, contentID, ref valid, ref status );

		}
		//

		//Save a file uploaded from a device
		public AttachmentDto UploadDeviceFile( int nodeID, HttpPostedFile file, string filePath, string savingUrl, ref bool valid, ref string status )
		{
			try
			{
				//Get the file bytes
				var temp = new MemoryStream();
				file.InputStream.Position = 0;
				file.InputStream.CopyTo( temp );
				var fileBytes = temp.ToArray();
				temp.Dispose();
				file.InputStream.Position = 0;

				//Create the attachment file object
				var attachment = new AttachmentFile(){
					FileName = file.FileName,
					MimeType = file.ContentType,
					FileBytes = fileBytes,
					FileStream = file.InputStream
				};

				//Do the save
				var contentID = SaveNewAttachmentFile( nodeID, attachment, filePath, savingUrl, ref valid, ref status );
				if ( contentID == 0 )
				{
					valid = false;
					//Status already set
					return null;
				}

				return GetAttachment( nodeID, contentID, ref valid, ref status );
			}
			catch ( Exception ex )
			{
				valid = false;
				status = ex.Message;
				return null;
			}
		}
		//

		//Save a file from Google Drive
		public AttachmentDto UploadGoogleDriveFile( int nodeID, string name, string mimeType, string url, string token, string filePath, string savingUrl, ref bool valid, ref string status )
		{
			try
			{
				//Try to ensure the file has an extension
				try
				{
					var exportFormat = url.Split( new string[] { "&exportFormat=" }, StringSplitOptions.RemoveEmptyEntries )[ 1 ];
					if ( name.IndexOf( "." + exportFormat ) == -1 && exportFormat.Length > 0 )
					{
						name = name + "." + exportFormat;
					}
				}
				catch { }

				//Get the file bytes
				var getter = new HttpClient();
				getter.DefaultRequestHeaders.Add( "User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36" );
				getter.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue( "Bearer", token );
				var result = getter.GetAsync( url ).Result.Content;

				//Create the attachment file object
				var attachment = new AttachmentFile()
				{
					FileName = name,
					MimeType = mimeType,
					FileBytes = result.ReadAsByteArrayAsync().Result,
					FileStream = result.ReadAsStreamAsync().Result
				};

				//Do the save
				var contentID = SaveNewAttachmentFile( nodeID, attachment, filePath, savingUrl, ref valid, ref status );
				if ( contentID == 0 )
				{
					valid = false;
					//Status already set
					return null;
				}

				return GetAttachment( nodeID, contentID, ref valid, ref status );
			}
			catch ( Exception ex )
			{
				valid = false;
				status = ex.Message;
				return null;
			}
		}
		//

		//Save a file and return its content ID
		public int SaveNewAttachmentFile( int nodeID, AttachmentFile file, string filePath, string savingUrl, ref bool valid, ref string status )
		{
			//Validate permission
			var access = GetValidatedUserForContent( nodeID );
			HandleStandardResponse( access, false, ref valid, ref status );
			if ( !valid )
			{
				return 0;
			}

			//Validate the file
			ValidateFile( file.FileStream, ref valid, ref status );
			if ( !valid )
			{
				return 0;
			}

			//Build content item
			var attachment = new ContentItem()
			{
				ParentId = access.Content.Id,
				Title = file.FileName,
				StatusId = ContentItem.INPROGRESS_STATUS,
				PrivilegeTypeId = ContentItem.PUBLIC_PRIVILEGE,
				TypeId = ContentItem.DOCUMENT_CONTENT_ID,
				MimeType = file.MimeType,
				CreatedById = access.User.Id,
				Created = DateTime.Now,
				LastUpdatedById = access.User.Id,
				SortOrder = 5
			};

			//Build the document object
			var doc = new DocumentVersion()
			{
				RowId = new Guid(),
				CreatedById = access.User.Id,
				Created = DateTime.Now,
				FileDate = DateTime.Now,
				LastUpdatedById = access.User.Id,
				FileName = FileSystemHelper.SanitizeFilename( Path.GetFileNameWithoutExtension( file.FileName ) + Path.GetExtension( file.FileName ).ToLower() ),
				MimeType = file.MimeType,
				FilePath = filePath,
			};
			doc.Title = Path.GetFileNameWithoutExtension( doc.FileName );
			doc.ResourceUrl = savingUrl + "/" + doc.FileName;

			//Save the file
			try
			{
				FileSystemHelper.CreateDirectory( filePath );
				var diskFile = filePath + "\\" + file.FileName;
				File.WriteAllBytes( diskFile, file.FileBytes );
			}
			catch ( Exception ex )
			{
				valid = false;
				status = ex.Message.ToString();
				LoggingHelper.LogError( ex, "ContentAttachmentServices.SaveFile( " + filePath + ", " + file.FileName + ")" );
				return 0;
			}

			//Set resource data
			doc.SetResourceData( file.FileBytes.LongLength, file.FileBytes );
			var docID = contentServices.DocumentVersionCreate( doc, ref status );
			if ( string.IsNullOrWhiteSpace( docID ) )
			{
				valid = false;
				status = "Error setting resource data";
				return 0;
			}
			doc.RowId = new Guid( docID );

			//Create new ContentItem
			attachment.DocumentRowId = doc.RowId;
			attachment.DocumentUrl = doc.ResourceUrl;
			var fileID = contentServices.Create( attachment, ref status );
			if ( fileID == 0 )
			{
				valid = false;
				status = "Error updating content item after uploading document: " + status;
				return 0;
			}

			//Fix sort orders
			//Refresh the list of child items
			var updatedNode = curriculumServices.GetCurriculumNodeForEdit( nodeID, access.User );
			ResetSortOrders( updatedNode.ChildItems );

			//Success
			valid = true;
			status = "okay";
			return fileID;

		}
		//

		//Get an attachment's image, or create it if necessary
		public string GetAttachmentImage( int nodeID, int contentID, bool generateIfMissing, ref bool isBeingGenerated, ref bool valid, ref string status )
		{
			if ( generateIfMissing )
			{
				//Validate permission
				var access = GetValidatedUserForContent( nodeID );
				HandleStandardResponse( access, false, ref valid, ref status );
				if ( !valid )
				{
					return "";
				}

				//Verify that the target attachment actually belongs to the node
				var attachment = access.Content.ChildItems.FirstOrDefault( m => m.Id == contentID );
				if ( attachment != null )
				{
					return DetermineImageUrl( attachment, true, true, ref isBeingGenerated );
				}
				else
				{
					valid = false;
					status = "Error: invalid attachment.";
					return "";
				}
			}
			else
			{
				var attachment = contentServices.Get(contentID);
				return DetermineImageUrl( attachment, false, false, ref isBeingGenerated );
			}

		}

		#region Helper Methods
		//Get AttachmentDto from ContentItem
		protected AttachmentDto GetAttachmentDtoFromContentItem( ContentItem item )
		{
			//Prevent errors
			if( usageRightsCodes == null || usageRightsCodes.Count() == 0 )
			{
				usageRightsCodes = new ResourceV2Services().GetUsageRightsList();
			}
			if( privilegeCodes == null || privilegeCodes.Count() == 0 )
			{
				privilegeCodes = contentServices.ContentPrivilegeCodes_SelectList();

			}

			//Assign the correct usage rights
			UsageRights matchedRights = usageRightsCodes.FirstOrDefault( m => m.CodeId == item.ConditionsOfUseId ) ?? usageRightsCodes.FirstOrDefault( m => m.Unknown );
			//Set the URL if using a custom URL
			matchedRights.Url = item.ConditionsOfUseUrl;
			
			var generatingImage = false;
			var thumbnailUrl = DetermineImageUrl( item, true, true, ref generatingImage );
			return new AttachmentDto()
				{
					ContentId = item.Id,
					ResourceId = item.ResourceIntId,
					Url = item.DocumentUrl,
					SortOrder = item.SortOrder,
					ContentTypeId = item.TypeId,
					Title = item.Title,
					FileName = item.FileName,
					Summary = item.Summary,
					Keywords = ( item.ContentKeywords ?? new List<ContentKeyword>() ).Select( m => m.Keyword ).ToList(),
					PrivilegeTypeId = item.PrivilegeTypeId,
					PrivilegeTypeTitle = privilegeCodes.FirstOrDefault( m => m.Id == item.PrivilegeTypeId ).Title,
					UsageRights = matchedRights,
					ImageUrl = thumbnailUrl,
					ImageInProgress = generatingImage,
					Standards = GetStandards( item )
				};
		}

		//Arbitrarily reorder items
		protected void ResetSortOrders( List<ContentItem> list )
		{
			//Ensure the list is ordered
			list = list.OrderBy( m => m.SortOrder ).ThenBy( m => m.Title ).ToList();

			//Assign arbitrary sort order to all items - have to do this to overcome default sort order
			var order = 10;
			var tracker = new Dictionary<int, int>();
			foreach ( var item in list )
			{
				tracker.Add( item.Id, item.SortOrder ); //Track the item for comparison later
				item.SortOrder = order;
				order = order + 10;
			}

			//Save any applicable changes
			foreach ( var item in list )
			{
				if ( tracker[ item.Id ] != item.SortOrder ) //Only update if we need to
				{
					contentServices.Update( item );
				}
			}
		}
		//

		//Get a user and a content item along with the user's level of access to that item
		public ValidationPermissions GetValidatedUserForContent( int contentID )
		{
			var permissions = new ValidationPermissions();

			permissions.User = AccountServices.GetUserFromSession();
			if ( permissions.User == null || permissions.User.Id == 0 )
			{
				return permissions;
			}

			permissions.Content = curriculumServices.GetCurriculumNodeForEdit( contentID, permissions.User );
			if ( permissions.Content == null || permissions.Content.Id == 0 )
			{
				return permissions;
			}

			//Admin Check
			//Hack for specific admin IDs removed
			if ( permissions.User.TopAuthorization < ( int ) EUserRole.StateAdministrator )
			{
				permissions.CanRead = true;
				permissions.CanWrite = true;
				return permissions;
			}

			//Check to see if the user created the content
			if ( permissions.Content.CreatedById == permissions.User.Id )
			{
				permissions.CanRead = true;
				permissions.CanWrite = true;
				return permissions;
			}

			//Check for partner association with top level node
			var topNodeID = permissions.Content.ParentId > 0 ? curriculumServices.GetCurriculumIDForNode( permissions.Content ) : permissions.Content.Id;
			var topPartner = ContentServices.ContentPartner_Get( topNodeID, permissions.User.Id );
			if ( topPartner == null || topPartner.Id == 0 )
			{
				return permissions;
			}

			//Check third-party access privileges
			var privileges = SecurityManager.GetGroupObjectPrivileges( permissions.User, "IOER.controls.Authoring" );
			permissions.CanRead = topPartner.PartnerTypeId > 0;
			permissions.CanWrite = topPartner.PartnerTypeId >= 2 || privileges.WritePrivilege > ( int ) ILPathways.Business.EPrivilegeDepth.Region;
			return permissions;

		}
		//

		//Run boilerplate checks and send a vanilla response to common problems
		private void HandleStandardResponse( ValidationPermissions access, bool requireWriteAccess, ref bool valid, ref string status )
		{
			valid = false;
			if ( access.User == null || access.User.Id == 0 )
			{
				status = "You must be logged in to do that.";
				return;
			}
			if ( access.Content == null || access.Content.Id == 0 )
			{
				status = "Invalid Content Selected.";
				return;
			}
			if ( access.CanRead == false )
			{
				status = "You are not authorized to access that.";
				return;
			}
			if ( requireWriteAccess && access.CanWrite == false )
			{
				status = "You are not authorized to do that.";
				return;
			}

			valid = true;
			status = "okay";
			return;
		}
		//

		//Determine an image to use, ending with most preferable (if more than one URL is present)
		public string DetermineImageUrl( ContentItem item, bool updateIfMissing, bool generateIfMissing, ref bool isBeingGenerated )
		{
			var url = "";
			isBeingGenerated = false;
			
			//Try to load from database
			url = !string.IsNullOrWhiteSpace( item.ResourceThumbnailImageUrl ) ? item.ResourceThumbnailImageUrl : url;
			url = !string.IsNullOrWhiteSpace( item.ResourceImageUrl ) ? item.ResourceImageUrl : url;
			url = !string.IsNullOrWhiteSpace( item.ImageUrl ) ? item.ImageUrl : url;

			if ( !string.IsNullOrWhiteSpace( url ) )
			{
				return url;
			}

			//Check file system and update the content item if necessary
			var thumbnailFolder = UtilityManager.GetAppKeyValue( "serverThumbnailFolder", @"\\OERDATASTORE\OerThumbs\large\" );
			if ( File.Exists( thumbnailFolder + "content-" + item.Id + "-large.png" ) )
			{
				var template = UtilityManager.GetAppKeyValue( "thumbnailTemplate", "//ioer.ilsharedlearning.org/OERThumbs/large/{0}-large.png" );
				url = template.Replace( "{0}", "content-" + item.Id.ToString() );

				if ( updateIfMissing )
				{
					item.ImageUrl = url;
					contentServices.Update( item );
				}

				return url;
			}

			//Create the thumbnail if it isn't already queued up
			if ( generateIfMissing )
			{
				var queued = ThumbnailServices.ExamineChain().FirstOrDefault( m => m.Url == item.DocumentUrl );
				if ( queued == null )
				{
					ThumbnailServices.CreateThumbnail( "content-" + item.Id, item.DocumentUrl, true );
				}

				isBeingGenerated = true;
			}

			return url;
		}
		//

		//Get a list of standards for a given Content Item
		public List<StandardDto> GetStandards( ContentItem content )
		{
			var data = CurriculumServices.ContentStandard_Select( content.Id );
			var output = new List<StandardDto>();

			foreach ( var item in data )
			{
				var code = "Standard";
				try
				{
					code = string.IsNullOrWhiteSpace( item.NotationCode ) ? item.Description.Substring( 0, 20 ) + "..." : item.NotationCode;
				}
				catch { }
				output.Add( new StandardDto()
				{
					StandardId = item.StandardId,
					RecordId = item.StandardRecordId,
					Code = code,
					Text = item.Description,
					UsageTypeId = item.UsageTypeId,
					AlignmentTypeId = item.AlignmentTypeCodeId
				} );
			}

			return output;
		}
		//

		//Validate file
		public void ValidateFile( Stream inputStream, ref bool valid, ref string status )
		{
			//Check size
			var max = UtilityManager.GetAppKeyValue( "maxDocumentSize", 35000000 );
			if ( inputStream.Length > max )
			{
				valid = false;
				status = "File is too large. Maximum allowed size is " + ( max / 1024 ) + " KB.";
				return;
			}

			//Check for viruses
			new VirusScanner().Scan( inputStream, ref valid, ref status );
		}
		//

		#endregion


		#region Helper Classes
		public class AttachmentDto
		{
			public int ContentId { get; set; }
			public int ResourceId { get; set; }
			public string Url { get; set; }
			public int SortOrder { get; set; }
			public int ContentTypeId { get; set; }

			public string Title { get; set; }
			public string Summary { get; set; }
			public string FileName { get; set; }
			public List<string> Keywords { get; set; }
			public int PrivilegeTypeId { get; set; }
			public string PrivilegeTypeTitle { get; set; }
			public UsageRights UsageRights { get; set; }
			public string ImageUrl { get; set; }
			public bool ImageInProgress { get; set; }

			public List<StandardDto> Standards { get; set; }
		}
		//

		public class StandardDto
		{
			public int RecordId { get; set; } //database row int ID
			public int StandardId { get; set; } //ID of the standard in the standards table
			public string Code { get; set; }
			public string Text { get; set; }
			public int UsageTypeId { get; set; }
			public int AlignmentTypeId { get; set; }
		}
		//

		public class ValidationPermissions
		{
			public Patron User { get; set; }
			public ContentItem Content { get; set; }
			public bool CanWrite { get; set; }
			public bool CanRead { get; set; }
		}
		//

		public class AttachmentFile
		{
			public string FileName { get; set; }
			public string MimeType { get; set; }
			public byte[] FileBytes { get; set; }
			public Stream FileStream { get; set; }
		}
		#endregion

	}
}
