using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using LRWarehouse.DAL;
using System.Data;

namespace ILPathways.LRW.controls
{
    public partial class ListPanel : System.Web.UI.UserControl
    {
        public string TitleText { get; set; }
        public string CssClass { get; set; }
        public string UpdateMode { get; set; } //raw, read, append
        public bool AllowUncheck { get; set; }
        public int ResourceIntID { get; set; }
        public string ListMode { get; set; } //checkbox, radio, dropdown
        public bool UseBlankDefault { get; set; }
        public string TargetTable { get; set; } //assessmentType, careerCluster, educationalUse, mediaType, gradeLevel, groupType, endUser, itemType, language, resourceType
        public string ES_ID { get; set; }
        public bool ShowTotals { get; set; }
        string selectText = "Select...";
        bool useSelectedCodes;
        ResourceDataManager manager;
        ResourceDataManager.IResourceDataSubclass operatingTable;
        public ListControl list {
            get
            {
                switch ( ListMode )
                {
                    case "radio":
                        return this.rbList;
                    case "dropdown":
                        return this.ddList;
                    default:
                        return this.cbList;
                }
            }
            set { } //Should not be set this way
        }

        protected void Page_Load( object sender, EventArgs e )
        {
            InitSettings();
            //GenerateList();
            if ( IsPostBack & UpdateMode == "append" ) 
            {
                //Need to readd custom properties to existing list
                bool[] incomingSelections = new bool[ list.Items.Count ];
                for ( int i = 0 ; i < incomingSelections.Length ; i++ )
                {
                    incomingSelections[ i ] = list.Items[ i ].Selected;
                }
                GenerateList();
                bool hasSelectedBlank = false;
                bool hasOtherSelected = false;
                for ( int i = 0 ; i < list.Items.Count ; i++ )
                {
                    if ( list.Items[ i ].Text == selectText & list.Items[ i ].Selected )
                    {
                        hasSelectedBlank = true;
                    }
                    else if ( incomingSelections[ i ] )
                    {
                        hasOtherSelected = true;
                    }
                }
                if ( hasSelectedBlank & hasOtherSelected )
                {
                    list.Items.FindByText( selectText ).Enabled = (AllowUncheck ? true : false);
                    list.Items.FindByText( selectText ).Selected = false;
                }
                for ( int i = 0 ; i < incomingSelections.Length ; i++ )
                {
                    list.Items[ i ].Selected = incomingSelections[ i ];
                    if ( list.Items[ i ].Selected )
                    {
                        list.Items[ i ].Attributes.Add( "selected", "true" );
                    }
                    else
                    {
                        list.Items[ i ].Attributes.Remove( "selected" );
                    }
                    if ( ListMode != "dropdown" )
                    {
                        list.Items[ i ].Enabled = ( AllowUncheck ? true : !incomingSelections[ i ] );
                    }
                }
            }
            else
            {
                GenerateList();
            }
        }

        protected void InitSettings()
        {
            if ( ListMode == null )
            {
                ListMode = "checkbox";
            }
            else
            {
                ListMode = ListMode.ToLower();
            }
            switch ( ListMode )
            {
                case "radio":
                    cbList.Visible = false;
                    rbList.Visible = true;
                    ddList.Visible = false;
                    list = rbList;
                    break;
                case "dropdown":
                    cbList.Visible = false;
                    rbList.Visible = false;
                    ddList.Visible = true;
                    list = ddList;
                    break;
                default:
                    cbList.Visible = true;
                    rbList.Visible = false;
                    ddList.Visible = false;
                    list = cbList;
                    break;
            }
            manager = new ResourceDataManager();
            operatingTable = ResourceDataManager.ResourceDataSubclassFinder.getSubclassByName( TargetTable );
            useSelectedCodes = ( ( UpdateMode == "read" | UpdateMode == "append" ) & ResourceIntID != 0 );
            if ( UpdateMode == "" ) { UpdateMode = "raw"; }
            header.Visible = (TitleText != null && TitleText != "");
            title.Text = TitleText;
        }

