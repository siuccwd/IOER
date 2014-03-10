using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IPU = ILPathways.Utilities;
using ILPathways.classes;
using ILPathways.Services;

namespace ILPathways.Masters
{
    public partial class Pathway : System.Web.UI.MasterPage
    {
        protected string environmentClass;
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                InitializeForm();
            }
            //Regardless
            WebDALService webDAL = new WebDALService();
            if ( webDAL.IsLocalHost() )
            {
                environmentClass = " class=\"localhost\"";
            }
            else if ( webDAL.IsSandbox() )
            {
                environmentClass = " class=\"sandbox\"";
            }
            else
            {
                environmentClass = " class=\"production\"";
            }
        }

        protected void InitializeForm()
        {
            string hideHdr = IPU.FormHelper.GetRequestKeyValue( "hh", "" );
            if ( hideHdr == "" )
            {
                hideHdr = SessionManager.Get( HttpContext.Current.Session, "HideHeader", "" );
            }

            if ( hideHdr.Equals( "y" ) )
            {
                //this.header1.Visible = false;
                this.pageHeader.Visible = false;
                footer1.Visible = false;
                HttpContext.Current.Session[ "HideHeader" ] = "y";
            }
            else if ( hideHdr.Equals( "n" ) )
            {
                HttpContext.Current.Session.Remove( "HideHeader" );
            }
        }

    }
}