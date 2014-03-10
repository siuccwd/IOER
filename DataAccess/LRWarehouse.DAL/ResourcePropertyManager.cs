using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.ApplicationBlocks.Data;
using Entity = LRWarehouse.Business.ResourceProperty;

namespace LRWarehouse.DAL
{
    public class ResourcePropertyManager : BaseDataManager
    {
        public ResourcePropertyManager()
        {
        }

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <param name="pName"></param>
        /// <param name="pValue"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public Entity Get(string pResourceId, string pName, string pValue, ref string status)
        {
            status = "successful";
            Entity entity = new Entity();
            try
            {
                string filter = string.Format("ResourceId = '{0}' AND rpt.Title='{1}' AND [Value] = '{2}'",
          pResourceId.Replace("'", "''"), pName.Replace("'", "''"), pValue.Replace("'", "''"));

                DataSet ds = Select(filter, ref status);
                if (DoesDataSetHaveRows(ds))
                {
                    entity = Fill(ds.Tables[0].Rows[0]);
                    return entity;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogError("ResourcePropertyManager.Get(): " + ex.ToString());
                status = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public DataSet Select(string filter, ref string status)
        {
            status = "successful";
            try
            {
                DataSet ds = new DataSet();
                SqlParameter[] sqlParameters = new SqlParameter[1];
                sqlParameters[0] = new SqlParameter("@filter", SqlDbType.VarChar);
                sqlParameters[0].Size = 4000;
                sqlParameters[0].Value = filter;
                if (filter.Length > 0)
                {
                    sqlParameters[0].Value = " WHERE " + filter;
                }

                ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource.PropertySelect]", sqlParameters);
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
                LogError("ResourcePropertyManager.Select(): " + ex.ToString());
                status = ex.Message;
                return null;
            }
        }

        public Entity Fill(DataRow dr)
        {
            Entity entity = new Entity();
            //entity.ResourceId = GetRowColumn(dr, "ResourceId", "");
            string rowId = GetRowColumn(dr, "RowId", "");
            if (rowId.Length > 35)
                entity.RowId = new Guid(rowId);

            rowId = GetRowColumn(dr, "ResourceId", "");
            if (rowId.Length > 35)
                entity.ResourceId = new Guid(rowId);
            entity.ResourceIntId = GetRowColumn(dr, "ResourceIntId", 0);

            entity.PropertyTypeId = GetRowColumn(dr, "PropertyTypeId", 0);
            entity.PropertyType = GetRowColumn(dr, "Name", "");
            entity.Value = GetRowColumn(dr, "Value", "");

            return entity;
        }

        public string Create(Entity pEntity, ref string statusMessage)
        {
            statusMessage = "successful";
            bool usingPropId = false;
            string newId = "";
            string filter = "";
            DataSet ds;


            // Check to see if the property already exists.
            filter = string.Format("ResourceId = '{0}' AND [PropertyTypeId] = '{1}' AND [Value] = '{2}'", pEntity.ResourceId, pEntity.PropertyTypeId, pEntity.Value.Replace("'", "''"));
            ds = Select(filter, ref statusMessage);
            if (DoesDataSetHaveRows(ds))
            {
                // Property exists, return the RowId of the property
                newId = GetRowColumn(ds.Tables[0].Rows[0], "RowId", DEFAULT_GUID);
            }
            else
            {
                // Property does not exist, and does not exist (or no longer exists) as a keyword, add it.
                statusMessage = "successful";
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[4];
                    sqlParameters[0] = new SqlParameter("@ResourceId", pEntity.ResourceId.ToString());

                    //if (usingPropId == false)
                    //{
                    //  sqlParameters[2] = new SqlParameter("@PropertyName", pEntity.Name);
                    //}
                    //else
                    //{
                    //  sqlParameters[1] = new SqlParameter("@PropertyTypeId", pEntity.PropertyTypeId);
                    //}
                    sqlParameters[1] = new SqlParameter("@PropertyTypeId", pEntity.PropertyTypeId);
                    sqlParameters[2] = new SqlParameter("@PropertyName", pEntity.Name);
                    sqlParameters[3] = new SqlParameter("@Value", pEntity.Value);

                    //SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, "[Resource.PropertyInsert]", sqlParameters);
                    SqlDataReader dr = SqlHelper.ExecuteReader(ConnString, CommandType.StoredProcedure, "[Resource.PropertyInsert]", sqlParameters);
                    if (dr.HasRows)
                    {
                        dr.Read();
                        newId = dr[0].ToString();
                    }
                    dr.Close();
                    dr = null;
                    statusMessage = "successful";

                }
                catch (Exception ex)
                {
                    LogError("ResourcePropertyManager.Create(): " + ex.ToString());
                    statusMessage = ex.Message;
                }
            }
            return newId;
        }

        public string Delete(string rowId)
        {
            string retVal = "successful";
            try
            {
                string sql = string.Format("DELETE FROM [Resource.Property] WHERE RowId = '{0}'", rowId);
                SqlHelper.ExecuteNonQuery(ConnString, CommandType.Text, sql);
            }
            catch (Exception ex)
            {
                LogError("ResourcePropertyManager.Delete(): " + ex.ToString());
                retVal = ex.Message;
            }
            return retVal;
        }

        public DataSet ResPropertyTypeLookup(string title)
        {
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[1];
                sqlParameters[0] = new SqlParameter("@Title", SqlDbType.VarChar);
                sqlParameters[0].Size = 50;
                sqlParameters[0].Value = title;

                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Codes.ResPropertyType_Lookup]", sqlParameters);
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
                throw ex;
                //new AuditReportingManager().LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, "", "", ErrorType.Error, ErrorRouting.Technical,
                //    "ResPropertyTypeManager.Lookup(): " + ex.ToString());
                //return null;
            }

        } */

    }
}
