using System;
using System.Data;
using System.Data.Sql;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

using ILPathways.Business;
using ThisLibrary = ILPathways.Business.Library;
using IOER.classes;
using ILPathways.DAL;
using IOER.Library;
using ILPathways.Utilities;
using Isle.BizServices;

using LRWarehouse.Business;

namespace IOER.Controllers
{
    /// <summary>
    /// Class for handling file system related methods 
    /// </summary>
    public class FileResourceController
    {

        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
        const string thisClassName = "Controllers.FileResourceController";

        public struct PathParts
        {
            public string basePath; //from web.config
            public string root;     //Personal or org rowId
            public string parent;   //userid for personal, folder name for org. Using guid, but should use something shorter!
            public string child;    //??Seems to be usually empty

            public string filePath;
            public string url;

            public string delimiter;
        }
        /// <summary>
        /// Instantiate
        /// </summary>
        public FileResourceController()
        { }

        #region Image Methods
        /// <summary>
        /// Check if a file has been entered or selected in an upload control
        /// </summary>
        /// <param name="imageFileUpload">Upload control</param>
        /// <returns></returns>
        public bool DoesImageExist( FileUpload imageFileUpload )
        {
            bool isValid = true;

            if ( imageFileUpload.FileName != "" && imageFileUpload.FileName != null )
            {
                try
                {

                    Bitmap bitmap = new Bitmap( imageFileUpload.PostedFile.InputStream, false );
                    //Rewind the stream to the beginning
                    imageFileUpload.PostedFile.InputStream.Position = 0;
                }
                catch ( Exception ex )
                {
                    isValid = false;
                }
            }
            return isValid;
        }

        /// <summary>
        /// validate image is one of the allowed mime types
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool IsAllowedImageMimeType( string fileName )
        {
            bool isValid = true;

            string extension = Path.GetExtension( fileName ).ToLower();

            switch ( extension )
            {
                case ".gif":
                    break;
                case ".jpg":
                    break;
                case ".jpeg":
                    break;
                case ".jpe":
                    break;
                case ".png":
                    break;

                default:
                    isValid = false;
                    break;
            }
            return isValid;
        }//

        /// <summary>
        /// validate that image is within allowed maximum height and width
        /// </summary>
        /// <param name="imageFileUpload">Upload control</param>
        /// <param name="maxWidth">If Zero, no maximum</param>
        /// <param name="maxHeight">If Zero, no maximum</param>
        /// <returns></returns>
        public bool IsImageSizeValid( FileUpload imageFileUpload, int maxWidth, int maxHeight )
        {
            bool isValid = true;

            try
            {
                if ( imageFileUpload.FileName != "" && imageFileUpload.FileName != null )
                {

                    Bitmap bitmap = new Bitmap( imageFileUpload.PostedFile.InputStream, false );
                    //Rewind the stream to the beginning
                    imageFileUpload.PostedFile.InputStream.Position = 0;

                    if ( ( maxWidth > 0 && bitmap.Width > maxWidth )
                            || maxHeight > 0 && bitmap.Height > maxHeight )
                    {
                        isValid = false;
                    }
                }
            }
            catch ( Exception ex )
            {
                isValid = false;
            }
            return isValid;
        }//
        /// <summary>
        /// validate that image is within allowed maximum height and width
        /// </summary>
        /// <param name="imageFileUpload">Upload control</param>
        /// <returns></returns>
        public static bool IsImageSizeValid( FileUpload fileUpload )
        {
            int maxFileSize = UtilityManager.GetAppKeyValue( "maxImageSize", 1000000 );
            return IsFileSizeValid( fileUpload, maxFileSize );
        }//
        /// <summary>
        /// Validate if file selected for upload is withing the max is allowed for the site
        /// </summary>
        /// <param name="fileUpload"></param>
        /// <returns></returns>
        public static bool IsDocumentSizeValid( FileUpload fileUpload )
        {
            int maxFileSize = UtilityManager.GetAppKeyValue( "maxDocumentSize", 4000000 );
            return IsFileSizeValid( fileUpload, maxFileSize );
        }//
        /// <summary>
        /// validate that file is allowable size
        /// </summary>
        /// <param name="fileUpload">Upload control</param>
        /// <param name="maxFileSize">Max file size in bytes</param>
        /// <returns></returns>
        public static bool IsFileSizeValid( FileUpload fileUpload, int maxFileSize )
        {
            bool isValid = true;

            try
            {
                if ( fileUpload.FileName != "" && fileUpload.FileName != null )
                {

                    int imageSize = ( int ) fileUpload.PostedFile.InputStream.Length;
                    //Rewind the stream to the beginning
                    fileUpload.PostedFile.InputStream.Position = 0;

                    if ( imageSize > maxFileSize )
                    {
                        isValid = false;
                    }
                }
            }
            catch
            {
                isValid = false;
            }
            return isValid;
        }//

        /// <summary>
        /// Examine provided image and resize if appropriate while maintaining aspect ratio
        /// - uses a default server work directory from web.config app key of: path.ReportsOutputPath
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="imageFileUpload"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        public static bool HandleImageResizing( IImage entity, FileUpload imageFileUpload, int maxWidth, int maxHeight )
        {
            //the application must have update access to the related work folder
            string tempWorkDirectory = UtilityManager.GetAppKeyValue( "path.ReportsOutputPath" ); ;

            return HandleImageResizing( entity, imageFileUpload, maxWidth, maxHeight, tempWorkDirectory );
        }//

        /// <summary>
        /// Examine provided image and resize if appropriate while maintaining aspect ratio
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="imageFileUpload"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <param name="tempWorkDirectory">Server directory to use when resizing image</param>
        /// <returns></returns>
        public static bool HandleImageResizing( IImage entity, FileUpload imageFileUpload,
                                                int maxWidth, int maxHeight, string tempWorkDirectory )
        {
            bool isValid = true;

            try
            {
                //get image from upload control
                Bitmap img = new Bitmap( imageFileUpload.PostedFile.InputStream, false );
                //Rewind the stream to the beginning
                imageFileUpload.PostedFile.InputStream.Position = 0;

                if ( ( maxWidth > 0 && img.Width > maxWidth )
                    || ( maxHeight > 0 && img.Height > maxHeight ) )
                {
                    //resize image
                    double widthRatio = 1;
                    double heightRatio = 1;

                    if ( maxWidth > 0 )
                        widthRatio = ( double ) maxWidth / ( double ) img.Width;

                    if ( maxHeight > 0 )
                        heightRatio = ( double ) maxHeight / ( double ) img.Height;

                    double scaleFactor = Math.Min( widthRatio, heightRatio );

                    Stream fromStream = imageFileUpload.PostedFile.InputStream;
                    MemoryStream toStream = new MemoryStream();
                    //scale, and return file location (will be deleted)
                    string scaledDiskFile = ResizeImageToFile( scaleFactor, fromStream, toStream, entity, tempWorkDirectory );

                    //file path should be returned to resized image
                    if ( scaledDiskFile.Length > 0 )
                    {
                        FileStream fs = new FileStream( scaledDiskFile, FileMode.Open );
                        entity.Bytes = fs.Length;
                        byte[] data = new byte[ fs.Length ];
                        fs.Read( data, 0, data.Length );
                        fs.Close();
                        fs.Dispose();
                        entity.SetImageData( entity.Bytes, data );

                        //now delete the server file
                        FileInfo fi = new FileInfo( scaledDiskFile );
                        fi.Delete();
                    }
                    else
                    {
                        //otherwise we probably have an issue and should return an error!!!!!!
                        entity.Message = "Error - problem encountered while attempting to resize the selected image. ";
                        isValid = false;

                        //entity.Bytes = toStream.Length;
                        //byte[] data = new byte[ entity.Bytes ];

                        //toStream.Read( data, 0, data.Length );
                        //toStream.Close();
                        //toStream.Dispose();
                        //entity.SetImageData( entity.Bytes, data );
                    }
                }
                else
                {

                    entity.Height = img.Height;
                    entity.Width = img.Width;

                    entity.Bytes = imageFileUpload.PostedFile.InputStream.Length;
                    byte[] data = new byte[ entity.Bytes ];
                    imageFileUpload.PostedFile.InputStream.Read( data, 0, data.Length );

                    entity.SetImageData( entity.Bytes, data );
                }

            }
            catch ( Exception ex )
            {
                entity.Message = ex.Message.ToString();
                isValid = false;
            }
            return isValid;
        }//

