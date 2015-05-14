using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Net.Mail;
using System.Text;
using System.Xml;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.ApplicationBlocks.Data;

namespace ILPathways.Common
{
    /// <summary>
    /// Summary description for baseDataManager
    /// </summary>
    public abstract class BaseDataManager : IDisposable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseDataManager()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// Implement IDisposable.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        } //

        /// <summary>
        /// Implement IDisposable.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                // Free other state (managed objects).
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
        } //

        /// <summary>
        /// Use C# destructor syntax for finalization code.
        /// </summary>
        ~BaseDataManager()
        {
            // Simply call Dispose(false).
            Dispose( false );
        }
        #region ===== Helper Methods ======
        /// <summary>
        /// handleNulls
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static string handleNulls( DataRow row, string column )
        {
            // TODO - need to handle StrongTypingException
            try
            {

                return ( string ) row[ column ];
            }
            catch
            {
                return "";
            }
        } // end method

        /// <summary>
        /// Return true if column exists in row
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static bool RowHasColumn( SqlDataReader row, string column )
        {
            bool hasColumn = false;
            string colValue = "";
            try
            {
                colValue = row[ column ].ToString();
                hasColumn = true;

            }
            catch ( System.FormatException fex )
            {
                hasColumn = false;

            }
            catch ( Exception ex )
            {
                hasColumn = false;
            }
            return hasColumn;
        } // end method

        /// <summary>
        /// Helper method to retrieve a string column from a row but will ignore missing columns (unlike GetRowColumn)
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static string GetRowPossibleColumn( SqlDataReader row, string column )
        {
            return GetRowPossibleColumn( row, column, "" );
        } // end method

        public static string GetRowPossibleColumn( SqlDataReader row, string column, string defaultValue )
        {
            string colValue = "";

            try
            {
                colValue = row[ column ].ToString();

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                //this method will ignore not found
                colValue = defaultValue;
            }
            return colValue;
        } // end method
        public static int GetRowPossibleColumn( SqlDataReader row, string column, int defaultValue )
        {
            int colValue = 0;

            try
            {
                colValue = Int32.Parse( row[ column ].ToString() );

            }
            catch ( Exception ex )
            {

                colValue = defaultValue;
            }
            return colValue;

        } // end method
        public static bool GetRowPossibleColumn( SqlDataReader row, string column, bool defaultValue )
        {
            bool colValue;

            try
            {
                colValue = Boolean.Parse( row[ column ].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                //this method will ignore not found
                colValue = defaultValue;
            }
            return colValue;

        } // end method

        public static string GetRowPossibleColumn( DataRow row, string column )
        {
            return GetRowPossibleColumn( row, column, "" );
        } // end method
        public static string GetRowPossibleColumn( DataRow row, string column, string defaultValue )
        {
            string colValue = "";

            try
            {
                colValue = row[ column ].ToString();
            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                //this method will ignore not found
                colValue = defaultValue;
            }
            return colValue;

        } // end method
        public static int GetRowPossibleColumn( DataRow row, string column, int defaultValue )
        {
            int colValue = 0;

            try
            {
                colValue = Int32.Parse( row[ column ].ToString() );

            }
            catch ( Exception ex )
            {

                colValue = defaultValue;
            }
            return colValue;

        } // end method
        public static bool GetRowPossibleColumn( DataRow row, string column, bool defaultValue )
        {
            bool colValue;

            try
            {
                colValue = Boolean.Parse( row[ column ].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                //this method will ignore not found
                colValue = defaultValue;
            }
            return colValue;

        } // end method

        public static System.DateTime GetRowPossibleColumn( DataRow row, string column, System.DateTime defaultValue )
        {
            System.DateTime colValue;

            try
            {
                colValue = System.DateTime.Parse( row[ column ].ToString() );
            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                //this method will ignore not found
                colValue = defaultValue;
            }
            return colValue;

        } // end method
        public static System.DateTime GetRowPossibleColumn( SqlDataReader row, string column, System.DateTime defaultValue )
        {
            System.DateTime colValue;

            try
            {
                colValue = System.DateTime.Parse( row[ column ].ToString() );
            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                //this method will ignore not found
                colValue = defaultValue;
            }
            return colValue;

        } // end method



        /// <summary>
        /// Helper method to retrieve a string column from a row while handling invalid values
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="column">Column Name</param>
        /// <returns></returns>
        public static string GetRowColumn( DataRow row, string column )
        {
            return GetRowColumn( row, column, "" );
        } // end method

        /// <summary>
        /// Helper method to retrieve a string column from a row while handling invalid values
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>
        public static string GetRowColumn( DataRow row, string column, string defaultValue )
        {
            string colValue = "";

            try
            {
                colValue = row[ column ].ToString();

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                if ( column.IndexOf( "CUSTOMER_STATUS" ) > -1 )
                {
                    //ignore

                }
                else
                {
                    string queryString = GetWebUrl();
                    string exType = ex.GetType().ToString();
                    LogError( exType + " Exception in GetRowColumn( DataRow row, string column, string defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                }
                colValue = defaultValue;
            }
            return colValue;

        } // end method
        /// <summary>
        /// Helper method to retrieve an int column from a row while handling invalid values
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>
        public static int GetRowColumn( DataRow row, string column, int defaultValue )
        {
            int colValue = 0;

            try
            {
                colValue = Int32.Parse( row[ column ].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                string queryString = GetWebUrl();

                LogError( "Exception in GetRowColumn( DataRow row, string column, int defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                colValue = defaultValue;
                //throw ex;
            }
            return colValue;

        } // end method

        /// <summary>
        /// Helper method to retrieve a bool column from a row while handling invalid values
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>
        public static bool GetRowColumn( DataRow row, string column, bool defaultValue )
        {
            bool colValue;

            try
            {
                colValue = Boolean.Parse( row[ column ].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                string queryString = GetWebUrl();
                LogError( "Exception in GetRowColumn( DataRow row, string column, bool defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                colValue = defaultValue;
                //throw ex;
            }
            return colValue;

        } // end method

        /// <summary>
        /// Helper method to retrieve a DateTime column from a row while handling invalid values
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>      
        public static System.DateTime GetRowColumn( DataRow row, string column, System.DateTime defaultValue )
        {
            System.DateTime colValue;

            try
            {
                colValue = System.DateTime.Parse( row[ column ].ToString() );
            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                string queryString = GetWebUrl();
                LogError( "Exception in GetRowColumn( DataRow row, string column, System.DateTime defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                colValue = defaultValue;
            }
            return colValue;

        } // end method


        /// <summary>
        /// Helper method to retrieve a column from a row while handling invalid values
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>
        public static decimal GetRowColumn( DataRow row, string column, decimal defaultValue )
        {
            decimal colValue = 0;

            try
            {
                colValue = Convert.ToDecimal( row[ column ].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                string queryString = GetWebUrl();
                LogError( "Exception in GetRowColumn( DataRow row, string column, decimal defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                colValue = defaultValue;
            }
            return colValue;

        } // end method

        public static string GetRowColumn( DataRowView row, string column )
        {
            return GetRowColumn( row, column, "" );
        } // end method

        /// <summary>
        /// Helper method to retrieve a string column from a row while handling invalid values
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>
        public static string GetRowColumn( DataRowView row, string column, string defaultValue )
        {
            string colValue = "";

            try
            {
                colValue = row[ column ].ToString();

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                string queryString = GetWebUrl();
                string exType = ex.GetType().ToString();
                LogError( exType + " Exception in GetRowColumn( DataRowView row, string column, string defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                colValue = defaultValue;
            }
            return colValue;

        } // end method

        /// <summary>
        /// Helper method to retrieve a string column from a row while handling invalid values
        /// </summary>
        /// <param name="row">DataRowView</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>
        public static int GetRowColumn( DataRowView row, string column, int defaultValue )
        {
            int colValue = 0;

            try
            {
                colValue = Int32.Parse( row[ column ].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                string queryString = GetWebUrl();
                LogError( "Exception in GetRowColumn( DataRowView row, string column, int defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                colValue = defaultValue;
                //throw ex;
            }
            return colValue;

        } // end method

        /// <summary>
        /// Helper method to retrieve a bool column from a row while handling invalid values
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool GetRowColumn( DataRowView row, string column, bool defaultValue )
        {
            bool colValue;

            try
            {
                colValue = Boolean.Parse( row[ column ].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                string queryString = GetWebUrl();
                LogError( "Exception in GetRowColumn( DataRowView row, string column, bool defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                colValue = defaultValue;
                //throw ex;
            }
            return colValue;

        } // end method

        /// <summary>
        /// Helper method to retrieve a column from a row while handling invalid values
        /// </summary>
        /// <param name="row">DataRowView</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>
        public static decimal GetRowColumn( DataRowView row, string column, decimal defaultValue )
        {
            decimal colValue = 0;

            try
            {
                colValue = Convert.ToDecimal( row[ column ].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                string queryString = GetWebUrl();
                LogError( "Exception in GetRowColumn( DataRowView row, string column, decimal defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                colValue = defaultValue;
            }
            return colValue;

        } // end method

        /// <summary>
        /// Helper method to retrieve a column from a row while handling invalid values
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static string GetRowColumn( SqlDataReader row, string column )
        {
            return GetRowColumn( row, column, "" );
        } // end method
        /// <summary>
        /// Helper method to retrieve a column from a row while handling invalid values
        /// </summary>
        /// <param name="row">SqlDataReader</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>
        public static int GetRowColumn( SqlDataReader row, string column, int defaultValue )
        {
            int colValue = 0;

            try
            {
                colValue = Int32.Parse( row[ column ].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

                //} catch ( System.IndexOutOfRangeException iex )
                //{
                //  DoTrace( 1, "IndexOutOfRangeException in GetRowColumn( SqlDataReader row, string column, int defaultValue ) for column: " + column + ". \r\n" + iex.Message.ToString() );
                //  colValue = defaultValue;
                //throw iex;
            }
            catch ( Exception ex )
            {
                string queryString = GetWebUrl();

                LogError( "Exception in GetRowColumn( SqlDataReader row, string column, int defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                colValue = defaultValue;
            }
            return colValue;

        } // end method

        /// <summary>
        /// Helper method to retrieve a column from a row while handling invalid values
        /// </summary>
        /// <param name="row">SqlDataReader</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>
        public static string GetRowColumn( SqlDataReader row, string column, string defaultValue )
        {
            string colValue = "";
            try
            {
                colValue = row[ column ].ToString();

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                string queryString = GetWebUrl();

                LogError( "Exception in GetRowColumn( SqlDataReader row, string column, string defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                colValue = defaultValue;
            }
            return colValue;
        }

        /// <summary>
        /// Helper method to retrieve a column from a row while handling invalid values
        /// </summary>
        /// <param name="row">SqlDataReader</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>
        public static bool GetRowColumn( SqlDataReader row, string column, bool defaultValue )
        {
            bool colValue;
            try
            {
                colValue = Boolean.Parse( row[ column ].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                string queryString = GetWebUrl();
                LogError( "Exception in GetRowColumn( SqlDataReader row, string column, bool defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                colValue = defaultValue;
            }

            return colValue;
        }//

        /// <summary>
        /// Helper method to retrieve a column from a row while handling invalid values
        /// </summary>
        /// <param name="row">SqlDataReader</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>      
        public static System.DateTime GetRowColumn( SqlDataReader row, string column, System.DateTime defaultValue )
        {
            System.DateTime colValue;

            try
            {
                colValue = System.DateTime.Parse( row[ column ].ToString() );
            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                string queryString = GetWebUrl();
                LogError( "Exception in GetRowColumn( SqlDataReader row, string column, System.DateTime defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                colValue = defaultValue;
            }
            return colValue;



        } // end method

        /// <summary>
        /// Helper method to retrieve a column from a row while handling invalid values
        /// </summary>
        /// <param name="row">SqlDataReader</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>
        public static decimal GetRowColumn( SqlDataReader row, string column, decimal defaultValue )
        {
            decimal colValue = 0;

            try
            {
                colValue = Convert.ToDecimal( row[ column ].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                string queryString = GetWebUrl();
                LogError( "Exception in GetRowColumn( SqlDataReader row, string column, decimal defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                colValue = defaultValue;
            }
            return colValue;

        } // end method


        /// <summary>
        /// Helper method to set Column in data row.  Useful for when working with rows in a DataTable.
        /// </summary>
        /// <param name="dr">DataRow</param>
        /// <param name="column">string</param>
        /// <param name="value">int</param>
        public static void SetRowColumn( DataRow dr, string column, int value )
        {
            dr[ column ] = value;
        }

        /// <summary>
        /// Helper method to set Column in data row.  Useful for when working with rows in a DataTable.
        /// </summary>
        /// <param name="dr">DataRow</param>
        /// <param name="column">string</param>
        /// <param name="value">bool</param>
        public static void SetRowColumn( DataRow dr, string column, bool value )
        {
            dr[ column ] = value;
        }

        /// <summary>
        /// Helper method to set Column in data row.  Useful for when working with rows in a DataTable.
        /// </summary>
        /// <param name="dr">DataRow</param>
        /// <param name="column">string</param>
        /// <param name="value">string</param>
        public static void SetRowColumn( DataRow dr, string column, string value )
        {
            dr[ column ] = value;
        }

        private static string GetWebUrl()
        {
            string queryString = "n/a";

            if ( HttpContext.Current != null && HttpContext.Current.Request != null )
                queryString = HttpContext.Current.Request.RawUrl.ToString();

            return queryString;
        }
        public static string GetUserIPAddress()
        {
            string ip = "unknown";
            try
            {
                ip = HttpContext.Current.Request.ServerVariables[ "HTTP_X_FORWARDED_FOR" ];
                if ( ip == null || ip == "" || ip.ToLower() == "unknown" )
                {
                    ip = HttpContext.Current.Request.ServerVariables[ "REMOTE_ADDR" ];
                }
            }
            catch ( Exception ex )
            {

            }

            return ip;
        } //
        /// <summary>
        /// Check if column in passed row is null
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="column">Column name</param>
        /// <returns>True if null</returns>
        public static bool IsDbNull( DataRow row, string column )
        {
            // If the column contains a value, then return true, else false
            object value = row[ column ];
            if ( value == System.DBNull.Value )
                return true;
            else
                return false;
        } // end method

        /// <summary>
        /// Convert SQL DataReader to SQL DataSet
        /// </summary>
        /// <param name="dr">A DataReader</param>
        /// <returns>the datareader content as a dataset</returns>
        public static DataSet ConvertDataReaderToDataSet( SqlDataReader dr )
        {
            DataSet ds = new DataSet();

            do
            {
                // Create new Data Table
                DataTable schemaTable = dr.GetSchemaTable();
                DataTable dt = new DataTable();

                if ( schemaTable != null )
                {
                    for ( int i = 0 ; i < schemaTable.Rows.Count ; i++ )
                    {
                        DataRow dataRow = schemaTable.Rows[ i ];
                        string columnName = ( string ) dataRow[ "ColumnName" ];
                        DataColumn column = new DataColumn( columnName, ( Type ) dataRow[ "DataType" ] );
                        dt.Columns.Add( column );
                    }
                    ds.Tables.Add( dt );
                    // Fill data table we just created
                    while ( dr.Read() )
                    {
                        DataRow dataRow = dt.NewRow();
                        for ( int i = 0 ; i < dr.FieldCount ; i++ )
                        {
                            dataRow[ i ] = dr.GetValue( i );
                        }
                        dt.Rows.Add( dataRow );
                    }
                }
                /*               else {
                                        DataColumn column = new DataColumn("RowsAffected");
                                        dt.Columns.Add(column);
                                        ds.Tables.Add(dt);
                                        DataRow dataRow=dt.NewRow();
                                        dataRow[0]=dr.RecordsAffected;
                                        dt.Rows.Add(dataRow);
                                } */
            }
            while ( dr.NextResult() );
            return ds;
        }

        /// <summary>
        /// Convert an XmlNode to a DataSet
        /// </summary>
        /// <param name="node"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static DataSet ConvertXmlNodeToDataSet( XmlNode node, ref string status )
        {
            status = "successful";
            DataSet ds = new DataSet();
            if ( node == null || node.InnerXml.Length == 0 )
            {
                status = "ERROR: Nothing to convert";
                return null;
            }
            XmlTextReader reader = new XmlTextReader( node.OuterXml, XmlNodeType.Element, null );
            ds.ReadXml( reader );

            if ( ds.HasErrors )
            {
                status = "ERROR: Errors were found in the DataSet.";
                return null;
            }
            return ds;
        }

        /// <summary>
        /// Retrieve a data table from passed sql statement and connection string
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        protected static DataTable GetDataTable( string sql, string conn )
        {
            System.Data.SqlClient.SqlConnection cn = new System.Data.SqlClient.SqlConnection( conn );

            return GetDataTable( sql, cn );

        }//

        /// <summary>
        /// Retrieve a data table from passed sql statement and connection object
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        protected static DataTable GetDataTable( string sql, SqlConnection conn )
        {

            DataTable dt = new DataTable();
            try
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter( sql, conn );
                da.Fill( dt );
                conn.Close();
            }
            catch ( Exception )
            {
                throw;
            }
            finally
            {
                if ( conn.State == ConnectionState.Open ) { conn.Close(); }
            }
            return dt;

        }//


        /// <summary>
        /// Set a message to return to calling method as a DataSet - typically an error during processing
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected static DataSet SetDataSetMessage( string message )
        {

            DataSet ds = new DataSet( "ErrorMessage" );
            DataTable dt = new DataTable();
            //
            dt = ds.Tables.Add( "resultTable" );
            dt.Columns.Add( "RESULTS_TABLE" );

            DataRow newRow = dt.NewRow();
            newRow[ "RESULTS_TABLE" ] = message;
            dt.Rows.Add( newRow );

            return ds;
        } //


        /// <summary>
        /// Returns info about a Property
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static PropertyInfo[] GetPropertyInfo( Object data )
        {
            System.Type type = data.GetType();
            if ( type.Name.IndexOf( '[' ) > 0 )
            {
                object[] obj = ( object[] ) data;
                System.Type[] type2 = System.Type.GetTypeArray( obj );
                return type2[ 0 ].GetProperties();
            }
            else
            {
                return data.GetType().GetProperties();
            }
        }

        /// <summary>
        /// Creates a DataTable based on items in an object
        /// </summary>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static DataTable CreateTable( Object data, string name )
        {
            DataTable dt = new DataTable( name );
            foreach ( PropertyInfo pi in GetPropertyInfo( data ) )
            {
                dt.Columns.Add( pi.Name.ToString() );
            }
            return dt;
        }

        /// <summary>
        /// Fills previously created DataTable based on items in a collection.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static DataTable FillData( DataTable dt, Object data )
        {
            object[] arrArray = ( object[] ) data;
            //      string s = type.Name;
            //type arrArray = (type)data;
            //      ArrayList arrArray = (ArrayList)data;
            DataRow dr;
            for ( int i = 0 ; i < arrArray.Length ; i++ )
            {
                dr = dt.NewRow();
                object obj = arrArray[ i ];
                foreach ( PropertyInfo pi in GetPropertyInfo( obj ) )
                {
                    object o = pi.GetValue( obj, null );
                    dr[ pi.Name ] = o;
                }
                dt.Rows.Add( dr );
            }
            return dt;
        }

        /// <summary>
        /// Converts a collection to a DataSet.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DataSet CollectionToDataSet( Object data, string name )
        {
            DataSet ds = new DataSet( name );
            DataTable dt = CreateTable( data, name );
            dt = FillData( dt, data );
            ds.Tables.Add( dt );
            return ds;
        }

        /// <summary>
        /// <para>Write CSV file directly to the user's browser (as a downloadable file), bypassing the need to export to file then download the file.</para>
        /// <para>Note this automatically clears whatever has already been prepared for sending to the browser!!</para>
        /// <para>Copied from http://www.dotnetspider.com/resources/701-Export-Data-CSV-Excel.aspx 7/1/2010</para>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="response"></param>
        /// <param name="writeHeader"></param>
        public static void ExportCSV( DataTable dt, HttpResponse response, bool writeHeader )
        {
            response.Clear();
            response.Buffer = true;
            response.ContentType = "text/csv";
            response.AddHeader( "Content-disposition", "attachment; filename=Export.csv" );
            response.Charset = "";
            if ( writeHeader )
            {
                string[] arr = new string[ dt.Columns.Count ];
                for ( int i = 0 ; i < dt.Columns.Count ; i++ )
                {
                    arr[ i ] = dt.Columns[ i ].ColumnName;
                    arr[ i ] = GetWriteableValue( arr[ i ] );
                }
                response.Output.WriteLine( string.Join( ",", arr ) );
            }

            foreach ( DataRow dr in dt.Rows )
            {
                string[] dataArr = new string[ dt.Columns.Count ];
                for ( int i = 0 ; i < dt.Columns.Count ; i++ )
                {
                    object o = dr[ i ];
                    dataArr[ i ] = GetWriteableValue( o );
                }
                response.Output.WriteLine( string.Join( ",", dataArr ) );
            }

            // Stop processing!!! We don't want to output the rest of the page, which is what happens without the following statement.
            response.End();
        }

        /// <summary>
        /// used for ExportCSV method
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static string GetWriteableValue( object o )
        {
            if ( o == null || o == Convert.DBNull )
            {
                return "";
            }
            else if ( o.ToString().IndexOf( "," ) == -1 )
            {
                return o.ToString();
            }
            else
            {
                return "\"" + o.ToString() + "\"";
            }
        }

        #endregion

        #region Common Utility Methods
        /// <summary>
        /// Correct imbedded apostrophes - typically prior to a direct SQL statement
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static string HandleApostrophes( string strValue )
        {

            if ( strValue.IndexOf( "'" ) > -1 )
            {
                strValue = strValue.Replace( "'", "''" );
            }
            if ( strValue.IndexOf( "''''" ) > -1 )
            {
                strValue = strValue.Replace( "''''", "''" );
            }

            return strValue;
        }


        /// <summary>
        /// Do a check to ensure a parameter value doesn't exceed length defined in the related stored proc
        /// </summary>
        /// <param name="stringToTest"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        protected static string HandleParameterMaximums( string caller, string stringToTest, int maxLength )
        {
            //caller
            string traceTemplate = "***** In method: {0}, truncating parameter [{1}] from length of {2} to length of {3}. \n\rLost string: [{4}]";

            if ( stringToTest.Length > maxLength )
            {
                string truncated = stringToTest.Substring( maxLength );

                DoTrace( 1, String.Format( traceTemplate, caller, stringToTest, stringToTest.Length, maxLength, truncated ) );
                return stringToTest.Substring( 0, maxLength );
            }
            else
            {
                return stringToTest;
            }

        } //

        /// <summary>
        /// Format a string item for a search string (where)
        /// </summary>
        /// <param name="sqlWhere"></param>
        /// <param name="colName"></param>
        /// <param name="colValue"></param>
        /// <param name="booleanOperator"></param>
        /// <returns></returns>
        public static string FormatSearchItem( string sqlWhere, string colName, string colValue, string booleanOperator )
        {
            string item = "";
            string boolean = " ";

            if ( colValue.Length == 0 )
                return "";

            if ( sqlWhere.Length > 0 )
            {
                boolean = " " + booleanOperator + " ";
            }
            //allow asterisks
            colValue = colValue.Replace( "*", "%" );

            if ( colValue.IndexOf( "%" ) > -1 )
            {
                item = boolean + " (" + colName + " like '" + colValue + "') ";
            }
            else
            {
                item = boolean + " (" + colName + " = '" + colValue + "') ";
            }

            return item;

        }	// End method

        /// <summary>
        /// Format an integer item for a search string (where)
        /// </summary>
        /// <param name="sqlWhere"></param>
        /// <param name="colName"></param>
        /// <param name="colValue"></param>
        /// <param name="booleanOperator"></param>
        /// <returns></returns>
        public static string FormatSearchItem( string sqlWhere, string colName, int colValue, string booleanOperator )
        {
            string item = "";
            string boolean = " ";

            if ( sqlWhere.Length > 0 )
            {
                boolean = " " + booleanOperator + " ";
            }

            item = boolean + " (" + colName + " = " + colValue + ") ";

            return item;

        }	// End method

        /// <summary>
        /// Format an item for a search string (where)
        /// </summary>
        /// <param name="sqlWhere"></param>
        /// <param name="filter"></param>
        /// <param name="booleanOperator"></param>
        /// <returns></returns>
        public static string FormatSearchItem( string sqlWhere, string filter, string booleanOperator )
        {
            string item = "";
            string boolean = " ";

            if ( filter.Trim().Length == 0 )
                return "";

            if ( sqlWhere.Length > 0 )
            {
                boolean = " " + booleanOperator + " ";
            }

            item = boolean + " (" + filter + ") ";

            return item;

        }	// End method

        #endregion

        #region General Utility Routines
        /// <summary>
        /// populate a dropdown list using generic list of CodeItems
        /// </summary>
        /// <param name="list"></param>
        /// <param name="items"></param>
        /// <param name="dataValueField"></param>
        /// <param name="dataTextField"></param>
        /// <param name="selectTitle"></param>
        public static void PopulateList( DropDownList list, List<CodeItem> items, string dataValueField, string dataTextField, string selectTitle )
        {
            try
            {
                //clear current entries
                list.Items.Clear();

                if ( items.Count > 0 )
                {
                    int count = items.Count;
                    if ( selectTitle.Length > 0 )
                    {
                        // add select row
                        CodeItem hdr = new CodeItem();
                        hdr.Id = 0;
                        hdr.Title = selectTitle;
                        items.Insert( 0, hdr );
                    }
                    list.DataSource = items;
                    list.DataValueField = dataValueField;
                    list.DataTextField = dataTextField;
                    list.DataBind();
                    list.Enabled = true;
                    if ( selectTitle.Length > 0 )
                        list.SelectedIndex = 0;
                }
                else
                {
                    list.Items.Add( new ListItem( "No Selections Available", "" ) );
                    list.Enabled = false;
                }

            }
            catch ( Exception ex )
            {
                LogError( ex, "BaseDataManager.PopulateList( DropDownList list, DataSet ds, string " + dataValueField + ", string " + dataTextField + ", string selectTitle )" );
            }
        }

        public static void PopulateList( CheckBoxList list, List<CodeItem> items, string dataValueField, string dataTextField, string selectTitle )
        {
            try
            {
                //clear current entries
                list.Items.Clear();

                if ( items.Count > 0 )
                {
                    int count = items.Count;
                    if ( selectTitle.Length > 0 )
                    {
                        // add select row
                        CodeItem hdr = new CodeItem();
                        hdr.Id = 0;
                        hdr.Title = selectTitle;
                        items.Insert( 0, hdr );
                    }
                    list.DataSource = items;
                    list.DataValueField = dataValueField;
                    list.DataTextField = dataTextField;
                    list.DataBind();
                    list.Enabled = true;
                    if ( selectTitle.Length > 0 )
                        list.SelectedIndex = 0;
                }
                else
                {
                    list.Items.Add( new ListItem( "No Selections Available", "" ) );
                    list.Enabled = false;
                }

            }
            catch ( Exception ex )
            {
                LogError( ex, "BaseDataManager.PopulateList( CheckBoxList list, DataSet ds, string " + dataValueField + ", string " + dataTextField + ", string selectTitle )" );
            }
        }

        public static void PopulateList( RadioButtonList list, List<CodeItem> items, string dataValueField, string dataTextField, string selectTitle )
        {
            try
            {
                //clear current entries
                list.Items.Clear();

                if ( items.Count > 0 )
                {
                    int count = items.Count;
                    if ( selectTitle.Length > 0 )
                    {
                        // add select row
                        CodeItem hdr = new CodeItem();
                        hdr.Id = 0;
                        hdr.Title = selectTitle;
                        items.Insert( 0, hdr );
                    }
                    list.DataSource = items;
                    list.DataValueField = dataValueField;
                    list.DataTextField = dataTextField;
                    list.DataBind();
                    list.Enabled = true;
                    if ( selectTitle.Length > 0 )
                        list.SelectedIndex = 0;
                }
                else
                {
                    list.Items.Add( new ListItem( "No Selections Available", "" ) );
                    list.Enabled = false;
                }

            }
            catch ( Exception ex )
            {
                LogError( ex, "BaseDataManager.PopulateList( CheckBoxList list, DataSet ds, string " + dataValueField + ", string " + dataTextField + ", string selectTitle )" );
            }
        }

        /// <summary>
        /// Fill a list using passed dataset
        /// </summary>
        /// <param name="list">DropDownList</param>
        /// <param name="ds"></param>
        /// <param name="dataValueField"></param>
        /// <param name="dataTextField"></param>
        public static void PopulateList( DropDownList list, DataSet ds, string dataValueField, string dataTextField )
        {

            PopulateList( list, ds, dataValueField, dataTextField, "" );

        }

        /// <summary>
        /// Fill a list using passed dataset, also insert first prompt row (asking user to select a row)
        /// </summary>
        /// <param name="list">DropDownList</param>
        /// <param name="ds"></param>
        /// <param name="dataValueField"></param>
        /// <param name="dataTextField"></param>
        /// <param name="selectTitle"></param>
        public static void PopulateList( DropDownList list, DataSet ds, string dataValueField, string dataTextField, string selectTitle )
        {
            try
            {
                //clear current entries
                list.Items.Clear();

                if ( DoesDataSetHaveRows( ds ) )
                {
                    int count = ds.Tables[ 0 ].Rows.Count;
                    if ( selectTitle.Length > 0 )
                    {
                        // add select row
                        AddEntryToTable( ds.Tables[ 0 ], 0, selectTitle, dataValueField, dataTextField );
                    }
                    list.DataSource = ds;
                    list.DataValueField = dataValueField;
                    list.DataTextField = dataTextField;
                    list.DataBind();
                    list.Enabled = true;
                    if ( selectTitle.Length > 0 )
                        list.SelectedIndex = 0;
                }
                else
                {
                    list.Items.Add( new ListItem( "No Selections Available", "" ) );
                    list.Enabled = false;
                }

            }
            catch ( Exception ex )
            {
                LogError( ex, "BaseDataManager.PopulateList( DropDownList list, DataSet ds, string " + dataValueField + ", string " + dataTextField + ", string selectTitle )" );
            }
        }

        /// <summary>
        /// Fill a list using passed dataset, also insert first prompt row (asking user to select a row)
        /// </summary>
        /// <param name="list">DropDownList</param>
        /// <param name="ds"></param>
        /// <param name="dataValueField"></param>
        /// <param name="dataTextField"></param>
        /// <param name="selectTitle"></param>
        /// <param name="initialValue">value used for item added to list</param>
        public static void PopulateList( DropDownList list, DataSet ds, string dataValueField, string dataTextField, string selectTitle, string initialValue )
        {
            try
            {
                //clear current entries
                list.Items.Clear();

                if ( ds != null && ds.Tables.Count > 0 )
                {
                    if ( ds.Tables[ 0 ].Rows.Count > 0 )
                    {
                        // add select row
                        AddEntryToTable( ds.Tables[ 0 ], initialValue, selectTitle, dataValueField, dataTextField );

                        list.DataSource = ds;
                        list.DataValueField = dataValueField;
                        list.DataTextField = dataTextField;
                        list.DataBind();
                        list.Enabled = true;
                    }
                    else
                    {
                        DataTable tbl = new DataTable();
                        tbl.Columns.Add( dataValueField, typeof( string ) );
                        tbl.Columns.Add( dataTextField, typeof( string ) );

                        BaseDataManager.AddEntryToTable( tbl, 0, "No selections available", dataValueField, dataTextField );
                        list.DataSource = tbl;
                        list.DataValueField = dataValueField;
                        list.DataTextField = dataTextField;
                        list.DataBind();

                        list.Enabled = false;
                    }
                }
            }
            catch ( Exception ex )
            {
                LogError( ex, "BaseDataManager.PopulateList( DropDownList list, DataSet ds, string " + dataValueField + ", string " + dataTextField + ", string selectTitle )" );
            }
        }

        /// <summary>
        /// Fill a list using passed DataTable, also insert first prompt row (asking user to select a row)
        /// </summary>
        /// <param name="list"></param>
        /// <param name="dt"></param>
        /// <param name="dataValueField"></param>
        /// <param name="dataTextField"></param>
        /// <param name="selectTitle"></param>
        public static void PopulateDVList( DropDownList list, DataTable dt, string dataValueField, string dataTextField, string selectTitle )
        {
            try
            {
                //clear current entries
                list.Items.Clear();

                if ( dt != null )
                {
                    // add select row
                    AddEntryToTable( dt, 0, selectTitle, dataValueField, dataTextField );

                    list.DataSource = dt;
                    list.DataValueField = dataValueField;
                    list.DataTextField = dataTextField;
                    list.DataBind();
                }

            }
            catch ( Exception ex )
            {
                LogError( ex, "BaseDataManager.PopulateList( DropDownList list, DataTable dt, string " + dataValueField + ", string " + dataTextField + ", string selectTitle )" );
            }
        }

        /// <summary>
        /// Fill a list using passed dataset, also insert first prompt row (asking user to select a row)
        /// </summary>
        /// <param name="list">ListBox</param>
        /// <param name="ds"></param>
        /// <param name="dataValueField"></param>
        /// <param name="dataTextField"></param>
        /// <param name="selectTitle"></param>
        public static void PopulateList( ListBox list, DataSet ds, string dataValueField, string dataTextField, string selectTitle )
        {
            try
            {
                //clear current entries
                list.Items.Clear();

                if ( ds != null && ds.Tables.Count > 0 )
                {
                    // add select row
                    AddEntryToTable( ds.Tables[ 0 ], 0, selectTitle, dataValueField, dataTextField );

                    list.DataSource = ds;
                    list.DataValueField = dataValueField;
                    list.DataTextField = dataTextField;
                    list.DataBind();
                }

            }
            catch ( Exception ex )
            {
                LogError( ex, "BaseDataManager.PopulateList( DropDownList list, DataSet ds, string " + dataValueField + ", string " + dataTextField + ", string selectTitle )" );
            }
        }

        /// <summary>
        /// Add an entry to the beginning of a Data Table
        /// </summary>
        /// <param name="tbl"></param>
        /// <param name="displayValue"></param>
        public static void AddEntryToTable( DataTable tbl, string displayValue )
        {
            DataRow r = tbl.NewRow();
            r[ 0 ] = displayValue;
            tbl.Rows.InsertAt( r, 0 );
        }

        /// <summary>
        /// Add an entry to the beginning of a Data Table. Uses a default key name of "id" and display column of "name"
        /// </summary>
        /// <param name="tbl"></param>
        /// <param name="keyValue"></param>
        /// <param name="displayValue"></param>
        public static void AddEntryToTable( DataTable tbl, int keyValue, string displayValue )
        {
            //DataRow r = tbl.NewRow();
            //r[ 0 ] = id;
            //r[ 1 ] = displayValue;
            //tbl.Rows.InsertAt( r, 0 );

            AddEntryToTable( tbl, keyValue, displayValue, "id", "name" );

        }

        /// <summary>
        /// Add an entry to the beginning of a Data Table. Uses a default key name of "id" and display column of "name"
        /// </summary>
        /// <param name="tbl"></param>
        /// <param name="keyValue"></param>
        /// <param name="displayValue"></param>
        /// <param name="keyName"></param>
        /// <param name="displayName"></param>
        public static void AddEntryToTable( DataTable tbl, int keyValue, string displayValue, string keyName, string displayName )
        {
            DataRow r = tbl.NewRow();
            r[ keyName ] = keyValue;
            r[ displayName ] = displayValue;
            tbl.Rows.InsertAt( r, 0 );

        }
        /// <summary>
        /// Add an entry to the beginning of a Data Table. Uses the provided key name and display column
        /// </summary>
        /// <param name="tbl"></param>
        /// <param name="keyValue"></param>
        /// <param name="displayValue"></param>
        /// <param name="keyName"></param>
        /// <param name="displayName"></param>
        public static void AddEntryToTable( DataTable tbl, string keyValue, string displayValue, string keyName, string displayName )
        {
            DataRow r = tbl.NewRow();
            r[ keyName ] = keyValue;
            r[ displayName ] = displayValue;
            tbl.Rows.InsertAt( r, 0 );

        }

        /// <summary>
        /// Check is dataset is valid and has at least one table with at least one row
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static bool DoesDataSetHaveRows( DataSet ds )
        {

            try
            {
                if ( ds != null && ds.Tables.Count > 0 && ds.Tables[ 0 ].Rows.Count > 0 )
                    return true;
                else
                    return false;
            }
            catch
            {

                return false;
            }
        }//

        #endregion

        #region Logging - keep in sync with methods in UtilityManager until cleaned up or replaced
        /// <summary>
        /// Format an exception and message, and then log it
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="message">Additional message regarding the exception</param>
        public static void LogError( Exception ex, string message )
        {

            // string user = "";
            string sessionId = "unknown";
            string remoteIP = "unknown";
            string path = "unknown";
            string queryString = "unknown";
            string mcmsUrl = "unknown";
            string parmsString = "";

            try
            {
                sessionId = HttpContext.Current.Session.SessionID.ToString();
                remoteIP = HttpContext.Current.Request.ServerVariables[ "REMOTE_HOST" ];

                string serverName = GetAppKeyValue( "serverName", HttpContext.Current.Request.ServerVariables[ "LOCAL_ADDR" ] );
                path = serverName + HttpContext.Current.Request.Path;
                queryString = GetWebUrl();
                mcmsUrl = GetPublicUrl( queryString );

                mcmsUrl = HttpContext.Current.Server.UrlDecode( mcmsUrl );
                if ( mcmsUrl.IndexOf( "?" ) > -1 )
                {
                    parmsString = mcmsUrl.Substring( mcmsUrl.IndexOf( "?" ) + 1 );
                    mcmsUrl = mcmsUrl.Substring( 0, mcmsUrl.IndexOf( "?" ) );
                }

                //user = UserManager.GetCurrentUserid();
            }
            catch
            {
                //eat any additional exception
            }

            try
            {
                string errMsg = message +
                    "\r\nType: " + ex.GetType().ToString() +
                    "\r\nSession Id - " + sessionId + "____IP - " + remoteIP +
                    "\r\nException: " + ex.Message.ToString() +
                    "\r\nStack Trace: " + ex.StackTrace.ToString() +
                    "\r\nServer\\Template: " + path +
                    "\r\nUrl: " + mcmsUrl;

                if ( parmsString.Length > 0 )
                    errMsg += "\r\nParameters: " + parmsString;

                LogError( errMsg );
            }
            catch
            {
                //eat any additional exception
            }

        } //


        /// <summary>
        /// Write the message to the log file.
        /// </summary>
        /// <remarks>
        /// The message will be appended to the log file only if the flag "logErrors" (AppSetting) equals yes.
        /// The log file is configured in the web.config, appSetting: "error.log.path"
        /// </remarks>
        /// <param name="message">Message to be logged.</param>
        public static void LogError( string message )
        {

            if ( GetAppKeyValue( "notifyOnException", "no" ).ToLower() == "yes" )
            {
                LogError( message, true );
            }
            else
            {
                LogError( message, false );
            }

        } //

        /// <summary>
        /// Write the message to the log file.
        /// </summary>
        /// <remarks>
        /// The message will be appended to the log file only if the flag "logErrors" (AppSetting) equals yes.
        /// The log file is configured in the web.config, appSetting: "error.log.path"
        /// </remarks>
        /// <param name="message">Message to be logged.</param>
        /// <param name="notifyAdmin"></param>
        public static void LogError( string message, bool notifyAdmin )
        {
            if ( GetAppKeyValue( "logErrors" ).ToString().Equals( "yes" ) )
            {
                try
                {
                    //string logFile  = GetAppKeyValue("error.log.path","C:\\VOS_LOGS.txt");
                    string datePrefix = System.DateTime.Today.ToString( "u" ).Substring( 0, 10 );
                    string logFile = GetAppKeyValue( "path.error.log", "C:\\VOS_LOGS.txt" );
                    string outputFile = logFile.Replace( "[date]", datePrefix );

                    StreamWriter file = File.AppendText( outputFile );
                    file.WriteLine( DateTime.Now + ": " + message );
                    file.WriteLine( "---------------------------------------------------------------------" );
                    file.Close();

                    if ( notifyAdmin )
                    {
                        if ( GetAppKeyValue( "notifyOnException", "no" ).ToLower() == "yes" )
                        {
                            NotifyAdmin( "Isle Exception encountered", message );
                        }
                    }
                }
                catch
                {
                    //eat any additional exception
                }
            }
        } //
        /// <summary>
        /// Sends an email message to the system administrator
        /// </summary>
        /// <param name="subject">Email subject</param>
        /// <param name="message">Email message</param>
        /// <returns>True id message was sent successfully, otherwise false</returns>
        public static bool NotifyAdmin( string subject, string message )
        {

            //avoid infinite loop by ensuring this method didn't generate the exception
            string emailTo = GetAppKeyValue( "systemAdminEmail", "mparsons@siuccwd.com" );
            string emailFrom = GetAppKeyValue( "systemNotifyFromEmail", "TheWatcher@siuccwd.com" );
            string cc = "";

            //work on implementing some specific routing based on error type
            if ( message.IndexOf( "SqlClient.SqlConnection.Open()" ) > -1 )
                cc = "jgrimmer@siuccwd.com";

            if ( message.IndexOf( "BaseDataManager.NotifyAdmin" ) > -1 )
            {
                //skip may be error on send

            }
            else
            {
                message = message.Replace( "\r", "<br/>" );

                MailMessage email = new MailMessage( emailFrom, emailTo );

                //try to make subject more specific
                //if: Isle Exception encountered, try to insert type
                if ( subject.IndexOf( "Isle Exception" ) > -1 )
                {
                    subject = FormatExceptionSubject( subject, message );
                }
                email.Subject = subject;

                if ( message.IndexOf( "Type:" ) > 0 )
                {
                    int startPos = message.IndexOf( "Type:" );
                    int endPos = message.IndexOf( "Error Message:" );
                    if ( endPos > startPos )
                    {
                        subject += " - " + message.Substring( startPos, endPos - startPos );
                    }
                }
                //if ( cc.Length > 0 )
                //{
                //  MailAddress ma = new MailAddress( cc );
                //  email.CC.Add( ma );
                //}

                email.Body = DateTime.Now + "<br>" + message.Replace( "\n\r", "<br>" );
                email.Body = email.Body.Replace( "\r\n", "<br>" );
                email.Body = email.Body.Replace( "\n", "<br>" );
                email.Body = email.Body.Replace( "\r", "<br>" );
                //SmtpMail.SmtpServer = GetAppKeyValue("smtpEmail");
                SmtpClient smtp = new SmtpClient( GetAppKeyValue( "smtpEmail" ) );
                email.IsBodyHtml = true;
                //email.BodyFormat = MailFormat.Html;
                try
                {
                    if ( GetAppKeyValue( "sendEmailFlag", "false" ) == "TRUE" )
                    {
                        smtp.Send( email );
                        //SmtpMail.Send(email);
                    }

                    if ( GetAppKeyValue( "logAllEmail", "no" ) == "yes" )
                    {
                        LogEmail( 1, email );
                        //DoTrace(1,"    ***** Email Log ***** "
                        //  + "\r\nFrom:" + fromEmail
                        //  + "\r\nTo:  " + toEmail
                        //  + "\r\nCC:(" + CC + ") BCC:(" + BCC + ") "
                        //  + "\r\nSubject:" + subject
                        //  + "\r\nMessage:" + message
                        //  + "\r\n============================================");
                    }
                    return true;
                }
                catch ( Exception exc )
                {
                    LogError( "BaseDataManager.NotifyAdmin(): Error while attempting to send:"
                        + "\r\nSubject:" + subject + "\r\nMessage:" + message
                        + "\r\nError message: " + exc.ToString(), false );
                }
            }

            return false;
        } //

        /// <summary>
        /// Log email message - for future resend/reviews
        /// </summary>
        /// <param name="level"></param>
        /// <param name="email"></param>
        public static void LogEmail( int level, MailMessage email )
        {

            string msg = "";
            int appTraceLevel = 0;
            //bool useBriefFormat = true;

            try
            {
                appTraceLevel = GetAppKeyValue( "appTraceLevel", 1 );

                //Allow if the requested level is <= the application thresh hold
                if ( level <= appTraceLevel )
                {

                    msg = "\n=============================================================== ";
                    msg += "\nDate:	" + System.DateTime.Now.ToString();
                    msg += "\nFrom:	" + email.From.ToString();
                    msg += "\nTo:		" + email.To.ToString();
                    msg += "\nCC:		" + email.CC.ToString();
                    msg += "\nBCC:  " + email.Bcc.ToString();
                    msg += "\nSubject: " + email.Subject.ToString();
                    msg += "\nMessage: " + email.Body.ToString();
                    msg += "\n=============================================================== ";

                    string datePrefix = System.DateTime.Today.ToString( "u" ).Substring( 0, 10 );
                    string logFile = GetAppKeyValue( "path.email.log", "C:\\VOS_LOGS.txt" );
                    string outputFile = logFile.Replace( "[date]", datePrefix );

                    StreamWriter file = File.AppendText( outputFile );

                    file.WriteLine( msg );
                    file.Close();

                }
            }
            catch
            {
                //ignore errors
            }

        }

        /// <summary>
        /// Handle trace requests - typically during development, but may be turned on to track code flow in production.
        /// </summary>
        /// <param name="message">Trace message</param>
        /// <remarks>This is a helper method that defaults to a trace level of 10</remarks>
        public static void DoTrace( string message )
        {
            //default level to 10
            DoTrace( 10, message );

        }

        /// <summary>
        /// Handle trace requests - typically during development, but may be turned on to track code flow in production.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public static void DoTrace( int level, string message )
        {
            //TODO: Future provide finer control at the control level
            string msg = "";
            int appTraceLevel = 0;
            //bool useBriefFormat = true;

            try
            {
                appTraceLevel = GetAppKeyValue( "appTraceLevel", 1 );

                //Allow if the requested level is <= the application thresh hold
                if ( level <= appTraceLevel )
                {
                    string usingBriefFormat = GetAppKeyValue( "usingBriefFormat", "yes" );
                    if ( usingBriefFormat == "yes" )
                    {
                        //if (useBriefFormat) {
                        msg = "\n " + System.DateTime.Now.ToString() + " - " + message;

                    }
                    else
                    {
                        msg = "\n======================= Trace ================================= ";
                        msg += "\nTime: " + System.DateTime.Now.ToString();
                        msg += "\nTrace: " + message;
                        msg += "\n=============================================================== ";
                    }
                    string datePrefix = System.DateTime.Today.ToString( "u" ).Substring( 0, 10 );
                    string logFile = GetAppKeyValue( "path.trace.log", "C:\\VOS_LOGS.txt" );
                    string outputFile = logFile.Replace( "[date]", datePrefix );

                    StreamWriter file = File.AppendText( outputFile );

                    file.WriteLine( msg );
                    file.Close();

                }
            }
            catch
            {
                //ignore errors
            }

        }

        /// <summary>
        /// Attempt to format a more meaningful subject for an exception related email
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        public static string FormatExceptionSubject( string subject, string message )
        {
            string work = "";

            try
            {
                int start = message.IndexOf( "Type:" );
                int end = message.IndexOf( "User:" );
                if ( start > -1 && end > start )
                {
                    work = message.Substring( start, end - start );
                    //remove line break
                    work = work.Replace( "\r\n", "" );
                    work = work.Replace( "<br>", "" );
                    work = work.Replace( "Type:", "Exception:" );
                    if ( message.IndexOf( "Caught in Application_Error event" ) > -1 )
                    {
                        work = work.Replace( "Exception:", "Unhandled Exception:" );
                    }

                }
                if ( work.Length == 0 )
                {
                    work = subject;
                }
            }
            catch
            {
                work = subject;
            }

            return work;
        } //
        /// <summary>
        /// Return the public version of the current MCMS url - removes MCMS specific parameters
        /// </summary>
        public static string GetPublicUrl( string url )
        {
            string publicUrl = "";

            //find common parms
            int nrmodePos = url.ToLower().IndexOf( "nrmode" );
            int urlStartPos = url.ToLower().IndexOf( "nroriginalurl" );
            int urlEndPos = url.ToLower().IndexOf( "&nrcachehint" );

            if ( urlStartPos > 0 && urlEndPos > urlStartPos )
            {
                publicUrl = url.Substring( urlStartPos + 14, urlEndPos - ( urlStartPos + 14 ) );
            }
            else
            {
                //just take everything??
                publicUrl = url;
            }

            return publicUrl;
        } //
        #endregion

        #region === Application Keys Methods ===

        /// <summary>
        /// Gets the value of an application key from web.config. Returns blanks if not found
        /// </summary>
        /// <remarks>This property is explicitly thread safe.</remarks>
        public static string GetAppKeyValue( string keyName )
        {

            return GetAppKeyValue( keyName, "" );
        } //

        /// <summary>
        /// Gets the value of an application key from web.config. Returns the default value if not found
        /// </summary>
        /// <remarks>This property is explicitly thread safe.</remarks>
        public static string GetAppKeyValue( string keyName, string defaultValue )
        {
            string appValue = "";

            try
            {
                appValue = System.Configuration.ConfigurationManager.AppSettings[ keyName ];
                if ( appValue == null )
                    appValue = defaultValue;
            }
            catch
            {
                appValue = defaultValue;
            }

            return appValue;
        } //
        public static int GetAppKeyValue( string keyName, int defaultValue )
        {
            int appValue = -1;

            try
            {
                appValue = Int32.Parse( System.Configuration.ConfigurationManager.AppSettings[ keyName ] );

                // If we get here, then number is an integer, otherwise we will use the default
            }
            catch
            {
                appValue = defaultValue;
            }

            return appValue;
        } //


        #endregion


    }
}
