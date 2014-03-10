using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Business;
using ILPathways.Library;
using ILPathways.Utilities;
using EmailHelper = ILPathways.Utilities.EmailManager;
using MyManager = Isle.BizServices.LibraryBizService;
using ENoticeMgr = Isle.BizServices.EmailServices;

namespace ILPathways.Controls.Libraries
{
    public partial class LibraryMemberInvite : BaseUserControl
    {
        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
        const string thisClassName = "LibraryMemberInvite";

        //initialize page manager
        MyManager myManager = new MyManager();
        public enum EInvitationType
        {
            Individual,
            Group
        }

        #region Properties
        /// <summary>
        /// INTERNAL PROPERTY: CurrentRecord
        /// Set initially and store in ViewState
        /// </summary>
        protected LibraryInvitation CurrentRecord
        {
            get { return ViewState[ "CurrentRecord" ] as LibraryInvitation; }
            set { ViewState[ "CurrentRecord" ] = value; }
        }
        /// <summary>
        /// Get/Set InvitationType property
        /// Individual, Group
        /// </summary>
        public EInvitationType InvitationType
        {
            get
            {
                if ( ViewState[ "InvitationType" ] == null || ViewState[ "InvitationType" ].ToString() == "" )
                    ViewState[ "InvitationType" ] = EInvitationType.Individual;

                return ( EInvitationType ) ViewState[ "InvitationType" ];
            }
            set { ViewState[ "InvitationType" ] = value; }
        } //
        #endregion

        protected void Page_Load( object sender, EventArgs e )
        {

            if ( Page.IsPostBack )
            {

            }
            else
            {
                this.InitializeForm();
            }
        }//

        private void InitializeForm()
        {
            CurrentRecord = new LibraryInvitation();

            //= Add attribute to btnDelete to allow client side confirm
            btnAccept.Attributes.Add( "onClick", "return confirmAccept(this);" );
            btnDeny.Attributes.Add( "onClick", "return confirmDeny(this);" );

            //could be indv or group (maybe use separate page for group reg - less complex)
            string inviteId = this.GetRequestKeyValue( "invite", "" );
            string inviteType = "TBD-Type";

            string thisUrl = "";
            string loginUrl = UtilityManager.GetAppKeyValue( "loginUrl", "/Account/Login.aspx" );
            string registerUrl = UtilityManager.GetAppKeyValue( "registerPage", "/Account/Register.aspx" );

            if ( inviteId.Length > 20 )
            {
                thisUrl = string.Format( inviteUrl.Text + "?invite={0}", inviteId );

            }
            else
            {
                if ( inviteType.ToLower().Equals( "group" ) )
                    thisUrl = groupInviteUrl.Text;
                else
                    thisUrl = inviteUrl.Text;
            }

            this.loginLink.NavigateUrl = loginUrl + "?nextUrl=" + thisUrl;
            this.registerLink.NavigateUrl = registerUrl + "?nextUrl=" + thisUrl;

            profileReturnUrl.Text = this.profileUrl.Text + "nextUrl=" + thisUrl;

            if ( this.IsUserAuthenticated() )
            {
                this.authCheckPanel.Visible = false;
                CheckRecordRequest( inviteId, inviteType );
            }
            else
            {
                this.authCheckPanel.Visible = true;

            }

        }	// End 

        #region retrieval
        /// <summary>
        /// Check the request type for the form
        /// </summary>
        private void CheckRecordRequest( string inviteId, string inviteType )
        {

            if ( inviteId.Length > 20 )
            {
                this.Get( inviteId );

            }
            else
            {
                if ( inviteType.ToLower().Equals( "group" ) )
                {
                    passcodePanel.Visible = true;
                }
                else
                {
                    noInviteCodePanel.Visible = true;
                }
            }


        }	// End 

        /// <summary>
        /// Get - retrieve form data
        /// </summary>
        /// <param name="recId"></param>
        private void Get( string rowId )
        {
            try
            {
                //get record
                LibraryInvitation entity = myManager.LibraryInvitation_GetByGuid( rowId );

                if ( entity == null || entity.IsValid == false )
                {
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
                //this.ResetForm();
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );

            }

        }	// End method

        private void GetByPasscode( string pGroupPassCode )
        {
            try
            {
                //get record
                LibraryInvitation entity = myManager.LibraryInvitation_GetByPasscode( pGroupPassCode );

                if ( entity == null || entity.IsValid == false )
                {
                    this.SetConsoleErrorMessage( "Sorry the requested pass code does not exist" );
                    return;

                }
                else
                {
                    passcodePanel.Visible = false;
                    PopulateForm( entity );
                }

            }
            catch ( System.Exception ex )
            {
                //Action??		- display message and close form??	
                LoggingHelper.LogError( ex, thisClassName + ".Get() - Unexpected error encountered" );
                //this.ResetForm();
                this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );

            }

        }	// End method

