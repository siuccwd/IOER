using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using ILPathways.Utilities;
using LRWarehouse.DAL;
using LRWarehouse.Business;
using System.Web.Script.Serialization;

namespace ILPathways.Services
{
  /// <summary>
  /// Summary description for UtilityService
  /// </summary>
  [WebService( Namespace = "http://tempuri.org/" )]
  [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
  [System.ComponentModel.ToolboxItem( false )]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  [System.Web.Script.Services.ScriptService]
  public class UtilityService : System.Web.Services.WebService
  {
    JavaScriptSerializer serializer = new JavaScriptSerializer();

    #region Validation Methods

    [WebMethod]
    public string ValidateURL( string text, bool mustBeNew )
    {
      bool isValid = false;
      string status = "";
      text = ValidateURL( text, mustBeNew, ref isValid, ref status );
      return serializer.Serialize
      (
        new
        {
          data = text,
          isValid = isValid,
          status = status
        }
      );
    }
    public string ValidateURL( string url, bool mustBeNew, ref bool isValid, ref string status )
    {
      //Do basic text validation
      url = ValidateText( url, 12, "Resource URL", ref isValid, ref status );
      if ( !isValid )
      {
        return url;
      }

      //Check for existing URL, if we care
      if ( mustBeNew )
      {
        var test = new ResourceVersionManager().GetByUrl( url );
        if ( test.Count > 0 )
        {
          var first = test.First<LRWarehouse.Business.ResourceVersion>();
          status = "Resource already exists in IOER: <a href=\"/IOER/" + first.Id + "/" + first.SortTitle.Replace( " ", "_" ) + "\">Click Here</a>";
          isValid = false;
          return "";
        }
      }

      //Check for basic URL formatting
      try
      {
        var testURL = new Uri( url );
        if ( 
          (url.IndexOf( "http://" ) != 0 && url.IndexOf( "https://" ) != 0 ) ||
          url.IndexOf(".") == -1
        )
        {
          throw new Exception();
        }
        
      }
      catch ( Exception ex )
      {
        status = "Improperly formatted URL.";
        isValid = false;
        return "";
      }

      //Set return values
      isValid = true;
      status = "okay";
      return url;
    }

    [WebMethod]
    public string ValidateText( string text, int minimumLength, string fieldTitle )
    {
      bool isValid = false;
      string status = "";
      text = ValidateText( text, minimumLength, fieldTitle, ref isValid, ref status );
      return serializer.Serialize(
        new
        {
          data = text,
          isValid = isValid,
          status = status
        }
      );
    }
    public string ValidateText( string text, int minimumLength, string fieldTitle, ref bool isValid, ref string status )
    {
      text = FormHelper.SanitizeUserInput( text );
      if ( text == "" )
      {
        status = "Invalid value(s) detected in " + fieldTitle;
        isValid = false;
        return text;
      }
      if ( BadWordChecker.CheckForBadWords( text ) )
      {
        status = "Inappropriate language detected in " + fieldTitle;
        isValid = false;
        return text;
      }

      text = text.Trim();

      if ( text.Length < minimumLength )
      {
        status = fieldTitle + " must be at least " + minimumLength + " character" + ( minimumLength == 1 ? "" : "s" ) + " long.";
        isValid = false;
        return text;
      }

      isValid = true;
      status = "okay";
      return text;
    }

    [WebMethod]
    public string ValidateLettersNumbers( string text, int minimumLength, string fieldTitle )
    {
      bool isValid = false;
      string status = "";
      text = ValidateLettersNumbers( text, minimumLength, fieldTitle, ref isValid, ref status, true );

      return serializer.Serialize
      (
        new
        {
          data = text,
          isValid = isValid,
          status = status
        }
      );
    }
    public string ValidateLettersNumbers( string text, int minimumLength, string fieldTitle, ref bool isValid, ref string status, bool allowSpaces )
    {
      //Basic validation
      text = ValidateText( text, minimumLength, fieldTitle, ref isValid, ref status );
      if ( !isValid )
      {
        return text;
      }

      //Ensure only Letters and Numbers
      var test = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
      test = ( allowSpaces ? test + " " : test );
      var nameParts = text.ToCharArray();

      foreach ( char item in nameParts )
      {
        if ( test.IndexOf( item ) == -1 )
        {
          isValid = false;
          status = "Invalid character(s) found in " + fieldTitle + ". You may only use " + ( allowSpaces ? "letters, numbers, and/or spaces" : "letters and/or numbers" ) + ".";
          break;
        }
      }

      return text;
    }

    [WebMethod]
    public string ValidateCBXL( List<int> ids, string fieldName )
    {
      bool isValid = true;
      string status = "";
      return serializer.Serialize(
        new
        {
          data = ids,
          isValid = isValid,
          status = status
        }
      );
    }
    public void ValidateCBXL( List<System.Web.UI.WebControls.ListItem> items, int minimumLength, string fieldName, ref bool isValid, ref string status )
    {
      if ( items.Count < minimumLength )
      {
        isValid = false;
        status = "You must select at least " + minimumLength + " item" + ( minimumLength == 1 ? "" : "s" ) + " from " + fieldName + ".";
      }
      else
      {
        isValid = true;
        status = "okay";
      }
    }

    [WebMethod]
    public string ValidateEmail( string text, bool mustBeNew )
    {
      bool isValid = false;
      bool alreadyExists = false;
      string status = "";
      text = ValidateEmail( text, ref isValid, ref status, ref alreadyExists );

      if ( alreadyExists && mustBeNew )
      {
        isValid = false;
        status = "That Email is already registered with our system.";
      }

      return serializer.Serialize(
        new
        {
          data = text,
          isValid = isValid,
          alreadyExists = alreadyExists,
          status = status
        }
      );
    }
    public string ValidateEmail( string text, ref bool isValid, ref string status, ref bool emailAlreadyExists )
    {
      //Do basic validation
      text = ValidateText( text, 6, "Email", ref isValid, ref status );
      if ( !isValid )
      {
        return text;
      }

      //Ensure the address is email formatted
      try
      {
        var test = new System.Net.Mail.MailAddress( text );
        isValid = true;
        status = "okay";
      }
      catch(Exception ex)
      {
        isValid = false;
        status = "Invalid Email Address.";
        return "";
      }

      //Check for existing email
      var testUser = new PatronManager().GetByEmail( text );
      emailAlreadyExists = ( testUser.IsValid && testUser.Id > 0 );

      return text;
    }

    [WebMethod]
    public string ValidatePassword( string text )
    {
      bool isValid = false;
      string status = "";
      bool hasBonus = false;
      text = ValidatePassword( text, ref isValid, ref status, ref hasBonus );
      return serializer.Serialize(
        new
        {
          data = text,
          isValid = isValid,
          hasBonus = hasBonus,
          status = status
        }
      );

    }
    public string ValidatePassword( string text, ref bool isValid, ref string status, ref bool hasBonus )
    {
      isValid = true;
      //Basic validation
      var bannedChars = @"<>\".ToCharArray();
      foreach ( char item in bannedChars )
      {
        if ( text.IndexOf( item ) >= 0 )
        {
          isValid = false;
          status = @"Password cannot contain <, >, or \";
          return "";
        }
      }

      //Ensure required password has at least one of each
      var tests = new char[][] {
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(),
        "abcdefghijklmnopqrstuvwxyz".ToCharArray(),
        "1234567890".ToCharArray()
      };
      foreach ( char[] items in tests )
      {
        var found = false;
        foreach ( char item in items )
        {
          if ( text.IndexOf( item ) >= 0 )
          {
            found = true;
            break;
          }
        }
        if ( !found )
        {
          isValid = false;
          break;
        }
      }

      //Bonus security
      var bonusChars = "~!@#$%^&*()_+-=[]{};:?|., ".ToCharArray();
      hasBonus = false;
      foreach ( char item in bonusChars )
      {
        if ( text.IndexOf( item ) >= 0 )
        {
          hasBonus = true;
          break;
        }
      }

      if ( !isValid )
      {
        status = "Password requires at least one of each: lowercase letter, uppercase letter, and number.";
      }
      else if ( hasBonus )
      {
        status = "Password is very secure.";
      }
      else
      {
        status = "Password is okay.";
      }

      return text;

    }

    [WebMethod]
    public string ValidateName( string text, string fieldTitle )
    {
      bool isValid = false;
      string status = "";
      text = ValidateName( text, 1, fieldTitle, ref isValid, ref status );
      return serializer.Serialize(
        new
        {
          data = text,
          isValid = isValid,
          status = status
        }
      );
    }
    public string ValidateName( string text, int minimumLength, string fieldTitle, ref bool isValid, ref string status )
    {
      text = text.Trim();

      text = FormHelper.SanitizeUserInput( text );
      if ( text == "" )
      {
        status = "Invalid character(s) in " + fieldTitle + ".";
        isValid = false;
        return text;
      }

      var test = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890&.- ";
      var nameParts = text.ToCharArray();

      foreach ( char item in nameParts )
      {
        if ( test.IndexOf( item ) == -1 )
        {
          isValid = false;
          status = "Invalid character(s) in " + fieldTitle + ".";
          return text;
        }
      }

      if ( text.Length < minimumLength )
      {
        isValid = false;
        status = fieldTitle + " requires at least " + minimumLength + " character" + ( minimumLength > 1 ? "s." : "." );
        return text;
      }

      isValid = true;
      status = "okay";
      return text;
    }

    #endregion

    #region Miscellaneous
    public int GetIntIDFromVersionID( int versionID )
    {
      return new ResourceManager().GetByVersion( versionID ).Id;
    }

    [WebMethod]
    public string GetThumbnail( int intID, string url )
    {
      var thumbURL = "";
      var largeThumbURL = "";
      var isValid = false;
      var status = "";

      //Not exposing a version of this to client-side that allows forcing the creation of thumbnails, to prevent potential spam/attacks/problems
      GetThumbnail( intID, url, false, ref isValid, ref status, ref thumbURL, ref largeThumbURL );

      //Fetch thumbnails
      return serializer.Serialize(
        new
        {
          thumbURL = thumbURL,
          largeThumbURL = largeThumbURL,
          isValid = isValid,
          status = status
        }
      );
    }
    public void GetThumbnail( int intID, string url, bool createIfNeeded, ref bool isValid, ref string status, ref string thumbURL, ref string largeThumbURL )
    {
      url = ValidateURL( url, false, ref isValid, ref status );
      if ( !isValid )
      {
        return;
      }
      string imgUrl = ContentHelper.GetAppKeyValue( "cachedImagesUrl", "//ioer.ilsharedlearning.org/OERThumbs/" );

      if ( ! new WebDALService().IsProduction() )
      {
        isValid = true;
        status = "Only Production Servers should create thumbnails!";
        thumbURL = imgUrl + "thumb/" + intID + "-thumb.png";
        largeThumbURL = imgUrl + "large/" + intID + "-large.png";
        return;
      }

      new ResourceThumbnailManager().GetThumbnail( intID, createIfNeeded, url, ref isValid, ref status, ref thumbURL, ref largeThumbURL );
    }

    public Patron GetUserFromGUID( string userGUID )
    {
      Patron user = new PatronManager().GetByRowId( userGUID );
      if ( user.Id == 0 || !user.IsValid )
      {
        user.IsValid = false;
        user.Id = 0;
      }

      return user;
    }

    public bool isUserAdmin( Business.IWebUser user )
    {
      return isUserAdmin( ( Patron ) user );
    }
    public bool isUserAdmin( Patron user )
    {
      return Isle.BizServices.SecurityManager.GetGroupObjectPrivileges( user, "ILPathways.Admin" ).DeletePrivilege > ( int )ILPathways.Business.EPrivilegeDepth.State;
    }

    public string ImmediateReturn( object data, bool isValid, string status, object extra )
    {
      return serializer.Serialize(
        new
        {
          data = data,
          isValid = isValid,
          status = status,
          extra = extra
        }
      );
    }
    #endregion

    #region subclasses

    #endregion
  }
}
