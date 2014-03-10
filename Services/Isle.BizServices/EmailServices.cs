using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILPNotice = ILPathways.Business.EmailNotice;
//using IoerContentBusinessEntities;
using IOERBusinessEntities;

namespace Isle.BizServices
{

    public class EmailServices
    {
        static ResourceEntities ctx = new ResourceEntities();


        public static ILPNotice EmailNotice_Get( string noticeCode )
        {
            ILPNotice entity = new ILPNotice();

            EmailNotice mbr = ctx.EmailNotices.SingleOrDefault( s => s.NoticeCode == noticeCode );
            if ( entity != null && entity.Id > 0 )
            {
                entity.Id = (int) mbr.id;
                entity.Title = mbr.Title;
                entity.Description = mbr.Description;
                entity.Filter = mbr.Filter;
                entity.BccEmail = mbr.BccEmail;
                entity.CcEmail = mbr.CcEmail;
                entity.Category = mbr.Category;
                entity.Created = (System.DateTime) mbr.Created;
                entity.CreatedBy = mbr.CreatedBy;
               // entity.CreatedById = (int) mbr.CreatedById;

                entity.FromEmail = mbr.FromEmail;
                entity.HtmlBody = mbr.HtmlBody;
                entity.IsActive = (bool) mbr.isActive;
                entity.IsValid = true;
                entity.LanguageCode = mbr.LanguageCode;
                entity.LastUpdated = ( System.DateTime ) mbr.LastUpdated;
                entity.LastUpdatedBy = mbr.LastUpdatedBy;
              //  entity.LastUpdatedById = (int) mbr.LastUpdatedById;
                entity.NoticeCode = mbr.NoticeCode;
                entity.Subject = mbr.Subject;
                entity.TextBody = mbr.TextBody;

            }

            return entity;
        }
    }
}
