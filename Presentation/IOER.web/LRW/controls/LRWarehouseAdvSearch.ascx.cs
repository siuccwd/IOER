using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using ILPathways.Utilities;
using ILPathways.classes;

using LRWarehouse.Business;
using MyManager = LRWarehouse.DAL.LRManager;

using LRWarehouse.DAL;

namespace ILPathways.LRW.controls
{
    public partial class LRWarehouseAdvSearch : BaseUserControl
    {
        MyManager mgr = new MyManager();
        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
        const string thisClassName = "LRWarehouseAdvSearch";

        #region Properties

        protected int SearchType
        {
            get
            {
                if ( ViewState[ "SearchType" ] == null )
                    ViewState[ "SearchType" ] = 1;

                if ( IsInteger( ViewState[ "SearchType" ].ToString() ) )
                    return Int32.Parse( ViewState[ "SearchType" ].ToString() );
                else
                    return 1;
            }
            set { ViewState[ "SearchType" ] = value; }
        }//

        /// <summary>
        /// Store retrieve the last page number - used after updates to attempt to show the same page
        /// </summary>
        protected int LastPageNumber
        {
            get
            {
                if ( ViewState[ "LastPageNumber" ] == null )
                    ViewState[ "LastPageNumber" ] = 0;

                if ( IsInteger( ViewState[ "LastPageNumber" ].ToString() ) )
                    return Int32.Parse( ViewState[ "LastPageNumber" ].ToString() );
                else
                    return 0;
            }
            set { ViewState[ "LastPageNumber" ] = value; }
        }//
        /// <summary>
        /// Store last retrieved total rows. Need to use to properly reset pager item count after none search postbacks
        /// </summary>
        protected int LastTotalRows
        {
            get
            {
                if ( ViewState[ "LastTotalRows" ] == null )
                    ViewState[ "LastTotalRows" ] = 0;

                if ( IsInteger( ViewState[ "LastTotalRows" ].ToString() ) )
                    return Int32.Parse( ViewState[ "LastTotalRows" ].ToString() );
                else
                    return 0;
            }
            set { ViewState[ "LastTotalRows" ] = value; }
        }//

        /// <summary>
        /// Store last retrieved total rows. Need to use to properly reset pager item count after none search postbacks
        /// </summary>
        protected bool UsingFullTextOption
        {
            get
            {
                return _usingFullTextOption;
            }
            set { this._usingFullTextOption = value; }
        }//
        private bool _usingFullTextOption = true;

        protected string CustomFilter
        {
            get
            {
                if ( ViewState[ "CustomFilter" ] == null )
                    ViewState[ "CustomFilter" ] = "";

                return ViewState[ "CustomFilter" ].ToString();
            }
            set { ViewState[ "CustomFilter" ] = value; }
        }//

        protected string CustomTitle
        {
            get
            {
                if ( ViewState[ "CustomTitle" ] == null )
                    ViewState[ "CustomTitle" ] = "";

                return ViewState[ "CustomTitle" ].ToString();
            }
            set { ViewState[ "CustomTitle" ] = value; }
        }//


        #endregion
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( Page.IsPostBack )
            {
                // HandleClusterChange();
            }
            else
            {
                //MP-now down by parent page!
                //this.InitializeForm();
            }
        }
        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        public void InitializeForm()
        {

            CustomTitle = "";


            if ( IsTestEnv() )
            {
                showTotalsPanel1.Visible = true;
                ftPanel.Visible = true;
            }

            // Set source for form lists
            this.PopulateControls();

        }	// End 

        #region Housekeeping
        /// <summary>
        /// Populate form controls
        /// </summary>
        private void PopulateControls()
        {

            PopulateClustersCbxList();
            //
            SetLanguagesList();
            PopulateResTypeCbxList();
            PopulateEducationLevelCbxList();
        } //

        private void PopulateClustersCbxList()
        {
            cbxlCluster.Visible = true;
            // Load checkboxes
            cbxlCluster.Items.Clear();
            DataSet ds = mgr.ILPathwaysClusterSelect();
            if ( MyManager.DoesDataSetHaveRows( ds ) == true )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
                {
                    ListItem item = new ListItem();
                    item.Value = DatabaseManager.GetRowColumn( dr, "Id", "0" );
                    item.Text = dr[ careerClusterTitleDisplay.Text ].ToString().Trim();

                    cbxlCluster.Items.Add( item );

                } //end foreach
            }
        }

        private void PopulateEducationLevelCbxList()
        {
            // Load checkboxes
            this.cbxGradeLevel1.Items.Clear();

            DataSet ds = mgr.EducationLevel_Select();
            if ( MyManager.DoesDataSetHaveRows( ds ) == true )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
                {
                    ListItem item = new ListItem();
                    item.Value = DatabaseManager.GetRowColumn( dr, "Id", "0" );
                    item.Text = dr[ "FormattedTitle" ].ToString().Trim();

                    cbxGradeLevel1.Items.Add( item );

                } //end foreach
            }
        }

        private void PopulateResTypeCbxList()
        {
            // Load checkboxes
            cbxlResType2.Items.Clear();

            DataSet ds = mgr.ResourceType_Select();
            if ( MyManager.DoesDataSetHaveRows( ds ) == true )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
                {
                    ListItem item = new ListItem();
                    item.Value = DatabaseManager.GetRowColumn( dr, "Id", "0" );
                    item.Text = dr[ "Title" ].ToString().Trim();

                    cbxlResType2.Items.Add( item );

                } //end foreach
            }
        }
        private void PopulateResTypeCbxList( string filter )
        {
            cbxlResType2.Visible = true;
            // Load checkboxes
            cbxlResType2.Items.Clear();

            DataSet ds = mgr.ResourceType_SearchCounts( filter );
            if ( MyManager.DoesDataSetHaveRows( ds ) == true )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
                {
                    ListItem item = new ListItem();
                    item.Value = DatabaseManager.GetRowColumn( dr, "Id", "0" );
                    item.Text = dr[ "FormattedTitle" ].ToString().Trim();

                    cbxlResType2.Items.Add( item );

                } //end foreach
            }
        }

        private void PopulateResTypeCbxList( DataTable dt )
        {
            cbxlResType2.Visible = true;
            // Load checkboxes
            cbxlResType2.Items.Clear();

            if ( dt != null && dt.Rows.Count > 0 )
            {
                foreach ( DataRow dr in dt.DefaultView.Table.Rows )
                {
                    ListItem item = new ListItem();
                    item.Value = DatabaseManager.GetRowColumn( dr, "Id", "0" );
                    item.Text = dr[ "FormattedTitle" ].ToString().Trim();

                    cbxlResType2.Items.Add( item );

                } //end foreach
            }
        }//

        private void SetLanguagesList()
        {
            DataSet ds1 = DatabaseManager.DoQuery( this.selectLanguages.Text );
            DatabaseManager.PopulateList( this.ddlLanguages, ds1, "Id", "Title", "Any Language" );
            if ( DatabaseManager.DoesDataSetHaveRows( ds1 ) )
                ddlLanguages.SelectedIndex = 1;

        } //
        #endregion
    }
}