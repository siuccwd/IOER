using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;

namespace IOER.LRW.controls
{
    public partial class Dashboard1 : BaseUserControl
    {
        public string userGUID { get; set; }

        protected void Page_Load( object sender, EventArgs e )
        {
            if ( demoMode.Text == "false" )
            {
                //Check login
                if ( IsUserAuthenticated() )
                {
                    dashboardContent.Visible = true;
                    notLoggedInContent.Visible = false;
                    userGUID = WebUser.RowId.ToString();
                }
                else
                {
                    dashboardContent.Visible = false;
                    notLoggedInContent.Visible = true;
                    userGUID = "";
                }
            }

        }

        /*
        #region button click methods
        public void submitProfileAvatar_Click( object sender, EventArgs e )
        {
            HttpPostedFile file = fileProfileAvatar.PostedFile;
            int size = file.ContentLength;
            //TODO: Do something
        }

        public void submitLibraryAvatar_Click( object sender, EventArgs e )
        {
            HttpPostedFile file = fileLibraryAvatar.PostedFile;
            int size = file.ContentLength;
            //TODO: Do something
        }
        #endregion
        */
    }
}