using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;

using ILPathways.Business;

namespace ILPathways.Utilities
{
	/// <summary>
	/// Helper class, provides email services
	/// </summary>
    public class EmailManager : BaseUtilityManager
	{

		/// <summary>
		/// Default constructor for EmailManager
		/// </summary>
		public EmailManager()
		{ }

		public static string FormatEmailAddress( string address, string userName )
		{
			MailAddress fAddress;
			fAddress = new MailAddress( address, userName );

			return fAddress.ToString();
		} //

    /// <summary>
    /// Send test e-mail created with the parameters. Does same substitutions as SQL Send of e-mail.
    /// </summary>
    /// <param name="appUser"></param>
    /// <param name="fromEmail"></param>
    /// <param name="subject"></param>
    /// <param name="message"></param>
        public static void SendTestEmail( IWebUser appUser, string fromEmail, string subject, string message )
    {
      SendTestEmail(appUser, fromEmail, subject, message, "");
    }
    /// <summary>
    /// Send test e-mail created with the parameters.  Does same substitutions as SQL Send of e-mail.
    /// </summary>
    /// <param name="toEmail"></param>
    /// <param name="fromEmail"></param>
    /// <param name="subject"></param>
    /// <param name="message"></param>
    /// <returns></returns>
        public static void SendTestEmail( IWebUser appUser, string fromEmail, string subject, string message, string unsubscribeUrl )
    {
      string loginLink =
        "<a href=\"http://www.illinoisworknet.com/vos_portal/residents/en/admin/login.htm?g=@RowId\">" +
        "Click here to automatically log into your account.</a>" +
        " &nbsp;Or copy all of the following address and paste it into the address line of your internet browser:<br />" +
        "http://www.illinoisworknet.com/vos_portal/residents/en/admin/login.htm?g=@RowId<br />";

      subject = "*** TESTING *** " + subject.Replace("[FirstName]", appUser.FirstName);
      subject = subject.Replace("@FirstName", appUser.FirstName);
      subject = subject.Replace("[LastName]", appUser.LastName);
      subject = subject.Replace("@LastName", appUser.LastName);
      subject = subject.Replace("[FullName]", appUser.FullName());
      subject = subject.Replace("@FullName", appUser.FullName());
      subject = subject.Replace("[option1]", "1");
      subject = subject.Replace("[option2]", "2");
      subject = subject.Replace("[option3]", "3");
      subject = subject.Replace("@LoginLink", loginLink);
      subject = subject.Replace("@RowId", appUser.RowId.ToString());

      message = message.Replace("@UnsubscribeUrl", unsubscribeUrl);
      message = message.Replace("[UnsubscribeUrl]", unsubscribeUrl);
      message = message.Replace("[FirstName]", appUser.FirstName);
      message = message.Replace("@FirstName", appUser.FirstName);
      message = message.Replace("[LastName]", appUser.LastName);
      message = message.Replace("@LastName", appUser.LastName);
      message = message.Replace("[FullName]", appUser.FullName());
      message = message.Replace("@FullName", appUser.FullName());
      message = message.Replace("[option1]", "1");
      message = message.Replace("[option2]", "2");
      message = message.Replace("[option3]", "3");
      message = message.Replace("@LoginLink", loginLink);
      message = message.Replace("@RowId", appUser.RowId.ToString());

			SendEmail( appUser.Email, fromEmail, subject, message );

    }

		/// <summary>
		/// Send a email created with the parameters
		/// </summary>
		/// <remarks>
		/// Use the SMTP server configured in the web.config as smtpEmail
		/// </remarks>
		/// <param name="toEmail">Recipient address</param>
		/// <param name="fromEmail">Sender address</param>
		/// <param name="subject">Email subject</param>
		/// <param name="message">Message Text</param>
		/// <returns></returns>
		public static bool SendEmail( string toEmail, string fromEmail, string subject, string message )
		{
			return SendEmail( toEmail, fromEmail, subject, message, "", "" );

		} //

		/// <summary>
		/// Send an e-mail using a formatted EmailNotice
		/// - assumes the Message property contains the formatted e-mail - allows for not HTML variety
		/// </summary>
		/// <param name="toEmail"></param>
		/// <param name="notice"></param>
		/// <returns></returns>
		public static bool SendEmail( string toEmail, EmailNotice notice )
		{

			return SendEmail( toEmail, notice.FromEmail, notice.Subject, notice.Message, notice.CcEmail, notice.BccEmail );

		} //

