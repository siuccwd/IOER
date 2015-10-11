using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Text;

using LRWarehouse.Business;
using LRWarehouse.DAL;
using IOERUser = LRWarehouse.Business.Patron;

using IOER.Library;
//using ILPathways.DAL;
using Isle.BizServices;
using IOER.classes;
using IOER.Controllers;
using ILPathways.Utilities;
using ILPathways.Business;
using Isle.BizServices;
using System.Web.Script.Serialization;


namespace IOER.LRW.controls
{
    public partial class PublishResource_Narrow2 : BaseUserControl
    {

        #region Properties

        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string FormSecurityName
        {
            get { return this.txtFormSecurityName.Text; }
            set { this.txtFormSecurityName.Text = value; }
        }
        public int AuthoredResourceID
        {
            get { return Int32.Parse( this.txtContentSourceId.Text ); }
            set { this.txtContentSourceId.Text = value.ToString(); }
        }//
        public string LRPublishAction
        {
            get { return this.txtDoingLRPublish.Text; }
            set { this.txtDoingLRPublish.Text = value; }
        }//
        public string RequiresApproval
        {
            get { return this.txtRequiresApproval.Text; }
            set { this.txtRequiresApproval.Text = value; }
        }//txtRequiresApproval
        protected Resource resource;
        public string justPublished;
        public string returnMessage;
        public string additionalMessage = "";
        public string returnError;
        public string publishedMessage = "Successful Publish!";
        public string postbackMode;
        public int resourceVersionID = 0;
        public string sandboxText;

        #endregion

        #region Core Methods

        protected void Page_Load( object sender, EventArgs e )
        {
          if ( !IsUserAuthenticated() )
          {
                SetConsoleErrorMessage( "You must be logged in and authorized in order tag online resources." );
                Response.Redirect( "/Account/Login.aspx?nextUrl=/Publish.aspx" );
          }
            if ( Page.IsPostBack )
            {
                //Check for publish button click
                if ( Request.Form[ "__EVENTTARGET" ] == "btnPublish" )
                {
                    btnPublish_Click( this, new EventArgs() );
                }
                postbackMode = "true";

            }
            else
            {
                this.InitializeForm();
                postbackMode = "false";
            }
            //Regardless
            if ( new IOER.Services.WebDALService().IsSandbox() )
            {
                sandboxText = "true";
            }
            else
            {
                sandboxText = "false";
            }
        }

        private void InitializeForm()
        {
            if ( IsUserAuthenticated() )
            {
                loginContainer.Visible = false;

                //Get Form Privileges
                this.FormPrivileges = SecurityManager.GetGroupObjectPrivileges( this.WebUser, FormSecurityName );

                if ( FormPrivileges.CanCreate() )
                {
                    publishButton.Visible = true;
                }
                else
                {
                    publishButton.Visible = false;
                }
            }
            else
            {
                loginContainer.Visible = true;
                publishButton.Visible = false;
            }

            this.PopulateControls();

            //check if Authored Resource
            int authoredResourceID = SessionManager.Get( Session, "authoredResourceID", 0 );
            string authoredResourceURL = SessionManager.Get( Session, "authoredResourceURL", "" );

            if ( authoredResourceID > 0 && authoredResourceURL.Length > 10 )
            {
                LoadAuthoredResource( authoredResourceID, authoredResourceURL );
            }

        }

