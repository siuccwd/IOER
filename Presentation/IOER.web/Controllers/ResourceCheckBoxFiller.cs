using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using LRWarehouse.DAL;

namespace ILPathways.Controllers
{
    public static class ResourceCheckBoxFiller
    {
        

        #region Career Clusters
        public static CheckBoxList FillCheckBoxListFromCareerClusters( CheckBoxList cbxl )
        {
            return FillCheckBoxListFromCareerClusters( cbxl, false );
        }
        public static CheckBoxList FillCheckBoxListFromCareerClusters( CheckBoxList cbxl, bool includeDescriptions )
        {
            return FillCheckBoxListFromCareerClusters( cbxl, "IlPathwayName", includeDescriptions );
        }

        public static CheckBoxList FillCheckBoxListFromCareerClusters( CheckBoxList cbxl, string titleField )
        {
            return FillCheckBoxListFromCareerClusters( cbxl, titleField, false );
        }
        public static CheckBoxList FillCheckBoxListFromCareerClusters( CheckBoxList cbxl, string titleField, bool includeDescriptions )
        {
            string addDescriptions = "";
            if ( includeDescriptions )
            {
                addDescriptions = ", [Description]";
            }
            DataSet ds = DatabaseManager.DoQuery( "SELECT [Id], [IlPathwayName], [IlPathwayName] AS Title, isnull(convert(varchar,WareHouseTotal),0) As ItemCounts, [IlPathwayName] + ' (' + isnull(convert(varchar,WareHouseTotal),0) + ')' As FormattedTitle" + addDescriptions + " FROM [CareerCluster] WHERE [IsIlPathway] = 1 ORDER BY [IlPathwayName]" );
            ConstructListItems( titleField, ref cbxl, ds, includeDescriptions, "CareerCluster" );
            return cbxl;
        }
        #endregion

        #region Education Levels
        private static CheckBoxList FillCheckBoxListFromEducationLevels( CheckBoxList cbxl )
        {
            return FillCheckBoxListFromEducationLevels( cbxl, false );
        }
        private static CheckBoxList FillCheckBoxListFromEducationLevels( CheckBoxList cbxl, bool includeDescriptions )
        {
            string addDescriptions = "";
            if ( includeDescriptions )
            {
                addDescriptions = ", [Description]";
            }
            DataSet ds = DatabaseManager.DoQuery( "SELECT MAX([Id]) AS Id, [GradeRange], MAX([SortOrder]) As SortOrder, [IsActive]" + addDescriptions + " FROM [Codes.GradeLevel] WHERE [IsActive] = 1 GROUP BY [GradeRange], [IsActive]" + addDescriptions + " ORDER BY [SortOrder]" );
            ConstructListItems( "GradeRange", ref cbxl, ds, includeDescriptions, "GradeLevel" );
            return cbxl;
        }

        public static CheckBoxList FillCheckBoxList( CheckBoxList cbxl, string resourceTable, bool isCareerCluster, bool isGradeLevel )
        {
            return FillCheckBoxList( cbxl, resourceTable, isCareerCluster, isGradeLevel, false );
        }
        #endregion

        #region Language

        public static CheckBoxList FillCheckBoxListFromLanguage( CheckBoxList cbxl )
        {
            DataSet ds = DatabaseManager.DoQuery("SELECT [Id], [Title] FROM [Codes.Language] WHERE [IsPathwaysLanguage] = 1");
            ConstructListItems( "Title", ref cbxl, ds, false, "Language" );
            return cbxl;
        }

        #endregion

        #region Generic Methods
        /// <summary>
        /// Returns a checkbox list created from values pulled from the supplied table. Uses alternate methods for Career Clusters and Education level because those tables are setup differently.
        /// If the desired tables is one of those two, the resourceTable string may be left empty.
        /// The other useful tables are: Codes.AudienceType, Codes.ResourceType, Codes.ResourceFormat.
        /// </summary>
        /// <param name="cbxl"></param>
        /// <param name="resourceTable"></param>
        /// <param name="isCareerCluster"></param>
        /// <returns></returns>
        public static CheckBoxList FillCheckBoxList( CheckBoxList cbxl, string resourceTable, bool isCareerCluster, bool isGradeLevel, bool includeDescriptions )
        {
            string addDescriptions = "";
            if ( includeDescriptions )
            {
                addDescriptions = ", [Description]";
            }
            if ( isCareerCluster )
            {
                return FillCheckBoxListFromCareerClusters( cbxl, includeDescriptions );
            }
            else if ( isGradeLevel )
            {
                return FillCheckBoxListFromEducationLevels( cbxl, includeDescriptions );
            }
            else
            {
                DataSet ds;
                if ( resourceTable == "Codes.ResourceType" )
                {
                    ds = DatabaseManager.DoQuery( "SELECT [Id], [Title]" + addDescriptions + " FROM [" + resourceTable + "] WHERE [IsActive] = 1 ORDER BY [SortOrder], [Title]" );
                }
                else
                {
                    ds = DatabaseManager.DoQuery( "SELECT [Id], [Title]" + addDescriptions + " FROM [" + resourceTable + "] WHERE [IsActive] = 1 ORDER BY [Title]" );
                }
                ConstructListItems( "Title", ref cbxl, ds, includeDescriptions, resourceTable.Replace(".","-") );
                return cbxl;
            }
        }

