using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using Scripting;

//using workNet.DAL;
using ILPathways.Business;

namespace ILPathways.Utilities
{
	public class FileSystemHelper
	{
		const string thisClassName = "FileSystemHelper";

		public FileSystemHelper() 
		{ }

		#region Documents
        public static string HandleDocumentCaching( string targetFolder, IDocument document )
        {
            bool overwritingFile = false;
            return HandleDocumentCaching( targetFolder, document, overwritingFile );
        } //

		/// <summary>
		/// Handle document caching. Check if file exists, if not cache the document
		/// </summary>
		/// <param name="targetFolder"></param>
		/// <param name="document"></param>
		/// <returns>Blank if was successful, otherwise an error message</returns>
        public static string HandleDocumentCaching( string targetFolder, IDocument document, bool overwritingFile )
		{
			string message = "";

			string destFile = targetFolder + "\\" + document.FileName;
			try
			{
				if ( System.IO.File.Exists( destFile ) )
				{
					//may want to return path for display in a link?
                    if ( overwritingFile )
                    {
                        //download
                        byte[] buffer = document.ResourceData;
                        using ( FileStream fs = new FileStream( destFile, FileMode.Create ) )
                        {
                            fs.Write( buffer, 0, buffer.Length );
                        }
                    }
				} else
				{
					//ensure directory structure exists
					CreateDirectory( targetFolder );

					//download
					byte[] buffer = document.ResourceData;
					using ( FileStream fs = new FileStream( destFile, FileMode.Create ) )
					{
						fs.Write( buffer, 0, buffer.Length );
					}
				}

			} catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".HandleDocumentCaching() - Unexpected error encountered while retrieving document:<br/>" + ex.Message );

				message = thisClassName + ".HandleDocumentCaching() - Unexpected error encountered. You could try closing the form and then try again. System Administration has been notified)<br/>" + ex.ToString();
			}