        private void PopulateControls()
        {
            //Load DDLs
            LoadDDLs();
        }
        protected void LoadAuthoredResource( int authoredResourceID, string authoredResourceURL )
        {

            //load stuff from authored resource
            AuthoredResourceID = authoredResourceID;
            ContentServices contentManager = new ContentServices();
            ContentItem contentItem = contentManager.Get( AuthoredResourceID );
            //ContentReference references = contentManager.ContentReferenceGet( authoredResourceID );

            string defaultAuthor = "";
            string defaultPublisher = "";
            if ( IsUserAuthenticated() )
            {
                if ( contentItem.HasOrg() )
                {
                    defaultAuthor = contentItem.ContentOrg.Name;
                    defaultPublisher = UtilityManager.GetAppKeyValue( "defaultPublisher", "Illinois Shared Learning Environment" );
                }
                else
                {
                    defaultAuthor = WebUser.FullName();
                    defaultPublisher = UtilityManager.GetAppKeyValue( "defaultPublisher", "Illinois Shared Learning Environment" );
                }
            }

            if ( contentItem.IsValid )
            {
                txtResourceURL.Value = authoredResourceURL;
                txtTitle.Value = contentItem.Title;
                txtDescription.Value = contentItem.Summary;
                txtCreator.Value = defaultAuthor;
                txtPublisher.Value = defaultPublisher;
                conditionsSelector.selectedValue = contentItem.ConditionsOfUseId.ToString();
                conditionsSelector.conditionsURL = contentItem.ConditionsOfUseUrl;

                if ( contentItem.PrivilegeTypeId != ContentItem.PUBLIC_PRIVILEGE )
                    LRPublishAction = "no";

                if ( contentItem.IsOrgContent() == true )
                {
                    RequiresApproval = "yes";
                    //if requires approval, can't publish yet
                    if ( contentItem.PrivilegeTypeId == ContentItem.PUBLIC_PRIVILEGE )
                        LRPublishAction = "save";
                }
            }

            //Empty the items from the session
            Session.Remove( "authoredResourceID" );
            Session.Remove( "authoredResourceURL" );
        }


        #endregion

        #region Retrieval

        protected void LoadDDLs()
        {
            //Time Taken: Hours
            for ( int i = 0; i < 24; i++ )
            {
                ddlTimeTakenHours.Items.Add( new ListItem( i.ToString(), i.ToString() ) );
            }

            //Time Taken: Minutes
            string[] minuteSets = new string[] { "0", "5", "10", "15", "20", "25", "30", "45" };
            for ( int i = 0; i < minuteSets.Length; i++ )
            {
                ddlTimeTakenMinutes.Items.Add( new ListItem( minuteSets[ i ], minuteSets[ i ] ) );
            }
        }

