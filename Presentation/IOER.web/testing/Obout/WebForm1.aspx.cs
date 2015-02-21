using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Obout.Ajax.UI.TreeView;
using ILPathways.Business;
using MyManager = ILPathways.DAL.ContentManager;

namespace ILPathways.testing.Obout
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        MyManager myManager = new MyManager();
        protected void Page_Load( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handle this event to format the node text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OBTreeview_TreeNodeDataBound( object sender, NodeEventArgs e )
        {
            System.Data.DataRowView dv = ( e.Node.DataItem as System.Data.DataRowView );
            if ( dv != null )
            {
                e.Node.Text = String.Format( "{0} - {1}", dv[ "Id" ].ToString(), dv[ "Title" ].ToString() );
                e.Node.Value = dv[ "Id" ].ToString();
            }
        }

        /// <summary>
        /// handle  click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OBTreeview_SelectedTreeNodeChanged( object sender, NodeEventArgs e )
        {
            nodeClick.Text = string.Format( "clicked Node: {0}, value: {1}<br/>", e.Node.Text, e.Node.Value );
            int id = 0;
            if ( Int32.TryParse( e.Node.Value, out id ) )
            {
                ContentItem entity = myManager.Get( id );
                nodeClick.Text += "<h1>" + entity.Title + "</h1>" + entity.Description;
            }
        }
        //OnNodeSelect
    }
}