		/// <summary>
		/// Send a email created with the parameters
		/// </summary>
		/// <param name="toEmail"></param>
		/// <param name="fromEmail"></param>
		/// <param name="subject"></param>
		/// <param name="message"></param>
		/// <param name="CC"></param>
		/// <param name="BCC"></param>
		/// <returns></returns>
		public static bool SendEmail( string toEmail, string fromEmail, string subject, string message, string CC, string BCC )
		{
			char[] delim = new char[ 1 ];
			delim[ 0 ] = ',';
            MailMessage email = new MailMessage();
            string appEmail = UtilityManager.GetAppKeyValue( "contactUsMailFrom", "mparsons@siuccwd.com" );

			try
			{
				MailAddress maFrom;
				if ( fromEmail.Trim().Length == 0 )
                    fromEmail = appEmail;

				//10-04-06 mparsons - with our new mail server, we can't send emails where the from address is anything but @siuccwd.com
				//									- also try to handle the aliases 
				if ( fromEmail.IndexOf( "_siuccwd.com" ) > -1 )
					fromEmail = HandleProxyEmails( fromEmail.Trim() );

				if ( fromEmail.ToLower().IndexOf( "@ilsharedlearning.org" ) == -1
                  && fromEmail.ToLower().IndexOf( "@siuccwd.com" ) == -1 )
				{
					//not Illinois workNet so set to DoNotReply@ and switch
					string orig = fromEmail;
                    fromEmail = appEmail;
					//insert as first
                    //TODO - need a method parm to determine when should add to CC if at all. Should only show note on orginal sender
                    //if ( CC.Trim().Length > 0 )
                    //    CC = orig + "; " + CC;
                    //else
                    //    CC = orig;

					//may want to insert a quick note on to not reply to info@ but to organization instead
					if ( UtilityManager.GetAppKeyValue( "usingReplyNoteSnippet", "no" ) == "yes" )
					{
						string snippet = GetReplyNoteSnippet( orig );
						message += snippet;
					}
				}


                //if ( fromEmail.ToLower().Equals( "info@Isle.com" ) )
                //    maFrom = new MailAddress( fromEmail, "ISLE" );
                //else
					maFrom = new MailAddress( fromEmail );

				//check for overrides on the to email 
				if ( UtilityManager.GetAppKeyValue( "usingTempOverrideEmail", "no" ) == "yes" )
				{
                    if ( toEmail.ToLower().IndexOf( "illinoisworknet.com" ) < 0 && toEmail.ToLower().IndexOf( "siuccwd.com" ) < 0 )
					{
						toEmail = UtilityManager.GetAppKeyValue( "systemAdminEmail", "mparsons@siuccwd.com" );
					}
				}

				if ( toEmail.Trim().EndsWith( ";" ) )
					toEmail = toEmail.TrimEnd( Char.Parse( ";" ), Char.Parse( " " ) );


				email.From = maFrom;
				//use the add format to handle multiple email addresses - not sure what delimiters are allowed
				toEmail = toEmail.Replace( ";", "," );
				//email.To.Add( toEmail );
				string[] toSplit = toEmail.Trim().Split( delim );
				foreach ( string item in toSplit )
				{
					if ( item.Trim() != "" )
					{
						string addr = HandleProxyEmails( item.Trim() );
						MailAddress ma = new MailAddress( addr );
						email.To.Add( ma );

					}
				}

				//email.To = FormatEmailAddresses( toEmail );


				if ( CC.Trim().Length > 0 )
				{
					CC = CC.Replace( ";", "," );
					//email.CC.Add( CC );

					string[] ccSplit = CC.Trim().Split( delim );
					foreach ( string item in ccSplit )
					{
						if ( item.Trim() != "" )
						{
							string addr = HandleProxyEmails( item.Trim() );
							MailAddress ma = new MailAddress( addr );
							email.CC.Add( ma );

						}
					}
				}
				if ( BCC.Trim().Length > 0 )
				{
					BCC = BCC.Replace( ";", "," );
					//email.Bcc.Add( BCC );

					string[] bccSplit = BCC.Trim().Split( delim );
					foreach ( string item in bccSplit )
					{
						if ( item.Trim() != "" )
						{
							string addr = HandleProxyEmails( item.Trim() );
							MailAddress ma = new MailAddress( addr );
							email.Bcc.Add( ma );
						}
					}
				}

				email.Subject = subject;
				email.Body = message;

				SmtpClient smtp = new SmtpClient( UtilityManager.GetAppKeyValue( "smtpEmail" ) );
				email.IsBodyHtml = true;
				//email.BodyFormat = MailFormat.Html;
				if ( UtilityManager.GetAppKeyValue( "sendEmailFlag", "false" ) == "TRUE" )
				{
					smtp.Send( email );
					//SmtpMail.Send(email);
				}

				if ( UtilityManager.GetAppKeyValue( "logAllEmail", "no" ) == "yes" )
				{
					LogEmail( 1, email );
				}
				return true;
			} catch ( Exception exc )
			{
                if ( UtilityManager.GetAppKeyValue( "logAllEmail", "no" ) == "yes" )
                {
                    LogEmail( 1, email );
                }
				LoggingHelper.LogError( "UtilityManager.sendEmail(): Error while attempting to send:"
					+ "\r\nFrom:" + fromEmail + "   To:" + toEmail
					+ "\r\nCC:(" + CC + ") BCC:(" + BCC + ") "
					+ "\r\nSubject:" + subject
					+ "\r\nMessage:" + message
					+ "\r\nError message: " + exc.ToString() );
			}

			return false;
		} //

