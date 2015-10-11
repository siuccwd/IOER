using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using SD = System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

using IOER.Controllers;
using ILPathways.Business;
using ILPathways.Utilities;
using ILPlibrary = IOER.Library;

//using GDAL = Isle.BizServices;
//using ThisLibrary = ILPathways.Business.Library;
//using MyManager = Isle.BizServices.LibraryBizService;
//using ContentManager = Isle.BizServices.ContentServices;


namespace IOER.LRW.controls
{
    public partial class ImageHelper : ILPlibrary.BaseUserControl
    {

        const string thisClassName = "ImageHelper";

        //future point to external folder
        static string workFolder = "temp";
        string path = HttpContext.Current.Request.PhysicalApplicationPath + workFolder + "\\";
        static int maxWidth = 150;
        bool setToMaxWidth = true;
        //need folder that can be shown in url

        static string croppedPrefix = "cropped_";
        static string resizedPrefix = "resized_";

        #region Properties
        public string WorkImageName
        {
            get { return txtWorkImageName.Text; }
            set { txtWorkImageName.Text = value; }
        }
        /// <summary>
        /// WidthHeightRatio - defaults to 1 (square)
        /// </summary>
        public decimal WidthHeightRatio
        {
            get { return  decimal.Parse( this.txtWidthHeightRatio.Text); }
            set { this.txtWidthHeightRatio.Text = value.ToString(); }
        }

        public string DocumentVersionId
        {
            get { return this.txtDocumentVersionId.Text; }
            set { this.txtDocumentVersionId.Text = value; }
        }
        /// <summary>
        /// TargetWidth - max width (after cropping)
        /// </summary>
        public int TargetWidth
        {
            get { return Int32.Parse( txtTargetWidth.Text ); }
            set { txtTargetWidth.Text = value.ToString(); }
        }

        /// <summary>
        /// UsingExactWidth - if true, then smaller images will be increased to match the target width
        /// </summary>
        public bool UsingExactWidth
        {
            get { return bool.Parse( txtUsingExactWidth.Text ); }
            set { txtUsingExactWidth.Text = value.ToString(); }
        }
        /// <summary>
        /// WorkPath - appkey that points to the server folder for work
        /// Probably not necessary to change, so not exposing as an option
        /// </summary>
        public string WorkPathKey
        {
            get { return txtWorkPathKey.Text; }
            set { txtWorkPathKey.Text = value; }
        }
        public string WorkPath
        {
            get { return txtWorkPath.Text; }
            set { txtWorkPath.Text = value; }
        }
        public string WorkPathUrl
        {
            get { return txtWorkPathUrl.Text; }
            set { txtWorkPathUrl.Text = value; }
        }
        /// <summary>
        /// DestinationPath - if used, would be full path to prod location
        /// Issue will if using for preview, then could wipe out an existing image (even for current user). Need mechanism to avoid collisions! Perference may be to always render small images from database
        /// </summary>
        public string DestinationPath
        {
            get { return txtDestinationPath.Text; }
            set { txtDestinationPath.Text = value; }
        }

        #endregion
        /// <summary>
        /// Handle Page Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void Page_Load( object sender, EventArgs ex )
        {
            if ( IsUserAuthenticated() )
            {
                //probably only allow for authenticated?
                CurrentUser = GetAppUser();
            }
            else
            {
                formMessage.Text = "Error - you must be logged in and authorized to use this function.";
                contentPanel.Enabled = false;
                return;
            }
            if ( Page.IsPostBack == false )
            {
                this.InitializeForm();
            }
        }//

        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        private void InitializeForm()
        {

            TargetWidth = this.GetRequestKeyValue( "width", 0 );
            //check for a doc version rowid
            string rid = this.GetRequestKeyValue( "rid", "" );
            //check for a ratio
            string ratio = this.GetRequestKeyValue( "ratio", "" );
            if ( FormHelper.IsNumeric(ratio) )
            {
                WidthHeightRatio = decimal.Parse( ratio );
            }
            //check if filling width
            UsingExactWidth = FormHelper.GetRequestKeyValue( "fillWidth", true );

            WorkPath = UtilityManager.GetAppKeyValue( "path.WorkOutputPath" );
            //make sure path exists
            FileSystemHelper.CreateDirectory( WorkPath );

            WorkPathUrl = UtilityManager.GetAppKeyValue( "path.WorkOutputUrl" ); 

            //hmm handling destination? Easier to fill entity and let caller handle, even if caching after fact.
            DestinationPath = UtilityManager.GetAppKeyValue( "path.ContentOutputPath" );
            //may need folders under destination path

            //need to do something so the jquery reads the current values
          
        }	// End 


