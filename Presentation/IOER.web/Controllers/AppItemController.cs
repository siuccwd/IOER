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
using MyManager = ILPathways.DAL.AppItemManager;

using wnAppItem = IOER.AppItemsServiceReference;
using BDM = LRWarehouse.DAL.DatabaseManager;
using LRWarehouse.DAL;

namespace IOER.Controllers
{
	public class AppItemController : BaseController
    {

        public static string workNetItemsPassword = "Achtung!SieVerlassenJetztWest-Berlin";

        public static DataSet AppItemWebServiceSelectGroup( string groupCode)
        {
            wnAppItem.ItemsSoapClient wsClient = new wnAppItem.ItemsSoapClient();

            string password = workNetItemsPassword; // UtilityManager.GetAppKeyValue( "workNetItemsPassword", "" );
            DataSet ds = wsClient.GetItemsInGroup( groupCode, AppItem.HighlightedTopicItemType, "", "", "published", password );
            return ds;
        }

        public static DataSet AppItemWebServiceNewsSearch( string newsCategory, string keywords, int maxRecords )
        {
            wnAppItem.ItemsSoapClient wsClient = new wnAppItem.ItemsSoapClient();
            //TODO - error on max message size > 65K??
            //pass in the max nbr of records, and limit at db!!
            int pTotalRows = 0;
            string booleanOperator = "AND";
            string password = workNetItemsPassword; // UtilityManager.GetAppKeyValue( "workNetItemsPassword", "" );
            string filter = string.Format( " (TypeId = 1085 AND Category  = '{0}' )", newsCategory);

            string status = " (Status='Published') ";
            bool usingwebOnlyPublished = true;
            //
            if ( usingwebOnlyPublished )
            {
                status = " (Status='Published' 	OR Status = 'Published-WebOnly') ";
            }
            filter += BaseDataManager.FormatSearchItem( filter, status, booleanOperator );

            if ( keywords.Trim().Length > 0 )
            {
                string keyword = BDM.HandleApostrophes( FormHelper.CleanText( keywords.Trim() ) );

                if ( keyword.IndexOf( "%" ) == -1 )
                    keyword = "%" + keyword + "%";

                string where = " (Title like '" + keyword + "'	OR [Description] like '" + keyword + "') ";
                filter += BDM.FormatSearchItem( filter, where, booleanOperator );
            }
            //string where = string.Format( " (lrl.LanguageId = {0} ) ", ddlLanguages.SelectedValue );
            string sortOrder = " Created DESC";

            DataSet ds = wsClient.Search( filter, sortOrder, 1, maxRecords, ref pTotalRows, password );
            return ds;
        }

        public static DataSet AppItemWebServiceNewsSearch( string filter, string sortOrder, int selectedPageNbr, int maxRecords, ref int pTotalRows )
        {
            wnAppItem.ItemsSoapClient wsClient = new wnAppItem.ItemsSoapClient();

            string password = workNetItemsPassword; // UtilityManager.GetAppKeyValue( "workNetItemsPassword", "" );


            DataSet ds = wsClient.Search( filter, sortOrder, selectedPageNbr, maxRecords, ref pTotalRows, password );
            return ds;
        }

        public static AppItem AppItemGet( string recId )
        {
            AppItem entity = new AppItem();
            MyManager myManager = new MyManager();

            DataSet ds = AppItemGetDs( recId );
            if ( BDM.DoesDataSetHaveRows( ds ) )
            {
                entity = myManager.Fill( ds.Tables[ 0 ].Rows[ 0 ] );
            }
            return entity;
        }
        public static DataSet AppItemGetDs( string recId )
        {
            wnAppItem.ItemsSoapClient wsClient = new wnAppItem.ItemsSoapClient();

            DataSet ds = wsClient.GetItem( recId, workNetItemsPassword );
            return ds;
        }
        /// <summary>
        /// NOT Operational!!!!
        /// </summary>
        /// <param name="newsCategory"></param>
        /// <param name="maxRecords"></param>
        /// <returns></returns>
        private static DataSet AppItemWebServiceNewsSelect( string newsCategory, int maxRecords )
        {
            wnAppItem.ItemsSoapClient wsClient = new wnAppItem.ItemsSoapClient();
            //TODO - error on max message size > 65K??
            //pass in the max nbr of records, and limit at db!!


            string password = workNetItemsPassword; // UtilityManager.GetAppKeyValue( "workNetItemsPassword", "" );
            DataSet ds = wsClient.Select( AppItem.NewsItemType, newsCategory, "", "published", "", "", "", password );
            return ds;
        }


    }
}