        /// <summary>
        /// Resize image to upload to a max size and width
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="imageFileUpload"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <param name="setToMaxWidth"></param>
        /// <param name="saveToFile"></param>
        /// <returns></returns>
        public static bool HandleImageResizingToWidth( IImage entity, FileUpload imageFileUpload, int maxWidth, int maxHeight, bool setToMaxWidth, bool saveToFile )
        {
            //the application must have update access to the related work folder
            string tempWorkDirectory = UtilityManager.GetAppKeyValue( "path.ReportsOutputPath" );

            return HandleImageResizingToWidth( entity, imageFileUpload, maxWidth, maxHeight, setToMaxWidth, saveToFile, tempWorkDirectory );
        }//
        /// <summary>
        /// Size image to fit a width, and crop height
        /// --just reducing for now, next step would be to use the Crop method
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="imageFileUpload"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <param name="setToMaxWidth">If true, set to max width, even if narrower</param>
        /// <param name="saveToFile">If true, save to file system, if false bytes will be returned in entity</param>
        /// <param name="tempWorkDirectory"></param>
        /// <returns></returns>
        public static bool HandleImageResizingToWidth( IImage entity, FileUpload imageFileUpload,
                                             int maxWidth, int maxHeight, bool setToMaxWidth,
                                             bool saveToFile, string tempWorkDirectory )
        {
            bool isValid = true;
            string scaledDiskFile = "";
            ImageFormat imgFormat;

            //if not max, why call
            if ( maxWidth < 1 )
                return false;
            //don't check height, may be situation to allow tall image regardless of height, but unlikely
            try
            {
                //get image from upload control
                Bitmap img = new Bitmap( imageFileUpload.PostedFile.InputStream, false );
                //Rewind the stream to the beginning
                imageFileUpload.PostedFile.InputStream.Position = 0;

                imgFormat = img.RawFormat;

                double widthRatio = 1;

                if ( img.Width > maxWidth )
                    widthRatio = ( double ) maxWidth / ( double ) img.Width;
                else if ( img.Width < maxWidth && setToMaxWidth == true )
                    widthRatio = ( double ) img.Width / ( double ) maxWidth;

                Bitmap thumbnailBitmap = null;
                if ( widthRatio != 1 )
                {
                    //first set to proper width

                    // Create new bitmap:
                    Stream fromStream = imageFileUpload.PostedFile.InputStream;
                    MemoryStream toStream = new MemoryStream();
                    thumbnailBitmap = ResizeImageToBitmap( widthRatio, fromStream, toStream, entity );
                    if ( thumbnailBitmap != null )
                    {
                        if ( thumbnailBitmap.Height > maxHeight )
                        {
                            //crop - do a center crop if height is close, otherwise top
                            int startY = ( thumbnailBitmap.Height - maxHeight ) / 2;
                            if ( startY > 10 )
                                startY = 0;
                            Rectangle imageRectangle = new Rectangle( 0, startY, thumbnailBitmap.Width, maxHeight );
                            thumbnailBitmap = CropBitmap( thumbnailBitmap, imageRectangle );
                        }

                    }
                    else
                    {
                        isValid = false;
                        entity.Message = "Unable to resize file";
                    }
                }
                else
                {
                    //width is ok, check if needs to be cropped
                    if ( img.Height > maxHeight )
                    {
                        Rectangle imageRectangle = new Rectangle( 0, 0, img.Width, maxHeight );
                        thumbnailBitmap = CropBitmap( img, imageRectangle );
                    }
                    else
                    {
                        //set to input
                        thumbnailBitmap = img;
                    }
                }

                if ( thumbnailBitmap != null )
                {
                    if ( saveToFile == true )
                    {
                        try
                        {
                            // output path should contain trailling \\
                            if ( tempWorkDirectory.Trim().EndsWith( "\\" ) == false )
                            {
                                tempWorkDirectory = tempWorkDirectory.Trim() + "\\";
                            }

                            string origDiskFile = tempWorkDirectory + entity.FileName;
                            //scaledDiskFile = tempWorkDirectory + "scaled_" + entity.FileName;
                            //don't want to rename, unless passing back, as may be saving to planned location
                            scaledDiskFile = tempWorkDirectory + entity.FileName;
                            //image.Save( origDiskFile, image.RawFormat );
                            //NOTE - 
                            if ( System.IO.File.Exists( scaledDiskFile ) )
                            {
                                FileInfo fi = new FileInfo( scaledDiskFile );
                                fi.Delete();
                            }
                            thumbnailBitmap.Save( scaledDiskFile, imgFormat );
                            FileStream fs = new FileStream( scaledDiskFile, FileMode.Open, FileAccess.Read );
                            entity.Bytes = fs.Length;
                            byte[] data = new byte[ fs.Length ];
                            fs.Read( data, 0, data.Length );
                            fs.Close();
                            fs.Dispose();
                            entity.SetImageData( entity.Bytes, data );

                        }
                        catch ( Exception ex )
                        {
                            LoggingHelper.LogError( ex, thisClassName + ".HandleImageResizingToWidth(): Error resizing image: id-" + entity.Id.ToString() + " filename: " + entity.FileName );

                        }
                    }
                    else
                    {

                    }
                }
                else
                {
                    isValid = false;
                    entity.Message = "Unable to resize file";
                }
            }
            catch ( Exception ex )
            {
                entity.Message = ex.Message.ToString();
                isValid = false;
            }
            return isValid;
        }//


