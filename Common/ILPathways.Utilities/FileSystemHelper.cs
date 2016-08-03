using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using Scripting;

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

        public static string HandleImageCaching( string targetFolder, IDocument document)
        {
            bool overwritingFile = false;
            return HandleDocumentCaching( targetFolder, document, overwritingFile );
        } //
        public static string HandleImageCaching( string targetFolder, IDocument document, bool overwritingFile )
        {
            //bool overwritingFile = false;
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
            string destFile = "";
            if ( document == null || document.FileName == null || document.FileName.Trim().Length == 0 )
            {
                message = "Error - an invalid or incomplete document was encountered.";
                return message;
            }
           
			try
			{
                if ( targetFolder == null || targetFolder.Trim().Length == 0 )
                {
                    //just in case, use default and report
                    targetFolder = UtilityManager.GetAppKeyValue( "path.ContentOutputPath", "C:\\" );

                    LoggingHelper.LogError( string.Format( thisClassName + ".HandleDocumentCaching() - targetFolder not provided, using path.ContentOutputPath. RowId: {0}, FileName: {1}", document.RowId, document.FileName ), true );
                }
                //check if target contains file name
                if ( targetFolder.ToLower().IndexOf( document.FileName.ToLower() ) > -1 )
                {
                    destFile = targetFolder;
                }
                else
                {
                    if ( targetFolder.Trim().EndsWith( "\\" ) == false )
                    {
                        targetFolder = targetFolder.Trim() + "\\";
                    }
                    destFile = targetFolder + document.FileName;
                }

				if ( System.IO.File.Exists( destFile ) )
				{
					//may want to return path for display in a link?
                    if ( overwritingFile )
                    {
						LoggingHelper.DoTrace( 3, thisClassName + ".HandleDocumentCaching() overwriting existing file: " + destFile );
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
        /// Set a filepath from a url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string SetFilePathFromUrl( string url, string fileName )
        {
            if ( url == null || url.Trim().Length < 5 )
                return "";

            string documentFolder = "";
            string baseUrl = UtilityManager.GetAppKeyValue( "path.ContentOutputUrl", "/Content/" );
            string basePath = UtilityManager.GetAppKeyValue( "path.ContentOutputPath" );

            //the base url should be in the url
            int start = url.ToLower().IndexOf( baseUrl.ToLower() );
            if ( start > -1 )
            {
                documentFolder = basePath + url.Substring( start + baseUrl.Length );
                documentFolder = documentFolder.Replace( "/", "\\" );
                //extract filename
                int pos = documentFolder.ToLower().IndexOf( fileName.ToLower() );
                if ( pos > -1 )
                {
                    documentFolder = documentFolder.Substring( 0, pos - 1 );
                }

            }
            else
            {
                //if not, then a problem
            }

            return documentFolder;
        }//

        /// <summary>
        /// Delete file from server
        /// </summary>
        /// <param name="targetFolder"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static bool DeleteDocumentFromServer( string targetFolder, IDocument document )
        {
            bool isSuccessful = false;
            string message = "";

            string destFile = targetFolder + "\\" + document.FileName;
            try
            {
                if ( System.IO.File.Exists( destFile ) )
                {
                    System.IO.File.Delete( destFile );
                    isSuccessful = true;
                }
                else
                {
                    isSuccessful = true;
                }

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".DeleteDocumentFromServer() - Unexpected error encountered while deleting document:<br/>" + ex.Message );

                message = thisClassName + ".DeleteDocumentFromServer() - Unexpected error encountered. You could try closing the form and then try again. System Administration has been notified)<br/>" + ex.ToString();
            }
            return isSuccessful;
        }
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
		/// Analyze a filename, and return system friendly name
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		public static string SanitizeFilename( string filename )
		{
			return SanitizeFilename( filename, 0 );
		}
		/// <summary>
		/// Analyze a filename, and return system friendly name
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="maxLength">If greater than zero, use to truncate the file length</param>
		/// <returns></returns>
		public static string SanitizeFilename( string filename, int maxLength )
		{
			if ( filename == null || filename.Trim().Length == 0 )
				return "";

			string file = filename.Trim();
			file = string.Concat( file.Split( System.IO.Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries ) );

			if ( maxLength > 0 && file.Length > maxLength )
			{
				file = file.Substring( 0, maxLength );
			}
			return file;

		} //

        /// <summary>
		/// Return true if the passed path exists
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool DoesPathExist( string path )
		{
            if ( path == null )
                return false;

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
			if ( documentFolder == null || fileName == null )
                return false;
			string pathAndFileName = documentFolder + "\\" + fileName;

			return DoesFileExist( pathAndFileName );
		}

		/// <summary>
		/// Check if file exists on server
		/// </summary>
		/// <param name="documentFolder"></param>
		/// <param name="fileName"></param>
		public static bool DoesFileExist( string pathAndFileName )
		{
			if ( pathAndFileName == null || pathAndFileName == null )
                return false;

			try
			{
				if ( System.IO.File.Exists( pathAndFileName ) )
				{
					return true;
				} else
				{
					return false;
				}
			} catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".DoesFileExist() - Unexpected error encountered while retrieving document. File: " + pathAndFileName );

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
