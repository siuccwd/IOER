using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;
using System.Web.Script.Serialization;
using LRWarehouse.DAL;
using System.Data;

namespace IOER.LRW.controls
{
    public partial class ConditionsOfUseSelector : BaseUserControl
    {
        public string conditionsData;
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        public string selectedValue;
        public int selectedIndex
        {
            get { return ddlConditionsOfUse.SelectedIndex; }
            set { ddlConditionsOfUse.SelectedIndex = value; }
        }
        public string conditionsURL
        {
            get { return txtConditionsOfUse.Text; }
            set { txtConditionsOfUse.Text = value; }
        }
		public string IsNewContext
        {
			get { return litIsNewContext.Text; }
			set { litIsNewContext.Text = value; }
        }
		
        protected void Page_Load( object sender, EventArgs e )
        {
            PopulateItems();
            if ( !IsPostBack )
            {
                ddlConditionsOfUse.SelectedValue = selectedValue;
            }
        }

        protected void PopulateItems()
        {
            DataSet ds = CodeTableManager.ConditionsOfUse_Select();
            ConditionsOfUseJSON[] uses = new ConditionsOfUseJSON[ 0 ];
            if ( DoesDataSetHaveRows( ds ) )
            {
                uses = new ConditionsOfUseJSON[ ds.Tables[0].Rows.Count ];
                int counter = 0;
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    //Inject data into the drop down if needed, ASP-style
                    if ( !IsPostBack )
                    {
                        ListItem item = new ListItem( DatabaseManager.GetRowColumn( dr, "Title" ), DatabaseManager.GetRowColumn( dr, "Id" ) );
                        ddlConditionsOfUse.Items.Add( item );
                    }

                    //Inject the other data into the JSON regardless
                    //Wouldn't need to do this if asp:literal could be used inside script blocks
                    ConditionsOfUseJSON use = new ConditionsOfUseJSON();
                    use.value = DatabaseManager.GetRowColumn( dr, "Id" );
                    use.text = DatabaseManager.GetRowColumn( dr, "Title" );
                    use.descriptor = DatabaseManager.GetRowColumn( dr, "Description" );
                    use.url = DatabaseManager.GetRowPossibleColumn( dr, "Url" );
                    use.thumbnail = DatabaseManager.GetRowColumn( dr, "IconUrl" );
                    if ( use.url == "" )
                    {
                        use.hide = false;
                    }
                    else
                    {
                        use.hide = true;
                    }
                    uses[ counter ] = use;
                    counter++;
                }
            }
            conditionsData = "data: " + serializer.Serialize( uses ) + ",";
        }

        public void SetSelectedValue( string value )
        {
            ddlConditionsOfUse.SelectedValue = value;
        }
        public string GetSelectedValue()
        {
            return ddlConditionsOfUse.SelectedValue;
        }

        [Serializable]
        public class ConditionsOfUseJSON
        {
            public string text;
            public string value;
            public string url;
            public string thumbnail;
            public string descriptor;
            public bool hide;
        }
    }
}