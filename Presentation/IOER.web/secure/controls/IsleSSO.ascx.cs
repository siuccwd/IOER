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
            applicant.Username = Request.ServerVariables["HTTP_MAIL"];
            applicant.Email = Request.ServerVariables["HTTP_MAIL"];
            applicant.FirstName = Request.ServerVariables["HTTP_GIVENNAME"];
            applicant.LastName = Request.ServerVariables["HTTP_SN"];
            applicant.Password = UtilityManager.Encrypt("sl&tj#");

            string logMessage = string.Format("Values scraped from Shibboleth: Email address: {0}\tUser name: {1}\tFirstName: {2}\tLastName: {3}",
                applicant.Email, applicant.Username, applicant.FirstName, applicant.LastName);
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
                currentUser = myManager.Authorize(applicant.Username, applicant.Password, ref status);
                if (currentUser.IsValid)
                {
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
            applicant.Email = applicant.Username = txtEmail.Text;
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