using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


using ILPathways.Utilities;

namespace ILPathways.Pages
{
    public partial class gooruPlayer : System.Web.UI.Page
    {
        public string gooruID { get; set; }
        protected void Page_Load( object sender, EventArgs e )
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

            string type = FormHelper.GetRequestKeyValue( "t", "r" );
            string id = FormHelper.GetRequestKeyValue( "id", "" );
            
            if ( id != null || id.Length == 36 )
            {
                if ( type == "r" )
                {
                    string url = string.Format( playerUrl.Text, id, type );
                    playerFrame.Attributes.Add( "src", url );
                }
                else
                {
                    viewTitle.Text = "gooru Collection";
                    string url = string.Format( collectionPlayerUrl.Text, id );
                    playerFrame.Attributes.Add( "src", url );
                }

                gooruID = id; 
            }
            //else if ( cid != null || cid.Length == 36 )
            //{
            //    string url = string.Format( collectionPlayerUrl.Text, cid );
            //    playerFrame.Attributes.Add( "src", url );

            //}
            else 
            {
                playerFrame.Visible = false;
                messagePanel.Visible = true;
            }
        }
    }
}