using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.ApplicationBlocks.Data;
using ILPathways.Business;
using ILPathways.Common;
using ILPathways.Business;

namespace ILPathways.DAL
{
    public class WorkNetSyncManager : BaseDataManager
    {
        public WorkNetSyncManager()
        { }//

        public static AppItemAnnouncementSubscription SubscriptionFill( DataRow dr )
        {
            AppItemAnnouncementSubscription entity = new AppItemAnnouncementSubscription();

            entity.Category = GetRowColumn( dr, "Category", "" );
            entity.Frequency = GetRowColumn( dr, "Frequency", 0 );
            entity.Created = GetRowColumn( dr, "Created", DateTime.Now );
            entity.LastUpdated = entity.Created;
            entity.IsValidated = GetRowColumn( dr, "IsValidated", false );
            entity.Email = GetRowColumn( dr, "Email", "" );
            entity.Id = GetRowColumn( dr, "Id", 0 );

            string rowId = GetRowColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
                entity.RowId = new Guid( rowId );

            return entity;
        } //Fill

        public static NewsEmailTemplate NewsEmailTemplateFill( DataRow dr )
        {
            NewsEmailTemplate entity = new NewsEmailTemplate();

            entity.IsValid = true;

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.NewsItemCode = GetRowColumn( dr, "NewsItemCode", "" );
            entity.Category = GetRowColumn( dr, "Category", "" );
            entity.Title = GetRowColumn( dr, "Title", "" );
            entity.Description = GetRowColumn( dr, "Description", "" );
            entity.SearchUrl = GetRowColumn( dr, "SearchUrl", "" );
            entity.DisplayUrl = GetRowColumn( dr, "DisplayUrl", "" );
            entity.UnsubscribeUrl = GetRowColumn( dr, "UnsubscribeUrl", "" );
            entity.ConfirmUrl = GetRowColumn( dr, "ConfirmUrl", "" );
            entity.Template = GetRowColumn( dr, "Template", "" );

            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            entity.CreatedBy = GetRowColumn( dr, "CreatedBy", "" );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
            entity.LastUpdatedBy = GetRowColumn( dr, "LastUpdatedBy", "" );

            return entity;
        }//
    }
}
