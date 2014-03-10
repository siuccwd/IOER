using System;
using System.Data;
using System.Configuration;
using System.Resources;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using ILPathways.Utilities;
using ILPathways.classes;
using ILPathways.Business;

using wnEmailItem = ILPathways.EmailNoticeServiceReference;

namespace ILPathways.Controllers
{
    public class EmailController
    {

        public static EmailNotice GetByCode( string emailCode )
        {
            wnEmailItem.EmailNoticeWSSoapClient wsClient = new wnEmailItem.EmailNoticeWSSoapClient();
            wnEmailItem.EmailNotice en = new wnEmailItem.EmailNotice();
            en = wsClient.GetByCode( emailCode );

            EmailNotice entity = Map( en );
            return entity;
        }

        /// <summary>
        /// call ws to send email via a fully populated email notice
        /// </summary>
        /// <param name="notice"></param>
        /// <param name="toEmail"></param>
        /// <returns></returns>
        public static string Send( EmailNotice notice, string toEmail )
        {
            string password = AppItemController.workNetItemsPassword;
            wnEmailItem.EmailNoticeWSSoapClient wsClient = new wnEmailItem.EmailNoticeWSSoapClient();
            wnEmailItem.EmailNotice en = Map( notice );
            string status = wsClient.Send( en, toEmail, password );


            return status;
        }
        /// <summary>
        /// call ws to send email using an email notice code.
        /// NOTE: assumes no substitutions are necessary, just the content of the notice will be sent
        /// </summary>
        /// <param name="noticeCode"></param>
        /// <param name="toEmail"></param>
        /// <returns></returns>
        public static string Send( string noticeCode, string toEmail )
        {
            string password = AppItemController.workNetItemsPassword;
            wnEmailItem.EmailNoticeWSSoapClient wsClient = new wnEmailItem.EmailNoticeWSSoapClient();

            string status = wsClient.SendByCode( noticeCode, toEmail, password );


            return status;
        }

        public static EmailNotice Map( wnEmailItem.EmailNotice from )
        {
            EmailNotice entity = new EmailNotice();
			entity.Id = from.Id;
			entity.NoticeCode = from.NoticeCode;
			entity.Title = from.Title;
			entity.Description = from.Description;
			entity.Category = from.Category;
			entity.Filter = from.Filter;

			entity.FromEmail = from.FromEmail;
			entity.CcEmail = from.CcEmail;
			entity.BccEmail = from.BccEmail;
			entity.Subject = from.Subject;
			entity.TextBody = from.TextBody;
			entity.HtmlBody = from.HtmlBody;
			entity.LanguageCode = from.LanguageCode;

			entity.IsValid = from.IsValid;

			entity.Message = from.Message;

            return entity;



        }


        public static wnEmailItem.EmailNotice Map( EmailNotice from )
        {

			wnEmailItem.EmailNotice wsEntity = new wnEmailItem.EmailNotice();
			wsEntity.Id = from.Id;
            wsEntity.NoticeCode = from.NoticeCode;
            wsEntity.Title = from.Title;
			wsEntity.Description = from.Description;
			wsEntity.Category = from.Category;
			wsEntity.Filter = from.Filter;

			wsEntity.FromEmail = from.FromEmail;
			wsEntity.CcEmail = from.CcEmail;
			wsEntity.BccEmail = from.BccEmail;
			wsEntity.Subject = from.Subject;
			wsEntity.TextBody = from.TextBody;
			wsEntity.HtmlBody = from.HtmlBody;
			wsEntity.LanguageCode = from.LanguageCode;

			wsEntity.IsValid = from.IsValid;

			wsEntity.Message = from.Message;
            return wsEntity;
        }
    }
}