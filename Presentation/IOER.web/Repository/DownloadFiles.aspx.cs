using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using Ionic.Zip;

using Isle.BizServices;
using AcctManager = Isle.BizServices.AccountServices;

using IOER.Controllers;
using IOER.Library;
using ILPathways.Business;
using ILPathways.Utilities;
//using LRB = LRWarehouse.Business;
using Patron = LRWarehouse.Business.Patron;

namespace IOER.Repository
{
    /// <summary>
    /// Handle the download of a curriculum node
    /// </summary>
    public partial class DownloadFiles : BaseAppPage
    {
        string Level1 = "Curriculum";
        string Level2 = "Module";
        string Level2Plural = "Modules";
        string Level3 = "Unit";
        string Level3Plural = "Units";
        string Level4 = "Lesson";
        string Level4Plural = "Lessons";
        string Level5 = "Activity";
        string Level5Plural = "Activities";

        protected void Page_Load( object sender, EventArgs e )
        {
            HandleRequest();
        }

        private void HandleRequest()
        {
            bool includingChildren = FormHelper.GetRequestKeyValue( "all", true );
            int nodeId = 0;
            nodeId = this.GetRequestKeyValue( "nid", 0 );
            try
            {
                if ( nodeId > 0 )
                {
                    DownloadNodeFiles2( nodeId, includingChildren );
                }
            }
            catch ( System.Threading.ThreadAbortException tae )
            {
                //LoggingHelper.LogError( tae, "DownloadFiles.HandleRequest()" );
                LoggingHelper.DoTrace( 5, "DownloadFiles.HandleRequest() ThreadAbortException: " + tae.Message );
                mainPanel.Visible = false;
                errorPanel.Visible = true;
            }
            catch ( Exception ex )
            {
                LoggingHelper.DoTrace( 1, "DownloadFiles.HandleRequest() Exception: " + ex.Message );
                mainPanel.Visible = false;
                errorPanel.Visible = true;
            }
        }

        protected void DownloadNodeFiles2( int nodeId, bool includingChildren )
        {
            string downloadFolder = UtilityManager.GetAppKeyValue( "path.WorkOutputPath", "C:\\IOER\\ContentDocs\\" );
            string downloadUrl = UtilityManager.GetAppKeyValue( "path.WorkOutputUrl", "/ContentDocs/" );
            Patron user = new Patron();
            //will need to pass user if present
            if ( IsUserAuthenticated() )
                user = ( Patron )WebUser;

            ContentItem entity = new CurriculumServices().DownloadCurriculumNode( nodeId, user, includingChildren, false );
            if ( entity == null || entity.Id == 0 )
            {
                this.lblResults.Text = "Invalid request - item not found";
                return;
            }

            new ActivityBizServices().DownloadHit( entity, user, "Download learning list" );

            DateTime start = DateTime.Now;

            string zipName = entity.ContentType + " - " + ResourceBizService.FormatFriendlyTitle( entity.Title ) + ".zip"; 
            //TODO - handle caching - check if file already downloaded
            //          ===> need to handle where last version may have contained privileges files
            if ( checkForExistingFile.Text.Equals( "yes" ) )
            {
                string filePath = downloadFolder + zipName;
                if ( System.IO.File.Exists( filePath ) )
                {
                    //check time
                    if ( doingImmediateDownload.Text == "yes" )
                    {
                        Response.Redirect( downloadUrl + zipName, true );
                    }
                    else
                    {
                        if ( IsUserAuthenticated() && WebUser.UserName == "mparsons" )
                            SetConsoleSuccessMessage( downloadUrl + zipName + "<br/> USED CACHED COPY" );
                        else
                            SetConsoleSuccessMessage( "The requested file is ready for download:<br/> " + zipName );
                    }
                }
            }

            string zipFolder = ResourceBizService.FormatFriendlyTitle( entity.Title );

            string summary = ( entity.Description == "" ? entity.Summary : entity.Description );

            string manifest = "Title: " + entity.Title
                        + "\\rDescription: " + ( entity.Description == "" ? entity.Summary : entity.Description )
                        + "\\rFiles:";

            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration( "1.0", "UTF-8", null );
            doc.AppendChild( docNode );

            XmlNode nodes = doc.CreateElement( "Nodes" );
            doc.AppendChild( nodes );


            //top level node
            XmlNode node = doc.CreateElement( entity.ContentType.Replace( " ", "_" ) );
            node.Attributes.Append( AddAttribute( doc, "Title", entity.Title ) );
            node.Attributes.Append( AddAttribute( doc, "Description", summary ) );

            nodes.AppendChild( node );


            using ( ZipFile zip = new ZipFile() )
            {
                if ( entity.HasChildItems )
                {
                    CreateDownload( entity, doc, node, zip, zipFolder, ref manifest );

                    using ( StringWriter sw = new StringWriter() )
                    {
                        doc.Save( sw );
                        string xmlText = sw.ToString();
                        xmlText = xmlText.Replace( "utf-16", "utf-8" );
                        zip.AddEntry( "manifest.xml", xmlText );
                    }


                    zip.Save( downloadFolder + zipName );
                    DateTime end = DateTime.Now;
                    TimeSpan elapsed = end.Subtract( start );

                    hlFileUrl.Text = string.Format( "Download: {0}", zipName );
                    hlFileUrl.NavigateUrl = downloadUrl + zipName;
                    hlFileUrl.Visible = true;

                    if ( doingImmediateDownload.Text == "yes" )
                    {
                        Response.Redirect( downloadUrl + zipName, true );
                    }
                    else
                    {
                        if ( IsUserAuthenticated() && WebUser.UserName == "mparsons" )
                            SetConsoleSuccessMessage( downloadUrl + zipName + "<br/>Duration: " + elapsed.TotalSeconds.ToString() );
                        else
                            SetConsoleSuccessMessage( "The requested file is ready for download:<br/> " + zipName );
                    }


                }
                else
                {
                    XmlNode files = doc.CreateElement( "Files" );
                    node.AppendChild( files );
                    files.AppendChild( FormatNode( doc, "Summary", "Title", "No files" ) );

                    using ( StringWriter sw = new StringWriter() )
                    {
                        doc.Save( sw );
                        string xmlText = sw.ToString();
                        xmlText = xmlText.Replace( "utf-16", "utf-8" );
                        zip.AddEntry( "manifest.xml", xmlText );
                    }

                    zip.Save( downloadFolder + zipName );
                    DateTime end = DateTime.Now;
                    TimeSpan elapsed = end.Subtract( start );
                    // lblResults.Text = documentFolder + "/test.zip";
                    hlFileUrl.Text = string.Format( "Download: {0}", zipName );
                    hlFileUrl.NavigateUrl = downloadUrl + zipName;
                    hlFileUrl.Visible = true;
                    SetConsoleSuccessMessage( "No files were found for the requested item" );

                }
            }
        }

