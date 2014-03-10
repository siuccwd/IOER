using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Configuration;
using System.Web.UI.WebControls;
using Microsoft.ApplicationBlocks.Data;
using System.Globalization;

using ILPathways.Business;

namespace LRWarehouse.DAL
{
    public class DatabaseManager : BaseDataManager
    {
        static string thisClassName = "DatabaseManager";

        #region Generic Search Methods

        /// <summary>
        /// Select related data using passed parameters
        /// - uses main connection string
        /// - does not allow custom sorting
        /// - uses custom paging
        /// - only requested range of rows will be returned
        /// </summary>
        /// <param name="searchProc"></param>
        /// <param name="pFilter"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static DataSet Search( string searchProc, string pFilter, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            string sortOrder = "";
            string connectionString = LRWarehouseRO();
            return Search( searchProc, connectionString, pFilter, sortOrder, pStartPageIndex, pMaximumRows, ref pTotalRows );
        } //


        /// <summary>
        /// Select related data using passed parameters
        /// - uses main connection string
        /// - uses custom paging
        /// - only requested range of rows will be returned
        /// </summary>
        /// <param name="searchProc"></param>
        /// <param name="pFilter"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static DataSet Search( string searchProc, string pFilter, string sortOrder, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            string connectionString = LRWarehouseRO();

