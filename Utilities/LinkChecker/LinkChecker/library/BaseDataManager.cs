using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using Microsoft.ApplicationBlocks.Data;

namespace LinkChecker.library
{
    public class BaseDataManager : LRWarehouse.DAL.BaseDataManager
    {
        private string _connString;
        protected new string ConnString
        {
            get { return this._connString; }
            set { this._connString = value; }
        }
        private string _readOnlyConnString;
        protected new string ReadOnlyConnString
        {
            get { return this._readOnlyConnString; }
            set { this._readOnlyConnString = value; }
        }

        public BaseDataManager()
        {
            ConnString = LRLinkChecker();
            ReadOnlyConnString = LRLinkCheckerRO();
        }

        public static string LRLinkChecker()
        {

            string conn = ConfigurationManager.ConnectionStrings["LR_LinkChecker"].ConnectionString;
            return conn;

        }
        /// <summary>
        /// Get the connection string for the LR_WarehouseRO database
        /// </summary>
        /// <returns></returns>
        public static string LRLinkCheckerRO()
        {

            string conn = ConfigurationManager.ConnectionStrings["LR_LinkCheckerRO"].ConnectionString;
            return conn;
        }
        public static string WorkNet2013Connection()
        {

            string conn = ConfigurationManager.ConnectionStrings[ "worknet2013Connection" ].ConnectionString;
            return conn;

        }

        /// <summary>
        /// General query, requires userid and password
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <returns>Dataset with Query results or null if there are not results or if there was any error.</returns>
        public static DataSet DoLRQuery( string sql )
        {
            string connection = LRWarehouseRO();
            return DoQuery( sql, connection );
        }

        public static DataSet DoDWQuery( string sql )
        {
            string connection = WorkNet2013Connection();
            return DoQuery( sql, connection );
        }
        /// <summary>
        /// General query, requires userid and password
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <returns>Dataset with Query results or null if there are not results or if there was any error.</returns>
        public static DataSet DoQuery( string sql, string connection )
        {
            DataSet ds = new DataSet();

            try
            {

                //use default database connection for sql
                ds = SqlHelper.ExecuteDataset( connection, System.Data.CommandType.Text, sql );

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
                string queryString = GetWebUrl();
                LogError( string.Format( "DatabaseManager.DoQuery(): \r\nUser: {0}\r\nURL: " + queryString + "\r\nSQL:" + sql + "\r\n" + e.ToString(), "n/a" ) );
                //return SetDataSetMessage( "DatabaseManager.DoQuery(): " + e.Message.ToString() );
                return null;
            }

        } //

