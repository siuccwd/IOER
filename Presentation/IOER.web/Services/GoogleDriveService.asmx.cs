using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using System.Web.Script.Serialization;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v2;
using Google.Apis.Download;

using Isle.BizServices;
using LRWarehouse.Business;
using ILPathways.Business;
using ILPathways.Utilities;


namespace IOER.Services
{
	/// <summary>
	/// Summary description for GoogleDriveService
	/// </summary>
	[WebService( Namespace = "http://tempuri.org/" )]
	[WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
	[System.ComponentModel.ToolboxItem( false )]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[System.Web.Script.Services.ScriptService]
	public class GoogleDriveService : System.Web.Services.WebService
	{
		JavaScriptSerializer serializer = new JavaScriptSerializer();
		UtilityService util = new UtilityService();

		[WebMethod(EnableSession=true)]
		public string ImportFile( int resourceID, int contentID, string fileName, string mimeType, string url, string accessToken )
		{
			var valid = true;
			var status = "";

			try
			{

				//Validate the user
				var user = AccountServices.GetUserFromSession( Session );
				if ( user == null || user.Id == 0 )
				{
					return util.ImmediateReturn( null, false, "You must be logged in.", null );
				}

				//Download the file from google drive
				var bytes = DoFileDownload( url, accessToken, ref valid, ref status );
				if ( !valid )
				{
					return util.ImmediateReturn( null, false, status, new { fileBytesLength = bytes.Length, downloadURL = url, accessToken = accessToken } );
				}

				//Construct the file name with extension
				if ( url.IndexOf( "&exportFormat=" ) > -1 )
				{
					var extension = "txt";
					try
					{
						extension = url.ToLower().Split( new string[] { "&exportformat=" }, StringSplitOptions.RemoveEmptyEntries )[ 1 ];
					}
					catch { }
					if ( fileName.IndexOf( extension ) == -1 )
					{
						fileName = fileName + "." + extension;
					}
				}

				//Save as content item
				var content = new ResourceService().CreateContentDocument( bytes, fileName, mimeType, user, resourceID, contentID, ref valid, ref status );

				//Return result
				var info = new ResourceService.UploadInfo()
				{
					resourceID = content.ResourceIntId,
					contentID = content.Id,
					contentURL = "http://" + HttpContext.Current.Request.Url.Host + content.DocumentUrl,
					fileName = fileName,
					extraData = "drive",
					valid = true,
					status = "okay",
					command = "upload"
				};

				if ( valid )
				{
					return util.ImmediateReturn( info, true, "okay", null );
				}
				else
				{
					info.valid = valid;
					info.status = status;
					return util.ImmediateReturn( null, false, status, new { info = info, fileBytesLength = bytes.Length, downloadURL = url, accessToken = accessToken } );
				}

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( "Error getting file from Google: ResourceID: " + resourceID + ", ContentID: " + contentID + ", MimeType: " + mimeType + ", URL: " + url + ", AccessToken: " + accessToken + ", File Name: " + fileName + " | Exception: " + System.Environment.NewLine + ex.Message + System.Environment.NewLine + ex.ToString() );
				return util.ImmediateReturn( null, false, "There was an error retrieving the file from Google Drive.", new { error = ex.Message, exception = ex.ToString() } );
			}
		}

		public byte[] DoFileDownload( string url, string accessToken, ref bool valid, ref string status )
		{
			// Get the access token and file into C# objects
			string[] scopes = new string[] { DriveService.Scope.DriveReadonly };
			ClientSecrets secrets = new ClientSecrets
			{
				ClientId = UtilityManager.GetAppKeyValue( "googleClientId", "" ),
				ClientSecret = UtilityManager.GetAppKeyValue( "googleClientSecret", "" )
			};

			//Setup the google drive service
			var token = new TokenResponse { RefreshToken = accessToken };
			var credential = new UserCredential( new GoogleAuthorizationCodeFlow( new GoogleAuthorizationCodeFlow.Initializer
			{
				ClientSecrets = secrets
			} ), "user", token );
			var service = new DriveService( new Google.Apis.Services.BaseClientService.Initializer() { HttpClientInitializer = credential, ApplicationName = "IOER" } );

			//Download the file
			var bytes = new byte[ 0 ];
			try
			{
				bytes = service.HttpClient.GetByteArrayAsync( url ).Result;
				if ( bytes.Length == 0 )
				{
					throw new Exception( "The file import process resulted in an empty file" );
				}
			}
			catch ( Exception ex )
			{
				valid = false;
				status = ex.Message;
			}

			return bytes;

		}

	}
}
