using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.ApplicationBlocks.Data;

using MyEntity = LRWarehouse.Business.ResourceAccessibilityHazard;
using MyEntityCollection = System.Collections.Generic.List<LRWarehouse.Business.ResourceAccessibilityHazard>;

namespace LRWarehouse.DAL
{
    class ResourceAccessibilityHazardManager : BaseDataManager
    {
        public string Import(MyEntity entity)
        {
            string status = "successful";
            #region Sql Parameters
            SqlParameter[] parameters = new SqlParameter[4];
            parameters[0] = new SqlParameter("@ResourceIntId", entity.ResourceIntId);
            parameters[1] = new SqlParameter("@CodeId", entity.CodeId);
            parameters[2] = new SqlParameter("@OriginalValue", entity.OriginalValue);
            parameters[3] = new SqlParameter("@TotalRows", SqlDbType.Int);
            parameters[3].Direction = ParameterDirection.Output;
            #endregion

            try
            {
                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, "[Resource.AccessibilityHazardImport]", parameters);
            }
            catch (Exception ex)
            {
                status = "ResourceAccessibilityHazardManager.Import(): " + ex.Message;
                LogError("ResourceAccessibilityHazardManager.Import(): " + ex.ToString());
            }

            return status;
        }

        public MyEntityCollection Select(int resourceIntId, int codeId, ref string status)
        {
            status = "successful";
            MyEntityCollection collection = new MyEntityCollection();
            DataSet ds = SelectDS(resourceIntId, codeId, ref status);
            if (DoesDataSetHaveRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    MyEntity entity = Fill(dr);
                    collection.Add(entity);
                }
            }
            else
            {
                return null;
            }
            return collection;
        }

        public DataSet SelectDS(int resourceIntId, int codeId, ref string status)
        {
            status = "successful";
            #region Sql Parameters
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@ResourceIntId", resourceIntId);
            parameters[1] = new SqlParameter("@AccessibilityHazardId", codeId);
            #endregion

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[Resource.AccessibilityHazardSelect]", parameters);
                if (DoesDataSetHaveRows(ds))
                {
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                status = "ResourceAccessibilityHazardManager.SelectDS(): " + ex.Message;
                LogError("ResourceAccessibilityHazardManager.SelectDS(): " + ex.ToString());
                return null;
            }
        }

        public MyEntity Fill(DataRow dr)
        {
            MyEntity entity = new MyEntity();
            entity.Id = GetRowColumn(dr, "Id", 0);
            entity.ResourceIntId = GetRowColumn(dr, "ResourceIntId", 0);
            entity.CodeId = GetRowColumn(dr, "AccessibilityHazardId", 0);
            entity.OriginalValue = GetRowColumn(dr, "OriginalValue", "");
            entity.AntonymId = GetRowColumn(dr, "AntonymId", 0);

            return entity;
        }

        public string Create(MyEntity entity)
        {
            string status = "successful";
            #region Sql Parameters
            SqlParameter[] parameters = new SqlParameter[4];
            parameters[0] = new SqlParameter("@ResourceIntId", entity.ResourceIntId);
            parameters[1] = new SqlParameter("@AccessibilityHazardId", entity.CodeId);
            parameters[2] = new SqlParameter("@OriginalValue", entity.OriginalValue);
            parameters[3] = new SqlParameter("@CreatedById", entity.CreatedById);
            #endregion

            try
            {
                SqlDataReader dr = SqlHelper.ExecuteReader(ConnString, CommandType.StoredProcedure, "[Resource.AccessibilityHazardInsert]", parameters);
                if (dr.HasRows)
                {
                    dr.Read();
                    entity.Id = int.Parse(dr[0].ToString());
                }
                dr.Close();
                dr = null;
            }
            catch (Exception ex)
            {
                LogError("ResourceAccessibilityHazardManager.Create(): " + ex.ToString());
                status = "ResourceAccessibilityHazardManager.Create(): " + ex.Message;
            }

            return status;
        }
    }
}