        protected void btnUpload_Click( object sender, EventArgs e )
        {
            Boolean FileOK = false;
            Boolean FileSaved = false;

            if ( fileUpload.HasFile )
            {
                WorkImageName = CleanInput( fileUpload.FileName );
                String FileExtension = Path.GetExtension( WorkImageName ).ToLower();
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
                    img.ImageFileName = WorkImageName;
 
                    //size to a managable size for cropping
                    FileResourceController.HandleImageResizingToWidth( img, fileUpload, 600, 600, true, true, WorkPath );

                    //fileUpload.PostedFile.SaveAs( WorkPath + WorkImageName );
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
                lblError.Text = "Cannot accept files of this type, select a different file type.";
                lblError.Visible = true;
            }

            if ( FileSaved )
            {
                pnlUpload.Visible = false;
                pnlCrop.Visible = true;
                imgCrop.ImageUrl = WorkPathUrl + "/" + WorkImageName;
                if ( txtUsingPreview.Text == "yes")
                    previewImage.ImageUrl = WorkPathUrl + "/" + WorkImageName;

            }
        }
        protected void btnCrop_Click( object sender, EventArgs e )
        {
            string imageName = WorkImageName;
            int w = Convert.ToInt32( W.Value );
            int h = Convert.ToInt32( H.Value );
            int x = Convert.ToInt32( X.Value );
            int y = Convert.ToInt32( Y.Value );

            //set crop section first
            byte[] CropImage = Crop( WorkPath + imageName, w, h, x, y );
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
            //now save to destination or maybe just entity, or ???
            string sourceImage = WorkPath + resizedPrefix + WorkImageName;

            SaveImageToEntity( sourceImage, WorkImageName );

            //delete work files
            DeleteWorkfile( WorkPath + WorkImageName );
            DeleteWorkfile( WorkPath + croppedPrefix + WorkImageName );
            DeleteWorkfile( WorkPath + resizedPrefix + WorkImageName );

        }

        void DeleteWorkfile( string fileName )
        {
            if ( System.IO.File.Exists( fileName ) )
            {
                FileInfo fi = new FileInfo( fileName );
                fi.Delete();
            }
        }

        void SaveResizedImage( string imageName, SD.Image image )
        {
            string SaveTo = WorkPath + resizedPrefix + imageName;

            double scaleFactor = 1;
            if ( image.Width > TargetWidth )
                scaleFactor = ( double ) TargetWidth / ( double ) image.Width;
            else if ( image.Width < TargetWidth && UsingExactWidth == true )
                scaleFactor = ( double ) TargetWidth / ( double ) image.Width; //same but increases

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

       
            //image.Save( SaveTo, image.RawFormat );
            pnlCrop.Visible = false;
            pnlResized.Visible = true;
            imgResized.ImageUrl = WorkPathUrl + "/" + resizedPrefix + imageName;
        }
        void SaveImageToEntity( string sourceImage, string imageName )
        {

            ImageStore entity = new ImageStore();
            entity.ImageFileName = imageName;
            //read source image
            FileStream fs = new FileStream( sourceImage, FileMode.Open, FileAccess.Read );
            entity.Bytes = fs.Length;
            byte[] data = new byte[ fs.Length ];
            fs.Read( data, 0, data.Length );
            fs.Close();
            fs.Dispose();
            entity.SetImageData( entity.Bytes, data );
            //now ???
        }

        void SaveCroppedImage( string imageName, SD.Image croppedImage )
        {
            string SaveTo = WorkPath + croppedPrefix + imageName;
            croppedImage.Save( SaveTo, croppedImage.RawFormat );
            pnlCrop.Visible = false;
            pnlCropped.Visible = true;
            imgCropped.ImageUrl = WorkPathUrl + "/" + croppedPrefix + imageName;
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

        static string CleanInput( string strIn )
        {
            // Replace invalid characters with empty strings. 
            try
            {
                return Regex.Replace( strIn, @"[^\w\.@-]", "" ); 
            }
            // If we timeout when replacing invalid characters,  
            // we should return Empty. 
            //catch  ( RegexMatchTimeoutException rex )
            //{
            //    return String.Empty;
            //}
             catch  (Exception ex)
            {
                return String.Empty;
            }
        }
        public string ParseAllLettersOrDigitsOrUnderscores(string s)
        {
            string result = "";
            foreach (char c in s)
            {
                if (Char.IsLetterOrDigit(c) && c != '_')
                    result += c;
            }
            return result;
        }
        public bool IsAlphaNumericWithUnderscore( string input )
        {
            return Regex.IsMatch( input, "^[a-zA-Z0-9_]+$" );
        }
    }
}