		/// <summary>
		/// Handles multiple addresses in the passed email part
		/// </summary>
		/// <param name="address">String of one or more Email message</param>
		/// <returns>MailAddressCollection</returns>
		public static MailAddressCollection FormatEmailAddresses( string address )
		{
			char[] delim = new char[ 1 ];
			delim[ 0 ] = ',';
			MailAddressCollection collection = new MailAddressCollection();

			address = address.Replace( ";", "," );
			string[] split = address.Trim().Split( delim );
			foreach ( string item in split )
			{
				if ( item.Trim() != "" )
				{
					string addr = HandleProxyEmails( item );
					MailAddress copy = new MailAddress( addr );
					collection.Add( copy );
				}
			}

			return collection;

		} //

		/// <summary>
		/// Handle 'proxy' email addresses - wn address used for testing that include a number to make unique
		/// Can also handle any emails where the @:
		///		- Is followed by underscore, 
		///		- then any characters
		///		- then two underscore characters
		///		- followed with the valid domain name
		/// </summary>
		/// <param name="address">Email address</param>
		/// <returns>translated email address as needed</returns>
		private static string HandleProxyEmails( string address )
		{
			string newAddr = address;

			int atPos = address.IndexOf( "@" );
			int wnPos = address.IndexOf( "_siuccwd.com" );
			if ( wnPos > atPos )
			{
				newAddr = address.Substring( 0, atPos + 1 ) + address.Substring( wnPos + 1 );
			} else
			{
				//check for others with format:
				//	someName@_ ??? __realDomain.com
				atPos = address.IndexOf( "@_" );
				if ( atPos > 1 )
				{
					wnPos = address.IndexOf( "__", atPos );
					if ( wnPos > atPos )
					{
						newAddr = address.Substring( 0, atPos + 1 ) + address.Substring( wnPos + 2 );
					}
				}
			}

			return newAddr;
		} //

		/// <summary>
		/// Sends an email message to the system administrator
		/// </summary>
		/// <param name="subject">Email subject</param>
		/// <param name="message">Email message</param>
		/// <returns>True id message was sent successfully, otherwise false</returns>
		public static bool NotifyAdmin( string subject, string message )
		{
			string emailTo = UtilityManager.GetAppKeyValue( "systemAdminEmail", "mparsons@siuccwd.com" );
			//work on implementing some specific routing based on error type
			if ( message.IndexOf( "Summaries.careerSpecialistSummary.LoadSites" ) > -1 
				|| message.IndexOf( "SqlClient.SqlConnection.Open()" ) > -1
				|| message.ToLower().IndexOf( "schoolswebservicemanager" ) > -1
				)
			{
				emailTo = "jgrimmer@siuccwd.com";
			}
		

			return  NotifyAdmin( emailTo, subject, message );
        } 

