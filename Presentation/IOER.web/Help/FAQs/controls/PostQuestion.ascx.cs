using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using EmailHelper = ILPathways.Utilities.EmailManager;
using MyManager = IOER.Controllers.FaqController;
using ILPathways.Business;
using IOER.classes;
using BDM = ILPathways.Common;
using IOER.Controllers;
//using ILPathways.DAL;
using IOER.Library;
using ILPathways.Utilities;
using LRWarehouse.Business;
using LRDAL = LRWarehouse.DAL.DatabaseManager;

namespace IOER.Controls.FAQs
{
    public partial class PostQuestion : BaseUserControl
    {
        AppUser currentUser;
        //initialize page manager
        MyManager myManager = new MyManager();

        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
        const string thisClassName = "@controls_FAQ_PostQuestion";
        string statusMessage = "";

        #region Properties
        private bool _allowingQuestions = false;
        public bool AllowingQuestions
        {
            get { return _allowingQuestions; }
            set { _allowingQuestions = value; }
        }
        /// <summary>
        /// INTERNAL PROPERTY: CurrentRecord
        /// Set initially and store in ViewState
        /// </summary>
        protected FaqItem CurrentRecord
        {
            get { return ViewState[ "CurrentRecord" ] as FaqItem; }
            set { ViewState[ "CurrentRecord" ] = value; }
        }
        /// <summary>
        /// Get/Set default category. If blank, may want to show category list
        /// </summary>
        public string DefaultCategory
        {
            get { return defaultCategory.Text; }
            set { defaultCategory.Text = value; }
        }

        public string DefaultTargetPathways
        {
            get { return defaultTargetPathways.Text; }
            set { defaultTargetPathways.Text = value; }
        }

        /// <summary>
        /// refers to the appKey value that will hold the email address of the Faq manager for a particular instance of Faqs.  A default value is provided. It can be overridden by adding a parameter to the mcms placeholder. 
        /// </summary>
        public string EmailAppKey
        {
            get { return emailAppKey.Text; }
            set { emailAppKey.Text = value; }
        }

        public string SubcategoryTitle
        {
            get { return subcategoryTitle.Text; }
            set { subcategoryTitle.Text = value; }
        }
        private bool _mustBeAuthenticated = false;
        public bool MustBeAuthenticated
        {
            get { return _mustBeAuthenticated; }
            set { _mustBeAuthenticated = value; }
        }
        public string ConfirmLink
        {
            get { return confirmLink.Text; }
            set { confirmLink.Text = value; }
        }
        public string FaqCategoryTitle
        {
            get { return faqCategoryTitle.Text; }
            set { faqCategoryTitle.Text = value; }
        }
        #endregion

        /// <summary>
        /// Handle Page Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void Page_Load( object sender, EventArgs ex )
        {

            if ( Page.IsPostBack == false )
            {
                if (AllowingQuestions)
                    this.InitializeForm();
            }
        }//

        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        public void InitializeForm()
        {
            txtId.Text = "";
            this.btnSave.Enabled = true;

            if ( IsUserAuthenticated() && WebUser.Email.Length > 0 )
            {
                txtEmail.Text = WebUser.Email;
                txtEmail.Enabled = false;
                this.txtConfirmEmail.Text = WebUser.Email;
                txtConfirmEmail.Enabled = false;
            }

            if ( showingSubcategory.Text.Equals( "Yes" ) )
                this.subcatPanel.Visible = true;

            //check for mcms parms
            //string pageControl = "";    //McmsHelper.GetPageControl();
            //string defCategory = ContentHelper.ExtractNameValue( pageControl, "DefaultCategory", ";" );
            //if ( defCategory.Length > 0 )
            //    DefaultCategory = defCategory;
            //string emailAppKey = ContentHelper.ExtractNameValue( pageControl, "EmailAppKey", ";" );
            //if ( emailAppKey.Length > 0 )
            //    EmailAppKey = emailAppKey;

            //string url = CmsHttpContext.Current.Posting.Url;
            string url = Request.RawUrl;
            if ( ConfirmLink.ToLower().IndexOf( url.ToLower() ) == -1 )
            {
                ConfirmLink = url + "?id={0}";
            }

            // Set source for form lists
            this.PopulateControls();

            CheckRecordRequest();

            //optionally format help sections
            //this.FormatHelpItem( lblStatus, lblStatus_Help, true );
        }	// 