        private static string GetWebUrl()
        {
            string queryString = "n/a";

            if ( HttpContext.Current != null && HttpContext.Current.Request != null )
                queryString = HttpContext.Current.Request.RawUrl.ToString();

            return queryString;
        }
        public static void LogError(string message)
        {

            if (GetAppKeyValue("notifyOnException", "no").ToLower() == "yes")
            {
                LogError(message, true);
            }
            else
            {
                LogError(message, false);
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
        public static void LogError(string message, bool notifyAdmin)
        {
            if (GetAppKeyValue("logErrors").ToString().Equals("yes"))
            {
                try
                {
                    //string logFile  = GetAppKeyValue("error.log.path","C:\\VOS_LOGS.txt");
                    string datePrefix = System.DateTime.Today.ToString("u").Substring(0, 10);
                    string logFile = GetAppKeyValue("path.error.log", "C:\\VOS_LOGS.txt");
                    string outputFile = logFile.Replace("[date]", datePrefix);

                    StreamWriter file = File.AppendText(outputFile);
                    file.WriteLine(DateTime.Now + ": " + message);
                    file.WriteLine("---------------------------------------------------------------------");
                    file.Close();

                    if (notifyAdmin)
                    {
                        if (GetAppKeyValue("notifyOnException", "no").ToLower() == "yes")
                        {
                            NotifyAdmin("Isle Exception encountered", message);
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
        public static bool NotifyAdmin(string subject, string message)
        {

            //avoid infinite loop by ensuring this method didn't generate the exception
            string emailTo = GetAppKeyValue("systemAdminEmail", "mparsons@siuccwd.com");
            string emailFrom = GetAppKeyValue("systemNotifyFromEmail", "TheWatcher@siuccwd.com");
            string cc = "";

            //work on implementing some specific routing based on error type
            if (message.IndexOf("SqlClient.SqlConnection.Open()") > -1)
                cc = "jgrimmer@siuccwd.com";

            if (message.IndexOf("BaseDataManager.NotifyAdmin") > -1)
            {
                //skip may be error on send

            }
            else
            {
                message = message.Replace("\r", "<br/>");

                MailMessage email = new MailMessage(emailFrom, emailTo);

                //try to make subject more specific
                //if: Isle Exception encountered, try to insert type
                if (subject.IndexOf("Isle Exception") > -1)
                {
                    subject = FormatExceptionSubject(subject, message);
                }
                email.Subject = subject;

                if (message.IndexOf("Type:") > 0)
                {
                    int startPos = message.IndexOf("Type:");
                    int endPos = message.IndexOf("Error Message:");
                    if (endPos > startPos)
                    {
                        subject += " - " + message.Substring(startPos, endPos - startPos);
                    }
                }
                //if ( cc.Length > 0 )
                //{
                //  MailAddress ma = new MailAddress( cc );
                //  email.CC.Add( ma );
                //}

                email.Body = DateTime.Now + "<br>" + message.Replace("\n\r", "<br>");
                email.Body = email.Body.Replace("\r\n", "<br>");
                email.Body = email.Body.Replace("\n", "<br>");
                email.Body = email.Body.Replace("\r", "<br>");
                //SmtpMail.SmtpServer = GetAppKeyValue("smtpEmail");
                SmtpClient smtp = new SmtpClient(GetAppKeyValue("smtpEmail"));
                email.IsBodyHtml = true;
                //email.BodyFormat = MailFormat.Html;
                try
                {
                    if (GetAppKeyValue("sendEmailFlag", "false") == "TRUE")
                    {
                        smtp.Send(email);
                        //SmtpMail.Send(email);
                    }

                    if (GetAppKeyValue("logAllEmail", "no") == "yes")
                    {
                        LogEmail(1, email);
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
                catch (Exception exc)
                {
                    LogError("BaseDataManager.NotifyAdmin(): Error while attempting to send:"
                        + "\r\nSubject:" + subject + "\r\nMessage:" + message
                        + "\r\nError message: " + exc.ToString(), false);
                }
            }

            return false;
        } //

        /// <summary>
        /// Log email message - for future resend/reviews
        /// </summary>
        /// <param name="level"></param>
        /// <param name="email"></param>
        public static void LogEmail(int level, MailMessage email)
        {

            string msg = "";
            int appTraceLevel = 0;
            //bool useBriefFormat = true;

            try
            {
                appTraceLevel = GetAppKeyValue("appTraceLevel", 1);

                //Allow if the requested level is <= the application thresh hold
                if (level <= appTraceLevel)
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

                    string datePrefix = System.DateTime.Today.ToString("u").Substring(0, 10);
                    string logFile = GetAppKeyValue("path.email.log", "C:\\VOS_LOGS.txt");
                    string outputFile = logFile.Replace("[date]", datePrefix);

                    StreamWriter file = File.AppendText(outputFile);

                    file.WriteLine(msg);
                    file.Close();

                }
            }
            catch
            {
                //ignore errors
            }

        }
        public static string GetAppKeyValue(string keyName)
        {

            return GetAppKeyValue(keyName, "");
        } //

        /// <summary>
        /// Gets the value of an application key from web.config. Returns the default value if not found
        /// </summary>
        /// <remarks>This property is explicitly thread safe.</remarks>
        public static string GetAppKeyValue(string keyName, string defaultValue)
        {
            string appValue = "";

            try
            {
                appValue = System.Configuration.ConfigurationManager.AppSettings[keyName];
                if (appValue == null)
                    appValue = defaultValue;
            }
            catch
            {
                appValue = defaultValue;
            }

            return appValue;
        } //
        public static int GetAppKeyValue(string keyName, int defaultValue)
        {
            int appValue = -1;

            try
            {
                appValue = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings[keyName]);

                // If we get here, then number is an integer, otherwise we will use the default
            }
            catch
            {
                appValue = defaultValue;
            }

            return appValue;
        } //
        public static bool DoesDataSetHaveRows(DataSet ds)
        {

            try
            {
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    return true;
                else
                    return false;
            }
            catch
            {

                return false;
            }
        }//

        /// <summary>
        /// Attempt to format a more meaningful subject for an exception related email
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        public static string FormatExceptionSubject(string subject, string message)
        {
            string work = "";

            try
            {
                int start = message.IndexOf("Type:");
                int end = message.IndexOf("User:");
                if (start > -1 && end > start)
                {
                    work = message.Substring(start, end - start);
                    //remove line break
                    work = work.Replace("\r\n", "");
                    work = work.Replace("<br>", "");
                    work = work.Replace("Type:", "Exception:");
                    if (message.IndexOf("Caught in Application_Error event") > -1)
                    {
                        work = work.Replace("Exception:", "Unhandled Exception:");
                    }

                }
                if (work.Length == 0)
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
        /// Helper method to retrieve a string column from a row while handling invalid values
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>
        public static string GetRowColumn(DataRow row, string column, string defaultValue)
        {
            string colValue = "";

            try
            {
                colValue = row[column].ToString();

            }
            catch (System.FormatException fex)
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch (Exception ex)
            {
                if (column.IndexOf("CUSTOMER_STATUS") > -1)
                {
                    //ignore

                }
                else
                {
                    string queryString = string.Empty;
                    string exType = ex.GetType().ToString();
                    LogError(exType + " Exception in GetRowColumn( DataRow row, string column, string defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true);
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
        public static int GetRowColumn(DataRow row, string column, int defaultValue)
        {
            int colValue = 0;

            try
            {
                colValue = Int32.Parse(row[column].ToString());

            }
            catch (System.FormatException fex)
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch (Exception ex)
            {
                string queryString = string.Empty;

                LogError("Exception in GetRowColumn( DataRow row, string column, int defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true);
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
        public static bool GetRowColumn(DataRow row, string column, bool defaultValue)
        {
            bool colValue;

            try
            {
                colValue = Boolean.Parse(row[column].ToString());

            }
            catch (System.FormatException fex)
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch (Exception ex)
            {
                string queryString = string.Empty;
                LogError("Exception in GetRowColumn( DataRow row, string column, bool defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true);
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
        public static System.DateTime GetRowColumn(DataRow row, string column, System.DateTime defaultValue)
        {
            System.DateTime colValue;

            try
            {
                colValue = System.DateTime.Parse(row[column].ToString());
            }
            catch (System.FormatException fex)
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch (Exception ex)
            {
                string queryString = string.Empty;
                LogError("Exception in GetRowColumn( DataRow row, string column, System.DateTime defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true);
                colValue = defaultValue;
            }
            return colValue;

        } // end method


    }
}