		/// <summary>
		/// Sends an email message to the system administrator
		/// </summary>
		/// <param name="emailTo">admin resource responsible for exceptions</param>
		/// <param name="subject">Email subject</param>
		/// <param name="message">Email message</param>
		/// <returns>True id message was sent successfully, otherwise false</returns>
		public static bool NotifyAdmin( string emailTo, string subject, string message )
		{
			char[] delim = new char[ 1 ];
			delim[ 0 ] = ',';
			string emailFrom = UtilityManager.GetAppKeyValue( "systemNotifyFromEmail", "TheWatcher@siuccwd.com" );
			string cc = UtilityManager.GetAppKeyValue( "systemAdminEmail", "mparsons@siuccwd.com" );
            if ( emailTo == "" )
            {
                emailTo = cc;
                cc = "";
            }
			//avoid infinite loop by ensuring this method didn't generate the exception
			if ( message.IndexOf( "EmailManager.NotifyAdmin" ) > -1 )
			{
				//skip may be error on send
				return true;

			} else
			{
				if ( emailTo.ToLower() == cc.ToLower() )
					cc = "";

				message = message.Replace("\r", "<br/>");

				MailMessage email = new MailMessage( emailFrom, emailTo );

				//try to make subject more specific
				//if: workNet Exception encountered, try to insert type
				if ( subject.IndexOf( "workNet Exception" ) > -1 )
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
				if ( cc.Trim().Length > 0 )
				{
					cc = cc.Replace( ";", "," );
					//email.CC.Add( CC );

					string[] ccSplit = cc.Trim().Split( delim );
					foreach ( string item in ccSplit )
					{
						if ( item.Trim() != "" )
						{
							string addr = HandleProxyEmails( item.Trim() );
							MailAddress ma = new MailAddress( addr );
							email.CC.Add( ma );

						}
					}
				}

				email.Body = DateTime.Now + "<br>" + message.Replace( "\n\r", "<br>" );
				email.Body = email.Body.Replace( "\r\n", "<br>" );
				email.Body = email.Body.Replace( "\n", "<br>" );
				email.Body = email.Body.Replace( "\r", "<br>" );
				SmtpClient smtp = new SmtpClient( UtilityManager.GetAppKeyValue( "smtpEmail" ) );
				email.IsBodyHtml = true;
				//email.BodyFormat = MailFormat.Html;
				try
				{
					//The trace was a just in case, if the send fails, a LogError call will be made anyway. Set to a high level so not shown in prod
					LoggingHelper.DoTrace( 11, "EmailManager.NotifyAdmin: - Admin email was requested:\r\nSubject:" + subject + "\r\nMessage:" + message );
					if ( UtilityManager.GetAppKeyValue( "sendEmailFlag", "false" ) == "TRUE" )
					{
						smtp.Send( email );
						//SmtpMail.Send(email);
					}

					if ( UtilityManager.GetAppKeyValue( "logAllEmail", "no" ) == "yes" )
					{
						LogEmail( 1, email );
						//LoggingHelper.DoTrace(1,"    ***** Email Log ***** "
						//  + "\r\nFrom:" + fromEmail
						//  + "\r\nTo:  " + toEmail
						//  + "\r\nCC:(" + CC + ") BCC:(" + BCC + ") "
						//  + "\r\nSubject:" + subject
						//  + "\r\nMessage:" + message
						//  + "\r\n============================================");
					}
					return true;
				} catch ( Exception exc )
				{
					LoggingHelper.LogError( "EmailManager.NotifyAdmin(): Error while attempting to send:"
						+ "\r\nSubject:" + subject + "\r\nMessage:" + message
						+ "\r\nError message: " + exc.ToString(), false );
				}
			}

			return false;
		} //

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
                int start = message.IndexOf( "Exception:" );
                int end = message.IndexOf( "Stack Trace:" );
                if ( end == -1)
                    end = message.IndexOf( ";",start );

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
			} catch
			{
				work = subject;
			}

			return work;
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
			try
			{
				appTraceLevel = UtilityManager.GetAppKeyValue( "appTraceLevel", 1 );

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
					string logFile = UtilityManager.GetAppKeyValue( "path.email.log", "C:\\VOS_LOGS.txt" );
					string outputFile = logFile.Replace( "[date]", datePrefix );

					StreamWriter file = File.AppendText( outputFile );

					file.WriteLine( msg );
					file.Close();

				}
			} catch
      {
				//ignore errors
			}

		}

		/// <summary>
		/// Retrieve and format the reply to note snippet
		/// </summary>
		/// <param name="fromEmail"></param>
		/// <returns></returns>
		private static string GetReplyNoteSnippet( string fromEmail )
		{
			const string key = "ReplyNoteSnippet";

			string body = HttpContext.Current.Cache[ key ] as string;

			if ( string.IsNullOrEmpty( body ) )
			{
				string file = System.Web.HttpContext.Current.Server.MapPath( "~/App_Data/ReplyNoteSnippet.txt" );
				body = File.ReadAllText( file );

				HttpContext.Current.Cache[ key ] = body;
			}

			body = body.Replace( "<%SenderEmail%>", fromEmail );

			return body;
		}//

  }
}
