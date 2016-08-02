using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web;
using LRWarehouse.Business;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using ILPathways.Business;
using ILPathways.Utilities;

namespace Isle.BizServices
{
	public class UploadServices
	{
		//Services
		ContentServices contentService = new ContentServices();

		//Globals
		string SITE_ROOT = UtilityManager.GetAppKeyValue( "siteRoot", "http://ioer.ilsharedlearning.org" );
		string SITE_HOST_NAME = UtilityManager.GetAppKeyValue( "siteHostName", "ioer.ilsharedlearning.org" );
		int MAX_FILE_SIZE = UtilityManager.GetAppKeyValue( "maxDocumentSize", 35000000 );


		//Handle uploading a user profile image
		public void UploadContentImage()
		{

		}

		//Handle uploading a library image
		public void UploadLibraryImage()
		{

		}

		//Handle uploading a collection image
		public void UploadCollectionImage()
		{

		}

		/// <summary>
		/// Handle uploading a content item custom image that replaces the thumbnail. 
		/// Will select only the first file from either the posted files or the google drive IDs, depending on which is available (and in that order).
		/// NOTE: currently relies on a workaround in AjaxUploadService to handle things related to IOER.Controllers
		/// </summary>
		/// <param name="contentID"></param>
		/// <param name="postedFiles"></param>
		/// <param name="googleIDs"></param>
		/// <param name="user"></param>
		/// <param name="valid"></param>
		/// <param name="status"></param>
		/// <returns>URL to the newly available image</returns>
		public Image UploadContentImage( int contentID, HttpFileCollection postedFiles, List<GoogleDriveFileData> googleFiles, Patron user, string gdAccessToken, string savingFolder, ref bool valid, ref string status )
		{
			//Verify user
			if ( user == null || user.Id == 0 )
			{
				valid = false;
				status = "You must be logged in to update the image.";
				return null;
			}

			//Verify content
			var content = contentService.Get( contentID );
			if ( content == null || content.Id == 0 )
			{
				valid = false;
				status = "Invalid content ID";
				return null;
			}
			
			//Verify permissions
			if( !CanUserEditContent( content.Id, content.CreatedById, user ) )
			{
				valid = false;
				status = "You do not have permission to make that change.";
				return null;
			}

			//Get the finalized image
			Image image = CreateImageFromSource( contentID, postedFiles, googleFiles, gdAccessToken, ref valid, ref status );
			if ( !valid )
			{
				return null;
			}

			//Do the update and return the image's URL
			WriteContentImage( content, image, savingFolder, ref valid, ref status );
			if ( !valid )
			{
				return null;
			}

			return image;
		}

		//Handle uploading files to a content item - does not handle replacing files
		public List<FileStatus> UploadContentAttachments( int contentID, HttpFileCollection postedFiles, List<GoogleDriveFileData> googleFiles, Patron user, string gdAccessToken, string filePath, string savingUrl, ref bool valid, ref string status )
		{
			var results = new List<FileStatus>();

			//Verify user
			if ( user == null || user.Id == 0 )
			{
				valid = false;
				status = "You must be logged in to upload files.";
				return null;
			}

			//Verify content
			var content = contentService.Get( contentID );
			if ( content == null || content.Id == 0 )
			{
				valid = false;
				status = "Invalid content ID";
				return null;
			}

			//Verify permissions
			if ( !CanUserEditContent( content.Id, content.CreatedById, user ) )
			{
				valid = false;
				status = "You do not have permission to make that change.";
				return null;
			}

			//Validate and save each file, one by one
			for ( var i = 0; i < postedFiles.Count; i++ )
			{
				try
				{
					var file = postedFiles[ i ];

					ValidateFile( file.InputStream, ref valid, ref status );
					if ( !valid )
					{
						results.Add( new FileStatus()
						{
							Successful = false,
							Status = status,
							Name = file.FileName
						} );

						continue;
					}

					var fileStatus = CreateContentWithFile( content, file.InputStream, user, file.FileName, file.ContentType, filePath, savingUrl, ref valid, ref status );
					results.Add( fileStatus );
				}
				catch ( Exception ex )
				{
					results.Add( new FileStatus()
					{
						Status = "Error uploading this file: " + ex.Message,
						Successful = false,
						Name = postedFiles[i].FileName
					} );
				}
			}
			foreach ( var gdFile in googleFiles )
			{
				try
				{
					var file = GetFileFromGoogleDrive( gdFile, gdAccessToken );

					ValidateFile( file.ContentStream, ref valid, ref status );
					if ( !valid )
					{
						results.Add( new FileStatus()
						{
							Successful = false,
							Status = status,
							Name = file.Name
						} );

						continue;
					}

					var fileStatus = CreateContentWithFile( content, file.ContentStream, user, file.Name, file.MimeType, filePath, savingUrl, ref valid, ref status );
					results.Add( fileStatus );
				}
				catch ( Exception ex )
				{
					results.Add( new FileStatus() { 
						Status = "Error retrieving file from Google Drive: " + ex.Message,
						Successful = false,
						Name = gdFile.Name,
					} );
				}
			}

			return results;
		}

