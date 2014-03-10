using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Configuration;
using System.Web.UI.WebControls;
using Microsoft.ApplicationBlocks.Data;

namespace ILPathways.DAL
{
    public class DatabaseManager : BaseDataManager
    {
        static string thisClassName = "DatabaseManager";

        #region code tables

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
                    GatewayConnectionRO(), "CodeTable_Select", sqlParameters );

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

        #endregion
        #region Generic methods
        /// <summary>
        /// General query, requires userid and password in interface
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <returns>Dataset with Query results or null if there are not results or if there was any error.</returns>
        public static DataSet DoQuery( string sql )
        { 
           return DoQuery( sql, GatewayConnectionRO() ); 
        }

        /// <summary>
        /// General query, requires userid and password in interface
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="connString"></param>
        /// <returns></returns>
        public static DataSet DoQuery( string sql, string connString )
        {
            DataSet ds = new DataSet();

            try
            {

                //do check for apos, in case not properly done
                //if ( sql.IndexOf( "'" ) > -1 && sql.IndexOf( "''" ) == -1 )
                //    sql = HandleApostrophes( sql );

                ds = SqlHelper.ExecuteDataset( connString, System.Data.CommandType.Text, sql );

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
            return GetTableCount( tableName, filter, GatewayConnectionRO() );
        }

        public static int GetTableCount( string tableName, string filter, string connString )
        {
            int rowCount = 0;

            string sql = String.Format( "select count(*) As RowsFound from {0} where {1}", tableName, filter );

            try
            {
                SqlConnection con = new SqlConnection( connString );
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
            return GetTableCount( sql, GatewayConnectionRO() );
        }

        public static int DoQueryCount( string sql, string connString )
        {
            DataSet ds = new DataSet();
            int count = 0;
            try
            {
                //use default database connection for sql
                ds = SqlHelper.ExecuteDataset( connString, System.Data.CommandType.Text, sql );

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
            return GetSqlResult( sql, GatewayConnectionRO() );
        }

        public static string GetSqlResult( string sql, string connString )
        {
            DataSet ds = new DataSet();
            DataRow dr;

            string result = "";

            try
            {
                //use default database connection for sql
                ds = SqlHelper.ExecuteDataset( connString, System.Data.CommandType.Text, sql );

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
        public static string ExecuteSql( string sql )
        {
            return ExecuteSql( sql, GatewayConnection() );
        }

        public static string ExecuteSql( string sql, string connString )
        {
            string message = "Unknown";

            try
            {
                SqlHelper.ExecuteNonQuery( connString, CommandType.Text, sql );
                message = "Successful";
            }
            catch ( Exception ex )
            {
                string queryString = GetPublicUrl( HttpContext.Current.Request.QueryString.ToString() );

                LogError( string.Format( "DatabaseManager.ExecuteSql(): \r\nUser: {0}\r\nURL: " + queryString + "\r\nSQL:" + sql + "\r\n" + ex.ToString(), "n/a" ) );

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
            return ExecuteProc(  procedureName, parameters, GatewayConnection() );
        }

        public static DataSet ExecuteProc( string procedureName, SqlParameter[] parameters, string connString )
        {
            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connString, CommandType.StoredProcedure, procedureName, parameters );
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
    }
}
