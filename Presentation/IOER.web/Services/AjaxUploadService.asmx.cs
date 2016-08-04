using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using System.Web.Script.Serialization;
using Isle.BizServices;

namespace IOER.Services
{
	/// <summary>
	/// Summary description for AjaxUploadService
	/// </summary>
	[WebService( Namespace = "http://tempuri.org/" )]
	[WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
	[System.ComponentModel.ToolboxItem( false )]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[System.Web.Script.Services.ScriptService]
	public class AjaxUploadService : System.Web.Services.WebService
	{
		JavaScriptSerializer serializer = new JavaScriptSerializer();
		UtilityService utils = new UtilityService();
		UploadServices upload = new UploadServices();

		[ WebMethod( EnableSession = true ) ]
		public string UploadFiles()
		{
			var uploads = HttpContext.Current.Request.Files;
			return "";
		}

		[WebMethod( EnableSession = true )]
		public string UploadContentAttachments()
		{
			try
			{
				var context = GetContext();
				var postedFiles = GetPostedFiles();
				var gdFiles = GetGoogleDriveFilesData();
				if ( postedFiles.Count > 0 || gdFiles.Count() > 0 )
				{
					var valid = false;
					var status = "";

					//Workaround - can't do this in the service layer due to circular reference
					var content = new ContentServices().Get( context.contentID );
					var parts = IOER.Controllers.FileResourceController.DetermineDocumentPathUsingParentItem( content ); //This code should probably be moved to somewhere else
					var filePath = IOER.Controllers.FileResourceController.FormatPartsFilePath( parts );
					var savingUrl = IOER.Controllers.FileResourceController.FormatPartsRelativeUrl( parts );
					//End workaround

					var statuses = upload.UploadContentAttachments( context.contentID, postedFiles, gdFiles, utils.GetUserFromSession(), context.gdAccessToken, filePath, savingUrl, ref valid, ref status );
					if ( !valid )
					{
						return Fail( status, null );
					}

					return Success( statuses, "okay", null );
				}
				else
				{
					throw new Exception( "No files selected." );
				}
			}
			catch ( Exception ex )
			{
				return Fail( ex.Message, null );
			}

		}

		[ WebMethod( EnableSession = true ) ]
		public string UploadSingleImage()
		{
			try
			{
				var context = GetContext();
				var postedFiles = GetPostedFiles();
				var gdFiles = GetGoogleDriveFilesData();
				if ( postedFiles.Count > 0 || gdFiles.Count() > 0 )
				{
					//Workaround - can't do this in the service layer due to circular reference
					var content = new ContentServices().Get( context.contentID );
					var parts = IOER.Controllers.FileResourceController.DetermineDocumentPathUsingParentItem( content ); //This code should probably be moved to somewhere else
					var savingFolder = parts.filePath;
					var savingName = "list" + context.contentID + ".png";
					var savingUrl = IOER.Controllers.FileResourceController.FormatPartsFullUrl( parts, savingName );
					//End workaround

					var valid = false;
					var status = "";
					upload.UploadContentImage( context.contentID, postedFiles, gdFiles, utils.GetUserFromSession(), context.gdAccessToken, savingFolder, ref valid, ref status );

					if( !valid )
					{
						return Fail( status, null );
					}

					return Success( savingUrl, "Image updated.", null );
				}
				else
				{
					throw new Exception();
				}
			}
			catch {
				return Fail( "You must select an image.", null );
			}
		}
		//

		#region Helper methods

		protected string Success( object data, string status, object extra )
		{
			return serializer.Serialize( UtilityService.DoReturn( data, true, "okay", extra ) );
		}
		//

		protected string Fail( string status, object extra )
		{
			return serializer.Serialize( UtilityService.DoReturn( null, false, status, extra ) );
		}
		//

		protected UploadContext GetContext()
		{
			try
			{
				var raw = HttpContext.Current.Request.Params[ "Context" ];
				return serializer.Deserialize<UploadContext>( raw );
			}
			catch {
				return new UploadContext();
			}
		}
		//

		protected HttpFileCollection GetPostedFiles()
		{
			try
			{
				return HttpContext.Current.Request.Files;
			}
			catch
			{
				return null;
			}
		}
		//

		protected List<UploadServices.GoogleDriveFileData> GetGoogleDriveFilesData()
		{
			try
			{
				var raw = HttpContext.Current.Request.Params[ "GoogleDriveFilesData" ];
				return serializer.Deserialize<List<UploadServices.GoogleDriveFileData>>( raw );
			}
			catch
			{
				return null;
			}
		}
		//

		#endregion

	}


	public class UploadContext
	{
		public string gdAccessToken { get; set; } //Token acquired for the current user
		public int resourceID { get; set; }
		public int contentID { get; set; }
		public object data { get; set; }
	}

}