		#region Helper Methods

		//Create a new content item from a file - does not handle replacing files
		protected FileStatus CreateContentWithFile( ContentItem parent, Stream fileStream, Patron user, string fileName, string mimeType, string filePath, string savingUrl, ref bool valid, ref string status )
		{
			var fileStatus = new FileStatus();
			try
			{
				//Get bytes from file stream
				var temp = new MemoryStream();
				fileStream.Position = 0;
				fileStream.CopyTo( temp );
				var fileBytes = temp.ToArray();
				temp.Dispose();
				fileStream.Dispose();

				//Build content item
				var attachment = new ContentItem()
				{
					ParentId = parent.Id,
					Title = fileName,
					StatusId = ContentItem.INPROGRESS_STATUS,
					PrivilegeTypeId = ContentItem.PUBLIC_PRIVILEGE,
					TypeId = ContentItem.DOCUMENT_CONTENT_ID,
					MimeType = mimeType,
					CreatedById = user.Id,
					Created = DateTime.Now,
					LastUpdatedById = user.Id,
					SortOrder = 10
				};

				//NOTE - cannot save file the normal way here due to FileResourceController living in IOER namespace - would require circular reference
				//Construct document object
				var doc = new DocumentVersion()
				{
					RowId = Guid.NewGuid(),
					CreatedById = user.Id,
					Created = DateTime.Now,
					FileDate = DateTime.Now,
					LastUpdatedById = user.Id,
					FileName = FileSystemHelper.SanitizeFilename( Path.GetFileNameWithoutExtension( fileName ) + Path.GetExtension( fileName ).ToLower() ),
					MimeType = mimeType,
					FilePath = filePath,
				};
				doc.Title = Path.GetFileNameWithoutExtension( doc.FileName );
				doc.ResourceUrl = savingUrl + "/" + doc.FileName;

				//Update file status object
				fileStatus.Name = doc.FileName;
				fileStatus.Url = doc.ResourceUrl;

				//Save document
				SaveFile( fileBytes, doc.FilePath, doc.FileName, ref valid, ref status );
				if ( !valid )
				{
					fileStatus.Successful = false;
					fileStatus.Status = "Error writing document to disk: " + status;
					return fileStatus;
				}

				//Set resource data
				doc.SetResourceData( fileBytes.LongLength, fileBytes );
				var docID = contentService.DocumentVersionCreate( doc, ref status );
				if ( string.IsNullOrWhiteSpace( docID ) )
				{
					valid = false;
					fileStatus.Successful = false;
					fileStatus.Status = "Error setting resource data: " + status;
					return fileStatus;
				}
				doc.RowId = new Guid( docID );

				//Create content
				attachment.DocumentRowId = doc.RowId;
				attachment.DocumentUrl = doc.ResourceUrl;
				var fileID = contentService.Create( attachment, ref status );
				if ( fileID == 0 )
				{
					valid = false;
					fileStatus.Successful = false;
					fileStatus.Status = "Error updating content item after uploading document: " + status;
					return fileStatus;
				}

				//Success - hopefully covered all the bases
				valid = true;
				status = "okay";
				fileStatus.ContentId = fileID;
				fileStatus.Successful = true;
				fileStatus.Status = "okay";

			}
			catch ( Exception ex )
			{
				valid = false;
				status = ex.Message;
				fileStatus.Successful = false;
				fileStatus.Status = "Error creating resource: " + status;
			}
			return fileStatus;
		}

