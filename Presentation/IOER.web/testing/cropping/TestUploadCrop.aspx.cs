using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using SD = System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

using ILPathways.Controllers;
using ILPathways.Business;

namespace JqueryDevelopment.Projects.ImageResizerSamples
{
	public partial class TestUploadCrop : System.Web.UI.Page
	{
        static int tempUserId = 2;

        //future point to external folder
        static string workFolder = "temp";
        static string path = HttpContext.Current.Request.PhysicalApplicationPath + workFolder + "\\";
        static string workUrl= "/temp";
        static string workCroppedUrl = "/temp";
        static string workResizedUrl = "/temp";
        static int maxWidth = 150;
        bool setToMaxWidth = true;

		protected void Page_Load(object sender, EventArgs e)
		{

		}

        protected void btnUpload_Click( object sender, EventArgs e )
        {
            Boolean FileOK = false;
            Boolean FileSaved = false;
            //use ioer structure
            imagePath.Text = FileResourceController.DetermineDocumentPath( tempUserId, 0 ) + "\\" ;
            

            if ( Upload.HasFile )
            {
                Session[ "WorkingImage" ] = Upload.FileName;
                workUrl = FileResourceController.DetermineDocumentUrl( tempUserId, 0, Upload.FileName );
                workCroppedUrl = FileResourceController.DetermineDocumentUrl( tempUserId, 0, "cropped_" + Upload.FileName );
                workResizedUrl = workCroppedUrl.Replace( "cropped_", "resized_" );


                String FileExtension = Path.GetExtension( Session[ "WorkingImage" ].ToString() ).ToLower();
                String[] allowedExtensions = { ".png", ".jpeg", ".jpg", ".gif" };
                for ( int i = 0 ; i < allowedExtensions.Length ; i++ )
                {
                    if ( FileExtension == allowedExtensions[ i ] )
                    {
                        FileOK = true;
                    }
                }
            }

            if ( FileOK )
            {
                try
                {
                    //handle image size
                    ImageStore img = new ImageStore();
                    img.ImageFileName = Upload.FileName;
                    //size to a manageble size for cropping
                    FileResourceController.HandleImageResizingToWidth( img, Upload, 600, 600, true, true, imagePath.Text );

                    //Upload.PostedFile.SaveAs( path + Session[ "WorkingImage" ] );
                    FileSaved = true;
                }
                catch ( Exception ex )
                {
                    lblError.Text = "File could not be uploaded." + ex.Message.ToString();
                    lblError.Visible = true;
                    FileSaved = false;
                }
            }
            else
            {
                lblError.Text = "Cannot accept files of this type.";
                lblError.Visible = true;
            }

            if ( FileSaved )
            {
                pnlUpload.Visible = false;
                pnlCrop.Visible = true;
                imgCrop.ImageUrl = workUrl;// "/" + workFolder + "/" + Session[ "WorkingImage" ].ToString();
                previewImage.ImageUrl = workUrl; // "/" + workFolder + "/" + Session[ "WorkingImage" ].ToString();
                
            }
   }


        protected void btnCrop_Click( object sender, EventArgs e )
        {
            string imageName = Session[ "WorkingImage" ].ToString();
            int w = Convert.ToInt32( W.Value );
            int h = Convert.ToInt32( H.Value );
            int x = Convert.ToInt32( X.Value );
            int y = Convert.ToInt32( Y.Value );

            //set crop section first
            byte[] CropImage = Crop( imagePath.Text + imageName, w, h, x, y );
            using ( MemoryStream ms = new MemoryStream( CropImage, 0, CropImage.Length ) )
            {
                ms.Write( CropImage, 0, CropImage.Length );
                using ( SD.Image croppedImage = SD.Image.FromStream( ms, true ) )
                {
                    //now resize to max (or use two steps?)

                    SaveCroppedImage( imageName, croppedImage );

                    SaveResizedImage( imageName, croppedImage );
                    //croppedImage.Dispose();
                }
            }
        } 
        
        protected void btnSave_Click( object sender, EventArgs e )
        {
            //now resize the cropped image
            string imageName = Session[ "WorkingImage" ].ToString();
        }
 