        private void CreateDownload( ContentItem entity, XmlDocument doc, XmlNode node, ZipFile zip, string zipFolder, ref string manifest )
        {

            bool hasFilesNodeBeenAdded = false;

            XmlNode files = doc.CreateElement( "Files" );

            bool hasChildNodeBeenAdded = false;
            XmlNode children = doc.CreateElement( Level4Plural.Replace( " ", "_" ) );
            string linksPage = "";
            string lastItem = "";
            string lastFile = "";
            string filePath = "";
            int privateCntr = 0;

            foreach ( ContentItem item in entity.ChildItems )
            {
                lastItem = string.Format( "id: {0}, type: {1}, title: {2}", item.Id, item.TypeId, item.Title );

                try
                {
                    //assuming all docs are together, need to validate
                    //could now be mixed with urls?
                    if ( item.TypeId == ContentItem.DOCUMENT_CONTENT_ID )
                    {
                        if ( hasFilesNodeBeenAdded == false )
                        {
                            hasFilesNodeBeenAdded = true;
                            node.AppendChild( files );
                        }

                        // var kid = new ContentItem();
                        var kid = new JSONNode();
                        kid.title = item.Title;
                        //NOTE - 14-11-15 now docs will not have a description
                        kid.description = ( item.Description == "" ? item.Summary : item.Description );
                        kid.description = kid.description.Replace( "\"", "'" );
                        kid.description = kid.description.Replace( "“", "'" );
                        kid.description = kid.description.Replace( "”", "'" );
                        if ( item.CanViewDocument )
                        {
                            filePath = ValidateDocumentOnServer( item, false );
                            if ( filePath.Length > 10 )
                            {
                                try
                                {
                                    LoggingHelper.DoTrace(6, string.Format( "Zipping item: id: {0}, type: {1}, title: {2}, filePath: {3}", item.Id, item.TypeId, item.Title, filePath ));

                                    //adding duplicate file name (even if a different folder?), results in an argument exception
                                    if ( zip.ContainsEntry( filePath ) == false )
                                    {
                                        zip.AddFile( filePath, zipFolder );
                                    }
                                    else
                                    {
                                        //need to make it unique

                                        LoggingHelper.LogError( "DownloadFiles.CreateDownload. Duplicate file path: " + filePath, true );
                                        filePath = ValidateDocumentOnServer( item, true );
                                        //skip for now
                                        //zip.AddFile( filePath, zipFolder );
                                    }
                                }
                                catch ( ArgumentException aex )
                                {
                                    LoggingHelper.LogError( aex, string.Format("DownloadFiles.CreateDownload. Adding item to zip: Item: {0}. File: {1} ", lastItem, filePath) );
                                    manifest += "Title: UNEXPECTED ERROR ENCOUNTERED"
                                            + "\\rDescription: " + aex.Message
                                            + "\\rItem: " + lastItem;

                                }

                            }
                            kid.documentUrl = item.DocumentUrl;
                            kid.resourceUrl = item.ResourceFriendlyUrl;
                            manifest += "Title: " + item.Title
                                    + "\\rDescription: " + kid.description
                                    + "\\rFile: " + item.FileName;
                            files.AppendChild( FormatNode( doc, item.ContentType, item.Title, kid.description, item.FileName ) );
                        }
                        else
                        {
                            //TODO - add something to manifest to indicate this
                            manifest += "\\rPrivate document - not included"
                                    + "d\\rTitle: " + item.Title
                                    + "\\n\\Description: " + kid.description;

                            files.AppendChild( FormatNode( doc, item.ContentType, item.Title, kid.description, string.Format( "{0}. Private document - not included",privateCntr++) ) );
                        }
                    }
                    else if ( item.TypeId == ContentItem.EXTERNAL_URL_CONTENT_ID )
                    {
                        //URLs
                        if ( hasFilesNodeBeenAdded == false )
                        {
                            hasFilesNodeBeenAdded = true;
                            node.AppendChild( files );
                        }

                        var kid = new JSONNode();
                        kid.title = item.Title;
                        //prob no description
                        kid.description = ( item.Description == "" ? item.Summary : item.Description );
                        kid.description = kid.description.Replace( "\"", "'" );
                        kid.description = kid.description.Replace( "“", "'" );
                        kid.description = kid.description.Replace( "”", "'" );

                        //append url
                        //kid.description = kid.description.Length == 0 ? item.DocumentUrl : "\\r\\n" + item.DocumentUrl;

                        manifest += "Title: " + item.Title
                        + "\\rDescription: " + kid.description
                        + "\\rURL: " + item.DocumentUrl;

                        files.AppendChild( FormatNode( doc, item.ContentType, item.Title, "Website URL", item.DocumentUrl ) );

                    }
                    else
                    {
                        if ( hasChildNodeBeenAdded == false )
                        {
                            //new node level (ex. lessons)
                            string nodeLevel = item.ContentType + "s";
                            if ( nodeLevel.EndsWith( "ys" ) )
                                nodeLevel = nodeLevel.Replace( "ys", "ies" );

                            children = doc.CreateElement( nodeLevel.Replace( " ", "_" ) );
                            node.AppendChild( children );

                            hasChildNodeBeenAdded = true;
                        }

                        //child level node, ex. lesson
                        XmlNode child = doc.CreateElement( item.ContentType.Replace( " ", "_" ) );
                        child.Attributes.Append( AddAttribute( doc, "Title", item.Title ) );
                        string summary = ( item.Description == "" ? item.Summary : item.Description );
                        summary = summary.Replace( "\"", "'" );
                        summary = summary.Replace( "“", "'" );
                        summary = summary.Replace( "”", "'" );
                        child.Attributes.Append( AddAttribute( doc, "Description", summary ) );

                        children.AppendChild( child );
                        if ( item.HasChildItems )
                        {
                            string folder = ResourceBizService.FormatFriendlyTitle( item.Title );

                            CreateDownload( item, doc, child, zip, folder, ref manifest );
                        }
                        else
                        {
                            //XmlNode files2 = doc.CreateElement( "Files" );
                            //child.AppendChild( files2 );
                            //files.AppendChild( FormatNode( doc, "Summary", "Title", "No files" ) );
                        }
                    }

                }

                catch ( ArgumentException aex )
                {
                    LoggingHelper.LogError( aex, "DownloadFiles.CreateDownload. Item: " + lastItem );
                    manifest += "Title: UNEXPECTED ERROR ENCOUNTERED"
                            + "\\rDescription: " + aex.Message
                            + "\\rItem: " + lastItem;

                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, "DownloadFiles.CreateDownload. Item: " + lastItem );
                    manifest += "Title: UNEXPECTED ERROR ENCOUNTERED"
                            + "\\rDescription: " + ex.Message
                            + "\\rItem: " + lastItem;

                }
            } //end foreach
        }   //

