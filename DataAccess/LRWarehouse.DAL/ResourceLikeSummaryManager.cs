using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using Microsoft.ApplicationBlocks.Data;
using ResourceLikeSummary = LRWarehouse.Business.ResourceLikeSummary;

namespace LRWarehouse.DAL
{
    public class ResourceLikeSummaryManager : BaseDataManager
    {
        const string className = "ResourceLikeSummaryManager";
        const string INSERT_PROC = "[Resource.LikeSummaryInsert]";
        const string GET_PROC = "[Resource.LikeSummaryGet]";
        const string UPDATE_PROC = "[Resource.LikeSummaryUpdate]";

        public int Create(ResourceLikeSummary entity, ref string status)
        {
            int retVal = 0;
            status = "successful";

            try
            {
                #region sqlParameters
                SqlParameter[] parameter = new SqlParameter[4];
                parameter[0] = new SqlParameter("@ResourceId", entity.ResourceId);
                parameter[1] = new SqlParameter("@ResourceIntId", entity.ResourceIntId);
                parameter[2] = new SqlParameter("@LikeCount", entity.LikeCount);
                parameter[3] = new SqlParameter("@DislikeCount", entity.DislikeCount);
                #endregion

                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, INSERT_PROC, parameter);
                if (DoesDataSetHaveRows(ds))
                {
                    retVal = GetRowColumn(ds.Tables[0].Rows[0], "Id", 0);
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".Create(): " + ex.ToString());
                status = ex.Message;
            }

            return retVal;
        }

        public ResourceLikeSummary Get(int resourceIntId, ref string status)
        {
            status = "successful";
            ResourceLikeSummary likeSummary = null;

            try
            {
                #region parameters
                SqlParameter[] parameter = new SqlParameter[1];
                parameter[0] = new SqlParameter("@ResourceIntId", resourceIntId);
                #endregion

                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, GET_PROC, parameter);
                if (DoesDataSetHaveRows(ds))
                {
                    likeSummary = Fill(ds.Tables[0].Rows[0]);
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".Get(): " + ex.ToString());
                status = ex.Message;
            }

            return likeSummary;
        }

        public ResourceLikeSummary Fill(DataRow dr)
        {
            ResourceLikeSummary likeSummary = new ResourceLikeSummary();
            likeSummary.Id = GetRowColumn(dr, "Id", 0);
            likeSummary.ResourceId = new Guid(GetRowColumn(dr, "ResourceId", DEFAULT_GUID));
            likeSummary.ResourceIntId = GetRowColumn(dr, "ResourceIntId", 0);
            likeSummary.LikeCount = GetRowColumn(dr, "LikeCount", 0);
            likeSummary.DislikeCount = GetRowColumn(dr, "DislikeCount", 0);
            likeSummary.LastUpdated = GetRowColumn(dr, "LastUpdated", DateTime.Now);

            return likeSummary;
        }

        public ResourceLikeSummary GetForDisplay(int resourceIntId, int userId, ref string status)
        {
            status = "successful";
            ResourceLikeSummary likeSummary = null;

            try
            {
                #region parameters
                SqlParameter[] parameter = new SqlParameter[2];
                parameter[0] = new SqlParameter("@Id", resourceIntId);
                parameter[1] = new SqlParameter("@UserId", userId);
                #endregion

                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[Resource.LikeGetDisplay]", parameter);
                if (DoesDataSetHaveRows(ds))
                {
                    likeSummary = FillForDisplay(ds.Tables[0].Rows[0]);
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".GetForDisplay(): " + ex.ToString());
                status = ex.Message;
            }

            return likeSummary;
        }

        public ResourceLikeSummary FillForDisplay(DataRow dr)
        {
            ResourceLikeSummary likeSummary = new ResourceLikeSummary();
            likeSummary.ResourceIntId = GetRowColumn(dr, "ResourceIntId", 0);
            likeSummary.LikeCount = GetRowColumn(dr, "LikeCount", 0);
            likeSummary.DislikeCount = GetRowColumn(dr, "DislikeCount", 0);
            likeSummary.YouLikeThis = GetRowColumn(dr, "YouLikeThis", false);
            likeSummary.YouDislikeThis = GetRowColumn(dr, "YouDislikeThis", false);

            return likeSummary;
        }

        public void Update(ResourceLikeSummary entity, ref string status)
        {
            status = "successful";

            try
            {
                #region parameters
                SqlParameter[] parameter = new SqlParameter[3];
                parameter[0] = new SqlParameter("@Id", entity.Id);
                parameter[1] = new SqlParameter("@LikeCount", entity.LikeCount);
                parameter[2] = new SqlParameter("@DislikeCount", entity.DislikeCount);
                #endregion

                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, UPDATE_PROC, parameter);
            }
            catch (Exception ex)
            {
                LogError(className + ".Update(): " + ex.ToString());
                status = ex.Message;
            }
        }
    }
}
