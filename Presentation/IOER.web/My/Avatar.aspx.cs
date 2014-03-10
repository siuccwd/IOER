using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using ILPathways.Services;
using ILPathways.Utilities;
using ILPathways.Controllers;
using ILPathways.Business;
using System.IO;
using Isle.BizServices;
using AccountManager = Isle.BizServices.AccountServices;

namespace ILPathways.My
{
    public partial class Avatar : System.Web.UI.Page
    {
      LibraryBizService libService = new LibraryBizService();
      int maxFileSize = UtilityManager.GetAppKeyValue( "maxLibraryImageSize", 100000 );
      int libraryImageWidth = UtilityManager.GetAppKeyValue( "libraryImageWidth", 150 );

      protected void Page_Load( object sender, EventArgs e )
      {

      }

      public void SubmitAvatar_Click( object sender, EventArgs e )
      {
        try
        {
          var userGUID = Request.Params[ "guid" ];
          var user = new AccountManager().GetByRowId( userGUID );
          var library = libService.Get( int.Parse( Request.Params[ "lib" ] ) );
          if ( !libService.Library_DoesUserHaveEditAccess( library.Id, user.Id ) )
          {
            return;
          }

          //Get the collection if available, and ensure it belongs to the user
          var collection = new ILPathways.Business.LibrarySection();
          try
          {
              collection.IsValid = false;
              if ( collectionID.Text != null )
              {
                  int collId = 0;
                  if ( Int32.TryParse( collectionID.Text, out collId ) )
                  {
                      collection = libService.LibrarySectionGet( collId );
                  }
              }
          }
          catch { }
          if ( collection.IsValid )
          {
              //need to allow this for curators, etc
              //options: 
              //- if interface allows, it could assume OK here
              //- otherwise requires security check
            //if ( collection.CreatedById != user.Id ) { return; }
          }

          //Validation
          if ( !user.IsValid ) { return; }
          UpdateAvatar( fileUpload, library, collection );
          /*
          if ( !fileUpload.HasFile ) { return; }
          if ( !FileResourceController.IsFileSizeValid( fileUpload, maxFileSize ) )
          {
              ltlOutput.Text = string.Format( "Error: File must be {0}KB or less.", ( maxFileSize / 1024 ) );
              return;
          }
          if ( fileUpload.PostedFile.ContentType.IndexOf( "image/" ) != 0 )
          {
              ltlOutput.Text = "Error: You must select an image file.";
              return;
          }

          var targetGUID = collection.IsValid ? collection.RowId.ToString() : library.RowId.ToString();
          string savingName = targetGUID.ToString().Replace( "-", "" ) + System.IO.Path.GetExtension( fileUpload.FileName );
          string savingFolder = FileResourceController.DetermineDocumentPath( library );
          string savingURL = FileResourceController.DetermineDocumentUrl( library, savingName );
          if ( collection.IsValid )
          {
            collection.ImageUrl = savingURL;
          }
          else
          {
            library.ImageUrl = savingURL;
          }

          ImageStore img = new ImageStore();
          img.FileName = savingName;
          img.FileDate = DateTime.Now;

          FileResourceController.HandleImageResizingToWidth( img, fileUpload, libraryImageWidth, libraryImageWidth, true, true );
          FileSystemHelper.HandleDocumentCaching( savingFolder, img, true );


          if ( collection.IsValid )
          {
            libService.LibrarySectionUpdate( collection );
          }
          else
          {
            libService.Update( library );
          }*/

        }
        catch
        {
            ltlOutput.Text = "There was an error uploading your image. Please try again later.";
        }
      }

      public void UpdateAvatar( FileUpload fileControl, ILPathways.Business.Library library, Business.LibrarySection collection )
      {
        //Validation
          if ( !fileControl.HasFile ) 
        { 
            return; 
        }
          if ( !FileResourceController.IsFileSizeValid( fileControl, maxFileSize ) )
        {
          ltlOutput.Text = string.Format( "Error: File must be {0}KB or less.", ( maxFileSize / 1024 ) );
          return;
        }
        if ( fileControl.PostedFile.ContentType.IndexOf( "image/" ) != 0 )
        {
          ltlOutput.Text = "Error: You must select an image file.";
          return;
        }

        var targetGUID = ( collection != null && collection.Id > 0 ) ? collection.RowId.ToString() : library.RowId.ToString();
        string savingName = targetGUID.ToString().Replace( "-", "" ) + System.IO.Path.GetExtension( fileControl.FileName );
        string savingFolder = FileResourceController.DetermineDocumentPath( library );
        string savingURL = FileResourceController.DetermineDocumentUrl( library, savingName );
        if ( collection != null && collection.Id > 0 )
        {
          collection.ImageUrl = savingURL;
        }
        else
        {
          library.ImageUrl = savingURL;
        }

        ImageStore img = new ImageStore();
        img.FileName = savingName;
        img.FileDate = DateTime.Now;

        FileResourceController.HandleImageResizingToWidth( img, fileControl, libraryImageWidth, libraryImageWidth, false, true );
        FileSystemHelper.HandleDocumentCaching( savingFolder, img, true );


        if ( collection != null && collection.Id > 0 )
        {
          libService.LibrarySectionUpdate( collection );
        }
        else
        {
          libService.Update( library );
        }
      }

      private void CreateDocumentVersion()
      {
          //14-01-09 mparsons - avatars are not being stored in documentVersion at this time!
          //DocumentVersion entity = new DocumentVersion();
          //entity.CreatedById = user.user.Id;
          //entity.CreatedBy = user.user.FullName();
          //entity.CreatedBy = img.FileDate.ToShortDateString();
          //entity.MimeType = fileUpload.PostedFile.ContentType;
          //entity.FileName = savingName;
          //entity.FileDate = img.FileDate;
          //entity.Title = user.user.FullName() + "'s Avatar";
          //entity.Summary = "TBD";
          //entity.Status = "initial";
          //entity.URL = savingURL;

          //if ( img.Bytes > 0 )
          //{
          //    entity.ResourceBytes = img.Bytes;
          //    entity.SetResourceData( entity.ResourceBytes, img.ResourceData );
          //}
          //else
          //{
          //    fileUpload.PostedFile.InputStream.Position = 0;
          //    Stream fs = fileUpload.PostedFile.InputStream;

          //    byte[] data = new byte[ fs.Length ];
          //    fs.Read( data, 0, data.Length );
          //    fs.Close();
          //    fs.Dispose();
          //    entity.SetResourceData( entity.ResourceBytes, data );
          //}

          //string statusMessage = "";


          //ContentServices services = new ContentServices();
          //if ( entity.Id == 0 )
          //{
          //    services.DocumentVersionCreate( entity, ref statusMessage );
          //}
          //else
          //{
          //    services.DocumentVersionUpdate( entity );
          //}
      }
    }

}