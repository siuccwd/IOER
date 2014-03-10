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
                LogError(className + ".Create(): " + ex.ToString());
            }
            return retVal;
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
                LogError(className + ".CreateDetailPageView(): " + ex.ToString());
            }
            return retVal;
        }
            
    }
}
