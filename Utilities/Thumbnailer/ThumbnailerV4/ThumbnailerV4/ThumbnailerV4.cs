using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

using System.IO;
using System.Drawing;
using System.Drawing.Imaging;


namespace ThumbnailerV4
{
  public class ThumbnailerV4
  {
      /*
      ChromeOptions options = new ChromeOptions();
      options.AddArgument( "--window-size=1040,857" );

      //Load the browser
      ChromeDriver driver = new ChromeDriver( options );

      //Navigate to the page
      driver.Navigate().GoToUrl( "http://www.youtube.com/" );
      
      //Wait extra time for the page to load
      System.Threading.Thread.Sleep( 10000 );
      
      //Take screenshot
      Screenshot shot = ( ( ITakesScreenshot ) driver ).GetScreenshot();
      Image img = Image.FromStream( new MemoryStream( shot.AsByteArray ) );
      shot.SaveAsFile( "raw.png", ImageFormat.Png );
      new Bitmap( img, new Size( 400, 300 ) ).Save( "large.png", ImageFormat.Png );
      new Bitmap( img, new Size( 200, 150 ) ).Save( "thumb.png", ImageFormat.Png );

      //Exit
      driver.Quit();
       */

    private string ThumbnailRootFolder = "";

    public void MakeThumbnails( string url, string name, string thumbnailRootFolder, bool overwriteIfExists, bool useNormalProportions )
    {
      var data = new List<ThumbnailParams>() {
        new ThumbnailParams() {
          url = url,
          name = name
        } 
      };
      MakeThumbnailsFromList( data, thumbnailRootFolder, overwriteIfExists, useNormalProportions );
    }

    public void MakeThumbnailsFromList( List<ThumbnailParams> data, string thumbnailRootFolder, bool overwriteIfExists, bool useNormalProportions )
    {
      ThumbnailRootFolder = thumbnailRootFolder;
      LogLine( "Starting as " + System.Security.Principal.WindowsIdentity.GetCurrent().Name + "..." + System.Environment.NewLine, true );

      ChromeOptions options = new ChromeOptions();
      if ( useNormalProportions )
      {
        //When run by code, the virtual environment is 1024x768. Starting in kiosk mode hides the browser's chrome
        options.AddArgument( "--kiosk" );
      }
      else
      {
        options.AddArgument( "--window-size=1040,857" );
      }

      //Load the browser
      LogLine( "Loading browser...", false );
      ChromeDriver driver = new ChromeDriver( options );

      var count = 1;
      LogLine( "Examining Data...", false );
      foreach ( ThumbnailParams item in data )
      {
        try
        {
          double percent = ( ( ( double ) count / data.Count ) * 100 );
          Console.WriteLine( "Processing " + item.name + " ( " + count + " of " + data.Count + ": " + ( Math.Truncate( percent * 100 ) / 100 ) + "% )" );
          LogLine( "Processing: " + item.name + ": " + item.url, false );
          count++;
          var largeName = thumbnailRootFolder + "large/" + item.name + "-large.png";
          try
          {
            File.ReadAllBytes( largeName );
          }
          catch ( Exception ex )
          {
            LogLine( "Unable to read existing image file: " + ex.Message.ToString(), false );
          }
          if ( !overwriteIfExists && File.Exists( largeName ) )
          {
            LogLine( "File already exists: " + item.name, false );
            continue;
          }

          //Navigate to the page
          LogLine( "Navigating... " + item.name + ": " + item.url, false );
          try
          {
            driver.Navigate().GoToUrl( item.url );
            ( ( IJavaScriptExecutor ) driver ).ExecuteScript( "window.confirm = function(msg) { return false; }" );
            ( ( IJavaScriptExecutor ) driver ).ExecuteScript( "window.prompt = function(msg) { return false; }" );
            ( ( IJavaScriptExecutor ) driver ).ExecuteScript( "window.alert = function(msg) { return true; }" );
          }
          catch ( UnhandledAlertException )
          {
            IAlert alert = driver.SwitchTo().Alert();
            alert.Dismiss();
          }

          LogLine( "Navigated to site: " + item.name + ": " + item.url, false );

          //Wait extra time for the page to load
          LogLine( "Sleeping..." + item.name + ": " + item.url, false );
          System.Threading.Thread.Sleep( 10000 );
          //driver.Keyboard.SendKeys( Keys.F11 );

          //Take screenshot
          LogLine( "Taking screenshot: " + item.name + ": " + item.url, false );
          LogLine( "Image Name: " + largeName, false );
          Image img = Image.FromStream( new MemoryStream() );
          try
          {
            Screenshot shot = ( ( ITakesScreenshot ) driver ).GetScreenshot();
            img = Image.FromStream( new MemoryStream( shot.AsByteArray ) );
            new Bitmap( img, new Size( 400, 300 ) ).Save( largeName, ImageFormat.Png );
            //new Bitmap( img, new Size( 200, 150 ) ).Save( thumbName, ImageFormat.Png );
            //shot.SaveAsFile( "test.png", ImageFormat.Png );
            LogLine( "Screenshotted: " + item.name + ": " + item.url, false );

          }
          catch ( Exception ex )
          {
            try
            {
              new Bitmap( img, new Size( 400, 300 ) ).Save( @"C:\Thumbnail Generator 4\ToCopy\" + item.name + "-large.png", ImageFormat.Png );
            }
            catch { }
            LogLine( "Error taking screenshot: " + item.name + ": " + item.url + ":", false );
            LogLine( "Root Path: " + ThumbnailRootFolder, false );
            LogLine( ex.Message.ToString(), false );
            
          }
        }
        catch ( Exception ex )
        {
          //Need to make sure the thumbnailer gets through the list even if some items fail, no matter what
          LogLine( "Error: " + ex.Message.ToString(), false );
        }
      }

      //Exit
      driver.Quit();
    }

    public void LogLine( string line, bool overwrite )
    {
      try
      {
        if ( overwrite )
        {
          File.WriteAllText( @"C:\Thumbnail Generator 4\lastrun.txt", line + System.Environment.NewLine );
          File.WriteAllText( ThumbnailRootFolder + "lastrun.txt", line + System.Environment.NewLine );
        }
        else
        {
          File.AppendAllText( @"C:\Thumbnail Generator 4\lastrun.txt", line + System.Environment.NewLine );
          File.AppendAllText( ThumbnailRootFolder + "lastrun.txt", line + System.Environment.NewLine );
        }
      }
      catch ( Exception ex )
      {
        Console.WriteLine( line );
        File.AppendAllText( @"C:\Thumbnail Generator 4\lastrun.txt", "Current Windows Identity: " + System.Security.Principal.WindowsIdentity.GetCurrent().Name + System.Environment.NewLine );
        File.AppendAllText( @"C:\Thumbnail Generator 4\lastrun.txt", ex.Message.ToString() + System.Environment.NewLine );
        try
        {
          File.ReadAllText( ThumbnailRootFolder + "lastrun.txt" );
        }
        catch ( Exception ex2 )
        {
          File.AppendAllText( @"C:\Thumbnail Generator 4\lastrun.txt", "Could not read from foreign lastrun.txt file" + System.Environment.NewLine );
          File.AppendAllText( @"C:\Thumbnail Generator 4\lastrun.txt", ex2.Message.ToString() + System.Environment.NewLine );
        }
      }
    }

    public class ThumbnailParams
    {
      public string url { get; set; }
      public string name { get; set; }
    }

  }
}


