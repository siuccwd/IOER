using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;


using ILPathways.Common;
using Isle.DataContracts;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using LDBM = LRWarehouse.DAL.DatabaseManager;

namespace Isle.BizServices
{
    public class CodeTableBizService : ServiceHelper
    {
        public CodeSearchResponse CodeTableSearch( CodeSearchRequest request )
        {
            DatabaseManager myManager = new DatabaseManager();
           // int totalRows = 0;
            string message = "";
            CodeSearchResponse searchResponse = new CodeSearchResponse();
            //ServiceHelper.DoTrace( 6, "Isle.BizServices.CodeTableSearch, table: " + request.TableName );

            //search
            DataSet ds = DatabaseManager.CodeTableSearch( request.TableName, request.IdColumn, request.TitleColumn, request.OrderBy, request.Filter, request.UseWarehouseTotalTitle );

            List<CodesDataContract> dataContractList = new List<CodesDataContract>();
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                //check for error message
                if ( ServiceHelper.HasErrorMessage( ds ) )
                {
                    message = ServiceHelper.GetWsMessage( ds );
                    searchResponse.Error.Message += message + "; ";
                    searchResponse.Status = StatusEnumDataContract.Failure;
                }
                else
                {

                    CodesDataContract dataContract;
                    foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
                    {
                        int id = DatabaseManager.GetRowColumn( dr, request.IdColumn, 0 );
                        string title = DatabaseManager.GetRowColumn( dr, request.TitleColumn, "" );

                        dataContract = new CodesDataContract() { Id = id, Title = title };
                        dataContract.TableName = request.TableName;
                        dataContractList.Add( dataContract );

                    } //end foreach
                }
            }


            searchResponse.ResultList = dataContractList;
            searchResponse.TableName = request.TableName;
            searchResponse.ResultCount = dataContractList.Count;
            searchResponse.TotalRows = dataContractList.Count;

            return searchResponse;
        }//

        public static void PopulateGridPageSizeList( ref DropDownList list )
        {
            DataSet ds = LDBM.GetCodeValues( "GridPageSize", "SortOrder" );
            LDBM.PopulateList( list, ds, "StringValue", "StringValue", "Select Size" );
        } //

        /// <summary>
        /// Return values for a code table, optionally specify where to return all rows or only those with total used > 0
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="mustHaveValues">If true, only return rows with total</param>
        /// <returns></returns>
        public static List<CodeItem> Resource_CodeTableSelectList( string tableName, bool mustHaveValues )
        {
            return Resource_CodeTableSelectLists( tableName, mustHaveValues );
        } //
        /// <summary>
        /// Return all values for a code table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static List<CodeItem> Resource_CodeTableSelectList( string tableName )
        {
            return Resource_CodeTableSelectLists( tableName, false );
        } //
        private static List<CodeItem> Resource_CodeTableSelectLists( string tableName, bool mustHaveValues )
        {
            List<CodeItem> list = new List<CodeItem>();
            CodeItem code = new CodeItem();
            DataSet ds = Resource_CodeTableSelect( tableName );
            if ( DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow row in ds.Tables[ 0 ].Rows )
                {
                    code = new CodeItem();
                    code.Id = GetRowColumn(row, "Id", 0);
                    code.Title = GetRowColumn( row, "Title", "Missing" );
                    code.Description = GetRowColumn( row, "Description", "Missing" );
                    code.WarehouseTotal = GetRowColumn( row, "WarehouseTotal", 0 );
                    code.SortOrder = GetRowPossibleColumn( row, "SortOrder", 10 );
                }
            }
            return list;

        } //
        public static DataSet Resource_CodeTableSelect( string tableName )
        {
            string sql = string.Format( "SELECT [Id],[Title]  ,[Description] ,isnull([WarehouseTotal],0) As [WarehouseTotal]  FROM [dbo].[{0}] where IsActive= 1 order by title", tableName);
            DataSet ds = LDBM.DoQuery( sql );
            return ds;

        } //
    }
}
