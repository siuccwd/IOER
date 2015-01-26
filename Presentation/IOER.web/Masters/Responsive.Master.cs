using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IPU = ILPathways.Utilities;
using ILPathways.classes;

namespace ILPathways.Masters
{
    public partial class Responsive : System.Web.UI.MasterPage
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            Page.Header.DataBind(); 

            if ( !Page.IsPostBack )
            {
                InitializeForm();
            }
        }
        protected void InitializeForm()
        {

            
            string hideHdr = IPU.FormHelper.GetRequestKeyValue( "hh", "" );
            if ( hideHdr == "" )
            {
                hideHdr = SessionManager.Get( HttpContext.Current.Session, "HideHeader", "" );
            }

            if ( hideHdr.Equals( "temp" ) )
            {
                //hide, but only for current
                this.header.Visible = false;
                pagefooter.Visible = false;

            } else if ( hideHdr.Equals( "y" ) )
            {
                //this.header1.Visible = false;
                this.header.Visible = false;
                pagefooter.Visible = false;
                HttpContext.Current.Session[ "HideHeader" ] = "y";

            } else  if ( hideHdr.Equals( "n" ) )
            {
                HttpContext.Current.Session.Remove( "HideHeader" );
            }
        }

       
    }
}