        public static CheckBoxList FillCheckBoxList( CheckBoxList cbxl, string resourceTable, string titleField )
        {
            return FillCheckBoxList( cbxl, resourceTable, titleField, false );
        }
        /// <summary>
        /// Fill textbox list from a standard code table
        /// </summary>
        /// <param name="cbxl"></param>
        /// <param name="resourceTable"></param>
        /// <param name="titleField">Either Title or FormattedTitle</param>
        /// <returns></returns>
        public static CheckBoxList FillCheckBoxList( CheckBoxList cbxl, string resourceTable, string titleField, bool includeDescriptions )
        {
            return FillCheckBoxList( cbxl, resourceTable, titleField, "Title", includeDescriptions );
        }
        public static CheckBoxList FillCheckBoxList( CheckBoxList cbxl, string resourceTable, string titleField, string sortField )
        {
            return FillCheckBoxList( cbxl, resourceTable, titleField, sortField, false );
        }
        public static CheckBoxList FillCheckBoxList( CheckBoxList cbxl, string resourceTable, string titleField, string sortField, string filter )
        {
            return FillCheckBoxList( cbxl, resourceTable, titleField, sortField, false, filter );
        }
        public static CheckBoxList FillCheckBoxList( CheckBoxList cbxl, string resourceTable, string titleField, string sortField, bool includeDescriptions )
        {
            string addDescriptions = "";
            if ( includeDescriptions )
            {
                addDescriptions = ", [Description]";
            }

            string sql = string.Format( "SELECT [Id], [Title], isnull(convert(varchar,WareHouseTotal),0) As ItemCounts" + addDescriptions + " FROM [{0}] Where [IsActive] = 1 ORDER BY {1}", resourceTable, sortField );
            DataSet ds = DatabaseManager.DoQuery( sql );
            ConstructListItems( titleField, ref cbxl, ds, includeDescriptions, resourceTable.Replace( ".", "-" ) );
            return cbxl;
        }
        public static CheckBoxList FillCheckBoxList( CheckBoxList cbxl, string resourceTable, string titleField, string sortField, bool includeDescriptions, string filter )
        {
            string addDescriptions = "";
            if ( includeDescriptions )
            {
                addDescriptions = ", [Description]";
            }
            if ( filter.Trim().Length > 0 )
                filter = " AND " + filter;

            string sql = string.Format( "SELECT [Id], [Title], isnull(convert(varchar,WareHouseTotal),0) As ItemCounts" + addDescriptions + " FROM [{0}] Where [IsActive] = 1 {2} ORDER BY {1}", resourceTable, sortField, filter );

            DataSet ds = DatabaseManager.DoQuery( sql );
            ConstructListItems( titleField, ref cbxl, ds, includeDescriptions, resourceTable.Replace( ".", "-" ) );
            cbxl.Enabled = true;
            return cbxl;
        }
        public static CheckBoxList FillCheckBoxList( string sql, CheckBoxList cbxl, string titleField, bool includeDescriptions )
        {
            DataSet ds = DatabaseManager.DoQuery( sql );
            ConstructListItems( titleField, ref cbxl, ds, includeDescriptions, "custom" );
            return cbxl;

        }
        public static CheckBoxList FillCheckBoxList( string sql, CheckBoxList cbxl, string titleField )
        {
            return FillCheckBoxList( sql, cbxl, titleField, false );
        }
        #endregion

        #region Educational Use
        public static void FillCheckBoxListFromEducationalUse( CheckBoxList cbxl )
        {
            bool includeDescriptions = false;
            string addDescriptions = "";
            if ( includeDescriptions )
            {
                addDescriptions = ", codes.[Description]";
            }
            DataSet ds = DatabaseManager.DoQuery( "SELECT codes.[Id], codes.[Title] " + addDescriptions + "  FROM [Codes.EducationalUse] codes WHERE [IsActive] = 1 ORDER BY codes.[Title]" );
            ConstructListItems( "Title", ref cbxl, ds, includeDescriptions, "EducationalUse" );
            
        }
        public static CheckBoxList FillCheckBoxListFromEducationalUse( CheckBoxList cbxl, string targetCategory )
        {
            return FillCheckBoxListFromEducationalUse( cbxl, targetCategory, false );
        }
        /// <summary>
        /// Fill checkbox list from Educational Use, based on the chosen category.
        /// </summary>
        /// <param name="cbxl"></param>
        /// <param name="targetCategory"></param>
        /// <returns></returns>
        public static CheckBoxList FillCheckBoxListFromEducationalUse( CheckBoxList cbxl, string targetCategory, bool includeDescriptions )
        {
            string addDescriptions = "";
            if ( includeDescriptions )
            {
                addDescriptions = ", codes.[Description]";
            }

            DataSet ds = DatabaseManager.DoQuery( "SELECT codes.[Id], codes.[Title], categories.[Category]" + addDescriptions + " FROM [Codes.EducationalUse] codes LEFT JOIN [Codes.EducationalUseCategory] categories ON codes.[EdUseCategoryId] = categories.[Id] WHERE categories.[Category] = '" + targetCategory + "' AND [IsActive] = 1 ORDER BY codes.[Title]" );
            ConstructListItems( "Title", ref cbxl, ds, includeDescriptions, "EducationalUse" );
            return cbxl;
        }
        #endregion

