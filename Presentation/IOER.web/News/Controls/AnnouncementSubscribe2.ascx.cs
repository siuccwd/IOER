using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

//using utilityClasses;
//using vos_portal.classes;
//using vos_portal.Library;
//using workNet.BusObj.Entity;

using MyManager = ILPathways.Controllers.NewsController;
//using MyAppEmailManager = workNet.DAL.AnnouncementEmailManager;

using ILPathways.Controllers;
using ILPathways.Library;
using ILPathways.Utilities;
using ILPathways.Business;

namespace ILPathways.Controls.AppItems
{
    public partial class AnnouncementSubscribe2 : ILPathways.Library.BaseUserControl
    {
        private MyManager myManager = new MyManager();
        //private MyAppEmailManager myEmailTemplateMgr = new MyAppEmailManager();

        public string catText = "", catText2 = "";

        // public int sid;
        public int loopcategory;
        public int count_selected = 0;
        private const string lblSuccessfulSubscription = "NewsletterSubscriptionSuccessfulMessage";

        #region Properties
        public string NewsItemTemplateCode
        {
            get
            {
                return txtNewsItemCode.Text;
            }
            set { txtNewsItemCode.Text = value; }
        }
        private AppItemAnnouncementSubscription CurrentRecord
        {
            get
            {
                try
                {
                    return ( AppItemAnnouncementSubscription ) ViewState[ "CurrentRecord" ];
                }
                catch ( NullReferenceException nex )
                {
                    return null;
                }
            }
            set { ViewState[ "CurrentRecord" ] = value; }
        }

        /// <summary>
        /// CurrentNewsTemplate - where multiple categories are supplied, the first will be the default, for urls, etc
        /// </summary>
        private NewsEmailTemplate CurrentNewsTemplate
        {
            get
            {
                try
                {
                    return ( NewsEmailTemplate ) ViewState[ "CurrentNewsTemplate" ];
                }
                catch ( NullReferenceException nex )
                {
                    return null;
                }
            }
            set { ViewState[ "CurrentNewsTemplate" ] = value; }
        }
        private string Action
        {
            get
            {
                try
                {
                    return ViewState[ "Action" ].ToString();
                }
                catch ( NullReferenceException nex )
                {
                    return null;
                }
            }
            set { ViewState[ "Action" ] = value; }
        }
        public string AnnouncementCategory
        {
            get { return ( string ) ViewState[ "AnnouncementCategory" ]; }
            set { ViewState[ "AnnouncementCategory" ] = value; }
        }

        public string AnnouncementTitle
        {
            get { return ( string ) ViewState[ "AnnouncementTitle" ]; }
            set { ViewState[ "AnnouncementTitle" ] = value; }
        }

        public string SubscribeUrl
        {
            get { return ( string ) ViewState[ "SubscribeUrl" ]; }
            set { ViewState[ "SubscribeUrl" ] = value; }
        }


