using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;

using ILPathways.DAL;
using ILPathways.Library;
using ILPathways.Utilities;
using ILPathways.classes;
using ILPathways.Controllers;
using LRWarehouse.Business;
using LRWarehouse.DAL;

using MyManager = LRWarehouse.DAL.LRManager;
using MyVersionManager = LRWarehouse.DAL.ResourceVersionManager;
using MyEntity = LRWarehouse.Business.Patron;



namespace ILPathways.LRW.controls
{
    public partial class LR_Detail : BaseUserControl
    {
        string thisClassName = "LR_Detail";
        MyManager myManager = new MyManager();
        MyVersionManager myVersionManager = new MyVersionManager();
        LResourceDataHelper lrHelper = new LResourceDataHelper();

        #region Properties
        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string FormSecurityName
        {
            get { return this.txtFormSecurityName.Text; }
            set { this.txtFormSecurityName.Text = value; }
        }
        /// <summary>
        /// INTERNAL PROPERTY: CurrentRecord
        /// Set initially and store in ViewState
        /// </summary>
        public ResourceVersion CurrentRecord
        {
            get { return ViewState[ "CurrentRecord" ] as ResourceVersion; }
            set { ViewState[ "CurrentRecord" ] = value; }
        }

        public int urlStatusCode = 0;


