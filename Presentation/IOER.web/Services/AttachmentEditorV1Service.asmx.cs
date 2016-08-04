using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using Isle.BizServices;
using System.Web.Script.Serialization;
using ResponseJson = IOER.Services.UtilityService.GenericReturn;

namespace IOER.Services
{
	/// <summary>
	/// Summary description for AttachmentEditorV1Service
	/// </summary>
	[WebService( Namespace = "http://tempuri.org/" )]
	[WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
	[System.ComponentModel.ToolboxItem( false )]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[System.Web.Script.Services.ScriptService]
	public class AttachmentEditorV1Service : System.Web.Services.WebService
	{
		UtilityService utilServices = new UtilityService();
		ContentAttachmentServices attachmentServices = new ContentAttachmentServices();
		JavaScriptSerializer serializer = new JavaScriptSerializer();
		bool valid = true;
		string status = "";

		//Get a list of attachments for a given node (as JSON response)
		[WebMethod( EnableSession = true )]
		public ResponseJson GetAttachments( int nodeID )
		{
			var list = attachmentServices.GetAttachments( nodeID, ref valid, ref status );
			if ( !valid )
			{
				return Respond( null, false, status, null );
			}

			return Respond( list, true, "okay", null );
		}
		//

		[WebMethod(EnableSession=true)]
		public ResponseJson SetSortOrder( int nodeID, int attachmentID, int targetOrder )
		{
			var list = attachmentServices.SetSortOrder( nodeID, attachmentID, targetOrder, ref valid, ref status );
			if ( !valid )
			{
				return Respond( null, false, status, null );
			}

			return Respond( list, true, "okay", null );
		}
		//

		[WebMethod(EnableSession=true)]
		public ResponseJson DeleteAttachment( int nodeID, int attachmentID )
		{
			attachmentServices.DeleteAttachment( nodeID, attachmentID, ref valid, ref status );
			if ( !valid )
			{
				return Respond( null, false, status, null );
			}

			return Respond( attachmentID, true, "okay", null );
		}
		//

		[WebMethod(EnableSession=true)]
		public ResponseJson UpdateAttachment( int nodeID, ContentAttachmentServices.AttachmentDto attachment )
		{
			//Validate inputs
			//Title
			attachment.Title = utilServices.ValidateText( attachment.Title.Trim(), 3, "Title", ref valid, ref status );
			if ( !valid )
			{
				return Respond( null, false, status, null );
			}

			//Summary
			if ( attachment.Summary.Trim().Length > 0 )
			{
				attachment.Summary = utilServices.ValidateText( attachment.Summary.Trim(), 15, "Summary", ref valid, ref status );
				if ( !valid )
				{
					return Respond( null, false, status, null );
				}
			}
			
			//Usage Rights URL
			if( attachment.UsageRights.CodeId == 4 ) //If custom usage rights
			{
				attachment.UsageRights.Url = utilServices.ValidateURL( attachment.UsageRights.Url.Trim(), false );
				if ( !valid )
				{
					return Respond( null, false, status, null );
				}
			}
			
			//Keywords
			foreach ( var word in attachment.Keywords )
			{
				utilServices.ValidateText( word, 2, "Keyword: " + word, ref valid, ref status );
				if ( !valid )
				{
					return Respond( null, false, status, null );
				}
			}

			//Do the update
			var updatedAttachment = attachmentServices.UpdateAttachment( nodeID, attachment, ref valid, ref status );
			if ( !valid )
			{
				return Respond( null, false, status, null );
			}

			return Respond( updatedAttachment, true, "okay", null );
		}
		//

		[WebMethod(EnableSession=true)]
		public ResponseJson RemoveAttachmentStandard( int nodeID, int contentID, int recordID )
		{
			//Do the update
			attachmentServices.RemoveAttachmentStandard( nodeID, contentID, recordID, ref valid, ref status );
			if ( !valid )
			{
				return Respond( null, false, status, null );
			}

			return Respond( new { contentID = contentID, recordID = recordID }, true, "okay", null );
		}
		//

		[WebMethod(EnableSession=true)]
		public ResponseJson AddUrl( int nodeID, string url )
		{
			//Validate URL
			url = utilServices.ValidateURL( url, false, ref valid, ref status );
			if ( !valid )
			{
				return Respond( null, false, status, null );
			}

			//Add the URL
			var newAttachment = attachmentServices.AddUrl( nodeID, url, ref valid, ref status );
			return Respond( newAttachment, valid, status, url );
		}
		//

		[WebMethod(EnableSession=true)]
		public string AddDeviceFile() //Have to use string because ASP.NET doesn't seem to allow returning JSON to a request that was sent as multipart/form-data
		{
			try
			{
				//Get data
				var nodeID = int.Parse( HttpContext.Current.Request.Form[ "NodeId" ] );
				var file = HttpContext.Current.Request.Files[0];

				//Workaround - can't do this in the service layer due to circular reference
				var node = new ContentServices().Get( nodeID );
				var parts = IOER.Controllers.FileResourceController.DetermineDocumentPathUsingParentItem( node ); //This code should probably be moved to somewhere else
				var filePath = IOER.Controllers.FileResourceController.FormatPartsFilePath( parts );
				var savingUrl = IOER.Controllers.FileResourceController.FormatPartsRelativeUrl( parts );
				//End workaround

				//Do the upload
				var data = attachmentServices.UploadDeviceFile( nodeID, file, filePath, savingUrl, ref valid, ref status );

				//Return the data
				return serializer.Serialize( Respond( data, valid, status, null ) );

			}
			catch ( Exception ex )
			{
				return serializer.Serialize( Respond( null, false, ex.Message, null ) );
			}

		}
		//

		[WebMethod(EnableSession=true)]
		public ResponseJson AddGoogleDriveFile( int nodeID, string name, string mimeType, string url, string token )
		{
			try
			{
				//Get data

				//Workaround - can't do this in the service layer due to circular reference
				var node = new ContentServices().Get( nodeID );
				var parts = IOER.Controllers.FileResourceController.DetermineDocumentPathUsingParentItem( node ); //This code should probably be moved to somewhere else
				var filePath = IOER.Controllers.FileResourceController.FormatPartsFilePath( parts );
				var savingUrl = IOER.Controllers.FileResourceController.FormatPartsRelativeUrl( parts );
				//End workaround

				//Do the upload
				var data = attachmentServices.UploadGoogleDriveFile( nodeID, name, mimeType, url, token, filePath, savingUrl, ref valid, ref status );

				//Return the data
				return Respond( data, valid, status, null );

			}
			catch ( Exception e )
			{
				return Respond( null, false, e.Message, null );
			}
		}
		//

		[WebMethod(EnableSession=true)]
		public ResponseJson GetAttachmentImage( int nodeID, int contentID )
		{
			var isBeingGenerated = false;
			var data = attachmentServices.GetAttachmentImage( nodeID, contentID, true, ref isBeingGenerated, ref valid, ref status );
			if ( !valid )
			{
				return Respond( null, false, status, null );
			}

			return Respond( data, true, "okay", isBeingGenerated );
		}

		#region Helper Methods
		//Shortcut for standard JSON response
		private ResponseJson Respond( object data, bool valid, string status, object extra )
		{
			return new ResponseJson()
			{
				data = data,
				valid = valid,
				status = status,
				extra = extra
			};
		}
		//

		#endregion


	}
}