        void SaveResizedImage( string imageName, SD.Image image )
        {
            string SaveTo = imagePath.Text + "resized_" + imageName;
            double scaleFactor = 1;
            if ( image.Width > maxWidth )
                scaleFactor = ( double ) maxWidth / ( double ) image.Width;
            else if ( image.Width < maxWidth && setToMaxWidth == true )
                scaleFactor = ( double ) maxWidth / ( double ) image.Width; //same but increases

            if ( scaleFactor != 1 )
            {
                ImageFormat imgFormat;
                imgFormat = image.RawFormat;
                int newWidth = ( int ) ( image.Width * scaleFactor );
                int newHeight = ( int ) ( image.Height * scaleFactor );

                SD.Bitmap thumbnailBitmap = new SD.Bitmap( newWidth, newHeight );
                SD.Graphics thumbnailGraph = SD.Graphics.FromImage( thumbnailBitmap );

                thumbnailGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbnailGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbnailGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;

                SD.Rectangle imageRectangle = new SD.Rectangle( 0, 0, newWidth, newHeight );

                thumbnailGraph.DrawImage( image, imageRectangle );

                //thumbnailBitmap.Save( toStream, imgFormat );
                thumbnailBitmap.Save( SaveTo, image.RawFormat );
                //thumbnailGraph.Dispose();
                //thumbnailBitmap.Dispose();
                //image.Dispose();
            }
            else
            {
                image.Save( SaveTo, image.RawFormat );
            }

            ImageStore entity = new ImageStore();
            entity.ImageFileName = imageName;
            FileStream fs = new FileStream( SaveTo, FileMode.Open, FileAccess.Read );
            entity.Bytes = fs.Length;
            byte[] data = new byte[ fs.Length ];
            fs.Read( data, 0, data.Length );
            fs.Close();
            fs.Dispose();
            entity.SetImageData( entity.Bytes, data );

            //image.Save( SaveTo, image.RawFormat );
            pnlCrop.Visible = false;
            pnlCropped.Visible = true;
            imgResized.ImageUrl = workResizedUrl;   // "/" + workFolder + "/" + "resized_" + imageName;
        }
        void SaveImageToDB( string imageName, SD.Image croppedImage )
        {

            string SaveTo = imagePath.Text + "resized" + imageName;
            ImageStore entity = new ImageStore();
            entity.ImageFileName = imageName;
            FileStream fs = new FileStream( SaveTo, FileMode.Open, FileAccess.Read );
            entity.Bytes = fs.Length;
            byte[] data = new byte[ fs.Length ];
            fs.Read( data, 0, data.Length );
            fs.Close();
            fs.Dispose();
            entity.SetImageData( entity.Bytes, data );
        }

        void SaveCroppedImage( string imageName, SD.Image croppedImage )
        {
            string SaveTo = imagePath.Text + "cropped_" + imageName;
            croppedImage.Save( SaveTo, croppedImage.RawFormat );
            pnlCrop.Visible = false;
            pnlCropped.Visible = true;
            imgCropped.ImageUrl = workCroppedUrl;// "/" + workFolder + "/" + "cropped_" + imageName;
            //croppedImage.Dispose();
        }

        static byte[] Crop( string imagePath, int Width, int Height, int X, int Y )
        {
            try
            {
                using ( SD.Image originalImage = SD.Image.FromFile( imagePath ) )
                {
                    using ( SD.Bitmap bmp = new SD.Bitmap( Width, Height ) )
                    {
                        bmp.SetResolution( originalImage.HorizontalResolution, originalImage.VerticalResolution );
                        using ( SD.Graphics Graphic = SD.Graphics.FromImage( bmp ) )
                        {
                            Graphic.SmoothingMode = SmoothingMode.AntiAlias;
                            Graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            Graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            Graphic.DrawImage( originalImage, new SD.Rectangle( 0, 0, Width, Height ), X, Y, Width, Height, SD.GraphicsUnit.Pixel );
                            MemoryStream ms = new MemoryStream();
                            bmp.Save( ms, originalImage.RawFormat );
                            return ms.GetBuffer();
                        }
                    }
                }
            }
            catch ( Exception Ex )
            {
                throw ( Ex );
            }
        }

        protected void btnNew_Click( object sender, EventArgs e )
        {
            pnlUpload.Visible = true;
            pnlCrop.Visible = false;
            pnlCropped.Visible = false;
        }

	}
}