        /// <summary>
        /// save clusters dataset for use in check for any changes on update
        /// </summary>
        protected DataSet CurrentClusterList
        {
            get
            {
                if ( ViewState[ "CurrentClusterList" ] != null )
                {
                    DataSet ds = ( DataSet ) ViewState[ "CurrentClusterList" ];
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            set { ViewState[ "CurrentClusterList" ] = value; }
        }

        protected DataSet CurrentGradeLevels
        {
            get
            {
                if ( ViewState[ "CurrentGradeLevels" ] != null )
                {
                    DataSet ds = ( DataSet ) ViewState[ "CurrentGradeLevels" ];
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            set { ViewState[ "CurrentGradeLevels" ] = value; }
        }
        protected DataSet CurrentIntendedAudience
        {
            get
            {
                if ( ViewState[ "CurrentIntendedAudience" ] != null )
                {
                    DataSet ds = ( DataSet ) ViewState[ "CurrentIntendedAudience" ];
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            set { ViewState[ "CurrentIntendedAudience" ] = value; }
        }

        protected DataSet CurrentEducationUse
        {
            get
            {
                if ( ViewState[ "CurrentEducationUse" ] != null )
                {
                    DataSet ds = ( DataSet ) ViewState[ "CurrentEducationUse" ];
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            set { ViewState[ "CurrentEducationUse" ] = value; }
        }
        #endregion

        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
               //InitializeForm();

            }

        }//

        public void InitializeForm()
        {


            CurrentRecord = new ResourceVersion();
            commentsUpdatePanel.Visible = false;
            if ( IsUserAuthenticated() )
            {
                pnlLoginToTag.Visible = false;
                FormPrivileges = SecurityManager.GetGroupObjectPrivileges( WebUser, FormSecurityName );
                if ( FormPrivileges.CanUpdate() == true )
                {
                    pnlTaggingAndComments.Visible = true;
                }
                else
                {
                    pnlTaggingAndComments.Enabled = false;
                    //pnlTaggingAndComments.Visible = false;
                }
                if (WebUser.UserName.ToLower().Equals("mparsons"))
                    commentsUpdatePanel.Visible = true;
            }
            else
            {
                //still want to show values
                if ( showingLoginSection.Text.Equals("yes"))
                    pnlLoginToTag.Visible = true;
                pnlTaggingAndComments.Enabled = false;
                txtComments.Text = "You must be authenticated in order to post comments";
                txtReason.Text = "You must be authenticated in order to report issues";
            }

            //this.FormPrivileges = SecurityManager.GetGroupObjectPrivileges( this.WebUser, FormSecurityName );
            string url = HttpContext.Current.Request.Url.AbsolutePath.ToString();
            if ( url.IndexOf( "aspx" ) > -1 )
            {
                url = url.Substring( 0, url.IndexOf( "aspx" ) + 4 );
                // publisherDisplayTemplate.Text = publisherDisplayTemplate.Text.Replace( "lrtestsearch.aspx", url );
            }

            CheckRecordRequest();

            //if ( Session[ "currentPatron" ] != null )
            //{
            //    pnlTaggingAndComments.Visible = true;
            //    pnlLoginToTag.Visible = false;
            //    CurrentUserId = ( ( MyEntity ) Session[ "currentPatron" ] ).Id;
            //}
            //else
            //{
            //    pnlTaggingAndComments.Visible = false;
            //    pnlLoginToTag.Visible = true;
            //    CurrentUserId = -1;
            //}
            //if ( Session[ "currentUserId" ] != null ) { lblSubjects.Text = Session[ "currentUserId" ].ToString(); }
            //lblSubjects.Text = lblSubjects.Text + " " + CurrentUserId.ToString() + " " + ViewState["CurrentUserId"].ToString();

        }

        #region retrieval
        /// <summary>
        /// Check the request type for the form
        /// </summary>
        private void CheckRecordRequest()
        {
            //check for the resourceId
            int pid = this.GetRequestKeyValue( "vid", 0 );

            if ( pid == 0 )
            {
                detailPanel.Visible = false;
                SetConsoleErrorMessage( "Error: A valid record Identifier must be provided" );
            }
            else
            {
                Get( pid );
            }


        }	// End 


        /// <summary>
        /// Get - retrieve form data
        /// </summary>
        /// <param name="recId"></param>
        private void Get( int recId )
        {
            try
            {
                //get record
                ResourceVersion entity = myVersionManager.Display( recId );

                if ( entity == null || entity.IsValid == false )
                {
                    detailPanel.Visible = false;
                    this.SetConsoleErrorMessage( "Sorry the requested record does not exist" );
                    return;

                }
                else
                {
                    PopulateForm( entity );
                }

            }
            catch ( System.Exception ex )
            {
                //Action??		- display message and close form??	
                LoggingHelper.LogError( ex, thisClassName + ".Get() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );

            }

        }	// End method

        /// <summary>
        /// Populate the form 
        ///</summary>
        private void PopulateForm( ResourceVersion entity )
        {
            CurrentRecord = entity;
            
            detailPanel.Visible = true;
            this.lblTitle.Text = entity.Title;
            this.lblDescription.Text = CleanDescription( entity.Description );
            this.lblSubjects.Text = entity.Subjects.Trim(new char[] { ',' });
            FormatSubjectsAsLinks( entity.Subjects );

            //this.lblKeywords.Text = entity.Keywords.Trim( new char[] { ',' } );
            FormatKeywordsAsLinks( entity.Keywords );

            this.lblEducationLevels.Text = entity.EducationLevels;
            this.lblAccessRights.Text = entity.AccessRights;
            lblConditions.Text = FormatRightsUrl( entity.Rights );

            if ( entity.ResourceUrl.Length > 0 && CanPreview( entity.ResourceUrl ) )
            {
                abcPreview.Visible = true;
                tmbPanel.Visible = false;
            }
            else
            {
                abcPreview.Visible = true;
                tmbPanel.Visible = false;
            }

            try
            {
                if ( entity.ResourceUrl.Length > 0 )
                {
                    //validate the target url
                    var request = ( HttpWebRequest ) WebRequest.Create( entity.ResourceUrl );
                    var response = ( HttpWebResponse ) request.GetResponse();
                    //TODO - should do a notify or just initiate action to inactivate??
                    //don't this far if an error occurs
                    this.urlStatusCode = ( int ) response.StatusCode;
                }
            }
            catch ( WebException ex ) //Apparently this will throw an error if the code is 4xx or 5xx instead of simply returning normally with a 4xx/5xx status code
            {
                //TBD
                string url = "";
                if ( ex.Message.IndexOf( "(404) Not Found" ) > -1
                  || ex.Message.IndexOf( "(403) Forbidden" ) > -1
                  || ex.Message.IndexOf( "The remote name could not be resolved" ) > -1
                    )
                    this.urlStatusCode = 404;
                else if ( ex.Message.IndexOf( "(500) Internal Server Error" ) > -1
                       || ex.Message.IndexOf( "(405) Method Not Allowed" ) > -1
                       || ex.Message.IndexOf( "The connection was closed unexpectedly" ) > -1
                    )
                {
                    //might be OK
                    LoggingHelper.DoTrace( 5, thisClassName + string.Format( ".PopulateForm. url validation ({0}), \r\nLRId: {1}", entity.ResourceUrl, entity.ResourceId.ToString() ) + "\r\nmessage: " + ex.Message );
                    this.urlStatusCode = 500;
                }
                else
                {
                    if ( entity.ResourceUrl != null && entity.ResourceUrl.Length > 0 )
                        url = entity.ResourceUrl;

                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".PopulateForm. url validation ({0}), \r\nLRId: {1}", url, entity.ResourceId.ToString() ), false );
                }

            }
            catch ( Exception ex ) 
            {
                //TBD
                string url = "";
                if ( entity.ResourceUrl != null && entity.ResourceUrl.Length > 0 )
                    url = entity.ResourceUrl;

                LoggingHelper.LogError( ex, thisClassName + string.Format( ".PopulateForm. url validation ({0}), \r\nLRId: {1}", url, entity.ResourceId.ToString() ), false );

            }

            GetClusters( entity.ResourceId.ToString() );
            GetGradeLevels( entity.ResourceId.ToString() );
            GetIntendedAudience( entity.ResourceId.ToString() );
            GetEducationUse( entity );

            if ( entity.Publisher != null && entity.Publisher.Length > 4 && entity.Publisher.ToLower() != "unknown" )
            {
                lblPublisher.Text = string.Format( this.publisherDisplayTemplate.Text, entity.Publisher );
                lblPublisher.Visible = true;
            }
            else
            {
                lblPublisher.Text = this.noPublisherTemplate.Text;
                lblPublisher.Visible = true;
            }

            if ( !IsTestEnv() && this.IsUserAuthenticated() == false )
            {
                this.clusterPanel.Enabled = false;
                this.gradeLevelsPanel.Enabled = false;
                this.intendedAudiencePanel.Enabled = false;
                this.ratingPanel.Enabled = false;
            }
            lblCreator.Text = CurrentRecord.Creator;
            lblResourceUrl.Text = CurrentRecord.ResourceUrl;
            lblLanguageList.Text = CurrentRecord.LanguageList;
            lblAudienceList.Text = CurrentRecord.AudienceList;
            lblResourceTypesList.Text = CurrentRecord.ResourceTypesList;

        }//
        public string CleanDescription( string description )
        {
            string text  = description;
           
            text = text.Replace( "&nbsp;", " " );
            text = text.Replace( "&lt;p&gt;", "" );
            text = text.Replace( "&lt;/p&gt;", "" );
            text = text.Replace( "&amp;nbsp;", " " );

            return text;
            // }
        }
        protected string FormatRightsUrl( string url )
        {
            string formatted = "";
            if ( url == null || url.Trim().Length < 15 )
                return "";

            if ( url.Trim().ToLower().IndexOf( "http://" ) != 0 )
            {
                if ( usingRightsSlider.Text.Equals( "yes" ) )
                    formatted = string.Format( textRightsTemplate.Text, url );
                else
                {
                    formatted = url;
                }
                return formatted;
            }
                

            //TODO - extract domain name from url
            if ( url.ToLower().IndexOf( "http://creativecommons.org/licenses/" ) > -1 )
            {
                formatted = FormatCCRightsUrl( url );
            }
            else
            {
                formatted = string.Format( formattedSourceTemplate.Text, url, "Custom Rights" );
            }

            return formatted;
        }

        protected string FormatCCRightsUrl( string url )
        {
            string formatted = string.Format( formattedCCRightsUrl.Text, url, "Creative Commons" );
            return formatted;
        }

        public bool CanPreview( string resourceUrl )
        {
            bool isValid = false;

            if ( resourceUrl.ToLower().IndexOf( ".htm" ) > -1
                || resourceUrl.ToLower().IndexOf( ".pdf" ) > -1 )
                isValid = true;

            return isValid;
        }//
        public void FormatSubjectsAsLinks( string items )
        {
            if ( items.Trim().Length == 0 )
                return;

            string comma = "";
            string list = "";
            // separate individual items between commas
            string[] eValue = items.Split( new char[] { ',' } );
            foreach ( string item in eValue )
            {
                list += comma + string.Format( this.subjectSearchLinkTemplate.Text, item.Trim(new char[] { '"','.', ' ' }) );
                comma = ", ";
            }
            list = list.Trim();
            this.lblSubjects.Text = list;
        }

        public void FormatKeywordsAsLinks( string items )
        {
            if ( items.Trim().Length == 0 )
                return;

            string comma = "";
            string list = "";
            // separate individual items between commas
            string[] eValue = items.Split( new char[] { ',' } );
            foreach ( string item in eValue )
            {
                string text = item.Trim( new char[] { '"', ' ' } );
                if ( text.Length > 2 )
                {
                    list += comma + string.Format( this.keywordSearchLinkTemplate.Text, text );
                    comma = ", ";
                }
            }
            list = list.Trim();
            if ( list.StartsWith( "," ) )
                list = list.Substring( 1 );
            this.lblKeywords.Text = list;
        }
        #endregion

        #region Rating methods
        /// <summary>
        /// Run custom code when the user rates something and then return a custom string
        /// to the JavaScript client
        /// </summary>
        /// <param name="sender">Rating control</param>
        /// <param name="e">RatingEventArgs</param>
        protected void ItemRating_Changed( object sender, RatingEventArgs e )
        {
            Thread.Sleep( 400 );
            e.CallbackResult = "Update done. Value = " + e.Value + " Tag = " + e.Tag;

            // cast rating control which has initiated the call:
            AjaxControlToolkit.Rating myRating =
                      ( AjaxControlToolkit.Rating ) sender;
            // regular expression which will help identifying row number: 
            System.Text.RegularExpressions.Regex rexLineNo =
              new System.Text.RegularExpressions.Regex( "ctl\\d+" );

            // update the record based on the recodrd id
            this.updateRating( CurrentRecord.ResourceId.ToString(), e.Value );
            // the above line does the following: 
            // rexLineNo.Match(myRating.UniqueID).ToString()
            //     - finds "ctl and line number  
        }

        /// <summary>
        /// Updates given product with new rating
        /// </summary>
        /// <param name="resourceId">Product Id</param>
        /// <param name="Rating">New rating</param>
        private void updateRating( string resourceId, string rating )
        {
            //TODO - call proc to update rating
            LoggingHelper.DoTrace( 1, string.Format( "TODO - call proc to update rating, Id={0}, rating = {1}", resourceId, rating ) );
            // put your values into parameters:
            //OleDbParameter paramRating = new OleDbParameter( "@Rating", Rating );
            //OleDbParameter paramresourceId =
            //   new OleDbParameter( "@resourceId", resourceId );


            //using ( OleDbCommand cmd = new OleDbCommand(
            //    "UPDATE Products SET CustomerRating " +
            //    "= @Rating WHERE Products.ProductID=@resourceId",
            //    new OleDbConnection(
            //     ConfigurationManager.ConnectionStrings[ "db1" ].ToString() ) ) )
            //{
            //    cmd.Parameters.Add( paramRating );
            //    cmd.Parameters.Add( paramresourceId );
            //    cmd.Connection.Open();
            //    cmd.ExecuteNonQuery();

            //}
        }
        #endregion

   

        protected void cbxlCluster_SelectedIndexChanged( object sender, EventArgs e )
        {
            string statusMessage = "";

            ResourceClusterManager manager = new ResourceClusterManager();
            lrHelper.CheckBoxListUpdateItem( CurrentRecord.ResourceId.ToString(),
                manager, CurrentClusterList, cbxlCluster,
                WebUser.Id, ref statusMessage );

            //bool doingAudit = false;
            //UpdateClusters( CurrentRecord.ResourceId.ToString(), ref statusMessage, ref doingAudit );

        }

        //protected bool UpdateClusters( string pResourceId, ref string statusMessage, ref bool doingAudit )
        //{
        //    bool isValid = true;
        //    string messages = "";
        //    statusMessage = "";
        //    ResourceClusterManager cmgr = new ResourceClusterManager();
        //    try
        //    {
        //        DataSet ds = new DataSet();
        //        //Retrieve tags list from session
        //        //first time, there won't be any tags retrieved!
        //        if ( this.CurrentClusterList != null )
        //        {
        //            ds = CurrentClusterList;
        //            StringBuilder deletedItems = new StringBuilder( "" );
        //            StringBuilder addedItems = new StringBuilder( "" );
        //            int counter = 0;

        //            //Might be OK
        //            foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
        //            {
        //                //Build the added and deleted string
        //                if ( cbxlCluster.Items[ counter ].Selected
        //                    && DatabaseManager.GetRowColumn( dr, "IsSelected", false ) == false )
        //                {
        //                    addedItems.Append( dr[ "id" ] ).Append( "|" );

        //                }
        //                else if ( !cbxlCluster.Items[ counter ].Selected
        //                               && DatabaseManager.GetRowColumn( dr, "IsSelected", false ) == true )
        //                {
        //                    deletedItems.Append( dr[ "id" ] ).Append( "|" );
        //                }
        //                counter++;
        //            }

        //            if ( addedItems.Length > 0 || deletedItems.Length > 0 )
        //            {
        //                //Update the database with the changes
        //                cmgr.ApplyChanges( pResourceId, 1, addedItems.ToString(), deletedItems.ToString() );
        //            }

        //        }
        //        else
        //        {

        //            foreach ( ListItem li in this.cbxlCluster.Items )
        //            {
        //                if ( li.Selected == true )
        //                {
        //                    isValid = cmgr.Insert( pResourceId, Int32.Parse( li.Value ), ref statusMessage );
        //                    if ( !isValid )
        //                    {
        //                        statusMessage += messages + "<br/>";
        //                    }
        //                }
        //            }
        //            if ( statusMessage.Length > 0 )
        //                isValid = false;
        //        }
        //    }
        //    catch ( Exception ex )
        //    {
        //        this.LogError( ex, thisClassName + ".UpdateClusters" );
        //        isValid = false;
        //    }

        //    //need to refresh CurrentClusterList (OK, done in the Get)
        //    DataSet ds2 = cmgr.SelectedCodes( pResourceId );
        //    CurrentClusterList = ds2;

        //    return isValid;
        //}

        private void GetClusters( string resourceId )
        {
            // Load checkboxes
            ResourceClusterManager manager = new ResourceClusterManager();
            CurrentClusterList = lrHelper.PopulateCheckBoxList( resourceId, manager, cbxlCluster );
            //cbxlCluster.Items.Clear();

            //DataSet ds = new ResourceClusterManager().SelectedCodes( resourceId );
            //CurrentClusterList = ds;

            //if ( ds != null && ds.Tables.Count > 0 && ds.Tables[ 0 ].Rows.Count > 0 )
            //{
            //    foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
            //    {
            //        ListItem item = new ListItem();
            //        item.Value = DatabaseManager.GetRowColumn( dr, "id", "0" );
            //        item.Text = dr[ "Title" ].ToString().Trim();
            //        bool isSelected = DatabaseManager.GetRowColumn( dr, "IsSelected", false );

            //        item.Selected = isSelected;

            //        cbxlCluster.Items.Add( item );

            //    } //end foreach
            //}

        }

        #region education levels
        private void GetGradeLevels( string resourceId )
        {
            // Load checkboxes
            EducationLevelManager manager = new EducationLevelManager();
            CurrentGradeLevels = lrHelper.PopulateCheckBoxList( resourceId, manager, cbxlGradeLevels );

            //cbxlGradeLevels.Items.Clear();
            //DataSet ds = manager.Select( resourceId );
            //CurrentGradeLevels = ds;

            //if ( ds != null && ds.Tables.Count > 0 && ds.Tables[ 0 ].Rows.Count > 0 )
            //{
            //    foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
            //    {
            //        ListItem item = new ListItem();
            //        item.Value = DatabaseManager.GetRowColumn( dr, "id", "0" );
            //        item.Text = dr[ "Title" ].ToString().Trim();
            //        bool isSelected = DatabaseManager.GetRowColumn( dr, "IsSelected", false );

            //        item.Selected = isSelected;
            //        if (item.Text.ToLower().Equals("unknown"))
            //            item.Enabled = false;
            //        else 
            //            cbxlGradeLevels.Items.Add( item );

            //    } //end foreach
            //}

        }

        protected void cbxlGradeLevels_SelectedIndexChanged( object sender, EventArgs e )
        {
            string statusMessage = "";
            EducationLevelManager manager = new EducationLevelManager();
            lrHelper.CheckBoxListUpdateItem( CurrentRecord.ResourceId.ToString(),
                            manager, CurrentGradeLevels, cbxlGradeLevels,
                            WebUser.Id, ref statusMessage );


        }
        #endregion

        #region Education Use
        private void GetEducationUse( ResourceVersion entity )
        {
            // Load checkboxes
            ResourceEducationUseManager manager = new ResourceEducationUseManager();
            CurrentEducationUse = lrHelper.PopulateCheckBoxList( entity.ResourceIntId, manager, cbxEducationUse );
        }

        protected void cbxEducationUse_SelectedIndexChanged( object sender, EventArgs e )
        {
            string statusMessage = "";
            ResourceEducationUseManager manager = new ResourceEducationUseManager();
            CurrentEducationUse = lrHelper.CheckBoxListUpdateApply( CurrentRecord.ResourceIntId,
                            manager, CurrentEducationUse, cbxEducationUse,
                            WebUser.Id, ref statusMessage );


        }
        #endregion

        #region Intended Audience
        private void GetIntendedAudience( string resourceId )
        {
            ResourceIntendedAudienceManager manager = new ResourceIntendedAudienceManager();
            CurrentIntendedAudience = lrHelper.PopulateCheckBoxList( resourceId, manager, cbxlIntendedAudience );

        }

        protected void btnApplyAudienceUpdates_Click( object sender, EventArgs e )
        {
            string statusMessage = "";

            ResourceIntendedAudienceManager manager = new ResourceIntendedAudienceManager();
            lrHelper.CheckBoxListUpdateItem( CurrentRecord.ResourceId.ToString(),
                        manager, CurrentIntendedAudience, cbxlIntendedAudience,
                        WebUser.Id, ref statusMessage );
        }
        protected void cbxlIntendedAudience_SelectedIndexChanged( object sender, EventArgs e )
        {
            string statusMessage = "";

            ResourceIntendedAudienceManager manager = new ResourceIntendedAudienceManager();
            lrHelper.CheckBoxListUpdateItem( CurrentRecord.ResourceId.ToString(),
                        manager, CurrentIntendedAudience, cbxlIntendedAudience,
                        WebUser.Id, ref statusMessage );
        }
        
        #endregion

        protected void btnReportIssue_Click( object sender, EventArgs e )
        {
            txtReason.Text = FormHelper.CleanText( txtReason.Text );
            //do bad word check
            if ( BadWordChecker.CheckForBadWords( this.txtReason.Text ) )
            {
                txtReason.Text = "********************";
                SetConsoleErrorMessage( "Inappropriate content found, ignoring request" );
            }
        }

        protected void btnAddComment_Click( object sender, EventArgs e )
        {
            commentMessage.Text = "";
            commentMessage.Visible = true;
            txtComments.Text = FormHelper.CleanText( txtComments.Text );
            //do bad word check
            if ( BadWordChecker.CheckForBadWords( this.txtComments.Text ) )
            {
                txtComments.Text = "********************" + Environment.NewLine + "Inappropriate content found, ignoring request";
                commentMessage.Text = "Inappropriate content found, ignoring request<br/>";
                commentMessage.Visible = true;
                SetConsoleErrorMessage( "Inappropriate content found, ignoring request" );
            }
            else if ( txtComments.Text.Trim().Length < 15 )
            {
                SetConsoleErrorMessage( "Please provide a comment of a meaningful length" );
                commentMessage.Text = "Please provide a comment of a meaningful length<br/>";
                commentMessage.Visible = true;
            }
            else
            {
                string statusMessage = "";
                Patron user = ( Patron ) WebUser;
                ResourceCommentManager mgr = new ResourceCommentManager();
                //TODO - we may initially delay post of comments, pending approval
                ResourceComment entity = new ResourceComment();
                entity.ResourceId = CurrentRecord.ResourceId;
                entity.ResourceIntId = CurrentRecord.ResourceIntId;
                entity.Comment = txtComments.Text;
                entity.CreatedBy = user.FullName();
                //TODO - add commenter context - from Patron table
                //entity.Commenter = user.TBD;
                entity.IsActive = true;
                entity.CreatedById = user.Id;

                int id = mgr.Create( entity, ref statusMessage );
                if ( id > 0 )
                {
                    commentMessage.Text = "Comment was saved<br/>";
                    txtComments.Text = "";
                    //commentMessage.Visible = false;
                }
                else
                {
                    SetConsoleErrorMessage( "Error encountered saving comment:<br/>" + statusMessage );
                    commentMessage.Text = "Error encountered saving comment<br/>";
                }
            }
            //
        }

        
    public void SaveProfile(object sender, EventArgs e)
    {
    }


    } //class
}
