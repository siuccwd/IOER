using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Business;
//using ILPathways.DAL;
using IOER.Library;
using ILPathways.Utilities;
using MyManager = Isle.BizServices.ContentServices;

namespace IOER.Content
{
    /// <summary>
    /// Show a document from the Document.Version table
    /// </summary>
    public partial class Show : BaseAppPage
    {
        const string thisClassName = "Content/Show.aspx";
        protected void Page_Load( object sender, EventArgs e )
        {

            lblMessage.Text = "";
            if ( !Page.IsPostBack )
            {
                this.InitializeForm();

            }
        }//

        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        private void InitializeForm()
        {
            DocumentVersion entity = new DocumentVersion();
            MyManager mgr = new MyManager(); 

            //get actual requested document id
            string rowId = this.GetRequestKeyValue( "rid", "" );
            try
            {
                if ( rowId.Length != 36 )
                {
                    lblMessage.Text = "Invalid request";
                    return;
                }

                //get
                //need to add security checks. May need to go thru content (in case had privacy at content level, but not CS level) and/or content supplement 
                //so may pass the attachment id??
                entity = mgr.DocumentVersionGet( rowId );

                if ( entity.HasValidRowId() )
                {
                    if ( entity.MimeType.Length > 0 )
                    {
                        Response.ContentType = entity.MimeType;
                    }
                    else
                    {
                        //guess, future, derive from file extension
                        Response.ContentType = "image/jpeg";
                    }
                    Response.BinaryWrite( entity.ResourceData );
                }
                else
                {
                    lblMessage.Text = "Invalid request";
                    //log condition
                    string queryString = HttpContext.Current.Request.QueryString.ToString();
                    LoggingHelper.LogError( string.Format( thisClassName + ".InitializeForm - DocumentVersion record not found for passed id of: {0} for url: {1}", rowId, queryString ), false );
                }

            }
            catch ( Exception ex )
            {
                //Action??		- display message and close form??	
                LoggingHelper.LogError( ex, thisClassName + ".InitializeForm - Unexpected error encountered" );
                //can't show anything to the user
                lblMessage.Text = "Invalid request";
            }


        }	// End 
    }
}