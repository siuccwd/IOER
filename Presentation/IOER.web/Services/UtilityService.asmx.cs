using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Serialization;

using ILPathways.Utilities;
using ILPathways.Business;
using Isle.BizServices;
using LRWarehouse.DAL;
using Thumbnailer = LRWarehouse.DAL.ResourceThumbnailManager;
using LRWarehouse.Business;
using ThisUser = LRWarehouse.Business.Patron;

namespace IOER.Services
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
	public string ValidateURL(string url, bool mustBeNew, ref bool isValid, ref string status)
	{
		//Do basic text validation without rewriting the URL
		ValidateText(url, 12, "URL", ref isValid, ref status);
		if (!isValid)
		{
			return url;
		}

		//Just to be sure
		url = url.Replace("<", "").Replace(">", "");

		//Check for existing URL, if we care
		if (mustBeNew)
		{
			var test = ResourceBizService.ResourceVersion_GetByUrl(url);
			if (test.Count > 0)
			{
				var first = test.First<LRWarehouse.Business.ResourceVersion>();
				status = "Resource already exists in IOER: <a href=\"/Resource/" + first.ResourceIntId + "/" + ResourceVersion.UrlFriendlyTitle(first.SortTitle) + "\">Click Here</a>";
				isValid = false;
				return "";
			}
		}

		//Check for basic URL formatting
		var currentEnvironment = ServiceHelper.GetAppKeyValue("envType", "prod");
		try
		{
			var testURL = new Uri(url);
			if (currentEnvironment != "dev") //localhost URLs don't have a .
			{
				if (
					(url.IndexOf("http://") != 0 && url.IndexOf("https://") != 0) ||
					url.IndexOf(".") == -1
				)
				{
					throw new Exception();
				}
			}
		}
		catch (Exception ex)
		{
			status = "Improperly formatted URL.";
			isValid = false;
			return "";
		}

		if (currentEnvironment != "dev") //localhost is blacklisted and that got annoying
		{
			//Check for blacklist
			Uri uri = new Uri(url);
			string blStatus = "successful";
			BlacklistedHost bh = new BlacklistedHostManager().GetByHostname(uri.Host, ref blStatus);
			if (bh != null)
			{
				status = "This page is suspected to be a phishing page, contain malware, or may otherwise be inappropriate.  " +
						"Learn more about <a href='http://www.antiphishing.org/'>phishing</a> and <a href='http://www.stopbadware.org/'>malware</a>.";
				isValid = false;
				return "";
			}
			else
			{
				string reputation = UtilityManager.CheckUnsafeUrl(url);
				if (reputation == "Blacklisted")
				{
					status = "This page is suspected to be a phishing page, contain malware, or may otherwise be inappropriate.  " +
							"Learn more about <a href='http://www.antiphishing.org/'>phishing</a> and <a href='http://www.stopbadware.org/'>malware</a>.  " +
							"Advisory provided by <a href='http://code.google.com/apis/safebrowsing/safebrowsing_faq.html#whyAdvisory'>Google</a>.";
					isValid = false;
					return "";
				}
			}
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
      bool allowingHtmlPosts = false;
      text = ValidateText( text, minimumLength, fieldTitle, allowingHtmlPosts, ref isValid, ref status );
      return serializer.Serialize(
        new
        {
          data = text,
          isValid = isValid,
          status = status
        }
      );
    }
    public string ValidateText(string text, int minimumLength, string fieldTitle, ref bool isValid, ref string status)
    {
        bool allowingHtmlPosts = false;
        return ValidateText(text, minimumLength, fieldTitle, allowingHtmlPosts, ref isValid, ref status);
    }

    public string ValidateText( string text, int minimumLength, string fieldTitle, bool allowingHtmlPosts, ref bool isValid, ref string status )
    {
        text = FormHelper.CleanText(text, allowingHtmlPosts);
      text = text.Trim();
      if ( minimumLength > 0 && text.Length < minimumLength )
      {
        status = fieldTitle + " must be at least " + minimumLength + " character" + ( minimumLength == 1 ? "" : "s" ) + " long.";
        isValid = false;
        return text;
      }
      if ( BadWordChecker.CheckForBadWords( text ) )
      {
        status = "Inappropriate language detected in " + fieldTitle;
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
        if ( test.User.Length == 0 ) { throw new ArgumentException(); }
        var testURI = new Uri( "http://" + test.Host, UriKind.Absolute); //Should throw an error if improperly formatted
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
			if ( emailAlreadyExists )
			{
				status = "Email Address already exists in system.";
			}
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

      text = FormHelper.CleanText( text );
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
    /// <summary>
    /// Retrieve a Resource id using the related resource version id to retrieve the Resource record
    /// </summary>
    /// <param name="versionID"></param>
    /// <returns></returns>
    public int GetIntIDFromVersionID( int versionID )
    {
        return ResourceBizService.ResourceGet_ViaVersionID( versionID ).Id;
    }
    /// <summary>
    /// Retrieve resource version id using resourceIntId to retrieve the resource version record
    /// </summary>
    /// <param name="resourceId"></param>
    /// <returns></returns>
    public int GetVersionIDFromIntID( int resourceId )
    {
        //return new ResourceVersionManager().GetByResourceId( resourceId ).Id;

        return ResourceBizService.ResourceVersion_GetByResourceId( resourceId ).Id;
    }


    [WebMethod]
    public string GetThumbnail( int intID, string url )
    {
      return "/OERThumbs/large/" + intID + "-large.png";
    }

    [WebMethod]
    public string RegenerateThumbnail( string userGUID, int intID, string url )
    {
      try
      {
        var user = GetUserFromGUID( userGUID );
				if ( user.Id > 0 )
				{
					if ( isUserAdmin( user ) )
					{
						new Thumbnailer().CreateThumbnail( intID, url, true );
						return ImmediateReturn( true, true, "Regenerating, please wait", null );
					}
					else
					{
						return ImmediateReturn( false, false, "You don't have permission to do that.", null );
					}
				}
				else
				{
					return ImmediateReturn( false, false, "You must be logged in to do that.", null );
				}
      }
      catch ( Exception ex )
      {
        return ImmediateReturn( false, false, "Error: " + ex.Message, ex.ToString() );
      }
    }

    /*
    //Run in console:
    $.ajax({
      url: "/Services/UtilityService.asmx/RefreshElasticSearchRecords",
      success: function(msg){ console.log(msg); },
      type: "POST",
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      data: JSON.stringify({ ids: [1, 2, 3] }),
    });
    */
    [WebMethod( EnableSession = true )]
    public GenericReturn RefreshElasticSearchRecords( List<int> ids )
    {
      //Validate user
      var user = ( ThisUser ) Session[ "user" ];
      if ( user == null || user.Id == 0 )
      {
        return DoReturn( "", false, "invalid user", null );
      }

      if ( isUserAdmin( user ) )
      {
        try
        {
          //new ElasticSearchManager().RefreshResources( ids );
          var manager = new ResourceV2Services();
          foreach ( var id in ids )
          {
            manager.RefreshResource( id );
          }
        }
        catch ( Exception ex )
        {
          return DoReturn( ex.Message, false, "Error: " + ex.Message, ex );
        }
        return DoReturn( "", true, "Success", null );
      }

      return DoReturn( "", false, "invalid user", null );
    }

	#region === user methods ===
	/// <summary>
	/// Determine if current user is a logged in (registered) user 
	/// </summary>
	/// <returns></returns>
	public bool IsUserAuthenticated()
	{
		//bool isUserAuthenticated = false;
		//try
		//{
		//	ThisUser appUser = GetUserFromSession();
		//	if ( appUser == null || appUser.Id == 0 )
		//	{
		//		isUserAuthenticated = false;
		//	}
		//	else
		//	{
		//		isUserAuthenticated = true;
		//	}
		//}
		//catch
		//{

		//}

		//return isUserAuthenticated;
		return AccountServices.IsUserAuthenticated();

	} //

	//Get user from session or return null
	public ThisUser GetUser(bool returnNullInsteadOfNewIfNotFound)
	{
		try
		{
			var user = AccountServices.GetUserFromSession(Session);
			//var user = (ThisUser)Session[Constants.USER_REGISTER];
			if (user == null || user.Id == 0)
			{
				throw new UnauthorizedAccessException();
			}
			return user;
		}
		catch
		{
			return returnNullInsteadOfNewIfNotFound ? null : new ThisUser();
		}
	}
	public ThisUser GetUserFromSession()
	{
		//ThisUser user = new ThisUser();
		//try
		//{ 		//Get the user
		//	user = ( ThisUser ) Session[ Constants.USER_REGISTER ];

		//	if ( user.Id == 0 || !user.IsValid )
		//	{
		//		user.IsValid = false;
		//		user.Id = 0;
		//	}
		//}
		//catch
		//{
		//	user = new ThisUser();
		//	user.IsValid = false;
		//}
		//return user;
		return AccountServices.GetUserFromSession(Session);
	}
	public ThisUser GetUserFromGUID( string userGUID )
    {
	  //ThisUser user = new PatronManager().GetByRowId( userGUID );
	  //if ( user.Id == 0 || !user.IsValid )
	  //{
	  //  user.IsValid = false;
	  //  user.Id = 0;
	  //}
	  return new AccountServices().GetByRowId(userGUID);
      //return user;
    }

    public bool isUserAdmin( IWebUser user )
    {
      return isUserAdmin( ( ThisUser ) user );
    }
	  /// <summary>
	  /// this should be in account services!
	  /// ==> actually, based on the check of a delete privilege, is this properly named?
	  /// </summary>
	  /// <param name="user"></param>
	  /// <returns></returns>
    public bool isUserAdmin( ThisUser user )
    {
		string siteAdminObjectName = ServiceHelper.GetAppKeyValue("siteAdminObjectName");
		//"Site.Admin"
      return Isle.BizServices.SecurityManager.GetGroupObjectPrivileges( user, siteAdminObjectName ).DeletePrivilege > ( int )ILPathways.Business.EPrivilegeDepth.State;
    }
	#endregion

	public string ImmediateReturn( object data, bool isValid, string status, object extra )
    {
		serializer.MaxJsonLength = Int32.MaxValue;
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

    public static GenericReturn DoReturn( object data, bool valid, string status, object extra )
    {
      return new GenericReturn()
      {
        data = data,
        valid = valid,
        status = status,
        extra = extra
      };
    }


    #endregion

    #region subclasses
    public class GenericReturn
    {
      protected internal GenericReturn() { }
      public object data { get; set; }
      public bool valid { get; set; }
      public string status { get; set; }
      public object extra { get; set; }
    }
    #endregion
  }
}