        /// <summary>
        /// Populate the form  - LibraryInvitation
        ///</summary>
        private void PopulateForm( LibraryInvitation entity )
        {
            CurrentRecord = entity;
            acceptPanel.Visible = true;
            acceptPanel.Enabled = true;
            lblInviteSummary.Text = "Invitation Request from:";
            this.txtRowId.Text = entity.RowId.ToString();

            //if ( entity.ParentId.Length > 0 )
            //    lblInviteSummary.Text = "<h3>" + entity.Subject + "</h3>";

            //if ( entity.MessageContent.Length > 0 )
            //    lblInviteSummary.Text += "<br/>" + entity.MessageContent;

            lblInviteSummary.Text += "<p>" + entity.InvitationByUser.EmailSignature() + "</p>";

            //ensure has an email 
            if ( WebUser.Email.Length == 0 )
            {
                acceptPanel.Enabled = false;
                string profileLink = string.Format( profileReturnTemplate.Text, profileReturnUrl.Text );

                SetConsoleErrorMessage( "Error: you do not have an email address associated with your profile. Please update your profile with the email address that was provided to the case manager.<br/>" + profileLink );
                return;

            }

            if ( InvitationType == EInvitationType.Individual )
            {
                if ( WebUser.Email.ToLower() != CurrentRecord.TargetEmail.ToLower() )
                {
                    acceptPanel.Enabled = false;
                    SetConsoleErrorMessage( "Error: your email address associated with your profile does not match the email address for this invitation. Please update your profile with the email address that was provided to the case manager." );
                    return;
                }

                if ( CurrentRecord.Response.Length > 0 )
                {
                    SetConsoleErrorMessage( "You have already responded to this invitation. You "
                                        + CurrentRecord.Response + " the invitation on " + CurrentRecord.ResponseDate.ToString( "MMM dd,yyyy" ) );
                    acceptPanel.Enabled = false;
                    return;
                }


                //check if already a member of this group
                if ( myManager.IsLibraryMember( CurrentRecord.ParentId, WebUser.Id ) == true )
                {
                    SetConsoleErrorMessage( "You are already a member of this library. No action is necessary on your part. The requesting case manager will be notified that you are already in this group." );
                    acceptPanel.Enabled = false;

                    //update invite and notify sender?
                    CurrentRecord.Response = "Ignored (Duplicate Invitation)";
                    CurrentRecord.TargetUserId = WebUser.Id;
                    //??????myManager.Response( CurrentRecord );
                    NotifyResponse( true );
                    return;
                }
            }
            else
            {
                if ( myManager.IsLibraryMember( CurrentRecord.ParentId, WebUser.Id ) == true )
                {
                    SetConsoleErrorMessage( "You are already a member of this library. No action is necessary on your part." );
                    acceptPanel.Enabled = false;
                    return;
                }
            }


            //check expiry date
            if ( CurrentRecord.ExpiryDate < System.DateTime.Now )
            {
                SetConsoleErrorMessage( "This invitation has expired. Please contact your case manager/instructor for another invitation." );
                acceptPanel.Enabled = false;
                return;
            }
        }//

        #endregion
        protected void btnValidateCode_Click( object sender, EventArgs e )
        {
            if ( this.txtPasscode.Text.Trim().Length == 0 )
            {
                SetConsoleErrorMessage( "Error a pass code must be entered." );
                return;
            }

            InvitationType = EInvitationType.Group;
            //now retrieve the invite by the pass code --> note have to prevent duplicate passcodes on entry!
            string passcode = FormHelper.SanitizeUserInput( this.txtPasscode.Text.Trim() );
            GetByPasscode( passcode );

        }//


