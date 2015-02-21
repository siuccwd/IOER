using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


using Ionic.Zip;

using Isle.BizServices;
using AcctManager = Isle.BizServices.AccountServices;

using ILPathways.Controllers;
using ILPathways.Library;
using ILPathways.Business;
using ILPathways.Utilities;
using LRB = LRWarehouse.Business;


namespace ILPathways.Repository
{
    public partial class Download : BaseAppPage
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            HandleRequest();
        }

        private void HandleRequest()
        {
            int nodeId = 2220;
            nodeId = this.GetRequestKeyValue( "nid", 0 );
            if (nodeId > 0)
                DownloadNodeFiles(nodeId);
        }
        
        protected void DownloadNodeFiles(int nodeId)
        {
            

            LRB.Patron user = new LRB.Patron(); 
            //will need to pass user if present
            if ( IsUserAuthenticated() )
                user = ( LRB.Patron )WebUser;

            ContentItem node = new ContentServices().GetCurriculumNode( nodeId, user, false );
            using (ZipFile zip = new ZipFile())
            {
                if ( node.HasChildItems )
                {
                    foreach ( ContentItem item in node.ChildItems )
                    {
                       // var kid = new ContentItem();
                        var kid = new JSONNode();
                        kid.title = item.Title;
                        kid.description = ( item.Description == "" ? item.Summary : item.Description );
                        if ( item.CanViewDocument )
                        {
                            string fileUrl = ValidateDocumentOnServer( item, item.RelatedDocument );
                            if ( fileUrl.Length > 10 )
                            {


                                zip.AddFile( fileUrl, Path.GetFileName( fileUrl ) );
                            }
                            kid.documentUrl = item.DocumentUrl;
                            kid.resourceUrl = item.ResourceFriendlyUrl;
                        }
                        else
                        {
                            //TDO - add something to manifest to indicate this
                        }

                        //output.children.Add( kid );
                    }
                    string documentFolder = FileResourceController.DetermineDocumentPath( node );
                    zip.Save( documentFolder + "/test.zip" );

                   // lblResults.Text = documentFolder + "/test.zip";
                    SetConsoleSuccessMessage( documentFolder + "/test.zip" );
                }
            }
        }
        private string ValidateDocumentOnServer( ContentItem parentEntity, DocumentVersion doc )
        {
            string fileUrl = "";

            try
            {
                string documentFolder = FileResourceController.DetermineDocumentPath( parentEntity );
                string message = FileSystemHelper.HandleDocumentCaching( documentFolder, doc );
                if ( message == "" )
                {
                    //blank returned message means ok
                    fileUrl = FileResourceController.DetermineDocumentUrl( parentEntity, doc.FileName );
                }
                else
                {
                    //error, should return a message
                    this.SetConsoleErrorMessage( message );
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "Curriculum1.ValidateDocumentOnServer() - Unexpected error encountered while retrieving document" );

                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }
            return fileUrl;
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