        #region Group Methods
        /// <summary>
        /// Shortcut method to get all of the commonly-used Pathways checkboxes.
        /// Returns an array of CheckBoxLists with the following indexes:
        /// [0] = Clusters, [1] = Grade Levels, [2] = Intended Audience, [3] = Resource Types, [4] = Resource Formats, [5] = Educational Use (Knowledge), [6] = Educational Use (Reasoning), [7] = Educational Use (Skills)
        /// </summary>
        /// <returns></returns>
        public static CheckBoxList[] GetAllCommonCheckBoxes()
        {
            return GetAllCommonCheckBoxes( false );
        }

        public static CheckBoxList[] GetAllCommonCheckBoxes( bool includeDescriptions )
        {
            CheckBoxList[] cbxl = new CheckBoxList[ 10 ];

            cbxl[ 0 ] = FillCheckBoxListFromCareerClusters( new CheckBoxList(), includeDescriptions );
            cbxl[ 1 ] = FillCheckBoxListFromEducationLevels( new CheckBoxList(), includeDescriptions );
            cbxl[ 2 ] = FillCheckBoxList( new CheckBoxList(), "Codes.AudienceType", false, false, includeDescriptions );
            cbxl[ 3 ] = FillCheckBoxList( new CheckBoxList(), "Codes.ResourceType", false, false, includeDescriptions );
            cbxl[ 4 ] = FillCheckBoxList( new CheckBoxList(), "Codes.ResourceFormat", false, false, includeDescriptions );
            cbxl[ 5 ] = FillCheckBoxListFromEducationalUse( new CheckBoxList(), "Knowledge", includeDescriptions);
            cbxl[ 6 ] = FillCheckBoxListFromEducationalUse( new CheckBoxList(), "Reasoning", includeDescriptions );
            cbxl[ 7 ] = FillCheckBoxListFromEducationalUse( new CheckBoxList(), "Skills", includeDescriptions );
            cbxl[ 8 ] = FillCheckBoxList( new CheckBoxList(), "Codes.GroupType", false, false, includeDescriptions );
            cbxl[ 9 ] = FillCheckBoxList( new CheckBoxList(), "Codes.ItemType", false, false, includeDescriptions );

            return cbxl;
        }
        public static void FillAllCommonCheckBoxes( ref CheckBoxList[] lists, bool includeDescriptions )
        {
            CheckBoxList[] cbxls = GetAllCommonCheckBoxes( includeDescriptions );

            for ( var i = 0 ; i < lists.Length ; i++ )
            {
                for ( var j = 0 ; j < cbxls[ i ].Items.Count ; j++ )
                {
                    lists[ i ].Items.Add( cbxls[ i ].Items[ j ] );
                }
            }
        }

        public static void FillAllCommonCheckBoxes( ref CheckBoxList[] lists )
        {
            FillAllCommonCheckBoxes( ref lists, false );
        }

        public static void ConstructListItems( string titleField, ref CheckBoxList cbxl, DataSet ds, bool includeDescriptions, string tableName )
        {
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    ListItem item = new ListItem();
                    if ( includeDescriptions & DatabaseManager.GetRowPossibleColumn(dr, "Description") != "" )
                    {
                        item.Attributes.Add( "id", tableName + "_" + DatabaseManager.GetRowPossibleColumn( dr, "Id" ) );
                        item.Attributes.Add( "title", DatabaseManager.GetRowPossibleColumn(dr, titleField) + "|" + DatabaseManager.GetRowPossibleColumn( dr, "Description" ) );
                        item.Attributes.Add( "class", "toolTipLink" );
                    }
                    item.Text = DatabaseManager.GetRowColumn( dr, titleField );
                    item.Value = DatabaseManager.GetRowColumn( dr, "Id" );
                    item.Attributes.Add( "itemName", item.Text );
                    item.Attributes.Add( "itemID", item.Value );
                    item.Attributes.Add( "itemCounts", DatabaseManager.GetRowPossibleColumn( dr, "ItemCounts" ) );
                    cbxl.Items.Add( item );
                }
            }
        }
        #endregion
    }
}
