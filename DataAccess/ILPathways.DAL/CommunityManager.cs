using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.ApplicationBlocks.Data;

using ILPathways.Business;

namespace ILPathways.DAL
{
    public class CommunityManager : BaseDataManager
    {
        static string thisClassName = "CommunityManager";

        /// <summary>
        /// Search for CommunityPosting related data using passed parameters
        /// - returns List
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public List<CommunityPosting> SearchAsList( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            List<CommunityPosting> list = new List<CommunityPosting>();
            DataSet ds = Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
            if ( DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow row in ds.Tables[ 0 ].Rows )
                {
                    CommunityPosting lib = Fill( row );
                    list.Add( lib );
                }
            }

            return list;
        }//

        /// <summary>
        /// Search for CommunityPosting related data using passed parameters
        /// - uses custom paging
        /// - only requested range of rows will be returned
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public DataSet Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {

            int outputCol = 4;
            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", pOrderBy );
            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", pStartPageIndex );
            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", pMaximumRows );

            sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ outputCol ].Direction = ParameterDirection.Output;

            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Community.PostingSearch]", sqlParameters );
                    //get output paramter
                    string rows = sqlParameters[ outputCol ].Value.ToString();
                    try
                    {
                        pTotalRows = Int32.Parse( rows );
                    }
                    catch
                    {
                        pTotalRows = 0;
                    }



                    if ( ds.HasErrors )
                    {
                        return null;
                    }
                    return ds;
                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".Search() " );
                    return null;

                }
            }
        }

        public CommunityPosting Fill( DataRow dr )
        {
            CommunityPosting entity = FillLazy( dr );

            entity.UserFullName = GetRowPossibleColumn( dr, "UserFullName", "" );
            entity.UserImageUrl = GetRowPossibleColumn( dr, "UserImageUrl", "" );
            //entity.UserProfileUrl = GetRowPossibleColumn( dr, "UserProfileUrl", "" );

            return entity;
        }//

        public CommunityPosting FillLazy( DataRow dr )
        {
            CommunityPosting entity = new CommunityPosting();

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.Message = GetRowColumn( dr, "Message", "missing" );
            entity.CommunityId = GetRowColumn( dr, "CommunityId", 0 );
            entity.RelatedPostingId = GetRowColumn( dr, "RelatedPostingId", 0 );

            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );


            return entity;
        }//
    }
}