        private XmlAttribute AddAttribute( XmlDocument doc, string attributeType, string attribute )
        {

            XmlAttribute nodeAttribute = doc.CreateAttribute( attributeType );
            nodeAttribute.Value = attribute;

            return nodeAttribute;
        }

        private XmlNode FormatNode( XmlDocument doc, string contentType, string attrName ,string attrValue)
        {
            XmlNode node = doc.CreateElement( contentType.Replace( " ", "_" ) );
            XmlAttribute nodeAttribute = doc.CreateAttribute( attrName );
            nodeAttribute.Value = attrValue;
            node.Attributes.Append( nodeAttribute );

            return node;
        }

        private XmlNode FormatNode( XmlDocument doc, string contentType, string title, string description, string filename)
        {
            XmlNode node = doc.CreateElement( contentType.Replace( " ", "_" ) );
            XmlAttribute nodeAttribute = doc.CreateAttribute( "Title" );
            nodeAttribute.Value = title;
            XmlAttribute nodeAttribute2 = doc.CreateAttribute( "Description" );
            nodeAttribute2.Value = description;
            XmlAttribute nodeAttribute3 = doc.CreateAttribute( "FileName" );
            nodeAttribute3.Value = filename;

            node.Attributes.Append( nodeAttribute );
            node.Attributes.Append( nodeAttribute2 );
            node.Attributes.Append( nodeAttribute3 );

            return node;
        }

