using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Script.Serialization;
using LRWarehouse.Business;
using ILPathways.Utilities;
using ILPathways.Business;
using IOER.Library;
using IOER.Controllers;
using Isle.BizServices;

namespace IOER.Controls.UberTaggerV2
{
  public partial class upload : BaseAppPage
  {
    JavaScriptSerializer serializer = new JavaScriptSerializer();

    public string LoadMessage { get; set; }

	protected void Page_Load( object sender, EventArgs e )
	{
		//If something happened
		if ( IsPostBack )
		{
			//Get data
			var info = serializer.Deserialize<UploadInfo>( hdnData.Value );

			//If we need to remove a file...
			if ( info.command == "remove" )
			{
				//Make sure the content exists
				var content = new ContentServices().Get( info.contentID );
				if ( content.Id == 0 )
				{
					LoadMessage = GetLoadMessage( false, info.command, "Invalid File ID.", "", 0, "Content ID is 0" );
					return;
				}
				//Make sure the content wasn't published
				if ( info.resourceID != 0 )
				{
					LoadMessage = GetLoadMessage( false, info.command, "You can't delete a file that has already been published.", "", info.contentID, "Resource ID is " + info.resourceID );
					return;
				}
				//Make sure the user is logged in
				if ( !IsUserAuthenticated() )
				{
					LoadMessage = GetLoadMessage( false, info.command, "You must be logged in to delete.", "", info.contentID, "User is not logged in" );
					return;
				}
				var user = ( Patron ) WebUser;
				//Make sure the user created the file
				if ( content.CreatedById != user.Id )
				{
					LoadMessage = GetLoadMessage( false, info.command, "You cannot delete a file you didn't create.", "", info.contentID, "" );
					return;
				}

				//Do the delete
				var valid = true;
				var status = "";
				valid = new ContentServices().Delete( info.contentID, ref status );
				if ( !valid )
				{
					LoadMessage = GetLoadMessage( false, info.command, status, "", info.contentID, "" );
					return;
				}

				//Return to client
				LoadMessage = GetLoadMessage( true, info.command, "File Removed.", "", info.contentID, "" );
			}
			//If we need to upload/replace a file...
			else if ( info.command == "upload" )
			{
				//Handle file upload
				if ( !fileUpload.HasFile )
				{
					LoadMessage = GetLoadMessage( false, info.command, "No file selected.", "", info.contentID, "No file detected" );
					return;
				}

				//Make sure the user is logged in
				if ( !IsUserAuthenticated() )
				{
					LoadMessage = GetLoadMessage( false, info.command, "You must be logged in to upload.", "", info.contentID, "User is not logged in" );
					return;
				}
				var user = ( Patron ) WebUser;

				//Do the upload
				var valid = true;
				var status = "";
				var fileURL = UploadNewFile( fileUpload, user, info, ref valid, ref status );

				//Send to client
				LoadMessage = GetLoadMessage( true, info.command, "Upload Successful", fileURL, info.contentID, "" );
			}
			//Should not occur
			else
			{
				LoadMessage = GetLoadMessage( false, info.command, "There was an error processing the upload.", "", info.contentID, "Unknown error" );
			}
		}
		//Nothing happened, just a page load
		else
		{
			LoadMessage = GetLoadMessage( true, "load", "Ready", "", 0, "" );
		}
	}

	private string GetLoadMessage( bool valid, string command, string status, string url, int contentID, object extra )
	{
		return serializer.Serialize( new { type = "uploadMessage", command, valid = valid, status = status, url = url, contentID = contentID, extra = extra } );
	}

	private string UploadNewFile( FileUpload uploader, Patron user, UploadInfo info, ref bool valid, ref string status )
	{
		var fileURL = "";

		//Check file size
		var maxFileSize = UtilityManager.GetAppKeyValue( "maxDocumentSize", 20000000 );
		if ( uploader.FileBytes.Length > maxFileSize )
		{
			valid = false;
			status = "Uploaded file is too large. Maximum file size is " + Math.Floor( ( maxFileSize / 1024f ) / 1024f ) + " megabytes.";
			return "";
		}

		//Scan the file for viruses
		ScanFile( uploader, maxFileSize, ref valid, ref status );
		if ( !valid )
		{
			//Status is already set by the scan method
			return "";
		}

		//Continue - borrow from QC tool's CreateDocumentItem method as a basis
		fileURL = CreateDocumentItem( uploader, user, info, ref valid, ref status );

		return fileURL;
	}

	private void ScanFile( FileUpload uploader, int maxDocumentSize, ref bool isClean, ref string status )
	{
		new VirusScanner( maxDocumentSize ).Scan( uploader.FileBytes, ref isClean, ref status );
	}

	private string CreateDocumentItem( FileUpload uploader, Patron user, UploadInfo info, ref bool valid, ref string status )
		{
			var url = "";
			var content = new ContentItem()
			{
				Title = "Uploaded File",
				TypeId = ContentItem.DOCUMENT_CONTENT_ID,
				Summary = "File uploaded by " + user.FullName() + " (" + user.Id + ") on " + DateTime.Now.ToLongDateString(),
				CreatedById = user.Id,
				OrgId = user.OrgId,
				PrivilegeTypeId = ContentItem.PUBLIC_PRIVILEGE,
				StatusId = ContentItem.SUBMITTED_STATUS //Set to 3, just in case of a failure before publish,then set after actual publish
			};

			if ( info.resourceID > 0 )
			{
				//Ensure the user can associate content with this resource
				if ( info.resourceID > 0 && ResourceBizService.CanUserEditResource( info.resourceID, user.Id ) )
				{
					content.ResourceIntId = info.resourceID;
				}
				else
				{
					valid = false;
					status = "You don't have access to update that resource's file.";
					return "";
				}
			}

			if ( info.contentID > 0 )
			{
				//Ensure the user can update the file for this resource
				var existingContent = new ContentServices().Get( info.contentID );
				//if ( existingContent.CreatedById != user.Id ) //May need a more sophisticated check?

				if ( existingContent.CreatedById != user.Id 
					&&  ContentServices.DoesUserHaveContentEditAccess(info.contentID, user.Id) == false)
				{
					valid = false;
					status = "You don't have access to replace that item.";
					return "";
				}
				else
				{
					content.Id = info.contentID;
				}
			}

			//Attempt to get content by resource ID - covering all the bases if possible
			if ( info.resourceID > 0 && info.contentID == 0 )
			{
				var canView = true;
				//NOTE - may not want to use this, could be multiple content items (but not yet possible for files - yet). Will return first
				var existingContent = new ContentServices().GetForResourceDetail( info.resourceID, user, ref canView );
				
				if ( existingContent != null && existingContent.Id > 0 )
				{
					info.contentID = existingContent.Id;
				}
			}

			//Upsert the file
			if ( info.contentID > 0 )
			{
				//Replace existing file
				var update = new FileResourceController().ReplaceDocument( uploader, info.contentID, user.Id, ref status );
				if ( update )
				{
					valid = true;
				}
				else
				{
					valid = false;
					return "";
				}
			}
			else
			{
				//Create the file
				var newID = FileResourceController.CreateContentItemWithFileOnly( uploader, new ContentItem(), content, ref status );
				if ( newID > 0 )
				{
					info.contentID = newID;
					valid = true;
				}
				else
				{
					valid = false;
					status = "There was a problem uploading the file. Please try again later.";
					return "";
				}
			}

			//Set URL
			url = "/content/" + info.contentID;

			return url;
		}

    public class UploadInfo
    {
      public string command { get; set; }
      public int resourceID { get; set; }
      public int contentID { get; set; }
    }
  }
}