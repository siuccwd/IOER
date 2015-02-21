using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LRWarehouse.Business;
using LRWarehouse.DAL;

namespace Isle.BizServices
{
    public class ElasticIndexServices
    {

        /// <summary>
        /// Remove a resource version document 
        /// </summary>
        /// <param name="resourceVersionId"></param>
        /// <param name="response"></param>
        public static void RemoveResourceVersion( int resourceVersionId, ref string response )
        {
            response = "";
            //new ElasticSearchManager().DeleteByVersionID( resourceVersionId, ref response );
            new ElasticSearchManager().DeleteResource( new ResourceVersionManager().Get( resourceVersionId ).ResourceIntId );
        }

        /// <summary>
        /// Remove a resource version document 
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="response"></param>
        public static void RemoveResource( int resourceId, ref string response )
        {
            response = "";
            //new ElasticSearchManager().DeleteByIntID( resourceId, ref response );
            new ElasticSearchManager().DeleteResource( resourceId );
        }

        public static void RemoveResource_NewCollection( int resourceId, ref string response )
        {
            response = "";
            //new ElasticSearchManager().NewCollection_DeleteByID( resourceId, ref response );
            new ElasticSearchManager().DeleteResource( resourceId );
        }
    }
}