        #endregion


        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                InitializeForm();
            }
        }

        protected void InitializeForm()
        {
            CurrentRecord = new AppItemAnnouncementSubscription();
            CurrentNewsTemplate = new NewsEmailTemplate();
            if ( NewsItemTemplateCode.Length == 0 )
            {
                //hide, no code found
                pnlDetail.Visible = false;
                return;
            }
            CurrentNewsTemplate = ILPathways.Controllers.NewsController.NewsTemplateGet( NewsItemTemplateCode );
            if ( CurrentNewsTemplate == null || CurrentNewsTemplate.Id == 0 )
            {
                //hide, no code found
                pnlDetail.Visible = false;
                return;
            }
            AnnouncementCategory = CurrentNewsTemplate.Category;
            SubscribeUrl = CurrentNewsTemplate.ConfirmUrl;

            HandleTextDescriptions();

            if ( AnnouncementCategory == null || AnnouncementCategory.Trim() == "" )
            {
                //hide, maybe notify
                pnlDetail.Visible = false;
                return;
            }
            else
            {
                //handle multiple
                FormatCategory();
            }

            HandleActionType();

            if ( txtEmail.Text.Length == 0
            && IsUserAuthenticated() == true
            && this.WebUser.Email != null )
                txtEmail.Text = this.WebUser.Email;
        }// 

        protected void HandleActionType()
        {

            string guid = GetRequestKeyValue( "RowId", CurrentRecord.DEFAULT_GUID);
            Action = GetRequestKeyValue( "action", "" );
            if ( Action.Trim() != "" )
            {
                optionsPanel2.Visible = false;

                if ( Action.ToLower().IndexOf( "step" ) > -1 )
                {
                    guid = CurrentRecord.DEFAULT_GUID;
                }

                if ( guid != CurrentRecord.DEFAULT_GUID )
                {
                    txtDesc2.Text = txtDesc2.Text.Replace( "@RowId", guid );
                    CBLCategoryNewsletter.Visible = false;
                    //
                    CurrentRecord = NewsController.SubscriptionGet( guid );
                    ProcessActionFields( guid );
                }
                else
                {
                    ProcessActionFields( CurrentRecord.DEFAULT_GUID );
                }
            }
            else
            {
                if ( guid != CurrentRecord.DEFAULT_GUID )
                {
                    //if have id and no action, assume manage preferences??
                    pnlSubscribe.Visible = true;
                    pnlDetail.Visible = true;
                    btnUpdate.Visible = true;
                    btnUnSubscribe.Visible = true;

                    CurrentRecord = NewsController.SubscriptionGet( guid );
                    txtEmail.Text = CurrentRecord.Email;
                    rbNotificationFrequency.SelectedValue = CurrentRecord.Frequency.ToString();
                }
                else
                {
                    btnIdentify.Visible = true;
                    txtEmail.Text = GetRequestKeyValue( "email", "" );
                    txtEmail.ReadOnly = false;
                    pnlSubscribe.Visible = false;
                    SetButtonVisibility();
                    optionsPanel2.Visible = false;
                    //CBLCategoryNewsletter.Visible = true; 

                }
            }
        }// 

        protected void HandleTextDescriptions()
        {

            string parentUrl = Request.UrlReferrer == null ? Request.Url.ToString() : Request.UrlReferrer.AbsoluteUri.ToString();  // CmsHttpContext.Current.Posting.Parent.Url;

            string referrer = parentUrl;
            //get referrer for returning after subscribe
            try
            {
                referrer = Request.UrlReferrer.AbsoluteUri.ToString();
                if ( referrer == null || referrer.ToLower().IndexOf( "?vos_portal/" ) == -1 )
                {
                    referrer = Request.UrlReferrer.AbsoluteUri.ToString();
                    if ( referrer == null || referrer.ToLower().IndexOf( "?/lrw/" ) == -1 )
                    {
                        referrer = parentUrl;
                    }
                }
            }
            catch
            {
                referrer = parentUrl;
            }
            txtDesc1.Text = string.Format( this.defaultDesc1.Text, referrer );
            txtDesc2.Text = string.Format( this.defaultDesc2.Text, CurrentNewsTemplate.Title, referrer, CurrentNewsTemplate.ConfirmUrl );
            txtDesc3.Text = string.Format( this.defaultDesc3.Text, CurrentNewsTemplate.Title, referrer );
            //}
        }// 

        protected void FormatCategory()
        {
            int cntr = 1;

            ListItem item = new ListItem();
            item.Value = CurrentNewsTemplate.NewsItemCode;	// tmp.Id;	// ;
            item.Text = CurrentNewsTemplate.Category;

            CBLCategoryNewsletter.Items.Add( item );
            if ( cntr == 1 )
            {
                //CurrentNewsTemplate = net;
                //set defaults for now
                AnnouncementTitle = CurrentNewsTemplate.Title;
                SubscribeUrl = CurrentNewsTemplate.ConfirmUrl;
            }

            //string[] categories = AnnouncementCategory.Split( ',' );
            //foreach ( string cat in categories )
            //{
            //    cntr++;
            //    NewsEmailTemplate tmp = myEmailTemplateMgr.Get( cat.Trim() );

            //    ListItem item = new ListItem();
            //    item.Value = tmp.NewsItemCode;	// tmp.Id;	// ;
            //    item.Text = tmp.Category;

            //    CBLCategoryNewsletter.Items.Add( item );
            //    if ( cntr == 1 )
            //    {
            //        CurrentNewsTemplate = tmp;
            //        //set defaults for now
            //        AnnouncementTitle = tmp.Title;
            //        SubscribeUrl = tmp.ConfirmUrl;
            //    }
            //}

            CBLCategoryNewsletter.Items[ 0 ].Selected = true;

        }// 
        protected void SetButtonVisibility()
        {
            if ( CurrentRecord != null && CurrentRecord.IsValid
                && ( CurrentRecord.Email != null && CurrentRecord.Email.Length > 0 ) )
            {
                if ( CurrentRecord.IsValidated == false )
                {
                    btnValidate.Visible = true;
                    btnSubscribe.Visible = false;
                    btnUpdate.Visible = true;
                    btnUnSubscribe.Visible = true;
                    CBLCategoryNewsletter.Visible = false;
                }
                else
                {
                    btnValidate.Visible = false;
                    btnSubscribe.Visible = false;
                    btnUpdate.Visible = true;
                    btnUnSubscribe.Visible = true;
                    CBLCategoryNewsletter.Visible = false;
                }
            }
            else
            {
                btnValidate.Visible = false;
                btnSubscribe.Visible = true;
                btnUpdate.Visible = false;
                btnUnSubscribe.Visible = false;
            }
        }

        protected void PopulateForm()
        {
            rbNotificationFrequency.SelectedValue = CurrentRecord.Frequency.ToString();
        }


        protected void FormButton_Click( object sender, CommandEventArgs e )
        {
            switch ( e.CommandName )
            {
                case "Identify":
                    HandleIdentify();
                    break;
                case "Subscribe":
                    HandleSubscribe();
                    break;
                case "Update":
                    HandleUpdate();
                    break;
                case "UnSubscribe":
                    HandleUnsubscribe();
                    break;
                case "Validate":
                    HandleValidate();
                    break;
                default:
                    break;
            }
        }
        protected bool IsEmailValid()
        {

            txtEmail.Text = FormHelper.CleanText( txtEmail.Text );

            if ( this.txtEmail.Text.Trim().Length == 0 )
            {
                SetConsoleErrorMessage( "Error: You must enter a valid email address." );
                return false;
            }
            revEmail.Validate();
            if ( revEmail.IsValid == false )
            {
                SetConsoleErrorMessage( revEmail.ErrorMessage );
                return false;
            }

            return true;
        }
        protected void HandleIdentify()
        {
            if ( IsEmailValid() == false )
                return;

            int cntr = 1;
            //HandleCategory( this.CurrentNewsTemplate.Category, cntr );
            HandleCategory( this.CurrentNewsTemplate, cntr );
            //foreach ( ListItem li in this.CBLCategoryNewsletter.Items )
            //{
            //    if ( li.Selected == true )
            //    {
            //        cntr++;

            //        HandleCategory( li.Value, cntr );
            //    }
            //}

            if ( cntr == 0 )
            {
                SetConsoleErrorMessage( "Error: You must select at least one of the available news options." );
            }
            else
            {
                btnIdentify.Visible = false;
                SetButtonVisibility();
                pnlSubscribe.Visible = true;
                btnUpdate.Visible = true;
            }
        }
        /// <summary>
        /// Handle subscribing to a category
        /// Currently not set up to handle multiple subscriptions completely:
        /// - for subscribes allow
        /// , so ignore other than first
        /// </summary>
        /// <param name="newsItemCode"></param>
        /// <param name="cntr"></param>
        protected void HandleCategory( string newsItemCode, int cntr )
        {
            //NewsEmailTemplate net = myEmailTemplateMgr.Get( CurrentNewsTemplate.NewsItemCode );
            HandleCategory( CurrentNewsTemplate, cntr );
        }
        /// <summary>
        /// Handle subscribing to a category
        /// Currently not set up to handle multiple subscriptions completely:
        /// - for subscribes allow
        /// , so ignore other than first
        /// </summary>
        /// <param name="newsItemCode"></param>
        /// <param name="cntr"></param>
        protected void HandleCategory( NewsEmailTemplate net, int cntr )
        {
            //

            CurrentRecord = NewsController.SubscriptionGet( net.Category, txtEmail.Text );

            if ( CurrentRecord == null || !CurrentRecord.IsValid )
            {
                //Subscribe them, they're not subscribed!
                //!!! WARNING HandleSubscribe also loops thru the categories !!!!
                //only do one for now
                if ( cntr == 1 )
                    HandleSubscribe( net.Category, net.ConfirmUrl, net.Title );

            }
            else if ( CurrentRecord.IsValid && !CurrentRecord.IsValidated )
            {
                //subscribed but not validated, send another email
                SendConfirmationEmail( CurrentRecord.RowId.ToString(), CurrentRecord.Email, net.ConfirmUrl, net.Title );
                try
                {
                    //?????? we are already on the subscribe control, why the redirect?????????????/
                    //PLUS doing a redirect means additional categories will not be processed
                    Response.Redirect( SubscribeUrl + "?action=step1" );
                }
                catch ( ThreadAbortException taex )
                {
                    //Do nothing
                }
            }
            else if ( Request.RawUrl != SubscribeUrl )
            {
                //current record is valid and subscription has been validated 
                //but current url is different than the subscribe url
                //-not sure of purpose. We want to be able to subscribe from different locations
                try
                {
                    if ( Action.ToLower() == "identify" )
                        PopulateForm();
                    else
                        Response.Redirect( SubscribeUrl + "?action=identify&email=" + txtEmail.Text );
                }
                catch ( ThreadAbortException taex )
                {
                    //Do nothing
                }
            }
            else
            {
                //current record is valid and subscription has been validated 
                //and current url is same as the subscribe url
                PopulateForm();
            }
        }


        protected void HandleSubscribe()
        {
            //need to verify the following parms were set for the default path
            HandleSubscribe( AnnouncementCategory, SubscribeUrl, AnnouncementTitle );
        }

        /// <summary>
        /// HandleSubscribe
        /// Note 
        /// - category is not used
        /// - why do loop here if called from a loop??
        /// - this is a problem as there is no checking for existance, etc after the first category!!
        /// </summary>
        /// <param name="category"></param>
        /// <param name="subscribeUrl"></param>
        /// <param name="title"></param>
        protected void HandleSubscribe( string category, string subscribeUrl, string title )
        {
            string status = "successful";
            if ( CurrentRecord != null && CurrentRecord.IsValid )
            {
                SetConsoleErrorMessage( "You are already subscribed!" );
            }
            else
            {
                if ( Request.RawUrl != SubscribeUrl )
                {
                    // No record is found - so user is not yet subscribed.  Subscribe them.
                    CurrentRecord = new AppItemAnnouncementSubscription();
                    CurrentRecord.Category = CurrentNewsTemplate.Category;
                    CurrentRecord.Frequency = 0; // Default to immediately receive news
                    CurrentRecord.Created = DateTime.Now;
                    CurrentRecord.Email = txtEmail.Text;
                    CurrentRecord.IsValidated = false;

                    status = NewsController.Subscribe( CurrentRecord );
                    //myManager.Insert( CurrentRecord, ref status );

                    if ( status == "successful" )
                    {
                        SendConfirmationEmail( CurrentRecord.RowId.ToString(), txtEmail.Text, CurrentNewsTemplate.ConfirmUrl, CurrentNewsTemplate.Title );

                        try
                        {
                            //redirect to default url
                            Response.Redirect( subscribeUrl + "?action=step1", false );
                            Context.ApplicationInstance.CompleteRequest();
                        }
                        catch ( ThreadAbortException taex )
                        {
                            //Do nothing
                        }
                    }
                    else
                    {
                        SetConsoleErrorMessage( "An error has occurred: " + status );
                    }
                }
            }
        }// HandleSubscribe

        protected void HandleUpdate()
        {
            string status = "successful";
            if ( Action.ToLower() == "update" )
            {
                //TODO - handle RowId processing==> 
                string guid = GetRequestKeyValue( "RowId", "" );
                string email = GetRequestKeyValue( "email", "" );
                if ( guid == "" )
                {
                    SetConsoleErrorMessage( "Invalid subscription parameter!" );
                }
                else
                {
                    CurrentRecord = NewsController.SubscriptionGet( guid );
                    if ( CurrentRecord == null || !CurrentRecord.IsValid )
                    {
                        SetConsoleErrorMessage( "Subscription record not found!" );
                        btnIdentify.Visible = true;
                        txtEmail.Text = email;
                        pnlSubscribe.Visible = false;
                    }
                    else
                    {
                        CurrentRecord.Email = email;
                        status = NewsController.SubscribeUpdate( CurrentRecord );
                        if ( status == "successful" )
                        {
                            pnlSubscribeMsg.Visible = false;
                            pnlGuestMsg.Visible = false;
                            pnlUpdatedEmail.Visible = true;
                            pnlDetail.Visible = false;
                            CurrentRecord.IsValidated = true;
                            SetButtonVisibility();
                        }
                        else
                        {
                            SetConsoleErrorMessage( "An error has occurred: " + status );
                        }
                    }
                }
            }
            else
            {
                if ( CurrentRecord.IsValid )
                {
                    if ( IsEmailValid() == false )
                        return;

                    CurrentRecord.Frequency = int.Parse( rbNotificationFrequency.SelectedValue );
                    bool emailChg = false;
                    if ( CurrentRecord.Email.ToLower() != txtEmail.Text.ToLower() )
                    {
                        emailChg = true;
                        // if email address has changed, check if another subscription already exists
                        AppItemAnnouncementSubscription checkSub = NewsController.SubscriptionGet( CurrentRecord.Category, txtEmail.Text );
                        if ( checkSub != null && CurrentRecord.IsValid )
                        {
                            SetConsoleErrorMessage( "Error: a subscription record already exists for the new email address" );
                            return;
                        }
                    }
                    //an email chg is not done immediately. A confirmation email is sent requiring to user to confirm a request to chg the email address
                    //CurrentRecord.Email = txtEmail.Text;

                    status = NewsController.SubscribeUpdate( CurrentRecord );
                    if ( status == "successful" )
                    {
                        if ( emailChg == true )
                        {
                            //TODO - for updates, do we only have the context of a single subscription, ie, the current path?????
                            SendUpdateConfirmation( CurrentRecord.RowId.ToString(), txtEmail.Text );
                            SetConsoleSuccessMessage( "The frequency at which you have been e-mailed has been updated.  You should receive a confirmation request in an email shortly for the email address change.  Follow the instructions in that e-mail to confirm the email address change." );
                        }
                        else
                        {
                            SetConsoleSuccessMessage( "Your subscription has been updated." );
                        }
                    }
                    else
                    {
                        SetConsoleErrorMessage( "An error has occurred: " + status );
                    }
                    SetButtonVisibility();
                }
            }
        }// HandleUpdate()

        protected void HandleUnsubscribe()//handle delete confirmation
        {
            string status = "successful";
            if ( CurrentRecord.IsValid )
            {
                status = NewsController.UnSubscribe( CurrentRecord.RowId.ToString() );
                if ( status == "successful" )
                {
                    SetConsoleSuccessMessage( "You have been unsubscribed." );
                    // Do not send an unsubscribe confirmation
                    // SendDeleteConfirmation( CurrentRecord );
                    //reset for a resubscribe?
                    btnIdentify.Visible = true;
                    pnlSubscribe.Visible = false;
                }
                else
                {
                    SetConsoleErrorMessage( "An error has occurred: " + status );
                }
            }
        }// HandleUnsubscribe();

        protected void HandleValidate()
        {
            string status = "successful";
            if ( Action.ToLower() == "validate" || Action == "" )
            {
                //TODO - handle RowId processing==> 
                string guid = GetRequestKeyValue( "RowId", "" );
                if ( guid == "" )
                {
                    SetConsoleErrorMessage( "Invalid RowId parameter!" );
                }
                else
                {
                    CBLCategoryNewsletter.Visible = false;
                    CurrentRecord = NewsController.SubscriptionGet( guid );

                    if ( CurrentRecord == null || !CurrentRecord.IsValid )
                    {
                        SetConsoleErrorMessage( "Subscription record not found!" );

                        //show button to allow a subscribe
                        btnIdentify.Visible = true;
                    }
                    else
                    {
                        CurrentRecord.IsValidated = true;
                        status = NewsController.SubscribeUpdate( CurrentRecord );
                        if ( status == "successful" )
                        {
                            pnlSubscribeMsg.Visible = false;
                            pnlGuestMsg.Visible = true;
                            txtDesc2.Text = txtDesc2.Text.Replace( "@RowId", CurrentRecord.RowId.ToString() );
                            pnlUpdatedEmail.Visible = false;
                            pnlDetail.Visible = false;
                            CurrentRecord.IsValidated = true;
                            SetButtonVisibility();

                        }
                        else
                        {
                            SetConsoleErrorMessage( "An error has occurred: " + status );
                        }
                    }
                }
            }
            else
            {
                if ( CurrentRecord.IsValid )
                {
                    SendDeleteConfirmation( CurrentRecord );
                    SetConsoleSuccessMessage( "You should receive a confirmation e-mail shortly.  Follow the instructions in that e-mail to confirm your subscription." );
                }
            }
        }// HandleValidate()

        protected void SendConfirmationEmail( string guid, string pEmail, string subscribeUrl, string title )
        {

            EmailNotice notice = EmailController.GetByCode( "NewsValidationEmail" );
            if ( notice.HtmlBody.Trim().Length == 0 )
            {
                //should only happen in dev env. show error message
                SetConsoleErrorMessage( "Error: the request email notice (NewsValidationEmail) was not found." );
            }
            string emailMessage = notice.HtmlBody;
            string host = Request.Url.Host;
            //TODO - handle RowId processing==> 
            string relativeUrl = string.Format( subscribeUrl + "?action=validate&RowId={0}", guid );
            string absoluteUrl = UtilityManager.FormatAbsoluteUrl( relativeUrl, host, false );

            string url = string.Format( txtSubscribeUrl.Text, absoluteUrl );
            emailMessage = emailMessage.Replace( "@SubscribeUrl", url );

            string newsTitle = title;
            if ( newsTitle.ToLower().IndexOf( "news" ) > -1 )
            {
                newsTitle = newsTitle.Replace( " News", "" );
                newsTitle = newsTitle.Replace( " news", "" );
            }
            notice.Subject = notice.Subject.Replace( "@NewsTitle", newsTitle );

            notice.Message = emailMessage.Replace( "@NewsTitle", newsTitle );
            //EmailController.Send( notice, pEmail );
            EmailManager.SendEmail(pEmail, notice.FromEmail, notice.Subject, emailMessage, "", "");
        }// SendConfirmationEmail

        /// <summary>
        /// SendUpdateConfirmation
        /// NOTES: do we need to provide a dynamic url and title??
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pEmail"></param>
        protected void SendUpdateConfirmation( string guid, string pEmail )
        {
            //TODO - for updates, do we only have the context of a single subscription, ie, the current path?????

            EmailNotice notice = EmailController.GetByCode( "NewsUpdateEmail" );
            if ( notice.HtmlBody.Trim().Length == 0 )
            {
                //should only happen in dev env. show error message
                SetConsoleErrorMessage( "Error: the request email notice (NewsUpdateEmail) was not found." );
            }
            string emailMessage = notice.HtmlBody;
            string host = Request.Url.Host;
            //TODO - handle RowId processing==> 
            string relativeUrl = string.Format( SubscribeUrl + "?action=update&RowId={0}&email={1}",
              guid, pEmail );
            string absoluteUrl = UtilityManager.FormatAbsoluteUrl( relativeUrl, host, false );

            string url = string.Format( txtSubscribeUpdateUrl.Text, absoluteUrl );
            emailMessage = emailMessage.Replace( "@SubscribeUrl", url );

            //								 ????????????
            string newsTitle = AnnouncementTitle;
            if ( newsTitle.ToLower().IndexOf( "news" ) > -1 )
            {
                newsTitle = newsTitle.Replace( " News", "" );
                newsTitle = newsTitle.Replace( " news", "" );
            }
            emailMessage = emailMessage.Replace( "@NewsTitle", newsTitle );
            notice.Subject = notice.Subject.Replace( "@NewsTitle", newsTitle );

            notice.Message = emailMessage;
            //EmailController.Send( notice, pEmail );
            EmailManager.SendEmail(pEmail, notice.FromEmail, notice.Subject, emailMessage, "", "");
        }// SendUpdateConfirmation

        /// <summary>
        /// SendDeleteConfirmation
        /// NOTES: do we need to provide a dynamic url and title??
        /// </summary>
        /// <param name="AppItemAnnouncementSubscription"></param>
        protected void SendDeleteConfirmation( AppItemAnnouncementSubscription entity )//send delete confirmation e-mail
        {
            EmailNotice notice = EmailController.GetByCode( "NewsUnsubscribeEmail" );
            if ( notice.HtmlBody.Trim().Length == 0 )
            {
                //should only happen in dev env. show error message
                SetConsoleErrorMessage( "Error: the request email notice (NewsUnsubscribeEmail) was not found." );
            }
            string emailMessage = notice.HtmlBody;
            string host = Request.Url.Host;

            //									 ????????????
            string relativeUrl = SubscribeUrl;
            string absoluteUrl = UtilityManager.FormatAbsoluteUrl( relativeUrl, host, false );

            string url = string.Format( txtREsubscribeUrl.Text, absoluteUrl );
            emailMessage = emailMessage.Replace( "@SubscribeUrl", url );

            string newsTitle = AnnouncementTitle;
            if ( newsTitle.ToLower().IndexOf( "news" ) > -1 )
            {
                newsTitle = newsTitle.Replace( " News", "" );
                newsTitle = newsTitle.Replace( " news", "" );
            }
            emailMessage = emailMessage.Replace( "@NewsTitle", newsTitle );
            notice.Subject = notice.Subject.Replace( "@NewsTitle", newsTitle );

            notice.Message = emailMessage;
            //EmailController.Send( notice, entity.Email );
            EmailManager.SendEmail( entity.Email, notice.FromEmail, notice.Subject, emailMessage, "", "" );
        }

        /// <summary>
        /// Actions via email ????
        /// Need to test thoroughly
        /// </summary>
        /// <param name="pId"></param>
        protected void ProcessActionFields( string guid )
        {
            CurrentRecord = NewsController.SubscriptionGet( guid );
            switch ( Action.ToLower() )
            {
                case "identify":
                    txtEmail.Text = GetRequestKeyValue( "email", "" );
                    HandleIdentify();
                    break;
                case "step1":
                    pnlSubscribeMsg.Visible = true;
                    pnlGuestMsg.Visible = false;
                    pnlUpdatedEmail.Visible = false;
                    pnlDetail.Visible = false;
                    break;
                case "delete":
                    HandleUnsubscribe();
                    pnlSubscribe.Visible = false;
                    break;
                case "update":
                    HandleUpdate();
                    break;
                case "validate":
                    HandleValidate();
                    break;
                default:
                    SetConsoleErrorMessage( "Invalid action parameter!" );
                    break;
            }
        }
    }

}
