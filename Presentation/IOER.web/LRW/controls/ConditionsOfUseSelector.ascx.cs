using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;
using Isle.BizServices;
using LRWarehouse.Business.ResourceV2;
using LRWarehouse.DAL;

namespace IOER.LRW.controls
{
    public partial class ConditionsOfUseSelector : BaseUserControl
    {
        public string conditionsData;
		public List<UsageRights> UsageRights { get; set; }

        JavaScriptSerializer serializer = new JavaScriptSerializer();
		ResourceV2Services resService = new ResourceV2Services();

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
			int counter = 0;
			ConditionsOfUseJSON[] uses = new ConditionsOfUseJSON[ 0 ];

			//15-11-04 mp - convert to be consistent with tagger
			//	never mind, this control should be considered obsolete
			//UsageRights = resService.GetUsageRightsList();
			//if ( UsageRights.Count > 0 )
			//{
			//	uses = new ConditionsOfUseJSON[ UsageRights.Count];
			//	foreach ( UsageRights dr in UsageRights )
			//	{
			//		//Inject the other data into the JSON regardless
			//		//Wouldn't need to do this if asp:literal could be used inside script blocks
			//		ConditionsOfUseJSON use = new ConditionsOfUseJSON();
			//		use.value = dr.CodeId.ToString();
			//		use.text = dr.Title;
			//		use.descriptor = dr.Description;
			//		use.url = dr.Url != null ? dr.Url : "";
			//		use.thumbnail = dr.IconUrl;
			//		if ( use.url == "" )
			//		{
			//			use.hide = false;
			//		}
			//		else
			//		{
			//			use.hide = true;
			//		}
			//		uses[ counter ] = use;
			//		counter++;
			//	}
			//}
            DataSet ds = CodeTableManager.ConditionsOfUse_Select();
            
            if ( DoesDataSetHaveRows( ds ) )
            {
                uses = new ConditionsOfUseJSON[ ds.Tables[0].Rows.Count ];
                
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

		protected void PopulateItemsOLD()
		{
			DataSet ds = CodeTableManager.ConditionsOfUse_Select();
			ConditionsOfUseJSON[] uses = new ConditionsOfUseJSON[ 0 ];
			if ( DoesDataSetHaveRows( ds ) )
			{
				uses = new ConditionsOfUseJSON[ ds.Tables[ 0 ].Rows.Count ];
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