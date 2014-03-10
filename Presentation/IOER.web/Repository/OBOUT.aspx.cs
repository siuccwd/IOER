using System;
using System.IO;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Obout.Ajax.UI.HTMLEditor;
using System.Collections.ObjectModel;
using System.Globalization;

namespace ILPathways.Content
{
    public partial class OBOUT : System.Web.UI.Page
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
               // StreamReader input;

                //input = new StreamReader( System.Web.HttpContext.Current.Server.MapPath( "contents/Content1.txt" ), System.Text.Encoding.ASCII );
                //editor.EditPanel.Content = input.ReadToEnd();
                //input.Close();
            }
            else
            {
                //ScriptManager.RegisterClientScriptBlock( this, this.GetType(), "EditorResponse", "alert('Submitted:\\n\\n" + editor.EditPanel.Content.Replace( "\"", "\\\"" ).Replace( "\n", "\\n" ).Replace( "\r", "" ).Replace( "'", "\\'" ) + "');", true );
            }
        }
    }
}