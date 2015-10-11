using System;
using System.Collections.ObjectModel;
using System.Net;

using IOER.ResourceServiceReference;
//using ILPathways.UserDataServiceReference;

using ILPathways.Utilities;
namespace IOER.Controllers
{
    public class ServiceReferenceHelper
    {

        public static string FileServicesAddress = "http://IllinoisPathways.com:3334/FileServices/";
        public static int AdDuration = 10;
        public static int AdRefreshInterval = 1200;

		public static int MaxImageSize = 1024000;
		public static int MaxLogoSize = 100000;
		public static int MaxVideoSize = 2048000;
		public static int MaxDocumentSize = 2048000;


        #region User cache
       // public static UserDataServiceReference.UserDataContract CurrentUser;
        public static string GetServicesAddress()
        {
            return UtilityManager.GetAppKeyValue( "ServicesAddress", "http://www.IllinoisPathways.com:3333/" );
        }

		public static Guid GetCurrentUserId()
		{
			Guid userId = Guid.NewGuid();
            //userId = CurrentUser.Id;
            //if ( userId.ToString().Equals( new Guid().ToString() ) )
            //{
            //    if ( CurrentPartner != null && CurrentPartner.User != null && CurrentPartner.User.Id.ToString() != new Guid().ToString() ) 
            //    {
            //        userId = CurrentPartner.User.Id;
            //    }
            //    else if ( CurrentPatron != null && CurrentPatron.User != null && CurrentPatron.User.Id.ToString() != new Guid().ToString() )
            //    {
            //        userId = CurrentPatron.User.Id;
            //    }
            //    else
            //    {
            //        //what now

            //    }

            //}
			return userId;
		}
        #endregion

        #region Patron/Parther cache
        //public static PatronDataServiceReference.PatronDataContract CurrentPatron;
        //public static PatronDataServiceReference.PatronDataContract SelectedPatron;

        #endregion

    }
}
