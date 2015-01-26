using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Utilities;
using ILPathways.classes;
using ILPathways.Business;
using Isle.BizServices;
using LRWarehouse.Business;
using ILPathways.Controllers;
using LDAL = LRWarehouse.DAL;
using MyManager = Isle.BizServices.AccountServices;
using CurrentUser = LRWarehouse.Business.Patron;


namespace ILPathways.secure.controls
{
    public partial class IsleSSO : ILPathways.Library.BaseUserControl
    {
        MyManager myManager = new MyManager();
        CurrentUser currentUser = new CurrentUser();
        public string errorMessage = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                InitializeForm();
            }
        }

        protected void InitializeForm()
        {
            if (Request.ServerVariables["HTTP_SHIBIDENTITYPROVIDER"] != null && Request.ServerVariables["HTTP_SHIBIDENTITYPROVIDER"] != string.Empty)
            {
                HandleShibboleth();
            }
        }

        protected void HandleShibboleth()
        {
            string whereTo = "";
            Patron applicant = new Patron();
            bool shouldUseProductionHeaders = bool.Parse(useProdHeaders.Text);
            if (shouldUseProductionHeaders)
            {
                /* These are going to be some of the values that are coming over from the production IC IdP in addition to current values.  Per Bernie A'cs (email sent 3/31/14 16:34), values are:
                 * eduPersonPrincipalName:  something like username@schoolDistrictDomainName.illinicloud.org. NB: This is not an email address!
                 * eduPersonAffiliation:    likely a delimited list of values: {faculty, member, employee} - fixed vocabulary, mixed values can be subject to interruption (interpretation??)
                 * eduPersonOrganizationDN: likely a single value defining a tenant's authoritative namespace, ie district87.org or unit5.org.
                 * eduPersonEntitlement:    roles.  Will likely take many forms, but one envisioned is a pseudo URL of the form http://applicationName/role/applicationRole 
                 *                          which will be mapped by either an LDAP or DB query by tenant.  Output might be http://ioer.ilsharedlearning.org/role/principal. */
                applicant.UserName = Request.ServerVariables["HTTP_EPPN"];
                applicant.Email = Request.ServerVariables["HTTP_MAIL"];
                applicant.FirstName = Request.ServerVariables["HTTP_GIVENNAME"];
                applicant.LastName = Request.ServerVariables["HTTP_SN"];
                applicant.Password = UtilityManager.Encrypt("sl&tj#");
                //applicant.Affiliation = Request.ServerVariables["HTTP_UNSCOPED_AFFILIATION"];
                //applicant.Role = Request.ServerVariables["HTTP_ENTITLEMENT"];
                //applicant.Organization = Request.ServerVariables["HTTP_ORG_DN"];
            }
            else
            {
                applicant.UserName = Request.ServerVariables["HTTP_MAIL"];
                applicant.Email = Request.ServerVariables["HTTP_MAIL"];
                applicant.FirstName = Request.ServerVariables["HTTP_GIVENNAME"];
                applicant.LastName = Request.ServerVariables["HTTP_SN"];
                applicant.Password = UtilityManager.Encrypt("sl&tj#");
            }

            string logMessage = string.Format("Values scraped from Shibboleth: Email address: {0}\tUser name: {1}\tFirstName: {2}\tLastName: {3}",
                applicant.Email, applicant.UserName, applicant.FirstName, applicant.LastName);
            LoggingHelper.DoTrace(logMessage);

            AddAndLoginUser(applicant, ref whereTo);
            try
            {
                Response.Redirect(whereTo);
            }
            catch (ThreadAbortException taex)
            {
                // Do nothing, this is okay
            }
        }

        protected void AddAndLoginUser(Patron applicant, ref string whereTo)
        {
            string status = "successful";
            if (myManager.DoesUserEmailExist(applicant.Email))
            {
                // User exists, attempt to log them in.
                applicant = myManager.GetByEmail(applicant.Email);
                currentUser = myManager.Authorize(applicant.UserName, applicant.Password, ref status);
                if (currentUser.IsValid)
                {
                    ActivityBizServices.UserPortalAuthentication( applicant );
                    SessionManager.SetUserToSession(Session, currentUser);
                    if (Request.QueryString["nextUrl"] != null && Request.QueryString["nextUrl"] != string.Empty)
                    {
                        whereTo = BuildUrl(Request.QueryString["nextUrl"]);
                    }
                    else
                    {
                        whereTo = BuildUrl(defaultRedirect.Text);
                    }
                }
                else
                {
                    errorMessage = "Error: Invalid username or password";
                }
            }
            else
            {
                // User does not exist, create it - no confirmation as they are doing SSO
                applicant.IsActive = true;
                applicant.Id = myManager.Create(applicant, ref status);
                if (applicant.IsValid && applicant.Id > 0)
                {
                    SessionManager.SetUserToSession(Session, applicant);

                    string profileMessage = string.Format(profileCreateMessage.Text, profileRedirect.Text);
                    SetConsoleSuccessMessage(profileMessage);

                    OrganizationBizService.AssociateUserWithOrg( applicant );

                    string ipAddress = this.GetUserIPAddress();
                    ActivityBizServices.UserRegistrationFromPortal( applicant, ipAddress );


                    if (Request.QueryString["nextUrl"] != null && Request.QueryString["nextUrl"] != string.Empty)
                    {
                        whereTo = BuildUrl(Request.QueryString["nextUrl"]);
                    }
                    else
                    {
                        whereTo = BuildUrl(defaultRedirect.Text);
                    }
                }
                else
                {
                    errorMessage = "Error: Could not create account";
                }
            }
        }
        private string GetUserIPAddress()
        {
            string ip = "";
            try
            {
                ip = Request.ServerVariables[ "HTTP_X_FORWARDED_FOR" ];
                if ( ip == null || ip == "" || ip.ToLower() == "unknown" )
                {
                    ip = Request.ServerVariables[ "REMOTE_ADDR" ];
                }
            }
            catch ( Exception ex )
            {

            }

            return ip;
        } //
        protected string BuildUrl(string url)
        {
            string retVal = "";
            if (url.IndexOf("http") > -1 && url.IndexOf("{0}") > -1)
            {
                string host = Request.ServerVariables["HTTP_HOST"];
                retVal = string.Format(url, host);
            }
            else
            {
                retVal = url;
            }

            return retVal;
        }

        protected void btnSubmitRegister_Click(object sender, EventArgs e)
        {
            string whereTo = "";
            Patron applicant = new Patron();
            applicant.Email = applicant.UserName = txtEmail.Text;
            applicant.FirstName = txtGivenName.Text;
            applicant.LastName = txtSurname.Text;
            applicant.Password = UtilityManager.Encrypt(txtPassword.Text);

            AddAndLoginUser(applicant, ref whereTo);
            try
            {
                Response.Redirect(whereTo);
            }
            catch (ThreadAbortException taex)
            {
                // Do nothing, this is okay
            }
        }
    }
}