using System;
using System.Data;
using System.Configuration;
using System.Resources;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;
using ILPathways.Utilities;
using IOER.classes;
using ILPathways.Business;

//using workNet.BusObj.Entity;
using MyManager = ILPathways.DAL.WorkNetSyncManager;
//using workNet.DAL;
using BDM = LRWarehouse.DAL.BaseDataManager; 

using wnNewsItem = IOER.NewsServiceReference;
using subService = IOER.workNetSubscriptionReference;

namespace IOER.Controllers
{
    public class NewsController
    {


        public static NewsEmailTemplate NewsTemlateGet(string newsCode)
        {
            return NewsTemplateGet(newsCode);
        }

        public static NewsEmailTemplate NewsTemplateGet( string newsCode )
        {
            NewsEmailTemplate entity = new NewsEmailTemplate();
            MyManager myManager = new MyManager();

            DataSet ds = NewsTemplateGetDS( newsCode );
            if ( BDM.DoesDataSetHaveRows( ds ) )
            {
                entity = MyManager.NewsEmailTemplateFill( ds.Tables[ 0 ].Rows[ 0 ] );
            }
            return entity;
        }

        public static DataSet NewsTemlateGetDs(string newsCode)
        {
            return NewsTemplateGetDS(newsCode);
        }

        public static DataSet NewsTemplateGetDS( string newsCode )
        {
            wnNewsItem.NewsServicesSoapClient wsClient = new wnNewsItem.NewsServicesSoapClient();

            DataSet ds = wsClient.NewsTemplateGet( newsCode );
            return ds;
        }

        #region Subscriptions 
        public static AppItemAnnouncementSubscription SubscriptionGet( string pRowId )
        {
            AppItemAnnouncementSubscription entity = new AppItemAnnouncementSubscription();
            subService.SubscriptionsSoapClient wsClient = new subService.SubscriptionsSoapClient();
            string password = AppItemController.workNetItemsPassword;

            subService.AppItemAnnouncementSubscription sub = wsClient.Get( pRowId, password );
            entity = Map( sub );
            return entity;
        } //

        public static AppItemAnnouncementSubscription SubscriptionGet( string pCategory, string pEmail )
        {
            AppItemAnnouncementSubscription entity = new AppItemAnnouncementSubscription();
            subService.SubscriptionsSoapClient wsClient = new subService.SubscriptionsSoapClient();
            string password = AppItemController.workNetItemsPassword;

            subService.AppItemAnnouncementSubscription sub = wsClient.GetByCodeAndEmail( pCategory, pEmail, password );
            entity = Map( sub );
            return entity;
        } //
        //public static AppItemAnnouncementSubscription SubscriptionGet( string pRowId, string pCategory, int pId, string pEmail )
        //{
        //    AppItemAnnouncementSubscription entity = new AppItemAnnouncementSubscription();
        //    MyManager myManager = new MyManager();

        //    DataSet ds = SubscriptionGetDS( pCategory, pEmail );
        //    if ( workNet.DAL.BaseDataManager.DoesDataSetHaveRows( ds ) )
        //    {
        //        entity = MyManager.SubscriptionFill( ds.Tables[ 0 ].Rows[ 0 ] );
        //    }
        //    return entity;
        //}


        public static string Subscribe( AppItemAnnouncementSubscription entity )
        {
            subService.SubscriptionsSoapClient wsClient = new subService.SubscriptionsSoapClient();
            string password = AppItemController.workNetItemsPassword;

            string status = wsClient.Subscribe( entity.Category, entity.Email, password );
            try
            {
                Guid guid = new Guid(status);
                entity.RowId = guid;
                status = "successful";
            }
            catch (Exception ex)
            {
                // Not a valid GUID, return the message passed back from the web service - by doing nothing.
            }
            return status;

        }


        public static string SubscribeUpdate( AppItemAnnouncementSubscription entity )
        {

            subService.SubscriptionsSoapClient wsClient = new subService.SubscriptionsSoapClient();
            string password = AppItemController.workNetItemsPassword;
            subService.AppItemAnnouncementSubscription wsSub = Map( entity );
            string status = wsClient.Update( wsSub, password );
            return status;
        }


        public static string UnSubscribe( string pRowId )
        {
            AppItemAnnouncementSubscription entity = new AppItemAnnouncementSubscription();
            subService.SubscriptionsSoapClient wsClient = new subService.SubscriptionsSoapClient();
            string password = AppItemController.workNetItemsPassword;

            string status = wsClient.Delete( pRowId, password );
            
            return status;

        }

        public static AppItemAnnouncementSubscription Map( subService.AppItemAnnouncementSubscription from )
        {
            AppItemAnnouncementSubscription entity = new AppItemAnnouncementSubscription();
            entity.Id = from.Id;
            entity.RowId = from.RowId;
            entity.Category = from.Category;
            entity.Frequency = from.Frequency;
            entity.Email = from.Email;
            entity.IsValidated = from.IsValidated;

            entity.IsValid = from.IsValid;

            return entity;



        }


        public static subService.AppItemAnnouncementSubscription Map( AppItemAnnouncementSubscription from )
        {

            subService.AppItemAnnouncementSubscription wsEntity = new subService.AppItemAnnouncementSubscription();
            wsEntity.Id = from.Id;
            wsEntity.Category = from.Category;
            wsEntity.Frequency = from.Frequency;
            wsEntity.Email = from.Email;
            wsEntity.IsValidated = from.IsValidated;

            wsEntity.IsValid = from.IsValid;

            return wsEntity;
        }
        #endregion
    }
}