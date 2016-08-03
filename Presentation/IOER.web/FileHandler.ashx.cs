using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Isle.BizServices;
using ILPathways.Business;
using ILPathways.Utilities;
using LRWarehouse.Business;

namespace IOER
{
	/// <summary>
	/// Summary description for FileHandler
	/// </summary>
	public class FileHandler : IHttpHandler
	{

		public void ProcessRequest( HttpContext context )
		{
			string message = "";
			string action = FormHelper.GetRequestKeyValue( "a", "v" );
			int contentId = FormHelper.GetRequestKeyValue("fileId", 0);
			if ( contentId < 1 )
			{
				//context.Response.ContentType = "text/plain";
				//context.Response.Write("File cannot be found!");
				//
				FormHelper.SetSessionItem( context.Session, "AppErrorMessage", "Invalid file request!" );
				context.Response.Redirect( "/ErrorPage.aspx?ErrorPage=RequestContentItem of " + contentId.ToString(), true );
			}
			else
			{
				if ( context.User.Identity.IsAuthenticated )
				{
					//can I use this?
				}
				Patron user = AccountServices.GetUserFromSession( context.Session );

				ContentItem item = new ContentServices().Get( contentId);
				if ( item == null || item.Id == 0 
					|| string.IsNullOrWhiteSpace( item.FileName ) )
				{
					//context.Response.ContentType = "text/plain";
					//context.Response.Write( "Invalid file request!" );
					FormHelper.SetSessionItem( context.Session, "AppErrorMessage", "Invalid file request!" );
					context.Response.Redirect( "/ErrorPage.aspx?ErrorPage=RequestContentItem of " + contentId.ToString(), true );
				}
				else if ( item.PrivilegeTypeId == 1)
				{
					//should probable check status?
					if ( action == "v" )
						context.Response.Redirect( item.DocumentUrl, true );
					else
						ServeFile( context, item );

				} else 
				{
					//context.Response.ContentType = "text/plain";
					//context.Response.Write( "File: " + item.FilePath + "/" + item.FileName + "-" + item.FileLocation() );
					if ( ValidateFileAccess( context, item, ref message ) == false )
					{
						//ServeText( context, message );
						FormHelper.SetSessionItem( context.Session, "AppErrorMessage", message );
						context.Response.Redirect( "/ErrorPage.aspx?ErrorPage=RequestContentItem of " + contentId.ToString(), true );
					}
					else
					{
						if ( FileSystemHelper.DoesFileExist( item.FileLocation()) == false ) 
						{
							message = FileSystemHelper.HandleDocumentCaching( item.FilePath, item.RelatedDocument, true );
							if ( message.Length > 0 )
							{
								//ServeText( context, message );
								FormHelper.SetSessionItem( context.Session, "AppErrorMessage", message );
								context.Response.Redirect( "/ErrorPage.aspx?ErrorPage=RequestContentItem of " + contentId.ToString(), true );
								return;
							}
						}
						if (action == "v" )
							context.Response.Redirect( item.DocumentUrl, true );
						else
							ServeFile( context, item );
					}

					//context.Response.Clear();
					//context.Response.ContentType = "application/octet-stream";
					////I have set the ContentType to "application/octet-stream" which cover any type of file
					//context.Response.AddHeader( "content-disposition", "attachment;filename=" + item.FileLocation() );
					//context.Response.WriteFile( item.FileLocation());

					////here you can do some statistic or tracking
					////you can also implement other business request such as delete the file after download
					//context.Response.End();
				}
			}
		}
		private void ServeText( HttpContext context, string message )
		{
			context.Response.ContentType = "text/plain";
			context.Response.Write( message );
		}
		private void ServeFile( HttpContext context, ContentItem item )
		{
			LoggingHelper.DoTrace( 6, "downloading: " + item.FileLocation() );
			string ext = System.IO.Path.GetExtension( item.FileName );

			context.Response.Clear();
			context.Response.ContentType = "application/octet-stream";
			//set the ContentType to "application/octet-stream" which cover any type of file
			//we could hide the file name completely by using the file title. But that may result in too general of a name
			context.Response.AddHeader( "content-disposition", "attachment;filename=" + item.Title + ext );
			context.Response.WriteFile( item.FileLocation() );

			//here you can do some statistic or tracking
			//you can also implement other business request such as delete the file after download
			context.Response.End();
		}
		private bool ValidateFileAccess( HttpContext context, ContentItem item, ref string message )
		{
			bool isValid = true;

			message = "";
			Patron user = AccountServices.GetUserFromSession( context.Session );

			if ( item.PrivilegeTypeId > 1 && user == null || user.Id == 0 )
			{
				message = "You must be logged in to view this file!";
				isValid = false;
			}

			return isValid;
		}
		public bool IsReusable
		{
			get
			{
				return false;
			}
		}
	}
}