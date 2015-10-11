using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EmailHelper = ILPathways.Utilities.EmailManager;

using ILPathways.Business;
using IOER.classes;
using IOER.Library;
using ILPathways.Utilities;

using LRWarehouse.DAL;


namespace IOER.Controllers 
{
    public class SearchController : BaseUserControl
    {

        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
        const string thisClassName = "SearchController";

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

        private void PopulateNarrowedCbxList( CheckBoxList cbxl, DataTable dt, string title )
        {

            if ( dt != null && dt.Rows.Count > 0 )
            {
                foreach ( DataRow dr in dt.DefaultView.Table.Rows )
                {
                    ListItem item = new ListItem();
                    item.Value = DatabaseManager.GetRowColumn( dr, "Id", "0" );
                    item.Text = dr[ title ].ToString().Trim();

                    cbxl.Items.Add( item );

                } //end foreach
            }
        }//

        #region Format filters

        public static void FormatAccessRightsFilter( CheckBoxList cbxl,
                string booleanOperator, ref string filter, ref string filterDesc )
        {
            //13-01-31 mp - change to use Id, from text
            string csv = "";
            string selDesc = "";
            string comma = "";
            foreach ( ListItem li in cbxl.Items )
            {
                if ( li.Selected )
                {
                    string item = ExtractParens( li.Text );
                    csv += li.Value + ",";

                    selDesc += comma + item;
                    comma = ", ";
                }
            }
            if ( csv.Length > 0 )
            {
                csv = csv.Substring( 0, csv.Length - 1 );

                string where = string.Format( " (lr.AccessRightsId in ({0})) ", csv );
                filter += BaseDataManager.FormatSearchItem( filter, where, booleanOperator );
                filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
            }
        }


        public static void FormatSubselectFilter( CheckBoxList cbxl, string tableName, string columnName, string booleanOperator, ref string filter, ref string filterDesc )
        {
            string csv = "";
            string selDesc = "";
            string comma = "";
            foreach ( ListItem li in cbxl.Items )
            {
                if ( li.Selected )
                {
                    if ( li.Value != "0" )
                    {
                        csv += li.Value + ",";

                        string item = ExtractParens( li.Text );
                        selDesc += comma + item;
                        comma = ", ";
                    }

                }
            }
            if ( csv.Length > 0 )
            {
                csv = csv.Substring( 0, csv.Length - 1 );


                string where = string.Format( " lr.id in (select ResourceIntId from {0} where {1} in ({2})) ", tableName, columnName, csv );
                filter += BaseDataManager.FormatSearchItem( filter, where, booleanOperator );
                filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
            }
        }

        public static string ExtractParens( string item )
        {

            if ( item.IndexOf( "(" ) > -1 )
            {
                item = item.Substring( 0, item.IndexOf( "(" ) ).Trim();
            }

            return item;
        }
        #endregion

        #region Housekeeping
        /// <summary>
        /// Populate form controls
        /// </summary>
        public void PopulateControls()
        {


        } //

        /// <summary>
        /// note - consider caching these lists (and not datasets)
        /// </summary>


        public void PopulateClustersCbxList( CheckBoxList cbxl )
        {
            ResourceCheckBoxFiller.FillCheckBoxListFromCareerClusters( cbxl, "FormattedTitle");
        }

        public void PopulateAccessRightsList( CheckBoxList cbxl )
        {
            ResourceCheckBoxFiller.FillCheckBoxList( cbxl, "Codes.AccessRights", "FormattedTitle", "SortOrder" );
        }

        public void PopulateAlignmentTypeList( CheckBoxList cbxl )
        {
            ResourceCheckBoxFiller.FillCheckBoxList( cbxl, "Codes.AlignmentType", "FormattedTitle", "FormattedTitle" );
        }

        public void PopulateAudienceTypeList( CheckBoxList cbxl )
        {
            ResourceCheckBoxFiller.FillCheckBoxList( cbxl, "Codes.AudienceType", "FormattedTitle", "FormattedTitle", true );
        }

        public void PopulateEducationalUseLists( CheckBoxList cbxl1, CheckBoxList cbxl2, CheckBoxList cbxl3 )
        {
            ResourceCheckBoxFiller.FillCheckBoxListFromEducationalUse( cbxl1, "Knowledge" );
            ResourceCheckBoxFiller.FillCheckBoxListFromEducationalUse( cbxl2, "Reasoning" );
            ResourceCheckBoxFiller.FillCheckBoxListFromEducationalUse( cbxl3, "Skills" );
        }
        public void PopulateEducationalUseList( CheckBoxList cbxl1 )
        {
            ResourceCheckBoxFiller.FillCheckBoxListFromEducationalUse( cbxl1 );

        }

        public void PopulateGradeLevelCbxList( CheckBoxList cbxl )
        {
            ResourceCheckBoxFiller.FillCheckBoxList( cbxl, "Codes.GradeLevel", "FormattedTitle", "SortOrder", true );
        }

        public void PopulateGroupTypeList( CheckBoxList cbxlGroupType )
        {
            ResourceCheckBoxFiller.FillCheckBoxList( cbxlGroupType, "Codes.GroupType", "Title", "Title" );
        }

        public void PopulateItemTypeList( CheckBoxList cbxl )
        {
            ResourceCheckBoxFiller.FillCheckBoxList( cbxl, "Codes.ItemType", "Title", "Title" );
        }
        public void PopulateInteractivityTypeList( CheckBoxList cbxlGroupType )
        {
            ResourceCheckBoxFiller.FillCheckBoxList( cbxlGroupType, "Codes.InteractivityType", "Title", "Title" );
        }


        public void PopulateLanguagesList( CheckBoxList cbxl )
        {
            string sql = "SELECT [Id],[Title] + ' (' + convert(varchar, isnull([WarehouseTotal],0)) + ')' As Title  FROM [dbo].[Codes.Language] where [IsPathwaysLanguage] = 1 order by id";

            ResourceCheckBoxFiller.FillCheckBoxList( sql, cbxl, "Title" );
        }

        public void PopulateLanguagesList( DropDownList ddlLanguages )
        {
            string sql = "SELECT [Id],[Title] + ' (' + convert(varchar, isnull([WarehouseTotal],0)) + ')' As Title  FROM [dbo].[Codes.Language] where [IsPathwaysLanguage] = 1 order by id";
            DataSet ds1 = DatabaseManager.DoQuery( sql );
            DatabaseManager.PopulateList( ddlLanguages, ds1, "Id", "Title", "Select language" );
            if ( DatabaseManager.DoesDataSetHaveRows( ds1 ) )
                ddlLanguages.SelectedIndex = 1;
        } //

        public void PopulateEducationLevelList( CheckBoxList cbxl )
        {
            ResourceCheckBoxFiller.FillCheckBoxList( cbxl, "Codes.PathwaysEducationLevel", "FormattedTitle", "SortOrder" );
        }

        public void PopulateResFormatsList( CheckBoxList cbxl )
        {
            ResourceCheckBoxFiller.FillCheckBoxList( cbxl, "Codes.ResourceFormat", "FormattedTitle", "FormattedTitle" );
        }


        public void PopulateResTypeList( CheckBoxList cbxl )
        {
            ResourceCheckBoxFiller.FillCheckBoxList( cbxl, "Codes.ResourceType", "FormattedTitle", "Title" );
        }


        #endregion
    }
}