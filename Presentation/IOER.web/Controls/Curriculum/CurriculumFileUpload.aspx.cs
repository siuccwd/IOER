using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Script.Serialization;
using ILPathways.Services;
using ILPathways.Business;
using LRWarehouse.Business;
using Isle.BizServices;
using ILPathways.Controllers;
using ILPathways.Library;
using System.IO;
using System.Drawing;
using ILPathways.Utilities;

namespace ILPathways.Controls.Curriculum
{
  public partial class CurriculumFileUpload : System.Web.UI.Page
  {
    public string dataJSON { get; set; }

    protected void Page_Load( object sender, EventArgs e )
    {
      dataJSON = "null";
      if ( IsPostBack )
      {
        if ( fileUpload.HasFile )
        {
          var serializer = new JavaScriptSerializer();

          //Validate user
          var user = ( Patron ) Session[ "user" ];
          if ( user == null || user.Id == 0 )
          {
            dataJSON = serializer.Serialize( UtilityService.DoReturn( "", false, "You must login to do that.", null ) );
            return;
          }

          //Handle file upload
          var file = fileUpload.PostedFile;
          var usage = FormHelper.GetRequestKeyValue( "usage" ).ToLower();
          bool valid = true;
          string status = "";

          //Verify user is logged in
          if ( Session[ "user" ] == null )
          {
            dataJSON = serializer.Serialize( UtilityService.DoReturn( usage, false, "You must login to do that.", usage ) );
            return;
          }

          switch ( usage )
          {
            case "curriculumimage":
              {
                  HandleImageUpload( usage, user );
                
                break;
              }
            case "attachment":
              {
                  HandleAttachment( usage, user );
                
                break;
              }
            default:
              break;
          }
        }
      }
    }
    private void HandleImageUpload( string usage, Patron user )
    {
        var serializer = new JavaScriptSerializer();
        bool valid = true;
        string status = "";

        //Uploaded a curriculum image
        var metadata = serializer.Deserialize<CurriculumImageInput>( hdnMetadata.Value );

        //Validate permissions
        var contentService = new ContentServices();
        var node = contentService.Get( metadata.nodeID );

        if ( node == null || node.Id == 0 )
        {
            dataJSON = serializer.Serialize( UtilityService.DoReturn( usage, false, "Error: invalid node ID.", usage ) );
            return;
        }

        var curriculumWebService = new CurriculumService1();
        var nodePermissions = curriculumWebService.GetValidatedUser( metadata.nodeID, node.CreatedById );
        if ( !nodePermissions.write )
        {
            dataJSON = serializer.Serialize( UtilityService.DoReturn( usage, false, "You don't have permission to alter this node.", usage ) );
            return;
        }

        //Validate image
        var extension = Path.GetExtension( fileUpload.FileName.ToLower() );
        if ( string.IsNullOrWhiteSpace( extension ) || (extension != ".jpg" && extension != ".gif" && extension != ".png" ) )
        {
            dataJSON = serializer.Serialize( UtilityService.DoReturn( usage, false, "File must be jpg, gif, or png.", usage ) );
            return;
        }
        var savingName = node.RowId.ToString() + extension;
        FileResourceController.PathParts parts = FileResourceController.DetermineDocumentPathUsingParentItem( node );
        var savingFolder = parts.filePath;

        //var savingFolder = FileResourceController.DetermineDocumentPath( node, user.Id );
//        var savingURL = FileResourceController.DetermineDocumentUrl( node, savingName );
        var savingURL = FileResourceController.FormatPartsFullUrl( parts, savingName );
        var image = new ImageStore() 
        { 
            FileName = savingName, 
            FileDate = DateTime.Now 
        };

        //Resize image - may want to add fanciness like centered cropping later; creating an image that exactly matches thumbnail size (400x300) is #1 priority
        var targetSize = new Size( 400, 300 ); //TODO: make this a web.config thing
        var imageData = System.Drawing.Image.FromStream( fileUpload.PostedFile.InputStream );
        var stream = new MemoryStream();
        var newImage = ( System.Drawing.Image ) new Bitmap( imageData, targetSize );
        newImage.Save( stream, System.Drawing.Imaging.ImageFormat.Png );
        var bytes = stream.ToArray();
        image.SetImageData( bytes.LongLength, bytes );

        //Save image
        ILPathways.Utilities.FileSystemHelper.HandleDocumentCaching( savingFolder, image, true );

        node.ImageUrl = savingURL;
        contentService.Update( node );

        if ( UtilityManager.GetAppKeyValue( "envType" ) != "dev" )
        {
            //Write placeholder to thumbnail folder
            var thumbnailFolder = ILPathways.Utilities.UtilityManager.GetAppKeyValue( "serverThumbnailFolder", @"\\OERDATASTORE\OerThumbs\large\" );
            File.WriteAllBytes( thumbnailFolder + node.RowId + ".png", bytes );

            //Replace existing thumbnail if applicable
            if ( node.ResourceIntId > 0 )
            {
            File.WriteAllBytes( thumbnailFolder + node.ResourceIntId + "-large.png", bytes );
            }
        }

        //Return data
        dataJSON = serializer.Serialize( UtilityService.DoReturn( savingURL, valid, status, usage ) );
    }

    private void HandleAttachment( string usage, Patron user )
    {
        var serializer = new JavaScriptSerializer();
        bool valid = true;
        string status = "";

        //Uploaded an attachment document
        var metadata = serializer.Deserialize<AttachmentInput>( hdnMetadata.Value );

        //Validate permissions
        var contentService = new ContentServices();
        var node = contentService.Get( metadata.nodeID );
        var item = contentService.Get( metadata.attachmentID );

        if ( node == null || node.Id == 0 )
        {
            dataJSON = serializer.Serialize( UtilityService.DoReturn( usage, false, "Error: invalid node ID.", usage ) );
            return;
        }

        var curriculumWebService = new CurriculumService1();
        var nodePermissions = curriculumWebService.GetValidatedUser( metadata.nodeID, node.CreatedById );
        if ( !nodePermissions.write )
        {
            dataJSON = serializer.Serialize( UtilityService.DoReturn( usage, false, "You don't have permission to alter this node.", usage ) );
            return;
        }

        //Apply changes regardless of new/update
        item.Title = metadata.title;
        item.PrivilegeTypeId = metadata.accessID;
        item.StatusId = ContentItem.PUBLISHED_STATUS; //Published
        item.SortOrder = metadata.featured ? -1 : 10;
        item.LastUpdatedById = user.Id;

        //Save to database
        if ( metadata.attachmentID == 0 )
        {
            //Creating a new item
            item.ParentId = metadata.nodeID;
            item.TypeId = 40; //Document
            item.CreatedById = user.Id;

            string folder = node.RowId.ToString();

            //New file, new ContentItem
            var newID = FileResourceController.CreateContentItemWithFileOnly( fileUpload, node, item, ref status );
            if ( newID == 0 )
            {
                dataJSON = serializer.Serialize( UtilityService.DoReturn( usage, false, "There was an error creating the file.", usage ) );
                return;
            }

            //Create thumbnail
            var content = new ContentServices().Get( newID );
            var fixedURL = content.DocumentUrl.IndexOf( "ilsharedlearning" ) > -1 ? content.DocumentUrl : ( "http://ioer.ilsharedlearning.org" + ( content.DocumentUrl.IndexOf( "/" ) == 0 ? content.DocumentUrl : "/" + content.DocumentUrl ) );
            new LRWarehouse.DAL.ResourceThumbnailManager().CreateThumbnailAsync( "content-" + newID, fixedURL, true, 4 );

        }
        else
        {
            //Validate permissions
            /*var attachmentPermissions = curriculumWebService.GetValidatedUser( item.Id, item.CreatedById );
            if ( !attachmentPermissions.write )
            {
              dataJSON = serializer.Serialize( UtilityService.DoReturn( usage, false, "You don't have permission to alter that attachment.", usage ) );
              return;
            }*/
            //Only need to check node permissions

            //Updating existing file
            valid = new FileResourceController().ReplaceDocument( fileUpload, metadata.attachmentID, user.Id, ref status );
            if ( !valid )
            {
                dataJSON = serializer.Serialize( UtilityService.DoReturn( usage, false, "There was an error replacing the file.", usage ) );
                return;
            }

            //Update existing contentItem
            status = contentService.Update( item );
            if ( status != "successful" )
            {
                dataJSON = serializer.Serialize( UtilityService.DoReturn( usage, false, "The file was replaced, but there was an error updating the other information.", usage ) );
                return;
            }

            //Recreate thumbnail
            var content = new ContentServices().Get( item.Id );
            var fixedURL = content.DocumentUrl.IndexOf( "ilsharedlearning" ) > -1 ? content.DocumentUrl : ( "http://ioer.ilsharedlearning.org" + ( content.DocumentUrl.IndexOf( "/" ) == 0 ? content.DocumentUrl : "/" + content.DocumentUrl ) );
            new LRWarehouse.DAL.ResourceThumbnailManager().CreateThumbnailAsync( "content-" + content.Id, fixedURL, true, 4 );
        }

        //Return data
        var newAttachments = new CurriculumService1().GetAttachments( metadata.nodeID ).data;
        dataJSON = serializer.Serialize( UtilityService.DoReturn( newAttachments, valid, status, usage ) );
    }      
    
    public class AttachmentInput : CurriculumService1.AttachmentDTO {
      public int nodeID { get; set; }
    }

    public class CurriculumImageInput
    {
      public int nodeID { get; set; }

    }
  }
}