        /// <summary>
        /// Resize an image from a stream
        /// </summary>
        /// <param name="scaleFactor"></param>
        /// <param name="fromStream"></param>
        /// <param name="toStream"></param>
        /// <param name="entity"></param>
        /// <param name="tempWorkDirectory"></param>
        /// <returns></returns>
        public static string ResizeImageToFile( double scaleFactor, Stream fromStream, Stream toStream, IImage entity, string tempWorkDirectory )
        {
            bool action = false;
            string scaledDiskFile = "";
            ImageFormat imgFormat;

            try
            {
                //long startingLen = fromStream.Length;

                System.Drawing.Image image = System.Drawing.Image.FromStream( fromStream );

                //note determine if the following is necesary or if the raw format is all that is necessary - more maintainable if true!
                //if ( entity.FileName.ToLower().IndexOf( ".gif" ) > 0 )
                //  imgFormat = ImageFormat.Gif;
                //else if ( entity.FileName.ToLower().IndexOf( ".bmp" ) > 0 )
                //  imgFormat = ImageFormat.Bmp;
                //else if ( entity.FileName.ToLower().IndexOf( ".jpe" ) > 0 )
                //  imgFormat = ImageFormat.Jpeg;
                //else if ( entity.FileName.ToLower().IndexOf( ".jpg" ) > 0 )
                //  imgFormat = ImageFormat.Jpeg;
                //else if ( entity.FileName.ToLower().IndexOf( ".jpeg" ) > 0 )
                //  imgFormat = ImageFormat.Jpeg;
                //else if ( entity.FileName.ToLower().IndexOf( ".png" ) > 0 )
                //  imgFormat = ImageFormat.Png;
                //else
                imgFormat = image.RawFormat;

                int newWidth = ( int ) ( image.Width * scaleFactor );
                int newHeight = ( int ) ( image.Height * scaleFactor );

                entity.Height = newHeight;
                entity.Width = newWidth;

                Bitmap thumbnailBitmap = new Bitmap( newWidth, newHeight );

                Graphics thumbnailGraph = Graphics.FromImage( thumbnailBitmap );

                thumbnailGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbnailGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbnailGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;

                Rectangle imageRectangle = new Rectangle( 0, 0, newWidth, newHeight );

                thumbnailGraph.DrawImage( image, imageRectangle );

                thumbnailBitmap.Save( toStream, imgFormat );

                //save resized image to a server folder
                try
                {
                    // output path should contain trailling \\
                    if ( tempWorkDirectory.Trim().EndsWith( "\\" ) == false )
                    {
                        tempWorkDirectory = tempWorkDirectory.Trim() + "\\";
                    }

                    string origDiskFile = tempWorkDirectory + entity.FileName;
                    //scaledDiskFile = tempWorkDirectory + "scaled_" + entity.FileName;
                    //don't want to rename, unless passing back, as may be saving to planned location
                    scaledDiskFile = tempWorkDirectory + entity.FileName;
                    //image.Save( origDiskFile, image.RawFormat );
                    if ( System.IO.File.Exists( scaledDiskFile ) )
                    {
                        FileInfo fi = new FileInfo( scaledDiskFile );
                        fi.Delete();
                    }
                    thumbnailBitmap.Save( scaledDiskFile, imgFormat );
                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, thisClassName + ".ResizeImageToFile(): Error resizing image: id-" + entity.Id.ToString() + " filename: " + entity.FileName );

                }


                thumbnailGraph.Dispose();
                thumbnailBitmap.Dispose();
                image.Dispose();

                action = true;
            }
            catch ( Exception ex )
            {
                //return what?
            }

            return scaledDiskFile;
        }//
        public static Bitmap ResizeImageToBitmap( double scaleFactor, Stream fromStream, Stream toStream, IImage entity )
        {
            ImageFormat imgFormat;

            try
            {
                //long startingLen = fromStream.Length;

                System.Drawing.Image image = System.Drawing.Image.FromStream( fromStream );

                imgFormat = image.RawFormat;
                int newWidth = ( int ) ( image.Width * scaleFactor );
                int newHeight = ( int ) ( image.Height * scaleFactor );

                entity.Height = newHeight;
                entity.Width = newWidth;

                Bitmap thumbnailBitmap = new Bitmap( newWidth, newHeight );
                Graphics thumbnailGraph = Graphics.FromImage( thumbnailBitmap );

                thumbnailGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbnailGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbnailGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;

                Rectangle imageRectangle = new Rectangle( 0, 0, newWidth, newHeight );

                thumbnailGraph.DrawImage( image, imageRectangle );

                thumbnailBitmap.Save( toStream, imgFormat );

                //thumbnailGraph.Dispose();
                //thumbnailBitmap.Dispose();
                image.Dispose();

                return thumbnailBitmap;
            }
            catch ( Exception ex )
            {
                //return what?
                return null;
            }

        }//


        /// <summary>
        /// Crops an image according to a selection rectangel
        /// </summary>
        /// <param name="image">
        /// the image to be cropped
        /// </param>
        /// <param name="selection">
        /// the selection
        /// </param>
        /// <returns>
        /// cropped image
        /// </returns>
        public static System.Drawing.Image CropImage( System.Drawing.Image image, Rectangle selection )
        {
            Bitmap bmp = image as Bitmap;

            // Check if it is a bitmap:
            if ( bmp == null )
                throw new ArgumentException( "Not valid bitmap" );

            // Release the resources:
            image.Dispose();

            return CropImage( bmp, selection );
        }//

        /// <summary>
        /// Crops an image according to a selection rectangel
        /// </summary>
        /// <param name="image">
        /// the image to be cropped
        /// </param>
        /// <param name="selection">
        /// the selection
        /// </param>
        /// <returns>
        /// cropped image
        /// </returns>
        public static System.Drawing.Image CropImage( Bitmap bmp, Rectangle selection )
        {

            // Check if it is a bitmap:
            if ( bmp == null )
                throw new ArgumentException( "Not valid bitmap" );

            // Crop the image:
            Bitmap cropBmp = bmp.Clone( selection, bmp.PixelFormat );


            return cropBmp;
        }
        /// <summary>
        /// Crop image to rectangle size
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="selection"></param>
        /// <returns></returns>
        public static Bitmap CropBitmap( Bitmap bmp, Rectangle selection )
        {

            // Check if it is a bitmap:
            if ( bmp == null )
                throw new ArgumentException( "Not valid bitmap" );

            // Crop the image:
            Bitmap cropBmp = bmp.Clone( selection, bmp.PixelFormat );


            return cropBmp;
        }


        /// <summary>
        /// Examine provided image parameters and return the resized width and height appropriate for the provided maximums
        /// </summary>
        /// <param name="pCurrentWidth"></param>
        /// <param name="pCurrentHeight"></param>
        /// <param name="pMaxWidth"></param>
        /// <param name="pMaxHeight"></param>
        /// <param name="displayWidth"></param>
        /// <param name="displayHeight"></param>
        /// <returns></returns>
        public static bool SetImageDisplayAttributes( string pCurrentWidth, string pCurrentHeight, int pMaxWidth, int pMaxHeight, ref int pDisplayWidth, ref int pDisplayHeight )
        {
            //need to anticipate requirement to only resize in one direction
            //==> then create different method!
            //- check maxWidth and maxHeight, if only one exists, take appropriate actions
            bool isValid = true;
            int currentWidth, currentHeight;
            try
            {
                if ( pCurrentWidth.Length > 0 )
                    currentWidth = Int32.Parse( pCurrentWidth );
                else
                    currentWidth = pMaxWidth;

                if ( pCurrentHeight.Length > 0 )
                    currentHeight = Int32.Parse( pCurrentHeight );
                else
                    currentHeight = pMaxHeight;

                isValid = SetImageDisplayAttributes( currentWidth, currentHeight, pMaxWidth, pMaxHeight, ref pDisplayWidth, ref pDisplayHeight );

            }
            catch ( Exception ex )
            {
                pDisplayWidth = pMaxWidth;
                pDisplayHeight = pMaxHeight;
                isValid = false;
            }
            return isValid;

        }//

        /// <summary>
        /// Examine provided image parameters and return the resized width and height appropriate for the provided maximums
        /// </summary>
        /// <param name="currentWidth"></param>
        /// <param name="currentHeight"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <param name="displayWidth"></param>
        /// <param name="displayHeight"></param>
        /// <returns></returns>
        public static bool SetImageDisplayAttributes( int currentWidth, int currentHeight, int maxWidth, int maxHeight, ref int displayWidth, ref int displayHeight )
        {
            bool isValid = true;
            try
            {
                if ( ( maxWidth > 0 && currentWidth > maxWidth )
                        || maxHeight > 0 && currentHeight > maxHeight )
                {
                    //resize image
                    double widthRatio = 1;
                    double heightRatio = 1;

                    if ( maxWidth > 0 )
                        widthRatio = ( double ) maxWidth / ( double ) currentWidth;

                    if ( maxHeight > 0 )
                        heightRatio = ( double ) maxHeight / ( double ) currentHeight;

                    double scaleFactor = Math.Min( widthRatio, heightRatio );

                    displayWidth = ( int ) ( currentWidth * scaleFactor );
                    displayHeight = ( int ) ( currentHeight * scaleFactor );

                }
                else
                {
                    displayWidth = currentWidth;
                    displayHeight = currentHeight;
                }
            }
            catch ( Exception ex )
            {
                displayWidth = maxWidth;
                displayHeight = maxHeight;
                isValid = false;
            }
            return isValid;
        }
        #endregion