        /// <summary>
        /// allow parent to reset category (in case wasn't properly set or derived.
        /// </summary>
        public void ResetDefaultCategory( string category )
        {
            if ( category.Length > 0 )
            {
                DefaultCategory = category;
                this.PopulateControls();

            }
        }	// End 
        /// <summary>
        /// Check for a question confirmation
        /// </summary>
        private void CheckRecordRequest()
        {
            string id = this.GetRequestKeyValue( "id", "" );
            if ( id.Trim().Length == 36 )
            {
                //may want to have an act1on??
                this.Get( id );

            }

        }	// End 

        /// <summary>
        /// Get - retrieve form data
        /// </summary>
        /// <param name="recId"></param>
        private void Get( string id )
        {
            try
            {
                if ( myManager.ConfirmQuestion( id, ref statusMessage ) )
                {
                    //send dceo email
                    SendMgrEmail( id );
                    this.SetConsoleSuccessMessage( authUserConfirmMsg.Text );
                }

                //get record
                //CurrentRecord = myManager.Get( id );

                //if ( CurrentRecord != null && CurrentRecord.HasValidRowId()
                //    && CurrentRecord.Status == "Pending" )
                //{
                //    //change to status?
                //    CurrentRecord.Status = "Submitted";
                //    //txtId.Text = CurrentRecord.RowId.ToString();
                //    //statusMessage = myManager.Update( CurrentRecord );
                //    myManager.ConfirmQuestion( id, ref statusMessage );
                //    //send dceo email
                //    SendMgrEmail( CurrentRecord );
                //    this.SetConsoleSuccessMessage( authUserConfirmMsg.Text );
                //}

            }
            catch ( System.Exception ex )
            {
                //Action??		- display message and close form??	
                LoggingHelper.LogError( ex, thisClassName + ".Get() - Unexpected error encountered" );
                this.ResetForm();
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );

            }

        }	// End method

