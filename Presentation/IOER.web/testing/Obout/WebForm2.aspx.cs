using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Obout.Ajax.UI.TreeView;
using ILPathways.Business;

using MyManager = Isle.BizServices.ContentServices;

namespace ILPathways.testing.Obout
{
    public partial class WebForm2 : System.Web.UI.Page
    {
        MyManager myManager = new MyManager();
        
        protected void Page_Load( object sender, EventArgs e )
        {
            InitializeTree();
        }
        private void InitializeTree()
        {
            //this.OBTreeview.ExpandAll();
            //Populate( 2176 );
            Populate( 2207 );

        }

        private void Populate( int contentId )
        {
            this.OBTreeview.Nodes.Clear();
            //get top level
            ContentItem entity = myManager.GetCurriculum( contentId );
            bool showAll = true;

            foreach ( ContentItem mod in entity.ChildItems )
            {
                Node node = new Node();
                
                node.Text = mod.Title;
                node.Value = mod.Id.ToString();
                this.OBTreeview.Nodes.Add( node );

                if ( showAll )
                {
                    this.PopulateChildren( node, mod );
                }
            }
        }


        private void PopulateChildren( Node parent, ContentItem entity )
        {
             foreach ( ContentItem child in entity.ChildItems )
            {
                Node node = new Node();

                node.Text = child.Title;
                node.Value = child.Id.ToString();
                 
                parent.ChildNodes.Add( node );
                if (child.ChildItems != null && child.ChildItems.Count > 0)
                   this.PopulateChildren( node, child );
 
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