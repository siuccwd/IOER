using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.IO;
using ILPathways.Utilities;


namespace LRWarehouse.DAL
{
  public class ResourceThumbnailManagerOld
  {

      public ResourceThumbnailManagerOld()
    {
      thumbnailerPath = UtilityManager.GetAppKeyValue( "thumbnailGeneratorV2Folder" );
      cachedImagesURL = UtilityManager.GetAppKeyValue( "cachedImagesUrl", "http://209.175.164.200/OERThumbs/" );
    }
    string thumbnailerPath;
    string cachedImagesURL;

    #region Create Methods

    //Create a thumbnail if it doesn't exist
    public void CreateThumbnail( int intID, string URL )
    {
			if ( UtilityManager.GetAppKeyValue( "envType" ) == "prod" )
			{
				CallThumbnailerV4( intID.ToString(), URL );
			}
    }
    public void CreateThumbnail( int resourceID, string URL, bool overwriteIfExists )
    {
        if ( UtilityManager.GetAppKeyValue( "envType" ) == "prod" )
        {
            CallThumbnailerV4( resourceID.ToString(), URL );
        }
    }

    //Create a thumbnail (only if it doesn't exist)
    public void CreateThumbnail( int intID, string URL, bool overwriteIfExists, ref bool successful, ref string status, ref string thumbnailURL, ref string largeThumbnailURL )
    {
      successful = true;
      status = "okay";

      //Call the batch file on \\STAGE and send it the URL, intID, and a boolean to indicate whether or not to check for an existing pair of thumbnails
      //Suspended usage of this for now, due to IE security settings making it unworkable
      //CallGeneratorBatchFile( intID, URL, overwriteIfExists, ref successful, ref status );

      //Call the original Thumbnailer
      if ( UtilityManager.GetAppKeyValue( "envType" ) == "prod" )
      {
        CallThumbnailerV1( intID, URL, ref successful, ref status );
      }

      if ( successful )
      {
        thumbnailURL = cachedImagesURL + "thumb/" + intID + "-thumb.png";
        largeThumbnailURL = cachedImagesURL + "large/" + intID + "-large.png";
      }
      else
      {
        LoggingHelper.LogError( "Error generating thumbnail: " + status );
      }
    }

    #endregion
    #region Asynchronous Create Methods
    //Convenience method/wrapper for async method
    public void CreateThumbnailAsynchronously( int intID, string url, bool overwriteIfExists )
    {
      var input = new ThumbnailInputs()
      {
        intID = intID,
        URL = url,
        overwriteIfExists = overwriteIfExists
      };
      LoggingHelper.DoTrace( 5, "Beginning Asynchronous thumbnail Generation..." );
      System.Threading.ThreadPool.QueueUserWorkItem( new ResourceThumbnailManagerOld().CreateThumbnail, input );
    }
    //Method intended for use asynchronously
    public void CreateThumbnail( dynamic inputs )
    {
      bool successful = false;
      string status = "";
      string url1 = "";
      string url2 = "";
      CreateThumbnail( inputs.intID, inputs.URL, inputs.overwriteIfExists, ref successful, ref status, ref url1, ref url2 );
    }
    //For use with the above method--handy for async stuff
    public class ThumbnailInputs
    {
      public int intID { get; set; }
      public string URL { get; set; }
      public bool overwriteIfExists { get; set; }
    }
    #endregion

