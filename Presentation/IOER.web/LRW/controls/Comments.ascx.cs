using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Utilities;
using LRWarehouse.Business;
using LRWarehouse.DAL;

namespace IOER.LRW.controls
{
    public partial class Comments : IOER.Library.BaseUserControl
    {
        public int currentResourceIntID;
        public System.Guid currentUserGUID;
        public bool usable;

        protected void Page_Load( object sender, EventArgs e )
        {
            InitializeForm();
        }

        protected void InitializeForm()
        {
            if ( usable )
            {
                btnAddComment.Visible = true;
                lblAddCommentMessage.Visible = false;
                commentsUpdatePanel.Visible = true;
                btnAddComment.OnClientClick = "postComment('" + commentsBox.ClientID + "','#" + txtComments.ClientID + "',this)";
            }
            else
            {
                btnAddComment.Visible = false;
                lblAddCommentMessage.Visible = true;
                lblAddCommentMessage.Text = "<p class=\"authenticateMessage\">You must sign in to post comments.</p>";
                commentsUpdatePanel.Visible = false;
            }
        }

        protected void btnAddComment_Click( object sender, EventArgs e )
        {

        }


    }
}