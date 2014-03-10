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
using IPC = ILPathways.Common;

namespace LRWarehouse.DAL
{
    /// <summary>
    /// Summary description for baseDataManager
    /// </summary>
    public abstract class BaseDataManager : IPC.BaseDataManager
    {

        private string _connString;
        protected string ConnString
        {
            get { return this._connString; }
            set { this._connString = value; }
        }
        private string _readOnlyConnString;
        protected string ReadOnlyConnString
        {
            get { return this._readOnlyConnString; }
            set { this._readOnlyConnString = value; }
        }

        public static string DEFAULT_GUID = "00000000-0000-0000-0000-000000000000";

        public BaseDataManager()
        {
            ConnString = LRWarehouse();
            ReadOnlyConnString = LRWarehouseRO();
        }
        /// <summary>
        /// Use C# destructor syntax for finalization code.
        /// </summary>
        ~BaseDataManager()
        {
            // Simply call Dispose(false).
            Dispose( false );
        }

        #region get database connection strings

        /// <summary>
        /// Get the connection string for the LRWarehouse database
        /// </summary>
        /// <returns></returns>
        public static string LRWarehouse()
        {

            string conn = WebConfigurationManager.ConnectionStrings[ "LR_Warehouse" ].ConnectionString;
            return conn;

        }
        /// <summary>
        /// Get the connection string for the LR_WarehouseRO database
        /// </summary>
        /// <returns></returns>
        public static string LRWarehouseRO()
        {

            string conn = WebConfigurationManager.ConnectionStrings[ "LR_WarehouseRO" ].ConnectionString;
            return conn;

        }

        /// <summary>
        /// Retrieve connection string for read only access to main database 
        /// </summary>
        /// <returns></returns>
        public static string GetReadOnlyConnection()
        {
            return LRWarehouseRO();

        }
        /// <summary>
        /// Converts Learning Registry DateTime format to .NET DateTime format
        /// </summary>
        /// <param name="timeToConvert"></param>
        /// <returns></returns>
        public static DateTime ConvertLRTimeToDateTime(string timeToConvert)
        {
            timeToConvert = timeToConvert.Replace("T", " ");
            timeToConvert = timeToConvert.Replace("Z", "");
            return DateTime.Parse(timeToConvert);
        }

        /// <summary>
        /// Get the connection string for the Content database
        /// </summary>
        /// <returns></returns>
        public static string ContentConnection()
        {

            string conn = WebConfigurationManager.ConnectionStrings[ "contentConString" ].ConnectionString;
            return conn;

        }
        /// <summary>
        /// Get the read only connection string for the Content database
        /// </summary>
        /// <returns></returns>
        public static string ContentConnectionRO()
        {

            string conn = WebConfigurationManager.ConnectionStrings[ "contentConString_RO" ].ConnectionString;
            return conn;

        }
        /// <summary>
        /// Get the connection string for the common database
        /// </summary>
        /// <returns></returns>
        public static string GetMainDBConnection()
        {

            string conn = WebConfigurationManager.ConnectionStrings[ "dbConString" ].ConnectionString;
            return conn;

        }
        /// <summary>
        /// Get a specified database connection string
        /// </summary>
        /// <param name="dbConString">Name of a connection string (found in the web.config</param>
        /// <returns>Connection string</returns>
        public static string GetDatabaseCon( string dbConString )
        {

            string conn = WebConfigurationManager.ConnectionStrings[ dbConString ].ConnectionString;
            return conn;
        }



        /// <summary>
        /// Gets the database connection string for the application visit log
        /// </summary>
        /// <returns>Database Connection string for current environment</returns>
        public static string GetAppLogDBCon()
        {

            string conn = WebConfigurationManager.ConnectionStrings[ "appLogConString" ].ConnectionString;
            //string conn = GetAppKeyValue("appLogConString").ToString();
            return conn;
        } //


        /// <summary>
        /// Gets the database connection string for the application visit log
        /// </summary>
        /// <returns>Database Connection string for current environment</returns>
        public static string GetPartnerDBCon()
        {

            string conn = WebConfigurationManager.ConnectionStrings[ "partnerConString" ].ConnectionString;
            return conn;
        } //

        /// <summary>
        /// Gets the database connection string for the VosPosting related database
        /// - tables unique to an env
        /// </summary>
        /// <returns>Database Connection string for current environment</returns>
        public static string GetEnvSpecificConnection()
        {
            string conn = "";
            if ( GetAppKeyValue( "isVosPostingUsingSeparateDB", "no" ) == "yes" )
            {
                conn = WebConfigurationManager.ConnectionStrings[ "envSpecificConnString" ].ConnectionString;
            }
            else
            {
                conn = WebConfigurationManager.ConnectionStrings[ "dbConString" ].ConnectionString;
            }
            return conn;
        } //

        /// <summary>
        /// Check if primary connection is available. If not return secondary connection
        /// </summary>
        /// <param name="dbConn"></param>
        /// <returns></returns>
        private static string GetActiveConnection( string dbConn )
        {
            string activeConn = dbConn;

            try
            {
                SqlConnection conn = new SqlConnection( dbConn );
                //test availability
                conn.Open();

                conn.Close();
            }
            catch ( Exception ex )
            {
                //what exception??
                DoTrace( 4, "BaseDataManager.GetActiveConnection exception: " + ex.Message );
                //assume same database name,try different server??
                string primaryDBServer = GetAppKeyValue( "primaryDBServer" );
                string secondaryDBServer = GetAppKeyValue( "secondaryDBServer" );

                activeConn = dbConn.Replace( primaryDBServer, secondaryDBServer );
                //not going to check secondary at this time. if both down then out of luck?
                //could attempt an email?
            }

            return activeConn;
        }

        /// <summary>
        /// Gets the database connection string for the workNetZipCodes database 
        /// </summary>
        /// <returns>Database Connection string for [workNetZipCodes]</returns>
        public static string ZipValidationConnection()
        {

            string conn = WebConfigurationManager.ConnectionStrings[ "zipValidationConString" ].ConnectionString;

            return conn;
        } //
        #endregion



    }
}
