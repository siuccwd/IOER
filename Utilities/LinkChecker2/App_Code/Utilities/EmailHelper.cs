using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace LinkChecker2.App_Code.Utilities
{
    public class EmailHelper
    {
        public static void SendEmail(string fromEmail, string[] toEmail, string[] ccEmail, string[] bccEmail, string subject, string emailBody, string[] attachments)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient smtpServer = new SmtpClient(ConfigurationManager.AppSettings["smtpEmail"].ToString());
                mail.From = new MailAddress(fromEmail);
                if (toEmail != null)
                {
                    foreach (string address in toEmail)
                    {
                        mail.To.Add(address.Trim());
                    }
                }
                if (ccEmail != null)
                {
                    foreach (string address in ccEmail)
                    {
                        mail.CC.Add(address.Trim());
                    }
                }
                if (bccEmail != null)
                {
                    foreach (string address in bccEmail)
                    {
                        mail.Bcc.Add(address.Trim());
                    }
                }
                mail.Subject = subject;
                mail.Body = emailBody;
                if (attachments != null)
                {
                    foreach (string fileName in attachments)
                    {
                        Attachment mailAttachment = new Attachment(fileName.Trim());
                        mail.Attachments.Add(mailAttachment);
                    }
                }
                smtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