        protected void GenerateList()
        {
            list.Items.Clear();
            ResourceDataManager manager = new ResourceDataManager();
            DataSet ds;
            if ( useSelectedCodes )
            {
                ds = manager.SelectedCodes( operatingTable, ResourceIntID );
            }
            else
            {
                ds = manager.GetCodetable( operatingTable );
            }
            
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                bool foundSelected = false;
                if ( ListMode == "radio" & UseBlankDefault ) //Inserts an extra item in the radio list
                {
                    AddListItem( "0", "Not Applicable", "0", "Does not apply", false );
                }
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    bool isSelected = false;
                    if ( useSelectedCodes )
                    {
                        isSelected = bool.Parse( GetField( dr, "IsSelected" ) );
                        if ( isSelected )
                        {
                            foundSelected = true;
                        }
                    }
                    AddListItem( GetField( dr, "Id" ), GetField( dr, "Title" ), GetField( dr, "ItemCount" ), GetField( dr, "Description" ), isSelected );
                }
                if (ListMode == "dropdown" & UseBlankDefault & ((UpdateMode != "append" & UpdateMode != "read") || (!foundSelected & UpdateMode == "append" & useSelectedCodes) ) )
                {
                    list.Items.Insert( 0, new ListItem( selectText, "0", true ) );
                    list.Items.FindByText( selectText ).Selected = true;
                }


                list.Attributes.Add( "tableName", TargetTable );
                list.Attributes.Add( "class", CssClass );
                if ( ListMode == "dropdown" & !useSelectedCodes & UseBlankDefault ) //Sets the drop-down box to a blank item
                {
                    list.SelectedIndex = -1;
                }
                if ( ListMode == "dropdown" & useSelectedCodes & UpdateMode != "append" )
                {
                    if ( list.Items.Count == 0 )
                    {
                        ltlList.Text = "";
                    }
                    else
                    {
                        ltlList.Text = list.Items[ 0 ].Text;
                    }
                    ltlList.Visible = true;
                    list.Visible = false;
                }
            }
        }

        protected void AddListItem( string value, string text, string count, string description, bool selected )
        {
            ListItem item = new ListItem();
            item.Value = value;
            item.Text = text;
            item.Attributes.Add( "itemID", value );
            item.Attributes.Add( "itemName", text );
            item.Attributes.Add( "itemDescription", description );
            item.Attributes.Add( "itemCount", count );
            if ( useSelectedCodes )
            {
                if ( selected )
                {
                    item.Selected = true;
                    item.Attributes.Add( "selected", "true" );
                    if ( ListMode != "dropdown" )
                    {
                        item.Enabled = (AllowUncheck ? true : false );
                    }
                }
            }
            if ( ES_ID != "" )
            {
                list.Attributes.Add( "ES_ID", ES_ID );
            }
            if ( ShowTotals )
            {
                try
                {
                    bool amountNeeded = true;
                    double check = 0;
                    int total = int.Parse( count );
                    while ( amountNeeded )
                    {
                        if ( total <= check )
                        {
                            amountNeeded = false;
                            if ( total <= 10 )
                            {
                                item.Text = item.Text + string.Format( template_itemCountCustom.Text, "Under 10" );
                            }
                            else if ( total <= 50 )
                            {
                                item.Text = item.Text + string.Format( template_itemCountCustom.Text, "Under 50" );
                            }
                            else if ( total <= 100 )
                            {
                                item.Text = item.Text + string.Format( template_itemCountCustom.Text, "Under 100" );
                            }
                            else
                            {
                                double decimalized = Math.Floor( ( double ) ( total / ( check * 0.01 ) ) ) * ( check * 0.01 );
                                string formatted = string.Format( "{0:n0}", decimalized );
                                item.Text = item.Text + string.Format( template_itemCount.Text, formatted );
                            }
                        }
                        else
                        {
                            if ( check < 1 ) { check = 1; }
                            check = check * 10;
                        }
                    }
                }
                catch { } //Fault-tolerance
            }
            if ( UpdateMode == "read") //If we're in read-only mode, only add list items for selected stuff.
            {
                if( item.Selected )
                {
                    list.Items.Add( item );
                }
            }
            else
            {
                list.Items.Add( item );
            }
        }

        public List<ListItem> GetCheckedItems()
        {
            List<ListItem> responseList = new List<ListItem>();
            foreach ( ListItem item in list.Items )
            {
                if ( item.Selected )
                {
                    responseList.Add( item );
                }
            }
            return responseList;
        }

        protected string GetField( DataRow dr, string field )
        {
            return DatabaseManager.GetRowPossibleColumn( dr, field );
        }
    }
}