        #region Document methods
        //public static int CreateContentItemWithFileOnly2( FileUpload fileUpload, ContentItem parentItem, ContentItem fileResource, ref string statusMessage )
        //{

        //}

        /// <summary>
        /// Upload related file, create a document version and Create a ContentFile
        /// </summary>
        /// <param name="fileUpload"></param>
        /// <param name="fileResource"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static int CreateContentItemWithFileOnly( FileUpload fileUpload, ContentItem parentItem, ContentItem fileResource, ref string statusMessage )
        {
            int fileId = 0;
            //valid fileResource

            if ( fileUpload.HasFile == false || fileUpload.FileName == "" )
            {
                // no file, skip - this only ok , if update
                statusMessage = "Error no file was selected. Please select a file and try again.";
                return 0;
            }
            if ( fileResource.CreatedById == 0 )
            {
                statusMessage = "Error - the created by identifier must be assigned to the current user";
                return 0;
            }

            DocumentVersion entity = new DocumentVersion();
            entity.RowId = new Guid();
            entity.CreatedById = fileResource.CreatedById;
            entity.Created = System.DateTime.Now;
            entity.LastUpdatedById = fileResource.CreatedById;
            entity.Title = fileResource.Title;

            if ( parentItem != null && parentItem.Id > 0 )
            {
                if ( CreateDocument( fileUpload, entity, parentItem, ref statusMessage ) == true )
                {
                    fileResource.DocumentRowId = entity.RowId;
                    fileResource.DocumentUrl = entity.ResourceUrl;
                    fileId = new ContentServices().Create( fileResource, ref statusMessage );
                }
                else
                {
                    //reason should already be in the statusmessage
                }
            }
            else if ( CreateDocument( fileUpload, entity, fileResource.OrgId, ref statusMessage ) == true )
            {
                fileResource.DocumentRowId = entity.RowId;
                fileResource.DocumentUrl = entity.ResourceUrl;
                fileId = new ContentServices().Create( fileResource, ref statusMessage );
            }
            else
            {
                //reason should already be in the statusmessage
            }
            return fileId;

        }
    
        /// <summary>
        /// Upload a document to server and save/update in a DocumentVersion record
        /// NOT FOR USE WITH IMAGES!!
        /// </summary>
        /// <param name="fileUpload"></param>
        /// <param name="entity">DocumentVersion containing at least: createdById, title</param>
        /// <param name="parentItem"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static bool CreateDocument( FileUpload fileUpload, DocumentVersion entity, ContentItem parentItem, ref string statusMessage )
        {
            bool isValid = true;

            ContentServices myManager = new ContentServices();

            if ( fileUpload.HasFile == false || fileUpload.FileName == "" )
            {
                // no file, skip - this only ok , if update
                statusMessage = "Error no document";
                return false;
            }
            string orgRowId = "";
            if ( parentItem.OrgId > 0 )
            {
                if ( parentItem.HasOrg() )
                {
                    orgRowId = parentItem.ContentOrg.RowId.ToString();
                }
                else
                {
                    parentItem.ContentOrg = OrganizationBizService.EFGet( parentItem.OrgId );
                    if ( parentItem.ContentOrg != null && parentItem.ContentOrg.Id > 0 )
                        orgRowId = parentItem.ContentOrg.RowId.ToString();
                }
            }

            try
            {
                int maxFileSize = UtilityManager.GetAppKeyValue( "maxDocumentSize", 20000000 );
                if ( FileResourceController.IsFileSizeValid( fileUpload, maxFileSize ) == false )
                {
                    statusMessage = string.Format( "Error the selected file exceeds the size limits ({0} bytes).", maxFileSize ) ;
                    return false;
                }

                entity.MimeType = fileUpload.PostedFile.ContentType;
                entity.FileName = fileUpload.FileName;
                entity.FileDate = System.DateTime.Now;

                string sFileType = System.IO.Path.GetExtension( entity.FileName );
                sFileType = sFileType.ToLower();

                entity.FileName = System.IO.Path.ChangeExtension( entity.FileName, sFileType );

                //probably want to fix filename to standardize
                entity.CleanFileName();

                PathParts parts = DetermineDocumentPathUsingParentItem( parentItem );
                entity.FilePath = FormatPartsFilePath( parts );
                string url = FormatPartsRelativeUrl( parts );

                //string url = DetermineDocumentUrl( entity.CreatedById, parentItem.OrgId, orgRowId, entity.FileName );

                entity.ResourceUrl = url + "/" + entity.FileName;

                UploadFile( fileUpload, entity.FilePath, entity.FileName );
                //rewind for db save
                fileUpload.PostedFile.InputStream.Position = 0;
                Stream fs = fileUpload.PostedFile.InputStream;

                entity.ResourceBytes = fs.Length;
                byte[] data = new byte[ fs.Length ];
                fs.Read( data, 0, data.Length );
                fs.Close();
                fs.Dispose();
                entity.SetResourceData( entity.ResourceBytes, data );

                if ( entity.HasValidRowId() )
                {
                    statusMessage = myManager.DocumentVersionUpdate( entity );
                }
                else
                {
                    string documentId = myManager.DocumentVersionCreate( entity, ref statusMessage );
                    if ( documentId.Length > 0 )
                    {
                        entity.RowId = new Guid( documentId );
                        //fileResource.DocumentRowId = entity.RowId;
                        statusMessage = "Successfully saved document!";
                    }
                    else
                    {
                        statusMessage = "Error - Document save failed: " + statusMessage;
                        isValid = false;
                    }
                }

            }
            catch ( Exception ex )
            {
                statusMessage = "Unexpected error occurred while attempting to upload your file.<br/>" + ex.Message;
                return false;
            }


            return isValid;

        }//