			return message;
		}//

		/// <summary>
		/// Handle document caching. Check if file exists, if not retrieve and cache
		/// </summary>
		/// <param name="path"></param>
		/// <param name="fileName"></param>
		/// <param name="fileUrl"></param>
		/// <param name="docRowId"></param>
		/// <returns></returns>
        public static string HandleDocumentCaching( string path, string fileName, string fileUrl, string docRowId )
		{
			string url = "";
			try
			{
				if ( DoesFileExist( path, fileName ) )
				{
					url = fileUrl + fileName;
				} else
				{

                    //DocumentVersion doc = DocumentStoreManager.Get( docRowId );
                    ////ensure direc structure exists
                    //CreateDirectory( path );
                    //string destFile = path + "\\" + doc.FileName;
                    ////download
                    //byte[] buffer = doc.ResourceData;
                    //using ( FileStream fs = new FileStream( destFile, FileMode.Create ) )
                    //{
                    //    fs.Write( buffer, 0, buffer.Length );
                    //}

                    //url = fileUrl + doc.FileName;
                    //if ( doc.URL.Length == 0 )
                    //{
                    //    //update url
                    //    doc.URL = url;
                    //    DocumentStoreManager.Update( doc );
                    //}
				}
			} catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".HandleDocumentCaching(" + path + ")" );
				url = "";
			}

			return url;
		} //
		#endregion 

		#region export methods

		/// <summary>
		/// ExportDataTableAsCsv - formats a DataTable in CSV format and then streams to the browser
		/// </summary>
		/// <param name="dt">DataTable</param>
		/// <param name="tempFilename">Name of temporary file</param>
		public void ExportDataTableAsCsv( DataTable dt, string tempFilename )
		{
			string datePrefix = System.DateTime.Today.ToString( "u" ).Substring( 0, 10 );
			string logFile = UtilityManager.GetAppKeyValue( "path.ReportsOutputPath", "C:\\VOS_LOGS.txt" );

			string outputFile = logFile + datePrefix + "_" + tempFilename;
			//
			//string filename = "budgetExport.csv";

			string csvFilename = this.DataTableAsCsv( dt, outputFile, false );

			HttpContext.Current.Response.ContentType = "application/octet-stream";
			HttpContext.Current.Response.AddHeader( "Content-Disposition", "attachment; filename=" + tempFilename + "" );

			HttpContext.Current.Response.WriteFile( csvFilename );
			HttpContext.Current.Response.End();
			// Delete the newly created file.
			//TODO: - this line is not actually executed - need scheduled clean ups?
			//File.Delete(Server.MapPath(csvFilename));
		}

		/// <summary>
		/// DataTableAsCsv - formats a DataTable in csv format
		/// 								 The code first loops through the columns of the data table to export the names of all the data columns. 
		/// 								 Then in next loop the code iterates over each data row to export all the values in the table. 
		///									 This method creates a temporary file on the server. This temporary file will need to
		///									 be manually deleted at a later time.
		/// </summary>
		/// <param name="dt">DataTable</param>
		/// <param name="tempFilename">Name of temporary file</param>
		/// <param name="doingMapPath">If true use Server.MapPath(</param>/// 
		/// <returns>Name of temp file created on the server</returns>
		public string DataTableAsCsv( DataTable dt, string tempFilename, bool doingMapPath )
		{

			string strColumn = "";
			string strCorrected = "";
			StreamWriter sw;
			string serverFilename = ""; ;

			if ( doingMapPath )
			{
				serverFilename = "~/" + tempFilename;
				// Create the CSV file to which grid data will be exported.
				sw = new StreamWriter( System.Web.HttpContext.Current.Server.MapPath( serverFilename ), false );
			} else
			{
				serverFilename = tempFilename;
				sw = new StreamWriter( serverFilename, false );
			}

			// First we will write the headers.
			int intCount = dt.Columns.Count;
			for ( int i = 0; i < intCount; i++ )
			{
				sw.Write( dt.Columns[ i ].ToString() );
				if ( i < intCount - 1 )
				{
					sw.Write( "," );
				}
			}
			sw.Write( sw.NewLine );
			// Now write all the rows.
			foreach ( DataRow dr in dt.Rows )
			{
				for ( int i = 0; i < intCount; i++ )
				{
					if ( !Convert.IsDBNull( dr[ i ] ) )
					{
						strColumn = dr[ i ].ToString();

						strCorrected = strColumn.Replace( "\"", "\'" );

						sw.Write( "\"" + strCorrected + "\"" );
					} else
					{
						sw.Write( "" );
					}
					if ( i < intCount - 1 )
					{
						sw.Write( "," );
					}
				}
				sw.Write( sw.NewLine );
			}
			sw.Close();


			return serverFilename;

		} //

		#endregion


        #region Folders and files
        
        /// <summary>
		/// Return true if the passed path exists
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool DoesPathExist( string path )
		{
			try
			{
				if ( Directory.Exists( path ) )
				{
					return true;
				} else
				{
					return false;
				}
			} catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".DoesPathExist(" + path + ")" );
				return false;
			}
		}

		/// <summary>
		/// Check if file exists on server
		/// </summary>
		/// <param name="documentFolder"></param>
		/// <param name="fileName"></param>
		public static bool DoesFileExist( string documentFolder, string fileName )
		{
			string destFile = documentFolder + "\\" + fileName;
			try
			{
				if ( System.IO.File.Exists( destFile ) )
				{
					return true;
				} else
				{
					return false;
				}
			} catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".DoesFileExist() - Unexpected error encountered while retrieving document" );

				return false;
			}
		}//

		/// <summary>
		/// Create the passed directory structure
		/// As System.IO.Directory.CreateDirectory() checks folder existence from root to lowest folder, it will fail if an intermediate folder 
		/// doesn't exist. This method performs the scan in the reverse way - from lowest to upper. Therefore, it won't fail unless it will get to
		/// some folder with no read permissions.
		/// requires Microsoft Scripting Runtime COM (windows\system32\scrrun.dll)
		/// ref: http://www.codeproject.com/KB/files/createdirectorymethod.aspx
		/// </summary>
		/// <param name="path"></param>
		public static void CreateDirectory( string path )
		{
			// trim leading \ character

			try
			{
				//first check if already exists
				if ( DoesPathExist( path ) )
					return;


				path = path.TrimEnd( Path.DirectorySeparatorChar );
                Scripting.FileSystemObject fso = new Scripting.FileSystemObject();  //.FileSystemObjectClass();
                // check if folder exists, if yes - no work to do

                if ( !fso.FolderExists( path ) )
                {
                    int i = path.LastIndexOf( Path.DirectorySeparatorChar );
                    // find last\lowest folder name

                    string CurrentDirectoryName = path.Substring( i + 1,  path.Length - i - 1 );
                    // find parent folder of the last folder

                    string ParentDirectoryPath = path.Substring( 0, i );
                    // recursive calling of function to create all parent folders 

                    CreateDirectory( ParentDirectoryPath );
                    // create last folder in current path

                    Scripting.Folder folder = fso.GetFolder( ParentDirectoryPath );
                    folder.SubFolders.Add( CurrentDirectoryName );

                }
			} catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".CreateDirectory(" + path + ")" );
			}
		}

		/// <summary>
		/// Get the domain for the current environment
		/// </summary>
		/// <returns></returns>
		public static string GetThisDomainUrl()
		{
			string domain = UtilityManager.GetAppKeyValue( "envDomainUrl" );
			return domain;
		}

		/// <summary>
		/// Get the root path for the current environment
		/// </summary>
		/// <returns></returns>
		public static string GetThisRootPath()
		{
			return UtilityManager.GetAppKeyValue( "path.RootPath" );
		}

		/// <summary>
		/// Get absolute url for cache
		/// </summary>
		/// <returns></returns>
		public static string GetCacheOutputUrl()
		{
			return GetCacheOutputUrl( "" );
		}
		/// <summary>
		/// Get absolute url for cache
		/// </summary>
		/// <param name="subPath"></param>
		/// <returns></returns>
		public static string GetCacheOutputUrl( string subPath )
		{
			string domain = GetThisDomainUrl();
			string cacheUrl = UtilityManager.GetAppKeyValue( "path.CacheUrl" );
			
			if ( subPath.Length > 0 )
				return domain + cacheUrl + "/" + subPath + "/";
			else
				return domain + cacheUrl + "/";
		}
		/// <summary>
		/// Get output path for cache
		/// </summary>
		/// <returns></returns>
		public static string GetCacheOutputPath()
		{
			return GetCacheOutputPath( "" );
		}
		public static string GetCacheOutputPath(string subPath)
		{
			string root = GetThisRootPath();
			string cacheFolder = UtilityManager.GetAppKeyValue( "path.CacheFolder" );
			if (subPath.Length > 0)
				return root + cacheFolder + "\\" + subPath + "\\";
			else 
				return root + cacheFolder + "\\";
        }
        #endregion
    }
}
