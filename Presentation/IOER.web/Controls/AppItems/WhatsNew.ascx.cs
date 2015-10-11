using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

//using workNet.BusObj.Entity;
//using MyManager = workNet.DAL.AppItemManager;
//using MyAppEmailManager = workNet.DAL.AnnouncementEmailManager;
using ILPathways.Business;
using IOER.Controllers;
using IOER.Library;
using ILPathways.Utilities;
using BDM = ILPathways.DAL.DatabaseManager;

namespace IOER.Controls.AppItems
{
    public partial class WhatsNew : BaseUserControl
    {
        /// <summary>
        /// Set this to "true" to get from the local database.  "False" means use the service on production.
        /// </summary>
        private bool useLocalItemService = false;
        //private MyAppEmailManager myEmailTemplateMgr = new MyAppEmailManager();
        //MyManager myManager;

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


        private int _nbrItems = 7;
        public int NbrItems
        {
            get
            {
                return _nbrItems;
            }
            set { _nbrItems = value; }
        }

        string _headingStyle = "";
        /// <summary>
        /// Override default style of "What's New" heading
        /// </summary>
        public string HeadingStyle
        {
            get
            {
                return _headingStyle;
            }
            set { _headingStyle = value; }
        }

        /// <summary>
        /// Override default text of "What's New" heading
        /// </summary>
        public string HeadingText
        {
            get
            {
                try
                {
                    //if ( wnHeadingText.Text == null || wnHeadingText.Text.Trim().Length == 0)
                    //    wnHeadingText.Text = "What's New";

                    return wnHeadingText.Text;
                }
                catch ( NullReferenceException nex )
                {
                    return "";
                }
            }
            set { wnHeadingText.Text = value; }
        }

        /// <summary>
        /// Override visibility of "Stay Connected" footer
        /// </summary>
        public bool DisplayFooter
        {
            get
            {
                try
                {
                    if ( ViewState[ "wnDisplayFooter" ] == null )
                        ViewState[ "wnDisplayFooter" ] = false;

                    return bool.Parse( ViewState[ "wnDisplayFooter" ].ToString() );

                }
                catch
                {
                    ViewState[ "wnDisplayFooter" ] = false;
                    return false;
                }
            }
            set
            {
                try
                {
                    ViewState[ "wnDisplayFooter" ] = value;
                }
                catch
                {
                    ViewState[ "wnDisplayFooter" ] = false;
                }
            }
        }

        /// <summary>
        /// Set to true to show link to all news items 
        /// NOTE: CurrentNewsTemplate.SearchUrl must contain the url
        /// </summary>
        public bool DisplaySeeAllUpdates
        {
            get
            {
                try
                {
                    if ( ViewState[ "wnSeeAllUpdates" ] == null )
                        ViewState[ "wnSeeAllUpdates" ] = false;

                    return bool.Parse( ViewState[ "wnSeeAllUpdates" ].ToString() );
                }
                catch
                {
                    ViewState[ "wnSeeAllUpdates" ] = false;
                    return false;
                }
            }
            set
            {
                //Make sure we're receiving a valid bool
                try
                {
                    ViewState[ "wnSeeAllUpdates" ] = value;
                }
                catch
                {
                    ViewState[ "wnSeeAllUpdates" ] = false;
                }
            }
        }


        #endregion
        protected void Page_Load( object sender, EventArgs e )
        {
            //myManager = new MyManager();
            if ( !Page.IsPostBack )
            {
                InitializeForm();
            }
        }