    #region Get Methods
    //Get a thumbnail
    public void GetThumbnail( int intID, ref string thumbnailURL, ref string largeThumbnailURL )
    {
      string status = "";
      bool successful = true;
      GetThumbnail( intID, false, "", ref successful, ref status, ref thumbnailURL, ref largeThumbnailURL );
    }
    public void GetThumbnail( int intID, bool createIfNotExists, string createURL, ref bool createSuccessful, ref string createStatus, ref string thumbnailURL, ref string largeThumbnailURL  )
    {
      //if ( File.Exists( @"\\STAGE\OER Thumbnails\large\" + intID + "-large.png" ) && File.Exists( @"\\STAGE\OER Thumbnails\thumb\" + intID + "-thumb.png" ) )
      if( File.Exists( thumbnailerPath + @"large\" + intID + "-large.png" ) && File.Exists( thumbnailerPath + @"thumb\" + intID + "-thumb.png" ) )
      {
        thumbnailURL = cachedImagesURL + "thumb/" + intID + "-thumb.png";
        largeThumbnailURL = cachedImagesURL + "large/" + intID + "-large.png";
      }
      else if ( createIfNotExists && createURL.Length > 0 )
      {
        CreateThumbnail( intID, createURL, false, ref createSuccessful, ref createStatus, ref thumbnailURL, ref largeThumbnailURL );
      }
    }
    #endregion

    #region Helper Methods
    private void CallThumbnailerV1( int intID, string url, ref bool successful, ref string status )
    {
      try
      {
        successful = false;
        int exitCode;
        ProcessStartInfo processInfo;
        Process process;
        url = "\"" + url + "\"";

        //processInfo = new ProcessStartInfo( "cmd.exe", "/c " + command );
        processInfo = new ProcessStartInfo( @"""C:\Thumbnail Generator\ThumbnailUpdater.exe""", "/single " + intID + " " + url );
        processInfo.CreateNoWindow = true;
        processInfo.UseShellExecute = false;
        processInfo.RedirectStandardError = true;
        processInfo.RedirectStandardOutput = true;

        process = Process.Start( processInfo );

        process.WaitForExit();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        exitCode = process.ExitCode;

        status = "STATUS: " + output + "; ERROR: " + error;
        process.Close();
        successful = ( exitCode == 0 );
      }
      catch ( Exception ex )
      {
        status = "There was an error processing your request: " + ex.Message.ToString();
        successful = false;
      }
    }

    private void CallGeneratorBatchFile( int intID, string url, bool overwriteIfExists, ref bool successful, ref string status )
    {
      try
      {
        successful = false;
        int exitCode;
        ProcessStartInfo processInfo;
        Process process;
        url = "\"" + url + "\"";

        //processInfo = new ProcessStartInfo( "cmd.exe", "/c " + command );
        processInfo = new ProcessStartInfo( thumbnailerPath + "makethumb.bat", url + " " + intID + " " + overwriteIfExists.ToString().ToLower() );
        processInfo.CreateNoWindow = true;
        processInfo.UseShellExecute = false;
        processInfo.RedirectStandardError = true;
        processInfo.RedirectStandardOutput = true;

        process = Process.Start( processInfo );

        process.WaitForExit();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        File.AppendAllText( thumbnailerPath + "output.txt", output + System.Environment.NewLine + System.Environment.NewLine );
        File.AppendAllText( thumbnailerPath + "error.txt", error + System.Environment.NewLine + System.Environment.NewLine );

        exitCode = process.ExitCode;

        status = "STATUS: " + output + "; ERROR: " + error;
        process.Close();
        successful = true;
      }
      catch ( Exception ex )
      {
        status = "There was an error processing your request: " + ex.Message.ToString();
        successful = false;
      }
    }

    private void CallThumbnailerV4()
    {
      CallThumbnailerV4( "" );
    }

    private void CallThumbnailerV4( string name, string url )
    {
      CallThumbnailerV4( name + " \"" + url + "\"" );
    }

    private void CallThumbnailerV4( string arguments )
    {
      var nl = System.Environment.NewLine;
      var log = @"C:\Thumbnail Generator 4\lastrun.txt";
      File.WriteAllText( log, "Starting..." + nl );

      try
      {
        int exitCode;
        ProcessStartInfo processInfo;
        Process process;

        //processInfo = new ProcessStartInfo( "cmd.exe", "/c " + command );
        if ( arguments.Length == 0 )
        {
          processInfo = new ProcessStartInfo( @"C:\Thumbnail Generator 4\ThumbnailerV4User.exe" );
        }
        else
        {
          processInfo = new ProcessStartInfo( @"C:\Thumbnail Generator 4\ThumbnailerV4User.exe", arguments );
        }
        processInfo.WorkingDirectory = @"C:\Thumbnail Generator 4";
        processInfo.CreateNoWindow = false;
        processInfo.UseShellExecute = true;

        File.AppendAllText( log, "ProcessInfo setup." + nl );
        File.AppendAllText( log, "Thumbnailer exists: " + File.Exists( @"C:\Thumbnail Generator 4\ThumbnailerV4User.exe" ) + nl );

        process = Process.Start( processInfo );

        File.AppendAllText( log, "Process started." + nl );

        process.WaitForExit( 20000 );

        File.AppendAllText( log, "Moving on..." + nl );

        process.Close();
        File.AppendAllText( log, "Finished." + nl + nl + nl );
      }
      catch ( Exception ex )
      {
        File.WriteAllText( @"C:\Thumbnail Generator 4\lastrun.txt", ex.Message.ToString() );
      }

    }

    public void DetermineThumbnails( int intID, ref string thumb, ref string large )
    {
      var url = new ResourceVersionManager().GetByResourceId( intID ).ResourceUrl;
      DetermineThumbnails( intID, url, ref thumb, ref large );
    }

    public void DetermineThumbnails( int intID, string url, ref string thumb, ref string large )
    {
      var thumbnailRoot = "/OERThumbs/";
      thumb = thumbnailRoot + "thumb/" + intID + "-thumb.png";
      large = thumbnailRoot + "large/" + intID + "-large.png";

      var fileThumbRoot = "/images/icons/filethumbs/";
      var fileTypes = new Dictionary<string, string>()
      {
        { ".pdf", "filethumb_pdf_" },
        { ".doc", "filethumb_docx_" },
        { ".ppt", "filethumb_pptx_" },
        { ".xls", "filethumb_xlsx_" }
      };

      foreach ( KeyValuePair<string, string> item in fileTypes )
      {
        if ( url.IndexOf( item.Key ) > -1 )
        {
          thumb = fileThumbRoot + item.Value + "200x150.png";
          large = fileThumbRoot + item.Value + "400x300.png";
        }
      }
    }
    
    #endregion
   
  }
}