		protected void SaveFile( byte[] bytes, string folder, string fileName, ref bool valid, ref string status )
		{
			try
			{
				FileSystemHelper.CreateDirectory( folder );
				var diskFile = folder + "\\" + fileName;
				File.WriteAllBytes( diskFile, bytes );
				valid = true;
				status = "okay";
			}
			catch ( Exception ex )
			{
				valid = false;
				status = ex.Message;
				LoggingHelper.LogError( ex, "UploadServices.SaveFile( " + folder + ", " + fileName + ")" );
			}
		}

		//Given a set of posted files and a set of google file data, find the first available file and attempt to process it as an image
		protected Image CreateImageFromSource( int contentID, HttpFileCollection postedFiles, List<GoogleDriveFileData> googleFiles, string gdAccessToken, ref bool valid, ref string status )
		{
			Image image;
			try
			{
				if ( postedFiles.Count > 0 )
				{
					//Select file
					var file = postedFiles[ 0 ];
					
					//Validate type
					if ( !IsValidMimeType( file.ContentType, new List<string>() { "jpg", "jpeg", "png", "gif", "bmp" } ) )
					{
						valid = false;
						status = "You must select an image.";
						return null;
					}
					
					//Validate file
					ValidateFile( file.InputStream, ref valid, ref status );
					if ( !valid )
					{
						valid = false;
						return null;
					}

					//Resize and convert
					image = ProcessImage( file.InputStream, file.ContentType, contentID.ToString(), ImageFormat.Png, 400, 300 );

				}
				else if ( googleFiles.Count() > 0 )
				{
					//Select file
					var file = googleFiles.First();
					
					//Validate type
					if ( !IsValidMimeType( file.MimeType, new List<string>() { "jpg", "jpeg", "png", "gif", "bmp" } ) )
					{
						valid = false;
						status = "You must select an image.";
						return null;
					}
					
					//Get image
					var rawFile = GetFileFromGoogleDrive( googleFiles.First(), gdAccessToken );

					//Validate file
					ValidateFile( rawFile.ContentStream, ref valid, ref status );
					if ( !valid )
					{
						valid = false;
						return null;
					}

					//Resize and convert
					image = ProcessImage( rawFile.ContentStream, rawFile.MimeType, rawFile.Name, ImageFormat.Png, 400, 300 );

				}
				else
				{
					throw new Exception( "You must select an image" );
				}
			}
			catch ( Exception ex )
			{
				valid = false;
				status = ex.Message;
				return null;
			}

			//Return result
			if ( image == null )
			{
				valid = false;
				status = "Error creating image";
				return null;
			}

			valid = true;
			status = "okay";
			return image;
		}
		//

		//Validate file
		public void ValidateFile( Stream inputStream, ref bool valid, ref string status )
		{
			//Check size
			if ( inputStream.Length > MAX_FILE_SIZE )
			{
				valid = false;
				status = "File is too large. Maximum allowed size is " + ( MAX_FILE_SIZE / 1024 ) + " KB.";
				return;
			}

			//Check for viruses
			new VirusScanner().Scan( inputStream, ref valid, ref status );

		}

		//General image processing
		protected Image ProcessImage( Stream inputStream, string contentType, string outputName, ImageFormat outputFormat, int outputWidth, int outputHeight )
		{
			//Convert stream to bitmap to handle resizing
			var newImage = new Bitmap( Image.FromStream( inputStream ), new Size() { Width = outputWidth, Height = outputHeight } );
			//Convert bitmap to new format to handle switching output formats
			var newStream = new MemoryStream();
			newImage.Save( newStream, outputFormat );
			//Return the finalized image
			var final = Image.FromStream( newStream );

			return final;
		}
		//