        protected void Category_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            try
            {
                if ( ddlCategory.SelectedIndex > 0 )
                {
                    int categoryId = Int32.Parse( this.ddlCategory.SelectedValue );
                    SetSubcategoryList( categoryId );
                }
            }
            catch ( Exception ex )
            {
                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.Message );
                this.LogError( ex, thisClassName + ".Category_OnSelectedIndexChanged" );
            }
        }


        /// <summary>
        /// Handle a form button clicked 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        public void FormButton_Click( Object sender, CommandEventArgs ex )
        {

            switch ( ex.CommandName )
            {
                //case "New":
                //  this.HandleNewRequest();
                //  break;
                case "Update":
                    if ( this.IsFormValid() )
                    {
                        this.UpdateForm();
                    }

                    break;

            }
        } // end 

        #region Validation Methods
        /// <summary>
        /// validate form content
        /// </summary>
        /// <returns></returns>
        private bool IsFormValid()
        {
            //int recId = 0;
            bool isValid = true;

            try
            {
                txtTitle.Text = FormHelper.CleanText( txtTitle.Text );
                vsErrorSummary.Controls.Clear();

                Page.Validate();
                //other edits

                //recId = int.Parse( this.Id.Text );

                // check additional required fields
                isValid = HasRequiredFields();
                if ( isValid )
                    isValid = DoEmailValidation();
                if ( isValid )
                    isValid = DoBadWordsCheck();

            }
            catch ( System.Exception ex )
            {
                //SetFormMessage( "Unexpected error encountered<br>Contact system administration.<br>" + ex.ToString() );
                return false;
            }

            if ( vsErrorSummary.Controls.Count > 0 || isValid == false )
            {
                return false;
            }
            else
            {
                return true;
            }
        } //

        /// <summary>
        /// verify required fields are present
        /// </summary>
        /// <returns></returns>
        private bool HasRequiredFields()
        {
            bool isValid = true;
            try
            {
                //check for potential bot filling
                if ( this.firstName.Value.Length > 0 | this.lastName.Value.Length > 0 )
                {
                    //unexpected value in hidden field - reject
                    SetConsoleErrorMessage( "Invalid request" );
                    Response.Redirect( "/", true );
                    return false;
                }
            }
            catch
            {

            }
            if ( this.ddlCategory.SelectedIndex < 1 )
            {
                SetConsoleErrorMessage( "A category must be selected for an FAQ item" );
                isValid = false;
                this.AddReqValidatorError( this.vsErrorSummary, "A category must be selected for an FAQ item", "txtTitle", true );
            }


            if ( subcatPanel.Visible && this.ddlSubcategory.SelectedIndex < 1 )
            {
                SetConsoleErrorMessage( "A " + SubcategoryTitle + " must be selected" );
                isValid = false;
                this.AddReqValidatorError( this.vsErrorSummary, "A " + SubcategoryTitle + " must be selected", "txtTitle", true );
            }


            if ( this.txtTitle.Text.Trim().Length == 0 )
            {
                this.AddReqValidatorError( this.vsErrorSummary, "A question must be entered", "txtTitle", true );
            }

            if ( this.txtEmail.Text.Trim().Length == 0 )
            {
                this.AddReqValidatorError( this.vsErrorSummary, "An email must be entered", "txtEmail", true );
            }

            if ( vsErrorSummary.Controls.Count > 0 || isValid == false )
            {
                return false;
            }
            else
            {
                return true;
            }
        } //

        /// <summary>
        /// Validate entered email
        /// </summary>
        /// <param name="rm"></param>
        private bool DoEmailValidation()
        {
            bool isValid = true;
            if ( txtEmail.Text.Length > 0 )
            {
                revEmail.Validate();
                rfvConfirmEmail.Validate();
                cvEmail.Validate();
                if ( revEmail.IsValid == false )
                {
                    SetConsoleErrorMessage( revEmail.ErrorMessage );
                    isValid = false;
                }
                else
                {
                    //confirm must exist
                    if ( this.txtConfirmEmail.Text.Length == 0 )
                    {
                        SetConsoleErrorMessage( rfvConfirmEmail.ErrorMessage );
                        isValid = false;
                    }
                    else
                    {
                        //check if emails match
                        if ( ( ( cvEmail.IsValid == false ) || ( txtConfirmEmail.Text.ToLower() != txtEmail.Text.ToLower() ) )
                            && ( txtConfirmEmail.Text.Length > 0 || txtEmail.Text.Length > 0 ) )
                        {
                            SetConsoleErrorMessage( cvEmail.ErrorMessage );
                            isValid = false;
                        }
                    }
                }
            }
            else
            {
                SetConsoleErrorMessage( rfvEmail.ErrorMessage );
                isValid = false;
            }

            if ( vsErrorSummary.Controls.Count > 0 || isValid == false )
            {
                return false;
            }
            else
            {
                return true;
            }
        }//

        /// <summary>
        /// Check for bad words
        /// </summary>
        private bool DoBadWordsCheck()
        {
            bool isValid = true;
            string comments = FormHelper.CleanText( this.txtTitle.Text );

            // Check for bad words      
            if ( BadWordChecker.CheckForBadWords( comments ) )
            {
                isValid = false;

                //highlight badwords if found 
                // not sure if applicable for text ????
                string badWords = BadWordChecker.Highlight( comments );

                txtTitle.Text = badWords;

                SetConsoleErrorMessage( "Bad words were found and replaced by #$!@. Please remove and try again!" );

            }
            return isValid;
        } // end 


        #endregion
        #region Form Actions

        /// <summary>
        /// Prepare for a new record request
        /// </summary>
        private void HandleNewRequest()
        {
            this.ResetForm();

        } // end 

        /// <summary>
        /// Reset form values
        /// </summary>
        private void ResetForm()
        {
            //reset form by populating with an empty business object
            FaqItem entity = new FaqItem();
            //do any defaults. 
            entity.IsActive = true;
            entity.HasChanged = false;
            txtId.Text = "";
            txtTitle.Text = "";
            //leave email fields alone to allow entry of multiple questions

            //reset any controls or buttons not handled in the populate method

            //unselect forms list
            this.ddlSubcategory.SelectedIndex = -1;
        }//
        /// <summary>
        /// Handle form update
        /// </summary>
        private void UpdateForm()
        {
            string id = "";
            string msg = "";
            string action = "";

            FaqItem entity = new FaqItem();

            try
            {
                //update is n/a, but left template code in
                id = this.txtId.Text;
                if ( doingFullDebug.Text.Equals( "yes" ) )
                    LoggingHelper.DoTrace( 1, "UpdateForm, 1" );

                if ( id.Length == 0 || id == "0" )
                {
                    LoggingHelper.DoTrace( 1, "UpdateForm, 2" );
                    if ( IsUserAuthenticated() )
                    {
                        entity.CreatedById = WebUser.Id;
                    }
                    entity.CreatedBy = this.txtEmail.Text.Trim();
                    entity.Created = System.DateTime.Now;
                    entity.Status = "Pending";
                    action = "Create";
                    // anything else??

                }
                else
                {
                    // get current record from viewstate
                    entity = CurrentRecord;
                    action = "Update";
                }

                /* assign form fields 			 */
                entity.Title = LRDAL.HandleApostrophes( FormHelper.CleanText( this.txtTitle.Text ) );
                //entity.FaqCode = this.txtEmail.Text.Trim();
                LoggingHelper.DoTrace( 1, "UpdateForm, 3a" );
                //if ( ddlCategory.SelectedIndex > -1 )
                //  entity.CategoryId = int.Parse( this.ddlCategory.SelectedValue.ToString() );
                entity.CategoryId = GetListSelectionId( ddlCategory );
                LoggingHelper.DoTrace( 1, "UpdateForm, 3b" );
                //if (ddlSubcategory.SelectedIndex > -1)
                //  entity.SubcategoryId = int.Parse( this.ddlSubcategory.SelectedValue.ToString() );
                entity.SubcategoryId = GetListSelectionId( ddlSubcategory );
                LoggingHelper.DoTrace( 1, "UpdateForm, 3c" );
                entity.LastUpdated = System.DateTime.Now;
                if ( IsUserAuthenticated() )
                {
                    entity.LastUpdatedBy = WebUser.UserName;
                    entity.LastUpdatedById = WebUser.Id;
                }
                else
                {
                    entity.LastUpdatedBy = this.txtEmail.Text.Trim();
                }

                entity.IsActive = true;

                LoggingHelper.DoTrace( 1, "UpdateForm, 4" );
                //call insert/update
                string statusMessage = "";
                if ( id.Length == 0 )
                {
                    string rowId = myManager.Create( entity, ref statusMessage );
                    LoggingHelper.DoTrace( 1, "UpdateForm, 5" );
                    if ( rowId.Length > 0 )
                    {
                        this.txtId.Text = rowId.ToString();
                        entity.RowId = new Guid( rowId );
                        //add to 
                        if ( DefaultTargetPathways.Length > 0 )
                        {
                            foreach ( string item in DefaultTargetPathways.Split( ',' ) )
                            {
                                if ( item.Length > 0 && this.IsInteger( item ) )
                                {
                                    int pPathwayId = Int32.Parse( item );
                                    myManager.FaqPathway_Insert( entity.RowId, pPathwayId, ref statusMessage );
                                }
                            }
                        }

                        if ( IsUserAuthenticated() )
                        {
                            SendMgrEmail( entity );
                            this.SetConsoleSuccessMessage( authUserConfirmMsg.Text );
                        }
                        else
                        {
                            SendConfirmEmail( entity );
                            this.SetConsoleSuccessMessage( guestUserConfirmMsg.Text );
                        }
                    }

                }
                else
                {
                    statusMessage = myManager.Update( entity );
                }


                if ( statusMessage.Equals( "successful" ) )
                {

                    this.ResetForm();
                }
                else
                {
                    this.SetConsoleErrorMessage( statusMessage );
                }

            }
            catch ( System.Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".UpdateForm() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
            }
        }

        protected int GetListSelectionId( DropDownList list )
        {
            int value = 0;
            try
            {
                if ( list.SelectedIndex > -1 )
                    value = int.Parse( list.SelectedValue.ToString() );
            }
            catch ( System.Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".GetListSelectionId() - Unexpected error encountered" );
                LoggingHelper.DoTrace( 1, "GetListSelectionId, 2<br/>" + ex.Message );
                value = 0;
            }

            return value;
        }

        protected void SendConfirmEmail( FaqItem entity )
        {
            bool sendSuccessful = true;
            string infoEmail = UtilityManager.GetAppKeyValue( "contactUsMailTo", "" );
            string adminEmail = UtilityManager.GetAppKeyValue( "systemAdminEmail", "" );
            //string statusMessage = "";


            EmailNotice notice = new EmailNotice();
            notice.Subject = userEmailSubject.Text;

            try
            {
                string toEmail = this.txtEmail.Text;

                string faqUrl = string.Format( ConfirmLink, entity.RowId );
                faqUrl = UtilityManager.FormatAbsoluteUrl( faqUrl, true );
                string subject = "";
                if ( this.ddlSubcategory.SelectedIndex > 0 )
                {
                    subject = string.Format( subjectTemplate.Text, this.ddlSubcategory.SelectedItem.Text );
                }
                string emailMessage = string.Format( userEmailTemplate.Text, subject, this.txtTitle.Text, faqUrl );
                notice.FromEmail = infoEmail;
                //
                if ( doingBccInfoOnConfirmEmail.Text.Equals( "yes" ) )
                {
                    notice.BccEmail = infoEmail;
                }
                if ( doingBccAdminOnConfirmEmail.Text.Equals( "yes" ) )
                {
                    notice.BccEmail += notice.BccEmail.Length > 0 ? ", " + adminEmail : adminEmail;
                }

                notice.Message = emailMessage;
                //send email
                EmailManager.SendEmail( toEmail, notice );


            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".SendConfirmEmail() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
                sendSuccessful = false;
            }


            //return sendSuccessful;
   }//

        protected void SendMgrEmail( FaqItem entity )
        {
        }//

        protected void SendMgrEmail( string id )
        {
            bool sendSuccessful = true;
            string infoEmail = UtilityManager.GetAppKeyValue( "contactUsMailTo", "" );
            string toEmail = "mparsons@siuccwd.com";    // infoEmail;

            string adminUserId = UtilityManager.GetAppKeyValue( EmailAppKey, "programAdmin" );
            //string statusMessage = "";
            string questionFromEmail = "mparsons@siuccwd.com"; // entity.CreatedBy;
            AppUser dceoContact = new AppUser();
                //UserManager.( adminUserId, workNet.Controllers.AccountController.SpecialPasswordEncrypted, ref statusMessage );

            EmailNotice notice = new EmailNotice();
            //if ( dceoContact != null && dceoContact.Id > 0 )
            //    toEmail = dceoContact.Email;

            notice.Subject = dceoEmailSubject.Text;

            try
            {
                string faqUrl = string.Format( faqLink.Text, id );
                string autoLink = "";   //UserProfileController.FormatAutoLoginLink( dceoContact, autoLoginTemplate.Text, faqUrl, true );
                string subject = "";
                //if ( entity.Subcategory.Length > 0 )
                //{
                //    subject = string.Format( subjectTemplate.Text, entity.Subcategory );
                //}
                string emailMessage = string.Format( dceoEmailTemplate.Text, questionFromEmail, subject, "entity.Title", autoLink );
                notice.FromEmail = questionFromEmail;
                //notice.CcEmail = this.txtEmail.Text;

                notice.Message = emailMessage;
                //send email
                EmailManager.SendEmail( toEmail, notice );


            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".SendMgrEmail() - Unexpected error encountered" );
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
                sendSuccessful = false;
            }


            //return sendSuccessful;
        }//

        #endregion

        #region Housekeeping
        /// <summary>
        /// Populate form controls
        /// </summary>
        /// <param name="recId"></param>
        private void PopulateControls()
        {

            SetCategoryList();
            if ( DefaultCategory.Length > 0 )
            {
                //this.SetListSelection( this.ddlCategory, DefaultCategory );
                SetListSelectionByText( ddlCategory, DefaultCategory );
                SetSubcategoryList( DefaultCategory );

            }
            else
            {
                this.catPanel.Visible = true;
            }

        } //
        /// <summary>
        /// populate the main form list
        /// </summary>
        private void SetCategoryList()
        {
            ddlCategory.Items.Clear();

            DataSet ds = LRDAL.DoQuery( sqlSelectSearchCategories.Text );
            LRDAL.PopulateList( this.ddlCategory, ds, "Id", "Category", "Select a Category" );

        } //

        private void SetSubcategoryList( int pCategoryId )
        {

            ddlSubcategory.Items.Clear();

            string sql = string.Format( sqlSelectSubcategoriesById.Text, pCategoryId.ToString() );
            DataSet ds = LRDAL.DoQuery( sql );
            LRDAL.PopulateList( ddlSubcategory, ds, "Id", "Title", "Select a " + SubcategoryTitle );

        } //

        private void SetSubcategoryList( string pCategory )
        {

            ddlSubcategory.Items.Clear();

            string SUBCATEGORY_SELECT_SQL = sqlSelectSubcategoriesByCategory.Text;

            string sql = string.Format( SUBCATEGORY_SELECT_SQL, pCategory.ToString() );
            DataSet ds = LRDAL.DoQuery( sql );
            LRDAL.PopulateList( ddlSubcategory, ds, "Id", "Title", "Select a " + SubcategoryTitle );

        } //

        #endregion
    }

}