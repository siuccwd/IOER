using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

//using MyManager = ILPathways.DAL.AppItemManager;
//using MyAppEmailManager = workNet.DAL.AnnouncementEmailManager;
using IOER.Controllers;
using IOER.Library;
using ILPathways.Utilities;
using ILPathways.Business;

namespace IOER.Controls.AppItems
{
    public partial class AnnouncementDisplay : IOER.Library.BaseUserControl
    {
        /// <summary>
        /// Display an announcement
        /// </summary>

        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
        const string thisClassName = "AnnouncementDisplay";

        private string usingAppItemWebService = "yes";

        /// <summary>
        /// Gets/Sets SubscribeUrl
        /// </summary>
        string SubscribeUrl
        {
            get
            {
                try
                {
                    return ViewState[ "SubscribeUrl" ].ToString();
                }
                catch ( NullReferenceException nex )
                {
                    ViewState[ "SubscribeUrl" ] = "";
                    return "";
                }
            }
            set { ViewState[ "SubscribeUrl" ] = value; }
        }
        string SearchUrl
        {
            get
            {
                try
                {
                    return ViewState[ "SearchUrl" ].ToString();
                }
                catch ( NullReferenceException nex )
                {
                    ViewState[ "SearchUrl" ] = "";
                    return "";
                }
            }
            set { ViewState[ "SearchUrl" ] = value; }
        }

        public string txtTitle;
        public string txtDescr1;
        public string published, updated;

        #region properties
        public string NewsItemTemplateCode
        {
            get
            {
                return txtNewsItemCode.Text;
            }
            set { txtNewsItemCode.Text = value; }
        }
        public string UsingWebService
        {
            get
            {
                return usingWebService.Text;
            }
            set
            {
                usingWebService.Text = value;
            }
        }

        /// <summary>
        /// CurrentNewsTemplate
        /// </summary>
        public NewsEmailTemplate CurrentNewsTemplate
        {
            get
            {
                try
                {
                    if ( ViewState[ "CurrentNewsTemplate" ] == null )
                        ViewState[ "CurrentNewsTemplate" ] = new NewsEmailTemplate();

                    return ( NewsEmailTemplate ) ViewState[ "CurrentNewsTemplate" ];
                }
                catch ( NullReferenceException nex )
                {
                    return null;
                }
            }
            set { ViewState[ "CurrentNewsTemplate" ] = value; }
        }

        string SearchTitle
        {
            get
            {
                try
                {
                    return ViewState[ "SearchTitle" ].ToString();
                }
                catch ( NullReferenceException nex )
                {
                    ViewState[ "SearchTitle" ] = "";
                    return "";
                }
            }
            set { ViewState[ "SearchTitle" ] = value; }
        }
        #endregion

        /// <summary>
        /// Handle page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void Page_Load( object sender, EventArgs ex )
        {
            //Initialize the form resource manager
            //usingAppItemWebService = UtilityManager.GetAppKeyValue( "appItemReadUseWebService", "yes" );
            if ( !IsPostBack )
            {
                InitializeForm();
            }

        }//	

        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        private void InitializeForm()
        {
            CurrentNewsTemplate = new NewsEmailTemplate();
            if ( NewsItemTemplateCode.Length == 0 )
            {
                //hide, no code found
                detailsPanel.Visible = false;
                return;
            }

            //NewsEmailTemplate CurrentNewsTemplate = myEmailTemplateMgr.Get( NewsItemTemplateCode );
            CurrentNewsTemplate = IOER.Controllers.NewsController.NewsTemlateGet( NewsItemTemplateCode );
            if ( CurrentNewsTemplate == null || CurrentNewsTemplate.Id == 0 )
            {
                //hide, no code found
                detailsPanel.Visible = false;
                return;
            }

            if ( SearchTitle == "" )
                SearchTitle = "Read another news item";
            SearchUrl = CurrentNewsTemplate.SearchUrl;

            SubscribeUrl = CurrentNewsTemplate.ConfirmUrl;

            if ( SubscribeUrl != "" )
            {
                lnkSubscribe.NavigateUrl = SubscribeUrl;
                lnkSubscribe.Visible = true;
            }


            //future: may add some config properties. For now, just look for a rowId and display if found
            string id = FormHelper.GetRequestKeyValue( "id", "" ); ;

            if ( id.Length > 0 )
            {
                this.Get( id );
            }
            else
            {
                //hide and show message?
                detailsPanel.Visible = false;
                return;
            }
        }	// End 	

        /// <summary>
        /// Get - retrieve form data
        /// </summary>
        /// <param name="recId"></param>
        private void Get( string recId )
        {
            try
            {
                //get record
                AppItem entity = new AppItem();
                if ( usingAppItemWebService == "no" )
                {
//                    entity = new AppItemManager().Get( recId );
                    SetConsoleErrorMessage( "Error - direct database retrieve is not supported!" );
                }
                else
                {
                    entity = IOER.Controllers.AppItemController.AppItemGet( recId );
                }

                if ( entity == null || entity.HasValidRowId() == false )
                {
                    this.SetConsoleErrorMessage( "Sorry the requested item does not exist" );
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
                detailsPanel.Visible = false;
            }

        }	// End method


        /// <summary>
        /// Populate the form 
        ///</summary>
        private void PopulateForm( AppItem entity )
        {

            detailsPanel.Visible = true;
            lblDocumentLink.Text = "";
            lblDocumentLink.Visible = false;

            //txtTitle = entity.Title;
            //assign description after handling custom rendering for items like glossary items, videos, links opening in a new window
            //OR should this be really done when item is saved????
            //txtDescr1 = UtilityManager.HandleCustomTextRendering( formRM, entity.Description );
            txtDescr1 = entity.Description;


            if ( SearchUrl != null && SearchUrl.Length > 0 )
            {
                ReadAnotherItem1.Text = ReadAnotherItem2.Text = string.Format( "<a href='{0}'>{1}</a>", SearchUrl, SearchTitle );
            }


            //check for doc info and display
            //not sure of location - may require author to provide a name placeholder in the html. If not found, then display at the bottom of the text
            if ( entity.DocumentRowId != null && !entity.IsInitialGuid( entity.DocumentRowId ) )
            {
                //===> NOT HANDLING docs
                //HandleDocumentPopulate( entity.DocumentRowId );
            }

            //check for an image ==========================================
            //===> NOT HANDLING IMAGES
            if ( entity.ImageId > 0 )
            {
                //image details should be in the entity, but check just in case
                if ( entity.AppItemImage == null || entity.AppItemImage.Id == 0 )
                {
                    //entity.AppItemImage = ImageStoreManager.Get( entity.ImageId );
                    //TODO - format
                }

            }
            else
            {
                //imgCurrent.Visible = false;
            }

            try
            {
                published = entity.Approved.ToString( "MM-dd-yyyy" );
                updated = entity.LastUpdated.ToString( "MM-dd-yyyy" );
                if ( published != updated )
                {
                    History.Visible = true;
                }
                else
                {
                    History.Visible = false;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( thisClassName + ".PopulateForm(): " + ex.ToString() );
            }
        }//

    }
}