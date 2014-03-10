using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using MyManager = Isle.BizServices.AccountServices;
using ILPathways.Controllers;
using LRWarehouse.DAL;
using LRWarehouse.Business;
using ILPathways.Library;
using ILPathways.Utilities;

namespace ILPathways.Account
{
    public partial class ForgotPassword : BaseAppPage
    {
        MyManager myManager = new MyManager();

        protected void Page_Load( object sender, EventArgs ex )
        {

            if ( Page.IsPostBack == false )
            {
                this.InitializeForm();
            }
        }//

        private void InitializeForm()
        {
            try
            {
                //we don't want addThis on this page, so show literal in master
                Literal showingAddThis = ( Literal ) FormHelper.FindChildControl( Page, "litHidingAddThis" );
                if ( showingAddThis != null )
                    showingAddThis.Visible = true;
            }
            catch
            {
            }
        }


        protected void btnSubmit_Click( object sender, EventArgs e )
        {

            string lookup = FormHelper.CleanText( this.txtEmail.Text.Trim());
            if ( lookup.Length < 4 )
            {
                SetConsoleErrorMessage( "Error - enter a valid email address or logon id" );
                return;
            }


            Patron currentUser = myManager.RecoverPassword( lookup );
            if ( currentUser.IsValid )
            {
                bool isSecure= false;

                if ( UtilityManager.GetAppKeyValue( "SSLEnable", "0" ) == "1" )
                    isSecure = true;

                string url = string.Format( loginLink.Text, currentUser.RowId.ToString() );
                url = UtilityManager.FormatAbsoluteUrl( url, isSecure );
                string message = string.Format( this.recoverMessage.Text, currentUser.FirstName, url );
                string fromEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", "DoNotReply@ilsharedlearning.org" );
                EmailManager.SendEmail( currentUser.Email, fromEmail, "Recover your IOER account password", message, "", addToBcc.Text );

                SetConsoleSuccessMessage( confirmationMessage.Text );

            }
            else
            {
                SetConsoleErrorMessage( "Error - no user information was found." );
            }
        }

    }
}