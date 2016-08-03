﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.ApplicationBlocks.Data;

using MyEntity = LRWarehouse.Business.ResourceChildItem;
using MyEntityCollection = System.Collections.Generic.List<LRWarehouse.Business.ResourceChildItem>;

namespace LRWarehouse.DAL
{
    public class ResourceAccessibilityApiManager : BaseDataManager
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
                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, "[Resource.AccessibilityApiImport]", parameters);
            }
            catch (Exception ex)
            {
                status = "ResourceAccessibilityApiManager.Import(): " + ex.Message;
                LogError("ResourceAccessibilityApiManager.Import(): " + ex.ToString());
            }

            return status;
        }

        public string ImportV2(MyEntity entity)
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
                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, "[Resource.AccessibilityApiImportV2]", parameters);
            }
            catch (Exception ex)
            {
                status = "ResourceAccessibilityApiManager.ImportV2(): " + ex.Message;
                LogError("ResourceAccessibilityApiManager.ImportV2(): " + ex.ToString());
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
            parameters[1] = new SqlParameter("@AccessibilityApiId", codeId);
            #endregion

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[Resource.AccessibilityApiSelect]", parameters);
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
                status = "ResourceAccessibilityApiManager.SelectDS(): " + ex.Message;
                LogError("ResourceAccessibilityApiManager.SelectDS(): " + ex.ToString());
                return null;
            }
        }

        public MyEntity Fill(DataRow dr)
        {
            MyEntity entity = new MyEntity();
            entity.Id = GetRowColumn(dr, "Id", 0);
            entity.ResourceIntId = GetRowColumn(dr, "ResourceIntId", 0);
            entity.CodeId = GetRowColumn(dr, "AccessibilityApiId", 0);
            entity.OriginalValue = GetRowColumn(dr, "OriginalValue", "");

            return entity;
        }

        public string Create(MyEntity entity)
        {
            string status = "successful";
            #region Sql Parameters
            SqlParameter[] parameters = new SqlParameter[4];
            parameters[0] = new SqlParameter("@ResourceIntId", entity.ResourceIntId);
            parameters[1] = new SqlParameter("@AccessibilityApiId", entity.CodeId);
            parameters[2] = new SqlParameter("@OriginalValue", entity.OriginalValue);
            parameters[3] = new SqlParameter("@CreatedById", entity.CreatedById);
            #endregion

            try
            {
                SqlDataReader dr = SqlHelper.ExecuteReader(ConnString, CommandType.StoredProcedure, "[Resource.AccessibilityApiInsert]", parameters);
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
                LogError("ResourceAccessibilityApiManager.Create(): " + ex.ToString());
                status = "ResourceAccessibilityApiManager.Create(): " + ex.Message;
            }

            return status;
        }
    }
}
