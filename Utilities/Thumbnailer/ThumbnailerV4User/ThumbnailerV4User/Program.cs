using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Data;
using ThumbnailerV4;
using System.Web.Script.Serialization;

using LRWarehouse.DAL;
using ILPathways.Common;

namespace ThumbnailerV4User
{
  class Program
  {
    static void Main( string[] args )
    {
      try
      {
        //Read settings from file
        var serializer = new JavaScriptSerializer();
        var settings = serializer.Deserialize<Settings>( File.ReadAllText( "settings.json" ) );
        var thumbnailer = new ThumbnailerV4.ThumbnailerV4();

        //If arguments, do single mode
        if ( args.Length > 0 )
        {
          var name = args[ 0 ];
          var url = args[ 1 ];

          thumbnailer.MakeThumbnails( url, name, settings.thumbnailRootFolder, settings.overwriteIfExists, settings.useNormalProportions );
        }
        //Otherwise, do batch mode
        else
        {
          var ParamsList = new List<ThumbnailerV4.ThumbnailerV4.ThumbnailParams>();
          if ( settings.mode == "file" )
          {
            var data = File.ReadAllLines( "input.txt" );
            foreach ( string item in data )
            {
              var info = item.Split( '	' );
              var thisParam = new ThumbnailerV4.ThumbnailerV4.ThumbnailParams()
              {
                name = info[ 0 ],
                url = info[ 1 ]
              };
              ParamsList.Add( thisParam );
            }
          }
          else if ( settings.mode == "database" )
          {
            var dateString = DateTime.Now.AddDays( settings.lastNDays * -1 ).ToShortDateString();
            string sql = "SELECT [ResourceUrl], Id FROM [Resource] WHERE [Created] > '" + dateString + "' ORDER BY [LastUpdated] desc";
            DataSet ds = LRWarehouse.DAL.DatabaseManager.DoQuery( sql );

            foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
            {
              var thisParam = new ThumbnailerV4.ThumbnailerV4.ThumbnailParams();
              thisParam.name = DatabaseManager.GetRowColumn( dr, "Id" );
              thisParam.url = DatabaseManager.GetRowColumn( dr, "ResourceUrl" );
              ParamsList.Add( thisParam );
            }
          }

          //if ( ParamsList.Count() > 100 )
          if ( false )
          {
            //Do parallel processing
            new ThreadedThumbnailMaker().MakeThreadedThumbnails( ParamsList, settings.thumbnailRootFolder, settings.overwriteIfExists, settings.useNormalProportions );
          }
          else
          {
            thumbnailer.MakeThumbnailsFromList( ParamsList, settings.thumbnailRootFolder, settings.overwriteIfExists, settings.useNormalProportions );
          }
        }
      }
      catch
      {
        //Catch any error and attempt to kill any stray processes to ensure the program doesn't cause memory leaks
        try
        {
          foreach ( var process in System.Diagnostics.Process.GetProcessesByName( "chromedriver" ) )
          {
            process.Kill();
          }
        }
        catch { }
        try
        {
          foreach ( var process in System.Diagnostics.Process.GetProcessesByName( "chrome" ) )
          {
            process.Kill();
          }
        }
        catch { }
        //ensure this program, at least, exits no matter what
      }
    }

    public class ThreadedThumbnailMaker
    {
      public void MakeThreadedThumbnails( List<ThumbnailerV4.ThumbnailerV4.ThumbnailParams> ParamsList, string thumbnailRootFolder, bool overwriteIfExists, bool useNormalProportions )
      {
        var lists = new List<ThumbnailerV4.ThumbnailerV4.ThumbnailParams>[ 6 ];
        for ( var track = 0 ; track < 6 ; track++ )
        {
          lists[ track ] = new List<ThumbnailerV4.ThumbnailerV4.ThumbnailParams>();
        }
        var chunk = ( int ) Math.Ceiling( ( decimal ) ( ParamsList.Count() / 6 ) );
        var count = 0;
        var currentArray = 0;
        foreach ( ThumbnailerV4.ThumbnailerV4.ThumbnailParams par in ParamsList )
        {
          lists[ currentArray ].Add( par );
          count++;
          if ( count > chunk )
          {
            currentArray++;
            count = 0;
          }
        }

        foreach ( List<ThumbnailerV4.ThumbnailerV4.ThumbnailParams> item in lists )
        {
          var sendData = new asyncObject()
          {
            data = item,
            thumbnailRootFolder = thumbnailRootFolder,
            overwriteIfExists = overwriteIfExists,
            useNormalProportions = useNormalProportions
          };
          System.Threading.ThreadPool.QueueUserWorkItem( MakeThumbs, sendData );
        }
      }

      public void MakeThumbs( object settings )
      {
        var info = (asyncObject) settings;
        new ThumbnailerV4.ThumbnailerV4().MakeThumbnailsFromList( info.data, info.thumbnailRootFolder, info.overwriteIfExists, info.useNormalProportions );
      }
    }

    public class asyncObject
    {
      public List<ThumbnailerV4.ThumbnailerV4.ThumbnailParams> data;
      public string thumbnailRootFolder;
      public bool overwriteIfExists;
      public bool useNormalProportions;
    }

    public class Settings
    {
      public int lastNDays { get; set; }
      public bool overwriteIfExists { get; set; }
      public string thumbnailRootFolder { get; set; }
      public string mode { get; set; }
      public bool useNormalProportions { get; set; }
    }
  }
}