		//Get file from google
		protected GoogleDriveFileData GetFileFromGoogleDrive( GoogleDriveFileData data, string accessToken )
		{
			var getter = new HttpClient();
			getter.DefaultRequestHeaders.Add( "User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36" );
			getter.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue( "Bearer", accessToken );
			var result = getter.GetAsync( data.Url ).Result.Content;
			data.ContentString = result.ReadAsStringAsync().Result;
			data.ContentBytes = result.ReadAsByteArrayAsync().Result;
			data.ContentStream = result.ReadAsStreamAsync().Result;

			return data;
		}
		//

		//Check to see if the user is able to edit content
		protected bool CanUserEditContent( int contentID, int contentCreatedById, Patron user )
		{
			var partner = ContentServices.ContentPartner_Get( contentID, user.Id );
			var privileges = SecurityManager.GetGroupObjectPrivileges( user, "IOER.controls.Authoring" );
			return
				user.TopAuthorization < ( int ) EUserRole.StateAdministrator || //If user is admin...
				contentCreatedById == user.Id || //Or if user is the creator of the content...
				privileges.WritePrivilege > ( int ) EPrivilegeDepth.Region || //Or if the user has sufficient write privileges..
				( partner != null && partner.PartnerTypeId >= 2 ); //Or if the user is a partner with sufficient access...
		}
		//
		
		//Check to see if the mime type contains a value from a list of strings
		protected bool IsValidMimeType( string mimeType, List<string> values )
		{
			var valid = false;
			foreach ( var item in values )
			{
				if ( mimeType.IndexOf( item ) > -1 )
				{
					valid = true;
				}
			}
			return valid;
		}
		//

		//Create thumbnail images using the data from the uploaded image
		protected void CreateContentThumbnailFromImageBytes( ContentItem content, byte[] imageBytes )
		{
			//If relevant, create thumbnail
			if ( UtilityManager.GetAppKeyValue( "creatingThumbnails" ) == "yes" )
			{
				//Write placeholder to thumbnail folder
				var thumbnailFolder = ILPathways.Utilities.UtilityManager.GetAppKeyValue( "serverThumbnailFolder", @"\\OERDATASTORE\OerThumbs\large\" );
				File.WriteAllBytes( thumbnailFolder + content.RowId + ".png", imageBytes );

				//Replace existing thumbnail if applicable
				if ( content.ResourceIntId > 0 )
				{
					File.WriteAllBytes( thumbnailFolder + content.ResourceIntId + "-large.png", imageBytes );
				}
			}
		}
		//

		//Store an uploaded content image
		protected void WriteContentImage( ContentItem content, Image image, string savingFolder, ref bool valid, ref string status )
		{
			try
			{
				var savingName = "list" + content.Id.ToString() + ".png";
				var store = new ImageStore()
				{
					FileName = savingName,
					FileDate = DateTime.Now
				};

				//Convert image to byte[] and store
				var imageStream = new MemoryStream();
				image.Save( imageStream, ImageFormat.Png );
				var imageBytes = imageStream.ToArray();
				store.SetImageData( imageBytes.LongLength, imageBytes );

				FileSystemHelper.HandleDocumentCaching( savingFolder, store, true );
				contentService.Update( content );

				//Create Thumbnails
				CreateContentThumbnailFromImageBytes( content, imageBytes );
			}
			catch ( Exception ex )
			{
				valid = false;
				status = ex.Message;
			}

			valid = true;
			status = "okay";
		}

		#endregion
		#region Helper Classes

		public class GoogleDriveFileData
		{
			public string Id { get; set; }
			public string Name { get; set; }
			public string MimeType { get; set; }
			public string Url { get; set; }
			public string ContentString { get; set; }
			public byte[] ContentBytes { get; set; }
			public Stream ContentStream { get; set; }
		}

		public class FileStatus
		{
			public int ContentId { get; set; }
			public string Name { get; set; }
			public string Url { get; set; }
			public string Status { get; set; }
			public bool Successful { get; set; }
		}

		#endregion

	}
}