            return Search( searchProc, pFilter, sortOrder, pStartPageIndex, pMaximumRows, ref pTotalRows );

        } //


        /// <summary>
        /// Select related data using passed parameters
        /// - uses custom paging
        /// - only requested range of rows will be returned
        /// </summary>
        /// <param name="searchProc"></param>
        /// <param name="pFilter"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static DataSet Search( string searchProc, string connectionString, string pFilter, string sortOrder, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            if ( pFilter.Length > 0 )
            {
                if ( pFilter.ToLower().IndexOf( "where" ) == -1
                  || pFilter.ToLower().IndexOf( "where" ) > 10 )
                    pFilter = " Where " + pFilter;
            }

            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", SqlDbType.VarChar );
            sqlParameters[ 0 ].Size = 500;
            sqlParameters[ 0 ].Value = pFilter;

            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", SqlDbType.VarChar );
            sqlParameters[ 1 ].Size = 100;
            sqlParameters[ 1 ].Value = sortOrder;

            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", SqlDbType.Int );
            sqlParameters[ 2 ].Value = pStartPageIndex;

            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", SqlDbType.Int );
            sqlParameters[ 3 ].Value = pMaximumRows;

            sqlParameters[ 4 ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ 4 ].Direction = ParameterDirection.Output;

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, searchProc, sqlParameters );

                string rows = sqlParameters[ 4 ].Value.ToString();
                try
                {
                    pTotalRows = Int32.Parse( rows );
                }
                catch
                {
                    pTotalRows = 0;
                }

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".Search() " );
                return null;

            }
        }
        #endregion
        #region Code tables
        /// <summary>
        /// Format a code table list
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="valueField"></param>
        /// <param name="textField"></param>
        /// <param name="sortField"></param>
        /// <returns></returns>
        public static DataSet CodeTableSearch( string tableName, string valueField, string textField, string sortField )
        {
            bool includeWareHouseTotals = false;
            //maybe always include desc
            bool includeDescriptions = false;
            string filter = "";
            return CodeTableSearch( tableName, valueField, textField, sortField, filter, includeWareHouseTotals, includeDescriptions );
        }

        public static DataSet CodeTableSearch( string tableName, string valueField, string textField, string sortField, string filter )
        {
            bool includeWareHouseTotals = false;
            //maybe always include desc
            bool includeDescriptions = false;
            return CodeTableSearch( tableName, valueField, textField, sortField, filter, includeWareHouseTotals, includeDescriptions );
        }

        public static DataSet CodeTableSearch( string tableName, string valueField, string textField, string sortField, string filter, bool includeWareHouseTotals )
        {
            //maybe always include desc
            bool includeDescriptions = false;
            return CodeTableSearch( tableName, valueField, textField, sortField, filter, includeWareHouseTotals, includeDescriptions );
        }

        /// <summary>
        /// Format a code table list
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="valueField"></param>
        /// <param name="textField"></param>
        /// <param name="sortField"></param>
        /// <param name="includeWareHouseTotals"></param>
        /// <param name="includeDescriptions"></param>
        /// <returns></returns>
        public static DataSet CodeTableSearch( string tableName, string valueField, string textField, string sortField, string filter, bool includeWareHouseTotals, bool includeDescriptions )
        {
            string title = textField;
            if (includeWareHouseTotals)
                title = string.Format( "{0} + ' (' + isnull(convert(varchar,WareHouseTotal),0) + ')' As {0} ", textField );

            string addDescriptions = "";
            if ( includeDescriptions )
            {
                addDescriptions = ", [Description]";
            }
            string where = "([IsActive] = 1)";

            if ( filter!= null && filter.Trim().Length > 0 )
            {
                where += FormatSearchItem( where, filter, "AND" );

            }
            string sql = string.Format( "SELECT {0}, {1} " + addDescriptions + ", WareHouseTotal FROM [{2}] Where {3} ORDER BY {4}", valueField, title, tableName, where, sortField );

            DataSet ds = DatabaseManager.DoQuery( sql );
            
            return ds;
        }

        /// <summary>
        /// Populate a list from the code table
        /// </summary>
        /// <param name="list">Droplist to be populated</param>
        /// <param name="language"></param>
        /// <param name="codeName">CodeName</param>
        /// <param name="sortOrder">Code table column to sort by (Ex. SortOrder, or IntegerValue</param>
        /// <param name="textName">Code table column to be assigned to text name of the list (Often StringValue)</param>
        /// <param name="valueName">Code table column to be assigned to value name of the list (often StringValue)</param>
        /// <param name="selectTitle">Title to display for first row in the list</param>
        public static void PopulateCodeList( DropDownList list, string language, string codeName, string sortOrder, string textName, string valueName, string selectTitle )
        {
            DataSet ds1 = DatabaseManager.GetCodeValues( language, codeName, sortOrder );
            if ( ds1 == null )
                return;
            DataSet ds = ds1.Copy();
            AddEntryToTable( ds.Tables[ 0 ], 0, selectTitle, textName, valueName );

            // get BusinessProfileStatus
            list.DataSource = ds;
            list.DataTextField = textName;
            list.DataValueField = valueName;
            list.DataBind();

        }//


        /// <summary>
        /// Returns a listing of values for provided codes
        /// Uses a default language of English
        /// </summary>
        /// <param name="code">Code to look up</param>
        /// <param name="sort">Sort order of code list</param>
        /// <returns>DataSet of values related to the code</returns>
        public static DataSet GetCodeValues( string code, string sort )
        {
            return GetCodeValues( "en", code, sort );

        } //

        /// <summary>
        /// Returns a listing of values for provided codes
        /// </summary>
        /// <param name="language">Language for code values</param>
        /// <param name="code">Code to look up</param>
        /// <param name="sort">Sort order of code list</param>
        /// <returns>DataSet of values related to the code</returns>
        public static DataSet GetCodeValues( string language, string code, string sort )
        {
            try
            {

                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Code", code );
                sqlParameters[ 1 ] = new SqlParameter( "@Sort", sort );
                sqlParameters[ 2 ] = new SqlParameter( "@Language", language );

                DataSet ds = SqlHelper.ExecuteDataset(
                    LRWarehouseRO(), "CodeTable_Select", sqlParameters );

                if ( DoesDataSetHaveRows( ds ) == false )
                    return null;
                else
                    return ds;
            }
            catch ( Exception e )
            {
                LogError( "DatabaseManager.GetCodeValues(): \r\nlanguage: " + language + ", code: " + code + ", sort: " + sort + " \r\n" + e.ToString() );
                return null;
            }

        } //

        /// <summary>
        /// Get a single CodeTable record IntegerValue
        /// </summary>
        /// <param name="code"></param>
        /// <param name="stringValue"></param>
        /// <returns></returns>
        public static int CodeTable_Get( string code, string stringValue, int defaultValue )
        {
            string language = "en";
            int id = 0;
            int intValue = defaultValue;

            DataSet ds = CodeTable_Get( id, code, language, stringValue );
            if ( DoesDataSetHaveRows( ds ) == false )
                return defaultValue;

            intValue = GetRowColumn( ds.Tables[ 0 ].Rows[ 0 ], "IntegerValue", defaultValue );

            return intValue;
        } //

        /// <summary>
        /// Get a single CodeTable record
        /// </summary>
        /// <param name="code"></param>
        /// <param name="stringValue"></param>
        /// <returns></returns>
        public static DataSet CodeTable_Get( string code, string stringValue )
        {
            string language = "en";
            int id = 0;
            return CodeTable_Get( id, code, language, stringValue );
        } //

        /// <summary>
        /// Get a single CodeTable record
        /// </summary>
        /// <param name="id"></param>
        /// <param name="code"></param>
        /// <param name="stringValue"></param>
        /// <returns></returns>
        public static DataSet CodeTable_Get( int id, string code, string stringValue )
        {
            string language = "en";
            return CodeTable_Get( id, code, language, stringValue );
        } //

        /// <summary>
        /// Get a single CodeTable record
        /// </summary>
        /// <param name="id"></param>
        /// <param name="code"></param>
        /// <param name="language"></param>
        /// <param name="stringValue"></param>
        /// <returns></returns>
        private static DataSet CodeTable_Get( int id, string code, string language, string stringValue )
        {
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", id );
                sqlParameters[ 1 ] = new SqlParameter( "@CodeName", code );
                sqlParameters[ 2 ] = new SqlParameter( "@LanguageCode", language );
                sqlParameters[ 3 ] = new SqlParameter( "@StringValue", stringValue );

                DataSet ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), "CodeTableGet", sqlParameters );

                if ( DoesDataSetHaveRows( ds ) == false )
                    return null;
                else
                    return ds;
            }
            catch ( Exception e )
            {
                LogError( "DatabaseManager.CodeTable_Get(): \r\nlanguage: " + language + ", code: " + code + ", stringValue: " + stringValue + " \r\n" + e.ToString() );
                return null;
            }

        } //

        public static string UpdateWarehouseTotals()
        {
            string status = "successful";

            try
            {
                using (SqlConnection conn = new SqlConnection(LRWarehouse()))
                {
                    using (SqlCommand cmd = new SqlCommand("CodeTables_UpdateWarehouseTotals", conn))
                    {
                        cmd.CommandTimeout = 300; // As of 2/18/13 this was taking 28 seconds on the test server.
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("DatabaseManager.UpdateWarehouseTotals(): " + ex.ToString());
                status = ex.Message;
            }
            return status;
        }

        public static string UpdatePublisherTotals()
        {
            string status = "successful";

            try
            {
                using (SqlConnection conn = new SqlConnection(LRWarehouse()))
                {
                    using (SqlCommand cmd = new SqlCommand("CodeTables_UpdatePublisherTotals", conn))
                    {
                        cmd.CommandTimeout = 300;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("DatabaseManager.UpdatePublisherTotals(): " + ex.ToString());
                status = ex.Message;
            }
            return status;
        }
        #endregion

        #region General Codelike table methods

        /// <summary>
        /// Select [Id] and [Title] from a generic code table
        /// </summary>
        /// <param name="codeTableName"></param>
        /// <returns></returns>
        public static DataSet selectCodesFromTable( string codeTableName )
        {
            try
            {
                string sql = String.Format( "SELECT [Id], [Title] FROM {0}", codeTableName );

                DataSet ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), System.Data.CommandType.Text, sql );

                if ( DoesDataSetHaveRows( ds ) == false )
                {
                    return null;
                }
                else
                {
                    return ds;
                }
            }
            catch( Exception ex )
            {
                LogError( ex, thisClassName + ".selectCodesFromTable()" );
                return null;
            }
        }

        /// <summary>
        /// Select all values from the provided code table for the provided language
        /// </summary>
        /// <param name="codeTableName"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static DataSet SelectCodesTable( string codeTableName, string lang )
        {

            try
            {

                string sqlDefault = String.Format( "SELECT [Code], [Title] FROM {0} where languageCode = {1} order by [code] ", codeTableName, lang );

                DataSet ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), System.Data.CommandType.Text, sqlDefault );

                if ( DoesDataSetHaveRows( ds ) == false )
                    return null;
                else
                    return ds;

            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".SelectCodesTable() " );
                return null;
            }

        } //

        /// <summary>
        /// Generic method to select rows from a code table
        /// - the Optional, properly structured filter is to be formatted by the caller
        /// </summary>
        /// <param name="codeTableName"></param>
        /// <param name="codeKeyName"></param>
        /// <param name="codeTitleName"></param>
        /// <param name="orderByName"></param>
        /// <param name="filterValue"></param>
        /// <returns></returns>
        public static DataSet SelectCodesTable( string codeTableName,
                                                                                        string codeKeyName,
                                                                                        string codeTitleName,
                                                                                        string orderByName,
                                                                                        string filterValue )
        {
            //10-04-28 mparsons - changed to assume all filtering done by caller. This should just be a dumb method
            //string codeFilterValue = "";
            //return SelectCodesTable( codeTableName, codeKeyName, codeTitleName, orderByName, codeFilterValue, filterValue );

            //set base sql
            string sqlDefault = String.Format( "SELECT [{0}] As Code, [{1}] As Title FROM {2} ",
                                                    codeKeyName,
                                                    codeTitleName,
                                                    codeTableName );
            try
            {

                if ( filterValue.Length > 0 )
                {
                    sqlDefault = sqlDefault + " Where " + filterValue;
                }

                sqlDefault = sqlDefault + " order by " + orderByName;

                DataSet ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), System.Data.CommandType.Text, sqlDefault );

                if ( DoesDataSetHaveRows( ds ) == false )
                    return null;
                else
                    return ds;

            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".SelectCodesTable() " );
                return null;
            }
        } //

        /// <summary>
        /// Generic method to select rows from a code table - OBSOLETE - REOVE AFTER VALIDATING LATTER CHANGES
        /// </summary>
        /// <param name="codeTableName"></param>
        /// <param name="codeKeyName"></param>
        /// <param name="codeTitleName"></param>
        /// <param name="orderByName"></param>
        /// <param name="codeFilterValue">This can just be a partial code value or be used for a custom filter. If it contains parenthesis, it assumed to already be properly formatted</param>
        /// <param name="titleFilterValue"></param>
        /// <returns></returns>
        public static DataSet SelectCodesTable( string codeTableName,
                                                                                        string codeKeyName,
                                                                                        string codeTitleName,
                                                                                        string orderByName,
                                                                                        string codeFilterValue,
                                                                                        string titleFilterValue )
        {
            //set base sql
            string sqlDefault = String.Format( "SELECT [{0}] As Code, [{1}] As Title FROM {2} ",
                                                    codeKeyName,
                                                    codeTitleName,
                                                    codeTableName );
            try
            {
                string filter = "";
                string booleanOperator = "AND";

                if ( codeFilterValue.Trim().Length > 0 )
                {
                    //if filter contains parens, assume custom filter
                    if ( codeFilterValue.IndexOf( "(" ) == -1 )
                    {
                        //assume code is on a begins with, unless wild card passed in 
                        if ( codeFilterValue.IndexOf( "%" ) == -1 )
                        {
                            codeFilterValue = codeFilterValue + "%";
                        }
                    }

                    //determine if a custom filter was provided
                    if ( codeFilterValue.IndexOf( "(" ) > -1 )
                        filter += BaseDataManager.FormatSearchItem( filter, codeFilterValue, booleanOperator );
                    else
                        filter += BaseDataManager.FormatSearchItem( filter, codeKeyName, codeFilterValue, booleanOperator );
                }

                if ( titleFilterValue.Trim().Length > 0 )
                {
                    string keywordFilter = HandleApostrophes( titleFilterValue );
                    if ( keywordFilter.IndexOf( "%" ) == -1 )
                    {
                        keywordFilter = "%" + keywordFilter + "%";
                    }
                    filter += BaseDataManager.FormatSearchItem( filter, codeTitleName, keywordFilter, booleanOperator );
                }


                if ( filter.Length > 0 )
                {
                    filter = " Where " + filter;

                    sqlDefault = sqlDefault + filter;
                }

                sqlDefault = sqlDefault + " order by " + orderByName;

                DataSet ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), System.Data.CommandType.Text, sqlDefault );

                if ( DoesDataSetHaveRows( ds ) == false )
                    return null;
                else
                    return ds;

            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".SelectCodesTable() " );
                return null;
            }

        } //

        public static DataSet CodeTableInsert( string codeTableName,
                                                                                        string codeKeyName,
                                                                                        string codeTitleName,
                                                                                        string orderByName,
                                                                                        string codeFilterValue,
                                                                                        string titleFilterValue )
        {
            //set base sql
            string sqlDefault = String.Format( "Insert [{0}] As Code, [{1}] As Title FROM {2} ",
                                                    codeKeyName,
                                                    codeTitleName,
                                                    codeTableName );
            try
            {
                string filter = "";
                string booleanOperator = "AND";

                if ( codeFilterValue.Trim().Length > 0 )
                {
                    //if filter contains parens, assume custom filter
                    if ( codeFilterValue.IndexOf( "(" ) == -1 )
                    {
                        //assume code is on a begins with, unless wild card passed in 
                        if ( codeFilterValue.IndexOf( "%" ) == -1 )
                        {
                            codeFilterValue = codeFilterValue + "%";
                        }
                    }

                    //determine if a custom filter was provided
                    if ( codeFilterValue.IndexOf( "(" ) > -1 )
                        filter += BaseDataManager.FormatSearchItem( filter, codeFilterValue, booleanOperator );
                    else
                        filter += BaseDataManager.FormatSearchItem( filter, codeKeyName, codeFilterValue, booleanOperator );
                }

                if ( titleFilterValue.Trim().Length > 0 )
                {
                    string keywordFilter = HandleApostrophes( titleFilterValue );
                    if ( keywordFilter.IndexOf( "%" ) == -1 )
                    {
                        keywordFilter = "%" + keywordFilter + "%";
                    }
                    filter += BaseDataManager.FormatSearchItem( filter, codeTitleName, keywordFilter, booleanOperator );
                }


                if ( filter.Length > 0 )
                {
                    filter = " Where " + filter;

                    sqlDefault = sqlDefault + filter;
                }

                sqlDefault = sqlDefault + " order by " + orderByName;

                DataSet ds = SqlHelper.ExecuteDataset( LRWarehouse(), System.Data.CommandType.Text, sqlDefault );

                if ( DoesDataSetHaveRows( ds ) == false )
                    return null;
                else
                    return ds;

            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".SelectCodesTable() " );
                return null;
            }

        } //
        #endregion

        #region ****************** Generic methods for Join type tables - typically only have parent id (type=int) and child id
        /// <summary>
        /// Generic method to select parent and related code table values
        /// </summary>
        /// <param name="parentTableName"></param>
        /// <param name="parentKeyName"></param>
        /// <param name="parentCodeKeyName"></param>
        /// <param name="codeTableName"></param>
        /// <param name="codeKeyName"></param>
        /// <param name="codeTitleName"></param>
        /// <param name="orderByName"></param>
        /// <param name="parentKeyValue"></param>
        /// <param name="filterValue">optional value provided fully formed by caller</param>
        /// <returns></returns>
        public static DataSet SelectParentRelatedTable( string parentTableName,
                string parentKeyName,
                string parentCodeKeyName,
                string codeTableName,
                string codeKeyName,
                string codeTitleName,
                string orderByName,
                int parentKeyValue,
                string filterValue )
        {
            string sql = "SELECT Distinct parent.[" + parentKeyName + "] " +
                                            ",parent.[" + parentCodeKeyName + "] " +
                                            ",codeTable.[" + codeKeyName + "] As Code " +
                                            ",codeTable.[" + codeTitleName + "] As Title " +
                                    "FROM " + parentTableName + " parent  " +
                                    "	Inner join " + codeTableName + " codeTable " +
                                    "		on 	parent.[" + parentCodeKeyName + "] = codeTable.[" + codeKeyName + "] " +
                                    "Where parent.[" + parentKeyName + "] = " + parentKeyValue + " ";

            string orderBy = "order by parent.[" + parentKeyName + "], codeTable.[" + orderByName + "] ";
            string sqlDefault = "";

            try
            {
                if ( filterValue.Trim().Length == 0 )
                {
                    sqlDefault = sql + orderBy;
                }
                else
                {
                    //if filter contains parens, assume custom filter
                    if ( filterValue.IndexOf( "(" ) == -1 )
                    {
                        if ( filterValue.IndexOf( "%" ) == -1 )
                        {
                            filterValue = " '%" + filterValue + "%' ";
                        }
                    }
                    sqlDefault = sql + " AND " + filterValue + " " + orderBy;
                }

                DataSet ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), System.Data.CommandType.Text, sqlDefault );

                if ( DoesDataSetHaveRows( ds ) == false )
                    return null;
                else
                    return ds;

            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".SelectParentRelatedTable(). For table: " + parentTableName );
                return null;
            }

        } //


        /// <summary>
        /// Generic method to insert a row in a join-type table
        /// </summary>
        /// <param name="relatedTableName"></param>
        /// <param name="parentKeyName"></param>
        /// <param name="relatedKeyName"></param>
        /// <param name="parentKeyValue"></param>
        /// <param name="relatedKeyValue">Related table key is an integer</param>
        /// <param name="createdById"></param>
        /// <returns></returns>
        public static bool InsertRelatedTableRow( string relatedTableName,
                                string parentKeyName,
                                string relatedKeyName,
                                int parentKeyValue,
                                int relatedKeyValue,
                                int createdById,
                                ref string statusMessage )
        {
            string sql = String.Format( "Insert into " + relatedTableName + " " +
                                    "(" + parentKeyName + ", " + relatedKeyName + ", CreatedById) " +
                                    "VALUES({0},{1},{2})", parentKeyValue, relatedKeyValue, createdById );

            /*
Example
INSERT INTO [dbo].[ProjectSector] ([ProjectId],[SectorId] ,[CreatedById]			)
     VALUES (ProjectId, SectorId, CreatedById )
             * 
            */
            try
            {
                SqlHelper.ExecuteNonQuery( LRWarehouse(), CommandType.Text, sql );
                return true;

            }
            catch ( Exception ex )
            {
                statusMessage = "Error encountered while trying to add a row from the " + relatedTableName + " table: " + ex.Message.ToString();
                LogError( ex, thisClassName + ".InsertRelatedTableRow(int relatedKeyValue). For table: " + relatedTableName );
                return false;
            }

        } //

        /// <summary>
        /// Generic method to insert a row in a join-type table
        /// </summary>
        /// <param name="relatedTableName"></param>
        /// <param name="parentKeyName"></param>
        /// <param name="relatedKeyName"></param>
        /// <param name="parentKeyValue"></param>
        /// <param name="relatedKeyValue">Related table key is a string</param>
        /// <param name="createdById"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static bool InsertRelatedTableRow( string relatedTableName,
                                string parentKeyName,
                                string relatedKeyName,
                                int parentKeyValue,
                                string relatedKeyValue,
                                int createdById,
                                ref string statusMessage )
        {
            string sql = String.Format( "Insert into " + relatedTableName + " " +
                                    "(" + parentKeyName + ", " + relatedKeyName + ", CreatedById) " +
                                    "VALUES({0},'{1}',{2})", parentKeyValue, relatedKeyValue, createdById );
            try
            {
                SqlHelper.ExecuteNonQuery( LRWarehouse(), CommandType.Text, sql );
                return true;

            }
            catch ( Exception ex )
            {
                statusMessage = "Error encountered while trying to add a row from the " + relatedTableName + " table: " + ex.Message.ToString();
                LogError( ex, thisClassName + ".InsertRelatedTableRow(string relatedKeyValue). For table: " + relatedTableName );
                return false;
            }

        } //

        /// <summary>
        /// Generic method to delete row from a child table
        /// </summary>
        /// <param name="relatedTableName"></param>
        /// <param name="parentKeyName"></param>
        /// <param name="relatedKeyName"></param>
        /// <param name="parentKeyValue"></param>
        /// <param name="relatedKeyValue">Related table key is an integer</param>
        /// <returns></returns>
        public static bool RemoveRelatedTableRow( string relatedTableName,
                                string parentKeyName,
                                string relatedKeyName,
                                int parentKeyValue,
                                int relatedKeyValue,
                                ref string statusMessage )
        {

            if ( parentKeyValue == 0 || relatedKeyValue == 0 )
            {
                //error
                statusMessage = "Error: both a parent and child key must be provided in order to remove a remove from this table (" + relatedTableName + ")";
                return false;
            }

            string sql = "DELETE FROM " + relatedTableName + " " +
                                    "Where " + parentKeyName + "  = " + parentKeyValue + " " +
                                    "And   " + relatedKeyName + "  = " + relatedKeyValue + " ";

            /*
                Example
                DELETE FROM [dbo].[ProjectSector]
                WHERE [ProjectId = 1] And  [SectorId = 2]
            */
            try
            {
                SqlHelper.ExecuteNonQuery( LRWarehouse(), CommandType.Text, sql );
                return true;

            }
            catch ( Exception ex )
            {
                statusMessage = "Error encountered while trying to remove a row from the " + relatedTableName + " table: " + ex.Message.ToString();
                LogError( ex, thisClassName + ".RemoveRelatedTableRow(). For table: " + relatedTableName );
                return false;
            }

        } //

        /// <summary>
        /// Generic method to delete row from a child table
        /// </summary>
        /// <param name="relatedTableName"></param>
        /// <param name="parentKeyName"></param>
        /// <param name="relatedKeyName"></param>
        /// <param name="parentKeyValue"></param>
        /// <param name="relatedKeyValue">Related table key is a string</param>
        /// <returns></returns>
        public static bool RemoveRelatedTableRow( string relatedTableName,
                                string parentKeyName,
                                string relatedKeyName,
                                int parentKeyValue,
                                string relatedKeyValue,
                                ref string statusMessage )
        {

            if ( parentKeyValue == 0 || relatedKeyValue.Trim().Length == 0 )
            {
                //error
                statusMessage = "Error: both a parent and child key must be provided in order to remove a remove from this table (" + relatedTableName + ")";
                return false;
            }

            string sql = "DELETE FROM " + relatedTableName + " " +
                                    "Where " + parentKeyName + "  = " + parentKeyValue + " " +
                                    "And   " + relatedKeyName + "  = '" + relatedKeyValue + "' ";

            /*
                Example
                DELETE FROM [dbo].[ProjectSector]
                WHERE [ProjectId = 1] And  [SectorId = 2]
            */
            try
            {
                SqlHelper.ExecuteNonQuery( LRWarehouse(), CommandType.Text, sql );
                return true;

            }
            catch ( Exception ex )
            {
                statusMessage = "Error encountered while trying to remove a row from the " + relatedTableName + " table: " + ex.Message.ToString();
                LogError( ex, thisClassName + ".RemoveRelatedTableRow(string relatedKeyValue). For table: " + relatedTableName );
                return false;
            }

        } //
        #endregion


        #region Generic methods
        /// <summary>
        /// General query, requires userid and password
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <returns>Dataset with Query results or null if there are not results or if there was any error.</returns>
        public static DataSet DoQuery( string sql )
        {
            DataSet ds = new DataSet();

            try
            {

                //use default database connection for sql
                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), System.Data.CommandType.Text, sql );

                if ( ds.Tables.Count == 0 ) ds = null;

                return ds;
            }
            catch ( SqlException sex )
            {
                if ( sex.Message.ToLower().IndexOf( "timeout expired" ) > -1 )
                {
                    return SetDataSetMessage( "Error this feaure is temporarily unavailable. Try again in a few minutes." );
                }
                else
                {
                    return SetDataSetMessage( "DatabaseManager.DoQuery(): " + sex.Message.ToString() );
                }
            }
            catch ( Exception e )
            {
                string queryString = GetPublicUrl( HttpContext.Current.Request.QueryString.ToString() );

                LogError( string.Format( "DatabaseManager.DoQuery(): \r\nUser: {0}\r\nURL: " + queryString + "\r\nSQL:" + sql + "\r\n" + e.ToString(), "n/a" ) );
                //return SetDataSetMessage( "DatabaseManager.DoQuery(): " + e.Message.ToString() );
                return null;
            }

        } //

        /// <summary>
        /// Retrieve count of rows in passed table (or view), using passed filter
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static int GetTableCount( string tableName, string filter )
        {
            int rowCount = 0;

            string sql = String.Format( "select count(*) As RowsFound from {0} where {1}", tableName, filter );

            try
            {
                SqlConnection con = new SqlConnection( BaseDataManager.LRWarehouseRO() );
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = sql;

                SqlDataAdapter dAdapter = new SqlDataAdapter();
                dAdapter.SelectCommand = cmd;
                con.Open();
                DataSet objDs = new DataSet();
                dAdapter.Fill( objDs );
                con.Close();

                foreach ( DataRow dRow in objDs.Tables[ 0 ].Rows )
                {
                    string cnt = dRow[ "RowsFound" ].ToString();

                    rowCount = Int32.Parse( cnt );
                }

            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".GetTableCount() - Unexpected error encountered." );
                rowCount = 0;
            }

            return rowCount;
        }//

        /// <summary>
        /// Excute passed sql and return row count
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int DoQueryCount( string sql )
        {
            DataSet ds = new DataSet();
            int count = 0;
            try
            {
                //use default database connection for sql
                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), System.Data.CommandType.Text, sql );

                if ( ds != null && ds.Tables.Count > 0 && ds.Tables[ 0 ].Rows.Count > 0 )
                    count = ds.Tables[ 0 ].Rows.Count;
                else
                    count = 0;

            }
            catch ( Exception e )
            {
                LogError( "DatabaseManager.DoQueryCount(): " + e.ToString() );
            }
            return count;
        } //

        /// <summary>
        /// Passed sql that is expected to return a single column
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static string GetSqlResult( string sql )
        {
            DataSet ds = new DataSet();
            DataRow dr;

            string result = "";

            try
            {
                //use default database connection for sql
                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), System.Data.CommandType.Text, sql );

                if ( ds != null && ds.Tables.Count > 0 && ds.Tables[ 0 ].Rows.Count > 0 )
                {
                    dr = ds.Tables[ 0 ].Rows[ 0 ];

                    result = dr[ 0 ].ToString();
                }



            }
            catch ( Exception e )
            {
                string queryString = GetPublicUrl( HttpContext.Current.Request.QueryString.ToString() );
                LogError( "DatabaseManager.GetSqlResult(): \r\nURL: " + queryString + "\r\nSQL:" + sql + "\r\n" + e.ToString() );
            }
            return result;
        } //

        /// <summary>
        /// Execute passed sql (primarily for quick updates)
        /// </summary>
        /// <param name="executeSQL"></param>
        /// <returns></returns>
        public static string ExecuteSql( string executeSQL )
        {
            string message = "Unknown";

            try
            {
                SqlHelper.ExecuteNonQuery( LRWarehouse(), CommandType.Text, executeSQL );
                message = "Successful";
            }
            catch ( Exception ex )
            {
                string queryString = GetPublicUrl( HttpContext.Current.Request.QueryString.ToString() );

                LogError( string.Format( "DatabaseManager.ExecuteSql(): \r\nUser: {0}\r\nURL: " + queryString + "\r\nSQL:" + executeSQL + "\r\n" + ex.ToString(), "n/a" ) );

                message = String.Format( "Unsuccessful: {0}.ExecuteSql(): {1}", thisClassName, ex.Message.ToString() );

            }//


            return message;
        }//

        /// <summary>
        /// Run the stored procedure, passing parameters to it
        /// Uses a read only connection, so an update proc will fail
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// 
        public static DataSet ExecuteProc( string procedureName, SqlParameter[] parameters )
        {
            string conString = BaseDataManager.LRWarehouse();

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( conString, CommandType.StoredProcedure, procedureName, parameters );
                if ( ds.HasErrors )
                {
                    return null;
                }
            }
            catch ( Exception ex )
            {
                BaseDataManager.LogError( ex, thisClassName + ".ExecuteProc(): " );
                return null;
            }

            return ds;
        }
        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public DatabaseManager()
        {

        }
    }
}