        protected void InitializeForm()
        {

            CurrentNewsTemplate = new NewsEmailTemplate();

            // Set properties if they are not already set
           // string pageControl = "";    //McmsHelper.GetPageControl();
            //NewsItemTemplateCode = "";  // DetermineProperty( pageControl, "NewsItemTemplateCode", NewsItemTemplateCode );
            if ( NewsItemTemplateCode.Length == 0 )
            {
                //hide, no code found
                detailPanel.Visible = false;
                return;
            }

            //NewsEmailTemplate CurrentNewsTemplate = myEmailTemplateMgr.Get( NewsItemTemplateCode );
            CurrentNewsTemplate = IOER.Controllers.NewsController.NewsTemlateGet( NewsItemTemplateCode );
            if ( CurrentNewsTemplate == null || CurrentNewsTemplate.Id == 0 )
            {
                //hide, no code found
                detailPanel.Visible = false;
                return;
            }


            //HeadingStyle = DetermineProperty( pageControl, "HeadingStyle", HeadingStyle );
            if ( HeadingStyle != null && HeadingStyle.Length > 0 )
            {
                headSpan.Attributes[ "style" ] = HeadingStyle;
            }
            //check overrides
            //HeadingText = DetermineProperty( pageControl, "HeadingText", HeadingText );
            if ( HeadingText != null && HeadingText.Length > 0 )
            {
                Archive.Text = HeadingText;
            }

            //DisplayFooter = DetermineProperty( pageControl, "DisplayFooter", DisplayFooter );
            Footer.Visible = DisplayFooter;

            //DisplaySeeAllUpdates = DetermineProperty( pageControl, "DisplaySeeAllUpdates", DisplaySeeAllUpdates );
            if ( CurrentNewsTemplate.SearchUrl.Length > 0 && DisplaySeeAllUpdates )
            {
                Archive.NavigateUrl = CurrentNewsTemplate.SearchUrl;
                Archive1.NavigateUrl = CurrentNewsTemplate.SearchUrl;
                SeeAllUpdates.Visible = true;
            }


            DoSearch();
        }

        void DoSearch()
        {
            DataSet ds = new DataSet();
            // make the item type configurable
            int itemType = AppItem.NewsItemType;
            if ( useNewsItemType.Text.ToLower() != "yes" )
            {
                itemType = AppItem.AnnouncementItemType;
            }

            if ( UsingWebService == "yes" )
            {
                ds = IOER.Controllers.AppItemController.AppItemWebServiceNewsSearch( CurrentNewsTemplate.Category, "", NbrItems );
            }
            else
            {
                //ds = myManager.Select( itemType, CurrentNewsTemplate.Category, "", "published" );
            }

            if ( DoesDataSetHaveRows( ds ) && ds.Tables[ 0 ].TableName != "ErrorMessage" )
            {
                if ( ds.Tables[ 0 ].Rows.Count > 0 && BDM.GetRowColumn( ds.Tables[ 0 ].Rows[ 0 ], "LastUpdated", "" ) != "" )
                { // We have rows!
                    ds.Tables[ 0 ].DefaultView.Sort = "Approved DESC";
                    DataTable dt = ds.Tables[ 0 ].DefaultView.ToTable();
                    ArrayList items = new ArrayList();
                    for ( int i = 0 ; i < NbrItems && i < dt.Rows.Count ; i++ )
                    {
                        DataItem item = new DataItem();
                        item.param1 = BDM.GetRowColumn( dt.Rows[ i ], "Approved", DateTime.Now ).ToString( dateFormat.Text );
                        item.Title = BDM.GetRowColumn( dt.Rows[ i ], "Title", "" );
                        string id = BDM.GetRowColumn( dt.Rows[ i ], "RowId", "" );
                        if ( CurrentNewsTemplate.DisplayUrl.IndexOf( "{0}" ) > -1 )
                            item.Url = string.Format( CurrentNewsTemplate.DisplayUrl, id );
                        else
                            item.Url = CurrentNewsTemplate.DisplayUrl.Replace( "@RowId", id );
                        items.Add( item );
                    }
                    rptItems.DataSource = items;
                    rptItems.DataBind();
                }
            }
            else
            {
                // Hide me - no items to display!
                this.Visible = false;
            }
        }

    }

}