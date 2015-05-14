using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILPathways.Business;
using ILPathways.Utilities;

using MyManager = ILPathways.DAL.ContentManager;
using DocManager = ILPathways.DAL.DocumentStoreManager;
using EFManager = IoerContentBusinessEntities.EFContentManager;

namespace Isle.BizServices
{
    public class DocumentServices
    {
        static string thisClassName = "DocumentServices";
        

        public static DocumentVersion Document_Version_Get( Guid id )
        {
            return EFManager.Document_Version_Get( id );
        }

        /// <summary>
        /// Update a document version
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>"successful" or an error message</returns>
        public string Document_Version_Update( DocumentVersion entity )
        {
            DocManager mgr = new DocManager();
            string message = mgr.Update( entity );

            return message;
        }

        /// <summary>
        /// While full object is used, only filepath, filename, and url will actually be used in the update
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>"successful" or an error message</returns>
        public string Document_Version_UpdateFileInfo( DocumentVersion entity )
        {
            DocManager mgr = new DocManager();
            string message = mgr.UpdateFileInfo( entity );

            return message;
        }

        public bool DocumentVersion_Delete( Guid? id, ref string statusMessage )
        {
            bool isSuccessful = false;
            try
            {
                statusMessage = "";
                EFManager myEfManager = new EFManager();
                isSuccessful = myEfManager.Document_Version_Delete( id, ref statusMessage );
                
            }
            catch ( Exception ex )
            {
                isSuccessful = false;
                statusMessage = ex.Message;
            }
            return isSuccessful;
        }

        public static string HandleDocumentCaching( string targetFolder, DocumentVersion document )
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
        public static string HandleDocumentCaching( string targetFolder, DocumentVersion document, bool overwritingFile )
        {
            string message = "";
            if ( document == null || document.FileName == null || document.FileName.Trim().Length == 0 )
            {
                message = "Error - an invalid or incomplete document was encountered.";
                return message;
            }
            string destFile = targetFolder + "\\" + document.FileName;
            try
            {
                if ( System.IO.File.Exists( destFile ) )
                {
                    //may want to return path for display in a link?
                    if ( overwritingFile )
                    {
                        if ( document.HasDocument() == false )
                        {
                            document = Document_Version_Get( document.RowId );
                        }

                        FileSystemHelper.HandleDocumentCaching( targetFolder, document, true );
                        
                    }
                }
                else
                {
                    if ( document.HasDocument() == false )
                    {
                        document = Document_Version_Get( document.RowId );
                    }
                    //download
                    FileSystemHelper.HandleDocumentCaching( targetFolder, document, true );
                    
                }

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".HandleDocumentCaching() - Unexpected error encountered while retrieving document:<br/>" + ex.Message );

                message = thisClassName + ".HandleDocumentCaching() - Unexpected error encountered. You could try closing the form and then try again. System Administration has been notified)<br/>" + ex.ToString();
            }

            return message;
        }//

    }
}
