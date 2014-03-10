using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using Microsoft.ApplicationBlocks.Data;
//using IllinoisPathways.DataAccess.Common.DBML;
using ILPathways.Business;
//using ILPathways.Utilities;

namespace ILPathways.DAL
{
    public class LogManager : BaseDataManager
    {
        public static void LogException( ExceptionLogBEO exceptionLogDAO )
        {

            //using Linq???
            //using ( LogDataContext logDataContext = LinqHelper.CreateDataContext<LogDataContext>() )
            //{
            //    logDataContext.usp_ExceptionLogInsert( exceptionLogDAO.MessageId, exceptionLogDAO.MessageText, exceptionLogDAO.ExceptionMessage, exceptionLogDAO.StackTrace, exceptionLogDAO.LastUpdatedBy );
            //}
        }

        #region Page Logging Routines
        /// <summary>
        /// Record a page visit in the database log
        /// </summary>
        /// <param name="sessionId">Session Id</param>
        /// <param name="isPostBack">Was this a page postback</param>
        /// <param name="template">MCMS Template</param>
        /// <param name="queryString">Request URL</param>
        /// <param name="parmString">Request Parameters (if any)</param>
        /// <param name="userid">Userid of current user (guest if not logged in)</param>
        /// <param name="partner">Partner name</param>
        /// <param name="comment">Comment</param>
        /// <param name="remoteIP">client IP address</param>
        /// <param name="officeId">Lwia Office Id</param>
        /// <param name="pathway">Current pathway</param>
        /// <param name="lang">Language of current page</param>
        /// <param name="mainChannel">Main channel of current page</param>
        /// <param name="currentZip">current active zipcode</param>
        /// <remarks>06/09/15 mparsons - added remoteIP
        /// 06/10/04 mparsons - changed to get the db connection from DatabaseManager.GetAppLogDBCon
        /// 08/08/25 mparsons -added: Pathway ,[Language],MainChannel</remarks>
        public static void LogPageVisit( string sessionId, string template, string queryString, string parmString, string isPostBack, string userid, string partner, string comment, string remoteIP, string officeId, string pathway, string lang, string mainChannel, string currentZip )
        {
            string serverName = GetAppKeyValue( "serverName" );
            try
            {
                string dbCon = GetAppLogDBCon();
                string partnerItem = "lwia" + partner + ",office" + officeId;
                string officeItem = "office" + officeId;

                string usingAD = GetAppKeyValue( "useAD" );

                if ( queryString.EndsWith( "/" ) )
                    queryString = queryString.Substring( 0, queryString.Length - 1 );

                queryString = queryString.Replace( "%2f", "/" );

                queryString = queryString.Replace( "%3a", ":" );

                //check for values larger than the max allowed
                template = HandleParameterMaximums( "LogManager.LogPageVisit", template, 200 );
                queryString = HandleParameterMaximums( "LogManager.LogPageVisit", queryString, 200 );
                parmString = HandleParameterMaximums( "LogManager.LogPageVisit", parmString, 300 );
                comment = HandleParameterMaximums( "LogManager.LogPageVisit", comment, 200 );

                //skip call to retrieve number of parms for speed
                SqlParameter[] arParms = new SqlParameter[ 15 ];

                //  ==============================================================================
                arParms[ 0 ] = new SqlParameter( "@SessionId", sessionId );
                arParms[ 1 ] = new SqlParameter( "@Path", template );
                arParms[ 2 ] = new SqlParameter( "@QueryString", queryString );
                arParms[ 3 ] = new SqlParameter( "@parmString", parmString );

                arParms[ 4 ] = new SqlParameter( "@IsPostback", isPostBack );
                arParms[ 5 ] = new SqlParameter( "@Userid", userid );
                arParms[ 6 ] = new SqlParameter( "@Partner", partner );
                arParms[ 7 ] = new SqlParameter( "@Comment", comment );

                arParms[ 8 ] = new SqlParameter( "@RemoteIP", remoteIP );
                if ( officeId.Length > 0 )
                    arParms[ 9 ] = new SqlParameter( "@OfficeId", AssignWithDefault( officeId, 0 ) );
                else
                    arParms[ 9 ] = new SqlParameter( "@OfficeId", 0 );

                arParms[ 10 ] = new SqlParameter( "@ServerName", serverName );
                if ( arParms.Length > 11 )
                {
                    arParms[ 11 ] = new SqlParameter( "@Pathway", pathway );
                    arParms[ 12 ] = new SqlParameter( "@Language", lang );
                    arParms[ 13 ] = new SqlParameter( "@MainChannel", mainChannel );
                }
                if ( arParms.Length > 14 )
                    arParms[ 14 ] = new SqlParameter( "@CurrentZip", currentZip );


                //  ==============================================================================

                if ( remoteIP.Equals( "127.0.0.1" ) && usingAD.ToLower().Equals( "true" ) )
                {
                    //skip - some local activity
                    //this.DoTrace( 8, "LogManager.LogPageVisit() skipping remoteIP 127.0.0.1: " + queryString );
                }
                else
                {
                    SqlHelper.ExecuteNonQuery( dbCon, "AppVisitLogInsert", arParms );
                }
            }
            catch ( Exception e )
            {
                //log error and ignore
                System.DateTime visitDate = System.DateTime.Now;
                string logEntry = sessionId + ","
                        + visitDate.ToString() + ","
                        + template + ","
                        + queryString + ",'"
                        + parmString + "',"
                        + isPostBack + ","
                        + userid + ","
                        + partner + ","
                        + "'" + comment + "',"
                        + remoteIP + ","
                        + officeId + ","
                        + serverName + ","
                        + pathway + ","
                        + lang + ","
                        + mainChannel + ","
                        + currentZip
                        ;

                LogError( e, "LogManager.LogPageVisit() Exception: See trace log for entry\r\n " + queryString );
                //this.DoTrace( 1, "LogManager.LogPageVisit():\r\n " + logEntry );

            }
        } //
        /// <summary>
        /// Checks passed string, if not nullthen returns the passed string. 
        ///	Otherwise returns the passed default value
        /// </summary>
        /// <param name="stringToTest"></param>
        /// <param name="defaultValue"></param> 
        /// <returns>int</returns>
        private static string AssignWithDefault( string stringToTest, string defaultValue )
        {
            string newVal;

            try
            {
                if ( stringToTest == null )
                {
                    newVal = defaultValue;
                }
                else
                {
                    newVal = stringToTest;
                }
            }
            catch
            {

                newVal = defaultValue;
            }

            return newVal;

        } //
        /// <summary>
        /// Returns passed string as an integer, if is an integer and not null/empty. 
        /// Otherwise returns the passed default value
        /// </summary>
        /// <param name="stringToTest"></param>
        /// <param name="defaultValue"></param>
        /// <returns>The string parameter as an int or the default value if the parameter is not a vlid integer</returns>
        private static int AssignWithDefault( string stringToTest, int defaultValue )
        {
            int newVal;

            try
            {
                if ( IsInteger( stringToTest ) && stringToTest.Length > 0 )
                {
                    newVal = int.Parse( stringToTest );
                }
                else
                {
                    newVal = defaultValue;
                }
            }
            catch
            {

                newVal = defaultValue;
            }

            return newVal;

        } //
        /// <summary>
        /// IsInteger - test if passed string is an integer
        /// </summary>
        /// <param name="stringToTest"></param>
        /// <returns></returns>
        private static bool IsInteger( string stringToTest )
        {
            int newVal;
            bool result = false;
            try
            {
                newVal = Int32.Parse( stringToTest );

                // If we get here, then number is an integer
                result = true;
            }
            catch
            {

                result = false;
            }
            return result;

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


        #endregion

    }
}