        protected void LoadStandardDDL( DataSet ds, DropDownList ddl )
        {
            //DataSet ds = DatabaseManager.DoQuery( query );
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    ddl.Items.Add( new ListItem( DatabaseManager.GetRowColumn( dr, "Title" ), DatabaseManager.GetRowColumn( dr, "Id" ) ) );
                }
            }
        }

        #endregion

        #region Events

        public void btnPublish_Click( object sender, EventArgs e )
        {
            if ( !IsUserAuthenticated() | !FormPrivileges.CanCreate() )
            {
                return;
            }

            if ( !ValidateForm() )
            {
                return;
            }
            //Initialize the Resource
            resource = new Resource();

            //Get data from hidden fields
            resource.Version.Submitter = WebUser.FullName();
            resource.CreatedById = WebUser.Id;
            resource.Keyword = FillListResourceChildItem( hdnKeywords.Value.Split( ',' ) );
            resource.SubjectMap = FillListResourceChildItem( hdnSubjects.Value.Split( ',' ) );
            resource.Standard = GetStandardsFromHdnJSON( hdnStandards.Value );
            resource.ResourceUrl = txtResourceURL.Value;
            resource.Version.ResourceUrl = txtResourceURL.Value;
            resource.Language = FillListResourceChildItem( lpLanguage.list );
            resource.Version.Creator = txtCreator.Value;
            resource.Version.Publisher = txtPublisher.Value;
            resource.Version.Title = txtTitle.Value;
            resource.Version.Description = txtDescription.Value;
            resource.Version.Rights = conditionsSelector.conditionsURL;
            resource.Version.AccessRightsId = int.Parse( lpAccessRights.list.SelectedItem.Value );
            resource.Version.AccessRights = lpAccessRights.list.SelectedItem.Text;
            resource.Audience = FillListResourceChildItem( lpEndUser.list );
            resource.Gradelevel = FillListResourceChildItem( lpGradeLevels.list );
            resource.ClusterMap = FillListResourceChildItem( lpCareerClusters.list );
            resource.ResourceType = FillListResourceChildItem( lpResourceType.list );
            resource.ResourceFormat = FillListResourceChildItem( lpMediaType.list );
            resource.GroupType = FillListResourceChildItem( lpGroupType.list );
            resource.Version.Created = GetDateCreated( txtDateCreated.Value );
            resource.relatedURL = FillListResourceChildItem( txtRelatedURL.Value );
            resource.Version.Requirements = txtRequirements.Value;
            resource.Version.TypicalLearningTime = GetTimeRequired( txtTimeTakenDays.Value, ddlTimeTakenHours.SelectedItem.Text, ddlTimeTakenMinutes.SelectedItem.Text );
            resource.ItemType = FillListResourceChildItem( lpItemType.list );
            resource.EducationalUse = FillListResourceChildItem( lpEducationalUse.list );
            try
            {
                resource.AssessmentType = ( ResourceChildItem )FillListResourceChildItem( lpAssessmentType.list ).First<ResourceChildItem>();
            }
            catch
            {
                resource.AssessmentType = new ResourceChildItem();
            }

            string statusMessage = "";
            IOERUser user = GetAppUser();
            bool isSuccessful = true;
            string sortTitle = "";
            int intID = 0;
            //Publish!
            //ResourcePublishUpdateController pubController = new ResourcePublishUpdateController();
            //PublishController newPubController = new PublishController();


            //TODO - add option to publish to an org   **************************
            int publishForOrgId = 0;
            try
            {
                if ( LRPublishAction.Equals( "no" ) )
                {
                    PublishingServices.PublishToDatabase( resource, 
                        publishForOrgId,
                        ref isSuccessful,
                        ref statusMessage,
                        ref resourceVersionID, 
                        ref intID,
                        ref sortTitle );
                    justPublished = "true";
                    publishedMessage = "Successfully tagged your resource";
                }
                else if ( LRPublishAction.Equals( "save" ) )
                {
                    PublishingServices.PublishToDatabase(resource,
                        publishForOrgId,
                       ref isSuccessful,
                        ref statusMessage,
                        ref resourceVersionID,
                        ref intID,
                        ref sortTitle );

                    PublishingServices.BuildSaveLRDocument( resource, 
                        ref isSuccessful,
                        ref statusMessage );

                    justPublished = "true";
                    publishedMessage = "Successfully tagged your resource.";
                }
                else
                {
                    bool skipLRPublish = false;
                    bool updatingElasticSearch = true;

                    //pubController.PublishToAll( resource, ref resourceVersionID, ref statusMessage );
                    PublishingServices.PublishToAll( resource, 
                                ref isSuccessful, 
                                ref statusMessage, 
                                ref resourceVersionID, 
                                ref intID, 
                                ref sortTitle,
                                updatingElasticSearch,
                                skipLRPublish,
                                (Patron) WebUser,
                                publishForOrgId );

                    justPublished = "true";
                    publishedMessage = "Successful Publish.";
                    PublishingServices.SendPublishNotification( user, resource );
                }
                if ( AuthoredResourceID > 0 )
                {

                    bool hasApproval = false;
                    string statusMessage2 = "";
                    bool isValid = new ContentServices().HandleContentApproval( resource, AuthoredResourceID, user, ref hasApproval, ref statusMessage2 );
                    if ( hasApproval )
                    {
                        publishedMessage = "Successfully tagged your resource.";
                        additionalMessage = " <p>NOTE: Approval is required, an email has been sent requesting a review of this resource.</p>";
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( "Error in PublishResource_Narrow PublishToAll: " + ex.ToString() );
            }

            //Return message ==> warning, actually expects the resource version id!!!
            if ( resourceVersionID > 0 )
            {
                returnMessage = resourceVersionID.ToString();
            }
            else
            {
                returnMessage = "";
            }

        }
       
        #endregion


        #region Helper Methods

        protected bool ValidateForm()
        {
            //Sanitize Form
            foreach ( TextBox box in this.Controls.OfType<TextBox>() )
            {
                box.Text = FormHelper.CleanText( box.Text );
            }
            foreach ( HiddenField hdn in this.Controls.OfType<HiddenField>() )
            {
                hdn.Value = FormHelper.CleanText( hdn.Value );
            }

            txtResourceURL.Value = txtResourceURL.Value.Trim();
            if ( txtResourceURL.Value.IndexOf( "http://" ) != 0 & txtResourceURL.Value.IndexOf( "https://" ) != 0 )
            {
                txtResourceURL.Value = "http://" + txtResourceURL.Value;
            }
            txtRelatedURL.Value = txtRelatedURL.Value.Trim();
            if ( txtRelatedURL.Value.IndexOf( "http://" ) != 0 & txtRelatedURL.Value.IndexOf( "https://" ) != 0 )
            {
                txtRelatedURL.Value = "http://" + txtRelatedURL.Value;
            }

            //Validate Form
            bool isValid = true;
            returnError = "";

            //check existing URL
            DataSet ds = DatabaseManager.DoQuery( "SELECT TOP 1 [ResourceUrl] FROM [Resource] WHERE [ResourceUrl] = '" + txtResourceURL.Value + "'" );
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                isValid = false;
                returnError = returnError + string.Format( ltl_errorTemplate.Text, "That URL already exists in the database." );
            }

            //check for minimum lengths

            string[] minimumLengths = ltl_minimumLengths.Text.Split( ',' ); //Title, Description, URL

            if ( txtTitle.Value.Length < int.Parse( minimumLengths[ 0 ] ) )
            {
                isValid = false;
                returnError = returnError + string.Format( ltl_errorTemplate.Text, "Title must be at least " + minimumLengths[ 0 ] + " characters." );
            }
            if ( txtDescription.Value.Length < int.Parse( minimumLengths[ 1 ] ) )
            {
                isValid = false;
                returnError = returnError + string.Format( ltl_errorTemplate.Text, "Description must be at least " + minimumLengths[ 1 ] + " characters." );
            }
            if ( txtResourceURL.Value.Length < int.Parse( minimumLengths[ 2 ] ) )
            {
                isValid = false;
                returnError = returnError + string.Format( ltl_errorTemplate.Text, "URL must be at least " + minimumLengths[ 2 ] + " characters." );
            }

            //Ensure the time taken days field is numeric
            try
            {
                int.Parse( txtTimeTakenDays.Value );
            }
            catch ( Exception ex )
            {
                isValid = false;
                returnError = returnError + string.Format( ltl_errorTemplate.Text, "Days field must be numeric." );
            }


            //Ensure there is at least one Resource Type
            if ( lpResourceType.list.SelectedIndex == -1 )
            {
                isValid = false;
                returnError = returnError + string.Format( ltl_errorTemplate.Text, "You must select at least one <a href=\"javascript:showPage('resourceTypes')\">Resource Type</a>." );
            }

            //Ensure there is at least one Resource Format
            if ( lpMediaType.list.SelectedIndex == -1 )
            {
                isValid = false;
                returnError = returnError + string.Format( ltl_errorTemplate.Text, "You must select at least one <a href=\"javascript:showPage('mediaTypes')\">Media Type</a>." );
            }

            //Ensure there is an Item Type if Assessment Item has been selected
            if ( lpResourceType.list.Items.FindByText( "Assessment Item" ).Selected )
            {
                if ( lpItemType.list.SelectedIndex == -1 )
                {
                    isValid = false;
                    returnError = returnError + string.Format( ltl_errorTemplate.Text, "You must select an <a href=\"javascript:showPage('mediaTypes')\">Item Type</a>." );
                }
            }

            //Ensure that a Conditions of Use item has been selected
            if ( conditionsSelector.conditionsURL == "" )
            {
                isValid = false;
                returnError = returnError + string.Format( ltl_errorTemplate.Text, "You must select appropriate <a href=\"javascript:showPage('copyrightInfo')\">Usage Rights</a>." );
            }

            return isValid;
        }

        protected string GetTimeRequired( string days, string hours, string minutes )
        {
            if ( days == "0" & hours == "0" & minutes == "0" )
            {
                return "";
            }
            else
            {
                return "P" + days + "D" + hours + "H" + minutes + "M";
            }
        }

        protected DateTime GetDateCreated( string data )
        {
            try
            {
                DateTime dt = DateTime.Parse( data );
                return dt;
            }
            catch ( Exception ex )
            {
                return DateTime.Now;
            }
        }

        protected List<ResourceChildItem> FillListResourceChildItem( string data )
        {
            return FillListResourceChildItem( new string[] { data } );
        }

        protected List<ResourceChildItem> FillListResourceChildItem( ListControl cbxl )
        {
            List<ResourceChildItem> collection = new List<ResourceChildItem>();
            foreach ( ListItem item in cbxl.Items )
            {
                if ( item.Selected )
                {
                    collection.Add( FillNormalMap( item.Value, item.Text ) );
                }
            }
            return collection;
        }

        protected List<ResourceChildItem> FillListResourceChildItem( string[] tags )
        {
            List<ResourceChildItem> collection = new List<ResourceChildItem>();
            foreach ( string tag in tags )
            {
                if ( tag != "" & tag != "http://" & tag != "https://" & tag != "ftp://" )
                {
                    collection.Add( FillNormalMap( "", tag ) );
                }
            }
            return collection;
        }

        protected ResourceChildItem FillNormalMap( string codeID, string value )
        {
            ResourceChildItem map = new ResourceChildItem();
            map.OriginalValue = value;
            try
            {
                map.CodeId = int.Parse( codeID );
            }
            catch ( Exception ex ) { }
            return map;
        }

        public class ResourceStandardJSON
        {
            public string alignmentID;
            public string alignmentText;
            public string code;
            public string[] grades;
            public int id;
            public int parent;
            public string text;
            public string url;
        }

        protected ResourceStandardCollection GetStandardsFromHdnJSON( string data )
        {
            ResourceStandardJSON[] itemList = new JavaScriptSerializer().Deserialize<ResourceStandardJSON[]>( data );
            ResourceStandardCollection collection = new ResourceStandardCollection();
            foreach ( ResourceStandardJSON item in itemList )
            {
                ResourceStandard standard = new ResourceStandard();
                standard.StandardId = item.id;
                standard.Id = item.id;
                standard.StandardUrl = item.url;
                standard.AlignmentTypeCodeId = int.Parse( item.alignmentID );
                standard.AlignmentTypeValue = item.alignmentText;
                standard.StandardNotationCode = item.code;
                collection.Add( standard );
            }
            return collection;
        }

        protected ResourceStandardCollection GetStandardsFromHdn( string data )
        {
            string standardSeparator = ltlStandardSeparator.Text;
            string standardDataSeparator = ltlStandardDataSeparator.Text;
            string[] standards = data.Split( new string[] { standardSeparator }, StringSplitOptions.RemoveEmptyEntries );
            ResourceStandardCollection collection = new ResourceStandardCollection();
            for ( var i = 0; i < standards.Length; i++ )
            {
                string[] standardBits = standards[ i ].Split( new string[] { standardDataSeparator }, StringSplitOptions.RemoveEmptyEntries );
                ResourceStandard standard = new ResourceStandard();
                try
                {
                    string[] ids = standardBits[ 0 ].Split( '-' );
                    if ( ids[ 1 ] == "" ) //semi-kludge to fix things based on the way standard IDs and component IDs are handled
                    {
                        standard.StandardId = int.Parse( ids[ 0 ] );
                    }
                    else
                    {
                        standard.StandardId = int.Parse( ids[ 1 ] );
                    }
                    standard.Id = standard.StandardId;
                    standard.StandardUrl = standardBits[ 2 ];
                    standard.AlignmentTypeCodeId = int.Parse( standardBits[ 4 ] );
                    standard.AlignmentTypeValue = standardBits[ 5 ];
                    standard.StandardNotationCode = standardBits[ 1 ];
                    collection.Add( standard );
                }
                catch ( Exception ex )
                {
                    continue;
                }
            }
            return collection;
        }


        #endregion

    }
}