        /// <summary>
        /// Validate file is on server and return path
        /// NOTE - change to use the stored file path
        /// </summary>
        /// <param name="parentEntity"></param>
        /// <param name="doc"></param>
        /// <param name="alteringFilename">if true, will need to alter the file name</param>
        /// <returns>file path to file</returns>
        private string ValidateDocumentOnServer( ContentItem parentEntity, bool alteringFilename )
        {
            string filePath = "";
            string documentFolder = "";
            DocumentVersion doc = new DocumentVersion();
            try
            {
                //15-01-06 mp - the main retrieve should be handling existance. 
                //              - if a the documentPath exists, then a successful check has been done. Or could check in case new methods are added without the same safe guards
                if ( parentEntity.FileLocation().Length > 10 && alteringFilename == false )
                {
                    if ( FileSystemHelper.DoesFileExist( parentEntity.FilePath, parentEntity.FileName ) )
                    {
                        return parentEntity.FileLocation();
                    }
                    //return parentEntity.DocumentPath;

                }


                //don't want to do this (ie get full version) but should be unlikely
                if ( parentEntity.HasDocument() == false )
                    doc = DocumentServices.Document_Version_Get( parentEntity.DocumentRowId );
                else
                    doc = parentEntity.RelatedDocument;

                if ( doc == null || doc.CreatedById == 0 )
                {
                    //should report!!
                    LoggingHelper.LogError( string.Format( "DownloadFiles.ValidateDocumentOnServer() - doc not found. ContentId: {0}", parentEntity.Id), true );
                    return "";
                }

                if ( alteringFilename )
                {
                    doc.FileName = DateTime.Now.Second.ToString() + "_" + doc.FileName;
                }


                //should use filePath and fileName from doc
                if ( FileSystemHelper.DoesFileExist( doc.FilePath, doc.FileName ) )
                {
                    return doc.FileLocation();
                }

                if ( doc.FilePath.Length > 0 )
                    documentFolder = doc.FilePath;
                else
                {
                    //NOTE: should try not to carry doc version, would be heavy with lots of docs!
                    //WARNING - this parent is not what we want. Prob should do a notify
                    FileResourceController.PathParts parts = FileResourceController.DetermineDocumentPathUsingParentItem( parentEntity );
                    documentFolder = parts.filePath;
                    LoggingHelper.LogError( string.Format( "DownloadFiles.ValidateDocumentOnServer() - doc FilePath not found, had to redo the cache. ContentId: {0}, Folder: {1}", parentEntity.Id, documentFolder ), true );
                }


                string message = FileSystemHelper.HandleDocumentCaching( documentFolder, doc, true );
                if ( message == "" )
                {
                    //blank returned message means ok
                    if ( documentFolder.Trim().EndsWith( "\\" ) )
                        filePath = documentFolder + doc.FileName;
                    else
                        filePath = documentFolder + "\\" + doc.FileName;
                    
                }
                else
                {
                    //error, should return a message
                    this.SetConsoleErrorMessage( message );
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "DownloadFiles.ValidateDocumentOnServer() - Unexpected error encountered while retrieving document" );
            }
            return filePath;
        }//

        public class JSONNode
        {
            public JSONNode()
            {
                resourceIDs = new List<int>();
                children = new List<JSONNode>();

            }
            public int id { get; set; }
            public int parentID { get; set; }
            public string resourceUrl { get; set; }
            public string documentUrl { get; set; }

            public string title { get; set; }
            public string description { get; set; }
            public string message { get; set; }
            public string contentType { get; set; }
            public List<int> resourceIDs { get; set; }
            public List<JSONNode> children { get; set; }

        }
    }

}