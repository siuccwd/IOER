using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using Isle.BizServices;

namespace ILPathways.Content
{
    public partial class Default : BaseAppPage
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                InitializeForm();
            }
          
        }//
        protected void InitializeForm()
        {
            if ( IsUserAuthenticated() )
            {
                CurrentUser = GetAppUser();
                //check if authorized, if not found, will check by orgId
                this.FormPrivileges = SecurityManager.GetGroupObjectPrivileges( CurrentUser, txtAuthorSecurityName.Text );
                //if ( FormPrivileges.CanCreate() == false )
                //{
                //    //try org
                //    CurrentUser = GetAppUser();
                //    if ( CurrentUser.OrgId == 0 && CurrentUser.HasUserProfile() ==  false)
                //    {
                //        AccountServices mgr = new AccountServices();
                //        CurrentUser.UserProfile =  mgr.PatronProfile_Get( CurrentUser.Id );
                //    }
                //    this.FormPrivileges = GDAL.SecurityManager.GetGroupOrgObjectPrivileges( CurrentUser.UserProfile.OrganizationId, txtAuthorSecurityName.Text );
                //}
                if ( this.FormPrivileges.CanCreate() || WebUser.Id == 22 )
                {
                    authoringPanel.Visible = true;
                    noContentMesssage.Visible = false;
                }
                else
                {
                    //display message, and lock down

                    noContentMesssage.Visible = true;
                }
            }
            else
            {
                notAuthenticatedMesssage.Visible = true;
            }
        }
    }
}