using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.ApplicationBlocks.Data;

namespace LRWarehouse.DAL
{
    public class ResourceViewManager : BaseDataManager
    {
        const string className = "ResourceViewManager";

        public ResourceViewManager()
        {
        }

        /* This class contains only one method because all we want to do is count the number of page views done by our users and update the ViewCount field
         * in the Resource table.  The [Resource.View] table is here solely for the purpose of publishing paradata to the LR.  Summarizing paradata for publishing
         * is handled in the ParadataManager. */

        /// <summary>
        /// Inserts a row into [Resource.View] and updates the view count in the [Resource] Table.
        /// </summary>
        /// <param name="resourceIntId"></param>
        /// <param name="createdById"></param>
        /// <returns></returns>
        public int Create(int resourceIntId, int createdById)
        {
            int retVal = 0;

            #region parameters
            SqlParameter[] parameter = new SqlParameter[2];
            parameter[0] = new SqlParameter("@ResourceIntId", resourceIntId);
            parameter[1] = new SqlParameter("@CreatedById", createdById);
            #endregion

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource.ViewInsert]", parameter);

                if (DoesDataSetHaveRows(ds))
                {
                    retVal = GetRowColumn(ds.Tables[0].Rows[0], "Id", 0);
                }
            }
            catch (Exception ex)
            {
				if ( ex.Message.ToLower().IndexOf( "foreign key constraint" ) == -1 )
					LogError(ex, className + string.Format(".Create(). resourceId: {0} " ,resourceIntId));
            }
            return retVal;
        }

        public DataSet Select(int resourceIntId)
        {
            #region parameters
            SqlParameter[] parameter = new SqlParameter[1];
            parameter[0] = new SqlParameter("@ResourceIntId", resourceIntId);
            #endregion
            DataSet ds = null;
            try
            {
                ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource.ViewSelect]", parameter);
                if (DoesDataSetHaveRows(ds))
                {
                    return ds;
                }
            }
            catch (Exception ex)
            {
                LogError(ex, className + string.Format(".SelectDetailPageView(). resourceId: {0} ", resourceIntId));
            }

            return ds;
        }

        /// <summary>
        /// The only reason to call this is for a merging of two resources.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="resourceIntId"></param>
        /// <returns></returns>
        public string UpdateView(int id, int resourceIntId)
        {
            string status = "successful";
            #region parameters
            SqlParameter[] parameter = new SqlParameter[2];
            parameter[0] = new SqlParameter("@Id", id);
            parameter[1] = new SqlParameter("@ResourceIntId", resourceIntId);
            #endregion

            try
            {
                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, "[Resource.ViewUpdate]", parameter);
            }
            catch (Exception ex)
            {
                LogError(ex, className + string.Format(".UpdateView().  Id: {0}", id));
                status = className + string.Format(".UpdateView(). Id: {0}", id);
            }

            return status;
        }

        public int CreateDetailPageView(int resourceIntId, int createdById) {
            int retVal = 0;

            #region parameters
            SqlParameter[] parameter = new SqlParameter[2];
            parameter[0] = new SqlParameter("@ResourceIntId", resourceIntId);
            parameter[1] = new SqlParameter("@CreatedById", createdById);
            #endregion

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource.DetailViewInsert]", parameter);

                if (DoesDataSetHaveRows(ds))
                {
                    retVal = GetRowColumn(ds.Tables[0].Rows[0], "Id", 0);
                }
            }
            catch (Exception ex)
            {
				LogError( ex, className + string.Format( ".CreateDetailPageView(). resourceId: {0} ", resourceIntId ) );
            }
            return retVal;
        }

        public DataSet SelectDetailPageView(int resourceIntId)
        {
            #region parameters
            SqlParameter[] parameter = new SqlParameter[1];
            parameter[0] = new SqlParameter("@ResourceIntId", resourceIntId);
            #endregion

            DataSet ds=null;
            try
            {
                ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource.DetailViewSelect]", parameter);
                if (DoesDataSetHaveRows(ds))
                {
                    return ds;
                }
            }
            catch (Exception ex)
            {
                LogError(ex, className + string.Format(".SelectDetailPageView(). resourceId: {0} ", resourceIntId));
            }

            return null;
        }

        /// <summary>
        /// The only reason to call this is for a merging of two resources.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="resourceIntId"></param>
        /// <returns></returns>
        public string UpdateDetailPageView(int id, int resourceIntId)
        {
            string status = "successful";
            #region parameters
            SqlParameter[] parameter = new SqlParameter[2];
            parameter[0] = new SqlParameter("@Id", id);
            parameter[1] = new SqlParameter("@ResourceIntId", resourceIntId);
            #endregion

            try {
                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, "[Resource.DetailViewUpdate]", parameter);
            }
            catch (Exception ex)
            {
                LogError(ex,className+string.Format(".UpdateDetailPageView().  Id: {0}", id));
                status = className+string.Format(".UpdateDetailPageView(). Id: {0}",id);
            }

            return status;
        }
            
    }
}