        /// <summary>
        /// Upload a document to server and save/update in a DocumentVersion record
        /// NOT FOR USE WITH IMAGES!!
        /// </summary>
        /// <param name="fileUpload"></param>
        /// <param name="entity"></param>
        /// <param name="owningOrgId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static bool CreateDocument( FileUpload fileUpload, DocumentVersion entity, int owningOrgId, ref string statusMessage )
        {
            bool isValid = true;

            ContentServices myManager = new ContentServices();

            if ( fileUpload.HasFile == false || fileUpload.FileName == "" )
            {
                // no file, skip - this only ok , if update
                statusMessage = "Error no document";
                return false;
            }
            string orgRowId = "";
            if ( owningOrgId > 0 )
            {
                Organization org = OrganizationBizService.EFGet( owningOrgId );
                if ( org != null && org.Id > 0 )
                    orgRowId = org.RowId.ToString();
            }

            try
            {
                int maxFileSize = UtilityManager.GetAppKeyValue( "maxDocumentSize", 20000000 );
                if ( FileResourceController.IsFileSizeValid( fileUpload, maxFileSize ) == false )
                {
                    statusMessage = string.Format( "Error the selected file exceeds the size limits ({0} bytes).", maxFileSize );
                    return false;
                }

                entity.MimeType = fileUpload.PostedFile.ContentType;
                entity.FileName = fileUpload.FileName;
                entity.FileDate = System.DateTime.Now;

                string sFileType = System.IO.Path.GetExtension( entity.FileName );
                sFileType = sFileType.ToLower();

                entity.FileName = System.IO.Path.ChangeExtension( entity.FileName, sFileType );

                //probably want to fix filename to standardize
                entity.CleanFileName();

                PathParts parts = DetermineDocumentPath( entity.CreatedById, owningOrgId, orgRowId, "" );
                entity.FilePath = FormatPartsFilePath( parts );
                string url = FormatPartsRelativeUrl( parts );

                //entity.FilePath = DetermineDocumentPath( entity.CreatedById, owningOrgId, orgRowId );
                //string url = DetermineDocumentUrl( entity.CreatedById, owningOrgId, orgRowId, entity.FileName );

                entity.ResourceUrl = url + "/" + entity.FileName;

                UploadFile( fileUpload, entity.FilePath, entity.FileName );
                //rewind for db save
                fileUpload.PostedFile.InputStream.Position = 0;
                Stream fs = fileUpload.PostedFile.InputStream;

                entity.ResourceBytes = fs.Length;
                byte[] data = new byte[ fs.Length ];
                fs.Read( data, 0, data.Length );
                fs.Close();
                fs.Dispose();
                entity.SetResourceData( entity.ResourceBytes, data );

                if ( entity.HasValidRowId() )
                {
                    statusMessage = myManager.DocumentVersionUpdate( entity );
                }
                else
                {
                    string documentId = myManager.DocumentVersionCreate( entity, ref statusMessage );
                    if ( documentId.Length > 0 )
                    {
                        entity.RowId = new Guid( documentId );
                        //fileResource.DocumentRowId = entity.RowId;
                        statusMessage = "Successfully saved document!";
                    }
                    else
                    {
                        statusMessage = "Error - Document save failed: " + statusMessage;
                        isValid = false;
                    }
                }

            }
            catch ( Exception ex )
            {
                statusMessage = "Unexpected error occurred while attempting to upload your file.<br/>" + ex.Message;
                return false;
            }


            return isValid;

        }//
        public static void UploadFile( FileUpload fileUpload, string documentFolder, string filename )
        {
            try
            {
                FileSystemHelper.CreateDirectory( documentFolder );

                string diskFile = documentFolder + "\\" + filename;
                //string diskFile = MapPath( documentFolder ) + "\\" + entity.FileName;

                LoggingHelper.DoTrace( 5, thisClassName + " UploadFile(). doing SaveAs" );
                fileUpload.SaveAs( diskFile );

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format( "UploadFile(documentFolder: {0}, filename: {1})", documentFolder, filename ) );
            }
        }//

        /// <summary>
        /// Replace a document.
        /// Only the file contents can change. The file name and url do NOT change.
        /// </summary>
        /// <param name="fileUpload"></param>
        /// <param name="contentId"></param>
        /// <param name="userId"></param>
        /// <param name="statusMessage">Returns 'successful', if OK, else an error message.</param>
        /// <returns>true if replacement was successful, else false</returns>
        public bool ReplaceDocument( FileUpload fileUpload, int contentId, int userId, ref string statusMessage )
        {
            ContentManager mgr = new ContentManager();
            DocumentVersion docVersion = new DocumentVersion();
            ContentServices myManager = new ContentServices();
            int maxFileSize = UtilityManager.GetAppKeyValue( "maxDocumentSize", 4000000 );
            if ( IsFileSizeValid( fileUpload, maxFileSize ) == false )
            {
                statusMessage = string.Format( "Error the selected file exceeds the size limits ({0} bytes).", maxFileSize ) ;
                return false;
            }

            ContentItem entity = mgr.Get( contentId );
            if ( entity == null || entity.Id == 0 )
            {
                statusMessage = "Error: content item was not found ";
                return false;
            }
            if ( entity.IsValidRowId(entity.DocumentRowId) == false )
            {
                statusMessage = "Error: a document is not associated with this content item ";
                return false;
            }

            docVersion = myManager.DocumentVersionGet( entity.DocumentRowId );
            return ReplaceDocument( fileUpload, entity, docVersion, userId, ref statusMessage );
           }//

        /// <summary>
        /// Replace a document.
        /// Only the file contents can change. The file name and url do NOT change.
        /// TODO - need to check for change to the file type (extension)
        /// </summary>
        /// <param name="fileUpload"></param>
        /// <param name="contentId"></param>
        /// <param name="userId"></param>
        /// <param name="statusMessage">Returns 'successful', if OK, else an error message.</param>
        /// <returns>true if replacement was successful, else false</returns>
        public bool ReplaceDocument( FileUpload fileUpload, ContentItem entity, DocumentVersion docVersion, int userId, ref string statusMessage )
        {
            ContentServices myManager = new ContentServices();

            docVersion.LastUpdatedById = userId;
            docVersion.FileDate = System.DateTime.Now;
            docVersion.MimeType = fileUpload.PostedFile.ContentType;

            if ( docVersion.FilePath == null || docVersion.FilePath.Trim().Length == 0 )
            {
                //need to derive the file path from url
                //what it not resolved?
                GetFilePathFromDocumentUrl( docVersion, entity, userId );
            }
            UploadFile( fileUpload, docVersion.FilePath, docVersion.FileName );
            //rewind for db save
            fileUpload.PostedFile.InputStream.Position = 0;
            Stream fs = fileUpload.PostedFile.InputStream;

            docVersion.ResourceBytes = fs.Length;
            byte[] data = new byte[ fs.Length ];
            fs.Read( data, 0, data.Length );
            fs.Close();
            fs.Dispose();
            docVersion.SetResourceData( docVersion.ResourceBytes, data );

            statusMessage = myManager.DocumentVersionUpdate( docVersion );
            if ( statusMessage.Equals( "successful" ) )
            {
                ValidateDocumentOnServer( entity, docVersion, docVersion.FilePath );
                return true;
            }
            else
            {
                statusMessage = "Error encountered: " + statusMessage;
                return false;
            }
          
        }

        private static bool GetFilePathFromDocumentUrl( DocumentVersion doc, ContentItem entity, int userId )
        {
            bool isValid = false;
            string baseFolder = UtilityManager.GetAppKeyValue( "path.ContentOutputPath", "C:\\IOER\\ContentDocs\\" );
            if ( doc.URL == null || doc.URL.Trim().Length == 0 )
            {
                //should not happen? create the folder the traditional way
                string orgRowId = "";
                if ( entity.OrgId > 0 )
                {
                    Organization org = OrganizationBizService.EFGet( entity.OrgId );
                    if ( org != null && org.Id > 0 )
                        orgRowId = org.RowId.ToString();
                }

                PathParts parts = DetermineDocumentPath( userId, entity.OrgId, orgRowId, "" );
                doc.FilePath = FormatPartsFilePath( parts );
                doc.URL = FormatPartsRelativeUrl( parts );

                //doc.FilePath = DetermineDocumentPath( userId, entity.OrgId, orgRowId );
                //doc.URL = DetermineDocumentUrl( userId, entity.OrgId, orgRowId, doc.FileName );

                EmailManager.NotifyAdmin( "Document found without a URL", string.Format( "ContentItem.Id: {0}, document RowId: {1}, userId: {2}", entity.Id, doc.RowId.ToString(), userId ) );
                return true;
            }

            int filePos = doc.URL.ToLower().LastIndexOf( "/" );
            if ( filePos > 0 )
            {
                //extract up to latter and prefix with base
                string part = doc.URL.Substring( 0, filePos + 1 );
                //this is an unfortunate assumption. but this step should not be necessary after a while.
                //necessary as ContentDocs is prob part of baseFolder
                part = part.Replace( "/ContentDocs", "" );
                part = part.Replace( "/", "\\" );
                part = part.Replace( "\\\\", "\\" );
                doc.FilePath = baseFolder + part;
                doc.FilePath = doc.FilePath.Replace( "\\ContentDocs\\ContentDocs", "\\ContentDocs" );
                isValid = true;
            }
            else
            {
                //shoudn't happen, but what if?
                string orgRowId = "";
                if ( entity.OrgId > 0 )
                {
                    Organization org = OrganizationBizService.EFGet( entity.OrgId );
                    if ( org != null && org.Id > 0 )
                        orgRowId = org.RowId.ToString();
                }
                //doc.FilePath = DetermineDocumentPath( userId, entity.OrgId, orgRowId );
                //doc.URL = DetermineDocumentUrl( userId, entity.OrgId, orgRowId, doc.FileName );

                PathParts parts = DetermineDocumentPath( userId, entity.OrgId, orgRowId, "" );
                doc.FilePath = FormatPartsFilePath( parts );
                doc.URL = FormatPartsRelativeUrl( parts );

                EmailManager.NotifyAdmin( "Document found with missing/strange URL", string.Format( "ContentItem.Id: {0}, document RowId: {1}, userId: {2}", entity.Id, doc.RowId.ToString(), userId ) );
                return true;
            }

            return isValid;
        }//

        /// <summary>
        /// Validate the related doc is on the server, and cache if not found
        /// </summary>
        /// <param name="parentEntity"></param>
        /// <param name="doc"></param>
        /// <returns>url to the file if found</returns>
        public static string ValidateDocumentOnServer( ContentItem parentEntity, DocumentVersion doc )
        {
            string documentFolder = "";
            if ( doc.FileLocation().Length > 0 )
                documentFolder = doc.FilePath;
            else
            {
                PathParts parts = DetermineDocumentPathUsingParentItem( parentEntity );
                documentFolder = parts.filePath;
            }

            return ValidateDocumentOnServer( parentEntity, doc, documentFolder );
        }//

        /// <summary>
        /// handle validation for a content supplement - contains the doc
        /// TODO - update the filepath if not already existing
        /// </summary>
        /// <param name="docParent"></param>
        /// <param name="doc"></param>
        /// <param name="parentEntity"></param>
        /// <returns></returns>
        public static string ValidateSupplementDocumentOnServer( ContentSupplement docParent, DocumentVersion doc, ContentItem parentEntity )
        {
            PathParts parts = new PathParts();
            string documentFolder = "";
            bool assignedFolder = false;
            string fileUrl = "";
            string baseUrl = UtilityManager.GetAppKeyValue( "path.ContentOutputUrl", "/Content/" );
            string basePath = UtilityManager.GetAppKeyValue( "path.ContentOutputPath" );
            string filename = doc.FileName.ToLower();

            try
            {
                //NOTE: the doc parent should be the main source of the url!
                if ( doc.URL != null && doc.URL.Length > 10 )
                {
                    fileUrl = doc.URL;
                    //do we need a doc folder check here?
                    if ( doc.FilePath == null || doc.FilePath.Trim().Length < 5 )
                    {
                        documentFolder = FileSystemHelper.SetFilePathFromUrl( fileUrl, filename );
                        if (documentFolder.Length > 5)
                            assignedFolder = true;
                    }
                }
                else if ( docParent.ResourceUrl != null && docParent.ResourceUrl.Length > 10 )
                {
                    fileUrl = docParent.ResourceUrl;
                    //TODO - convert a resource url to a file path
                    int start = fileUrl.ToLower().IndexOf( baseUrl.ToLower() );
                    if ( start > -1 )
                    {
                        documentFolder = basePath + fileUrl.Substring( start + baseUrl.Length );
                        documentFolder = documentFolder.Replace( "/", "\\" );
                        //extract filename
                        int pos = documentFolder.ToLower().IndexOf( filename );
                        if ( pos > -1 )
                        {
                            documentFolder = documentFolder.Substring( 0, pos - 1 );
                        }
                        assignedFolder = true;
                    }
                }

                //only do if ???
                if ( doc.FileLocation().Length > 0 )
                {
                    if (assignedFolder ==false)
                        documentFolder = doc.FilePath;
                }
                else if (documentFolder == "")
                {
                    //no file path exists, so...
                    //determine if the orgRowId will be used
                    string orgRowId = "";
                    Organization org = OrganizationBizService.EFGet( parentEntity.OrgId );
                    if ( org != null && org.Id > 0 )
                        orgRowId = org.RowId.ToString();

                    string childFolder = "";
                    if ( parentEntity.HasValidRowId() )
                        childFolder = parentEntity.RowId.ToString();

                    parts = DetermineDocumentPath( parentEntity.CreatedById, parentEntity.OrgId, orgRowId, childFolder );
                    documentFolder = parts.filePath;
                    fileUrl = parts.url;
                    fileUrl = baseUrl + parts.url + "/" + doc.FileName;
                }

                //TODO - may want pass option to overwrite file (rather than using false)
                string message = FileSystemHelper.HandleDocumentCaching( documentFolder, doc, false );
                if ( message == "" )
                {
                    //blank returned message means ok
                    //handle url
                    if ( fileUrl == null || fileUrl.Trim().Length < 10 )
                    {
                        //TODO - may be problem using parentEntity???
                        fileUrl = DetermineDocumentUrl( parentEntity, doc.FileName );
                    }

                    if ( documentFolder.ToLower() != doc.FilePath.ToLower()
                        || fileUrl.ToLower() != doc.URL.ToLower() )
                    {
                        //update
                        doc.FilePath = documentFolder;
                        doc.URL = fileUrl;
                        //note ensure have full record - may want to just do a subset to make sure
                        new DocumentServices().Document_Version_Update( doc );
                    }
                }
                else
                {
                    //error, should return a message
                    //this.SetConsoleErrorMessage( message );
                    parentEntity.Message = message;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ValidateDocumentOnServer() - Unexpected error encountered while retrieving document" );


            }
            return fileUrl;
        }//


        /// <summary>
        /// Validate the related doc is on the server, and cache if not found
        /// </summary>
        /// <param name="parentEntity"></param>
        /// <param name="doc"></param>
        /// <param name="documentFolder"></param>
        /// <returns>url to the file if found</returns>
        public static string ValidateDocumentOnServer( ContentItem parentEntity, DocumentVersion doc, string documentFolder )
        {
            string fileUrl = "";

            try
            {

                string message = FileSystemHelper.HandleDocumentCaching( documentFolder, doc, true );
                if ( message == "" )
                {
                    //blank returned message means ok

                    if ( doc.URL != null || doc.URL.Length > 10 )
                        fileUrl = doc.URL;
                    else 
                        fileUrl = DetermineDocumentUrl( parentEntity, doc.FileName );
                }
                else
                {
                    //error, should return a message
                    //this.SetConsoleErrorMessage( message );
                    parentEntity.Message = message;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ValidateDocumentOnServer() - Unexpected error encountered while retrieving document" );

                
            }
            return fileUrl;
        }//
        #endregion

        #region Authoring methods
        /// <summary>
        /// determine path using ContentItem for org and created by id
        /// The child folder will be the parent entity guid, to prevent name collisions
        /// SHOULD NOT USE WHERE A FILE PATH EXISTS IN DOCUMENT
        /// </summary>
        /// <param name="parentEntity"></param>
        /// <returns></returns>
        public static PathParts DetermineDocumentPathUsingParentItem( ContentItem parentEntity )
        {
            //first check for existing/known path
            if ( parentEntity.HasDocument() )
            {
                //fill path
                PathParts parts = new PathParts();
                parts.filePath = parentEntity.RelatedDocument.FilePath;
                parts.url = parentEntity.RelatedDocument.URL;

                parts.basePath = "";
                parts.root = "";
                parts.parent = "";
                parts.child = "";

                if (parts.filePath != null && parts.filePath.Trim().Length > 10)
                    return parts;
            }
            else if ( parentEntity.DocumentRowId != null && parentEntity.IsValidRowId( parentEntity.DocumentRowId ) )
            {
                //WARNING - SHOULD NOT GET HERE. DON'T WANT TO CALL CALL TO DAO FROM HERE! or maybe?
                string msg = string.Format("EntityId: {0}, title: {1}", parentEntity.Id, parentEntity.Title );
                EmailManager.NotifyAdmin( "Unexpected path hit in DetermineDocumentPathUsingParentItem ***", msg );

            }
            //determine if the orgRowId will be used
            string orgRowId = GetOrgRowid( parentEntity );
            string childFolder = "";
            if ( parentEntity.HasValidRowId() )
                childFolder = parentEntity.RowId.ToString();

            return DetermineDocumentPath( parentEntity.CreatedById, parentEntity.OrgId, orgRowId, childFolder );
        }//

    
        /// <summary>
        /// determine path using ContentItem for org, and the provided author id
        /// </summary>
        /// <param name="parentEntity"></param>
        /// <param name="authorId"></param>
        /// <returns></returns>
        //public static string DetermineDocumentPath( ContentItem parentEntity, int authorId )
        //{
        //    //determine if the orgRowId will be used
        //    string orgRowId = GetOrgRowid( parentEntity );
        //    string childFolder = "";
        //    if ( parentEntity.HasValidRowId() )
        //        childFolder = parentEntity.RowId.ToString();
        //    PathParts parts = DetermineDocumentPath( authorId, parentEntity.OrgId, orgRowId, childFolder );

        //    return FormatPartsFilePath( parts );
        //}//
        public static string DetermineDocumentPath( ThisLibrary parentEntity )
        {
            PathParts parts = DetermineDocumentPath( parentEntity.CreatedById, parentEntity.OrgId, "", "" );
           return FormatPartsFilePath( parts );
        }//
        public static string DetermineDocumentPath( int createdById, int orgId )
        {
            string orgRowId = "";
            Organization org = OrganizationBizService.EFGet( orgId );
            if ( org != null && org.Id > 0 )
                orgRowId = org.RowId.ToString();

            PathParts parts = DetermineDocumentPath( createdById, orgId, orgRowId, "" );
            return FormatPartsFilePath( parts );
        }

		public static string DetermineDocumentPath( int createdById, Organization org )
		{
			string orgRowId = "";
			if ( org != null && org.Id > 0 )
				orgRowId = org.RowId.ToString();

			PathParts parts = DetermineDocumentPath( createdById, org.Id, orgRowId, "" );
			return FormatPartsFilePath( parts );
		}

        public static PathParts DetermineDocumentPath( int createdById, int orgId, string orgRowId, string childFolder )
        {
            PathParts parts = new PathParts();

            string path = "";
            string relativePath = "";
            if ( childFolder != null && childFolder.Trim().Length > 0 )
            {
                childFolder = "\\" + childFolder;
            }
            else
                childFolder = "";

            string orgPart = "";
            string userPart = "\\" + createdById.ToString();
            if ( orgId > 0 )
            {
                if ( FormHelper.IsValidRowId( orgRowId ) )
                    orgPart = orgRowId;
                else
                    orgPart = orgId.ToString();

                //if ( childFolder.Length > 0 )
                //    relativePath = orgPart + childFolder;
                //else
                //    relativePath = orgPart + userPart;

                parts.root = orgPart;
                if ( childFolder.Length > 0 )
                    parts.parent = childFolder;
                else
                    parts.parent = userPart;
                parts.child = "";
            }
            else
            {
                //orgPart = "Personal";
                //relativePath = orgPart + userPart + childFolder;

                parts.root = "Personal";
                parts.parent = userPart;
                if ( childFolder.Length > 0 )
                    parts.child = childFolder;
                else
                    parts.child = "";
            }

            //parts.filePath = parts.basePath + parts.root + parts.parent + parts.child;

            //parts.url = "/" + parts.root + parts.parent + parts.child;

            //path = GetContentPath( relativePath );

            parts.filePath = FormatPartsFilePath( parts );
            parts.url = FormatPartsRelativeUrl( parts );
            return parts;
        }//

        public static string FormatPartsFilePath( PathParts parts )
        {
            //parts.filePath = parts.basePath;
            //we assume the web.config values ends with \, but could check

            parts.basePath = UtilityManager.GetAppKeyValue( "path.ContentOutputPath", "C:\\" );
            //make sure file path is blank
            parts.filePath = parts.basePath;

            parts.filePath = AddPart( parts.filePath, parts.root );
            parts.filePath = AddPart( parts.filePath, parts.parent );
            parts.filePath = AddPart( parts.filePath, parts.child );

            //if ( parts.root.Length > 0 )
            //{
            //    if ( parts.filePath.Trim().EndsWith( "\\" ) )
            //        parts.filePath += parts.root;
            //    else
            //        parts.filePath += "\\" + parts.root;
            //}

            //if ( parts.parent.Length > 0 )
            //    parts.filePath += "\\" + parts.parent;
            //if ( parts.child.Length > 0 )
            //    parts.filePath += "\\" + parts.child;

            parts.filePath = parts.filePath.Replace( "\\\\", "\\" );
            parts.parent = parts.parent.Replace( "\\\\", "\\" );

            return parts.filePath;
        }//

        private static string AddPart( string basePart, string part )
        {
            if ( part == null || part.Trim().Length == 0 )
                return basePart;

            if ( basePart.Trim().EndsWith( "\\" ) )
                basePart += part;
            else
                basePart += "\\" + part;

            return basePart;
        }//

        public static string FormatPartsRelativeUrl( PathParts parts )
        {
            ///?????
            string baseUrl = UtilityManager.GetAppKeyValue( "path.ContentOutputUrl", "/ContentDocs/" );

            parts.url = baseUrl + parts.root + "/" + parts.parent;
            if ( parts.child.Length > 0 )
                parts.url += "/" + parts.child;

            parts.url = parts.url.Replace( "\\", "/" );
            parts.url = parts.url.Replace( "//", "/" );

            return parts.url;
        }//
        public static string FormatPartsFullUrl( PathParts parts, string docFileName )
        {
            parts.url = "/" + parts.root + "/" + parts.parent;
            if ( parts.child.Length > 0 )
                parts.url += "/" + parts.child;

            string baseUrl = UtilityManager.GetAppKeyValue( "path.ContentOutputUrl", "/ContentDocs/" );
            string path = baseUrl + parts.url + "/" + docFileName;
            path = path.Replace( "/\\", "/" );
            path = path.Replace( "//", "/" );
            return path;
        }//

        public static string DetermineDocumentPath( string parentFolder, string childFolder  )
        {
            string path = "";
            string relativePath = "";
            if ( parentFolder.Trim().EndsWith( "\\" ) == false )
                parentFolder = parentFolder.Trim() + "\\";
            if ( childFolder.Trim().EndsWith( "\\" ) == false )
                childFolder = childFolder.Trim() + "\\";


            relativePath = parentFolder + childFolder;

            path = GetContentPath( relativePath );

            return path;
        }//

        public static string GetRelativePath( Patron user )
        {
            return GetRelativePath( user.Id, user.OrgId, "" );
        } //

        /// <summary>
        /// Construct relative path using user, and org
        /// </summary>
        /// <param name="createdById"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public static string GetRelativePath( int createdById, int orgId )
        {
            return GetRelativePath( createdById, orgId, "" );
        }

        /// <summary>
        /// Construct relative path using user, org, or org rowId
        /// </summary>
        /// <param name="createdById"></param>
        /// <param name="orgId"></param>
        /// <param name="orgRowId"></param>
        /// <returns></returns>
        public static string GetRelativePath( int createdById, int orgId, string orgRowId )
        {
            string path = "";

            string orgPart = "";
            string userPart = "\\" + createdById.ToString();
            if ( orgId > 0 )
            {
                if ( FormHelper.IsValidRowId( orgRowId ) )
                    orgPart = orgRowId;
                else
                    orgPart = orgId.ToString();
            }
            else
            {
                orgPart = "Personal";
            }
            path = orgPart + userPart;
            return path;

        }

        /// <summary>
        /// return base system path for storing content related files using folderIdentifier
        /// Assuming the base path contains the ending slash (\)
        /// </summary>
        /// <param name="folderIdentifier"></param>
        /// <returns></returns>
        public static string GetContentPath( string folderIdentifier )
        {
            string baseFolder = UtilityManager.GetAppKeyValue( "path.ContentOutputPath", "C:\\" );

            return baseFolder + folderIdentifier;

        }

        public static string DetermineDocumentUrl( ContentItem parentEntity, IDocument doc )
        {
 
            return DetermineDocumentUrl( parentEntity, doc.FileName );
        }//

        public static string DetermineDocumentUrl( ContentItem parentEntity, string docFileName )
        {
            //get base path
            string path = "";
            string relativePath = "";
            string orgRowId = "";
            if ( parentEntity.OrgId > 0 )
            {
                if ( parentEntity.HasOrg() )
                    relativePath = GetRelativeUrlPath( parentEntity.CreatedById, parentEntity.OrgId, parentEntity.ContentOrg.RowId.ToString() );
                else
                {
                    Organization org = OrganizationBizService.EFGet( parentEntity.OrgId );
                    if ( org != null && org.Id > 0 )
                        orgRowId = org.RowId.ToString();
                    relativePath = GetRelativeUrlPath( parentEntity.CreatedById, parentEntity.OrgId, orgRowId );
                }
            }
            else
            {
                relativePath = GetRelativeUrlPath( parentEntity.CreatedById, 0, "" );
            }
            string baseUrl = UtilityManager.GetAppKeyValue( "path.ContentOutputUrl", "/ContentDocs/" );
            path = baseUrl + relativePath + "/" + docFileName;

            return path;
        }//

        public static string DetermineDocumentUrl( ThisLibrary parentEntity, string docFileName )
        {
            return DetermineDocumentUrl( parentEntity.CreatedById, parentEntity.OrgId, "", docFileName );
        }
        public static string DetermineDocumentUrl( IBaseObject parentEntity, int orgId, string docFileName )
        {
            string orgRowId = "";
            if ( orgId > 0 )
            {
                Organization org = OrganizationBizService.EFGet( orgId );
                if ( org != null && org.Id > 0 )
                    orgRowId = org.RowId.ToString();
            }
            return DetermineDocumentUrl( parentEntity.CreatedById, orgId, orgRowId, docFileName );
        }//

        public static string DetermineDocumentUrl( int createdById, int orgId, string orgRowId, string docFileName )
        {
            //get base path
            string path = "";

            string relativePath = "";
            string orgPart = "";
            string userPart = "/" + createdById.ToString();
            if ( orgId > 0 )
            {
                if ( FormHelper.IsValidRowId( orgRowId ) )
                    orgPart = orgRowId;
                else
                    orgPart = orgId.ToString();

            }
            else
            {
                orgPart = "Personal";
            }
            relativePath = orgPart + userPart;

            string baseUrl = UtilityManager.GetAppKeyValue( "path.ContentOutputUrl", "/Content/" );
            path = baseUrl + relativePath + "/" + docFileName;

            return path;
        }//

        public static string DetermineMyImageUrl( int createdById, string docFileName )
        {
            return DetermineDocumentUrl( createdById, 0, "", docFileName );
        }//

        public static string GetRelativeUrlPath( ContentItem parentEntity )
        {
            //determine if the orgRowId will be used
            string orgRowId = GetOrgRowid( parentEntity );

            return GetRelativeUrlPath( parentEntity.CreatedById, parentEntity.OrgId, orgRowId );
        }//
        public static string GetRelativeUrlPath( int createdById, int orgId, string orgRowId )
        {
            string path = "";

            string orgPart = "";
            string userPart = "/" + createdById.ToString();
            if ( orgId > 0 )
            {
                if ( FormHelper.IsValidRowId( orgRowId ) )
                    orgPart = orgRowId;
                else
                    orgPart = orgId.ToString();
            }
            else
            {
                orgPart = "Personal";
            }
            path = orgPart + userPart;
            return path;

        }

        public static string GetOrgRowid( ContentItem parentEntity )
        {
            if ( parentEntity == null || parentEntity.Id == 0)
                return "";

            //determine if the orgRowId will be used
            string orgRowId = "";
            if ( parentEntity.OrgId > 0 )
            {
                if ( parentEntity.HasOrg() )
                {
                    if ( parentEntity.ContentOrg != null && parentEntity.ContentOrg.HasValidRowId() )
                    {
                        orgRowId = parentEntity.ContentOrg.RowId.ToString();
                    }
                }
                else
                {
                    //should retrieve just in case
                    Organization org = OrganizationBizService.EFGet( parentEntity.OrgId );
                    if ( org != null && org.Id > 0)
                        orgRowId = org.RowId.ToString();
                }
            }

            return orgRowId;
        }//
        /// <summary>
        /// Return url for displaying a document
        /// </summary>
        /// <param name="relPath"></param>
        /// <param name="doc"></param>
        public static string SetContentItemUrl( string relPath, string fileName )
        {
            string baseUrl = UtilityManager.GetAppKeyValue( "path.ContentOutputUrl", "/Content/" );

            return baseUrl + relPath + "/" + fileName;
        }//

        /// <summary>
        /// Return url for displaying a policy version
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string GetContentItemUrl( ContentItem parentEntity, IDocument doc )
        {
            string url = "";
            string baseUrl = UtilityManager.GetAppKeyValue( "path.ContentOutputUrl", "/Content/" );
            //will resource url be relative or absolute??
            if ( doc.ResourceUrl.ToLower().IndexOf( baseUrl.ToLower() ) == -1 )
                url = baseUrl + doc.ResourceUrl;
            else
                url = doc.ResourceUrl;
            //???????????????????
            //string url = "/Content/" + "????" + "/version_" + "/" + doc.FileName;
            return url;
        }

        public static string GetOrganizationDocumentsPath( ContentItem parentEntity, IDocument doc )
        {


            //TBD
            //have to consider organization, and perhaps a hierarchy??
            //user may not always be relevent?????????????
            //====> probably has to be stored somewhere

            string path = "";
            string orgPath = "org\\" + parentEntity.CreatedById.ToString();
            path = GetContentPath( orgPath );
            return path;
        }//

        #endregion


        #region File system methods
        //in FileSystemHelper


        #endregion
    }
}