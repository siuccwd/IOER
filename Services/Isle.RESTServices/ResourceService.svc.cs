using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using Isle.DataContracts;
using Isle.BizServices;


namespace Isle.RESTServices
{
    [AspNetCompatibilityRequirements( RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed )]
    public class ResourceService : IResourceService
    {
        ResourceBizService resourceBizService = new ResourceBizService();

        #region Resource get

        public ResourceGetResponse ResourceGet( ResourceGetRequest request )
        {
            return resourceBizService.ResourceGet( request );
        }

        public ResourceGetResponse ResourceGetId( int id )
        {
            ResourceGetRequest request = new ResourceGetRequest();
            //request.ResourceVersionId = new Guid( id );
            request.ResourceVersionId = id;

            return resourceBizService.ResourceGet( request );
        }
        #endregion

        public ResourceSearchResponse Search( string pageNbr, string filter )
        {
            ResourceSearchRequest request = new ResourceSearchRequest();
            request.StartingPageNbr = ServiceHelper.StringToInt( pageNbr, 1 );
            request.PageSize = 25;
            request.Filter = filter;

            return resourceBizService.ResourceSearch( request );
        }

        /// <summary>
        ///     http://localhost:26431ResourceService.svc/Search2?pageNbr=1&clusters=8&accessType=1,4
        /// </summary>
        /// <param name="pageNbr"></param>
        /// <param name="clusters"></param>
        /// <param name="accessType"></param>
        /// <returns></returns>
        public ResourceSearchResponse Search2( string pageNbr, string clusters, string accessType )
        {
            ResourceSearchRequest request = new ResourceSearchRequest();
            request.StartingPageNbr = ServiceHelper.StringToInt( pageNbr, 1 );
            request.PageSize = 25;

            string filter = "";
            string booleanOperator = "AND";

            FormatListFilter( "lr.AccessRightsId", accessType, booleanOperator, ref filter );

            FormatListFilter( "[Resource.Cluster]", "ClusterId", clusters, booleanOperator, ref filter );

            request.Filter = filter;
            return resourceBizService.ResourceSearch( request );
        }


        public ResourceSearchResponse Search( ResourceSearchRequest request )
        {
            if ( request.PageSize == 0 || request.PageSize > 50 )
                request.PageSize = 25;
            if ( request.StartingPageNbr == 0  )
                request.PageSize = 1;
            return resourceBizService.ResourceSearch( request );
        }




        /// <summary>
        /// Testing:
        /// http://localhost:99/RestServices/ResourceSearchDemo?filter=(edList.PathwaysEducationLevelId in (4,5,6) )
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public ResourceSearchResponse ResourceSearchDemo( string filter )
        {
            ResourceSearchRequest request = new ResourceSearchRequest();
            request.Filter = filter;
            request.SortOrder = "";
            request.PageSize = 25;
            request.StartingPageNbr = 1;

            return resourceBizService.ResourceSearch( request );

        }//

        /// <summary>
        /// format sql where from csv list - column must be on base table/view
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="list"></param>
        /// <param name="booleanOperator"></param>
        /// <param name="filter"></param>
        private void FormatListFilter( string columnName, string list, string booleanOperator, ref string filter )
        {
            
            if ( list.Trim().Length > 0 )
            {
                //should not have last comma issue
                //csv = csv.Substring( 0, csv.Length - 1 );

                string where = string.Format( " ({0} in ({1})) ", columnName, list );
                filter += ServiceHelper.FormatSearchItem( filter, where, booleanOperator );

            }
        }

        /// <summary>
        /// Format using csv list and named child table and FK column name
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="list"></param>
        /// <param name="booleanOperator"></param>
        /// <param name="filter"></param>
        private void FormatListFilter( string tableName, string columnName, string list, string booleanOperator, ref string filter )
        {

            if ( list.Trim().Length > 0 )
            {
                string where = string.Format( " lr.id in (select ResourceIntId from {0} where {1} in ({2})) ", tableName, columnName, list );
                filter += ServiceHelper.FormatSearchItem( filter, where, booleanOperator );

            }
        }
    }
}
