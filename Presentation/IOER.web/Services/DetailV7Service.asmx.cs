using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using System.Web.Script.Serialization;

using Isle.DTO;
using Isle.BizServices;
using IOER.classes;
using ILPathways.Utilities;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using Patron = LRWarehouse.Business.Patron;

namespace IOER.Services
{
	/// <summary>
	/// Summary description for DetailV7Service
	/// </summary>
	[WebService( Namespace = "http://tempuri.org/" )]
	[WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
	[System.ComponentModel.ToolboxItem( false )]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[System.Web.Script.Services.ScriptService]
	public class DetailV7Service : System.Web.Services.WebService
	{
		JavaScriptSerializer serializer = new JavaScriptSerializer();


		#region Get methods
		[WebMethod( EnableSession = true )]
		public string GetUserLibraryDataJSON( int resourceID )
		{
			var valid = true;
			var status = "";
			var result = GetUserLibraryData(resourceID, ref valid, ref status);

			return serializer.Serialize( UtilityService.DoReturn( result, valid, status, null ) );
		}
		public List<LibColData> GetUserLibraryData( int resourceID )
		{
			var valid = true;
			var status = "";
			return GetUserLibraryData( resourceID, ref valid, ref status );
		}
		public List<LibColData> GetUserLibraryData( int resourceID, ref bool valid, ref string status )
		{
			var userLibraries = new List<LibrarySummaryDTO>();
			var libData = new List<LibColData>();
			var service = new LibraryBizService();

			//Validate User
			var user = GetUser();
			if ( user.Id == 0 )
			{
				valid = false;
				status = "You must be logged in to do that.";
				return null;
			}

			//Get all libraries this user has access to
			userLibraries = service.Library_SelectListWithContributeAccess( user.Id );
			foreach ( var item in userLibraries )
			{ //For each one, get its collections
				var collections = service.LibrarySectionsSelectList( item.Id );
				var cols = new List<LibColData>();
				//Get data for each collection
				foreach ( var col in collections )
				{
					cols.Add( new LibColData()
					{
						Id = col.Id,
						Title = col.Title,
						Image = col.ImageUrl,
						Contains = service.IsResourceInLibraryByCollectionId( col.Id, resourceID )
					} );
				}
				//Get data for library
				var data = new LibColData()
				{
					Id = item.Id,
					Title = item.Title,
					Image = item.ImageUrl,
					Contains = service.IsResourceInLibrary( item.Id, resourceID ),
					Collections = cols
				};
				libData.Add( data );
			}

			//Return data
			return libData;
		}

		[WebMethod]
		public string GetResourceLibraryDataJSON( int resourceID )
		{
			var result = GetResourceLibraryData( resourceID );
			return serializer.Serialize( UtilityService.DoReturn( result, true, "", null ) );
		}
		public List<LibColData> GetResourceLibraryData( int resourceID )
		{
			//Get library data relevant to the resource
			var resourceLibraryData = new LibraryBizService().GetAllLibrariesWithResource( resourceID );
			var resourceLibraries = new List<LibColData>();
			foreach ( var item in resourceLibraryData )
			{
				resourceLibraries.Add( new LibColData()
				{
					Id = item.Id,
					Title = item.Title,
					Contains = true,
					Image = item.ImageUrl
				} );
			}

			return resourceLibraries;
		}

		#endregion 

		#region Update methods

		[WebMethod( EnableSession = true )]
		public string AddResourceToCollectionsJSON( int resourceID, List<int> collectionIDs )
		{
			//Attempt the add
			var valid = true;
			var status = "";
			var updatedUserLibraryData = AddResourceToCollections( resourceID, collectionIDs, ref valid, ref status );
			//If successful, return updated data
			if ( valid )
			{
				var resourceLibraryData = GetResourceLibraryData( resourceID );
				return serializer.Serialize( UtilityService.DoReturn( new { resourceLibraryData = resourceLibraryData, userLibraryData = updatedUserLibraryData }, true, "okay", null ) );
			}
			//Otherwise, return error
			else
			{
				return serializer.Serialize( Fail( status, "" ) );
			}
		}
		public List<LibColData> AddResourceToCollections( int resourceID, List<int> collectionIDs, ref bool valid, ref string status )
		{
			var libService = new LibraryBizService();

			//Validate User
			var user = GetUser();
			if ( user.Id == 0 )
			{
				valid = false;
				status = "You must be logged in to do that.";
				return null;
			}

			//Get user's library data to prevent spoofing attacks
			var libData = GetUserLibraryData( resourceID, ref valid, ref status );
			if ( !valid )
			{
				//Status is already set
				return null;
			}

			//Do the add(s)
			try
			{
				var test = libService.LibrarySections_SelectListWithEditAccess( 0, user.Id );
				var validCollections = libData.SelectMany( t => t.Collections ).ToList();
				foreach ( var item in collectionIDs )
				{
					//If the ID belongs to a collection the user has access to...
					var targetCollection = libData.SelectMany(t => t.Collections).Where(t => t.Id == item).FirstOrDefault();
					if ( targetCollection != null )
					{
						libService.LibraryResourceCreate( item, resourceID, user.Id, ref status );
						targetCollection.Contains = true; //added
					}
				}
			}
			catch ( Exception ex )
			{
				valid = false;
				status = "There was an error adding the Resource to one or more of the selected collections.";
				return null;
			}

			valid = true;
			status = "okay";
			return libData;
		}

		[WebMethod( EnableSession = true )]
		public string PostCommentJSON( int resourceID, string text )
		{
			try {
				var valid = true;
				var status = "";
				var results = PostComment( resourceID, text, ref valid, ref status );
				return serializer.Serialize( UtilityService.DoReturn( results, valid, status, null ) );
			}
			catch(Exception ex) {
				return serializer.Serialize( Fail( "Sorry, there was an error posting your comment.", ex.Message ) );
			}
		}
		public List<ResourceComment> PostComment( int resourceID, string text, ref bool valid, ref string status )
		{
			//Validate User
			var user = GetUser();
			if ( user.Id == 0 )
			{
				valid = false;
				status = "You must be logged in to do that.";
				return null;
			}

			//Validate text
			text = new UtilityService().ValidateText( text, 10, "Comment", ref valid, ref status );
			if ( !valid )
			{
				return null;
			}

			//Post comment
			var commentManager = new ResourceCommentManager();
			var comment = new ResourceComment()
			{
				ResourceIntId = resourceID,
				Comment = text,
				CreatedById = user.Id,
				CreatedBy = user.FullName()
			};
			var commentID = commentManager.Create( comment, ref status );
			if ( commentID == 0 )
			{
				valid = false;
				return null;
			}

			//Return list of comments for that resource
			return new ResourceCommentManager().SelectList( resourceID );
		}

		[WebMethod( EnableSession = true )]
		public string ReportIssueJSON( int resourceID, string text )
		{
			try
			{
				var valid = true;
				var status = "";
				var results = ReportIssue( resourceID, text, ref valid, ref status );
				return serializer.Serialize( UtilityService.DoReturn( results, valid, status, null ) );
			}
			catch ( Exception ex )
			{
				return serializer.Serialize( Fail( "Sorry, there was an error posting your comment.", ex.Message ) );
			}
		}
		public bool ReportIssue( int resourceID, string text, ref bool valid, ref string status )
		{
			//Validate User
			var user = GetUser();
			if ( user.Id == 0 )
			{
				valid = false;
				status = "You must be logged in to do that.";
				return false;
			}

			//Validate text
			text = new UtilityService().ValidateText( text, 5, "Issue", ref valid, ref status );
			if ( !valid )
			{
				return false;
			}

			//Report the issue
			string url = "/Resource/" + resourceID + "/";
			url = UtilityManager.FormatAbsoluteUrl( url, false );
			string toEmail = UtilityManager.GetAppKeyValue( "contactUsMailTo", "info@ilsharedlearning.org" );
			string subject = "Reporting an issue!";
			string body = "<p>" + text + "</p>" +
			"<br/>IOER: " + string.Format( "<a href='{0}'>Resource Detail url</a>", url ) +
			"<br/>From: " + user.EmailSignature();
			string from = user.Email;
			EmailManager.SendEmail( toEmail, from, subject, body, "", "" );

			//Return true
			return true;
		}


		#endregion

		#region Helper methods
		//Fail gracefully
		public UtilityService.GenericReturn Fail( string message, string exception )
		{
			return UtilityService.DoReturn( null, false, message, exception );
		}

		//Get user
		public Patron GetUser()
		{
			return (Patron) SessionManager.GetUserFromSession( Session ) ?? new Patron();
		}

		#endregion

		#region Helper classes

		public class LibColData
		{
			public LibColData()
			{
				Collections = new List<LibColData>();
			}
			public int Id { get; set; }
			public string Title { get; set; }
			public string Image { get; set; }
			public bool Contains { get; set; } //Already contains this Resource
			public List<LibColData> Collections { get; set; }
		}


		#endregion
	}
}
