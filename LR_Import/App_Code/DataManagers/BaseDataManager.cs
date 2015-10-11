using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;

namespace LearningRegistryCache2.App_Code.DataManagers
{
  public class BaseDataManager
  {
    private string _connString;
    protected string ConnString
    {
      get { return this._connString; }
      set { this._connString = value; }
    }

    public static string DEFAULT_GUID = "00000000-0000-0000-0000-000000000000";

    public BaseDataManager()
    {
      ConnString = ConfigurationManager.ConnectionStrings["dbConString"].ConnectionString;
    }
       
    public bool DoesDataSetHaveRows(DataSet ds)
    {
      if (ds != null && ds.Tables.Count != 0 && ds.Tables[0].Rows.Count != 0)
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    public string GetRowColumn(DataRow dr, string fieldName, string defaultValue)
    {
      string colValue = defaultValue;
      try
      {
        colValue = dr[fieldName].ToString();
      }
      catch (Exception ex)
      {
        colValue = defaultValue;
      }
      return colValue;
    }

    public int GetRowColumn(DataRow dr, string fieldName, int defaultValue)
    {
      int colValue = defaultValue;
      try
      {
        colValue = int.Parse(GetRowColumn(dr, fieldName, ""));
      }
      catch (Exception ex)
      {
        colValue = defaultValue;
      }
      return colValue;
    }

    public decimal GetRowColumn(DataRow dr, string fieldName, decimal defaultValue)
    {
        decimal colValue = defaultValue;
        try
        {
            colValue = decimal.Parse(GetRowColumn(dr, fieldName, ""));
        }
        catch (Exception ex)
        {
            colValue = defaultValue;
        }
        return colValue;
    }

    public bool GetRowColumn(DataRow dr, string fieldName, bool defaultValue)
    {
      bool colValue = defaultValue;
      try
      {
        colValue = bool.Parse(GetRowColumn(dr, fieldName, ""));
      }
      catch (Exception ex)
      {
        colValue = defaultValue;
      }
      return colValue;
    }

    public DateTime GetRowColumn(DataRow dr, string fieldName, DateTime defaultValue)
    {
      DateTime colValue = defaultValue;
      try
      {
        colValue = DateTime.Parse(GetRowColumn(dr, fieldName, ""));
      }
      catch (Exception ex)
      {
        colValue = defaultValue;
      }
      return colValue;
    }

    public static DateTime ConvertLRTimeToDateTime(string timeToConvert)
    {
        timeToConvert = timeToConvert.Replace("T", " ");
        timeToConvert = timeToConvert.Replace("Z", "");
        return DateTime.Parse(timeToConvert);
    }


		#region Logging - keep in sync with methods in UtilityManager until cleaned up or replaced
		/// <summary>
		/// Format an exception and message, and then log it
		/// </summary>
		/// <param name="ex">Exception</param>
		/// <param name="message">Additional message regarding the exception</param>
		public static void LogError( Exception ex, string message )
		{

			string user = "";
			string sessionId = "unknown";
			string remoteIP = "unknown";
			string path = "unknown";
			string queryString = "unknown";
			string mcmsUrl = "unknown";
			string parmsString = "";

			try
			{

			} catch
			{
				//eat any additional exception
			}

			try
			{
				string errMsg = message +
					"\r\nType: " + ex.GetType().ToString() +
					"\r\nException: " + ex.Message.ToString() +
					"\r\nStack Trace: " + ex.StackTrace.ToString() +
					"\r\nServer\\Template: " + path ;

				if ( parmsString.Length > 0 )
					errMsg += "\r\nParameters: " + parmsString;

				LogError( errMsg );
			} catch
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
			} else
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
							//NotifyAdmin( "workNet Exception encountered", message );
						}
					}
				} catch
				{
					//eat any additional exception
				}
			}
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
			} catch
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
			} catch
			{
				appValue = defaultValue;
			}

			return appValue;
		} //


		#endregion
	}
}
