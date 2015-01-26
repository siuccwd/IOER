using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILPathways.Utilities;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace LRWarehouse.DAL
{
  public class ResourceThumbnailManager
  {
    //The UNC Path to the thumbnail folder -- should add this to web.config
    private const string thumbnailRootFolder = @"\\OERDATASTORE\OerThumbs\";

    //Create a single thumbnail 
    public void CreateThumbnail( int resourceID, string url )
    {
      CreateThumbnail( resourceID, url, false );
    }
    //Create a single thumbnail
    public void CreateThumbnail( int resourceID, string url, bool overwriteIfExists )
    {
      if ( UtilityManager.GetAppKeyValue( "envType" ) != "prod" ) { return; }
      CreateThumbnailIndirectly( resourceID, url, overwriteIfExists );
    }

    public void CreateThumbnailIndirectly( int resourceID, string url, bool overwriteIfExists )
    {
      CreateThumbnailIndirectly( resourceID.ToString(), url, overwriteIfExists );
    }
    public void CreateThumbnailIndirectly( string id, string url, bool overwriteIfExists )
    {
      var arguments = id + " \"" + url + "\" " + ( overwriteIfExists ? "true" : "false" );

      try
      {
        File.WriteAllText( @"C:\Thumbnail Generator 4\lastrun.txt", "Running thumbnailer indirectly..." + System.Environment.NewLine );

        ProcessStartInfo processInfo;
        Process process;

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

        process = Process.Start( processInfo );
        process.WaitForExit( 20000 );
        process.Close();
      }
      catch ( Exception ex )
      {
        File.WriteAllText( @"C:\Thumbnail Generator 4\lastrun.txt", "Project Level Error: " + ex.Message.ToString() );
      }
    }

    //Create asynchronously
    public void CreateThumbnailAsync( string resourceID, string url, bool overwriteIfExists, int secondsToWaitBeforeReturn )
    {
      CreateThumbnailAsync( resourceID, url, overwriteIfExists );
      Thread.Sleep( secondsToWaitBeforeReturn * 1000 );
    }
    public void CreateThumbnailAsync( int resourceID, string url, bool overwriteIfExists )
    {
      CreateThumbnailAsync( resourceID.ToString(), url, overwriteIfExists );
    }
    public void CreateThumbnailAsync( string resourceID, string url, bool overwriteIfExists )
    {
      var data = new ThumbnailParams()
      {
        resourceID = resourceID,
        url = url,
        overwriteIfExists = overwriteIfExists
      };
      ThreadPool.QueueUserWorkItem( CreateThumbnailAsyncCaller, data );
    }
    private void CreateThumbnailAsyncCaller( Object input )
    {
      var data = ( ThumbnailParams ) input;
      CreateThumbnailIndirectly( data.resourceID, data.url, data.overwriteIfExists );
    }
    private class ThumbnailParams
    {
      public string resourceID { get; set; }
      public string url { get; set; }
      public bool overwriteIfExists { get; set; }
    }

  }
}
