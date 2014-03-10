using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.ApplicationBlocks.Data;

using LRWarehouse.Business;
using Entity = LRWarehouse.Business.RatingSummary;
using EntityCollection = LRWarehouse.Business.RatingSummaryCollection;

namespace LRWarehouse.DAL
{
    public class RatingSummaryManager : BaseDataManager
    {
        public RatingSummaryManager()
        {
            ConnString = LRWarehouse();
            ReadOnlyConnString = LRWarehouseRO();
        }

        public string Create(Entity entity)
        {
            string retVal = "successful";

            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[6];
                sqlParameter[0] = new SqlParameter("@ResourceId", SqlDbType.UniqueIdentifier);
                sqlParameter[0].Value = entity.ResourceId;
                sqlParameter[1] = new SqlParameter("@RatingTypeId", SqlDbType.Int);
                sqlParameter[1].Value = entity.RatingTypeId;
                sqlParameter[2] = new SqlParameter("@RatingCount", SqlDbType.Int);
                sqlParameter[2].Value = entity.RatingCount;
                sqlParameter[3] = new SqlParameter("@RatingTotal", SqlDbType.Int);
                sqlParameter[3].Value = entity.RatingTotal;
                sqlParameter[4] = new SqlParameter("@RatingAverage", SqlDbType.Decimal);
                sqlParameter[4].Value = entity.RatingAverage;
                sqlParameter[5] = new SqlParameter("@ResourceIntId", SqlDbType.Int);
                sqlParameter[5].Value = entity.ResourceIntId;

                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource.RatingSummaryInsert]", sqlParameter);
                if (DoesDataSetHaveRows(ds))
                {
                    string rowId = GetRowColumn(ds.Tables[0].Rows[0], "RowId", "");
                    if (rowId.Length > 35)
                    {
                        entity.ResourceId = new Guid(rowId);
                    }
                    else
                    {
                        retVal = string.Format("Value {0} returned from proc is too short", rowId);
                        LogError(retVal);
                    }
                    return retVal;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                retVal = ex.ToString();
                LogError(ex, "RatingSummaryManager.Create(): ");
                return retVal;
            }
        }
        public Entity Get(string resourceId, int ratingTypeId, string type, string identifier)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[4];
                sqlParameter[0] = new SqlParameter("@ResourceId", SqlDbType.UniqueIdentifier);
                sqlParameter[0].Value = new Guid(resourceId);
                sqlParameter[1] = new SqlParameter("@RatingTypeId", SqlDbType.Int);
                sqlParameter[1].Value = ratingTypeId;
                sqlParameter[2] = new SqlParameter("@Type", SqlDbType.VarChar);
                sqlParameter[2].Size = 50;
                sqlParameter[2].Value = type;
                sqlParameter[3] = new SqlParameter("@Identifier", SqlDbType.VarChar);
                sqlParameter[3].Size = 500;
                sqlParameter[3].Value = identifier;

                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource.RatingSummaryGet]", sqlParameter);
                if (DoesDataSetHaveRows(ds))
                {
                    Entity entity = Fill(ds.Tables[0].Rows[0]);
                    return entity;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, "", "", ErrorType.Error, ErrorRouting.Technical, ex.ToString());
                //return null;
            }
        }

        public Entity Get(string resourceId, int ratingTypeId, string type, string identifier, ref string status)
        {
            status = "successful";
            EntityCollection collection = Select(resourceId, ratingTypeId, type, identifier, ref status);
            if (collection != null && collection.Count > 0)
            {
                return collection[0];
            }
            else
            {
                Entity entity = new Entity();
                entity.IsValid = false;
                entity.Message = "Not found";
                return entity;
            }
        }

        public EntityCollection Select(string resourceId, int ratingTypeId, string type, string identifier, ref string status) 
        {
            status = "successful";
            EntityCollection collection = new EntityCollection();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[4];
                sqlParameter[0] = new SqlParameter("@ResourceId", SqlDbType.UniqueIdentifier);
                sqlParameter[0].Value = new Guid(resourceId);
                sqlParameter[1] = new SqlParameter("@RatingTypeId", SqlDbType.Int);
                sqlParameter[1].Value = ratingTypeId;
                sqlParameter[2] = new SqlParameter("@Type", SqlDbType.VarChar);
                sqlParameter[2].Size = 50;
                sqlParameter[2].Value = type;
                sqlParameter[3] = new SqlParameter("@Identifier", SqlDbType.VarChar);
                sqlParameter[3].Size = 500;
                sqlParameter[3].Value = identifier;

                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource.RatingSummaryGet]", sqlParameter);
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {

                        Entity entity = Fill(dr);
                        collection.Add(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "RatingSummaryManager.Select(): ");
                return null;
            }

            return collection;
        }

        public Entity Fill(DataRow dr)
        {
            Entity entity = new Entity();
            string resourceId = GetRowColumn(dr, "ResourceId", "");
            if (resourceId.Length > 35)
            {
                entity.ResourceId = new Guid(resourceId);
            }
            entity.ResourceIntId = GetRowColumn(dr, "ResourceIntId", 0);
            entity.RatingTypeId = GetRowColumn(dr, "RatingTypeId", 0);
            entity.RatingCount = GetRowColumn(dr, "RatingCount", 0);
            entity.RatingTotal = GetRowColumn(dr, "RatingTotal", 0);
            entity.RatingAverage = GetRowColumn(dr, "RatingAverage", (decimal)0);
            entity.RatingTypeIdentifier = GetRowColumn(dr, "Identifier", "");
            entity.RatingTypeDescription = GetRowColumn(dr, "Description", "");
            entity.LastUpdated = GetRowColumn(dr, "LastUpdated", new DateTime(2000, 1, 1));

            return entity;
        }

        public string Update(Entity entity)
        {
            string retVal = "successful";

            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[6];
                sqlParameter[0] = new SqlParameter("@ResourceId", SqlDbType.UniqueIdentifier);
                sqlParameter[0].Value = entity.ResourceId;
                sqlParameter[1] = new SqlParameter("@RatingTypeId", SqlDbType.Int);
                sqlParameter[1].Value = entity.RatingTypeId;
                sqlParameter[2] = new SqlParameter("@RatingCount", SqlDbType.Int);
                sqlParameter[2].Value = entity.RatingCount;
                sqlParameter[3] = new SqlParameter("@RatingTotal", SqlDbType.Int);
                sqlParameter[3].Value = entity.RatingTotal;
                sqlParameter[4] = new SqlParameter("@RatingAverage", SqlDbType.Decimal);
                sqlParameter[4].Value = entity.RatingAverage;
                sqlParameter[5] = new SqlParameter("@ResourceIntId", SqlDbType.Int);
                sqlParameter[5].Value = entity.ResourceIntId;

                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, "[Resource.RatingSummaryUpdate]", sqlParameter);
            }
            catch (Exception ex)
            {
                retVal = ex.ToString();
                LogError(ex, "RatingSummaryManager.Update(): ");
            }

            return retVal;
        }

        public string Delete(string resourceId)
        {
            string retVal = "successful";

            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[1];
                sqlParameter[0] = new SqlParameter("@ResourceId", SqlDbType.UniqueIdentifier);
                sqlParameter[0].Value = new Guid(resourceId);

                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, "[Resource.RatingSummaryDelete]", sqlParameter);
            }
            catch (Exception ex)
            {
                retVal = ex.ToString();
                LogError(ex, "RatingSummaryManager.Delete(): ");
            }

            return retVal;
        }
    }
}