        protected void btnAccept_Click( object sender, EventArgs e )
        {
            //add to group
            LibraryMember mbr = new LibraryMember();
            string statusMessage = "";
            //do a quick check for repeated accepts
            if ( myManager.IsLibraryMember( CurrentRecord.ParentId, WebUser.Id ) == true )
            {
                SetConsoleErrorMessage( "You are already a member of this library. " );
                acceptPanel.Enabled = false;
                return;
            }

            try
            {

                //add group member			
                mbr.ParentId = CurrentRecord.ParentId;
                mbr.UserId = WebUser.Id;
                mbr.IsActive = true;
                mbr.MemberTypeId = 3; ///??????
                mbr.CreatedBy = WebUser.UserName;
                mbr.CreatedById = CurrentRecord.CreatedById;

                int mbrId = myManager.LibraryMember_Create( mbr.ParentId, mbr.UserId, mbr.MemberTypeId, WebUser.Id, ref statusMessage );
                if ( mbrId > 0 )
                {
                    //update invite and notify sender?
                    CurrentRecord.Response = "Accepted";
                    CurrentRecord.TargetUserId = WebUser.Id;
                    
                    NotifyResponse( true );

                    if ( afterAcceptUrl.Text.Length > 0 )
                    {
                        Response.Redirect( afterAcceptUrl.Text, true );
                    }

                }
                else
                {
                    LoggingHelper.LogError( thisClassName + ".btnAccept_Click(). Add group member failed: " + statusMessage, true );
                    this.SetConsoleErrorMessage( "Unexpected error encountered accepting invitation. The system administrator has been notified:<br/>" + statusMessage );
                }
            }
            catch ( System.Threading.ThreadAbortException taex )
            {
                //ignore 
                //LoggingHelper.LogError( ex, thisClassName + ".SendInvitation() - Unexpected error encountered" );


            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".btnAccept_Click() " );
                this.SetConsoleErrorMessage( "Unexpected error encountered adding selected user. The system administrator has been notified:<br/>" + ex.Message.ToString() );
            }

        }
        protected void btnDeny_Click( object sender, EventArgs e )
        {
            try
            {
                //update invite and notify sender?
                CurrentRecord.Response = "Denied";
                CurrentRecord.TargetUserId = WebUser.Id;
                //myManager.Response( CurrentRecord );

                NotifyResponse( false );

                //SetConsoleSuccessMessage( string.Format( denyConfirmationMessage.Text, CurrentRecord.InvitationByUserUser.FullName() ) );
                if ( afterAcceptUrl.Text.Length > 0 )
                {
                    Response.Redirect( afterAcceptUrl.Text, true );
                }

            }
            catch ( System.Threading.ThreadAbortException taex )
            {
                //ignore 
                //LoggingHelper.LogError( ex, thisClassName + ".SendInvitation() - Unexpected error encountered" );


            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".btnDeny_Click() " );
                this.SetConsoleErrorMessage( "Unexpected error encountered adding selected user. The system administrator has been notified:<br/>" + ex.Message.ToString() );
            }

        }

        private bool NotifyResponse( bool didAccept )
        {
            bool isValid = true;
            string emailMessage = "";
            string statusMessage = "";

            btnAccept.Enabled = false;
            btnDeny.Enabled = false;

            try
            {
                //future  may need to check if in group and remove
                EmailNotice notice = new EmailNotice();
                string emailNoticeCode = this.responseEmailCode.Text;

                notice = ENoticeMgr.EmailNotice_Get( emailNoticeCode );
                if ( notice.HtmlBody.Trim().Length == 0 )
                {
                    //error
                    SetConsoleErrorMessage( "Error - the e-mail template is missing, system administation has been notified." );
                    LoggingHelper.LogError( thisClassName + String.Format( ".SendNotice. Error missing e-mail notice. Code: {0}", emailNoticeCode ), true );
                    return false;
                }

                string toEmail = CurrentRecord.InvitationByUser.Email;
                //response is from user
                notice.FromEmail = WebUser.Email;
                notice.Subject += " " + CurrentRecord.Response;
                emailMessage = notice.HtmlBody;

                //substitutions
                emailMessage = emailMessage.Replace( "[response]", CurrentRecord.Response );

                //optionally add the senders email signature
                emailMessage += WebUser.EmailSignature();


                //when using the email notice as the parameter, the .Message property should contain the actual email to send
                notice.Message = emailMessage;
                EmailHelper.SendEmail( toEmail, notice );

                if ( didAccept )
                {
                    SetConsoleSuccessMessage( string.Format( acceptConfirmationMessage.Text, CurrentRecord.InvitationByUser.FullName() ) );
                }
                else
                {
                    SetConsoleSuccessMessage( string.Format( denyConfirmationMessage.Text, CurrentRecord.InvitationByUser.FullName() ) );
                }

            }
            catch ( System.Threading.ThreadAbortException taex )
            {
                //ignore 
                //LoggingHelper.LogError( ex, thisClassName + ".SendInvitation() - Unexpected error encountered" );
                isValid = true;


            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".SendInvitation() - Unexpected error encountered" );
                isValid = false;
            }

            return isValid;


        }
    }

}